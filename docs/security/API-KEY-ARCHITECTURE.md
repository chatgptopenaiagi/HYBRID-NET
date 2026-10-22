# API credentials and authorization

**ENGINEERING DECISION — credential categories remain separate.** Concrete issuance,
storage and transport mechanisms are **PROPOSED ARCHITECTURE**, not implemented
security controls.

| Category | Authenticates or authorizes | Must not imply |
|---|---|---|
| A: AI provider credentials | Requests to a configured external AI provider | Permission to invoke Windows capabilities or enroll a client |
| B: Client identity credentials | A particular client communicating with an authorized server | Unrestricted remote administration or access to every local application |
| C: HYBRID.NET API tokens | An external agent or program invoking scoped framework operations | Possession of provider secrets, client private keys or administrator authority |

The proposed authorization decision combines authenticated principal, capability
version, resource scope, allowed effect, expiry and local policy. An export token
might permit reading one workspace and creating PDFs within its output directory;
it must not authorize arbitrary paths, commands, plugins or network destinations.
Resolved filesystem targets require validation at use time, including link/reparse
behavior, rather than trusting a textual path prefix. Token format and signing or
opaque-token storage are unresolved; a JWT is not required by this architecture.

No raw credential belongs in manifests, invocation bodies, URLs, progress events,
database rows or diagnostics. Store opaque references where needed. Receipts record
the authorization decision ID and policy version, not the bearer token. Redaction
must precede log persistence, including error paths and adapter stderr. Tests should
use synthetic credentials and verify denied scopes, expiry, revocation and log safety.

The optional local API proposal is `http://127.0.0.1:55001`, with protected operation
authentication and local authorization. Loopback restricts network reachability;
it does not authenticate a caller. No API listener or token issuer exists yet.
The server cannot override a client's local deny policy. Offline local permissions
must not depend on remote provider credentials or continuous server contact.

Provider keys should be released only to the selected provider adapter, for a
specifically authorized network request. Credential-provider integration remains
optional; [CAE](../integrations/CAE.md) is a design reference, not a dependency.
See [storage boundaries](CREDENTIAL-STORAGE.md) and [client enrollment](CLIENT-IDENTITY.md).
