using System.Collections.Generic;
using DiceFree.Characters;
using UnityEngine;

namespace DiceFree.Combat
{
    [DefaultExecutionOrder(-100), RequireComponent(typeof(ActorStats), typeof(Health), typeof(TraversalMotor))]
    public sealed class CombatActor : MonoBehaviour
    {
        private static readonly List<CombatActor> actors = new();
        public static IReadOnlyList<CombatActor> All => actors;
        [SerializeField] private int faction;
        [SerializeField, Min(0.1f)] private float radius = 0.45f;
        public ActorStats Stats { get; private set; }
        public Health Health { get; private set; }
        public TraversalMotor Motor { get; private set; }
        public float Radius => radius;
        public bool Alive => isActiveAndEnabled && Health != null && Health.Alive;
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
        }
        private void Start() => Motor.SetSpeed(Stats.MoveSpeed);
        private void OnEnable() { actors.Add(this); Health.Died += OnDeath; Health.Restored += OnRestore; Stats.LevelChanged += OnLevel; }
        private void OnDisable() { actors.Remove(this); Health.Died -= OnDeath; Health.Restored -= OnRestore; Stats.LevelChanged -= OnLevel; }
        private void OnLevel(float previousMaximum) => Motor.SetSpeed(Stats.MoveSpeed);
        private void OnDeath() { Motor.SetMotionAllowed(false); SetColliders(false); }
        private void OnRestore() { Motor.SetMotionAllowed(true); SetColliders(true); }
        private void SetColliders(bool value) { foreach (var collider in GetComponents<Collider>()) collider.enabled = value; }
        public bool IsHostileTo(CombatActor other) => Alive && other != null && other.Alive && other != this && faction != other.faction;
        public bool HasSightOf(CombatActor other) => !Physics.Linecast(transform.position + Vector3.up * 0.8f,
            other.transform.position + Vector3.up * 0.8f, 1 << 9, QueryTriggerInteraction.Ignore);
        public void Configure(int team, float bodyRadius) { faction = team; radius = bodyRadius; }
    }
}
