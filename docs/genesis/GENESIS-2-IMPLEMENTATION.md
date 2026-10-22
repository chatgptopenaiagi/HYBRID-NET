# Genesis 2 implementation and operator workflow

**IMPLEMENTED COMPONENT — HYBRID.NET 0.1.0 local prototype.** The first real
packaged LibreOffice conversion and a later independent re-verification passed
the ten frozen criteria at V4. Full milestone acceptance still requires the
consolidated regression, failure-injection, comparison and reproducibility evidence.
The [original experiment](GENESIS-2-EXPERIMENT.md) remains the criterion authority;
its historical planning language is not a claim that current code is absent.

## Implemented ownership

| Component | Source and responsibility |
| --- | --- |
| CLI | [Program.cs](../../client/Hybrid.CLI/Program.cs): local JSON commands, explicit paths and exit codes |
| HMCP | [Contracts.cs](../../core/Hybrid.Core/Contracts.cs): strict fields/version/path validation, canonical request hash, operation IDs and legal lifecycle transitions |
| HACE/adapters | [Adapters.cs](../../core/Hybrid.Core/Adapters.cs): registered operations, current application probes, LibreOffice conversion and managed UTF-8 normalization |
| AIXR | [Runtime.cs](../../core/Hybrid.Core/Runtime.cs): admission, execution, independent verification, evidence completion, re-verification and reproduction |
| Processes | [ProcessRunner.cs](../../core/Hybrid.Core/ProcessRunner.cs): explicit argument arrays, bounded capture, deadlines, cancellation and owned-tree cleanup |
| Persistence | [EvidenceStore.cs](../../core/Hybrid.Core/EvidenceStore.cs): transactional SQLite, immutable original receipts/events/verifications, ownership-aware recovery |
| Independent observer | [verify.py](../../verification/verify.py): PDF/text criteria; [store_readback.py](../../verification/store_readback.py): committed evidence through a new Python SQLite connection |
| Fixture | [fixture.py](../../verification/fixture.py) and [criteria](../../verification/genesis2-criteria.json): exact original content, deterministic DOCX ZIP and two-page expectation |

This is one core library and one CLI, not three HACE/HMCP/AIXR services. There is
no server, Studio, MCP endpoint, network API, Windows service, driver, SQL Server
dependency or inference requirement. Future human and AI interfaces can share
the core; no graphical user interface is built during this milestone.

## Tool selection and reproducible setup

The observed environment provides .NET SDK 10.0.402, published .NET 10.0.13 x64,
Python 3.14.7 and LibreOffice 26.8.1.1. These are dated observations, not universal
prerequisite guarantees. `global.json` selects stable .NET 10 instead of the
installed preview. Microsoft.Data.Sqlite is pinned at 10.0.13; Python dependencies
are pinned in [requirements.txt](../../verification/requirements.txt). The
[runtime ADR](../decisions/ADR-0009-GENESIS-2-RUNTIME.md) records tradeoffs.

PowerShell 7 setup on the observed workstation:

```powershell
$repo = 'I:\HYBRID-NET'
& "$repo\scripts\setup.ps1" -Python 'C:\Python314\python.exe'
if ($LASTEXITCODE -ne 0) { throw 'Local dependency setup failed' }
& "$repo\scripts\build.ps1"
& "$repo\scripts\test.ps1"
& "$repo\scripts\validate-docs.ps1" -ReportPath "$repo\.local\docs-validation.json"
```

The [setup entry point](../../scripts/setup.ps1) creates the repository-local
environment if absent, installs pinned Python requirements and restores the CLI.
Use a suitable installed 64-bit Python path on another development workstation;
the example `C:\Python314\python.exe` is the observed base interpreter.
Build/dependency acquisition may use network access. Executing the installed local
capability does not require a network service, cloud key, AI provider or server.
This distinction is not a clean-machine or disconnected-host test.

[environment.ps1](../../scripts/environment.ps1) sets process-local .NET home,
NuGet/pip caches and temporary directories under `.local`, disables telemetry and
opts out of ASP.NET certificate generation. Initial SDK first-run output included
a development-certificate creation message; no trust request or explicit certificate
commands were performed. No further certificate actions are required for this CLI.
This incidental SDK initialization is not a framework credential feature or an
operational dependency.

## Build and complete delivery bundle

