# ADR-0002: Explicit Window State in V1 Capture/Restore

## Status

Accepted

## Date

2026-03-07

## Context

V1 currently treats window geometry as the primary restore signal.
In practice, minimized and maximized windows are stateful and cannot be modeled reliably by bounds alone.
This causes frustrating outcomes, especially for minimized windows that may reappear with off-screen/small placeholder geometry instead of a true minimized state.

## Decision

Explicit window state is part of V1.

- Capture and recapture must store window state for each captured window.
- Supported states in V1 are: `Normal`, `Minimized`, `Maximized`.
- Restore must replay state explicitly after bounds handling:
  - `Normal`: restore as normal window.
  - `Maximized`: maximize window.
  - `Minimized`: minimize window.

This decision updates V1 contracts in spec/architecture/milestones and is not deferred to backlog.

## Consequences

### Positive

- More deterministic and user-expected restore behavior.
- Minimization/maximization semantics are preserved across capture/restore cycles.
- Reduces cases where minimized windows become hard to recover due to placeholder geometry.

### Negative

- Adds scope to WinApi and Engine implementation.
- Requires schema/model changes and additional tests.
- Increases restore sequencing complexity (bounds + state replay interaction).

## Alternatives Considered

1. Keep V1 geometry-only and defer explicit state to backlog.
2. Infer state heuristically from bounds/position without explicit model.

## References

- `docs/spec/spec.md`
- `docs/architecture/modules.md`
- `docs/roadmap/milestones-v1.md`
- `docs/adr/0001-tech-stack-dotnet-wpf.md`
