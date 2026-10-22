# ADR-0006: SQLite client and optional SQL Server

Status: **ACCEPTED** as persistence directions; providers are unimplemented.
Date: 2026-10-22. Classification: **ENGINEERING DECISION**.

## Context

Standalone clients need durable state without a database service. Distributed
installations may need centralized storage and administration. The founder
explicitly includes SQL technology in the framework.

## Decision

Use SQLite as the intended client persistence engine and Microsoft SQL Server as
an optional server provider. Define logical entities and repository operations
before coupling execution code to provider-specific SQL. Database engine, HMCP
contract and schema versions are separate identifiers. No database server is
installed and no schema migration is implemented in Genesis 1.

The shared logical model includes applications, capabilities, adapters, clients,
agents, jobs, events, verification, state observations, recovery checkpoints and
dependencies. Store credential references rather than secret values. The client
database remains authoritative for its local execution outcomes.

## Consequences and alternatives

SQL semantics, concurrency and migration behavior need provider-specific tests;
an abstraction does not make the engines interchangeable. A mandatory remote
database is rejected. Additional server providers remain possible after a demonstrated
need; a generic ORM is not selected here.

## Validation

Future tests must cover transaction interruption, schema upgrades, event/job
consistency and server disconnection. Provider conformance must preserve execution
identity and unknown-outcome handling rather than relying on distributed
exactly-once assumptions.
