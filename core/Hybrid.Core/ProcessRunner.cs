using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;

namespace Hybrid.Core;

/// <summary>Bounded execution of one owned child process; this is not an OS sandbox.</summary>
public static class ProcessRunner
{
    public const int CaptureLimitBytes = 65_536;
    private static readonly TimeSpan CleanupLimit = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PipeDrainLimit = TimeSpan.FromSeconds(2);

    public static async Task<JsonObject> RunAsync(
        string executable, IReadOnlyList<string> arguments, string workingDirectory,
        TimeSpan timeout, CancellationToken cancellation = default)
    {
        var started = DateTimeOffset.UtcNow;
        var clock = Stopwatch.StartNew();
        var result = new JsonObject
        {
            ["executable"] = executable,
            ["arguments"] = new JsonArray(arguments.Select(x => (JsonNode?)JsonValue.Create(x)).ToArray()),
            ["working_directory"] = workingDirectory,
            ["exit_code"] = null, ["timed_out"] = false, ["cancelled"] = false,
            ["stdout"] = "", ["stderr"] = "", ["output_truncated"] = false,
            ["pid"] = null, ["started_at"] = started.ToString("O"),
            ["finished_at"] = null, ["duration_ms"] = 0,
            ["peak_working_set_bytes"] = null, ["error_code"] = null,
            ["timeout_ms"] = timeout.TotalMilliseconds,
            ["capture_limit_bytes_per_stream"] = CaptureLimitBytes,
            ["process_tree_cleanup"] = "not_required",
            ["memory_measurement"] = "sampled_root_process_only"
        };
        JsonObject Finish(string? error = null)
        {
            if (error is not null) result["error_code"] = error;
            result["finished_at"] = DateTimeOffset.UtcNow.ToString("O");
            result["duration_ms"] = clock.Elapsed.TotalMilliseconds;
            return result;
        }
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromDays(1))
            return Finish("INVALID_TIMEOUT");
        if (cancellation.IsCancellationRequested)
        {
            result["cancelled"] = true;
            return Finish("CANCELLED");
        }
        if (!Path.IsPathFullyQualified(executable) || !File.Exists(executable))
            return Finish("EXECUTABLE_NOT_FOUND");
        if (!Path.IsPathFullyQualified(workingDirectory) || !Directory.Exists(workingDirectory))
            return Finish("WORKING_DIRECTORY_NOT_FOUND");

        var start = new ProcessStartInfo(executable)
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        start.Environment["TEMP"] = workingDirectory;
        start.Environment["TMP"] = workingDirectory;

        using var process = new Process { StartInfo = start };
        try
        {
            if (!process.Start()) return Finish("PROCESS_START_FAILED");
        }
        catch (Exception exception) when (exception is Win32Exception or IOException or InvalidOperationException or UnauthorizedAccessException or ArgumentException)
        {
            result["start_error_type"] = exception.GetType().Name;
            return Finish("PROCESS_START_FAILED");
        }
        result["pid"] = process.Id;
        var stdout = new BoundedCapture();
        var stderr = new BoundedCapture();
        using var pipeCancellation = new CancellationTokenSource();
        var outputTask = CaptureAsync(process.StandardOutput, stdout, pipeCancellation.Token);
        var errorTask = CaptureAsync(process.StandardError, stderr, pipeCancellation.Token);
        using var samplingCancellation = new CancellationTokenSource();
        long peak = 0;
        var sampling = SampleMemoryAsync(process, value => peak = Math.Max(peak, value), samplingCancellation.Token);

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        deadline.CancelAfter(timeout);
        try
        {
            await process.WaitForExitAsync(deadline.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            bool cancelled = cancellation.IsCancellationRequested;
            result["cancelled"] = cancelled;
            result["timed_out"] = !cancelled;
            result["error_code"] = cancelled ? "CANCELLED" : "PROCESS_TIMEOUT";
            try
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                // Root exit alone does not prove all descendants have exited.
                result["process_tree_cleanup"] = "termination_requested";
                await process.WaitForExitAsync().WaitAsync(CleanupLimit).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is Win32Exception or InvalidOperationException or AggregateException or TimeoutException or NotSupportedException)
            {
                result["process_tree_cleanup"] = "incomplete";
                result["cleanup_error_type"] = exception.GetType().Name;
            }
        }
        finally
        {
            samplingCancellation.Cancel();
            await sampling.ConfigureAwait(false);
        }

        bool incompleteCapture = false;
        try
        {
            // A descendant may inherit a pipe after the direct child has exited.
            await Task.WhenAll(outputTask, errorTask).WaitAsync(PipeDrainLimit).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            incompleteCapture = true;
            pipeCancellation.Cancel();
            process.StandardOutput.Close();
            process.StandardError.Close();
            if (result["error_code"] is null) result["error_code"] = "OUTPUT_CAPTURE_INCOMPLETE";
        }
        result["stdout"] = stdout.Text;
        result["stderr"] = stderr.Text;
        result["output_truncated"] = stdout.Truncated || stderr.Truncated || incompleteCapture;
        result["stdout_capture_bytes"] = stdout.Length;
        result["stderr_capture_bytes"] = stderr.Length;
        result["peak_working_set_bytes"] = peak > 0 ? JsonValue.Create(peak) : null;
        try
        {
            if (process.HasExited)
            {
                result["exit_code"] = process.ExitCode;
                if (process.ExitCode != 0 && result["error_code"] is null)
                    result["error_code"] = "PROCESS_NONZERO_EXIT";
            }
        }
        catch (InvalidOperationException) { }
        return Finish();
    }

    private static async Task CaptureAsync(StreamReader reader, BoundedCapture capture, CancellationToken token)
    {
        var buffer = new char[4096];
        try
        {
            while (true)
            {
                int read = await reader.ReadAsync(buffer.AsMemory(), token).ConfigureAwait(false);
                if (read == 0) return;
                capture.Append(buffer.AsSpan(0, read));
            }
        }
        catch (Exception exception) when (exception is IOException or ObjectDisposedException or OperationCanceledException)
        {
            capture.MarkTruncated();
        }
    }

    private static async Task SampleMemoryAsync(Process process, Action<long> sample, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                try { process.Refresh(); sample(process.PeakWorkingSet64); }
                catch (Exception exception) when (exception is InvalidOperationException or Win32Exception) { return; }
                await Task.Delay(25, token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) { }
    }

    private sealed class BoundedCapture
    {
        private readonly StringBuilder content = new();
        private readonly object sync = new();
        private int length;
        private bool truncated;
        public string Text { get { lock (sync) return content.ToString(); } }
        public int Length { get { lock (sync) return length; } }
        public bool Truncated { get { lock (sync) return truncated; } }
        public void MarkTruncated() { lock (sync) truncated = true; }
        public void Append(ReadOnlySpan<char> text)
        {
            lock (sync)
            {
                // Once a prefix is truncated, later output must not replace its omitted middle.
                if (truncated) return;
                int bytes = Encoding.UTF8.GetByteCount(text);
                if (length + bytes <= CaptureLimitBytes)
                {
                    content.Append(text);
                    length += bytes;
                    return;
                }
                int count = text.Length;
                while (count > 0 && Encoding.UTF8.GetByteCount(text[..count]) > CaptureLimitBytes - length) count--;
                if (count > 0 && char.IsHighSurrogate(text[count - 1])) count--;
                content.Append(text[..count]);
                length += Encoding.UTF8.GetByteCount(text[..count]);
                truncated = true;
            }
        }
    }
}
