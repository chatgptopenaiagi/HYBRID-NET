using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Data.Sqlite;

namespace Hybrid.Core;

/// <summary>
/// Local transactional evidence. Original receipts, verification records and events are
/// immutable; later observations are additional records, not replacement conclusions.
/// This is crash recovery and audit history, not a security boundary against database owners.
/// </summary>
public sealed class EvidenceStore
{
    public string Path { get; }
    private static string Now() => DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);
    private static string Json(JsonObject value) => value.ToJsonString();

    public EvidenceStore(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        Path = System.IO.Path.GetFullPath(databasePath);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
        using var connection = Open();
        using var version = connection.CreateCommand();
        version.CommandText = "PRAGMA user_version";
        var schemaVersion = Convert.ToInt32(version.ExecuteScalar(), CultureInfo.InvariantCulture);
        if (schemaVersion is not (0 or 1)) throw new InvalidOperationException("UNSUPPORTED_EVIDENCE_SCHEMA");
        using var transaction = connection.BeginTransaction();
        Execute(connection, transaction, """
            CREATE TABLE IF NOT EXISTS operations (
                id TEXT PRIMARY KEY,
                request_hash TEXT NOT NULL,
                request_json TEXT NOT NULL,
                run_directory TEXT NOT NULL,
                state TEXT NOT NULL,
                level TEXT NOT NULL,
                receipt_json TEXT,
                owner_pid INTEGER NOT NULL,
                owner_started_at TEXT NOT NULL,
                created_at TEXT NOT NULL,
                updated_at TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS execution_events (
                sequence INTEGER PRIMARY KEY AUTOINCREMENT,
                event_id TEXT NOT NULL UNIQUE,
                operation_id TEXT NOT NULL REFERENCES operations(id),
                timestamp TEXT NOT NULL,
                state TEXT NOT NULL,
                level TEXT NOT NULL,
                data_json TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS events_operation ON execution_events(operation_id, sequence);
            CREATE TABLE IF NOT EXISTS verification_results (
                id TEXT PRIMARY KEY,
                operation_id TEXT NOT NULL REFERENCES operations(id),
                kind TEXT NOT NULL,
                json TEXT NOT NULL,
                created_at TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS verifications_operation ON verification_results(operation_id, created_at);
            CREATE TABLE IF NOT EXISTS registry (
                capability_id TEXT PRIMARY KEY,
                json TEXT NOT NULL,
                observed_at TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS reproducibility_runs (
                id TEXT PRIMARY KEY,
                first_operation_id TEXT NOT NULL REFERENCES operations(id),
                second_operation_id TEXT NOT NULL REFERENCES operations(id),
                json TEXT NOT NULL,
                created_at TEXT NOT NULL,
                CHECK(first_operation_id <> second_operation_id)
            );
            CREATE TRIGGER IF NOT EXISTS preserve_receipt BEFORE UPDATE OF receipt_json ON operations
              WHEN OLD.receipt_json IS NOT NULL AND NEW.receipt_json IS NOT OLD.receipt_json
              BEGIN SELECT RAISE(ABORT, 'ORIGINAL_RECEIPT_IMMUTABLE'); END;
            CREATE TRIGGER IF NOT EXISTS preserve_event_update BEFORE UPDATE ON execution_events
              BEGIN SELECT RAISE(ABORT, 'EVENT_IMMUTABLE'); END;
            CREATE TRIGGER IF NOT EXISTS preserve_event_delete BEFORE DELETE ON execution_events
              BEGIN SELECT RAISE(ABORT, 'EVENT_IMMUTABLE'); END;
            CREATE TRIGGER IF NOT EXISTS preserve_verification_update BEFORE UPDATE ON verification_results
              BEGIN SELECT RAISE(ABORT, 'VERIFICATION_IMMUTABLE'); END;
            CREATE TRIGGER IF NOT EXISTS preserve_verification_delete BEFORE DELETE ON verification_results
              BEGIN SELECT RAISE(ABORT, 'VERIFICATION_IMMUTABLE'); END;
            CREATE TRIGGER IF NOT EXISTS preserve_reproduction_update BEFORE UPDATE ON reproducibility_runs
              BEGIN SELECT RAISE(ABORT, 'REPRODUCTION_IMMUTABLE'); END;
            CREATE TRIGGER IF NOT EXISTS preserve_reproduction_delete BEFORE DELETE ON reproducibility_runs
              BEGIN SELECT RAISE(ABORT, 'REPRODUCTION_IMMUTABLE'); END;
            PRAGMA user_version=1;
            """);
        transaction.Commit();
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = Path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
            DefaultTimeout = 5
        }.ToString());
        try
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA busy_timeout=5000; PRAGMA foreign_keys=ON; PRAGMA journal_mode=WAL; PRAGMA synchronous=FULL;";
            command.ExecuteNonQuery();
            return connection;
        }
        catch { connection.Dispose(); throw; }
    }

    private static SqliteCommand Command(SqliteConnection connection, SqliteTransaction? transaction,
        string sql, params (string, object?)[] parameters)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach (var (name, value) in parameters) command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return command;
    }

    private static int Execute(SqliteConnection connection, SqliteTransaction? transaction,
        string sql, params (string, object?)[] parameters)
    {
        using var command = Command(connection, transaction, sql, parameters);
        return command.ExecuteNonQuery();
    }

    private static void RequireOperation(SqliteConnection connection, SqliteTransaction transaction, string id)
    {
        using var command = Command(connection, transaction, "SELECT 1 FROM operations WHERE id=$id", ("$id", id));
        if (command.ExecuteScalar() is null) throw new KeyNotFoundException("OPERATION_NOT_FOUND");
    }

    public bool Reserve(string operationId, string requestHash, JsonObject request, string runDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestHash);
        ArgumentNullException.ThrowIfNull(request);
        using var process = Process.GetCurrentProcess();
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        var now = Now();
        var inserted = Execute(connection, transaction, """
            INSERT OR IGNORE INTO operations
              (id, request_hash, request_json, run_directory, state, level, owner_pid, owner_started_at, created_at, updated_at)
            VALUES ($id,$hash,$request,$directory,'CREATED','V0',$pid,$started,$now,$now)
            """, ("$id", operationId), ("$hash", requestHash), ("$request", Json(request)),
            ("$directory", System.IO.Path.GetFullPath(runDirectory)), ("$pid", Environment.ProcessId),
            ("$started", process.StartTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)), ("$now", now));
        if (inserted == 0)
        {
            using var check = Command(connection, transaction, "SELECT request_hash FROM operations WHERE id=$id", ("$id", operationId));
            if (!string.Equals((string?)check.ExecuteScalar(), requestHash, StringComparison.Ordinal))
                throw new InvalidOperationException("OPERATION_ID_CONFLICT");
            transaction.Commit();
            return false;
        }
        InsertEvent(connection, transaction, operationId, "CREATED", "V0", new JsonObject { ["event"] = "OPERATION_RESERVED" }, now);
        transaction.Commit();
        return true;
    }

    private static void InsertEvent(SqliteConnection connection, SqliteTransaction transaction,
        string id, string state, string level, JsonObject data, string timestamp)
    {
        if (level is not ("V0" or "V1" or "V2" or "V3" or "V4" or "V5"))
            throw new ArgumentException("INVALID_VERIFICATION_LEVEL", nameof(level));
        Execute(connection, transaction, """
            INSERT INTO execution_events(event_id,operation_id,timestamp,state,level,data_json)
            VALUES($event,$id,$now,$state,$level,$data)
            """, ("$event", Guid.NewGuid().ToString("N")), ("$id", id), ("$now", timestamp),
            ("$state", state), ("$level", level), ("$data", Json(data)));
    }

    public void AppendEvent(string id, string state, string level, JsonObject data)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        RequireOperation(connection, transaction, id);
        var now = Now();
        InsertEvent(connection, transaction, id, state, level, data, now);
        Execute(connection, transaction, "UPDATE operations SET state=$state,level=$level,updated_at=$now WHERE id=$id",
            ("$state", state), ("$level", level), ("$now", now), ("$id", id));
        transaction.Commit();
    }

    public string SaveVerification(string id, JsonObject verification, string kind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        var verificationId = Guid.NewGuid().ToString("N");
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        RequireOperation(connection, transaction, id);
        Execute(connection, transaction, """
            INSERT INTO verification_results(id,operation_id,kind,json,created_at)
            VALUES($verification,$id,$kind,$json,$now)
            """, ("$verification", verificationId), ("$id", id), ("$kind", kind),
            ("$json", Json(verification)), ("$now", Now()));
        transaction.Commit();
        return verificationId;
    }

    public void Complete(string id, JsonObject receipt)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        RequireOperation(connection, transaction, id);
        var changed = Execute(connection, transaction, """
            UPDATE operations SET receipt_json=$receipt,updated_at=$now WHERE id=$id AND receipt_json IS NULL
            """, ("$receipt", Json(receipt)), ("$now", Now()), ("$id", id));
        if (changed != 1) throw new InvalidOperationException("ORIGINAL_RECEIPT_IMMUTABLE");
        if(receipt["state"] is JsonValue terminalState && receipt["verification"]?["level"] is JsonValue evidenceLevel)
        {
            var state=terminalState.GetValue<string>(); var level=evidenceLevel.GetValue<string>(); var now=Now();
            InsertEvent(connection,transaction,id,state,level,new JsonObject { ["event"]="TERMINAL_RECEIPT_COMMITTED" },now);
            Execute(connection,transaction,"UPDATE operations SET state=$state,level=$level,updated_at=$now WHERE id=$id",("$state",state),("$level",level),("$now",now),("$id",id));
        }
        transaction.Commit();
    }

    public JsonObject? GetReceipt(string id)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction(deferred: true);
        JsonObject result;
        using (var command = Command(connection, transaction,
            "SELECT receipt_json,request_json,state,level,run_directory,created_at FROM operations WHERE id=$id", ("$id", id)))
        using (var reader = command.ExecuteReader())
        {
            if (!reader.Read()) return null;
            result = reader.IsDBNull(0)
                ? new JsonObject
                {
                    ["operation_id"] = id, ["original_receipt_pending"] = true,
                    ["request"] = JsonNode.Parse(reader.GetString(1)), ["state"] = reader.GetString(2),
                    ["verification_level"] = reader.GetString(3), ["run_directory"] = reader.GetString(4),
                    ["created_at"] = reader.GetString(5)
                }
                : JsonNode.Parse(reader.GetString(0))!.AsObject();
        }
        var verifications = new JsonArray();
        using (var command = Command(connection, transaction,
            "SELECT id,kind,json,created_at FROM verification_results WHERE operation_id=$id ORDER BY rowid", ("$id", id)))
        using (var reader = command.ExecuteReader())
            while (reader.Read()) verifications.Add(new JsonObject
            {
                ["verification_id"] = reader.GetString(0), ["kind"] = reader.GetString(1),
                ["verification"] = JsonNode.Parse(reader.GetString(2)), ["created_at"] = reader.GetString(3)
            });
        var events = new JsonArray();
        using (var command = Command(connection, transaction,
            "SELECT sequence,event_id,timestamp,state,level,data_json FROM execution_events WHERE operation_id=$id ORDER BY sequence", ("$id", id)))
        using (var reader = command.ExecuteReader())
            while (reader.Read()) events.Add(new JsonObject
            {
                ["sequence"] = reader.GetInt64(0), ["event_id"] = reader.GetString(1),
                ["timestamp"] = reader.GetString(2), ["state"] = reader.GetString(3),
                ["level"] = reader.GetString(4), ["data"] = JsonNode.Parse(reader.GetString(5))
            });
        result["verification_history"] = verifications;
        result["events"] = events;
        transaction.Commit();
        return result;
    }

    public JsonArray History()
    {
        using var connection = Open();
        using var command = Command(connection, null, """
            SELECT id,state,level,created_at,updated_at,receipt_json,request_json FROM operations ORDER BY created_at DESC,id
            """);
        using var reader = command.ExecuteReader();
        var history = new JsonArray();
        while (reader.Read()) history.Add(new JsonObject
        {
            ["operation_id"] = reader.GetString(0), ["state"] = reader.GetString(1),
            ["verification_level"] = reader.GetString(2), ["created_at"] = reader.GetString(3),
            ["updated_at"] = reader.GetString(4), ["original_receipt_pending"] = reader.IsDBNull(5),
            ["capability"] = JsonNode.Parse(reader.GetString(6))?["capability"]?.DeepClone()
        });
        return history;
    }

    public void RecordRegistry(JsonArray capabilities)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        foreach (var capability in capabilities)
        {
            if (capability is not JsonObject obj) throw new ArgumentException("INVALID_CAPABILITY_REGISTRATION");
            var id = obj["capability"]?.GetValue<string>() ?? obj["capability_id"]?.GetValue<string>()
                ?? throw new ArgumentException("CAPABILITY_ID_REQUIRED");
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            Execute(connection, transaction, """
                INSERT INTO registry(capability_id,json,observed_at) VALUES($id,$json,$now)
                ON CONFLICT(capability_id) DO UPDATE SET json=excluded.json,observed_at=excluded.observed_at
                """, ("$id", id), ("$json", Json(obj)), ("$now", Now()));
        }
        transaction.Commit();
    }

    public JsonObject? LastVerified(string capability, string environmentKey)
    {
        using var connection = Open();
        using var command = Command(connection, null, """
            SELECT o.receipt_json,
              (SELECT v.json FROM verification_results v WHERE v.operation_id=o.id AND v.kind='reverification'
               ORDER BY v.rowid DESC LIMIT 1)
            FROM operations o WHERE o.receipt_json IS NOT NULL ORDER BY o.updated_at DESC,o.id
            """);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var receipt = JsonNode.Parse(reader.GetString(0))!.AsObject();
            // A later failed/inconclusive observation does not rewrite the historical
            // V4 receipt, but it does invalidate that receipt as current availability evidence.
            if (!reader.IsDBNull(1))
            {
                var latest = JsonNode.Parse(reader.GetString(1));
                if (latest?["outcome"]?.GetValue<string>() != "VERIFIED") continue;
            }
            var receivedCapability = receipt["capability"]?.GetValue<string>() ?? receipt["request"]?["capability"]?.GetValue<string>();
            var outcome = receipt["verification"]?["outcome"]?.GetValue<string>();
            if (receivedCapability == capability && receipt["environment_key"]?.GetValue<string>() == environmentKey && outcome == "VERIFIED")
                return receipt;
        }
        return null;
    }

    private static bool OwnerAlive(int pid, string startedAt)
    {
        try
        {
            using var owner = Process.GetProcessById(pid);
            var actual = owner.StartTime.ToUniversalTime();
            var recorded = DateTime.Parse(startedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
            return !owner.HasExited && actual == recorded;
        }
        catch (ArgumentException) { return false; }
        catch (InvalidOperationException) { return false; }
        // If another owner's identity cannot be inspected, do not steal its operation.
        catch (System.ComponentModel.Win32Exception) { return true; }
    }

    public JsonArray RecoverInterrupted()
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        var unfinished = new List<(string Id, string Level, int Pid, string Started, string Request, string Directory)>();
        using (var command = Command(connection, transaction,
            "SELECT id,level,owner_pid,owner_started_at,request_json,run_directory FROM operations WHERE receipt_json IS NULL"))
        using (var reader = command.ExecuteReader())
            while (reader.Read()) unfinished.Add((reader.GetString(0), reader.GetString(1), reader.GetInt32(2),
                reader.GetString(3), reader.GetString(4), reader.GetString(5)));
        var recovered = new JsonArray();
        foreach (var item in unfinished)
        {
            if (OwnerAlive(item.Pid, item.Started)) continue;
            var now = Now();
            var level = item.Level is "V4" or "V5" ? "V3" : item.Level;
            var receipt = new JsonObject
            {
                ["operation_id"] = item.Id, ["request"] = JsonNode.Parse(item.Request),
                ["state"] = "INCONCLUSIVE", ["run_directory"] = item.Directory,
                ["verification"] = new JsonObject { ["outcome"] = "INCONCLUSIVE", ["level"] = level },
                ["error"] = new JsonObject { ["code"] = "INTERRUPTED_OWNER", ["stage"] = "recovery", ["effect_status"] = "unknown" },
                ["recovered_at"] = now,
                ["recovery"] = new JsonObject { ["owner_pid"] = item.Pid, ["owner_started_at"] = item.Started, ["automatic_replay"] = false }
            };
            InsertEvent(connection, transaction, item.Id, "INCONCLUSIVE", level,
                new JsonObject { ["event"] = "INTERRUPTED_OPERATION_RECOVERED", ["effect_status"] = "unknown" }, now);
            Execute(connection, transaction, """
                UPDATE operations SET state='INCONCLUSIVE',level=$level,receipt_json=$receipt,updated_at=$now
                WHERE id=$id AND receipt_json IS NULL
                """, ("$level", level), ("$receipt", Json(receipt)), ("$now", now), ("$id", item.Id));
            recovered.Add(receipt);
        }
        transaction.Commit();
        return recovered;
    }

    public string RecordReproducibility(string firstId, string secondId, JsonObject evidence)
    {
        if (firstId == secondId) throw new ArgumentException("DISTINCT_REPRODUCTION_OPERATIONS_REQUIRED");
        var id = Guid.NewGuid().ToString("N");
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        RequireOperation(connection, transaction, firstId);
        RequireOperation(connection, transaction, secondId);
        foreach (var operationId in new[] { firstId, secondId })
        {
            using var check = Command(connection, transaction, "SELECT receipt_json FROM operations WHERE id=$id", ("$id", operationId));
            var raw = check.ExecuteScalar();
            var original = raw is string json ? JsonNode.Parse(json) : null;
            if (original?["verification"]?["outcome"]?.GetValue<string>() != "VERIFIED" ||
                original?["verification"]?["level"]?.GetValue<string>() is not ("V4" or "V5"))
                throw new InvalidOperationException("REPRODUCTION_REQUIRES_VERIFIED_OPERATIONS");
        }
        var now = Now();
        Execute(connection, transaction, """
            INSERT INTO reproducibility_runs(id,first_operation_id,second_operation_id,json,created_at)
            VALUES($id,$first,$second,$json,$now)
            """, ("$id", id), ("$first", firstId), ("$second", secondId), ("$json", Json(evidence)), ("$now", now));
        InsertEvent(connection, transaction, secondId, "VERIFIED", "V5", new JsonObject
        {
            ["event"] = "REPRODUCIBILITY_VERIFIED", ["reproducibility_id"] = id,
            ["first_operation_id"] = firstId, ["second_operation_id"] = secondId
        }, now);
        Execute(connection, transaction, "UPDATE operations SET state='VERIFIED',level='V5',updated_at=$now WHERE id=$id",
            ("$id", secondId), ("$now", now));
        transaction.Commit();
        return id;
    }
}
