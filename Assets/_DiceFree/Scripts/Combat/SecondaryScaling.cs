using System;

namespace DiceFree.Combat
{
    public enum SecondaryStat { PhysicalDefense, MagicalDefense, AttackSpeed, MoveSpeed, HealingDone, HealingReceived }

    [Serializable]
    public struct SecondaryCoefficientOverride
    {
        public SecondaryStat stat;
        public float coefficient;
    }

    // Percentages are fractions: 0.1 means +10% of the coefficient, not +10 percentage points.
    public struct SecondaryScalingModifier
    {
        public SecondaryStat stat;
        public float add, percent;
    }
}
