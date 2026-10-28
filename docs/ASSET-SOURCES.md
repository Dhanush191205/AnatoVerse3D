# Asset sources and licensing

## CardioVascular41.fbx (Z-Anatomy, PC-Version branch)

| Field | Value |
|---|---|
| File | `CardioVascular41.fbx` |
| Source URL | https://github.com/LluisV/Z-Anatomy/blob/PC-Version/Resources/Models/FBX/CardioVascular41.fbx |
| Upstream revision | `6c7f9016bd5899ac8edafd31b9900c151df42ed6` (committed 2024-12-25; still the PC-Version head on 2026-10-28) |
| Upstream size | 64,889,388 bytes |
| Upstream Git blob SHA-1 | `aa5bbcb63ae6e9ace8051111b3a85ea87f00c4f3` |
| Local copy | `D:\XR Unity\Project\CardioVascular41.fbx`, 64,889,388 bytes, file date 2026-10-26 19:59 IST |
| In project | `Assets/AnatoVerse3D/ThirdParty/Z-Anatomy/Models/CardioVascular41.fbx` (unmodified copy, stored with Git LFS) |
| Integrity check | **Verified 2026-10-28**: `git hash-object` on the local file printed `aa5bbcb63ae6e9ace8051111b3a85ea87f00c4f3`, identical to the upstream blob. |
| Upstream caveat | Z-Anatomy notes that these resource models may not be its latest models. |

### Licence notices (copies in `docs/licenses/z-anatomy/`)
- `Models-License.txt` — from `Resources/Models/License.txt` at the revision above (model-specific notice).
- `Repository-LICENSE.txt` — from `LICENSE` at the revision above (CC BY-SA 4.0 text plus app credits).

### Required attribution (as requested by the model notice)
- "BodyParts3D - The Database Center for Life Science - CC-BY-SA 2.1 Japan"
- "Z-Anatomy - The open source atlas of anatomy - CC-BY-SA 4.0"

### Conditions that apply to this project
- Attribute as above wherever the model or a derivative is distributed (application credits screen, README, repository).
- Distribute derivative works of the model (modified meshes, extracted heart subset, re-exported files) under the same licence (ShareAlike).
- The same notice lists other models with different licences (University of Dundee CC BY / CC BY-NC-SA, lissiecowley CC BY-NC, University of Washington). They are not part of this project's selected asset unless found inside the FBX; check the imported hierarchy for them.

### Open licence questions (not resolved)
- Whether ShareAlike reaches the application's own C# code, or only the model and its adaptations. The usual reading is that code which merely displays the model is a separate work, but this has not been confirmed for this project.
- Whether CardioVascular41.fbx contains any of the separately licensed "other models". To be checked during Stage 1 import.

### Modifications made by this project
- **The FBX file itself is not modified.**
- Import settings (Unity importer, not the file): Convert Units on, scale factor 1, no cameras/lights/animation/blend shapes, no mesh compression.
- Materials: the 7 FBX materials (all grey) are remapped to project-owned URP Lit materials in `Assets/AnatoVerse3D/Materials/` with colours chosen by this project (myocardium red-brown, valve leaflets ivory, systemic arteries red, systemic veins blue, pulmonary arteries blue, pulmonary veins red). The colours follow the usual textbook convention and are **not** from the source.
- HeartViewer scene: shows only the heart, coronary vessels, cardiac veins and the great-vessel segments next to the heart; all other parts of the cardiovascular model are deactivated on the scene instance. Zero-triangle label markers are not drawn.
- These presentation changes count as adaptations shown alongside the model; the ShareAlike terms apply to any distributed derivative of the model.

### Model structure
See docs/MODEL-STRUCTURE.md for the verified hierarchy, available heart parts and source anomalies.
