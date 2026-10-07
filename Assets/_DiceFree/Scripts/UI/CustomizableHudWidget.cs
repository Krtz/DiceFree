using UnityEngine;

namespace DiceFree.UI
{
    public abstract class CustomizableHudWidget : HudWidget
    {
        public abstract string LayoutId { get; }
        public virtual string DisplayName => LayoutId;
        public abstract Rect DefaultNormalizedBounds { get; }
        public virtual Vector2 MinimumPixelSize => new(120, 60);
        public virtual float LockedAspectRatio => 0f;

        public sealed override Rect Bounds
        {
            get
            {
                var manager = HudLayoutManager.Current;
                if (manager != null) return manager.Resolve(this);

                var value = DefaultNormalizedBounds;
                return new Rect(
                    value.x * Screen.width,
                    value.y * Screen.height,
                    value.width * Screen.width,
                    value.height * Screen.height);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            HudLayoutManager.Current?.Register(this);
        }

        protected override void OnDisable()
        {
            HudLayoutManager.Current?.Unregister(this);
            base.OnDisable();
        }

        protected HudThemeMetrics Theme =>
            HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);

        protected void DrawPanel(Rect rect) => HudChrome.DrawPanel(rect, Theme);

        protected Rect Inner(Rect rect, float extra = 0f)
        {
            float inset = Theme.outerPadding + extra;
            return new Rect(
                rect.x + inset,
                rect.y + inset,
                Mathf.Max(0, rect.width - inset * 2),
                Mathf.Max(0, rect.height - inset * 2));
        }

        protected static void DrawBar(Rect rect, float fraction, Color fill, string label)
        {
            fraction = Mathf.Clamp01(fraction);
            var previous = GUI.color;
            GUI.color = new Color(0.02f, 0.02f, 0.02f, 0.96f);
            GUI.Box(rect, GUIContent.none);
            GUI.color = fill;
            GUI.DrawTexture(
                new Rect(rect.x + 2, rect.y + 2, Mathf.Max(0, (rect.width - 4) * fraction), Mathf.Max(0, rect.height - 4)),
                Texture2D.whiteTexture);
            GUI.color = previous;
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(rect, label, style);
        }
    }
}
