# Java engineering domain

**Status: PROPOSED ARCHITECTURE.** `Hybrid.Java.Worker` is a candidate JVM worker.
Genesis 1 installs no Java runtime and provides no enterprise connector.

The worker would expose supported Java libraries, JVM application interfaces and
enterprise software operations through the same HMCP contracts as other workers.
A client without Java capabilities should not need a JVM merely to run local
HYBRID.NET operations implemented elsewhere.

Prefer documented Java APIs behind a process-level protocol when they meet the
task. Native interoperation is an additional option, not the default architecture.
Oracle's [JNI introduction](https://docs.oracle.com/en/java/javase/25/docs/specs/jni/intro.html)
describes Java/native interoperability and points to the Foreign Function & Memory
API for applicable cases. Choosing either requires the target library's supported
JDK, ABI and lifecycle requirements; Genesis 1 selects neither.

The future adapter manifest should state Java version constraints, application or
library versions, required configuration and available operations. Authentication
to an enterprise application remains separate from framework invocation authority.
Do not log application credentials or serialize arbitrary internal Java objects
into execution receipts.

Conformance tests should cover class-loading failures, dependency mismatch, a
controlled exception, output encoding and deadline handling. Cross-platform worker
portability is a long-term possibility, not a verified Genesis capability. Each
claimed operating-system/JDK combination requires its own execution evidence.

Open questions include process-per-job versus a persistent worker, isolation of
conflicting library versions and resource cleanup after cancellation. These should
be resolved against a concrete integration rather than a hypothetical universal
JVM host.

See [multilanguage runtime](../architecture/MULTILANGUAGE-RUNTIME.md).
