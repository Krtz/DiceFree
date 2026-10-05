using System;
using UnityEngine;

namespace DiceFree.Combat
{
    [Serializable]
    public struct AttributeValues
    {
        public float vitality, strength, agility, intelligence, spirit;
        public AttributeValues(float all) { vitality = strength = agility = intelligence = spirit = all; }
        public float Highest => Mathf.Max(vitality, Mathf.Max(strength, Mathf.Max(agility, Mathf.Max(intelligence, spirit))));
        public static AttributeValues AtLevel(AttributeValues basis, AttributeValues growth, int level)
        {
            var n = Mathf.Max(0, level - 1);
            return new AttributeValues {
                vitality = basis.vitality + growth.vitality * n, strength = basis.strength + growth.strength * n,
                agility = basis.agility + growth.agility * n, intelligence = basis.intelligence + growth.intelligence * n,
                spirit = basis.spirit + growth.spirit * n
            };
        }
        public static AttributeValues operator +(AttributeValues a, AttributeValues b) => new AttributeValues {
            vitality = a.vitality + b.vitality, strength = a.strength + b.strength,
            agility = a.agility + b.agility, intelligence = a.intelligence + b.intelligence,
            spirit = a.spirit + b.spirit
        };
        public float Weighted(AttributeValues weights) => vitality * weights.vitality + strength * weights.strength +
            agility * weights.agility + intelligence * weights.intelligence + spirit * weights.spirit;
    }
}
