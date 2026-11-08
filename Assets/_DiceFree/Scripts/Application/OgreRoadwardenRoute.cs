using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Trimery
{
    [DisallowMultipleComponent, RequireComponent(typeof(TimedInteractionTarget))]
    public sealed class OgreRoadwardenRoute : MonoBehaviour, IWorkAvailability
    {
        public const string QuestId = "quest.arborium.roadwarden";
        public const string RepairId = "work.trimery.reopen-highway";

        [SerializeField] private QuestJournal journal;
        [SerializeField] private CombatActor roadwarden;
        [SerializeField] private GameObject[] wreckage;
        [SerializeField] private GameObject repairCrew;
        [SerializeField] private GameObject restoredHighway;

        public bool Reopened => journal.GetProgress(QuestId)?.status == QuestStatus.Completed;

        private void OnEnable()
        {
            journal.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => journal.Changed -= Refresh;

        public bool WorkAvailable(CombatActor actor) => actor.GetComponent<QuestJournal>() == journal &&
            NeedsRepair(journal.GetProgress(QuestId));

        private static bool NeedsRepair(QuestProgress progress) =>
            progress?.status == QuestStatus.Active && progress.stage == 1;

        private void Refresh()
        {
            var progress = journal.GetProgress(QuestId);
            bool reopened = progress?.status == QuestStatus.Completed;
            bool fighting = progress?.status == QuestStatus.Active && progress.stage == 0;
            // Leave the defeated Ogre where he fell until the crew clears the site.
            bool corpse = !reopened && roadwarden.gameObject.activeSelf &&
                roadwarden.Health != null && !roadwarden.Health.Alive;
            roadwarden.gameObject.SetActive(fighting || corpse);
            foreach (var piece in wreckage) piece.SetActive(!reopened);
            restoredHighway.SetActive(reopened);
            bool ready = NeedsRepair(progress);
            repairCrew.SetActive(ready);
            GetComponent<Collider>().enabled = ready;
        }

        public void Configure(QuestJournal owner, CombatActor boss, GameObject[] closed,
            GameObject crew, GameObject opened)
        {
            journal = owner;
            roadwarden = boss;
            wreckage = closed;
            repairCrew = crew;
            restoredHighway = opened;
        }
    }
}
