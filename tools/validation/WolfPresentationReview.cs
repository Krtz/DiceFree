using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.Characters;
using DiceFree.Banking;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.Persistence;
using DiceFree.World;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;
public static class WolfPresentationReview
{
 static int checks;static float meters;static void Need(bool b,string text){checks++;if(!b)throw new InvalidOperationException(text);}
 static async Task Until(Func<bool> f,string text){var end=DateTime.UtcNow.AddSeconds(40);while(!f()){Need(DateTime.UtcNow<end,text);await Task.Delay(25);}}
 public static string Run(){checks=0;meters=0;_=Execute();return "Started wolf presentation review";}
 static async Task Capture(string name){Directory.CreateDirectory("VisualBaselines/WorldPolish");await Task.Delay(180);ScreenCapture.CaptureScreenshot("VisualBaselines/WorldPolish/"+name+".png");await Task.Delay(120);}
 static async Task Execute()
 {
  try
  {
   CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.GetComponent<ManifestationPersistence>().Ready,"Ready hero");p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;p.GetComponent<BasicAttack>().enabled=false;p.GetComponent<Interactor>()?.Cancel();
   typeof(BankPanel).GetField("open",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(p.GetComponent<BankPanel>(),false);
   var portal=Object.FindAnyObjectByType<WolfDungeonPortal>();Need(p.Motor.TeleportAcrossMaps(portal.ApproachPosition),"Entrance fixture");portal.Interact(p);await Until(()=>WolfDungeonRun.Current?.StagingReady==true,"Staging");var run=WolfDungeonRun.Current;
   typeof(WolfDungeonRun).GetField("deadline",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(run,Time.time+180);
   var method=typeof(WolfDungeonRun).GetMethod("PackEntrance",BindingFlags.Static|BindingFlags.NonPublic);Need(method!=null,"Shared entry geometry");
   for(int i=0;i<12;i++)
   {
    var point=(Vector3)method.Invoke(null,new object[]{i});Need(NavMesh.SamplePosition(point,out var hit,.4f,-1),"Precise entry navigation "+i);Need(Vector3.Distance(hit.position,point)<.4f,"Entry stays at treeline "+i);var path=new NavMeshPath();Need(NavMesh.CalculatePath(hit.position,p.transform.position,-1,path)&&path.status==NavMeshPathStatus.PathComplete,"Entry has complete route "+i);
   }
   async Task Walk(Vector3 dest){Need(p.Motor.MoveTo(dest),"Trail walk order");var prev=p.transform.position;await Until(()=>{meters+=Vector3.Distance(prev,p.transform.position);prev=p.transform.position;return Vector3.Distance(p.transform.position,dest)<.75f;},"Trail walk");p.Motor.Stop();}
   await Capture("wolf-blended-clearing");for(int i=0;i<3;i++)await Walk((Vector3)method.Invoke(null,new object[]{i}));await Walk(WolfDungeonRun.Origin+new Vector3(0,0,-8));
   var camera=Object.FindAnyObjectByType<ExplorationCamera>();camera.SetPresentationPose(WolfDungeonRun.Origin+new Vector3(43,46,-55),Quaternion.LookRotation(new Vector3(-43,-33,55)),50);await Capture("wolf-ground-and-entry-routes");camera.ClearPresentationPose();
   p.GetComponent<BasicAttack>().enabled=true;Need(run.StartNow(p)&&run.Announcement.Contains("WAVE 1"),"Immediate wave banner");await Capture("wolf-wave-announcement");await Until(()=>run.Fighting,"Treeline pack spawn");Need(run.Wolves.All(w=>Vector3.Distance(w.transform.position,WolfDungeonRun.Origin)>27),"Wolves emerge from outer tree routes");await Capture("wolf-treeline-pack");
   for(int wave=1;wave<=10;wave++)
   {
    await Until(()=>run.Fighting&&run.Wave==wave,"Wave "+wave);foreach(var wolf in run.Wolves.ToArray())wolf.Actor.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});await Until(()=>run.Phase==WolfRunPhase.Intermission||run.Phase==WolfRunPhase.Victory,"Clear "+wave);if(wave<10)Need(run.StartNextWave(p),"Next wave "+wave);
   }
   Need(run.WavesCleared==10&&run.RunSeconds>0,"Victory summary");float duration=run.RunSeconds;await Task.Delay(350);Need(run.RunSeconds==duration,"Frozen completion duration");await Capture("wolf-victory-summary");run.Abandon();await Until(()=>WolfDungeonRun.Current==null,"Exit cleanup");File.WriteAllText("Logs/WolfPresentationReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=true,checks,walkedMeters=meters,entrySlots=12,treelineSpawns=true,banners=true,frozenVictoryTime=true},Newtonsoft.Json.Formatting.Indented));
  }
  catch(Exception e){File.WriteAllText("Logs/WolfPresentationReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=false,checks,error=e.ToString()},Newtonsoft.Json.Formatting.Indented));Debug.LogError(e);}
 }
}
