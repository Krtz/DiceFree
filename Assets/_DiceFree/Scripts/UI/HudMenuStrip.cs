using DiceFree.Combat;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class HudMenuStrip : CustomizableHudWidget
    {
        [SerializeField] private ActorStats stats;
        [SerializeField] private InventoryPanel inventory;
        [SerializeField] private NoviceSkillPanel noviceSkills;
        [SerializeField] private PhysicalSkillPanel physicalSkills;
        [SerializeField] private MagicalSkillPanel magicalSkills;
        [SerializeField] private OptionsPanel options;

        public override string LayoutId => "menu-strip";
        public override string DisplayName => "Menu Buttons";
        public override Rect DefaultNormalizedBounds => new(0.943f, 0.785f, 0.047f, 0.195f);
        public override Vector2 MinimumPixelSize => new(48, 135);

        private void Awake()
        {
            stats ??= GetComponent<ActorStats>();
            inventory ??= GetComponent<InventoryPanel>();
            noviceSkills ??= GetComponent<NoviceSkillPanel>();
            physicalSkills ??= GetComponent<PhysicalSkillPanel>();
            magicalSkills ??= GetComponent<MagicalSkillPanel>();
            options ??= FindAnyObjectByType<OptionsPanel>();
        }

        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen) return;

            Rect panel = Bounds;
            DrawPanel(panel);
            Rect inner = Inner(panel);
            float gap = Theme.gap;
            float height = (inner.height - gap * 2) / 3f;

            var inventoryRect = new Rect(inner.x, inner.y, inner.width, height);
            var skillsRect = new Rect(inner.x, inventoryRect.yMax + gap, inner.width, height);
            var optionsRect = new Rect(inner.x, skillsRect.yMax + gap, inner.width, height);

            bool editing = HudLayoutManager.Current?.EditMode ?? false;
            GUI.enabled = !editing;
            if (GUI.Button(inventoryRect, new GUIContent("Bag", "Inventory [B]")))
                inventory?.Show();
            if (GUI.Button(skillsRect, new GUIContent("★", "Skills [K]")))
                ShowSkills();
            if (GUI.Button(optionsRect, new GUIContent("⚙", "Options [F10]")))
                options?.Show();
            GUI.enabled = true;
        }

        private void ShowSkills()
        {
            string classId = stats?.Definition?.stableId;
            if (classId == NoviceSkillProgression.ClassStableId) noviceSkills?.Show();
            else if (classId == PhysicalSkillProgression.ClassStableId) physicalSkills?.Show();
            else if (classId == MagicalSkillProgression.ClassStableId) magicalSkills?.Show();
        }
    }
}
