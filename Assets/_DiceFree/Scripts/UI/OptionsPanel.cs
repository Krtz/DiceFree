using DiceFree.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    [DefaultExecutionOrder(-10000)]
    public sealed class OptionsPanel : HudWidget
    {
        public enum OptionsTab { Gameplay, Audio, HUD, Keybinds, Session }
        public OptionsTab SelectedTab { get; private set; } = OptionsTab.Gameplay;
        private readonly Vector2[] tabScroll = new Vector2[5];
        private InputBindings bindings;
        private GameplayPreferences gameplayPreferences;
        private IPlayerAudioSettings audioSettings;
        public System.Func<bool> ReturnToStartMenu { get; set; }
        private bool confirmReturn;
        private string returnFeedback = "";
        public bool RequestReturnToStartMenu()
        {
            bool saved = ReturnToStartMenu?.Invoke()==true;
            if(!saved)returnFeedback="Could not save progress. Return cancelled.";
            return saved;
        }
        public bool Open { get; private set; }
        private Rect ButtonBounds => new Rect(Mathf.Max(10,Screen.width-54),
            Mathf.Max(10,Screen.height-54),44,44);
        public override Rect Bounds => Open
            ? new Rect(0,0,Screen.width,Screen.height) : ButtonBounds;
        private void Awake()
        {
            bindings=InputBindings.Current;
            gameplayPreferences=GameplayPreferences.Current;
            foreach(var component in GetComponents<MonoBehaviour>())
                if(component is IPlayerAudioSettings audio){audioSettings=audio;break;}
        }
        protected override void OnDisable(){Close();base.OnDisable();}
        public void Show()
        {
            Open=true;bindings.SetModal(true);
        }
        public void SelectTab(OptionsTab tab)
        {
            if(bindings.Listening)bindings.CancelRebind();
            SelectedTab=tab;
        }
        public void Close()
        {
            if(bindings==null)return;
            bindings.CancelRebind();
            PlayerPrefs.Save();
            Open=false;confirmReturn=false;
            bindings.SetModal(false);
        }
        private void Update()
        {
            bindings.Tick();
            if(bindings.Listening||bindings.LastRebindFrame==Time.frameCount)return;
            if(Open&&bindings.Action("UI/Close options").WasPressedThisFrame())
            {Close();return;}
            if(bindings.Action("UI/Options menu toggle").WasPressedThisFrame())
            {if(Open)Close();else Show();}
        }
        private void OnGUI()
        {
            GUI.depth=-100;
            if(!Open)
            {
                if(HudLayoutManager.Current==null && GUI.Button(ButtonBounds,"Options"))
                    Show();
                return;
            }
            HudChrome.Fill(Bounds,new Color(.015f,.020f,.030f,.92f));
            float width=Mathf.Min(920,Screen.width-24);
            float height=Mathf.Max(150,Screen.height-24);
            Rect panel=new((Screen.width-width)*.5f,12,width,height);
            var hudTheme=HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);
            HudChrome.DrawPanel(panel,hudTheme);
            var inside=new Rect(panel.x+10,panel.y+10,
                Mathf.Max(0,panel.width-20),Mathf.Max(0,panel.height-20));
            GUILayout.BeginArea(inside);
            GUILayout.BeginHorizontal();
            GUILayout.Label("DiceFree  /  Options",GUILayout.ExpandWidth(true));
            if(!bindings.Listening && GUILayout.Button("Close [Escape]",GUILayout.Width(140)))Close();
            GUILayout.EndHorizontal();
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            foreach(OptionsTab tab in System.Enum.GetValues(typeof(OptionsTab)))
            {
                bool active=tab==SelectedTab;
                if(GUILayout.Toggle(active,tab==OptionsTab.HUD?"HUD / Themes":tab.ToString(),
                        "Button",GUILayout.MinHeight(34))&&!active)
                    SelectTab(tab);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(8);
            int n=(int)SelectedTab;
            tabScroll[n]=GUILayout.BeginScrollView(tabScroll[n],GUILayout.ExpandHeight(true));
            switch(SelectedTab)
            {
                case OptionsTab.Gameplay: DrawGameplay();break;
                case OptionsTab.Audio: DrawAudio();break;
                case OptionsTab.HUD:
                    if(HudLayoutManager.Current!=null)
                        DrawHudSettings(HudLayoutManager.Current);
                    else GUILayout.Label("HUD is not yet initialized.");
                    break;
                case OptionsTab.Keybinds: DrawKeybinds(panel.width);break;
                case OptionsTab.Session: DrawSession();break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        private void DrawGameplay()
        {
            GUILayout.Label("Gameplay");
            bool retaliate=GUILayout.Toggle(gameplayPreferences.AutoRetaliate,
                "Auto retaliate when attacked");
            if(retaliate!=gameplayPreferences.AutoRetaliate)
                gameplayPreferences.SetAutoRetaliate(retaliate);
            GUILayout.Label("Only retaliates while you aren't already attacking a selected target.");
            GUILayout.Space(12);
            GUILayout.Label("When a targeted skill is out of range:");
            GUILayout.BeginHorizontal();
            bool follow=gameplayPreferences.OutOfRangeSkillBehavior ==
                OutOfRangeSkillBehavior.WalkIntoRange;
            if(GUILayout.Toggle(follow,"Walk into cast range","Button")&&!follow)
                gameplayPreferences.SetOutOfRangeSkillBehavior(OutOfRangeSkillBehavior.WalkIntoRange);
            bool stay=gameplayPreferences.OutOfRangeSkillBehavior ==
                OutOfRangeSkillBehavior.DoNothing;
            if(GUILayout.Toggle(stay,"Do nothing","Button")&&!stay)
                gameplayPreferences.SetOutOfRangeSkillBehavior(OutOfRangeSkillBehavior.DoNothing);
            GUILayout.EndHorizontal();
            bool showRange=GUILayout.Toggle(gameplayPreferences.ShowCastRange,
                "Show cast range when targeting a skill");
            if(showRange!=gameplayPreferences.ShowCastRange)
                gameplayPreferences.SetShowCastRange(showRange);
        }
        private static float AudioSlider(string label,float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label,GUILayout.Width(160));
            float result=GUILayout.HorizontalSlider(value,0f,1f,
                GUILayout.MinWidth(120),GUILayout.ExpandWidth(true));
            GUILayout.Label(Mathf.RoundToInt(result*100f)+"%",GUILayout.Width(52));
            GUILayout.EndHorizontal();
            GUILayout.Space(12);
            return result;
        }
        private void DrawAudio()
        {
            GUILayout.Label("Audio Mixer");
            // The options UI is often on the HUD root while AudioDirector is
            // on the player. Looking only on this GameObject hid the controls.
            if(audioSettings==null)
                foreach(var candidate in FindObjectsByType<MonoBehaviour>())
                    if(candidate is IPlayerAudioSettings source){audioSettings=source;break;}
            if(audioSettings==null)
            {
                GUILayout.Label("Audio sources are unavailable in this scene.");
                return;
            }
            GUILayout.Label("Changes apply immediately and are saved for future sessions.");
            GUILayout.Space(8);
            float master=AudioSlider("Master volume",audioSettings.MasterLevel);
            float effects=AudioSlider("Sound effects",audioSettings.SfxLevel);
            float steps=AudioSlider("Footsteps",audioSettings.FootstepsLevel);
            float ambient=AudioSlider("Environment / ambience",audioSettings.AmbientLevel);
            float wildlife=AudioSlider("Wildlife sounds",audioSettings.WildlifeLevel);
            if(!Mathf.Approximately(master,audioSettings.MasterLevel) ||
               !Mathf.Approximately(effects,audioSettings.SfxLevel) ||
               !Mathf.Approximately(steps,audioSettings.FootstepsLevel) ||
               !Mathf.Approximately(ambient,audioSettings.AmbientLevel) ||
               !Mathf.Approximately(wildlife,audioSettings.WildlifeLevel))
                audioSettings.SetExtendedLevels(master,effects,ambient,steps,wildlife);
        }
        private void DrawKeybinds(float width)
        {
            GUILayout.Label("Keybinds");
            GUILayout.Label("Shared bindings are allowed and flagged below. "+
                "Gameplay and camera controls are paused while Options is open.");
            if(bindings.Listening)
            {
                GUILayout.Label("Listening: "+bindings.ListeningLabel+
                    ". Press a keyboard key or mouse button; Backspace cancels.");
                if(GUILayout.Button("Cancel rebinding"))bindings.CancelRebind();
            }
            GUILayout.Label(bindings.Status);
            GUILayout.Space(7);
            string lastMap=null;
            foreach(var entry in bindings.Entries)
            {
                // Kept in the input asset for old profiles, but the obsolete
                // allocation window is no longer a gameplay control.
                if(entry.Map=="UI" && entry.Label.StartsWith("Novice skills menu"))
                    continue;
                if(entry.Map!=lastMap)
                {
                    lastMap=entry.Map;
                    GUILayout.Space(11);GUILayout.Label(lastMap);
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label(entry.Label,GUILayout.Width(Mathf.Min(380,width*.53f)));
                GUI.enabled=!bindings.Listening;
                if(GUILayout.Button(entry.Display.Length==0?"Unbound":entry.Display))
                    bindings.BeginRebind(entry);
                GUI.enabled=true;
                GUILayout.EndHorizontal();
                string conflict=bindings.Conflict(entry);
                if(conflict.Length>0)GUILayout.Label(conflict);
            }
            GUILayout.Space(10);
            GUI.enabled=!bindings.Listening;
            if(GUILayout.Button("Reset ALL keybinds to defaults"))
                bindings.ResetDefaults();
            GUI.enabled=true;
        }
        private void DrawSession()
        {
            GUILayout.Label("Session");
            GUILayout.Label("Your progress is saved before returning to the start menu.");
            if(ReturnToStartMenu==null)
            {GUILayout.Label("Return to Start Menu is unavailable in this scene.");return;}
            if(!confirmReturn && GUILayout.Button("Save and Return to Start Menu"))
                confirmReturn=true;
            if(confirmReturn)
            {
                GUILayout.Label("Save progress and leave? Active dungeon runs will end.");
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Save and Return"))RequestReturnToStartMenu();
                if(GUILayout.Button("Cancel"))confirmReturn=false;
                GUILayout.EndHorizontal();
            }
            if(!string.IsNullOrEmpty(returnFeedback))GUILayout.Label(returnFeedback);
        }

        private static readonly string[] BarVisibilityChoices =
            { "Always", "Only when hurt", "Never" };
        private static OverheadHealthVisibility DrawOverheadSetting(
            string label,OverheadHealthVisibility value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label,GUILayout.Width(110));
            int next=GUILayout.SelectionGrid((int)value,BarVisibilityChoices,3,
                GUILayout.MinHeight(26));
            GUILayout.EndHorizontal();
            return (OverheadHealthVisibility)Mathf.Clamp(next,0,2);
        }
        private void DrawHudSettings(HudLayoutManager hud)
        {
            GUILayout.Label("HUD");
            GUILayout.Space(4);
            GUILayout.Label("Health bars above units");
            var self=DrawOverheadSetting("Your character",gameplayPreferences.SelfHealthBar);
            var allies=DrawOverheadSetting("Allies",gameplayPreferences.AllyHealthBar);
            var enemies=DrawOverheadSetting("Enemies",gameplayPreferences.EnemyHealthBar);
            gameplayPreferences.SetOverheadHealthVisibility(self,allies,enemies);
            GUILayout.Label("Only when hurt: visible while HP is below maximum. Different choices are saved between sessions.");
            GUILayout.Space(12);
            GUILayout.Label("Minimap orientation");
            GUILayout.BeginHorizontal();
            bool north=gameplayPreferences.MinimapOrientation==MinimapOrientation.NorthUp;
            if(GUILayout.Toggle(north,"North at top","Button")&&!north)
                gameplayPreferences.SetMinimapOrientation(MinimapOrientation.NorthUp);
            if(GUILayout.Toggle(!north,"Rotate with camera","Button")&&north)
                gameplayPreferences.SetMinimapOrientation(MinimapOrientation.CameraUp);
            GUILayout.EndHorizontal();
            GUILayout.Space(8);
            GUILayout.Label("Minimap compass rose");
            string[] compassChoices={"Off","Bottom left","Bottom right","Top left","Top right"};
            int compass=GUILayout.SelectionGrid((int)gameplayPreferences.MinimapCompassCorner,compassChoices,3);
            if(compass!=(int)gameplayPreferences.MinimapCompassCorner)
                gameplayPreferences.SetMinimapCompassCorner((MinimapCompassCorner)compass);
            GUILayout.Space(12);
            GUILayout.Label("Theme");

            var themes = HudThemes.All;
            int selectedIndex = System.Array.FindIndex(themes, theme => theme.id == hud.ThemeId);
            if (selectedIndex < 0) selectedIndex = 0;
            int nextIndex = GUILayout.SelectionGrid(selectedIndex,
                System.Array.ConvertAll(themes, theme => theme.displayName),
                Mathf.Clamp(Mathf.FloorToInt((Screen.width - 80f) / 175f), 2, 5),
                GUILayout.MinHeight(Mathf.CeilToInt(themes.Length / 4f) * 29f));
            if (nextIndex >= 0 && nextIndex < themes.Length &&
                nextIndex != selectedIndex) hud.SetTheme(themes[nextIndex].id);
            GUILayout.Label(HudThemeArtwork.HasArt(hud.ThemeId)
                ? "Using locally installed UI pack artwork for this theme."
                : "Using DiceFree's built-in procedural frame artwork.");

            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Edit HUD layout", GUILayout.Width(150)))
            {
                Close();
                hud.SetEditMode(true);
                return;
            }

            if (GUILayout.Button("Reset HUD layout", GUILayout.Width(150)))
                hud.ResetLayout();

            if (GUILayout.Button("Use theme colours", GUILayout.Width(150)))
                hud.ResetHudTint();
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label("HUD colour");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Gold")) hud.SetHudTint(new Color(0.82f, 0.61f, 0.18f));
            if (GUILayout.Button("Ruby")) hud.SetHudTint(new Color(0.80f, 0.20f, 0.18f));
            if (GUILayout.Button("Emerald")) hud.SetHudTint(new Color(0.20f, 0.70f, 0.35f));
            if (GUILayout.Button("Sapphire")) hud.SetHudTint(new Color(0.20f, 0.48f, 0.92f));
            if (GUILayout.Button("Violet")) hud.SetHudTint(new Color(0.62f, 0.28f, 0.92f));
            if (GUILayout.Button("Cyan")) hud.SetHudTint(new Color(0.18f, 0.80f, 0.86f));
            GUILayout.EndHorizontal();

            Color tint = hud.HudTint;
            float red = DrawColorSlider("R", tint.r);
            float green = DrawColorSlider("G", tint.g);
            float blue = DrawColorSlider("B", tint.b);
            var edited = new Color(red, green, blue, 1f);

            if (Vector3.SqrMagnitude(
                    new Vector3(edited.r - tint.r, edited.g - tint.g, edited.b - tint.b)) > 0.000001f)
                hud.SetHudTint(edited);

            Rect swatch = GUILayoutUtility.GetRect(1f, 20f, GUILayout.ExpandWidth(true));
            HudChrome.Fill(swatch, hud.CustomTintEnabled ? hud.HudTint : hud.Theme.accentTint);
            GUI.Label(
                swatch,
                hud.CustomTintEnabled ? "Custom HUD colour" : "Theme default colour",
                new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = Color.white }
                });

            GUILayout.Label(
                "Theme changes frame/chrome style and spacing. Colour tint recolours the chrome while preserving readability. HUD panels remain independently movable and resizable.");
        }

        private static float DrawColorSlider(string label, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(18));
            float next = GUILayout.HorizontalSlider(value, 0f, 1f);
            GUILayout.Label(Mathf.RoundToInt(next * 255f).ToString(), GUILayout.Width(34));
            GUILayout.EndHorizontal();
            return next;
        }
    }
}
