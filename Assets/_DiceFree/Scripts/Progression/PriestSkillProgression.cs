using System;
using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Skills
{
    public enum PriestSkill { HolyLight, Grace }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(ExperienceProgression), typeof(ActorResourceController))]
    public sealed class PriestSkillProgression : MonoBehaviour, IClassSkillState
    {
        public const string PriestClassId = "class.priest";
        public const string GraceResourceId = "resource.grace";
        private const string GraceLimitSource = "skill.priest.grace";
        private ExperienceProgression experience;
        private ActorResourceController resources;
        private int holyLightRank;
        private int graceRank;
        private bool active;

        public string ClassId => PriestClassId;
        public int UnspentPoints => active ? Mathf.Max(0, experience.Level - holyLightRank - graceRank) : 0;
        public event Action Changed;

        private void Awake()
        {
            experience = GetComponent<ExperienceProgression>();
            resources = GetComponent<ActorResourceController>();
        }

        private void OnEnable() => experience.Changed += OnLevelChanged;
        private void OnDisable() => experience.Changed -= OnLevelChanged;
        private void OnLevelChanged() => Changed?.Invoke();

        public static int MaxRank(PriestSkill skill) => skill == PriestSkill.Grace ? 5 : 18;
        public static int RequiredLevel(PriestSkill skill, int nextRank) => skill == PriestSkill.Grace ? nextRank * 5 : 1;
        public int Rank(PriestSkill skill) => active ? (skill == PriestSkill.Grace ? graceRank : holyLightRank) : 0;
        public bool CanSpend(PriestSkill skill)
        {
            int rank = Rank(skill);
            return UnspentPoints > 0 && rank < MaxRank(skill) && experience.Level >= RequiredLevel(skill, rank + 1);
        }

        public bool Spend(PriestSkill skill)
        {
            if (!CanSpend(skill)) return false;
            if (skill == PriestSkill.Grace) graceRank++;
            else holyLightRank++;
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
                if (value.stableId == "skill.priest.holy-light")
                {
                    if (value.rank > MaxRank(PriestSkill.HolyLight)) return false;
                    spent += value.rank;
                }
                else if (value.stableId == GraceLimitSource)
                {
                    if (value.rank > MaxRank(PriestSkill.Grace) || level < RequiredLevel(PriestSkill.Grace, value.rank)) return false;
                    spent += value.rank;
                }
            }
            return level > 0 && spent <= level;
        }

        public void RestoreState(SkillRankState[] values)
        {
            if (!CanRestore(values, experience.Level)) throw new InvalidOperationException("Invalid Priest skill ranks.");
            holyLightRank = graceRank = 0;
            foreach (var value in values ?? Array.Empty<SkillRankState>())
            {
                if (value.stableId == "skill.priest.holy-light") holyLightRank = value.rank;
                else if (value.stableId == GraceLimitSource) graceRank = value.rank;
            }
            Refresh();
        }

        public SkillRankState[] CaptureState() => active
            ? new[] { new SkillRankState("skill.priest.holy-light", holyLightRank), new SkillRankState(GraceLimitSource, graceRank) }
            : Array.Empty<SkillRankState>();

        public void SetClassActive(bool value)
        {
            if (active == value) return;
            active = value;
            holyLightRank = graceRank = 0;
            Refresh();
        }

        private void Refresh()
        {
            float maximum = active && graceRank > 0 ? 11 - graceRank : 0;
            resources.SetMaximumFlatModifier(GraceResourceId, GraceLimitSource, maximum);
            Changed?.Invoke();
        }
    }
}
