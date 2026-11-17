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
        private static readonly HashSet<CapsuleCollider> movingPedestrians = new();
        private bool pedestriansMatter, detouring;
        private float checkPedestriansAt;
        private Vector3 requestedDestination;
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
        public bool Travelling => Ready && (agent.hasPath || detouring);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPedestrians() => movingPedestrians.Clear();

        public static void TrackPedestrian(CapsuleCollider body) => movingPedestrians.Add(body);
        public static void ForgetPedestrian(CapsuleCollider body) => movingPedestrians.Remove(body);
        public void ConsiderMovingPedestrians() => pedestriansMatter = true;

        private void Awake()
        {
            candidatePath = new NavMeshPath();
            agent = GetComponent<NavMeshAgent>();
            ApplySpeed();
            agent.acceleration = 35f;
            agent.angularSpeed = 720f;
            agent.stoppingDistance = 0.15f;
            agent.autoRepath = true;
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
            if (detouring && (hit.position - requestedDestination).sqrMagnitude < .1f) return true;
            if (!agent.CalculatePath(hit.position, candidatePath) ||
                candidatePath.status != NavMeshPathStatus.PathComplete)
                return false; // Keep the previous valid command when a click is unreachable.
            agent.isStopped = false;
            if (!agent.SetPath(candidatePath)) return false;
            requestedDestination = hit.position;
            detouring = false;
            checkPedestriansAt = 0;
            AvoidPedestrianAhead();
            return true;
        }

        // Ground clicks may fall just inside a prop's clearance or on a disconnected
        // sliver. Keep the click on its intended storey and prefer the closest usable ring.
        public bool TryFindReachablePoint(Vector3 destination, out Vector3 point)
        {
            point = default;
            if (!MotionAllowed || !Ready) return false;
            if (TryCandidate(destination, destination.y, .75f, out point)) return true;

            var toward = Vector3.ProjectOnPlane(transform.position - destination, Vector3.up);
            if (toward.sqrMagnitude < .001f) toward = Vector3.forward;
            toward.Normalize();
            for (int ring = 1; ring <= 5; ring++)
            {
                float bestDistance = float.PositiveInfinity;
                float offset = ring == 1 ? .42f : (ring - 1) * .85f;
                for (int i = 0; i < 12; i++)
                {
                    var probe = destination + Quaternion.Euler(0, i * 30f, 0) * toward * offset;
                    if (!TryCandidate(probe, destination.y, .48f, out var sample)) continue;
                    float score = (sample - destination).sqrMagnitude + .025f * (sample - transform.position).magnitude;
                    if (score >= bestDistance) continue;
                    bestDistance = score;
                    point = sample;
                }
                if (!float.IsPositiveInfinity(bestDistance)) return true;
            }
            return false;
        }

        private bool TryCandidate(Vector3 probe, float intendedHeight, float radius, out Vector3 point)
        {
            point = default;
            if (!NavMesh.SamplePosition(probe, out var hit, radius, agent.areaMask) ||
                Mathf.Abs(hit.position.y - intendedHeight) > .70f ||
                !agent.CalculatePath(hit.position, candidatePath) || candidatePath.status != NavMeshPathStatus.PathComplete)
                return false;
            point = hit.position;
            return true;
        }

        public void Stop()
        {
            detouring = false;
            if (Ready) agent.ResetPath();
        }

        private void AvoidPedestrianAhead()
        {
            if (!pedestriansMatter || !MotionAllowed || !Ready || agent.pathPending) return;
            if (detouring)
            {
                if (agent.hasPath && agent.remainingDistance > .23f) return;
                detouring = false;
                if (agent.CalculatePath(requestedDestination, candidatePath) &&
                    candidatePath.status == NavMeshPathStatus.PathComplete) agent.SetPath(candidatePath);
            }
            if (!agent.hasPath || Time.time < checkPedestriansAt) return;
            checkPedestriansAt = Time.time + .08f;

            var start = agent.nextPosition;
            var leg = agent.steeringTarget - start;
            leg.y = 0;
            float length = leg.magnitude;
            if (length < .25f) return;
            var heading = leg / length;
            var side = Vector3.Cross(Vector3.up, heading);
            float lookahead = Mathf.Min(length, Mathf.Max(2.5f, effectiveSpeed * .58f));
            float nearest = float.PositiveInfinity, clearance = 0;
            Vector3 obstruction = default;
            foreach (var body in movingPedestrians)
            {
                if (body == null || !body.enabled || !body.gameObject.activeInHierarchy) continue;
                var center = body.transform.TransformPoint(body.center);
                var fromStart = center - start;
                if (Mathf.Abs(fromStart.y - agent.height * .5f) > .95f) continue;
                float forward = Vector3.Dot(fromStart, heading);
                if (forward <= 0 || forward > lookahead || forward >= nearest) continue;
                var scale = body.transform.lossyScale;
                float safeRadius = agent.radius + body.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z)) + .32f;
                if (Mathf.Abs(Vector3.Dot(fromStart, side)) >= safeRadius) continue;
                nearest = forward;
                clearance = safeRadius;
                obstruction = new Vector3(center.x, start.y, center.z);
            }

            if (float.IsPositiveInfinity(nearest))
            {
                if (agent.isStopped) agent.isStopped = false;
                return;
            }
            var preferred = Vector3.Dot(start - obstruction, side) < 0 ? -side : side;
            if (!TryPedestrianDetour(start, obstruction, heading, preferred, clearance) &&
                !TryPedestrianDetour(start, obstruction, heading, -preferred, clearance))
            {
                // In a narrow doorway, wait briefly for a crossing NPC to clear an actual route.
                agent.isStopped = nearest <= clearance;
                return;
            }
            agent.isStopped = false;
            detouring = true;
        }

        private bool TryPedestrianDetour(Vector3 start, Vector3 obstruction, Vector3 heading, Vector3 side, float clearance)
        {
            var bypass = obstruction + side * (clearance * 1.77f) - heading * (clearance * .24f);
            if (!NavMesh.SamplePosition(bypass, out var hit, .38f, agent.areaMask) ||
                Mathf.Abs(hit.position.y - start.y) > .58f ||
                NavMesh.Raycast(start, hit.position, out _, agent.areaMask) ||
                !agent.CalculatePath(hit.position, candidatePath) || candidatePath.status != NavMeshPathStatus.PathComplete)
                return false;
            return agent.SetPath(candidatePath);
        }

        public void MoveDirect(Vector3 direction, float deltaTime)
        {
            if (!Ready || !MotionAllowed) return;
            Stop();
            direction = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(direction, Vector3.up), 1f);
            var start = agent.nextPosition;
            var end = start + direction * (Speed * deltaTime);
            // Keep the component of input parallel to a wall or an inside corner.
            // The second ray keeps that slide inside the same connected surface.
            if (NavMesh.Raycast(start, end, out var hit, agent.areaMask))
            {
                var normal = Vector3.ProjectOnPlane(hit.normal, Vector3.up).normalized;
                var slide = Vector3.ProjectOnPlane(end - hit.position, normal);
                slide.y = 0;
                var edge = Vector3.MoveTowards(hit.position, start, .025f);
                end = edge + slide;
                if (NavMesh.Raycast(edge, end, out var boundary, agent.areaMask)) end = boundary.position;
                end = Vector3.MoveTowards(end, start, .01f);
            }
            // Direct movement also respects live unit footprints, not only the static bake.
            var displacement = end - start;
            var distance = displacement.magnitude;
            if (distance > 0.001f)
            {
                var thisActor=GetComponent<DiceFree.Combat.CombatActor>();
                foreach (var unit in Physics.SphereCastAll(start + Vector3.up * 0.8f, agent.radius,
                    displacement.normalized, distance, (1 << 9) | (1 << 10) | (1 << 11), QueryTriggerInteraction.Ignore))
                {
                    if(unit.collider.transform.root==transform.root)continue;
                    var other=unit.collider.GetComponentInParent<DiceFree.Combat.CombatActor>();
                    if(other!=null && ((thisActor!=null && thisActor.CanPassThrough(other)) ||
                        other.Ghosting || !other.Alive))continue;
                    distance = Mathf.Min(distance, Mathf.Max(0, unit.distance - 0.03f));
                }
                end = start + displacement.normalized * distance;
            }
            agent.Move(end - start);
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(direction), 720f * deltaTime);
        }

        private void OnDisable() => Stop();
        private void Update()
        {
            RefreshSpeed();
            AvoidPedestrianAhead();
        }
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
            if (ready) GetComponent<DiceFree.Combat.ActorFootprintSeparation>()?.RebaseAfterTeleport();
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
            Stop();
            bool moved = agent.Warp(hit.position);
            if (moved) GetComponent<DiceFree.Combat.ActorFootprintSeparation>()?.RebaseAfterTeleport();
            RefreshSpeed();
            return moved;
        }
    }
}
