using UnityEngine;

namespace DiceFree.Progression
{
    [CreateAssetMenu(menuName = "DiceFree/Progression/Experience curve")]
    public sealed class ExperienceCurve : ScriptableObject
    {
        [Header("Quadratic XP per level: firstThreshold + quadraticCoefficient * (level - 1)^2")]
        [Min(1)] public int firstThreshold = 30;
        [Min(0)] public int quadraticCoefficient = 3;
        [Min(1)] public int levelLimit = 200;
        public int ToNextLevel(int level) => (int)System.Math.Clamp(
            (long)firstThreshold + (long)quadraticCoefficient * Mathf.Max(0, level - 1) * Mathf.Max(0, level - 1), 1, int.MaxValue);
    }
}
