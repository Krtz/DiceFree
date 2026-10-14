using System;
using UnityEngine;

namespace DiceFree.Combat
{
    [DefaultExecutionOrder(-110), RequireComponent(typeof(ActorStats))]
    public sealed class Health : MonoBehaviour
    {
        private ActorStats stats;
        private readonly System.Collections.Generic.SortedDictionary<string,(float amount,float until)> shields = new(System.StringComparer.Ordinal);
        public float TemporaryShield
        {
            get
            {
                var expired=new System.Collections.Generic.List<string>();
                float total=0;
                foreach(var pair in shields)if(Time.time>=pair.Value.until)expired.Add(pair.Key);else total+=pair.Value.amount;
                foreach(string id in expired)shields.Remove(id);
                return total;
            }
        }
        public void RemoveTemporaryShield(string source)=>shields.Remove(source);
        public void AddTemporaryShield(string source,float amount,float seconds)
        {
            if(string.IsNullOrWhiteSpace(source)||!float.IsFinite(amount)||!float.IsFinite(seconds)||amount<0||seconds<=0)
                throw new System.ArgumentException("Shield requires finite positive tuning and stable source.");
            if(Alive)shields[source]=(amount,Time.time+seconds);
        }
        private float Absorb(float damage)
        {
            _=TemporaryShield;
            float remaining=damage;
            foreach(string id in new System.Collections.Generic.List<string>(shields.Keys))
            {
                var value=shields[id];float take=Mathf.Min(remaining,value.amount);remaining-=take;
                if(value.amount<=take)shields.Remove(id);else shields[id]=(value.amount-take,value.until);
                if(remaining<=0)break;
            }
            return remaining;
        }
        public bool Invulnerable { get; set; }
        public void SetEncounterHp(float value) => Current = Mathf.Clamp(value, 1, Maximum);
        public float Current { get; private set; }
        public float Maximum => stats.MaximumHp;
        public bool Alive => Current > 0;
        public event Action Died;
        public event Action<CombatActor> Defeated;
        public event Action Restored;
        public event Action<CombatActor, DamageResult> Damaged;
        // Connected packet, including zero damage or resistance restoration. Distinct from ordinary healing.
        public event Action<CombatActor, DamageResult> PacketResolved;
        public event Action<float> Healed;
        private void Awake() { stats = GetComponent<ActorStats>(); Current = Maximum; }
        private void OnEnable() => stats.Changed += RefreshMaximum;
        private void OnDisable() => stats.Changed -= RefreshMaximum;
        private void Update() { if (Alive) Heal(stats.Regeneration * Time.deltaTime, false); }
        public float ApplyDamage(CombatActor source, DamageResult result) => ApplyPacket(source, result).applied;
        public DamageResult ApplyPacket(CombatActor source, DamageResult result)
        {
            result.applied = result.restored = result.absorbed = 0;
            if (!Alive || Invulnerable || result.Missed) return result;
            if (result.Outcome == PacketOutcome.ResistanceRestoration)
            {
                // Not Heal(): no Healing Done/Received, ordinary-heal event or resurrection.
                result.restored = Mathf.Min(Maximum - Current, result.RestorationPotential);
                Current += result.restored;
            }
            else
            {
                float unshielded=Absorb(result.DamagePotential);
                result.absorbed=result.DamagePotential-unshielded;
                result.applied = Mathf.Min(Current, unshielded);
                Current -= result.applied;
                if (result.applied > 0) Damaged?.Invoke(source, result);
            }
            PacketResolved?.Invoke(source, result);
            if (!Alive) { Died?.Invoke(); Defeated?.Invoke(source); }
            return result;
        }
        public float Heal(float amount, bool present = true)
        {
            if (!Alive) return 0; // Healing is not resurrection.
            var applied = Mathf.Min(Maximum - Current, Mathf.Max(0, amount) * stats.HealingReceived);
            Current += applied;
            if (present && applied > 0) Healed?.Invoke(applied);
            return applied;
        }
        // Environmental healing/regeneration uses Heal; an authored healer supplies its current stats here.
        public float HealFrom(ActorStats source, float amount, bool present = true) =>
            Heal(amount * (source == null ? 1 : source.HealingDone), present);
        public void Restore(float fraction = 1)
        {
            shields.Clear();
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
