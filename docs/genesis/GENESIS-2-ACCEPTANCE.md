# Genesis 2 acceptance matrix

Status: **VERIFIED OPERATIONAL PROTOTYPE; WHOLE-MILESTONE ACCEPTANCE QUALIFIED**.
Evidence date: 2026-10-22. Version 0.1.0, Windows 10 Pro x64.

The [frozen experiment](GENESIS-2-EXPERIMENT.md) remains unchanged. All ten criteria
were evaluated independently for three direct LibreOffice trials and three actual
packaged HYBRID.NET trials. Each trial passed. This demonstrates the scoped
capability, not universal application interoperability or production readiness.
Exact measurements and local evidence references are in the
[test report](GENESIS-2-TEST-REPORT.md); the
[aggregate JSON](genesis2-results.json) is intentionally selected publication evidence.

## Required functionality

| Requirement | Result | Evidence and limit |
| --- | --- | --- |
| Genesis 1 preserved | PASS | Foundation history and frozen experiment retained; founding coverage/documentation checker |
| Local runtime implemented | PASS | Actual C# CLI and core; no server/API/model needed |
| HACE registry operational | PASS | Actual application/version probes, registered-versus-verified state, current artifact hash and evidence reference |
| HMCP contracts validated | PASS | Version 0.1 JSON, duplicate-key/unknown-field/argument/path/version rejection; 44 contract tests |
| AIXR lifecycle | PASS | Legal states, bounded processes, explicit IDs, immutable terminal receipts; V4 and terminal record commit atomically |
| LibreOffice adapter operational | PASS | Installed soffice.com, supported headless PDF export, isolated profile for every invocation |
| DOCX fixture generated and preserved | PASS | Deterministic ORCHID-7319 source, fixed two-page expectation; before/after SHA-256 identical |
| PDF actually generated | PASS | Three framework PDFs, each two pages and 78,562 bytes |
| Independent PDF parsing | PASS | Separate Python process, pypdf strict parsing and pdfplumber table/text observations |
| AC-01 through AC-10 | PASS | All ten evaluated and passed in each of six trials; detailed per-criterion receipts |
| SQLite evidence persisted | PASS | Transactional operations/events/verification and independent read-only connection; no SQL Server |
| Receipts retrievable after restart | PASS | New packaged CLI processes retrieve all three framework trial receipts and history |
| Timeout/cancellation handling | PASS | Real owned-process and real LibreOffice tests; timeout remains INCONCLUSIVE, cancellation CANCELLED/INCONCLUSIVE |
| Failure injection and false success rejection | PASS | 11/11 packaged faults detected, zero false V4; additional interrupted-persistence and verifier defects in automated suites |
| Direct baseline/framework measurements | PASS | A,B,B,A,A,B order, same source/criteria, fresh isolated profiles and outputs, three trials per mode |
| Windows executable built | PASS | Release self-contained win-x64 complete bundle, build command/versions/file hashes recorded |
| Packaged executable tested | PASS | Separate copied bundle, working directory outside source/development directories; 33/33 checks |
| Standalone local operation | PASS | Executed without central server, cloud credentials, AI inference or a network endpoint |
| Physically disconnected offline operation | NOT TESTED | No global networking changes made on the shared workstation; a dedicated disconnected environment remains required for the strongest offline claim |
| Second capability evaluated | PASS | Managed text.normalize plus independent Python verification using the same registry/contracts/runtime/store |
| Reproducibility | PASS (scoped V5) | Two distinct V4 operations, same input/criteria/environment/verifier; semantic results agree while PDF byte hashes differ |
| Documentation and Genesis 1 regression | PASS | Existing documentation procedure plus scoped source review; final counts in test report |
| Development branch commit/publication | See Git | Work belongs to feat/genesis-2-runtime; publication receipt is local and ignored; main must remain unmerged |

## Frozen acceptance criteria

| Criterion | Independent observation in each real trial | Result |
| --- | --- | --- |
| AC-01 | Newly generated PDF exists at the requested destination | PASS |
| AC-02 | pypdf strict parser and pdfplumber read the structure without unresolved diagnostics | PASS |
| AC-03 | Exact unique title appears on page 1 | PASS |
| AC-04 | Predetermined paragraphs appear on their specified pages, in order | PASS |
| AC-05 | Ruled-cell extraction yields the exact 4-by-3 table matrix and row order | PASS |
| AC-06 | Exactly two pages; continuation heading and closing marker on page 2 | PASS |
| AC-07 | Nonempty PDF, computed hash, unchanged source hash and stable bytes on readback | PASS |
| AC-08 | Recorded supported headless LibreOffice invocation, PDF export filter and dedicated profile | PASS |
| AC-09 | Identified independent Python verifier and frozen criteria hash | PASS |
| AC-10 | Committed request/discovery/execution/artifact/verification bundle read from a separate SQLite connection | PASS |

AC-10 reads the committed provisional evidence before the final V4 transaction.
The final terminal receipt and V4 event then commit atomically. Later CLI retrieval
proves the terminal result survived process restart. A later inspection is a new
verification record; it does not rewrite the original V4 observation.

## Engineering gates

| Gate | Decision | Basis |
| --- | --- | --- |
| 1: Feasibility | PASSED | Real non-GUI LibreOffice operation plus independent acceptance |
| 2: Reusability | PASSED | Second managed text adapter reuses HMCP/HACE/AIXR/SQLite and the independent verification boundary |
| 3: Engineering advantage | INCONCLUSIVE | Direct mean wall time 15.331 s; framework 20.086 s. Human effort and maintainability baselines are not established |

Human intervention count, intervention time, adapter development effort and
HUMAN_TIME_RECOVERED remain NOT ESTABLISHED. Automated retries were zero; that does
not establish zero human engineering labor. Six successful positive trials and
11 correctly detected injected faults are this sample, not population reliability.

## Remaining scope and limitations

The offline requirement is architecturally satisfied and locally exercised without
remote services, but physical network disconnection was not tested. Therefore the
full mission matrix is not labeled an unconditional pass. Windows 11, clean-machine
deployment, visual fidelity, hostile-code isolation, multi-user security and signed
distribution remain untested. The .NET runtime is bundled; Python and LibreOffice
are external dependencies. SDKs for the other founding languages, a server, Studio,
MCP/network APIs, credential providers and installers remain planned.

The next bounded step is a dedicated disconnected deployment trial with the same
fixture and frozen criteria, followed by comparable human-effort observations.
Retain the existing proof and history; do not broaden the runtime merely to hide
these remaining evidence gaps.
