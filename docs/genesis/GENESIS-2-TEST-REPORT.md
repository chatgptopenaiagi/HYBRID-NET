# Genesis 2 implementation and verification report

Evidence date: **2026-10-22**. Version **0.1.0**. Classification: **local prototype**.

The actual packaged executable passed 33 acceptance checks, including three real
LibreOffice framework conversions, three equivalent direct conversions, semantic
reproduction, independent text-capability reuse and 11 fault scenarios. All six
positive conversion trials passed AC-01–AC-10. The automated suite passed 118 tests
with zero failures or skips. These results support feasibility and reuse; engineering
advantage remains inconclusive and physically disconnected execution was not tested.
The [acceptance matrix](GENESIS-2-ACCEPTANCE.md) keeps those distinctions explicit.

## Repository and baseline

Verified root: `I:\HYBRID-NET`. Starting branch: `main`. Starting clean HEAD:
`59924a07d7870665478d9ed279f2af8ab76daf32`. Origin:
`https://github.com/chatgptopenaiagi/HYBRID-NET.git`. Fetch/history comparison found
no newer origin/main commit. Work uses `feat/genesis-2-runtime`; no main reset,
history rewrite, merge or release publication is authorized by this report.
Implementation and tests are committed as
`358fc92d9e8fd03093a2d8a577b6df3c23697bb1`; this report is a following scoped
documentation commit. Use Git for the final branch tip and the ignored
`.local/genesis2-publication.json` for the observed remote publication receipt.

The baseline `scripts/validate-docs.ps1` passed, exit 0: 71 Markdown files,
289 local links, four JSON files, 45 founding sections and 79 reviewed files.
Receipt: `.local/genesis2-baseline.json`. There was no pre-existing runtime suite.
No baseline failure has been reclassified as a new regression or hidden.

## Implemented components

Two C# projects implement the CLI/core. Core files define validated HMCP requests,
HACE adapters/registry, AIXR lifecycle, bounded process execution, independent
verification orchestration and SQLite durability. Python supplies the controlled
DOCX generator, PDF/text verifier and independent SQLite readback. A managed text
adapter demonstrates a different execution path. Six PowerShell entry points
provide environment setup, build, tests, demonstration and re-verification.

The [implementation guide](GENESIS-2-IMPLEMENTATION.md),
[HMCP binding](../protocols/HMCP-0.1.md) and
[ADR-0009](../decisions/ADR-0009-GENESIS-2-RUNTIME.md) describe contracts, selected
technologies, legal transitions, limits and future language-worker boundaries.
Original acceptance criteria and Genesis 1 architecture remain preserved.

## Environment and build

| Component | Observed selection |
| --- | --- |
| Operating system | Windows 10 Pro 10.0.19045 x64 |
| PowerShell | 7.6.6, C:\Program Files\PowerShell\7\pwsh.exe |
| SDK/runtime | C:\Program Files\dotnet\dotnet.exe; SDK 10.0.402, bundled .NET 10.0.13 |
| Publish mode | Release, win-x64, self-contained directory bundle |
| SQLite provider | Microsoft.Data.Sqlite 10.0.13 with native e_sqlite3 in the bundle |
| Python | C:\Python314\python.exe 3.14.7 x64; project venv .local\venv\Scripts\python.exe |
| Independent parsers | pypdf 6.21.2, pdfplumber 0.11.10, pdfminer.six 20260107 |
| Independent SQLite readback | Python sqlite3 3.50.4 |
| LibreOffice | C:\Program Files\LibreOffice\program\soffice.com; 26.8.1.1 bf6ef2662401c7d6dce671ab4deb26ee0af3a7c1 |
| Fixture font | Installed Arial; hash retained in .local/font-evidence.json |

The build command, explicit arguments, exit 0, duration and complete file manifest
are in `.local/publish/win-x64/build-receipt.json`:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' publish `
  'I:\HYBRID-NET\client\Hybrid.CLI\Hybrid.CLI.csproj' `
  --configuration Release --runtime win-x64 --self-contained true `
  --output 'I:\HYBRID-NET\.local\publish\win-x64' --verbosity minimal
