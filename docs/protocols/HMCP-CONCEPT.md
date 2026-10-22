# Hybrid Machine Capability Protocol

**PROPOSED ARCHITECTURE — HMCP 0.1 draft.** Genesis 1 preserves semantics and
illustrative messages; it implements no API, validator, wire protocol or listener.
HMCP names the common contract across agents, adapters and all six language domains.
It does not require every call to pass through Go or an AI model.

## Discovery and registration

An adapter registration should identify its publisher, adapter version, application,
capability ID and version, integration level L0–L5, input/output schemas, side
effects, required permissions, cancellation support and verification procedure.
The registry validates registration provenance and authorization before advertising
it. A manifest is a declaration, not proof that the application is installed or
that the operation works. Availability is separately `declared`, `unavailable`,
`available_unverified` or `verified`, accompanied by probe time, environment and
evidence references. Discovery must return stale or unavailable evidence explicitly.

Semantic tags such as `document`, `pdf` and `export` assist discovery; an agent's
inference cannot upgrade a declaration to a verified capability. Registry entries
must not expose paths or capabilities beyond the querying principal's scope.

## Proposed message semantics

| Element | Required meaning for a future implementation |
|---|---|
| `protocol_version` | Negotiated HMCP contract version; reject unsupported major semantics explicitly. |
| `request_id` | Correlates one exchange; it is neither authorization nor a replay guarantee. |
| `job_id` | Durable execution identity assigned once after authorized admission; reused for status and recovery. |
| Capability/adapter versions | Pin the operation meaning and implementation identity in the receipt. |
| Inputs and outputs | Validate types, bounds, resource references and allowed paths; never accept arbitrary shell text as a generic capability. |
| Authorization context | Derived from authenticated transport identity and local policy; a caller-supplied principal is not trusted. |
| `idempotency_key` | Scoped to principal, capability version and resource domain; pair with a canonical input fingerprint and reject key reuse with different inputs. |
| `expected_application_instance_id`, `expected_state_version` | Paired optimistic precondition; bind the revision to the observed application instance and reject either mismatch before side effects where the adapter supports this guarantee. Both may be absent for a declared stateless operation; unsupported guarantees must be explicit. |
| Deadline and limits | Bound execution, output size and retained events; report which limits are enforceable. |

Canonical input encoding, exact version negotiation and schema language remain
decisions for Genesis 2. [Conceptual JSON examples](../../protocols/README.md) are
review material, not conforming implementations of a finalized standard.

## Lifecycle, observation and recovery

Proposed states are `accepted`, `running`, `cancel_requested`, `succeeded`, `failed`,
`cancelled` and `unknown_outcome`. Progress events carry job ID, monotonically
increasing sequence, timestamp and a phase; percentages are optional estimates.
State snapshots carry application instance identity, observation time, version,
source and completeness. A restart creates a new instance identity; reusing a
revision number must not make a pre-restart observation current. Absence
of an event does not establish absence of a side effect.

Cancellation is a request with its own acknowledgment. An adapter may finish before
it can cancel; a canceled process may already have modified external state. Report
the final known effect and any compensation separately. Transport loss or a deadline
after dispatch can produce `unknown_outcome`; reconcile using the durable job,
adapter observation and evidence before any replay. Non-idempotent operations must
not be automatically resubmitted merely because their response was lost.

Errors need a stable code, stage, bounded safe message, evidence reference and
effect status (`none`, `partial`, `complete`, `unknown`). A retry hint never overrides
authorization, idempotency or unknown-outcome policy.

Execution `succeeded` means the adapter completed its declared operation. Verification
is separately `not_requested`, `pending`, `passed`, `failed` or `inconclusive`, tied
to fixed criteria, verifier identity, actual inputs/outputs and artifact hashes.
A zero exit code or an adapter's assertion alone cannot prove the requested result.
The client can verify returned remote artifacts without trusting a remote success
label. [Persistence](../persistence/DATA-MODEL.md) defines intended durable records.
