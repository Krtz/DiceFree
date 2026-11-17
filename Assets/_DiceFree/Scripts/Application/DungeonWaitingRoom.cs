using DiceFree.Characters;
using DiceFree.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Dungeons
{
    public sealed class DungeonWaitingRoom : MonoBehaviour
    {
        public const string ResourcePath = "DungeonWaitingRoom";
        public const float Seconds = 60;
        [SerializeField] private NavMeshData navigation;
        [SerializeField] private ActorDefinition practiceTarget;
        [SerializeField] private Transform arrival;
        [SerializeField] private Transform dummySpot;

        private NavMeshDataInstance registeredNavigation;

        public Vector3 Arrival => arrival.position;
        public Rect Bounds => new(transform.position.x - 14, transform.position.z - 12, 28, 24);
        public bool Ready => registeredNavigation.valid && practiceTarget != null && arrival != null && dummySpot != null;

        public void Configure(NavMeshData data, ActorDefinition target, Transform spawn, Transform dummy)
        {
            navigation = data;
            practiceTarget = target;
            arrival = spawn;
            dummySpot = dummy;
        }

        private void OnEnable()
        {
            if (navigation != null)
                registeredNavigation = NavMesh.AddNavMeshData(navigation, transform.position, transform.rotation);
        }

        private void OnDisable()
        {
            if (registeredNavigation.valid) registeredNavigation.Remove();
        }

        internal void CreatePracticeTarget()
        {
            var target = new GameObject("Practice target — no rewards");
            target.SetActive(false);
            target.transform.SetParent(dummySpot, false);
            target.layer = 10;

            var agent = target.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            agent.height = 2;
            var motor = target.AddComponent<TraversalMotor>();
            target.AddComponent<ActorStats>().Configure(practiceTarget);
            target.AddComponent<Health>().Invulnerable = true;
            target.AddComponent<CombatActor>().Configure(1, .6f, "practice-target");
            target.AddComponent<CombatStatusController>();
            var capsule = target.AddComponent<CapsuleCollider>();
            capsule.radius = .6f;
            capsule.height = 2;
            capsule.center = Vector3.up;
            PracticeDummyVisual.Create(target.transform);
            target.SetActive(true);
            motor.SetMotionBlocked("dungeon.practice-target", true);
        }
    }
}
