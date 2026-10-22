# AIXR: Artificial Intelligence Execution Runtime

**Status: VISION and PROPOSED ARCHITECTURE.** AIXR is a working subsystem name
inside HYBRID.NET. It is not a separate project, implemented runtime, or replacement
for the principal HYBRID.NET identity.

AIXR describes the proposed invocation and execution responsibility: validate an
authorized HMCP request, establish execution identity, invoke an eligible adapter
or worker, manage its lifecycle, collect state and outcomes, and perform controlled
recovery. [HACE](../architecture/CAPABILITY-ENGINE.md) owns application-capability
discovery and metadata/evidence associations. [HMCP](../protocols/HMCP-CONCEPT.md)
defines shared schemas, operation identities and structured results independently
of languages and transports. These are three logical responsibilities, not a
requirement for three executables or services.

AIXR records execution outcomes and arranges
[independent result verification](../architecture/INDEPENDENT-VERIFICATION.md);
an adapter's success response cannot certify its own output. AI assistance can
interpret intent or diagnose failure, but ordinary deterministic operations must
not depend on an additional inference call.

The comparison with established application runtimes describes an architectural
role: reusable execution infrastructure across applications. It does not imply a
new virtual machine, universal binary compatibility, Microsoft affiliation, or
replacement of existing language runtimes. Workers may continue to use Go, C++,
the JVM, JavaScript, Python and .NET behind common HMCP contracts.

The local runtime owns local execution even without a server. Connected operation
adds scheduling and evidence exchange across authorized clients; it must preserve
local authority. Each request needs a deadline, cancellation semantics, bounded
outputs, an outcome distinct from verification, and durable identity for restart
reconciliation. An interrupted operation may have an unknown outcome rather than
be safe to repeat.

The preferred [Genesis 2 experiment](../genesis/GENESIS-2-EXPERIMENT.md) follows
Codex → HACE → HMCP → AIXR → a supported LibreOffice interface for a real DOCX-to-PDF
operation, followed by independent PDF inspection and SQLite evidence. That chain
describes responsibilities, not mandatory process hops. Deterministic execution
remains local, non-GUI, user-space and independent of a central server or internet;
the requesting agent may have separate connectivity requirements. This is future
work, not a delivered adapter. Runtime language and transport remain open decisions
to resolve within the bounded experiment.

See [system overview](../architecture/SYSTEM-OVERVIEW.md) and
[multilanguage runtime](../architecture/MULTILANGUAGE-RUNTIME.md).
