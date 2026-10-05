using System;
using System.Collections.Generic;
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
            public GameObject[] baselineVisuals = Array.Empty<GameObject>();
        }

        [SerializeField] private Binding[] bindings = Array.Empty<Binding>();
        private Equipment equipment;
        private CarriedInventory inventory;
        private readonly Dictionary<GameObject, bool> baselineStates = new();
        public Binding[] Bindings => bindings;

        private void Awake()
        {
            equipment = GetComponent<Equipment>();
            inventory = GetComponent<CarriedInventory>();
            CaptureBaseline();
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
            RestoreBaseline();
        }

        public void Configure(Binding[] values)
        {
            foreach (var binding in bindings) SetVisible(binding, false);
            RestoreBaseline();
            baselineStates.Clear();
            bindings = values ?? Array.Empty<Binding>();
            CaptureBaseline();
            if (isActiveAndEnabled && equipment != null) Refresh();
        }

        private void Refresh()
        {
            RestoreBaseline();
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
                if (visible)
                    foreach (var visual in binding.baselineVisuals)
                        if (visual != null) visual.SetActive(false);
            }
        }

        private void CaptureBaseline()
        {
            foreach (var binding in bindings)
                foreach (var visual in binding.baselineVisuals)
                    if (visual != null && !baselineStates.ContainsKey(visual)) baselineStates.Add(visual, visual.activeSelf);
        }

        private void RestoreBaseline()
        {
            foreach (var state in baselineStates)
                if (state.Key != null) state.Key.SetActive(state.Value);
        }

        private static void SetVisible(Binding binding, bool visible)
        {
            foreach (var visual in binding.visuals)
                if (visual != null) visual.SetActive(visible);
        }
    }
}
