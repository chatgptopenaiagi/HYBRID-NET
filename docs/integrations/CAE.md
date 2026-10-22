# CODEX-API-ENGINE reference boundary

Status: **PROPOSED ARCHITECTURE**. CODEX-API-ENGINE (CAE) is a possible source of
architectural ideas for credential and API integration on Windows. It is not a
Genesis 1 dependency, credential source or implemented authentication backend.

HYBRID.NET must define its own separation between AI-provider credentials, client
identity credentials and scoped framework API tokens. A future CAE connector would
require an explicit supported interface, independent authorization and tests that
prove which identity is being used for which operation. Sharing a machine or owner
does not permit borrowing another application's credentials.

The Windows platform offers candidate building blocks independently of CAE:
[DPAPI's CryptProtectData](https://learn.microsoft.com/en-us/windows/win32/api/dpapi/nf-dpapi-cryptprotectdata)
protects data using Windows-associated context, and the
[Credential Management API](https://learn.microsoft.com/en-us/windows/win32/secauthn/credentials-management)
provides credential-management facilities. These primitives do not themselves
implement capability authorization or isolate mutually untrusted code running as
the same user.

The first credential experiment belongs to a later milestone and must use unique
synthetic values and HYBRID.NET-owned targets. Test store/read/delete behavior,
redaction, denied access, lifecycle and recovery without enumerating unrelated
secrets. A database should retain opaque credential references, not secret values.

No CAE file, broker, process, token store, authentication session or installation is
read for secrets or changed by Genesis 1. If an optional provider is absent,
HYBRID.NET should report that status explicitly and continue operations that do
not need it. Further choices belong in the credential-storage design and an ADR.
