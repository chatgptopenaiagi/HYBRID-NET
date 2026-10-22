# Capability engine

**Status: PROPOSED ARCHITECTURE.** No installed-application scan or adapter probe
is implemented in Genesis 1.

Semantic discovery answers what a system can accomplish, such as finding a
verified PDF-export operation. It should search registered capability descriptions
and structured tags, then filter by policy, resources and current evidence. A
language model may interpret a request, but must not invent a capability absent
from the registry.

| Level | Integration maturity |
| --- | --- |
| L0 | Explicit graphical-automation fallback |
| L1 | Command-line integration |
| L2 | Application scripting |
| L3 | Native API, SDK, COM or library integration |
| L4 | Native HYBRID.NET machine interface |
| L5 | Full AI-native lifecycle and state interoperability |

Levels describe the integration of an individual capability. They are neither
permission levels nor a guarantee that all functions in an application are
exposed. A rich L3 read-only operation may require less authority than a destructive
L1 command. Availability and authorization must be represented separately.

Proposed registry records include operation identity and version, adapter identity,
application versions, inputs/outputs, side-effect class, required resources,
integration level, probe result, observation timestamp, evidence reference and
verification contract. Distinguish application detected, adapter registered,
preflight available, execution verified, stale, unavailable and unknown states.
Verification applies to a recorded environment and case, not all future inputs.

Before dispatch, resolve actual application paths and check relevant dependencies.
Avoid expensive or state-changing probes without a defined policy. Cache metadata
with explicit freshness rules; invalidate affected claims after application,
adapter, configuration or permission changes. A historical successful receipt
cannot prove that an executable remains present today.

Future tests should demonstrate filtering of unverified claims, expiration of stale
evidence and refusal of an unauthorized operation despite a valid installation.
See [HMCP](../protocols/HMCP-CONCEPT.md).
