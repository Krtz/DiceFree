using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Skills
{
    public enum PsionSkill { PsychicBolt, PsionicBladesMastery, MentalImage, LightningStorm, MetalShield }

    [DisallowMultipleComponent, RequireComponent(typeof(ExperienceProgression))]
    public sealed class PsionSkillProgression : MonoBehaviour, IClassSkillState
    {
        public const string PsionClassId = "class.psion";
        private static readonly string[] Ids = {
            "skill.psion.psychic-bolt", "skill.psion.psionic-blades", "skill.psion.mental-image",
            "skill.psion.lightning-storm", "skill.psion.metal-shield"
        };
        private static readonly string[] Names = {
            "Psychic Bolt", "Psionic Blades Mastery", "Mental Image", "Lightning Storm", "Metal Shield"
        };
        private static readonly int[] Caps = { 20, 18, 18, 12, 12 };
        private static readonly int[] FirstGates = { 1, 1, 5, 8, 10 };
        private static readonly int[] RankIntervals = { 4, 4, 4, 6, 6 };
        public const int SkillCount = 5;
        private readonly int[] ranks = new int[SkillCount];
        private ExperienceProgression experience;

        public string ClassId => PsionClassId;
        public bool Active { get; private set; }
        public int UnspentPoints => Active ? Mathf.Max(0, Mathf.Min(experience.Level, 80) - ranks.Sum()) : 0;
        public event Action Changed;

        private void Awake() => experience = GetComponent<ExperienceProgression>();
        private void OnEnable() => experience.Changed += Notify;
        private void OnDisable() => experience.Changed -= Notify;
        private void Notify() => Changed?.Invoke();

        private static bool Known(PsionSkill skill) => (uint)skill < SkillCount;
        public static string Name(PsionSkill skill) => Known(skill) ? Names[(int)skill] : "";
        public static int MaxRank(PsionSkill skill) => Known(skill) ? Caps[(int)skill] : 0;
        public static int RequiredLevel(PsionSkill skill, int rank) =>
            !Known(skill) || rank < 1 ? int.MaxValue : (skill, rank) switch
            {
                (PsionSkill.PsychicBolt, >1) => 3 + 4 * (rank - 2),
                (PsionSkill.PsionicBladesMastery, >1) => 2 + 4 * (rank - 2),
                _ => FirstGates[(int)skill] + RankIntervals[(int)skill] * (rank - 1)
            };
        public int Rank(PsionSkill skill) => Active && Known(skill) ? ranks[(int)skill] : 0;

        public bool CanSpend(PsionSkill skill)
        {
            int rank = Rank(skill);
            return Known(skill) && UnspentPoints > 0 && rank < MaxRank(skill) &&
                experience.Level >= RequiredLevel(skill, rank + 1);
        }

        public bool Spend(PsionSkill skill)
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
                var skill = (PsionSkill)index;
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
            if (!CanRestore(values, experience.Level)) throw new InvalidOperationException("Invalid Psion skill ranks.");
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
