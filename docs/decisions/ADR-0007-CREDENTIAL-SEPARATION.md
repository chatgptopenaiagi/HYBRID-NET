# ADR-0007: Separate credential and authority domains

Status: **ACCEPTED**. Date: 2026-10-22. Classification: **ENGINEERING DECISION**.

## Context

AI-provider API keys, client enrollment and framework invocation tokens serve
different purposes. Conflating them would make a connected agent or computer more
powerful than its intended scope.

## Decision

Keep three domains: provider credentials authenticate external model calls;
client identity credentials authenticate client/server peers; scoped HYBRID.NET
API tokens authorize selected framework operations. Successful authentication
does not replace per-capability, per-resource authorization or Windows permissions.

Investigate DPAPI and Windows Credential Manager behind an owned storage interface.
Per-client certificates, mutual TLS and rotation are proposed identity mechanisms.
Persistent records contain opaque references and redacted metadata, not secrets.
CAE is an optional architectural reference, never an initialization dependency or
an implicit credential donor.

## Consequences and alternatives

Credential lifecycles, revocation and identity mapping require separate tests.
User-scoped secret protection alone does not defend against every process running
as that user. A universal administrator token and embedding provider keys in
manifests are rejected. Concrete storage and enrollment mechanisms remain open.

## Validation

Later synthetic tests must reject token misuse across domains, expired/revoked
identity and unauthorized capability scopes. Diagnostics, database exports and
artifacts must exclude test secrets. No real credential access is needed for
Genesis 1 documentation validation.