[build.ps1](../../scripts/build.ps1) publishes Release `win-x64`, self-contained
by default, to `.local/publish/win-x64`. It records the command, selected SDK,
duration, file sizes and SHA-256 hashes in `build-receipt.json`. Keep the entire
directory, including native SQLite and `verification` files. Copying only
`Hybrid.CLI.exe` is not a supported deployment.

`-FrameworkDependent` explicitly selects a bundle requiring an installed compatible
.NET runtime. Default self-contained delivery includes .NET, but **Python with
pinned dependencies and LibreOffice remain external**. Set `--python` explicitly;
the default looks for `python/python.exe` beside the CLI and does not supply it.
No installer, code signing, Windows 11 verification or clean-machine test is implied.

## Actual CLI contract

Run `Hybrid.CLI.exe help` for the implemented command list. All responses are JSON.
The [HMCP 0.1 contract](../protocols/HMCP-0.1.md) and
[structural schema](../../protocols/hmcp-0.1.schema.json) describe the implemented
interface; the older conceptual examples remain historical architecture material.

| Command | Behavior |
| --- | --- |
| `version` | Product, HMCP, runtime and process architecture |
| `doctor` | Python probe, verifier presence, database location and registered capabilities; inspect fields, not only query exit code |
| `discover`, `applications`, `capabilities` | Same current explicit adapter inventory; installed, registered and verified are separate fields |
| `inspect LibreOffice` | Current matching application entry |
| `capability document.export_pdf` | Current matching capability entry |
| `execute <capability> --source <absolute> --output <absolute>` | Construct and validate a local HMCP request |
| `execute --request <json-file>` | Read an explicit request and reject malformed/unsupported contracts |
| `receipt <id>`, `history` | Retrieve durable state after process restart |
| `verify <id>` | Add independent re-verification evidence without replacing the original receipt |
| `reproduce <first-id> <second-id>` | Validate separate compatible V4 trials and persist a separate V5 claim |

Global options are `--workspace`, `--python`, `--verifier-dir` and `--libreoffice`.
Execution options are `--operation-id`, `--criteria`, `--timeout` (1–300 seconds)
and `--cancel-after-ms` (bounded cancellation experiment). Ctrl+C requests
cancellation. Exit codes are 0 for a successful query or VERIFIED operation, 2 for
FAILED, 3 for INCONCLUSIVE, 4 for invalid command/infrastructure error and 130 for
CANCELLED. Query exit 0 alone does not establish an available or verified capability.

Without `--workspace`, state defaults to `state` beside the executable, or
`HYBRID_WORKSPACE` when set. Without `--python`, `HYBRID_PYTHON` may supply the
interpreter. Prefer explicit arguments for reviewable commands. Source, destination
and custom criteria must resolve inside the declared workspace. Reparse paths,
UNC paths, reserved Windows names and alternate data streams are rejected. Existing
outputs are not silently overwritten. This policy is not a full OS sandbox.

## Create and execute the frozen fixture

Use a new workspace for each controlled demonstration. This preserves previous
receipts and avoids mistaking an existing output for a fresh conversion.

```powershell
$repo = 'I:\HYBRID-NET'
. "$repo\scripts\environment.ps1"
$cli = "$repo\.local\publish\win-x64\Hybrid.CLI.exe"
$workspace = Join-Path "$repo\.local\manual-demo" ([Guid]::NewGuid().ToString('N'))
$source = Join-Path $workspace 'input\hnet-g2-orchid-7319.docx'
& $PythonPath "$repo\verification\fixture.py" --output $source
if ($LASTEXITCODE -ne 0) { throw 'Fixture creation failed' }
New-Item -ItemType Directory -Force -Path "$workspace\output" | Out-Null
& $cli execute document.export_pdf --workspace $workspace --python $PythonPath `
    --source $source --output "$workspace\output\result.pdf" `
    --criteria "$workspace\input\criteria.json" --operation-id demo-one
& $cli receipt demo-one --workspace $workspace --python $PythonPath
& $cli verify demo-one --workspace $workspace --python $PythonPath
```

The fixture generator refuses existing fixture/criteria paths. It preserves the
specified title, paragraphs, 4×3 table, explicit page break and expected two pages.
The adapter copies source bytes into an operation directory and records before,
staged and after hashes. It invokes `soffice.com` with a unique
`-env:UserInstallation=file:///...` profile, `--headless`, `--convert-to
pdf:writer_pdf_Export` and `--outdir`. Regular LibreOffice profiles and unrelated
processes are not reused or terminated.

