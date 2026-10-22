# Founding mission traceability

Status: **VISION preservation / documentation review index**. Each numbered section
of the founding mission is mapped below. This is a curated semantic index, not a
claim that the described framework functions are implemented. The final directive
is represented by sections 0, 36–42 and 44. The full vision is preserved in
[FOUNDING-VISION.md](../vision/FOUNDING-VISION.md); the machine-readable companion is
[founding-requirements.json](founding-requirements.json).

| Original section | Topic | Durable location | Preserved boundary |
| --- | --- | --- | --- |
| 0 | Executive mission | [README.md](../../README.md), [GENESIS-1-SCOPE.md](../../docs/genesis/GENESIS-1-SCOPE.md) | Architecture foundation; no full framework implementation. |
| 1 | Fundamental philosophy | [HUMAN-AI-INTEROPERABILITY.md](../../docs/vision/HUMAN-AI-INTEROPERABILITY.md), [FOUNDING-VISION.md](../../docs/vision/FOUNDING-VISION.md) | Human visual interfaces, machine capabilities, supported-interface limits. |
| 2 | Dual executable applications | [DUAL-EXECUTABLE-CONCEPT.md](../../docs/vision/DUAL-EXECUTABLE-CONCEPT.md), [ADR-0005-DUAL-EXECUTABLES.md](../../docs/decisions/ADR-0005-DUAL-EXECUTABLES.md) | Shared native core; third-party companion does not grant internal access. |
| 3 | AI-native execution framework and AIXR | [AIXR-CONCEPT.md](../../docs/vision/AIXR-CONCEPT.md), [FOUNDING-VISION.md](../../docs/vision/FOUNDING-VISION.md) | Shared infrastructure; AIXR is a subsystem, HYBRID.NET retains identity. |
| 4 | New software interaction layer | [ARCHITECTURE.md](../../ARCHITECTURE.md), [SYSTEM-OVERVIEW.md](../../docs/architecture/SYSTEM-OVERVIEW.md) | Agents, core, discovery/orchestration/verification, adapters and user-space OS boundary. |
| 5 | Six-language ecosystem | [MULTILANGUAGE-RUNTIME.md](../../docs/architecture/MULTILANGUAGE-RUNTIME.md), [ADR-0003-MULTILANGUAGE.md](../../docs/decisions/ADR-0003-MULTILANGUAGE.md) | Common semantics without six mandatory installed runtimes. |
| 6 | Go engineering domain | [GO.md](../../docs/languages/GO.md) | Candidate concurrent orchestration and communication domain. |
| 7 | C++ engineering domain | [CPP.md](../../docs/languages/CPP.md) | Documented native SDK, DLL and Windows integration. |
| 8 | Java engineering domain | [JAVA.md](../../docs/languages/JAVA.md) | JVM and enterprise workers sharing capability contracts. |
| 9 | JavaScript and TypeScript domain | [JAVASCRIPT.md](../../docs/languages/JAVASCRIPT.md) | Studio/browser/developer clients; UI technology remains open. |
| 10 | Python engineering domain | [PYTHON.md](../../docs/languages/PYTHON.md) | AI, scripting, diagnostics; deterministic operations do not need inference. |
| 11 | C# and .NET engineering domain | [DOTNET.md](../../docs/languages/DOTNET.md) | Managed Windows/COM services; distinct from Microsoft products. |
| 12 | HMCP universal capability contract | [HMCP-CONCEPT.md](../../docs/protocols/HMCP-CONCEPT.md), [README.md](../../protocols/README.md), [ADR-0004-CAPABILITY-CONTRACTS.md](../../docs/decisions/ADR-0004-CAPABILITY-CONTRACTS.md) | Versioned discovery, invocation, state, identity, errors, cancellation and verification. |
| 13 | Semantic capability discovery and L0-L5 | [CAPABILITY-ENGINE.md](../../docs/architecture/CAPABILITY-ENGINE.md) | Installed versus evidenced capability; maturity is not permission. |
| 14 | Live application state | [APPLICATION-STATE.md](../../docs/architecture/APPLICATION-STATE.md) | Supported structured observations with versions, timestamps and limits. |
| 15 | Two-sided deployment | [ADR-0002-DUAL-DEPLOYMENT.md](../../docs/decisions/ADR-0002-DUAL-DEPLOYMENT.md), [ARCHITECTURE.md](../../ARCHITECTURE.md) | Separate client/server products; server optional. |
| 16 | Client responsibilities and proposed products | [STANDALONE-CLIENT.md](../../docs/architecture/STANDALONE-CLIENT.md), [README.md](../../client/README.md) | Local gateway/runtime, adapters, verification, identity, persistence. |
| 17 | Standalone mode | [STANDALONE-CLIENT.md](../../docs/architecture/STANDALONE-CLIENT.md) | Local registry/evidence/configuration and offline deterministic operations. |
| 18 | Connected mode | [DISTRIBUTED-SERVER.md](../../docs/architecture/DISTRIBUTED-SERVER.md) | Authorized distributed discovery, jobs and evidence. |
| 19 | Hybrid mode | [HYBRID-MODE.md](../../docs/architecture/HYBRID-MODE.md) | Appropriate local-first selection and authorized remote specialization. |
| 20 | Server-independent operation | [STANDALONE-CLIENT.md](../../docs/architecture/STANDALONE-CLIENT.md), [HYBRID-MODE.md](../../docs/architecture/HYBRID-MODE.md) | Outages do not stop local work; uncertain side effects are reconciled. |
| 21 | Optional local API | [HMCP-CONCEPT.md](../../docs/protocols/HMCP-CONCEPT.md), [API-KEY-ARCHITECTURE.md](../../docs/security/API-KEY-ARCHITECTURE.md) | Conceptual loopback 55001/execute interface; scoped authentication. |
| 22 | Server responsibilities and proposed products | [DISTRIBUTED-SERVER.md](../../docs/architecture/DISTRIBUTED-SERVER.md), [README.md](../../server/README.md) | Enrollment, identity, scheduling, registry, health, optional SQL Server. |
| 23 | Three credential authorities | [API-KEY-ARCHITECTURE.md](../../docs/security/API-KEY-ARCHITECTURE.md), [CLIENT-IDENTITY.md](../../docs/security/CLIENT-IDENTITY.md) | Provider credentials, client identity and scoped framework tokens. |
| 24 | Windows credential protection | [CREDENTIAL-STORAGE.md](../../docs/security/CREDENTIAL-STORAGE.md), [CAE.md](../../docs/integrations/CAE.md) | DPAPI/Credential Manager candidates; no secrets in source/logs/ordinary rows. |
| 25 | SQL persistence and logical entities | [SQLITE-CONCEPT.md](../../docs/persistence/SQLITE-CONCEPT.md), [SQL-SERVER-CONCEPT.md](../../docs/persistence/SQL-SERVER-CONCEPT.md), [DATA-MODEL.md](../../docs/persistence/DATA-MODEL.md) | SQLite client, optional SQL Server, eleven conceptual entities, no installation. |
| 26 | Human interface and Studio | [SYSTEM-OVERVIEW.md](../../docs/architecture/SYSTEM-OVERVIEW.md), [JAVASCRIPT.md](../../docs/languages/JAVASCRIPT.md) | Human management/exploration over shared runtime state; UI stack open. |
| 27 | AI interface | [MCP-INTEGRATION.md](../../docs/protocols/MCP-INTEGRATION.md), [CODEX.md](../../docs/integrations/CODEX.md) | Structured agent discovery/status/errors/evidence; local and remote AI optional. |
| 28 | Existing project integration | [CFA.md](../../docs/integrations/CFA.md), [SCE.md](../../docs/integrations/SCE.md), [QASC-CLARA.md](../../docs/integrations/QASC-CLARA.md), [CAE.md](../../docs/integrations/CAE.md), [POK.md](../../docs/integrations/POK.md) | Independent optional neighboring projects; no merger or mandatory dependency. |
| 29 | Reference application adapters | [REFERENCE-TARGETS.md](../../adapters/REFERENCE-TARGETS.md), [README.md](../../adapters/README.md) | All named targets preserved as future candidates, including LibreOffice. |
| 30 | Ten design principles | [FOUNDING-VISION.md](../../docs/vision/FOUNDING-VISION.md), [AGENTS.md](../../AGENTS.md) | Human outcome, direct interfaces, independent client, modularity and verified labor reduction. |
| 31 | Repository organization | [README.md](../../README.md), [README.md](../../docs/README.md) | Substantive ownership documents; no dummy code modules. |
| 32 | Documentation evidence classes | [CONTRIBUTING.md](../../CONTRIBUTING.md), [PROJECT_STATUS.md](../../PROJECT_STATUS.md) | Vision/proposal/decision/hypothesis/implementation/verified distinctions. |
| 33 | Eight initial ADRs | [README.md](../../docs/decisions/README.md) | Identity, deployment, languages, contracts, dual interfaces, SQL, credentials, verification. |
| 34 | Genesis 1-9 roadmap | [ROADMAP.md](../../ROADMAP.md) | Measured milestone exits and evidence-driven refinement. |
| 35 | Future agent continuity | [AGENTS.md](../../AGENTS.md) | First reads, scope, invariants, repository layout and milestone discipline. |
| 36 | GitHub creation and publication | [GENESIS-1-REPORT.md](../../docs/genesis/GENESIS-1-REPORT.md) | Check existence/authentication; preserve existing work; actual publication evidence. |
| 37 | Local workspace preservation | [GENESIS-1-SCOPE.md](../../docs/genesis/GENESIS-1-SCOPE.md), [GENESIS-1-REPORT.md](../../docs/genesis/GENESIS-1-REPORT.md) | I:\HYBRID-NET checked before creation; neighboring repositories untouched. |
| 38 | README, architecture and status requirements | [README.md](../../README.md), [ARCHITECTURE.md](../../ARCHITECTURE.md), [PROJECT_STATUS.md](../../PROJECT_STATUS.md) | Clear answers, system diagrams, accurate implementation boundary. |
| 39 | Twenty acceptance criteria | [GENESIS-1-ACCEPTANCE.md](../../docs/genesis/GENESIS-1-ACCEPTANCE.md) | Fixed documentation/commit/publication checks; no accidental credentials. |
| 40 | Genesis implementation boundary | [GENESIS-1-SCOPE.md](../../docs/genesis/GENESIS-1-SCOPE.md), [README.md](../../tests/README.md) | No runtime prototype, service, OS modifications or third-party installations. |
| 41 | Version and readiness | [VERSION](../../VERSION), [PROJECT_STATUS.md](../../PROJECT_STATUS.md), [CHANGELOG.md](../../CHANGELOG.md) | 0.0.1, Genesis 1, architecture foundation; no product readiness claim. |
| 42 | Final report | [GENESIS-1-REPORT.md](../../docs/genesis/GENESIS-1-REPORT.md) | Local/remote/branch/commit/docs/decisions/questions/next step and actual verification. |
| 43 | Long-term framework identity | [FOUNDING-VISION.md](../../docs/vision/FOUNDING-VISION.md), [ADR-0001-PROJECT-IDENTITY.md](../../docs/decisions/ADR-0001-PROJECT-IDENTITY.md) | AI-native execution/interoperability across runtimes and authorized computers. |
| 44 | Founding principle and final directive | [FOUNDING-VISION.md](../../docs/vision/FOUNDING-VISION.md), [AGENTS.md](../../AGENTS.md) | Human intent, machine execution, evidence-based verification and reduced repetitive labor. |

The documentation checker verifies unique coverage IDs 0–44 and the existence of
each target. Human/agent review must still assess substantive completeness and
technical credibility. Acceptance IDs G1-01 through G1-20 are a separate checklist.

The later [architectural addendum](ARCHITECTURAL-ADDENDUM.md) has its own nine-section
map. It refines subsystem/evidence responsibilities and the preferred Genesis 2
experiment while preserving this original founding record.
