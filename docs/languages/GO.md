# Go engineering domain

**Status: PROPOSED ARCHITECTURE.** `Hybrid.Go.Runtime` is a candidate component
name. No Go runtime implementation or Go toolchain requirement exists in Genesis 1.

Go is a candidate for process orchestration, concurrent work scheduling, IPC and
network communication, worker lifecycle management, protocol handling, health
monitoring and service discovery. Its language documentation describes goroutines
and channels as concurrency mechanisms; those mechanisms do not themselves solve
distributed coordination or durable recovery.
[Effective Go](https://go.dev/doc/effective_go#concurrency) is the primary reference
for that language feature, not a HYBRID.NET benchmark.

The candidate coordinator should manage deadlines, admission limits and worker
receipts without requiring all application computation to run in Go. Specialized
capabilities may execute directly in an appropriate worker. HMCP identity and
error semantics should survive across that boundary.

A future comparison should measure startup time, memory, cancellation behavior,
Windows packaging and implementation complexity against a .NET or other suitable
host. Reuse of existing supported libraries, debugging experience and deployment
requirements matter alongside throughput. The principal-runtime choice remains
open until that evidence is recorded in an ADR.

An initial Go experiment should exchange one bounded capability request with a
separate process, distinguish timeout from worker failure, and preserve malformed
response evidence. Later scheduling tests should demonstrate backpressure and
that a disconnected worker does not cause an uncertain side effect to replay.
Nothing in this language role grants network exposure or administrative authority.

See [multilanguage runtime](../architecture/MULTILANGUAGE-RUNTIME.md).
