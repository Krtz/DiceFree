using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor))]
    public sealed class OverworldRespawn : MonoBehaviour
    {
        [SerializeField] private RespawnDefinition definition;
        private CombatActor actor;
        private Vector3 home;
        private Quaternion rotation;
        private float due = float.PositiveInfinity;
        public float Remaining => Mathf.Max(0, due - Time.time);
        public RespawnDefinition Definition => definition;
        private void Awake() { actor = GetComponent<CombatActor>(); home = transform.position; rotation = transform.rotation; }
        private void OnEnable() { actor.Health.Died += OnDeath; actor.Health.Restored += OnRestored; }
        private void OnDisable() { actor.Health.Died -= OnDeath; actor.Health.Restored -= OnRestored; }
        private void OnDeath() => due = Time.time + Mathf.Max(0.1f, definition.delaySeconds);
        private void OnRestored() => due = float.PositiveInfinity;
        private void Update()
        {
            if (actor.Health.Alive || Time.time < due) return;
            if (!actor.Motor.Teleport(home)) { due = Time.time + 1; return; }
            transform.rotation = rotation;
            GetComponent<BasicAttack>()?.ResetForSpawn();
            actor.Stats.ResetTransientModifiers();
            actor.Health.Restore(); // Subscribers clear AI state and the per-life defeat latch.
        }
        public void Configure(RespawnDefinition value) => definition = value;
    }
}
