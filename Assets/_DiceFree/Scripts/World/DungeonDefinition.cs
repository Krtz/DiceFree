using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceFree.World
{
    public enum DungeonConditionKind
    {
        AveragePartyLevelAtLeast = 0,
        PartyContainsClass = 1,
        SessionFlag = 2,
        EchoFlag = 3,
        QuestFlag = 4,
        ItemPresent = 5,
        Achievement = 6
    }

    [Serializable]
    public sealed class DungeonConditionSpec
    {
        public DungeonConditionKind kind;
        public float threshold;
        public string value;
    }

    [Serializable]
    public sealed class DungeonVariantRule
    {
        public string stableId;
        public int priority;
        public DungeonConditionSpec[] allConditions = Array.Empty<DungeonConditionSpec>();
        public string[] enableRouteIds = Array.Empty<string>();
        public string[] enableBossIds = Array.Empty<string>();
        public string[] lootTableIds = Array.Empty<string>();
        public string[] variantTags = Array.Empty<string>();
    }

    [Serializable]
    public sealed class DungeonParticipantSnapshot
    {
        public string participantId;
        public int level;
        public string classId;
    }

    [Serializable]
    public sealed class DungeonRunVariantSnapshot
    {
        public string dungeonId;
        public float averagePartyLevel;
        public string[] matchedRuleIds = Array.Empty<string>();
        public string[] enabledRouteIds = Array.Empty<string>();
        public string[] enabledBossIds = Array.Empty<string>();
        public string[] lootTableIds = Array.Empty<string>();
        public string[] variantTags = Array.Empty<string>();

        public bool HasRoute(string id) => enabledRouteIds?.Contains(id) == true;
        public bool HasBoss(string id) => enabledBossIds?.Contains(id) == true;
        public bool HasTag(string id) => variantTags?.Contains(id) == true;
    }

    public sealed class DungeonConditionContext
    {
        public IReadOnlyList<DungeonParticipantSnapshot> participants = Array.Empty<DungeonParticipantSnapshot>();
        public ISet<string> sessionFlags = new HashSet<string>(StringComparer.Ordinal);
        public ISet<string> echoFlags = new HashSet<string>(StringComparer.Ordinal);
        public ISet<string> questFlags = new HashSet<string>(StringComparer.Ordinal);
        public ISet<string> itemIds = new HashSet<string>(StringComparer.Ordinal);
        public ISet<string> achievementIds = new HashSet<string>(StringComparer.Ordinal);

        public float AveragePartyLevel =>
            participants == null || participants.Count == 0
                ? 0f
                : (float)participants.Average(value => value?.level ?? 0);
    }

    [CreateAssetMenu(menuName = "DiceFree/World/Dungeon Definition")]
    public sealed class DungeonDefinition : ScriptableObject
    {
        public string stableId;
        public string sceneId;
        [Min(0f)] public float stagingSeconds = 60f;
        public DungeonVariantRule[] variants = Array.Empty<DungeonVariantRule>();

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(stableId))
                throw new InvalidOperationException(name + " has no stable dungeon ID.");
            if (string.IsNullOrWhiteSpace(sceneId))
                throw new InvalidOperationException(stableId + " has no scene ID.");
            if (stagingSeconds < 0f)
                throw new InvalidOperationException(stableId + " has negative staging time.");

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var rule in variants ?? Array.Empty<DungeonVariantRule>())
            {
                if (rule == null || string.IsNullOrWhiteSpace(rule.stableId))
                    throw new InvalidOperationException(stableId + " has an invalid variant rule.");
                if (!ids.Add(rule.stableId))
                    throw new InvalidOperationException(stableId + " repeats variant rule " + rule.stableId + ".");
            }
        }
    }

    public static class DungeonConditionEvaluator
    {
        public static DungeonRunVariantSnapshot Evaluate(
            DungeonDefinition definition,
            DungeonConditionContext context)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (context == null) throw new ArgumentNullException(nameof(context));
            definition.Validate();

            var matched = new List<DungeonVariantRule>();
            foreach (var rule in definition.variants ?? Array.Empty<DungeonVariantRule>())
                if (rule != null && Matches(rule, context))
                    matched.Add(rule);

            matched.Sort((a, b) =>
            {
                int priority = b.priority.CompareTo(a.priority);
                return priority != 0
                    ? priority
                    : string.CompareOrdinal(a.stableId, b.stableId);
            });

            return new DungeonRunVariantSnapshot
            {
                dungeonId = definition.stableId,
                averagePartyLevel = context.AveragePartyLevel,
                matchedRuleIds = Distinct(matched.Select(value => value.stableId)),
                enabledRouteIds = Distinct(matched.SelectMany(value => value.enableRouteIds ?? Array.Empty<string>())),
                enabledBossIds = Distinct(matched.SelectMany(value => value.enableBossIds ?? Array.Empty<string>())),
                lootTableIds = Distinct(matched.SelectMany(value => value.lootTableIds ?? Array.Empty<string>())),
                variantTags = Distinct(matched.SelectMany(value => value.variantTags ?? Array.Empty<string>()))
            };
        }

        private static bool Matches(DungeonVariantRule rule, DungeonConditionContext context)
        {
            foreach (var condition in rule.allConditions ?? Array.Empty<DungeonConditionSpec>())
                if (condition == null || !Matches(condition, context))
                    return false;
            return true;
        }

        private static bool Matches(DungeonConditionSpec condition, DungeonConditionContext context) =>
            condition.kind switch
            {
                DungeonConditionKind.AveragePartyLevelAtLeast =>
                    context.AveragePartyLevel >= condition.threshold,
                DungeonConditionKind.PartyContainsClass =>
                    context.participants?.Any(value => value != null && value.classId == condition.value) == true,
                DungeonConditionKind.SessionFlag => context.sessionFlags?.Contains(condition.value) == true,
                DungeonConditionKind.EchoFlag => context.echoFlags?.Contains(condition.value) == true,
                DungeonConditionKind.QuestFlag => context.questFlags?.Contains(condition.value) == true,
                DungeonConditionKind.ItemPresent => context.itemIds?.Contains(condition.value) == true,
                DungeonConditionKind.Achievement => context.achievementIds?.Contains(condition.value) == true,
                _ => false
            };

        private static string[] Distinct(IEnumerable<string> values) =>
            values.Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
    }
}
