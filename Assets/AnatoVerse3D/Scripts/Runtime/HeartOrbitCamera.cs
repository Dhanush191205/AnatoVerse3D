using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace AnatoVerse3D
{
    /// <summary>
    /// Orbit / pan / zoom / reset camera for inspecting the heart model.
    /// Mouse: left or right drag = orbit, middle drag or Shift + drag = pan, wheel = zoom.
    /// Keyboard: arrow keys = orbit, + / - = zoom, F = reset to the starting view.
    /// Input that starts over a UI element is ignored. A press that does not move beyond
    /// <see cref="dragThresholdPixels"/> is treated as a click and does not move the camera.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public sealed class HeartOrbitCamera : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Point the camera orbits around (HeartViewTarget at the heart's bounds centre).")]
        [SerializeField] Transform target;

        [Header("Limits")]
        [SerializeField, Min(0.001f)] float minDistance = 0.12f;
        [SerializeField, Min(0.01f)] float maxDistance = 1.2f;
        [SerializeField, Range(0f, 89f)] float maxPitch = 85f;
        [Tooltip("How far the orbit point may be panned away from the target (metres).")]
        [SerializeField, Min(0f)] float maxPanDistance = 0.2f;

        [Header("Speeds")]
        [SerializeField, Min(0f)] float orbitDegreesPerPixel = 0.3f;
        [SerializeField, Min(0f)] float keyOrbitDegreesPerSecond = 90f;
        [SerializeField, Range(0.01f, 0.5f)] float zoomStepPerNotch = 0.12f;
        [SerializeField, Min(0f)] float keyZoomNotchesPerSecond = 4f;
        [Tooltip("Higher = snappier. 0 = no smoothing.")]
        [SerializeField, Min(0f)] float smoothing = 14f;
        [SerializeField, Min(0f)] float dragThresholdPixels = 4f;

        [Header("Keys")]
        [SerializeField] Key resetKey = Key.F;

        struct ViewState
        {
            public Vector3 Pivot;
            public float Yaw, Pitch, Distance;
        }

        Camera cam;
        ViewState home, goal, current;
        bool hasHome;
        bool anyButtonHeldLastFrame;
        bool gestureStartedOverUI;
        bool dragging;
        Vector2 pressPosition;

        public Transform Target { get => target; set => target = value; }
        public float MinDistance => minDistance;
        public float MaxDistance => maxDistance;
        public float CurrentDistance => current.Distance;
        /// <summary>True while a mouse drag is moving the camera (Stage 3 uses this to tell clicks from drags).</summary>
        public bool IsDragging => dragging;

        public void SetLimits(float min, float max, float maxPan)
        {
            minDistance = Mathf.Max(0.001f, min);
            maxDistance = Mathf.Max(minDistance, max);
            maxPanDistance = Mathf.Max(0f, maxPan);
        }

        void Awake() => cam = GetComponent<Camera>();

        void OnEnable() => CaptureHome();

        /// <summary>Uses the camera's current pose as the reset ("home") view.</summary>
        public void CaptureHome()
        {
            if (cam == null) cam = GetComponent<Camera>();
            Vector3 pivot;
            if (target != null) pivot = target.position;
            else
            {
                Debug.LogWarning("[AnatoVerse3D] HeartOrbitCamera has no target; orbiting a point 0.5 m ahead.", this);
                pivot = transform.position + transform.forward * 0.5f;
            }
            OrbitMath.FromPose(transform.position, pivot, out float yaw, out float pitch, out float distance);
            home = new ViewState
            {
                Pivot = pivot,
                Yaw = yaw,
                Pitch = OrbitMath.ClampPitch(pitch, maxPitch),
                Distance = Mathf.Clamp(distance, minDistance, maxDistance)
            };
            goal = current = home;
            hasHome = true;
            Apply(current);
        }

        /// <summary>Returns smoothly to the starting view.</summary>
        public void ResetView() => goal = home;

        void LateUpdate()
        {
            if (!hasHome) CaptureHome();
            float dt = Time.unscaledDeltaTime;
            ReadMouse();
            ReadKeyboard(dt);

            float t = OrbitMath.SmoothingFactor(smoothing, dt);
            current.Pivot = Vector3.Lerp(current.Pivot, goal.Pivot, t);
            current.Yaw = Mathf.Lerp(current.Yaw, goal.Yaw, t);
            current.Pitch = Mathf.Lerp(current.Pitch, goal.Pitch, t);
            current.Distance = Mathf.Lerp(current.Distance, goal.Distance, t);
            Apply(current);
        }

        void ReadMouse()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            bool left = mouse.leftButton.isPressed, right = mouse.rightButton.isPressed, middle = mouse.middleButton.isPressed;
            bool anyHeld = left || right || middle;
            Vector2 position = mouse.position.ReadValue();

            if (anyHeld && !anyButtonHeldLastFrame)
            {
                // A new gesture starts: remember where, and whether it began on UI.
                gestureStartedOverUI = IsPointerOverUI();
                pressPosition = position;
                dragging = false;
            }
            if (!anyHeld)
            {
                gestureStartedOverUI = false;
                dragging = false;
            }
            anyButtonHeldLastFrame = anyHeld;

            if (anyHeld && !gestureStartedOverUI)
            {
                if (!dragging && (position - pressPosition).sqrMagnitude >= dragThresholdPixels * dragThresholdPixels)
                    dragging = true;

                Vector2 delta = mouse.delta.ReadValue();
                if (dragging && delta != Vector2.zero)
                {
                    var keyboard = Keyboard.current;
                    bool shift = keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
                    if (middle || shift) Pan(delta);
                    else Orbit(delta.x * orbitDegreesPerPixel, -delta.y * orbitDegreesPerPixel);
                }
            }

            float scroll = mouse.scroll.ReadValue().y;
            if (scroll != 0f && !IsPointerOverUI() && !(anyHeld && gestureStartedOverUI))
                Zoom(OrbitMath.ScrollToNotches(scroll));
        }

        void ReadKeyboard(float dt)
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            float yaw = 0f, pitch = 0f;
            if (kb.leftArrowKey.isPressed) yaw += 1f;
            if (kb.rightArrowKey.isPressed) yaw -= 1f;
            if (kb.upArrowKey.isPressed) pitch += 1f;
            if (kb.downArrowKey.isPressed) pitch -= 1f;
            if (yaw != 0f || pitch != 0f)
                Orbit(yaw * keyOrbitDegreesPerSecond * dt, pitch * keyOrbitDegreesPerSecond * dt);

            float zoom = 0f;
            if (kb.equalsKey.isPressed || kb.numpadPlusKey.isPressed) zoom += 1f;
            if (kb.minusKey.isPressed || kb.numpadMinusKey.isPressed) zoom -= 1f;
            if (zoom != 0f) Zoom(zoom * keyZoomNotchesPerSecond * dt);

            if (kb[resetKey].wasPressedThisFrame) ResetView();
        }

        void Orbit(float yawDelta, float pitchDelta)
        {
            goal.Yaw += yawDelta;
            goal.Pitch = OrbitMath.ClampPitch(goal.Pitch + pitchDelta, maxPitch);
        }

        void Pan(Vector2 pixelDelta)
        {
            float unitsPerPixel = OrbitMath.WorldUnitsPerPixel(goal.Distance, cam.fieldOfView, Screen.height);
            Quaternion rot = OrbitMath.Rotation(goal.Yaw, goal.Pitch);
            Vector3 move = (rot * Vector3.right * pixelDelta.x + rot * Vector3.up * pixelDelta.y) * unitsPerPixel;
            goal.Pivot = OrbitMath.ClampPivot(goal.Pivot - move, home.Pivot, maxPanDistance);
        }

        void Zoom(float notches)
        {
            goal.Distance = OrbitMath.ZoomedDistance(goal.Distance, notches, zoomStepPerNotch, minDistance, maxDistance);
        }

        void Apply(ViewState s)
        {
            Quaternion rot = OrbitMath.Rotation(s.Yaw, s.Pitch);
            transform.SetPositionAndRotation(OrbitMath.CameraPosition(s.Pivot, s.Yaw, s.Pitch, s.Distance), rot);
            if (cam != null)
            {
                // Keep depth precision good at close range without clipping the model.
                cam.nearClipPlane = Mathf.Max(0.002f, s.Distance * 0.05f);
                cam.farClipPlane = s.Distance + maxDistance * 2f;
            }
        }

        static bool IsPointerOverUI() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
