using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Advancement
{
    [DisallowMultipleComponent]
    public sealed class PsionMentor : InteractionTarget
    {
        public const string QuestId = "quest.psion.advancement";
        public const string ParentId = "class.magically-touched-novice";

        [SerializeField] private QuestDefinition trial;
        [SerializeField] private AdvancementDefinition edge;
        [SerializeField] private ItemDefinition[] starterGear;
        private CombatActor visitor;
        public string Feedback { get; private set; } = "";

        public void Configure(QuestDefinition quest, AdvancementDefinition advancement, ItemDefinition[] gear)
        {
            trial = quest;
            edge = advancement;
            starterGear = gear;
        }

        public override bool Available(CombatActor actor) => base.Available(actor) && edge != null &&
            edge.Eligible(actor.Stats) && actor.GetComponent<ManifestationPersistence>()?.Ready == true &&
            actor.GetComponent<AdvancementController>()?.HasManifestation(edge.targetClass.stableId) == false;

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
            var before = journal.CaptureState();
            if (!journal.Accept(trial)) return false;
            if (actor.GetComponent<ManifestationPersistence>().Flush()) return true;
            journal.RestoreState(before);
            Feedback = "The trial could not be saved. Try again.";
            return false;
        }

        public bool EnterTrial(CombatActor actor)
        {
            if (!CanInteract(actor) || actor.GetComponent<QuestJournal>().GetProgress(QuestId)?.status != QuestStatus.Active)
                return false;
            bool entered = PsionMirrorRun.TryBegin(actor, ApproachPosition, out string feedback);
            Feedback = feedback;
            if (entered) Close();
            return entered;
        }

        public bool CompleteTrial(CombatActor actor)
        {
            if (!CanInteract(actor)) return false;
            var controller = actor.GetComponent<AdvancementController>();
            bool completed = controller.TryTurnInAdvancementQuest(edge, trial, starterGear);
            Feedback = controller.Feedback;
            if (completed) Close();
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

        private static Rect Dialogue
        {
            get
            {
                float width = Mathf.Min(Screen.width-24,540);
                return new Rect((Screen.width-width)*.5f, Screen.height*.22f, width, 226);
            }
        }

        private bool Open => visitor != null && CanInteract(visitor) && visitor.GetComponent<Interactor>().Active == this;
        private bool Covers(Vector2 point) => Open && Dialogue.Contains(new Vector2(point.x,Screen.height-point.y));

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
            var box = Dialogue;
            var theme = HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);
            HudChrome.DrawPanel(box,theme);
            HudChrome.DrawHeader(new Rect(box.x+5,box.y+5,box.width-10,28),theme,"Ilyra  /  " + trial.title);
            string text = status switch
            {
                QuestStatus.Available => trial.offer,
                QuestStatus.ReadyToTurnIn => "You saw the will behind the reflection. Take the Mindblade and Codex, and make your first step as a Psion.",
                _ => trial.activeDialogue
            };
            GUI.Label(new Rect(box.x+16,box.y+45,box.width-32,88),text,
                new GUIStyle(GUI.skin.label) { wordWrap = true, normal = { textColor = theme.textTint } });
            var action = new Rect(box.x+16,box.y+139,box.width-144,34);
            if (status == QuestStatus.Available && GUI.Button(action,"Accept the mirror trial")) AcceptTrial(visitor);
            else if (status == QuestStatus.Active && GUI.Button(action,"Enter the mirror chamber")) EnterTrial(visitor);
            else if (status == QuestStatus.ReadyToTurnIn && GUI.Button(action,"Become a Psion")) CompleteTrial(visitor);
            if (visitor == null) return;
            if (GUI.Button(new Rect(box.xMax-112,box.y+139,96,34),"Leave")) Close();
            if (Feedback.Length > 0) GUI.Label(new Rect(box.x+16,box.y+181,box.width-32,32),Feedback);
        }
    }
}
