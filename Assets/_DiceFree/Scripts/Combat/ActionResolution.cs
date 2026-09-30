using System;
using System.Collections.Generic;

namespace DiceFree.Combat
{
    public interface IMissRollSource { float NextUnit(); } // [0,1), caller-owned RNG.

    public sealed class MissChance
    {
        public float Unclamped { get; }
        public float Effective { get; }
        // Explicit percentages only; 0.25 means 25%, deltas are additive percentage points.
        public MissChance(float baseChance, params float[] additiveDeltas)
        {
            float sum = Finite(baseChance);
            if (additiveDeltas != null) foreach (float delta in additiveDeltas) sum = Finite(sum + Finite(delta));
            Unclamped = sum; Effective = Math.Max(0, Math.Min(1, sum));
        }
        private static float Finite(float value) => float.IsNaN(value) || float.IsInfinity(value)
            ? throw new ArgumentOutOfRangeException(nameof(value)) : value;
    }

    public readonly struct ActionHitResolution
    {
        public readonly ActionProvenance origin;
        public readonly MissChance missChance;
        public readonly float? roll;
        public readonly bool missed;
        internal ActionHitResolution(ActionProvenance origin, MissChance chance, float? roll, bool missed)
        { this.origin = origin; missChance = chance; this.roll = roll; this.missed = missed; }
    }

    // Call once AFTER action validity checks; all normal packets share this immutable result.
    // Child effects require their own resolution unless explicitly authored otherwise.
    public sealed class ActionResolution
    {
        public ActionProvenance Origin => Hit.origin;
        public ActionHitResolution Hit { get; }
        public ActionCriticalResolution Critical { get; } // null on miss: crit was never evaluated.
        private ActionResolution(ActionHitResolution hit, ActionCriticalResolution critical) { Hit = hit; Critical = critical; }
        public static ActionResolution Resolve(ActionProvenance origin, MissChance missChance = null,
            IMissRollSource missRolls = null, IEnumerable<CriticalRule> critRules = null, ICriticalRollSource critRolls = null)
        {
            if (origin == null) throw new ArgumentNullException(nameof(origin));
            float chance = missChance?.Effective ?? 0;
            float? roll = null;
            bool missed = chance == 1; // 0% and 100% are deterministic and consume no RNG.
            if (chance > 0 && chance < 1)
            {
                if (missRolls == null) throw new ArgumentNullException(nameof(missRolls));
                roll = missRolls.NextUnit();
                if (float.IsNaN(roll.Value) || float.IsInfinity(roll.Value) || roll < 0 || roll >= 1)
                    throw new ArgumentOutOfRangeException(nameof(missRolls), "Miss roll must be in [0, 1).");
                missed = roll < chance;
            }
            var hit = new ActionHitResolution(origin, missChance, roll, missed);
            return new ActionResolution(hit, missed ? null : CriticalResolver.EvaluateAction(critRules, critRolls, origin));
        }
    }
}
