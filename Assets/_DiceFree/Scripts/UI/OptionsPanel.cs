using DiceFree.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    [DefaultExecutionOrder(-10000)]
    public sealed class OptionsPanel : HudWidget
    {
        private InputBindings bindings;
        private GameplayPreferences gameplayPreferences;
        private IPlayerAudioSettings audioSettings;
        private Vector2 scroll;
        public System.Func<bool> ReturnToStartMenu {get;set;}
        private bool confirmReturn;private string returnFeedback="";
        public bool RequestReturnToStartMenu(){bool saved=ReturnToStartMenu?.Invoke()==true;if(!saved)returnFeedback="Could not save progress. Return cancelled.";return saved;}

        public bool Open { get; private set; }
        private Rect ButtonBounds => new Rect(Mathf.Max(10, Screen.width - 54), Mathf.Max(10, Screen.height - 54), 44, 44);
        public override Rect Bounds => Open ? new Rect(0, 0, Screen.width, Screen.height) : ButtonBounds;

        private void Awake()
        {
            bindings = InputBindings.Current;
            gameplayPreferences = GameplayPreferences.Current;
            foreach(var component in GetComponents<MonoBehaviour>())
                if(component is IPlayerAudioSettings audio){audioSettings=audio;break;}
        }

        protected override void OnDisable()
        {
            Close();
            base.OnDisable();
        }

        public void Show()
        {
            Open = true;
            bindings.SetModal(true);
        }

        public void Close()
        {
            if (bindings == null) return;
            bindings.CancelRebind();
            PlayerPrefs.Save();
            Open = false;
            confirmReturn=false;
            bindings.SetModal(false);
        }

        private void Update()
        {
            bindings.Tick();
            if (bindings.Listening || bindings.LastRebindFrame == Time.frameCount) return;

            if (Open && bindings.Action("UI/Close options").WasPressedThisFrame())
            {
                Close();
                return;
            }

            if (bindings.Action("UI/Options menu toggle").WasPressedThisFrame())
            {
                if (Open) Close();
                else Show();
            }
        }

        private void OnGUI()
        {
            GUI.depth = -100;
            if (!Open)
            {
                // The customizable HUD owns the visible launcher; keep this fallback only for legacy scenes.
                if (HudLayoutManager.Current == null &&
                    GUI.Button(
                        ButtonBounds,
                        new GUIContent("⚙", "Options [" +
                            InputBindings.Display(bindings.Action("UI/Options menu toggle")) + "]")))
                    Show();
                return;
            }

            GUI.Box(Bounds, GUIContent.none);
            float width = Mathf.Min(760, Screen.width - 24);
            float height = Mathf.Max(120, Screen.height - 24);
            var panel = new Rect((Screen.width - width) / 2, 12, width, height);

            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Options — Controls");
            if (!bindings.Listening && GUILayout.Button("Close [Escape]", GUILayout.Width(130))) Close();
            GUILayout.EndHorizontal();
            if(ReturnToStartMenu!=null)
            {
                if(!confirmReturn&&GUILayout.Button("Save and Return to Start Menu"))confirmReturn=true;
                if(confirmReturn){GUILayout.Label("Save progress and leave this session? Active dungeon runs will end.");GUILayout.BeginHorizontal();if(GUILayout.Button("Save and Return"))RequestReturnToStartMenu();if(GUILayout.Button("Cancel"))confirmReturn=false;GUILayout.EndHorizontal();}
                if(!string.IsNullOrEmpty(returnFeedback))GUILayout.Label(returnFeedback);
            }

            if (bindings.Listening)
            {
                GUILayout.Label("Listening: " + bindings.ListeningLabel +
                                ". Press a keyboard key or mouse button. Backspace cancels.");
                if (GUILayout.Button("Cancel rebinding")) bindings.CancelRebind();
            }

            GUILayout.Label(bindings.Status);
            GUILayout.Label("Shared bindings are allowed and are highlighted below. " +
                            "Gameplay and camera controls are paused while this menu is open.");

            GUILayout.Space(8);
            if(audioSettings==null)
                foreach(var component in GetComponents<MonoBehaviour>())
                    if(component is IPlayerAudioSettings audio){audioSettings=audio;break;}
            if(audioSettings!=null)
            {
                GUILayout.Label("Audio");
                GUILayout.BeginHorizontal();
                GUILayout.Label("Sound effects",GUILayout.Width(135));
                float sfx=GUILayout.HorizontalSlider(audioSettings.SfxLevel,0f,1f);
                GUILayout.Label((int)(sfx*100)+"%",GUILayout.Width(50));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Ambience",GUILayout.Width(135));
                float ambient=GUILayout.HorizontalSlider(audioSettings.AmbientLevel,0f,1f);
                GUILayout.Label((int)(ambient*100)+"%",GUILayout.Width(50));
                GUILayout.EndHorizontal();
                if(!Mathf.Approximately(sfx,audioSettings.SfxLevel) ||
                   !Mathf.Approximately(ambient,audioSettings.AmbientLevel))
                    audioSettings.SetLevels(sfx,ambient);
                GUILayout.Space(8);
            }
            GUILayout.Label("Gameplay");
            bool autoRetaliate = GUILayout.Toggle(
                gameplayPreferences.AutoRetaliate,
                "Auto retaliate when attacked");
            if (autoRetaliate != gameplayPreferences.AutoRetaliate)
                gameplayPreferences.SetAutoRetaliate(autoRetaliate);
            GUILayout.Label("Defaults to on. It only retaliates while you are not already attacking a chosen target.");

            GUILayout.Space(4);
            GUILayout.Label("When a targeted skill is out of range:");
            GUILayout.BeginHorizontal();
            bool walkIntoRange = gameplayPreferences.OutOfRangeSkillBehavior ==
                                 OutOfRangeSkillBehavior.WalkIntoRange;
            if (GUILayout.Toggle(walkIntoRange, "Walk into cast range", "Button") && !walkIntoRange)
                gameplayPreferences.SetOutOfRangeSkillBehavior(OutOfRangeSkillBehavior.WalkIntoRange);

            bool doNothing = gameplayPreferences.OutOfRangeSkillBehavior ==
                             OutOfRangeSkillBehavior.DoNothing;
            if (GUILayout.Toggle(doNothing, "Do nothing", "Button") && !doNothing)
                gameplayPreferences.SetOutOfRangeSkillBehavior(OutOfRangeSkillBehavior.DoNothing);
            GUILayout.EndHorizontal();

            bool showCastRange = GUILayout.Toggle(
                gameplayPreferences.ShowCastRange,
                "Show cast range while targeting a skill");
            if (showCastRange != gameplayPreferences.ShowCastRange)
                gameplayPreferences.SetShowCastRange(showCastRange);

            var hud = HudLayoutManager.Current;
            if (hud != null)
            {
                GUILayout.Space(8);
                DrawHudSettings(hud);
            }

            scroll = GUILayout.BeginScrollView(scroll);
            string lastMap = null;
            foreach (var entry in bindings.Entries)
            {
                if (entry.Map != lastMap)
                {
                    lastMap = entry.Map;
                    GUILayout.Space(8);
                    GUILayout.Label(lastMap);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label(entry.Label, GUILayout.Width(Mathf.Min(340, panel.width * 0.50f)));
                GUI.enabled = !bindings.Listening;
                if (GUILayout.Button(entry.Display.Length == 0 ? "Unbound" : entry.Display))
                    bindings.BeginRebind(entry);
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                string conflict = bindings.Conflict(entry);
                if (conflict.Length > 0) GUILayout.Label(conflict);
            }
            GUILayout.EndScrollView();

            GUI.enabled = !bindings.Listening;
            if (GUILayout.Button("Reset all controls to defaults")) bindings.ResetDefaults();
            GUI.enabled = true;
            GUILayout.EndArea();
        }

        private void DrawHudSettings(HudLayoutManager hud)
        {
            GUILayout.Label("HUD");
            GUILayout.Label("Theme");

            GUILayout.BeginHorizontal();
            foreach (var theme in HudThemes.All)
            {
                bool selected = theme.id == hud.ThemeId;
                bool pressed = GUILayout.Toggle(selected, theme.displayName, "Button");
                if (pressed && !selected) hud.SetTheme(theme.id);
            }
            GUILayout.EndHorizontal();

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
