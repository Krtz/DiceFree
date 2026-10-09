using DiceFree.Quests;
using UnityEngine;
namespace DiceFree.UI
{
    public sealed class QuestJournalPanel : CustomizableHudWidget
    {
        QuestJournal journal;Vector2 scroll;public bool Open {get;private set;}
        public override string LayoutId=>"quest-journal";
        public override string DisplayName=>"Quest Journal";
        public override Rect DefaultNormalizedBounds=>new(.22f,.15f,.5f,.65f);
        public override Vector2 MinimumPixelSize=>new(400,300);
        public override bool BlocksPointer=>Open;
        void Awake()=>journal=GetComponent<QuestJournal>();
        public void Show()=>Open=true;
        public void Close()=>Open=false;
        void OnGUI()
        {
            if(!Open||journal==null||HudPointerBlocker.ModalOpen)return;
            DrawPanel(Bounds);var inner=Inner(Bounds);
            GUILayout.BeginArea(inner);GUILayout.BeginHorizontal();GUILayout.Label("Quest Journal");if(GUILayout.Button("Close",GUILayout.Width(70)))Close();GUILayout.EndHorizontal();
            scroll=GUILayout.BeginScrollView(scroll);
            foreach(var quest in journal.Definitions)
            {
                var p=journal.GetProgress(quest.stableId);if(p==null||p.status==QuestStatus.Available&&!journal.PrerequisitesMet(quest))continue;
                GUILayout.Label(quest.title+" - "+p.status,new GUIStyle(GUI.skin.label){fontStyle=FontStyle.Bold});
                GUILayout.Label(quest.offer,new GUIStyle(GUI.skin.label){wordWrap=true});
                if(p.definitionVersion!=quest.version)GUILayout.Label("Saved version unavailable; history preserved.");
                else if(p.status==QuestStatus.Completed){foreach(var stage in quest.stages)GUILayout.Label("Completed: "+stage.instruction);}
                else if(p.status==QuestStatus.Available)GUILayout.Label(quest.locationHint);
                else if(quest.stages.Length>0)GUILayout.Label(ObjectiveProgress.Describe(quest.stages[Mathf.Clamp(p.stage,0,quest.stages.Length-1)],p));
                GUILayout.Space(10);
            }
            GUILayout.EndScrollView();GUILayout.EndArea();
        }
    }
}
