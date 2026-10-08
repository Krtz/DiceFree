using System.Text;
using DiceFree.Items;
using UnityEngine;

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
            Append(text, stats.attributes.vitality, "VIT");
            Append(text, stats.attributes.strength, "STR");
            Append(text, stats.attributes.agility, "AGI");
            Append(text, stats.attributes.intelligence, "INT");
            Append(text, stats.attributes.spirit, "SPI");
            Append(text, stats.physicalDefense, "Physical Defense");
            Append(text, stats.magicalDefense, "Magical Defense");
            Append(text, stats.attackSpeedPercent, "% Attack Speed");
            Append(text, stats.movementSpeedPercent, "% Move Speed");
            Append(text, stats.basicAttackFlatBonus, "Basic Attack Damage (min and max)");

            if (!string.IsNullOrWhiteSpace(definition.flavor))
                text.AppendLine().AppendLine().Append(definition.flavor.Trim());

            return text.ToString();
        }

        public static void DrawCurrent()
        {
            string tooltip = GUI.tooltip;
            if (string.IsNullOrWhiteSpace(tooltip)) return;

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

            float width = Mathf.Min(340f, Mathf.Max(190f, style.CalcSize(content).x + 20f));
            float height = Mathf.Min(270f, style.CalcHeight(content, width) + 8f);
            var mouse = Event.current.mousePosition;
            float x = Mathf.Min(Screen.width - width - 8, mouse.x + 18);
            float y = Mathf.Min(Screen.height - height - 8, mouse.y + 18);
            var rect = new Rect(x, y, width, height);
            HudChrome.DrawPanel(rect, theme);
            GUI.Label(rect, content, style);
        }

        private static void Append(StringBuilder text, float value, string label)
        {
            if (Mathf.Approximately(value, 0f)) return;
            text.AppendLine().Append(value >= 0 ? "+" : "")
                .Append(value.ToString("0.##")).Append(" ").Append(label);
        }
    }
}
