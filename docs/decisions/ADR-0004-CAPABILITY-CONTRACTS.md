# ADR-0004: Common capability contracts

Status: **ACCEPTED** for common semantics; HMCP wire format is **PROPOSED**.
Date: 2026-10-22. Classification: **ENGINEERING DECISION**.

## Context

An installed executable, an agent tool and an application function describe
different things. Language-specific wrappers alone cannot establish consistent
authorization, identity, failure and verification semantics.

## Decision

Define the proposed Hybrid Machine Capability Protocol (HMCP) around discovery,
registration, versioned inputs/outputs, invocation, state, progress, cancellation,
errors, execution identity and verification. Separate semantic contracts from
framing and transport. Capability availability must include adapter provenance
and current evidence where available.

MCP is an optional agent-facing mapping. JSON-RPC, Protocol Buffers/gRPC, standard
streams, named pipes and WebSockets are candidates to evaluate, not a required
combined stack. HMCP is a project proposal, not an adopted industry standard or
an implemented API in Genesis 1.

## Consequences and alternatives

Adapters must translate application-specific behavior without hiding unsupported
operations. Descriptions may aid semantic discovery but cannot grant execution
authority. Pure ad hoc command strings are rejected as the universal contract;
reuse of an existing protocol remains preferable where it preserves requirements.

## Validation

Genesis 2 must demonstrate discovery and one invocation across two local processes,
including malformed input, unsupported version and typed failure. A mapping is
accepted only after tests show that identity, scope and verification survive it.
