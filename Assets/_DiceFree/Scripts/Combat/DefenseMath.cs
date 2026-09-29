using UnityEngine;

namespace DiceFree.Combat
{
    // Transient target buffs/reductions and attacker penetration share a channel, not an owner.
    // Resolution reads target fields only from the victim and penetration only from the attacker.
    public struct DefenseModifier
    {
        public DamageChannel channel;
        public float buffPercent, buffFlat, reductionPercent, reductionFlat, penetrationPercent, penetrationFlat;
        public void Add(DefenseModifier value)
        {
            buffPercent += value.buffPercent; buffFlat += value.buffFlat;
            reductionPercent += value.reductionPercent; reductionFlat += value.reductionFlat;
            penetrationPercent += value.penetrationPercent; penetrationFlat += value.penetrationFlat;
        }
    }

    public static class DefenseMath
    {
        public static float Effective(float underlying, DefenseModifier target, DefenseModifier attacker)
        {
            float reference = Mathf.Max(0, underlying);
            float current = underlying * (1 + target.buffPercent) + target.buffFlat;
            current -= reference * target.reductionPercent;
            current -= target.reductionFlat;
            current -= reference * attacker.penetrationPercent;
            return current - attacker.penetrationFlat;
        }
        public static float DamageMultiplier(float defense, CombatTuning tuning)
        {
            float q = Mathf.Pow(Mathf.Abs(defense) / Mathf.Max(1, tuning.defenseConstant), Mathf.Max(0.01f, tuning.defenseExponent));
            float inverse = 1 / (1 + q);
            return defense >= 0 ? inverse : 2 - inverse;
        }
    }
}
