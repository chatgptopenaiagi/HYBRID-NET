# Existing technologies and research provenance

Status: **PROPOSED ARCHITECTURE / research**. Sources were consulted on
2026-10-22. These are external documented mechanisms, not HYBRID.NET components
or local test results. Versioned links deliberately identify research editions;
implementation must check the version actually selected. No claim of exhaustive
prior-art research, legal novelty or interoperability certification is made.

## Contracts and communication

| Technology | Documented role | HYBRID.NET question |
|---|---|---|
| JSON-RPC 2.0 | Defines JSON request, response and error structures independently of transport. [Specification](https://www.jsonrpc.org/specification) | Can a small mapping carry HMCP identity, discovery and verification without excessive extensions? Framing, authorization and durable job semantics still need a design. |
| Protocol Buffers and gRPC | gRPC supports an interface definition and message interchange based on Protocol Buffers, with generated clients/servers. [gRPC introduction](https://grpc.io/docs/what-is-grpc/introduction/) | Do generated bindings and streaming justify toolchain/runtime overhead for six languages? Benchmark and test schema evolution before selecting them. |
| MCP | Its tools contract includes discovery, schemas and invocation results. [Tools, 2025-11-25 edition](https://modelcontextprotocol.io/specification/2025-11-25/server/tools) | Map selected capabilities to agent tools while retaining HMCP policy and evidence. Do not make MCP an implicit authorization authority or require model inference for execution. |
| Windows named pipes | Windows provides named communication channels, with access governed by security checks; pipes can also be accessible remotely. [Microsoft documentation](https://learn.microsoft.com/en-us/windows/win32/ipc/named-pipes) | A local-only design needs explicit remote rejection and access policy, not just a pipe name. Compare lifecycle, ACLs and cross-language implementation cost with child-process standard streams. |

Standard streams are a candidate two-process bootstrap channel: parent-owned
launch and a bounded framing protocol reduce discovery configuration. This is
an engineering hypothesis. It does not provide durable jobs, authentication of an
arbitrary executable or an OS sandbox by itself. WebSockets are an alternative
for future Studio progress streams; no requirement currently justifies adding
them to the initial worker path.

## Persistence and Windows facilities

[SQLite's serverless model](https://www.sqlite.org/serverless.html) embeds database
access without an intermediary service, which supports the intended standalone
client direction. [SQL Server databases](https://learn.microsoft.com/en-us/sql/relational-databases/databases/databases?view=sql-server-ver17)
provide structured tables and database security concepts suitable for evaluation
as an optional centralized provider. HYBRID.NET still needs transaction, migration,
concurrency and recovery contracts for both; matching entity names does not make
their SQL dialects or deployment properties equivalent.

[DPAPI](https://learn.microsoft.com/en-us/windows/win32/api/dpapi/nf-dpapi-cryptprotectdata)
and [Credential Management](https://learn.microsoft.com/en-us/windows/win32/secauthn/credentials-management)
are candidate secret-protection facilities. Scope choices and Windows identity
matter. Encryption at rest does not prove that an untrusted process under the
same account cannot obtain authorized access. Token scopes and application policy
must be enforced independently.

## Application interoperability

UNO, Blender scripting, Adobe extension interfaces, Joomla web services,
PowerShell and the Windows Service Control Manager show that direct interfaces
already exist for particular software domains. The
[reference-target catalog](../../adapters/REFERENCE-TARGETS.md) links the primary
documentation and records limitations per target. No universal executable
introspection mechanism is established by these examples.

HYBRID.NET's research objective is to compose useful contracts, policy and
verification around supported interfaces. Reuse existing mechanisms when they
fit. Any proposed HMCP field should be justified by a concrete requirement, with
an explicit explanation of what an existing protocol does or does not preserve.
