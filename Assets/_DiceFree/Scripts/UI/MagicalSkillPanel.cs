using DiceFree.Input;
using DiceFree.Skills;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    public sealed class MagicalSkillPanel : CustomizableHudWidget
    {
        [SerializeField] private MagicalSkillProgression progression;
        private InputBindings bindings;
        private InputAction toggle;
        private bool open;
        private float notifiedUntil;
        private Vector2 scroll;

        public override string LayoutId => "skill-selection-magical";
        public override string DisplayName => "Skills";
        public override string LayoutGroupId => "skills";
        // Allocation lives in the action bar; preserve this legacy component
        // for existing serialized scenes without exposing a second menu.
        public override bool ShowInLayoutEditor => false;
        public override Rect DefaultNormalizedBounds => new(0.68f, 0.35f, 0.30f, 0.36f);
        public override Vector2 MinimumPixelSize => new(360f, 230f);
        public override bool BlocksPointer => false;
        private Rect LegacyBounds => open
            ? new Rect(Mathf.Max(12, Screen.width - 490), Mathf.Max(12, Screen.height - 630), 474, 350)
            : default;

        public bool Open => false;
        public void Show() { } // Moved to in-bar '+' skill upgrades.
        public void Close() => open = false;

        public void Configure(MagicalSkillProgression value) => progression = value;

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
            // Legacy skill-menu binding intentionally does nothing.
            // Rank-ups happen only on the 12-slot action bar.
            open=false;
        }

        private void Notify() => notifiedUntil = Time.unscaledTime + 8f;

        private void OnGUI()
        {
            // No second skill allocation pop-up. The primary action bar now
            // contains active skills, auras/passives and all upgrade controls.
        }
    }
}
