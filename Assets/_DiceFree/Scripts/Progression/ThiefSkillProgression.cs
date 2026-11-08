using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Skills
{
    public enum ThiefSkill { Stealth, Envenom, KeenEdge, FanOfKnives, SmokeBomb, Cutpurse }

    [DisallowMultipleComponent, RequireComponent(typeof(ExperienceProgression))]
    public sealed class ThiefSkillProgression : MonoBehaviour, IClassSkillState
    {
        public const string ThiefClassId = "class.thief";
        private static readonly string[] Ids = {
            "skill.thief.stealth", "skill.thief.envenom", "skill.thief.keen-edge",
            "skill.thief.fan-of-knives", "skill.thief.smoke-bomb", "skill.thief.cutpurse"
        };
        private static readonly string[] Names = {
            "Stealth", "Envenom", "Keen Edge", "Fan of Knives", "Smoke Bomb", "Cutpurse"
        };
        private static readonly int[] Caps = { 16, 18, 10, 18, 10, 8 };
        private static readonly int[] FirstGates = { 1, 1, 5, 1, 6, 1 };
        private static readonly int[] RankIntervals = { 5, 4, 8, 4, 7, 10 };
        public const int SkillCount = 6;
        private readonly int[] ranks = new int[SkillCount];
        private ExperienceProgression experience;

        public string ClassId => ThiefClassId;
        public bool Active { get; private set; }
        public int UnspentPoints => Active ? Mathf.Max(0, Mathf.Min(experience.Level, 80) - ranks.Sum()) : 0;
        public event Action Changed;

        private void Awake() => experience = GetComponent<ExperienceProgression>();
        private void OnEnable() => experience.Changed += Notify;
        private void OnDisable() => experience.Changed -= Notify;
        private void Notify() => Changed?.Invoke();

        private static bool Known(ThiefSkill skill) => (uint)skill < SkillCount;
        public static string Name(ThiefSkill skill) => Known(skill) ? Names[(int)skill] : "";
        public static int MaxRank(ThiefSkill skill) => Known(skill) ? Caps[(int)skill] : 0;
        public static int RequiredLevel(ThiefSkill skill, int rank) =>
            Known(skill) ? FirstGates[(int)skill] + RankIntervals[(int)skill] * (rank - 1) : int.MaxValue;
        public int Rank(ThiefSkill skill) => Active && Known(skill) ? ranks[(int)skill] : 0;

        public bool CanSpend(ThiefSkill skill)
        {
            int rank = Rank(skill);
            return Known(skill) && UnspentPoints > 0 && rank < MaxRank(skill) &&
                experience.Level >= RequiredLevel(skill, rank + 1);
        }

        public bool Spend(ThiefSkill skill)
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
                var skill = (ThiefSkill)index;
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
            if (!CanRestore(values, experience.Level)) throw new InvalidOperationException("Invalid Thief skill ranks.");
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
