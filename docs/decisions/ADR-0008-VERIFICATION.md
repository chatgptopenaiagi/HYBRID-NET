# ADR-0008: Verification determines outcome

Status: **ACCEPTED**. Date: 2026-10-22. Classification: **ENGINEERING DECISION**.

## Context

An application can exit successfully while producing the wrong artifact. A model
can describe success without executing anything. Historical capability evidence
can become stale when tools, source or configuration change.

## Decision

Record execution and verification as distinct outcomes. Each claimed verified
capability needs explicit criteria, versioned inputs, actual execution evidence,
relevant source/tool identity and a verifier result. Artifact descriptors include
hashes and sizes; hashes establish identity, not functional correctness. Unknown
or untested results remain explicit.

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

Genesis 2 tests must include a process that completes but returns an incorrect
result, plus interrupted and invalid invocations. Later artifact tests must verify
content independently of the producing adapter and record the actual environment.
