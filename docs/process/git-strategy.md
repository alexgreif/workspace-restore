# Git Strategy (V1)

This project uses a simple, milestone-aligned workflow focused on stability and fast iteration.

## Branch Model

- `main` is always releasable/stable.
- Work happens in short-lived branches.
- No long-lived `develop` branch in V1.

## Branch Naming

- `feat/m{N}-{short-desc}` for milestone work (example: `feat/m3-window-enumerator`)
- `fix/{short-desc}` for bug fixes
- `docs/{short-desc}` for docs-only work
- `chore/{short-desc}` for tooling/refactor work

## Commits

Use Conventional Commits with layer/module scope when possible:

- `feat(winapi): enumerate visible windows with cloaked filter`
- `feat(engine): group captured windows by exePath`
- `fix(persistence): preserve schema version on update`
- `docs(roadmap): clarify milestone 7 matching behavior`

Guidelines:
- Keep commits small and logically atomic.
- Prefer one concern per commit.
- Include short rationale in the body when behavior changes.

## Pull Requests

- Keep PRs focused and small.
- Include verification steps (especially for WinApi/restore behavior).
- Confirm architecture rules are still respected.
- Update docs/ADR when public behavior or architecture changes.

Use template: `.github/PULL_REQUEST_TEMPLATE/pull_request_template.md`.

## Milestone Tags

Tag `main` when a milestone is done:

- `m0-baseline`, `m1-domain-model`, ..., `m12-hardening`

Optional semantic pre-release tags can be added in parallel (example: `v0.3.0-m3`).

## Main Branch Protection (Recommended)

- Require PRs for changes to `main`
- Require successful build/tests before merge
- Require linear history (squash or rebase)
- Avoid direct pushes except emergency hotfixes

## ADR Coupling

If a change alters architecture or an accepted technical decision:

- Add a new ADR in `docs/adr/`
- Do not rewrite accepted ADR history
- Reference the ADR in the PR description
