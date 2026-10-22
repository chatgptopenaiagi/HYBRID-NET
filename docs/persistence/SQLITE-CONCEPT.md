# SQLite for standalone clients

**ENGINEERING DECISION — local SQL persistence uses SQLite as the initial direction.**
Schema, access library, durability settings and migrations remain **PROPOSED
ARCHITECTURE**. Genesis 1 creates no database and installs no database software.

SQLite is designed for local storage in applications and devices, while a
client/server database targets shared enterprise data. This supports an embedded
client store that continues operating without HYBRID.NET Server.
[SQLite's appropriate-use guidance](https://www.sqlite.org/whentouse.html).
The proposed store holds local registry data, authorized job history, verification
metadata, state observations, checkpoints and an optional synchronization outbox.
Large artifacts remain in managed files referenced by hashes and access-controlled
paths; raw provider credentials and bearer tokens never belong in SQL.

The runtime should own database access behind semantic operations such as
`admit_job`, `append_event`, `record_verification` and `claim_outbox_batch`. Workers
submit results through contracts rather than performing unrelated schema writes.
This boundary can preserve invariants across languages and prevent a distributed
server from becoming a hidden requirement for local reads or execution.

SQLite permits concurrent readers but only one simultaneous write transaction.
Keep transactions short, define bounded busy handling and test interruption at
commit boundaries. Journal mode and connection ownership must be measured against
actual workloads rather than selected by reputation.
[SQLite transaction documentation](https://www.sqlite.org/lang_transaction.html).

Job admission should atomically reserve its idempotency key, create the job and
append the first event. A completed local transition and its outgoing synchronization
record should also commit together. Application side effects outside SQLite cannot
be rolled back by its transaction; a crash after dispatch may leave an unknown
outcome requiring reconciliation.

Later acceptance must cover migrations with backups, restart recovery, duplicate
admission, conflicting key reuse, corrupted or unavailable stores, disk-full behavior
and evidence retention. Remote synchronization may copy permitted records after
reconnection but must never block independently authorized local jobs. See the
[logical data model](DATA-MODEL.md) for proposed entities and shared semantics.
