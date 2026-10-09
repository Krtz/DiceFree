using UnityEngine;

namespace DiceFree.World
{
    /// <summary>Visual-only idle and walk cycles. Never translates NPC roots or changes navigation.</summary>
    [DisallowMultipleComponent]
    public sealed class VillageVisualIdle : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        [SerializeField] private int seed = 1;
        [SerializeField, Range(0, 5)] private float swayDegrees = 2f;
        [SerializeField, Range(0, .05f)] private float bobMeters = .015f;
        [SerializeField, Range(0, 8)] private float walkSwingDegrees = 4.2f;
        private Vector3 position, scale;
        private Quaternion rotation;
        private bool captured;
        private VillageHomeWander wander;
        private float locomotion;
        public Transform Visual => visual;
        public void ConfigureVisibleMotion() { swayDegrees = 2f; bobMeters = .015f; walkSwingDegrees = 4.2f; }
        public void Configure(Transform child, int phaseSeed) { Restore(); visual = child; seed = phaseSeed; Capture(); }
        private void OnEnable() { wander = GetComponent<VillageHomeWander>(); Capture(); }
        private void Capture()
        {
            if (visual == null || visual == transform || !visual.IsChildOf(transform)) return;
            position = visual.localPosition; rotation = visual.localRotation; scale = visual.localScale; captured = true;
        }
        public void Sample(float seconds)
        {
            if (!captured || visual == null) return;
            float phase = seconds + (uint)seed % 1009 * .173f;
            bool walking = wander != null && wander.enabled && wander.Walking;
            locomotion = Mathf.MoveTowards(locomotion, walking ? 1f : 0f, Mathf.Max(0, Time.deltaTime) * 3.5f);
            float breath = Mathf.Sin(phase * 1.65f);
            float glance = Mathf.Pow(Mathf.Max(0, Mathf.Sin(phase * .27f)), 8) * Mathf.Sin(phase * .63f) * 2.1f;
            float gait = seconds * 10.0f + (uint)seed % 13;
            float step = Mathf.Sin(gait);
            float bounce = Mathf.Abs(Mathf.Sin(gait));
            var idleRotation = Quaternion.Euler(breath * swayDegrees * .25f, glance, Mathf.Sin(phase * .91f) * swayDegrees);
            var walkingRotation = Quaternion.Euler(step * walkSwingDegrees, step * 1.4f, -step * walkSwingDegrees * .35f);
            visual.localRotation = rotation * Quaternion.Slerp(idleRotation, walkingRotation, locomotion);
            var parent = visual.parent;
            float height = Mathf.Lerp(breath * bobMeters, bounce * .022f, locomotion);
            visual.localPosition = position + parent.InverseTransformVector(Vector3.up * height);
            float breathing = 1 + breath * .008f * (1 - locomotion);
            visual.localScale = new Vector3(scale.x, scale.y * breathing, scale.z);
        }
        private void LateUpdate() => Sample(Time.time);
        private void OnDisable() => Restore();
        private void OnDestroy() => Restore();
        private void Restore()
        {
            if (captured && visual != null) { visual.localPosition = position; visual.localRotation = rotation; visual.localScale = scale; }
            captured = false; locomotion = 0;
        }
    }
}
