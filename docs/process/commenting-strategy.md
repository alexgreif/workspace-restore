# Commenting Strategy

This document defines the commenting philosophy for the Workspace App
codebase.

The goal is to keep the code **self-explanatory**, while still
documenting important intent, constraints, and non-obvious behavior.

Comments should improve readability and maintainability without creating
noise.

------------------------------------------------------------------------

# 1. Core Principle

**Comments explain *why*, not *what*.**

Good comments answer one of the following questions:

-   Why is the code written this way?
-   What assumption or constraint exists?
-   What non-obvious behavior must future readers understand?

Avoid comments that merely restate what the code already expresses.

Example (bad):

``` csharp
// Increment counter
counter++;
```

Example (good):

``` csharp
// Windows sometimes returns duplicate HWNDs during enumeration.
// We deduplicate here to avoid capturing the same window twice.
```

Prefer expressive names over explanatory comments whenever possible.

Example:

Bad:

``` csharp
Process()
```

Better:

``` csharp
RestoreWorkspaceWindows()
```

------------------------------------------------------------------------

# 2. File-Level Comments

Most files **do not require a header comment**.

The filename and namespace already communicate purpose.

Add a file header only when the file represents something
architecturally significant or contains complex behavior.

Typical candidates:

-   Engine services
-   OS integration components
-   Algorithms with multiple phases
-   Orchestrators

Example:

``` csharp
// CaptureService
// Responsible for capturing the current visible desktop state
// and converting it into a Workspace domain model.
//
// This service does not call Win32 directly.
// All OS interaction is delegated to the WinApi layer.
```

Do not add headers to simple value objects or trivial classes.

------------------------------------------------------------------------

# 3. Class-Level Comments

Important **public classes and interfaces** should have a short
description.

These should:

-   Explain the responsibility of the type
-   Stay concise (1-3 lines)
-   Avoid repeating the class name

Public APIs should generally be documented.

Internal or private classes should only be documented when their
behavior or constraints are not obvious.

Example:

``` csharp
/// Represents a persisted workspace configuration.
/// A workspace contains application entries and window layouts
/// that can be restored from a clean desktop state.
public sealed class Workspace
```

------------------------------------------------------------------------

# 4. Method-Level Comments

Method comments are only necessary when:

-   The intent is not obvious
-   The method enforces a business rule
-   The algorithm has multiple steps
-   Side effects or constraints exist

Public API methods should document behavioral contracts and failure
semantics when relevant.

Example:

``` csharp
/// Restores a workspace using the V1 clean-slate restore process.
/// This includes global close, application launch,
/// layout restoration, and z-order replay.
OperationResult RestoreWorkspace(...)
```

Do **not** comment trivial methods such as getters or simple helpers.

------------------------------------------------------------------------

# 5. Inline Comments

Inline comments are often the most valuable type of documentation.

Use them for:

-   OS quirks
-   ordering constraints
-   algorithm decisions
-   unusual edge cases

Example:

``` csharp
// EnumWindows returns windows in Z-order.
// We invert the list so index 0 becomes the bottom-most window.
```

Example:

``` csharp
// Abort restore if a window refuses to close.
// This enforces the "clean slate" guarantee.
```

------------------------------------------------------------------------

# 6. Domain Layer Guidelines

Domain types should contain **very few comments**.

Domain code should be simple and self-explanatory.

Example:

``` csharp
public sealed record Rect(int X, int Y, int Width, int Height);
```

Only comment when enforcing an invariant or constraint.

Example:

``` csharp
// Width and Height must be positive.
```

------------------------------------------------------------------------

# 7. Engine Layer Guidelines

Engine code contains most of the application behavior.

Comments should explain:

-   restore algorithm phases
-   window matching logic
-   ordering rules
-   timeout policies
-   interactions with the operating system

Example:

``` csharp
// Step 1: Close all currently visible windows.
// Restore always begins from a clean desktop state.
```

------------------------------------------------------------------------

# 8. WinApi Layer Guidelines

WinApi integration requires the **most comments**.

Reasons:

-   Win32 behavior is complex
-   API documentation is inconsistent
-   OS quirks must be recorded

Typical examples:

``` csharp
// EnumWindows enumerates windows in top-to-bottom Z-order.
// This ordering is used to compute the captured stacking order.
```

``` csharp
// Some windows appear visible but are "cloaked".
// We filter these using DwmGetWindowAttribute.
```

------------------------------------------------------------------------

# 9. TODO Comments

TODO comments are allowed when they describe a **specific future
investigation**.

Each TODO should include at least one traceability anchor:

-   issue/PR/ADR reference, or
-   owner initials and date

Example:

``` csharp
// TODO(#123): Investigate whether Electron apps spawn hidden helper windows
// that appear briefly during launch.
```

Example:

``` csharp
// TODO(AS, 2026-03-04): Re-evaluate timeout defaults after Milestone 8 manual runs.
```

Avoid vague TODOs such as:

    TODO: fix later

------------------------------------------------------------------------

# 10. Algorithm Step Comments

For complex operations, it is useful to document phases explicitly.

Example:

``` csharp
// Phase 1 — Global Close
CloseAllWindows();

// Phase 2 — Launch Applications
LaunchWorkspaceApplications();

// Phase 3 — Restore Layout
ApplyWindowLayouts();

// Phase 4 — Replay Z-order
RestoreStackingOrder();
```

This makes complex flows easier to understand.

------------------------------------------------------------------------

# 11. Surprising or Non-Obvious Code

Any code that appears unusual, counterintuitive, or incorrect at first
glance must include a comment explaining the reasoning.

Examples include:

-   performance optimizations
-   framework or OS workarounds
-   intentional violations of style rules
-   defensive logic protecting against external system behavior

Example:

``` csharp
// Intentionally not using LINQ here.
// Allocation overhead becomes significant during large window captures.
```

------------------------------------------------------------------------

# 12. Comment Density Guidelines

Different layers require different comment density.

  Layer         Comment Density
  ------------- -----------------
  Domain        Low
  Persistence   Low
  Engine        Medium
  WinApi        High
  UI            Low

------------------------------------------------------------------------

# 13. Documentation Comment Types

Use XML documentation comments (`///`) for **public APIs** when they
clarify behavioral contracts, constraints, or failure semantics.

If a public signature is fully self-evident, XML documentation is
optional.

Use regular comments (`//`) for **implementation details** inside
methods, algorithms, and internal logic.

------------------------------------------------------------------------

# 14. Preventing Comment Rot

Comments must be updated whenever the behavior of the code changes.

Outdated comments are worse than missing comments because they mislead
future readers.

During code review, treat comment correctness as part of the change.

------------------------------------------------------------------------

# 15. Invariant Comments

Invariant comments document **conditions that must always remain true**.

They are particularly useful in complex algorithms, state machines, or
multi-step operations where assumptions must be preserved.

Example:

``` csharp
// Invariant:
// - windows list is always ordered bottom-to-top
// - no duplicate HWND values exist
// - all entries represent visible windows
```

Invariant comments help future developers safely modify the code without
accidentally violating important assumptions.

------------------------------------------------------------------------

# 16. Comment Review Checklist

During code reviews, reviewers should consider the following questions:

**Clarity**
- Does the code rely on assumptions that are not obvious?
- Would a new developer understand the intent without external knowledge?

**Correctness**
- Are comments still accurate after the change?
- Do any existing comments contradict the implementation?

**Noise**
- Are there comments that merely restate the code?
- Can clearer naming remove the need for the comment?

**Documentation**
- Are public APIs documented where necessary?
- Are OS quirks, constraints, or invariants recorded?

------------------------------------------------------------------------

# 17. Summary

A well-commented codebase should:

-   Prefer **clear naming over comments**
-   Use comments to explain **intent and constraints**
-   Document **algorithms and OS behavior**
-   Record **quirks and surprising behavior**
-   Avoid redundant or obvious comments

Good comments capture **intent that cannot be derived from the code
alone** and act as **navigation markers for future readers**.