```

The final build source manifest contains 17 files and fingerprint
`4ccbd7b1eaf7dee287e144848d04f4d021addc865bc0a6eb5a4ff0474a2bdde8`.
It records the foundation HEAD plus a dirty worktree because compilation preceded
the implementation commit. The manifest, rather than a future commit claim, binds
the tested source bytes. All listed files matched at final inspection. The copied
acceptance bundle retains its original earlier build receipt; the later publish
receipt adds source-manifest metadata without changing runtime binary hashes.

The 205 delivered runtime/verifier files total 83,006,040 bytes, excluding the build
receipt. Retain the entire directory. Python and LibreOffice are not bundled.
No clean-machine or Windows 11 deployment result is claimed. The first SDK invocation
reported creating its development HTTPS certificate; no trust operation was run.
Subsequent scripts disable that SDK first-run behavior. No runtime network listener
or service was introduced.

## Final automated regression

Command: `& I:\HYBRID-NET\scripts\test.ps1`, exit **0**.
Run: 2026-10-22 09:41:19–09:41:52 UTC.

| Suite | Tests | Passed | Failed | Evidence type |
| --- | ---: | ---: | ---: | --- |
| Contracts | 44 | 44 | 0 | Actual validation, path/scope and lifecycle code |
| SQLite store | 24 | 24 | 0 | Actual SQLite transactions, concurrent reservations, killed owned writer, restart/rollback and immutable receipts |
| Process/adapter | 17 | 17 | 0 | Actual disposable child processes, bounded capture, timeout/cancel/tree cleanup and text transformations |
| Python verification | 33 | 33 | 0 | Synthetic valid/defective PDFs, source preservation, deterministic DOCX, parser and readback behavior |
| Total | 118 | 118 | 0 | Zero skipped |

Exact executable paths, argument arrays, exit codes and per-suite logs:
`I:\HYBRID-NET\.local\test-runs\20261022T094119974Z-b888353f\summary.json`.
These tests do not substitute for real LibreOffice acceptance.

Tests exposed Windows alias handling and SQLite test-connection cleanup issues;
these were fixed and affected/full suites rerun. Review also tightened CLI argument
validation, internal reparse-path rejection and atomic V4 finalization. Earlier
receipts remain in `.local`; final results do not erase those attempts.

A final script review corrected setup/build SDK selection when invoked from a
different current directory. An actual caller directory pinned to SDK 8.0.426
invoked both scripts successfully; they selected repository SDK 10.0.402, restored
the caller directory and produced exactly the same 205 payload file hashes.
Receipt: `.local/script-cwd-verification.json`. No runtime source changed after
the final regression and packaged experiment.

## Real packaged experiment

Command:

```powershell
& I:\HYBRID-NET\scripts\demo-genesis2.ps1 `
  -RunDirectory 'I:\HYBRID-NET\.local\genesis2-final'
```

Exit **0**, 33/33 checks, run 09:39:50–09:43:51 UTC. The harness copied the complete
bundle into `separate delivery bundle` and launched it with explicit paths from
`outside source working directory`. No development working directory or accidental
PATH entry supplied its dependencies. The external Python venv was explicit.

Final evidence root: `I:\HYBRID-NET\.local\genesis2-final`.
`summary.json` records every check and raw receipt reference. `commands` preserves
command arguments, stdout, stderr and results. `framework state\engine\evidence.sqlite`
contains operation/event/verification history. Direct trials use separate
`direct baseline\trial-*-a\baseline-evidence.sqlite` databases with equivalent
independent readback. `delivery-manifest.json` fingerprints every copied file.

The DOCX fixture is `framework state\fixture\hnet-g2-orchid-7319.docx`, 35,894 bytes,
SHA-256 `09d5f6e97ffe1ee88ae1dd499405f2bdeb7b3bf1c061658a33742cd746be310d`.
The criteria SHA-256 is
`8fa474df1d2134d28ba0fa489d40b44691e73757b3ee38b84bf1698f466296de`.
The two-page expectation, title, paragraphs, ordered table, continuation heading
and closing marker were fixed before execution and remained unchanged.

Representative output: `framework state\outputs\trial-2-b.pdf`, 78,562 bytes,
SHA-256 `b3f48aa050c5b4f2e3e42698c1dd399898ca55f95f9ae37fc5f5722809dcc98f`.
Its `commands\trial-2-b-result.json` records all ten criteria passing at V4.
Fresh CLI processes retrieved all framework trial receipts. Re-verification added
history without changing original receipt JSON. A later capability query returned
`verified_for_recorded_case` with matching output integrity and an evidence reference.

LibreOffice emitted the nonfatal warning
`Could not find platform independent libraries <prefix>` with exit 0. It is retained
in execution evidence. Independent observations, not the process exit, establish V4.

## Direct baseline and framework comparison

Run order was **A,B,B,A,A,B**; no retries. Both modes used identical fixture bytes,
the same frozen criteria, the same installed LibreOffice/Python and fresh isolated
profiles/output paths. Baseline A invoked LibreOffice directly through the harness,
not through AIXR, then ran equivalent independent parsing and durable SQLite readback.

| Mode | Trials | Success | Mean conversion ms | Mean parser verification ms | Mean total wall ms |
| --- | ---: | ---: | ---: | ---: | ---: |
| A: direct | 3 | 3/3 | 12,263.504 | 271.376 | 15,331.300 |
| B: HYBRID.NET | 3 | 3/3 | 13,376.668 | 268.800 | 20,086.258 |

Total wall time includes orchestration, current preflight/version probing, persistence
and readback; parser time excludes independent SQLite readback. Repeated framework
preflights contribute overhead. This small ordered sample supports no speed advantage;
there was no cold-cache reset or statistical population claim. Root-launcher sampled
working sets were roughly 5.9 MB and exclude the much larger LibreOffice child tree.
They are not total application memory measurements.

SQLite evidence storage was 921,600 bytes at summary creation across the experiment
databases; this is not the size of all retained profiles, logs and delivery files.
Automated retry count was zero. Human intervention count/time, adapter development
effort and HUMAN_TIME_RECOVERED are NOT ESTABLISHED because no comparable observed
human baseline was collected.

