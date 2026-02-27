# ADR-0001: V1 Technology Stack (.NET 8 + C# + WPF)

## Status

Accepted

## Date

2026-02-26

## Context

V1 is explicitly Windows-only and depends on Win32 APIs for visible top-level window enumeration, graceful close (`WM_CLOSE`), process launch, monitor/work-area queries, and window positioning/z-order replay.

The architecture defines clear module boundaries (`WinApi`, `Domain`, `Persistence`, `WorkspaceEngine`, `AppOrchestrator`, `UI`) and benefits from strong typing, testable interfaces, and low-friction Windows interop.

## Decision

Use the following reference implementation stack for V1:

- .NET 8
- C#
- WPF for the desktop UI

## Consequences

### Positive

- Strong fit for Windows and Win32 interop requirements.
- Stable and mature tooling for desktop development.
- Good alignment with the architecture's interface-driven layering.
- Fast path to V1 delivery with predictable deployment/runtime behavior.

### Negative

- Windows-specific UI stack reduces portability.
- WPF is mature but not the newest Microsoft desktop UI framework.
- A future move to another UI framework would require migration effort.

## Alternatives Considered

1. .NET 8 + C# + WinUI 3
2. .NET 8 + C# + Avalonia
3. Electron/Tauri-style web UI shells

## References

- `docs/spec/spec.md` (Section 0.1)
- `docs/architecture/modules.md` (Design Constraints)
