using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Skills
{
    public enum PriestSkill { HolyLight, Grace, LightBurst, IceAegis, WildGrowth, Revive }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(ExperienceProgression), typeof(ActorResourceController))]
    public sealed class PriestSkillProgression : MonoBehaviour, IClassSkillState
    {
        public const string PriestClassId = "class.priest";
        public const string GraceResourceId = "resource.grace";
        private const string GraceLimitSource = "skill.priest.grace";
        private static readonly string[] Ids = {
            "skill.priest.holy-light", GraceLimitSource, "skill.priest.light-burst",
            "skill.priest.ice-aegis", "skill.priest.wild-growth", "skill.priest.revive"
        };
        public static int SkillCount => Ids.Length;
        private ExperienceProgression experience;
        private ActorResourceController resources;
        private readonly int[] ranks = new int[Ids.Length];
        private bool active;

        public string ClassId => PriestClassId;
        public bool Active => active;
        public int UnspentPoints => active ? Mathf.Max(0, experience.Level - ranks.Sum()) : 0;
        public event Action Changed;

        private void Awake()
        {
            experience = GetComponent<ExperienceProgression>();
            resources = GetComponent<ActorResourceController>();
        }

        private void OnEnable() => experience.Changed += OnLevelChanged;
        private void OnDisable() => experience.Changed -= OnLevelChanged;
        private void OnLevelChanged() => Changed?.Invoke();

        private static bool Known(PriestSkill skill) => (uint)skill < (uint)Ids.Length;
        public static int MaxRank(PriestSkill skill) => !Known(skill) ? 0 : skill switch
        {
            PriestSkill.Grace => 5,
            PriestSkill.Revive => 1,
            _ => 18
        };
        public static int RequiredLevel(PriestSkill skill, int nextRank) => skill switch
        {
            PriestSkill.Grace => nextRank * 5,
            PriestSkill.Revive => 20,
            _ => 1
        };
        public int Rank(PriestSkill skill) => active && Known(skill) ? ranks[(int)skill] : 0;
        public bool CanSpend(PriestSkill skill)
        {
            int rank = Rank(skill);
            return UnspentPoints > 0 && rank < MaxRank(skill) && experience.Level >= RequiredLevel(skill, rank + 1);
        }

        public bool Spend(PriestSkill skill)
        {
            if (!CanSpend(skill)) return false;
            ranks[(int)skill]++;
            Refresh();
            return true;
        }

        public bool CanRestore(SkillRankState[] values, int level)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            int spent = 0;
            foreach (var value in values ?? Array.Empty<SkillRankState>())
            {
                if (string.IsNullOrWhiteSpace(value.stableId) || value.rank < 0 || !seen.Add(value.stableId)) return false;
                int index = Array.IndexOf(Ids, value.stableId);
                if (index < 0) continue;
                var skill = (PriestSkill)index;
                if (value.rank > MaxRank(skill) || value.rank > 0 && level < RequiredLevel(skill, value.rank)) return false;
                spent += value.rank;
            }
            return level > 0 && spent <= level;
        }

        public void RestoreState(SkillRankState[] values)
        {
            if (!CanRestore(values, experience.Level)) throw new InvalidOperationException("Invalid Priest skill ranks.");
            Array.Clear(ranks, 0, ranks.Length);
            foreach (var value in values ?? Array.Empty<SkillRankState>())
            {
                int index = Array.IndexOf(Ids, value.stableId);
                if (index >= 0) ranks[index] = value.rank;
            }
            Refresh();
        }

        public SkillRankState[] CaptureState() => active
            ? Ids.Select((id, index) => new SkillRankState(id, ranks[index])).ToArray()
            : Array.Empty<SkillRankState>();

        public void SetClassActive(bool value)
        {
            if (active == value) return;
            active = value;
            Array.Clear(ranks, 0, ranks.Length);
            Refresh();
        }

        private void Refresh()
        {
            int graceRank = Rank(PriestSkill.Grace);
            float maximum = graceRank > 0 ? 11 - graceRank : 0;
            resources.SetMaximumFlatModifier(GraceResourceId, GraceLimitSource, maximum);
            Changed?.Invoke();
        }
    }
}
