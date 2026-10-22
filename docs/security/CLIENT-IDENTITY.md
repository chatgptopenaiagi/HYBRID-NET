# Client identity and server enrollment

**PROPOSED ARCHITECTURE.** HYBRID.NET Client has an independent local identity and
policy store. Connecting to HYBRID.NET Server adds an authorized relationship; it
does not replace the local runtime or enroll the machine as an administrator target.
No enrollment, certificate authority or remote-execution service exists in Genesis 1.

A future enrollment flow should require explicit local authorization, authenticate
the intended server, use a short-lived enrollment credential, establish a stable
client identifier and bind it to a client-controlled key. Per-client certificates
with mutual TLS are a candidate. Token-based alternatives require the same binding,
rotation, revocation and replay analysis before selection. Do not share one identity
credential across all clients or reuse an AI provider key for enrollment.

After authentication, both parties still apply policy. The client decides which
capability versions and resources it advertises, which principals may request
operations, and which jobs need additional local approval. A server command carries
a scoped job request, not an unrestricted shell. A client should reject expired or
unrecognized delegated authority even if its connection is authenticated.

Rotation needs overlap rules that avoid losing acknowledged jobs or accepting a
retired key indefinitely. Revocation should stop new remote admissions according to
explicit freshness policy. While disconnected, the client continues independently
authorized local operations; it cannot claim knowledge of newly published remote
revocations. Expired remote authority must not become an offline bypass.

Reconnection exchanges durable job IDs and event cursors. A server's missing reply
does not authorize repeating a client's non-idempotent side effect. Reconcile
`unknown_outcome` using local receipts and supported application observation; if
still uncertain, report uncertainty and require an explicit recovery decision.

Future tests must cover wrong-server enrollment, stolen/expired enrollment material,
identity rotation during execution, revoked clients, disconnected local execution,
replayed remote requests and reconnect without duplicate work. Certificate lifecycle,
service-account installation, federation and machine recovery remain open design
questions. [HMCP semantics](../protocols/HMCP-CONCEPT.md) apply regardless of the
eventual authentication mechanism.
