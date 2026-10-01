using System;
using UnityEngine;

namespace DiceFree.Combat
{
    [Serializable]
    public struct ElementResistance
    {
        public ElementDefinition element;
        public float fraction;
    }

    // Only the combat stat package. No class ancestry, advancement graph or resource assumption.
    [CreateAssetMenu(menuName = "DiceFree/Combat/Actor definition")]
    public sealed class ActorDefinition : ScriptableObject
    {
        public string stableId;
        public string displayName;
        public string familyId;
        public string[] tags = Array.Empty<string>();
        [Min(0)] public int experienceReward;
        public AttributeValues baseAttributes = new AttributeValues(1);
        public AttributeValues growth = new AttributeValues(1);
        [Min(1)] public float baseHp = 10;
        [Min(0)] public float physicalDefense, magicalDefense;
        [Min(0.1f)] public float moveSpeed = 5;
        public AttackDefinition basicAttack;
        public CombatTuning tuning;
        public SecondaryCoefficientOverride[] secondaryOverrides = Array.Empty<SecondaryCoefficientOverride>();
        public float SecondaryCoefficient(SecondaryStat stat)
        {
            foreach (var entry in secondaryOverrides) if (entry.stat == stat) return entry.coefficient;
            return tuning.Coefficient(stat);
        }
        public bool overrideVitalityHp, overrideVitalityRegeneration;
        [Min(0)] public float hpPerVitality = 15;
        [Min(0)] public float regenerationPerVitality = 0.1f;
        public float VitalityHpCoefficient => overrideVitalityHp ? hpPerVitality : tuning.hpPerVitality;
        public float VitalityRegenerationCoefficient => overrideVitalityRegeneration ? regenerationPerVitality : tuning.regenerationPerVitality;
        public ElementResistance[] resistances = Array.Empty<ElementResistance>();
    }
}
