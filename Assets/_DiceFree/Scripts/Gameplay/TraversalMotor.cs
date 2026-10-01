using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Characters
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class TraversalMotor : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 5f;
        private NavMeshAgent agent;
        private NavMeshPath candidatePath;
        private bool motionAllowed = true;
        private readonly SortedDictionary<string, float> speedFactors = new(StringComparer.Ordinal);
        private float effectiveSpeed;
        public float BaseSpeed => speed;
        public float Speed { get { RefreshSpeed(); return effectiveSpeed; } }
        public bool MotionAllowed => motionAllowed;
        // Context providers refresh their own named contribution; inputs never calculate speed.
        public event Action RefreshSpeedSources;
        public bool Ready => agent != null && agent.isOnNavMesh;
        public bool Travelling => Ready && agent.hasPath;

        private void Awake()
        {
            candidatePath = new NavMeshPath();
            agent = GetComponent<NavMeshAgent>();
            ApplySpeed();
            agent.acceleration = 35f;
            agent.angularSpeed = 720f;
            agent.stoppingDistance = 0.15f;
        }

        private void Start()
        {
            // Native agent OnEnable can precede scene MonoBehaviour OnEnable in a player build.
            // The scene serializes the agent disabled; all navigation is registered before Start.
            agent.enabled = true;
            if (!agent.isOnNavMesh)
                Debug.LogError("Traversal spawn is not on the registered world navigation.", this);
        }

        public bool MoveTo(Vector3 destination)
        {
            RefreshSpeed();
            if (!motionAllowed || !Ready || !NavMesh.SamplePosition(destination, out var hit, 0.75f, agent.areaMask))
                return false;
            if (!agent.CalculatePath(hit.position, candidatePath) ||
                candidatePath.status != NavMeshPathStatus.PathComplete)
                return false; // Keep the previous valid command when a click is unreachable.
            agent.isStopped = false;
            return agent.SetPath(candidatePath);
        }

        public void Stop()
        {
            if (Ready) agent.ResetPath();
        }

        public void MoveDirect(Vector3 direction, float deltaTime)
        {
            if (!Ready || !motionAllowed) return;
            Stop();
            direction = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(direction, Vector3.up), 1f);
            var start = agent.nextPosition;
            var end = start + direction * (Speed * deltaTime);
            // Both schemes respect the same baked radius, slopes and natural boundaries.
            if (NavMesh.Raycast(start, end, out var hit, agent.areaMask)) end = hit.position;
            // Direct movement also respects live unit footprints, not only the static bake.
            var displacement = end - start;
            var distance = displacement.magnitude;
            if (distance > 0.001f)
            {
                foreach (var unit in Physics.SphereCastAll(start + Vector3.up * 0.8f, agent.radius,
                    displacement.normalized, distance, 1 << 10, QueryTriggerInteraction.Ignore))
                    if (unit.collider.transform.root != transform.root)
                        distance = Mathf.Min(distance, Mathf.Max(0, unit.distance - 0.03f));
                end = start + displacement.normalized * distance;
            }
            agent.Move(end - start);
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(direction), 720f * deltaTime);
        }

        private void OnDisable() => Stop();
        private void Update() => RefreshSpeed();
        public void SetSpeed(float value) { speed = Mathf.Max(0.1f, value); RefreshSpeed(); }
        public void SetSpeedFactor(string sourceId, float factor)
        {
            if (string.IsNullOrWhiteSpace(sourceId) || float.IsNaN(factor) || float.IsInfinity(factor) || factor <= 0)
                throw new ArgumentException("A speed contribution needs a stable source and positive finite factor.");
            speedFactors[sourceId] = factor; ApplySpeed();
        }
        public void RemoveSpeedFactor(string sourceId) { speedFactors.Remove(sourceId); ApplySpeed(); }
        private void RefreshSpeed() { RefreshSpeedSources?.Invoke(); ApplySpeed(); }
        private void ApplySpeed()
        {
            effectiveSpeed = speed;
            foreach (var factor in speedFactors.Values) effectiveSpeed *= factor;
            if (agent != null) agent.speed = effectiveSpeed;
        }
        public void SetMotionAllowed(bool value) { motionAllowed = value; if (!value) Stop(); RefreshSpeed(); }
        public bool Teleport(Vector3 point)
        {
            if (!Ready || !NavMesh.SamplePosition(point, out var hit, 2, agent.areaMask)) return false;
            Stop(); bool moved = agent.Warp(hit.position); RefreshSpeed(); return moved;
        }
    }
}
