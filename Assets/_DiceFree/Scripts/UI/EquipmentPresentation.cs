using System;
using DiceFree.Items;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>Form-authored visual bindings; equipment remains the sole item/state authority.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Equipment), typeof(CarriedInventory))]
    public sealed class EquipmentPresentation : MonoBehaviour
    {
        [Serializable]
        public sealed class Binding
        {
            public EquipmentSlot slot;
            public ItemDefinition definition;
            public GameObject[] visuals = Array.Empty<GameObject>();
        }

        [SerializeField] private Binding[] bindings = Array.Empty<Binding>();
        private Equipment equipment;
        private CarriedInventory inventory;
        public Binding[] Bindings => bindings;

        private void Awake()
        {
            equipment = GetComponent<Equipment>();
            inventory = GetComponent<CarriedInventory>();
        }

        private void OnEnable()
        {
            equipment.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            equipment.Changed -= Refresh;
            foreach (var binding in bindings) SetVisible(binding, false);
        }

        public void Configure(Binding[] values)
        {
            foreach (var binding in bindings) SetVisible(binding, false);
            bindings = values ?? Array.Empty<Binding>();
            if (isActiveAndEnabled && equipment != null) Refresh();
        }

        private void Refresh()
        {
            foreach (var binding in bindings)
            {
                bool visible = false;
                if (binding.definition != null && binding.definition.slot == binding.slot)
                    foreach (var slot in equipment.Slots)
                    {
                        if (slot.slotId != binding.slot.ToString() || !equipment.IsResolved(slot)) continue;
                        var item = inventory.Find(slot.instanceId);
                        visible |= item != null && string.Equals(item.definitionId, binding.definition.stableId, StringComparison.Ordinal);
                    }
                SetVisible(binding, visible);
            }
        }

        private static void SetVisible(Binding binding, bool visible)
        {
            foreach (var visual in binding.visuals)
                if (visual != null) visual.SetActive(visible);
        }
    }
}
