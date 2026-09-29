using DiceFree.Combat;
using UnityEngine;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    public static class SecondaryStatsValidation
    {
        private static void Equal(float actual, float expected, string message) =>
            Require(Mathf.Abs(actual - expected) < 0.0001f, message + $" (actual {actual}, expected {expected})");

        public static void Check(CombatActor player)
        {
            var definition = Object.Instantiate(player.Stats.Definition);
            var targetDefinition = Object.Instantiate(definition);
            var sourceObject = new GameObject("Secondary source fixture");
            var targetObject = new GameObject("Secondary target fixture");
            var attack = Object.Instantiate(definition.basicAttack);
            try
            {
                var source = sourceObject.AddComponent<ActorStats>(); source.Configure(definition);
                var target = targetObject.AddComponent<ActorStats>(); target.Configure(targetDefinition);
                definition.baseAttributes = new AttributeValues(100);
                Equal(source.Defense(DamageChannel.Physical), 20, "STR Defense coefficient");
                Equal(source.Defense(DamageChannel.Magical), 20, "INT Defense coefficient");
                Equal(source.AttackSpeed, 1.025f, "AGI attack speed percent conversion");
                Equal(source.MoveSpeed / definition.moveSpeed, 1.01f, "AGI move speed percent conversion");
                Equal(source.HealingDone, 1.15f, "SPI healing done percent conversion");
                Equal(source.HealingReceived, 1.075f, "SPI healing received percent conversion");
                definition.secondaryOverrides = new[] { new SecondaryCoefficientOverride { stat = SecondaryStat.PhysicalDefense, coefficient = 0.4f } };
                Equal(source.Defense(DamageChannel.Physical), 40, "Actor coefficient override");
                Equal(source.Defense(DamageChannel.Magical), 20, "Override must not affect other coefficient");
                source.SetSecondaryModifier("test.scaling", new SecondaryScalingModifier { stat = SecondaryStat.PhysicalDefense, add = 0.1f, percent = 0.2f });
                Equal(source.Defense(DamageChannel.Physical), 60, "Coefficient additive/percentage modifiers");
                source.RemoveSecondaryModifier("test.scaling");
                Equal(source.Defense(DamageChannel.Physical), 40, "Coefficient modifier removal");

                var tuning = definition.tuning;
                Equal(DefenseMath.DamageMultiplier(0, tuning), 1, "Zero Defense");
                Equal(DefenseMath.DamageMultiplier(300, tuning), 0.5f, "Positive Defense anchor");
                Equal(DefenseMath.DamageMultiplier(-300, tuning), 1.5f, "Negative Defense anchor");
                Equal(DefenseMath.DamageMultiplier(100, tuning), 0.68331f, "Nonlinear positive Defense");
                Equal(DefenseMath.DamageMultiplier(-100, tuning), 1.31669f, "Symmetric vulnerability");
                Require(DefenseMath.DamageMultiplier(-1000000, tuning) < 2, "Negative Defense vulnerability bound");
                Equal(DefenseMath.Effective(100, new DefenseModifier { buffPercent = 0.2f, buffFlat = 50, reductionPercent = 0.2f }, default),
                    150, "Positive buffs order and underlying reduction reference");
                Equal(DefenseMath.Effective(100, new DefenseModifier { reductionPercent = 0.5f, reductionFlat = 10 },
                    new DefenseModifier { penetrationPercent = 0.2f, penetrationFlat = 5 }), 15, "Reduction before penetration, original reference");
                Equal(DefenseMath.Effective(100, new DefenseModifier { reductionPercent = 1.1f }, default), -10, "Additive reduction beyond 100 percent");
                Equal(DefenseMath.Effective(100, default, new DefenseModifier { penetrationPercent = 1.2f }), -20, "Penetration beyond 100 percent");
                Equal(DefenseMath.Effective(5, default, new DefenseModifier { penetrationFlat = 10 }), -5, "Flat overpenetration");
                Equal(DefenseMath.Effective(-10, new DefenseModifier { reductionPercent = 0.5f, reductionFlat = 2 },
                    new DefenseModifier { penetrationPercent = 0.5f, penetrationFlat = 3 }), -15, "Negative underlying Defense ignores percentage reduction/penetration");

                targetDefinition.baseAttributes = new AttributeValues(0);
                targetDefinition.physicalDefense = targetDefinition.magicalDefense = 100;
                attack.baseDamage = 100; attack.coefficient = 0; attack.element = null;
                target.SetDefenseModifier("test.shred-a", new DefenseModifier { reductionPercent = 0.5f });
                target.SetDefenseModifier("test.shred-b", new DefenseModifier { reductionPercent = 0.6f });
                source.SetDefenseModifier("test.penetration", new DefenseModifier { penetrationFlat = 10 });
                var hit = DamageResolver.Calculate(source, target, attack);
                Equal(hit.defense, -20, "Runtime additive reductions plus source penetration");
                Equal(hit.mitigated, 100 * DefenseMath.DamageMultiplier(-20, tuning), "Runtime uses vulnerability curve");
                attack.channel = DamageChannel.Magical;
                Equal(DamageResolver.Calculate(source, target, attack).defense, 100, "Physical modifiers must not affect magical channel");
                Equal(DamageResolver.Calculate(source, target, attack).mitigated, 100 * DefenseMath.DamageMultiplier(100, tuning), "Magical curve matches physical");
                source.SetDefenseModifier("test.penetration", new DefenseModifier { channel = DamageChannel.Magical, penetrationPercent = 0.5f });
                Equal(DamageResolver.Calculate(source, target, attack).defense, 50, "Source-key replacement and magical penetration");
                source.RemoveDefenseModifier("test.penetration"); target.ResetTransientModifiers();
                Equal(DamageResolver.Calculate(source, target, attack).defense, 100, "Modifier removal/reset");

                player.Health.ApplyDamage(null, new DamageResult { mitigated = 10 });
                Equal(player.Health.HealFrom(source, 1), 1.15f * player.Stats.HealingReceived, "Source and receiver healing each applied once");
                Equal(player.Health.HealFrom(null, 1), player.Stats.HealingReceived, "Environmental healing has no source scaling");
                Equal(player.Health.HealFrom(source, -1), 0, "Negative source healing is not damage");
            }
            finally
            {
                player.Health.Restore(); Object.Destroy(sourceObject); Object.Destroy(targetObject);
                Object.Destroy(definition); Object.Destroy(targetDefinition); Object.Destroy(attack);
            }
            Debug.Log("SECONDARY_STATS_OK: coefficients, overrides, modifiers, Defense curve/order/channels and source healing.");
        }
    }
}
