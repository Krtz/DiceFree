using UnityEngine;

namespace DiceFree.World
{
    /// <summary>Absolute, reversible motion of a dedicated visual child. Never moves the NPC root.</summary>
    [DisallowMultipleComponent]
    public sealed class VillageVisualIdle : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        [SerializeField] private int seed = 1;
        [SerializeField, Range(0, 2)] private float swayDegrees = .65f;
        [SerializeField, Range(0, .02f)] private float bobMeters = .003f;
        private Vector3 position, scale;
        private Quaternion rotation;
        private bool captured;
        public Transform Visual => visual;
        public void ConfigureVisibleMotion() { swayDegrees = 2f; bobMeters = .015f; }
        public void Configure(Transform child, int phaseSeed) { Restore(); visual = child; seed = phaseSeed; Capture(); }
        private void OnEnable() => Capture();
        private void Capture()
        {
            if (visual == null || visual == transform || !visual.IsChildOf(transform)) return;
            position = visual.localPosition; rotation = visual.localRotation; scale = visual.localScale; captured = true;
        }
        public void Sample(float seconds)
        {
            if (!captured || visual == null) return;
            float t = seconds + (uint)seed % 1009 * .173f;
            float breath = Mathf.Sin(t * 1.65f);
            // A smooth occasional glance, with a long quiet interval and no discontinuous random targets.
            float glance = Mathf.Pow(Mathf.Max(0, Mathf.Sin(t * .27f)), 8) * Mathf.Sin(t * .63f) * 2.1f;
            visual.localRotation = rotation * Quaternion.Euler(breath * swayDegrees * .25f, glance, Mathf.Sin(t * .91f) * swayDegrees);
            var parent = visual.parent;
            visual.localPosition = position + parent.InverseTransformVector(Vector3.up * (breath * bobMeters));
            visual.localScale = new Vector3(scale.x, scale.y * (1 + breath * .008f), scale.z);
        }
        private void LateUpdate() => Sample(Time.time);
        private void OnDisable() => Restore();
        private void OnDestroy() => Restore();
        private void Restore()
        {
            if (captured && visual != null) { visual.localPosition = position; visual.localRotation = rotation; visual.localScale = scale; }
            captured = false;
        }
    }
}
