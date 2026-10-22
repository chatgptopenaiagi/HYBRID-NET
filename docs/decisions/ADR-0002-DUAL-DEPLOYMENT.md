# ADR-0002: Standalone client and optional server

Status: **ACCEPTED**. Date: 2026-10-22. Classification: **ENGINEERING DECISION**.

## Context

Distributed scheduling adds useful capabilities but can make an otherwise local
operation depend on network availability, enrollment or a central database.
The founder requires local operation to survive server loss.

## Decision

HYBRID.NET Client owns local discovery, policy, execution, verification,
configuration and persistence. HYBRID.NET Server is a separate optional product
for coordinating authorized clients. Hybrid operation chooses local or remote
capabilities under explicit policy. Server identity conveys no unrestricted
administrative authority over a client.

Local deterministic work must function offline without a model or central server.
Remote-provider operations naturally retain their own connectivity requirements.
After interrupted remote work, an uncertain outcome must be reconciled by execution
identity and evidence; a non-idempotent action cannot be blindly replayed.

## Consequences and alternatives

The client needs its own durable records and explicit synchronization rules.
Server-only orchestration and mandatory cloud licensing checks are rejected as
foundational execution dependencies. Exact transport, enrollment and deployment
mechanisms remain proposed, not selected by this ADR.

## Validation

Later tests must disconnect, stop and restart the server while a local capability
continues to work. Reconnection tests must distinguish never-started, completed and
unknown-outcome remote operations without duplicating side effects.
