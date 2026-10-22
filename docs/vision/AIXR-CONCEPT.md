# AIXR: Artificial Intelligence Execution Runtime

**Status: VISION and PROPOSED ARCHITECTURE.** AIXR is a working subsystem name
inside HYBRID.NET. It is not a separate project, implemented runtime, or replacement
for the principal HYBRID.NET identity.

AIXR describes the proposed common execution services between agents and
capabilities: validate a request, establish execution identity, resolve an eligible
adapter, enforce policy, schedule bounded work, collect state and events, verify
results, and preserve recovery information. AI assistance can interpret intent or
diagnose failure; ordinary registered deterministic operations must not depend on
an additional inference call.

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

Genesis 2 should test only two local processes and one synthetic capability. Its
evidence should cover discovery, success, invalid input and a controlled worker
failure. Persistent scheduling, multiple languages, distributed recovery and
application adapters remain later milestones. Runtime language and transport are
open decisions requiring focused experiments and ADRs.

See [system overview](../architecture/SYSTEM-OVERVIEW.md) and
[multilanguage runtime](../architecture/MULTILANGUAGE-RUNTIME.md).
