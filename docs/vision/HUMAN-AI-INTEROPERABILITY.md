# Human and AI interoperability

**Status: VISION and PROPOSED ARCHITECTURE.** Genesis 1 preserves an interaction
model; it provides no application-control runtime.

Humans benefit from windows, previews, tools and spatial representations. Machines
benefit from explicit operations, typed inputs, observable state and testable
outcomes. HYBRID.NET aims to let both use the same software capabilities without
requiring every agent operation to reproduce a human gesture sequence.

A request such as "export the current document as a PDF" should resolve to an
authorized capability with a defined document identity, destination, progress
model and verification contract. The agent interprets the intent; the framework
validates and executes the request; verification determines whether the requested
artifact exists and meets the agreed criteria. An AI statement that the work is
complete is not execution evidence.

The preferred interface depends on the application. A supported native API, SDK,
CLI or scripting interface may expose the desired function. Another operation in
the same application may require visual automation. Discovery must identify the
actual integration method and its limitations for each capability. No universal
access to proprietary executable internals is presumed.

Human involvement remains meaningful: users define outcomes, authorize sensitive
operations and resolve ambiguity. The framework should reduce repetitive manual
navigation and diagnosis, while retaining understandable execution history and
control. It should measure human interventions and verification outcomes without
inventing a conventional-work baseline.

Future validation should compare the same bounded task through direct and visual
interfaces, recording execution time, interventions, errors and independently
verified results. Direct invocation is a preference supported by task evidence,
not a claim that every GUI workflow can already be replaced.

See [dual executables](DUAL-EXECUTABLE-CONCEPT.md) and the
[capability engine](../architecture/CAPABILITY-ENGINE.md).
