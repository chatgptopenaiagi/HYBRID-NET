using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace Hybrid.Core;

public interface ICapabilityAdapter
{
    string Capability { get; }
    string Application { get; }
    string Id { get; }
    string Version { get; }
    Task<JsonObject> DiscoverAsync(CancellationToken token = default);
    Task<JsonObject> ExecuteAsync(string source, string destination, string runDirectory,
        TimeSpan timeout, CancellationToken token = default);
}

/// <summary>An explicit supported LibreOffice CLI integration, not universal application discovery.</summary>
public sealed class LibreOfficeAdapter : ICapabilityAdapter
{
    private readonly string executable;
    private readonly string discoveryDirectory;
    public string Capability => "document.export_pdf";
    public string Application => "LibreOffice";
    public string Id => "libreoffice.pdf-export";
    public string Version => "0.2.0";

    public LibreOfficeAdapter(string executable, string? discoveryDirectory = null)
    {
        this.executable = executable;
        this.discoveryDirectory = discoveryDirectory ?? Path.Combine(AppContext.BaseDirectory, ".local", "discovery");
    }

    public async Task<JsonObject> DiscoverAsync(CancellationToken token = default)
    {
        bool installed = Path.IsPathFullyQualified(executable) && File.Exists(executable);
        var description = AdapterMetadata.Description(this, executable, installed,
            "LibreOffice CLI headless", ".docx", ".pdf");
        description["verification_requirements"] = new JsonArray("AC-01", "AC-02", "AC-03", "AC-04", "AC-05", "AC-06", "AC-07", "AC-08", "AC-09", "AC-10");
        description["limitations"] = new JsonArray(
            "Only the explicitly implemented DOCX-to-PDF adapter is registered.",
            "A current version probe proves executable access, not successful document conversion.",
            "Independent acceptance is required; no pixel-perfect rendering or OS sandbox claim.",
            "LibreOffice is an external application dependency; no user profile is reused.");
        if (!installed)
        {
            description["availability"] = "unavailable";
            description["error_code"] = "EXECUTABLE_NOT_FOUND";
            return description;
        }
        try
        {
            var probeDirectory = Path.Combine(discoveryDirectory, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(probeDirectory);
            string profileUri = AdapterMetadata.ProfileUri(Path.Combine(probeDirectory, "profile"));
            var probe = await ProcessRunner.RunAsync(executable,
                [$"-env:UserInstallation={profileUri}", "--headless", "--version"],
                probeDirectory, TimeSpan.FromSeconds(15), token).ConfigureAwait(false);
            description["version_probe"] = probe;
            string version = (probe["stdout"]?.GetValue<string>() ?? "").Trim();
            if (version.Length > 512) version = version[..512];
            bool identified = probe["exit_code"]?.GetValue<int>() == 0 && probe["error_code"] is null
                && version.StartsWith("LibreOffice ", StringComparison.OrdinalIgnoreCase);
            description["application_version"] = identified ? version : null;
            description["availability"] = identified ? "available_untested" : "unavailable";
            description["error_code"] = identified ? null : probe["error_code"]?.DeepClone() ?? JsonValue.Create("APPLICATION_IDENTITY_UNCONFIRMED");
            description["executable_sha256"] = JsonData.HashFile(executable);
            description["environment_key"] = JsonData.Hash($"{Path.GetFullPath(executable)}\n{version}\n{Id}\n{Version}\n{description["executable_sha256"]}");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            description["availability"] = "unavailable";
            description["error_code"] = "DISCOVERY_FAILED";
            description["error_type"] = exception.GetType().Name;
        }
        return description;
    }

    public async Task<JsonObject> ExecuteAsync(string source, string destination, string runDirectory,
        TimeSpan timeout, CancellationToken token = default)
    {
        var metadata = AdapterMetadata.Execution(this, "LibreOffice CLI headless", source, destination);
        var clock = Stopwatch.StartNew();
        JsonObject Fail(string code)
        {
            metadata["error_code"] = code;
            metadata["finished_at"] = DateTimeOffset.UtcNow.ToString("O");
            metadata["duration_ms"] = clock.Elapsed.TotalMilliseconds;
            metadata["output_exists"] = File.Exists(destination);
            return metadata;
        }
        if (token.IsCancellationRequested) { metadata["cancelled"] = true; return Fail("CANCELLED"); }
        var pathError = AdapterMetadata.ValidatePaths(source, destination, runDirectory);
        if (pathError is not null) return Fail(pathError);
        if (!Path.GetExtension(source).Equals(".docx", StringComparison.OrdinalIgnoreCase)) return Fail("INVALID_INPUT_EXTENSION");
        if (!Path.GetExtension(destination).Equals(".pdf", StringComparison.OrdinalIgnoreCase)) return Fail("INVALID_OUTPUT_EXTENSION");
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromDays(1)) return Fail("INVALID_TIMEOUT");
        try
        {
            var discovery = await DiscoverAsync(token).ConfigureAwait(false);
            metadata["application_version"] = discovery["application_version"]?.DeepClone();
            metadata["executable"] = executable;
            metadata["executable_sha256"] = discovery["executable_sha256"]?.DeepClone();
            metadata["environment_key"] = discovery["environment_key"]?.DeepClone();
            metadata["version_probe"] = discovery["version_probe"]?.DeepClone();
            if (discovery["availability"]?.GetValue<string>() != "available_untested")
            {
                if (token.IsCancellationRequested) { metadata["cancelled"] = true; return Fail("CANCELLED"); }
                return Fail("APPLICATION_UNAVAILABLE");
            }

            string inputDirectory = Path.Combine(runDirectory, "input");
            string convertedDirectory = Path.Combine(runDirectory, "converted");
            string profileDirectory = Path.Combine(runDirectory, "profile");
            // Refuse previous conversion state; every invocation gets an independent profile and output.
            if (Directory.Exists(inputDirectory) || Directory.Exists(convertedDirectory) || Directory.Exists(profileDirectory))
                return Fail("RUN_DIRECTORY_NOT_FRESH");
            Directory.CreateDirectory(inputDirectory);
            Directory.CreateDirectory(convertedDirectory);
            Directory.CreateDirectory(profileDirectory);
            string inputCopy = Path.Combine(inputDirectory, Path.GetFileName(source));
            metadata["source_sha256_before"] = JsonData.HashFile(source);
            File.Copy(source, inputCopy, overwrite: false);
            metadata["staged_source_sha256"] = JsonData.HashFile(inputCopy);
            if (metadata["source_sha256_before"]!.GetValue<string>() != metadata["staged_source_sha256"]!.GetValue<string>())
                return Fail("SOURCE_CHANGED_DURING_STAGING");
            string profileUri = AdapterMetadata.ProfileUri(profileDirectory);
            metadata["profile_uri"] = profileUri;
            metadata["staged_source_path"] = inputCopy;
            metadata["converted_directory"] = convertedDirectory;
            string[] arguments = [$"-env:UserInstallation={profileUri}", "--headless", "--nologo", "--nodefault", "--norestore",
                "--convert-to", "pdf:writer_pdf_Export", "--outdir", convertedDirectory, inputCopy];
            var process = await ProcessRunner.RunAsync(executable, arguments, runDirectory, timeout, token).ConfigureAwait(false);
            foreach (var field in process) metadata[field.Key] = field.Value?.DeepClone();
            // Keep total adapter duration separate from the measured conversion subprocess duration.
            metadata["adapter_duration_ms"] = clock.Elapsed.TotalMilliseconds;
            if (process["error_code"] is not null) return metadata;
            string converted = Path.Combine(convertedDirectory, Path.GetFileNameWithoutExtension(inputCopy) + ".pdf");
            metadata["converted_path"] = converted;
            if (!File.Exists(converted)) return Fail("OUTPUT_MISSING");
            if (token.IsCancellationRequested) { metadata["cancelled"] = true; return Fail("CANCELLED"); }
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Move(converted, destination, overwrite: false);
            metadata["output_exists"] = true;
            metadata["output_size_bytes"] = new FileInfo(destination).Length;
            metadata["output_sha256"] = JsonData.HashFile(destination);
            metadata["source_sha256_after"] = JsonData.HashFile(source);
            metadata["adapter_duration_ms"] = clock.Elapsed.TotalMilliseconds;
            if (metadata["source_sha256_before"]!.GetValue<string>() != metadata["source_sha256_after"]!.GetValue<string>())
                return Fail("SOURCE_CHANGED_DURING_EXECUTION");
            return metadata;
        }
        catch (OperationCanceledException) { metadata["cancelled"] = true; return Fail("CANCELLED"); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            metadata["error_type"] = exception.GetType().Name;
            return Fail(File.Exists(destination) ? "OUTPUT_CONFLICT_OR_IO_FAILURE" : "ADAPTER_IO_FAILURE");
        }
    }
}

