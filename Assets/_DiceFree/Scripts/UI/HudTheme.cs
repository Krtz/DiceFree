using System;
using System.Linq;
using UnityEngine;

namespace DiceFree.UI
{
    public enum HudFrameStyle
    {
        Stone,
        Ironwood,
        Arcane,
        Obsidian,
        Minimal
    }

    public readonly struct HudThemeMetrics
    {
        public readonly string id;
        public readonly string displayName;
        public readonly HudFrameStyle frameStyle;
        public readonly float outerPadding;
        public readonly float innerPadding;
        public readonly float gap;
        public readonly float border;
        public readonly Color panelTint;
        public readonly Color accentTint;
        public readonly Color slotTint;
        public readonly Color headerTint;
        public readonly Color textTint;

        public HudThemeMetrics(
            string id,
            string displayName,
            HudFrameStyle frameStyle,
            float outerPadding,
            float innerPadding,
            float gap,
            float border,
            Color panelTint,
            Color accentTint,
            Color slotTint,
            Color headerTint,
            Color textTint)
        {
            this.id = id;
            this.displayName = displayName;
            this.frameStyle = frameStyle;
            this.outerPadding = outerPadding;
            this.innerPadding = innerPadding;
            this.gap = gap;
            this.border = border;
            this.panelTint = panelTint;
            this.accentTint = accentTint;
            this.slotTint = slotTint;
            this.headerTint = headerTint;
            this.textTint = textTint;
        }

        public HudThemeMetrics WithAlpha(float alpha)
        {
            alpha = Mathf.Clamp01(alpha);
            return new HudThemeMetrics(
                id,
                displayName,
                frameStyle,
                outerPadding,
                innerPadding,
                gap,
                border,
                new Color(panelTint.r, panelTint.g, panelTint.b, panelTint.a * alpha),
                new Color(accentTint.r, accentTint.g, accentTint.b, accentTint.a * alpha),
                new Color(slotTint.r, slotTint.g, slotTint.b, slotTint.a * alpha),
                new Color(headerTint.r, headerTint.g, headerTint.b, headerTint.a * alpha),
                new Color(textTint.r, textTint.g, textTint.b, textTint.a * alpha));
        }

        public HudThemeMetrics WithUserTint(Color tint)
        {
            tint.a = 1f;
            Color accent = Color.Lerp(accentTint, tint, 0.78f);
            accent.a = accentTint.a;

            Color panelTarget = new(
                Mathf.Lerp(panelTint.r, tint.r * 0.18f, 0.55f),
                Mathf.Lerp(panelTint.g, tint.g * 0.18f, 0.55f),
                Mathf.Lerp(panelTint.b, tint.b * 0.18f, 0.55f),
                panelTint.a);
            Color slotTarget = new(
                Mathf.Lerp(slotTint.r, tint.r * 0.12f, 0.50f),
                Mathf.Lerp(slotTint.g, tint.g * 0.12f, 0.50f),
                Mathf.Lerp(slotTint.b, tint.b * 0.12f, 0.50f),
                slotTint.a);
            Color headerTarget = Color.Lerp(headerTint, tint, 0.34f);
            headerTarget.a = headerTint.a;

            return new HudThemeMetrics(
                id,
                displayName,
                frameStyle,
                outerPadding,
                innerPadding,
                gap,
                border,
                panelTarget,
                accent,
                slotTarget,
                headerTarget,
                textTint);
        }
    }

    public static class HudThemes
    {
        public const string CompactStone = "wc3-compact";
        public const string Ironwood = "ironwood";
        public const string ArcaneGlass = "arcane-glass";
        public const string Obsidian = "obsidian";
        public const string Minimal = "minimal";

        private static readonly HudThemeMetrics[] themes =
        {
            new(
                CompactStone,
                "Compact Stone",
                HudFrameStyle.Stone,
                4f, 5f, 3f, 3f,
                new Color(0.055f, 0.052f, 0.047f, 0.95f),
                new Color(0.73f, 0.56f, 0.19f, 1f),
                new Color(0.025f, 0.025f, 0.023f, 0.96f),
                new Color(0.15f, 0.125f, 0.075f, 0.98f),
                new Color(0.92f, 0.90f, 0.84f, 1f)),

            new(
                Ironwood,
                "Ironwood",
                HudFrameStyle.Ironwood,
                5f, 6f, 4f, 4f,
                new Color(0.085f, 0.050f, 0.030f, 0.95f),
                new Color(0.72f, 0.42f, 0.16f, 1f),
                new Color(0.045f, 0.028f, 0.018f, 0.97f),
                new Color(0.22f, 0.105f, 0.045f, 0.98f),
                new Color(0.94f, 0.87f, 0.75f, 1f)),

            new(
                ArcaneGlass,
                "Arcane Glass",
                HudFrameStyle.Arcane,
                3f, 5f, 3f, 2f,
                new Color(0.025f, 0.045f, 0.085f, 0.84f),
                new Color(0.20f, 0.72f, 0.95f, 1f),
                new Color(0.018f, 0.035f, 0.075f, 0.90f),
                new Color(0.06f, 0.18f, 0.32f, 0.91f),
                new Color(0.86f, 0.96f, 1f, 1f)),

            new(
                Obsidian,
                "Obsidian",
                HudFrameStyle.Obsidian,
                4f, 5f, 3f, 3f,
                new Color(0.018f, 0.018f, 0.023f, 0.97f),
                new Color(0.68f, 0.70f, 0.76f, 1f),
                new Color(0.008f, 0.008f, 0.012f, 0.98f),
                new Color(0.065f, 0.065f, 0.080f, 0.99f),
                new Color(0.92f, 0.93f, 0.96f, 1f)),

            new(
                Minimal,
                "Minimal",
                HudFrameStyle.Minimal,
                1f, 3f, 2f, 1f,
                new Color(0.025f, 0.025f, 0.028f, 0.76f),
                new Color(0.74f, 0.76f, 0.80f, 0.92f),
                new Color(0.015f, 0.015f, 0.018f, 0.78f),
                new Color(0.05f, 0.05f, 0.055f, 0.80f),
                new Color(0.93f, 0.93f, 0.94f, 1f))
        };

        public static HudThemeMetrics[] All => themes.ToArray();

        public static HudThemeMetrics Get(string id) =>
            themes.FirstOrDefault(value => value.id == id).id == null
                ? themes[0]
                : themes.First(value => value.id == id);

        public static string Next(string id)
        {
            int index = Array.FindIndex(themes, value => value.id == id);
            if (index < 0) return themes[0].id;
            return themes[(index + 1) % themes.Length].id;
        }
    }
}
