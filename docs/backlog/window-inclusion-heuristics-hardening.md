# Backlog --- Window Inclusion Heuristics Hardening (Post-V1)

## Title

Window Inclusion Heuristics Hardening for Capture/Recapture/Restore

## Status

Backlog (Post-V1 / V1.5 or V2)

## Motivation

V1 intentionally uses a simple, deterministic inclusion rule:
visible + not cloaked + not shell infrastructure.

Real desktop environments contain auxiliary and notification windows that satisfy those rules
but are not user-intended workspace windows (for example app helper bars, tray bridges,
driver OSD helper windows).

## Scope Candidates

- Expand shell infrastructure detection beyond baseline class-name checks.
- Add generic auxiliary-window exclusion heuristics (non-app-specific where possible).
- Re-evaluate handling of windows with placeholder/minimized geometry in capture lists.
- Consider opt-in diagnostic categories for why a window was included/excluded.

## Non-Goals

- App-specific hardcoded allow/deny lists for individual products in V1.
- Breaking the deterministic V1 inclusion contract retroactively.

## Architecture Notes

- Keep OS detection logic in `WinApi`.
- Keep inclusion policy decisions in `WorkspaceEngine` where possible/testable.
- Preserve strict layer boundaries.

## References

- `docs/spec/spec.md` (Window Inclusion Semantics)
- `docs/architecture/modules.md` (L0 window filter contract)
- `docs/roadmap/milestones-v1.md`
- `docs/adr/0002-window-state-in-v1-capture-restore.md`
