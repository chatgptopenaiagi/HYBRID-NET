using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Nodes;
using Hybrid.Core;
using Microsoft.Data.Sqlite;

// This is a dependency-free console test runner. Only disposable databases below
// the chosen repository-local .local directory are touched. Child processes are
// this test executable itself, never LibreOffice or another user's application.
if (args.Length == 2 && args[0] == "--abandon")
{
    var childStore = new EvidenceStore(args[1]);
    childStore.Reserve("abandoned", "request-hash", new JsonObject { ["capability"] = "test.capability" }, System.IO.Path.GetDirectoryName(args[1])!);
    childStore.AppendEvent("abandoned", "RUNNING", "V1", new JsonObject { ["event"] = "TEST_OWNER_STARTED" });
    return 0;
}
if (args.Length == 2 && args[0] == "--hold-uncommitted")
{
    using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = args[1], Pooling = false }.ToString());
    connection.Open();
    using var transaction = connection.BeginTransaction();
    using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText = "UPDATE operations SET state='VERIFYING',level='V4' WHERE id='crash-window'";
    command.ExecuteNonQuery();
    Console.WriteLine("UNCOMMITTED_READY");
    Console.Out.Flush();
    Thread.Sleep(Timeout.Infinite);
    return 2;
}

var root = args.Length == 2 && args[0] == "--root"
    ? System.IO.Path.GetFullPath(args[1])
    : System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "../../../../../.local/store-tests"));
