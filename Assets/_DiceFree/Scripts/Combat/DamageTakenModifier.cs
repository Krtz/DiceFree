using UnityEngine;

namespace DiceFree.Combat
{
    public readonly struct DamageTakenModifier
    {
        public readonly DamageChannel channel;
        public readonly float reductionFraction;

        public DamageTakenModifier(DamageChannel damageChannel, float reduction)
        {
            channel = damageChannel;
            reductionFraction = Mathf.Clamp(reduction, 0, 0.95f);
        }
    }
}
