using DiceFree.Combat;
using DiceFree.Gameplay;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;
namespace DiceFree.Advancement
{
    public sealed class FireMageMentor : InteractionTarget,IQuestOfferSource
    {
        public const string Teacher="teacher.mage.fire-mage";
        public const string QuestId="quest.fire-mage.ember-walk", TrialId="trial.fire-mage.ember-walk";

        [SerializeField] private QuestDefinition trial;
        [SerializeField] private AttackDefinition hazard;
        [SerializeField] private AdvancementDefinition edge;
        [SerializeField] private ItemDefinition[] starterGear;
        private CombatActor visitor;
        private float nextDiscovery;
        public string TeacherId=>Teacher;
        public System.Collections.Generic.IEnumerable<QuestDefinition> Offers { get { yield return trial; } }
        private void Start(){if(GetComponent<QuestWorldMarker>()==null)gameObject.AddComponent<QuestWorldMarker>();}
        public static string TeacherName(string id)=>"Maelis Cinderweave";
        public string Feedback {get;private set;}="";
        public bool Eligible(CombatActor actor)=>actor!=null&&actor.Stats.Definition.stableId=="class.mage"&&actor.Stats.Level>=60&&actor.GetComponent<ManifestationPersistence>()?.Ready==true;
        private void Update()
        {
            if(Time.time<nextDiscovery)return;nextDiscovery=Time.time+.15f;
            foreach(var actor in CombatActor.All)
                if(actor.Alive&&actor.GetComponent<DiceFree.Characters.TraversalInput>()!=null&&(actor.transform.position-transform.position).sqrMagnitude<=4)
                    actor.GetComponent<CodexProgression>()?.DiscoverTeacher(TeacherId);
        }
        public override void Interact(CombatActor actor){if(!CanInteract(actor))return;visitor=actor;Feedback="";}
        public bool AcceptTrial(CombatActor actor)
        {
            if(!CanInteract(actor)||!Eligible(actor)||actor.GetComponent<AdvancementController>().HasManifestation(edge.targetClass.stableId))return false;
            var journal=actor.GetComponent<QuestJournal>();var old=journal.CaptureState();
            if(!journal.Accept(trial))return false;
            if(actor.GetComponent<ManifestationPersistence>().Flush())return true;journal.RestoreState(old);Feedback="The quest could not be saved. Please try again.";return false;
        }
        public bool CompleteTrial(CombatActor actor)
        {
            if(!CanInteract(actor)||!Eligible(actor))return false;
            var controller=actor.GetComponent<AdvancementController>();bool result=controller.TryTurnInAdvancementQuest(edge,trial,starterGear);Feedback=controller.Feedback;
            if(result){visitor?.GetComponent<Interactor>()?.Cancel();visitor=null;}return result;
        }
        public bool EnterTrial(CombatActor actor){if(!CanInteract(actor)||!Eligible(actor))return false;bool result=FireEscapeRun.TryBegin(actor,trial,hazard,ApproachPosition,out var feedback);Feedback=feedback;return result;}
        protected override void OnEnable(){base.OnEnable();HudPointerBlocker.OverlayCovers+=Covers;}
        protected override void OnDisable(){HudPointerBlocker.OverlayCovers-=Covers;visitor=null;base.OnDisable();}
        private static Rect Dialogue=>new((Screen.width-Mathf.Min(560,Screen.width-24))*.5f,Screen.height*.2f,Mathf.Min(560,Screen.width-24),290);
        private bool Open=>visitor!=null&&CanInteract(visitor)&&visitor.GetComponent<Interactor>()?.Active==this;
        private bool Covers(Vector2 p)=>Open&&Dialogue.Contains(new Vector2(p.x,Screen.height-p.y));
        private void OnGUI()
        {
            if(!Open||HudPointerBlocker.ModalOpen)return;
            var theme=HudLayoutManager.Current?.Theme??HudThemes.Get(HudThemes.CompactStone);var r=Dialogue;
            HudChrome.DrawPanel(r,theme);HudChrome.DrawHeader(new Rect(r.x+5,r.y+5,r.width-10,30),theme,TeacherName(TeacherId));
            bool classMatch=visitor.Stats.Definition.stableId=="class.mage";
            bool existing=visitor.GetComponent<AdvancementController>().HasManifestation(edge.targetClass.stableId);
            var status=visitor.GetComponent<QuestJournal>().GetProgress(trial.stableId)?.status;
            string text=!classMatch?"I have nothing to teach your current path. Another reflection of you may be ready.":visitor.Stats.Level<60?"Your will is promising. Come back when you are stronger — a level 60 Mage.":existing?"You have already walked this path. Your Echo remembers it.":status==QuestStatus.Available?trial.offer:status==QuestStatus.ReadyToTurnIn?trial.turnInDialogue:trial.activeDialogue;
            GUI.Label(new Rect(r.x+18,r.y+48,r.width-36,132),text,new GUIStyle(GUI.skin.label){wordWrap=true,normal={textColor=theme.textTint}});
            if(Eligible(visitor)&&!existing)
            {
                if(status==QuestStatus.Available&&GUI.Button(new Rect(r.x+18,r.y+188,r.width-145,35),"Accept the trial"))AcceptTrial(visitor);
                else if(status==QuestStatus.Active&&GUI.Button(new Rect(r.x+18,r.y+188,r.width-145,35),"Enter the Ember Walk"))EnterTrial(visitor);
                else if(status==QuestStatus.ReadyToTurnIn&&GUI.Button(new Rect(r.x+18,r.y+188,r.width-145,35),"Become a "+edge.targetClass.displayName))CompleteTrial(visitor);
            }
            if(GUI.Button(new Rect(r.xMax-108,r.y+188,90,35),"Leave")){visitor?.GetComponent<Interactor>()?.Cancel();visitor=null;}
            GUI.Label(new Rect(r.x+18,r.y+237,r.width-36,38),Feedback);
        }
        public void Configure(QuestDefinition quest,AdvancementDefinition advancement,ItemDefinition[] gear,AttackDefinition damage)
        {trial=quest;edge=advancement;starterGear=gear;hazard=damage;ConfigureName(TeacherName(TeacherId));}
    }
}
