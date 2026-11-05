using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.Ironpine;
using DiceFree.Persistence;
using DiceFree.World;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
public static class WolfClearingReview
{
 static void Need(bool b,string why){if(!b)throw new InvalidOperationException(why);}
 static async Task Until(Func<bool> f,string why){var end=DateTime.UtcNow.AddSeconds(40);while(!f()){Need(DateTime.UtcNow<end,"Timeout "+why);await Task.Delay(25);}}
 public static string Run(){_=Execute();return "Started final clearing review; inspect Logs/WolfClearingReview.json";}
 static async Task Capture(string name){Directory.CreateDirectory("VisualBaselines/WolfSurvival");await Task.Delay(250);ScreenCapture.CaptureScreenshot("VisualBaselines/WolfSurvival/"+name+".png");await Task.Delay(120);}
 static async Task Execute()
 {
  try
  {
   CombatActor hero=null;await Until(()=>(hero=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&hero.Motor.Ready&&hero.GetComponent<ManifestationPersistence>().Ready,"Hero");hero.GetComponent<TraversalInput>().enabled=false;hero.GetComponent<CombatInput>().enabled=false;hero.Health.Invulnerable=true;
   var portal=Object.FindAnyObjectByType<WolfDungeonPortal>();Need(hero.Motor.TeleportAcrossMaps(portal.ApproachPosition),"Portal fixture");portal.Interact(hero);await Until(()=>WolfDungeonRun.Current!=null&&WolfDungeonRun.Current.StagingReady,"Staging");
   var run=WolfDungeonRun.Current;var dummy=GameObject.Find("Practice target — no rewards");Need(dummy.GetComponent<IronpineWildlife>()==null,"Practice target must not receive a wolf overlay");Need(dummy.GetComponent<Health>().Invulnerable&&dummy.GetComponentsInChildren<Renderer>().Any(r=>r.enabled),"Visible invulnerable practice dummy");
   await Capture("final-moonlit-staging");var camera=Object.FindAnyObjectByType<ExplorationCamera>();camera.SetPresentationPose(WolfDungeonRun.Origin+new Vector3(43,46,-55),Quaternion.LookRotation(new Vector3(-43,-33,55)),50);await Capture("final-clearing-overview");camera.ClearPresentationPose();
   float meters=0;
   foreach(var dest in new[]{new Vector3(-23,0,9),new Vector3(23,0,9),new Vector3(0,0,25),new Vector3(0,0,-8)})
   {
    var target=WolfDungeonRun.Origin+dest;Need(hero.Motor.MoveTo(target),"Arena path "+dest);var prev=hero.transform.position;
    await Until(()=>{meters+=Vector3.Distance(prev,hero.transform.position);prev=hero.transform.position;return Vector3.Distance(hero.transform.position,target)<.75f;},"Gate approach walk "+dest);hero.Motor.Stop();
   }
   Need(run.StartNow(hero),"Start waves");Need(Object.FindObjectsByType<LineRenderer>().Count(l=>l.name=="Incoming wolf trail")==Mathf.Min(3,run.Composition.Count),"Warn only occupied entry trails");await Until(()=>run.Fighting,"First pack");await Task.Delay(1000);await Capture("final-first-pack");run.Abandon();await Until(()=>WolfDungeonRun.Current==null&&!SceneManager.GetSceneByName("WolfSurvival").isLoaded,"Cleanup");
   var result=new{passed=true,realPracticeDummy=true,entryTrailsWalked=3,meters,warningTrailsAccurate=true};File.WriteAllText("Logs/WolfClearingReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));Debug.Log("DICEFREE_WOLF_CLEARING_OK");
  }
  catch(Exception e){File.WriteAllText("Logs/WolfClearingReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=false,error=e.ToString()},Newtonsoft.Json.Formatting.Indented));Debug.LogError(e);}
 }
}
