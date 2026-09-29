using System;
using DiceFree.Combat;
using UnityEngine;
using static DiceFree.EditorTools.CombatMathValidation;
using Object = UnityEngine.Object;

namespace DiceFree.EditorTools
{
    public static class ActionCriticalValidation
    {
        private sealed class Sequence : ICriticalRollSource
        {
            private readonly float[] values;
            public int Calls { get; private set; }
            public Sequence(params float[] values) => this.values = values;
            public float NextUnit()
            {
                Require(Calls < values.Length, "Unexpected extra critical roll.");
                return values[Calls++];
            }
        }
        private static void Equal(float actual, float expected, string message) =>
            Require(Mathf.Abs(actual - expected) < 0.0001f, message);
        public static void Check(CombatActor player, CombatActor enemy)
        {
            var noneRolls = new Sequence();
            var none = CriticalResolver.EvaluateAction(null, noneRolls);
            Require(!none.Triggered && none.Evaluated.Count == 0 && noneRolls.Calls == 0, "Empty action rolled.");
            var eight = new CriticalRule("source.eight", "rule.eight", true, 0.125f, 8, -100);
            var two = new CriticalRule("source.two", "rule.two", true, 0.5f, 2, 999);
            foreach (var input in new[] { new[] { two, eight }, new[] { eight, two } })
            {
                var firstRolls = new Sequence(0.1f);
                var first = CriticalResolver.EvaluateAction(input, firstRolls);
                Require(first.Triggered && first.Winner.sourceId == eight.SourceId && first.Evaluated.Count == 1 && firstRolls.Calls == 1,
                    "Highest multiplier must win before lower priority/multiplier rule is rolled.");
                var fallbackRolls = new Sequence(0.9f, 0.1f);
                var fallback = CriticalResolver.EvaluateAction(input, fallbackRolls);
                Require(fallback.Triggered && fallback.Winner.ruleId == two.RuleId && fallback.Winner.sourceId == two.SourceId &&
                    fallback.Winner.requestedMultiplier == 2 && fallback.Winner.chance == 0.5f && fallback.Winner.roll == 0.1f &&
                    fallback.Evaluated[0].ruleId == eight.RuleId && fallback.Evaluated.Count == 2 && fallbackRolls.Calls == 2,
                    "Fallback must keep correct winner/order/provenance.");
                var failRolls = new Sequence(0.9f, 0.9f);
                var fail = CriticalResolver.EvaluateAction(input, failRolls);
                Require(!fail.Triggered && !fail.Winner.HasRule && fail.Evaluated.Count == 2 && failRolls.Calls == 2 &&
                    fail.Evaluated[0].ruleId == eight.RuleId && fail.Evaluated[1].ruleId == two.RuleId, "All-fail resolution failed.");
            }
            var ties = new[] {
                new CriticalRule("a", "a", true, 0.99f, 2),
                new CriticalRule("A", "b", true, 0.9f, 2),
                new CriticalRule("Z", "priority", true, 0.1f, 2, 5),
                new CriticalRule("A", "a", true, 0.2f, 2)
            };
            for (int pass = 0; pass < 2; pass++)
            {
                var tieRolls = new Sequence(0.999f, 0.999f, 0.999f, 0.999f);
                var result = CriticalResolver.EvaluateAction(ties, tieRolls);
                Require(result.Evaluated[0].ruleId == "priority" && result.Evaluated[1].sourceId == "A" &&
                    result.Evaluated[1].ruleId == "a" && result.Evaluated[2].ruleId == "b" && result.Evaluated[3].sourceId == "a",
                    "Equal multipliers must use priority then ordinal SourceId/RuleId, not chance or input order.");
                Array.Reverse(ties);
            }
            var modified = new CriticalRule(two.SourceId, two.RuleId, true, 0.5f, 10); // Caller-finalized replacement, no modifier formula.
            Require(CriticalResolver.EvaluateAction(new[] { eight, modified }, new Sequence(0)).Winner.ruleId == two.RuleId,
                "Ordering did not use final modified multiplier.");
            var cappedRolls = new Sequence(0.9999f);
            var capped = CriticalResolver.EvaluateAction(new[] { new CriticalRule("cap", "cap", true, 1.35f, 2, 1000), two }, cappedRolls);
            Require(capped.Triggered && capped.Winner.chance == 1 && capped.Winner.unclampedChance == 1.35f &&
                capped.Winner.requestedMultiplier == 2 && cappedRolls.Calls == 1, "Chance cap overflowed into extra roll/multiplier.");
            var negative = new CriticalRule("negative", "negative", true, -0.3f, 8);
            var denied = new CriticalRule("denied", "denied", false, 1, 20);
            var skipped = CriticalResolver.EvaluateAction(new[] { denied, negative }, new Sequence());
            Require(!skipped.Triggered && skipped.Evaluated.Count == 1 && !skipped.Evaluated[0].rolled && negative.Chance == 0,
                "Denied permission/negative clamped chance consumed RNG.");

            var targetDefinition = Object.Instantiate(enemy.Stats.Definition);
            var fixture = new GameObject("Action crit packet target");
            var packet = Object.Instantiate(player.Stats.Definition.basicAttack);
            var element = ScriptableObject.CreateInstance<ElementDefinition>(); element.stableId = "test.fire";
            try
            {
                var target = fixture.AddComponent<ActorStats>(); target.Configure(targetDefinition);
                targetDefinition.baseAttributes = new AttributeValues(0);
                targetDefinition.physicalDefense = 300; targetDefinition.magicalDefense = 0;
                targetDefinition.resistances = new[] { new ElementResistance { element = element, fraction = 0.5f } };
                packet.baseDamage = 100; packet.coefficient = 0; packet.channel = DamageChannel.Physical; packet.element = element;
                var force = new CriticalRule("fixture", "double", true, 1, 2);
                var rolls = new Sequence(0);
                var action = CriticalResolver.EvaluateAction(new[] { force }, rolls);
                var hit = DamageResolver.Calculate(player.Stats, target, packet, action);
                Equal(hit.baseRaw, 100, "Packet construction changed."); Equal(hit.raw, 200, "Crit must modify RAW before mitigation.");
                Equal(hit.defense, 300, "Defense input changed."); Equal(hit.resistance, 0.5f, "Resistance input changed.");
                Equal(hit.mitigated, 50, "Raw -> Defense -> resistance pipeline failed.");
                var pairRolls = new Sequence(0);
                var pairAction = CriticalResolver.EvaluateAction(new[] { force }, pairRolls);
                packet.baseDamage = 60; packet.element = null;
                var physical = DamageResolver.Calculate(player.Stats, target, packet, pairAction);
                packet.baseDamage = 40; packet.channel = DamageChannel.Magical; packet.element = element;
                var fire = DamageResolver.Calculate(player.Stats, target, packet, pairAction);
                Equal(physical.raw, 120, "First shared raw packet."); Equal(fire.raw, 80, "Second shared raw packet.");
                Equal(physical.mitigated, 60, "Physical packet mitigation."); Equal(fire.mitigated, 40, "Elemental packet mitigation.");
                Require(ReferenceEquals(physical.critical, pairAction) && ReferenceEquals(fire.critical, pairAction) &&
                    pairAction.Evaluated.Count == 1 && pairRolls.Calls == 1, "Packets must share one action result without further rolls/signals.");
                foreach (var actor in Object.FindObjectsByType<CombatActor>(FindObjectsInactive.Include))
                {
                    var stats = actor.GetComponent<ActorStats>(); var ordinary = stats.Definition.basicAttack;
                    var victim = actor == player ? enemy.Stats : player.Stats;
                    var baseline = DamageResolver.Calculate(stats, victim, ordinary);
                    var explicitNone = DamageResolver.Calculate(stats, victim, ordinary, none);
                    float expected = ordinary.RawDamage(stats.Attributes) * DefenseMath.DamageMultiplier(victim.Defense(ordinary.channel), victim.Definition.tuning)
                        * (1 - victim.Resistance(ordinary.element));
                    Require(baseline.critical == null && baseline.raw == baseline.baseRaw && baseline.raw == explicitNone.raw &&
                        baseline.mitigated == explicitNone.mitigated && baseline.mitigated == expected && noneRolls.Calls == 0,
                        "Existing Cornberg attack changed without a critical grant.");
                }
            }
            finally { Object.Destroy(fixture); Object.Destroy(targetDefinition); Object.Destroy(packet); Object.Destroy(element); }
            Debug.Log("ACTION_CRIT_OK: order, stopping, fallback, ties, caps, finalized rules, raw placement and shared packet result; ordinary Cornberg unchanged.");
        }
    }
}
