using UnityEngine;

namespace DiceFree.Combat
{
    /// <summary>A cast-owned capacity that multiple Health components can spend.</summary>
    public sealed class SharedDamageShield
    {
        private readonly float expires;
        private readonly string elementId;
        private float capacity;
        public float Remaining => Time.time < expires ? capacity : 0;

        public SharedDamageShield(float amount, float duration, ElementDefinition element)
        {
            capacity = Mathf.Max(0, amount);
            expires = Time.time + duration;
            elementId = element != null ? element.stableId : null;
        }

        public void Expire() => capacity = 0;

        internal static float CostPerDamage(string shieldElement, ElementDefinition incoming) =>
            string.IsNullOrEmpty(shieldElement) ? 1f : incoming != null && incoming.stableId == shieldElement ? .8f : 1.2f;

        internal float Absorb(float incoming, ElementDefinition element)
        {
            float cost = CostPerDamage(elementId, element);
            float blocked = Mathf.Min(incoming, Remaining / cost);
            capacity = Mathf.Max(0, capacity - blocked * cost);
            return incoming - blocked;
        }
    }
}