/// <summary>A second genuine capability using managed code instead of the LibreOffice process adapter.</summary>
public sealed class TextNormalizeAdapter : ICapabilityAdapter
{
    public string Capability => "text.normalize";
    public string Application => "Hybrid.Text";
    public string Id => "managed.text-normalize";
    public string Version => "0.2.0";
    public const long MaximumInputBytes = 16 * 1024 * 1024;

    public Task<JsonObject> DiscoverAsync(CancellationToken token = default)
    {
        var description = AdapterMetadata.Description(this, Environment.ProcessPath, true, "managed text transform", "UTF-8 text", ".txt");
        description["application_version"] = Version;
        description["runtime_version"] = Environment.Version.ToString();
        description["availability"] = "available_untested";
        description["environment_key"] = JsonData.Hash($"{Id}\n{Version}\n{Environment.Version}\n{JsonData.HashFile(typeof(TextNormalizeAdapter).Assembly.Location)}");
        description["verification_requirements"] = new JsonArray("Independent UTF-8 decoding, NFC and line-ending checks", "Original source SHA-256 preserved", "Durable SQLite readback");
        description["limitations"] = new JsonArray("UTF-8 text without BOM only, maximum 16 MiB", "NFC normalization and CRLF/CR to LF; all other content preserved", "No arbitrary scripts or command execution");
        return Task.FromResult(description);
    }

