# Genesis 1 acceptance

Status: **ENGINEERING DECISION / fixed documentation acceptance criteria**.
These criteria preserve the owner's twenty founding checks. Results are recorded
separately in [GENESIS-1-REPORT.md](GENESIS-1-REPORT.md); an entry here is not itself
proof of completion.

| ID | Required outcome | Verification method and durable evidence |
| --- | --- | --- |
| G1-01 | Local Git repository exists | `git rev-parse --show-toplevel` resolves the intended checkout |
| G1-02 | Clear HYBRID.NET identity | README, VERSION, founding vision and ADR-0001 agree |
| G1-03 | Complete substantive founding architecture | Manual review of all 45 numbered mission topics and their traceability map |
| G1-04 | Dual executable concept preserved | Native shared core and third-party adapter limits in vision and ADR-0005 |
| G1-05 | AIXR concept preserved | Internal runtime role, boundary and non-implementation stated |
| G1-06 | Six language roles documented | Six language documents plus common worker contract boundaries |
| G1-07 | Standalone client documented | Server independence, offline deterministic operation, local authority/state |
| G1-08 | Distributed server documented | Enrollment, routing, identity, evidence and limits on remote authority |
| G1-09 | Hybrid operation documented | Local/remote selection and unknown-outcome reconciliation |
| G1-10 | HMCP contracts documented | Discovery/invocation/state/progress/cancel/error/verification/version semantics; conceptual JSON parses |
| G1-11 | Credentials and identity documented | Provider keys, client identity and scoped framework tokens remain separate |
| G1-12 | SQLite/SQL Server roles documented | Standalone/optional server modes and all eleven logical entities |
| G1-13 | Future integrations documented | CFA, SCE, QASC/CLARA, CAE, POK, Codex and possible application adapters; no mandatory dependency |
| G1-14 | Realistic roadmap | Genesis 1–9 have measurable exits, boundaries and a focused next step |
| G1-15 | Agent continuity | AGENTS.md gives first reads, scope, invariants, evidence vocabulary and milestone discipline |
| G1-16 | Accurate implementation status | No claim of an executable/runtime/adapter from documentation alone |
| G1-17 | Initial Git commit | `git log` and `git show` verify the reviewed foundation commit |
| G1-18 | Publish when access permits | Authenticated repository metadata, visibility and remote main SHA read back; otherwise exact blocker |
| G1-19 | Final Git status reported | Branch, HEAD, origin, upstream comparison and working-tree state recorded at handoff |
| G1-20 | No accidental private or unrelated files | Review staged file list/content; limited credential-pattern check; local evidence ignored |

## Validation boundaries

`scripts/validate-docs.ps1` checks required files via the traceability index, local
Markdown targets, balanced fenced blocks, JSON syntax and selected accidental-secret
patterns. It performs no external network or application calls. Manual review is
required for substantive fidelity, technical credibility and accurate status labels.
The checker is neither a comprehensive security scanner nor a runtime acceptance suite.

Record command, PowerShell version, counts and exit result in the milestone report.
Keep detailed machine-local command receipts under ignored `.local/`. Public evidence
should contain only deliberately selected repository/validation facts. Public
publication does not require a release tag or installer, and none is part of Genesis 1.
