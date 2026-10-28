# CardioVascular41.fbx — structure

Analysed 2026-10-28 by parsing the binary FBX directly (file verified against upstream blob
`aa5bbcb6…`). Unity's own import results are recorded separately in `docs/stage1/import-report.md`.

## File facts
- Format: binary FBX 7.4, exported by "Blender (stable FBX IO) 3.0.0 / 4.27.0".
- Axes: Y up, Z front, X coord (right-handed). UnitScaleFactor 1.0 → 1 FBX unit = 1 cm.
  With Unity's Convert Units the whole body is about 1.70 m tall; the heart is about 11–12 cm across.
- 760 nodes, 717 meshes, 7 materials, no textures, no animation used.
- All 7 materials have the same grey colour (0.8, 0.8, 0.8). The FBX carries **no meaningful colour
  differences**; any colours in AnatoVerse3D are a presentation choice of this project.
- Material names are Z-Anatomy/Blender working names, not anatomical labels:
  `Trapezius.001` (used on heart chambers and papillary muscles), `Ligament.002` (atrioventricular
  valve leaflets), `Cartilage.002` (semilunar valve leaflets), `Artery`, `Vein`, `Pulmonary artery`,
  `Pulmonary vein`.

## Naming conventions in the hierarchy (observed)
- `.g` — group node (no geometry).
- `.j` — 2-vertex marker mesh with no faces (41 of these). Not anatomy; not drawn.
- `.t` — group holding a surface/landmark label (e.g. "Apex of heart.t"); contains only a `.j` marker.
- `.l` / `.r` — left / right.
- Root also contains three 4-vertex planes `Cross Section X/Y/Z.002` (Z-Anatomy section tools). Hidden.

## Top level
```
Arterial system.g
  Systemic arteries.g        (aorta, head/neck, limb, spinal and cerebral arteries)
  Heart.g                    (18 children)
  Pulmonary arteries.g
  Arteries of heart.g        (coronary arteries)
Venous system.g
  Systemic veins.g
  Pulmonary veins.g
  Cardiac veins.g
Cross Section X.002 / Y.002 / Z.002
```

## Heart geometry available as separate meshes
| Group | Mesh | Vertices |
|---|---|---:|
| Heart.g | Right atrium | 4,756 |
| Heart.g | Right ventricle | 4,726 |
| Right ventricle | Anterior papillary muscle of right ventricle | 318 |
| Right ventricle | Inferior papillary muscle of right ventricle | 225 |
| Right ventricle | Septal papillary muscle of right ventricle | 175 |
| Heart.g | Left atrium | 1,092 |
| Heart.g | Left ventricle | 4,718 |
| Left ventricle | Inferior papillary muscle of left ventricle | 321 |
| Right atrioventricular valve.g | Inferior leaflet of right atrioventricular valve | 2,005 |
| Right atrioventricular valve.g | Septal leaflet of right atrioventricular valve | 1,839 |
| Right atrioventricular valve.g | Posterior leaflet of left atrioventricular valve (see anomaly 1) | 1,544 |
| Aortic valve.g | Left coronary leaflet / Right coronary leaflet / Non-coronary leaflet | 189 / 190 / 66 |
| Pulmonary valve.g | Anterior / Left / Right semilunar leaflet of pulmonary valve | 523 / 410 / 565 |
| Arteries of heart.g | Right coronary artery (+ right inferolateral branch) | 5,717 / 3,874 |
| Arteries of heart.g | Left coronary artery → Anterior interventricular artery (+ septal branches), Circumflex artery of heart | 130 → 2,433 (1,488), 3,437 |
| Cardiac veins.g | Coronary sinus → Great cardiac vein, Middle cardiac vein, Inferior vein of left ventricle, "Inferior vein of left ventricle (//Posterior '')" | 307 → 3,103, 6,709, 2,468, 4,642 |

Heart surface landmarks (apex, base, borders, surfaces, sulci, interatrial septum) exist **only as
label markers** (`.t` groups), not as separate geometry.

## Great vessels near the heart (single meshes, branches hidden in the heart scene)
Ascending aorta, Aortic arch, Pulmonary trunk (+ Bifurcation of pulmonary trunk), Left and Right
pulmonary artery, four pulmonary veins, Superior vena cava, Inferior vena cava (thoracic part).
The pulmonary arteries and all four pulmonary veins also have child meshes for their branches inside the
lungs (e.g. "Apical vein of right lung"); the heart scene shows only the vessel segments next to the heart.

## Source anomalies and limits (do not hide these in the UI)
1. **"Posterior leaflet of left atrioventricular valve"** is grouped under *Right atrioventricular valve.g*.
   Its position (patient's left, posterior, beside the left ventricle) fits the mitral valve, so the
   grouping is probably a source error [likely]. The left atrioventricular valve group itself has no
   leaflet meshes.
2. **Missing leaflets:** no anterior leaflet of the mitral valve and no anterior leaflet of the tricuspid
   valve exist as geometry.
3. **No separate interventricular septum, myocardium layers, or chordae tendineae.**
4. One cardiac-vein mesh has an unfinished name: `Inferior vein of left ventricle (//Posterior '')`.
5. Three node names were exported as literal question marks (non-Latin characters lost in the source
   export): `????????` (a child of Coronary sinus, 4,645 vertices, probably a posterior cardiac vein
   [Guessing]) and `?x.r` / `?x.l` (arteries near the pterygoid canal). They cannot be named reliably and
   will not be given invented names.
6. Upstream notes that these resource models may not be Z-Anatomy's latest.

## Orientation (FBX space, from chamber centroids)
- +Z is anterior (right ventricle centre is in front of the left atrium).
- +X is the patient's left (left ventricle centre is left of the right atrium).
- Unity's importer converts handedness; the Stage 1 report re-checks orientation inside Unity.
