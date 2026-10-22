# Protocol artifact boundary

Status: **PROPOSED ARCHITECTURE / conceptual examples only**.

The proposed HMCP contract describes semantics shared by workers, adapters and
coordinators. No stable wire protocol, schema compiler, generated client or transport
binding has shipped. Protocol version 0.1-draft is an illustration, separate from
the project version 0.0.1. See [HMCP](../docs/protocols/HMCP-CONCEPT.md),
[MCP mapping](../docs/protocols/MCP-INTEGRATION.md) and
[IPC alternatives](../docs/protocols/IPC-OPTIONS.md).

The JSON files under examples preserve a possible manifest, invocation and execution
receipt. Valid JSON syntax is not an implemented endpoint or proof of interoperability:

- [Capability manifest](examples/capability-manifest.json)
- [Invocation](examples/invocation.json)
- [Execution receipt](examples/execution-receipt.json)

Genesis 2 must choose an initial schema/profile and define framing, limits,
compatibility, authorization and failure semantics before these become executable
contracts. MCP and HMCP identities must remain distinct even if messages share a
transport. Do not add every transport as a mandatory dependency.
