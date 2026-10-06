using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Skills
{
    public enum NoviceSkillKind
    {
        StrengthMeleeStun,
        MagicSand,
        AgilityAttackSpeedBuff,
        SpiritHeal,
        AllStatPassive
    }

    [Serializable]
    public struct SkillRankState
    {
        public string stableId;
        public int rank;

        public SkillRankState(string id, int value)
        {
            stableId = id;
            rank = value;
        }
    }

    [CreateAssetMenu(menuName = "DiceFree/Skills/Novice skill")]
    public sealed class NoviceSkillDefinition : ScriptableObject
    {
        public string stableId;
        public string displayName;
        public NoviceSkillKind kind;
        [Min(1)] public int maxRank = 10;
        [Min(0)] public float cooldownSeconds = 10;
        [Min(0)] public float range = 1;
        [Min(0)] public float durationSeconds = 5;
        public AttackDefinition attack;

        public bool Active => kind != NoviceSkillKind.AllStatPassive;

        public int ClampRank(int rank) => Mathf.Clamp(rank, 0, Mathf.Max(1, maxRank));
        public float StunDuration(int rank) => kind == NoviceSkillKind.StrengthMeleeStun ? ClampRank(rank) * 0.2f : 0;
        public float MagicSandMissChance(int rank) => kind == NoviceSkillKind.MagicSand ? ClampRank(rank) * 0.075f : 0;
        public float AttackSpeedBonusPercent(AttributeValues attributes, int rank) =>
            kind == NoviceSkillKind.AgilityAttackSpeedBuff && rank > 0
                ? 10f + ClampRank(rank) * 0.2f * attributes.agility
                : 0;
        public float HealAmount(AttributeValues attributes, int rank) =>
            kind == NoviceSkillKind.SpiritHeal && rank > 0 ? attributes.spirit * 10f : 0;
    }
}
