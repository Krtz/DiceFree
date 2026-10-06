using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Progression
{
    [CreateAssetMenu(menuName = "DiceFree/Progression/Advancement")]
    public sealed class AdvancementDefinition : ScriptableObject
    {
        public string stableId;
        public string displayName;
        public ActorDefinition sourceClass;
        public ActorDefinition targetClass;
        [Min(1)] public int requiredLevel = 10;

        public bool Eligible(ActorStats stats) =>
            stats != null &&
            sourceClass != null &&
            targetClass != null &&
            stats.Definition != null &&
            stats.Definition.stableId == sourceClass.stableId &&
            stats.Level >= requiredLevel;
    }
}
