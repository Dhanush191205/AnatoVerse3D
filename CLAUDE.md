# AnatoVerse3D — working instructions

## Project
- Unity project root: `D:\XR Unity\Project\My project` (folder name kept by owner's choice; product name AnatoVerse3D).
- Editor: Unity 6000.6.5f1. Render pipeline: URP 17.6.0. Input: Input System 1.20.0 only (`activeInputHandler: 1`).
- Removed packages: com.unity.purchasing, com.unity.analytics (deprecated), com.unity.collab-proxy (switched VCS mode to Plastic). Don't re-add without a reason.
- Version Control mode must stay "Visible Meta Files".
- Windows uses quality level **PC** → `Assets/Settings/PC_RPAsset.asset`. GraphicsSettings has no default RP asset; the quality level supplies it.
- Owner chose "My project" as canonical (2026-10-28). `D:\XR Unity\Project\AnatoVerse` is an unused template to archive; never edit it.

## Storage rules
- Everything (assets, downloads, docs, builds) stays on D:. Never create projects or download assets on C:.
- Builds go to `D:\XR Unity\Project\My project\Builds\` (git-ignored).
- Source FBX original: `D:\XR Unity\Project\CardioVascular41.fbx`. Project copy (LFS): `Assets/AnatoVerse3D/ThirdParty/Z-Anatomy/Models/CardioVascular41.fbx`. Do not re-download.
- Main scene: `Assets/AnatoVerse3D/Scenes/HeartViewer.unity`. Model structure: docs/MODEL-STRUCTURE.md.

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
- Remote `origin`: https://github.com/Dhanush191205/AnatoVerse3D (public). First commit `b9e4778` pushed 2026-10-28.
- Commit trailer: `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Runtime notes
- Runtime code: `Assets/AnatoVerse3D/Scripts/Runtime` (asmdef AnatoVerse3D.Runtime → Unity.InputSystem, UnityEngine.UI). Editor tools reference it.
- Enter Play Mode Options are on (no domain/scene reload): no static state in runtime scripts.
- Rebuilding the Stage 1 scene wipes Stage 2 setup; rerun Stage 2 menu.

## Owner preferences
- PowerShell commands with `D:\...` paths; label any Git Bash commands (`/d/...`).
- Stop after each functional stage for the owner's test before starting the next.

## Stage checkpoints
- [x] Stage 0 — audit, Git, Unity connection (done 2026-10-28)
- [x] Stage 1 — visible heart (done 2026-10-28; tooling: menu AnatoVerse3D → Stage 1)
- [x] Stage 2 — orbit/zoom/pan/reset camera (done 2026-10-28; menu AnatoVerse3D → Stage 2; tests in Assets/AnatoVerse3D/Tests/EditMode)
- [ ] Stage 3 — selection, highlight, info panel
- [ ] Stage 4 — presentation + tested Windows build

## Docs
README.md, docs/PROJECT-OVERVIEW.md (todo), docs/ASSET-SOURCES.md, docs/MODEL-STRUCTURE.md, docs/TESTING.md, docs/PROGRESS.md
