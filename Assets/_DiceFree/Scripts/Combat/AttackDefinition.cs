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
            => RawDamageRange(attributes, coefficient);

        public Vector2 RawDamageRange(AttributeValues attributes, float coefficientOverride)
        {
            float minimum = minimumDamageMultiplier <= 0f ? 1f : minimumDamageMultiplier;
            float maximum = maximumDamageMultiplier <= 0f ? 1f : maximumDamageMultiplier;
            if (maximum < minimum) maximum = minimum;
            float baseTerm=Mathf.Max(0,baseDamage);
            float primaryTerm=Mathf.Max(0,Mathf.Max(0,coefficientOverride)*ScaledAttributeValue(attributes));
            // Primary scaling contributes fully to the maximum and 99% of that
            // contribution to the minimum. Authored weapon spread still applies
            // to the base-damage term.
            return new Vector2(baseTerm*minimum+primaryTerm*.99f,
                baseTerm*maximum+primaryTerm);
        }

        private float ScaledAttributeValue(AttributeValues attributes) => scaling switch
        {
            AttributeScaling.Highest => attributes.Highest,
            AttributeScaling.HighestSelected => attributes.HighestSelected(weights),
            _ => attributes.Weighted(weights)
        };

        public float RollDamageMultiplier(AttributeValues attributes,float damageScale=1f,float? coefficientOverride=null)
        {
            float scale=Mathf.Max(0,damageScale);
            float raw=RawDamage(attributes,coefficientOverride??coefficient);
            if(raw<=.0001f)return scale;
            Vector2 range=RawDamageRange(attributes,coefficientOverride??coefficient)*scale;
            return Random.Range(range.x,range.y)/raw;
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
            float scaled = ScaledAttributeValue(attributes);
            return Mathf.Max(0, baseDamage + Mathf.Max(0, coefficientOverride) * scaled);
        }
    }
}
