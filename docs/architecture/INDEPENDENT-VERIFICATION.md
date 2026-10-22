# Independent verification and evidence

**ENGINEERING DECISION — Independent Evidence Principle.** A reported success
does not establish that the intended result occurred. Conclusions must follow
documented observations evaluated against criteria defined before execution.
The evidence model below is **PROPOSED ARCHITECTURE**; no observer, verifier or
runtime is implemented by this document.

## Preserve the complete chain

Each operation should distinguish eight facts, including missing or uncertain facts:

1. **Requested:** the human or authorized agent's intended result and fixed criteria.
2. **Selected capability:** the eligible versioned capability and its resource scope.
3. **Selected interface:** the concrete application/adapter and supported API, CLI,
   scripting or other mechanism chosen to reach that capability.
4. **Executed action:** the dispatch and execution evidence, including identity,
   bounded command or invocation, timing, exit result and interruption.
5. **Application-reported result:** what the worker or application claimed happened.
6. **Independently observed result:** what another component actually inspected,
   including observation mechanism, freshness and limits.
7. **Criteria evaluation:** each predetermined criterion, its observed value and
   pass/fail/inconclusive result.
8. **Conclusion:** the justified verification outcome and evidence level, with
   limitations and failure diagnostics rather than a stronger unsupported claim.

An application can report success while producing an incorrect file. Conversely,
a connection loss can leave an artifact present but the full side-effect outcome
unknown. Preserve execution state, application reports and verification findings
separately; neither case authorizes blind replay.

## Proposed HYBRID.NET evidence levels

These are HYBRID.NET design terms, not an external standard, integration maturity
levels L0–L5 or claims of implemented capability.

| Level | Meaning | Evidence needed to assert the level |
|---|---|---|
| V0 REQUESTED | A defined operation was requested. | Request identity and intended result; no acceptance or execution implied. |
| V1 ACCEPTED | The responsible runtime accepted a contract-valid operation request. | Contract-validation and authorized-admission receipt with durable operation identity; no execution implied. |
| V2 EXECUTED | Execution completion was reported. | Dispatch/execution receipt and worker/application report, including its result; this label alone does not prove the requested effect. |
| V3 OBSERVED | A component independent of the producing component observed the relevant result. | Observer identity, actual observations, mechanism and independence limits; observation can reveal failure. |
| V4 VERIFIED | The observed result passed predetermined verification criteria. | Independent observations, fixed criterion version and explicit passing evaluations for the claimed result. |
| V5 REPRODUCIBLE | A V4 result was reproduced in a separate trial under documented conditions using the same criteria. | Linked V4 receipts, separate trial identities, conditions, relevant input/tool identities and passing evaluations. |

Levels are evidence assertions, not automatic lifecycle transitions or a confidence
score. A runtime must not increment a level merely because time passed or an exit
code was zero. `verification.status` remains independently `not_requested`, `pending`,
`passed`, `failed` or `inconclusive`; failed criteria can accompany a V3 observation
without achieving V4. Evidence gaps must remain explicit, including a null level
when no level is actually supported. A partial observation does not settle an
`unknown_outcome` for unobserved side effects.

V5 requires V4; repeating the same wrong answer establishes neither correctness
nor verified reproducibility. A new failed trial remains a failure, not an upgrade
to a prior receipt. Reproduction is limited to the recorded conditions and criteria,
not a promise about every machine or future input. Repeating an operation requires
its own authorization and a safe test/recovery policy, especially for non-idempotent
effects.

## Independence and proportionate checks

Use an independent observer where feasible: for example, another component parses
the produced document or inspects supported application state instead of trusting
the producing adapter's success flag. Calling the same implementation twice, or
relaying its message through another process, does not by itself establish
independence. Record shared libraries, application APIs, accounts and other common
failure sources. A separate verifier is not automatically a separate security domain.

Verification strength should match the operation's consequences, complexity and
available evidence. A deterministic synthetic result may permit an exact comparison;
a document export may require content, format and resource checks; consequential
state changes may require independent state observation and recovery evidence.
If independent observation is unavailable, report that limitation and the supported
lower level instead of treating a producer assertion as V4. No finite criterion set
proves all possible behavior.

## Provenance contract

A receipt should retain the operation ID (`job_id` after HMCP admission), request
and trial identity; application and adapter identities/versions; capability and
contract version; authorized input references and hashes where applicable;
dispatch/execution result and application report; observer identity and independent
mechanism; criterion version/hash; actual observations and evaluations; output
references/hashes; verification status and level; and bounded failure diagnostics.
Record timestamps, relevant conditions and evidence locations. Missing facts must
be explicit, not filled with plausible values. Hashes establish byte identity,
not semantic correctness. References must enforce access policy, and receipts must
contain no credentials or unnecessary private content.

## Three subsystem boundaries and first proof

**HACE — Hybrid Application Capability Engine** identifies eligible capabilities,
supported interfaces and current evidence. **HMCP — Hybrid Machine Capability
Protocol** carries versioned requests, reports, observations and verification
provenance. **AIXR — Artificial Intelligence Execution Runtime** coordinates
authorized execution, independent observation, criteria evaluation and durable
receipts. Verification is a component responsibility within these boundaries, not
a fourth named subsystem or permission for the producing adapter to certify itself.

See [HACE](CAPABILITY-ENGINE.md), [HMCP](../protocols/HMCP-CONCEPT.md) and the
[Genesis 2 experiment and AC-01–AC-10](../genesis/GENESIS-2-EXPERIMENT.md).
The first experiment must demonstrate an independently detected wrong result and
honest uncertainty as well as a successful result. Its planned acceptance criteria
are not evidence that any execution has already occurred.
