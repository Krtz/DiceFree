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
                if(!QuestReadout.Supported(quest,p))GUILayout.Label("Saved version unavailable; history preserved.");
                else if(p.status==QuestStatus.Completed){foreach(var stage in quest.stages)GUILayout.Label("Completed: "+stage.instruction,new GUIStyle(GUI.skin.label){wordWrap=true});}
                else if(p.status==QuestStatus.Available)GUILayout.Label(quest.locationHint);
                else
                {
                    if(p.status==QuestStatus.ReadyToTurnIn)GUILayout.Label("Ready to turn in. "+QuestReadout.TurnIn(quest),new GUIStyle(GUI.skin.label){wordWrap=true,normal={textColor=new Color(1f,.84f,.38f)}});
                    for(int i=0;i<quest.stages.Length;i++)
                    {
                        bool done=i<p.stage || p.status==QuestStatus.ReadyToTurnIn;
                        string prefix=done?"Completed: ":i==p.stage?"Current: ":"Later: ";
                        string instruction=i==p.stage&&!done?ObjectiveProgress.Describe(quest.stages[i],p):quest.stages[i].instruction;
                        GUILayout.Label(prefix+instruction,new GUIStyle(GUI.skin.label){wordWrap=true,normal={textColor=done?new Color(.65f,.85f,.63f):Theme.textTint}});
                    }
                }
                if(!string.IsNullOrWhiteSpace(quest.rewardTitle))GUILayout.Label("Title reward: "+quest.rewardTitle);
                GUILayout.Space(10);
            }
            GUILayout.EndScrollView();GUILayout.EndArea();
        }
    }
}
