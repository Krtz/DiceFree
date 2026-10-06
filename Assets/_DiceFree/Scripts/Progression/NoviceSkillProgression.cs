using System;
using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent, RequireComponent(typeof(ExperienceProgression), typeof(ActorStats))]
    public sealed class NoviceSkillProgression : MonoBehaviour, IClassSkillState
    {
        public const string ClassStableId = "class.novice";
        public const string PassiveStableId = "skill.novice.all-stat-passive";
        private const string PassiveAttributeSource = "class-skill:" + PassiveStableId;

        [SerializeField] private NoviceSkillDefinition[] definitions = Array.Empty<NoviceSkillDefinition>();

        private readonly SortedDictionary<string, int> ranks = new(StringComparer.Ordinal);
        private ExperienceProgression xp;
        private ActorStats stats;
        private bool classActive;

        public string ClassId => ClassStableId;
        public bool ActiveForCurrentClass => classActive;
        public event Action Changed;
        public event Action SkillPointAvailable;

        public IReadOnlyList<NoviceSkillDefinition> Definitions => definitions;
        public int TotalPoints => !classActive || xp == null ? 0 : xp.Level;
        public int SpentPoints
        {
            get
            {
                int total = 0;
                foreach (int rank in ranks.Values) total += Mathf.Max(0, rank);
                return total;
            }
        }
        public int UnspentPoints => Mathf.Max(0, TotalPoints - SpentPoints);

        private void Awake()
        {
            xp = GetComponent<ExperienceProgression>();
            stats = GetComponent<ActorStats>();
            classActive = stats.Definition != null && stats.Definition.stableId == ClassStableId;
            ValidateDefinitions();
            ApplyPassive();
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

        public void Configure(NoviceSkillDefinition[] values)
        {
            definitions = values == null ? Array.Empty<NoviceSkillDefinition>() : (NoviceSkillDefinition[])values.Clone();
            if (Application.isPlaying)
            {
                ValidateDefinitions();
                ApplyPassive();
                Changed?.Invoke();
            }
        }

        public int Rank(string stableId) => !string.IsNullOrEmpty(stableId) && ranks.TryGetValue(stableId, out int rank) ? rank : 0;

        public NoviceSkillDefinition Definition(string stableId)
        {
            if (string.IsNullOrEmpty(stableId)) return null;
            foreach (var definition in definitions)
                if (definition != null && definition.stableId == stableId) return definition;
            return null;
        }

        public NoviceSkillDefinition ActiveAtSlot(int slot)
        {
            if (slot < 0) return null;
            int current = 0;
            foreach (var definition in definitions)
            {
                if (definition == null || !definition.Active) continue;
                if (current == slot) return definition;
                current++;
            }
            return null;
        }

        public bool Spend(string stableId)
        {
            var definition = Definition(stableId);
            if (!classActive || definition == null || UnspentPoints <= 0) return false;
            int current = Rank(stableId);
            if (current >= definition.maxRank) return false;
            ranks[stableId] = current + 1;
            ApplyPassive();
            Changed?.Invoke();
            return true;
        }

        public void ResetAllocatedRanks()
        {
            var unresolved = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (var pair in ranks)
                if (Definition(pair.Key) == null) unresolved[pair.Key] = pair.Value;
            ranks.Clear();
            foreach (var pair in unresolved) ranks[pair.Key] = pair.Value;
            ApplyPassive();
            Changed?.Invoke();
        }

        public bool CanRestore(SkillRankState[] state, int level)
        {
            if (level < 1) return false;
            if (state == null) return true;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            long spent = 0;
            foreach (var entry in state)
            {
                if (string.IsNullOrWhiteSpace(entry.stableId) || entry.rank < 0 || !seen.Add(entry.stableId)) return false;
                var definition = Definition(entry.stableId);
                if (definition != null && entry.rank > definition.maxRank) return false;
                spent += entry.rank;
                if (spent > level) return false;
            }
            return true;
        }

        public void RestoreState(SkillRankState[] state)
        {
            if (!CanRestore(state, xp.Level)) throw new ArgumentException("Invalid saved Novice skill allocation.");
            ranks.Clear();
            if (state != null)
                foreach (var entry in state)
                    if (entry.rank > 0) ranks[entry.stableId] = entry.rank;
            ApplyPassive();
            Changed?.Invoke();
        }

        public SkillRankState[] CaptureState()
        {
            if (!classActive) return Array.Empty<SkillRankState>();
            var result = new SkillRankState[ranks.Count];
            int index = 0;
            foreach (var pair in ranks) result[index++] = new SkillRankState(pair.Key, pair.Value);
            return result;
        }

        private void OnLevel(int _)
        {
            if (!classActive) return;
            Changed?.Invoke();
            SkillPointAvailable?.Invoke();
        }

        public void SetClassActive(bool active)
        {
            if (classActive == active) return;
            classActive = active;
            if (!active) ranks.Clear();
            ApplyPassive();
            Changed?.Invoke();
        }

        private void ApplyPassive()
        {
            if (stats == null) return;
            if (!classActive)
            {
                stats.RemoveAttributeContribution(PassiveAttributeSource);
                return;
            }
            int rank = Rank(PassiveStableId);
            if (rank <= 0) stats.RemoveAttributeContribution(PassiveAttributeSource);
            else stats.SetAttributeContribution(PassiveAttributeSource, new AttributeValues(rank));
        }

        private void ValidateDefinitions()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in definitions)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.stableId))
                    throw new InvalidOperationException("Novice skill definitions require stable IDs.");
                if (!ids.Add(definition.stableId))
                    throw new InvalidOperationException("Duplicate Novice skill definition: " + definition.stableId);
                if (definition.maxRank < 1) throw new InvalidOperationException("Novice skill rank cap must be positive.");
            }
        }
    }
}
