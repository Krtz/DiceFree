using UnityEngine;

namespace DiceFree.Combat
{
    public enum DamageChannel { Physical, Magical }
    public enum AttributeScaling { Weighted, Highest }

    [CreateAssetMenu(menuName = "DiceFree/Combat/Basic attack")]
    public sealed class AttackDefinition : ScriptableObject
    {
        public string stableId;
        public string displayName;
        public DamageChannel channel;
        public ElementDefinition element;
        public bool requiresAccuracy;
        public AttributeScaling scaling;
        public AttributeValues weights;
        [Min(0)] public float baseDamage = 1;
        [Min(0)] public float coefficient = 2;
        [Min(0.1f)] public float interval = 1.1f;
        [Min(0)] public float windup = 0.25f;
        [Min(0.1f)] public float reach = 0.7f;
        public float RawDamage(AttributeValues attributes) => Mathf.Max(0, baseDamage + coefficient *
            (scaling == AttributeScaling.Highest ? attributes.Highest : attributes.Weighted(weights)));
    }
}
