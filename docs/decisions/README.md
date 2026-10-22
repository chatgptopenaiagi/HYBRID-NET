# Architecture decision index

Status vocabulary: PROPOSED, ACCEPTED, EXPERIMENTAL, SUPERSEDED. An accepted
architectural constraint does not mean its component has been implemented.
Read each ADR for its exact status and for subordinate options still undecided.

| ADR | Decision scope |
| --- | --- |
| [0001](ADR-0001-PROJECT-IDENTITY.md) | HYBRID.NET identity, purpose and Genesis boundary |
| [0002](ADR-0002-DUAL-DEPLOYMENT.md) | Standalone client and optional centralized server |
| [0003](ADR-0003-MULTILANGUAGE.md) | Six-language ecosystem behind shared contracts; initial implementation open |
| [0004](ADR-0004-CAPABILITY-CONTRACTS.md) | Common HMCP semantics separate from transport and optional MCP mapping |
| [0005](ADR-0005-DUAL-EXECUTABLES.md) | Human/AI interfaces over a native shared core; external adapter limits |
| [0006](ADR-0006-PERSISTENCE.md) | SQLite client and optional SQL Server behind meaningful storage contracts |
| [0007](ADR-0007-CREDENTIAL-SEPARATION.md) | Provider credentials, client identity and scoped framework tokens |
| [0008](ADR-0008-VERIFICATION.md) | Evidence-driven verification independent of execution completion |
| [0009](ADR-0009-GENESIS-2-RUNTIME.md) | Genesis 2 C#/.NET local runtime, Python verification, SQLite and self-contained Windows bundle |

Future records should state context, alternatives, choice, consequences and the
evidence needed to evaluate them. Supersede decisions explicitly and retain the
old record. Do not mark transport, language, UI framework, identity format or
isolation options accepted merely because they appear in a founding diagram.
