"""Independent verifier unit tests; synthetic PDFs are not LibreOffice evidence."""
from __future__ import annotations

from copy import deepcopy
from contextlib import closing
from datetime import datetime, timezone
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import sqlite3
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch
import zipfile

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "verification"))
import fixture
import store_readback
import verify


class VerifierTests(unittest.TestCase):
    def setUp(self):
        scratch = ROOT / ".local" / "python-tests"
        scratch.mkdir(parents=True, exist_ok=True)
        self.temporary = tempfile.TemporaryDirectory(prefix="verifier path spaces ", dir=scratch)
        self.addCleanup(self.temporary.cleanup)
        self.directory = Path(self.temporary.name)
        self.source = self.directory / "source.docx"
        self.source.write_bytes(b"synthetic test input identity, not a real DOCX")
        self.output = self.directory / "result.pdf"
        self.spec = json.loads((ROOT / "verification" / "genesis2-criteria.json").read_text())
        self.criteria = self.directory / "criteria.json"
        self.criteria.write_text(json.dumps(self.spec), encoding="utf-8")
        fake_executable = self.directory / "soffice.com"
        fake_executable.write_text("Unit test only; never executed", encoding="utf-8")
        converted = self.directory / "converted"
        converted.mkdir()
        self.request = {"capability": "document.export_pdf", "source": str(self.source), "output": str(self.output),
                        "source_sha256": verify.digest(self.source.read_bytes()), "criteria_path": str(self.criteria),
                        "execution_path": {"executable": str(fake_executable), "arguments": [
                            "-env:UserInstallation=" + (self.directory / "profile").as_uri(), "--headless", "--convert-to",
                            "pdf:writer_pdf_Export", "--outdir", str(converted), str(self.source)],
                            "exit_code": 0, "timed_out": False, "cancelled": False, "application_version": "SYNTHETIC TEST",
                            "adapter_id": "test.libreoffice", "adapter_version": "test", "interface": "LibreOffice CLI headless",
                            "started_at": "2026-01-01T00:00:00Z", "finished_at": "2026-01-01T00:01:00Z"}}

    def make_pdf(self, *, title=None, paragraphs=None, matrix=None, pages=2, borders=True):
        from reportlab.pdfgen import canvas
        from reportlab.lib.pagesizes import A4
        from reportlab.platypus import Table, TableStyle, Paragraph
        from reportlab.lib.styles import ParagraphStyle
        from reportlab.lib import colors
        output = canvas.Canvas(str(self.output), pagesize=A4, invariant=1)
        output.setFont("Helvetica-Bold", 16)
        output.drawString(50, 790, self.spec["title"] if title is None else title)
        style = ParagraphStyle("test", fontName="Helvetica", fontSize=11, leading=13)
        y = 750
        for text in self.spec["paragraphs"]["1"] if paragraphs is None else paragraphs:
            paragraph = Paragraph(text, style)
            _, height = paragraph.wrap(470, 100)
            paragraph.drawOn(output, 50, y - height)
            y -= height + 12
        table = Table(self.spec["table_matrix"] if matrix is None else matrix, colWidths=[85, 240, 155], rowHeights=[25] * 4)
        commands = [("FONTNAME", (0, 0), (-1, -1), "Helvetica"), ("FONTSIZE", (0, 0), (-1, -1), 11)]
        if borders:
            commands.append(("GRID", (0, 0), (-1, -1), 0.5, colors.black))
        table.setStyle(TableStyle(commands))
        table.wrapOn(output, 480, 100)
        table.drawOn(output, 50, y - 110)
        if pages >= 2:
            output.showPage()
            output.setFont("Helvetica", 11)
            output.drawString(50, 790, self.spec["second_heading"])
            paragraph = Paragraph(self.spec["paragraphs"]["2"][0], style)
            _, height = paragraph.wrap(470, 100)
            paragraph.drawOn(output, 50, 750 - height)
            output.drawString(50, 700, self.spec["closing_line"])
        for _ in range(2, pages):
            output.showPage()
            output.drawString(50, 790, "Unexpected page")
        output.save()

    def assess(self):
        return verify.verify(self.request)

    def criterion(self, result, identifier):
        return next(item for item in result["criteria"] if item["id"] == identifier)

    def test_valid_synthetic_pdf_all_nine_pass(self):
        self.make_pdf()
        result = self.assess()
        self.assertEqual("VERIFIED", result["outcome"], result)
        self.assertEqual(9, len(result["criteria"]))
        self.assertEqual(2, result["page_count"])
        self.assertTrue(all(item["outcome"] == "PASS" for item in result["criteria"]))

    def test_missing_output_fails(self):
        result = self.assess()
        self.assertEqual("FAILED", result["outcome"])
        self.assertEqual("FAIL", self.criterion(result, "AC-01")["outcome"])

    def test_truncated_pdf_is_inconclusive(self):
        self.make_pdf()
        self.output.write_bytes(self.output.read_bytes()[:-100])
        result = self.assess()
        self.assertEqual("INCONCLUSIVE", result["outcome"])
        self.assertEqual("INCONCLUSIVE", self.criterion(result, "AC-02")["outcome"])

    def test_unreadable_structure_is_inconclusive(self):
        self.output.write_bytes(b"not a PDF")
        self.assertEqual("INCONCLUSIVE", self.assess()["outcome"])

    def test_wrong_page_count_fails(self):
        self.make_pdf(pages=3)
        self.assertEqual("FAIL", self.criterion(self.assess(), "AC-06")["outcome"])

    def test_changed_expected_page_count_is_enforced(self):
        self.make_pdf()
        self.spec["expected_page_count"] = 3
        self.criteria.write_text(json.dumps(self.spec), encoding="utf-8")
        self.assertEqual("FAIL", self.criterion(self.assess(), "AC-06")["outcome"])

    def test_missing_title_fails(self):
        self.make_pdf(title="wrong title")
        self.assertEqual("FAIL", self.criterion(self.assess(), "AC-03")["outcome"])

    def test_missing_paragraph_fails(self):
        self.make_pdf(paragraphs=[self.spec["paragraphs"]["1"][0]])
        self.assertEqual("FAIL", self.criterion(self.assess(), "AC-04")["outcome"])

    def test_reordered_paragraphs_fail(self):
        self.make_pdf(paragraphs=list(reversed(self.spec["paragraphs"]["1"])))
        self.assertEqual("FAIL", self.criterion(self.assess(), "AC-04")["outcome"])

    def test_altered_table_value_fails(self):
        matrix = deepcopy(self.spec["table_matrix"])
        matrix[1][2] = "99.99"
        self.make_pdf(matrix=matrix)
        self.assertEqual("FAIL", self.criterion(self.assess(), "AC-05")["outcome"])

    def test_swapped_table_rows_fail_even_all_words_present(self):
        matrix = deepcopy(self.spec["table_matrix"])
        matrix[1], matrix[2] = matrix[2], matrix[1]
        self.make_pdf(matrix=matrix)
        self.assertEqual("FAIL", self.criterion(self.assess(), "AC-05")["outcome"])

    def test_missing_cell_geometry_never_passes(self):
        self.make_pdf(borders=False)
        self.assertNotEqual("PASS", self.criterion(self.assess(), "AC-05")["outcome"])

    def test_source_change_fails(self):
        self.make_pdf()
        self.source.write_bytes(b"modified source")
        self.assertEqual("FAIL", self.criterion(self.assess(), "AC-07")["outcome"])

    def test_artifact_change_during_observation_fails(self):
        self.make_pdf()
        original = verify.parse_pdf
        def replace_after_parse(data):
            result = original(data)
            self.output.write_bytes(b"replaced")
            return result
        with patch.object(verify, "parse_pdf", side_effect=replace_after_parse):
            self.assertEqual("FAIL", self.criterion(self.assess(), "AC-07")["outcome"])

    def test_missing_parser_is_inconclusive(self):
        self.make_pdf()
        with patch.object(verify, "parse_pdf", side_effect=ModuleNotFoundError):
            result = self.assess()
        self.assertEqual("INCONCLUSIVE", result["outcome"])

    def test_resource_limit_is_inconclusive_not_semantic_failure(self):
        self.make_pdf()
        original = verify.bounded_read
        def bounded_failure(path):
            if path == self.output:
                raise ValueError("Synthetic size limit")
            return original(path)
        with patch.object(verify, "bounded_read", side_effect=bounded_failure):
            self.assertEqual("INCONCLUSIVE", self.assess()["outcome"])

    def test_stale_output_fails(self):
        self.make_pdf()
        os.utime(self.output, (1, 1))
        self.assertEqual("FAIL", self.criterion(self.assess(), "AC-01")["outcome"])

    def test_gui_interface_never_passes(self):
        self.make_pdf()
        self.request["execution_path"]["arguments"].remove("--headless")
        self.assertEqual("FAIL", self.criterion(self.assess(), "AC-08")["outcome"])

    def test_nonisolated_profile_fails(self):
        self.make_pdf()
        self.request["execution_path"]["arguments"][0] = "-env:UserInstallation=file:///C:/Users/Example/AppData/LibreOffice"
        self.assertEqual("FAIL", self.criterion(self.assess(), "AC-08")["outcome"])

    def test_nonzero_exit_does_not_override_independent_content(self):
        self.make_pdf()
        self.request["execution_path"]["exit_code"] = 1
        self.assertEqual("FAILED", self.assess()["outcome"])

    def test_timeout_never_verified(self):
        self.make_pdf()
        self.request["execution_path"]["timed_out"] = True
        self.assertEqual("FAILED", self.assess()["outcome"])

    def test_cancelled_never_verified(self):
        self.make_pdf()
        self.request["execution_path"]["cancelled"] = True
        self.assertEqual("FAILED", self.assess()["outcome"])

    def test_mismatched_criteria_rejected(self):
        self.spec["capability"] = "unknown"
        self.criteria.write_text(json.dumps(self.spec), encoding="utf-8")
        with self.assertRaises(ValueError):
            self.assess()

    def test_cli_outputs_only_json_and_zero_for_failed_assessment(self):
        request_file = self.directory / "request.json"
        request_file.write_text(json.dumps(self.request), encoding="utf-8")
        process = subprocess.run([sys.executable, str(ROOT / "verification" / "verify.py"), "--request", str(request_file)], capture_output=True, text=True, timeout=30)
        self.assertEqual(0, process.returncode)
        self.assertEqual("FAILED", json.loads(process.stdout)["outcome"])

    def test_text_transform_independent_unicode_and_newlines(self):
        self.source.write_bytes("Cafe\u0301\r\nsecond\rthird\n".encode("utf-8"))
        self.output.write_bytes("Café\nsecond\nthird\n".encode("utf-8"))
        self.request.update(capability="text.normalize", source_sha256=verify.digest(self.source.read_bytes()), criteria_path=str(ROOT / "verification" / "text-criteria.json"))
        result = self.assess()
        self.assertEqual("VERIFIED", result["outcome"])
        self.assertEqual(5, len(result["criteria"]))

    def test_text_wrong_output_fails(self):
        self.source.write_bytes(b"a\r\nb")
        self.output.write_bytes(b"wrong")
        self.request.update(capability="text.normalize", source_sha256=verify.digest(self.source.read_bytes()), criteria_path=str(ROOT / "verification" / "text-criteria.json"))
        self.assertEqual("FAILED", self.assess()["outcome"])

    def test_text_input_bom_is_explicitly_unsupported(self):
        self.source.write_bytes(b"\xef\xbb\xbfa\r\nb")
        self.output.write_bytes(b"a\nb")
        self.request.update(capability="text.normalize", source_sha256=verify.digest(self.source.read_bytes()), criteria_path=str(ROOT / "verification" / "text-criteria.json"))
        self.assertEqual("INCONCLUSIVE", self.assess()["outcome"])

    def test_text_output_bom_violates_encoding(self):
        self.source.write_bytes(b"a\r\nb")
        self.output.write_bytes(b"\xef\xbb\xbfa\nb")
        self.request.update(capability="text.normalize", source_sha256=verify.digest(self.source.read_bytes()), criteria_path=str(ROOT / "verification" / "text-criteria.json"))
        self.assertEqual("FAILED", self.assess()["outcome"])

    def test_fixture_bytes_deterministic_and_exact_layout(self):
        first = fixture.create_fixture(self.directory / "a" / "fixture.docx")
        second = fixture.create_fixture(self.directory / "b" / "fixture.docx")
        self.assertEqual(first["source_sha256"], second["source_sha256"])
        from docx import Document
        doc = Document(first["source"])
        self.assertEqual(self.spec["table_matrix"], [[cell.text for cell in row.cells] for row in doc.tables[0].rows])
        self.assertEqual("Arial", doc.styles["Normal"].font.name)
        self.assertEqual(11, doc.styles["Normal"].font.size.pt)
        with zipfile.ZipFile(first["source"]) as archive:
            xml = archive.read("word/document.xml").decode("utf-8")
            self.assertEqual(1, xml.count('w:type="page"'))
        with self.assertRaises(FileExistsError):
            fixture.create_fixture(Path(first["source"]))

    def test_missing_evidence_database_is_inconclusive(self):
        result = store_readback.readback(self.directory / "missing.sqlite", "op", "verification")
        self.assertEqual("INCONCLUSIVE", result["outcome"])

    def make_database(self):
        self.make_pdf()
        assessment = self.assess()
        request = {"protocol": "HMCP", "version": "0.1", "operation_id": "test-op", "capability": "document.export_pdf"}
        bundle = {"request": request, "discovery": {"capability": "document.export_pdf"}, "execution": self.request["execution_path"],
                  "verification": assessment, "artifacts": [{"path": str(self.output), "sha256": assessment["output_sha256"], "size": self.output.stat().st_size, "type": "pdf"}],
                  "source_sha256": self.request["source_sha256"], "criteria_sha256": assessment["criteria_sha256"], "criteria_path": str(self.criteria), "environment_key": "test"}
        database = self.directory / "evidence.sqlite"
        connection = sqlite3.connect(database)
        connection.executescript("CREATE TABLE operations(id TEXT PRIMARY KEY,request_json TEXT,request_hash TEXT,state TEXT,level TEXT,run_directory TEXT); CREATE TABLE verification_results(id TEXT PRIMARY KEY,operation_id TEXT,kind TEXT,json TEXT);")
        connection.execute("INSERT INTO operations VALUES(?,?,?,?,?,?)", ("test-op", json.dumps(request), "a" * 64, "VERIFYING", "V3", str(self.directory)))
        connection.execute("INSERT INTO verification_results VALUES(?,?,?,?)", ("test-verification", "test-op", "initial", json.dumps(bundle)))
        connection.commit()
        connection.close()
        return database

    def test_committed_storage_readback_passes_separate_connection(self):
        result = store_readback.readback(self.make_database(), "test-op", "test-verification")
        self.assertEqual("PASS", result["outcome"], result)

    def test_storage_does_not_trust_incomplete_criteria(self):
        database = self.make_database()
        with closing(sqlite3.connect(database)) as connection, connection:
            bundle = json.loads(connection.execute("SELECT json FROM verification_results").fetchone()[0])
            bundle["verification"]["criteria"].pop()
            connection.execute("UPDATE verification_results SET json=?", (json.dumps(bundle),))
        self.assertEqual("FAIL", store_readback.readback(database, "test-op", "test-verification")["outcome"])

    def test_interrupted_uncommitted_storage_is_not_success(self):
        database = self.make_database()
        with closing(sqlite3.connect(database)) as connection, connection:
            connection.execute("DELETE FROM verification_results")
        connection = sqlite3.connect(database)
        connection.execute("INSERT INTO verification_results VALUES(?,?,?,?)", ("uncommitted", "test-op", "initial", "{}"))
        self.assertEqual("FAIL", store_readback.readback(database, "test-op", "uncommitted")["outcome"])
        connection.close()


if __name__ == "__main__":
    unittest.main()
