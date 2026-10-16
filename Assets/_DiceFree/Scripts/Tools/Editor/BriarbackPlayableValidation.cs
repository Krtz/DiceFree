using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;
using DiceFree.Core;using DiceFree.Advancement;using DiceFree.Characters;using DiceFree.Combat;using DiceFree.Development;using DiceFree.Dungeons;using DiceFree.Items;using DiceFree.Persistence;using DiceFree.Progression;using DiceFree.Quests;using DiceFree.UI;using DiceFree.Visuals;using DiceFree.World;using DiceFree.AI;using DiceFree.Briarback;
using UnityEngine;using UnityEngine.AI;using UnityEditor;using UnityEditor.SceneManagement;using Unity.Pipeline.Commands;using Object=UnityEngine.Object;
namespace DiceFree.EditorTools
{
 [InitializeOnLoad] public static class BriarbackPlayableValidation
 {
  const string Key="DiceFree.Briarback.";static readonly Stack<IEnumerator> steps=new();static double deadline;static int checks;
  [Serializable]sealed class Report{public string status,error;public int checks,walked,kills,slamHits;public float slamRaw,walkMeters,animatedFootDegrees;public bool rewardPersisted,menuPortrait;}
  static Report report;
  static BriarbackPlayableValidation()=>EditorApplication.playModeStateChanged+=Changed;
  static void Need(bool value,string text){if(!value)throw new InvalidOperationException(text);checks++;}
  static void Phase(string p){SessionState.SetString(Key+"phase",p);SessionState.SetInt(Key+"checks",checks);}
  [CliCommand("dicefree.briarback.test","Walk the real northern route, verify level-independent pursuit, elite dodge/interrupt, quest/reward and equipped save/menu.")]
  public static object Start(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode only");
   SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
   SessionState.SetBool(Key+"cleanup",true);SessionState.SetBool(Key+"resume",false);SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"phase","initializing");SessionState.SetString(Key+"error","");SessionState.SetInt(Key+"checks",0);
   string root=Path.Combine(Path.GetTempPath(),"DiceFree-Briarback-"+Guid.NewGuid().ToString("N"));SessionState.SetString(Key+"root",root);PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);EditorSettings.enterPlayModeOptionsEnabled=false;
   EditorSceneManager.OpenScene(BriarbackAuthoring.Scene);EditorApplication.EnterPlaymode();return Status();
  }
  [CliCommand("dicefree.briarback.status","Read the focused region verification.")]
  public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),phase=SessionState.GetString(Key+"phase",""),checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error","")};
  [CliCommand("dicefree.briarback.resume","Finish saved equipped-menu checks after region visual capture.")]
  public static object Resume(){SessionState.SetBool(Key+"resume",true);return Status();}
  static void Changed(PlayModeStateChange s){
   if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running"){checks=0;report=new Report();steps.Clear();steps.Push(Flow());deadline=EditorApplication.timeSinceStartup+600;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
   if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false)){
    SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;PersistenceTestGuard.ClearValidationOverrides();
    EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);
    string p=SessionState.GetString(Key+"scene","");if(!string.IsNullOrWhiteSpace(p))EditorSceneManager.OpenScene(p);
   }
  }
  static void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)Finish(false,text+"\n"+stack);}
  static void Tick(){if(!EditorApplication.isPlaying)return;try{
   if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Briarback validation timed out");
   while(steps.Count>0){var step=steps.Peek();if(!step.MoveNext()){steps.Pop();continue;}if(step.Current is IEnumerator child){steps.Push(child);continue;}return;}Finish(true,null);
  }catch(Exception e){Finish(false,e.ToString());}}
  static void Finish(bool pass,string error){
   EditorApplication.update-=Tick;Application.logMessageReceived-=Log;report??=new Report();report.status=pass?"passed":"failed";report.error=error;report.checks=checks;
   SessionState.SetString(Key+"status",report.status);SessionState.SetString(Key+"error",error??"");SessionState.SetInt(Key+"checks",checks);File.WriteAllText("Library/BriarbackPlayableResults.json",JsonUtility.ToJson(report,true));EditorApplication.ExitPlaymode();
  }
  static IEnumerator Wait(float t){float end=Time.time+t;while(Time.time<end)yield return null;}
  static IEnumerator Frames(int n){for(int i=0;i<n;i++)yield return null;}
  static Vector3 Point(Vector2 p){Need(NavMesh.SamplePosition(new Vector3(p.x,0,p.y),out var hit,2,1),"No live navigation at "+p);return hit.position;}
  static IEnumerator Walk(CombatActor p,Vector2 point){
   var target=Point(point);Need(p.Motor.MoveTo(target),"Move order rejected at "+point);float until=Time.time+18;Vector3 previous=p.transform.position;
   while(Vector3.Distance(p.transform.position,target)>1.0f&&Time.time<until){report.walkMeters+=Vector3.Distance(previous,p.transform.position);previous=p.transform.position;yield return null;}
   Need(Vector3.Distance(p.transform.position,target)<=1.0f,"Walking blocked at "+point+" from "+p.transform.position);p.Motor.Stop();report.walked++;
  }
  static long TotalXp(ExperienceProgression xp){long total=xp.CurrentXp;for(int i=1;i<xp.Level;i++)total+=30+3L*(i-1)*(i-1);return total;}
  static void Defeat(CombatActor p,CombatActor target){target.Health.Invulnerable=false;target.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});Need(!target.Alive,"Scripted fixture defeat failed");}
  static IEnumerator Flow(){
   CombatActor p=null;while(p==null){p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();yield return null;}
   var save=p.GetComponent<ManifestationPersistence>();while(!save.Ready||!p.Motor.Ready)yield return null;
   p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.GetComponent<BasicAttack>().enabled=false;p.Health.Invulnerable=true;
   var xp=p.GetComponent<ExperienceProgression>();xp.RestoreState(24,0);p.Health.Restore();
   var creatures=Object.FindObjectsByType<QuilboarCombat>();Need(creatures.Length==13,"Live creature count");
   foreach(var c in creatures){c.GetComponent<AggroBehaviour>().enabled=false;c.GetComponent<BasicAttack>().enabled=false;c.enabled=false;}
   var giver=Object.FindObjectsByType<QuestGiver>().Single(q=>q.name=="Scout Runa - northern trail");var quest=giver.Quest;var journal=p.GetComponent<QuestJournal>();
   var first=creatures.First(c=>c.role==QuilboarRole.Raider).GetComponent<CombatActor>();
   Phase("quest prerequisites, no early credit and real northern walking");
   Defeat(p,first);Need(journal.GetProgress(quest.stableId).status==QuestStatus.Available&&journal.GetProgress(quest.stableId).count==0,"Earlier kills credited");
   Need(first.GetComponent<OverworldRespawn>().Remaining>60,"Authored respawn timer");first.Health.Restore();
   Need(p.Motor.Teleport(Point(new Vector2(250,182))),"Trimery setup");yield return Wait(.7f);Need(p.GetComponent<RespawnAtAnchor>().AnchorId==TrimeryVillageAuthoring.AnchorId,"Trimery stone not retained");
   yield return Walk(p,new Vector2(242,199));Need(giver.CanInteract(p)&&giver.Accept(p),"Scout quest not reachable/acceptable");
   Need(!giver.TurnIn(p),"Unfinished quest granted reward");
   foreach(var point in BriarbackAuthoring.Route)yield return Walk(p,point);
   Need(journal.GetProgress(quest.stableId).stage==1&&journal.GetProgress(quest.stableId).count==0,"Actual watchpost trigger did not advance quest");
   var fog=Object.FindAnyObjectByType<WorldFogOfWar>();Need(fog.MapMax==new Vector2(353,406)&&fog.IsVisible(p.transform.position),"Northern fog/revealer bounds");
   Need(Object.FindAnyObjectByType<ExplorationCamera>().MapBounds==BriarbackAuthoring.Bounds,"Camera map bounds");
   foreach(var point in BriarbackAuthoring.Route.Reverse().Skip(1))yield return Walk(p,point);
   Phase("level-independent pursuit, skinned walking and leash");
   var aggro=first.GetComponent<AggroBehaviour>();var basic=first.GetComponent<BasicAttack>();xp.RestoreState(200,0);p.Health.Restore();
   bool placed=false;for(int i=0;i<12;i++){
    var trial=aggro.Home+new Vector3(Mathf.Sin(i*Mathf.PI/6)*9,0,Mathf.Cos(i*Mathf.PI/6)*9);
    if(!NavMesh.SamplePosition(trial,out var hit,2,1)||!p.Motor.Teleport(hit.position))continue;
    Physics.SyncTransforms();var path=new NavMeshPath();
    if(first.HasSightOf(p)&&NavMesh.CalculatePath(first.transform.position,p.transform.position,1,path)&&path.status==NavMeshPathStatus.PathComplete){placed=true;break;}
   }
   Need(placed,"No visible connected pursuit fixture around camp");
   aggro.enabled=true;basic.enabled=true;var model=first.GetComponent<QuilboarPresentation>().model;var leg=model.GetComponentsInChildren<Transform>().Single(t=>t.name=="LeftUpLeg");var rotation=leg.localRotation;
   Vector3 start=first.transform.position;float end=Time.time+4;
   while(Time.time<end){report.animatedFootDegrees=Mathf.Max(report.animatedFootDegrees,Quaternion.Angle(rotation,leg.localRotation));yield return null;}
   Need(basic.Target==p&&!aggro.Returning&&Vector3.Distance(start,first.transform.position)>2,"Higher-level player not pursued: state="+aggro.State+" sight="+first.HasSightOf(p)+" moved="+Vector3.Distance(start,first.transform.position)+" target="+basic.Target?.name);
   Need(report.animatedFootDegrees>4,"Walking creature pose frozen");
   Need(p.Motor.Teleport(Point(new Vector2(aggro.Home.x,aggro.Home.z+40))),"Leash placement");
   yield return Wait(.7f);Need(aggro.Returning||Vector3.Distance(first.transform.position,aggro.Home)<.6f,"Leash did not return");
   yield return Wait(6);Need(basic.Target==null&&Vector3.Distance(first.transform.position,aggro.Home)<1,"Leash return incomplete");
   aggro.enabled=false;basic.enabled=false;xp.RestoreState(24,0);p.Health.Restore();
   Phase("accepted kill credit and elite telegraph");
   foreach(var c in creatures.Where(c=>c.role!=QuilboarRole.Harrowtusk).Take(8)){
    var target=c.GetComponent<CombatActor>();Need(p.Motor.Teleport(Point(new Vector2(target.transform.position.x,target.transform.position.z+4))),"Kill credit placement");
    Defeat(p,target);report.kills++;
   }
   Need(journal.GetProgress(quest.stableId).stage==2&&journal.GetProgress(quest.stableId).count==0,"Eight kills did not advance to elite");
   var elite=creatures.Single(c=>c.role==QuilboarRole.Harrowtusk);var boss=elite.GetComponent<CombatActor>();elite.enabled=true;
   Need(p.Motor.Teleport(Point(new Vector2(boss.transform.position.x,boss.transform.position.z+2))),"Slam placement");
   yield return Wait(elite.CooldownRemaining+.1f);int hits=elite.SlamHits;Need(elite.BeginSlam()&&elite.Winding&&!boss.CanAct,"Slam did not telegraph/lock own motion");
   Need(p.Motor.MoveTo(Point(new Vector2(boss.transform.position.x,boss.transform.position.z+10))),"Dodge walk order");yield return Wait(1.3f);
   Need(elite.SlamHits==hits,"Walking outside warning did not dodge");p.Motor.Stop();
   yield return Wait(elite.CooldownRemaining+.1f);Need(p.Motor.Teleport(Point(new Vector2(boss.transform.position.x,boss.transform.position.z+2))),"Hit placement");
   float raw=0;Action<CombatActor,DamageResult> capture=(source,result)=>{if(source==boss)raw=result.raw;};p.Health.PacketResolved+=capture;p.Health.Invulnerable=false;p.Health.Restore();
   Need(elite.BeginSlam(),"Second slam");yield return Wait(1.3f);p.Health.PacketResolved-=capture;p.Health.Invulnerable=true;
   Need(elite.SlamHits==hits+1&&Mathf.Approximately(raw,160)&&p.Alive,"Slam raw damage or target count");report.slamRaw=raw;report.slamHits=elite.SlamHits;
   yield return Wait(elite.CooldownRemaining+.1f);Need(elite.BeginSlam()&&boss.Effects.TryApplyStun(4,false),"Interrupt fixture");yield return Wait(.2f);Need(!elite.Winding,"Interrupted slam continued");yield return Wait(2.1f);Need(boss.CanAct,"Slam action lease leaked after interrupt");
   elite.enabled=false;Defeat(p,boss);report.kills++;Need(journal.GetProgress(quest.stableId).status==QuestStatus.ReadyToTurnIn,"Elite did not ready quest");
   Phase("return, exactly-once gear reward and persistence");
   Need(p.Motor.Teleport(Point(new Vector2(241,324))),"Return trail setup");foreach(var point in BriarbackAuthoring.Route.Take(7).Reverse())yield return Walk(p,point);
   yield return Walk(p,new Vector2(242,199));var inv=p.GetComponent<CarriedInventory>();var wallet=p.GetComponent<GoldWallet>();int owned=inv.Items.Length;long gold=wallet.Gold,experience=TotalXp(xp);
   Need(giver.TurnIn(p)&&journal.GetProgress(quest.stableId).status==QuestStatus.Completed,"Quest turn-in failed");
   Need(inv.Items.Length==owned+1&&wallet.Gold==gold+150&&TotalXp(xp)==experience+1800,"Reward quantities");
   Need(!giver.TurnIn(p)&&inv.Items.Length==owned+1&&wallet.Gold==gold+150,"Duplicate reward");
   var mantle=inv.Items.Single(i=>i.definitionId==BriarbackMantleVisual.ItemId);Need(mantle.bound||inv.Resolve(mantle.definitionId).bound,"Quest mantle binding");
   var equipment=p.GetComponent<Equipment>();float vitality=p.Stats.Attributes.vitality;Need(equipment.Equip(mantle.instanceId,EquipmentSlot.Back),"Equip reward");yield return Frames(4);Need(Mathf.Approximately(p.Stats.Attributes.vitality-vitality,3),"Reward stats");
   var visual=p.GetComponent<ClassPresentationSwitcher>().ActiveVisualRoot;Need(visual.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Bristlehide Mantle").gameObject.activeInHierarchy,"Live reward model");
   Need(save.Flush(),"Quest/gear save");var disk=new LocalEchoStore(Path.GetDirectoryName(save.SavePath)).Load();var state=ManifestationRoster.ReadManifestation(disk,"class.novice");
   report.rewardPersisted=state.quests.Single(q=>q.questId==quest.stableId).status==QuestStatus.Completed&&state.equipment.Any(e=>e.instanceId==mantle.instanceId);Need(report.rewardPersisted,"On-disk quest/equipment");
   boss.Health.Restore();Need(p.Motor.Teleport(Point(new Vector2(308,330))),"Visual placement");p.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(boss.transform.position-p.transform.position,Vector3.up));p.Health.Restore();p.GetComponent<TargetSelection>().Select(boss);
   Object.FindAnyObjectByType<ExplorationCamera>().RecenterForTravel(p.transform);yield return Frames(10);
   Phase("visual_ready");SessionState.SetString(Key+"status","visual_ready");while(!SessionState.GetBool(Key+"resume",false)){deadline=EditorApplication.timeSinceStartup+300;yield return null;}
   Need(save.Flush()&&p.GetComponent<ReturnToStartMenu>().Return(),"Return menu");StartMenuController menu=null;
   while(menu==null||!menu.HasLoadedProfile){menu=Object.FindAnyObjectByType<StartMenuController>();yield return null;}yield return Frames(10);
   var portrait=menu.PortraitStage.Portraits.Single(x=>x.classId=="class.novice");report.menuPortrait=portrait.rendered&&portrait.equippedIds.Contains(BriarbackMantleVisual.ItemId);
   Need(report.menuPortrait&&portrait.root.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Bristlehide Mantle").gameObject.activeInHierarchy,"Saved equipped menu portrait");
  }
 }
}
