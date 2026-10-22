# Contributing to HYBRID.NET

HYBRID.NET is at Genesis 1, an architecture foundation. Read [AGENTS.md](AGENTS.md),
[status](PROJECT_STATUS.md) and the [roadmap](ROADMAP.md) before proposing work.
There is no product runtime to install or executable to download.

## A scoped engineering contribution

1. Check the actual repository root, branch, HEAD and working tree. Preserve existing
   edits and choose a branch scoped to one milestone or architectural question.
2. Identify the human outcome, affected contracts, explicit non-goals and measurable
   acceptance criteria. Keep other repositories and local services unchanged.
3. Record architectural choices in an ADR. Accepted constraints do not establish
   implemented features; uncertain technology selections stay proposed or experimental.
4. Use primary technology documentation and record assumptions, alternatives,
   limitations and the experiment that would settle the question.
5. Validate local documentation links and JSON syntax with PowerShell 7:
   `& .\scripts\validate-docs.ps1`. Read the results; this is not a runtime test.
6. Stage only the contribution, inspect the diff and commit. Describe the problem,
   resulting design or behavior, validation and remaining uncertainty in review.

## Documentation vocabulary

| Label | Meaning |
| --- | --- |
| VISION | Human-defined direction, not implemented functionality |
| PROPOSED ARCHITECTURE | A design to evaluate and refine |
| ENGINEERING DECISION | An ADR choice with scope, alternatives and consequences |
| EXPERIMENTAL HYPOTHESIS | A testable claim with an explicit evaluation plan |
| IMPLEMENTED COMPONENT | Existing code or tooling with a defined boundary |
| VERIFIED CAPABILITY | Implementation supported by reproducible execution evidence |

ADRs use PROPOSED, ACCEPTED, EXPERIMENTAL or SUPERSEDED. Accepted vision constraints
and design decisions are not capability certifications. Prefer precise relative
links, diagrams with clear boundaries and conceptual examples labeled as such.
Do not populate empty source packages to suggest implementation progress.

## Later implementation contributions

All workers must preserve the common contract, standalone behavior and independent
verification criteria. Test failures, cancellation, unknown outcomes, state conflicts
and restart behavior, not just a happy path. Record exact commands, versions,
bounded outputs and artifact hashes. Keep secrets and machine-specific logs in
ignored local storage, and never import another project's credentials or code by
assumption. Do not install shared services to satisfy a documentation milestone.

License selection remains an owner decision. No LICENSE file or unrequested
third-party source import forms part of this foundation. Repository publication
does not imply production readiness or authorization to publish later releases.
