using UnityEngine;

namespace DiceFree.Combat
{
    public struct DamageResult
    {
        public float baseRaw, raw, defense, resistance, mitigated, applied;
        public ActionCriticalResolution critical;
        public ElementalResistanceResolution elementalResistance;
        public DamageChannel channel;
        public ElementDefinition element;
    }

    public static class DamageResolver
    {
        // Explicit construction/mitigation/application boundaries for future effects and telemetry.
        public static DamageResult Calculate(ActorStats source, ActorStats target, AttackDefinition attack, ActionCriticalResolution critical = null)
        {
            float baseRaw = attack.RawDamage(source.Attributes);
            float raw = critical == null ? baseRaw : critical.ApplyToRaw(baseRaw);
            float defense = DefenseMath.Effective(target.Defense(attack.channel), target.DefenseModifiers(attack.channel), source.DefenseModifiers(attack.channel));
            var elemental = target.ResolveResistance(attack.element, ElementalContext.Damage, source);
            float resistance = elemental.effective;
            return new DamageResult {
                baseRaw = baseRaw, raw = raw, critical = critical, defense = defense, resistance = resistance, elementalResistance = elemental, channel = attack.channel, element = attack.element,
                mitigated = raw * DefenseMath.DamageMultiplier(defense, target.Definition.tuning) * (1 - resistance)
            };
        }
        public static float Hit(CombatActor source, CombatActor target, AttackDefinition attack, ActionCriticalResolution critical = null)
        {
            if (!source.IsHostileTo(target)) return 0;
            return target.Health.ApplyDamage(source, Calculate(source.Stats, target.Stats, attack, critical));
        }
    }
}
