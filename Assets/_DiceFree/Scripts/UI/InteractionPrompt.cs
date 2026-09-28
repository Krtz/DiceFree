using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class InteractionPrompt : MonoBehaviour
    {
        [SerializeField] private InteractionTarget target;
        [SerializeField] private CombatActor player;
        private void OnGUI()
        {
            if (Camera.main == null) return;
            var p=Camera.main.WorldToScreenPoint(target.transform.position+Vector3.up*2.2f);
            if (p.z <= 0 || Vector3.Distance(player.transform.position,target.transform.position)>25) return;
            string marker="";
            if (target is QuestGiver giver)
            {
                var state=player.GetComponent<QuestJournal>().GetProgress(giver.Quest.stableId);
                marker=state.status == QuestStatus.Available ? "! " : state.status == QuestStatus.ReadyToTurnIn ? "? " : "";
            }
            GUI.Box(new Rect(p.x-130,Screen.height-p.y-10,260,24),marker+target.DisplayName);
            if (target.CanInteract(player)) GUI.Label(new Rect(p.x-100,Screen.height-p.y+15,210,24),"I / right-click: talk");
        }
        public void Configure(InteractionTarget value,CombatActor actor) { target=value; player=actor; }
    }
}
