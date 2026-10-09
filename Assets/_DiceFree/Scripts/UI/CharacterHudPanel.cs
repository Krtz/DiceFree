using System;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Items;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class CharacterHudPanel : CustomizableHudWidget
    {
        private static readonly EquipmentSlot[] LeftSlots =
        {
            EquipmentSlot.Head,
            EquipmentSlot.Shoulders,
            EquipmentSlot.Chest,
            EquipmentSlot.Hands,
            EquipmentSlot.Legs,
            EquipmentSlot.Feet
        };

        private static readonly EquipmentSlot[] RightSlots =
        {
            EquipmentSlot.MainHand,
            EquipmentSlot.OffHand,
            EquipmentSlot.Ring,
            EquipmentSlot.Amulet,
            EquipmentSlot.Back
        };

        [SerializeField] private CombatActor player;
        [SerializeField] private ActorResourceController resources;
        [SerializeField] private Equipment equipment;
        [SerializeField] private CarriedInventory inventory;
        [SerializeField] private HudPortraitRenderer portrait;

        public override string LayoutId => "character";
        public override string DisplayName => "Character";
        public override Rect DefaultNormalizedBounds => new(0.006f, 0.715f, 0.222f, 0.275f);
        public override Vector2 MinimumPixelSize => new(260, 215);

        public void Configure(CombatActor actor)
        {
            player = actor;
            resources = actor == null ? null : actor.GetComponent<ActorResourceController>();
            equipment = actor == null ? null : actor.GetComponent<Equipment>();
            inventory = actor == null ? null : actor.GetComponent<CarriedInventory>();
            portrait = actor == null ? null : actor.GetComponent<HudPortraitRenderer>();
        }

        private void Awake()
        {
            if (player == null) player = GetComponent<CombatActor>();
            if (player != null) Configure(player);
        }

        private void OnGUI()
        {
            if (player == null || HudPointerBlocker.ModalOpen) return;

            Rect panel = Bounds;
            DrawPanel(panel);
            Rect inner = Inner(panel);
            float gap = Theme.gap;
            float barHeight = Mathf.Clamp(inner.height * 0.085f, 17f, 25f);
            float slotWidth = Mathf.Clamp(inner.width * 0.135f, 28f, 46f);
            float contentBottom = inner.yMax - barHeight * 2 - gap * 2;
            float portraitX = inner.x + slotWidth + gap;
            float portraitWidth = Mathf.Max(40, inner.width - (slotWidth + gap) * 2);
            var portraitRect = new Rect(
                portraitX,
                inner.y,
                portraitWidth,
                Mathf.Max(40, contentBottom - inner.y));

            HudChrome.DrawSlot(portraitRect, Theme, true, false);
            float portraitInset = Mathf.Max(2f, Theme.border);
            var portraitImageRect = new Rect(
                portraitRect.x + portraitInset,
                portraitRect.y + portraitInset,
                Mathf.Max(0, portraitRect.width - portraitInset * 2f),
                Mathf.Max(0, portraitRect.height - portraitInset * 2f));

            if (portrait != null && portrait.Texture != null)
                GUI.DrawTexture(portraitImageRect, portrait.Texture, ScaleMode.ScaleToFit, false);
            else
                GUI.Label(portraitImageRect, "3D character",
                    new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });

            DrawEquipmentColumn(inner.x, inner.y, slotWidth, portraitRect.height, LeftSlots);
            DrawEquipmentColumn(inner.xMax - slotWidth, inner.y, slotWidth, portraitRect.height, RightSlots);

            float hpMaximum = Mathf.Max(1, player.Health.Maximum);
            var hpRect = new Rect(inner.x, contentBottom + gap, inner.width, barHeight);
            DrawBar(
                hpRect,
                player.Health.Current / hpMaximum,
                new Color(0.72f, 0.06f, 0.035f, 1f),
                $"{player.Health.Current:0} / {hpMaximum:0}");

            var resourceRect = new Rect(inner.x, hpRect.yMax + gap, inner.width, barHeight);
            if (resources != null && resources.Has(ResourceIds.Mana))
            {
                float maximum = Mathf.Max(1, resources.Maximum(ResourceIds.Mana));
                DrawBar(
                    resourceRect,
                    resources.Current(ResourceIds.Mana) / maximum,
                    new Color(0.08f, 0.30f, 0.80f, 1f),
                    $"{resources.Current(ResourceIds.Mana):0} / {maximum:0}");
            }
            else
            {
                DrawBar(resourceRect, 0, new Color(0.18f, 0.18f, 0.18f, 1f), "No class resource");
            }

            HudTooltip.DrawCurrent();
        }

        private void DrawEquipmentColumn(
            float x,
            float y,
            float width,
            float availableHeight,
            EquipmentSlot[] slots)
        {
            float gap = Theme.gap;
            float height = Mathf.Min(width, (availableHeight - gap * (slots.Length - 1)) / slots.Length);

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                var rect = new Rect(x, y + i * (height + gap), width, height);
                string label = SlotAbbreviation(slot);
                string tooltip = slot.ToString();
                ItemDefinition definition = null;

                var equipped = equipment?.Slots.FirstOrDefault(value => value.slotId == slot.ToString());
                if (equipped != null && inventory != null)
                {
                    var item = inventory.Find(equipped.instanceId);
                    definition = item == null ? null : inventory.Resolve(item.definitionId);
                    if (definition != null)
                    {
                        label = definition.displayName.Length <= 3
                            ? definition.displayName
                            : definition.displayName.Substring(0, 3);
                        tooltip = HudTooltip.Item(definition, slot.ToString());
                    }
                }

                bool hovered = rect.Contains(Event.current.mousePosition);
                HudChrome.DrawSlot(rect, Theme, equipped != null, hovered);

                string finalTooltip = equipped != null
                    ? tooltip + " — click to unequip"
                    : tooltip;

                if (equipped != null && !(HudLayoutManager.Current?.EditMode ?? false))
                {
                    if (GUI.Button(rect, new GUIContent("", finalTooltip), GUIStyle.none))
                        equipment.Unequip(slot.ToString());
                }
                else
                {
                    GUI.Label(rect, new GUIContent("", finalTooltip), GUIStyle.none);
                }

                if (!ItemIconGUI.Draw(rect, definition))
                    GUI.Label(rect, label, HudChrome.SlotTextStyle(Theme, true));
            }
        }

        private static string SlotAbbreviation(EquipmentSlot slot) => slot switch
        {
            EquipmentSlot.Head => "Hd",
            EquipmentSlot.Shoulders => "Sh",
            EquipmentSlot.Chest => "Ch",
            EquipmentSlot.Hands => "Gl",
            EquipmentSlot.Legs => "Lg",
            EquipmentSlot.Feet => "Ft",
            EquipmentSlot.MainHand => "MH",
            EquipmentSlot.OffHand => "OH",
            EquipmentSlot.Ring => "Rg",
            EquipmentSlot.Amulet => "Am",
            EquipmentSlot.Back => "Bk",
            _ => "?"
        };
    }
}
