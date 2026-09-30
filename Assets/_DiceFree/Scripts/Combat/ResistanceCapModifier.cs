using System;

namespace DiceFree.Combat
{
    // Percentage-point delta in normalized units: 0.05 means +5% cap.
    public readonly struct ResistanceCapModifier
    {
        public readonly string elementId; // null explicitly means all elements.
        public readonly float delta;
        private ResistanceCapModifier(string elementId, float delta)
        {
            if (float.IsNaN(delta) || float.IsInfinity(delta)) throw new ArgumentOutOfRangeException(nameof(delta));
            this.elementId = elementId; this.delta = delta;
        }
        public static ResistanceCapModifier Global(float delta) => new(null, delta);
        public static ResistanceCapModifier ForElement(string elementId, float delta)
        {
            if (string.IsNullOrWhiteSpace(elementId)) throw new ArgumentException("Stable element identity required.");
            return new ResistanceCapModifier(elementId, delta);
        }
    }
}
