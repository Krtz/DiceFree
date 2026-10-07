using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Skills
{
    public enum MagicalSkillKind
    {
        MagicSand,
        Mend,
        FireImbuement,
        IceBurst,
        ManaAttunement
    }

    [CreateAssetMenu(menuName = "DiceFree/Skills/Magically Touched skill")]
    public sealed class MagicalSkillDefinition : ScriptableObject
    {
        public string stableId;
        public string displayName;
        public MagicalSkillKind kind;
        [Min(1)] public int maxRank = 6;
        [Min(0)] public float cooldownSeconds;
        [Min(0)] public float durationSeconds;
        [Min(0)] public float range;
        public AttackDefinition attack;
        public AttackDefinition secondaryAttack;

        [Header("Mana cost curve")]
        [Min(0)] public float manaCostRank1;
        [Min(0)] public float manaCostLinear;
        [Min(0)] public float manaCostQuadratic;

        [Header("Damage / healing")]
        [Min(0)] public float coefficientRank1;
        [Min(0)] public float coefficientPerRank;
        [Min(0)] public float healCoefficientRank1;
        [Min(0)] public float healCoefficientPerRank;

        [Header("Ice Burst")]
        [Min(0)] public float iceDelaySeconds = 0.75f;
        [Min(0)] public float iceRadius = 3.5f;
        [Min(0)] public float slowDurationSeconds = 4;

        [Header("Mana Attunement")]
        [Min(0)] public float maximumManaPerRank = 20;
        [Min(0)] public float personalRegenPerRank = 0.75f;
        [Min(0)] public float auraRegenPerRank = 0.25f;
        [Min(0)] public float auraRadius = 8;

        public bool Active => kind != MagicalSkillKind.ManaAttunement;
        public int ClampRank(int rank) => Mathf.Clamp(rank, 0, maxRank);

        public float ManaCost(int rank)
        {
            int extra = Mathf.Max(0, ClampRank(rank) - 1);
            return manaCostRank1 + manaCostLinear * extra + manaCostQuadratic * extra * extra;
        }

        public float DamageCoefficient(int rank) =>
            coefficientRank1 + coefficientPerRank * Mathf.Max(0, ClampRank(rank) - 1);

        public float HealAmount(AttributeValues attributes, int rank) =>
            attributes.spirit *
            (healCoefficientRank1 + healCoefficientPerRank * Mathf.Max(0, ClampRank(rank) - 1));

        public float MagicSandMissChance(int rank) => Mathf.Clamp01(ClampRank(rank) * 0.075f);
        public float ImbuementCoefficient(int rank) => DamageCoefficient(rank);
        public float IceSlowPercent(int rank) => ClampRank(rank) * 2f;
        public float MaximumManaBonus(int rank) => maximumManaPerRank * ClampRank(rank);
        public float PersonalRegenBonus(int rank) => personalRegenPerRank * ClampRank(rank);
        public float AuraRegenBonus(int rank) => auraRegenPerRank * ClampRank(rank);
    }
}
