# Standalone client

**Status: PROPOSED ARCHITECTURE; server independence is an accepted founding constraint.**
`Hybrid.Client.exe` and `HYBRID-NET-CLIENT-SETUP.exe` are proposed delivery names.
Neither exists in Genesis 1.

The client owns local capability registration, application inventory, execution,
verification, diagnostics, identity configuration, credential access and SQLite
records. Studio and authorized agents address this local authority. A central
server must not be required for initialization, local history access, policy
evaluation or deterministic application operations.

An optional API could initially listen on `127.0.0.1:55001`. This address and a
possible `POST /api/v1/execute` route are conceptual, not live endpoints or settled
contracts. Protected operations need authentication and capability/resource
authorization even on loopback. A browser-accessible design must also address
origin validation and unwanted cross-origin requests. External network exposure
would require a separate explicit deployment decision.

The execution path is local request validation, fresh capability preflight, policy
evaluation, bounded worker execution, independent verification and durable receipt.
Local models may support offline planning when already installed and compatible;
deterministic operations must remain possible without a model. A remote AI
provider is optional and requires its own connectivity and provider credential.

Future acceptance must start and exercise the client with no server configuration
and with network connectivity unavailable. Repeat after configuring an unreachable
server, and during loss of a previously reachable server. Local discovery and
execution must remain usable; remote work must report unavailable or uncertain
status accurately. These are planned tests, not Windows 10/11 compatibility claims.

The client must expose human-readable and machine-readable failure information
without persisting secrets. Its private files and credential stores are separate
from source repositories and downloadable evidence bundles.

See [hybrid mode](HYBRID-MODE.md) and [credential storage](../security/CREDENTIAL-STORAGE.md).
