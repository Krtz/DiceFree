using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class TargetFrame : CustomizableHudWidget
    {
        [SerializeField] private TargetSelection selection;

        public override string LayoutId => "target";
        public override string DisplayName => "Target";
        public override Rect DefaultNormalizedBounds => new(0.41f, 0.025f, 0.20f, 0.085f);
        public override Vector2 MinimumPixelSize => new(235, 72);

        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen) return;
            var target = selection == null ? null : selection.Selected;
            if (target == null && !(HudLayoutManager.Current?.EditMode ?? false)) return;

            Rect panel = Bounds;
            DrawPanel(panel);
            Rect inner = Inner(panel);
            if (target == null)
            {
                GUI.Label(inner, "No target", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                return;
            }

            float titleHeight = Mathf.Clamp(inner.height * 0.38f, 20, 30);
            GUI.Label(
                new Rect(inner.x, inner.y, inner.width, titleHeight),
                $"{target.Stats.Definition.displayName}   Lv {target.Stats.Level}",
                new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

            var hp = new Rect(inner.x, inner.y + titleHeight, inner.width, inner.height - titleHeight);
            DrawBar(
                hp,
                target.Health.Current / Mathf.Max(1, target.Health.Maximum),
                new Color(0.72f, 0.06f, 0.035f, 1f),
                $"{target.Health.Current:0} / {target.Health.Maximum:0}");
        }

        public void Configure(TargetSelection value) => selection = value;
    }
}