    public async Task<JsonObject> ExecuteAsync(string source, string destination, string runDirectory,
        TimeSpan timeout, CancellationToken token = default)
    {
        var clock = Stopwatch.StartNew();
        var result = AdapterMetadata.Execution(this, "managed text transform", source, destination);
        result["application_version"] = Version;
        result["runtime_version"] = Environment.Version.ToString();
        result["arguments"] = new JsonArray();
        result["memory_measurement"] = "not_measured_in_process";
        JsonObject Finish(string? error = null)
        {
            result["error_code"] = error;
            result["finished_at"] = DateTimeOffset.UtcNow.ToString("O");
            result["duration_ms"] = clock.Elapsed.TotalMilliseconds;
            result["output_exists"] = File.Exists(destination);
            return result;
        }
        string? pathError = AdapterMetadata.ValidatePaths(source, destination, runDirectory);
        if (pathError is not null) return Finish(pathError);
        if (timeout <= TimeSpan.Zero || timeout > TimeSpan.FromDays(1)) return Finish("INVALID_TIMEOUT");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
        deadline.CancelAfter(timeout);
        try
        {
            deadline.Token.ThrowIfCancellationRequested();
            if (new FileInfo(source).Length > MaximumInputBytes) return Finish("INPUT_TOO_LARGE");
            result["source_sha256_before"] = JsonData.HashFile(source);
            byte[] bytes = await File.ReadAllBytesAsync(source, deadline.Token).ConfigureAwait(false);
            if (bytes.Length > MaximumInputBytes) return Finish("INPUT_TOO_LARGE");
            // Decode UTF-8 explicitly: StreamReader's BOM autodetection would also accept UTF-16.
            if (bytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf })) return Finish("UNSUPPORTED_UTF8_BOM");
            string text = new UTF8Encoding(false, true).GetString(bytes);
            string normalized = text.Normalize(NormalizationForm.FormC).Replace("\r\n", "\n").Replace("\r", "\n");
            byte[] output = new UTF8Encoding(false, true).GetBytes(normalized);
            deadline.Token.ThrowIfCancellationRequested();
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            await using (var file = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await file.WriteAsync(output, deadline.Token).ConfigureAwait(false);
                await file.FlushAsync(deadline.Token).ConfigureAwait(false);
            }
            result["source_sha256_after"] = JsonData.HashFile(source);
            result["output_sha256"] = JsonData.HashFile(destination);
            result["output_size_bytes"] = output.Length;
            if (result["source_sha256_before"]!.GetValue<string>() != result["source_sha256_after"]!.GetValue<string>())
                return Finish("SOURCE_CHANGED_DURING_EXECUTION");
            result["exit_code"] = 0;
            return Finish();
        }
        catch (OperationCanceledException)
        {
            result["cancelled"] = token.IsCancellationRequested;
            result["timed_out"] = !token.IsCancellationRequested;
            return Finish(token.IsCancellationRequested ? "CANCELLED" : "EXECUTION_TIMEOUT");
        }
        catch (DecoderFallbackException) { return Finish("INVALID_UTF8"); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            result["error_type"] = exception.GetType().Name;
            return Finish(File.Exists(destination) ? "OUTPUT_CONFLICT_OR_IO_FAILURE" : "ADAPTER_IO_FAILURE");
        }
    }
}

