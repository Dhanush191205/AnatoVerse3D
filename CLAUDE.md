# AnatoVerse3D — working instructions

## Project
- Unity project root: `D:\XR Unity\Project\My project` (folder name kept by owner's choice; product name AnatoVerse3D).
- Editor: Unity 6000.6.5f1. Render pipeline: URP 17.6.0. Input: Input System 1.20.0 only (`activeInputHandler: 1`).
- Windows uses quality level **PC** → `Assets/Settings/PC_RPAsset.asset`. GraphicsSettings has no default RP asset; the quality level supplies it.
- Owner chose "My project" as canonical (2026-10-28). `D:\XR Unity\Project\AnatoVerse` is an unused template to archive; never edit it.

## Storage rules
- Everything (assets, downloads, docs, builds) stays on D:. Never create projects or download assets on C:.
- Builds go to `D:\XR Unity\Project\My project\Builds\` (git-ignored).
- Source FBX currently at `D:\XR Unity\Project\CardioVascular41.fbx` (outside the project). Do not re-download.

## Conventions
- Project-owned content: `Assets/AnatoVerse3D/` (Scenes, Scripts, Prefabs, Materials, UI, Data).
- Third-party: `Assets/AnatoVerse3D/ThirdParty/Z-Anatomy/` with licence notices beside the model.
- Never modify the source FBX; presentation changes go in wrappers/prefabs/scene instances.
- Use Editor APIs or the Editor for scenes/prefabs; do not hand-edit serialized YAML.
- Do not invent anatomical part names. Only expose parts that exist as separate geometry in the model.
- UI: uGUI/TextMeshPro or UI Toolkit, never OnGUI.
- `Assets/Editor/HubForceResolve.cs` is a Unity Hub bootstrap script that deletes itself after a package resolve. Do not commit or edit it.

## Git
- Repo root = project root, branch `main`. Never force-push, rewrite history, or delete branches without the owner's agreement.
- Do not use the Z-Anatomy repo as origin.
- Git LFS (repo-local install) tracks `*.fbx`, `*.obj`, `*.blend` via `.gitattributes`. Verify with `git lfs ls-files` after adding any model.
- Remote: owner's public GitHub repo `AnatoVerse3D` (URL to be recorded here once created).
- Commit trailer: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Owner preferences
- PowerShell commands with `D:\...` paths; label any Git Bash commands (`/d/...`).
- Stop after each functional stage for the owner's test before starting the next.

## Stage checkpoints
- [~] Stage 0 — audit, Git, Unity connection (see docs/PROGRESS.md)
- [ ] Stage 1 — visible heart (scene, URP materials, framing, attribution)
- [ ] Stage 2 — orbit/zoom/pan/reset camera
- [ ] Stage 3 — selection, highlight, info panel
- [ ] Stage 4 — presentation + tested Windows build

## Docs
README.md, docs/PROJECT-OVERVIEW.md, docs/ASSET-SOURCES.md, docs/TESTING.md, docs/PROGRESS.md
