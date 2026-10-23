using System;
using System.Linq;
using DiceFree.Gameplay;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class CodexPanel : CustomizableHudWidget
    {
        private enum Tab
        {
            Monsters,
            Dungeons
        }

        [SerializeField] private CodexProgression codex;
        private bool open;
        private Tab tab;
        private Vector2 scroll;

        public override string LayoutId => "codex";
        public override string DisplayName => "Codex";
        public override Rect DefaultNormalizedBounds => new(0.20f, 0.14f, 0.52f, 0.60f);
        public override Vector2 MinimumPixelSize => new(480f, 320f);
        public override bool BlocksPointer => open;

        public bool Open => open;

        private void Awake()
        {
            codex ??= GetComponent<CodexProgression>();
        }

        public void Configure(CodexProgression value) => codex = value;
        public void Show() => open = true;
        public void Close() => open = false;

        private void OnGUI()
        {
            if (!open || codex == null || HudPointerBlocker.ModalOpen) return;

            Rect panel = Bounds;
            DrawPanel(panel);
            // The Codex opens as a bound volume: a dark leather cover, two
            // paper leaves and a visible center crease, while retaining the
            // existing responsive HUD bounds and contents.
            HudChrome.Fill(new Rect(panel.x+5,panel.y+5,panel.width-10,panel.height-10),
                new Color(.18f,.105f,.055f,.98f));
            Rect inner = new(panel.x+16,panel.y+14,panel.width-32,panel.height-28);
            HudChrome.Fill(inner,new Color(.79f,.70f,.51f,.98f));
            HudChrome.Fill(new Rect(inner.center.x-3,inner.y+7,6,inner.height-14),
                new Color(.27f,.17f,.10f,.85f));
            var pageTint=GUI.color;
            GUI.color=new Color(.92f,.86f,.70f,.94f);
            GUI.DrawTexture(new Rect(inner.x+8,inner.y+8,inner.width*.5f-14,inner.height-16),Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(inner.center.x+7,inner.y+8,inner.width*.5f-15,inner.height-16),Texture2D.whiteTexture);
            GUI.color=pageTint;
            var contentPage=new Rect(inner.x+18,inner.y+14,inner.width-36,inner.height-28);

            const float headerHeight = 30f;
            var header = new Rect(contentPage.x, contentPage.y, contentPage.width, headerHeight);
            GUI.Label(header,"THE ECHO CODEX",new GUIStyle(GUI.skin.label)
            {fontStyle=FontStyle.Bold,fontSize=18,alignment=TextAnchor.MiddleCenter,
                normal={textColor=new Color(.25f,.14f,.07f)}});
            if (GUI.Button(
                    new Rect(header.xMax - 64f, header.y + 3f, 60f, header.height - 6f),
                    "Close",
                    new GUIStyle(GUI.skin.button)))
                open = false;

            float tabsY = header.yMax + Theme.gap;
            float tabWidth = 110f;
            var monsters = new Rect(inner.x, tabsY, tabWidth, 26f);
            var dungeons = new Rect(monsters.xMax + Theme.gap, tabsY, tabWidth, 26f);

            if (GUI.Toggle(monsters, tab == Tab.Monsters, "Monsters", "Button"))
                tab = Tab.Monsters;
            if (GUI.Toggle(dungeons, tab == Tab.Dungeons, "Dungeons", "Button"))
                tab = Tab.Dungeons;

            var content = new Rect(
                contentPage.x,
                monsters.yMax + Theme.gap,
                contentPage.width,
                contentPage.yMax - monsters.yMax - Theme.gap);

            GUILayout.BeginArea(content);
            scroll = GUILayout.BeginScrollView(scroll);
            if (tab == Tab.Monsters) DrawMonsters();
            else DrawDungeons();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawMonsters()
        {
            var monsters = codex.Monsters;
            if (monsters.Length == 0)
            {
                GUILayout.Label("No monsters discovered yet.");
                GUILayout.Label("Kill an exact monster/variant to add it to the Codex.");
                return;
            }

            foreach (var monster in monsters.OrderBy(value => value.monsterId, StringComparer.Ordinal))
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(monster.monsterId, new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold
                });
                GUILayout.Label("Kills: " + monster.kills);

                if (monster.drops?.Length > 0)
                {
                    GUILayout.Label("Observed drops:");
                    foreach (var drop in monster.drops.OrderBy(value => value.dropId, StringComparer.Ordinal))
                        GUILayout.Label("  " + drop.dropId + "  ×" + drop.timesSeen);
                }

                if (monster.mechanics?.Length > 0)
                {
                    GUILayout.Label("Witnessed mechanics:");
                    foreach (var mechanic in monster.mechanics.OrderBy(value => value.mechanicId, StringComparer.Ordinal))
                        GUILayout.Label("  " + mechanic.mechanicId);
                }

                GUILayout.EndVertical();
            }
        }

        private void DrawDungeons()
        {
            var dungeons = codex.Dungeons;
            if (dungeons.Length == 0)
            {
                GUILayout.Label("No dungeon discoveries yet.");
                return;
            }

            foreach (var dungeon in dungeons.OrderBy(value => value.dungeonId, StringComparer.Ordinal))
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(dungeon.dungeonId, new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold
                });

                GUILayout.Label(
                    $"Clears {dungeon.clears}   Wipes {dungeon.wipes}   Player deaths {dungeon.playerDeaths}");
                GUILayout.Label(
                    $"Solo clears {dungeon.soloClears}   No-death clears {dungeon.noDeathClears}");

                if (dungeon.fastestClears?.Length > 0)
                {
                    GUILayout.Label("Fastest clears:");
                    foreach (var fastest in dungeon.fastestClears.OrderBy(value => value.partySize))
                        GUILayout.Label($"  Party {fastest.partySize}: {fastest.seconds:0.00}s");
                }

                if (dungeon.completedModes?.Length > 0)
                    GUILayout.Label("Modes: " + string.Join(", ", dungeon.completedModes));
                if (dungeon.lootDiscoveries?.Length > 0)
                    GUILayout.Label("Loot seen: " + string.Join(", ", dungeon.lootDiscoveries));
                if (dungeon.secretDiscoveries?.Length > 0)
                    GUILayout.Label("Secrets: " + string.Join(", ", dungeon.secretDiscoveries));

                if (dungeon.bossKills?.Length > 0)
                {
                    GUILayout.Label("Boss kills:");
                    foreach (var boss in dungeon.bossKills.OrderBy(value => value.bossId, StringComparer.Ordinal))
                        GUILayout.Label("  " + boss.bossId + " ×" + boss.kills);
                }

                GUILayout.EndVertical();
            }
        }
    }
}
