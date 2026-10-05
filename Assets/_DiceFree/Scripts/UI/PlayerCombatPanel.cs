using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class PlayerCombatPanel : HudWidget
    {
        [SerializeField] private CombatActor player;
        [SerializeField] private HealingArea well;
        public override Rect Bounds => new Rect(16, Screen.height - 162, 430, 146);
        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen) return;
            var r = Bounds;
            GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x+10,r.y+6,410,24), $"{player.Stats.Definition.displayName} · Level {player.Stats.Level} · No class resource");
            Hp(new Rect(r.x+10,r.y+32,410,24), player.Health.Current, player.Health.Maximum);
            string state = !player.Alive ? "DEAD — R / Return to Cornberg" : well.Contains(player) ? "Magical well · healing" :
                player.InCombat ? "IN COMBAT" : "Exploring · Cornberg anchor available";
            GUI.Label(new Rect(r.x+10,r.y+58,410,22), state);
            GUI.Label(new Rect(r.x+10,r.y+80,410,22), "Fists: " + player.GetComponent<BasicAttack>().State);
            GUI.Label(new Rect(r.x+10,r.y+102,410,38), "Left-click / Tab: target   Right-click enemy / X: attack\nWASD / ground move / Space: cancel attack   Esc: clear");
        }
        public void Configure(CombatActor actor, HealingArea area) { player = actor; well = area; }
    }
}
