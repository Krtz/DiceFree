using System;

namespace DiceFree.Combat
{
    [Serializable] public struct VitalityModifier
    {
        public float hpPerVitalityAdd, regenerationPerVitalityAdd;
        public float hpCoefficientPercent, regenerationCoefficientPercent;
    }
}
