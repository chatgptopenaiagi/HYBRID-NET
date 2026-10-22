# Working on HYBRID.NET

## Mission and first reads

HYBRID.NET means Hybrid Native Execution and Interoperability Framework. Genesis 1,
version 0.0.1, establishes an architecture foundation. It does not implement a
runtime, application adapter, client, server, SDK, installer or executable.

Read this file, README.md, PROJECT_STATUS.md, ARCHITECTURE.md, ROADMAP.md and
docs/genesis/GENESIS-1-SCOPE.md before work. Check the actual Git root, branch,
HEAD, origin and working tree; dated reports do not replace current checks.
Preserve user edits. The intended checkout is I:\HYBRID-NET. Do not modify CAE,
CFA, SCE, QASC, CLARA, POK, capability labs, local AI installations or credentials.
Referencing another project does not authorize edits or imports from it.

## Architectural invariants

- Human intent defines the outcome; independent verification determines success.
- The client works independently of the optional central server. Deterministic
  local operations must not require internet access, remote AI or a server.
- Human and AI executables share a core in native applications. A companion for
  an existing application can expose only supported and authorized interfaces.
- Prefer direct APIs, CLI, scripting, COM and documented SDKs where available.
  Graphical automation is an explicitly labeled fallback, never hidden evidence
  of deeper application access. Installed software is not a verified capability.
- HMCP is the proposed common capability contract. MCP is an optional agent-facing
  mapping. Keep semantic contracts separate from transport and implementation.
- HACE identifies and registers capabilities with provenance; HMCP defines shared
  contracts; AIXR coordinates execution and recovery. These are three logical
  responsibilities, not a requirement for three executables or services.
- Go, C++, Java, JavaScript/TypeScript, Python and C#/.NET have candidate roles.
  Do not mandate six runtimes or select the principal runtime without an ADR and
  a scoped experiment. AIXR is a subsystem concept, not a new project identity.
- Begin in user space. Do not introduce drivers, kernel changes, background
  services or system-wide installations without a demonstrated milestone need.
- Separate provider credentials, client identity and scoped framework tokens.
  Connection or authentication does not grant unrestricted execution authority.
  Never put credentials in tracked files, logs, examples or execution records.
- SQLite is the standalone persistence direction; SQL Server is an optional
  server direction. Do not install a database server for architecture work.

## Evidence and documentation

Use the labels VISION, PROPOSED ARCHITECTURE, ENGINEERING DECISION,
EXPERIMENTAL HYPOTHESIS, IMPLEMENTED COMPONENT and VERIFIED CAPABILITY consistently.
Accepted architectural constraints are not implementation claims. Record ADRs
as PROPOSED, ACCEPTED, EXPERIMENTAL or SUPERSEDED. Cite primary documentation for
external technology facts; distinguish research from locally tested behavior.

For later executable milestones, retain versioned contracts, exact inputs and
artifact hashes, process outcomes, verification criteria, failure receipts and
recovery behavior. Separate execution completion from verified outcome. Never
weaken fixed criteria or replay an uncertain non-idempotent operation automatically.
No directory boundary, timeout or credential API alone is an OS sandbox.
Use [the canonical evidence model](docs/architecture/INDEPENDENT-VERIFICATION.md)
for proposed V0–V5 levels. V4 needs independent observations meeting predetermined
criteria; V5 requires a separate reproduction of V4. Hashes do not prove correctness.

Keep local environment reports, caches, secrets, model exchanges and generated
artifacts outside tracked source. Deliberately selected aggregate validation
results belong in project status and milestone reports. Documentation examples
must say when they are conceptual and must contain no real credentials.

## Layout and milestone discipline

Root documents introduce the project and current milestone. docs/vision preserves
intent; architecture, protocols, security, persistence and languages hold detailed
designs; integrations contains optional boundaries; research records alternatives;
decisions contains ADRs; genesis contains scope, traceability and acceptance.
Top-level core/client/server/sdk/adapters/protocols/database/tests/examples/scripts
directories describe future ownership rather than imply implemented modules.

Choose the smallest measurable step in ROADMAP.md. The preferred Genesis 2 proof
is [LibreOffice DOCX-to-PDF conversion](docs/genesis/GENESIS-2-EXPERIMENT.md),
independent PDF verification and local SQLite evidence. A synthetic process check
may prepare that experiment but cannot replace it. Do not implement it during
Genesis 1. Update status, contracts, evidence and roadmap together. Prefer explicit
argument arrays, bounded execution and reproducible PowerShell 7 commands. Run
scripts/validate-docs.ps1 for documentation integrity; it is not a runtime test.
Do not add dummy implementations merely to populate the architecture.

Preserve repository history and stage only scoped changes. Publication authority
is task-specific: the founding mission authorizes creation and publication of the
Genesis foundation; it is not standing permission for future releases or changes
to other repositories. Do not invent a license grant or production-readiness claim.
