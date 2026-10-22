# HYBRID.NET founding vision

Status: **VISION**. Preserved from the owner's HYBRID.NET Genesis mission on
2026-10-22. The project is **HYBRID.NET**, repository **HYBRID-NET**, owned by
**chatgptopenaiagi**. Version 0.0.1 denotes an architecture foundation, not a
working product. [Traceability](../genesis/FOUNDING-TRACEABILITY.md) maps every
numbered founding section to its durable design location.

## Purpose

Human intent defines the destination. Artificial intelligence interprets the
engineering objective. HYBRID.NET is intended to provide direct machine-to-machine
execution and interoperability; verification determines whether the destination
was reached. The purpose is measurable reduction of repetitive human engineering
labor, not autonomous activity without bounds.

The framework connects human intent, AI agents, application engines, programming
language runtimes, Windows executables, operating-system services and local or
remote computers. It should become a common environment for discovering, invoking,
coordinating, observing and verifying software capabilities. It is a long-term
initiative, not merely another graphical Windows automation application.

Humans interact with visual representations. Machines should interact with
structured capabilities where supported. An AI agent should not need hundreds of
simulated clicks for an operation exposed by an authorized API. Human interfaces
remain valuable, and visual automation remains an optional fallback where a suitable
machine interface is absent. No claim is made that every application exposes its
complete functionality through APIs, scripting, libraries or OS interoperability.

## Founding architectural idea: dual executables

A new native application may offer `Application.exe` to people and
`Application.AI.exe` to agents. Both reach a shared application core and coherent
state. The AI executable is a capability gateway: it exposes structured commands,
operations, state and verification information rather than a second GUI.

`Photoshop.exe` and `Photoshop.AI.exe` illustrate the idea; they are not a delivered
integration or an endorsement. A companion for existing proprietary software can
only adapt available, supported interfaces. The founding idea is preserved without
asserting that its novelty, patentability or universal feasibility has been proven.

## Framework and runtime identity

HYBRID.NET expands to **Hybrid Native Execution and Interoperability Framework**.
**AIXR**, Artificial Intelligence Execution Runtime, is the working name of an
internal runtime subsystem. The intended architectural role is shared execution
infrastructure, comparable at a broad level to a framework/runtime ecosystem, not
an assertion of Microsoft .NET compatibility or affiliation.

The framework is positioned between agents and application capabilities. It
coordinates discovery, orchestration and verification through adapters for native
APIs, scripting and local IPC. The initial boundary is user space. Deeper OS
interoperability remains possible future work, but drivers or kernel changes are
not assumptions or Genesis tasks.

## Six language domains

The founding ecosystem includes Go, C++, Java, JavaScript/TypeScript, Python and
C#/.NET. Go is a candidate for orchestration and communication; C++ for native SDK
and DLL bridges; Java for JVM workers and enterprise integration; JavaScript and
TypeScript for Studio, browsers and developer clients; Python for AI, scripting,
diagnosis and transformations; C#/.NET for managed Windows, COM and service bridges.

Working component names are `Hybrid.Go.Runtime`, `Hybrid.Cpp.Bridge`,
`Hybrid.Java.Worker`, `Hybrid.Studio`, `Hybrid.Python.Worker` and
`Hybrid.DotNet.Bridge`. They describe ownership proposals. They are not implemented
packages, mandatory dependencies or a decision that all calls must pass through Go.
Ordinary verified operations should not depend on unnecessary inference.

## Capability semantics and state

HMCP, Hybrid Machine Capability Protocol, is the proposed common internal contract
for registration, discovery, invocation, inputs, outputs, state, progress,
cancellation, errors, execution identity, verification and versions. Protocol
Buffers, gRPC, JSON-RPC, named pipes, stdio and WebSockets require evaluation. MCP
is an optional AI-agent integration mechanism and may map onto HMCP semantics.
They are not all mandatory transports or interchangeable security systems.

Semantic discovery should answer questions such as which registered applications
can create, edit or export PDFs. It must distinguish an installed application from
a verified available operation. Proposed maturity levels are L0 graphical fallback,
L1 CLI, L2 scripting, L3 native API/SDK/COM/library, L4 native HYBRID.NET interface
and L5 full AI-native lifecycle/state interoperability. No installed product is
automatically assigned an implemented level.

Agents should observe supported structured state: current document, active project,
loaded file, commands, process status, operation progress, errors and events.
Native applications may build this into their core; third-party adapters must use
available observation interfaces and disclose missing or stale state.

