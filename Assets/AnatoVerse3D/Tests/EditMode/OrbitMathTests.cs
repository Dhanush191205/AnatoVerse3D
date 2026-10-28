using NUnit.Framework;
using UnityEngine;

namespace AnatoVerse3D.Tests
{
    public class OrbitMathTests
    {
        const float Eps = 1e-4f;

        [TestCase(0f, 0f, 0.5f)]
        [TestCase(180f, 0f, 0.5f)]
        [TestCase(45f, 30f, 1.2f)]
        [TestCase(-120f, -60f, 0.2f)]
        public void PoseRoundTrip_RecoversCameraPosition(float yaw, float pitch, float distance)
        {
            var pivot = new Vector3(0.1f, 1.3f, -0.02f);
            Vector3 pos = OrbitMath.CameraPosition(pivot, yaw, pitch, distance);
            OrbitMath.FromPose(pos, pivot, out float y, out float p, out float d);
            Vector3 again = OrbitMath.CameraPosition(pivot, y, p, d);
            Assert.That(Vector3.Distance(pos, again), Is.LessThan(Eps));
            Assert.That(d, Is.EqualTo(distance).Within(Eps));
        }

        [Test]
        public void CameraLooksAtPivot()
        {
            var pivot = new Vector3(0f, 1.28f, 0f);
            Vector3 pos = OrbitMath.CameraPosition(pivot, 30f, 20f, 0.5f);
            Vector3 forward = OrbitMath.Rotation(30f, 20f) * Vector3.forward;
            Assert.That(Vector3.Angle(forward, pivot - pos), Is.LessThan(0.01f));
        }

        [Test]
        public void AnteriorView_CameraOnPositiveZ_GivesYaw180()
        {
            OrbitMath.FromPose(new Vector3(0f, 0f, 1f), Vector3.zero, out float yaw, out float pitch, out _);
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(yaw, 180f)), Is.LessThan(Eps));
            Assert.That(pitch, Is.EqualTo(0f).Within(Eps));
        }

        [Test]
        public void PositivePitch_PlacesCameraAbovePivot()
        {
            Vector3 pos = OrbitMath.CameraPosition(Vector3.zero, 0f, 40f, 1f);
            Assert.That(pos.y, Is.GreaterThan(0f));
        }

        [Test]
        public void ClampPitch_StaysWithinLimit()
        {
            Assert.That(OrbitMath.ClampPitch(120f, 85f), Is.EqualTo(85f));
            Assert.That(OrbitMath.ClampPitch(-120f, 85f), Is.EqualTo(-85f));
            Assert.That(OrbitMath.ClampPitch(10f, 85f), Is.EqualTo(10f));
        }

        [Test]
        public void Zoom_InAndOut_RespectsLimits()
        {
            Assert.That(OrbitMath.ZoomedDistance(0.5f, 1f, 0.1f, 0.1f, 2f), Is.EqualTo(0.45f).Within(Eps));
            Assert.That(OrbitMath.ZoomedDistance(0.5f, -1f, 0.1f, 0.1f, 2f), Is.GreaterThan(0.5f));
            Assert.That(OrbitMath.ZoomedDistance(0.5f, 100f, 0.1f, 0.1f, 2f), Is.EqualTo(0.1f));
            Assert.That(OrbitMath.ZoomedDistance(0.5f, -100f, 0.1f, 0.1f, 2f), Is.EqualTo(2f));
        }

        [Test]
        public void Zoom_InThenOut_ReturnsToStart()
        {
            float d = OrbitMath.ZoomedDistance(0.5f, 2f, 0.12f, 0.01f, 10f);
            d = OrbitMath.ZoomedDistance(d, -2f, 0.12f, 0.01f, 10f);
            Assert.That(d, Is.EqualTo(0.5f).Within(Eps));
        }

        [TestCase(120f, 1f)]
        [TestCase(-240f, -2f)]
        [TestCase(1f, 1f)]
        [TestCase(0.25f, 0.25f)]
        [TestCase(1200f, 3f)]
        public void ScrollToNotches_NormalisesAndClamps(float raw, float expected)
        {
            Assert.That(OrbitMath.ScrollToNotches(raw), Is.EqualTo(expected).Within(Eps));
        }

        [Test]
        public void WorldUnitsPerPixel_MatchesFrustumHeight()
        {
            // Frustum height at 1 m with 90 degree FOV is 2 m; over 1000 px that is 0.002 m/px.
            Assert.That(OrbitMath.WorldUnitsPerPixel(1f, 90f, 1000f), Is.EqualTo(0.002f).Within(1e-6f));
            Assert.That(OrbitMath.WorldUnitsPerPixel(1f, 90f, 0f), Is.EqualTo(0f));
        }

        [Test]
        public void ClampPivot_LimitsPanDistance()
        {
            Vector3 home = new Vector3(0f, 1f, 0f);
            Vector3 clamped = OrbitMath.ClampPivot(home + new Vector3(5f, 0f, 0f), home, 0.2f);
            Assert.That(Vector3.Distance(clamped, home), Is.EqualTo(0.2f).Within(Eps));
            Vector3 inside = home + new Vector3(0.05f, 0f, 0f);
            Assert.That(OrbitMath.ClampPivot(inside, home, 0.2f), Is.EqualTo(inside));
        }

        [Test]
        public void SmoothingFactor_IsBoundedAndInstantWhenDisabled()
        {
            Assert.That(OrbitMath.SmoothingFactor(0f, 0.016f), Is.EqualTo(1f));
            float t = OrbitMath.SmoothingFactor(14f, 0.016f);
            Assert.That(t, Is.GreaterThan(0f).And.LessThan(1f));
            Assert.That(OrbitMath.SmoothingFactor(14f, 10f), Is.EqualTo(1f).Within(1e-4f));
        }
    }
}
