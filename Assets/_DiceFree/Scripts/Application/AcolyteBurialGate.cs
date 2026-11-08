using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Advancement
{
    [DisallowMultipleComponent, RequireComponent(typeof(TimedInteractionTarget))]
    public sealed class AcolyteBurialGate : MonoBehaviour, IWorkAvailability
    {
        public const string RiteId = "work.arborium.acolyte-burial";

        [SerializeField] private QuestJournal journal;
        [SerializeField] private GameObject candleFlames;

        private void OnEnable() { journal.Changed += Refresh; Refresh(); }
        private void OnDisable() => journal.Changed -= Refresh;

        private void Refresh()
        {
            var status = journal.GetProgress(AcolyteMentor.QuestId)?.status;
            candleFlames.SetActive(status is QuestStatus.ReadyToTurnIn or QuestStatus.Completed);
        }

        public void Configure(QuestJournal owner, GameObject flames)
        {
            journal = owner;
            candleFlames = flames;
        }

        public bool WorkAvailable(CombatActor actor)
        {
            if (actor == null || actor.Stats.Definition.stableId != AcolyteMentor.ParentClassId) return false;
            var progress = actor.GetComponent<QuestJournal>()?.GetProgress(AcolyteMentor.QuestId);
            return progress?.status == QuestStatus.Active && progress.stage == 1;
        }
    }
}
