using System.Linq;using DiceFree.Advancement;using DiceFree.Combat;using DiceFree.Progression;using DiceFree.Quests;using DiceFree.World;using DiceFree.UI;using UnityEngine;
namespace DiceFree.Visuals
{
 public sealed class MageMentor:InteractionTarget
 {
  public const string QuestId="quest.mage.slime-puzzle";
  [SerializeField] QuestDefinition trainingQuest;CombatActor visitor;string feedback="";
  public QuestDefinition TrainingQuest=>trainingQuest;public void Configure(QuestDefinition value)=>trainingQuest=value;
  Rect Box=>new(Screen.width*.5f-270,Screen.height*.22f,540,280);
  protected override void OnEnable(){base.OnEnable();HudPointerBlocker.OverlayCovers+=Covers;}
  protected override void OnDisable(){HudPointerBlocker.OverlayCovers-=Covers;visitor=null;base.OnDisable();}
  bool Covers(Vector2 p)=>visitor!=null&&CanInteract(visitor)&&Box.Contains(new Vector2(p.x,Screen.height-p.y));
  public override bool Available(CombatActor actor)=>base.Available(actor)&&trainingQuest!=null&&actor.Stats.Definition?.stableId=="class.magically-touched-novice"&&actor.Stats.Level>=30&&actor.GetComponent<AdvancementController>()?.HasManifestation(MageProgressionRules.ClassId)==false;
  public override void Interact(CombatActor actor){if(!CanInteract(actor))return;visitor=actor;feedback="";actor.GetComponent<BasicAttack>()?.Cancel();actor.Motor.Stop();}
  public bool AcceptTraining(CombatActor actor)=>CanInteract(actor)&&actor.GetComponent<QuestJournal>().Accept(trainingQuest);
  public bool CompleteTraining(CombatActor actor){
   if(!CanInteract(actor))return false;var controller=actor.GetComponent<AdvancementController>();
   var edge=controller.Definitions.FirstOrDefault(d=>d.targetClass.stableId==MageProgressionRules.ClassId);
   bool result=controller.TryTurnInAdvancementQuest(edge,trainingQuest);feedback=controller.Feedback;if(result)visitor=null;return result;
  }
  void OnGUI(){
   if(visitor==null)return;if(!CanInteract(visitor)){visitor=null;return;}if(HudPointerBlocker.ModalOpen)return;
   var state=visitor.GetComponent<QuestJournal>().GetProgress(QuestId);if(state==null)return;
   var b=Box;GUI.Box(b,"The retired mage — A Mind for Magic");
   string text=state.status==QuestStatus.Available?trainingQuest.offer:state.status==QuestStatus.ReadyToTurnIn?"You listened, observed and solved the tree's riddle. Now you are ready to become a Mage.":trainingQuest.activeDialogue;
   GUI.Label(new Rect(b.x+18,b.y+35,b.width-36,85),text,new GUIStyle(GUI.skin.label){wordWrap=true});
   GUI.Label(new Rect(b.x+18,b.y+122,b.width-36,25),"Slime Dungeon puzzle solved: "+state.count+" / 1");
   if(state.status==QuestStatus.Available&&GUI.Button(new Rect(b.x+18,b.y+153,b.width-36,34),"Accept the puzzle trial"))AcceptTraining(visitor);
   if(state.status==QuestStatus.ReadyToTurnIn&&GUI.Button(new Rect(b.x+18,b.y+153,b.width-36,34),"Complete training and become a level-1 Mage"))CompleteTraining(visitor);
   if(visitor==null)return;
   var shop=GetComponent<DiceFree.Items.ItemShop>();
   if(shop!=null&&GUI.Button(new Rect(b.x+18,b.y+193,160,30),"Browse brewing supplies")){var actor=visitor;visitor=null;actor.GetComponent<Interactor>()?.Order(shop);return;}
   GUI.Label(new Rect(b.x+190,b.y+194,b.width-205,35),feedback);
   if(GUI.Button(new Rect(b.x+18,b.y+237,b.width-36,28),"Leave"))visitor=null;
  }
 }
}
