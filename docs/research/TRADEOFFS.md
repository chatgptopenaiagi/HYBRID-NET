# Architectural tradeoffs

Status: **PROPOSED ARCHITECTURE**. The accepted invariants are recorded in the
[ADRs](../decisions/ADR-0001-PROJECT-IDENTITY.md). This comparison does not select a
runtime, transport, Studio framework or deployment technology. External technology
facts and primary sources are collected in [research provenance](EXISTING-TECHNOLOGIES.md).

| Choice | Reason to consider it | Cost or uncertainty | Decision evidence needed |
|---|---|---|---|
| Go principal runtime | Concurrency, network and process orchestration are plausible fits for its candidate role. | Windows-specific integration may require a bridge; an additional runtime boundary must earn its place. | Same synthetic operation and failure tests in a focused prototype, measured startup/memory/lifecycle behavior. |
| .NET principal runtime | Managed Windows APIs and service integration align with the initial OS scope. | Must preserve language-neutral workers and avoid coupling HMCP to CLR-only types. | Compare Windows integration effort and deployment with Go; no assumption that either wins all operations. |
| Child-process standard streams | Simple launch ownership and a small local experiment. | Need framing, bounded buffers, cleanup and protocol/log separation; unrelated clients need another connection path. | Two-process tests for partial frames, output flood, timeout and abrupt exit. |
| Named pipes | Windows process communication with configurable access controls. | Identity/access policy and local-only enforcement require deliberate design; connection lifecycle adds work. | Cross-user denial, remote rejection and handle cleanup tests on the supported OS. |
| JSON-RPC mapping | Inspectable messages and a small bootstrap surface. | Schema validation and HMCP lifecycle semantics remain application work. | Equivalent success/error/unknown-outcome receipts and incompatible-version rejection. |
| gRPC / Protocol Buffers | Generated language bindings and structured service definitions. | Adds generators and dependencies; schema compatibility must be governed. | Six-language conformance plus measured message/stream needs, after the minimal experiment. |
| In-process native calls | Can avoid serialization for SDK-heavy operations. | ABI/lifetime errors can affect the host; third-party code shares its process boundary. | Demonstrated performance need and fault containment analysis. |
| Worker processes | Separate lifecycles and runtime versions. | Extra memory, IPC and artifact transfer; a separate process is not automatically a sandbox. | Crash recovery, bounded resources and deterministic receipts. |
| Browser Studio / native shell / packaged web UI | Each can consume a common runtime API. | Accessibility, deployment size, offline operation and attack surface differ. | Actual UI requirements and small accessibility/deployment comparison in Genesis 7. |
| SQLite / optional SQL Server | Local independence plus centralized administration where useful. | Concurrency, migration and synchronization are distinct design problems. | Provider conformance, interrupted writes and disconnected-client recovery. |

The first experiment should minimize simultaneous decisions: one synthetic
capability, two processes, one framing method and a fixed verifier. A prototype
choice is reversible and must not silently become a mandate for all components.
Adding six workers, a remote server and a GUI at once would make failures difficult
to attribute and provide little evidence about the core contract.

There is an unavoidable tension between convenient automatic retries and avoiding
duplicate side effects. Use explicit idempotency semantics and durable execution
identity; when the outcome is unknown, reconcile instead of assuming safe replay.
Likewise, semantic discovery can improve ranking, but only registry evidence and
policy can determine whether a capability is eligible for execution.

Performance decisions should compare equivalent verified work, including startup,
serialization, application time and verification cost. No benchmark is supplied
in Genesis 1, so claims of superior speed or human time saved remain unmeasured.
