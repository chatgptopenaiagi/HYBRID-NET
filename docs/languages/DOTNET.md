# C# and .NET engineering domain

**Status: PROPOSED ARCHITECTURE.** `Hybrid.DotNet.Bridge` is a candidate managed
Windows bridge. HYBRID.NET is a distinct project, not an official Microsoft
product, .NET distribution or claim of Microsoft endorsement.

Candidate responsibilities include managed application APIs, COM integration,
Windows services, process lifecycle, native-library interoperation, structured
enterprise interfaces and access to operating-system credential protection.
Microsoft documents .NET's [native interoperability options](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/),
including P/Invoke and COM-related scenarios. Each target still needs a supported
application interface and specific compatibility evidence.

The bridge should translate a narrow HMCP operation into documented managed or
native calls. It should retain framework execution identity, resource scope,
deadlines and verification references. Being a managed component does not make an
operation safe, reversible or authorized by default.

The principal local host could also be implemented in .NET; Go is not an imposed
intermediate layer. Compare deployment options, Windows integration needs,
maintenance, startup cost and process communication before settling that decision.
No Windows service should be installed merely to host an early prototype.

Future tests should cover absent COM registration, unsupported application version,
architecture mismatch, permission denial and cancellation limitations. Credential
integration tests should use synthetic test identities and must never expose
existing user secrets. Interactive desktop dependencies require explicit tests
before service deployment is claimed.

Runtime selection, supported .NET versions, packaging strategy and worker lifetime
remain open. Genesis 1 records responsibilities without installing an SDK or
producing `Hybrid.DotNet.Bridge` binaries.

See [Windows interoperability](../architecture/WINDOWS-INTEROPERABILITY.md) and
[credential storage](../security/CREDENTIAL-STORAGE.md).
