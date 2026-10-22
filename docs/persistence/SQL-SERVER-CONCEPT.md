# Optional SQL Server persistence

**ENGINEERING DECISION — Microsoft SQL Server is an optional centralized server
persistence direction.** No server instance, database, connector or migration is
installed or implemented in Genesis 1. HYBRID.NET Client must not require this
provider for standalone operation.

The proposed server store aggregates authorized client registrations, advertised
capabilities, distributed jobs, execution events and verification receipts. Local
clients retain authority over their accepted jobs and observations; a stale central
copy cannot establish that a remote side effect did or did not happen. Privacy and
retention policy determine which evidence is uploaded. The server must not collect
client credentials or entire local inventories merely because synchronization exists.

SQL Server provides transactions, locking and row-versioning mechanisms. The
application remains responsible for choosing transaction boundaries that preserve
its business invariants. Provider-specific concurrency behavior must be tested
rather than assumed equivalent to SQLite.
[Microsoft transaction and locking guide](https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-transaction-locking-and-row-versioning-guide?view=sql-server-ver17).

Use a persistence interface defined around semantic operations and outcomes, not
an abstraction that promises identical SQL dialects. Atomic job admission, unique
idempotency reservations, event append, optimistic version checks and outbox claims
must have equivalent observable behavior across supported providers. Distributed
scheduling needs leases or fencing that cannot dispatch the same non-idempotent job
twice after ownership changes. A transaction in the central database cannot make
arbitrary remote application effects atomic.

The runtime database identity should have only required schema privileges. Secrets
reside in a credential provider; SQL contains references. Secure transport, deployment
identity, connection pooling, backup/restore and recovery objectives require a later
deployment-specific design. SQL Server edition/version selection is deferred until
load and operational requirements are measured.

Provider acceptance must test duplicate event ingestion, concurrent schedulers,
lease expiry, database outage, reconnect and uncertain dispatch outcomes. An outage
may suspend new distributed admissions; it must not disable client-local capability
discovery and execution. [DATA-MODEL.md](DATA-MODEL.md) describes the shared logical
entities without prescribing physical DDL.
