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
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

public static class ArboriumFloorsReview
{
 static readonly Vector3 Center=new(98.04f,0,304.74f);
 static int checks,walks,levels;static float meters;static string phase;static readonly List<object> rows=new();
 static readonly List<string> errors=new();
 static void Log(string m,string stack,LogType kind){if(kind==LogType.Error||kind==LogType.Exception||kind==LogType.Assert)errors.Add(m);}
 static void Need(bool b,string why){checks++;if(!b)throw new InvalidOperationException(why);}
 static void Record(bool passed,string error=null)=>File.WriteAllText("Logs/ArboriumFloorsReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed,phase,checks,walks,levels,meters,error,unityErrors=errors,rows},Newtonsoft.Json.Formatting.Indented));
 static void Phase(string p){phase=p;Record(false);}
 static async Task Until(Func<bool> test,string why){var end=DateTime.UtcNow.AddSeconds(50);while(!test()){if(DateTime.UtcNow>end)throw new TimeoutException(why);await Task.Delay(25);}}
 static Vector3 Point(Vector3 p){Need(NavMesh.SamplePosition(p,out var hit,.35f,-1),"No precise navigation "+p);Need(Mathf.Abs(p.y-hit.position.y)<.25f,"Wrong navigation floor "+p+" vs "+hit.position);return hit.position;}
 public static string Start()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean Edit Mode required");
  var guard=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,new object[]{Path.Combine(Path.GetTempPath(),"DiceFree-ArboriumFloors-"+Guid.NewGuid())});UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");EditorApplication.EnterPlaymode();return "Started isolated floor review";
 }
 public static string Stop(){EditorApplication.ExitPlaymode();return "Stopped isolated floor review";}
 public static string Run(){checks=walks=levels=0;meters=0;rows.Clear();errors.Clear();Application.logMessageReceived+=Log;_=Execute();return "Started; inspect Logs/ArboriumFloorsReview.json";}
 static async Task Capture(string name){Directory.CreateDirectory("VisualBaselines/ArboriumFloors");await Task.Delay(250);ScreenCapture.CaptureScreenshot("VisualBaselines/ArboriumFloors/"+name+".png");await Task.Delay(150);}
 static async Task Execute()
 {
  try
  {
   CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready,"Player ready");
   p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
   foreach(var b in Object.FindObjectsByType<MonoBehaviour>().Where(b=>b is AggroBehaviour||b is BasicAttack||b is QuilboarCombat||b is BriarbackChieftainEncounter))b.enabled=false;
   foreach(var a in CombatActor.All)if(a.Motor!=null&&a.Motor.Ready)a.Motor.Stop();
   var camera=Object.FindAnyObjectByType<ExplorationCamera>();Object.FindAnyObjectByType<WorldDayNightCycle>().SetHour(11,false);GraphicsQualityRuntime.Select(VisualQuality.High,false);
   void Place(Vector3 v){Need(p.Motor.Teleport(Point(v)),"Fixture placement "+v);Physics.SyncTransforms();camera.RecenterForTravel(p.transform);}
   async Task Walk(Vector3 v)
   {
    var dest=Point(v);var path=new NavMeshPath();Need(NavMesh.CalculatePath(p.transform.position,dest,-1,path)&&path.status==NavMeshPathStatus.PathComplete,"Complete path "+v);Need(p.Motor.MoveTo(dest),"Move order "+v);
    var prev=p.transform.position;var end=DateTime.UtcNow.AddSeconds(55);float maxDelta=0;
    while(Vector3.Distance(p.transform.position,dest)>.65f){if(DateTime.UtcNow>end)throw new TimeoutException("Stuck walking "+v+" at "+p.transform.position);await Task.Delay(25);var delta=Vector3.Distance(prev,p.transform.position);maxDelta=Mathf.Max(maxDelta,delta);meters+=delta;prev=p.transform.position;}
    p.Motor.Stop();walks++;Need(maxDelta<2,"Walking used a discontinuous jump");await Task.Delay(100);
   }
   Phase("gates and ridge");Need(!Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Single(t=>t.name=="Distant wooded ridge").gameObject.activeSelf,"Crossing ridge still active");
   foreach(int side in new[]{-1,1}){Place(Center+new Vector3(side*70,.12f,0));await Walk(Center+new Vector3(side*92,.12f,0));await Capture(side<0?"west-gate-outside":"east-gate-outside");await Walk(Center+new Vector3(side*66,.12f,0));}
   Phase("six buildings real stair traversal");var buildings=Object.FindObjectsByType<ArboriumInterior>().OrderBy(b=>b.BuildingName).ToArray();Need(buildings.Length==6&&buildings.Sum(b=>b.FloorCount)==21,"Six buildings / 21 levels");
   foreach(var building in buildings)
   {
    Phase(building.BuildingName);var origin=building.transform.position;float z=building.Footprint.y/2;
    Place(origin+new Vector3(0,.12f,z+2));await Walk(origin+new Vector3(0,.12f,z-1.5f));Need(building.Inside&&building.CurrentFloor==0,"Ground floor cutaway "+building.name);
    var parts=building.GetComponentsInChildren<Transform>().Where(t=>t.name=="Copper roof and carved eaves").Single();Need(parts.GetComponentsInChildren<Renderer>().All(r=>!r.enabled),"Roof must cut away");
    for(int f=0;f<building.FloorCount;f++)
    {
     await Walk(origin+new Vector3(0,.12f+f*building.StoreyHeight,z-1.5f));Need(building.CurrentFloor==f,"Occupied floor label "+building.name+" "+f);levels++;
     var ray=new Ray(origin+new Vector3(0,30,z-1.5f),Vector3.down);Need(ArboriumInterior.RaycastGround(ray,out var hit)&&hit.collider.gameObject.layer==8&&Mathf.Abs(hit.point.y-(origin.y+.12f+f*building.StoreyHeight))<.2f,"Click must hit occupied floor "+building.name+" "+f);
     if(f==1||f==building.FloorCount-1)await Capture(building.BuildingName.Replace(" ","-")+"-floor-"+(f+1));
    }
    await Walk(origin+new Vector3(0,.12f,z+2));Need(!building.Inside,"Exit restores shell "+building.name);Need(parts.GetComponentsInChildren<Renderer>().All(r=>r.enabled),"Roof restored outside");rows.Add(new{building=building.BuildingName,floors=building.FloorCount,ascendedAndDescended=true});
   }
   Place(Center+new Vector3(0,.12f,72));await Capture("north-arrival-gameplay");camera.SetPresentationPose(Center+new Vector3(140,140,190),Quaternion.LookRotation(new Vector3(-140,-100,-190)),50);await Capture("capital-three-gates");camera.ClearPresentationPose();
   Need(errors.Count==0,"Unity error events: "+string.Join(";",errors));Phase("passed");Record(true);Debug.Log("DICEFREE_ARBORIUM_FLOORS_OK");
  }
  catch(Exception e){Record(false,e.ToString());Debug.LogError(e);}
  finally{Application.logMessageReceived-=Log;}
 }
}
