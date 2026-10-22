# Multilanguage runtime

**Status: PROPOSED ARCHITECTURE.** Six language ecosystems have founding roles;
Genesis 1 installs or implements none. A client need not carry all six runtimes.

| Ecosystem | Candidate component | Proposed responsibility |
| --- | --- | --- |
| [Go](../languages/GO.md) | Hybrid.Go.Runtime | Coordination, communication, worker lifecycle |
| [C++](../languages/CPP.md) | Hybrid.Cpp.Bridge | Native SDKs, Windows APIs, specialized processing |
| [Java](../languages/JAVA.md) | Hybrid.Java.Worker | JVM and enterprise application integrations |
| [JavaScript / TypeScript](../languages/JAVASCRIPT.md) | Hybrid.Studio | Human interfaces, browser clients, developer tools |
| [Python](../languages/PYTHON.md) | Hybrid.Python.Worker | AI adapters, scripting, diagnostics, transformations |
| [C# / .NET](../languages/DOTNET.md) | Hybrid.DotNet.Bridge | Managed APIs, COM, services and credential integration |

HMCP should define behavior independently of these implementations. Every worker
needs compatible capability metadata, input/output validation, execution identity,
deadlines, errors, cancellation and verification references. Language-specific
exceptions must map to a stable error envelope while retaining bounded diagnostic
details.

Separate processes are a promising initial boundary because they allow independent
runtime selection and failure reporting. This is not a sandbox claim. In-process
libraries or native bridges remain alternatives when measured integration needs
justify ABI, memory ownership and lifecycle complexity. No decision yet requires
every operation to pass through Go or a single language host.

Conformance fixtures should test the same request across workers, including Unicode
paths, null versus omitted fields, numeric precision, binary artifact references,
unknown versions, deadline expiry and invalid inputs. Protocol framing and output
limits must prevent ordinary worker logging from corrupting machine messages.

Genesis 2 should establish a small contract with two local processes. Genesis 3
can add languages incrementally using the same fixtures. Supported interpreter,
compiler and OS versions must be based on executed compatibility tests, not the
existence of a language document or compiler installation.
