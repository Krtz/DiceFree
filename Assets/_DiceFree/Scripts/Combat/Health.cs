using System;
using UnityEngine;

namespace DiceFree.Combat
{
    [DefaultExecutionOrder(-110), RequireComponent(typeof(ActorStats))]
    public sealed class Health : MonoBehaviour
    {
        private ActorStats stats;
        public float Current { get; private set; }
        public float Maximum => stats.MaximumHp;
        public bool Alive => Current > 0;
        public event Action Died;
        public event Action<CombatActor> Defeated;
        public event Action Restored;
        public event Action<CombatActor, DamageResult> Damaged;
        public event Action<float> Healed;
        private void Awake() { stats = GetComponent<ActorStats>(); Current = Maximum; }
        private void OnEnable() => stats.Changed += RefreshMaximum;
        private void OnDisable() => stats.Changed -= RefreshMaximum;
        private void Update() { if (Alive) Heal(stats.Regeneration * Time.deltaTime, false); }
        public float ApplyDamage(CombatActor source, DamageResult result)
        {
            if (!Alive) return 0;
            float applied = Mathf.Min(Current, Mathf.Max(0, result.mitigated));
            Current -= applied;
            result.applied = applied;
            Damaged?.Invoke(source, result);
            if (!Alive) { Died?.Invoke(); Defeated?.Invoke(source); }
            return applied;
        }
        public float Heal(float amount, bool present = true)
        {
            if (!Alive) return 0; // Healing is not resurrection.
            var applied = Mathf.Min(Maximum - Current, Mathf.Max(0, amount) * stats.HealingReceived);
            Current += applied;
            if (present && applied > 0) Healed?.Invoke(applied);
            return applied;
        }
        public void Restore(float fraction = 1)
        {
            Current = Maximum * Mathf.Clamp(fraction, 0.01f, 1);
            Restored?.Invoke();
        }
        public void RefreshMaximum(float previousMaximum)
        {
            // Preserve missing HP when possible; a maximum change itself is neither damage nor resurrection.
            if (Alive) Current = Mathf.Clamp(Current + Maximum - previousMaximum, Mathf.Min(1, Maximum), Maximum);
        }
    }
}
