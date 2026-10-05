using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    public sealed class InventoryPanel : HudWidget
    {
        private CarriedInventory inventory;
        private Equipment equipment;
        private GoldWallet wallet;
        private bool open;
        private Vector2 scroll;
        public override Rect Bounds => open ? new Rect(20, 200, 470, 360) : new Rect(20, 200, 160, 28);
        private void Awake() { inventory = GetComponent<CarriedInventory>(); equipment = GetComponent<Equipment>(); wallet = GetComponent<GoldWallet>(); }
        private void Update() { if (Keyboard.current?.bKey.wasPressedThisFrame == true) open = !open; }
        private void OnGUI()
        {
            if (!open) { if (GUI.Button(Bounds, "Inventory [B]")) open = true; return; }
            GUILayout.BeginArea(Bounds, GUI.skin.box);
            if (GUILayout.Button("Close inventory [B]")) open = false;
            GUILayout.Label($"Gold: {wallet.Gold}   Move Speed: {MovementUnits.DisplaySpeed(GetComponent<TraversalMotor>().Speed):0.##}");
            scroll = GUILayout.BeginScrollView(scroll);
            foreach (var item in inventory.Items)
            {
                var definition = inventory.Resolve(item.definitionId);
                var slot = equipment.Slots.FirstOrDefault(s => s.instanceId == item.instanceId);
                GUILayout.Label(definition == null ? "Unresolved: " + item.definitionId : definition.displayName + " / " + definition.slot);
                GUILayout.Label("Copy " + item.instanceId.Substring(0, 8) + (slot == null ? "" : " — Equipped " + slot.slotId));
                if (definition != null)
                {
                    var s = definition.stats;
                    if (s.attributes.vitality != 0) GUILayout.Label($"{s.attributes.vitality:+0.##;-0.##} Vitality");
                    if (s.attributes.strength != 0) GUILayout.Label($"{s.attributes.strength:+0.##;-0.##} Strength");
                    if (s.attributes.agility != 0) GUILayout.Label($"{s.attributes.agility:+0.##;-0.##} Agility");
                    if (s.attributes.intelligence != 0) GUILayout.Label($"{s.attributes.intelligence:+0.##;-0.##} Intelligence");
                    if (s.attributes.spirit != 0) GUILayout.Label($"{s.attributes.spirit:+0.##;-0.##} Spirit");
                    if (s.attackSpeedPercent != 0) GUILayout.Label($"{s.attackSpeedPercent:+0.##;-0.##}% Attack Speed");
                    if (s.physicalDefense != 0) GUILayout.Label($"{s.physicalDefense:+0.##;-0.##} Physical Defense");
                    if (s.magicalDefense != 0) GUILayout.Label($"{s.magicalDefense:+0.##;-0.##} Magical Defense");
                    if (s.movementSpeedPercent != 0) GUILayout.Label($"{s.movementSpeedPercent:+0.##;-0.##}% Movement Speed");
                    if (slot == null && GUILayout.Button("Equip " + definition.slot)) equipment.Equip(item.instanceId, definition.slot);
                }
                if (slot != null && GUILayout.Button("Unequip " + slot.slotId)) equipment.Unequip(slot.slotId);
            }
            GUILayout.EndScrollView();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GUILayout.Label("Development grants (each click creates a new copy)");
            foreach (var definition in inventory.Definitions)
                if (GUILayout.Button("Grant " + definition.displayName)) FixedItemGrant.Grant(inventory, definition);
            if (GUILayout.Button("Grant 10 gold (debug)")) wallet.Grant(10);
#endif
            GUILayout.EndArea();
        }
    }
}
