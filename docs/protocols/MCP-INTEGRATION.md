# MCP integration boundary

**PROPOSED ARCHITECTURE.** An optional MCP gateway could expose selected HMCP
capabilities to Codex and other compatible agents. Neither an MCP server nor a
Codex integration is implemented in Genesis 1. Local deterministic execution must
remain available without MCP, a model provider or an internet connection.

The reviewed MCP specification uses JSON-RPC and distinguishes tools, resources
and prompts. It also describes capability negotiation, progress, cancellation and
errors. These features make MCP a candidate agent-facing mapping; they do not
establish HYBRID.NET's authorization or verification guarantees automatically.
[MCP specification, revision 2025-11-25](https://modelcontextprotocol.io/specification/2025-11-25).

| Potential MCP surface | Proposed HMCP mapping |
|---|---|
| Tool discovery | Authorized capability manifests, availability and version metadata |
| Tool invocation | Validate inputs, authorize resource scope, admit a durable job |
| Tool result | Structured execution outcome plus separate verification status and receipt reference |
| Resource | Authorized state snapshots or bounded evidence, with freshness information |
| Progress/cancellation | Translate identifiers while preserving job identity and cancellation uncertainty |

The gateway must not flatten an `unknown_outcome` into an ordinary retryable error,
or label a successful tool response as independently verified output. Adapter
descriptions and application content remain untrusted data. They cannot grant
permissions, choose arbitrary executable paths or request disclosure of secrets.
Gateway policy must bind an authenticated principal to allowed capabilities and
resource scopes, including visibility of receipts.

For the reviewed MCP revision, standard transport choices include stdio and
Streamable HTTP. The HTTP guidance calls for Origin validation, localhost binding
when local, and authentication. The eventual gateway must select a specification
revision and test its actual client compatibility; a generic WebSocket or named-pipe
HMCP transport is not automatically an MCP transport.
[MCP transport specification](https://modelcontextprotocol.io/specification/2025-11-25/basic/transports).

Genesis 2 should establish a transport-independent HMCP lifecycle first. A later
gateway experiment should prove discovery, invocation, denied scope, cancellation,
lost response and separate verification results using a synthetic capability.
Provider sampling remains optional and needs its own authorization; invoking a
known deterministic capability must not secretly trigger model inference.
