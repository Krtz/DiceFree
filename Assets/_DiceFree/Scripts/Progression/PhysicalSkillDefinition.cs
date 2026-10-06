using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Skills
{
    public enum PhysicalSkillKind
    {
        HeavyStrike,
        Guard,
        Quickening,
        ArrowRain,
        MartialAptitude
    }

    [CreateAssetMenu(menuName = "DiceFree/Skills/Physically Blessed skill")]
    public sealed class PhysicalSkillDefinition : ScriptableObject
    {
        public string stableId;
        public string displayName;
        public PhysicalSkillKind kind;
        [Min(1)] public int maxRank = 6;
        [Min(0)] public float cooldownSeconds;
        [Min(0)] public float durationSeconds;
        [Min(0)] public float range = 0.7f;
        [Min(0)] public float manaBase;
        [Min(0)] public float manaPerAdditionalRank;
        public AttackDefinition attack;

        [Header("Prototype tuning")]
        [Min(0)] public float damageCoefficientRank1;
        [Min(0)] public float damageCoefficientPerAdditionalRank;
        [Min(0)] public float arrowRadius = 3;
        [Min(1)] public int arrowHitCount = 3;
        [Min(0)] public float arrowHitInterval = 0.35f;

        public bool Active => kind != PhysicalSkillKind.MartialAptitude;
        public int ClampRank(int rank) => Mathf.Clamp(rank, 0, Mathf.Max(1, maxRank));
        public float ManaCost(int rank) => rank <= 0 ? 0 : manaBase + Mathf.Max(0, ClampRank(rank) - 1) * manaPerAdditionalRank;
        public float DamageCoefficient(int rank) => rank <= 0 ? 0 : damageCoefficientRank1 + Mathf.Max(0, ClampRank(rank) - 1) * damageCoefficientPerAdditionalRank;
        public float HeavyStunSeconds(int rank) => kind == PhysicalSkillKind.HeavyStrike && rank > 0 ? 0.2f + ClampRank(rank) * 0.2f : 0;
        public float GuardReduction(int rank) => kind == PhysicalSkillKind.Guard && rank > 0 ? 0.10f + ClampRank(rank) * 0.05f : 0;
        public float QuickeningAttackSpeedPercent(int rank) => kind == PhysicalSkillKind.Quickening && rank > 0 ? 8f + ClampRank(rank) * 4f : 0;
        public float QuickeningMoveSpeedPercent(int rank) => kind == PhysicalSkillKind.Quickening && rank > 0 ? 3f + ClampRank(rank) : 0;
        public float MartialBasicAttackPercent(int rank) => kind == PhysicalSkillKind.MartialAptitude ? ClampRank(rank) * 5f : 0;
        public float MartialPhysicalDefense(int rank) => kind == PhysicalSkillKind.MartialAptitude ? ClampRank(rank) * 1.5f : 0;
    }
}
