using System;
using UnityEngine;

namespace DiceFree.Quests
{
    public enum ObjectiveKind { Kill, ReachArea }
    [Serializable]
    public sealed class QuestObjective
    {
        public ObjectiveKind kind;
        public string instruction;
        public string contentId;
        public string familyId;
        public string areaId;
        [Min(1)] public int count = 1;
    }
    [CreateAssetMenu(menuName = "DiceFree/Quests/Quest")]
    public sealed class QuestDefinition : ScriptableObject
    {
        public string stableId, title;
        [Min(1)] public int version = 1;
        [TextArea] public string offer;
        public string locationHint;
        public QuestObjective[] stages;
        public string[] completedQuestIds = Array.Empty<string>();
        [Min(0)] public int rewardXp;
    }
}
