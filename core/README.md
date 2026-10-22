# Core ownership boundary

Status: **PROPOSED ARCHITECTURE; no core runtime implemented**.

This area will own AIXR coordination semantics: capability identity, admission,
worker lifecycle, operation state, verification orchestration and recovery. It
must not become a second copy of every adapter or language runtime. The principal
implementation language remains undecided; Go is a candidate, not an obligatory
hop for every operation.

Inputs will be authorized, versioned HMCP requests and current capability evidence.
Outputs will distinguish accepted work, execution outcomes and verification results.
Local execution must remain independent of server health. The first code belongs
to the scoped [Genesis 2 LibreOffice experiment](../docs/genesis/GENESIS-2-EXPERIMENT.md),
with fixed criteria, independent verification and SQLite evidence. A synthetic
process check may prepare this path; distributed orchestration remains later work.
See [AIXR](../docs/vision/AIXR-CONCEPT.md) and [HMCP](../docs/protocols/HMCP-CONCEPT.md).
