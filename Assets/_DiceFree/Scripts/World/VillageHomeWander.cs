using System;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.World
{
    /// <summary>Small home-bound NavMesh paths. No warp, NavMesh rebuild, or global random state.</summary>
    [DisallowMultipleComponent]
    public sealed class VillageHomeWander : MonoBehaviour
    {
        [SerializeField, Range(2, 4)] private float radius = 2.5f;
        [SerializeField, Range(2, 7)] private float minimumIdle = 2;
        [SerializeField, Range(2, 7)] private float maximumIdle = 7;
        [SerializeField, Range(1.1f, 1.5f)] private float speed = 1.2f;
        [SerializeField] private int seed = 1;
        private System.Random random;
        private NavMeshPath path;
        private Vector3 home;
        private float pause, heightOffset;
        private int corner;
        public Vector3 Home => home;
        public float Radius => radius;
        public bool Walking => path != null && corner < path.corners.Length;
        public void Configure(int value) => seed = value;
        private void OnEnable()
        {
            home = transform.position; random = new System.Random(seed); path = null; Pause();
        }
        private void OnDisable() { path = null; }
        private void Pause() { pause = Mathf.Lerp(Mathf.Clamp(minimumIdle, 2, 7), Mathf.Clamp(Mathf.Max(minimumIdle, maximumIdle), 2, 7), (float)random.NextDouble()); }
        public bool TryChoosePath()
        {
            path = null;
            if (!NavMesh.SamplePosition(transform.position, out var start, .65f, NavMesh.AllAreas)) return false;
            heightOffset = transform.position.y - start.position.y;
            for (int attempt = 0; attempt < 12; attempt++)
            {
                float a = (float)random.NextDouble() * Mathf.PI * 2;
                float r = Mathf.Sqrt((float)random.NextDouble()) * Mathf.Clamp(radius, 2, 4);
                var target = home + new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r);
                if (!NavMesh.SamplePosition(target, out var end, .5f, NavMesh.AllAreas) || Horizontal(end.position - home) > Radius || Horizontal(end.position - start.position) < .6f) continue;
                var candidate = new NavMeshPath();
                if (!NavMesh.CalculatePath(start.position, end.position, NavMesh.AllAreas, candidate) || candidate.status != NavMeshPathStatus.PathComplete || candidate.corners.Length < 2) continue;
                bool safe = true;
                foreach (var point in candidate.corners) if (Horizontal(point - home) > Radius) safe = false;
                if (!safe) continue;
                // Connect to the first corner without teleporting through an obstacle.
                if (NavMesh.Raycast(start.position, candidate.corners[0], out _, NavMesh.AllAreas)) continue;
                path = candidate; corner = 0; return true;
            }
            return false;
        }
        public void Step(float deltaTime)
        {
            if (!Walking)
            {
                pause -= deltaTime;
                if (pause <= 0) { TryChoosePath(); Pause(); }
                return;
            }
            var target = path.corners[corner] + Vector3.up * heightOffset;
            var next = Vector3.MoveTowards(transform.position, target, Mathf.Clamp(speed, 1.1f, 1.5f) * Mathf.Min(deltaTime, .1f));
            if (Horizontal(next - home) > Radius + .001f || !NavMesh.SamplePosition(transform.position - Vector3.up * heightOffset, out var current, .2f, NavMesh.AllAreas) || NavMesh.Raycast(current.position, next - Vector3.up * heightOffset, out _, NavMesh.AllAreas))
            { path = null; Pause(); return; }
            var direction = Vector3.ProjectOnPlane(next - transform.position, Vector3.up);
            if (direction.sqrMagnitude > .00001f)
            {
                var capsule = GetComponent<CapsuleCollider>();
                float footprint = capsule == null ? .3f : capsule.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
                foreach (var hit in Physics.SphereCastAll(transform.position + Vector3.up * .8f, footprint,
                    direction.normalized, direction.magnitude + .05f, (1 << 9) | (1 << 10) | (1 << 11), QueryTriggerInteraction.Ignore))
                    if (!hit.collider.transform.IsChildOf(transform)) { path = null; Pause(); return; }
            }
            if (direction.sqrMagnitude > .00001f) transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 120 * deltaTime);
            transform.position = next;
            if (Vector3.Distance(next, target) < .015f) { corner++; if (!Walking) Pause(); }
        }
        private static float Horizontal(Vector3 value) => new Vector2(value.x, value.z).magnitude;
        private void Update() => Step(Time.deltaTime);
    }
}
