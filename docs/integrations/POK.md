# Poker-Odds-Knowledge reference application

Status: **PROPOSED ARCHITECTURE**. POK is a possible demonstration target; no POK
interface was audited or implemented as part of HYBRID.NET Genesis 1.

POKER-ODDS-KNOWLEDGE could illustrate an application with a deterministic engine,
human presentation and an AI-readable state surface. A native integration would
expose validated operations such as evaluating a specified game state and
retrieving permitted state fields. The same underlying engine should govern both
human and machine interfaces so that rules and invariants do not diverge.

The prospective contract must distinguish public state, private state, derived
results and authorized mutations. Access to a process must not imply access to
hidden player information. Explicit game variant, rules version, input cards,
seed where relevant and calculation method would make results reproducible.
Any random simulation should report its actual method and uncertainty rather
than masquerade as an exact calculation.

A later experiment should use disposable, entirely synthetic game states, compare
known deterministic results, reject impossible inputs and verify stale-state
preconditions. It should demonstrate discovery, invocation, state observation and
receipt retrieval without screenshot interpretation. Existing software may need
a supported adapter; naming `Application.AI.exe` does not create one.

POK remains a separate project. Its availability, API and license terms must be
inspected at the future integration milestone. No repository relocation, source
import, user-data access or modification is authorized by this conceptual design.
