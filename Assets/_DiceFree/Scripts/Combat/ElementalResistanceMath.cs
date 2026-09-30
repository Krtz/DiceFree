using System;

namespace DiceFree.Combat
{
    [Flags]
    public enum ElementalContext { Damage = 1, Healing = 2 }

    public readonly struct ElementalResistanceResolution
    {
        public readonly float raw, penetration, preCap, effective;
        public readonly ElementalContext context;
        public ElementalResistanceResolution(float raw, float penetration, float cap, ElementalContext context)
        {
            if (context != ElementalContext.Damage && context != ElementalContext.Healing)
                throw new ArgumentOutOfRangeException(nameof(context));
            this.raw = raw; this.penetration = penetration; this.context = context;
            preCap = raw + (context == ElementalContext.Damage ? -penetration : penetration);
            effective = Math.Min(preCap, cap); // No negative floor. Cap policy supplied by caller.
        }
    }

    public readonly struct ElementalModifier
    {
        public readonly string elementId;
        public readonly float delta;
        public readonly ElementalContext contexts;
        public ElementalModifier(string elementId, float delta, ElementalContext contexts = ElementalContext.Damage | ElementalContext.Healing)
        {
            if (string.IsNullOrWhiteSpace(elementId) || float.IsNaN(delta) || float.IsInfinity(delta))
                throw new ArgumentException("Elemental modifier requires stable element identity and finite delta.");
            if (contexts == 0 || (contexts & ~(ElementalContext.Damage | ElementalContext.Healing)) != 0)
                throw new ArgumentOutOfRangeException(nameof(contexts));
            this.elementId = elementId; this.delta = delta; this.contexts = contexts;
        }
    }

    // Pure result only: no Health mutation, Defense channel, shields or ability framework.
    public readonly struct ElementalHealingResult
    {
        public readonly ElementDefinition element;
        public readonly ElementalResistanceResolution resistance;
        public readonly float signedAmount;
        public float Healing => Math.Max(0, signedAmount);
        public float InversionDamage => Math.Max(0, -signedAmount);
        public ElementalHealingResult(float amount, ElementDefinition element, ElementalResistanceResolution resistance)
        {
            if (amount < 0 || float.IsNaN(amount) || float.IsInfinity(amount)) throw new ArgumentOutOfRangeException(nameof(amount));
            if (element != null && resistance.context != ElementalContext.Healing) throw new ArgumentException("Healing context required.");
            this.element = element; this.resistance = resistance;
            signedAmount = amount * (element == null ? 1 : 1 + resistance.effective);
        }
    }
}
