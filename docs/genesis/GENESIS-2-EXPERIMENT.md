# Genesis 2: independently verified LibreOffice DOCX-to-PDF experiment

Status: **PROPOSED ARCHITECTURE / future experiment plan**. This document specifies
the next milestone; it creates no DOCX fixture, PDF, database, adapter or runtime.
Genesis 1 remains an architecture foundation. Nothing here is evidence that the
experiment has run.

## Purpose and execution path

The preferred Genesis 2 proof is a real document conversion with an independently
checked result:

```text
Codex
  -> HACE discovery
  -> HMCP capability invocation
  -> AIXR bounded local execution
  -> LibreOffice supported non-GUI interface
  -> independent PDF verifier
  -> local SQLite execution and verification evidence
```

HACE means Hybrid Application Capability Engine. Its discovery result identifies
the concrete adapter, supported operation, application version and current
availability. HACE participates in the broader discover/identify/inspect/adapt/
register/execute/verify lifecycle; it does not replace AIXR or independently own
every stage. Codex must invoke the exposed capability and receive its structured
receipt. A manually executed conversion alone cannot establish this complete path.

Choose the smallest local runtime, adapter and agent-facing mapping needed for
this experiment. No central HYBRID.NET Server, Studio GUI, SQL Server, six-language
worker set or modifications to LibreOffice are required. Runtime discovery,
conversion, verification and persistence must work locally without internet access;
any connectivity used by the chosen agent/model is a separately reported property,
not a dependency of those deterministic operations. The result proves this narrow
standalone path, not a finished standalone client product.

## Fixed future input fixture

Create a new, disposable fixture only during the authorized implementation
milestone. Its planned identifier is `HNET-G2-DOCXPDF-ORCHID-7319`, file name
`hnet-g2-orchid-7319.docx`, with these exact contents:

| Location | Required content |
|---|---|
| Page 1 title | `HYBRID.NET Genesis 2 - ORCHID-7319` |
| Page 1 paragraph 1 | `This synthetic document tests direct machine conversion without graphical automation.` |
| Page 1 paragraph 2 | `The source document must remain unchanged while the PDF is checked independently.` |
| Page 1 table header | `Item` / `Description` / `Amount` |
| Page 1 table row 1 | `A101` / `Amber sample` / `12.50` |
| Page 1 table row 2 | `B202` / `Blue sample` / `7.50` |
| Page 1 table row 3 | `C303` / `Coral sample` / `5.00` |
| Page break | One explicit page break after the table, before page 2 content |
| Page 2 heading | `Verification continuation - ORCHID-7319` |
| Page 2 paragraph | `All three synthetic table rows must survive conversion in their original order.` |
| Page 2 closing line | `END-OF-FIXTURE-ORCHID-7319` |

The predetermined page count is **2**, chosen before conversion. The planned
layout is A4 portrait, 20 mm margins, Arial 11 pt body text, 16 pt bold title,
1.15 line spacing and a 170 mm table with columns 30/85/55 mm. No dynamic fields,
macros, linked resources, tracked changes or personal content belong in the fixture.
Keep content short enough to fit comfortably on each prescribed page.

Before the trial, pin the fixture bytes and SHA-256, exact fonts and versions,
LibreOffice build, PDF export filter/options, verifier version and criteria file.
Confirm the declared font exists; do not infer that it does. If preflight requires
a layout/font adjustment, version and review the fixture specification before
running the trial. Preserve the two-page requirement and original content. Never
learn an expected page count from the candidate output or weaken a failed criterion.

## Supported conversion and independent verification

