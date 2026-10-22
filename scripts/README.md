# Repository engineering scripts

Status: **IMPLEMENTED COMPONENT — PowerShell 7 engineering entry points**.

All scripts belong to this repository. Machine-specific environments, caches,
builds, fixtures, logs and evidence go under ignored `.local` paths. The
[implementation guide](../docs/genesis/GENESIS-2-IMPLEMENTATION.md) documents actual
dependencies: selected .NET SDK for building, Python with pinned verifier packages,
and installed LibreOffice for real conversion. Dependency setup/build may require
network access; deterministic local execution requires no server or cloud API.

| Entry point | Actual work and output |
| --- | --- |
| [setup.ps1](setup.ps1) | Create `.local/venv` if absent, check a supplied Python when creating it, install pinned packages and restore the CLI |
| [environment.ps1](environment.ps1) | Dot-source process-local tool paths, caches and temporary-directory configuration; no runtime verification claim |
| [build.ps1](build.ps1) | Publish the complete Release win-x64 bundle; write build command, tool version, hashes and file sizes |
| [test.ps1](test.ps1) | Run C# contracts/store/process and Python suites sequentially; preserve actual report counts, exit codes and logs |
| [demo-genesis2.ps1](demo-genesis2.ps1) | Run genuine direct/framework comparison and controlled faults through the acceptance harness; use `-Phase benchmark`, `failures` or default `all` |
| [verify-genesis2.ps1](verify-genesis2.ps1) | Invoke packaged `verify` for an existing operation, append fresh independent evidence and preserve the original receipt |
| [validate-docs.ps1](validate-docs.ps1) | Check Git root, founding coverage, local links, fences, JSON and narrow accidental-secret patterns; not an application test |

Run from PowerShell 7 with exact script paths:

```powershell
& I:\HYBRID-NET\scripts\setup.ps1 -Python C:\Python314\python.exe
& I:\HYBRID-NET\scripts\build.ps1
& I:\HYBRID-NET\scripts\test.ps1
& I:\HYBRID-NET\scripts\demo-genesis2.ps1
& I:\HYBRID-NET\scripts\validate-docs.ps1
& I:\HYBRID-NET\scripts\validate-docs.ps1 -ReportPath I:\HYBRID-NET\.local\docs-validation.json
```

The Python path is the observed workstation interpreter; use `-Python` for another
compatible installed 64-bit interpreter. `build.ps1 -FrameworkDependent` explicitly
selects the alternate .NET-dependent bundle; the default includes .NET. Both retain
external Python and LibreOffice dependencies. Copy the complete delivery directory.

The demo chooses a new timestamped `.local/experiments` directory unless
`-RunDirectory` supplies one. Preserve prior evidence rather than reusing completed
operation identities. `-LibreOffice` accepts the actual supported `soffice.com`
path. The harness records a delivery copy, controlled fixture, direct baseline,
framework trials, reproduction and fault observations. See the
[test report](../docs/genesis/GENESIS-2-TEST-REPORT.md) for fair comparison conditions
and the [acceptance matrix](../docs/genesis/GENESIS-2-ACCEPTANCE.md) for gate outcomes.

For the preserved first real operation, append another verification with:

```powershell
& I:\HYBRID-NET\scripts\verify-genesis2.ps1 `
    -Workspace I:\HYBRID-NET\.local\first-real -OperationId first-real
```

For new runs, use the workspace and operation ID returned by their receipts.
Verification forwards the CLI exit code: 0 VERIFIED, 2 FAILED, 3 INCONCLUSIVE,
4 request/infrastructure error, or 130 CANCELLED. Build/setup return the relevant
tool failure code; automated test/demo failures return nonzero. A documentation
exit 0 establishes only its declared integrity checks. Test summaries explicitly
separate automated regressions from real application and packaged acceptance.
