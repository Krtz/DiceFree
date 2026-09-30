using System;
using DiceFree.Combat;
using UnityEngine;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    public static class CriticalValidation
    {
        private sealed class FixedRoll : ICriticalRollSource
        {
            public float value;
            public int calls;
            public float NextUnit() { calls++; return value; }
        }
        private static void Reject(Action action)
        {
            try { action(); } catch (ArgumentException) { return; }
            throw new InvalidOperationException("Invalid critical rule/roll accepted.");
        }
        public static void Check(CombatActor player, CombatActor enemy)
        {
            var rolls = new FixedRoll { value = 0 };
            var none = CriticalResolver.Evaluate(null, rolls);
            Require(!none.HasRule && !none.triggered && !none.rolled && none.requestedMultiplier == null && rolls.calls == 0,
                "No rule must mean no roll and no default multiplier.");
            Require(!CriticalResolver.Evaluate(null, null).triggered, "No rule must not need an RNG.");
            var denied = CriticalResolver.Evaluate(new CriticalRule("test.disabled", "rule.disabled", false, 1, 8), rolls);
            var zero = CriticalResolver.Evaluate(new CriticalRule("test.zero", "rule.zero", true, 0, 2), rolls);
            Require(!denied.triggered && !zero.triggered && rolls.calls == 0, "Permission/zero chance must skip rolls.");
            foreach (var chance in new[] { 0.5f, 0.125f })
            {
                float multiplier = chance == 0.5f ? 2 : 8;
                var rule = new CriticalRule("test.grant", "rule." + multiplier, true, chance, multiplier);
                rolls.value = chance - 0.001f;
                var hit = CriticalResolver.Evaluate(rule, rolls);
                Require(hit.triggered && hit.rolled && hit.permitted && hit.sourceId == rule.SourceId && hit.ruleId == rule.RuleId &&
                    hit.requestedMultiplier == multiplier && hit.chance == chance && hit.roll == rolls.value, "Explicit grant/provenance failed.");
                rolls.value = chance;
                Require(!CriticalResolver.Evaluate(rule, rolls).triggered, "Threshold boundary must not trigger.");
                rolls.value = chance + 0.001f;
                var miss = CriticalResolver.Evaluate(rule, rolls);
                Require(!miss.triggered && miss.requestedMultiplier == multiplier && miss.sourceId == rule.SourceId, "Non-trigger still retains authored telemetry.");
            }
            Require(rolls.calls == 6, "Each permitted evaluation must use exactly one supplied roll.");
            Reject(() => new CriticalRule("", "rule", true, 0.5f, 2));
            Reject(() => new CriticalRule("source", "rule", true, float.NaN, 2));
            Reject(() => new CriticalRule("source", "rule", true, float.PositiveInfinity, 2));
            Reject(() => new CriticalRule("source", "rule", true, 0.5f, 0));
            var valid = new CriticalRule("source", "rule", true, 0.5f, 2);
            foreach (var invalid in new[] { -0.1f, 1, float.NaN, float.PositiveInfinity })
            { rolls.value = invalid; Reject(() => CriticalResolver.Evaluate(valid, rolls)); }
            var attack = player.Stats.Definition.basicAttack;
            var before = DamageResolver.Calculate(player.Stats, enemy.Stats, attack);
            rolls.value = 0;
            CriticalResolver.Evaluate(valid, rolls); // A standalone result cannot mutate an ordinary attack.
            var after = DamageResolver.Calculate(player.Stats, enemy.Stats, attack);
            Require(before.raw == after.raw && before.mitigated == after.mitigated, "Crit evaluation changed ordinary damage.");
            ActionCriticalValidation.Check(player, enemy);
            ElementalValidation.Check(player, enemy);
            Debug.Log("EXPLICIT_CRIT_OK: absent/denied/zero grants, deterministic thresholds, provenance, validation and unchanged ordinary damage.");
        }
    }
}
