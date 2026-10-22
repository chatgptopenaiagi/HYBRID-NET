# Documentation map

HYBRID.NET preserves its Genesis 1 architecture and now implements a bounded
Genesis 2 local execution prototype. Start with the root [README](../README.md),
[architecture](../ARCHITECTURE.md), [status](../PROJECT_STATUS.md) and
[roadmap](../ROADMAP.md). Historical conceptual pages retain their original scope;
use the current implementation and evidence reports for operational claims.

| Area | Purpose and entry point |
| --- | --- |
| Vision | [Founding vision](vision/FOUNDING-VISION.md), human/AI interaction, dual executables and AIXR |
| Architecture | [System overview](architecture/SYSTEM-OVERVIEW.md), client/server/hybrid, workers, discovery, Windows and state |
| Evidence | [Independent verification](architecture/INDEPENDENT-VERIFICATION.md): founding V0–V5 vocabulary, provenance and observation limits |
| Protocols | [Implemented HMCP 0.1](protocols/HMCP-0.1.md), [broader HMCP concept](protocols/HMCP-CONCEPT.md), future MCP mapping and transport alternatives |
| Security | [Identity authorities](security/API-KEY-ARCHITECTURE.md), client enrollment and credential protection |
| Persistence | [Logical data model](persistence/DATA-MODEL.md), SQLite client and optional SQL Server |
| Languages | [Runtime map](architecture/MULTILANGUAGE-RUNTIME.md) and six candidate language domains |
| Integrations | [Codex](integrations/CODEX.md), CFA, SCE, QASC/CLARA, CAE and POK as optional independent consumers |
| Research | [Questions](research/TECHNICAL-QUESTIONS.md), alternatives, primary references and proposed experiments |
| Decisions | [ADR index](decisions/README.md): eight founding records plus the scoped Genesis 2 runtime decision |
| Genesis 1 history | [Scope](genesis/GENESIS-1-SCOPE.md), [traceability](genesis/FOUNDING-TRACEABILITY.md), [acceptance](genesis/GENESIS-1-ACCEPTANCE.md), [report](genesis/GENESIS-1-REPORT.md) |
| Frozen experiment | [Architectural refinements](genesis/ARCHITECTURAL-ADDENDUM.md), [predetermined LibreOffice fixture and AC-01–AC-10](genesis/GENESIS-2-EXPERIMENT.md) |
| Genesis 2 implementation | [Operator guide](genesis/GENESIS-2-IMPLEMENTATION.md), [runtime ADR](decisions/ADR-0009-GENESIS-2-RUNTIME.md), [engineering scripts](../scripts/README.md) |
| Genesis 2 results | [Test and benchmark report](genesis/GENESIS-2-TEST-REPORT.md), [acceptance and gates](genesis/GENESIS-2-ACCEPTANCE.md) |

Source now lives in [core](../core/README.md), [CLI](../client/README.md) and
[independent verification](../verification/verify.py), with scoped adapter and
storage implementations in the core library. Other ownership directories retain
future boundaries rather than pretend to contain completed products. The
[evidence vocabulary](../CONTRIBUTING.md) distinguishes a concept, proposal,
decision, experiment, implemented component and verified capability. A local
documentation check cannot establish application control or platform compatibility.
