using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class QuestInteractionPanel : HudWidget
    {
        [SerializeField] private Interactor interactor;
        private Vector2 scroll;
        private QuestDefinition visibleQuest;

        public override Rect Bounds
        {
            get
            {
                if (interactor == null || interactor.Active is not QuestGiver || HudPointerBlocker.ModalOpen)
                    return new Rect();
                float width = Mathf.Min(540, Screen.width - 24);
                float height = Mathf.Min(246, Screen.height - 24);
                return new Rect((Screen.width - width) * .5f, (Screen.height - height) * .37f, width, height);
            }
        }

        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen || interactor == null || interactor.Active is not QuestGiver giver)
            {
                visibleQuest = null;
                return;
            }
            var actor = interactor.GetComponent<CombatActor>();
            var quest = giver.CurrentQuest(actor);
            var state = quest != null ? actor.GetComponent<QuestJournal>()?.GetProgress(quest.stableId) : null;
            if (state == null) return;
            if (visibleQuest != quest)
            {
                visibleQuest = quest;
                scroll = Vector2.zero;
            }
            var r = Bounds;
            var theme = HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);
            int oldDepth = GUI.depth;
            GUI.depth = -120;
            try
            {
                HudChrome.DrawPanel(r, theme);
                HudChrome.DrawHeader(new Rect(r.x + 5, r.y + 5, r.width - 10, 27), theme, giver.DisplayName);
                var text = new GUIStyle(GUI.skin.label) { wordWrap = true, normal = { textColor = theme.textTint } };
                var title = new GUIStyle(text) { fontStyle = FontStyle.Bold, fontSize = 15 };
                bool supported = state.definitionVersion == quest.version;
                GUILayout.BeginArea(new Rect(r.x + 12, r.y + 39, r.width - 24, r.height - 85));
                scroll = GUILayout.BeginScrollView(scroll, false, false);
                GUILayout.Label(quest.title, title);
                string dialogue = state.status switch
                {
                    QuestStatus.ReadyToTurnIn => "You have completed the objectives. Turn in this quest to claim your reward.",
                    QuestStatus.Completed => "Quest completed. Thank you.",
                    QuestStatus.Active when !string.IsNullOrEmpty(quest.activeDialogue) => quest.activeDialogue,
                    _ => quest.offer
                };
                GUILayout.Label(dialogue, text);
                if (!supported) GUILayout.Label("Saved quest unavailable; progress preserved.", text);
                else if (state.status == QuestStatus.Active && quest.stages is { Length: > 0 })
                    GUILayout.Label(ObjectiveProgress.Describe(quest.stages[Mathf.Clamp(state.stage, 0, quest.stages.Length - 1)], state), text);
                string reward = RewardText(quest);
                if (reward.Length > 0) GUILayout.Label("Reward: " + reward, text);
                GUILayout.EndScrollView();
                GUILayout.EndArea();

                var action = new Rect(r.x + 12, r.yMax - 39, Mathf.Min(340, r.width - 140), 28);
                if (supported && state.status == QuestStatus.Available && GUI.Button(action, "Accept quest"))
                    giver.Accept(actor, quest);
                else if (supported && state.status == QuestStatus.ReadyToTurnIn && GUI.Button(action, "Turn in"))
                    giver.TurnIn(actor, quest);
                if (GUI.Button(new Rect(r.xMax - 120, r.yMax - 39, 108, 28), "Close [Esc]"))
                    interactor.Cancel();
            }
            finally { GUI.depth = oldDepth; }
        }

        private static string RewardText(QuestDefinition quest)
        {
            var parts = new List<string>();
            if (quest.rewardXp > 0) parts.Add(quest.rewardXp + " XP");
            if (quest.reward != null)
            {
                if (quest.reward.gold > 0) parts.Add(quest.reward.gold + " gold");
                if (quest.reward.items != null)
                    foreach (var item in quest.reward.items)
                        if (item != null) parts.Add(item.displayName);
            }
            return string.Join(" · ", parts);
        }

        public void Configure(Interactor value) => interactor = value;
    }
}
