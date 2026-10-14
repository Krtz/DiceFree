using System.Linq;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.World;
using DiceFree.UI;
using UnityEngine;
namespace DiceFree.Visuals
{
    public sealed class GreatbladeMentor:InteractionTarget
    {
        public const string QuestId="quest.greatblade.slime-trial";
        [SerializeField] QuestDefinition trainingQuest;
        CombatActor visitor;
        string feedback="";
        public QuestDefinition TrainingQuest=>trainingQuest;
        public void Configure(QuestDefinition quest)=>trainingQuest=quest;
        Rect DialogueBounds=>new(Screen.width*.5f-265,Screen.height*.22f,530,290);
        protected override void OnEnable(){base.OnEnable();HudPointerBlocker.OverlayCovers+=Covers;}
        protected override void OnDisable(){HudPointerBlocker.OverlayCovers-=Covers;visitor=null;base.OnDisable();}
        bool Covers(Vector2 screenPoint)=>visitor!=null&&CanInteract(visitor)&&DialogueBounds.Contains(new Vector2(screenPoint.x,Screen.height-screenPoint.y));
        public override bool Available(CombatActor actor)=>base.Available(actor)&&trainingQuest!=null&&
            actor.Stats.Definition?.stableId=="class.physically-blessed-novice"&&actor.Stats.Level>=30&&
            actor.GetComponent<AdvancementController>()?.HasManifestation(GreatbladeProgressionRules.ClassId)==false;
        public override void Interact(CombatActor actor)
        {
            if(!CanInteract(actor))return;
            visitor=actor;feedback="";actor.GetComponent<BasicAttack>()?.Cancel();actor.Motor.Stop();
        }
        public bool AcceptTraining(CombatActor actor)=>CanInteract(actor)&&actor.GetComponent<QuestJournal>().Accept(trainingQuest);
        public bool CompleteTraining(CombatActor actor)
        {
            if(!CanInteract(actor))return false;
            var controller=actor.GetComponent<AdvancementController>();
            var edge=controller.Definitions.FirstOrDefault(x=>x.targetClass.stableId==GreatbladeProgressionRules.ClassId);
            bool result=controller.TryTurnInAdvancementQuest(edge,trainingQuest);
            feedback=controller.Feedback;if(result)visitor=null;return result;
        }
        void OnGUI()
        {
            if(visitor==null)return;
            if(!CanInteract(visitor)){visitor=null;return;}
            if(HudPointerBlocker.ModalOpen)return;
            var journal=visitor.GetComponent<QuestJournal>();
            var state=journal.GetProgress(QuestId);if(state==null)return;
            var box=DialogueBounds;GUI.Box(box,"The retired swordswoman — "+trainingQuest.title);
            string dialogue=state.status==QuestStatus.Available?trainingQuest.offer:
                state.status==QuestStatus.ReadyToTurnIn?"You faced the Slime Boss and returned. You are ready to learn the Greatblade's Way.":trainingQuest.activeDialogue;
            GUI.Label(new Rect(box.x+16,box.y+30,box.width-32,62),dialogue,new GUIStyle(GUI.skin.label){wordWrap=true});
            GUI.Label(new Rect(box.x+16,box.y+97,box.width-32,24),"Normal Slime Boss defeated: "+state.count+" / 1");
            if(state.status==QuestStatus.Available&&GUI.Button(new Rect(box.x+16,box.y+129,box.width-32,32),"Accept training quest"))AcceptTraining(visitor);
            if(state.status==QuestStatus.ReadyToTurnIn&&GUI.Button(new Rect(box.x+16,box.y+129,box.width-32,32),"Complete training and become a level-1 Greatblade"))CompleteTraining(visitor);
            if(visitor==null)return;
            // The mentor shares the farmer's NPC. Keep pending farmer errands usable while eligible.
            var farmer=GetComponent<QuestGiver>();var errand=farmer?.CurrentQuest(visitor);
            if(errand!=null)
            {
                var progress=journal.GetProgress(errand.stableId);
                string label=progress?.status==QuestStatus.ReadyToTurnIn?"Turn in farmer quest: "+errand.title:
                    journal.CanAccept(errand)?"Accept farmer quest: "+errand.title:null;
                if(label!=null&&GUI.Button(new Rect(box.x+16,box.y+170,box.width-32,30),label))
                {if(progress?.status==QuestStatus.ReadyToTurnIn)journal.TurnIn(errand);else journal.Accept(errand);}
            }
            GUI.Label(new Rect(box.x+16,box.y+207,box.width-32,28),feedback);
            if(GUI.Button(new Rect(box.x+16,box.y+246,box.width-32,28),"Leave"))visitor=null;
        }
    }
}