internal static class AdapterMetadata
{
    public static string ProfileUri(string directory) => new Uri(Path.GetFullPath(directory) + Path.DirectorySeparatorChar).AbsoluteUri;
    public static JsonObject Description(ICapabilityAdapter adapter, string? executable, bool installed,
        string executionInterface, string input, string output) => new()
    {
        ["capability"] = adapter.Capability, ["application"] = adapter.Application,
        ["application_installed"] = installed, ["adapter_registered"] = true,
        ["capability_verified"] = false, ["last_successful_verification"] = null,
        ["executable"] = executable, ["application_version"] = null,
        ["adapter_id"] = adapter.Id, ["adapter_version"] = adapter.Version,
        ["interface"] = executionInterface, ["contract_version"] = "0.1",
        ["input_schema"] = new JsonObject
        {
            ["type"] = "object", ["required"] = new JsonArray("source", "destination"),
            ["additionalProperties"] = false,
            ["properties"] = new JsonObject
            {
                ["source"] = new JsonObject { ["type"] = "string", ["format"] = "absolute-path", ["content"] = input },
                ["destination"] = new JsonObject { ["type"] = "string", ["format"] = "absolute-path", ["content"] = output }
            }
        },
        ["output_schema"] = new JsonObject
        {
            ["type"] = "object", ["required"] = new JsonArray("output_path", "output_exists", "error_code"),
            ["properties"] = new JsonObject
            {
                ["output_path"] = new JsonObject { ["type"] = "string", ["format"] = "absolute-path" },
                ["output_exists"] = new JsonObject { ["type"] = "boolean" },
                ["error_code"] = new JsonObject { ["type"] = new JsonArray("string", "null") }
            }
        },
        ["observed_at"] = DateTimeOffset.UtcNow.ToString("O")
    };

    public static JsonObject Execution(ICapabilityAdapter adapter, string executionInterface, string source, string destination) => new()
    {
        ["application"] = adapter.Application, ["capability"] = adapter.Capability,
        ["adapter_id"] = adapter.Id, ["adapter_version"] = adapter.Version,
        ["interface"] = executionInterface, ["source_path"] = source, ["output_path"] = destination,
        ["output_exists"] = false, ["exit_code"] = null, ["timed_out"] = false, ["cancelled"] = false,
        ["stdout"] = "", ["stderr"] = "", ["output_truncated"] = false,
        ["pid"] = null, ["started_at"] = DateTimeOffset.UtcNow.ToString("O"),
        ["finished_at"] = null, ["duration_ms"] = 0, ["peak_working_set_bytes"] = null, ["error_code"] = null
    };

    public static string? ValidatePaths(string source, string destination, string runDirectory)
    {
        if (!Path.IsPathFullyQualified(source) || !Path.IsPathFullyQualified(destination) || !Path.IsPathFullyQualified(runDirectory)) return "INVALID_PATH";
        if (!File.Exists(source)) return "SOURCE_NOT_FOUND";
        if (string.Equals(Path.GetFullPath(source), Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase)) return "SOURCE_DESTINATION_CONFLICT";
        if (File.Exists(destination) || Directory.Exists(destination)) return "DESTINATION_EXISTS";
        if (!Directory.Exists(runDirectory)) return "WORKING_DIRECTORY_NOT_FOUND";
        return null;
    }
}
