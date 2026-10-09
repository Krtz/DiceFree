using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Input;
using DiceFree.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    public sealed class InventoryPanel : HudWidget
    {
        private const int Columns = 6;
        private const float SlotSize = 58f;
        private const float SlotGap = 5f;
        private const int MinimumVisibleSlots = 24;

        private CarriedInventory inventory;
        private Equipment equipment;
        private GoldWallet wallet;
        private TraversalMotor motor;
        private InputBindings bindings;
        private InputAction toggle;
        private bool open;
        private Vector2 scroll;

        public bool IsOpen => open;
        public override bool BlocksPointer => open;
        public override Rect Bounds => open ? new Rect(20, 190, 470, 390) : default;

        private HudThemeMetrics Theme =>
            HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);

        private void Awake()
        {
            inventory = GetComponent<CarriedInventory>();
            equipment = GetComponent<Equipment>();
            wallet = GetComponent<GoldWallet>();
            motor = GetComponent<TraversalMotor>();
            bindings = InputBindings.Current;
            toggle = bindings.Action("Gameplay/Inventory");
        }

        private void Update()
        {
            if (!bindings.Suppressed && toggle.WasPressedThisFrame()) open = !open;
        }

        public void Show() => open = true;
        public void Close() => open = false;

        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen || !open) return;

            Rect bounds = Bounds;
            HudThemeMetrics theme = Theme;
            HudChrome.DrawPanel(bounds, theme);

            float inset = theme.outerPadding + theme.innerPadding;
            var inner = new Rect(
                bounds.x + inset,
                bounds.y + inset,
                bounds.width - inset * 2f,
                bounds.height - inset * 2f);

            float headerHeight = 30f;
            var header = new Rect(inner.x, inner.y, inner.width, headerHeight);
            string speed = motor == null ? "?" : MovementUnits.DisplaySpeed(motor.Speed).ToString("0.##");
            HudChrome.DrawHeader(header, theme, $"Inventory    Gold {wallet.Gold}    Move Speed {speed}");

            string binding = InputBindings.Display(toggle);
            var closeRect = new Rect(header.xMax - 112f, header.y + 3f, 108f, header.height - 6f);
            if (GUI.Button(closeRect, "Close [" + binding + "]", HudChrome.HeaderButtonStyle(theme)))
                open = false;

            float debugHeight = 0f;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            debugHeight = 54f;
#endif
            var viewport = new Rect(
                inner.x,
                header.yMax + theme.gap,
                inner.width,
                inner.height - headerHeight - theme.gap - debugHeight);

            var items = equipment.BagItems;
            int totalSlots = Mathf.Max(
                MinimumVisibleSlots,
                Mathf.CeilToInt(items.Length / (float)Columns) * Columns);
            int rows = Mathf.CeilToInt(totalSlots / (float)Columns);
            float contentWidth = Columns * SlotSize + (Columns - 1) * SlotGap + 8f;
            float contentHeight = rows * SlotSize + Mathf.Max(0, rows - 1) * SlotGap + 4f;
            var content = new Rect(0, 0, contentWidth, contentHeight);

            scroll = GUI.BeginScrollView(viewport, scroll, content, false, contentHeight > viewport.height);
            for (int index = 0; index < totalSlots; index++)
            {
                int row = index / Columns;
                int column = index % Columns;
                var slotRect = new Rect(
                    column * (SlotSize + SlotGap),
                    row * (SlotSize + SlotGap),
                    SlotSize,
                    SlotSize);

                bool occupied = index < items.Length;
                ItemDefinition definition = null;
                string tooltip = "Empty inventory slot";
                string label = "";

                if (occupied)
                {
                    var item = items[index];
                    definition = inventory.Resolve(item.definitionId);
                    label = definition == null ? "?" : ShortName(definition.displayName);
                    tooltip = definition == null
                        ? "Unresolved item: " + item.definitionId
                        : HudTooltip.Item(definition);
                }

                bool hovered = slotRect.Contains(Event.current.mousePosition);
                HudChrome.DrawSlot(slotRect, theme, occupied, hovered);

                if (occupied && definition != null)
                {
                    if (GUI.Button(slotRect, new GUIContent("", tooltip), GUIStyle.none))
                        equipment.Equip(items[index].instanceId, definition.slot);
                }
                else
                {
                    GUI.Label(slotRect, new GUIContent("", tooltip), GUIStyle.none);
                }

                if (!ItemIconGUI.Draw(slotRect, definition) && !string.IsNullOrEmpty(label))
                    GUI.Label(slotRect, label, HudChrome.SlotTextStyle(theme, true));
            }
            GUI.EndScrollView();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var debug = new Rect(inner.x, inner.yMax - debugHeight + 2f, inner.width, debugHeight - 2f);
            HudChrome.Fill(debug, new Color(theme.slotTint.r, theme.slotTint.g, theme.slotTint.b, 0.72f));
            GUILayout.BeginArea(new Rect(debug.x + 4f, debug.y + 2f, debug.width - 8f, debug.height - 4f));
            GUILayout.Label("Development grants");
            GUILayout.BeginHorizontal();
            foreach (var definition in inventory.Definitions)
                if (GUILayout.Button("+" + ShortName(definition.displayName)))
                    FixedItemGrant.Grant(inventory, definition);
            if (GUILayout.Button("+10g", GUILayout.Width(48))) wallet.Grant(10);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
#endif

            HudTooltip.DrawCurrent();
        }

        private static string ShortName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "?";
            string[] words = value.Trim().Split(' ');
            if (words.Length > 1)
            {
                string acronym = string.Empty;
                foreach (string word in words)
                    if (word.Length > 0) acronym += char.ToUpperInvariant(word[0]);
                if (acronym.Length <= 4) return acronym;
            }

            return value.Length <= 4 ? value : value.Substring(0, 4);
        }
    }
}
