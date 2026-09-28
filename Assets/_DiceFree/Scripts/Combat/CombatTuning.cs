using UnityEngine;

namespace DiceFree.Combat
{
    [CreateAssetMenu(menuName = "DiceFree/Combat/Prototype tuning")]
    public sealed class CombatTuning : ScriptableObject
    {
        [Header("Provisional coefficients - see docs/CORNBERG_COMBAT.md")]
        [Min(0)] public float hpPerVitality = 5;
        [Min(0)] public float regenerationPerVitality = 0.05f;
        [Min(0)] public float defensePerAttribute = 1;
        [Min(1)] public float defenseConstant = 100;
        [Min(0)] public float attackSpeedPerAgility = 0.005f;
        [Min(0)] public float moveSpeedPerAgility = 0.001f;
        [Min(0)] public float healingReceivedPerSpirit = 0.01f;
        public float defaultElementResistance = -0.1f;
        [Range(0, 1)] public float resistanceCap = 0.75f;
    }
}
