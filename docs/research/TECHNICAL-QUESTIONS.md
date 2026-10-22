# Open technical questions and decision experiments

Status: **PROPOSED ARCHITECTURE**. These questions intentionally remain unresolved
in Genesis 1. Accepted invariants are constraints on the experiments, not evidence
that their implementation already exists.

| Priority / milestone | Question | Smallest useful experiment | Acceptance or decision criterion |
|---|---|---|---|
| P0 / Genesis 2 | What is HMCP's minimal semantic envelope? | Codex discovers the selected conversion through HACE and invokes it through HMCP/AIXR; retain the real document and verification receipts. | Version, execution ID, typed input/output, error and independent verification survive the full path; malformed and unsupported requests are rejected. |
| P0 / Genesis 2 | Which language and framing method best serve the first runtime? | Record a proposed choice between a small Go or .NET host and one bounded transport; implement only the selected prototype. | Reproducible PowerShell launch, explicit process ownership, bounded output, timeout and clean child termination. Record the tradeoff rather than claim a universal winner. |
| P0 / Genesis 2 | What counts as success? | Convert the fixed DOCX through the real supported non-GUI interface and inspect its PDF independently; use separately labeled negative fixtures to exercise verifier rejection. | All AC-01 through AC-10 in the canonical experiment pass for V4; process completion or title presence alone cannot pass. V5 requires a separate successful repeat. |
| P0 / Genesis 2 | What happens after process interruption? | Interrupt before dispatch, during work and after work before reply. | Receipts distinguish not started, failed and unknown outcome; non-idempotent replay is not inferred from a missing response. |
| P1 / Genesis 3 | Which wire types are portable across all six languages? | Shared fixtures for Unicode, null/missing, large integers, decimals and timestamps. | Each worker accepts/rejects the same fixtures; numeric precision loss cannot pass silently. |
| P1 / Genesis 3-4 | What identifies trusted adapters and workers? | Synthetic signed/unsigned or approved/unapproved identities under a documented policy. | Resolve exact executable and manifest identity; reject mismatches without treating signature alone as capability correctness. |
| P1 / Genesis 4 | How are local callers authenticated and scoped? | Synthetic identities call allowed and denied capabilities over the selected local endpoint. | No wildcard administrative authority; loopback binding is tested separately from authorization. |
| P1 / Genesis 4 | What isolation can the supported Windows configurations enforce? | Dedicated disposable processes exercise timeout, tree cleanup and selected OS controls. | Describe actual account/process/resource boundaries; do not call a directory or environment filter a sandbox. |
| P1 / Genesis 4 | How are jobs/events persisted consistently? | Interrupt client writes during job transition and restore from a copied test database. | No completed claim without its required evidence; migration and recovery preserve identity and explain unknown states. |
| P0 / Genesis 2 | Which LibreOffice interface and independent verifier satisfy the fixed proof? | Preflight the actual documented UNO or headless CLI route, pin layout/fonts/export versions, and implement the predetermined two-page DOCX-to-PDF trial. | Independent title, paragraphs, table matrix and page-count checks plus local SQLite readback; missing prerequisites remain blockers. |
| P1 / Genesis 5 | How does the first proof become a reusable adapter framework? | Generalize registration, lifecycle, version matching and verification to another bounded operation. | Demonstrate current availability, independent criteria, failure/recovery receipts and preserved user inputs for each added operation. |
| P2 / Genesis 5 | How does shared application state handle concurrency? | Human-equivalent and machine commands target the same versioned synthetic state. | Reject stale preconditions or serialize intentionally; identify freshness and unavailable fields. |
| P2 / Genesis 6 | How do client/server enrollment and reconnection work? | Two disposable identities and a deliberate disconnection/reconnect sequence. | Local work continues; revocation and expired credentials fail clearly; reconnect does not duplicate uncertain side effects. |
| P2 / Genesis 6 | What belongs in synchronized history? | Exchange bounded synthetic events and artifact descriptors with missing/duplicate messages. | Stable identifiers, provenance and conflict handling; private paths and secret values do not leak into shared records. |
| P2 / Genesis 7 | Which Studio host fits actual user needs? | Compare accessibility, offline behavior and deployment for a small capability explorer. | Shared runtime state and authorization; no second hidden execution authority inside the UI. |
| P3 / Genesis 8-9 | When does reuse reduce human effort? | Measure interventions, retries, verification time and repeated task duration in real authorized workflows. | Report actual observations and a defensible comparison; leave human time recovered unmeasured without a baseline. |

Additional open issues include adapter distribution and update policy, capability
naming governance, semantically equivalent operations across products, verifier
trust and independence, retention/deletion policy, cancellation after irreversible
effects, recovery checkpoint portability, and installer/update rollback behavior.
Each needs a bounded decision record when its milestone makes it relevant.

The [canonical Genesis 2 experiment](../genesis/GENESIS-2-EXPERIMENT.md) is a real
local LibreOffice conversion with independent verification and SQLite evidence.
A synthetic two-process check is optional preparation, never its completion gate.
No experiment or installation runs during Genesis 1. The future proof does not
require SQL Server, a distributed scheduler, GUI automation or all six workers.
The [roadmap](../../ROADMAP.md) owns milestone sequencing; this list supplies the
evidence needed to refine that roadmap.
