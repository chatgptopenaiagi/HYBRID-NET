# Windows credential storage

**PROPOSED ARCHITECTURE.** Evaluate Windows DPAPI and Credential Manager behind a
small credential-provider interface. Genesis 1 reads or modifies no credential
store and implements no vault. A consumer requests an authorized secret by opaque
reference; ordinary configuration and SQL records retain only that reference and
non-secret metadata such as purpose and rotation time.

DPAPI normally ties decryption to the same logon credentials and computer, with
documented exceptions such as roaming profiles. Machine-scoped protection permits
users on that computer to decrypt; it must not be described as per-user isolation.
User-scoped DPAPI is therefore a candidate for an interactive client, while service
identities and machine scope need separate threat analysis and filesystem ACLs.
[Microsoft CryptProtectData documentation](https://learn.microsoft.com/en-us/windows/win32/api/dpapi/nf-dpapi-cryptprotectdata).

Credential Manager's `CredWrite` associates a credential with the current token's
logon session and can replace an existing entry with the same target/type. An
implementation must use a HYBRID.NET-specific namespace and avoid modifying another
application's targets. Its behavior under interactive users, services and recovery
must be tested before selecting it as the default provider.
[Microsoft CredWrite documentation](https://learn.microsoft.com/en-us/windows/win32/api/wincred/nf-wincred-credwritew).

Neither API prevents all same-user malicious code, privileged compromise or capture
of plaintext after legitimate retrieval. Storage protection is not an OS sandbox
and is not capability authorization. Adapters should receive the minimum required
credential only at dispatch, never through command-line arguments, ordinary model
prompts or general worker environment inheritance. Avoid retaining plaintext longer
than needed; do not promise complete memory erasure in every language runtime.

The provider contract should support create, resolve, rotate, revoke and delete with
explicit target ownership and safe error codes. Backup and recovery must state which
identity can decrypt restored material; copying encrypted files is not a portability
guarantee. Tests require synthetic values, no secret-bearing logs and preservation
of unrelated credential entries.

[CAE](../integrations/CAE.md) may inform later design review, but its repository,
credentials and running services remain independent. HYBRID.NET initialization,
documentation checks and future basic tests must not require CAE.
