using System;
using UnityEngine;

namespace DiceFree.Quests
{
    public enum ObjectiveKind { Kill, ReachArea, TalkTo, Any }
    [Serializable]
    public class QuestLeaf
    {
        public ObjectiveKind kind;
        public string objectiveId;
        public string requiredTag;
        public QuestCreditMode creditMode;
        [Min(0)] public float creditRadius = 50;
        public string instruction;
        public string contentId;
        public string familyId;
        public string areaId;
        public string conversationId;
        [Min(1)] public int count = 1;
    }
    // One shallow ANY group is sufficient here; no recursive graph serialization.
    [Serializable]
    public sealed class QuestObjective : QuestLeaf
    {
        public QuestLeaf[] alternatives = Array.Empty<QuestLeaf>();
    }
    [CreateAssetMenu(menuName = "DiceFree/Quests/Quest")]
    public sealed class QuestDefinition : ScriptableObject
    {
        public string stableId, title;
        [Min(1)] public int version = 1;
        [TextArea] public string offer;
        [TextArea] public string activeDialogue;
        public string locationHint;
        public QuestObjective[] stages;
        public string[] completedQuestIds = Array.Empty<string>();
        [Min(0)] public int rewardXp;
        public DiceFree.Items.FixedRewardDefinition reward;
        // Opt-in story conversations; existing quests retain explicit acceptance/turn-in.
        public bool acceptOnTalk;
        public bool completeOnObjectives;
    }
}
