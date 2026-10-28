// AnatoVerse3D — Stage 2 editor tooling.
// Adds HeartOrbitCamera to the HeartViewer camera, sets zoom/pan limits from the drawn heart bounds,
// and adds a small on-screen controls panel with an Input System EventSystem.

using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace AnatoVerse3D.EditorTools
{
    public static class Stage2CameraSetup
    {
        const string MenuPath = "AnatoVerse3D/Stage 2/Set up camera controls";
        const string HelpCanvasName = "Controls Help";

        public const string HelpText =
            "<b>Controls</b>\n" +
            "Left or right drag — rotate\n" +
            "Middle drag or Shift + drag — pan\n" +
            "Mouse wheel or + / − — zoom\n" +
            "Arrow keys — rotate\n" +
            "F — reset view";

        [MenuItem(MenuPath, false, 100)]
        public static void SetUp()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(Stage1HeartSetup.ScenePath, OpenSceneMode.Single);

            var cam = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Camera>(true))
                           .FirstOrDefault(c => c.CompareTag("MainCamera"));
            var root = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "HeartViewer");
            var target = root != null ? root.transform.Find("HeartViewTarget") : null;
            if (cam == null || root == null || target == null)
            {
                Debug.LogError("[AnatoVerse3D] HeartViewer scene is missing Main Camera, HeartViewer or HeartViewTarget. " +
                               "Run AnatoVerse3D → Stage 1 → 4. Build HeartViewer scene first.");
                return;
            }

            // Radius of what is actually drawn (active, enabled renderers with geometry).
            var drawn = root.GetComponentsInChildren<Renderer>(false)
                            .Where(r => r.enabled && r.bounds.size.sqrMagnitude > 0f).ToList();
            if (drawn.Count == 0)
            {
                Debug.LogError("[AnatoVerse3D] No drawn renderers under HeartViewer; rebuild the Stage 1 scene.");
                return;
            }
            var bounds = drawn[0].bounds;
            foreach (var r in drawn) bounds.Encapsulate(r.bounds);
            float radius = bounds.extents.magnitude;

            // Centre the orbit on the drawn bounds (Stage 1 placed it there; refresh in case the subset changed).
            target.position = bounds.center;

            var orbit = cam.GetComponent<HeartOrbitCamera>();
            if (orbit == null) orbit = Undo.AddComponent<HeartOrbitCamera>(cam.gameObject);
            orbit.Target = target;
            float minD = radius * 0.8f, maxD = radius * 8f, maxPan = radius * 1.5f;
            orbit.SetLimits(minD, maxD, maxPan);
            EditorUtility.SetDirty(orbit);

            EnsureEventSystem(scene);
            EnsureHelpPanel(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);

            float startDistance = Vector3.Distance(cam.transform.position, target.position);
            var sb = new StringBuilder();
            sb.AppendLine("# Stage 2 camera setup");
            sb.AppendLine();
            sb.AppendLine($"Generated {DateTime.Now:yyyy-MM-dd HH:mm} by `{MenuPath}` (Unity {Application.unityVersion}).");
            sb.AppendLine();
            sb.AppendLine($"- Drawn renderers: {drawn.Count}; bounds centre {bounds.center.ToString("F3")}, size {bounds.size.ToString("F3")}, radius {radius:0.000} m");
            sb.AppendLine($"- Orbit target: `HeartViewer/HeartViewTarget` at {target.position.ToString("F3")}");
            sb.AppendLine($"- Start distance: {startDistance:0.000} m (default anterior view from Stage 1)");
            sb.AppendLine($"- Zoom limits: {minD:0.000} – {maxD:0.000} m (0.8× and 8× radius); pan limit {maxPan:0.000} m (1.5× radius)");
            sb.AppendLine($"- Start distance within limits: {(startDistance >= minD && startDistance <= maxD ? "yes" : "NO")}");
            sb.AppendLine("- EventSystem: InputSystemUIInputModule (Input System only project)");
            sb.AppendLine($"- Scene saved: {saved}");
            string dir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "docs", "stage2");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "camera-setup.md"), sb.ToString());

            Selection.activeGameObject = cam.gameObject;
            Debug.Log($"[AnatoVerse3D] Camera controls set up (radius {radius:0.000} m, zoom {minD:0.000}–{maxD:0.000} m). Scene saved: {saved}. Press Play to test.");
        }

        static void EnsureEventSystem(UnityEngine.SceneManagement.Scene scene)
        {
            var existing = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<EventSystem>(true)).FirstOrDefault();
            GameObject go = existing != null ? existing.gameObject : new GameObject("EventSystem");
            if (go.GetComponent<EventSystem>() == null) go.AddComponent<EventSystem>();
            var legacy = go.GetComponent<StandaloneInputModule>();
            if (legacy != null) UnityEngine.Object.DestroyImmediate(legacy);   // would error in an Input System only project
            if (go.GetComponent<InputSystemUIInputModule>() == null) go.AddComponent<InputSystemUIInputModule>();
        }

        static void EnsureHelpPanel(UnityEngine.SceneManagement.Scene scene)
        {
            var old = scene.GetRootGameObjects().FirstOrDefault(g => g.name == HelpCanvasName);
            if (old != null) UnityEngine.Object.DestroyImmediate(old);

            var canvasGo = new GameObject(HelpCanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasGo.transform, false);
            var prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0f, 0f);
            prt.anchoredPosition = new Vector2(24f, 24f);
            prt.sizeDelta = new Vector2(430f, 200f);
            var img = panel.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.55f);
            img.raycastTarget = true;   // pointer over the panel counts as UI, so the camera ignores it

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(panel.transform, false);
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(18f, 14f); trt.offsetMax = new Vector2(-18f, -14f);
            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.lineSpacing = 1.1f;
            text.supportRichText = true;
            text.color = new Color(0.95f, 0.95f, 0.95f);
            text.alignment = TextAnchor.UpperLeft;
            text.raycastTarget = false;
            text.text = HelpText;
        }
    }
}
