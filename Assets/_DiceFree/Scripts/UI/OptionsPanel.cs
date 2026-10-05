using DiceFree.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    [DefaultExecutionOrder(-10000)]
    public sealed class OptionsPanel : HudWidget
    {
        private InputBindings bindings;
        private Vector2 scroll;

        public bool Open { get; private set; }
        private Rect ButtonBounds => new Rect(Mathf.Max(10, Screen.width - 200), 20, 180, 30);
        public override Rect Bounds => Open ? new Rect(0, 0, Screen.width, Screen.height) : ButtonBounds;

        private void Awake() => bindings = InputBindings.Current;

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
            Open = false;
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
                if (GUI.Button(ButtonBounds,
                        "Options [" + InputBindings.Display(bindings.Action("UI/Options menu toggle")) + "]"))
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

            if (bindings.Listening)
            {
                GUILayout.Label("Listening: " + bindings.ListeningLabel +
                                ". Press a keyboard key or mouse button. Backspace cancels.");
                if (GUILayout.Button("Cancel rebinding")) bindings.CancelRebind();
            }

            GUILayout.Label(bindings.Status);
            GUILayout.Label("Shared bindings are allowed and are highlighted below. " +
                            "Gameplay and camera controls are paused while this menu is open.");

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
    }
}
