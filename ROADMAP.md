# HYBRID.NET roadmap

Status: **PROPOSED ARCHITECTURE / measurable engineering plan**. Version 0.0.1.
This is an evidence-gated sequence, not a release schedule or a promise that all
languages, applications or operating systems already work. Refine it with ADRs.

| Milestone | Scope | Required exit evidence |
| --- | --- | --- |
| Genesis 1: architecture foundation | Repository, preserved vision, contracts, governance and continuity | Founding traceability reviewed; document/link/JSON checks pass; local commit verified; remote publication verified when authenticated |
| Genesis 2: minimal HMCP experiment | One coordinator and one worker process, one synthetic deterministic capability, no external app | Version negotiation, discovery, scoped invocation, independent expected output, malformed input rejection, timeout/cancel/disconnect outcomes and durable receipt; no central-server dependency |
| Genesis 3: multilanguage workers | Incrementally add Go, C++, Java, JS/TS, Python and .NET workers behind the same contract | Each language passes identical contract fixtures; types, errors, cancellation and encoding agree; missing workers do not disable others |
| Genesis 4: standalone Windows client | Optional authenticated loopback API, local registry, SQLite, credential integration and configuration | Works offline with server absent; unauthorized requests denied; restart/migration/credential handling and local recovery demonstrated on recorded Windows versions |
| Genesis 5: first real adapter | Candidate: LibreOffice document manipulation through supported APIs | Current app/interface preflight; create/edit/export fixture; independently inspect resulting document/PDF; preserve originals; report unsupported states and app versions |
| Genesis 6: optional server | Enrollment, identity, distributed routing and optional SQL Server provider | Reject untrusted clients and unauthorized jobs; local client survives outages; reconnect reconciles unknown outcomes without duplicate non-idempotent effects; verify returned artifact provenance |
| Genesis 7: Studio | Human capability explorer, agent/client sessions, configuration, history and diagnostics | Human and agent views share state and policy; accessible UI tests; concurrent edits/conflicts surfaced; no UI-only path required for supported machine operations |
| Genesis 8: optional ecosystem integrations | CFA, SCE, QASC/CLARA, CAE and POK contracts | One independently versioned integration at a time; unavailable-provider fallback; permissions and evidence preserved; no repository merger or mandatory external dependency |
| Genesis 9: hardening and distribution | Client/server installers, measurements, recovery and deployment verification | Clean-machine installs/uninstalls, upgrade/rollback, failure injection, documented performance/resource limits and tested platform matrix; release claims limited to measured behavior |

## Next recommended step: Genesis 2

Choose the first coordinator/worker language pair and one transport through a
focused ADR. A small stdio JSON exchange is a candidate, not a settled framework
stack. Native pipes and gRPC should be evaluated against actual requirements,
deployment complexity, message limits, lifecycle and identity needs.

Use a synthetic capability such as bounded integer addition with independent
fixtures fixed before implementation. Keep it free of personal files, credentials,
external applications, provider inference and internet access. Define request,
job and capability identities, protocol versions and deterministic error cases.

Required experiments: unknown method/version; oversized request; invalid input;
worker exit before result; timeout; cancellation race; duplicate idempotency key;
restart with a dispatched job whose outcome is uncertain. Preserve raw bounded
receipts and distinguish process completion from successful verification. An
unknown-outcome job must not quietly become a success or an automatic retry.

Pass only after the same fixture can be invoked repeatedly from PowerShell with
recorded interpreter/binary versions and no server. Do not use a synthetic result
to claim LibreOffice control, distributed reliability or installed client support.

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
