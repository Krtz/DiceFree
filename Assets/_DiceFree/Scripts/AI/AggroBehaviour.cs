using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.AI
{
    [RequireComponent(typeof(CombatActor), typeof(BasicAttack))]
    public sealed class AggroBehaviour : MonoBehaviour
    {
        [SerializeField, Min(1)] private float awareness = 7;
        [SerializeField, Min(1)] private float leash = 13;
        [SerializeReference] private AutoAggroPolicy autoAggroPolicy = new();
        public AutoAggroPolicy AutoAggroPolicy => autoAggroPolicy;
        public void ConfigureAutoAggro(AutoAggroPolicy policy) => autoAggroPolicy = policy ?? new AutoAggroPolicy();
        private CombatActor actor;
        private BasicAttack attack;
        private Vector3 home;
        private float nextPath;
        public bool Returning { get; private set; }
        public Vector3 Home => home;
        public float Awareness => awareness;
        public float Leash => leash;
        public void Configure(float awarenessRadius, float leashRadius) { awareness = awarenessRadius; leash = leashRadius; }
        public string State => !actor.Alive ? "Defeated" : Returning ? "Returning" : attack.Target != null ? "Aggro" : "Idle";
        private void Awake() { actor = GetComponent<CombatActor>(); attack = GetComponent<BasicAttack>(); home = transform.position; }
        private void OnEnable() { actor.Health.Damaged += OnDamaged; actor.Health.Restored += ClearState; }
        private void OnDisable() { actor.Health.Damaged -= OnDamaged; actor.Health.Restored -= ClearState; attack.Cancel(); }
        private void ClearState() { attack.ResetForSpawn(); Returning = false; nextPath = 0; }
        private void OnDamaged(CombatActor source, DamageResult result)
        {
            if (!Returning && actor.IsHostileTo(source) && Vector3.Distance(source.transform.position, home) <= leash)
                attack.Order(source);
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
            float distance = awareness;
            foreach (var candidate in CombatActor.All)
            {
                if (!actor.IsHostileTo(candidate) || !actor.HasSightOf(candidate) ||
                    !autoAggroPolicy.Allows(actor, candidate)) continue;
                var candidateDistance = Vector3.Distance(transform.position, candidate.transform.position);
                if (candidateDistance < distance) { nearest = candidate; distance = candidateDistance; }
            }
            if (nearest != null) attack.Order(nearest);
            else if (Vector3.Distance(transform.position, home) > 0.6f) ReturnHome();
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
