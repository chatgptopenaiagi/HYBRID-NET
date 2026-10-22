# Server ownership boundary

Status: **PROPOSED ARCHITECTURE; no server process or installer**.

The optional server will enroll clients, expose authorized capability projections,
coordinate distributed jobs and collect evidence. It may use Microsoft SQL Server
behind a storage contract. It does not own unrestricted client administration or
replace local SQLite records and local execution policy. Proposed names are
`Hybrid.Server.exe` and `HYBRID-NET-SERVER-SETUP.exe`.

Future acceptance must demonstrate rejected unauthorized dispatch, client survival
during server outages, identity rotation, bounded queues and reconciliation of an
operation whose result was lost. Reconnection alone must not replay uncertain side
effects. Server work begins after the standalone foundation; no database, network
listener or service exists here. Read [distributed design](../docs/architecture/DISTRIBUTED-SERVER.md)
and [SQL Server direction](../docs/persistence/SQL-SERVER-CONCEPT.md).
