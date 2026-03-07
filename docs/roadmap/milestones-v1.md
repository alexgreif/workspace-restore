
# Workspace App — V1 Milestones

This document defines the implementation milestones for V1.
It is aligned with the V1 Specification and V1 Architecture documents.

Guiding principle:
V1 must validate the core loop with deterministic behavior:

Capture → Restore (global close, abort-on-timeout) → Deterministic layout recreation

------------------------------------------------------------------------

## Conventions

- Each milestone produces a demonstrable, testable deliverable.
- Milestones are ordered to de-risk the OS-integration core early.
- UI is intentionally late: V1 is OS-orchestration-first.
- “Done” means: works reliably on the developer machine for repeated runs, not just once.

------------------------------------------------------------------------

# Milestone 0 — Repository & Solution Baseline

## Goal
Establish repository structure, solution layout, and documentation placement.

## Deliverables
- Git repository initialized
- Folder structure created:
  - `src/` for code
  - `docs/spec`, `docs/architecture`, `docs/backlog`, `docs/roadmap`
- .NET solution created (WorkspaceApp.sln)
- Projects created per architecture layers:
  - WorkspaceApp.Domain
  - WorkspaceApp.Persistence
  - WorkspaceApp.Engine
  - WorkspaceApp.WinApi
  - WorkspaceApp.App
  - WorkspaceApp.UI (WPF)
- Project references wired to enforce dependency direction
- README and LICENSE committed

## Definition of Done
- Solution builds successfully from clean checkout
- No circular dependencies between projects
- Documentation is committed under `docs/`

------------------------------------------------------------------------

# Milestone 1 — Domain Model (Layer 1)

## Goal
Implement the V1 domain model with validation and stable serialization shape.

## Deliverables
- Domain types implemented:
  - Workspace, ApplicationEntry, WindowLayout, OperationResult (+ events/errors)
  - Rect and supporting enums/identifiers
- Invariants/validation utilities (lightweight)
- Stable schema version field included in Workspace model

## Definition of Done
- Domain compiles with no dependencies on other layers
- Domain objects can be instantiated and validated from a small test harness

------------------------------------------------------------------------

# Milestone 2 — Persistence (Layer 2)

## Goal
Support saving and loading workspaces as one JSON file per workspace.

## Deliverables
- `IWorkspaceRepository` implementation
  - Create / Get / Update / Delete / List
