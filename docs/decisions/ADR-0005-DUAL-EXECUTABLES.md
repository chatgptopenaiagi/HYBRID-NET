# ADR-0005: Human and AI executable interfaces

Status: **ACCEPTED** as the native-application design direction.
Date: 2026-10-22. Classification: **ENGINEERING DECISION**.

## Context

Human-oriented interfaces are valuable for interactive decisions but can obscure
machine-readable operations and state. A companion executable can expose a
structured gateway when application integration is supported.

## Decision

For new HYBRID.NET-native applications, prefer an application core shared by a
human executable, `Application.exe`, and machine gateway,
`Application.AI.exe`. Both interfaces use the same domain rules and coordinate
shared state. The AI executable exposes capabilities, state and receipts rather
than duplicating the graphical interface.

For existing software, a companion adapter can use only supported CLI, scripting,
SDK, COM, API or other documented interfaces. It cannot obtain arbitrary internal
functionality merely by sharing an application name. Graphical automation remains
an explicitly identified fallback with its own evidence limitations.

## Consequences and alternatives

State versioning and conflict handling are needed when human and machine commands
overlap. The two executables may share a library or communicate with an owning
engine process; packaging topology remains proposed. An embedded local API may
serve some products, provided it preserves the same authority boundaries.

## Validation

A future native fixture must demonstrate equivalent engine results through both
interfaces and reject stale-state mutations. Third-party adapters require their
own interface and output tests before any capability is advertised as verified.
