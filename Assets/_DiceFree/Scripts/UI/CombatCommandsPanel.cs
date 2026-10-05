using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class CombatCommandsPanel : HudWidget
    {
        [SerializeField] private CombatActor player;
        [SerializeField] private AggroBehaviour encounter;
        public override Rect Bounds => new Rect(Screen.width - 330, Screen.height - 162, 314, 146);
        private void OnGUI()
        {
            if (DiceFree.Foundation.ControlBindings.BlockGameplay) return;
            var r = Bounds; GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x+10,r.y+6,294,22), "Cornberg combat PoC · provisional tuning");
            GUI.enabled = player.Alive;
            if (GUI.Button(new Rect(r.x+10,r.y+34,142,28), "Attack selected ["+DiceFree.Foundation.ControlBindings.Display("attackSelected")+"]"))
                player.GetComponent<BasicAttack>().Order(player.GetComponent<TargetSelection>().Selected);
            if (GUI.Button(new Rect(r.x+160,r.y+34,144,28), "Stop ["+DiceFree.Foundation.ControlBindings.Display("stop")+"]")) player.GetComponent<BasicAttack>().Cancel();
            var respawn = player.GetComponent<RespawnAtAnchor>();
            GUI.enabled = respawn.CanReturn;
            if (GUI.Button(new Rect(r.x+10,r.y+70,294,28), "Return to Cornberg ["+DiceFree.Foundation.ControlBindings.Display("respawn")+"]")) respawn.Return();
            GUI.enabled = !player.InCombat;
            if (GUI.Button(new Rect(r.x+10,r.y+106,294,28), "DEBUG: reset the single crop Slime")) encounter.ResetEncounter();
            GUI.enabled = true;
        }
        public void Configure(CombatActor actor, AggroBehaviour enemy) { player = actor; encounter = enemy; }
    }
}
