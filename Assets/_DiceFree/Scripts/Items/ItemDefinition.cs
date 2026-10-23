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
        [Range(1,2)] public int handsRequired = 1;
        [Min(1)] public int itemLevel = 1;
        public string rarityId = "rarity.provisional";
        public string sourceId;
        public bool questItem, bound;
        [Tooltip("Prototype model-fitting gear: permits the plain offhand on all classes, including the two-handed Greatblade.")]
        public bool modelTestingItem;
        public bool canSell = true;
        [Header("Consumable (prototype)")]
        public bool consumable;
        [Min(0)] public float restoreHealth;
        public string restoreResourceId;
        [Min(0)] public float restoreResourceAmount;
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
