# AnatoVerse3D

Interactive 3D heart anatomy viewer for Windows, built with Unity 6 (URP) and C#.
Educational project — not a medical or diagnostic tool.

> Status: in development. Stage 1 (visible heart) and Stage 2 (camera controls) are implemented.
> See [docs/PROGRESS.md](docs/PROGRESS.md).

## Requirements
- Windows 10/11
- Unity **6000.6.5f1** (install through Unity Hub)
- Git with **Git LFS** (the 3D model is stored in LFS)

## Get the project
```powershell
git lfs install
git clone https://github.com/Dhanush191205/AnatoVerse3D.git
```
Check the model downloaded (not a small LFS pointer file):
```powershell
(Get-Item "AnatoVerse3D\Assets\AnatoVerse3D\ThirdParty\Z-Anatomy\Models\CardioVascular41.fbx").Length   # 64889388
```

## Open and run
1. Unity Hub → **Add project from disk** → select the cloned folder → open with 6000.6.5f1.
2. Open `Assets/AnatoVerse3D/Scenes/HeartViewer.unity`.
3. Press **Play**.

## Controls
| Action | Mouse | Keyboard |
|---|---|---|
| Rotate | Left or right drag | Arrow keys |
| Pan | Middle drag, or Shift + drag | — |
| Zoom | Mouse wheel | `+` / `−` |
| Reset view | — | `F` |

Dragging that starts on the on-screen controls panel does not move the camera.

## Editor tools
Menu **AnatoVerse3D**:
- *Stage 1 → Run all steps* — configure the model import, assign URP materials, build the HeartViewer scene, write `docs/stage1/` evidence.
- *Stage 2 → Set up camera controls* — add the orbit camera and controls panel to HeartViewer.

Rebuilding the Stage 1 scene replaces it; run the Stage 2 setup again afterwards.

## Tests
Unity: **Window → General → Test Runner → EditMode → Run All** (camera maths tests).
Results and manual checks: [docs/TESTING.md](docs/TESTING.md).

## Credits and licences
3D model: **Z-Anatomy** CardioVascular41.fbx, derived from **BodyParts3D**.
- "BodyParts3D - The Database Center for Life Science - CC-BY-SA 2.1 Japan"
- "Z-Anatomy - The open source atlas of anatomy - CC-BY-SA 4.0"

The model file is unmodified; colours and the shown subset are this project's presentation choices.
Details: [docs/ASSET-SOURCES.md](docs/ASSET-SOURCES.md), [docs/MODEL-STRUCTURE.md](docs/MODEL-STRUCTURE.md).
The licence for this project's own code has not been chosen yet.
