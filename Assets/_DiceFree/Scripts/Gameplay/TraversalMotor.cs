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
        private readonly HashSet<string> motionBlocks = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, float> speedFactors = new(StringComparer.Ordinal);
        private float effectiveSpeed;
        public float BaseSpeed => speed;
        public float Speed { get { RefreshSpeed(); return effectiveSpeed; } }
        public bool MotionAllowed => motionAllowed && motionBlocks.Count == 0;
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
            if (!MotionAllowed || !Ready || !NavMesh.SamplePosition(destination, out var hit, 0.75f, agent.areaMask))
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
            if (!Ready || !MotionAllowed) return;
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
        public void SetMotionAllowed(bool value) { motionAllowed = value; if (!MotionAllowed) Stop(); RefreshSpeed(); }
        public void SetMotionBlocked(string sourceId, bool blocked)
        {
            if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("Motion block requires a stable source.");
            if (blocked) motionBlocks.Add(sourceId); else motionBlocks.Remove(sourceId);
            if (!MotionAllowed) Stop();
        }
        /// <summary>Relocate the same agent between independently baked map NavMeshes.</summary>
        public bool TeleportAcrossMaps(Vector3 point)
        {
            if (agent == null || !NavMesh.SamplePosition(point, out var hit, 3f, agent.areaMask))
                return false;

            // Warp alone can fail when the destination polygon belongs to a new scene.
            Stop();
            if (agent.enabled) agent.enabled = false;
            transform.position = hit.position;
            agent.enabled = true;
            bool ready = agent.isOnNavMesh;
            RefreshSpeed();
            return ready;
        }

        public bool Teleport(Vector3 point)
        {
            // Persistence/start-menu restoration can request the authored spawn before this component's Start.
            // Enabling the already-configured agent here removes that script-order dependency while keeping
            // ordinary scene startup unchanged.
            if (agent == null) return false;
            if (!agent.enabled) agent.enabled = true;
            if (!agent.isOnNavMesh || !NavMesh.SamplePosition(point, out var hit, 2, agent.areaMask)) return false;
            Stop(); bool moved = agent.Warp(hit.position); RefreshSpeed(); return moved;
        }
    }
}
