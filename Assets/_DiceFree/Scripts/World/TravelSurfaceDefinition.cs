using UnityEngine;

namespace DiceFree.World
{
    [CreateAssetMenu(menuName = "DiceFree/Travel/Surface")]
    public sealed class TravelSurfaceDefinition : ScriptableObject
    {
        public string stableId, displayName;
        [Min(0)] public float speedBonusPercent;
        public int priority;
    }
}
