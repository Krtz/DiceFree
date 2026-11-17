using System.Text;
using DiceFree.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    public static class HudTooltip
    {
        public static string Item(ItemDefinition definition, string equippedSlot = null)
        {
            if (definition == null) return string.Empty;

            var text = new StringBuilder();
            text.AppendLine(definition.displayName);
            text.Append("Item level ").Append(definition.itemLevel)
                .Append(" · ").Append(definition.slot);

            if (!string.IsNullOrWhiteSpace(equippedSlot))
                text.AppendLine().Append("Equipped: ").Append(equippedSlot);

            var stats = definition.stats;
            if(definition.slot==EquipmentSlot.MainHand&&definition.handsRequired==2)text.AppendLine().Append("Two-handed weapon");
            Append(text, stats.attributes.vitality, "VIT");
            Append(text, stats.attributes.strength, "STR");
            Append(text, stats.attributes.agility, "AGI");
            Append(text, stats.attributes.intelligence, "INT");
            Append(text, stats.attributes.spirit, "SPI");
            Append(text, stats.physicalDefense, "Physical Defense");
            Append(text, stats.magicalDefense, "Magical Defense");
            Append(text, stats.attackSpeedPercent, "% Attack Speed");
            Append(text, stats.movementSpeedPercent, "% Move Speed");
            Append(text, stats.regeneration, "Health regeneration");
            Append(text, stats.basicAttackFlatBonus, "Basic Attack Damage (min and max)");
            Append(text, stats.basicAttackMaximumBonus, "Maximum basic attack damage");

            if (!string.IsNullOrWhiteSpace(definition.flavor))
                text.AppendLine().AppendLine().Append(definition.flavor.Trim());

            if(definition.consumable)
            {
                text.AppendLine().Append("Use:");
                if(definition.restoreHealth>0)text.Append(" restore up to ").Append(definition.restoreHealth.ToString("0.#")).Append(" health");
                if(definition.restoreResourceAmount>0)text.Append(" restore up to ").Append(definition.restoreResourceAmount.ToString("0.#")).Append(" ").Append(definition.restoreResourceId);
            }

            return text.ToString();
        }

        // Input System coordinates are bottom-left-origin screen pixels while
        // IMGUI uses top-left-origin coordinates. When a mouse device exists,
        // use its live position even if it is outside the game window. Falling
        // back to Event.current in that case can reuse an old mouse event and
        // leave a tooltip stuck on the last hovered control.
        public static Vector2 CurrentGuiPointerPosition()
        {
            if(Mouse.current!=null)
            {
                Vector2 pointer=Mouse.current.position.ReadValue();
                return new Vector2(pointer.x,Screen.height-pointer.y);
            }
            return Event.current==null ? new Vector2(-10000,-10000) :
                Event.current.mousePosition;
        }

        public static void DrawAt(string tooltip,Rect anchor)
        {
            if (string.IsNullOrWhiteSpace(tooltip) ||
                Event.current.type!=EventType.Repaint) return;

            var content = new GUIContent(tooltip);
            HudThemeMetrics theme =
                HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
                richText = false,
                padding = new RectOffset(9, 9, 8, 8),
                normal = { textColor = theme.textTint }
            };

            float width = Mathf.Min(400f, Mathf.Max(235f, style.CalcSize(content).x + 22f));
            float height = Mathf.Min(Screen.height - 20f,
                Mathf.Min(470f, style.CalcHeight(content, width - 12f) + 14f));
            float x = anchor.center.x-width*.5f;
            float y = anchor.y-height-8f;
            if(y<8f)y=anchor.yMax+8f;
            x=Mathf.Max(8f,Mathf.Min(Screen.width-width-8f,x));
            y=Mathf.Max(8f,Mathf.Min(Screen.height-height-8f,y));
            var rect = new Rect(x, y, width, height);
            int oldDepth=GUI.depth;
            GUI.depth=-180;
            HudChrome.DrawPanel(rect, theme);
            GUI.Label(rect, content, style);
            GUI.depth=oldDepth;
        }

        private static void Append(StringBuilder text, float value, string label)
        {
            if (Mathf.Approximately(value, 0f)) return;
            text.AppendLine().Append(value >= 0 ? "+" : "")
                .Append(value.ToString("0.##")).Append(" ").Append(label);
        }
    }
}
