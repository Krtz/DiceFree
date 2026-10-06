using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent, RequireComponent(typeof(ExperienceProgression), typeof(ActorStats))]
    public sealed class PhysicalSkillProgression : MonoBehaviour, IClassSkillState
    {
        public const string ClassStableId = "class.physically-blessed-novice";
        private const string MartialBasicSource = "skill.physical.martial-aptitude.basic";
        private const string MartialDefenseSource = "skill.physical.martial-aptitude.defense";

        [SerializeField] private PhysicalSkillDefinition[] definitions = Array.Empty<PhysicalSkillDefinition>();
        private readonly Dictionary<string, int> ranks = new(StringComparer.Ordinal);
        private ExperienceProgression xp;
        private ActorStats stats;
        private bool classActive;

        public string ClassId => ClassStableId;
        public bool ActiveForCurrentClass => classActive;
        public int TotalPoints => !classActive || xp == null ? 0 : xp.Level;
        public int SpentPoints => ranks.Values.Sum();
        public int UnspentPoints => Mathf.Max(0, TotalPoints - SpentPoints);
        public IReadOnlyList<PhysicalSkillDefinition> Definitions => definitions;
        public event Action Changed;
        public event Action SkillPointAvailable;

        private void Awake()
        {
            xp = GetComponent<ExperienceProgression>();
            stats = GetComponent<ActorStats>();
            classActive = stats.Definition != null && stats.Definition.stableId == ClassStableId;
            ValidateDefinitions();
        }

        private void OnEnable()
        {
            if (xp == null) xp = GetComponent<ExperienceProgression>();
            xp.LeveledUp += OnLevel;
        }
        private void OnDisable()
        {
            if (xp != null) xp.LeveledUp -= OnLevel;
        }

        public void Configure(params PhysicalSkillDefinition[] values)
        {
            definitions = values == null ? Array.Empty<PhysicalSkillDefinition>() : (PhysicalSkillDefinition[])values.Clone();
            ValidateDefinitions();
        }

        public int Rank(string stableId) => stableId != null && ranks.TryGetValue(stableId, out int rank) ? rank : 0;

        public PhysicalSkillDefinition ActiveAtSlot(int slot) =>
            definitions.Where(value => value != null && value.Active).ElementAtOrDefault(slot);

        public bool Spend(string stableId) => Spend(definitions.FirstOrDefault(value => value != null && value.stableId == stableId));

        public bool Spend(PhysicalSkillDefinition definition)
        {
            if (!classActive || definition == null || UnspentPoints <= 0) return false;
            int rank = Rank(definition.stableId);
            if (rank >= definition.maxRank) return false;
            ranks[definition.stableId] = rank + 1;
            ApplyPassive();
            Changed?.Invoke();
            return true;
        }

        public bool CanRestore(SkillRankState[] state, int level)
        {
            state ??= Array.Empty<SkillRankState>();
            var known = definitions.ToDictionary(value => value.stableId, StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            int total = 0;
            foreach (var entry in state)
            {
                if (string.IsNullOrWhiteSpace(entry.stableId) || entry.rank < 0 || !seen.Add(entry.stableId))
                    return false;
                if (!known.TryGetValue(entry.stableId, out var definition)) continue;
                if (entry.rank > definition.maxRank) return false;
                total += entry.rank;
            }
            return total <= Mathf.Max(1, level);
        }

        public void RestoreState(SkillRankState[] state)
        {
            state ??= Array.Empty<SkillRankState>();
            if (!CanRestore(state, xp == null ? 1 : xp.Level))
                throw new InvalidOperationException("Invalid Physically Blessed skill state.");
            ranks.Clear();
            var known = definitions.ToDictionary(value => value.stableId, StringComparer.Ordinal);
            foreach (var entry in state)
                if (known.TryGetValue(entry.stableId, out var definition) && entry.rank > 0)
                    ranks[entry.stableId] = definition.ClampRank(entry.rank);
            ApplyPassive();
            Changed?.Invoke();
        }

        public SkillRankState[] CaptureState()
        {
            if (!classActive) return Array.Empty<SkillRankState>();
            return ranks.OrderBy(value => value.Key, StringComparer.Ordinal)
                .Select(value => new SkillRankState(value.Key, value.Value))
                .ToArray();
        }

        public void SetClassActive(bool active)
        {
            if (classActive == active) return;
            classActive = active;
            if (!active) ranks.Clear();
            ApplyPassive();
            Changed?.Invoke();
        }

        private void OnLevel(int _)
        {
            if (!classActive) return;
            Changed?.Invoke();
            SkillPointAvailable?.Invoke();
        }

        private void ApplyPassive()
        {
            if (stats == null) return;
            var passive = definitions.FirstOrDefault(value => value != null && value.kind == PhysicalSkillKind.MartialAptitude);
            int rank = classActive && passive != null ? Rank(passive.stableId) : 0;
            if (rank <= 0 || passive == null)
            {
                stats.RemoveBasicAttackDamagePercentContribution(MartialBasicSource);
                stats.RemovePhysicalDefenseContribution(MartialDefenseSource);
                return;
            }
            stats.SetBasicAttackDamagePercentContribution(MartialBasicSource, passive.MartialBasicAttackPercent(rank));
            stats.SetPhysicalDefenseContribution(MartialDefenseSource, passive.MartialPhysicalDefense(rank));
        }

        private void ValidateDefinitions()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            int activeCount = 0, passiveCount = 0;
            foreach (var definition in definitions)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.stableId) || definition.maxRank != 6)
                    throw new InvalidOperationException("Physical skills require stable IDs and exactly six ranks.");
                if (!ids.Add(definition.stableId))
                    throw new InvalidOperationException("Duplicate Physical skill ID: " + definition.stableId);
                if (definition.Active) activeCount++; else passiveCount++;
            }
            if (definitions.Length > 0 && (activeCount != 4 || passiveCount != 1))
                throw new InvalidOperationException("Physically Blessed requires four actives and one passive.");
        }
    }
}
