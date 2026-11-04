using System;
using System.Linq;
using DiceFree.Quests;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>Quest display text only; never advances or rewrites journal records.</summary>
    public static class QuestReadout
    {
        private static QuestGiver[] givers = Array.Empty<QuestGiver>();
        private static float refreshAt;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Clear() { givers = Array.Empty<QuestGiver>(); refreshAt = 0; }
        public static bool Supported(QuestDefinition quest, QuestProgress state) =>
            quest != null && state != null && state.definitionVersion == quest.version &&
            quest.stages != null && quest.stages.Length > 0;
        public static string TurnIn(QuestDefinition quest)
        {
            if (!Application.isPlaying || Time.unscaledTime >= refreshAt || givers.Length == 0 || givers.All(g => g == null))
            {
                givers = UnityEngine.Object.FindObjectsByType<QuestGiver>();
                refreshAt = Time.unscaledTime + 1;
            }
            var giver = givers.Where(g => g != null && g.isActiveAndEnabled && g.gameObject.activeInHierarchy &&
                    g.Offers.Any(q => q != null && q.stableId == quest.stableId))
                .OrderBy(g => g.DisplayName, StringComparer.Ordinal).FirstOrDefault();
            return giver != null && !string.IsNullOrWhiteSpace(giver.DisplayName)
                ? "Return to " + giver.DisplayName + "."
                : "Return to the quest giver.";
        }
        public static string Objective(QuestDefinition quest, QuestProgress state)
        {
            if (!Supported(quest, state)) return "Saved quest version unavailable; progress preserved.";
            if (state.status == QuestStatus.ReadyToTurnIn) return TurnIn(quest);
            int stage = Mathf.Clamp(state.stage, 0, quest.stages.Length - 1);
            string prefix = quest.stages.Length > 1 ? $"Step {stage + 1} of {quest.stages.Length}\n" : "";
            return prefix + ObjectiveProgress.Describe(quest.stages[stage], state);
        }
        public static float Fraction(QuestDefinition quest, QuestProgress state)
        {
            if (!Supported(quest, state)) return 0;
            if (state.status is QuestStatus.ReadyToTurnIn or QuestStatus.Completed) return 1;
            if (state.status == QuestStatus.Available) return 0;
            int stage = Mathf.Clamp(state.stage, 0, quest.stages.Length - 1);
            var objective = quest.stages[stage];
            float current = Mathf.Clamp01((float)state.count / Mathf.Max(1, objective.count));
            if (objective.kind == ObjectiveKind.Any && objective.alternatives != null)
                current = objective.alternatives.Where(a => a != null).Select(a =>
                    Mathf.Clamp01((float)ObjectiveProgress.Count(state, a.objectiveId) / Mathf.Max(1, a.count))).DefaultIfEmpty(0).Max();
            return (stage + current) / quest.stages.Length;
        }
    }
}
