# Contributing

Thanks for your interest in contributing! This project is early-stage and optimized for a clean V1 implementation.

## Project Goals (V1)

- Windows-only utility for restoring **clean, deterministic workspaces**.
- Core loop: **Capture → Restore** with global close and deterministic layout recreation.

See:
- Spec: `docs/spec/spec.md`
- Architecture: `docs/architecture/modules.md`
- Milestones: `docs/roadmap/milestones-v1.md`
- Backlog: `docs/backlog/`

## Architecture Rules

Strict layering (do not break these references):

- **UI** → **App** → **Engine** → **Domain**
- **Engine** → **Persistence**
- **Engine** → **WinApi**
- **Persistence** → **Domain**
- **Domain** depends on nothing
- **WinApi** depends on nothing

Additional rules:
- No Win32 types (e.g., `IntPtr`, `HWND`, P/Invoke) outside `WorkspaceApp.WinApi`.
- UI must not reference Engine/Persistence/WinApi directly.
- Keep WinApi wrappers thin; keep behavior in Engine.

## Development Setup

- .NET: 8.x
- UI: WPF (V1)

Typical commands:
- Build: `dotnet build src/WorkspaceApp.sln`
- Test: `dotnet test src/WorkspaceApp.sln`

## Coding Guidelines

- Prefer explicit, deterministic behavior over heuristics.
- Avoid adding abstractions “just in case.” Add them when there is a second use-case.
- Keep public interfaces small and stable.
- Centralize timeouts/constants (Engine/App) rather than scattering magic numbers.

## Testing

V1 prioritizes rapid iteration and manual OS validation.
Unit tests are expected for deterministic logic (Domain/Engine). Avoid brittle UI automation tests in V1.

## Pull Requests

- Keep PRs small and focused.
- Include a short summary of behavior changes and how to verify them.
- Update docs only when necessary (public behavior, architecture, milestones).
- If you change an important decision, add an ADR in `docs/adr/`.

## Commit Messages (Suggested)

- `chore:` repo/tooling/doc maintenance
- `feat(<area>):` new behavior (e.g., `feat(engine): implement capture service`)
- `fix(<area>):` bug fixes
- `docs:` documentation-only changes

## Git Strategy

Use the project Git workflow in `docs/process/git-strategy.md`.

- Branch from `main` using short-lived feature/fix/docs/chore branches.
- Open focused PRs and use the PR template in `.github/PULL_REQUEST_TEMPLATE/pull_request_template.md`.
- Use Conventional Commits (optional local template: `.github/commit_template/commit-message-template.txt`).
- Tag completed milestones on `main` (`m0` ... `m12`).

