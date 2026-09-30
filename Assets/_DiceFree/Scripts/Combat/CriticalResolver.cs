using System;
using System.Collections.Generic;

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
        public readonly float? chance, unclampedChance, roll, requestedMultiplier;
        public readonly int priority;
        public bool HasRule => ruleId != null;
        internal CriticalResolution(CriticalRule rule, float? roll)
        {
            sourceId = rule.SourceId; ruleId = rule.RuleId; permitted = rule.Permitted;
            chance = rule.Chance; requestedMultiplier = rule.RequestedMultiplier;
            unclampedChance = rule.UnclampedChance; priority = rule.Priority;
            this.roll = roll; rolled = roll.HasValue; triggered = rolled && roll.Value < rule.Chance;
        }
    }

    public static class CriticalResolver
    {
        // Lower-level single-rule evaluation; ordinary actions use EvaluateAction once.
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
        public static ActionCriticalResolution EvaluateAction(IEnumerable<CriticalRule> finalizedRules, ICriticalRollSource rolls, ActionProvenance origin = null)
        {
            var ordered = new List<CriticalRule>();
            var identities = new HashSet<(string, string)>();
            if (finalizedRules != null)
                foreach (var rule in finalizedRules)
                {
                    if (rule == null) throw new ArgumentException("Null entry in critical rules.", nameof(finalizedRules));
                    if (!rule.Permitted) continue;
                    if (!identities.Add((rule.SourceId, rule.RuleId)))
                        throw new ArgumentException("Each finalized source/rule identity must be unique per action.", nameof(finalizedRules));
                    ordered.Add(rule);
                }
            ordered.Sort((a, b) => {
                int order = b.RequestedMultiplier.CompareTo(a.RequestedMultiplier);
                if (order != 0) return order;
                order = b.Priority.CompareTo(a.Priority);
                if (order != 0) return order;
                order = StringComparer.Ordinal.Compare(a.SourceId, b.SourceId);
                return order != 0 ? order : StringComparer.Ordinal.Compare(a.RuleId, b.RuleId);
            });
            var evaluated = new List<CriticalResolution>();
            CriticalResolution winner = default;
            foreach (var rule in ordered)
            {
                var result = Evaluate(rule, rolls); evaluated.Add(result);
                if (result.triggered) { winner = result; break; }
            }
            return new ActionCriticalResolution(winner, evaluated.ToArray(), origin);
        }
    }
}
