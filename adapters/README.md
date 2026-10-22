# Adapter ownership boundary

Status: **IMPLEMENTED COMPONENT — two explicit Genesis 2 adapter paths**.

The current source is [core/Hybrid.Core/Adapters.cs](../core/Hybrid.Core/Adapters.cs),
keeping the small first runtime in one library rather than duplicating it here.
`document.export_pdf` uses LibreOffice's supported headless conversion command,
staged input, a unique profile and bounded process supervision. `text.normalize`
uses managed code to normalize bounded UTF-8 without BOM to NFC and LF newlines.

Both implement `ICapabilityAdapter`: identity/version, current discovery and bounded
execution. HACE distinguishes installed, registered and verified-for-a-recorded-case
states, including evidence references. The runtime independently checks artifacts
before advertising retained proof. Neither adapter assigns V4; Python observation
and independent SQLite readback supply the verification gate. The managed text path
reuses HMCP, AIXR and storage without a separate framework.

The [adapter contract](../docs/protocols/HMCP-0.1.md) and
[implementation guide](../docs/genesis/GENESIS-2-IMPLEMENTATION.md) describe limits.
The [reference target catalog](REFERENCE-TARGETS.md) remains a list of future
integration candidates, not universal discovery or implemented proprietary-app
access. Each new adapter needs supported interfaces, current preflight, fixed
criteria, independent output checks and failure/cancellation evidence. No GUI
automation fallback is implemented in Genesis 2. Consult
[acceptance gates](../docs/genesis/GENESIS-2-ACCEPTANCE.md) before extending scope.
