"""Real packaged-CLI acceptance and equivalent direct LibreOffice comparison."""
from __future__ import annotations

import argparse
from contextlib import closing
import ctypes
from ctypes import wintypes
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import shutil
import sqlite3
import statistics
import subprocess
import sys
import threading
import time
import uuid
from urllib.parse import unquote

REPO = Path(__file__).resolve().parents[2]


def now():
    return datetime.now(timezone.utc).isoformat()


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def write(path, value):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2, ensure_ascii=True), encoding="utf-8")


def original_receipt_fields(receipt):
    """Store queries append history; those additions are not receipt mutation."""
    return {key: value for key, value in receipt.items() if key not in ("events", "verification_history", "duplicate_request", "replayed")}


def sampled_memory(process, stop, samples):
    if os.name != "nt":
        return
    class Counters(ctypes.Structure):
        _fields_ = [("cb", wintypes.DWORD), ("PageFaultCount", wintypes.DWORD)] + [(key, ctypes.c_size_t) for key in
                   ("PeakWorkingSetSize", "WorkingSetSize", "QuotaPeakPagedPoolUsage", "QuotaPagedPoolUsage", "QuotaPeakNonPagedPoolUsage", "QuotaNonPagedPoolUsage", "PagefileUsage", "PeakPagefileUsage")]
    while not stop.wait(0.025):
        values = Counters()
        values.cb = ctypes.sizeof(values)
        try:
            if ctypes.windll.psapi.GetProcessMemoryInfo(wintypes.HANDLE(process._handle), ctypes.byref(values), values.cb):
                samples.append(values.WorkingSetSize)
        except (AttributeError, OSError):
            return


