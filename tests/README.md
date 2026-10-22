# Verification ownership boundary

Status: **future runtime test plan; documentation checks only in Genesis 1**.

No runtime test suite or application integration test exists yet. The repository's
PowerShell checker verifies document paths, links, fenced blocks, conceptual JSON
syntax and selected accidental-secret patterns. Its success must not be reported
as verified machine execution or application control.

Genesis 2 should define independent fixtures before implementation, then exercise
both processes across discovery, valid/invalid invocation, unknown versions,
timeouts, cancellation races, worker exit and restart with uncertain effects.
Later language SDKs must share those fixtures. Application adapters additionally
need independently inspected artifacts and version-qualified results. Preserve
failed receipts and fixed criteria; do not accept only a model's or worker's claim.
See [roadmap](../ROADMAP.md) and [Genesis acceptance](../docs/genesis/GENESIS-1-ACCEPTANCE.md).
