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
        public float Speed => speed;
        public bool Ready => agent != null && agent.isOnNavMesh;
        public bool Travelling => Ready && agent.hasPath;

        private void Awake()
        {
            candidatePath = new NavMeshPath();
            agent = GetComponent<NavMeshAgent>();
            agent.speed = speed;
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
            if (!Ready || !NavMesh.SamplePosition(destination, out var hit, 0.75f, agent.areaMask))
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
            if (!Ready) return;
            Stop();
            direction = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(direction, Vector3.up), 1f);
            var start = agent.nextPosition;
            var end = start + direction * (speed * deltaTime);
            // Both schemes respect the same baked radius, slopes and natural boundaries.
            if (NavMesh.Raycast(start, end, out var hit, agent.areaMask)) end = hit.position;
            agent.Move(end - start);
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(direction), 720f * deltaTime);
        }

        private void OnDisable() => Stop();
    }
}
