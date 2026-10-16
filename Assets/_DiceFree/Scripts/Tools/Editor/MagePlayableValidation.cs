using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;
using DiceFree.Advancement;using DiceFree.Characters;using DiceFree.Combat;using DiceFree.Development;using DiceFree.Dungeons;using DiceFree.Foundation;using DiceFree.Gameplay;using DiceFree.Input;using DiceFree.Items;using DiceFree.Persistence;using DiceFree.Progression;using DiceFree.Quests;using DiceFree.Skills;using DiceFree.UI;using DiceFree.Visuals;using DiceFree.World;
using UnityEngine;using UnityEngine.SceneManagement;using UnityEditor;using UnityEditor.SceneManagement;using Unity.Pipeline.Commands;
using Object=UnityEngine.Object;using Kind=DiceFree.Progression.MageProgressionRules.Kind;
namespace DiceFree.EditorTools
{
 [InitializeOnLoad] public static class MagePlayableValidation
 {
  const string Key="DiceFree.MagePlayable.";static readonly Stack<IEnumerator> steps=new();static double deadline;static int checks;
  [Serializable] sealed class Report{public string status,error;public int checks,chainTargets,uniqueElements,puzzleCaptures;public float fullBarrierHp,partialBarrierHp,walkDistance;public bool questAdvanced;}
  static Report report;
  static MagePlayableValidation()=>EditorApplication.playModeStateChanged+=Changed;
  static void Need(bool value,string text){if(!value)throw new InvalidOperationException(text);checks++;}
  static void Phase(string p){SessionState.SetString(Key+"phase",p);SessionState.SetInt(Key+"checks",checks);}
  [CliCommand("dicefree.mage.test","Verify real puzzle advancement, all Mage spells, Weaving, Mana redirection, gear, walk poses and saves.")]
  public static object Start(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode.");
   SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
   SessionState.SetBool(Key+"cleanup",true);SessionState.SetBool(Key+"resume",false);SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"phase","initializing");SessionState.SetString(Key+"error","");SessionState.SetInt(Key+"checks",0);
   var root=Path.Combine(Path.GetTempPath(),"DiceFree-Mage-"+Guid.NewGuid().ToString("N"));PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
   SessionState.SetString(Key+"root",root);EditorSettings.enterPlayModeOptionsEnabled=false;
   EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");EditorApplication.EnterPlaymode();return Status();
  }
  [CliCommand("dicefree.mage.status","Read focused Mage verification.")]
  public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),phase=SessionState.GetString(Key+"phase",""),checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error","")};
  [CliCommand("dicefree.mage.resume","Finish Mage verification after visual capture.")]
  public static object Resume(){SessionState.SetBool(Key+"resume",true);return Status();}
  static void Changed(PlayModeStateChange s){
   if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running"){
    checks=0;report=new Report();steps.Clear();steps.Push(Flow());deadline=EditorApplication.timeSinceStartup+300;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;
   }
   if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false)){
    SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;PersistenceTestGuard.ClearValidationOverrides();DevelopmentSession.End();
    EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);
    string path=SessionState.GetString(Key+"scene","");if(!string.IsNullOrWhiteSpace(path))EditorSceneManager.OpenScene(path);
   }
  }
  static void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)Finish(false,text+"\n"+stack);}
  static void Tick(){if(!EditorApplication.isPlaying)return;try{
   if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Mage validation timed out in "+SessionState.GetString(Key+"phase",""));
   while(steps.Count>0){var step=steps.Peek();if(!step.MoveNext()){steps.Pop();continue;}if(step.Current is IEnumerator child){steps.Push(child);continue;}return;}
   Finish(true,null);
  }catch(Exception e){Finish(false,e.ToString());}}
  static void Finish(bool pass,string error){
   EditorApplication.update-=Tick;Application.logMessageReceived-=Log;report??=new Report();report.status=pass?"passed":"failed";report.error=error;report.checks=checks;
   SessionState.SetString(Key+"status",report.status);SessionState.SetString(Key+"error",error??"");SessionState.SetInt(Key+"checks",checks);
   File.WriteAllText("Library/MagePlayableResults.json",JsonUtility.ToJson(report,true));EditorApplication.ExitPlaymode();
  }
  static IEnumerator Frames(int n){for(int i=0;i<n;i++)yield return null;}
  static IEnumerator Wait(float seconds){float until=Time.time+seconds;while(Time.time<until)yield return null;}
  static IEnumerator Flow(){
   CombatActor p=null;while(p==null){p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();yield return null;}
   var save=p.GetComponent<ManifestationPersistence>();while(!save.Ready||!p.Motor.Ready)yield return null;
   p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;GameplayPreferences.Current.SetAutoRetaliate(false);Protect(p);
   var xp=p.GetComponent<ExperienceProgression>();var controller=p.GetComponent<AdvancementController>();var journal=p.GetComponent<QuestJournal>();var mentor=Object.FindAnyObjectByType<MageMentor>();var quest=mentor.TrainingQuest;
   var edge=controller.Definitions.Single(d=>d.targetClass.stableId==MageProgressionRules.ClassId);
   Phase("quest eligibility and isolated credit");
   xp.RestoreState(30,0);Need(!mentor.Available(p)&&!journal.CanAccept(quest),"Novice received Mage quest");
   xp.RestoreState(10,0);Need(controller.TryAdvance(controller.Definitions.Single(d=>d.targetClass.stableId==AdvancementAuthoring.MagicalId)),controller.Feedback);
   xp.RestoreState(29,0);Need(!mentor.Available(p)&&!journal.CanAccept(quest),"Mage quest below level 30");
   xp.RestoreState(30,0);Protect(p);
   Need(p.Motor.Teleport(mentor.ApproachPosition+Vector3.back*1.8f),"Brewer approach");yield return Frames(2);
   Need(mentor.CanInteract(p),"Brewer not reachable");Need(!controller.TryAdvance(edge),"Quest bypassed");
   var hub=new SemanticGameEventHub();journal.BindDungeonEvents(hub);hub.Publish(new DungeonPuzzleSolvedEvent("dungeon.slime","puzzle.slime-tree"));
   Need(journal.GetProgress(quest.stableId).count==0,"Preaccept puzzle credited");
   Need(mentor.AcceptTraining(p)&&!mentor.AcceptTraining(p),"Quest acceptance");
   hub.Publish(new DungeonBossKilledEvent("dungeon.slime","boss.slime"));hub.Publish(new DungeonPuzzleSolvedEvent("dungeon.slime","puzzle.other"));
   Need(journal.GetProgress(quest.stableId).count==0,"Boss or wrong puzzle credited");
   Need(controller.TrySwitch("class.novice"),controller.Feedback);hub.Publish(new DungeonPuzzleSolvedEvent("dungeon.slime","puzzle.slime-tree"));
   Need(controller.TrySwitch(AdvancementAuthoring.MagicalId),controller.Feedback);Protect(p);
   Need(journal.GetProgress(quest.stableId).status==QuestStatus.Active&&journal.GetProgress(quest.stableId).count==0,"Wrong class puzzle credit or accepted quest lost");
   Phase("real five-ring dungeon puzzle");
   var portal=Object.FindObjectsByType<DungeonPortal>().Single(t=>t.isActiveAndEnabled&&t.definition.stableId=="dungeon.slime");
   Need(p.Motor.Teleport(portal.ApproachPosition+Vector3.back*1.5f)&&portal.CanInteract(p),"Dungeon portal approach");
   portal.Interact(p);while(SlimeDungeonRun.Current==null||!SlimeDungeonRun.Current.StagingReady)yield return null;
   var run=SlimeDungeonRun.Current;Need(run.StartNow(p),"Start dungeon");
   var mini=Object.FindObjectsByType<DungeonSlime>().Single(s=>s.Role==SlimeRole.Miniboss);mini.Actor.Health.ApplyDamage(p,new DamageResult{mitigated=mini.Actor.Health.Maximum*2});yield return Frames(3);
   Need(journal.GetProgress(quest.stableId).count==0,"Miniboss solved Mage quest");
   var puzzle=Object.FindAnyObjectByType<SlimeTreePuzzle>();
   for(int i=0;i<5;i++){
    DungeonSlime slime=null;while(slime==null){slime=Object.FindObjectsByType<DungeonSlime>().FirstOrDefault(s=>s.Role==SlimeRole.Puzzle&&!s.Captured&&s.Actor.Alive&&s.Actor.Motor.Ready);yield return null;}
    Need(slime.Actor.Motor.Teleport(puzzle.rings[i].position),"Puzzle ring navigation "+i);yield return Frames(4);Need(puzzle.Filled==i+1,"Puzzle capture "+i);
   }
   report.puzzleCaptures=puzzle.Filled;
   Need(run.PuzzleSolved&&journal.GetProgress(quest.stableId).status==QuestStatus.ReadyToTurnIn,"Real puzzle event did not complete objective");
   Need(save.CurrentClassId==AdvancementAuthoring.MagicalId&&!controller.CanAdvance(edge),"Puzzle advanced without returning");
   run.SolvePuzzle(false);Need(journal.GetProgress(quest.stableId).count==1,"Puzzle replay duplicated objective");
   run.Abandon();while(SlimeDungeonRun.Current!=null)yield return null;
   Need(save.Flush(),"Ready quest save");Need(controller.TrySwitch("class.novice")&&controller.TrySwitch(AdvancementAuthoring.MagicalId),"Ready quest switch");
   Need(journal.GetProgress(quest.stableId).status==QuestStatus.ReadyToTurnIn,"Ready quest lost");
   Protect(p);Need(p.Motor.Teleport(mentor.ApproachPosition+Vector3.back*1.8f),"Return to brewer");yield return Frames(2);
   long revision=save.Revision;int parentLevel=xp.Level;Need(mentor.CompleteTraining(p),controller.Feedback);
   Need(save.Revision==revision+1&&p.Stats.Definition.stableId==MageProgressionRules.ClassId&&xp.Level==1,"Atomic level-1 Mage advancement");
   var disk=new LocalEchoStore(Path.GetDirectoryName(save.SavePath)).Load();
   Need(ManifestationRoster.ReadManifestation(disk,AdvancementAuthoring.MagicalId).level==parentLevel&&ManifestationRoster.ReadManifestation(disk,MageProgressionRules.ClassId).level==1,"Parent snapshot lost");
   Need(journal.GetProgress(quest.stableId).status==QuestStatus.Completed&&!mentor.CompleteTraining(p),"Quest completion or duplicate turn-in");report.questAdvanced=true;
   yield return Frames(8);
   Need(p.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot.GetComponentInChildren<MageFocusPose>()!=null,"Mage visual missing");
   Phase("dev combat and rank gates");DevelopmentSession.Begin();yield return SceneManager.LoadSceneAsync("DevelopmentRoom");
   while(DevelopmentRoomController.Current==null||DevelopmentRoomController.Current.Map==null)yield return null;
   var room=DevelopmentRoomController.Current;p=room.player;save=p.GetComponent<ManifestationPersistence>();while(!save.Ready||!p.Motor.Ready)yield return null;
   Need(room.SelectClass(MageProgressionRules.ClassId),"Development Mage selection");yield return Frames(8);Protect(p);
   var skills=p.GetComponent<MageSkillProgression>();var caster=p.GetComponent<MageSkillCaster>();var mana=p.GetComponent<ActorResourceController>();xp=p.GetComponent<ExperienceProgression>();
   xp.RestoreState(1,0);Need(skills.Rank(Kind.EmberBolt)==1&&skills.Rank(Kind.FrostSnare)==1&&skills.Rank(Kind.ArcLightning)==0,"Starter ranks");
   Need(!caster.Cast(Kind.ArcLightning,p),"Locked spell cast");Need(!skills.CanRestore(new[]{new SkillRankState(MageSkillProgression.Id(Kind.MathBarrier),5)},1),"Invalid saved rank accepted");
   xp.RestoreState(42,0);skills.RestoreState(MageProgressionRules.Skills.Select(r=>new SkillRankState(MageSkillProgression.Id(r.kind),r.rankLevels.Length)).ToArray());mana.RefillAll();
   var bar=p.GetComponent<AbilityBarSource>();Need(bar.AbilityCount==8,"Mage ability layout not flexible");
   Need(skills.CaptureState().Length==8&&skills.SpentPoints==36,"Mage rank economy");
   Need(Mathf.Approximately(mana.Maximum(ResourceIds.Mana),80+p.Stats.Attributes.intelligence*10+150),"Mana passive maximum");
   var targets=CombatActor.All.Where(t=>t!=p&&t.Stats.Definition.stableId=="dev.practice-target").ToArray();Need(targets.Length==3,"Three chain fixtures");
   foreach(var t in targets){t.Stats.SetEncounterMaximumHp(100000);t.Health.Restore();t.Health.Invulnerable=false;if(t.TryGetComponent<BasicAttack>(out var basic))basic.enabled=false;t.Motor.Stop();}
   Need(p.Motor.Teleport(new Vector3(-10,0,-8))&&targets[0].Motor.Teleport(new Vector3(-10,0,-2))&&targets[1].Motor.Teleport(new Vector3(-7,0,0))&&targets[2].Motor.Teleport(new Vector3(-4,0,-1)),"Spell arena placement");
   caster.ResetForManifestationLoad();float hp=targets[0].Health.Current,mp=mana.Current(ResourceIds.Mana);
   Need(caster.Cast(Kind.EmberBolt,targets[0]),caster.Feedback);Need(caster.WeavingStacks==1&&mana.Current(ResourceIds.Mana)<mp,"Ember cost/stack");
   mp=mana.Current(ResourceIds.Mana);Need(!caster.Cast(Kind.EmberBolt,targets[0])&&Mathf.Approximately(mp,mana.Current(ResourceIds.Mana))&&caster.WeavingStacks==1,"Rejected cast changed Mana/stacks");yield return Wait(.8f);
   Need(targets[0].Health.Current<hp,"Ember projectile did not hit");yield return Wait(4.4f);
   float oldDecay=caster.DecayStartsAt;Need(caster.Cast(Kind.EmberBolt,targets[0])&&caster.WeavingStacks==1&&caster.DecayStartsAt>oldDecay+4,"Repeated element did not refresh");
   float cool=caster.CooldownRemaining(Kind.EmberBolt),speed=targets[0].Stats.MoveSpeed;
   Need(caster.Cast(Kind.FrostSnare,targets[0]),caster.Feedback);Need(caster.WeavingStacks==2&&caster.CooldownRemaining(Kind.EmberBolt)<cool-.15f,"Unique element or other cooldown reduction");Need(targets[0].Stats.MoveSpeed<speed,"Frost slow missing");
   var health=targets.Select(t=>t.Health.Current).ToArray();Need(caster.Cast(Kind.ArcLightning,targets[0]),caster.Feedback);
   Need(targets.Select((t,i)=>t.Health.Current<health[i]).All(x=>x)&&caster.WeavingStacks==3,"Chain hit count or duplicate stacks");report.chainTargets=3;
   Need(caster.CastQuicksand(targets[0].transform.position),caster.Feedback);yield return Wait(1.1f);Need(caster.WeavingStacks==4,"Quicksand ticks added stacks");
   Need(caster.Cast(Kind.HalfLife,targets[0]),caster.Feedback);Need(Mathf.Approximately(targets[0].Stats.DefenseModifiers(DamageChannel.Physical).reductionFlat,25)&&Mathf.Approximately(targets[0].Stats.DefenseModifiers(DamageChannel.Magical).reductionFlat,25),"Half-Life DEF debuff");
   hp=targets[0].Health.Current;yield return Wait(1.1f);Need(targets[0].Health.Current<hp&&caster.WeavingStacks==5,"Radioactive DoT or periodic stack duplication");
   Need(caster.Cast(Kind.MathBarrier),caster.Feedback);Need(caster.WeavingStacks==6&&caster.BarrierRemaining>9.8f,"Math sixth element or duration");report.uniqueElements=caster.WeavingStacks;
   Phase("Math Barrier packet behavior");p.Health.Invulnerable=false;mana.SetCurrentForValidation(ResourceIds.Mana,1000);hp=p.Health.Current;
   var packet=p.Health.ApplyPacket(targets[0],new DamageResult{mitigated=100});report.fullBarrierHp=hp-p.Health.Current;
   Need(Mathf.Approximately(packet.applied,50)&&Mathf.Approximately(packet.absorbed,50)&&Mathf.Approximately(mana.Current(ResourceIds.Mana),925),"Full Mana Barrier fraction/efficiency");
   p.Health.AddTemporaryShield("qa.shield",20,3);mana.SetCurrentForValidation(ResourceIds.Mana,1000);
   packet=p.Health.ApplyPacket(targets[0],new DamageResult{mitigated=100});Need(Mathf.Approximately(packet.applied,40)&&Mathf.Approximately(packet.absorbed,60)&&Mathf.Approximately(mana.Current(ResourceIds.Mana),940),"Shield priority");
   mana.SetCurrentForValidation(ResourceIds.Mana,.75f);packet=p.Health.ApplyPacket(targets[0],new DamageResult{mitigated=100});report.partialBarrierHp=packet.applied;
   Need(Mathf.Approximately(packet.applied,99.5f)&&Mathf.Approximately(mana.Current(ResourceIds.Mana),0),"Partial Mana swallowed damage");
   packet=p.Health.ApplyPacket(targets[0],new DamageResult{mitigated=100});Need(Mathf.Approximately(packet.applied,100)&&packet.absorbed==0,"Zero Mana still protected");
   mp=mana.Current(ResourceIds.Mana);p.Health.ApplyPacket(targets[0],new DamageResult{mitigated=-20,element=caster.Tuning.attacks[5].element});Need(Mathf.Approximately(mp,mana.Current(ResourceIds.Mana)),"Resistance healing spent Mana");
   p.Health.Invulnerable=true;caster.ResetForManifestationLoad();Need(targets[0].Stats.DefenseModifiers(DamageChannel.Physical).reductionFlat==0,"Owned debuff leaked on reset");
   mana.RefillAll();var ranks=skills.CaptureState();skills.RestoreState(ranks.Select(r=>new SkillRankState(r.stableId,r.stableId==MageSkillProgression.Id(Kind.MathBarrier)?1:r.rank)).ToArray());
   Need(caster.Cast(Kind.MathBarrier)&&caster.BarrierRemaining<=2.01f,"Rank-1 duration");yield return Wait(2.15f);Need(caster.BarrierRemaining==0,"Barrier expiry");skills.RestoreState(ranks);
   Phase("Weaving refresh and gradual decay");caster.ResetForManifestationLoad();mana.RefillAll();
   Need(caster.Cast(Kind.EmberBolt,targets[0])&&caster.Cast(Kind.FrostSnare,targets[0])&&caster.Cast(Kind.ArcLightning,targets[0]),"Decay fixture casts");yield return Wait(7.5f);Need(caster.WeavingStacks==3,"Stacks decayed before 8 seconds");
   Need(caster.Cast(Kind.EmberBolt,targets[0])&&caster.WeavingStacks==3,"Repeat added a duplicate element");yield return Wait(7.5f);Need(caster.WeavingStacks==3,"Refresh did not extend the shared delay");yield return Wait(.7f);Need(caster.WeavingStacks==2,"First decay");yield return Wait(1.05f);Need(caster.WeavingStacks==1,"Gradual decay");yield return Wait(1.05f);Need(caster.WeavingStacks==0,"Full decay");
   Phase("walking, equipment and save");caster.ResetForManifestationLoad();mana.RefillAll();var driver=p.GetComponent<NovicePresentationDriver>();var model=driver.VisualAnimator;
   Need(model!=null&&model.isHuman,"Humanoid Mage");Need(p.Motor.Teleport(new Vector3(0,0,-10))&&p.Motor.MoveTo(new Vector3(0,0,0)),"Mage walking route");var start=p.transform.position;var foot=model.GetBoneTransform(HumanBodyBones.LeftFoot);var initial=p.transform.InverseTransformPoint(foot.position);
   float travel=0;float until=Time.time+2;while(Time.time<until){travel=Mathf.Max(travel,Vector3.Distance(initial,p.transform.InverseTransformPoint(foot.position)));yield return null;}
   report.walkDistance=Vector3.Distance(start,p.transform.position);Need(report.walkDistance>3&&travel>.04f,"Mage sliding/frozen walk");p.Motor.Stop();
   var eq=p.GetComponent<Equipment>();var inv=p.GetComponent<CarriedInventory>();var presentation=p.GetComponent<EquipmentPresentation>();
   foreach(var d in inv.Definitions.Where(d=>d!=null&&d.modelTestingItem)){
    var item=inv.Items.FirstOrDefault(i=>i.definitionId==d.stableId)??inv.Grant(d.stableId);Need(eq.Equip(item.instanceId,d.slot),"Mage equip "+d.slot);
   }yield return Frames(5);Need(eq.Slots.Length==11&&eq.Slots.All(eq.IsResolved),"All Mage slots");
   var visual=p.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot;Need(visual.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="Mage Hat"),"Original Mage hat missing");
   Need(!visual.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Mage Hat").gameObject.activeInHierarchy,"Equipped head did not suppress default hat");
   Need(visual.GetComponentsInChildren<Collider>(true).Length==0,"Mage art changed collision");
   mana.RefillAll();float savedMana=mana.Current(ResourceIds.Mana);
   Need(save.Flush(),"Mage skill/gear save");disk=new LocalEchoStore(Path.GetDirectoryName(save.SavePath)).Load();var saved=ManifestationRoster.ReadManifestation(disk,MageProgressionRules.ClassId);Need(saved.classSkills.Length==8&&saved.equipment.Length==11,"Mage durable state");
   controller=p.GetComponent<AdvancementController>();Need(controller.TrySwitch(AdvancementAuthoring.MagicalId)&&controller.TrySwitch(MageProgressionRules.ClassId),"Mage manifestation round trip");yield return Frames(6);Need(skills.CaptureState().Length==8&&eq.Slots.Length==11&&caster.WeavingStacks==0&&caster.BarrierRemaining==0,"Mage restored/transient split");
   Need(Mathf.Abs(mana.Current(ResourceIds.Mana)-savedMana)<.1f,$"Passive Mana lost during manifestation restore: saved {savedMana}, restored {mana.Current(ResourceIds.Mana)}, max {mana.Maximum(ResourceIds.Mana)}");
   foreach(var slot in eq.Slots)eq.Unequip(slot.slotId);Need(p.Motor.Teleport(new Vector3(0,0,-8)),"Mage visual placement");p.transform.rotation=Quaternion.Euler(0,180,0);
   room.explorationCamera.RecenterForTravel(p.transform);yield return Frames(8);
   Phase("visual_ready");SessionState.SetString(Key+"status","visual_ready");
   while(!SessionState.GetBool(Key+"resume",false)){deadline=EditorApplication.timeSinceStartup+300;yield return null;}
   Need(save.Flush(),"Final Mage save");Need(p.GetComponent<ReturnToStartMenu>().Return(),"Return menu");
   StartMenuController menu=null;while(menu==null||!menu.HasLoadedProfile){menu=Object.FindAnyObjectByType<StartMenuController>();yield return null;}
   yield return Frames(8);Need(menu.SelectedTier==2&&menu.EntriesForTier(2).Any(e=>e.classId==MageProgressionRules.ClassId&&e.active),"Mage Tier II menu");
   Need(menu.PortraitStage.Portraits.Any(p=>p.classId==MageProgressionRules.ClassId&&p.rendered),"Mage portrait did not render");
  }
  static void Protect(CombatActor p){p.Stats.SetEncounterMaximumHp(100000);p.Health.Restore();p.Health.Invulnerable=true;}
 }
}
