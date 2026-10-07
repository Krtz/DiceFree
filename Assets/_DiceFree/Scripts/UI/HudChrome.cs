using UnityEngine;

namespace DiceFree.UI
{
    public static class HudChrome
    {
        public static void DrawPanel(Rect rect, HudThemeMetrics theme)
        {
            switch (theme.frameStyle)
            {
                case HudFrameStyle.Minimal:
                    Fill(rect, theme.panelTint);
                    Fill(new Rect(rect.x, rect.y, rect.width, Mathf.Max(1f, theme.border)), theme.accentTint);
                    return;

                case HudFrameStyle.Arcane:
                    Fill(rect, new Color(theme.accentTint.r, theme.accentTint.g, theme.accentTint.b, 0.34f));
                    Fill(Inset(rect, 2f), theme.panelTint);
                    Border(Inset(rect, 4f), 1f,
                        new Color(theme.accentTint.r, theme.accentTint.g, theme.accentTint.b, 0.50f));
                    CornerMarks(rect, theme.accentTint, 9f, 2f);
                    return;

                case HudFrameStyle.Ironwood:
                    Fill(rect, Darken(theme.accentTint, 0.36f, 0.98f));
                    Fill(Inset(rect, theme.border), theme.panelTint);
                    Border(Inset(rect, theme.border + 1f), 1f,
                        new Color(theme.accentTint.r, theme.accentTint.g, theme.accentTint.b, 0.48f));
                    Rivets(rect, theme.accentTint);
                    return;

                case HudFrameStyle.Obsidian:
                    Fill(rect, new Color(0.005f, 0.005f, 0.008f, 1f));
                    Border(rect, Mathf.Max(2f, theme.border), theme.accentTint);
                    Fill(Inset(rect, theme.border + 1f), theme.panelTint);
                    Fill(new Rect(rect.x + theme.border + 2f, rect.y + theme.border + 2f,
                        Mathf.Max(0, rect.width - theme.border * 2f - 4f), 2f),
                        new Color(theme.accentTint.r, theme.accentTint.g, theme.accentTint.b, 0.65f));
                    return;

                default:
                    Fill(rect, Darken(theme.accentTint, 0.28f, 1f));
                    Fill(Inset(rect, theme.border), theme.panelTint);
                    Border(Inset(rect, theme.border + 1f), 1f,
                        new Color(theme.accentTint.r, theme.accentTint.g, theme.accentTint.b, 0.48f));
                    Bevel(rect, theme);
                    return;
            }
        }

        public static void DrawHeader(Rect rect, HudThemeMetrics theme, string text)
        {
            Fill(rect, theme.headerTint);
            Fill(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f),
                new Color(theme.accentTint.r, theme.accentTint.g, theme.accentTint.b, 0.72f));

            var style = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = theme.textTint },
                padding = new RectOffset(6, 4, 0, 0)
            };
            GUI.Label(rect, text, style);
        }

        public static void DrawSlot(Rect rect, HudThemeMetrics theme, bool occupied, bool hovered)
        {
            Color edge = hovered
                ? Lighten(theme.accentTint, 0.20f)
                : occupied
                    ? theme.accentTint
                    : new Color(theme.accentTint.r, theme.accentTint.g, theme.accentTint.b, 0.42f);

            Fill(rect, new Color(edge.r, edge.g, edge.b, occupied ? 0.82f : 0.48f));
            Fill(Inset(rect, Mathf.Max(1f, theme.border * 0.55f)), theme.slotTint);

            if (theme.frameStyle == HudFrameStyle.Arcane && occupied)
                Border(Inset(rect, 3f), 1f, new Color(edge.r, edge.g, edge.b, 0.48f));
            else if (theme.frameStyle == HudFrameStyle.Ironwood)
                Fill(new Rect(rect.x + 3f, rect.y + 3f, 2f, 2f), edge);
        }

        public static GUIStyle SlotTextStyle(HudThemeMetrics theme, bool compact = false) =>
            new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = compact ? Mathf.Max(9, GUI.skin.label.fontSize - 1) : GUI.skin.label.fontSize,
                normal = { textColor = theme.textTint },
                wordWrap = true
            };

        public static GUIStyle HeaderButtonStyle(HudThemeMetrics theme)
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = theme.textTint },
                hover = { textColor = Color.white },
                active = { textColor = Color.white }
            };
            return style;
        }

        public static void Fill(Rect rect, Color color)
        {
            if (rect.width <= 0 || rect.height <= 0) return;
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        public static void Border(Rect rect, float thickness, Color color)
        {
            thickness = Mathf.Max(1f, thickness);
            Fill(new Rect(rect.x, rect.y, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.y, thickness, rect.height), color);
            Fill(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static Rect Inset(Rect rect, float amount) =>
            new(
                rect.x + amount,
                rect.y + amount,
                Mathf.Max(0f, rect.width - amount * 2f),
                Mathf.Max(0f, rect.height - amount * 2f));

        private static void Bevel(Rect rect, HudThemeMetrics theme)
        {
            Color light = new(theme.accentTint.r, theme.accentTint.g, theme.accentTint.b, 0.36f);
            Color dark = Darken(theme.accentTint, 0.18f, 0.48f);
            Fill(new Rect(rect.x + theme.border, rect.y + theme.border, rect.width - theme.border * 2f, 1f), light);
            Fill(new Rect(rect.x + theme.border, rect.y + theme.border, 1f, rect.height - theme.border * 2f), light);
            Fill(new Rect(rect.x + theme.border, rect.yMax - theme.border - 1f, rect.width - theme.border * 2f, 1f), dark);
            Fill(new Rect(rect.xMax - theme.border - 1f, rect.y + theme.border, 1f, rect.height - theme.border * 2f), dark);
        }

        private static void Rivets(Rect rect, Color accent)
        {
            Color rivet = Lighten(accent, 0.18f);
            float size = 3f;
            Fill(new Rect(rect.x + 5f, rect.y + 5f, size, size), rivet);
            Fill(new Rect(rect.xMax - 8f, rect.y + 5f, size, size), rivet);
            Fill(new Rect(rect.x + 5f, rect.yMax - 8f, size, size), rivet);
            Fill(new Rect(rect.xMax - 8f, rect.yMax - 8f, size, size), rivet);
        }

        private static void CornerMarks(Rect rect, Color accent, float length, float thickness)
        {
            Color value = new(accent.r, accent.g, accent.b, 0.82f);
            Fill(new Rect(rect.x, rect.y, length, thickness), value);
            Fill(new Rect(rect.x, rect.y, thickness, length), value);
            Fill(new Rect(rect.xMax - length, rect.y, length, thickness), value);
            Fill(new Rect(rect.xMax - thickness, rect.y, thickness, length), value);
            Fill(new Rect(rect.x, rect.yMax - thickness, length, thickness), value);
            Fill(new Rect(rect.x, rect.yMax - length, thickness, length), value);
            Fill(new Rect(rect.xMax - length, rect.yMax - thickness, length, thickness), value);
            Fill(new Rect(rect.xMax - thickness, rect.yMax - length, thickness, length), value);
        }

        private static Color Darken(Color color, float amount, float alpha)
        {
            float scale = Mathf.Clamp01(1f - amount);
            return new Color(color.r * scale, color.g * scale, color.b * scale, alpha);
        }

        private static Color Lighten(Color color, float amount) =>
            new(
                Mathf.Lerp(color.r, 1f, amount),
                Mathf.Lerp(color.g, 1f, amount),
                Mathf.Lerp(color.b, 1f, amount),
                color.a);
    }
}
