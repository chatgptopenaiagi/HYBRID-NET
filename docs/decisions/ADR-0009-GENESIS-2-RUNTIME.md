# ADR-0009: first local runtime and independent verification

Status: **ACCEPTED for the Genesis 2 prototype**. Date: 2026-10-22.
This narrows implementation choices without replacing the founding architecture.

## Context

The owner authorized one complete Windows user-space path from capability discovery
through real LibreOffice DOCX-to-PDF conversion to independent verification and
durable evidence. The client must work without the central server, inference,
network APIs or all six language runtimes. The fixed two-page fixture and ten
acceptance criteria precede implementation and cannot be weakened for a result.

## Decision

Use C# and .NET 10 for one core library and one JSON CLI. `global.json` pins the
available stable SDK 10.0.402 with `latestPatch` and `allowPrerelease: false`, rather
than selecting the installed preview implicitly. The observed published runtime
is .NET 10.0.13 x64. Pin Microsoft.Data.Sqlite 10.0.13 for embedded SQLite; no
database server is involved. Preserve local tools and use project-local caches.

Implement HMCP 0.1 as validated semantic JSON requests, state, errors and receipts.
The local CLI is the transport for this milestone. Do not add HTTP, gRPC, named
pipes or MCP solely to make the first invocation more elaborate. Use an explicit
adapter registry; discovery checks implemented interfaces, not arbitrary executable
internals. C# interfaces are an initial implementation binding, not the universal
language contract planned for future milestones.

Use LibreOffice's supported headless command line with `writer_pdf_Export`, copied
input, a unique user profile and a new output directory. Use Python in a separate
process for pypdf/pdfplumber parsing and separate SQLite readback. The verifier
does not ask LibreOffice to validate its own output. Record identities, dependency
versions, criteria hashes, observations and limitations. Same-account processes
and shared storage are acknowledged common failure domains, not a security sandbox.

Publish a complete self-contained `win-x64` .NET directory. The alternative
framework-dependent mode is explicit in build.ps1. Neither mode bundles LibreOffice
or Python; users must preserve and configure those external dependencies. No
single-executable, installer, clean-machine or Windows 11 portability claim follows.

## Alternatives and consequences

A Go orchestration core remains plausible for later concurrency and distributed
work, but was unnecessary for the available managed Windows implementation. UNO
remains suitable for richer document state and operations; bounded conversion
does not need a live UNO service. A Python-only execution path would reduce the
number of runtimes but would not exercise the owner's preferred managed core.
SQLite provides a small durable local store; server SQL integration remains future.

HACE/HMCP/AIXR stay logical responsibilities. Go, C++, Java, JavaScript/TypeScript,
Python and C#/.NET remain the founding ecosystem. Adding worker transports later
must preserve operation identity, bounds, cancellation and truthful verification
rather than forcing all calls through six redundant processes.

## Evidence and review gate

The first packaged operation `first-real` independently passed all ten frozen
criteria at V4; a later re-verification also passed without replacing its original
receipt. See [implementation evidence](../genesis/GENESIS-2-IMPLEMENTATION.md).
Those receipts establish the narrow path, not the full Genesis 2 matrix,
performance superiority, reusable six-language SDKs or production readiness.
Keep feasibility, reuse and engineering advantage as separate recorded gates.
