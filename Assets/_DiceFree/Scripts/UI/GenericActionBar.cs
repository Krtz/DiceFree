using UnityEngine;

namespace DiceFree.UI
{
    public sealed class GenericActionBar : CustomizableHudWidget
    {
        [SerializeField] private AbilityBarSource source;

        public override string LayoutId => "action-bar";
        public override string DisplayName => "Action Bar";
        public override Rect DefaultNormalizedBounds => new(0.535f, 0.785f, 0.405f, 0.195f);
        public override Vector2 MinimumPixelSize => new(440, 135);

        private void Awake()
        {
            if (source == null) source = GetComponent<AbilityBarSource>();
        }

        private void Update()
        {
            if (source == null || HudPointerBlocker.ModalOpen ||
                (HudLayoutManager.Current != null && HudLayoutManager.Current.EditMode))
                return;

            for (int i = 0; i < 4; i++)
                if (source.WasPressedThisFrame(i))
                    source.Activate(i);
        }

        private void OnGUI()
        {
            if (source == null || HudPointerBlocker.ModalOpen) return;

            Rect panel = Bounds;
            DrawPanel(panel);
            Rect inner = Inner(panel);
            float gap = Theme.gap;
            float cellWidth = (inner.width - gap * 5) / 6f;
            float cellHeight = (inner.height - gap) / 2f;

            for (int row = 0; row < 2; row++)
            for (int column = 0; column < 6; column++)
            {
                int index = row * 6 + column;
                var rect = new Rect(
                    inner.x + column * (cellWidth + gap),
                    inner.y + row * (cellHeight + gap),
                    cellWidth,
                    cellHeight);

                if (index < 4)
                    DrawAbility(rect, index);
                else
                    DrawEmpty(rect, index);
            }
        }

        private void DrawAbility(Rect rect, int slot)
        {
            var view = source.View(slot);
            if (!view.Exists)
            {
                DrawEmpty(rect, slot);
                return;
            }

            bool interactable = view.learned && view.cooldown <= 0.001f &&
                                !(HudLayoutManager.Current?.EditMode ?? false);
            bool hovered = rect.Contains(Event.current.mousePosition);
            HudChrome.DrawSlot(rect, Theme, view.learned, hovered);

            string compactName = view.name.Length > 12
                ? view.name.Substring(0, 11) + "…"
                : view.name;
            string label = string.IsNullOrEmpty(view.key)
                ? compactName
                : "[" + view.key + "]\n" + compactName;

            var previous = GUI.enabled;
            GUI.enabled = interactable;
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) source.Activate(slot);
            GUI.enabled = previous;
            GUI.Label(rect, label, HudChrome.SlotTextStyle(Theme, true));

            if (view.rank > 0)
            {
                var rankStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.LowerRight,
                    fontSize = Mathf.Max(9, GUI.skin.label.fontSize - 1)
                };
                GUI.Label(rect, view.rank + "/" + view.maxRank + " ", rankStyle);
            }

            if (view.cooldown > 0)
            {
                var old = GUI.color;
                GUI.color = new Color(0.04f, 0.04f, 0.04f, 0.72f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = old;
                GUI.Label(
                    rect,
                    view.cooldown.ToString("0.0"),
                    new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        fontSize = Mathf.RoundToInt(Mathf.Clamp(rect.height * 0.25f, 11, 20))
                    });
            }
        }

        private void DrawEmpty(Rect rect, int index)
        {
            bool hovered = rect.Contains(Event.current.mousePosition);
            HudChrome.DrawSlot(rect, Theme, false, hovered);
            string label = index < 6 ? "" : (index - 5).ToString();
            if (!string.IsNullOrEmpty(label))
                GUI.Label(rect, label, HudChrome.SlotTextStyle(Theme, true));
        }
    }
}
