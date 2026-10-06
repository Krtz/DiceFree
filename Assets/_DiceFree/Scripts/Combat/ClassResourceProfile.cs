using System;
using UnityEngine;

namespace DiceFree.Combat
{
    [CreateAssetMenu(menuName = "DiceFree/Combat/Class resource profile")]
    public sealed class ClassResourceProfile : ScriptableObject
    {
        public string stableId;
        public string classId;
        public ResourceDefinition resource;
        [Min(0)] public float baseMaximum = 50;
        [Min(0)] public float maximumPerIntelligence;
        [Min(0)] public float maximumPerSpirit;
        [Min(0)] public float baseRegenPerSecond = 2;
        [Min(0)] public float regenPerIntelligence;
        [Min(0)] public float regenPerSpirit;
        [Range(0, 1)] public float initialFraction = 1;
        public ResourceRetentionPolicy loadPolicy = ResourceRetentionPolicy.Persist;

        public float Maximum(AttributeValues attributes) => Mathf.Max(
            0,
            baseMaximum +
            attributes.intelligence * maximumPerIntelligence +
            attributes.spirit * maximumPerSpirit);

        public float Regeneration(AttributeValues attributes) => Mathf.Max(
            0,
            baseRegenPerSecond +
            attributes.intelligence * regenPerIntelligence +
            attributes.spirit * regenPerSpirit);

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(stableId) ||
                string.IsNullOrWhiteSpace(classId) ||
                resource == null ||
                string.IsNullOrWhiteSpace(resource.stableId))
                throw new InvalidOperationException(
                    "Class resource profile requires stable profile/class/resource identity.");

            if (float.IsNaN(baseMaximum) ||
                float.IsInfinity(baseMaximum) ||
                float.IsNaN(baseRegenPerSecond) ||
                float.IsInfinity(baseRegenPerSecond))
                throw new InvalidOperationException(
                    "Class resource profile contains non-finite tuning.");
        }
    }
}
