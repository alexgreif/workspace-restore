# Workspace App — V1 Architecture (Formalized Modules & Public Interfaces)

This document formalizes the V1 layered architecture into concrete modules and public interfaces.
It is Windows-only, visible-window-only, and implements global close restore with abort-on-refusal.

------------------------------------------------------------------------

## 0. Design Constraints (V1)

- Platform: Windows only
- Implementation stack: .NET 8 + C# + WPF
- Universe: visible, top-level desktop windows (EnumWindows), not cloaked
- Restore: global close (WM_CLOSE) sequentially, abort on timeout/refusal → launch → position → restore z-order
- Capture: create-only
- Recapture: update-only, overwrite
- Window count per executable: best-effort (position up to N captured windows)

------------------------------------------------------------------------

# 1. Module Overview (Layers)

## Layer 0 — `WinApi`
Direct Win32 integration. No workspace/business rules.

## Layer 1 — `Domain`
Pure data types and invariants.

## Layer 2 — `Persistence`
Workspace repository (one JSON file per workspace) + schema versioning.

## Layer 3 — `WorkspaceEngine`
Core use cases: Capture, Recapture, Restore.

## Layer 4 — `AppOrchestrator`
Operation coordination, cancellation, progress, logging. UI-facing façade.

## Layer 5 — `UI`
Minimal GUI using orchestrator; no Win32 calls directly.
V1 UI technology: WPF.

------------------------------------------------------------------------

# L0. WinApi — Public Interfaces

## L0.1 Window Enumeration

```csharp
interface IWindowEnumerator {
  List<WindowInfo> EnumerateTopLevelWindows(WindowFilter filter);
}
```

```csharp
class WindowFilter {
  bool RequireVisible = true;
  bool ExcludeCloaked = true;
  bool ExcludeShellInfrastructure = true;   // taskbar/desktop host
}
```

```csharp
class WindowInfo {
  IntPtr Hwnd;
  int ProcessId;
  string? Title;
  string? ClassName;
  Rect Bounds;               // virtual desktop coordinates
  bool IsVisible;
  bool IsCloaked;
  bool IsShellInfrastructure;
  string? ExePath;           // resolved from PID; may be null on failure
  long ZOrderRank;           // derived from EnumWindows order or explicit query; used to compute ZOrderIndex
}
```

## L0.2 Graceful Close

```csharp
interface IWindowCloser {
  // Sends WM_CLOSE. Does not force-kill.
  void RequestClose(IntPtr hwnd);
}
```

## L0.3 Window Positioning & State

```csharp
interface IWindowMover {
  void SetBounds(IntPtr hwnd, Rect bounds);
  void Minimize(IntPtr hwnd);
  void Restore(IntPtr hwnd);          // optional; used if needed to position
  void Activate(IntPtr hwnd);         // used for z-order replay
}
```

## L0.4 Process Launch

```csharp
interface IProcessLauncher {
  LaunchResult Launch(string exePath);
}

class LaunchResult {
  bool Success;
  int? ProcessId;
  string? Error;
}
```

## L0.5 Monitor Utilities

```csharp
interface IMonitorService {
  Rect GetVirtualDesktopBounds();
  List<MonitorInfo> GetMonitors();
  Rect GetNearestVisibleWorkArea(Rect target);
  bool IsFullyOffscreen(Rect target);
  bool IsPartiallyOffscreen(Rect target);
  Rect ClampToVisibleWorkArea(Rect target);
}

class MonitorInfo {
  int Index;
  Rect Bounds;
  Rect WorkArea;
}
```

------------------------------------------------------------------------

# L1. Domain — Public Types

> Note: Signatures are language-agnostic and written in C#-like pseudocode for clarity.

## L1.1 Identifiers

```csharp
type WorkspaceId = string;   // e.g., UUID
```

## L1.2 Core Models

```csharp
class Workspace {
  WorkspaceId Id;
  string Name;
  DateTime CreatedAtUtc;
  DateTime UpdatedAtUtc;
  List<ApplicationEntry> Entries;
  SchemaVersion SchemaVersion;  // persisted
}

class ApplicationEntry {
  string ExePath;                 // primary identity (normalized absolute path)
  List<WindowLayout> Windows;     // 1..n captured layouts for this executable
}

class WindowLayout {
  Rect Bounds;                    // virtual desktop coordinates
  int ZOrderIndex;                // relative stacking order (0 = bottom ... n-1 = top)
  MonitorHint? MonitorHint;       // best-effort metadata
  string? TitleHint;              // non-authoritative metadata
}

struct Rect { int X; int Y; int Width; int Height; }

class MonitorHint {
  int? MonitorIndex;              // optional future-facing hint (not used for binding in V1)
}
```

## L1.3 Operation Results

