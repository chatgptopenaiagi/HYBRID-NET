# Core ownership boundary

Status: **IMPLEMENTED COMPONENT — Genesis 2, version 0.1.0**.

[Hybrid.Core](Hybrid.Core/Hybrid.Core.csproj) is the initial C#/.NET 10 library.
[Contracts.cs](Hybrid.Core/Contracts.cs) validates local HMCP requests, operation
identifiers, workspace paths and legal state transitions.
[Adapters.cs](Hybrid.Core/Adapters.cs) implements the explicit HACE registry and
the LibreOffice/text capability bindings. [Runtime.cs](Hybrid.Core/Runtime.cs)
coordinates AIXR execution, independent verification and evidence promotion.

[ProcessRunner.cs](Hybrid.Core/ProcessRunner.cs) uses argument arrays, bounded
capture, deadlines, cancellation and owned-process cleanup.
[EvidenceStore.cs](Hybrid.Core/EvidenceStore.cs) persists transactional SQLite
records, immutable original receipts and later observations. Execution completion
and verification remain separate; an adapter cannot certify its own result.
The Python observers live in [verification](../verification/verify.py).

The local library requires no server, network listener, inference or six-worker
installation. Go, C++, Java, JavaScript/TypeScript, Python and C#/.NET remain the
founding ecosystem; remote transport and other language workers are future work.
Do not duplicate the runtime in ownership directories. Read the
[implementation guide](../docs/genesis/GENESIS-2-IMPLEMENTATION.md),
[implemented HMCP contract](../docs/protocols/HMCP-0.1.md) and
[acceptance report](../docs/genesis/GENESIS-2-ACCEPTANCE.md) for the actual boundary.
The [AIXR concept](../docs/vision/AIXR-CONCEPT.md) preserves the wider Genesis 1 vision.
