# Project status

**HYBRID.NET GENESIS 2 · 0.1.0 · LOCAL EXECUTION PROTOTYPE**

Genesis 1 remains complete and preserved. Genesis 2 implements and verifies the
first local execution path on Windows 10 x64. The final packaged experiment passed
33 checks, including six real conversions, independent acceptance, restart retrieval,
semantic reproduction and a second capability. Full milestone acceptance remains
qualified: physical offline operation is not tested and engineering advantage is
inconclusive. See the [acceptance matrix](docs/genesis/GENESIS-2-ACCEPTANCE.md) and
[test report](docs/genesis/GENESIS-2-TEST-REPORT.md).

| Classification | Current state |
| --- | --- |
| IMPLEMENTED | C#/.NET CLI/core, HACE registry, HMCP JSON, AIXR lifecycle, LibreOffice and managed-text adapters, Python verification, SQLite evidence and recovery |
| TESTED | 118 automated tests passed: 44 contracts, 24 SQLite, 17 process/adapter, 33 Python; 33 packaged acceptance checks passed |
| VERIFIED | All six direct/framework trials: AC-01–AC-10 PASS; semantic V5 reproduction; text.normalize V4; 11/11 controlled faults detected with zero false V4 |
| INCONCLUSIVE / untested | Engineering advantage and human labor savings not established; physical offline, Windows 11, clean-machine and pixel-perfect fidelity not tested |
| PLANNED | Other language workers, server, Studio, network API, MCP tools, credential providers, SQL Server and installers |

The six language roles are not six implemented workers. `Hybrid.Client.exe`,
`Hybrid.Server.exe`, Studio and setup executables remain future products.
`Hybrid.CLI.exe` is the implemented local interface. The self-contained `win-x64`
bundle includes .NET; Python with pinned verifier packages and LibreOffice remain
external dependencies. Windows 11 and clean-machine portability have not been tested.

The client must remain independent of the server. Deterministic local operations
must not depend on inference or internet access. Installed programs are not verified
capabilities. Remote enrollment does not grant unrestricted local authority.

## Genesis evidence

The starting branch was `main` at
`59924a07d7870665478d9ed279f2af8ab76daf32`, with a clean working tree. Genesis 2 uses
`feat/genesis-2-runtime`; publication does not authorize merging into `main`.
The documentation baseline passed exit 0: 71 Markdown files, 289 local links, four
JSON files, 45 founding sections and 79 reviewed files. This was not a runtime suite.

First packaged evidence remains local and ignored:
`I:\HYBRID-NET\.local\first-real\cli-result.json` and
`I:\HYBRID-NET\.local\first-real\reverification.json`. It records LibreOffice
26.8.1.1, unchanged DOCX bytes, two PDF pages, independent content/table checks and
SQLite readback. These are dated checkpoints, not substitutes for final validation.

The [acceptance checklist](docs/genesis/GENESIS-1-ACCEPTANCE.md) defines completion.
The [report](docs/genesis/GENESIS-1-REPORT.md) records actual checks and publication.
The [founding traceability](docs/genesis/FOUNDING-TRACEABILITY.md) preserves every
numbered mission section. Git identifies the current commit; do not infer current
working-tree or remote state from a dated document.
The [architectural addendum](docs/genesis/ARCHITECTURAL-ADDENDUM.md) maps the HACE,
independent-evidence and first-experiment refinements to their canonical documents.

Initial local path: `I:\HYBRID-NET`. Published public repository:
[chatgptopenaiagi/HYBRID-NET](https://github.com/chatgptopenaiagi/HYBRID-NET).
Publication was verified on 2026-10-22: remote `main` matched the scoped addendum
commit `e949356a23db9edd848536c034778b19fdf5572f`, with all 79 files present.
The report records validation and the subsequent publication-evidence update.

## Current engineering work

The [implementation guide](docs/genesis/GENESIS-2-IMPLEMENTATION.md) records the
working path, dependencies, commands and limitations. The frozen
[experiment](docs/genesis/GENESIS-2-EXPERIMENT.md) remains the criterion authority.
The final evidence is preserved under `I:\HYBRID-NET\.local\genesis2-final`.
Feasibility and reuse gates passed; engineering advantage remains inconclusive.
Mean direct/framework wall times were 15.331/20.086 seconds, three trials each.
Next, close the controlled offline acceptance gap and establish comparable human
effort observations before advancing any advantage claim. See [roadmap](ROADMAP.md).

Do not interpret this local prototype as production readiness, universal
executable control, vendor integration, sandboxing, runtime interoperability or
measured human time saved. Existing CFA, SCE, QASC/CLARA, CAE and POK projects remain
independent and unchanged by this milestone.
