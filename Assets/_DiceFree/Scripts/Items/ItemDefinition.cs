using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Items
{
    public enum EquipmentSlot { Head, Shoulders, Chest, Hands, Legs, Feet, MainHand, OffHand, Ring, Amulet, Back }
    [CreateAssetMenu(menuName = "DiceFree/Items/Handcrafted item")]
    public sealed class ItemDefinition : ScriptableObject
    {
        public string stableId, displayName;
        public Sprite icon;
        public EquipmentSlot slot;
        [Min(1)] public int itemLevel = 1;
        public string rarityId = "rarity.provisional";
        public string sourceId;
        public bool questItem, bound;
        public bool canSell = true;
        public string[] allowedClassIds = Array.Empty<string>();
        [TextArea] public string flavor;
        public EquipmentStats stats;
    }
    [Serializable] public sealed class ItemInstance
    {
        public string instanceId, definitionId;
        public bool bound;
        public ItemInstance Copy() => new() { instanceId = instanceId, definitionId = definitionId, bound = bound };
    }
    [Serializable] public sealed class EquippedItem
    {
        // String preserves future unknown slots inertly without enum reinterpretation.
        public string slotId, instanceId;
        public EquippedItem Copy() => new() { slotId = slotId, instanceId = instanceId };
    }
}
