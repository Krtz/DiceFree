using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class ConversationPanel : HudWidget
    {
        [SerializeField] private Interactor interactor;
        public override Rect Bounds => interactor.Active is ConversationTarget
            ? new Rect(Screen.width / 2 - 205, Screen.height - 380, 410, 200) : new Rect();
        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen) return;
            if (interactor.Active is not ConversationTarget target) return;
            var r = Bounds; GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x + 12, r.y + 8, 386, 25), target.DisplayName);
            GUI.Label(new Rect(r.x + 12, r.y + 38, 386, 105), target.Dialogue, new GUIStyle(GUI.skin.label) { wordWrap = true });
            if (GUI.Button(new Rect(r.x + 12, r.y + 155, 190, 28), "I understand."))
            {
                if (target.Acknowledge(interactor.GetComponent<CombatActor>())) interactor.Cancel();
            }
            if (GUI.Button(new Rect(r.x + 278, r.y + 155, 120, 28), "Close [Esc]")) interactor.Cancel();
        }
        public void Configure(Interactor value) => interactor = value;
    }
}