Use a documented non-GUI interface: a narrowly scoped UNO operation or the
documented headless conversion CLI. LibreOffice documents headless operation and
`--convert-to`/`--outdir`; its PDF export documentation describes filter options.
These are interface research, not local availability evidence.
[Startup parameters](https://help.libreoffice.org/latest/en-US/text/shared/guide/start_parameters.html),
[PDF export parameters](https://help.libreoffice.org/latest/en-US/text/shared/guide/pdf_params.html).

At implementation time, resolve the actual executable, test the chosen interface
and use a disposable profile/output directory. Preserve user profiles, open
documents and existing processes. Use explicit arguments, bounded execution and
captured receipts; do not employ clicks, keystrokes, screenshot-driven control or
a hidden GUI fallback. Missing prerequisites produce a documented blocker. No
application is installed, repaired or reconfigured during this planning milestone.

A separate verifier must open the actual output PDF with a PDF-reading library
or tool independent of the LibreOffice conversion adapter. It receives the frozen
criteria and artifact reference, not the adapter's claim as its answer. A library
such as pypdf is a candidate, not a selection: its documentation explains that
text extraction and table structure need deliberate handling.
[PDF extraction limitations](https://pypdf.readthedocs.io/en/stable/user/extract-text.html).

Freeze text comparison rules before the trial: normalize Unicode to NFC, collapse
whitespace runs to one space, preserve case, punctuation and numeric spelling.
Compare known paragraphs on their prescribed pages and in their declared order.
For the table, extract or reconstruct the predefined four-by-three cell matrix
using a declared, tested structure/coordinate method; require exact row and column
association. Finding the words anywhere in the PDF is insufficient. If the chosen
parser cannot establish those associations, report inconclusive/failure and improve
the verifier without changing the fixed expected matrix.

## Mandatory acceptance cases

All ten cases below are mandatory and fixed before execution. Record actual
observations, outcome and evidence reference for each; skipped, inconclusive or
failed cases prevent V4.

| ID | Criterion | Required independent evidence |
|---|---|---|
| AC-01 | Expected PDF exists | Verifier resolves the new output path in the disposable run directory and confirms an actual file, not a stale artifact from a previous run. |
| AC-02 | PDF is readable by an independent parser | The separately identified parser opens the PDF and reads its page/content structures without an unreconciled parsing error. Adapter exit status is not a substitute. |
| AC-03 | Unique title is preserved | Page 1 contains exactly `HYBRID.NET Genesis 2 - ORCHID-7319` under the frozen normalization rule. |
| AC-04 | Known paragraphs are preserved | Both page 1 paragraphs and the page 2 paragraph match the specified text, page placement and order. |
| AC-05 | Table content is preserved | The page 1 four-by-three header/data matrix matches every cell and its row/column association, including `12.50`, `7.50` and `5.00`. |
| AC-06 | Predetermined page count matches | Independent parser reports exactly **2** pages; the second heading and closing line appear on page 2. |
| AC-07 | Artifact is nonempty and fingerprinted; input is preserved | Independent inspection records positive PDF byte size and SHA-256 of the same file read by the verifier; recheck output identity before recording the final outcome. Compare the source DOCX's post-conversion SHA-256 with its frozen pre-conversion hash and require equality. Record both input observations. |
| AC-08 | Conversion used a supported non-GUI interface | Execution receipt identifies the actual LibreOffice build, chosen documented interface, arguments/options and process result; the path contains no GUI automation. |
| AC-09 | Verification is independent of conversion | Record verifier implementation/version, frozen criteria hash and its own observations; it does not rely solely on LibreOffice's completion message, output filename or adapter self-report. |
| AC-10 | Evidence is recorded in local SQLite | Read back a committed local record linking request/job, HACE discovery, HMCP contract, AIXR execution, input/output hashes, verifier identity and AC-01 through AC-09 outcomes; persist AC-10's storage-readback receipt and final level. |

AC-10's checker should independently read the committed evidence through the local
storage boundary and then append its storage-validation outcome. A provisional
record must not present itself as V4 before the ten-case gate is satisfied. The
subsequent final record must remain queryable after closing and reopening the
local database. Secrets and unrelated document contents must not enter evidence.

## Verification level and reproducibility

Apply the [Independent Evidence Principle](../architecture/INDEPENDENT-VERIFICATION.md):
V0 requested; V1 accepted; V2 executed; V3 independently observed; V4 verified
against explicit predetermined criteria; V5 reproduced under documented conditions.
These are proposed HYBRID.NET labels, not an industry standard or implemented
state machine.

Award **V4 only when AC-01 through AC-10 all pass**. Process completion alone is V2 at most;
an independent inspection with a failed criterion is not V4. V5 requires a
separate repetition using the same original criteria, pinned input and documented
conditions, a fresh output location and new linked receipts. A second identical
failure is not reproducibility of a verified outcome. Record each output hash;
PDF metadata may differ, so functional reproducibility does not imply byte-for-byte
identity unless that stricter criterion was specified beforehand.

Text, table association and page-count checks do not prove pixel-perfect layout,
typography or accessibility. If those become requirements, add separately fixed
rendering/visual or accessibility criteria before claiming them; never infer them
from these ten cases. Results apply to the recorded fixture/environment, not all
DOCX documents, all LibreOffice versions or arbitrary applications.

## Supporting engineering and handoff

An optional synthetic two-process exchange can precede the real trial to test
framing, malformed requests, timeout and process cleanup. It is not the Genesis 2
completion criterion and cannot substitute for the real conversion. Additional
negative tests should reject missing input, stale output, missing title/table
content, wrong page count, parser failure and interrupted evidence writes without
weakening the successful-case criteria.

The future report must retain the original fixture/criteria hashes, actual
application and verifier versions, complete invocation receipts, all acceptance
observations, SQLite readback evidence, measured durations, blocked prerequisites
and whether V5 was separately tested. Keep machine-specific artifacts outside
tracked source. This plan authorizes no current execution or installation;
implementation begins only in the next engineering milestone.
