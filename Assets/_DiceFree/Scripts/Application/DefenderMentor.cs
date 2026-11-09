using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Advancement
{
    [DisallowMultipleComponent]
    public sealed class DefenderMentor : InteractionTarget
    {
        public const string QuestId = "quest.trimery.defender-gate-trial";
        public const string ParentId = "class.physically-blessed-novice";

        [SerializeField] private QuestDefinition trial;
        [SerializeField] private AdvancementDefinition edge;
        private CombatActor visitor;
        public string Feedback { get; private set; } = "";
        private static Rect Dialogue
        {
            get
            {
                float width = Mathf.Min(Screen.width - 24, 536);
                return new Rect((Screen.width - width) * .5f, Screen.height * .22f, width, 228);
            }
        }

        public void Configure(QuestDefinition definition, AdvancementDefinition advancement)
        {
            trial = definition;
            edge = advancement;
        }

        public override bool Available(CombatActor actor) => base.Available(actor) && edge != null &&
            edge.Eligible(actor.Stats) && actor.GetComponent<ManifestationPersistence>()?.Ready == true &&
            actor.GetComponent<AdvancementController>()?.HasManifestation(DefenderSkillProgression.DefenderClassId) == false;

        public override void Interact(CombatActor actor)
        {
            if (!CanInteract(actor)) return;
            visitor = actor;
            Feedback = "";
        }

        public bool AcceptTrial(CombatActor actor)
        {
            if (!CanInteract(actor)) return false;
            var journal = actor.GetComponent<QuestJournal>();
            var snapshot = journal.CaptureState();
            if (!journal.Accept(trial)) return false;
            if (actor.GetComponent<ManifestationPersistence>().Flush()) return true;
            journal.RestoreState(snapshot);
            Feedback = "The watch assignment could not be saved. Try again.";
            return false;
        }

        public bool Depart(CombatActor actor)
        {
            if (!CanInteract(actor) || actor.GetComponent<QuestJournal>().GetProgress(QuestId)?.status != QuestStatus.Active)
                return false;
            if (!DefenderGateRun.TryBegin(actor, trial, ApproachPosition, out string feedback))
            {
                Feedback = feedback;
                return false;
            }
            Close();
            return true;
        }

        public bool CompleteTrial(CombatActor actor)
        {
            if (!CanInteract(actor)) return false;
            var controller = actor.GetComponent<AdvancementController>();
            bool done = controller.TryTurnInAdvancementQuest(edge, trial);
            Feedback = controller.Feedback;
            if (done) Close();
            return done;
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

        private bool Open => visitor != null && CanInteract(visitor) && visitor.GetComponent<Interactor>().Active == this;
        private bool Covers(Vector2 pointer) => Open && Dialogue.Contains(new Vector2(pointer.x, Screen.height - pointer.y));
        private void Close()
        {
            visitor?.GetComponent<Interactor>()?.Cancel();
            visitor = null;
        }

        private void OnGUI()
        {
            if (visitor == null || HudPointerBlocker.ModalOpen) return;
            if (!Open) { visitor = null; return; }
            var status = visitor.GetComponent<QuestJournal>().GetProgress(QuestId)?.status;
            if (status == null) return;
            var box = Dialogue;
            var theme = HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);
            HudChrome.DrawPanel(box, theme);
            HudChrome.DrawHeader(new Rect(box.x + 5, box.y + 5, box.width - 10, 28), theme, "Veteran Mara  /  " + trial.title);
            string text = status switch
            {
                QuestStatus.Available => trial.offer,
                QuestStatus.ReadyToTurnIn => "The workers made it through and the watch held. Take up a shield with us, if you’re ready.",
                _ => trial.activeDialogue
            };
            GUI.Label(new Rect(box.x + 16, box.y + 44, box.width - 32, 88), text,
                new GUIStyle(GUI.skin.label) { wordWrap = true, normal = { textColor = theme.textTint } });
            var action = new Rect(box.x + 16, box.y + 139, box.width - 144, 36);
            if (status == QuestStatus.Available && GUI.Button(action, "Accept the Gate Watch")) AcceptTrial(visitor);
            else if (status == QuestStatus.Active && GUI.Button(action, "Take your post")) Depart(visitor);
            else if (status == QuestStatus.ReadyToTurnIn && GUI.Button(action, "Become a Defender")) CompleteTrial(visitor);
            if (visitor == null) return;
            if (GUI.Button(new Rect(box.xMax - 112, box.y + 139, 96, 36), "Leave")) Close();
            if (Feedback.Length > 0) GUI.Label(new Rect(box.x + 16, box.y + 182, box.width - 32, 32), Feedback);
        }
    }
}