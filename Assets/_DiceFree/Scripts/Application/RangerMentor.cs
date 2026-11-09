using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;

namespace DiceFree.Advancement
{
    [DisallowMultipleComponent]
    public sealed class RangerMentor : InteractionTarget
    {
        public const string QuestId = "quest.trimery.ranger-ironpine-hunt";
        public const string ParentClassId = "class.physically-blessed-novice";

        [SerializeField] private QuestDefinition trial;
        [SerializeField] private AdvancementDefinition advancement;
        private CombatActor visitor;
        public string Feedback { get; private set; } = "";

        public void Configure(QuestDefinition quest,AdvancementDefinition edge)
        { trial = quest; advancement = edge; }

        public override bool Available(CombatActor actor) => base.Available(actor) && advancement != null &&
            advancement.Eligible(actor.Stats) && actor.GetComponent<ManifestationPersistence>()?.Ready == true &&
            actor.GetComponent<AdvancementController>()?.HasManifestation(advancement.targetClass.stableId) == false;

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
            var previous = journal.CaptureState();
            if (!journal.Accept(trial)) return false;
            if (actor.GetComponent<ManifestationPersistence>().Flush()) return true;
            journal.RestoreState(previous);
            Feedback = "The hunt could not be saved. Try again.";
            return false;
        }

        public bool CompleteTrial(CombatActor actor)
        {
            if (!CanInteract(actor)) return false;
            var controller = actor.GetComponent<AdvancementController>();
            bool completed = controller.TryTurnInAdvancementQuest(advancement, trial);
            Feedback = controller.Feedback;
            if (completed) Close();
            return completed;
        }

        private bool Open => visitor != null && CanInteract(visitor) && visitor.GetComponent<Interactor>().Active == this;

        private static Rect Bounds
        {
            get
            {
                float width = Mathf.Min(Screen.width-24,530);
                return new Rect((Screen.width-width)*.5f,Screen.height*.22f,width,232);
            }
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

        private bool Covers(Vector2 pointer) => Open && Bounds.Contains(new Vector2(pointer.x,Screen.height-pointer.y));

        private void Close()
        {
            visitor?.GetComponent<Interactor>()?.Cancel();
            visitor = null;
        }

        private void OnGUI()
        {
            if (visitor == null || HudPointerBlocker.ModalOpen) return;
            if (!Open) { visitor = null; return; }
            var progress = visitor.GetComponent<QuestJournal>().GetProgress(QuestId);
            if (progress == null) return;
            var bounds = Bounds;
            var theme = HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);
            int depth = GUI.depth;
            GUI.depth = -120;
            try
            {
                HudChrome.DrawPanel(bounds,theme);
                HudChrome.DrawHeader(new Rect(bounds.x+5,bounds.y+5,bounds.width-10,28),theme,"Ilyra  /  "+trial.title);
                string text = progress.status switch
                {
                    QuestStatus.Available => trial.offer,
                    QuestStatus.ReadyToTurnIn => "Ironpine will be quieter tonight. I can still teach you how we watch its trails. Will you take up the bow?",
                    _ => trial.activeDialogue
                };
                GUI.Label(new Rect(bounds.x+16,bounds.y+44,bounds.width-32,93),text,
                    new GUIStyle(GUI.skin.label){wordWrap=true,normal={textColor=theme.textTint}});
                var action = new Rect(bounds.x+16,bounds.y+146,bounds.width-147,34);
                if (progress.status == QuestStatus.Available && GUI.Button(action,"Accept the Hunt")) AcceptTrial(visitor);
                if (progress.status == QuestStatus.ReadyToTurnIn && GUI.Button(action,"Become a Ranger")) CompleteTrial(visitor);
                if (GUI.Button(new Rect(bounds.xMax-119,bounds.y+146,103,34),"Leave")) Close();
                if (Feedback.Length != 0) GUI.Label(new Rect(bounds.x+16,bounds.y+185,bounds.width-32,32),Feedback);
            }
            finally { GUI.depth = depth; }
        }
    }
}
