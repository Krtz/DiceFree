using DiceFree.Quests;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class QuestTracker : HudWidget
    {
        [SerializeField] private QuestJournal journal;
        public override Rect Bounds => new Rect(Screen.width-310,130,294,120);
        private void OnGUI()
        {
            var r=Bounds; GUI.Box(r,GUIContent.none); float y=r.y+8;
            foreach (var quest in journal.Definitions)
            {
                var state=journal.GetProgress(quest.stableId);
                GUI.Label(new Rect(r.x+10,y,274,24),quest.title); y+=24;
                if (state.definitionVersion != quest.version)
                {
                    GUI.Label(new Rect(r.x+10,y,274,46), "Saved quest version unavailable; progress preserved.");
                    y += 46; continue;
                }
                var text=state.status switch {
                    QuestStatus.Available => "! " + quest.locationHint,
                    QuestStatus.ReadyToTurnIn => $"? {state.count}/{quest.stages[state.stage].count} complete · Return to the quest giver.",
                    QuestStatus.Completed => "Completed",
                    _ => $"Stage {state.stage+1}: {state.count}/{quest.stages[state.stage].count} · {quest.stages[state.stage].instruction}"
                };
                GUI.Label(new Rect(r.x+10,y,274,46),text,new GUIStyle(GUI.skin.label){wordWrap=true}); y+=46;
            }
        }
        public void Configure(QuestJournal value) => journal=value;
    }
}
