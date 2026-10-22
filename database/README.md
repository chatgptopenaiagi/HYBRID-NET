# Persistence ownership boundary

Status: **IMPLEMENTED COMPONENT — embedded SQLite evidence; server SQL remains planned**.

[EvidenceStore.cs](../core/Hybrid.Core/EvidenceStore.cs) implements the Genesis 2
store at `<workspace>/engine/evidence.sqlite`. Tables are `operations`,
`execution_events`, `verification_results`, `registry` and `reproducibility_runs`.
Microsoft.Data.Sqlite 10.0.13 provides the managed binding. Each connection uses
parameterized SQL, foreign keys, bounded busy waits, WAL and FULL synchronization.

Operation admission is atomic and duplicate requests do not replay. Original
receipts, event rows and verification records remain immutable; later observations
are additional records. Terminal evidence and its event are committed atomically.
Recovery checks PID plus process-start identity and records abandoned work as
INCONCLUSIVE without killing processes or repeating uncertain side effects.
Independent [Python readback](../verification/store_readback.py) checks committed
provenance before V4 promotion. Credentials are not database evidence fields.

[Store tests](../tests/store/Program.cs) cover restart retrieval, duplicate races,
immutability, rollback and killed owned-writer transactions. Exact counts and
limitations belong to the [test report](../docs/genesis/GENESIS-2-TEST-REPORT.md).
The wider [logical model](../docs/persistence/DATA-MODEL.md) and
[SQLite](../docs/persistence/SQLITE-CONCEPT.md) design preserve Genesis 1 direction;
their full entity list is not a claim that every table exists. Optional
[SQL Server](../docs/persistence/SQL-SERVER-CONCEPT.md) remains future work. No
database server installation or distributed synchronization is required.
