using System;using System.Collections;using System.Collections.Generic;using System.IO;using System.Linq;using System.Reflection;
using DiceFree.Characters;using DiceFree.Combat;using DiceFree.Development;using DiceFree.Foundation;using DiceFree.Input;using DiceFree.Items;using DiceFree.Persistence;using DiceFree.Skills;using DiceFree.UI;using DiceFree.World;
using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine.SceneManagement;using Unity.Pipeline.Commands;
namespace DiceFree.EditorTools{
[InitializeOnLoad]public static class CubeEdgeValidation{
const string Key="DiceFree.CubeEdge.";static IEnumerator flow;static readonly Stack<IEnumerator> steps=new();static double deadline;static int checks;
static CubeEdgeValidation()=>EditorApplication.playModeStateChanged+=Changed;
static void Need(bool ok,string m){if(!ok)throw new InvalidOperationException(m);checks++;}
[CliCommand("dicefree.cube-edge.test","Check visible neighbour, quarter-turn, control locks, round-trip state, failure recovery and automatic seam crossing using isolated saves.")]
public static object Start(){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
SessionState.SetBool(Key+"cleanup",true);SessionState.SetBool(Key+"resume",false);SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");SessionState.SetInt(Key+"checks",0);
var root=Path.Combine(Path.GetTempPath(),"DiceFree-CubeEdge-"+Guid.NewGuid().ToString("N"));var sentinel=new EchoSave{revision=1,writtenUtc=DateTime.UtcNow.ToString("O")};EchoIdentity.WriteName(sentinel,"Normal cube sentinel");new LocalEchoStore(root).Commit(sentinel);
SessionState.SetString(Key+"root",root);SessionState.SetString(Key+"sentinel",File.ReadAllText(new LocalEchoStore(root).Path));
PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);EditorSettings.enterPlayModeOptionsEnabled=false;EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/DevelopmentRoom.unity");EditorApplication.EnterPlaymode();return Status();}
[CliCommand("dicefree.cube-edge.status","Read cube-edge verification and capture stage.")]
public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error","")};
[CliCommand("dicefree.cube-edge.resume","Advance the current cube capture pause.")]
public static object Resume(){SessionState.SetBool(Key+"resume",true);return Status();}
static void Changed(PlayModeStateChange s){
if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running"){checks=0;deadline=EditorApplication.timeSinceStartup+600;flow=Flow();steps.Clear();steps.Push(flow);EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false)){SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);PersistenceTestGuard.ClearValidationOverrides();DevelopmentSession.End();var scene=SessionState.GetString(Key+"scene","");if(!string.IsNullOrEmpty(scene))EditorSceneManager.OpenScene(scene);}}
static void Log(string m,string s,LogType t){if(t==LogType.Error||t==LogType.Exception)Finish(false,m+"\n"+s);}
static void Tick(){if(!EditorApplication.isPlaying)return;try{
if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Cube test timed out");
while(steps.Count>0){var step=steps.Peek();if(!step.MoveNext()){steps.Pop();continue;}if(step.Current is IEnumerator nested){steps.Push(nested);continue;}return;}
if(flow!=null)Finish(true,null);
}catch(Exception e){Finish(false,e.ToString());}}
static IEnumerator Pause(string status){SessionState.SetInt(Key+"checks",checks);SessionState.SetString(Key+"status",status);SessionState.SetBool(Key+"resume",false);while(!SessionState.GetBool(Key+"resume",false))yield return null;SessionState.SetString(Key+"status","running");}
static IEnumerator WaitTravel(DevelopmentRoomController room){float until=Time.realtimeSinceStartup+30;while(room.Busy){NeedTime(until);yield return null;}yield return null;}
static void NeedTime(float until){if(Time.realtimeSinceStartup>until)throw new TimeoutException("Cube transition did not finish");}
static IEnumerator Flow(){
while(DevelopmentRoomController.Current==null||DevelopmentRoomController.Current.Map==null)yield return null;
var room=DevelopmentRoomController.Current;var p=room.player;while(!p.GetComponent<ManifestationPersistence>().Ready)yield return null;
var cube=room.GetComponent<CubeEdgePresentation>();var inv=p.GetComponent<CarriedInventory>();var eq=p.GetComponent<Equipment>();var res=p.GetComponent<ActorResourceController>();
Need(cube!=null&&cube.Edge!=null,"Cube presenter/edge missing");Need(cube.Edge.Sign==1&&cube.Edge.link.transitionStyle==WorldTransitionStyle.CubeRotate,"Wrong outgoing edge");
Need(Mathf.Approximately(room.Map.bounds.width,room.Map.bounds.height),"Test face is not square");
Need(cube.Neighbour!=null&&cube.Neighbour.GetComponentsInChildren<Renderer>().Length>10,"Neighbour has no visible geometry");
Need(cube.Neighbour.GetComponentsInChildren<Collider>(true).Length==0,"Neighbour owns collision");Need(cube.Neighbour.GetComponentsInChildren<CombatActor>(true).Length==0,"Neighbour duplicates actors");Need(cube.Neighbour.GetComponentsInChildren<WorldNavigation>(true).Length==0,"Neighbour duplicates navigation");
Need(room.SelectClass(MagicalTouchedAuthoring.ClassId)&&room.SetLevel(42),"Class/level setup failed");yield return null;yield return null;
var skills=p.GetComponent<MagicalSkillProgression>();Need(skills.Spend(MagicalTouchedAuthoring.MendId),"Learn Mend");
foreach(var d in inv.Definitions.Where(d=>d!=null&&d.modelTestingItem)){var i=inv.Grant(d.stableId);Need(eq.Equip(i.instanceId,d.slot),"Equip "+d.slot);}
p.GetComponent<GoldWallet>().Grant(137);p.Health.SetEncounterHp(p.Health.Maximum*.55f);res.SetCurrentForValidation(ResourceIds.Mana,res.Maximum(ResourceIds.Mana)*.25f);
res.enabled=false;p.Health.enabled=false; // Isolate transport conservation; the separate legacy suite checks natural regeneration over elapsed time.
var traversal=p.GetComponent<TraversalInput>();traversal.enabled=false;
var action=InputBindings.Current.Asset.FindAction("Gameplay/Class skill 8",true);action.Disable();
var home=p.GetComponent<ReturnHomeAbility>();typeof(ReturnHomeAbility).GetField("readyAt",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(home,Time.time+300f);
string playerId=p.GetEntityId().ToString(),inventory=string.Join("|",inv.Items.Select(i=>i.instanceId+":"+i.definitionId)),equipment=string.Join("|",eq.Slots.Select(s=>s.slotId+":"+s.instanceId));
string ranks=string.Join("|",skills.CaptureState().Select(r=>r.stableId+":"+r.rank));
float hp=p.Health.Current,mana=res.Current(ResourceIds.Mana);
Need(!room.Travel("Cornberg")&&!room.Travel("DevelopmentRoom"),"Invalid/same-scene travel accepted");
Need(p.Motor.Teleport(cube.Edge.Approach),"Edge approach unreachable");room.explorationCamera.RecenterForTravel(p.transform);
float until=Time.time+2;while(Time.time<until)yield return null;
Need(cube.VistaVisible&&room.explorationCamera.PresentationOverride,"Edge vista did not engage");
var planes=GeometryUtility.CalculateFrustumPlanes(room.explorationCamera.GetComponent<Camera>());
Need(cube.Neighbour.GetComponentsInChildren<Renderer>().Any(r=>r.enabled&&GeometryUtility.TestPlanesAABB(planes,r.bounds)),"Neighbour is outside camera view");
yield return Pause("vista_ready");
cube.CaptureHold=true;Need(room.Travel("World2TransitionTest"),"First crossing rejected");
Need(room.Busy&&cube.ControlsLocked,"Crossing did not lock actions/movement");Need(!room.Travel("World2TransitionTest"),"Duplicate crossing accepted");
Need(!room.SelectClass("class.novice")&&!room.SetLevel(100),"Class/level can change during fold");
Need(!eq.Unequip(EquipmentSlot.MainHand.ToString()),"Gear can change during fold");
Need(!p.GetComponent<DiceFree.Dungeons.ReturnToStartMenu>().Return(),"Menu return interrupted fold");
Need(!p.Motor.MoveTo(p.transform.position+Vector3.right),"Movement allowed during fold");
Need(!p.GetComponent<MagicalSkillCaster>().Cast(skills.Definitions.Single(d=>d.stableId==MagicalTouchedAuthoring.MendId),p),"Spell allowed during fold");
until=Time.realtimeSinceStartup+30;while(cube.Angle<44.99f){NeedTime(until);yield return null;}
Need(Mathf.Abs(cube.Angle-45)<.01f,"Midpoint is not a quarter-turn half-step");
var avatar=GameObject.Find("Crossing Echo (presentation only)");Need(avatar!=null&&avatar.GetComponentsInChildren<Renderer>().Any(r=>r.enabled),"Crossing avatar is invisible");
Need(avatar.GetComponentsInChildren<CombatActor>(true).Length==0&&avatar.GetComponentsInChildren<Collider>(true).Length==0,"Crossing clone owns gameplay");
Need(Vector3.Distance(avatar.transform.position,cube.Edge.Pivot)<.05f,"Midpoint avatar is not on seam");
p.Motor.SetMotionBlocked("qa.other-owner",true);p.SetActionBlocked("qa.other-owner",true);
yield return Pause("midpoint_ready");cube.CaptureHold=false;
yield return WaitTravel(room);
Need(room.Map.worldId=="world.test.2"&&room.CompletedTransitions==1&&cube.CompletedFlips==1,"World 2 arrival failed");
Need(Mathf.Abs(cube.Angle-90)<.01f&&cube.LastSignedTurn==-90&&cube.RotationSamples>10,"First flip was not an animated quarter-turn");
Need(!SceneManager.GetSceneByName("DevelopmentRoom").isLoaded,"Source face still loaded");
Need(!p.CanAct&&!p.Motor.MotionAllowed,"Transport released another owner's lock");p.SetActionBlocked("qa.other-owner",false);p.Motor.SetMotionBlocked("qa.other-owner",false);
Need(!traversal.enabled&&!action.enabled&&!res.enabled,"Previously disabled input/resource state not preserved");
CheckState(p,playerId,inventory,equipment,ranks,hp,mana);
Need(!room.explorationCamera.PresentationOverride&&p.Motor.Ready,"Camera/navigation did not settle");
until=Time.time+.2f;while(Time.time<until)yield return null;Need(!room.Busy&&room.CompletedTransitions==1,"Arrival immediately bounced back");
yield return Pause("arrival_ready");
Need(room.Travel("DevelopmentRoom"),"Return crossing rejected");yield return WaitTravel(room);
Need(room.CompletedTransitions==2&&cube.CompletedFlips==2&&cube.LastSignedTurn==90,"Reverse quarter-turn failed");
Need(room.Map.worldId=="world.test.1"&&!SceneManager.GetSceneByName("World2TransitionTest").isLoaded,"Return face lifecycle wrong");
CheckState(p,playerId,inventory,equipment,ranks,hp,mana);
Need(UnityEngine.Object.FindObjectsByType<TraversalInput>(FindObjectsInactive.Include).Length==1,"Duplicate player after round trip");
var target=cube.Edge.link.destinationSpawnId;cube.Edge.link.destinationSpawnId="invalid-fixture";
Need(room.Travel("World2TransitionTest"),"Failure fixture did not start");yield return WaitTravel(room);cube.Edge.link.destinationSpawnId=target;
Need(room.CompletedTransitions==2&&cube.CompletedFlips==2&&room.Map.worldId=="world.test.1","Failed edge committed a transition");
Need(!room.Busy&&!cube.Turning&&p.CanAct&&p.Motor.MotionAllowed&&!room.explorationCamera.PresentationOverride,"Failure left controls/camera locked");
Need(!SceneManager.GetSceneByName("World2TransitionTest").isLoaded,"Failed destination leaked");
CheckState(p,playerId,inventory,equipment,ranks,hp,mana);
Need(p.Motor.Teleport(cube.Edge.Origin),"Leave edge to rearm");yield return null;
traversal.enabled=true;Need(p.Motor.MoveTo(cube.Edge.Pivot-Vector3.forward*1.3f),"Walking route through seam unreachable");
until=Time.realtimeSinceStartup+30;while(room.CompletedTransitions<3){NeedTime(until);yield return null;}yield return null;
Need(room.Map.worldId=="world.test.2"&&cube.CompletedFlips==3,"Walking across seam did not travel");
Need(traversal.enabled&&!action.enabled,"Automatic crossing changed disabled bindings");
CheckState(p,playerId,inventory,equipment,ranks,hp,mana);
Need(p.GetComponent<ManifestationPersistence>().Flush(),"Save flush failed");
string root=SessionState.GetString(Key+"root","");Need(File.ReadAllText(new LocalEchoStore(root).Path)==SessionState.GetString(Key+"sentinel",""),"Normal Echo was modified");
Need(p.GetComponent<DiceFree.Dungeons.ReturnToStartMenu>().Return(),"Final menu return failed");yield return null;
Need(!DevelopmentSession.Active&&DevelopmentRoomController.Current==null,"Dev session leaked");
}
static void CheckState(CombatActor p,string id,string inventory,string equipment,string ranks,float hp,float mana){
Need(p.GetEntityId().ToString()==id,"Player identity changed");Need(p.Stats.Definition.stableId==MagicalTouchedAuthoring.ClassId&&p.Stats.Level==42,"Class/level changed");
Need(p.GetComponent<GoldWallet>().Gold==137,"Gold changed");Need(string.Join("|",p.GetComponent<CarriedInventory>().Items.Select(i=>i.instanceId+":"+i.definitionId))==inventory,"Inventory identity changed");
Need(string.Join("|",p.GetComponent<Equipment>().Slots.Select(s=>s.slotId+":"+s.instanceId))==equipment,"Equipment changed");
Need(string.Join("|",p.GetComponent<MagicalSkillProgression>().CaptureState().Select(r=>r.stableId+":"+r.rank))==ranks,"Skill ranks changed");
Need(Mathf.Abs(p.Health.Current-hp)<.01f,"Transport changed health");Need(Mathf.Abs(p.GetComponent<ActorResourceController>().Current(ResourceIds.Mana)-mana)<.01f,"Transport changed Mana");
Need(p.GetComponent<ReturnHomeAbility>().CooldownRemaining>100&&p.GetComponent<ReturnHomeAbility>().CooldownRemaining<300,"Cooldown reset");
}
static void Finish(bool ok,string error){flow=null;EditorApplication.update-=Tick;Application.logMessageReceived-=Log;SessionState.SetInt(Key+"checks",checks);SessionState.SetString(Key+"status",ok?"passed":"failed");SessionState.SetString(Key+"error",error??"");File.WriteAllText("Library/CubeEdgeResults.json",JsonUtility.ToJson(new Record{passed=ok,checks=checks,error=error},true));EditorApplication.ExitPlaymode();}
[Serializable]class Record{public bool passed;public int checks;public string error;}
}}
