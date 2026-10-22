# ADR-0003: Shared semantics across language workers

Status: **ACCEPTED** for interoperability boundaries; implementation selections
remain **PROPOSED**. Date: 2026-10-22. Classification: **ENGINEERING DECISION**.

## Context

The founding ecosystem includes Go, C++, Java, JavaScript/TypeScript, Python and
C#/.NET. Each has different runtime, memory, deployment and application integration
properties. Requiring all six for the first operation would defeat a small start.

## Decision

All workers must use common capability and execution semantics. The candidate
domains are Go orchestration, C++ native bridges, Java JVM workers,
JavaScript/TypeScript Studio and web integration, Python AI/scripting/diagnostics,
and .NET managed Windows integration. These are ownership proposals, not six
mandatory dependencies or a finalized call graph.

Start with only the components needed for the
[Genesis 2 LibreOffice experiment](../genesis/GENESIS-2-EXPERIMENT.md); a synthetic
process check is optional preparation. Select language and transport through a
focused ADR, with explicit worker boundaries. Go is a
candidate principal runtime; it is not a compulsory intermediary for every call.

## Consequences and alternatives

Process boundaries add serialization and lifecycle work but avoid assuming one
universal ABI. Direct library calls may be justified inside an adapter. A single
language implementation or embedded interpreters remain options for specific
components if shared semantics and isolation requirements are preserved.

## Validation

Cross-language conformance will test numeric ranges, Unicode, schema/version
rejection, error mapping, cancellation and artifact descriptors. Adding a language
requires actual worker evidence, not a directory with its name.
