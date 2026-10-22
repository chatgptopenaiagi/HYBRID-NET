# Protocol artifact boundary

Status: **IMPLEMENTED LOCAL HMCP 0.1 CONTRACT; preserved Genesis 1 examples**.

The [implemented contract](../docs/protocols/HMCP-0.1.md) and
[structural schema](hmcp-0.1.schema.json) describe the Genesis 2 JSON request,
capability, status, result, criterion, evidence and error shapes. Actual request
validation and additional filesystem policy are implemented in
[Contracts.cs](../core/Hybrid.Core/Contracts.cs). Product version is 0.1.0; protocol
version is 0.1. The schema is not a general-purpose runtime validator or generated SDK.

The local CLI binds HMCP semantics to real execution. No networking transport,
MCP server, remote worker SDK or universal ABI is implemented. The broader
[HMCP concept](../docs/protocols/HMCP-CONCEPT.md),
[MCP mapping](../docs/protocols/MCP-INTEGRATION.md) and
[IPC alternatives](../docs/protocols/IPC-OPTIONS.md) remain architectural direction.

The JSON files under examples preserve Genesis 1's possible manifest, invocation
and execution receipt, labeled conceptual with version 0.1-draft. They are not
accepted CLI requests or live receipts and were not silently reinterpreted:

- [Capability manifest](examples/capability-manifest.json)
- [Invocation](examples/invocation.json)
- [Execution receipt](examples/execution-receipt.json)

Use the current contract for required fields, canonical request identity, duplicate
handling, bounds, cancellation, evidence levels and immutable historical receipts.
Future worker transports must preserve these semantics and pass shared conformance
cases. MCP and HMCP remain distinct even if a future binding maps between them;
do not introduce every candidate transport as a mandatory dependency.
