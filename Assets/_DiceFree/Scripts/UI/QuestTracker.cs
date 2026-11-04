using DiceFree.Quests;
using System.Linq;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class QuestTracker : CustomizableHudWidget
    {
        public QuestDefinition[] TrackedQuests=>journal==null?System.Array.Empty<QuestDefinition>():journal.Definitions.Where(q=>journal.GetProgress(q.stableId)?.status is QuestStatus.Active or QuestStatus.ReadyToTurnIn).ToArray();
        [SerializeField] private QuestJournal journal;
        private Vector2 scroll;

        public override string LayoutId => "quests";
        public override string DisplayName => "Quest Tracker";
        public override Rect DefaultNormalizedBounds => new(0.79f, 0.30f, 0.195f, 0.25f);
        public override Vector2 MinimumPixelSize => new(225, 150);

        private void OnGUI()
        {
            if (journal == null || HudPointerBlocker.ModalOpen) return;

            Rect panel = Bounds;
            DrawPanel(panel);
            Rect inner = Inner(panel);

            float header = Mathf.Clamp(inner.height * 0.12f, 20, 30);
            var headerRect = new Rect(inner.x, inner.y, inner.width, header);
            HudChrome.DrawHeader(headerRect, Theme, "Quests");

            GUILayout.BeginArea(new Rect(inner.x, inner.y + header, inner.width, inner.height - header));
            scroll = GUILayout.BeginScrollView(scroll, false, false);
            var tracked = TrackedQuests;
            if(tracked.Length==0)
                GUILayout.Label("No active quests. Speak with a quest giver to begin.",
                    new GUIStyle(GUI.skin.label) {wordWrap=true,normal={textColor=Theme.textTint}});
            foreach (var quest in tracked)
            {
                if (!journal.PrerequisitesMet(quest)) continue;
                var state = journal.GetProgress(quest.stableId);
                if(state==null||state.status is not (QuestStatus.Active or QuestStatus.ReadyToTurnIn))continue;
                bool ready=state.status==QuestStatus.ReadyToTurnIn;
                Color accent=ready?new Color(1f,.84f,.38f):Theme.textTint;
                GUILayout.Label(quest.title, new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold,wordWrap=true,normal={textColor=accent} });

                if (state.definitionVersion != quest.version)
                {
                    GUILayout.Label(
                        "Saved quest version unavailable; progress preserved.",
                        new GUIStyle(GUI.skin.label) { wordWrap = true });
                    GUILayout.Space(4);
                    continue;
                }

                if(ready)GUILayout.Label("Ready to turn in",new GUIStyle(GUI.skin.label){fontSize=11,normal={textColor=accent}});
                GUILayout.Label(QuestReadout.Objective(quest,state), new GUIStyle(GUI.skin.label) { wordWrap = true,normal={textColor=Theme.textTint} });
                var progress=GUILayoutUtility.GetRect(0,4,GUILayout.ExpandWidth(true));
                HudChrome.Fill(progress,new Color(.04f,.035f,.025f,.85f));
                HudChrome.Fill(new Rect(progress.x,progress.y,progress.width*QuestReadout.Fraction(quest,state),progress.height),
                    ready?accent:new Color(.51f,.72f,.48f));
                GUILayout.Space(9);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void EnsureActivityFeed()
        {
            if(journal!=null && !journal.TryGetComponent<QuestActivityFeed>(out _))
                journal.gameObject.AddComponent<QuestActivityFeed>();
        }
        private void Start() => EnsureActivityFeed();
        public void Configure(QuestJournal value) { journal = value; if(Application.isPlaying)EnsureActivityFeed(); }
    }
}
