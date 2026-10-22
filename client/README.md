# Client ownership boundary

Status: **PROPOSED ARCHITECTURE; no client executable or installer**.

The future client owns local authority, registry, credentials, worker orchestration,
verification, SQLite records and optional machine/human gateways. Server enrollment
and remote AI configuration are optional extensions, not startup requirements.
`Hybrid.Client.exe` and `HYBRID-NET-CLIENT-SETUP.exe` are proposed product names.

Later acceptance must demonstrate deterministic local operations with network
disconnected and server absent, authenticated protected capabilities, durable
restart behavior and conflicting human/agent state changes. Studio may share this
client's state but must not bypass its policy. No background listener or service is
installed by this directory. Read [standalone design](../docs/architecture/STANDALONE-CLIENT.md)
and [identity separation](../docs/security/API-KEY-ARCHITECTURE.md) before implementation.
