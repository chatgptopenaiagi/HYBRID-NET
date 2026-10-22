# Independent verification implementation

This Python component reads actual artifact bytes independently of the C# execution
adapter. It does not invoke LibreOffice. `verify.py --request <json-file>` emits one
JSON assessment with all AC-01 through AC-09 (PDF) or TC-01 through TC-05 (text).
The runtime must add independently read-back SQLite evidence before assigning V4.
An assessment may be FAILED or INCONCLUSIVE with process exit zero; callers must
inspect `outcome`. Exit two means the request or verification infrastructure could
not supply a normal assessment. Missing files are failures, while parser inability
is inconclusive. A demonstrable criterion failure takes precedence over uncertainty.

The PDF parser uses [pypdf strict mode](https://pypdf.readthedocs.io/en/stable/user/robustness.html),
rejects unreconciled parser diagnostics and independently observes ruled table cells
with [pdfplumber](https://github.com/jsvine/pdfplumber#extracting-tables). Table
values must match the exact four-by-three matrix, including row/column associations.
Text comparisons use NFC and collapsed whitespace, preserving case and punctuation.
The two-page requirement, content and layout originate in the frozen
[Genesis 2 experiment](../docs/genesis/GENESIS-2-EXPERIMENT.md).
Neither text extraction nor table geometry establishes pixel-perfect fidelity.

`fixture.py --output <new-directory>/hnet-g2-orchid-7319.docx` creates the exact
controlled fixture and sibling `criteria.json`. Its ZIP entry metadata is stable;
generation refuses overwriting either file. Font availability is a separate machine
preflight. `genesis2-criteria.json` is frozen before real trials. Deliberately changed
criteria in negative tests never replace that original contract.

`store_readback.py --database <path> --operation-id <id> --verification-id <id>`
opens SQLite in a separate read-only connection, validates committed linkage and
returns AC-10 or TC-06. It does not mutate conclusions. A subsequent runtime append
records that observation and the final gate. Historical verification rows remain
distinct from later re-verification.

Artifact reads are bounded to 32 MiB and 64 PDF pages; this narrow prototype reports
unsupported larger evidence as inconclusive. The runtime must additionally bound
the verifier process time. These boundaries are not an operating-system sandbox.
Application execution metadata is recorder evidence, not independent OS attestation.
Python's Unicode NFC implementation independently checks the C# text adapter's
NFC and CRLF/CR-to-LF transformation against original input bytes.

Install pinned dependencies with the repository's Python virtual environment and
`python -m pip install -r verification/requirements.txt`. `reportlab` and `python-docx`
support synthetic verifier tests and fixture generation; they do not establish
LibreOffice conversion success. Unit tests under `tests/python` deliberately use
synthetic PDFs and execution metadata. The real application and packaged executable
experiments are reported separately.
