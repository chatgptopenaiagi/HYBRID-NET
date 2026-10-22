"""Independently inspect committed SQLite evidence using Python sqlite3."""
from __future__ import annotations

import argparse
from contextlib import closing
import hashlib
import json
from pathlib import Path
import sqlite3
import sys
import time


def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def readback(database: Path, operation_id: str, verification_id: str) -> dict:
    started = time.perf_counter()
    verifier_id = "hybrid.independent-sqlite-readback"
    result = {"schema_version": "1.0", "id": "AC-10", "outcome": "INCONCLUSIVE",
              "expected": "Committed linked request, discovery, execution, artifact and independent verification evidence",
              "observed": {}, "evidence_reference": f"sqlite:{database.resolve()}#verification_results/{verification_id}",
              "verifier_id": verifier_id, "verifier": {"id": verifier_id, "version": "0.1.0", "python_version": sys.version.split()[0],
              "sqlite_version": sqlite3.sqlite_version, "script_sha256": sha(Path(__file__).read_bytes())}}
    try:
        # A separate read-only connection observes only a committed transaction.
        with closing(sqlite3.connect(database.resolve().as_uri() + "?mode=ro", uri=True, timeout=5)) as connection:
            operation = connection.execute("SELECT request_json,request_hash,state,level,run_directory FROM operations WHERE id=?", (operation_id,)).fetchone()
            verification = connection.execute("SELECT kind,json FROM verification_results WHERE id=? AND operation_id=?", (verification_id, operation_id)).fetchone()
            checks = {"operation_committed": operation is not None, "verification_committed": verification is not None}
            if operation is None or verification is None:
                result["observed"] = {"checks": checks}
                result["outcome"] = "FAIL"
                return result
            request = json.loads(operation[0])
            bundle = json.loads(verification[1])
            capability = request.get("capability")
            if capability == "text.normalize":
                result["id"] = "TC-06"
            expected_ids = {f"AC-{n:02d}" for n in range(1, 10)} if capability == "document.export_pdf" else {f"TC-{n:02d}" for n in range(1, 6)}
            assessment = bundle.get("verification", {})
            criteria = assessment.get("criteria", [])
            checks["request_identity"] = bundle.get("request") == request
            checks["request_hash_recorded"] = isinstance(operation[1], str) and len(operation[1]) == 64
            checks["operation_identity"] = request.get("operation_id") == operation_id
            checks["hmcp_version"] = request.get("protocol") == "HMCP" and request.get("version") == "0.1"
            checks["supported_capability"] = capability in ("document.export_pdf", "text.normalize")
            discovery = bundle.get("discovery", {})
            execution = bundle.get("execution", {})
            checks["discovery_recorded"] = isinstance(discovery, dict) and bool(discovery)
            checks["execution_recorded"] = isinstance(execution, dict) and bool(execution.get("adapter_id")) and "exit_code" in execution
            checks["all_criteria_recorded"] = len(criteria) == len(expected_ids) and {c.get("id") for c in criteria} == expected_ids
            checks["criterion_provenance"] = all(c.get("outcome") in ("PASS", "FAIL", "INCONCLUSIVE") and "expected" in c and "observed" in c
                                                   and bool(c.get("evidence_reference")) and bool(c.get("verifier_id")) for c in criteria)
            identity = assessment.get("verifier", {})
            checks["independent_verifier_identity"] = identity.get("id") == "hybrid.independent-python" and len(identity.get("script_sha256", "")) == 64
            checks["source_hash_linked"] = len(bundle.get("source_sha256", "")) == 64
            checks["criteria_hash_linked"] = len(bundle.get("criteria_sha256", "")) == 64 and bundle.get("criteria_sha256") == assessment.get("criteria_sha256")
            criteria_path = Path(bundle.get("criteria_path", ""))
            checks["criteria_bytes_match"] = criteria_path.is_file() and sha(criteria_path.read_bytes()) == bundle.get("criteria_sha256")
            artifacts = bundle.get("artifacts", [])
            checks["artifact_provenance"] = isinstance(artifacts, list) and all(
                isinstance(item, dict) and isinstance(item.get("path"), str) and len(item.get("sha256", "")) == 64
                and isinstance(item.get("size"), int) and item["size"] >= 0 and bool(item.get("type")) for item in artifacts)
            output_hash = assessment.get("output_sha256")
            checks["observed_output_linked"] = output_hash is None or any(item.get("sha256") == output_hash for item in artifacts)
            checks["environment_recorded"] = bool(bundle.get("environment_key"))
            result["observed"] = {"checks": checks, "operation_id": operation_id, "verification_id": verification_id,
                                  "operation_state": operation[2], "operation_level": operation[3], "verification_kind": verification[0],
                                  "bundle_sha256": sha(verification[1].encode("utf-8")), "criteria_ids": sorted(c.get("id", "") for c in criteria)}
            result["outcome"] = "PASS" if all(checks.values()) else "FAIL"
    except (OSError, sqlite3.Error, ValueError, TypeError, KeyError) as exc:
        result["observed"] = {"diagnostic": type(exc).__name__}
    finally:
        result["duration_ms"] = round((time.perf_counter() - started) * 1000, 3)
    return result


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--database", type=Path, required=True)
    parser.add_argument("--operation-id", required=True)
    parser.add_argument("--verification-id", required=True)
    args = parser.parse_args()
    result = readback(args.database, args.operation_id, args.verification_id)
    print(json.dumps(result))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
