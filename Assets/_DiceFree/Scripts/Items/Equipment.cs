using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Items
{
    [RequireComponent(typeof(CarriedInventory), typeof(ActorStats))]
    public sealed class Equipment : MonoBehaviour
    {
        private CarriedInventory inventory;
        private ActorStats stats;
        private readonly List<EquippedItem> slots = new();
        private readonly HashSet<string> applied = new(StringComparer.Ordinal);
        public event Action Changed;
        public EquippedItem[] Slots => slots.Select(s => s.Copy()).ToArray();
        public bool IsEquipped(string instanceId) =>
            !string.IsNullOrWhiteSpace(instanceId) && slots.Any(s => s.instanceId == instanceId);
        public ItemInstance[] BagItems => inventory == null
            ? Array.Empty<ItemInstance>()
            : inventory.Items.Where(item => !IsEquipped(item.instanceId)).ToArray();
        private void Awake() { inventory = GetComponent<CarriedInventory>(); stats = GetComponent<ActorStats>(); }
        private void OnEnable() { inventory.Changed += OnInventoryChanged; Refresh(); }
        private void OnDisable() { inventory.Changed -= OnInventoryChanged; ClearContributions(); }
        public bool Equip(string instanceId, EquipmentSlot slot)
        {
            var item = inventory.Find(instanceId); var definition = item == null ? null : inventory.Resolve(item.definitionId);
            if (definition == null || definition.slot != slot || !Enum.IsDefined(typeof(EquipmentSlot), slot)) return false;
            slots.RemoveAll(s => s.slotId == slot.ToString() || s.instanceId == instanceId);
            slots.Add(new EquippedItem { slotId = slot.ToString(), instanceId = instanceId });
            Refresh(); Changed?.Invoke(); return true;
        }
        public bool Unequip(string slotId)
        {
            if (slots.RemoveAll(s => s.slotId == slotId) == 0) return false;
            Refresh(); Changed?.Invoke(); return true;
        }
        public bool IsResolved(EquippedItem slot)
        {
            var item = inventory.Find(slot.instanceId); var definition = item == null ? null : inventory.Resolve(item.definitionId);
            return definition != null && definition.slot.ToString() == slot.slotId;
        }
        private void OnInventoryChanged()
        {
            slots.RemoveAll(s => inventory.Find(s.instanceId) == null);
            Refresh(); Changed?.Invoke();
        }
        private void ClearContributions() { foreach (var id in applied) stats.RemoveEquipmentContribution(id); applied.Clear(); }
        private void Refresh()
        {
            ClearContributions();
            if (!isActiveAndEnabled) return;
            foreach (var slot in slots.Where(IsResolved).OrderBy(s => s.instanceId, StringComparer.Ordinal))
            {
                var item = inventory.Find(slot.instanceId);
                string source = "equipment:" + item.instanceId;
                stats.SetEquipmentContribution(source, inventory.Resolve(item.definitionId).stats); applied.Add(source);
            }
        }
        public static bool Valid(EquippedItem[] values, ItemInstance[] owned) => values != null &&
            values.All(s => s != null && !string.IsNullOrWhiteSpace(s.slotId) && owned.Any(i => i.instanceId == s.instanceId)) &&
            values.Select(s => s.slotId).Distinct(StringComparer.Ordinal).Count() == values.Length &&
            values.Select(s => s.instanceId).Distinct(StringComparer.Ordinal).Count() == values.Length;
        public void Restore(EquippedItem[] values)
        {
            if (!Valid(values, inventory.Items)) throw new ArgumentException("Invalid equipment references.");
            slots.Clear(); slots.AddRange(values.Select(s => s.Copy())); Refresh(); Changed?.Invoke();
        }
    }
}