## Faults, reproduction and reuse

The packaged harness detected all 11 injected faults, with zero false V4 conclusions:
missing source; invalid destination; corrupted DOCX; deliberately wrong page-count
criteria in a disposable negative test; missing verifier; execution timeout;
cancellation; corrupted generated PDF; deleted generated PDF; conflicting duplicate
operation; and interruption of only the owned LibreOffice process. The canonical
positive criteria were never changed. Identical duplicate requests returned the
existing receipt without re-execution. Timeout/cancellation/infrastructure uncertainty
stayed distinct from demonstrated acceptance failure.

Interrupted evidence persistence was safely tested in the SQLite suite: killed
owned writer, transaction rollback, dead-owner recovery and a forced finalization
failure between receipt update and terminal-event insertion. The latter left no
V4 state and preserved the prior VERIFYING/V3 evidence. No power-loss durability
or hardware fault is claimed. Python tests separately cover truncated/unreadable
PDFs, wrong pages, missing text, altered table values and missing artifacts.

The first acceptance harness had three overly strict whole-response comparisons
because retrieved receipts legitimately add history/events. Database `receipt_json`
and original file hashes showed the original receipts were unchanged. The corrected
harness compares immutable fields and stored original bytes. That failed 30/33 run
and diagnosis remain in `.local/genesis2-experiments/summary.json` and
`harness-assertion-review.json`; the final fresh 33/33 run is separate.

`trial-2-b` and `trial-3-b` passed the same contract with matching source, criteria,
environment and verifier identity. The separate reproducibility record reaches V5;
the original V4 receipts are unchanged. PDF byte hashes differ, while predetermined
semantic criteria agree. `text.normalize` passed its six independent criteria
through the same registry, validator, runtime and store with a managed adapter.
Feasibility and reuse gates **PASSED**. Engineering advantage is **INCONCLUSIVE**.

## Delivered artifact identity

Directory: `I:\HYBRID-NET\.local\genesis2-final\separate delivery bundle`.

| File | Size bytes | SHA-256 |
| --- | ---: | --- |
| Hybrid.CLI.exe | 162,304 | 856d0ab0b980948f090d5fa30776e41ea395f712ba4e5b75a54825e0853bd2df |
| Hybrid.CLI.dll | 22,528 | a95c588c3a2cf4c1dde2c30bc0bfcddf8900221835eed91a45274aed876625f4 |
| Hybrid.Core.dll | 96,256 | d931f715e41de252f000cc0f7fcdc8e01031388fc7e3cde5318a5f3eed60d087 |
| verification/verify.py | See complete manifest | 23fdc192df57eef7b5ff29b1a04c260c0569ca12cba6bad606013209a557bc81 |

The .exe is a .NET apphost, so its hash alone does not identify application logic.
Use the complete manifest including both DLLs, Python scripts, criteria and runtime.
This is managed .NET packaging, not native C++ compilation.

## Reproduce and inspect

```powershell
& I:\HYBRID-NET\scripts\setup.ps1
& I:\HYBRID-NET\scripts\build.ps1
& I:\HYBRID-NET\scripts\test.ps1
& I:\HYBRID-NET\scripts\demo-genesis2.ps1
& I:\HYBRID-NET\scripts\validate-docs.ps1

$cli = 'I:\HYBRID-NET\.local\genesis2-final\separate delivery bundle\Hybrid.CLI.exe'
$workspace = 'I:\HYBRID-NET\.local\genesis2-final\framework state'
$python = 'I:\HYBRID-NET\.local\venv\Scripts\python.exe'
& $cli version
& $cli receipt trial-2-b --workspace $workspace --python $python
& $cli history --workspace $workspace --python $python
& I:\HYBRID-NET\scripts\verify-genesis2.ps1 `
  -Workspace $workspace -OperationId trial-2-b -Executable $cli
```

Use a fresh run directory for new demonstrations; the default creates one. Do not
reuse or remove the preserved final experiment directory. Setup/build restore may
need network access to obtain dependencies; installed local execution does not
depend on a server or cloud credentials. A physically disconnected test, Windows 11,
clean-machine portability and visual rendering fidelity remain NOT TESTED.

Final documentation validation passed, exit 0: **78 Markdown files, 390 local links,
nine JSON files, all 45 founding sections, 119 reviewed files and 22 implementation
text files**. Receipt: `.local/genesis2-docs-final.json`. Scoped `git diff --check`
passed. The existing selected accidental-secret checks found no matches; this is
not an exhaustive security certification. No credentials or unrelated files were
included in the scoped commit review.

Final integrity inspection matched all 17 source-manifest entries and all 206
copied bundle files (including its original build receipt); no mismatches.
Receipt: `.local/genesis2-final-integrity.json`. All raw machine-local logs, PDFs,
databases and bundles remain ignored; only source, tests, documentation and selected
aggregate evidence belong in the development branch. Publication is to
`feat/genesis-2-runtime`, never an automatic merge or release. The separate local
publication receipt records the actual outcome and remote commit after push.
