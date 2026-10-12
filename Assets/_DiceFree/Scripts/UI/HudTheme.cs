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

        private static HudThemeMetrics Pack(string id, string name,
            HudFrameStyle frame, Color accent)
        {
            var panel=new Color(accent.r*.095f,accent.g*.082f,accent.b*.073f,.93f);
            var header=new Color(accent.r*.28f,accent.g*.20f,accent.b*.18f,.98f);
            var slot=new Color(accent.r*.055f,accent.g*.048f,accent.b*.040f,.97f);
            return new HudThemeMetrics(id,name,frame,4f,5f,3f,3f,
                panel,accent,slot,header,new Color(.95f,.93f,.87f,1f));
        }

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
                new Color(0.93f, 0.93f, 0.94f, 1f)),

            Pack("pack-bloodlines-dark-ui","Bloodlines",
                HudFrameStyle.Obsidian,new Color(0.80f,0.18f,0.22f,1f)),
            Pack("pack-game-input-controller-icons-free","Controller",
                HudFrameStyle.Minimal,new Color(0.38f,0.67f,0.91f,1f)),
            Pack("pack-dark-brown-gui-kit","Dark Brown",
                HudFrameStyle.Ironwood,new Color(0.63f,0.42f,0.25f,1f)),
            Pack("pack-fantasy-wooden-gui-free","Fantasy Wood",
                HudFrameStyle.Ironwood,new Color(0.78f,0.54f,0.24f,1f)),
            Pack("pack-free-rpg-fantasy-spell-icons","RPG Spell Icons",
                HudFrameStyle.Arcane,new Color(0.68f,0.43f,0.98f,1f)),
            Pack("pack-battle-skill-icon","Battle Skills",
                HudFrameStyle.Ironwood,new Color(0.91f,0.43f,0.25f,1f)),
            Pack("pack-basic-ui-elements-concrete","Concrete",
                HudFrameStyle.Stone,new Color(0.72f,0.71f,0.66f,1f)),
            Pack("pack-basic-ui-elements-metal","Metal",
                HudFrameStyle.Obsidian,new Color(0.71f,0.75f,0.79f,1f)),
            Pack("pack-camo-ui-elements-woodland","Woodland Camo",
                HudFrameStyle.Ironwood,new Color(0.43f,0.60f,0.34f,1f)),
            Pack("pack-steampunkui","Steampunk",
                HudFrameStyle.Ironwood,new Color(0.80f,0.56f,0.23f,1f)),
            Pack("pack-pixel-hud-ui-free-kit","Pixel HUD",
                HudFrameStyle.Minimal,new Color(0.55f,0.65f,0.83f,1f)),
            Pack("pack-2d-rpg-button-7","RPG Buttons",
                HudFrameStyle.Ironwood,new Color(0.87f,0.64f,0.28f,1f)),
            Pack("pack-simple-ui-elements","Simple Elements",
                HudFrameStyle.Stone,new Color(0.73f,0.75f,0.80f,1f)),
            Pack("pack-color-manager","Color Manager",
                HudFrameStyle.Arcane,new Color(0.39f,0.83f,0.78f,1f)),
            Pack("pack-simple-fantasy-gui","Simple Fantasy",
                HudFrameStyle.Stone,new Color(0.89f,0.69f,0.30f,1f)),
            Pack("pack-elemental-meters","Elemental Meters",
                HudFrameStyle.Arcane,new Color(0.38f,0.79f,0.91f,1f)),
            Pack("pack-sweet-land-gui","Sweet Land",
                HudFrameStyle.Stone,new Color(0.97f,0.73f,0.43f,1f)),
            Pack("pack-gui-parts","GUI Parts",
                HudFrameStyle.Stone,new Color(0.85f,0.66f,0.35f,1f)),
            Pack("pack-spellbook-preface","Spellbook",
                HudFrameStyle.Ironwood,new Color(0.71f,0.43f,0.19f,1f)),
            Pack("pack-stone-ui","Stone UI",
                HudFrameStyle.Stone,new Color(0.69f,0.71f,0.74f,1f)),
            Pack("pack-adventure-ui-kit","Adventure",
                HudFrameStyle.Ironwood,new Color(0.95f,0.63f,0.26f,1f)),
            Pack("pack-colorful-ui-kit","Colorful",
                HudFrameStyle.Arcane,new Color(0.43f,0.77f,0.97f,1f)),
            Pack("pack-jelly-pack-ui","Jelly",
                HudFrameStyle.Arcane,new Color(0.94f,0.52f,0.84f,1f)),
            Pack("pack-soft-touch-ui-kit-demo","Soft Touch",
                HudFrameStyle.Minimal,new Color(0.61f,0.78f,0.91f,1f)),
            Pack("pack-spooky-horror-ui-kit","Spooky Horror",
                HudFrameStyle.Obsidian,new Color(0.81f,0.23f,0.30f,1f)),
            Pack("pack-magic-spells-40-fantasy-icons-512x512-hd","Magic Spell Icons",
                HudFrameStyle.Arcane,new Color(0.59f,0.43f,0.96f,1f)),
            Pack("pack-shields-40-fantasy-icons-512x512-hd","Shields",
                HudFrameStyle.Obsidian,new Color(0.61f,0.73f,0.88f,1f)),
            Pack("pack-wooden-gui-ui-animation-extension","Animated Wood",
                HudFrameStyle.Ironwood,new Color(0.77f,0.51f,0.30f,1f)),
            Pack("pack-ui-examples-ugui-sample-project","Unity Examples",
                HudFrameStyle.Minimal,new Color(0.45f,0.77f,0.86f,1f)),
            Pack("pack-free-ui-pack","Free UI",
                HudFrameStyle.Stone,new Color(0.40f,0.84f,0.75f,1f))
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
