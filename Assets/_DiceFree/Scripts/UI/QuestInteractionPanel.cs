using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class QuestInteractionPanel : HudWidget
    {
        [SerializeField] private Interactor interactor;
        public override Rect Bounds => interactor.Active is QuestGiver
            ? new Rect(Screen.width/2-185,Screen.height-365,370,185) : new Rect();
        private void OnGUI()
        {
            if (interactor.Active is not QuestGiver giver) return;
            var actor=interactor.GetComponent<CombatActor>();
            var state=actor.GetComponent<QuestJournal>().GetProgress(giver.Quest.stableId);
            var r=Bounds; GUI.Box(r,GUIContent.none);
            GUI.Label(new Rect(r.x+12,r.y+8,346,25),giver.DisplayName);
            GUI.Label(new Rect(r.x+12,r.y+36,346,68),giver.Quest.offer,new GUIStyle(GUI.skin.label){wordWrap=true});
            if (state.status == QuestStatus.Available && GUI.Button(new Rect(r.x+12,r.y+110,220,28),"Accept: "+giver.Quest.title)) giver.Accept(actor);
            if (state.status == QuestStatus.ReadyToTurnIn && GUI.Button(new Rect(r.x+12,r.y+110,220,28),$"Turn in · {giver.Quest.rewardXp} XP")) giver.TurnIn(actor);
            if (state.status == QuestStatus.Active) GUI.Label(new Rect(r.x+12,r.y+110,340,28),"Complete the current objectives, then return.");
            if (state.status == QuestStatus.Completed) GUI.Label(new Rect(r.x+12,r.y+110,340,28),"Quest completed. Thank you.");
            if (GUI.Button(new Rect(r.x+250,r.y+145,108,28),"Close [Esc]")) interactor.Cancel();
        }
        public void Configure(Interactor value) => interactor=value;
    }
}
