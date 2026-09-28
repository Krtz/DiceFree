using System;
using UnityEngine;

namespace DiceFree.Quests
{
    [Serializable]
    public sealed class KillObjective
    {
        public string instruction;
        public string contentId;
        public string familyId;
        [Min(1)] public int count = 1;
    }
    [CreateAssetMenu(menuName = "DiceFree/Quests/Kill quest")]
    public sealed class QuestDefinition : ScriptableObject
    {
        public string stableId, title;
        [Min(1)] public int version = 1;
        [TextArea] public string offer;
        public string locationHint;
        public KillObjective[] stages;
        [Min(0)] public int rewardXp;
    }
}
