using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class CombatMathValidation
    {
        public static void Check(CombatActor player, CombatActor enemy)
        {
            var stats = player.Stats;
            Require(stats.Attributes.vitality == 1 && stats.Attributes.strength == 1 && stats.Attributes.agility == 1 &&
                stats.Attributes.intelligence == 1 && stats.Attributes.spirit == 1, "Novice starting attributes.");
            var ten = AttributeValues.AtLevel(stats.Definition.baseAttributes, stats.Definition.growth, 10);
            Require(ten.vitality == 10 && ten.strength == 10 && ten.agility == 10 && ten.intelligence == 10 && ten.spirit == 10, "Novice growth.");
            var fists = stats.Definition.basicAttack;
            foreach (var values in new[] {
                new AttributeValues(1) { vitality = 8 }, new AttributeValues(1) { strength = 8 },
                new AttributeValues(1) { agility = 8 }, new AttributeValues(1) { intelligence = 8 }, new AttributeValues(1) { spirit = 8 } })
                Require(Mathf.Approximately(fists.RawDamage(values), fists.baseDamage + 8 * fists.coefficient), "All five stats must scale fists.");
            Require(fists.element == null && enemy.Stats.Definition.basicAttack.element == null, "Starting attacks must have no element.");
            var neutral = DamageResolver.Calculate(stats, enemy.Stats, fists);
            Require(neutral.resistance == 0, "No-element damage must ignore negative elemental baseline.");
            // Unregistered arbitrary element proves mechanics do not depend on a named enum roster.
            var element = ScriptableObject.CreateInstance<ElementDefinition>(); element.stableId = "test.arbitrary";
            var elemental = UnityEngine.Object.Instantiate(fists); elemental.element = element;
            try
            {
                var result = DamageResolver.Calculate(stats, enemy.Stats, elemental);
                Require(Mathf.Approximately(result.mitigated, neutral.mitigated * 1.1f), "Generic negative elemental resistance.");
            }
            finally { UnityEngine.Object.Destroy(elemental); UnityEngine.Object.Destroy(element); }
            Require(player.Health.Heal(100) == 0,"Overhealing must be discarded.");
            player.Health.ApplyDamage(enemy,new DamageResult { raw = 2, mitigated = 2 });
            Require(Mathf.Approximately(player.Health.Heal(1),stats.HealingReceived),"All ordinary healing must apply receiver scaling once.");
            Require(player.Health.Heal(-1) == 0,"Ordinary negative healing is not damage.");
            player.Health.Heal(100);
            Require(Mathf.Approximately(player.Health.Current,player.Health.Maximum),"Healing exceeded max HP.");
            Debug.Log("CORNBERG_COMBAT_MATH_OK: base/growth, all five adaptive stats, neutral and arbitrary element.");
        }
        public static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
