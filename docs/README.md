# Documentation map

HYBRID.NET Genesis 1 preserves architecture rather than a runnable framework.
Start with the root [README](../README.md), [architecture](../ARCHITECTURE.md),
[status](../PROJECT_STATUS.md) and [roadmap](../ROADMAP.md).

| Area | Purpose and entry point |
| --- | --- |
| Vision | [Founding vision](vision/FOUNDING-VISION.md), human/AI interaction, dual executables and AIXR |
| Architecture | [System overview](architecture/SYSTEM-OVERVIEW.md), client/server/hybrid, workers, discovery, Windows and state |
| Protocols | [HMCP](protocols/HMCP-CONCEPT.md), agent-facing MCP mapping and transport alternatives |
| Security | [Identity authorities](security/API-KEY-ARCHITECTURE.md), client enrollment and credential protection |
| Persistence | [Logical data model](persistence/DATA-MODEL.md), SQLite client and optional SQL Server |
| Languages | [Runtime map](architecture/MULTILANGUAGE-RUNTIME.md) and six candidate language domains |
| Integrations | [Codex](integrations/CODEX.md), CFA, SCE, QASC/CLARA, CAE and POK as optional independent consumers |
| Research | [Questions](research/TECHNICAL-QUESTIONS.md), alternatives, primary references and proposed experiments |
| Decisions | [Eight ADRs](decisions/README.md) with explicit status, scope and consequences |
| Genesis | [Scope](genesis/GENESIS-1-SCOPE.md), [traceability](genesis/FOUNDING-TRACEABILITY.md), [acceptance](genesis/GENESIS-1-ACCEPTANCE.md), [report](genesis/GENESIS-1-REPORT.md) |

Top-level ownership directories describe future components and their acceptance
boundaries. They contain no dummy implementation modules. The
[evidence vocabulary](../CONTRIBUTING.md) distinguishes a concept, proposal,
decision, experiment, implemented component and verified capability. A local
documentation check cannot establish application control or platform compatibility.
