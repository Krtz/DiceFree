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
    public sealed class AcolyteMentor : InteractionTarget
    {
        public const string QuestId = "quest.arborium.acolyte-lanterns";
        public const string ParentClassId = "class.magically-touched-novice";
        [SerializeField] private QuestDefinition trial;
        [SerializeField] private AdvancementDefinition advancement;
        private CombatActor visitor;
        public string Feedback { get; private set; } = "";
        public QuestDefinition Trial => trial;

        public void Configure(QuestDefinition quest, AdvancementDefinition edge)
        {
            trial = quest;
            advancement = edge;
        }

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
            Feedback = "The commission could not be saved. Please try again.";
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

        private static Rect Bounds
        {
            get
            {
                float width = Mathf.Min(Screen.width - 24, 530);
                return new Rect((Screen.width-width)*.5f, Screen.height*.22f, width, 234);
            }
        }
        private bool Open => visitor != null && CanInteract(visitor) && visitor.GetComponent<Interactor>().Active == this;
        private bool Covers(Vector2 position) => Open && Bounds.Contains(new Vector2(position.x, Screen.height-position.y));

        private void Close()
        {
            visitor?.GetComponent<Interactor>()?.Cancel();
            visitor = null;
        }

        private void OnGUI()
        {
            if (visitor == null || HudPointerBlocker.ModalOpen) return;
            if (!Open) { visitor = null; return; }
            var state = visitor.GetComponent<QuestJournal>().GetProgress(QuestId);
            if (state == null) return;
            var bounds = Bounds;
            var theme = HudLayoutManager.Current?.Theme ?? HudThemes.Get(HudThemes.CompactStone);
            int depth = GUI.depth;
            GUI.depth = -120;
            try
            {
                HudChrome.DrawPanel(bounds, theme);
                HudChrome.DrawHeader(new Rect(bounds.x+5,bounds.y+5,bounds.width-10,28), theme, "Mara  /  " + trial.title);
                string text = state.status switch
                {
                    QuestStatus.Available => trial.offer,
                    QuestStatus.ReadyToTurnIn => "Both candles are lit. You kept watch to the end. If you are ready, I will show you how an Acolyte carries that vigil beyond Lanternrest.",
                    _ => trial.activeDialogue
                };
                GUI.Label(new Rect(bounds.x+16,bounds.y+44,bounds.width-32,102),text,
                    new GUIStyle(GUI.skin.label){wordWrap=true,normal={textColor=theme.textTint}});
                var button = new Rect(bounds.x+16,bounds.y+148,bounds.width-147,34);
                if (state.status == QuestStatus.Available && GUI.Button(button,"Accept the Lanternrest Vigil")) AcceptTrial(visitor);
                if (state.status == QuestStatus.ReadyToTurnIn && GUI.Button(button,"Take up the Acolyte's Way")) CompleteTrial(visitor);
                if (GUI.Button(new Rect(bounds.xMax-119,bounds.y+148,103,34),"Leave")) Close();
                if (Feedback.Length != 0) GUI.Label(new Rect(bounds.x+16,bounds.y+187,bounds.width-32,32),Feedback);
            }
            finally { GUI.depth = depth; }
        }
    }
}
