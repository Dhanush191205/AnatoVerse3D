# Testing

Editor results and standalone-build results are recorded separately. "Owner" = checked by Dhanush in the
Unity Editor; "Tool" = produced by the AnatoVerse3D editor scripts; "Claude" = verified from files.

## Stage 1 — visible heart (Unity 6000.6.5f1, Editor, 2026-10-28)

| # | Check | How | Result |
|---|---|---|---|
| 1.1 | FBX copy identical to verified source | `Get-FileHash` comparison | Pass (Owner) |
| 1.2 | FBX stored via Git LFS | `git check-attr filter`, `git lfs ls-files` | Pass (Owner) |
| 1.3 | Import scale correct | import-report: fileScale 0.01, body 1.70 m tall | Pass (Tool/Claude) |
| 1.4 | No pink / unsupported materials | import-report "Problems" section | Pass (Tool) |
| 1.5 | Heart parts present as separate meshes | import-report subset table | Pass (Tool) |
| 1.6 | Correct materials in saved scene | scene-check.md | Pass (Tool) |
| 1.7 | Recognisable heart in camera view | heartviewer-editor.png | Pass (Claude, visual) |
| 1.8 | Orientation not mirrored | import-report orientation check | Pass (Tool) |
| 1.9 | No compile errors | Editor.log / Console | Pass (Owner + log) |
| 1.10 | Play Mode shows the heart without errors | Press Play in HeartViewer; Editor.log | Pass (Owner + log, 2026-10-28) |
| 1.11 | Scene references survive reopening | Stage 2 setup reopened HeartViewer from disk and resolved camera, root and target | Pass (Tool) |

## Stage 2 — camera interaction (Unity 6000.6.5f1, Editor, 2026-10-28)

Input device: mouse with wheel. Setup values: docs/stage2/camera-setup.md
(radius 0.146 m, zoom 0.117–1.166 m, pan limit 0.219 m, start distance 0.509 m).

| # | Check | How | Result |
|---|---|---|---|
| 2.1 | Camera maths unit tests (19) | Test Runner → EditMode → Run All | Pass 19/19 (Owner) |
| 2.2 | Scripts compile, setup runs, scene saves | Editor.log, camera-setup.md | Pass (Tool) |
| 2.3 | Orbit with left and right drag, stays centred | Play Mode | Pass (Owner) |
| 2.4 | Pitch limit, no flip over the top | Drag far upward | Pass (Owner) |
| 2.5 | Pan with middle drag and Shift + drag | Play Mode | Pass (Owner) |
| 2.6 | Zoom-in limit stops outside the surface | Scroll in fully | Pass (Owner) |
| 2.7 | Zoom-out limit keeps heart visible | Scroll out fully | Pass (Owner) |
| 2.8 | F resets to the start view | Play Mode | Pass (Owner) |
| 2.9 | Drag starting on the controls panel is ignored | Play Mode | Pass (Owner) |
| 2.10 | Single click does not move the camera | Play Mode | Pass (Owner) |
| 2.11 | Arrow keys rotate, + / − zoom | Play Mode | Pass (Owner) |
| 2.12 | No errors in Play Mode | Editor.log | Pass (log) |
| 2.13 | Trackpad use | — | Not tested (no trackpad) |

## Standalone build
Not yet built (Stage 4).
