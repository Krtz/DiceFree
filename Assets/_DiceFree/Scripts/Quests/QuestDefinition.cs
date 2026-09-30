using System;
using UnityEngine;

namespace DiceFree.Quests
{
    public enum ObjectiveKind { Kill, ReachArea, TalkTo }
    [Serializable]
    public sealed class QuestObjective
    {
        public ObjectiveKind kind;
        public string instruction;
        public string contentId;
        public string familyId;
        public string areaId;
        public string conversationId;
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
        // Opt-in story conversations; existing quests retain explicit acceptance/turn-in.
        public bool acceptOnTalk;
        public bool completeOnObjectives;
    }
}
