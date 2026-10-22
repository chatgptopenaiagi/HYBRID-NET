# Client ownership boundary

Status: **IMPLEMENTED COMPONENT — local Genesis 2 CLI; broader client remains planned**.

[Hybrid.CLI](Hybrid.CLI/Hybrid.CLI.csproj) builds `Hybrid.CLI.exe`.
[Program.cs](Hybrid.CLI/Program.cs) exposes JSON discovery, capability inspection,
execution, receipt/history retrieval, re-verification and reproduction commands
over [the shared core](../core/README.md). It supplies a machine interface without
requiring a graphical application, server, model, credentials or network endpoint.

Build with [build.ps1](../scripts/build.ps1). The default Release `win-x64` directory
includes the .NET runtime and verification scripts; keep the complete directory.
Python with pinned packages and LibreOffice remain external dependencies. Use
explicit `--workspace` and `--python` paths. The
[operator guide](../docs/genesis/GENESIS-2-IMPLEMENTATION.md) documents exact commands,
exit codes, workspace policy and the default state location beside the executable.

`Hybrid.Client.exe`, Studio, credential providers and
`HYBRID-NET-CLIENT-SETUP.exe` remain future products. No background listener or service
is installed. Broader authentication, remote enrollment and shared human/AI state
require later scoped implementation. Preserve the
[standalone design](../docs/architecture/STANDALONE-CLIENT.md) and
[identity separation](../docs/security/API-KEY-ARCHITECTURE.md). See
[packaged test evidence](../docs/genesis/GENESIS-2-TEST-REPORT.md); no clean-machine
or Windows 11 certification follows from execution on this workstation.
