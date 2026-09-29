using System;

namespace DiceFree.Combat
{
    public interface ICriticalRollSource
    {
        // Uniform roll contract: [0, 1). Caller owns RNG/seed; no implicit global random source.
        float NextUnit();
    }

    public readonly struct CriticalResolution
    {
        public readonly string sourceId, ruleId;
        public readonly bool permitted, rolled, triggered;
        public readonly float? chance, roll, requestedMultiplier;
        public bool HasRule => ruleId != null;
        internal CriticalResolution(CriticalRule rule, float? roll)
        {
            sourceId = rule.SourceId; ruleId = rule.RuleId; permitted = rule.Permitted;
            chance = rule.Chance; requestedMultiplier = rule.RequestedMultiplier;
            this.roll = roll; rolled = roll.HasValue; triggered = rolled && roll.Value < rule.Chance;
        }
    }

    public static class CriticalResolver
    {
        // Evaluates exactly one grant. Selecting/combining grants and applying the outcome
        // within damage/healing resolution are intentionally NOT policies of this seam.
        public static CriticalResolution Evaluate(CriticalRule rule, ICriticalRollSource rolls)
        {
            if (rule == null) return default;
            if (!rule.Permitted || rule.Chance == 0) return new CriticalResolution(rule, null);
            if (rolls == null) throw new ArgumentNullException(nameof(rolls));
            float roll = rolls.NextUnit();
            if (float.IsNaN(roll) || float.IsInfinity(roll) || roll < 0 || roll >= 1)
                throw new ArgumentOutOfRangeException(nameof(rolls), "Critical roll must be in [0, 1).");
            return new CriticalResolution(rule, roll);
        }
    }
}
