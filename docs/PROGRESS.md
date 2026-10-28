# Progress

## Stage 0 — audit (2026-10-28, partial)

Verified from files on disk (read-only):
- Project `D:\XR Unity\Project\My project`: Unity 6000.6.5f1, URP 17.6.0, Input System 1.20.0 (Input System only), linear colour space, productName still "My project".
- Windows quality level "PC" uses `PC_RPAsset`; no default RP in GraphicsSettings.
- Only template content in Assets (SampleScene, URP settings, TutorialInfo, HubForceResolve.cs). No FBX imported.
- Git: repository initialised 2026-10-26, branch `main`, **no commits**, **no remote**. `.gitignore` present.
- Git for Windows with `git-lfs.exe` installed at `D:\XR Unity\Git`.
- No Unity Editor was running on either project at audit time (both logs end in a clean shutdown).
- Unity AI Assistant MCP: local Claude Code (`claude.exe`) was approved in the **AnatoVerse** project only; "My project" has no MCP approvals.
- Second template project at `D:\XR Unity\Project\AnatoVerse` (productName AnatoVerse, no Git, no FBX, used 2026-10-27).
- FBX present at `D:\XR Unity\Project\CardioVascular41.fbx`, size matches upstream.

Not verified (device shell was unavailable to this session):
- `git status`, `git lfs version`, GitHub authentication.
- FBX content hash.
- Whether the project compiles cleanly when reopened.
- Unity Editor connection.

## Known issues
- `Assets/Editor/HubForceResolve.cs` is still present; it should delete itself on the next Editor open.
