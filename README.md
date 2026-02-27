# Workspace App

Workspace App is a Windows-first productivity tool that restores clean, deterministic workspaces.

It allows users to:

- Capture the current visible desktop state
- Save it as a named workspace
- Restore that workspace later
- Enforce a clean slate by globally closing all visible windows before restoration

---

## Philosophy

Workspace App is opinionated.

Restoring a workspace means:

1. Gracefully closing all visible top-level windows.
2. Aborting if a window refuses to close.
3. Launching only the applications defined in the workspace.
4. Restoring their position and stacking order.

The goal is deterministic, distraction-free context switching.

---

## Platform

- Windows only (V1)
- Visible top-level windows only
- No background process management

---

## Architecture

The project follows a strict layered architecture:

- UI (WPF)
- AppOrchestrator
- WorkspaceEngine
- Persistence
- Domain
- WinApi

See [docs/architecture](docs/architecture/) for details.

---

## Status

V1 in development.

See [docs/spec](docs/spec/) for formal specification.
See [docs/backlog](docs/backlog/) for future extensions.

---

## License

MIT