```csharp
enum OperationStatus { Success, PartialSuccess, Failed, Aborted }

class OperationResult {
  OperationStatus Status;
  List<OperationEvent> Events;
  List<OperationError> Errors;
}

class OperationEvent {
  DateTime TimestampUtc;
  string Code;        // e.g. "GLOBAL_CLOSE_BEGIN", "WINDOW_CLOSED", "APP_LAUNCHED", "WINDOW_POSITIONED"
  string Message;
  Map<string, string> Data;  // structured details (hwnd/pid/exePath/etc.)
}

class OperationError {
  DateTime TimestampUtc;
  string Code;        // e.g. "CLOSE_TIMEOUT", "LAUNCH_FAILED", "WINDOW_MATCH_TIMEOUT"
  string Message;
  Map<string, string> Data;
}
```

------------------------------------------------------------------------

# L2. Persistence — Public Interfaces

## L2.1 Repository

```csharp
interface IWorkspaceRepository {
  List<WorkspaceSummary> List();
  Workspace Get(WorkspaceId id);
  WorkspaceId Create(Workspace workspace);      // writes one JSON file
  void Update(Workspace workspace);             // overwrite file
  void Delete(WorkspaceId id);
}
```

```csharp
class WorkspaceSummary {
  WorkspaceId Id;
  string Name;
  DateTime UpdatedAtUtc;
}
```

## L2.2 Schema Versioning

```csharp
enum SchemaVersion { V1 = 1 }

interface ISchemaMigrator {
  Workspace MigrateToLatest(Workspace workspace);
  SchemaVersion LatestVersion();
}
```

------------------------------------------------------------------------

# L3. WorkspaceEngine — Public Interfaces

## L3.1 Capture (create-only)

```csharp
interface ICaptureService {
  Workspace CaptureFromCurrentState(string workspaceName, CaptureOptions options);
}

class CaptureOptions {
  WindowFilter Filter;                   // usually defaults to V1 window definition
}
```

Capture algorithm (normative):
- Enumerate included windows
- Resolve exePath per window
- Group by exePath
- Store WindowLayouts (bounds + titleHint + computed z-order index)

## L3.2 Recapture (update-only, overwrite)

```csharp
interface IRecaptureService {
  Workspace RecaptureFromCurrentState(WorkspaceId id, RecaptureOptions options);
}

class RecaptureOptions {
  WindowFilter Filter;
  bool Overwrite = true;                 // V1 locked: overwrite semantics
}
```

## L3.3 Restore

```csharp
interface IRestoreService {
  OperationResult RestoreWorkspace(WorkspaceId id, RestoreOptions options, IProgressSink progress, CancellationToken ct);
}

class RestoreOptions {
  WindowFilter Filter;

  // Close policy
  TimeSpan ClosePerWindowTimeout;        // V1: abort-on-timeout
  bool AbortOnCloseTimeout = true;       // V1 locked

  // Launch/match policy
  TimeSpan WindowMatchTimeoutPerEntry;
  int WindowMatchPollIntervalMs;

  // Off-screen policy (V1 locked)
  bool ClampOffscreen = true;
  bool MinimizeIfFullyOffscreen = true;

  // Z-order replay
  bool RestoreZOrder = true;
}
```

### Restore sub-steps (normative)

**A) Global Close**
- Enumerate included windows (using Filter)
- For each window (sequential):
  - RequestClose(hwnd)
  - Wait until hwnd no longer exists OR timeout
  - If timeout and AbortOnCloseTimeout: abort operation

**B) Launch**
- For each ApplicationEntry:
  - Launch exePath
  - Poll for visible windows for launched PID
  - Select up to N windows (N = captured layouts count)

**C) Layout**
- For each matched window:
  - If fully off-screen after applying target bounds:
    - Clamp bounds
    - Minimize window
  - Else if partially off-screen:
    - Clamp bounds only
  - Apply bounds
- After all positioned:
  - Replay z-order using Activate(hwnd) in recorded order

------------------------------------------------------------------------

# L4. AppOrchestrator — Public Interface

UI should call only this façade.

```csharp
interface IWorkspaceAppController {
  List<WorkspaceSummary> ListWorkspaces();
  WorkspaceId Capture(string name);
  void Recapture(WorkspaceId id);
  OperationResult Restore(WorkspaceId id, CancellationToken ct);
  void Delete(WorkspaceId id);
}
```

```csharp
interface IProgressSink {
  void Report(OperationEvent evt);
}
```

Implementation notes:
- The controller owns default options/timeouts for V1
- The controller maps user actions to services and aggregates results

------------------------------------------------------------------------

# L5. UI — Minimal Contract

- Workspace list view (ListWorkspaces)
- Buttons: Capture / Recapture / Restore / Delete
- Progress display driven by IProgressSink events
- Error display driven by OperationResult.Errors

No manual editing in V1.

------------------------------------------------------------------------

# 8. Default V1 Constants (Recommended)

These are not hard requirements but should be centralized.

```text
ClosePerWindowTimeout:        10s
WindowMatchTimeoutPerEntry:   15s
MatchPollInterval:            200ms
```

------------------------------------------------------------------------

End of Architecture (V1 Modules & Interfaces)

