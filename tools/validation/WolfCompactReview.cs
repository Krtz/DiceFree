using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.Persistence;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
public static class WolfCompactReview
{
 static int checks;static void Need(bool b,string why){checks++;if(!b)throw new InvalidOperationException(why);}
 static async Task Until(Func<bool> f,string why,int seconds=30){var end=DateTime.UtcNow.AddSeconds(seconds);while(!f()){Need(DateTime.UtcNow<end,"Timeout: "+why);await Task.Delay(25);}}
 public static string Run(){checks=0;_=Execute();return "Started; inspect Logs/WolfCompactReview.json";}
 static async Task Capture(string name){Directory.CreateDirectory("VisualBaselines/WolfCompact");await Task.Delay(200);ScreenCapture.CaptureScreenshot("VisualBaselines/WolfCompact/"+name+".png");await Task.Delay(100);}
 static async Task Execute()
 {
  try
  {
   CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready,"Hero");p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
   var portal=Object.FindAnyObjectByType<WolfDungeonPortal>();Need(p.Motor.TeleportAcrossMaps(portal.ApproachPosition),"Portal fixture");portal.Interact(p);await Until(()=>WolfDungeonRun.Current!=null&&WolfDungeonRun.Current.StagingReady,"Staging");var run=WolfDungeonRun.Current;var origin=WolfDungeonRun.Origin;
   Need(portal.tuning.definition.stagingSeconds==30&&portal.tuning.intermissionSeconds==4&&portal.tuning.victorySeconds==5,"Compact timing assets");Need(!run.StartNextWave(p)&&!run.StartNextWave(null),"Cannot skip staging with Next wave");
   Vector3 Point(Vector3 v){Need(NavMesh.SamplePosition(origin+v,out var h,.5f,-1),"Navigation "+v);return h.position;}
   var left=Point(new Vector3(-15,0,0));var right=Point(new Vector3(-1,0,0));Need(Physics.Linecast(left+Vector3.up,right+Vector3.up,out var obstruction,1<<9)&&obstruction.collider.name.Contains("Western"),"Rock blocks sight");
   var path=new NavMeshPath();Need(NavMesh.CalculatePath(left,right,-1,path)&&path.status==NavMeshPathStatus.PathComplete&&path.corners.Length>2,"Complete detour around cover");Need(NavMesh.Raycast(left,right,out _,-1),"Cover excluded from walkable navigation");
   float meters=0;async Task Walk(Vector3 dest){Need(p.Motor.MoveTo(dest),"Cover walk order");var previous=p.transform.position;await Until(()=>{meters+=Vector3.Distance(previous,p.transform.position);previous=p.transform.position;return Vector3.Distance(p.transform.position,dest)<.7f;},"Actual cover walk");p.Motor.Stop();}
   await Walk(left);await Walk(right);await Walk(Point(new Vector3(0,0,-8)));await Capture("cover-gameplay");
   p.Stats.SetEncounterMaximumHp(5000);p.Health.Restore();p.Health.SetEncounterHp(2000);
   Need(run.StartNow(p),"Start");await Until(()=>run.Fighting,"Wave 1");Need(!run.StartNextWave(p),"Cannot skip a live pack");
   var first=run.Wolves.First();int before=run.RemainingComposition.Points;first.Actor.Health.ApplyPacket(null,new DamageResult{mitigated=1000000});Need(run.RemainingComposition.Points==before-(int)first.Rank,"Living pack counts update");
   foreach(var wolf in run.Wolves.Where(w=>w.Actor.Alive).ToArray())wolf.Actor.Health.ApplyPacket(null,new DamageResult{mitigated=1000000});await Until(()=>run.Phase==WolfRunPhase.Intermission,"Intermission");
   Need(!run.StartNextWave(null)&&run.StartNextWave(p)&&!run.StartNextWave(p)&&run.Wave==2,"Initiator advances exactly once");Need(p.Health.Current<4000,"No forced between-wave heal");await Until(()=>run.Fighting,"Wave 2");Need(run.Wolves.Any(w=>w.Rank==WolfRank.Strong),"Strong wolf test pack");
   var strong=run.Wolves.Where(w=>w.Rank==WolfRank.Strong).ToArray();var positions=strong.ToDictionary(w=>w,w=>w.transform.position);
   await Until(()=>strong.Any(w=>w.FlankAttempts>0),"Natural strong wolf flank",15);await Capture("strong-wolf-flanking");
   await Until(()=>strong.All(w=>w.Tactic=="Committed"),"Bounded flank commits",10);Need(strong.All(w=>Vector3.Distance(w.transform.position,positions[w])>2),"Flank uses actual navigation");Need(strong.All(w=>!w.ApproachPoint.HasValue),"No endless circling");
   var attempts=strong.Sum(w=>w.FlankAttempts);run.Abandon();await Until(()=>WolfDungeonRun.Current==null&&!SceneManager.GetSceneByName("WolfSurvival").isLoaded,"Cleanup");
   File.WriteAllText("Logs/WolfCompactReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=true,checks,coverWalkMeters=meters,sightBlocked=true,navDetour=true,flankAttempts=attempts,boundedFlanks=true,noForcedHeal=true,nextWaveGuard=true},Newtonsoft.Json.Formatting.Indented));Debug.Log("DICEFREE_WOLF_COMPACT_OK");
  }
  catch(Exception e){File.WriteAllText("Logs/WolfCompactReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed=false,checks,error=e.ToString()},Newtonsoft.Json.Formatting.Indented));Debug.LogError(e);}
 }
}
