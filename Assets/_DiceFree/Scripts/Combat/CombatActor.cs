using System.Collections.Generic;
using DiceFree.Characters;
using DiceFree.Foundation;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Combat
{
    [DefaultExecutionOrder(-100), RequireComponent(typeof(ActorStats), typeof(Health), typeof(TraversalMotor))]
    public sealed class CombatActor : MonoBehaviour
    {
        private static readonly List<CombatActor> actors = new();
        public static IReadOnlyList<CombatActor> All => actors;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSessionRegistry() => actors.Clear();
        [SerializeField] private int faction;
        [SerializeField, Min(0.1f)] private float radius = 0.45f;
        public int Faction => faction;
        public string DisplayName => GetComponent<IActorDisplayName>()?.DisplayName
            ?? Stats?.Definition?.displayName ?? name;
        public ActorStats Stats { get; private set; }
        public Health Health { get; private set; }
        public TraversalMotor Motor { get; private set; }
        public CombatStatusController Effects { get; private set; }
        public float Radius => radius;
        /// <summary>Ghosting is an explicit future spell/condition override.
        /// Normal alive units must respect each other's circular footprints.</summary>
        public bool Ghosting { get; private set; }
        public void SetGhosting(bool value) => Ghosting = value;
        private string familyOverride;
        private float nominalRadius;
        private bool footprintInitialized;
        public string FamilyId => familyOverride ?? Stats.Definition.familyId;
        public bool Alive => isActiveAndEnabled && Health != null && Health.Alive;
        private readonly HashSet<string> actionBlocks=new();
        public void SetActionBlocked(string sourceId,bool blocked)
        {
            if(string.IsNullOrWhiteSpace(sourceId))throw new System.ArgumentException("Action block needs a source ID.");
            if(blocked)actionBlocks.Add(sourceId);else actionBlocks.Remove(sourceId);
        }
        public bool CanAct => Alive && actionBlocks.Count==0 && (Effects == null || !Effects.Stunned);
        public bool InCombat
        {
            get
            {
                if (!Alive) return false;
                foreach (var actor in actors)
                {
                    var action = actor.GetComponent<BasicAttack>();
                    if (actor.Alive && action != null && action.Target != null &&
                        (actor == this || action.Target == this)) return true;
                }
                return false;
            }
        }
        private void Awake()
        {
            Stats = GetComponent<ActorStats>(); Health = GetComponent<Health>(); Motor = GetComponent<TraversalMotor>();
            Effects = GetComponent<CombatStatusController>();
        }
        private void Start()
        {
            Motor.SetSpeed(Stats.MoveSpeed);
            // Some wildlife configures its body during a different Start()
            // callback. Maintain the authored base radius separately so
            // Configure() can re-apply the runtime footprint in either order.
            nominalRadius=radius;
            footprintInitialized=true;
            RefreshFootprint();
            if(GetComponent<ActorFootprintSeparation>()==null)
                gameObject.AddComponent<ActorFootprintSeparation>();
        }

        private void RefreshFootprint()
        {
            // Giant dungeon bosses already have oversized authored collision.
            // Expand them modestly so they still fit their arena navigation.
            float expanded=nominalRadius>1.4f ?
                nominalRadius+Mathf.Min(.30f,nominalRadius*.15f) :
                nominalRadius*(faction==0?1.50f:1.40f);
            float minimum=faction==0?.675f:.54f;
            radius=Mathf.Max(minimum,expanded);
            var agent=GetComponent<NavMeshAgent>();
            if(agent!=null)
            {
                // Navigation radius is local to the scaled actor root; hitboxes and
                // combat ranges are world-space. Large boss roots otherwise become
                // enormous avoidance bodies and cannot follow a valid dungeon path.
                Vector3 worldScale=transform.lossyScale;
                float planarScale=Mathf.Max(.001f,Mathf.Max(Mathf.Abs(worldScale.x),Mathf.Abs(worldScale.z)));
                agent.radius=radius/planarScale;
                agent.obstacleAvoidanceType=ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                agent.avoidancePriority=faction==0?40:50;
            }
            var capsule=GetComponent<CapsuleCollider>();
            if(capsule!=null)
            {
                // Radius is a world-space combat contract; CapsuleCollider uses local units.
                Vector3 scale = transform.lossyScale;
                float radiusScale = capsule.direction == 0 ? Mathf.Max(Mathf.Abs(scale.y),Mathf.Abs(scale.z)) :
                    capsule.direction == 2 ? Mathf.Max(Mathf.Abs(scale.x),Mathf.Abs(scale.y)) :
                    Mathf.Max(Mathf.Abs(scale.x),Mathf.Abs(scale.z));
                capsule.radius=radius/Mathf.Max(.001f,radiusScale);
                capsule.isTrigger=false;
                capsule.enabled=Health.Alive;
            }
        }
        private void OnEnable() { if (!actors.Contains(this)) actors.Add(this); Health.Died += OnDeath; Health.Restored += OnRestore; Stats.Changed += OnLevel; }
        private void OnDisable() { actors.Remove(this); Health.Died -= OnDeath; Health.Restored -= OnRestore; Stats.Changed -= OnLevel; }
        private void OnLevel(float previousMaximum) => Motor.SetSpeed(Stats.MoveSpeed);
        private void OnDeath() { Motor.SetMotionAllowed(false); SetColliders(false); }
        private void OnRestore() { Motor.SetMotionAllowed(true); SetColliders(true); }
        private void SetColliders(bool value) { foreach (var collider in GetComponents<Collider>()) collider.enabled = value; }
        public bool IsHostileTo(CombatActor other) => Alive && other != null && other.Alive && other != this && faction != other.faction;
        public bool IsFriendlyTo(CombatActor other) => Alive && other != null && other.Alive && faction == other.faction;
        public bool HasSightOf(CombatActor other) => !Physics.Linecast(transform.position + Vector3.up * 0.8f,
            other.transform.position + Vector3.up * 0.8f, 1 << 9, QueryTriggerInteraction.Ignore);
        public void Configure(int team, float bodyRadius, string family = null)
        {
            faction=team;
            nominalRadius=bodyRadius;
            radius=bodyRadius;
            familyOverride=family;
            if(footprintInitialized)RefreshFootprint();
        }
    }
}
