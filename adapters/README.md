# Adapter ownership boundary

Status: **PROPOSED ARCHITECTURE; no implemented application adapters**.

An adapter will translate a supported application interface into explicit HMCP
capabilities, declare side effects and resource scopes, observe supported state,
and provide evidence for verification. It cannot promise functionality simply
because an executable is installed or an integration level is assigned.

The [reference target catalog](REFERENCE-TARGETS.md) preserves candidate applications
and limits. A later LibreOffice proof may use documented UNO interfaces, but no
application installation or modification is part of Genesis 1. Each future adapter
needs current dependency preflight, fixed fixtures, independent output checks,
failure/cancel behavior and version-qualified evidence. Graphical fallback must be
explicitly labeled. Adapter availability and authorization are separate questions;
read [capability design](../docs/architecture/CAPABILITY-ENGINE.md) first.
