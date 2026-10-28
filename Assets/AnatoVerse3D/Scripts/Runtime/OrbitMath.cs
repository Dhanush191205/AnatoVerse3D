using UnityEngine;

namespace AnatoVerse3D
{
    /// <summary>
    /// Pure camera maths used by <see cref="HeartOrbitCamera"/>. Kept separate so it can be unit-tested.
    /// Convention: rotation = Euler(pitch, yaw, 0); camera position = pivot - rotation * forward * distance.
    /// Positive pitch looks down on the target.
    /// </summary>
    public static class OrbitMath
    {
        public static Quaternion Rotation(float yawDeg, float pitchDeg) => Quaternion.Euler(pitchDeg, yawDeg, 0f);

        public static Vector3 CameraPosition(Vector3 pivot, float yawDeg, float pitchDeg, float distance) =>
            pivot - Rotation(yawDeg, pitchDeg) * Vector3.forward * distance;

        /// <summary>Recovers yaw, pitch and distance from a camera position looking at a pivot.</summary>
        public static void FromPose(Vector3 cameraPosition, Vector3 pivot, out float yawDeg, out float pitchDeg, out float distance)
        {
            Vector3 offset = cameraPosition - pivot;
            distance = offset.magnitude;
            if (distance < 1e-6f) { yawDeg = 0f; pitchDeg = 0f; return; }
            Vector3 forward = -offset / distance;
            yawDeg = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            pitchDeg = Mathf.Asin(Mathf.Clamp(-forward.y, -1f, 1f)) * Mathf.Rad2Deg;
        }

        public static float ClampPitch(float pitchDeg, float maxPitchDeg) =>
            Mathf.Clamp(pitchDeg, -maxPitchDeg, maxPitchDeg);

        /// <summary>Exponential zoom: each notch scales the distance by (1 - step). Positive notches zoom in.</summary>
        public static float ZoomedDistance(float distance, float notches, float stepPerNotch, float min, float max) =>
            Mathf.Clamp(distance * Mathf.Pow(1f - stepPerNotch, notches), min, max);

        /// <summary>
        /// Converts a raw scroll value to wheel notches. Windows mice report 120 per notch on some
        /// Input System settings and about 1 per notch on others; trackpads report small values.
        /// The result is clamped so a single fast flick cannot jump through the zoom range.
        /// </summary>
        public static float ScrollToNotches(float rawScrollY)
        {
            float notches = Mathf.Abs(rawScrollY) > 10f ? rawScrollY / 120f : rawScrollY;
            return Mathf.Clamp(notches, -3f, 3f);
        }

        /// <summary>World units moved per screen pixel at the pivot's depth for a perspective camera.</summary>
        public static float WorldUnitsPerPixel(float distance, float verticalFovDeg, float screenHeightPixels)
        {
            if (screenHeightPixels <= 0f) return 0f;
            return 2f * distance * Mathf.Tan(0.5f * verticalFovDeg * Mathf.Deg2Rad) / screenHeightPixels;
        }

        /// <summary>Keeps the pan pivot within maxOffset of the home pivot.</summary>
        public static Vector3 ClampPivot(Vector3 pivot, Vector3 homePivot, float maxOffset) =>
            homePivot + Vector3.ClampMagnitude(pivot - homePivot, maxOffset);

        /// <summary>Frame-rate independent smoothing factor in [0, 1].</summary>
        public static float SmoothingFactor(float sharpness, float deltaTime) =>
            sharpness <= 0f ? 1f : 1f - Mathf.Exp(-sharpness * Mathf.Max(0f, deltaTime));
    }
}
