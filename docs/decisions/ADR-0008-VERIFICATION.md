# ADR-0008: Verification determines outcome

Status: **ACCEPTED**. Date: 2026-10-22. Classification: **ENGINEERING DECISION**.
Updated by the owner's independent-verification and real-capability addendum.

## Context

An application can exit successfully while producing the wrong artifact. A model
can describe success without executing anything. Historical capability evidence
can become stale when tools, source or configuration change.

## Decision

Adopt the **Independent Evidence Principle**: the executing component's report
cannot by itself verify that the human's intended outcome occurred. Record
execution and verification as distinct outcomes. Each claimed verified
capability needs explicit criteria, versioned inputs, actual execution evidence,
relevant source/tool identity and a verifier result. Artifact descriptors include
hashes and sizes; hashes establish identity, not functional correctness. Unknown
or untested results remain explicit.

Use the proposed HYBRID.NET evidence levels: V0 REQUESTED (operation submitted),
V1 ACCEPTED (runtime accepts a contract-valid operation), V2 EXECUTED (executing
component reports completion), V3 OBSERVED (an independent component inspects
state or artifact), V4 VERIFIED (observations satisfy explicit predetermined
criteria), and V5 REPRODUCIBLE (that verified outcome is reproduced under documented
conditions without weakening the original criteria). V5 requires V4; repeated
execution does not establish correctness. These labels are project terminology,
not an industry standard or implemented state machine. The
[independent-verification design](../architecture/INDEPENDENT-VERIFICATION.md)
owns their detailed meaning and limitations.

Preserve failures and last verified states subject to a documented retention
policy. A correction candidate cannot weaken fixed criteria or promote itself.
Recovery must distinguish retry-safe work from uncertain non-idempotent effects.
Human time recovered is unmeasured until a defensible baseline exists.

## Consequences and alternatives

Verification adds work, storage and domain-specific design, but supplies the
evidence required for autonomous execution. Trusting exit code alone, model
confidence or installed software inventory is rejected. Reusing a prior result
requires matching identities and explicit freshness policy.

## Validation

The [Genesis 2 experiment](../genesis/GENESIS-2-EXPERIMENT.md) must demonstrate real
LibreOffice DOCX-to-PDF conversion through Codex, HACE, HMCP and AIXR, independent
PDF inspection and local SQLite evidence. Its ten mandatory fixed cases must all
pass for V4. A separate repeat with unchanged criteria is needed for V5. Supporting
negative tests must reject incorrect artifacts even after successful process exit.
A synthetic two-process check may prepare the engineering path but cannot replace
this real proof. Genesis 1 implements neither the experiment nor the level system.
