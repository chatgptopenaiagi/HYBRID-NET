# Real application and packaged executable acceptance

`run_experiments.py` copies the complete Windows delivery bundle to a fresh ignored
workspace, invokes its executable from a separate working directory, and preserves
every command and JSON receipt. The supplied Python environment and LibreOffice are
external dependencies; this is not clean-machine portability certification.

Run with `--cli <published Hybrid.CLI.exe> --python <python.exe> --root <new repository
.local directory>`. Optional `--libreoffice` selects the actual soffice.com;
`--phase benchmark` or `--phase failures` runs a subset in a new workspace. Existing
evidence directories are rejected, never replaced. Exit zero requires every selected
check to pass and the original fixture fingerprint to remain unchanged.

Experiment A directly invokes the supported LibreOffice command, without routing
execution through HYBRID.NET. Experiment B uses the copied HYBRID.NET executable.
Both use the same pinned input and AC-01 through AC-10, separate profiles/output
directories, an independent Python PDF verifier and committed SQLite readback.
The fixed order A/B/B/A/A/B gives three trials each. It is a small controlled
comparison, not a population performance estimate. Process execution, verifier
assessment and total wall times are separate. Memory samples cover the launcher
process only, excluding LibreOffice children. Human labor and engineering advantage
remain unestablished without comparable human observations.

Failure tests use only disposable documents and operation profiles. A deliberate
LibreOffice interruption first identifies a soffice.bin command line containing
the unique injected operation's UserInstallation path; unrelated processes are
untouched. Mutation tests preserve the prior PDF and immutable historical receipt,
then require a new negative re-verification. Missing dependencies and parser
uncertainty cannot become V4. Interrupted evidence transactions and synthetic
verifier faults additionally run in the separate unit suites.

The fixture and expected two-page content come from the preserved
[Genesis 2 experiment](../../docs/genesis/GENESIS-2-EXPERIMENT.md). Network services,
remote databases and cloud credentials are not configured. Physically disconnecting
the workstation is not part of this harness and must not be claimed as tested.
