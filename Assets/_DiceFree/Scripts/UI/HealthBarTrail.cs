using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>Recent damage stays briefly visible, then drains toward actual HP.</summary>
    public sealed class HealthBarTrail
    {
        private float health, maximum, holdUntil;
        private bool initialized;
        public float Fraction { get; private set; }
        public void Sample(float current, float max, float now, float delta)
        {
            float ratio = Mathf.Clamp01(current / Mathf.Max(1, max));
            if (!initialized || max != maximum || current <= 0)
            { Fraction = ratio; holdUntil = now; initialized = true; }
            else if (current < health) holdUntil = now + .3f;
            if (now > holdUntil) Fraction = Mathf.MoveTowards(Fraction, ratio, Mathf.Max(0, delta) * .85f);
            Fraction = Mathf.Max(Fraction, ratio);
            health = current; maximum = max;
        }
    }
}
