using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using Hybrid.Core;

if (args.Length >= 2 && args[0] == "--child")
{
    Console.OutputEncoding = Encoding.UTF8;
    switch (args[1])
    {
        case "echo": Console.Write(args[2]); return 0;
        case "nonzero": Console.Error.Write("controlled failure"); return 17;
        case "large":
            for (int i = 0; i < 128; i++) { Console.Write(new string('é', 2048)); Console.Error.Write(new string('界', 2048)); }
            return 0;
        case "sleep": await Task.Delay(TimeSpan.FromSeconds(20)); return 0;
        case "tree":
            var start = ChildStart("sleep");
            var child = Process.Start(start)!;
            Console.WriteLine(child.Id); Console.Out.Flush();
            await Task.Delay(TimeSpan.FromSeconds(20)); return 0;
        case "orphan-pipe":
            var orphan = Process.Start(ChildStart("short-sleep"))!;
            Console.WriteLine(orphan.Id); Console.Out.Flush(); return 0;
        case "short-sleep": await Task.Delay(TimeSpan.FromSeconds(5)); Console.Write("child finished"); return 0;
        default: return 19;
    }
}

var workspace = Path.GetFullPath(args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, ".local", "process-tests"));
Directory.CreateDirectory(workspace);
var testRoot = Path.Combine(workspace, "process tests " + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(testRoot);
var results = new JsonArray();
async Task Test(string name, Func<Task> action)
{
    var clock = Stopwatch.StartNew();
    try { await action(); results.Add(new JsonObject { ["name"] = name, ["outcome"] = "PASS", ["duration_ms"] = clock.Elapsed.TotalMilliseconds }); }
    catch (Exception exception) { results.Add(new JsonObject { ["name"] = name, ["outcome"] = "FAIL", ["error"] = exception.Message, ["duration_ms"] = clock.Elapsed.TotalMilliseconds }); }
}
void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
string Work(string name) { var path = Path.Combine(testRoot, name); Directory.CreateDirectory(path); return path; }
Task<JsonObject> Run(string mode, TimeSpan timeout, CancellationToken token = default, params string[] extra)
{
    var start = ChildStart(mode, extra);
    return ProcessRunner.RunAsync(start.FileName, start.ArgumentList.ToArray(), testRoot, timeout, token);
}

await Test("explicit_arguments_and_paths_with_spaces", async () =>
{
    const string value = "literal \"quote\" ; $nothing & folder with spaces\nsecond line";
    var receipt = await Run("echo", TimeSpan.FromSeconds(10), default, value);
    Check(receipt["exit_code"]?.GetValue<int>() == 0, "Child failed");
    Check(receipt["stdout"]?.GetValue<string>() == value, "Argument boundaries changed");
    Check(receipt["error_code"] is null, "Unexpected process error");
});
await Test("nonzero_exit_and_stderr", async () =>
{
    var receipt = await Run("nonzero", TimeSpan.FromSeconds(10));
    Check(receipt["exit_code"]?.GetValue<int>() == 17, "Exit code lost");
    Check(receipt["stderr"]?.GetValue<string>() == "controlled failure", "stderr lost");
    Check(receipt["error_code"]?.GetValue<string>() == "PROCESS_NONZERO_EXIT", "Wrong classification");
});
await Test("missing_executable", async () =>
{
    var receipt = await ProcessRunner.RunAsync(Path.Combine(testRoot, "missing.exe"), [], testRoot, TimeSpan.FromSeconds(1));
    Check(receipt["error_code"]?.GetValue<string>() == "EXECUTABLE_NOT_FOUND" && receipt["pid"] is null, "Missing executable started");
});
await Test("invalid_timeout", async () =>
{
    var receipt = await Run("echo", TimeSpan.Zero, default, "no execution");
    Check(receipt["error_code"]?.GetValue<string>() == "INVALID_TIMEOUT" && receipt["pid"] is null, "Invalid timeout accepted");
});
await Test("missing_working_directory", async () =>
{
    var start = ChildStart("echo", ["no execution"]);
    var receipt = await ProcessRunner.RunAsync(start.FileName, start.ArgumentList.ToArray(), Path.Combine(testRoot, "absent"), TimeSpan.FromSeconds(1));
    Check(receipt["error_code"]?.GetValue<string>() == "WORKING_DIRECTORY_NOT_FOUND", "Invalid working directory accepted");
});
await Test("bounded_utf8_stream_capture_drains", async () =>
{
    var receipt = await Run("large", TimeSpan.FromSeconds(10));
    Check(receipt["exit_code"]?.GetValue<int>() == 0 && receipt["timed_out"]?.GetValue<bool>() == false, "Output pipe deadlocked");
    Check(receipt["output_truncated"]?.GetValue<bool>() == true, "Truncation not reported");
    Check(Encoding.UTF8.GetByteCount(receipt["stdout"]!.GetValue<string>()) <= ProcessRunner.CaptureLimitBytes, "stdout overflow");
    Check(Encoding.UTF8.GetByteCount(receipt["stderr"]!.GetValue<string>()) <= ProcessRunner.CaptureLimitBytes, "stderr overflow");
});
await Test("timeout_terminates_owned_child", async () =>
{
    var receipt = await Run("sleep", TimeSpan.FromMilliseconds(200));
    Check(receipt["timed_out"]?.GetValue<bool>() == true && receipt["cancelled"]?.GetValue<bool>() == false, "Timeout classification incorrect");
    Check(receipt["duration_ms"]!.GetValue<double>() < 8000, "Timeout cleanup unbounded");
    Check(!Running(receipt["pid"]!.GetValue<int>()), "Owned root survived timeout");
});
await Test("cancellation_terminates_owned_child", async () =>
{
    using var cancellation = new CancellationTokenSource(200);
    var receipt = await Run("sleep", TimeSpan.FromSeconds(10), cancellation.Token);
    Check(receipt["cancelled"]?.GetValue<bool>() == true && receipt["timed_out"]?.GetValue<bool>() == false, "Cancellation classification incorrect");
    if (receipt["pid"] is not null) Check(!Running(receipt["pid"]!.GetValue<int>()), "Owned root survived cancellation");
});
await Test("pre_cancelled_request_starts_no_child", async () =>
{
    using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
    var receipt = await Run("sleep", TimeSpan.FromSeconds(10), cancellation.Token);
    Check(receipt["cancelled"]?.GetValue<bool>() == true && receipt["pid"] is null, "Pre-cancelled request started process");
});
await Test("timeout_terminates_owned_descendant", async () =>
{
    var receipt = await Run("tree", TimeSpan.FromMilliseconds(500));
    Check(receipt["timed_out"]?.GetValue<bool>() == true, "Tree did not time out");
    Check(int.TryParse(receipt["stdout"]!.GetValue<string>().Trim(), out int childPid), "No owned descendant receipt");
    await Task.Delay(100);
    Check(!Running(childPid), "Owned descendant survived timeout");
});
await Test("inherited_pipes_do_not_hang_runner", async () =>
{
    var receipt = await Run("orphan-pipe", TimeSpan.FromSeconds(10));
    Check(receipt["duration_ms"]!.GetValue<double>() < 8000, "Inherited pipe stalled runner");
    // The root exits before its child: either pipes close normally or incomplete capture is explicit.
    Check(receipt["error_code"] is null || receipt["error_code"]!.GetValue<string>() == "OUTPUT_CAPTURE_INCOMPLETE", "Unexpected inherited-pipe outcome");
    if (int.TryParse(receipt["stdout"]!.GetValue<string>().Trim(), out int childPid) && Running(childPid))
    {
        // Only this test's reported child is eligible; no user process enumeration or termination.
        using var child = Process.GetProcessById(childPid);
        if (child.StartTime.ToUniversalTime() >= DateTime.Parse(receipt["started_at"]!.GetValue<string>()).ToUniversalTime().AddSeconds(-1)) child.Kill(true);
    }
});
await Test("managed_adapter_real_transformation", async () =>
{
    var work = Work("text transformation");
    var source = Path.Combine(work, "source input.txt"); var output = Path.Combine(work, "output normalized.txt");
    await File.WriteAllTextAsync(source, "Cafe\u0301\r\nsecond\rthird\n", new UTF8Encoding(false));
    var original = JsonData.HashFile(source);
    var receipt = await new TextNormalizeAdapter().ExecuteAsync(source, output, work, TimeSpan.FromSeconds(5));
    Check(receipt["exit_code"]?.GetValue<int>() == 0 && receipt["error_code"] is null, "Managed transform failed");
    Check(await File.ReadAllTextAsync(output) == "Café\nsecond\nthird\n", "Transformation content incorrect");
    Check(JsonData.HashFile(source) == original, "Original source changed");
    Check(!receipt.ContainsKey("verification") && !receipt.ContainsKey("VERIFIED"), "Adapter claimed independent verification");
});
await Test("managed_adapter_rejects_existing_destination", async () =>
{
    var work = Work("existing destination"); var source = Path.Combine(work, "input.txt"); var output = Path.Combine(work, "output.txt");
    await File.WriteAllTextAsync(source, "source"); await File.WriteAllTextAsync(output, "preserve this");
    var receipt = await new TextNormalizeAdapter().ExecuteAsync(source, output, work, TimeSpan.FromSeconds(5));
    Check(receipt["error_code"]?.GetValue<string>() == "DESTINATION_EXISTS", "Existing destination accepted");
    Check(await File.ReadAllTextAsync(output) == "preserve this", "Existing destination overwritten");
});
await Test("managed_adapter_rejects_non_utf8", async () =>
{
    var work = Work("invalid UTF8"); var source = Path.Combine(work, "input.txt"); var output = Path.Combine(work, "output.txt");
    await File.WriteAllBytesAsync(source, [0xff, 0xfe, 0x61, 0x00]);
    var receipt = await new TextNormalizeAdapter().ExecuteAsync(source, output, work, TimeSpan.FromSeconds(5));
    Check(receipt["error_code"]?.GetValue<string>() == "INVALID_UTF8" && !File.Exists(output), "Invalid UTF-8 accepted");
});
await Test("managed_adapter_rejects_bom_without_silent_content_change", async () =>
{
    var work = Work("unsupported UTF8 BOM"); var source = Path.Combine(work, "input.txt"); var output = Path.Combine(work, "output.txt");
    await File.WriteAllBytesAsync(source, [0xef, 0xbb, 0xbf, 0x61, 0x0d, 0x0a]);
    var original = JsonData.HashFile(source);
    var receipt = await new TextNormalizeAdapter().ExecuteAsync(source, output, work, TimeSpan.FromSeconds(5));
    Check(receipt["error_code"]?.GetValue<string>() == "UNSUPPORTED_UTF8_BOM" && !File.Exists(output), "BOM input silently changed");
    Check(JsonData.HashFile(source) == original, "BOM source changed");
});
await Test("libreoffice_missing_executable_discovery", async () =>
{
    var receipt = await new LibreOfficeAdapter(Path.Combine(testRoot, "missing soffice.com"), Work("discovery")).DiscoverAsync();
    Check(receipt["application_installed"]?.GetValue<bool>() == false && receipt["adapter_registered"]?.GetValue<bool>() == true, "Installed and registered states conflated");
    Check(receipt["capability_verified"]?.GetValue<bool>() == false && receipt["availability"]?.GetValue<string>() == "unavailable", "Missing application claimed verified");
});
await Test("libreoffice_missing_source_no_process", async () =>
{
    var work = Work("missing source");
    var receipt = await new LibreOfficeAdapter(Path.Combine(testRoot, "missing soffice.com")).ExecuteAsync(Path.Combine(work, "missing.docx"), Path.Combine(work, "output.pdf"), work, TimeSpan.FromSeconds(5));
    Check(receipt["error_code"]?.GetValue<string>() == "SOURCE_NOT_FOUND" && receipt["pid"] is null, "Missing source dispatched");
});

var report = new JsonObject
{
    ["suite"] = "process-and-adapter", ["kind"] = "real_owned_child_process_and_managed_adapter_tests",
    ["real_libreoffice_conversion"] = false, ["timestamp"] = DateTimeOffset.UtcNow.ToString("O"),
    ["count"] = results.Count, ["passed"] = results.Count(r => r!["outcome"]!.GetValue<string>() == "PASS"),
    ["failed"] = results.Count(r => r!["outcome"]!.GetValue<string>() == "FAIL"), ["results"] = results
};
JsonData.WriteNew(Path.Combine(testRoot, "results.json"), report);
Console.WriteLine(report.ToJsonString(JsonData.Options));
return report["failed"]!.GetValue<int>() == 0 ? 0 : 1;

static bool Running(int pid) { try { using var process = Process.GetProcessById(pid); return !process.HasExited; } catch (ArgumentException) { return false; } }
static ProcessStartInfo ChildStart(string mode, params string[] extra)
{
    var start = new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = false, CreateNoWindow = true };
    if (Path.GetFileNameWithoutExtension(start.FileName).Equals("dotnet", StringComparison.OrdinalIgnoreCase)) start.ArgumentList.Add(typeof(ProcessRunner).Assembly.Location.Replace("Hybrid.Core.dll", "Hybrid.ProcessTests.dll"));
    start.ArgumentList.Add("--child"); start.ArgumentList.Add(mode);
    foreach (var item in extra) start.ArgumentList.Add(item);
    return start;
}
