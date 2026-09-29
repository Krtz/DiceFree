using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    [RequireComponent(typeof(CombatActor), typeof(BasicAttack))]
    public sealed class RespawnAtAnchor : MonoBehaviour
    {
        [SerializeField] private Transform anchor;
        [SerializeField, Range(0.01f, 1)] private float restoredFraction = 0.5f;
        [SerializeField, Min(0)] private float delay = 1.5f;
        private CombatActor actor;
        private BasicAttack attack;
        private float availableAt;
        private Transform fallbackAnchor;
        public bool CanReturn => !actor.Health.Alive && Time.time >= availableAt;
        public string AnchorName => anchor == null ? "No anchor" : anchor.name;
        public string AnchorId => anchor == null ? null : anchor.GetComponent<ResurrectionAnchor>()?.StableId;
        public bool LoadAtAnchor(string id)
        {
            var destination = fallbackAnchor; // Authored Cornberg anchor remains the fallback after selection.
            foreach (var candidate in FindObjectsByType<ResurrectionAnchor>())
                if (candidate.StableId == id) { destination = candidate.transform; break; }
            if (destination == null || !actor.Motor.Teleport(destination.position))
            {
                destination = fallbackAnchor;
                if (destination == null || !actor.Motor.Teleport(destination.position)) return false;
            }
            anchor = destination;
            attack.ResetForSpawn();
            GetComponent<TargetSelection>()?.Select(null);
            GetComponent<Interactor>()?.Cancel();
            actor.Stats.ResetTransientModifiers();
            actor.Health.Restore(); // Loading is a fresh session; combat Return retains its separate HP policy.
            return true;
        }
        private void Awake() { actor = GetComponent<CombatActor>(); attack = GetComponent<BasicAttack>(); fallbackAnchor = anchor; }
        private void OnEnable() => actor.Health.Died += OnDeath;
        private void OnDisable() => actor.Health.Died -= OnDeath;
        private void OnDeath() { attack.Cancel(); availableAt = Time.time + delay; }
        public bool Return()
        {
            if (!CanReturn || anchor == null || !actor.Motor.Teleport(anchor.position)) return false;
            actor.Health.Restore(restoredFraction);
            return true;
        }
        public void Configure(Transform value) => anchor = value;
    }
}
