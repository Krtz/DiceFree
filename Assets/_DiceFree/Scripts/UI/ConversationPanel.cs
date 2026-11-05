using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class ConversationPanel : HudWidget
    {
        [SerializeField] private Interactor interactor;
        public override Rect Bounds
        {
            get
            {
                if (interactor == null || interactor.Active is not ConversationTarget || HudPointerBlocker.ModalOpen) return new Rect();
                float width = Mathf.Min(540, Screen.width - 24), height = Mathf.Min(276, Screen.height - 24);
                return new Rect((Screen.width - width) * .5f, (Screen.height - height) * .35f, width, height);
            }
        }
        public Rect AcknowledgeBounds => new(Bounds.x + 12, Bounds.yMax - 40, Mathf.Min(230, Bounds.width - 156), 28);
        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen) return;
            if (interactor == null || interactor.Active is not ConversationTarget target) return;
            var r = Bounds;
            var theme = HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);
            int previousDepth = GUI.depth;
            GUI.depth = -120;
            try
            {
                HudChrome.DrawPanel(r, theme);
                HudChrome.DrawHeader(new Rect(r.x + 5, r.y + 5, r.width - 10, 27), theme, target.DisplayName);
                var text = new GUIStyle(GUI.skin.label) { wordWrap = true, normal = { textColor = theme.textTint } };
                GUI.Label(new Rect(r.x + 12, r.y + 42, r.width - 24, r.height - 95), target.Dialogue, text);
                if (GUI.Button(AcknowledgeBounds, "I understand."))
                {
                    if (target.Acknowledge(interactor.GetComponent<CombatActor>())) interactor.Cancel();
                }
                if (GUI.Button(new Rect(r.xMax - 132, r.yMax - 40, 120, 28), "Close [Esc]")) interactor.Cancel();
            }
            finally { GUI.depth = previousDepth; }
        }
        public void Configure(Interactor value) => interactor = value;
    }
}
