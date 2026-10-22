# Persistence ownership boundary

Status: **PROPOSED ARCHITECTURE; no database provider or migration implemented**.

The future client will use embedded SQLite for standalone records. Distributed
server deployments may use Microsoft SQL Server. This directory will own logical
storage contracts, provider mappings and migrations when a milestone needs them;
Genesis 1 requires no database installation.

The [logical data model](../docs/persistence/DATA-MODEL.md) preserves eleven entity
groups, their identities and evidence relationships. Transaction boundaries must
cover job intent, event sequencing, deduplication and recovery; a generic CRUD
interface alone does not establish those semantics. Provider-specific constraints
must be tested rather than assumed equivalent. Keep credential material out of
ordinary rows and use opaque references to protected storage. Follow the
[SQLite](../docs/persistence/SQLITE-CONCEPT.md) and
[SQL Server](../docs/persistence/SQL-SERVER-CONCEPT.md) design notes.