## Independent client, optional server, hybrid operation

The client and server are separate intended installable products. The client owns
the human UI, local AI gateway, runtime, adapters, workers, verification, diagnostics,
credential protection, local database and optional server connection. It must remain
fully functional for local deterministic work without the server or internet.
Local models may operate offline when installed; remote providers require their
own connectivity. Proposed names are `Hybrid.Client.exe` and
`HYBRID-NET-CLIENT-SETUP.exe`.

The server adds client enrollment and identity checks, distributed registry and
scheduling, agent coordination, execution history, API access, health and verification
collection, with optional SQL Server. Proposed names are `Hybrid.Server.exe` and
`HYBRID-NET-SERVER-SETUP.exe`. Registration never grants unrestricted administrative
access to the client.

Standalone mode routes agent requests through the local runtime. Connected mode
coordinates approved operations across enrolled computers. Hybrid mode chooses
appropriate local work and authorized remote specialization, then verifies returned
artifacts. Server downtime, crashes, intentional disconnection or restart must not
disable local capabilities. Recovery must reconcile uncertain remote outcomes and
avoid duplicate non-idempotent side effects.

The optional local API is proposed as loopback-only. `http://127.0.0.1:55001` and
`POST /api/v1/execute` are design examples, not implemented endpoints. Protected
capabilities require authentication and scoped authorization; no automatic external
network exposure is intended.

## Credentials, SQL and interfaces

Three credential categories remain separate: AI provider credentials; enrolled
client identity credentials, potentially certificates/mTLS with rotation; and
HYBRID.NET API tokens scoped to capabilities and resources. Windows DPAPI,
Credential Manager and appropriate OS APIs are research candidates. Secrets must
not enter source files, ordinary database records, diagnostic reports or logs.

SQL is an explicit founding requirement. SQLite serves standalone client records;
optional Microsoft SQL Server serves distributed installations. Storage abstractions
must preserve meaningful transaction and recovery semantics without forcing one
database product everywhere. The logical entities are Applications, Capabilities,
Adapters, Clients, Agents, ExecutionJobs, ExecutionEvents, VerificationResults,
ApplicationStates, RecoveryCheckpoints and Dependencies. No database server needs
installation during Genesis 1.

HYBRID.NET Studio is the future human capability explorer, inventory, provider and
connection configuration UI, history/report viewer, adapter/runtime dashboard and
agent/client management interface. JavaScript/TypeScript is a candidate, but a heavy
desktop framework is not predetermined. Human and AI interfaces cooperate over
shared runtime state. AI consumers may include Codex, local Qwen, remote provider
adapters, MCP-compatible agents and future CFA/SCE agents.

## Neighboring projects and possible applications

CFA may observe environments and plan future engineering; SCE may acquire/adapt
sources, build, verify and package software; QASC may coordinate model-assisted
engineering; CLARA may diagnose failures and verify isolated repairs; CAE may
provide architectural credential/API ideas; POK may demonstrate structured game
state and deterministic controls. These are optional integration roles, not verified
HYBRID.NET integrations. Their repositories remain independent and unchanged.

Future adapter candidates include LibreOffice, Blender, Photoshop, Dreamweaver,
CCleaner, Joomla, Poker-Odds-Knowledge, PowerShell, Windows services and other
Windows applications. LibreOffice document manipulation is a promising later
experiment because of its documented automation interfaces. No third-party adapter
or application installation belongs to this Genesis milestone.

## Ten founding principles

1. Human intent defines the destination.
2. HYBRID.NET provides machine execution and interoperability.
3. Verification determines whether the intended result occurred.
4. A client must work without the central server.
5. Direct machine interfaces are preferred when supported.
6. Visual automation remains an optional fallback.
7. No capability may be claimed without implementation evidence.
8. All supported languages use common execution contracts.
9. The framework must remain modular.
10. Human engineering labor should be reduced through verified automation.

## Genesis discipline

Genesis 1 establishes the repository, architecture, decisions, governance, roadmap
and agent continuity. Subsequent milestones progress through a minimal HMCP/local
process experiment, language workers, standalone client, a real application adapter,
distributed server, Studio, optional project integrations and measured hardening.
The ordering is revisable through evidence; passing a documentation check does not
advance a runtime milestone. Future contributors must keep vision, proposals,
decisions, hypotheses, implementation and verified capability distinct.
