using DiceFree.Quests;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class QuestTracker : HudWidget
    {
        [SerializeField] private QuestJournal journal;
        private int VisibleCount
        {
            get { int count = 0; foreach (var quest in journal.Definitions) if (journal.PrerequisitesMet(quest)) count += quest.stages[0].kind == ObjectiveKind.Any ? 2 : 1; return count; }
        }
        public override Rect Bounds => new Rect(Screen.width-310,130,294,Mathf.Max(120,VisibleCount*70+16));
        private void OnGUI()
        {
            var r=Bounds; GUI.Box(r,GUIContent.none); float y=r.y+8;
            foreach (var quest in journal.Definitions)
            {
                if (!journal.PrerequisitesMet(quest)) continue;
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
                    _ => ObjectiveProgress.Describe(quest.stages[state.stage], state)
                };
                float height = quest.stages[state.stage].kind == ObjectiveKind.Any ? 116 : 46;
                GUI.Label(new Rect(r.x+10,y,274,height),text,new GUIStyle(GUI.skin.label){wordWrap=true}); y+=height;
            }
        }
        public void Configure(QuestJournal value) => journal=value;
    }
}
