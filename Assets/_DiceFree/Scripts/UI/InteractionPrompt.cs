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
            if (DiceFree.Foundation.ControlBindings.BlockGameplay) return;
            if (Camera.main == null) return;
            var action = target.Resolve(player);
            if (action == null) return;
            var p=Camera.main.WorldToScreenPoint(target.transform.position+Vector3.up*2.2f);
            if (p.z <= 0 || Vector3.Distance(player.transform.position,target.transform.position)>25) return;
            string marker="";
            if (action is QuestGiver giver)
            {
                var quest = giver.CurrentQuest(player);
                var state=player.GetComponent<QuestJournal>().GetProgress(quest.stableId);
                if (giver.HasConversation(player) && state.definitionVersion == quest.version)
                    marker=state.status == QuestStatus.Available ? "! " : state.status == QuestStatus.ReadyToTurnIn ? "? " : "";
            }
            GUI.Box(new Rect(p.x-130,Screen.height-p.y-10,260,24),marker+target.DisplayName);
            if (target.CanInteract(player)) GUI.Label(new Rect(p.x-100,Screen.height-p.y+15,210,24),"I / right-click: talk");
        }
        public void Configure(InteractionTarget value,CombatActor actor) { target=value; player=actor; }
    }
}
