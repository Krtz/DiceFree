using UnityEngine;

namespace DiceFree.Progression
{
    [CreateAssetMenu(menuName = "DiceFree/Progression/Experience curve")]
    public sealed class ExperienceCurve : ScriptableObject
    {
        [Header("Provisional starter curve, not final level 1-200 balance")]
        [Min(1)] public int firstThreshold = 30;
        [Min(0)] public int thresholdGrowth = 10;
        [Min(1)] public int levelLimit = 200;
        public int ToNextLevel(int level) => (int)System.Math.Clamp(
            (long)firstThreshold + (long)Mathf.Max(0, level - 1) * thresholdGrowth, 1, int.MaxValue);
    }
}