- Storage layout defined and standardized to `%LocalAppData%\WorkspaceApp\workspaces\`
- Schema versioning support via `ISchemaMigrator` (V1 passthrough for now)

## Definition of Done
- Can create a workspace object in code → persist → reload → identical result
- One workspace equals one JSON file
- List returns correct summaries
- Delete removes file
- Workspace files are written under `%LocalAppData%\WorkspaceApp\workspaces\`

------------------------------------------------------------------------

# Milestone 3 — WinApi: Window Enumeration (Layer 0)

## Goal
Reliably enumerate included windows (V1 window definition) and extract metadata.

## Deliverables
- `IWindowEnumerator` implementation:
  - EnumWindows-based enumeration of top-level windows
  - Filters applied:
    - Visible
    - Not cloaked
    - Exclude shell infrastructure (taskbar/desktop host)
  - Extract:
    - hwnd, pid, bounds, title/class, exePath (best-effort), window state, z-order rank
- Small internal debug tool/harness (console or internal command) that prints snapshot

## Definition of Done
- Snapshot returns expected windows and excludes taskbar/desktop
- Bounds match what is visible on screen
- exePath resolves for common apps
- Z-order rank is consistent enough to derive zOrderIndex

------------------------------------------------------------------------

# Milestone 4 — WorkspaceEngine: Capture (create-only)

## Goal
Capture current visible windows into a new Workspace (grouped by exePath), persist it.

## Deliverables
- `ICaptureService` implementation:
  - Enumerate windows
  - Group by exePath into ApplicationEntry
  - Record WindowLayouts per window (including `Normal`/`Minimized`/`Maximized` state)
  - Compute zOrderIndex consistently
- Wiring into repository (create)
- Debug command to capture into a named workspace

## Definition of Done
- After capture, JSON reflects current visible layout
- Multiple windows of same executable create one entry with multiple WindowLayouts
- Captured WindowLayouts include explicit window state
- Z-order indices are stored consistently

------------------------------------------------------------------------

# Milestone 5 — WorkspaceEngine: Create from Scratch (empty workspace)

## Goal
Support explicit creation of an empty named workspace (no captured windows).

## Deliverables
- App-level command/use case to create a workspace with:
  - Name
  - Metadata (`Id`, `CreatedAtUtc`, `UpdatedAtUtc`, `SchemaVersion`)
  - Empty `Entries` list
- Persistence wiring for create-from-scratch path
- Validation for duplicate/invalid names aligned with app rules

## Definition of Done
- User/developer can create an empty workspace without running capture
- Created workspace persists as one valid JSON document with zero entries
- Empty workspace appears in workspace list and can be deleted

------------------------------------------------------------------------

# Milestone 6 — WinApi: Graceful Close Engine (Global Close)

## Goal
Implement sequential WM_CLOSE with abort-on-timeout behavior.

## Deliverables
- `IWindowCloser` implementation (WM_CLOSE)
- `IRestoreService` global-close phase implemented:
  - Enumerate included windows
  - Close sequentially
  - Wait for each to disappear
  - Abort on any timeout/refusal
- Progress events emitted for close attempts and outcomes

## Definition of Done
- Closing happens one-by-one
- Restore aborts if any window remains after timeout
- Common apps close successfully when no unsaved work exists
- Logs/progress clearly show which window caused abort

------------------------------------------------------------------------

# Milestone 7 — WinApi: Launch + Window Matching

## Goal
Launch executables and match resulting visible windows to apply layouts (best-effort up to N).

## Deliverables
- `IProcessLauncher` implementation
- Window matching logic implemented in RestoreService:
  - Launch exePath
  - Poll for visible windows belonging to launched PID
  - Select up to N windows (N = captured WindowLayouts count)

## Definition of Done
- For single-window apps, restore consistently finds and positions the window
- For multi-window apps, restore positions up to N windows that appear (best-effort)
- Failures/timeouts are recorded as OperationErrors

------------------------------------------------------------------------

# Milestone 8 — WinApi: Window Movement, Monitor Clamp, and State Replay

## Goal
Apply bounds deterministically, replay explicit window state, and handle off-screen behavior per V1 spec.

## Deliverables
- `IWindowMover` implementation (SetWindowPos + Minimize + Restore + Maximize + Activate)
- `IMonitorService` implementation:
  - Work area detection
  - Clamp logic
  - Fully vs partially off-screen detection
- Restore layout phase:
  - Apply bounds
  - Clamp if partially off-screen
  - Clamp + minimize if fully off-screen
  - Replay captured explicit state (`Normal`/`Minimized`/`Maximized`)

## Definition of Done
- Restored windows appear where expected on stable monitor topology
- When topology changes, off-screen windows are clamped correctly
- Fully off-screen windows end minimized (after clamp)
- Captured minimized/maximized windows replay their explicit state correctly
- Behavior matches spec precisely

------------------------------------------------------------------------

# Milestone 9 — Z-Order Capture & Replay

## Goal
Restore stacking order so overlaps match the captured workspace.

## Deliverables
- Consistent derivation of zOrderIndex during capture
- Replay logic during restore:
  - After positioning all windows, apply z-order via Activate in recorded order
- Optional safety handling if some windows could not be matched

## Definition of Done
- Overlapping windows restore in the same stacking order as captured (in typical cases)
- If some windows are missing, remaining windows still replay order deterministically

------------------------------------------------------------------------

# Milestone 10 — Recapture (update-only, overwrite)

## Goal
Overwrite an existing workspace based on current visible windows.

## Deliverables
- `IRecaptureService` implementation:
  - Enumerate current windows
  - Overwrite ApplicationEntries and WindowLayouts
  - Persist update
- Debug command to recapture an existing workspace

## Definition of Done
- Workspace content exactly matches current visible windows after recapture
- Entries/layouts not currently visible are removed

------------------------------------------------------------------------

# Milestone 11 — Minimal WPF UI (V1)

## Goal
Provide a minimal user interface to operate V1 end-to-end.

## Deliverables
- Workspace list (ListWorkspaces)
- Actions:
  - Capture (create-only)
  - Create from Scratch (empty workspace)
  - Recapture (overwrite)
  - Restore
  - Delete
- Progress UI driven by progress events
- Basic error reporting from OperationResult

## Definition of Done
- A non-technical user can create and restore workspaces
- Restore shows progress and clearly reports abort reason when applicable
- UI does not call WinApi directly; uses AppOrchestrator only

------------------------------------------------------------------------

# Milestone 12 — Hardening Pass (V1 Stabilization)

## Goal
Make the core loop reliable across repeated usage and common edge cases.

## Deliverables
- Robust logging (file or structured logs)
- Improved window matching heuristics for common apps (still generic, not “native app support”)
- Retry/timing tuning (centralized constants)
- Better error codes and messages
- Regression checklist for repeated restore cycles

## Definition of Done
- 10+ repeated capture/restore cycles succeed without layout drift (typical apps)
- Restore consistently aborts on refusal and reports cause
- No crashes under typical usage
- Clear logs for debugging

------------------------------------------------------------------------

# V1 Definition of Done (Project-Level)

V1 is considered complete when:

- Create from Scratch (empty workspace) works
- Capture (create-only) works
- Recapture (overwrite) works
- Restore performs:
  - sequential global close (WM_CLOSE)
  - abort on timeout/refusal
  - launch executables
  - best-effort positioning of up to N windows per entry
  - explicit window state replay (`Normal`/`Minimized`/`Maximized`)
  - clamp + minimize for fully off-screen windows
  - z-order replay
- One JSON file per workspace persistence is stable
- Minimal WPF UI supports core operations
- Hardening checklist passes for typical day-to-day workflows

------------------------------------------------------------------------

End of Milestones (V1)
