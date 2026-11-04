using DiceFree.Quests;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>Readable milestones in the existing chat history, without load/restore replay.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(QuestJournal))]
    public sealed class QuestActivityFeed : MonoBehaviour
    {
        private QuestJournal journal;
        private ChatHud chat;
        public int PublishedMessages { get; private set; }
        private void Awake() => journal = GetComponent<QuestJournal>();
        private void OnEnable() => journal.GameplayProgressed += OnProgress;
        private void OnDisable() => journal.GameplayProgressed -= OnProgress;
        private void OnProgress(QuestDefinition quest, QuestProgress before, QuestProgress after)
        {
            if (!QuestReadout.Supported(quest, after)) return;
            string message = null;
            if (before.status == QuestStatus.Available && after.status == QuestStatus.Active)
                message = "Accepted: " + quest.title + ".";
            else if (after.status == QuestStatus.Completed ||
                (quest.repeatable && before.status == QuestStatus.ReadyToTurnIn && after.status == QuestStatus.Available))
            {
                message = "Completed: " + quest.title + ".";
                if (quest.rewardXp > 0) message += " +" + quest.rewardXp + " XP.";
                if (quest.reward != null && quest.reward.gold > 0) message += " +" + quest.reward.gold + " gold.";
                if (!string.IsNullOrWhiteSpace(quest.rewardTitle)) message += " Title earned: " + quest.rewardTitle + ".";
                if (quest.repeatable) message += " Available again from the quest giver.";
            }
            else if (before.status != QuestStatus.ReadyToTurnIn && after.status == QuestStatus.ReadyToTurnIn)
                message = "Ready: " + quest.title + ". " + QuestReadout.TurnIn(quest);
            else if (after.status == QuestStatus.Active && after.stage > before.stage)
                message = quest.title + ": objective complete. " + QuestReadout.Objective(quest, after).Replace("\n", " ");
            if (message == null) return;
            if (chat == null) chat = GetComponent<ChatHud>() ?? FindAnyObjectByType<ChatHud>();
            if (chat == null) return;
            chat.Post("Quest", message);
            PublishedMessages++;
        }
    }
}
