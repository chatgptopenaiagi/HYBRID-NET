# Python engineering domain

**Status: PROPOSED ARCHITECTURE.** `Hybrid.Python.Worker` is a candidate for AI
provider adapters, local-model integration, application scripting, diagnostic
analysis, planning support, data transformations and test automation.

Python should complement deterministic execution components. A registered
conversion, calculation or state query must not require inference merely because
the invoking client is an AI agent. Model-assisted planning should produce a
structured proposal subject to the same authorization and verification as any
other request. Provider credentials and model exchanges remain outside ordinary
logs and committed fixtures.

The standard-library [subprocess API](https://docs.python.org/3/library/subprocess.html)
supports process invocation and capture of return codes and output. A future
worker still needs explicit argument handling, bounded capture, timeouts and
tested process-tree cleanup. Subprocess use is not an operating-system sandbox,
and process success does not prove the intended artifact is correct.

Adapters should declare interpreter and dependency constraints. Isolated package
environments are preferable to changing shared installations. The language role
does not mandate the newest Python release or establish compatibility with all
application scripting environments.

Future tests should cover structured diagnostics, a missing dependency, invalid
input, a timed-out subprocess and exact verification of a deterministic output.
Optional model integrations require separate live-model evidence; mocked model
responses cannot prove inference availability. A local-model outage must leave
ordinary deterministic capabilities available.

Open design questions include environment reuse, dependency provenance and which
diagnostic details can safely enter agent context. Resolve them with bounded
fixtures rather than unrestricted access to application state.

See [AIXR](../vision/AIXR-CONCEPT.md).
