using DiceFree.Input;
using DiceFree.Skills;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    public sealed class PhysicalSkillPanel : CustomizableHudWidget
    {
        [SerializeField] private PhysicalSkillProgression progression;
        private InputBindings bindings;
        private InputAction toggle;
        private bool open;
        private float notifiedUntil;
        private Vector2 scroll;

        public override string LayoutId => "skill-selection-physical";
        public override string DisplayName => "Physical Skills";
        public override Rect DefaultNormalizedBounds => new(0.68f, 0.35f, 0.30f, 0.36f);
        public override Vector2 MinimumPixelSize => new(360f, 230f);
        public override bool BlocksPointer => open;
        private Rect LegacyBounds => open
            ? new Rect(Mathf.Max(12, Screen.width - 490), Mathf.Max(12, Screen.height - 630), 474, 350)
            : default;

        public bool Open => open;
        public void Show() { if (progression != null && progression.ActiveForCurrentClass) open = true; }
        public void Close() => open = false;

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
            if (!open) return;
            var r = Bounds;
            DrawPanel(r);
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
