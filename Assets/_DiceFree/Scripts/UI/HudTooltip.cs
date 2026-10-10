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

        public static void DrawCurrent() =>
            DrawAt(GUI.tooltip,Event.current.mousePosition);

        public static void DrawAt(string tooltip,Vector2 position)
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
            float x = Mathf.Max(8,Mathf.Min(Screen.width - width - 8, position.x + 18));
            float y = Mathf.Max(8,Mathf.Min(Screen.height - height - 8, position.y + 18));
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
