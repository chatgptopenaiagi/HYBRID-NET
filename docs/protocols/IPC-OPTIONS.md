# IPC and transport alternatives

**PROPOSED ARCHITECTURE.** HMCP describes operation meaning. A transport provides
framing, delivery and connection behavior. Genesis 1 selects no mandatory stack.
Each later transport profile needs maximum message size, encoding, backpressure,
authentication, cancellation, disconnect and compatibility tests.

| Option | Suitable experiment | Costs and questions |
|---|---|---|
| JSON-RPC over stdio | Two local parent/worker processes; readable contract messages | Define framing and output limits; separate diagnostics from protocol output; parent lifetime and inherited handles need tests. |
| Windows named pipes | Independently launched local Windows client and workers | Define pipe ACLs, peer identity, local-only access and framing; process identity is distinct from capability permission. |
| gRPC with Protocol Buffers | Typed multilanguage workers and later streaming remote jobs | Generated code/toolchains, schema evolution, deployment size and cancellation behavior need measurement. |
| Loopback HTTP with JSON | Optional local API and development inspection | Authentication, Origin policy, port ownership, request limits and listener lifecycle add obligations. |
| WebSockets | Live Studio updates or duplex coordination | Define event replay, heartbeat, flow control, reconnection and authorization; connection continuity is not job durability. |
| MCP gateway | Existing AI-agent clients | A mapping at the framework edge; it need not become the internal worker transport. |

JSON-RPC specifies request/response correlation and errors independently of a
transport; it does not define HYBRID.NET's durable jobs or idempotency semantics.
[JSON-RPC 2.0 specification](https://www.jsonrpc.org/specification).
gRPC supports unary and streaming methods and uses Protocol Buffers by default,
making typed contracts a concrete alternative to hand-maintained JSON bindings.
[gRPC core concepts](https://grpc.io/docs/what-is-grpc/core-concepts/).

Windows named pipes can connect local or remote processes and are subject to
security checks. A chosen local-only profile must explicitly prevent remote access;
the word “pipe” alone does not establish locality or trust.
[Microsoft named-pipe documentation](https://learn.microsoft.com/en-us/windows/win32/ipc/named-pipes).

The founding example `http://127.0.0.1:55001` with `POST /api/v1/execute` remains a
future interface proposal. Nothing listens there as part of this repository.
An HTTP profile must authenticate protected operations even on loopback; network
exposure is opt-in and a separate design decision.

The first experiment should compare one simple local profile against requirements,
not install all candidates. Record launch complexity, bounded-message behavior,
failure recovery, language interoperability and measured overhead before accepting
an ADR. Do not infer exactly-once side effects from any transport's successful send.
