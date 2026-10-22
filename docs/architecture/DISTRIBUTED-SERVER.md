# Distributed server

**Status: PROPOSED ARCHITECTURE.** `Hybrid.Server.exe` and
`HYBRID-NET-SERVER-SETUP.exe` are proposed deliverables, not Genesis 1 artifacts.

The optional server coordinates registered clients, distributed capability
discovery, job placement, agent sessions, health information and verification
collection. Microsoft SQL Server is the proposed optional centralized persistence
engine. The server adds distributed coordination to a usable local client.

Enrollment must establish client identity separately from the credentials an
agent uses to request work. A connected client advertises approved capabilities
and receives only operations permitted by its local policy. Neither registration
nor a valid connection confers unrestricted process launch, administrative access,
file access or permission to export client data.

Capability advertisements include adapter/application versions, integration level,
verification timestamps and evidence scope. Remote availability is a changing
claim: scheduling must account for freshness, connectivity, dependencies and the
client's current authorization. A final client-side preflight remains necessary.

Proposed execution records use the [HMCP lifecycle](../protocols/HMCP-CONCEPT.md):
`accepted`, `running`, `cancel_requested`, `succeeded`, `failed`, `cancelled` and
`unknown_outcome`, separately from verification results. A scheduler's `queued`
placement is scheduling metadata, not a second incompatible execution-state enum.
Cancellation is a
request until confirmed; it cannot promise reversal of an external side effect.
After a disconnect, reconcile the durable execution identifier with the client.
Do not reissue a non-idempotent operation solely because an acknowledgement was
lost. Idempotency keys require defined scope, retention and duplicate handling.

Artifact transfer needs explicit authorization, size limits, integrity hashes and
provenance. Hash agreement confirms transferred bytes, not semantic correctness.
Independent verification may run at the originating client as well as the worker.

Future tests should cover server restart, revoked clients, stale advertisements,
lost completion messages and duplicate delivery. They must demonstrate continued
local operation throughout server failure. Consensus, high availability and
cross-machine transactions remain unresolved later design work.

See [hybrid mode](HYBRID-MODE.md) and [client identity](../security/CLIENT-IDENTITY.md).
