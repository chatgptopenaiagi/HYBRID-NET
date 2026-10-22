# HYBRID.NET roadmap

Status: **MEASURABLE ENGINEERING PLAN**. Current implementation version 0.1.0.
This is an evidence-gated sequence, not a release schedule or a promise that all
languages, applications or operating systems already work. Refine it with ADRs.

| Milestone | Scope | Required exit evidence |
| --- | --- | --- |
| Genesis 1: architecture foundation — completed | Repository, preserved vision, contracts, governance and continuity | Foundation commit `59924a07d7870665478d9ed279f2af8ab76daf32`; prior validation and publication retained in Genesis 1 report |
| Genesis 2: real independently verified capability — operational proof verified, acceptance qualified | C# CLI/core, HACE registry, HMCP JSON, AIXR, non-GUI LibreOffice conversion, independent Python verification and SQLite; second managed text adapter | 118 automated and 33 packaged checks pass; six AC-01–AC-10 trials, semantic V5 and 11/11 faults pass; feasibility/reuse pass, advantage inconclusive; physical offline test remains open |
| Genesis 3: multilanguage workers | Incrementally add Go, C++, Java, JS/TS, Python and .NET workers behind the same contract | Each language passes identical contract fixtures; types, errors, cancellation and encoding agree; missing workers do not disable others |
| Genesis 4: full standalone Windows client | Generalize the narrow Genesis 2 path into optional authenticated loopback API, local registry, SQLite, credential integration and configuration | Works offline with server absent; unauthorized requests denied; restart/migration/credential handling and local recovery demonstrated on recorded Windows versions |
| Genesis 5: broader application-adapter framework | Extend the initial LibreOffice proof to reusable adapter lifecycle and additional document/application operations | Versioned registration, current interface preflight, independent verification and failure/recovery evidence for each additional operation; preserve originals and report unsupported states/app versions |
| Genesis 6: optional server | Enrollment, identity, distributed routing and optional SQL Server provider | Reject untrusted clients and unauthorized jobs; local client survives outages; reconnect reconciles unknown outcomes without duplicate non-idempotent effects; verify returned artifact provenance |
| Genesis 7: Studio | Human capability explorer, agent/client sessions, configuration, history and diagnostics | Human and agent views share state and policy; accessible UI tests; concurrent edits/conflicts surfaced; no UI-only path required for supported machine operations |
| Genesis 8: optional ecosystem integrations | CFA, SCE, QASC/CLARA, CAE and POK contracts | One independently versioned integration at a time; unavailable-provider fallback; permissions and evidence preserved; no repository merger or mandatory external dependency |
| Genesis 9: hardening and distribution | Client/server installers, measurements, recovery and deployment verification | Clean-machine installs/uninstalls, upgrade/rollback, failure injection, documented performance/resource limits and tested platform matrix; release claims limited to measured behavior |

## Current engineering step: close the remaining Genesis 2 gaps

Use the [canonical Genesis 2 experiment](docs/genesis/GENESIS-2-EXPERIMENT.md).
The owner's revised priority is one useful real capability: convert a controlled
DOCX to PDF through a supported LibreOffice interface, verify the result with an
independent parser and persist execution/verification evidence in local SQLite.
Codex reaches it through HACE discovery, HMCP and AIXR. All ten predetermined
acceptance cases are mandatory for V4. V5 is a separate reproduction claim.
The [implementation guide](docs/genesis/GENESIS-2-IMPLEMENTATION.md),
[test report](docs/genesis/GENESIS-2-TEST-REPORT.md) and
[acceptance matrix](docs/genesis/GENESIS-2-ACCEPTANCE.md) record the working path.
The real proof, failure matrix, second capability and controlled reproduction pass.
Use a dedicated disconnected test environment for the remaining offline proof;
do not disrupt shared workstation networking. Establish comparable human effort
measurements before claiming an advantage. No optimization is justified solely by
the present six timing samples, in which the framework was slower.

Pin the input bytes, two-page expectation, exact synthetic text/table, fonts,
application/export settings and verifier criteria before the trial. The narrow
client execution path must work without a central server or GUI automation.
This does not require all six workers, a complete Studio UI or a finished client.
The Genesis 1 planning boundary remains historical; Genesis 2 explicitly authorizes
the scoped runtime and experiment. Existing application profiles remain protected.

The [runtime ADR](docs/decisions/ADR-0009-GENESIS-2-RUNTIME.md) selects C#/.NET,
local JSON contracts, Python verification and embedded SQLite. An
optional synthetic two-process exchange may test framing and lifecycle before
application integration, but it cannot complete Genesis 2. Preserve bounded
failure receipts for invalid requests, stale outputs, timeout, cancellation and
interrupted persistence. Unknown outcomes must not become success or unsafe replay.

Milestone completion requires the actual real pipeline and independently readable
evidence, not just a hand-run conversion or successful process exit. Text and table
checks do not establish pixel-perfect rendering; additional layout criteria are
separate. Record missing prerequisites as blockers instead of substituting a mock.

## Cross-cutting gates

Security, offline behavior, resource policy, evidence persistence, compatibility,
testability and recovery begin with the first executable milestone; they are not
postponed entirely until Genesis 9. Genesis 3 proves interoperability mechanisms;
it does not require every language runtime on every deployed client. Later user
interface work must not change the standalone-first execution contract.

Before performance optimization, record cold and repeated operation costs and
correctness under failure. Before artifact reuse, define source, environment and
capability-version invalidation. Before claiming reduced human labor, record
interventions and a measured baseline; otherwise time recovered is unmeasured.

A milestone advances only when its exit evidence is in PROJECT_STATUS.md and an
associated report. A blocked integration can remain blocked while independent
work continues; do not label the whole roadmap complete from one demonstration.
