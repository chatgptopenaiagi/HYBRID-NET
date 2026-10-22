# Genesis 1 architectural addendum

Status: **ARCHITECTURAL PRESERVATION / conceptual**, 2026-10-22. This records the
owner's refinement of the existing HYBRID.NET foundation. It creates no new
repository or implementation milestone. Version remains 0.0.1.

The table gives each addendum section one canonical design location so future
agents can recover the complete intent without duplicating the specifications.

| Owner section | Preserved requirement | Canonical document |
|---|---|---|
| 1 | HACE identifies/registers capabilities and provenance; HMCP defines contracts; AIXR coordinates execution/recovery. The seven-stage lifecycle is shared work, not three mandatory services. | [HACE and subsystem responsibilities](../architecture/CAPABILITY-ENGINE.md) |
| 2 | Separate requested operation, chosen capability/interface, execution, application report, independent observation, criteria evaluation and conclusion. Strength follows consequences and available evidence. | [Independent Evidence Principle](../architecture/INDEPENDENT-VERIFICATION.md) |
| 3 | Proposed V0 REQUESTED through V5 REPRODUCIBLE; V4 needs fixed criteria, V5 requires V4 and separate reproduction. These are not industry-standard labels. | [Verification hierarchy](../architecture/INDEPENDENT-VERIFICATION.md) |
| 4 | Record identity, component/contract versions, inputs, execution, verifier, fixed criteria, observations, hashes, level and diagnostics without credentials. Hashes do not prove semantics. | [Evidence provenance](../architecture/INDEPENDENT-VERIFICATION.md), [data model](../persistence/DATA-MODEL.md) |
| 5 | Preferred Genesis 2: Codex → HACE → HMCP → AIXR → supported LibreOffice conversion → independent PDF verification → SQLite evidence. Local user-space path without server, GUI control or runtime internet dependency. | [Genesis 2 experiment](GENESIS-2-EXPERIMENT.md) |
| 6 | Controlled DOCX with title, paragraphs, table, page break and predetermined page count; mandatory AC-01–AC-10; visual fidelity is a separate requirement; V5 a separate trial. | [Fixed fixture and acceptance plan](GENESIS-2-EXPERIMENT.md) |
| 7 | Preserve standalone client, dual executables, six language roles, local SQLite/optional central SQL Server and supported-interface limits; no kernel or universal-executable claims. | [Architecture](../../ARCHITECTURE.md), [agent continuity](../../AGENTS.md) |
| 8 | Update existing design, roadmap and ADRs with concise cross-references; no Genesis 2 implementation. | [Roadmap](../../ROADMAP.md), [ADR-0008](../decisions/ADR-0008-VERIFICATION.md), [scope](GENESIS-1-SCOPE.md) |
| 9 | Validate links/documentation, review tracked files, commit the scoped addendum and verify publication when available. | [Genesis report](GENESIS-1-REPORT.md) |

The original [45-section founding map](FOUNDING-TRACEABILITY.md) remains intact.
The preferred operational proof moves into Genesis 2; a synthetic two-process
exchange is optional preparation rather than a substitute. Later client and adapter
milestones broaden the narrow proof instead of claiming it already exists.

Human intent defines the destination. HYBRID.NET provides machine-to-machine
execution capabilities. Independent verification determines whether the intended
result was achieved. Genesis 1 preserves that architecture; Genesis 2 must prove it.
