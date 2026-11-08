using System.Collections.Generic;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.AI
{
    [RequireComponent(typeof(CombatActor), typeof(BasicAttack))]
    public sealed class AggroBehaviour : MonoBehaviour
    {
        [SerializeField, Min(1)] private float awareness = 7;
        [SerializeField, Min(1)] private float leash = 13;
        [SerializeField] private bool useAuthoredHome;
        [SerializeField] private Vector3 authoredHome;
        [SerializeField] private bool patrolControlled;
        [SerializeReference] private AutoAggroPolicy autoAggroPolicy = new();
        public AutoAggroPolicy AutoAggroPolicy => autoAggroPolicy;
        public void ConfigureAutoAggro(AutoAggroPolicy policy) => autoAggroPolicy = policy ?? new AutoAggroPolicy();
        private CombatActor actor;
        private BasicAttack attack;
        private Vector3 home;
        private float nextPath;
        private readonly Dictionary<CombatActor,float> threat = new();
        private readonly HashSet<CombatActor> retainedPursuits = new();
        private readonly List<CombatActor> departed = new();
        public bool Returning { get; private set; }
        public Vector3 Home => home;
        public float Awareness => awareness;
        public float Leash => leash;
        public bool PatrolControlled => patrolControlled;
        public void Configure(float awarenessRadius, float leashRadius) { awareness = awarenessRadius; leash = leashRadius; }
        public void ConfigurePatrolMovement(bool enabled) => patrolControlled = enabled;
        public void ConfigureHomePosition(Vector3 position)
        {
            authoredHome = position;
            useAuthoredHome = true;
            home = position;
        }
        public string State => !actor.Alive ? "Defeated" : Returning ? "Returning" : attack.Target != null ? "Aggro" : "Idle";
        private void Awake() { actor = GetComponent<CombatActor>(); attack = GetComponent<BasicAttack>(); home = useAuthoredHome ? authoredHome : transform.position; }
        private void OnEnable() { actor.Health.Damaged += OnDamaged; actor.Health.Restored += ClearState; CombatActor.ThreatReduced += ReduceThreat; }
        private void OnDisable() { actor.Health.Damaged -= OnDamaged; actor.Health.Restored -= ClearState; CombatActor.ThreatReduced -= ReduceThreat; ClearThreat(); attack.Cancel(); }
        private void ClearState() { ClearThreat(); attack.ResetForSpawn(); Returning = false; nextPath = 0; }
        private void ClearThreat() { threat.Clear(); retainedPursuits.Clear(); }
        private void ReduceThreat(CombatActor source, float retained)
        {
            if (!threat.TryGetValue(source,out float amount))
            {
                if (attack.Target != source) return;
                amount = 1;
            }
            threat[source] = amount * retained;
            retainedPursuits.Add(source);
        }
        private void OnDamaged(CombatActor source, DamageResult result)
        {
            if (Returning || !actor.IsHostileTo(source) || Vector3.Distance(source.transform.position, home) > leash) return;
            threat.TryGetValue(source, out float amount);
            threat[source] = amount + result.applied;
            if (actor.CanTargetHostile(source)) attack.Order(source);
        }
        private void Update()
        {
            if (!actor.Alive) { attack.Cancel(); return; }
            if (Returning)
            {
                if (Vector3.Distance(transform.position, home) < 0.5f)
                {
                    actor.Motor.Stop(); actor.Health.Restore(); Returning = false;
                }
                else if (Time.time > nextPath) { actor.Motor.MoveTo(home); nextPath = Time.time + 0.5f; }
                return;
            }
            if (Vector3.Distance(transform.position, home) > leash || (attack.Target != null &&
                (!attack.Target.Alive || Vector3.Distance(attack.Target.transform.position, home) > leash)))
            { ReturnHome(); return; }
            if (attack.Target != null) return;
            CombatActor nearest = null;
            CombatActor known = null;
            float strongest = 0;
            bool concealedPursuit = false;
            foreach (var entry in threat)
            {
                var target = entry.Key;
                if (target == null || !target.Alive || Vector3.Distance(target.transform.position,home)>leash)
                    departed.Add(target);
                else if (target.Concealed) concealedPursuit = true;
                else if (retainedPursuits.Contains(target) && entry.Value>strongest && actor.HasSightOf(target))
                { known=target; strongest=entry.Value; }
            }
            foreach (var target in departed) { threat.Remove(target); retainedPursuits.Remove(target); }
            departed.Clear();
            if (known!=null && attack.Order(known)) { retainedPursuits.Remove(known); return; }
            float distance = awareness;
            foreach (var candidate in CombatActor.All)
            {
                if (!actor.CanTargetHostile(candidate) || !actor.HasSightOf(candidate) ||
                    !autoAggroPolicy.Allows(actor, candidate)) continue;
                var candidateDistance = Vector3.Distance(transform.position, candidate.transform.position);
                if (candidateDistance < distance) { nearest = candidate; distance = candidateDistance; }
            }
            if (nearest != null)
            {
                threat.TryAdd(nearest,1);
                attack.Order(nearest);
            }
            else if (!concealedPursuit && !patrolControlled && Vector3.Distance(transform.position, home) > 0.6f) ReturnHome();
        }
        private void ReturnHome() { attack.Cancel(); Returning = true; nextPath = 0; }
        // Explicit debug reset, not a spawn/loot/quest rule. Exactly one actor is reused.
        public bool ResetEncounter()
        {
            attack.Cancel();
            if (!actor.Motor.Teleport(home)) return false;
            actor.Stats.ResetTransientModifiers();
            actor.Health.Restore(); Returning = false; return true;
        }
    }
}
