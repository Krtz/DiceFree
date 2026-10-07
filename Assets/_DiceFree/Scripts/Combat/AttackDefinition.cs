using UnityEngine;

namespace DiceFree.Combat
{
    public enum DamageChannel { Physical, Magical }
    public enum AttributeScaling { Weighted, Highest, HighestSelected }

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
        [Range(0.05f, 1f)] public float minimumDamageMultiplier = 1f;
        [Range(1f, 2f)] public float maximumDamageMultiplier = 1f;
        [Min(0.1f)] public float interval = 1.1f;
        [Min(0)] public float windup = 0.25f;
        [Min(0.1f)] public float reach = 0.7f;
        public float RawDamage(AttributeValues attributes) => RawDamage(attributes, coefficient);
        // Current combat uses fixed pre-mitigation basic-attack damage. Keep an explicit
        // range contract so UI remains correct when authored damage spread is introduced later.
        public Vector2 RawDamageRange(AttributeValues attributes)
        {
            float raw = RawDamage(attributes);
            float minimum = minimumDamageMultiplier <= 0f ? 1f : minimumDamageMultiplier;
            float maximum = maximumDamageMultiplier <= 0f ? 1f : maximumDamageMultiplier;
            if (maximum < minimum) maximum = minimum;
            return new Vector2(raw * minimum, raw * maximum);
        }

        public float RollDamageMultiplier()
        {
            float minimum = minimumDamageMultiplier <= 0f ? 1f : minimumDamageMultiplier;
            float maximum = maximumDamageMultiplier <= 0f ? 1f : maximumDamageMultiplier;
            if (maximum < minimum) maximum = minimum;
            return Mathf.Approximately(minimum, maximum)
                ? minimum
                : Random.Range(minimum, maximum);
        }

        public float RawDamage(AttributeValues attributes, float coefficientOverride)
        {
            float scaled = scaling switch
            {
                AttributeScaling.Highest => attributes.Highest,
                AttributeScaling.HighestSelected => attributes.HighestSelected(weights),
                _ => attributes.Weighted(weights)
            };
            return Mathf.Max(0, baseDamage + Mathf.Max(0, coefficientOverride) * scaled);
        }
    }
}
