# Windows interoperability

**Status: PROPOSED ARCHITECTURE with externally documented integration options.**
HYBRID.NET targets Windows 10/11 x64 initially; Genesis 1 has not certified a
runtime or adapter on either platform.

Prefer supported CLI commands, application scripting, native SDKs, COM, managed
APIs and documented IPC according to each application. COM defines binary
interfaces through which components can interact within or across processes; it
does not establish that any particular application exposes the desired function.
See Microsoft's [COM overview](https://learn.microsoft.com/en-us/windows/win32/com/the-component-object-model).

Named pipes are a candidate worker transport. They can communicate locally or
across a network, so a local-only design must deliberately configure and test
access restrictions; the transport name itself provides no locality guarantee.
Microsoft documents this distinction in
[Named Pipes](https://learn.microsoft.com/en-us/windows/win32/ipc/named-pipes).
Transport selection remains separate from HMCP semantics.

Adapters should declare executable or library identity, supported versions,
architecture, invocation method, thread/session requirements and permitted
resources. Explicit argument arrays, correctly resolved paths, bounded output and
timeouts are baseline execution controls. DLL loading and COM activation need
careful dependency/version handling. A working directory, filtered environment or
process timeout is not an operating-system sandbox.

Application state may be tied to an interactive user session. A future service
deployment must test that requirement rather than assuming a desktop automation
interface works unattended. Existing user applications and services must not be
reconfigured merely to discover capabilities.

The initial framework remains in user space. No drivers, kernel modification,
binary patching or universal internal-state inspection are prerequisites. Stronger
isolation, process-tree cleanup and restricted execution deserve measured tests
before claims are made. Graphical automation remains an explicit L0 fallback when
direct supported interfaces cannot meet the task.
