using System.Linq;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Advancement
{
    public sealed class PriestMentor : InteractionTarget
    {
        public const string QuestId = "quest.trimery.priest-escort";
        public const string ParentClassId = "class.magically-touched-novice";
        [SerializeField] private QuestDefinition trial;
        [SerializeField] private HealerInteraction treatment;
        [SerializeField] private Vector3 cornbergArrival;

        private CombatActor visitor;
        public string Feedback { get; private set; }
        public QuestDefinition Trial => trial;
        public Vector3 CornbergArrival => cornbergArrival;
        private static Rect Dialogue => new((Screen.width - 520) / 2f, Screen.height * .2f, 520, 260);

        public void Configure(QuestDefinition definition, HealerInteraction healer, Vector3 arrival)
        {
            trial = definition;
            treatment = healer;
            cornbergArrival = arrival;
        }

        public override bool Available(CombatActor actor) => base.Available(actor) && trial != null &&
            actor.Stats.Definition.stableId == ParentClassId && actor.Stats.Level >= trial.requiredLevel &&
            actor.GetComponent<AdvancementController>()?.HasManifestation(PriestSkillProgression.PriestClassId) == false &&
            actor.GetComponent<ManifestationPersistence>()?.Ready == true;

        public override void Interact(CombatActor actor)
        {
            if (!CanInteract(actor)) return;
            visitor = actor;
            Feedback = "";
            actor.GetComponent<BasicAttack>()?.Cancel();
            actor.Motor.Stop();
        }

        public bool AcceptTrial(CombatActor actor)
        {
            if (!CanInteract(actor)) return false;
            var journal = actor.GetComponent<QuestJournal>();
            var previous = journal.CaptureState();
            if (!journal.Accept(trial)) return false;
            if (actor.GetComponent<ManifestationPersistence>().Flush()) return true;
            journal.RestoreState(previous);
            Feedback = "The quest could not be saved. Please try again.";
            return false;
        }

        public bool Depart(CombatActor actor)
        {
            if (!CanInteract(actor) || actor.GetComponent<QuestJournal>().GetProgress(QuestId)?.status != QuestStatus.Active)
                return false;
            if (!PriestEscortRun.TryBegin(actor, trial, ApproachPosition, cornbergArrival, out var message))
            {
                Feedback = message;
                return false;
            }
            visitor = null;
            return true;
        }

        public bool CompleteTrial(CombatActor actor)
        {
            if (!CanInteract(actor)) return false;
            var advancement = actor.GetComponent<AdvancementController>();
            var edge = advancement.Definitions.SingleOrDefault(value => value.requiredQuestId == QuestId);
            bool completed = advancement.TryTurnInAdvancementQuest(edge, trial);
            Feedback = advancement.Feedback;
            if (completed) visitor = null;
            return completed;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            HudPointerBlocker.OverlayCovers += Covers;
        }

        protected override void OnDisable()
        {
            HudPointerBlocker.OverlayCovers -= Covers;
            visitor = null;
            base.OnDisable();
        }

        private bool Covers(Vector2 pointer) => visitor != null && CanInteract(visitor) &&
            Dialogue.Contains(new Vector2(pointer.x, Screen.height - pointer.y));

        private void OnGUI()
        {
            if (visitor == null || HudPointerBlocker.ModalOpen) return;
            if (!CanInteract(visitor)) { visitor = null; return; }
            var progress = visitor.GetComponent<QuestJournal>().GetProgress(QuestId);
            if (progress == null) return;
            var bounds = Dialogue;
            GUI.Box(bounds, "Trimery Healer   /   " + trial.title);
            string text = progress.status switch
            {
                QuestStatus.Available => trial.offer,
                QuestStatus.ReadyToTurnIn => "The inspector found nothing unusual on the Cornberg road. You brought them through safely. Are you ready to take up the Priest's Way?",
                _ => trial.activeDialogue
            };
            GUI.Label(new Rect(bounds.x + 18, bounds.y + 36, bounds.width - 36, 83), text,
                new GUIStyle(GUI.skin.label) { wordWrap = true });
            string action = progress.status switch
            {
                QuestStatus.Available => "Accept Priest advancement trial",
                QuestStatus.Active => "Depart with the inspector",
                QuestStatus.ReadyToTurnIn => "Complete training and become a level-1 Priest",
                _ => ""
            };
            if (action.Length > 0 && GUI.Button(new Rect(bounds.x + 18, bounds.y + 124, bounds.width - 36, 36), action))
            {
                if (progress.status == QuestStatus.Available) AcceptTrial(visitor);
                else if (progress.status == QuestStatus.Active) Depart(visitor);
                else CompleteTrial(visitor);
            }
            if (visitor == null) return;
            if (GUI.Button(new Rect(bounds.x + 18, bounds.y + 172, 215, 30), "Receive free treatment") && treatment != null)
            {
                treatment.Treat(visitor);
                Feedback = "The healer tends your wounds.";
            }
            if (GUI.Button(new Rect(bounds.xMax - 130, bounds.y + 172, 112, 30), "Leave")) visitor = null;
            GUI.Label(new Rect(bounds.x + 18, bounds.y + 209, bounds.width - 36, 36), Feedback ?? "");
        }
    }
}
