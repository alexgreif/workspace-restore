# Workspace App -- Specification

# Core Promise

The Workspace App exists to reliably restore and enforce predefined,
clean application contexts.

------------------------------------------------------------------------

# 0. Platform Target (V1)

V1 is a **Windows** application.

Other platforms may be considered later but are out of scope for V1.

## 0.1 Reference Implementation Stack (V1)

To reduce ambiguity in implementation and tooling, the V1 reference stack is fixed to:

- .NET 8
- C#
- WPF (desktop UI)

This is an implementation decision, not a user-facing product capability.
Core product behavior and scope are defined by the rest of this specification.
Decision record: ADR-0001 (`docs/adr/0001-tech-stack-dotnet-wpf.md`).

------------------------------------------------------------------------

# 1. Core Concepts

## 1.1 Workspace (Formal Definition)

A **workspace** is a named configuration consisting of:

1.  Application entries\
2.  Workspace rules

All components are optional.\
An empty workspace is a valid workspace.

A workspace represents:

-   A structured application context
-   A rule set governing how that context is restored and enforced

------------------------------------------------------------------------

## 1.2 Application Entry (V1)

In V1, application entries are derived exclusively from **visible,
top-level Windows OS windows**.

Background processes or headless application instances are out of scope
for V1.

Each application entry may include:

-   One or more visible windows

### Windows

A window is a Windows OS-level visible window belonging to an
application entry.

A window has:

-   Monitor
-   Coordinates
-   Size
-   State (`Normal`, `Minimized`, `Maximized`)

Workspaces are monitor-topology specific.\
If restored on a different monitor setup, positioning is best-effort.

------------------------------------------------------------------------

## 1.3 Special Case: The Workspace App Itself

The Workspace App has two roles:

1.  Controller (background management logic)
2.  UI window (visible management interface)

The controller component is never part of a workspace.

The Workspace App's UI window may be included or excluded like any other
application window.

------------------------------------------------------------------------

## 1.4 Workspace Rules (V1)

Workspace rules define how restoration and enforcement behave.

### Core Enforcement Rule (V1): Global Close on Restore

When restoring a workspace, the system first attempts to close **all**
currently open, visible, top-level windows (i.e., the current user-facing
desktop context), then opens the workspace.

-   The close attempt is **graceful** (equivalent to a normal user close
    action).
-   The Workspace App does not attempt to manage headless/background
    processes in V1.
-   If an application shows "unsaved changes" prompts, the user must
    resolve those prompts (V1 does not automate decision-making inside
    other applications).

This global close behavior is the primary differentiator of the app's
"clean slate" philosophy in V1.

### Not in V1 (Backlog / Extensions)

-   Close-exception-list (apps/windows that should remain open during
    restore)
-   Explicit close list (close only specified apps instead of global
    close)
-   Block-list / blocking behavior (prevent apps from running)

------------------------------------------------------------------------

# 2. Core Operations

## 2.1 Capture Workspace (Create Only)

Capture creates a new workspace from the current system state.

In V1:

-   Capture detects currently visible windows
-   Creates application entries based on those windows
-   Stores bounds and window state (`Normal`, `Minimized`, `Maximized`) per captured window
-   Workspace is stored

Capture does not modify existing workspaces.

------------------------------------------------------------------------

## 2.2 Restore Workspace (Global Close + Open)

Restoring a workspace means:

1.  Reading the stored workspace configuration
2.  Closing all currently visible, top-level windows (graceful close)
3.  Opening stored application entries
4.  Restoring window positions (best-effort)
5.  Restoring explicit window state (`Normal`, `Minimized`, `Maximized`)

Notes:

-   Applications that are part of the workspace are still subject to
    step (2) because restore always starts from a "clean slate".
-   V1 does not guarantee that all windows will close successfully (e.g.,
    unsaved changes prompts, permission issues).

------------------------------------------------------------------------

## 2.3 Recapture Workspace (Update Only, Overwrite)

Recapture updates an existing workspace.

In V1:

-   System re-reads currently visible windows
-   The selected workspace's application entries and window positions are
    **overwritten** to match the current visible windows:
    -   entries/windows currently visible are stored
    -   entries/windows not currently visible are removed from the
        workspace definition

Recapture does not create new workspaces.

------------------------------------------------------------------------

# 3. Core Use Cases (V1)

## Use Case 1 -- Switching to a Workspace

1.  User selects a workspace
2.  System closes all currently visible, top-level windows
3.  System opens stored application entries
4.  System restores window positions

Result: A clean predefined context is restored from a "clean slate".

------------------------------------------------------------------------

## Use Case 2 -- Creating a Workspace

### A) Create from Capture

-   Capture visible windows
-   Assign name
-   Store workspace

### B) Create from Scratch

-   Start empty
-   Assign name
-   Store workspace

------------------------------------------------------------------------

## Use Case 3 -- Updating a Workspace

User arranges visible windows → selects Recapture.

System overwrites the selected workspace definition accordingly.

------------------------------------------------------------------------

# 4. V1 Scope Definition

## Included in V1

-   Windows-only (V1)
-   Visible-window-based application entries
-   Window position
-   Explicit window state capture/restore (`Normal`, `Minimized`, `Maximized`)
-   Capture (create-only)
-   Restore (global close + open)
-   Recapture (update-only, overwrite)
-   GUI-based workspace selection

## Deferred from V1 (Backlog / Extensions)

-   State support (native apps)
-   Blocking (block-list)
-   Close-exception-list
-   Explicit close list (selective close)
-   Block-exceptions
-   Hotkeys
-   Manual GUI editing of window positions
-   Background/headless application management
-   Timer functionality

------------------------------------------------------------------------

# 5. Product Boundaries

## Explicit Exclusions

The Workspace App does **not** aim to:

1.  Persist or restore dynamic runtime state
2.  Replace the operating system's window manager
3.  Guarantee cross-environment fidelity
4.  Provide general automation or scripting
5.  Provide continuous background application control
6.  Modify application data or internal storage
