"""Run verifier unit tests with counts from unittest, preserving machine-readable evidence."""
from __future__ import annotations

import argparse
from datetime import datetime, timezone
import json
from pathlib import Path
import sys
import time
import unittest


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--report", type=Path, required=True)
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[2]
    report = args.report.resolve()
    if not report.is_relative_to(root / ".local"):
        parser.error("machine-local reports must be beneath the repository .local directory")
    report.parent.mkdir(parents=True, exist_ok=True)
    started = datetime.now(timezone.utc)
    timer = time.perf_counter()
    suite = unittest.defaultTestLoader.discover(str(Path(__file__).parent), pattern="test_*.py")
    result = unittest.TextTestRunner(stream=sys.stderr, verbosity=2).run(suite)
    failed = len(result.failures) + len(result.errors) + len(result.unexpectedSuccesses)
    skipped = len(result.skipped)
    expected_failures = len(result.expectedFailures)
    payload = {
        "suite": "independent-verifier",
        "kind": "synthetic_pdf_and_readback_unit_tests",
        "started_at": started.isoformat(),
        "finished_at": datetime.now(timezone.utc).isoformat(),
        "interpreter": sys.executable,
        "python_version": sys.version,
        "total": result.testsRun,
        "passed": result.testsRun - failed - skipped - expected_failures,
        "failed": failed,
        "skipped": skipped,
        "expected_failures": expected_failures,
        "successful": result.wasSuccessful(),
        "duration_seconds": time.perf_counter() - timer,
        "failures": [{"test": test.id(), "traceback": detail} for test, detail in result.failures],
        "errors": [{"test": test.id(), "traceback": detail} for test, detail in result.errors],
        "unexpected_successes": [test.id() for test in result.unexpectedSuccesses],
        "real_libreoffice_conversion": False,
    }
    encoded = json.dumps(payload, indent=2) + "\n"
    report.write_text(encoded, encoding="utf-8")
    sys.stdout.write(encoded)
    return 0 if result.wasSuccessful() and result.testsRun else 1


if __name__ == "__main__":
    raise SystemExit(main())
