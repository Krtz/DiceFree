using DiceFree.Combat;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(ActorResourceController))]
    public sealed class SoulChargeHud : CustomizableHudWidget
    {
        private ActorResourceController resources;
        private bool Visible => resources != null && resources.Has(AcolyteSkillProgression.SoulResourceId);
        public override string LayoutId => "soul-charges";
        public override string DisplayName => "Soul Charges";
        public override Rect DefaultNormalizedBounds => new(.794f, .785f, .184f, .04f);
        public override Vector2 MinimumPixelSize => new(160, 28);
        public override bool BlocksPointer => Visible || (HudLayoutManager.Current?.EditMode ?? false);

        private void Start() => resources = GetComponent<ActorResourceController>();

        private void OnGUI()
        {
            bool editing = HudLayoutManager.Current?.EditMode ?? false;
            if (HudPointerBlocker.ModalOpen || (!Visible && !editing)) return;
            var bounds = Bounds;
            DrawPanel(bounds);
            var inside = Inner(bounds, 1);
            int count = Visible ? Mathf.RoundToInt(resources.Current(AcolyteSkillProgression.SoulResourceId)) : 4;
            GUI.Label(new Rect(inside.x, inside.y, inside.width * .47f, inside.height), "Soul " + count + " / 8");
            float width = inside.width * .063f;
            for (int i = 0; i < 8; i++)
            {
                var bead = new Rect(inside.x + inside.width * .44f + i * width, inside.center.y - 5, width - 2, 10);
                HudChrome.Fill(bead, i < count ? new Color(.68f,.37f,.91f) : new Color(.15f,.09f,.22f));
                HudChrome.Border(bead, 1, Theme.accentTint);
            }
        }
    }
}
