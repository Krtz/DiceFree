using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.World;
using UnityEngine;
using System.Linq;

namespace DiceFree.UI
{
    public sealed class QuestInteractionPanel : HudWidget
    {
        [SerializeField] private Interactor interactor;
        public override Rect Bounds => interactor.Active is QuestGiver
            ? new Rect(Screen.width/2-250,Screen.height-390,500,260) : new Rect();
        private void OnGUI()
        {
            if (interactor.Active is not QuestGiver giver) return;
            var actor=interactor.GetComponent<CombatActor>();
            var quest = giver.CurrentQuest(actor);
            var state=actor.GetComponent<QuestJournal>().GetProgress(quest.stableId);
            var r=Bounds; GUI.Box(r,GUIContent.none);
            GUI.Label(new Rect(r.x+12,r.y+8,346,25),giver.DisplayName);
            GUI.Label(new Rect(r.x+12,r.y+36,476,100),quest.offer,new GUIStyle(GUI.skin.label){wordWrap=true});
            string reward = quest.rewardXp + " XP";
            if (quest.reward != null) reward += " · " + quest.reward.gold + " gold · " + string.Join(", ", quest.reward.items.Select(item => item.displayName));
            GUI.Label(new Rect(r.x+12,r.y+140,476,45),"Reward: " + reward,new GUIStyle(GUI.skin.label){wordWrap=true});
            if (state.definitionVersion != quest.version)
            {
                GUI.Label(new Rect(r.x+12,r.y+185,476,28), "Saved quest unavailable; progress preserved.");
                if (GUI.Button(new Rect(r.x+380,r.y+222,108,28),"Close [Esc]")) interactor.Cancel();
                return;
            }
            if (state.status == QuestStatus.Available && GUI.Button(new Rect(r.x+12,r.y+185,340,28),"Accept: "+quest.title)) giver.Accept(actor, quest);
            if (state.status == QuestStatus.ReadyToTurnIn && GUI.Button(new Rect(r.x+12,r.y+185,340,28),"Turn in")) giver.TurnIn(actor, quest);
            if (state.status == QuestStatus.Active) GUI.Label(new Rect(r.x+12,r.y+185,476,28),"Complete the current objectives, then return.");
            if (state.status == QuestStatus.Completed) GUI.Label(new Rect(r.x+12,r.y+185,476,28),"Quest completed. Thank you.");
            if (GUI.Button(new Rect(r.x+380,r.y+222,108,28),"Close [Esc]")) interactor.Cancel();
        }
        public void Configure(Interactor value) => interactor=value;
    }
}
