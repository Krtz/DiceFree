using System;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Visuals
{
    // Cosmetic handcart traffic follows authored pedestrian routes. No combat or rewards.
    public sealed class ArboriumAmbientRoute : MonoBehaviour
    {
        [SerializeField] private Vector3[] stops = Array.Empty<Vector3>();
        [SerializeField] private Transform[] wheels = Array.Empty<Transform>();
        [SerializeField, Min(.1f)] private float speed = 1.1f;
        private NavMeshPath path;
        private int destination = 1, direction = 1, corner;
        private float pause = 2, offset;
        public float TravelledMeters { get; private set; }
        public int CompletedStops { get; private set; }
        public bool Walking => path != null && corner < path.corners.Length;
        public void Configure(Vector3[] points, Transform[] cartWheels)
        { stops = points; wheels = cartWheels; }
        private void OnEnable() { path = null; pause = 2; destination = 1; direction = 1; }
        private void Update()
        {
            if (stops.Length < 2) return;
            if (!Walking)
            {
                pause -= Time.deltaTime;
                if (pause > 0) return;
                pause = 1;
                if (!NavMesh.SamplePosition(transform.position, out var from, 1, NavMesh.AllAreas) ||
                    !NavMesh.SamplePosition(stops[destination], out var to, 1, NavMesh.AllAreas)) return;
                var next = new NavMeshPath();
                if (!NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, next) || next.status != NavMeshPathStatus.PathComplete) return;
                offset = transform.position.y - from.position.y; path = next; corner = 0;
            }
            var target = path.corners[corner] + Vector3.up * offset;
            var nextPosition = Vector3.MoveTowards(transform.position, target, speed * Mathf.Min(Time.deltaTime, .1f));
            var delta = nextPosition - transform.position;
            if (delta.sqrMagnitude > .000001f)
            {
                foreach (var hit in Physics.SphereCastAll(transform.position + Vector3.up * .8f, .3f,
                    delta.normalized, delta.magnitude + .08f, (1 << 9) | (1 << 10) | (1 << 11), QueryTriggerInteraction.Ignore))
                    if (!hit.transform.IsChildOf(transform)) return;
                if (!NavMesh.SamplePosition(transform.position - Vector3.up * offset, out var current, .3f, NavMesh.AllAreas) ||
                    NavMesh.Raycast(current.position, nextPosition - Vector3.up * offset, out _, NavMesh.AllAreas)) { path = null; return; }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(delta), 110 * Time.deltaTime);
                transform.position = nextPosition; TravelledMeters += delta.magnitude;
                foreach (var wheel in wheels) if (wheel != null) wheel.Rotate(Vector3.right, delta.magnitude / .55f * Mathf.Rad2Deg, Space.Self);
            }
            if (Vector3.Distance(transform.position, target) > .025f) return;
            corner++;
            if (Walking) return;
            CompletedStops++;
            if (destination == stops.Length - 1) direction = -1;
            else if (destination == 0) direction = 1;
            destination += direction; pause = 3;
        }
    }
}
