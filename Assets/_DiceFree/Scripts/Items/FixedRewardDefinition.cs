using System;
using UnityEngine;

namespace DiceFree.Items
{
    [CreateAssetMenu(menuName = "DiceFree/Items/Fixed reward")]
    public sealed class FixedRewardDefinition : ScriptableObject
    {
        [Min(0)] public long gold;
        public ItemDefinition[] items = Array.Empty<ItemDefinition>();
    }
}
