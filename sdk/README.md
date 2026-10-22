# SDK ownership boundary

Status: **PROPOSED ARCHITECTURE; no published SDK**.

Future SDKs will encode/decode common HMCP contracts, expose typed errors and
verification states, and help adapter authors preserve identity and cancellation
semantics. They must not hide a transport failure as success or grant capability
authorization by constructing a client object. Generated bindings may be useful
after the schema and compatibility policy are selected.

Go, C++, Java, JavaScript/TypeScript, Python and .NET bindings should use common
conformance fixtures. The first pair is a Genesis 2 experiment; six simultaneous
installations are not prerequisites. A new SDK must document integer/string/path
semantics, version negotiation, resource limits and unsupported features before
interoperability is claimed. See [multilanguage design](../docs/architecture/MULTILANGUAGE-RUNTIME.md)
and [protocol options](../docs/protocols/IPC-OPTIONS.md).