The independent verifier uses strict pypdf parsing and pdfplumber text/table
observations; the table check inspects ruled cell geometry and exact normalized
values. It checks page-specific content, source preservation and output identity.
AC-10 uses another Python process to read the committed operation/verification
bundle from SQLite. Every AC-01–AC-10 result has explicit expectation, observation,
outcome and provenance. Missing evidence yields INCONCLUSIVE; a demonstrated
criterion violation yields FAILED. Text/structure checks do not prove pixel-perfect
formatting. SHA-256 identifies bytes and does not establish semantic correctness.

## Persistence, restart and evidence levels

The database is `<workspace>/engine/evidence.sqlite`; accepted operation files are
under `<workspace>/runs/op-<operation-id>-<first-12-hex-of-ID-SHA256>`, avoiding
Windows device and case aliases while preserving the full ID in the database.
Read the exact path from the receipt. Tables are `operations`, `execution_events`,
`verification_results`, `registry` and `reproducibility_runs`. Writes use transactions,
WAL, FULL synchronization, foreign keys, parameterized SQL and bounded busy waits.

Duplicate IDs with the same canonical request hash return existing state without
replay. A different hash raises `OPERATION_ID_CONFLICT`. Original receipts cannot be
overwritten; re-verification is a new record. A later failed or inconclusive
re-verification prevents that receipt from being used as current capability proof.
Current artifact hashes are checked when advertising prior proof.

Recovery inspects owner PID and process start identity. An abandoned operation is
recorded INCONCLUSIVE with unknown effects, preserving previous events; no process
is killed and no operation is automatically replayed. V5 is a separate linked
reproduction record with an appended event; original V4 receipts remain unchanged.
Matching output bytes are reported separately from semantic reproducibility.

## Adapter reuse and future languages

The current C# binding exposes capability/application/adapter identity, version,
`DiscoverAsync` and bounded `ExecuteAsync(source, destination, runDirectory,
timeout, cancellationToken)`. An adapter returns execution metadata, application
identity, actual interface arguments, output references, errors and interruption
status. It must not set the framework's independent verification conclusion.

`text.normalize` (`Hybrid.Text`, adapter `managed.text-normalize`) accepts bounded
UTF-8 input without BOM, normalizes Unicode to NFC and converts CRLF/CR to LF
through a managed path. The same runtime
and database invoke independent Python checks under its own frozen text contract.
This is a genuine second implementation path; the packaged reuse experiment passed
independent verification. No transport-neutral worker SDK is implemented yet. A future
Go, C++, Java, JavaScript/TypeScript or Python worker must map these semantics and
evidence requirements to a versioned process contract rather than receiving an
unrestricted shell interface. All six founding language roles remain preserved.

## First packaged evidence and remaining limits

The starting Genesis 1 documentation baseline passed exit 0: 71 Markdown files,
289 local links, four JSON files, 45 founding sections and 79 reviewed files.
There was no prior runtime regression suite to claim.

On 2026-10-22, packaged operation `first-real` passed all ten criteria at V4 with
LibreOffice 26.8.1.1. The two-page PDF was 78,562 bytes. The preserved source hash was
`09d5f6e97ffe1ee88ae1dd499405f2bdeb7b3bf1c061658a33742cd746be310d`; output hash was
`9b99f23a03d160bf6e29798d2c45e2b782714607e37397de61c60e24e6f58295`.
Local receipts are `.local/first-real/cli-result.json` and
`.local/first-real/reverification.json`; the latter also passed all ten criteria
without replacing the first receipt. Generated files, model-independent evidence,
databases, caches and logs remain ignored.

The real process emitted `Could not find platform independent libraries <prefix>`
on stderr while exiting 0; the warning is retained in evidence. The V4 conclusion
comes from independent content, structure, integrity and persistence checks, not
from hiding stderr or trusting that exit code.

These are historical checkpoint observations. The final [test report](GENESIS-2-TEST-REPORT.md)
records 118 automated tests and 33 packaged checks passing, six successful conversion
trials, 11 detected faults, semantic V5 and a verified text capability. The
[acceptance matrix](GENESIS-2-ACCEPTANCE.md) retains the physical offline gap and
inconclusive engineering-advantage gate. Human time recovered is NOT ESTABLISHED
without comparable observed labor; sampled root-process memory is not total
LibreOffice-tree memory. Windows 11, clean-machine deployment, pixel-perfect
rendering and production security remain untested. No neighboring project was
needed to initialize or execute this path.
