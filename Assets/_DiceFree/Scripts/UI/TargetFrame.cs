using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class TargetFrame : HudWidget
    {
        [SerializeField] private TargetSelection selection;
        public override Rect Bounds => new Rect(Screen.width * 0.5f - 145, 16, 290, 92);
        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen) return;
            var r = Bounds; var target = selection.Selected;
            GUI.Box(r, GUIContent.none);
            if (target == null) { GUI.Label(new Rect(r.x+10,r.y+10,270,24), "No target · Slime in the eastern crops"); return; }
            GUI.Label(new Rect(r.x+10,r.y+6,270,24), $"{target.Stats.Definition.displayName} · Level {target.Stats.Level}");
            Hp(new Rect(r.x+10,r.y+32,270,24), target.Health.Current, target.Health.Maximum);
            var attack = target.GetComponent<BasicAttack>();
            GUI.Label(new Rect(r.x+10,r.y+62,270,24), $"{attack.Definition.element?.displayName ?? "No element"} · {attack.State}");
        }
        public void Configure(TargetSelection value) => selection = value;
    }
}
