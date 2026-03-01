## Summary

- What changed?
- Why is this needed?

## Milestone

- Related milestone: `M?`

## Type of Change

- [ ] Feature
- [ ] Bug fix
- [ ] Refactor
- [ ] Docs
- [ ] Chore

## Architecture Check

- [ ] Layering is preserved (`UI -> App -> Engine -> Domain`)
- [ ] No WinApi leakage outside `WorkspaceApp.WinApi`
- [ ] UI does not directly call Engine/Persistence/WinApi

## V1 Behavior Check

- [ ] No unintended changes to V1 commitments
- [ ] If behavior changed intentionally, spec/docs were updated
- [ ] ADR added when decision-level change was made

## Verification

- Build/Test commands run:
  - `dotnet build src/WorkspaceApp.sln`
  - `dotnet test src/WorkspaceApp.sln`
- Manual validation performed (if applicable):
  - [ ] Capture
  - [ ] Restore (global close + abort-on-timeout)
  - [ ] Recapture
  - [ ] Off-screen clamp/minimize
  - [ ] Z-order replay

## Notes

- Risks/known limitations:
- Follow-up tasks:
