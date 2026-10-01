using System;
using System.Linq;
namespace DiceFree.Quests
{
    [Serializable] public sealed class ObjectiveCount
    {
        public string objectiveId;
        public int count;
        public ObjectiveCount Copy() => (ObjectiveCount)MemberwiseClone();
    }
    public static class ObjectiveProgress
    {
        public static int Count(QuestProgress state, string id) => Array.Find(state.alternatives, p => p.objectiveId == id)?.count ?? 0;
        public static bool Apply(QuestObjective objective, QuestProgress state, Func<QuestLeaf, bool> matches)
        {
            if (objective.kind != ObjectiveKind.Any)
            {
                if (!matches(objective)) return false;
                state.count = Math.Min(objective.count, state.count + 1); return true;
            }
            bool changed = false;
            foreach (var leaf in objective.alternatives)
            {
                if (!matches(leaf)) continue;
                var value = Array.Find(state.alternatives, p => p.objectiveId == leaf.objectiveId);
                if (value == null)
                {
                    value = new ObjectiveCount { objectiveId = leaf.objectiveId };
                    state.alternatives = state.alternatives.Concat(new[] { value }).ToArray();
                }
                value.count = Math.Min(leaf.count, value.count + 1); changed = true;
            }
            if (objective.alternatives.Any(a => Count(state, a.objectiveId) >= a.count)) state.count = objective.count;
            return changed;
        }
        public static bool Valid(QuestObjective objective, QuestProgress state)
        {
            if (state.alternatives == null || state.alternatives.Any(a => a == null || string.IsNullOrEmpty(a.objectiveId) || a.count < 0) ||
                state.alternatives.Select(a => a.objectiveId).Distinct().Count() != state.alternatives.Length) return false;
            if (objective.kind != ObjectiveKind.Any) return state.alternatives.Length == 0;
            foreach (var value in state.alternatives)
            {
                var leaf = Array.Find(objective.alternatives, a => a.objectiveId == value.objectiveId);
                if (leaf == null || value.count > leaf.count) return false;
            }
            bool complete = objective.alternatives.Any(a => Count(state, a.objectiveId) >= a.count);
            return state.count == (complete ? objective.count : 0) &&
                (state.status != QuestStatus.Available || state.alternatives.Length == 0);
        }
        public static string Describe(QuestObjective objective, QuestProgress state) => objective.kind == ObjectiveKind.Any
            ? string.Join("\nOR\n", objective.alternatives.Select(a => $"{a.instruction}: {Count(state, a.objectiveId)}/{a.count}"))
            : $"{state.count}/{objective.count} · {objective.instruction}";
    }
}
