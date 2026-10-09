using UnityEngine;

namespace DiceFree.Combat
{
    public enum PacketOutcome { NoHpChange, Damage, ResistanceRestoration, Miss }
    public struct DamageResult
    {
        public float baseRaw, raw, defense, afterDefense, resistance, mitigated, applied, restored;
        public ActionResolution action;
        public ActionCriticalResolution critical;
        public ElementalResistanceResolution elementalResistance;
        public DamageChannel channel;
        public ElementDefinition element;
        public bool Missed => action != null && action.Hit.missed;
        public float DamagePotential => Missed ? 0 : Mathf.Max(0, mitigated);
        public float RestorationPotential => Missed || element == null ? 0 : Mathf.Max(0, -mitigated);
        public PacketOutcome Outcome => Missed ? PacketOutcome.Miss : RestorationPotential > 0
            ? PacketOutcome.ResistanceRestoration : DamagePotential > 0 ? PacketOutcome.Damage : PacketOutcome.NoHpChange;
    }

    public static class DamageResolver
    {
        // Explicit construction/mitigation/application boundaries for future effects and telemetry.
        public static DamageResult Calculate(
            ActorStats source,
            ActorStats target,
            AttackDefinition attack,
            ActionCriticalResolution critical = null,
            float? coefficientOverride = null,
            float rawMultiplier = 1f,
            float basicAttackBonus = 0f,
            float basicAttackRangeOffset = 0f)
        {
            float baseRaw = coefficientOverride.HasValue
                ? attack.RawDamage(source.Attributes, coefficientOverride.Value)
                : attack.RawDamage(source.Attributes);
            baseRaw *= Mathf.Max(0f, rawMultiplier);
            if (source.Definition != null && object.ReferenceEquals(source.Definition.basicAttack, attack))
            {
                // A stat-only upper-end bonus; skills never inherit this equipment term.
                baseRaw += Mathf.Clamp(basicAttackBonus, 0, source.BasicAttackMaximumBonus);
                baseRaw *= source.BasicAttackDamageMultiplier;
                // Flat authored basic-attack damage follows attribute/percentage scaling.
                baseRaw += source.BasicAttackFlatBonus;
                baseRaw=Mathf.Max(0,baseRaw+Mathf.Clamp(basicAttackRangeOffset,source.Definition.basicAttackMinimumOffset,source.Definition.basicAttackMaximumOffset));
            }
            float raw = critical == null ? baseRaw : critical.ApplyToRaw(baseRaw);
            float defense = DefenseMath.Effective(target.Defense(attack.channel), target.DefenseModifiers(attack.channel), source.DefenseModifiers(attack.channel));
            var elemental = target.ResolveResistance(attack.element, ElementalContext.Damage, source);
            float resistance = elemental.effective;
            float afterDefense = raw * DefenseMath.DamageMultiplier(defense, target.Definition.tuning);
            return new DamageResult {
                baseRaw = baseRaw, raw = raw, critical = critical, defense = defense, resistance = resistance, elementalResistance = elemental, channel = attack.channel, element = attack.element,
                afterDefense = afterDefense, mitigated = afterDefense * (1 - resistance) * target.IncomingDamageMultiplier(attack.channel)
            };
        }
        public static DamageResult CalculatePacket(
            ActorStats source,
            ActorStats target,
            AttackDefinition attack,
            ActionResolution action,
            float rawMultiplier = 1f,
            float basicAttackBonus = 0f,
            float basicAttackRangeOffset = 0f)
        {
            if (action == null) throw new System.ArgumentNullException(nameof(action));
            // A miss never constructs raw packets or consults mitigation, and cannot resolve crit here.
            if (action.Hit.missed) return new DamageResult { action = action, channel = attack.channel, element = attack.element };
            var result = Calculate(source, target, attack, action.Critical, null, rawMultiplier, basicAttackBonus,basicAttackRangeOffset);
            result.action = action;
            return result;
        }
        public static float Hit(CombatActor source, CombatActor target, AttackDefinition attack, ActionCriticalResolution critical = null, float? coefficientOverride = null)
        {
            if (!source.IsHostileTo(target)) return 0;
            return target.Health.ApplyDamage(source, Calculate(source.Stats, target.Stats, attack, critical, coefficientOverride));
        }
    }
}
