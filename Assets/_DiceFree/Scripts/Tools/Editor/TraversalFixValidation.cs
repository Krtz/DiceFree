using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Development;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using DiceFree.World;
namespace DiceFree.EditorTools
{
 [InitializeOnLoad] public static class TraversalFixValidation
 {
  const string Key="DiceFree.TraversalFix.";
  static readonly Stack<IEnumerator> steps=new();
  static readonly List<WalkRow> rows=new();
  static int checks;static double deadline;
  [Serializable] public sealed class WalkRow {
   public string classId,clip;public bool loop;
   public float distance,lateFootTravel,lateLegAngle,directDistance,rate,idleSpeed;
  }
  [Serializable] sealed class Report {public string status,error;public int checks;public WalkRow[] walks;public float dashDistance,wallStopZ,friendlyStopZ;public int flips,npcWalkers;public float[] npcLegAngles;}
  static Report report;
  static TraversalFixValidation()=>EditorApplication.playModeStateChanged+=Changed;
  static void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);checks++;}
  [CliCommand("dicefree.traversal-fixes.test","Check all four looping walk poses, stop/recovery, enemy-only Spin Dash, walls and pad-triggered round trips.")]
  public static object Start(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode.");
   SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);
   SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);
   SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
   SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");
   SessionState.SetBool(Key+"cleanup",true);
   PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(Path.Combine(Path.GetTempPath(),"DiceFree-TraversalFix-"+Guid.NewGuid().ToString("N")));
   EditorSettings.enterPlayModeOptionsEnabled=false;EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/DevelopmentRoom.unity");
   EditorApplication.EnterPlaymode();return Status();
  }
  [CliCommand("dicefree.traversal-fixes.status","Read locomotion, dash and seam regression results.")]
  public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error","")};
  static void Changed(PlayModeStateChange s){
   if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running"){
    checks=0;rows.Clear();report=new Report();steps.Clear();steps.Push(Flow());deadline=EditorApplication.timeSinceStartup+150;
    EditorApplication.update+=Tick;Application.logMessageReceived+=Log;
   }
   if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false)){
    SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;
    PersistenceTestGuard.ClearValidationOverrides();EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);
    EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);
    string path=SessionState.GetString(Key+"scene","");if(!string.IsNullOrEmpty(path))EditorSceneManager.OpenScene(path);
   }
  }
  static void Log(string m,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)Finish(false,m+"\n"+stack);}
  static void Tick(){if(!EditorApplication.isPlaying)return;try{
   if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Traversal test timed out.");
   while(steps.Count>0){var s=steps.Peek();if(!s.MoveNext()){steps.Pop();continue;}if(s.Current is IEnumerator e){steps.Push(e);continue;}return;}
   Finish(true,null);
  }catch(Exception e){Finish(false,e.ToString());}}
  static void Finish(bool pass,string error){
   EditorApplication.update-=Tick;Application.logMessageReceived-=Log;
   report.status=pass?"passed":"failed";report.error=error;report.checks=checks;report.walks=rows.ToArray();
   SessionState.SetString(Key+"status",report.status);SessionState.SetString(Key+"error",error??"");SessionState.SetInt(Key+"checks",checks);
   File.WriteAllText("Library/TraversalFixResults.json",JsonUtility.ToJson(report,true));EditorApplication.ExitPlaymode();
  }
  static IEnumerator Wait(float seconds){float until=Time.time+seconds;while(Time.time<until)yield return null;}
  static IEnumerator WaitDash(GreatbladeSkillCaster caster){float until=Time.realtimeSinceStartup+5;while(caster.Busy){NeedTime(until);yield return null;}yield return null;}
  static void NeedTime(float until){if(Time.realtimeSinceStartup>until)throw new TimeoutException("Timed out waiting for motion.");}
  static IEnumerator Flow(){
   while(DevelopmentRoomController.Current==null||DevelopmentRoomController.Current.Map==null)yield return null;
   var room=DevelopmentRoomController.Current;var p=room.player;
   var driver=p.GetComponent<NovicePresentationDriver>();
   foreach(string id in new[]{"class.novice",PhysicalBlessedAuthoring.ClassId,MagicalTouchedAuthoring.ClassId,GreatbladeProgressionRules.ClassId}){
    Need(room.SelectClass(id),"Select "+id);yield return Wait(.2f);
    Need(p.Motor.Teleport(new Vector3(-10,0,-6)),"Walk spawn "+id);
    var a=driver.VisualAnimator;Need(a!=null&&a.isHuman,"Humanoid animator "+id);
    Need(p.Motor.MoveTo(new Vector3(-10,0,12)),"Click walk "+id);
    yield return Wait(1.2f);
    var foot=a.GetBoneTransform(HumanBodyBones.LeftFoot);var leg=a.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
    var first=p.transform.InverseTransformPoint(foot.position);var rotation=leg.localRotation;
    var row=new WalkRow{classId=id};var start=p.transform.position;float until=Time.time+1.2f;
    while(Time.time<until){
     row.lateFootTravel=Mathf.Max(row.lateFootTravel,Vector3.Distance(first,p.transform.InverseTransformPoint(foot.position)));
     row.lateLegAngle=Mathf.Max(row.lateLegAngle,Quaternion.Angle(rotation,leg.localRotation));yield return null;
    }
    row.distance=Vector3.Distance(start,p.transform.position);var clip=a.GetCurrentAnimatorClipInfo(0).First().clip;
    row.clip=clip.name;row.loop=clip.isLooping;row.rate=a.speed;
    Need(row.loop&&row.lateFootTravel>.04f&&row.lateLegAngle>2,"Frozen/non-looping feet after first cycle: "+id);
    Need(row.distance>3,"Click movement "+id);
    int walkHash=a.GetCurrentAnimatorStateInfo(0).fullPathHash;
    int idleHash=Animator.StringToHash(id==GreatbladeProgressionRules.ClassId?"Base Layer.Greatblade two-hand ready":"Base Layer.Idle");
    a.Play(idleHash,0,0);yield return Wait(.45f);
    Need(a.GetCurrentAnimatorStateInfo(0).fullPathHash==walkHash,"Stale idle did not recover to walk "+id);
    p.Motor.Stop();yield return Wait(.7f);row.idleSpeed=driver.LastDrivenSpeed;
    Need(row.idleSpeed<.1f&&a.GetCurrentAnimatorStateInfo(0).fullPathHash==idleHash,"Stop did not idle "+id);
    Need(p.Motor.Teleport(new Vector3(-10,0,-6)),"Direct spawn");
    start=p.transform.position;until=Time.time+.8f;
    while(Time.time<until){p.Motor.MoveDirect(Vector3.forward,Time.deltaTime);yield return null;}
    row.directDistance=Vector3.Distance(start,p.transform.position);
    Need(row.directDistance>2.5f&&driver.LastDrivenSpeed>.2f,"Direct movement animation "+id);
    p.Motor.Stop();yield return Wait(.4f);rows.Add(row);
   }
   yield return Dash(room);
   // Use the same ground command as click/minimap movement, ending at the visible pad.
   var cube=room.GetComponent<CubeEdgePresentation>();string identity=p.GetEntityId().ToString();
   Need(p.Motor.Teleport(cube.Edge.Origin),"Cube centre");
   Need(p.GetComponent<TraversalInput>().TryMoveFromMinimap(cube.Edge.Approach),"Walk onto visible World 2 pad");
   float end=Time.realtimeSinceStartup+20;
   while(room.CompletedTransitions<1||room.Busy){NeedTime(end);yield return null;}
   Need(room.Map.worldId=="world.test.2"&&cube.CompletedFlips==1,"Pad did not automatically cross to World 2");
   yield return Wait(.6f);Need(room.CompletedTransitions==1,"Arrival bounced back");
   Need(p.GetComponent<TraversalInput>().TryMoveFromMinimap(cube.Edge.Origin),"Step inward to rearm return edge");
   end=Time.realtimeSinceStartup+8;while(Vector3.Distance(p.transform.position,cube.Edge.Origin)>1){NeedTime(end);yield return null;}
   Need(p.GetComponent<TraversalInput>().TryMoveFromMinimap(cube.Edge.Approach),"Walk onto return pad");
   end=Time.realtimeSinceStartup+20;while(room.CompletedTransitions<2||room.Busy){NeedTime(end);yield return null;}
   Need(room.Map.worldId=="world.test.1"&&cube.CompletedFlips==2&&p.GetEntityId().ToString()==identity,"Pad round trip changed actor or failed");
   report.flips=cube.CompletedFlips;
   yield return SceneManager.LoadSceneAsync("Cornberg");
   yield return Wait(.5f);
   var villagers=UnityEngine.Object.FindObjectsByType<VillageWalkingAnimator>();
   Need(villagers.Length==4,"Four rigged Cornberg wanderers");
   var origins=villagers.Select(v=>v.transform.position).ToArray();
   var legs=villagers.Select(v=>v.Model.GetComponentsInChildren<Transform>(true).First(t=>t.name=="LeftUpperLeg")).ToArray();
   var rotations=legs.Select(l=>l.localRotation).ToArray();
   var moved=new bool[4];var stepped=new bool[4];var angles=new float[4];var retry=new float[4];
   end=Time.realtimeSinceStartup+25;
   while(Time.realtimeSinceStartup<end&&(!moved.All(x=>x)||!stepped.All(x=>x)||angles.Any(x=>x<5))){
    for(int i=0;i<4;i++){
     var v=villagers[i];var wander=v.GetComponent<VillageHomeWander>();
     if(!wander.Walking&&Time.time>=retry[i]){wander.TryChoosePath();retry[i]=Time.time+1;}
     moved[i]|=Vector3.Distance(origins[i],v.transform.position)>.25f;
     stepped[i]|=v.MeasuredSpeed>.2f&&v.Model.GetCurrentAnimatorStateInfo(0).IsName("Walk");
     angles[i]=Mathf.Max(angles[i],Quaternion.Angle(rotations[i],legs[i].localRotation));
    }
    yield return null;
   }
   for(int i=0;i<4;i++){
    Need(moved[i]&&stepped[i]&&angles[i]>5,"Villager slid without animated legs: "+villagers[i].name);
    Need(!villagers[i].Model.applyRootMotion,"NPC animation moved gameplay root");
    villagers[i].GetComponent<VillageHomeWander>().enabled=false;
   }
   yield return Wait(.8f);
   Need(villagers.All(v=>v.MeasuredSpeed<.05f&&v.Model.GetCurrentAnimatorStateInfo(0).IsName("Idle")),"Stopped villagers did not idle");
   report.npcWalkers=4;report.npcLegAngles=angles;
  }
  static IEnumerator Dash(DevelopmentRoomController room){
   var p=room.player;var caster=p.GetComponent<GreatbladeSkillCaster>();var nav=p.GetComponent<NavMeshAgent>();
   p.GetComponent<BasicAttack>().Cancel();p.Health.Invulnerable=true;
   var targets=CombatActor.All.Where(a=>a!=p&&a.Stats.Definition.stableId=="dev.practice-target").ToArray();
   Need(targets.Length==3,"Three body-block fixtures");
   foreach(var t in targets){t.Health.Restore();t.Configure(1,.5f);t.Motor.Stop();}
   Vector3 start=new(-10,0,-8),destination=new(-10,0,-2);
   Need(p.Motor.Teleport(start),"Dash start");
   Need(targets[0].Motor.Teleport(new Vector3(-10,0,-6))&&targets[1].Motor.Teleport(new Vector3(-10,0,-4))&&targets[2].Motor.Teleport(new Vector3(10,0,10)),"Enemy lane");
   float hp0=targets[0].Health.Current,hp1=targets[1].Health.Current;
   caster.ResetForManifestationLoad();Need(caster.CastSpinDash(destination),caster.Feedback);yield return WaitDash(caster);
   report.dashDistance=Vector3.Distance(start,p.transform.position);
   Need(Vector3.Distance(p.transform.position,destination)<.4f&&report.dashDistance>5.5f,"Spin Dash stuck at enemy bodies");
   Need(targets[0].Health.Current<hp0&&targets[1].Health.Current<hp1&&caster.LastCastHits==2,"Enemies along dash not hit exactly once");
   Need(!p.CanPassThrough(targets[0])&&nav.obstacleAvoidanceType==ObstacleAvoidanceType.HighQualityObstacleAvoidance,"Dash passage/avoidance leaked");
   caster.ResetForManifestationLoad();Need(p.Motor.Teleport(start),"Friendly fixture start");
   targets[0].Configure(p.Faction,.5f);targets[0].Motor.Teleport(new Vector3(-10,0,-5));targets[1].Motor.Teleport(new Vector3(10,0,8));
   Need(caster.CastSpinDash(destination),caster.Feedback);yield return WaitDash(caster);report.friendlyStopZ=p.transform.position.z;
   Need(p.transform.position.z< -6,"Dash passed a friendly body");
   targets[0].Configure(1,.5f);targets[0].Motor.Teleport(new Vector3(10,0,6));
   caster.ResetForManifestationLoad();Need(p.Motor.Teleport(start),"Wall fixture start");
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="Temporary dash terrain blocker";wall.layer=9;
   wall.transform.position=new Vector3(-10,1,-5);wall.transform.localScale=new Vector3(4,2,.25f);Physics.SyncTransforms();
   Need(caster.CastSpinDash(destination),caster.Feedback);yield return WaitDash(caster);report.wallStopZ=p.transform.position.z;
   Need(p.transform.position.z< -5.7f,"Dash passed a terrain collider");
   UnityEngine.Object.Destroy(wall);yield return null;Physics.SyncTransforms();
   caster.ResetForManifestationLoad();Need(p.Motor.Teleport(new Vector3(21,0,0)),"NavMesh boundary spawn");
   Need(!caster.CastSpinDash(new Vector3(26,0,0))&&caster.CooldownRemaining(GreatbladeProgressionRules.Kind.SpinDash)==0,"Dash crossed a map wall or spent rejected cooldown");
   Need(p.Motor.Teleport(start),"Cancel fixture start");
   p.SetEnemyPassage("qa.other-owner",true);nav.obstacleAvoidanceType=ObstacleAvoidanceType.LowQualityObstacleAvoidance;
   Need(caster.CastSpinDash(destination),caster.Feedback);caster.CancelCast();
   Need(p.CanPassThrough(targets[0])&&nav.obstacleAvoidanceType==ObstacleAvoidanceType.LowQualityObstacleAvoidance,"Cancel released another owner's passage or lost prior avoidance");
   p.SetEnemyPassage("qa.other-owner",false);Need(!p.CanPassThrough(targets[0]),"Cancel left its enemy passage");
   caster.ResetForManifestationLoad();nav.obstacleAvoidanceType=ObstacleAvoidanceType.HighQualityObstacleAvoidance;
   foreach(var t in targets)t.Motor.Teleport(new Vector3(10,0,6+Array.IndexOf(targets,t)*3));
  }
 }
}
