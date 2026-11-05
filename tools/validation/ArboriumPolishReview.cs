using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Persistence;
using DiceFree.UI;
using DiceFree.Visuals;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;
public static class ArboriumPolishReview
{
 static readonly Vector3 Center=new(98.04f,0,304.74f);
 static int checks,walks;static float meters;static string phase;static readonly List<object> rows=new();
 static void Need(bool value,string why){checks++;if(!value)throw new InvalidOperationException(why);}
 static void Record(bool passed,string error=null)=>File.WriteAllText("Logs/ArboriumPolishReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed,phase,checks,walks,meters,error,rows},Newtonsoft.Json.Formatting.Indented));
 static void Phase(string value){phase=value;Record(false);}
 static async Task Until(Func<bool> test,string why){var end=DateTime.UtcNow.AddSeconds(45);while(!test()){if(DateTime.UtcNow>end)throw new TimeoutException(why);await Task.Delay(25);}}
 static Vector3 Point(Vector3 p){Need(NavMesh.SamplePosition(p,out var hit,2,-1),"No navigation "+p);return hit.position;}
 static void Place(CombatActor p,Vector3 point){Need(p.Motor.Teleport(Point(point)),"Placement "+point);Physics.SyncTransforms();Object.FindAnyObjectByType<ExplorationCamera>().RecenterForTravel(p.transform);}
 static async Task Capture(string name){Directory.CreateDirectory("VisualBaselines/ArboriumPolish");await Task.Delay(300);ScreenCapture.CaptureScreenshot("VisualBaselines/ArboriumPolish/"+name+".png");await Task.Delay(120);}
 public static string Start(){if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean Edit Mode required.");var guard=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,new object[]{Path.Combine(Path.GetTempPath(),"DiceFree-ArboriumPolish-"+Guid.NewGuid())});UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");EditorApplication.EnterPlaymode();return "Started isolated polish review.";}
 public static string Stop(){EditorApplication.ExitPlaymode();return "Stopped isolated polish review.";}
 public static async Task<object> GateSightlines()
 {
  CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready,"Gate player");p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
  foreach(var b in Object.FindObjectsByType<MonoBehaviour>().Where(b=>b is AggroBehaviour||b is BasicAttack||b is QuilboarCombat||b is BriarbackChieftainEncounter))b.enabled=false;
  Object.FindAnyObjectByType<WorldDayNightCycle>().SetHour(11,false);
  var gate=GameObject.Find("Northern gate - camera-safe sections");Need(gate!=null&&gate.GetComponentsInChildren<Collider>().Length==0,"Gate presentation must not alter collision");
  Place(p,Center+new Vector3(-2.4f,0,89));await Capture("gate-outside-gameplay");bool faded=gate.GetComponentsInChildren<Renderer>().Where(r=>r.name.EndsWith("section 1")).Any(r=>r.sharedMaterials.Any(m=>m.color.a<=.2f));Need(faded,"Near gate tower must fade when blocking the hero outside");
  Place(p,Center+new Vector3(0,0,80));await Capture("gate-threshold-gameplay");Place(p,Center+new Vector3(0,0,72));await Capture("arrival-gameplay");
  var result=new{passed=true,outsideGateOccluderFaded=faded,collisionUnchanged=true};File.WriteAllText("Logs/ArboriumGateSightlines.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
 public static string Run(){checks=walks=0;meters=0;rows.Clear();_=Execute();return "Started; inspect Logs/ArboriumPolishReview.json.";}
 static async Task Execute()
 {
  try
  {
   CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready,"Player");
   p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
   foreach(var b in Object.FindObjectsByType<MonoBehaviour>().Where(b=>b is AggroBehaviour||b is BasicAttack||b is QuilboarCombat||b is BriarbackChieftainEncounter))b.enabled=false;
   foreach(var a in CombatActor.All)if(a.Motor!=null&&a.Motor.Ready)a.Motor.Stop();
   var navType=typeof(EditorWindow).Assembly.GetType("UnityEditor.AI.NavMeshVisualizationSettings");navType?.GetProperty("showNavMesh",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic)?.SetValue(null,false);var view=Resources.FindObjectsOfTypeAll(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).FirstOrDefault();if(view!=null)view.GetType().GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)?.SetValue(view,false);
   var camera=Object.FindAnyObjectByType<ExplorationCamera>();var clock=Object.FindAnyObjectByType<WorldDayNightCycle>();clock.SetHour(11,false);GraphicsQualityRuntime.Select(VisualQuality.High,false);
   var root=GameObject.Find("Arborium arrival district - polish").transform;Need(root.GetComponentsInChildren<CombatActor>().Length==0,"Ambient citizens must not become combat/reward actors");Need(root.GetComponentsInChildren<InteractionTarget>().Length==0,"Dressing must not replace quest/service interactions");
   var tree=root.Find("World Tree - gnarled trunk");Need(tree!=null&&tree.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.vertexCount)>1500,"Detailed World Tree trunk missing");var leaf=root.Find("World Tree - layered leaf canopy");Need(leaf.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3)>50000,"Layered leaf canopy missing");
   Need(Object.FindObjectsByType<Transform>().Where(t=>t.name=="Ancient trunk"||t.name=="Ancient bough"||t.name=="Vast crown").All(t=>t.GetComponents<Renderer>().All(r=>!r.enabled)),"Legacy tree primitives still visible");Need(root.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m!=null&&m.shader.isSupported)),"Unsupported district/tree material");
   Phase("visual review");Place(p,Center+new Vector3(0,0,72));await Capture("arrival-gameplay");
   camera.SetPresentationPose(Center+new Vector3(32,27,119),Quaternion.LookRotation(new Vector3(-32,-16,-43)),50);await Capture("gate-and-guild");
   camera.SetPresentationPose(Center+new Vector3(125,155,210),Quaternion.LookRotation(new Vector3(-125,-95,-210)),50);await Capture("world-tree-capital");camera.ClearPresentationPose();
   Place(p,Center+new Vector3(45,0,36));await Capture("market-gameplay-high");GraphicsQualityRuntime.Select(VisualQuality.Low,false);await Capture("market-gameplay-low");GraphicsQualityRuntime.Select(VisualQuality.High,false);
   Place(p,Center+new Vector3(0,0,23));await Capture("tree-roots-gameplay");camera.SetPresentationPose(Center+new Vector3(24,20,26),Quaternion.LookRotation(new Vector3(-24,-8,-26)),55);await Capture("world-tree-trunk-detail");camera.ClearPresentationPose();
   Phase("pedestrians convoy and audio");Place(p,Center+new Vector3(37,0,38));
   var citizens=root.GetComponentsInChildren<VillageHomeWander>();Need(citizens.Length==8,"Eight authored city pedestrians");var previous=citizens.ToDictionary(w=>w,w=>w.transform.position);var moved=citizens.ToDictionary(w=>w,w=>0f);var speeds=citizens.ToDictionary(w=>w,w=>0f);int walkingSamples=0;
   foreach(var citizen in citizens){var driver=citizen.GetComponent<VillageWalkingAnimator>();Need(driver!=null&&driver.Model!=null&&driver.Model.transform.IsChildOf(citizen.transform),"Citizen rig must bind to its own clone");Need(citizen.TryChoosePath(),"Citizen must have a safe home path "+citizen.name);}
   var convoy=root.GetComponentInChildren<ArboriumAmbientRoute>();Need(convoy!=null,"Arriving handcart missing");var end=DateTime.UtcNow.AddSeconds(22);
   while(DateTime.UtcNow<end){await Task.Delay(100);foreach(var citizen in citizens){var delta=Vector3.Distance(citizen.transform.position,previous[citizen]);Need(delta<.6f,"Citizen teleported");moved[citizen]+=delta;previous[citizen]=citizen.transform.position;Need(Vector3.ProjectOnPlane(citizen.transform.position-citizen.Home,Vector3.up).magnitude<=citizen.Radius+.1f,"Citizen escaped home patch");var driver=citizen.GetComponent<VillageWalkingAnimator>();speeds[citizen]=Mathf.Max(speeds[citizen],driver.MeasuredSpeed);if(driver.Model.GetCurrentAnimatorStateInfo(0).IsName("Walk"))walkingSamples++;}}
   Need(moved.Count(x=>x.Value>.5f)>=6&&walkingSamples>0,"Visible moving pedestrians");Need(convoy.TravelledMeters>5,"Convoy failed to enter the city");
   var sound=root.GetComponent<ArboriumAmbientSound>();var audio=p.GetComponent<PlayerAudioDirector>();await Task.Delay(150);Need(sound.Playing&&sound.Weight>.8f,"City ambience must play inside city");Need(Mathf.Abs(sound.AppliedVolume-.11f*audio.AmbientLevel*sound.Weight)<.001f,"Ambience must respect existing ambient gain");var gain=typeof(PlayerAudioDirector).GetField("ambientGain",BindingFlags.Instance|BindingFlags.NonPublic);float saved=(float)gain.GetValue(audio);try{gain.SetValue(audio,0f);await Task.Delay(150);Need(sound.AppliedVolume==0,"Ambient mute must silence city audio");}finally{gain.SetValue(audio,saved);}Place(p,new Vector3(235,0,365));await Task.Delay(150);Need(sound.Weight==0&&!sound.Playing,"City audio must stop outside city");
   rows.Add(new{citizens=citizens.Length,moving=moved.Count(x=>x.Value>.5f),walkingAnimationSamples=walkingSamples,convoyMeters=convoy.TravelledMeters,convoyStops=convoy.CompletedStops,ambientMute=true,ambientStopsOutside=true});
   Phase("real city walk regression");Place(p,Center+new Vector3(0,0,86));
   async Task Walk(Vector3 requested){var dest=Point(requested);var path=new NavMeshPath();Need(NavMesh.CalculatePath(p.transform.position,dest,-1,path)&&path.status==NavMeshPathStatus.PathComplete,"Complete city path "+requested);Need(p.Motor.MoveTo(dest),"Walk order "+dest);var previous=p.transform.position;var end=DateTime.UtcNow.AddSeconds(40);while(Vector3.Distance(p.transform.position,dest)>.9f){if(DateTime.UtcNow>end)throw new TimeoutException("Walk blocked "+dest);await Task.Delay(25);meters+=Vector3.Distance(previous,p.transform.position);previous=p.transform.position;}p.Motor.Stop();walks++;}
   foreach(var point in new[]{Center+new Vector3(0,0,72),Center+new Vector3(-26,0,66.8f),Center+new Vector3(28,0,63.8f),Center+new Vector3(54,0,25.2f),Center+new Vector3(45,0,0),Center+new Vector3(0,0,-24),Center+new Vector3(-44,0,0),Center+new Vector3(0,0,25),Center+new Vector3(-7,0,68.8f)})await Walk(point);
   Need(p.GetComponent<MinimapHud>().MapName=="Arborium","City map label retained");Need(p.GetComponent<RespawnAtAnchor>().AnchorId=="anchor.arborium","City stone still activates");clock.SetHour(20,false);Place(p,Center+new Vector3(0,0,72));await Capture("arrival-evening");clock.SetHour(11,false);
   Phase("passed");Record(true);Debug.Log("DICEFREE_ARBORIUM_POLISH_OK");
  }
  catch(Exception error){Record(false,error.ToString());Debug.LogError(error);}
 }
}
