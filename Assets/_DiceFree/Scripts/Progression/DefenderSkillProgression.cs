using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Skills
{
    public enum DefenderSkill { ShieldBash, Challenge, Bulwark, ShieldSweep, HoldTheLine, IronGuard }

    [DisallowMultipleComponent, RequireComponent(typeof(ExperienceProgression))]
    public sealed class DefenderSkillProgression : MonoBehaviour, IClassSkillState
    {
        public const string DefenderClassId = "class.defender";
        private static readonly string[] Ids = {
            "skill.defender.shield-bash", "skill.defender.challenge", "skill.defender.bulwark",
            "skill.defender.shield-sweep", "skill.defender.hold-the-line", "skill.defender.iron-guard"
        };
        private static readonly string[] Names = {
            "Shield Bash", "Challenge", "Bulwark", "Shield Sweep", "Hold the Line", "Iron Guard"
        };
        private static readonly int[] Caps = { 18, 10, 16, 20, 10, 6 };
        private static readonly int[] FirstGates = { 1, 1, 6, 1, 1, 12 };
        private static readonly int[] RankIntervals = { 1, 8, 4, 4, 8, 13 };
        public const int SkillCount = 6;
        private readonly int[] ranks = new int[SkillCount];
        private ExperienceProgression experience;

        public string ClassId => DefenderClassId;
        public bool Active { get; private set; }
        public int UnspentPoints => Active ? Mathf.Max(0, Mathf.Min(experience.Level, 80) - ranks.Sum()) : 0;
        public event Action Changed;

        private void Awake() => experience = GetComponent<ExperienceProgression>();
        private void OnEnable() => experience.Changed += Notify;
        private void OnDisable() => experience.Changed -= Notify;
        private void Notify() => Changed?.Invoke();

        private static bool Known(DefenderSkill skill) => (uint)skill < SkillCount;
        public static string Name(DefenderSkill skill) => Known(skill) ? Names[(int)skill] : "";
        public static int MaxRank(DefenderSkill skill) => Known(skill) ? Caps[(int)skill] : 0;
        public static int RequiredLevel(DefenderSkill skill, int rank) =>
            Known(skill) ? FirstGates[(int)skill] + RankIntervals[(int)skill] * (rank - 1) : int.MaxValue;
        public int Rank(DefenderSkill skill) => Active && Known(skill) ? ranks[(int)skill] : 0;

        public bool CanSpend(DefenderSkill skill)
        {
            int rank = Rank(skill);
            return Known(skill) && UnspentPoints > 0 && rank < MaxRank(skill) &&
                experience.Level >= RequiredLevel(skill, rank + 1);
        }

        public bool Spend(DefenderSkill skill)
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
                var skill = (DefenderSkill)index;
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
            if (!CanRestore(values, experience.Level)) throw new InvalidOperationException("Invalid Defender skill ranks.");
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
