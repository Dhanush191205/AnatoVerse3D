// AnatoVerse3D — Stage 1 editor tooling.
// Configures the Z-Anatomy CardioVascular41.fbx import, assigns URP materials,
// writes an inspection report, builds the HeartViewer scene and captures a screenshot.
// The source FBX is never modified: materials are applied through importer remaps and
// visibility is set on the scene instance only.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace AnatoVerse3D.EditorTools
{
    public static class Stage1HeartSetup
    {
        public const string ModelPath = "Assets/AnatoVerse3D/ThirdParty/Z-Anatomy/Models/CardioVascular41.fbx";
        public const string ScenePath = "Assets/AnatoVerse3D/Scenes/HeartViewer.unity";
        const string MaterialFolder = "Assets/AnatoVerse3D/Materials";
        const string SceneFolder = "Assets/AnatoVerse3D/Scenes";
        const string UrpLitShader = "Universal Render Pipeline/Lit";
        const string MenuRoot = "AnatoVerse3D/Stage 1/";

        // Node names verified in CardioVascular41.fbx (Z-Anatomy PC-Version, rev 6c7f9016) on 2026-10-28.
        // Whole subtree is shown:
        static readonly string[] IncludeSubtree =
        {
            "Heart.g", "Arteries of heart.g", "Cardiac veins.g", "Pulmonary trunk"
        };
        // Only this node's own mesh is shown (its branches further from the heart are hidden):
        static readonly string[] IncludeSelfOnly =
        {
            "Ascending aorta", "Aortic arch", "Left pulmonary artery", "Right pulmonary artery",
            "Superior vena cava", "Inferior vena cava (thoracic part)",
            // The four pulmonary veins without their intrapulmonary (lung) branches:
            "Right superior pulmonary vein", "Right inferior pulmonary vein",
            "Left superior pulmonary vein", "Left inferior pulmonary vein"
        };

        struct MaterialSpec
        {
            public string Source;   // material name inside the FBX
            public string Target;   // project-owned URP material
            public Color Color;     // presentation colour chosen by this project (FBX colours are all 0.8 grey)
            public float Smoothness;
            public MaterialSpec(string source, string target, Color color, float smoothness)
            { Source = source; Target = target; Color = color; Smoothness = smoothness; }
        }

        static readonly MaterialSpec[] MaterialSpecs =
        {
            new MaterialSpec("Trapezius.001",    "Heart_Myocardium",              new Color(0.60f, 0.18f, 0.16f), 0.35f),
            new MaterialSpec("Ligament.002",     "Heart_AtrioventricularLeaflet", new Color(0.92f, 0.86f, 0.76f), 0.45f),
            new MaterialSpec("Cartilage.002",    "Heart_SemilunarLeaflet",        new Color(0.88f, 0.82f, 0.72f), 0.45f),
            new MaterialSpec("Artery",           "Vessel_SystemicArtery",         new Color(0.78f, 0.10f, 0.10f), 0.50f),
            new MaterialSpec("Vein",             "Vessel_SystemicVein",           new Color(0.16f, 0.28f, 0.72f), 0.50f),
            new MaterialSpec("Pulmonary artery", "Vessel_PulmonaryArtery",        new Color(0.30f, 0.44f, 0.86f), 0.50f),
            new MaterialSpec("Pulmonary vein",   "Vessel_PulmonaryVein",          new Color(0.86f, 0.28f, 0.26f), 0.50f),
        };

        static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        static string DocsFolder => Path.Combine(ProjectRoot, "docs", "stage1");
        static string ReportPath => Path.Combine(DocsFolder, "import-report.md");
        static string ScreenshotPath => Path.Combine(DocsFolder, "heartviewer-editor.png");

        // ------------------------------------------------------------------ menu

        [MenuItem(MenuRoot + "Run all steps (1-5)", false, 0)]
        public static void RunAll()
        {
            if (!ConfigureImport()) return;
            if (!CreateAndRemapMaterials()) return;
            WriteImportReport();
            if (!BuildHeartViewerScene()) return;
            AssetDatabase.SaveAssets();
            // Capture on a later editor update so the renderer has picked up the new materials.
            EditorApplication.delayCall += () => { SceneView.RepaintAll(); EditorApplication.delayCall += CaptureScreenshot; };
            Debug.Log("[AnatoVerse3D] Stage 1 setup finished. Report: docs/stage1/import-report.md");
        }

        [MenuItem(MenuRoot + "Run all steps (1-5)", true)]
        [MenuItem(MenuRoot + "1. Configure model import", true)]
        [MenuItem(MenuRoot + "2. Create URP materials and remap", true)]
        [MenuItem(MenuRoot + "3. Write import report", true)]
        [MenuItem(MenuRoot + "4. Build HeartViewer scene", true)]
        static bool ModelExists() => AssetImporter.GetAtPath(ModelPath) is ModelImporter;

        [MenuItem(MenuRoot + "1. Configure model import", false, 11)]
        static void ConfigureImportMenu() => ConfigureImport();

        public static bool ConfigureImport()
        {
            var importer = GetImporter();
            if (importer == null) return false;

            importer.globalScale = 1f;
            importer.useFileScale = true;            // "Convert Units": FBX centimetres -> Unity metres
            importer.bakeAxisConversion = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importVisibility = true;
            importer.importBlendShapes = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importAnimation = false;
            importer.addCollider = false;            // colliders are a Stage 3 decision
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.indexFormat = ModelImporterIndexFormat.Auto;
            importer.importNormals = ModelImporterNormals.Import;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.SaveAndReimport();

            Debug.Log($"[AnatoVerse3D] Import configured. fileScale={importer.fileScale}, globalScale={importer.globalScale}, useFileScale={importer.useFileScale}");
            return true;
        }

        [MenuItem(MenuRoot + "2. Create URP materials and remap", false, 12)]
        static void CreateAndRemapMaterialsMenu() => CreateAndRemapMaterials();

        public static bool CreateAndRemapMaterials()
        {
            var importer = GetImporter();
            if (importer == null) return false;

            var shader = Shader.Find(UrpLitShader);
            if (shader == null)
            {
                Debug.LogError($"[AnatoVerse3D] Shader '{UrpLitShader}' not found. Is URP installed and active?");
                return false;
            }

            var embedded = new HashSet<string>(
                AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Material>().Select(m => m.name));
            var alreadyRemapped = new HashSet<string>(
                importer.GetExternalObjectMap().Keys.Where(k => k.type == typeof(Material)).Select(k => k.name));

            EnsureFolder(MaterialFolder);
            foreach (var spec in MaterialSpecs)
            {
                if (!embedded.Contains(spec.Source) && !alreadyRemapped.Contains(spec.Source))
                    Debug.LogWarning($"[AnatoVerse3D] Source material '{spec.Source}' not found in the FBX; remap skipped.");

                string path = $"{MaterialFolder}/{spec.Target}.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(shader) { name = spec.Target };
                    AssetDatabase.CreateAsset(mat, path);
                }
                mat.shader = shader;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", spec.Color);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", spec.Smoothness);
                if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);
                EditorUtility.SetDirty(mat);

                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), spec.Source), mat);
            }
            AssetDatabase.SaveAssets();
            importer.SaveAndReimport();
            Debug.Log("[AnatoVerse3D] URP materials created and remapped.");
            return true;
        }

        // ------------------------------------------------------------------ report

        [MenuItem(MenuRoot + "3. Write import report", false, 13)]
        public static void WriteImportReport()
        {
            var importer = GetImporter();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (importer == null || prefab == null) return;

            var preview = EditorSceneManager.NewPreviewScene();
            try
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, preview);
                var sb = new StringBuilder();
                sb.AppendLine("# Stage 1 import report — CardioVascular41.fbx");
                sb.AppendLine();
                sb.AppendLine($"Generated by `{MenuRoot}3. Write import report` on {DateTime.Now:yyyy-MM-dd HH:mm} (Unity {Application.unityVersion}).");
                sb.AppendLine();
                sb.AppendLine("## Importer settings");
                sb.AppendLine($"- Asset: `{ModelPath}`");
                sb.AppendLine($"- File scale (from FBX units): {importer.fileScale}; Convert Units (useFileScale): {importer.useFileScale}; Scale Factor: {importer.globalScale}");
                sb.AppendLine($"- Bake axis conversion: {importer.bakeAxisConversion}; Import visibility: {importer.importVisibility}");
                sb.AppendLine($"- Material import mode: {importer.materialImportMode}; location: {importer.materialLocation}");
                var remaps = importer.GetExternalObjectMap().Where(kv => kv.Key.type == typeof(Material)).ToList();
                sb.AppendLine($"- Material remaps ({remaps.Count}):");
                foreach (var kv in remaps.OrderBy(k => k.Key.name))
                    sb.AppendLine($"  - `{kv.Key.name}` → `{(kv.Value ? AssetDatabase.GetAssetPath(kv.Value) : "(missing)")}`");
                sb.AppendLine();

                var transforms = go.GetComponentsInChildren<Transform>(true);
                var renderers = go.GetComponentsInChildren<Renderer>(true);
                int inactive = transforms.Count(t => !t.gameObject.activeSelf);
                int disabledRenderers = renderers.Count(r => !r.enabled);
                long totalVerts = 0; int emptyMeshes = 0;
                var shaderUse = new Dictionary<string, int>();
                var problems = new List<string>();
                foreach (var r in renderers)
                {
                    var mesh = MeshOf(r);
                    if (mesh == null) { problems.Add($"No mesh: `{PathOf(r.transform, go.transform)}`"); continue; }
                    totalVerts += mesh.vertexCount;
                    if (TriangleCount(mesh) == 0) emptyMeshes++;
                    foreach (var m in r.sharedMaterials)
                    {
                        string key = m == null ? "(null material)" : $"{m.name} | {(m.shader ? m.shader.name : "(no shader)")}";
                        shaderUse[key] = shaderUse.TryGetValue(key, out int c) ? c + 1 : 1;
                        if (m == null || m.shader == null || !m.shader.isSupported || m.shader.name.Contains("InternalErrorShader"))
                            problems.Add($"Unsupported/missing material on `{PathOf(r.transform, go.transform)}`");
                    }
                }
                var fullBounds = CombinedBounds(renderers.Where(r => TriangleCount(MeshOf(r)) > 0));

                sb.AppendLine("## Whole model (as imported)");
                sb.AppendLine($"- GameObjects: {transforms.Length} (inactive in source: {inactive})");
                sb.AppendLine($"- Renderers: {renderers.Length} (disabled: {disabledRenderers}); meshes with 0 triangles (label/marker lines): {emptyMeshes}");
                sb.AppendLine($"- Total vertices: {totalVerts:N0}");
                sb.AppendLine($"- Bounds (m): centre {Fmt(fullBounds.center)}, size {Fmt(fullBounds.size)}");
                sb.AppendLine("- Top-level hierarchy:");
                foreach (Transform child in go.transform)
                {
                    sb.AppendLine($"  - `{child.name}` ({child.childCount} children)");
                    foreach (Transform g in child) sb.AppendLine($"    - `{g.name}` ({g.childCount} children)");
                }
                sb.AppendLine("- Materials and shaders on renderers:");
                foreach (var kv in shaderUse.OrderBy(k => k.Key)) sb.AppendLine($"  - {kv.Key}: {kv.Value}");
                sb.AppendLine();

                var subset = ResolveSubset(go.transform, out var missing);
                sb.AppendLine("## Heart scene subset");
                if (missing.Count > 0) sb.AppendLine($"- **Missing node names:** {string.Join(", ", missing)}");
                var subsetRenderers = subset.Select(t => t.GetComponent<Renderer>()).Where(r => r != null).ToList();
                var drawn = subsetRenderers.Where(r => TriangleCount(MeshOf(r)) > 0).ToList();
                var hb = CombinedBounds(drawn);
                sb.AppendLine($"- Renderers in subset: {subsetRenderers.Count} (with triangles: {drawn.Count})");
                sb.AppendLine($"- Bounds (m): centre {Fmt(hb.center)}, size {Fmt(hb.size)}");
                sb.AppendLine();
                sb.AppendLine("| Path | Vertices | Triangles | Material | Size (cm) |");
                sb.AppendLine("|---|---:|---:|---|---|");
                foreach (var r in subsetRenderers.OrderBy(r => PathOf(r.transform, go.transform)))
                {
                    var mesh = MeshOf(r);
                    string mats = string.Join(", ", r.sharedMaterials.Select(m => m ? m.name : "null"));
                    sb.AppendLine($"| `{PathOf(r.transform, go.transform)}` | {mesh.vertexCount} | {TriangleCount(mesh)} | {mats} | {Fmt(r.bounds.size * 100f, "0.0")} |");
                }
                sb.AppendLine();
                sb.AppendLine("## Orientation check");
                AppendOrientation(sb, go.transform);
                sb.AppendLine();
                sb.AppendLine("## Problems");
                if (problems.Count == 0) sb.AppendLine("- None detected (no missing meshes, no unsupported shaders).");
                foreach (var p in problems.Distinct().Take(50)) sb.AppendLine($"- {p}");

                Directory.CreateDirectory(DocsFolder);
                File.WriteAllText(ReportPath, sb.ToString());
                Debug.Log($"[AnatoVerse3D] Import report written to {ReportPath}");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(preview);
            }
        }

        static void AppendOrientation(StringBuilder sb, Transform root)
        {
            var rv = Centroid(Find(root, "Right ventricle"));
            var la = Centroid(Find(root, "Left atrium"));
            var lv = Centroid(Find(root, "Left ventricle"));
            var ra = Centroid(Find(root, "Right atrium"));
            if (rv == null || la == null || lv == null || ra == null)
            {
                sb.AppendLine("- Chamber meshes not found; orientation not checked.");
                return;
            }
            Vector3 anterior = Vector3.ProjectOnPlane(rv.Value - la.Value, Vector3.up).normalized;
            Vector3 patientLeft = (lv.Value - ra.Value).normalized;
            sb.AppendLine($"- Anterior direction (right ventricle − left atrium, horizontal): {Fmt(anterior, "0.00")}");
            sb.AppendLine($"- Patient-left direction (left ventricle − right atrium): {Fmt(patientLeft, "0.00")}");
            sb.AppendLine($"- Apex below base (left ventricle lower than left atrium): {(lv.Value.y < la.Value.y ? "yes" : "NO")}");
            // In an anterior view the camera looks along -anterior; viewer's right = Cross(up, viewDir)... computed explicitly:
            Vector3 viewDir = -anterior;
            Vector3 viewerRight = Vector3.Cross(Vector3.up, viewDir).normalized;
            float d = Vector3.Dot(patientLeft, viewerRight);
            sb.AppendLine($"- Anterior view shows the patient's left on the viewer's right (expected for an unmirrored model): {(d > 0 ? "yes" : "NO — model may be mirrored")} (dot {d:0.00})");
        }

        // ------------------------------------------------------------------ scene

        [MenuItem(MenuRoot + "4. Build HeartViewer scene", false, 14)]
        static void BuildHeartViewerSceneMenu() => BuildHeartViewerScene();

        public static bool BuildHeartViewerScene()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (prefab == null) { Debug.LogError($"[AnatoVerse3D] Model not found at {ModelPath}"); return false; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;

            EnsureFolder(SceneFolder);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("HeartViewer");
            var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            model.name = "CardioVascular41 (Z-Anatomy)";
            model.transform.SetParent(root.transform, false);

            var subset = ResolveSubset(model.transform, out var missing);
            if (missing.Count > 0) Debug.LogError($"[AnatoVerse3D] Missing node names: {string.Join(", ", missing)}");

            // Keep included nodes and their ancestors active; deactivate only the top-most excluded nodes.
            var keep = new HashSet<Transform>(subset);
            foreach (var t in subset)
                for (var p = t.parent; p != null && p != root.transform; p = p.parent) keep.Add(p);
            int deactivated = 0, hiddenEmpty = 0;
            foreach (var t in model.GetComponentsInChildren<Transform>(true))
            {
                if (t == model.transform) continue;
                if (!keep.Contains(t) && keep.Contains(t.parent)) { t.gameObject.SetActive(false); deactivated++; }
            }
            foreach (var t in keep)
            {
                var r = t.GetComponent<Renderer>();
                if (r == null) continue;
                // Ancestors kept only for hierarchy, and zero-triangle label/marker meshes, are not drawn.
                if (!subset.Contains(t) || TriangleCount(MeshOf(r)) == 0) { r.enabled = false; hiddenEmpty++; }
            }

            var drawn = subset.Select(t => t.GetComponent<Renderer>())
                              .Where(r => r != null && r.enabled && TriangleCount(MeshOf(r)) > 0).ToList();
            var bounds = CombinedBounds(drawn);

            var target = new GameObject("HeartViewTarget");
            target.transform.SetParent(root.transform, false);
            target.transform.position = bounds.center;

            // Camera: anterior view framed on the actual renderer bounds.
            Vector3 anterior = Vector3.forward;
            var rv = Centroid(Find(model.transform, "Right ventricle"));
            var la = Centroid(Find(model.transform, "Left atrium"));
            if (rv != null && la != null)
            {
                var a = Vector3.ProjectOnPlane(rv.Value - la.Value, Vector3.up);
                if (a.sqrMagnitude > 1e-8f) anterior = a.normalized;
            }
            float radius = bounds.extents.magnitude;
            const float fov = 35f;
            float distance = radius / Mathf.Sin(0.5f * fov * Mathf.Deg2Rad) * 1.05f;

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            cam.fieldOfView = fov;
            cam.nearClipPlane = Mathf.Max(0.005f, (distance - radius) * 0.25f);
            cam.farClipPlane = distance + radius * 10f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.09f, 0.10f, 0.12f);
            camGo.transform.position = bounds.center + anterior * distance;
            camGo.transform.LookAt(bounds.center, Vector3.up);

            // Lighting: key + fill directional lights and trilight ambient (no shadows at this stage).
            Vector3 right = Vector3.Cross(Vector3.up, -anterior).normalized;
            var key = CreateLight("Key Light", 1.2f, new Color(1f, 0.97f, 0.92f),
                -(anterior + Vector3.up * 0.8f + right * 0.6f).normalized);
            var fill = CreateLight("Fill Light", 0.35f, new Color(0.85f, 0.9f, 1f),
                -(anterior * 0.6f - Vector3.up * 0.2f - right * 0.9f).normalized);
            key.transform.SetParent(root.transform, true);
            fill.transform.SetParent(root.transform, true);

            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.55f, 0.60f);
            RenderSettings.ambientEquatorColor = new Color(0.35f, 0.33f, 0.33f);
            RenderSettings.ambientGroundColor = new Color(0.15f, 0.14f, 0.14f);

            Selection.activeGameObject = root;
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[AnatoVerse3D] HeartViewer scene {(saved ? "saved" : "NOT saved")} at {ScenePath}. " +
                      $"Drawn parts: {drawn.Count}, deactivated branches: {deactivated}, hidden helper renderers: {hiddenEmpty}, " +
                      $"bounds centre {Fmt(bounds.center)} size {Fmt(bounds.size)}, camera distance {distance:0.000} m.");

            WriteSceneCheck(drawn, model.transform, bounds, distance, cam);

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null) sceneView.Frame(bounds, false);
            return saved;
        }

        static void WriteSceneCheck(List<Renderer> drawn, Transform modelRoot, Bounds bounds, float distance, Camera cam)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# HeartViewer scene check");
            sb.AppendLine();
            sb.AppendLine($"Generated {DateTime.Now:yyyy-MM-dd HH:mm} from the saved scene instance (Unity {Application.unityVersion}).");
            sb.AppendLine();
            sb.AppendLine($"- Drawn parts: {drawn.Count}");
            sb.AppendLine($"- Bounds (m): centre {Fmt(bounds.center)}, size {Fmt(bounds.size)}");
            sb.AppendLine($"- Camera: position {Fmt(cam.transform.position)}, distance {distance:0.000} m, FOV {cam.fieldOfView}, near {cam.nearClipPlane:0.0000}, far {cam.farClipPlane:0.000}");
            sb.AppendLine();
            sb.AppendLine("## Materials actually assigned in the scene");
            foreach (var g in drawn.SelectMany(r => r.sharedMaterials).Where(m => m != null).GroupBy(m => m).OrderBy(g => g.Key.name))
            {
                var m = g.Key;
                string col = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor").ToString("F2") : "(no _BaseColor)";
                sb.AppendLine($"- {m.name} — shader `{m.shader.name}`, _BaseColor {col}, used by {g.Count()} renderers");
            }
            sb.AppendLine();
            sb.AppendLine("## Drawn parts");
            foreach (var r in drawn.OrderBy(r => PathOf(r.transform, modelRoot)))
                sb.AppendLine($"- `{PathOf(r.transform, modelRoot)}` — {string.Join(", ", r.sharedMaterials.Select(m => m ? m.name : "null"))}");
            Directory.CreateDirectory(DocsFolder);
            File.WriteAllText(Path.Combine(DocsFolder, "scene-check.md"), sb.ToString());
        }

        static Light CreateLight(string name, float intensity, Color color, Vector3 direction)
        {
            var go = new GameObject(name);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = intensity;
            light.color = color;
            light.shadows = LightShadows.None;
            go.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            return light;
        }

        // ------------------------------------------------------------------ screenshot

        [MenuItem(MenuRoot + "5. Capture screenshot", false, 15)]
        public static void CaptureScreenshot()
        {
            var cam = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (cam == null) { Debug.LogError("[AnatoVerse3D] No camera in the open scene."); return; }

            const int w = 1600, h = 1000;
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 1 };
            var previousTarget = cam.targetTexture;
            try
            {
                var request = new RenderPipeline.StandardRequest();
                if (RenderPipeline.SupportsRenderRequest(cam, request))
                {
                    request.destination = rt;
                    RenderPipeline.SubmitRenderRequest(cam, request);
                }
                else
                {
                    cam.targetTexture = rt;
                    cam.Render();
                }

                var previousActive = RenderTexture.active;
                RenderTexture.active = rt;
                var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply();
                RenderTexture.active = previousActive;

                Directory.CreateDirectory(DocsFolder);
                File.WriteAllBytes(ScreenshotPath, tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
                Debug.Log($"[AnatoVerse3D] Screenshot saved to {ScreenshotPath}");
            }
            finally
            {
                cam.targetTexture = previousTarget;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        // ------------------------------------------------------------------ helpers

        static ModelImporter GetImporter()
        {
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (importer == null)
                Debug.LogError($"[AnatoVerse3D] No model at {ModelPath}. Copy CardioVascular41.fbx there first.");
            return importer;
        }

        static HashSet<Transform> ResolveSubset(Transform modelRoot, out List<string> missing)
        {
            missing = new List<string>();
            var set = new HashSet<Transform>();
            foreach (var name in IncludeSubtree)
            {
                var t = Find(modelRoot, name);
                if (t == null) { missing.Add(name); continue; }
                foreach (var d in t.GetComponentsInChildren<Transform>(true)) set.Add(d);
            }
            foreach (var name in IncludeSelfOnly)
            {
                var t = Find(modelRoot, name);
                if (t == null) { missing.Add(name); continue; }
                set.Add(t);
            }
            return set;
        }

        static Transform Find(Transform root, string exactName)
        {
            var matches = root.GetComponentsInChildren<Transform>(true).Where(t => t.name == exactName).ToList();
            if (matches.Count > 1) Debug.LogWarning($"[AnatoVerse3D] '{exactName}' matched {matches.Count} objects; using the first.");
            return matches.FirstOrDefault();
        }

        static Mesh MeshOf(Renderer r)
        {
            if (r is SkinnedMeshRenderer s) return s.sharedMesh;
            var mf = r != null ? r.GetComponent<MeshFilter>() : null;
            return mf != null ? mf.sharedMesh : null;
        }

        static long TriangleCount(Mesh mesh)
        {
            if (mesh == null) return 0;
            long n = 0;
            for (int i = 0; i < mesh.subMeshCount; i++)
                if (mesh.GetTopology(i) == MeshTopology.Triangles) n += mesh.GetIndexCount(i) / 3;
            return n;
        }

        static Vector3? Centroid(Transform t)
        {
            if (t == null) return null;
            var r = t.GetComponent<Renderer>();
            return r != null ? r.bounds.center : (Vector3?)null;
        }

        static Bounds CombinedBounds(IEnumerable<Renderer> renderers)
        {
            bool any = false; var b = new Bounds();
            foreach (var r in renderers)
            {
                if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds);
            }
            return b;
        }

        static void EnsureFolder(string assetFolder)
        {
            if (AssetDatabase.IsValidFolder(assetFolder)) return;
            string parent = Path.GetDirectoryName(assetFolder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetFolder));
        }

        static string PathOf(Transform t, Transform root)
        {
            var parts = new List<string>();
            for (var p = t; p != null && p != root; p = p.parent) parts.Add(p.name);
            parts.Reverse();
            return string.Join(" / ", parts);
        }

        static string Fmt(Vector3 v, string f = "0.000") =>
            $"({v.x.ToString(f)}, {v.y.ToString(f)}, {v.z.ToString(f)})";
    }
}
