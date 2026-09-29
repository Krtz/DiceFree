using UnityEngine;

namespace DiceFree.Combat
{
    [CreateAssetMenu(menuName = "DiceFree/Combat/Prototype tuning")]
    public sealed class CombatTuning : ScriptableObject
    {
        [Header("Provisional coefficients - see docs/CORNBERG_COMBAT.md")]
        [Min(0)] public float hpPerVitality = 15;
        [Min(0)] public float regenerationPerVitality = 0.1f;
        [Min(0)] public float defensePerAttribute = 0.2f;
        [Min(1)] public float defenseConstant = 300;
        [Min(0.01f)] public float defenseExponent = 0.7f;
        [Min(0)] public float attackSpeedPerAgility = 0.00025f;
        [Min(0)] public float moveSpeedPerAgility = 0.0001f;
        [Min(0)] public float healingDonePerSpirit = 0.0015f;
        [Min(0)] public float healingReceivedPerSpirit = 0.00075f;
        public float defaultElementResistance = -0.1f;
        [Range(0, 1)] public float resistanceCap = 0.75f;
        public float Coefficient(SecondaryStat stat) => stat switch {
            SecondaryStat.PhysicalDefense or SecondaryStat.MagicalDefense => defensePerAttribute,
            SecondaryStat.AttackSpeed => attackSpeedPerAgility,
            SecondaryStat.MoveSpeed => moveSpeedPerAgility,
            SecondaryStat.HealingDone => healingDonePerSpirit,
            SecondaryStat.HealingReceived => healingReceivedPerSpirit,
            _ => throw new System.ArgumentOutOfRangeException(nameof(stat))
        };
    }
}
