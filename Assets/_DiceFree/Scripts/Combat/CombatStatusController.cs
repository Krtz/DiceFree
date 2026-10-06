using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor))]
    public sealed class CombatStatusController : MonoBehaviour
    {
        private readonly struct TimedValue
        {
            public readonly float value;
            public readonly float until;
            public TimedValue(float amount, float expiresAt) { value = amount; until = expiresAt; }
        }

        [SerializeField, Range(0, 1)] private float stunResistance;
        [SerializeField] private bool stunImmune;

        private readonly SortedDictionary<string, TimedValue> accuracyPenalties = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, TimedValue> attackSpeedBuffs = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, TimedValue> movementSpeedBuffs = new(StringComparer.Ordinal);
        private readonly SortedDictionary<string, TimedValue> damageReductionBuffs = new(StringComparer.Ordinal);
        private readonly List<string> expired = new();
        private CombatActor actor;
        private float stunnedUntil;
        private bool stunBlockApplied;
        private const string StunMotionBlock = "status.stun";

        public bool Stunned => Time.time < stunnedUntil;
        public bool StunImmune => stunImmune;
        public float StunResistance => stunResistance;
        public float AccuracyMissChance
        {
            get
            {
                CleanupExpired();
                float total = 0;
                foreach (var effect in accuracyPenalties.Values) total += effect.value;
                return Mathf.Clamp01(total);
            }
        }

        private void Awake() => actor = GetComponent<CombatActor>();

        private void OnEnable()
        {
            if (actor == null) actor = GetComponent<CombatActor>();
            if (actor?.Health != null) actor.Health.Restored += OnRestored;
        }

        private void OnDisable()
        {
            if (actor?.Health != null) actor.Health.Restored -= OnRestored;
            ClearTransient();
        }

        private void Update()
        {
            CleanupExpired();
            SyncStunBlock();
        }

        public void ConfigureControl(float ordinaryStunResistance, bool hardStunImmune)
        {
            stunResistance = Mathf.Clamp01(ordinaryStunResistance);
            stunImmune = hardStunImmune;
        }

        public float ResolveStunDuration(float requestedSeconds, bool ignoreOrdinaryResistance)
        {
            if (requestedSeconds <= 0 || stunImmune) return 0;
            return ignoreOrdinaryResistance
                ? requestedSeconds
                : requestedSeconds * (1 - Mathf.Clamp01(stunResistance));
        }

        public bool TryApplyStun(float requestedSeconds, bool ignoreOrdinaryResistance)
        {
            float duration = ResolveStunDuration(requestedSeconds, ignoreOrdinaryResistance);
            if (duration <= 0 || actor == null || !actor.Alive) return false;
            stunnedUntil = Mathf.Max(stunnedUntil, Time.time + duration);
            actor.GetComponent<BasicAttack>()?.Cancel();
            actor.Motor.Stop();
            SyncStunBlock();
            return true;
        }

        public void ApplyAccuracyPenalty(string effectId, float missChance, float durationSeconds)
        {
            ValidateEffect(effectId, durationSeconds);
            // Reapplying the same stable effect refreshes/replaces it; duplicate copies do not stack.
            accuracyPenalties[effectId] = new TimedValue(Mathf.Clamp01(missChance), Time.time + durationSeconds);
        }

        public void ApplyAttackSpeedBuff(string effectId, float percent, float durationSeconds)
        {
            ValidateEffect(effectId, durationSeconds);
            if (float.IsNaN(percent) || float.IsInfinity(percent) || percent < 0)
                throw new ArgumentOutOfRangeException(nameof(percent));
            // Reapplying the same stable effect refreshes/replaces it; duplicate copies do not stack.
            attackSpeedBuffs[effectId] = new TimedValue(percent, Time.time + durationSeconds);
            actor.Stats.SetAttackSpeedPercentModifier("status:" + effectId, percent);
        }

        public void ApplyMovementSpeedBuff(string effectId, float percent, float durationSeconds)
        {
            ValidateEffect(effectId, durationSeconds);
            if (float.IsNaN(percent) || float.IsInfinity(percent) || percent < 0)
                throw new ArgumentOutOfRangeException(nameof(percent));
            movementSpeedBuffs[effectId] = new TimedValue(percent, Time.time + durationSeconds);
            actor.Stats.SetMovementSpeedPercentModifier("status:" + effectId, percent);
        }

        public void ApplyDamageReduction(string effectId, float fraction, float durationSeconds)
        {
            ValidateEffect(effectId, durationSeconds);
            if (float.IsNaN(fraction) || float.IsInfinity(fraction) || fraction < 0 || fraction >= 1)
                throw new ArgumentOutOfRangeException(nameof(fraction));
            damageReductionBuffs[effectId] = new TimedValue(fraction, Time.time + durationSeconds);
            actor.Stats.SetDamageTakenModifier("status:" + effectId + ":physical", new DamageTakenModifier(DamageChannel.Physical, fraction));
            actor.Stats.SetDamageTakenModifier("status:" + effectId + ":magical", new DamageTakenModifier(DamageChannel.Magical, fraction));
        }

        public void ClearTransient()
        {
            if (actor != null)
            {
                foreach (var effectId in attackSpeedBuffs.Keys)
                    actor.Stats.RemoveAttackSpeedPercentModifier("status:" + effectId);
                foreach (var effectId in movementSpeedBuffs.Keys)
                    actor.Stats.RemoveMovementSpeedPercentModifier("status:" + effectId);
                foreach (var effectId in damageReductionBuffs.Keys)
                {
                    actor.Stats.RemoveDamageTakenModifier("status:" + effectId + ":physical");
                    actor.Stats.RemoveDamageTakenModifier("status:" + effectId + ":magical");
                }
            }
            accuracyPenalties.Clear();
            attackSpeedBuffs.Clear();
            movementSpeedBuffs.Clear();
            damageReductionBuffs.Clear();
            stunnedUntil = 0;
            SyncStunBlock();
        }

        private void CleanupExpired()
        {
            float now = Time.time;
            expired.Clear();
            foreach (var pair in accuracyPenalties)
                if (now >= pair.Value.until) expired.Add(pair.Key);
            foreach (string id in expired) accuracyPenalties.Remove(id);

            expired.Clear();
            foreach (var pair in attackSpeedBuffs)
                if (now >= pair.Value.until) expired.Add(pair.Key);
            foreach (string id in expired)
            {
                attackSpeedBuffs.Remove(id);
                actor?.Stats.RemoveAttackSpeedPercentModifier("status:" + id);
            }

            expired.Clear();
            foreach (var pair in movementSpeedBuffs)
                if (now >= pair.Value.until) expired.Add(pair.Key);
            foreach (string id in expired)
            {
                movementSpeedBuffs.Remove(id);
                actor?.Stats.RemoveMovementSpeedPercentModifier("status:" + id);
            }

            expired.Clear();
            foreach (var pair in damageReductionBuffs)
                if (now >= pair.Value.until) expired.Add(pair.Key);
            foreach (string id in expired)
            {
                damageReductionBuffs.Remove(id);
                actor?.Stats.RemoveDamageTakenModifier("status:" + id + ":physical");
                actor?.Stats.RemoveDamageTakenModifier("status:" + id + ":magical");
            }
        }

        private void SyncStunBlock()
        {
            if (actor == null) return;
            bool shouldBlock = actor.Alive && Stunned;
            if (shouldBlock == stunBlockApplied) return;
            stunBlockApplied = shouldBlock;
            actor.Motor.SetMotionBlocked(StunMotionBlock, shouldBlock);
        }

        private void OnRestored()
        {
            ClearTransient();
        }

        private static void ValidateEffect(string effectId, float durationSeconds)
        {
            if (string.IsNullOrWhiteSpace(effectId)) throw new ArgumentException("Stable effect identity required.");
            if (float.IsNaN(durationSeconds) || float.IsInfinity(durationSeconds) || durationSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
        }
    }
}
