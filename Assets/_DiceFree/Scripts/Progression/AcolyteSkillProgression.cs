using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Skills
{
    public enum AcolyteSkill { DrainLife, MortalCoil, WitheringCurse, RadiationPlague, SummonSkeleton, OccultAdept }

    [DisallowMultipleComponent, RequireComponent(typeof(ExperienceProgression))]
    public sealed class AcolyteSkillProgression : MonoBehaviour, IClassSkillState
    {
        public const string AcolyteClassId = "class.acolyte";
        public const string SoulResourceId = "resource.soul-charge";
        private static readonly string[] Ids = {
            "skill.acolyte.drain-life", "skill.acolyte.mortal-coil", "skill.acolyte.withering-curse",
            "skill.acolyte.radiation-plague", "skill.acolyte.summon-skeleton", "skill.acolyte.occult-adept"
        };
        private static readonly string[] Names = {
            "Drain Life", "Mortal Coil", "Withering Curse", "Radiation Plague", "Summon Skeleton", "Occult Adept"
        };
        private static readonly int[] Caps = { 20, 10, 14, 16, 12, 8 };
        private static readonly int[] RankIntervals = { 4, 7, 5, 5, 6, 10 };
        public const int SkillCount = 6;
        private readonly int[] ranks = new int[SkillCount];
        private ExperienceProgression experience;

        public string ClassId => AcolyteClassId;
        public bool Active { get; private set; }
        public int UnspentPoints => Active ? Mathf.Max(0, Mathf.Min(experience.Level, 80) - ranks.Sum()) : 0;
        public event Action Changed;

        private void Awake() => experience = GetComponent<ExperienceProgression>();
        private void OnEnable() => experience.Changed += Notify;
        private void OnDisable() => experience.Changed -= Notify;
        private void Notify() => Changed?.Invoke();

        private static bool Known(AcolyteSkill skill) => (uint)skill < SkillCount;
        public static string Name(AcolyteSkill skill) => Known(skill) ? Names[(int)skill] : "";
        public static int MaxRank(AcolyteSkill skill) => Known(skill) ? Caps[(int)skill] : 0;
        public static int RequiredLevel(AcolyteSkill skill, int rank) =>
            Known(skill) ? (skill == AcolyteSkill.MortalCoil ? 6 : 1) + RankIntervals[(int)skill] * (rank - 1) : int.MaxValue;
        public int Rank(AcolyteSkill skill) => Active && Known(skill) ? ranks[(int)skill] : 0;

        public bool CanSpend(AcolyteSkill skill)
        {
            int rank = Rank(skill);
            return Known(skill) && UnspentPoints > 0 && rank < MaxRank(skill) &&
                experience.Level >= RequiredLevel(skill, rank + 1);
        }

        public bool Spend(AcolyteSkill skill)
        {
            if (!CanSpend(skill)) return false;
            ranks[(int)skill]++;
            Notify();
            return true;
        }

        public bool CanRestore(SkillRankState[] values, int level)
        {
            if (level < 1) return false;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            int spent = 0;
            foreach (var value in values ?? Array.Empty<SkillRankState>())
            {
                if (string.IsNullOrWhiteSpace(value.stableId) || value.rank < 0 || !seen.Add(value.stableId)) return false;
                int index = Array.IndexOf(Ids, value.stableId);
                if (index < 0) continue;
                var skill = (AcolyteSkill)index;
                if (value.rank > MaxRank(skill) || (value.rank > 0 && level < RequiredLevel(skill, value.rank))) return false;
                spent += value.rank;
            }
            return spent <= Mathf.Min(level, 80);
        }

        public SkillRankState[] CaptureState() => Active
            ? Ids.Select((id, index) => new SkillRankState(id, ranks[index])).ToArray()
            : Array.Empty<SkillRankState>();

        public void RestoreState(SkillRankState[] values)
        {
            if (!CanRestore(values, experience.Level)) throw new InvalidOperationException("Invalid Acolyte skill ranks.");
            Array.Clear(ranks, 0, ranks.Length);
            foreach (var value in values ?? Array.Empty<SkillRankState>())
            {
                int index = Array.IndexOf(Ids, value.stableId);
                if (index >= 0) ranks[index] = value.rank;
            }
            Notify();
        }

        public void SetClassActive(bool value)
        {
            if (Active == value) return;
            Active = value;
            Array.Clear(ranks, 0, ranks.Length);
            Notify();
        }
    }
}
