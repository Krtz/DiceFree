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
        public bool CanReturn => !actor.Health.Alive && Time.time >= availableAt;
        public string AnchorName => anchor == null ? "No anchor" : anchor.name;
        private void Awake() { actor = GetComponent<CombatActor>(); attack = GetComponent<BasicAttack>(); }
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
