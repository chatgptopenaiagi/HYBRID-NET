# Codex and other agent consumers

Status: **PROPOSED ARCHITECTURE**. No Codex connector, model provider or agent
session implementation ships in Genesis 1. Repository creation and documentation
validation do not require an OpenAI API key.

An agent consumer should translate a human objective into a proposed sequence of
registered capability invocations. HYBRID.NET retains authority over validation,
scope, dispatch and outcome verification. Agent identity, model-provider identity
and Windows process identity remain separate. A model-generated argument is
untrusted input even when the agent itself is authorized.

The proposed integration surface is an optional MCP mapping or a future scoped
client API. MCP tools provide discovery metadata, input schemas and invocation
results; that makes them a candidate presentation layer for selected HMCP
capabilities. This inference does not establish compatibility with any particular
Codex version. See the [MCP tools specification, pinned research edition
2025-11-25](https://modelcontextprotocol.io/specification/2025-11-25/server/tools)
and [mapping design](../protocols/MCP-INTEGRATION.md).

The mapping must preserve the HMCP capability version, execution identifier,
authorization decision and verification result. A returned tool message cannot
upgrade an unverified result to success. Large artifacts should be referenced
through authorized, hashed descriptors rather than inserted into model context.

A later acceptance test should use a synthetic capability: discover it, invoke
it with valid input, reject invalid or unauthorized input, and retrieve the same
execution receipt by identifier. Deterministic runtime tests must also work with
no model connected. No existing Codex configuration, authentication or executable
is imported or changed by this integration proposal.
