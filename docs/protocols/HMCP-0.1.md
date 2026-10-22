# HMCP 0.1 implemented local contracts

Status: **IMPLEMENTED**, Genesis 2, product 0.1.0. The original
[HMCP concept](HMCP-CONCEPT.md) remains the broader architectural direction.
This version has a local CLI/JSON binding; no HTTP listener, MCP server or remote
worker transport is implemented. The [structural definitions](../../protocols/hmcp-0.1.schema.json)
describe messages; `Contracts.cs` performs actual request validation and additional
filesystem checks. The schema file is not a claim that every schema keyword has a
general-purpose validator in the runtime.

## Request and scope

Requests require `protocol: HMCP`, `version: 0.1`, an operation ID, capability,
matching application and `parameters.source`/`parameters.destination`. Supported
capabilities are `document.export_pdf` / `LibreOffice` and `text.normalize` /
`Hybrid.Text`. Paths are absolute local paths inside the explicitly supplied
workspace. UNC/device paths, alternate streams, reparse ancestors, reserved names,
invalid filename characters and source/output aliases are rejected. Outputs must
be new files; an existing output is never silently overwritten.

Optional `timeout_seconds` is an integer from 1 to 300. Optional `criteria_path`
must be inside the workspace; otherwise the packaged versioned criteria apply.
The runtime snapshots criteria before execution and checks their hash again at
promotion. Negative tests use separately identified criteria, preserving originals.
Unknown fields, duplicate JSON properties, unsupported versions and malformed
requests are rejected. CLI request-file mode cannot silently override JSON fields
with additional source/output flags.

An ID is 1–80 ASCII letters, digits, underscores or hyphens, beginning with a
letter/digit. IDs are case-sensitive database identities; prefixed hashed run
directory names avoid Windows device/case aliases. The canonical normalized JSON
fingerprint makes reordered object fields equivalent. An identical duplicate returns
the existing receipt without replay; conflicting reuse fails. A later changed input
does not cause a duplicate ID to execute again. Use a new ID for a new trial.

## Results, lifecycle and evidence

The implemented operation lifecycle is CREATED → VALIDATED → PREPARING → RUNNING
→ EXECUTED → VERIFYING → VERIFIED/FAILED/INCONCLUSIVE, with bounded failure and
cancellation exits. It refines the conceptual lifecycle names without changing
their execution-versus-verification distinction. V0–V5 are independent evidence
levels, not numerical success scores. V3 is recorded after an independent
observation. V4 is stored with the terminal receipt in one SQLite transaction only
after all mandatory criteria and storage readback pass.

Every criterion carries expected/observed values, PASS/FAIL/INCONCLUSIVE, a reference
and verifier identity. PDF criteria are AC-01–AC-10; text criteria TC-01–TC-06.
The Python component's VERIFIED outcome covers its nine/five observations only;
the runtime must add the independent storage gate before assigning V4. A process
exit code alone never provides that level. Request rejection is V0; absence of
execution evidence cannot be repaired by a plausible artifact.

Original receipts are immutable. `verify` appends a complete new observation and
storage check. Later failure invalidates the earlier receipt as current discovery
evidence without deleting its historical conclusion. `reproduce` requires two
distinct V4 trials with matching input, criteria, application/adapter environment
and verifier identity; it checks retained artifacts and rejects later failed
observations. A separate V5 event links the trials without rewriting either V4
receipt. This is semantic reproducibility, not necessarily equal PDF bytes.

## Adapter boundary and language expansion

`ICapabilityAdapter` supplies metadata, current discovery and bounded execution.
LibreOffice uses explicit process arguments and a unique profile; the text adapter
uses managed Unicode/UTF-8 APIs. Neither adapter can assign VERIFIED. The C# runtime
owns admission, lifecycle, independent verifier invocation, persistence and promotion.
The Python verifier exchanges JSON request/results over process files/stdout and
does not depend on LibreOffice's document parser.

Future Go, C++, Java, JavaScript/TypeScript, Python and C#/.NET workers may implement
these same capability/result semantics through a versioned process binding. The
current C# interface is an in-process implementation detail, not a universal ABI.
Worker handshake, transport framing and SDK conformance remain future work. Studio,
MCP and central-server consumers must preserve these identities and gates rather
than invent a second execution authority.

## Practical limits

The workspace boundary prevents ordinary accidental path escapes; it is not an OS
sandbox or a defense against a hostile same-user process racing filesystem links.
LibreOffice and the verifier run with the invoking user's rights. Capture/timeouts
are bounded, but root-process memory samples are not process-tree resource limits.
No credential, model or network service is required for the supported local path.
Clean-machine deployment, network-disconnected operation and Windows 11 require
their own evidence before certification claims.
