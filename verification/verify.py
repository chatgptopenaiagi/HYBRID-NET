"""Independent bounded artifact verifier. JSON stdout; no LibreOffice imports."""
from __future__ import annotations

import argparse
from datetime import datetime, timezone
import hashlib
import importlib.metadata
import io
import json
import logging
from pathlib import Path
import re
import sys
import time
import unicodedata
from urllib.parse import unquote, urlparse

VERIFIER_ID = "hybrid.independent-python"
VERSION = "0.1.0"
MAX_BYTES = 32 * 1024 * 1024


def digest(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def normalize(text: str) -> str:
    return " ".join(unicodedata.normalize("NFC", text).split())


def identity() -> dict:
    dependencies = {}
    for name in ("pypdf", "pdfplumber", "pdfminer.six"):
        try:
            dependencies[name] = importlib.metadata.version(name)
        except importlib.metadata.PackageNotFoundError:
            dependencies[name] = "UNAVAILABLE"
    return {"id": VERIFIER_ID, "version": VERSION, "script_sha256": digest(Path(__file__).read_bytes()),
            "python_version": sys.version.split()[0], "dependencies": dependencies}


def bounded_read(path: Path) -> bytes:
    with path.open("rb") as stream:
        data = stream.read(MAX_BYTES + 1)
    if len(data) > MAX_BYTES:
        raise ValueError("Artifact exceeds 32 MiB verifier limit")
    return data


def criterion(identifier: str, expected, observed, outcome: str, reference: str) -> dict:
    return {"id": identifier, "expected": expected, "observed": observed, "outcome": outcome,
            "evidence_reference": reference, "verifier_id": VERIFIER_ID}


def aggregate(criteria: list[dict]) -> str:
    if any(c["outcome"] == "FAIL" for c in criteria):
        return "FAILED"
    if any(c["outcome"] != "PASS" for c in criteria):
        return "INCONCLUSIVE"
    return "VERIFIED"


def parse_pdf(data: bytes) -> tuple[list[str], list]:
    from pypdf import PdfReader
    import pdfplumber
    diagnostics = io.StringIO()
    handler = logging.StreamHandler(diagnostics)
    logger = logging.getLogger("pypdf")
    logger.addHandler(handler)
    try:
        if not data.startswith(b"%PDF-") or not data.rstrip().endswith(b"%%EOF"):
            raise ValueError("Missing PDF header or terminal EOF marker")
        reader = PdfReader(io.BytesIO(data), strict=True)
        if reader.is_encrypted:
            raise ValueError("Encrypted PDF cannot be verified")
        if len(reader.pages) > 64:
            raise ValueError("Page count exceeds bounded verifier limit")
        pages = [normalize(page.extract_text()) for page in reader.pages]
        if diagnostics.getvalue().strip():
            raise ValueError("Unreconciled pypdf parsing diagnostic")
        with pdfplumber.open(io.BytesIO(data)) as document:
            if len(document.pages) != len(pages):
                raise ValueError("Independent parsers disagree on page count")
            matrices = document.pages[0].extract_tables(table_settings={
                "vertical_strategy": "lines", "horizontal_strategy": "lines",
                "snap_tolerance": 3, "join_tolerance": 3, "intersection_tolerance": 3,
            }) if document.pages else []
            matrices = [[[normalize(cell or "") for cell in row] for row in table] for table in matrices]
        return pages, matrices
    finally:
        logger.removeHandler(handler)


def execution_evidence(request: dict) -> dict:
    value = request["execution_path"]
    if isinstance(value, str):
        value = json.loads(bounded_read(Path(value)))
    if not isinstance(value, dict):
        raise ValueError("execution_path must be receipt object or JSON file path")
    return value


def fresh_file(output: Path, execution: dict) -> tuple[str, dict]:
    observation = {"path": str(output), "exists": output.is_file()}
    if not output.is_file():
        return "FAIL", observation
    try:
        started = datetime.fromisoformat(execution["started_at"].replace("Z", "+00:00"))
        if started.tzinfo is None:
            raise ValueError("Timestamp must identify timezone")
        modified = datetime.fromtimestamp(output.stat().st_mtime, timezone.utc)
        observation.update({"modified_at": modified.isoformat(), "execution_started_at": started.isoformat(),
                            "fresh": modified >= started})
        return ("PASS" if modified >= started else "FAIL"), observation
    except (KeyError, ValueError, TypeError, OSError) as exc:
        observation["diagnostic"] = type(exc).__name__
        return "INCONCLUSIVE", observation


def verify_interface(execution: dict) -> tuple[str, dict]:
    args = execution.get("arguments", [])
    executable = execution.get("executable", "")
    checks = {"executable": Path(executable).name.lower() in ("soffice.com", "soffice.exe") and Path(executable).is_file(),
              "headless": "--headless" in args, "zero_exit": execution.get("exit_code") == 0,
              "not_timed_out": execution.get("timed_out") is False, "not_cancelled": execution.get("cancelled") is False,
              "version_recorded": bool(execution.get("application_version")),
              "adapter_recorded": bool(execution.get("adapter_id")) and bool(execution.get("adapter_version")),
              "interface_recorded": bool(execution.get("interface"))}
    try:
        checks["pdf_filter"] = args[args.index("--convert-to") + 1] == "pdf:writer_pdf_Export"
        outdir = Path(args[args.index("--outdir") + 1]).resolve()
        checks["output_directory"] = outdir.is_dir()
    except (ValueError, IndexError, TypeError):
        outdir = None
        checks["pdf_filter"] = checks["output_directory"] = False
    profiles = [arg.split("=", 1)[1] for arg in args if isinstance(arg, str) and arg.startswith("-env:UserInstallation=")]
    checks["isolated_profile"] = False
    if len(profiles) == 1:
        parsed = urlparse(profiles[0])
        profile_text = unquote(parsed.path)
        if re.match(r"^/[A-Za-z]:", profile_text):
            profile_text = profile_text[1:]
        profile = Path(profile_text).resolve()
        # A dedicated profile adjacent to (or inside) the conversion directory;
        # never accept the user's normal LibreOffice profile as isolation evidence.
        if parsed.scheme == "file" and not parsed.netloc and outdir is not None:
            checks["isolated_profile"] = profile != outdir and profile.is_relative_to(outdir.parent) and "appdata" not in str(profile).lower()
    checks["docx_argument"] = any(isinstance(a, str) and a.lower().endswith(".docx") for a in args)
    return ("PASS" if all(checks.values()) else "FAIL"), {"checks": checks, "execution": execution}


def verify(request: dict) -> dict:
    started = time.perf_counter()
    required = {"capability", "source", "output", "source_sha256", "criteria_path", "execution_path"}
    if not isinstance(request, dict) or required - request.keys():
        raise ValueError("Missing required verifier request fields")
    if request["capability"] not in ("document.export_pdf", "text.normalize"):
        raise ValueError("Unsupported verifier capability")
    source, output = Path(request["source"]).resolve(), Path(request["output"]).resolve()
    criteria_path = Path(request["criteria_path"]).resolve()
    criteria_bytes = bounded_read(criteria_path)
    spec = json.loads(criteria_bytes)
    if spec.get("schema_version") != "1.0" or spec.get("capability") != request["capability"]:
        raise ValueError("Unsupported or mismatched criteria contract")
    execution = execution_evidence(request)
    result = {"schema_version": "1.0", "verifier": identity(), "criteria": [], "observed": False,
              "source_sha256_after": None, "output_sha256": None, "output_size": None, "page_count": None,
              "criteria_sha256": digest(criteria_bytes), "criteria_id": spec["criteria_id"], "criteria_version": spec["criteria_version"]}
    checks = result["criteria"]
    source_bytes = output_bytes = None
    source_error = output_error = None
    try:
        source_bytes = bounded_read(source)
        result["source_sha256_after"] = digest(source_bytes)
    except (OSError, ValueError) as exc:
        source_error = type(exc).__name__
    try:
        output_bytes = bounded_read(output)
        result["output_sha256"], result["output_size"] = digest(output_bytes), len(output_bytes)
        result["observed"] = True
    except (OSError, ValueError) as exc:
        output_error = type(exc).__name__
    existence, observation = fresh_file(output, execution)
    reference = str(output)
    if request["capability"] == "text.normalize":
        checks.append(criterion("TC-01", "Fresh output file exists", observation, existence, reference))
        text = actual = None
        decoding_error = output_error or source_error
        try:
            if source_bytes is not None and output_bytes is not None:
                text, actual = source_bytes.decode("utf-8"), output_bytes.decode("utf-8")
                if source_bytes.startswith(b"\xef\xbb\xbf"):
                    text = actual = None
                    decoding_error = "UNSUPPORTED_UTF8_BOM"
                elif output_bytes.startswith(b"\xef\xbb\xbf"):
                    actual = None
                    decoding_error = "OUTPUT_UTF8_BOM"
        except UnicodeDecodeError:
            decoding_error = "UnicodeDecodeError"
        checks.append(criterion("TC-02", "Source and output decode as UTF-8", {"diagnostic": decoding_error},
                                "FAIL" if decoding_error == "OUTPUT_UTF8_BOM" else ("PASS" if text is not None else "INCONCLUSIVE"), reference))
        expected = unicodedata.normalize("NFC", text).replace("\r\n", "\n").replace("\r", "\n") if text is not None else None
        exact = expected is not None and actual is not None and output_bytes == expected.encode("utf-8")
        checks.append(criterion("TC-03", "Exact NFC + CRLF/CR-to-LF UTF-8 bytes", {"exact_match": exact},
                                "PASS" if exact else ("FAIL" if expected is not None else "INCONCLUSIVE"), reference))
        preserved = result["source_sha256_after"] == request["source_sha256"]
        checks.append(criterion("TC-04", "Unchanged source and stable output fingerprints", {
            "source_before": request["source_sha256"], "source_after": result["source_sha256_after"],
            "output_sha256": result["output_sha256"]}, "PASS" if preserved and output_bytes is not None else "FAIL", str(source)))
        execution_ok = execution.get("exit_code") == 0 and execution.get("timed_out") is False and execution.get("cancelled") is False
        checks.append(criterion("TC-05", "Independent Python verification and recorded successful execution", {
            "verifier": result["verifier"], "execution": execution, "criteria_sha256": result["criteria_sha256"]},
            "PASS" if execution_ok and execution.get("adapter_id") != VERIFIER_ID else "FAIL", str(Path(__file__).resolve())))
    else:
        checks.append(criterion("AC-01", "New output PDF exists", observation, existence, reference))
        pages = tables = None
        parse_error = output_error
        try:
            if output_bytes is not None:
                pages, tables = parse_pdf(output_bytes)
                result["page_count"] = len(pages)
        except Exception as exc:
            parse_error = type(exc).__name__
        checks.append(criterion("AC-02", "Strict independent PDF parsing without unreconciled diagnostics", {
            "parser": "pypdf strict=True + pdfplumber", "diagnostic": parse_error, "page_count": result["page_count"]},
            "PASS" if pages is not None else "INCONCLUSIVE", reference))
        title_ok = pages is not None and bool(pages) and normalize(spec["title"]) in pages[0]
        checks.append(criterion("AC-03", {"page": 1, "title": spec["title"]}, {"present": title_ok},
                                "PASS" if title_ok else ("FAIL" if pages is not None else "INCONCLUSIVE"), reference))
        paragraph_observations = []
        for page, paragraphs in spec["paragraphs"].items():
            text = pages[int(page) - 1] if pages is not None and len(pages) >= int(page) else None
            positions = [text.find(normalize(value)) for value in paragraphs] if text is not None else []
            passed = len(positions) == len(paragraphs) and all(p >= 0 for p in positions) and positions == sorted(set(positions))
            paragraph_observations.append({"page": int(page), "positions": positions, "passed": passed})
        paragraph_ok = all(item["passed"] for item in paragraph_observations)
        checks.append(criterion("AC-04", spec["paragraphs"], paragraph_observations,
                                "PASS" if paragraph_ok else ("FAIL" if pages is not None else "INCONCLUSIVE"), reference))
        table_ok = tables is not None and spec["table_matrix"] in tables
        checks.append(criterion("AC-05", {"page": 1, "matrix": spec["table_matrix"], "method": spec["table_method"]},
                                {"matrices": tables}, "PASS" if table_ok else ("FAIL" if tables is not None else "INCONCLUSIVE"), reference))
        page_ok = pages is not None and len(pages) == spec["expected_page_count"] and len(pages) >= 2
        heading_at = pages[1].find(normalize(spec["second_heading"])) if pages is not None and len(pages) >= 2 else -1
        closing_at = pages[1].find(normalize(spec["closing_line"])) if pages is not None and len(pages) >= 2 else -1
        page_ok = page_ok and heading_at >= 0 and closing_at > heading_at
        checks.append(criterion("AC-06", {"page_count": spec["expected_page_count"], "page2_heading": spec["second_heading"], "page2_closing": spec["closing_line"]},
                                {"page_count": result["page_count"], "heading_position": heading_at, "closing_position": closing_at},
                                "PASS" if page_ok else ("FAIL" if pages is not None else "INCONCLUSIVE"), reference))
        preserved = result["source_sha256_after"] == request["source_sha256"]
        integrity = preserved and bool(output_bytes)
        integrity_outcome = "PASS" if integrity else "FAIL"
        if (source_error and source_error != "FileNotFoundError") or (output_error and output_error != "FileNotFoundError"):
            integrity_outcome = "INCONCLUSIVE"
        checks.append(criterion("AC-07", "Nonempty SHA-256 identified output; unchanged source; stable bytes on readback", {
            "source_sha256_before": request["source_sha256"], "source_sha256_after": result["source_sha256_after"],
            "output_sha256": result["output_sha256"], "output_size": result["output_size"], "source_diagnostic": source_error,
            "output_diagnostic": output_error}, integrity_outcome, reference))
        interface_outcome, interface_facts = verify_interface(execution)
        checks.append(criterion("AC-08", "Supported headless LibreOffice PDF export with isolated profile", interface_facts, interface_outcome, "execution_path"))
        independent = execution.get("adapter_id") != VERIFIER_ID and pages is not None
        checks.append(criterion("AC-09", "Separate identified verifier, frozen criteria hash and independent PDF observations", {
            "verifier": result["verifier"], "criteria_sha256": result["criteria_sha256"], "independent_observation": pages is not None},
            "PASS" if independent else "INCONCLUSIVE", str(Path(__file__).resolve())))
    # Re-read identities after all parsing. A concurrent replacement invalidates
    # the conclusion even if the previously observed bytes were correct.
    integrity_id = "AC-07" if request["capability"] == "document.export_pdf" else "TC-04"
    integrity_check = next(item for item in checks if item["id"] == integrity_id)
    try:
        stable = digest(bounded_read(output)) == result["output_sha256"] and digest(bounded_read(source)) == result["source_sha256_after"]
        integrity_check["observed"]["identity_stable_on_readback"] = stable
        if not stable:
            integrity_check["outcome"] = "FAIL"
    except (OSError, ValueError):
        integrity_check["outcome"] = "INCONCLUSIVE" if integrity_check["outcome"] == "PASS" else integrity_check["outcome"]
    result["outcome"] = aggregate(checks)
    result["duration_ms"] = round((time.perf_counter() - started) * 1000, 3)
    return result


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--request", required=True, type=Path)
    args = parser.parse_args()
    try:
        result = verify(json.loads(bounded_read(args.request)))
    except Exception as exc:
        print(json.dumps({"schema_version": "1.0", "outcome": "INCONCLUSIVE", "observed": False,
                          "error": {"code": "VERIFIER_REQUEST_OR_INFRASTRUCTURE", "type": type(exc).__name__},
                          "verifier": identity(), "criteria": []}))
        return 2
    print(json.dumps(result, ensure_ascii=True))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
