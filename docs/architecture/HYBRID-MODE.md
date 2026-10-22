# Hybrid operation

**Status: PROPOSED ARCHITECTURE.** Hybrid mode combines independently usable local
execution with explicitly authorized remote capabilities.

A plan may create a document locally, send an approved intermediate artifact to
a remote rendering worker, receive its output and verify that output locally.
This is a scenario for future testing, not an implemented workflow. Remote access
must remain visible in the plan, including what data leaves the machine and which
identity is permitted to process it.

Placement should evaluate available local capabilities first, then compare
authorized remote alternatives against required functions, evidence freshness,
resource needs, transfer cost and privacy constraints. Local preference does not
mean silently substituting a weaker capability. If no eligible target satisfies
the verification contract, return a structured unavailable result.

| Condition | Required proposed behavior |
| --- | --- |
| Server unavailable before dispatch | Continue local work; fail or queue remote work under explicit policy |
| Disconnect after dispatch | Record uncertain outcome; reconcile execution identity |
| Reconnect after completion | Import missing receipts once; verify artifact identity |
| Repeated non-idempotent request | Return known outcome or require reconciliation; no automatic replay |
| Remote artifact changed in transit | Reject integrity mismatch and retain failure evidence |

Local and remote records need stable job, attempt, client and artifact identifiers.
Synchronization should merge events by identity, not infer completion from wall
clock order alone. Client clocks and network delivery order may differ. The
authoritative owner for each execution and its reconciliation rules must be
specified before distributed implementation.

Future acceptance requires a workflow crossing two machines, deliberate connection
loss and proof that a side effect is not duplicated. Until that evidence exists,
distributed execution and recovery remain conceptual. Local operations must never
wait indefinitely for history synchronization or server health checks.

See [standalone client](STANDALONE-CLIENT.md) and [distributed server](DISTRIBUTED-SERVER.md).
