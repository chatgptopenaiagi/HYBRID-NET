# Structured application state

**Status: VISION and PROPOSED ARCHITECTURE.** State introspection is available only
where a future native interface or supported adapter can observe it.

Useful state can include application/process identity, current document, active
project, loaded files, available commands, current operation, progress, structured
errors and relevant events. Each field needs a defined source. An inaccessible
field must be absent or marked unknown, never inferred as a verified fact from a
plausible model response.

Proposed snapshots carry an application instance identifier, state-schema version,
monotonic revision within that instance, observation timestamp, observation method
and freshness indicator. They should identify whether the data is a consistent
snapshot or a best-effort observation assembled from multiple calls. A process
restart establishes a new instance identity rather than reusing old revisions.

Mutating operations can include the paired expected application instance identity
and state revision. Both must match; a restarted process cannot reuse a prior
instance's observation just because its revision number matches. If a human or another
agent changes the document first, reject or reconcile the conflict according to
the capability contract. Do not overwrite an unsaved human change based on an old
snapshot. State access and content access may require different authorization;
metadata should not automatically reveal confidential document contents.

Event subscriptions need sequence numbers, bounded retention and a resnapshot
mechanism when events are lost. Progress is observational: a worker reporting
100 percent does not establish that verification succeeded. Cancellation requested,
execution completed and result verified remain distinct facts.

For native dual-executable applications, the shared core should own this state
model. Third-party adapters may offer only limited polling or documented events.
Future tests should cover concurrent human/agent edits, stale revisions, process
restart, unavailable fields and subscription gaps. Screenshot-based observations
must be labeled as such rather than advertised as native application state.

See [dual executables](../vision/DUAL-EXECUTABLE-CONCEPT.md).
