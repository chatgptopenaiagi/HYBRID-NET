# QASC and CLARA

Status: **PROPOSED ARCHITECTURE**. Genesis 1 defines an optional integration boundary;
it imports no QASC code, private model exchanges or workstation evidence.

QWEN AUTONOMOUS SOFTWARE CREATOR (QASC) is the proposed consumer coordinating
model-assisted source generation, testing and delivery. CLARA supplies a repair
workflow: collect failure evidence, form a grounded hypothesis, propose a source
change, verify an isolated candidate and preserve correction history. These are
the founder's assigned roles; HYBRID.NET claims no implemented connector.

HYBRID.NET could expose deterministic execution capabilities and return typed
failure and verification receipts. QASC/CLARA would decide whether to request a
repair; an execution adapter should not make ordinary operations depend on model
inference. A future failure envelope should carry execution identity, capability
version, source/artifact fingerprint, stage, exit status, bounded diagnostic data
and references to retained evidence. It must label heuristic diagnoses separately
from observed facts.

Repair proposals require their own authorization and isolated validation. A model
response cannot edit independent acceptance criteria, promote itself or replace
the last verified revision. The consumer must recheck the source fingerprint
between diagnosis, candidate verification and delivery. Failed candidates and
unchanged proposals remain evidence, subject to retention policy.

Begin integration with synthetic success, assertion failure, timeout and stale
fingerprint fixtures. Then inspect QASC's current receipt and contract versions
before implementing a mapping. Missing QASC, CLARA or local Qwen must leave
HYBRID.NET's deterministic local capabilities available. Existing projects,
models and inference servers remain independently owned and unchanged.
