# C++ engineering domain

**Status: PROPOSED ARCHITECTURE.** `Hybrid.Cpp.Bridge` is a candidate for native
application adapters and specialized execution, not an implemented component.

The proposed domain includes documented Windows APIs, vendor SDKs, DLL interfaces,
application engine bridges and processing where measurements justify native code.
COM is one possible integration mechanism: Microsoft's
[COM overview](https://learn.microsoft.com/en-us/windows/win32/com/the-component-object-model)
describes interfaces and a binary component model. Its existence does not imply
that an arbitrary application publishes the necessary objects or operations.

A native adapter should expose a small HMCP capability surface with explicit
input/output ownership. Native pointers, handles and process-local object identity
must not become portable protocol values. Use opaque, scoped identifiers with
defined lifetime and cleanup. Exceptions and native error codes need a stable
external classification plus bounded diagnostic detail.

An out-of-process bridge is a candidate for containing crashes within a worker;
it does not by itself restrict file, process or network privileges. An in-process
library may be useful for a supported SDK but introduces ABI, architecture,
allocator and lifetime coupling. Select that tradeoff per adapter, not framework-wide.

Future acceptance should validate x64 dependencies, Unicode paths, missing DLLs,
version mismatches, cleanup after cancellation and structured reporting of a
controlled worker failure. Performance claims require measured application cases.
Binary patching, undocumented introspection and speculative kernel work are
outside the initial native-bridge design.

See [Windows interoperability](../architecture/WINDOWS-INTEROPERABILITY.md).
