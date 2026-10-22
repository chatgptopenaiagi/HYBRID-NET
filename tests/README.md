# Verification ownership boundary

Status: **IMPLEMENTED AND TESTED — separate automated and real-application evidence**.

[test.ps1](../scripts/test.ps1) runs four sequential suites and preserves actual
counts, exit codes, stdout/stderr and machine-readable results under
`.local/test-runs`. The recorded final regression run passed **118 tests**:

| Suite | Count | Scope |
| --- | --- | --- |
| [Contracts](contracts/Program.cs) | 44 | Strict HMCP fields, versions, identities, path policy and lifecycle |
| [SQLite store](store/Program.cs) | 24 | Persistence, immutable receipts, restart/owner recovery, transaction interruption |
| [Processes/adapters](process/Program.cs) | 17 | Real owned subprocesses, bounds, cancellation and managed adapter behavior |
| [Python verification](python/test_verification.py) | 33 | Synthetic defective PDFs, fixture integrity and independent readback checks |

These suites do not substitute for a real LibreOffice operation. The separate
[acceptance harness](acceptance/run_experiments.py), invoked by
[demo-genesis2.ps1](../scripts/demo-genesis2.ps1), exercised the packaged executable
and genuine application path: **33/33 packaged checks** and **11/11 controlled fault
scenarios**, with no false successful V4 conclusion in the fault cases. Do not add
these differently scoped counts as though they were distinct application executions.
See the [test report](../docs/genesis/GENESIS-2-TEST-REPORT.md) and
[acceptance matrix](../docs/genesis/GENESIS-2-ACCEPTANCE.md) for receipts and limits.

The [frozen LibreOffice experiment](../docs/genesis/GENESIS-2-EXPERIMENT.md) defines
AC-01–AC-10. Preserve source bytes and criteria, distinguish FAILED from INCONCLUSIVE,
and keep V5 as a separate reproduction of V4. Feasibility and reuse passed the
recorded gates; engineering advantage remains INCONCLUSIVE. Documentation validation
still checks links/coverage/JSON and is not a runtime test. The
[Genesis 1 acceptance record](../docs/genesis/GENESIS-1-ACCEPTANCE.md) remains history.
