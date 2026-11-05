using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Persistence;
using DiceFree.World;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;
public static class ArboriumFloorsControls
{
 static void Need(bool b,string why){if(!b)throw new InvalidOperationException(why);}
 public static string Run(){File.WriteAllText("Logs/ArboriumFloorsControls.json","{\"passed\":false,\"phase\":\"running\"}");_=Execute();return "Started controls/material review; inspect Logs/ArboriumFloorsControls.json";}
 static async Task Execute()
 {
  try
  {
   CombatActor p=null;var end=DateTime.UtcNow.AddSeconds(40);while((p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())==null||!p.Motor.Ready||!p.GetComponent<ManifestationPersistence>().Ready){Need(DateTime.UtcNow<end,"Ready timeout");await Task.Delay(25);}
   p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
   var camera=Object.FindAnyObjectByType<ExplorationCamera>();Object.FindAnyObjectByType<WorldDayNightCycle>().SetHour(11,false);
   var home=Object.FindObjectsByType<ArboriumInterior>().Single(b=>b.BuildingName=="East Residence");var origin=home.transform.position;
   Need(NavMesh.SamplePosition(origin+new Vector3(0,.12f,7),out var start,.35f,-1)&&p.Motor.Teleport(start.position),"Outside fixture");camera.RecenterForTravel(p.transform);
   float directMeters=0;int corners=0;
   async Task Direct(Vector3 dest)
   {
    Need(NavMesh.SamplePosition(dest,out var target,.35f,-1),"Direct destination");var path=new NavMeshPath();Need(NavMesh.CalculatePath(p.transform.position,target.position,-1,path)&&path.status==NavMeshPathStatus.PathComplete,"Direct continuous path");
    // Follow the authored centre lines: baked corner positions use a 0.5 m
    // erosion radius, while the player capsule is wider and keeps live collision.
    foreach(var corner in new[]{target.position})
    {
     corners++;var end=DateTime.UtcNow.AddSeconds(12);
     while(Vector3.ProjectOnPlane(corner-p.transform.position,Vector3.up).magnitude>.08f)
     {
      Need(DateTime.UtcNow<end,"Direct movement stuck at "+p.transform.position+" approaching "+corner);var before=p.transform.position;var delta=Vector3.ProjectOnPlane(corner-before,Vector3.up);float dt=Mathf.Min(.016f,delta.magnitude/p.Motor.Speed);p.Motor.MoveDirect(delta.normalized,dt);await Task.Delay(16);directMeters+=Vector3.Distance(before,p.transform.position);
     }
    }
    Need(Mathf.Abs(p.transform.position.y-dest.y)<.25f,"Direct movement reached wrong height");await Task.Delay(100);
   }
   var route=new System.Collections.Generic.List<Vector3>{origin+new Vector3(0,.12f,3.5f)};
   for(int f=0;f<2;f++)
   {
    float y=.12f+f*3.6f;
    foreach(var v in new[]{new Vector3(-2.95f,y,3.5f),new Vector3(-2.95f,y,-3.8f),new Vector3(-1.02f,y,-3.8f),new Vector3(-1.02f,y+1.8f,.56f),new Vector3(1.02f,y+1.8f,.56f),new Vector3(1.02f,y+3.6f,-3.8f),new Vector3(-2.95f,y+3.6f,-3.8f),new Vector3(-2.95f,y+3.6f,3.5f),new Vector3(0,y+3.6f,3.5f)})route.Add(origin+v);
   }
   foreach(var v in route)await Direct(v);Need(home.Inside&&home.CurrentFloor==2,"Direct top floor");
   var point=origin+new Vector3(-1,7.32f,3.5f);var screen=Camera.main.WorldToScreenPoint(point);Need(p.GetComponent<TraversalInput>().TryWorldPoint(new Vector2(screen.x,screen.y),out var selected)&&Mathf.Abs(selected.y-7.32f)<.2f,"Oblique mouse picking through cutaway walls");
   Need(home.GetComponentsInChildren<Collider>().Where(c=>c.gameObject.layer==9).All(c=>c.enabled),"Invisible solid walls must retain collision");
   Directory.CreateDirectory("VisualBaselines/ArboriumFloors");ScreenCapture.CaptureScreenshot("VisualBaselines/ArboriumFloors/residence-planks-and-cutaway.png");await Task.Delay(150);
   foreach(var v in route.AsEnumerable().Reverse().Skip(1))await Direct(v);await Direct(origin+new Vector3(0,.12f,7));Need(!home.Inside,"Direct exit");
   // Verify component shutdown cannot leave overhead floors unpickable or stale wall exemptions.
   Need(NavMesh.SamplePosition(origin+new Vector3(0,.12f,3.5f),out var ground,.35f,-1)&&p.Motor.Teleport(ground.position),"Ground fixture");await Task.Delay(100);home.enabled=false;
   Need(home.GetComponentsInChildren<Collider>().All(c=>c.enabled)&&home.GetComponentsInChildren<Renderer>().All(r=>r.enabled),"Disable must restore the complete shell");home.enabled=true;await Task.Delay(100);
   Need(home.CurrentFloor==0,"Reenabled interior");
   var result=new{passed=true,directAscentAndDescent=true,directMeters,corners,obliquePicking=true,solidWallsRetained=true,disableRestore=true};File.WriteAllText("Logs/ArboriumFloorsControls.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));Debug.Log("DICEFREE_ARBORIUM_FLOOR_CONTROLS_OK");
  }
  catch(Exception e){File.WriteAllText("Logs/ArboriumFloorsControls.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=false,error=e.ToString()},Newtonsoft.Json.Formatting.Indented));Debug.LogError(e);}
 }
}
