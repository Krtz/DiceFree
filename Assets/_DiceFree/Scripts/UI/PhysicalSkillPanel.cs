using DiceFree.Input;
using DiceFree.Skills;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    public sealed class PhysicalSkillPanel : HudWidget
    {
        [SerializeField] private PhysicalSkillProgression progression;
        private InputBindings bindings;
        private InputAction toggle;
        private bool open;
        private float notifiedUntil;
        private Vector2 scroll;

        private Rect ClosedBounds => new Rect(Screen.width - 330, Screen.height - 286, 314, 28);
        public override Rect Bounds => open
            ? new Rect(Mathf.Max(12, Screen.width - 490), Mathf.Max(12, Screen.height - 630), 474, 350)
            : ClosedBounds;

        public void Configure(PhysicalSkillProgression value) => progression = value;

        private void Awake()
        {
            bindings = InputBindings.Current;
            toggle = bindings.Action("UI/Novice skills menu");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (progression != null) progression.SkillPointAvailable += Notify;
        }

        protected override void OnDisable()
        {
            if (progression != null) progression.SkillPointAvailable -= Notify;
            base.OnDisable();
        }

        private void Update()
        {
            if (progression != null && !progression.ActiveForCurrentClass) { open = false; return; }
            if (bindings == null || bindings.Suppressed || toggle == null) return;
            if (toggle.WasPressedThisFrame()) open = !open;
        }

        private void Notify() => notifiedUntil = Time.unscaledTime + 8f;

        private void OnGUI()
        {
            if (progression == null || !progression.ActiveForCurrentClass || HudPointerBlocker.ModalOpen) return;

            string key = InputBindings.Display(toggle);
            if (GUI.Button(ClosedBounds, "Skills [" + key + "] — " + progression.UnspentPoints + " point(s) available"))
                open = !open;

            if (Time.unscaledTime < notifiedUntil)
                GUI.Label(new Rect(ClosedBounds.x, ClosedBounds.y - 24, ClosedBounds.width, 22), "Level up! Tier-1 skill point available.");

            if (!open) return;
            var r = Bounds;
            GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x + 10, r.y + 8, r.width - 90, 22),
                "Physically Blessed — " + progression.UnspentPoints + " unspent");
            if (GUI.Button(new Rect(r.xMax - 72, r.y + 6, 62, 24), "Close")) open = false;

            var view = new Rect(r.x + 8, r.y + 36, r.width - 16, r.height - 44);
            GUILayout.BeginArea(view);
            scroll = GUILayout.BeginScrollView(scroll);
            foreach (var definition in progression.Definitions)
            {
                if (definition == null) continue;
                int rank = progression.Rank(definition.stableId);
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label(definition.displayName + "\nRank " + rank + "/" + definition.maxRank,
                    GUILayout.Width(300), GUILayout.Height(42));
                GUI.enabled = progression.UnspentPoints > 0 && rank < definition.maxRank;
                if (GUILayout.Button("+1", GUILayout.Width(70), GUILayout.Height(38)))
                    progression.Spend(definition.stableId);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
