using UnityEngine;

namespace DiceFree.Combat
{
    public struct DamageResult
    {
        public float raw, defense, resistance, mitigated, applied;
        public DamageChannel channel;
        public ElementDefinition element;
    }

    public static class DamageResolver
    {
        // Explicit construction/mitigation/application boundaries for future effects and telemetry.
        public static DamageResult Calculate(ActorStats source, ActorStats target, AttackDefinition attack)
        {
            float raw = attack.RawDamage(source.Attributes);
            float defense = DefenseMath.Effective(target.Defense(attack.channel), target.DefenseModifiers(attack.channel), source.DefenseModifiers(attack.channel));
            float resistance = target.Resistance(attack.element);
            return new DamageResult {
                raw = raw, defense = defense, resistance = resistance, channel = attack.channel, element = attack.element,
                mitigated = raw * DefenseMath.DamageMultiplier(defense, target.Definition.tuning) * (1 - resistance)
            };
        }
        public static float Hit(CombatActor source, CombatActor target, AttackDefinition attack)
        {
            if (!source.IsHostileTo(target)) return 0;
            return target.Health.ApplyDamage(source, Calculate(source.Stats, target.Stats, attack));
        }
    }
}
