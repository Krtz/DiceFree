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

            foreach (var quest in TrackedQuests)
            {
                if (!journal.PrerequisitesMet(quest)) continue;
                var state = journal.GetProgress(quest.stableId);
                if(state==null||state.status is not (QuestStatus.Active or QuestStatus.ReadyToTurnIn))continue;
                GUILayout.Label(quest.title, new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

                if (state.definitionVersion != quest.version)
                {
                    GUILayout.Label(
                        "Saved quest version unavailable; progress preserved.",
                        new GUIStyle(GUI.skin.label) { wordWrap = true });
                    GUILayout.Space(4);
                    continue;
                }

                string text;
                if (state.status == QuestStatus.ReadyToTurnIn)
                    text = $"? {state.count}/{quest.stages[Mathf.Clamp(state.stage, 0, quest.stages.Length - 1)].count} complete · Return to the quest giver.";
                else
                {
                    int stageIndex = Mathf.Clamp(state.stage, 0, Mathf.Max(0, quest.stages.Length - 1));
                    text = ObjectiveProgress.Describe(quest.stages[stageIndex], state);
                }

                GUILayout.Label(text, new GUIStyle(GUI.skin.label) { wordWrap = true });
                GUILayout.Space(4);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        public void Configure(QuestJournal value) => journal = value;
    }
}
