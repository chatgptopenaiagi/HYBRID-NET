# Verification ownership boundary

Status: **future runtime test plan; documentation checks only in Genesis 1**.

No runtime test suite or application integration test exists yet. The repository's
PowerShell checker verifies document paths, links, fenced blocks, conceptual JSON
syntax and selected accidental-secret patterns. Its success must not be reported
as verified machine execution or application control.

Genesis 2 should freeze the [LibreOffice fixture and AC-01–AC-10](../docs/genesis/GENESIS-2-EXPERIMENT.md)
before execution, then independently inspect the exported PDF and read back SQLite
evidence. Supporting contract tests cover discovery, invalid invocation, unknown
versions, timeouts, cancellation races and interrupted operations. Later SDKs must
share contract fixtures. Preserve failed receipts and fixed criteria; an adapter's
success report is insufficient. V5 requires a separate same-criteria reproduction.
See [roadmap](../ROADMAP.md) and [Genesis acceptance](../docs/genesis/GENESIS-1-ACCEPTANCE.md).
