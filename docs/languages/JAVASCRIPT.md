# JavaScript and TypeScript engineering domain

**Status: PROPOSED ARCHITECTURE.** `Hybrid.Studio` is the candidate human interface;
there is no UI, dashboard or browser adapter in Genesis 1.

Proposed responsibilities include application inventory, capability exploration,
local AI configuration, server connections, execution history, verification
reports, adapter management, agent sessions, remote clients and runtime health.
Browser administration, WebSocket clients and developer tools are possible
extensions. The UI should consume the same authorized runtime operations as
machine clients and display the same execution identifiers and verification state.

TypeScript provides static checking for JavaScript programs, as described by the
[TypeScript handbook](https://www.typescriptlang.org/docs/handbook/intro.html).
Static types do not validate an untrusted runtime payload; future clients still
need contract validation and explicit error handling at IPC/API boundaries.

A local browser UI, a lightweight native shell, an embedded web view and other
desktop approaches remain alternatives. Evaluate offline operation, accessibility,
installation size, credential handling, update needs and Windows support before
selecting a desktop framework. No heavy UI dependency should become necessary for
a headless authorized agent to invoke a capability.

Future acceptance should show accurate partial progress, stale capability evidence,
failed verification and uncertain remote outcomes rather than a generic success
banner. A UI reconnect must recover job status without resubmitting a side effect.
Browser-facing endpoints require deliberate origin and authentication design;
loopback reachability is not sufficient authorization.

Studio should provide understandable controls for human intent and approvals while
leaving deterministic execution and persistence in the shared runtime boundary.

See [standalone client](../architecture/STANDALONE-CLIENT.md).