Directory.CreateDirectory(root);
var run = System.IO.Path.Combine(root, DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff") + "-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(run);
var results = new JsonArray();
var total = Stopwatch.StartNew();

void Check(bool condition, string description)
{
    if (!condition) throw new InvalidOperationException(description);
}
void Expect<T>(Action action, string? message = null) where T : Exception
{
    try { action(); }
    catch (T exception)
    {
        if (message is not null && !exception.Message.Contains(message, StringComparison.Ordinal)) throw;
        return;
    }
    throw new InvalidOperationException($"Expected {typeof(T).Name}");
}
JsonObject Request(string capability = "test.capability") => new() { ["capability"] = capability, ["source"] = "synthetic input only" };
JsonObject Receipt(string id, string outcome = "VERIFIED", string environment = "environment-one") => new()
{
    ["operation_id"] = id, ["capability"] = "test.capability", ["environment_key"] = environment,
    ["verification"] = new JsonObject { ["outcome"] = outcome, ["level"] = outcome == "VERIFIED" ? "V4" : "V3" }
};
EvidenceStore Store(string name) => new(System.IO.Path.Combine(run, name + ".sqlite3"));
object? Query(string database, string sql)
{
    using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = database, Pooling = false }.ToString());
    connection.Open();
    using var command = connection.CreateCommand();
    command.CommandText = sql;
    return command.ExecuteScalar();
}
async Task<Process> Child(string mode, string database)
{
    var host = Environment.ProcessPath ?? throw new InvalidOperationException("PROCESS_PATH_UNAVAILABLE");
    var start = new ProcessStartInfo(host) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
    if (System.IO.Path.GetFileNameWithoutExtension(host).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
        start.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
    start.ArgumentList.Add(mode);
    start.ArgumentList.Add(database);
    var process = Process.Start(start) ?? throw new InvalidOperationException("TEST_CHILD_START_FAILED");
    await Task.Yield();
    return process;
}
async Task Test(string name, Func<Task> action)
{
    var watch = Stopwatch.StartNew();
    try
    {
        await action();
        results.Add(new JsonObject { ["name"] = name, ["outcome"] = "passed", ["duration_ms"] = watch.Elapsed.TotalMilliseconds });
    }
    catch (Exception exception)
    {
        results.Add(new JsonObject { ["name"] = name, ["outcome"] = "failed", ["duration_ms"] = watch.Elapsed.TotalMilliseconds,
            ["exception"] = exception.GetType().Name, ["message"] = exception.Message });
    }
}
Task Sync(Action action) { action(); return Task.CompletedTask; }

await Test("reserve and immutable request survive a fresh store instance", () => Sync(() =>
{
    var store = Store("restart");
    Check(store.Reserve("restart", "hash", Request(), run), "first admission rejected");
    store.AppendEvent("restart", "VERIFIED", "V4", new JsonObject { ["event"] = "TEST_VERIFICATION" });
    store.Complete("restart", Receipt("restart"));
    var fresh = new EvidenceStore(store.Path);
    Check(fresh.GetReceipt("restart")?["verification"]?["outcome"]?.GetValue<string>() == "VERIFIED", "receipt lost after restart");
    Check(fresh.History().Count == 1, "operation history lost");
}));
await Test("duplicate identical requests do not replay or append events", () => Sync(() =>
{
    var store = Store("duplicate");
    store.Reserve("same", "hash", Request(), run);
    Check(!store.Reserve("same", "hash", Request(), run), "duplicate admitted");
    Check(store.GetReceipt("same")?["events"]?.AsArray().Count == 1, "duplicate wrote an event");
}));
await Test("conflicting operation identifiers preserve original request", () => Sync(() =>
{
    var store = Store("conflict");
    store.Reserve("same", "hash", Request(), run);
    Expect<InvalidOperationException>(() => store.Reserve("same", "other-hash", Request("different"), run), "OPERATION_ID_CONFLICT");
    Check(store.GetReceipt("same")?["request"]?["capability"]?.GetValue<string>() == "test.capability", "request overwritten");
}));
await Test("concurrent duplicate admission has one owner", async () =>
{
    var store = Store("concurrent");
    var admissions = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => Task.Run(() => store.Reserve("concurrent", "hash", Request(), run))));
    Check(admissions.Count(x => x) == 1, "concurrent duplicate admitted more than once");
    Check(store.GetReceipt("concurrent")?["events"]?.AsArray().Count == 1, "concurrent event duplication");
});
await Test("append events records ordered history and current state", () => Sync(() =>
{
    var store = Store("events");
    store.Reserve("events", "hash", Request(), run);
    store.AppendEvent("events", "VALIDATED", "V1", new JsonObject { ["event"] = "VALIDATED" });
    store.AppendEvent("events", "RUNNING", "V1", new JsonObject { ["event"] = "DISPATCHED" });
    var receipt = store.GetReceipt("events")!;
    Check(receipt["state"]!.GetValue<string>() == "RUNNING", "current state not updated");
    Check(receipt["events"]!.AsArray().Select(x => x!["sequence"]!.GetValue<long>()).SequenceEqual(new long[] { 1, 2, 3 }), "event order missing");
}));
await Test("invalid event level rolls back state and event atomically", () => Sync(() =>
{
    var store = Store("bad-event");
    store.Reserve("event", "hash", Request(), run);
    Expect<ArgumentException>(() => store.AppendEvent("event", "VERIFIED", "V99", new JsonObject()));
    var receipt = store.GetReceipt("event")!;
    Check(receipt["state"]!.GetValue<string>() == "CREATED", "invalid transition partially committed");
    Check(receipt["events"]!.AsArray().Count == 1, "invalid event committed");
}));
await Test("unknown operations cannot acquire events or verification", () => Sync(() =>
{
    var store = Store("unknown");
    Expect<KeyNotFoundException>(() => store.AppendEvent("missing", "RUNNING", "V1", new JsonObject()));
    Expect<KeyNotFoundException>(() => store.SaveVerification("missing", new JsonObject(), "initial"));
    Check(store.GetReceipt("missing") is null, "unknown receipt fabricated");
}));
await Test("verification records append without rewriting original conclusion", () => Sync(() =>
{
    var store = Store("reverify");
    store.Reserve("verified", "hash", Request(), run);
    var first = store.SaveVerification("verified", new JsonObject { ["outcome"] = "VERIFIED" }, "initial");
    store.Complete("verified", Receipt("verified"));
    var second = store.SaveVerification("verified", new JsonObject { ["outcome"] = "FAILED" }, "reverification");
    var receipt = store.GetReceipt("verified")!;
    Check(first != second, "verification ID reused");
    Check(receipt["verification"]!["outcome"]!.GetValue<string>() == "VERIFIED", "historical conclusion changed");
    Check(receipt["verification_history"]!.AsArray().Count == 2, "later verification lost");
}));
await Test("complete rejects replacement receipts", () => Sync(() =>
{
    var store = Store("complete");
    store.Reserve("complete", "hash", Request(), run);
    store.Complete("complete", Receipt("complete"));
    Expect<InvalidOperationException>(() => store.Complete("complete", Receipt("complete", "FAILED")), "ORIGINAL_RECEIPT_IMMUTABLE");
    Check(store.GetReceipt("complete")?["verification"]?["outcome"]?.GetValue<string>() == "VERIFIED", "receipt replaced");
}));
await Test("terminal receipt and V4 state commit together across restart", () => Sync(() =>
{
    var store = Store("atomic-finalization");
    store.Reserve("atomic", "hash", Request(), run);
    store.AppendEvent("atomic", "VERIFYING", "V3", new JsonObject { ["event"] = "INDEPENDENT_OBSERVATION" });
    var receipt = Receipt("atomic"); receipt["state"] = "VERIFIED";
    store.Complete("atomic", receipt);
    var fresh = new EvidenceStore(store.Path);
    var loaded = fresh.GetReceipt("atomic")!;
    Check(loaded["verification"]?["level"]?.GetValue<string>() == "V4", "terminal receipt missing after restart");
    Check(Convert.ToString(Query(store.Path, "SELECT state || ':' || level FROM operations WHERE id='atomic'")) == "VERIFIED:V4", "terminal current state not committed");
    Check(Convert.ToInt64(Query(store.Path, "SELECT count(*) FROM execution_events WHERE operation_id='atomic' AND level='V4' AND json_extract(data_json,'$.event')='TERMINAL_RECEIPT_COMMITTED'")) == 1, "terminal V4 event missing or duplicated");
}));
await Test("interrupted terminal event rolls back receipt and V4 promotion", () => Sync(() =>
{
    var store = Store("atomic-finalization-fault");
    store.Reserve("atomic-fault", "hash", Request(), run);
    store.AppendEvent("atomic-fault", "VERIFYING", "V3", new JsonObject { ["event"] = "INDEPENDENT_OBSERVATION" });
    store.SaveVerification("atomic-fault", new JsonObject { ["outcome"] = "VERIFIED", ["level"] = "V4" }, "initial");
    Query(store.Path, """
        CREATE TRIGGER inject_terminal_write_failure BEFORE INSERT ON execution_events
        WHEN json_extract(NEW.data_json,'$.event')='TERMINAL_RECEIPT_COMMITTED'
        BEGIN SELECT RAISE(ABORT,'TEST_FINALIZATION_INTERRUPTED'); END;
        """);
    var receipt = Receipt("atomic-fault"); receipt["state"] = "VERIFIED";
    Expect<SqliteException>(() => store.Complete("atomic-fault", receipt), "TEST_FINALIZATION_INTERRUPTED");
    var fresh = new EvidenceStore(store.Path);
    var pending = fresh.GetReceipt("atomic-fault")!;
    Check(pending["original_receipt_pending"]?.GetValue<bool>() == true, "failed transaction left terminal receipt");
    Check(pending["state"]?.GetValue<string>() == "VERIFYING" && pending["verification_level"]?.GetValue<string>() == "V3", "failed transaction left false current V4");
    Check(Convert.ToInt64(Query(store.Path, "SELECT receipt_json IS NULL FROM operations WHERE id='atomic-fault'")) == 1, "receipt update did not roll back");
    Check(Convert.ToInt64(Query(store.Path, "SELECT count(*) FROM execution_events WHERE operation_id='atomic-fault' AND level IN ('V4','V5')")) == 0, "false V4 event survived");
    Check(Convert.ToInt64(Query(store.Path, "SELECT count(*) FROM verification_results WHERE operation_id='atomic-fault'")) == 1, "preceding diagnostic evidence lost");
    Query(store.Path, "DROP TRIGGER inject_terminal_write_failure");
    fresh.Complete("atomic-fault", receipt);
    Check(fresh.GetReceipt("atomic-fault")?["verification"]?["level"]?.GetValue<string>() == "V4", "safe persistence retry failed");
    Check(Convert.ToInt64(Query(store.Path, "SELECT count(*) FROM execution_events WHERE operation_id='atomic-fault' AND level='V4'")) == 1, "retry duplicated terminal transition");
}));
await Test("database rejects updating original receipt events and verifications", () => Sync(() =>
{
    var store = Store("immutable-sql");
    store.Reserve("immutable", "hash", Request(), run);
    store.SaveVerification("immutable", new JsonObject { ["outcome"] = "FAILED" }, "initial");
    store.Complete("immutable", Receipt("immutable", "FAILED"));
    Expect<SqliteException>(() => Query(store.Path, "UPDATE operations SET receipt_json='{}' WHERE id='immutable'"));
    Expect<SqliteException>(() => Query(store.Path, "UPDATE execution_events SET level='V4'"));
    Expect<SqliteException>(() => Query(store.Path, "DELETE FROM execution_events"));
    Expect<SqliteException>(() => Query(store.Path, "UPDATE verification_results SET json='{}'"));
    Expect<SqliteException>(() => Query(store.Path, "DELETE FROM verification_results"));
}));
await Test("registry batch rollback preserves prior registrations", () => Sync(() =>
{
    var store = Store("registry");
    store.RecordRegistry(new JsonArray(new JsonObject { ["capability"] = "existing", ["status"] = "available_unverified" }));
    Expect<ArgumentException>(() => store.RecordRegistry(new JsonArray(new JsonObject { ["capability"] = "new" }, new JsonObject())));
    Check(Convert.ToInt64(Query(store.Path, "SELECT count(*) FROM registry")) == 1, "partial registry batch committed");
    store.RecordRegistry(new JsonArray(new JsonObject { ["capability"] = "existing", ["status"] = "unavailable" }));
    Check(Convert.ToInt64(Query(store.Path, "SELECT count(*) FROM registry")) == 1, "registry update duplicated entry");
}));
await Test("verified capability evidence is bound to environment", () => Sync(() =>
{
    var store = Store("last-verified");
    store.Reserve("success", "hash", Request(), run);
    store.Complete("success", Receipt("success"));
    store.Reserve("failure", "hash", Request(), run);
    store.Complete("failure", Receipt("failure", "FAILED"));
    Check(store.LastVerified("test.capability", "environment-one")?["operation_id"]?.GetValue<string>() == "success", "successful evidence not found");
    Check(store.LastVerified("test.capability", "environment-two") is null, "stale environment evidence trusted");
    Check(store.LastVerified("other.capability", "environment-one") is null, "other capability evidence trusted");
}));
await Test("failed or inconclusive re-verification invalidates current registry proof", () => Sync(() =>
{
    var store = Store("invalidate-proof");
    store.Reserve("success", "hash", Request(), run);
    store.Complete("success", Receipt("success"));
    Check(store.LastVerified("test.capability", "environment-one") is not null, "initial proof missing");
    store.SaveVerification("success", new JsonObject { ["outcome"] = "FAILED" }, "reverification");
    Check(store.LastVerified("test.capability", "environment-one") is null, "failed observation left stale proof");
    Check(store.GetReceipt("success")?["verification"]?["outcome"]?.GetValue<string>() == "VERIFIED", "historical receipt was rewritten");
    store.SaveVerification("success", new JsonObject { ["outcome"] = "VERIFIED" }, "reverification");
    Check(store.LastVerified("test.capability", "environment-one") is not null, "new passing observation not reflected");
    store.SaveVerification("success", new JsonObject { ["outcome"] = "INCONCLUSIVE" }, "reverification");
    Check(store.LastVerified("test.capability", "environment-one") is null, "uncertainty became current proof");
}));
await Test("recovery never steals operations from a living owner", () => Sync(() =>
{
    var store = Store("living-owner");
    store.Reserve("active", "hash", Request(), run);
    store.AppendEvent("active", "RUNNING", "V1", new JsonObject());
    Check(store.RecoverInterrupted().Count == 0, "living owner was recovered");
    Check(store.GetReceipt("active")?["state"]?.GetValue<string>() == "RUNNING", "active state changed");
}));
await Test("dead owner is recovered across a real process boundary", async () =>
{
    var store = Store("dead-owner");
    using var child = await Child("--abandon", store.Path);
    await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
    Check(child.ExitCode == 0, "test owner failed to reserve");
    var recovered = new EvidenceStore(store.Path).RecoverInterrupted();
    Check(recovered.Count == 1, "dead operation not recovered");
    Check(recovered[0]?["verification"]?["outcome"]?.GetValue<string>() == "INCONCLUSIVE", "interrupted operation claimed success");
    Check(store.GetReceipt("abandoned")?["events"]?.AsArray().Count == 3, "failure evidence missing");
    Check(store.RecoverInterrupted().Count == 0, "recovery duplicated receipt");
});
await Test("PID reuse does not impersonate the original owner", () => Sync(() =>
{
    var store = Store("pid-reuse");
    store.Reserve("reused", "hash", Request(), run);
    Query(store.Path, "UPDATE operations SET owner_started_at='2001-01-01T00:00:00.0000000Z' WHERE id='reused'");
    Check(store.RecoverInterrupted().Count == 1, "PID alone was trusted");
}));
await Test("terminal transition crash window remains inconclusive", () => Sync(() =>
{
    var store = Store("terminal-window");
    store.Reserve("window", "hash", Request(), run);
    store.AppendEvent("window", "VERIFIED", "V4", new JsonObject());
    Query(store.Path, "UPDATE operations SET owner_started_at='2001-01-01T00:00:00.0000000Z' WHERE id='window'");
    store.RecoverInterrupted();
    var receipt = store.GetReceipt("window")!;
    Check(receipt["verification"]!["outcome"]!.GetValue<string>() == "INCONCLUSIVE", "missing durable receipt became success");
    Check(receipt["verification"]!["level"]!.GetValue<string>() == "V3", "incomplete evidence promoted to V4");
}));
await Test("killed writer transaction rolls back without false V4", async () =>
{
    var store = Store("writer-crash");
    store.Reserve("crash-window", "hash", Request(), run);
    using var child = await Child("--hold-uncommitted", store.Path);
    try
    {
        var ready = await child.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(30));
        Check(ready == "UNCOMMITTED_READY", "writer did not reach uncommitted state");
    }
    finally
    {
        if (!child.HasExited) child.Kill(entireProcessTree: true);
        await child.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
    }
    var receipt = new EvidenceStore(store.Path).GetReceipt("crash-window")!;
    Check(receipt["state"]!.GetValue<string>() == "CREATED", "uncommitted state survived interruption");
    Check(receipt["verification_level"]!.GetValue<string>() == "V0", "uncommitted V4 survived interruption");
    Check(Convert.ToString(Query(store.Path, "PRAGMA integrity_check")) == "ok", "database damaged");
});
await Test("reproduction records preserve both original V4 receipts", () => Sync(() =>
{
    var store = Store("reproduction");
    foreach (var id in new[] { "first", "second" })
    {
        store.Reserve(id, "hash", Request(), run);
        store.AppendEvent(id, "VERIFIED", "V4", new JsonObject());
        store.Complete(id, Receipt(id));
    }
    store.RecordReproducibility("first", "second", new JsonObject { ["criteria_version"] = "frozen-test-version" });
    Check(store.GetReceipt("second")?["verification"]?["level"]?.GetValue<string>() == "V4", "original receipt promoted in place");
    Check(store.GetReceipt("second")?["events"]?.AsArray().Last()?["level"]?.GetValue<string>() == "V5", "reproduction event missing");
    Check(Convert.ToString(Query(store.Path, "SELECT level FROM operations WHERE id='second'")) == "V5", "current level not promoted");
    Check(Convert.ToInt64(Query(store.Path, "SELECT count(*) FROM reproducibility_runs")) == 1, "reproduction not persisted");
    Expect<ArgumentException>(() => store.RecordReproducibility("first", "first", new JsonObject()));
    Expect<SqliteException>(() => Query(store.Path, "DELETE FROM reproducibility_runs"));
}));
await Test("reproduction cannot promote unverified operations", () => Sync(() =>
{
    var store = Store("unverified-reproduction");
    foreach (var id in new[] { "first", "second" }) store.Reserve(id, "hash", Request(), run);
    store.Complete("first", Receipt("first"));
    store.Complete("second", Receipt("second", "FAILED"));
    Expect<InvalidOperationException>(() => store.RecordReproducibility("first", "second", new JsonObject()), "REPRODUCTION_REQUIRES_VERIFIED_OPERATIONS");
    Check(Convert.ToInt64(Query(store.Path, "SELECT count(*) FROM reproducibility_runs")) == 0, "invalid reproduction partially committed");
    Check(Convert.ToString(Query(store.Path, "SELECT level FROM operations WHERE id='second'")) == "V0", "failed operation promoted");
}));
await Test("paths and parameterized identifiers with spaces and apostrophes", () => Sync(() =>
{
    var store = new EvidenceStore(System.IO.Path.Combine(run, "space's directory", "evidence store.sqlite3"));
    var id = "operation'; DELETE FROM operations; --";
    store.Reserve(id, "hash'", Request(), run);
    store.Complete(id, Receipt(id));
    Check(store.GetReceipt(id) is not null && store.History().Count == 1, "identifier treated as SQL");
}));
await Test("unsupported database versions fail explicitly", () => Sync(() =>
{
    var store = Store("future-schema");
    Query(store.Path, "PRAGMA user_version=99");
    Expect<InvalidOperationException>(() => new EvidenceStore(store.Path), "UNSUPPORTED_EVIDENCE_SCHEMA");
}));

var failed = results.Count(x => x?["outcome"]?.GetValue<string>() == "failed");
var report = new JsonObject
{
    ["suite"] = "sqlite-evidence-store", ["test_kind"] = "isolated persistence and process-interruption tests",
    ["total"] = results.Count, ["passed"] = results.Count - failed, ["failed"] = failed,
    ["duration_seconds"] = total.Elapsed.TotalSeconds, ["artifact_directory"] = run, ["tests"] = results
};
var reportText = report.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(System.IO.Path.Combine(run, "results.json"), reportText);
Console.WriteLine(reportText);
return failed == 0 ? 0 : 1;
