# HACE: Hybrid Application Capability Engine

**Status: PROPOSED ARCHITECTURE.** No installed-application scan or adapter probe
is implemented in Genesis 1.

HACE is the proposed application-capability responsibility inside HYBRID.NET. It
identifies application capabilities, discovers supported integration interfaces,
inspects their limitations, registers capability metadata and provenance, and
associates each capability with execution and independent-verification evidence.
HACE does not gain access to hidden application internals or manufacture an adapter
when a supported interface is absent.

## Three logical responsibilities

| Responsibility | Architectural boundary |
| --- | --- |
| HACE | Discovery and capability lifecycle: identify supported operations, maintain metadata, provenance, availability and evidence associations |
| HMCP | Common schemas and contracts: operation identities, requests, structured results, errors and verification references, independent of languages and transports |
| AIXR | Invocation and execution: launch or communicate with workers, manage operation lifecycle and outcomes, and perform controlled recovery |

These boundaries do not require three executables, services or separate language
implementations. HMCP is a contract, not a mandatory running intermediary. The
smallest coherent implementation may share one host while keeping responsibilities
testable. See [HMCP](../protocols/HMCP-CONCEPT.md) and
[AIXR](../vision/AIXR-CONCEPT.md).

## Capability lifecycle

The conceptual lifecycle is **DISCOVER → IDENTIFY → INSPECT → ADAPT → REGISTER →
EXECUTE → VERIFY**. It spans the framework; HACE does not implement every stage.

| Stage | Required meaning |
| --- | --- |
| DISCOVER | Find application candidates and declared interfaces; presence alone is not a capability claim |
| IDENTIFY | Establish application, version, adapter and operation identities |
| INSPECT | Examine documented interfaces, prerequisites, scope and current evidence |
| ADAPT | Map a supported interface through an implemented adapter; otherwise retain a proposed or unavailable capability |
| REGISTER | Record HMCP metadata, provenance, authorization requirements and availability |
| EXECUTE | AIXR dispatches an authorized bounded invocation and records its actual outcome |
| VERIFY | Independently assess the result against fixed criteria and associate the evidence with the operation |

Verification is a separate result, not an automatic consequence of registration or
successful execution. Its canonical requirements are in
[independent verification](INDEPENDENT-VERIFICATION.md).

## Discovery and evidence

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
The preferred [Genesis 2 experiment](../genesis/GENESIS-2-EXPERIMENT.md) is a real
LibreOffice DOCX-to-PDF workflow through a supported non-GUI interface, with an
independent PDF parser and SQLite evidence. It remains planned, not implemented;
the deterministic application pipeline must run locally without a central server
or network dependency. The experiment document owns its precise acceptance scope.