def run_process(command, cwd, timeout=180):
    start = time.perf_counter()
    started_at = now()
    process = subprocess.Popen([str(a) for a in command], cwd=cwd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    samples, stop = [], threading.Event()
    monitor = threading.Thread(target=sampled_memory, args=(process, stop, samples), daemon=True)
    monitor.start()
    timed_out = False
    try:
        stdout, stderr = process.communicate(timeout=timeout)
    except subprocess.TimeoutExpired:
        timed_out = True
        if os.name == "nt":
            subprocess.run([str(Path(os.environ["SystemRoot"]) / "System32" / "taskkill.exe"), "/PID", str(process.pid), "/T", "/F"], capture_output=True, timeout=15)
        else:
            process.kill()
        stdout, stderr = process.communicate(timeout=15)
    finally:
        stop.set()
        monitor.join(timeout=1)
    return {"executable": str(command[0]), "arguments": [str(a) for a in command[1:]], "working_directory": str(cwd),
            "pid": process.pid, "started_at": started_at, "finished_at": now(), "exit_code": process.returncode,
            "stdout": stdout.decode("utf-8", errors="replace"), "stderr": stderr.decode("utf-8", errors="replace"),
            "timed_out": timed_out, "cancelled": False, "duration_ms": (time.perf_counter() - start) * 1000,
            "peak_working_set_bytes": max(samples) if samples else None,
            "memory_measurement": "sampled_root_process_only" if samples else "NOT_MEASURED"}


class Experiments:
    def __init__(self, args):
        self.root = Path(args.root).resolve()
        if not self.root.is_relative_to(REPO / ".local"):
            raise ValueError("Experiment root must be inside repository .local")
        if self.root.exists():
            raise FileExistsError("Use a fresh experiment root; existing evidence is preserved")
        self.root.mkdir(parents=True)
        self.python = Path(args.python).resolve()
        self.libreoffice = Path(args.libreoffice).resolve()
        original_cli = Path(args.cli).resolve()
        self.delivery = self.root / "separate delivery bundle"
        shutil.copytree(original_cli.parent, self.delivery)
        self.cli = self.delivery / original_cli.name
        self.verification = self.delivery / "verification"
        self.cwd = self.root / "outside source working directory"
        self.cwd.mkdir()
        self.framework_workspace = self.root / "framework state"
        self.framework_workspace.mkdir()
        (self.framework_workspace / "outputs").mkdir()
        self.artifact_metadata = {"executable": str(self.cli), "sha256": sha(self.cli), "size": self.cli.stat().st_size,
                                  "copied_from": str(original_cli), "source_sha256": sha(original_cli),
                                  "bundle_files": [{"path": str(path.relative_to(self.delivery)), "size": path.stat().st_size, "sha256": sha(path)}
                                                   for path in sorted(self.delivery.rglob("*")) if path.is_file()]}
        write(self.root / "delivery-manifest.json", self.artifact_metadata)
        self.summary = {"schema_version": "1.0", "started_at": now(), "root": str(self.root), "artifact": self.artifact_metadata,
                        "benchmark": [], "checks": [], "failures": [], "human_intervention_count": "NOT_ESTABLISHED",
                        "human_intervention_time": "NOT_ESTABLISHED", "human_time_recovered": "NOT_ESTABLISHED",
                        "retry_count": 0, "adapter_development_effort": "NOT_ESTABLISHED",
                        "network_condition": "No remote framework/server/API credentials; network physically disabled NOT_TESTED",
                        "memory_limitations": "Root launcher samples exclude LibreOffice child-process working sets"}
        fixture_path = self.framework_workspace / "fixture" / "hnet-g2-orchid-7319.docx"
        generated = run_process([self.python, self.verification / "fixture.py", "--output", fixture_path], self.cwd)
        write(self.root / "fixture-process.json", generated)
        if generated["exit_code"]:
            raise RuntimeError("Fixture generation failed")
        self.fixture = json.loads(generated["stdout"])
        self.source, self.criteria = Path(self.fixture["source"]), Path(self.fixture["criteria_path"])
        self.text_criteria = self.framework_workspace / "text-criteria.json"
        shutil.copy2(self.verification / "text-criteria.json", self.text_criteria)
        self.summary["fixture"] = self.fixture
        self.save()

    def save(self):
        write(self.root / "summary.json", self.summary)

    def progress(self, text):
        print(text, flush=True)

    def check(self, name, passed, evidence, details=None):
        self.summary["checks"].append({"name": name, "passed": bool(passed), "evidence": str(evidence), "details": details})
        self.save()

    def cli_call(self, label, arguments, python=None, timeout=180):
        command = [self.cli, *arguments, "--workspace", self.framework_workspace, "--python", python or self.python, "--libreoffice", self.libreoffice]
        process = run_process(command, self.cwd, timeout)
        try:
            result = json.loads(process["stdout"])
        except ValueError:
            result = {"unparseable_response": True}
        process_file = self.root / "commands" / (label + "-process.json")
        result_file = self.root / "commands" / (label + "-result.json")
        write(process_file, process)
        write(result_file, result)
        return result, process, result_file

    def request(self, identifier, source=None, destination=None, criteria=None, timeout=60, capability="document.export_pdf"):
        return {"protocol": "HMCP", "version": "0.1", "operation_id": identifier, "capability": capability,
                "application": "LibreOffice" if capability == "document.export_pdf" else "Hybrid.Text",
                "parameters": {"source": str(source or self.source), "destination": str(destination or (self.framework_workspace / "outputs" / (identifier + ".pdf")))},
                "options": {"timeout_seconds": timeout, "criteria_path": str(criteria or self.criteria)}}

    def execute(self, identifier, request=None, python=None, extra=None):
        request = request or self.request(identifier)
        request_file = self.framework_workspace / "requests" / (identifier + ".json")
        write(request_file, request)
        return self.cli_call(identifier, ["execute", "--request", request_file, *(extra or [])], python=python)

    @staticmethod
    def verified(result, count=10):
        assessment = result.get("verification", {})
        return assessment.get("outcome") == "VERIFIED" and assessment.get("level") in ("V4", "V5") and len(assessment.get("criteria", [])) == count and all(item.get("outcome") == "PASS" for item in assessment.get("criteria", []))

    def baseline(self, identifier):
        self.progress("Direct LibreOffice baseline " + identifier)
        started = time.perf_counter()
        directory = self.root / "direct baseline" / identifier
        converted = directory / "converted"
        converted.mkdir(parents=True)
        source = directory / self.source.name
        shutil.copy2(self.source, source)
        criteria = directory / "criteria.json"
        shutil.copy2(self.criteria, criteria)
        probe = run_process([self.libreoffice, "-env:UserInstallation=" + (directory / "probe profile").as_uri(), "--headless", "--version"], directory, 15)
        version = probe["stdout"].strip()
        command = [self.libreoffice, "-env:UserInstallation=" + (directory / "profile").as_uri(), "--headless", "--nologo", "--nodefault", "--norestore", "--convert-to", "pdf:writer_pdf_Export", "--outdir", converted, source]
        execution = run_process(command, directory, 60)
        execution.update({"application_version": version, "adapter_id": "baseline.direct-libreoffice-command", "adapter_version": "1.0", "interface": "LibreOffice CLI headless", "executable_sha256": sha(self.libreoffice)})
        output = converted / (source.stem + ".pdf")
        write(directory / "execution.json", execution)
        request = self.request(identifier, source, output, criteria)
        verifier_request = {"capability": "document.export_pdf", "source": str(source), "output": str(output), "source_sha256": self.fixture["source_sha256"], "criteria_path": str(criteria), "execution_path": execution}
        write(directory / "verifier-request.json", verifier_request)
        verification_process = run_process([self.python, self.verification / "verify.py", "--request", directory / "verifier-request.json"], directory, 30)
        write(directory / "verification-process.json", verification_process)
        assessment = json.loads(verification_process["stdout"])
        artifacts = [{"path": str(output), "sha256": sha(output), "size": output.stat().st_size, "type": "pdf"}] if output.is_file() else []
        bundle = {"request": request, "discovery": {"application": "LibreOffice", "executable": str(self.libreoffice), "version_probe": probe, "classification": "DIRECT_BASELINE_PROBE_NOT_HACE"},
                  "execution": execution, "verification": assessment, "artifacts": artifacts, "source_sha256": self.fixture["source_sha256"],
                  "criteria_sha256": sha(criteria), "criteria_path": str(criteria), "environment_key": hashlib.sha256((version + sha(self.libreoffice)).encode()).hexdigest()}
        database = directory / "baseline-evidence.sqlite"
        with closing(sqlite3.connect(database)) as connection, connection:
            connection.executescript("PRAGMA journal_mode=WAL; PRAGMA synchronous=FULL; CREATE TABLE operations(id TEXT PRIMARY KEY,request_json TEXT,request_hash TEXT,state TEXT,level TEXT,run_directory TEXT,receipt_json TEXT); CREATE TABLE verification_results(id TEXT PRIMARY KEY,operation_id TEXT,kind TEXT,json TEXT);")
            request_json = json.dumps(request)
            connection.execute("INSERT INTO operations VALUES(?,?,?,?,?,?,NULL)", (identifier, request_json, hashlib.sha256(request_json.encode()).hexdigest(), "VERIFYING", "V3" if assessment.get("observed") else "V2", str(directory)))
            connection.execute("INSERT INTO verification_results VALUES(?,?,?,?)", (identifier + "-provisional", identifier, "provisional", json.dumps(bundle)))
        readback_process = run_process([self.python, self.verification / "store_readback.py", "--database", database, "--operation-id", identifier, "--verification-id", identifier + "-provisional"], directory, 15)
        write(directory / "readback-process.json", readback_process)
        storage = json.loads(readback_process["stdout"])
        assessment["criteria"].append(storage)
        assessment["outcome"] = "VERIFIED" if all(c["outcome"] == "PASS" for c in assessment["criteria"]) else ("FAILED" if any(c["outcome"] == "FAIL" for c in assessment["criteria"]) else "INCONCLUSIVE")
        assessment["level"] = "V4" if assessment["outcome"] == "VERIFIED" else "V3"
        bundle["verification"] = assessment
        with closing(sqlite3.connect(database)) as connection, connection:
            connection.execute("INSERT INTO verification_results VALUES(?,?,?,?)", (identifier + "-final", identifier, "initial", json.dumps(assessment)))
            connection.execute("UPDATE operations SET state=?,level=?,receipt_json=? WHERE id=?", (assessment["outcome"], assessment["level"], json.dumps(bundle), identifier))
        # Close/reopen and inspect final receipt, rather than trusting commit alone.
        with closing(sqlite3.connect(database.as_uri() + "?mode=ro", uri=True)) as connection:
            persisted = json.loads(connection.execute("SELECT receipt_json FROM operations WHERE id=?", (identifier,)).fetchone()[0])
        final_equal = persisted == bundle
        metrics = {"mode": "A_DIRECT", "operation_id": identifier, "execution_ms": execution["duration_ms"], "verification_ms": assessment["duration_ms"],
                   "total_wall_ms": (time.perf_counter() - started) * 1000, "peak_working_set_bytes": execution["peak_working_set_bytes"],
                   "memory_measurement": execution["memory_measurement"], "exit_code": execution["exit_code"], "success": self.verified(bundle) and final_equal,
                   "output_sha256": assessment.get("output_sha256"), "criteria_sha256": sha(criteria), "source_sha256": sha(source),
                   "evidence_database_size": database.stat().st_size, "evidence": str(directory / "receipt.json"), "human_interventions": "NOT_ESTABLISHED", "retry_count": 0}
        bundle["metrics"] = metrics
        write(directory / "receipt.json", bundle)
        self.summary["benchmark"].append(metrics)
        self.check(identifier, metrics["success"], directory / "receipt.json")

    def framework(self, identifier):
        self.progress("Packaged HYBRID.NET trial " + identifier)
        result, process, evidence = self.execute(identifier)
        metrics = {"mode": "B_FRAMEWORK", "operation_id": identifier, "execution_ms": result.get("execution", {}).get("duration_ms"),
                   "verification_ms": result.get("metrics", {}).get("verification_ms"), "total_wall_ms": process["duration_ms"],
                   "peak_working_set_bytes": result.get("execution", {}).get("peak_working_set_bytes"), "memory_measurement": "sampled_root_process_only",
                   "exit_code": result.get("execution", {}).get("exit_code"), "success": self.verified(result),
                   "output_sha256": result.get("verification", {}).get("output_sha256"), "criteria_sha256": result.get("criteria_sha256"),
                   "source_sha256": result.get("source_sha256"), "evidence": str(evidence), "human_interventions": "NOT_ESTABLISHED", "retry_count": 0}
        self.summary["benchmark"].append(metrics)
        self.check(identifier, metrics["success"], evidence)
        return result

    def benchmark(self):
        queries = [("version", ["version"]), ("doctor", ["doctor"]), ("discovery", ["discover"]), ("capability", ["capability", "document.export_pdf"])]
        for label, command in queries:
            result, process, evidence = self.cli_call(label, command)
            self.check(label, process["exit_code"] == 0 and not (isinstance(result, dict) and "error" in result), evidence)
        order = ["A", "B", "B", "A", "A", "B"]
        self.summary["run_order"] = order
        b_ids = []
        for index, mode in enumerate(order, 1):
            identifier = f"trial-{index}-{mode.lower()}"
            if mode == "A":
                self.baseline(identifier)
            else:
                self.framework(identifier)
                b_ids.append(identifier)
        for identifier in b_ids:
            result, process, evidence = self.cli_call("restart-receipt-" + identifier, ["receipt", identifier])
            self.check("receipt-retrieved-after-new-process-" + identifier, self.verified(result), evidence)
        result, process, evidence = self.cli_call("history", ["history"])
        self.check("history-after-restart", process["exit_code"] == 0 and len(result) >= len(b_ids), evidence)
        result, process, evidence = self.cli_call("reproduction", ["reproduce", b_ids[0], b_ids[1]])
        self.check("semantic-reproduction-V5", result.get("level") == "V5", evidence, result)
        self.summary["reproducibility"] = result
        result, process, evidence = self.cli_call("reverify-original", ["verify", b_ids[0]])
        self.check("full-reverification-original", self.verified(result), evidence)
        text_source = self.framework_workspace / "text input.txt"
        text_source.write_bytes("Cafe\u0301\r\nsecond\rthird\n".encode("utf-8"))
        text_request = self.request("reuse-text", source=text_source, destination=self.framework_workspace / "text normalized.txt", criteria=self.text_criteria, capability="text.normalize")
        result, process, evidence = self.execute("reuse-text", text_request)
        self.check("second-capability-reuse", self.verified(result, count=6), evidence)
        self.summary["second_capability"] = {"capability": "text.normalize", "verified": self.verified(result, count=6), "evidence": str(evidence)}
        self.summary["comparison"] = {mode: {"trials": len([m for m in self.summary["benchmark"] if m["mode"] == mode]),
            "mean_total_wall_ms": statistics.mean(m["total_wall_ms"] for m in self.summary["benchmark"] if m["mode"] == mode),
            "success_rate": statistics.mean(int(m["success"]) for m in self.summary["benchmark"] if m["mode"] == mode)} for mode in ("A_DIRECT", "B_FRAMEWORK")}
        self.save()

    def negative(self, name, request=None, python=None, extra=None, expected=None):
        self.progress("Controlled failure " + name)
        result, process, evidence = self.execute(name, request=request, python=python, extra=extra)
        no_false_v4 = result.get("verification", {}).get("level") not in ("V4", "V5") and result.get("verification", {}).get("outcome") != "VERIFIED"
        expected_response = expected(result, process) if expected else process["exit_code"] != 0
        self.summary["failures"].append({"fault": name, "false_success": not no_false_v4, "detected": no_false_v4 and expected_response,
                                         "state": result.get("state"), "outcome": result.get("verification", {}).get("outcome"), "evidence": str(evidence)})
        self.check(name, no_false_v4 and expected_response, evidence)
        return result

    def failures(self):
        self.negative("missing-source", self.request("missing-source", source=self.framework_workspace / "missing.docx"))
        self.negative("invalid-destination", self.request("invalid-destination", destination=self.framework_workspace / "CON.pdf"))
        corrupted_source = self.framework_workspace / "corrupted.docx"
        corrupted_source.write_bytes(b"Deliberately not an OOXML ZIP")
        self.negative("corrupted-docx", self.request("corrupted-docx", source=corrupted_source))
        wrong_criteria = self.framework_workspace / "negative-three-pages.json"
        spec = json.loads(self.criteria.read_text())
        spec["expected_page_count"] = 3
        write(wrong_criteria, spec)
        self.negative("incorrect-page-count", self.request("incorrect-page-count", criteria=wrong_criteria), expected=lambda r, p: any(c.get("id") == "AC-06" and c.get("outcome") == "FAIL" for c in r.get("verification", {}).get("criteria", [])))
        self.negative("verifier-unavailable", python=self.framework_workspace / "missing-python.exe",
                      expected=lambda r, p: r.get("verification", {}).get("outcome") == "INCONCLUSIVE")
        self.negative("execution-timeout", self.request("execution-timeout", timeout=1), expected=lambda r, p: r.get("execution", {}).get("timed_out") is True and r.get("verification", {}).get("outcome") == "INCONCLUSIVE")
        self.negative("cancelled-execution", extra=["--cancel-after-ms", "6000"], expected=lambda r, p: r.get("state") == "CANCELLED" and r.get("verification", {}).get("outcome") == "INCONCLUSIVE")
        # Seed disposable verified operations specifically for destructive fault cases.
        for label, mode in [("corrupted-generated-pdf", "corrupt"), ("missing-output-artifact", "missing")]:
            self.progress("Prepare isolated mutation fixture " + label)
            result, process, evidence = self.execute(label)
            self.check("seed-" + label, self.verified(result), evidence)
            output = Path(result.get("request", {}).get("parameters", {}).get("destination", ""))
            if not self.verified(result):
                continue
            if not output.resolve().is_relative_to(self.framework_workspace) or output.resolve() == self.source.resolve():
                raise ValueError("Fault injection target escaped its disposable workspace")
            original_receipt_hash = sha(evidence)
            original_pdf = output.with_suffix(".preserved.pdf")
            shutil.copy2(output, original_pdf)
            if mode == "corrupt":
                output.write_bytes(b"%PDF-1.7\ntruncated fault fixture")
            else:
                output.rename(output.with_suffix(".removed.pdf"))
            reverified, _, revidence = self.cli_call("reverify-" + label, ["verify", label])
            no_false = reverified.get("verification", {}).get("level") not in ("V4", "V5") and reverified.get("verification", {}).get("outcome") != "VERIFIED"
            self.summary["failures"].append({"fault": label, "false_success": not no_false, "detected": no_false, "evidence": str(revidence)})
            self.check(label, no_false, revidence)
            recovered, _, recovered_evidence = self.cli_call("historical-" + label, ["receipt", label])
            with closing(sqlite3.connect((self.framework_workspace / "engine" / "evidence.sqlite").as_uri() + "?mode=ro", uri=True)) as connection:
                stored_original = json.loads(connection.execute("SELECT receipt_json FROM operations WHERE id=?", (label,)).fetchone()[0])
            self.check("historical-receipt-preserved-" + label, original_receipt_fields(recovered) == result and stored_original == result and sha(evidence) == original_receipt_hash, recovered_evidence)
        # Identical retries return immutable evidence and perform no second execution.
        request = self.request("duplicate-original", source=self.framework_workspace / "duplicate-input.txt", destination=self.framework_workspace / "duplicate-output.txt", criteria=self.text_criteria, capability="text.normalize")
        Path(request["parameters"]["source"]).write_bytes(b"one\r\ntwo")
        first, _, first_evidence = self.execute("duplicate-original", request)
        duplicate_file = self.framework_workspace / "requests" / "duplicate-original.json"
        repeated, _, repeated_evidence = self.cli_call("duplicate-identical", ["execute", "--request", duplicate_file])
        repeated_original = original_receipt_fields(repeated)
        self.check("duplicate-identical-no-reexecution", self.verified(first, count=6) and repeated_original == first and repeated.get("duplicate_request") is True and repeated.get("replayed") is False, repeated_evidence)
        request["parameters"]["destination"] = str(self.framework_workspace / "different-output.txt")
        self.negative("duplicate-conflict", request, expected=lambda r, p: r.get("error", {}).get("code") == "OPERATION_ID_CONFLICT")
        self.interrupt_owned_conversion()
        self.summary["failure_tests_elsewhere"] = {"interrupted_evidence_persistence": "Automated SQLite store tests and Python uncommitted-readback test; see scripts/test.ps1 results", "verifier_defective_pdf_tests": "Python verifier tests cover truncation, unreadable structure, wrong content/table/page and missing file"}
        self.summary["false_success_count"] = sum(int(item["false_success"]) for item in self.summary["failures"])
        self.summary["failure_detection_rate"] = sum(int(item["detected"]) for item in self.summary["failures"]) / len(self.summary["failures"])
        self.save()

    def interrupt_owned_conversion(self):
        self.progress("Controlled interruption of dedicated LibreOffice child process")
        identifier = "interrupted-conversion-" + uuid.uuid4().hex[:8]
        request_file = self.framework_workspace / "requests" / (identifier + ".json")
        write(request_file, self.request(identifier))
        command = [self.cli, "execute", "--request", request_file, "--workspace", self.framework_workspace, "--python", self.python, "--libreoffice", self.libreoffice]
        started = time.perf_counter()
        process = subprocess.Popen([str(a) for a in command], cwd=self.cwd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        target = None
        # Query only soffice.bin; kill only command lines containing this new
        # operation's isolated profile. Unrelated LibreOffice processes are untouched.
        powershell = Path(os.environ["ProgramFiles"]) / "PowerShell" / "7" / "pwsh.exe"
        for _ in range(30):
            if process.poll() is not None:
                break
            query = f"Get-CimInstance Win32_Process -Filter \"Name='soffice.bin'\" | Where-Object {{ $_.CommandLine -like '*{identifier}*' }} | Select-Object ProcessId,CommandLine | ConvertTo-Json -Compress"
            probe = subprocess.run([str(powershell), "-NoProfile", "-Command", query], capture_output=True, text=True, timeout=10)
            if probe.stdout.strip():
                candidates = json.loads(probe.stdout)
                candidates = candidates if isinstance(candidates, list) else [candidates]
                for candidate in candidates:
                    owned_scope = str(self.framework_workspace).replace("\\", "/").lower()
                    observed_command = unquote(candidate.get("CommandLine", "")).replace("\\", "/").lower()
                    if identifier in candidate.get("CommandLine", "") and "UserInstallation" in candidate["CommandLine"] and owned_scope in observed_command:
                        target = candidate
                        subprocess.run([str(Path(os.environ["SystemRoot"]) / "System32" / "taskkill.exe"), "/PID", str(candidate["ProcessId"]), "/T", "/F"], capture_output=True, timeout=15)
                        break
            if target:
                break
            time.sleep(0.2)
        try:
            stdout, stderr = process.communicate(timeout=90)
        except subprocess.TimeoutExpired:
            subprocess.run([str(Path(os.environ["SystemRoot"]) / "System32" / "taskkill.exe"), "/PID", str(process.pid), "/T", "/F"], capture_output=True, timeout=15)
            stdout, stderr = process.communicate(timeout=15)
        result = json.loads(stdout.decode("utf-8"))
        evidence = self.root / "commands" / "interrupted-conversion-result.json"
        write(evidence, result)
        write(self.root / "commands" / "interrupted-conversion-process.json", {"command": [str(a) for a in command], "exit_code": process.returncode, "stderr": stderr.decode("utf-8", errors="replace"), "owned_target": target, "duration_ms": (time.perf_counter() - started) * 1000})
        no_false = result.get("verification", {}).get("outcome") != "VERIFIED" and result.get("verification", {}).get("level") not in ("V4", "V5")
        self.summary["failures"].append({"fault": "interrupted-libreoffice-conversion", "injection_applied": target is not None, "false_success": not no_false, "detected": target is not None and no_false, "evidence": str(evidence)})
        self.check("interrupted-libreoffice-conversion", target is not None and no_false, evidence)

    def finish(self):
        self.summary["finished_at"] = now()
        self.summary["passed"] = all(check["passed"] for check in self.summary["checks"])
        self.summary["checks_count"] = len(self.summary["checks"])
        self.summary["failed_checks"] = [check["name"] for check in self.summary["checks"] if not check["passed"]]
        self.summary["evidence_storage_bytes"] = sum(path.stat().st_size for path in self.root.rglob("*.sqlite"))
        self.summary["fixture_preserved"] = sha(self.source) == self.fixture["source_sha256"]
        self.summary["gates"] = {"feasibility": "PASSED" if any(m["mode"] == "B_FRAMEWORK" and m["success"] for m in self.summary["benchmark"]) else "INCONCLUSIVE",
                                 "reusability": "PASSED" if self.summary.get("second_capability", {}).get("verified") else "INCONCLUSIVE",
                                 "engineering_advantage": "INCONCLUSIVE"}
        self.save()
        self.progress(json.dumps({"summary": str(self.root / "summary.json"), "passed": self.summary["passed"], "checks": self.summary["checks_count"], "failed_checks": self.summary["failed_checks"]}))
        return 0 if self.summary["passed"] and self.summary["fixture_preserved"] else 1


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--cli", required=True)
    parser.add_argument("--python", required=True)
    parser.add_argument("--root", required=True)
    parser.add_argument("--libreoffice", default=r"C:\Program Files\LibreOffice\program\soffice.com")
    parser.add_argument("--phase", choices=["all", "benchmark", "failures"], default="all")
    args = parser.parse_args()
    experiments = Experiments(args)
    try:
        if args.phase in ("all", "benchmark"):
            experiments.benchmark()
        if args.phase in ("all", "failures"):
            experiments.failures()
    except Exception as exc:
        experiments.summary["harness_error"] = {"type": type(exc).__name__, "detail": str(exc)}
        experiments.check("harness-completion", False, experiments.root / "summary.json", type(exc).__name__)
        raise
    finally:
        experiments.finish()
    return 0 if experiments.summary["passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
