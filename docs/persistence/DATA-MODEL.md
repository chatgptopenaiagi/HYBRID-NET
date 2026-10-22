# Conceptual SQL data model

**PROPOSED ARCHITECTURE.** These are logical entities and invariants, not deployed
tables, executable DDL or a stable migration contract. SQLite and optional SQL
Server providers should implement the same job semantics with appropriately
different physical schemas and concurrency controls.

| Entity | Identity and intended responsibility |
|---|---|
| Applications | `application_id`; product identity, observed version, installation observation and discovery time. Installation does not prove capability availability. |
| Capabilities | `capability_id` plus contract version; operation meaning, schema references, semantic tags, required permissions and integration level. |
| Adapters | `adapter_id` plus version; registered implementation identity, provenance, application binding, supported capabilities and probe evidence. |
| Clients | `client_id`; enrollment metadata, authorized server relationships, public identity references and last observed health; no private keys. |
| Agents | `agent_id`; authenticated principal association, client relationship and policy references; no assumption that a model is itself a trusted identity. |
| ExecutionJobs | `job_id`; originating principal/client, pinned capability/adapter versions, input fingerprint, idempotency scope, effect class, deadline and current lifecycle state. |
| ExecutionEvents | `event_id`; job ID, producer ID, sequence, timestamp, event kind and bounded redacted payload; append-only engineering history. |
| VerificationResults | `verification_id`; job ID, fixed criterion version/hash, verifier identity, outcome, evidence references and output fingerprints. |
| ApplicationStates | application/client identity plus `application_instance_id` and `state_version`; observation time, source adapter, structured snapshot and completeness/freshness indicators. A restarted application gets a new instance identity. |
| RecoveryCheckpoints | `checkpoint_id`; job ID, adapter-supported resume information, evidence and known effect state; no claim of universal application rollback. |
| Dependencies | `dependency_id`; capability/adapter requirement, version constraint, observed availability, probe time and provenance. |

Applications can have several adapters; each adapter advertises versioned
capabilities. A job pins one concrete provider binding so registry updates cannot
silently change its meaning. Jobs have many events and verification results. A
verification receipt may be superseded by a later check but must not erase the
historical failure or input/output hashes that explain it.

## Consistency and recovery rules

Admission reserves a unique `(principal, capability_version, resource_scope,
idempotency_key)` together with the job and its initial event. Repeated identical
admission returns that job; different inputs with the same key are rejected.
Retention must preserve reservations for the documented replay window. After that
window, clients must not assume an old key still prevents duplication.

Application-state preconditions compare both instance identity and expected revision;
a matching revision from a different process lifetime is stale. Job state transitions
use their own expected versions. Event IDs and producer sequence numbers
allow duplicate ingestion to be detected; wall-clock timestamps alone do not order
concurrent distributed events. Current job state is an indexed projection updated
transactionally with its event, not a competing independent authority. An outbox
record committed with a transition supports later synchronization; an inbox tracks
accepted remote event IDs. These implementation support records complement the
eleven logical entities rather than add an external message system requirement.

No database transaction encloses an arbitrary application's external side effect.
Persist dispatch intent, dispatch once under an appropriate job ownership policy,
then persist known outcome. A crash between the effect and its receipt can require
`unknown_outcome`, observation or explicit recovery. Automatic retries are allowed
only when the capability contract and durable evidence justify them.

## Evidence, privacy and schema evolution

Artifacts use content hashes, sizes, media types and access-controlled references.
A matching hash establishes byte identity, not semantic correctness. Verification
criteria and actual observed results remain separate from adapter exit status.
Sensitive application state needs purpose limits and retention controls; ordinary
rows and event payloads must contain no credentials. Evidence deletion must leave
an explicit availability marker so an old `passed` label cannot imply its proof
remains retrievable.

Migration versioning, backups, restore tests, authoritative clock handling, canonical
input serialization and central/local reconciliation need later experiments. A
shared repository interface should expose semantic transactions and explicit
conflicts; tests must prove invariants on each selected provider. There is no
database implementation or data migration to run in Genesis 1.
