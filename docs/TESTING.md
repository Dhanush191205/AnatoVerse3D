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
| 1.10 | Play Mode shows the heart without errors | Press Play in HeartViewer | Owner to confirm |
| 1.11 | Scene references survive reopening | Open another scene, reopen HeartViewer | Owner to confirm |

## Standalone build
Not yet built (Stage 4).
