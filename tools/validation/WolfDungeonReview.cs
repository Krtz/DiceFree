using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiceFree.ApplicationFlow;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.Foundation;
using DiceFree.Ironpine;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
public static class WolfDungeonReview
{
 static int checks;static string phase;static readonly List<object> waves=new();static readonly List<string> errors=new();
 static void Need(bool b,string text){checks++;if(!b)throw new InvalidOperationException(text);}
 static void Log(string m,string stack,LogType kind){if(kind==LogType.Error||kind==LogType.Exception||kind==LogType.Assert)errors.Add(m);}
 static void Record(bool passed,string error=null)=>File.WriteAllText("Logs/WolfDungeonReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed,phase,checks,error,unityErrors=errors,waves},Newtonsoft.Json.Formatting.Indented));
 static void Phase(string p){phase=p;Record(false);}
 static async Task Until(Func<bool> f,string why,int seconds=45){var end=DateTime.UtcNow.AddSeconds(seconds);while(!f()){Need(DateTime.UtcNow<end,"Timeout: "+why);await Task.Delay(25);}}
 public static string Start()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean Edit Mode required");
  var guard=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,new object[]{Path.Combine(Path.GetTempPath(),"DiceFree-WolfSurvival-"+Guid.NewGuid())});EditorApplication.EnterPlaymode();return "Started isolated wolf review";
 }
 public static string Stop(){EditorApplication.ExitPlaymode();return "Stopped isolated wolf review";}
 public static string Run(){checks=0;waves.Clear();errors.Clear();Application.logMessageReceived+=Log;_=Execute();return "Started; inspect Logs/WolfDungeonReview.json";}
 static async Task Capture(string name){Directory.CreateDirectory("VisualBaselines/WolfSurvival");await Task.Delay(200);ScreenCapture.CaptureScreenshot("VisualBaselines/WolfSurvival/"+name+".png");await Task.Delay(120);}
 static long TotalXp(CombatActor p){var xp=p.GetComponent<ExperienceProgression>();long total=xp.CurrentXp;for(int i=1;i<xp.Level;i++)total+=30+3L*(i-1)*(i-1);return total;}
 static async Task Execute()
 {
  try
  {
   Phase("point schedule across 1000 seeds");var first=new HashSet<int>();
   for(int seed=0;seed<1000;seed++)for(int wave=1;wave<=10;wave++){var p=WolfWavePlan.Create(wave,seed);Need(p.Points==wave+2,"Budget mismatch");Need(p.Count>0,"Empty wave");if(wave==1){Need(p.Alpha==0&&p.Strong<=1,"First wave must match the two approved packs");first.Add(p.Strong);}if(wave==10)Need(p.Alpha==4&&p.Weak==0&&p.Strong==0,"Final wave must be four alphas");var replay=WolfWavePlan.Create(wave,seed);Need(p.Weak==replay.Weak&&p.Strong==replay.Strong&&p.Alpha==replay.Alpha,"Seed must replay");}Need(first.Count==2,"Both first-wave choices must occur");
   CombatActor hero=null;await Until(()=>(hero=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&hero.Motor.Ready&&hero.GetComponent<ManifestationPersistence>().Ready,"Player ready");
   hero.GetComponent<TraversalInput>().enabled=false;hero.GetComponent<CombatInput>().enabled=false;hero.Health.Invulnerable=true;
   var clock=Object.FindAnyObjectByType<WorldDayNightCycle>();clock.SetHour(11,false);var clockEnabled=clock.enabled;var outsideLight=Object.FindObjectsByType<Light>().Where(l=>l.type==LightType.Directional&&(l.cullingMask&(1<<8))!=0).ToDictionary(l=>l,l=>l.enabled);
   var portal=Object.FindAnyObjectByType<WolfDungeonPortal>();Need(portal!=null,"Physical entrance missing");
   void PlaceAtPortal(){Need(NavMesh.SamplePosition(portal.ApproachPosition,out var hit,1,-1)&&hero.Motor.TeleportAcrossMaps(hit.position),"Entrance fixture");Object.FindAnyObjectByType<ExplorationCamera>().RecenterForTravel(hero.transform);Physics.SyncTransforms();}
   async Task<WolfDungeonRun> Enter(){PlaceAtPortal();Need(portal.CanInteract(hero),"Reachable real entrance");portal.Interact(hero);Need(LoadingScreen.Busy,"Additive travel overlay missing");await Until(()=>WolfDungeonRun.Current!=null&&WolfDungeonRun.Current.StagingReady&&!LoadingScreen.Busy,"Staging loaded");return WolfDungeonRun.Current;}
   Phase("physical entrance and staging");PlaceAtPortal();await Capture("ironpine-entrance");var run=await Enter();Need(!clock.enabled,"Arena must own temporary lighting");Need(hero.GetComponent<RunLoadoutLock>().InsideDungeon&&!hero.GetComponent<RunLoadoutLock>().Locked,"Staging permits equipment preparation");Need(!run.StartNow(null),"Only initiator starts");var dummy=GameObject.Find("Practice target — no rewards").GetComponent<CombatActor>();Need(dummy.Health.Invulnerable&&dummy.Stats.Definition.experienceReward==0,"Safe no-reward practice target");await Capture("moonlit-staging");
   var walk=WolfDungeonRun.Origin+new Vector3(9,0,-4);Need(hero.Motor.MoveTo(walk),"Arena click path");await Until(()=>Vector3.Distance(hero.transform.position,walk)<1,"Arena walk");hero.Motor.Stop();
   Need(run.StartNow(hero)&&!run.StartNow(hero),"Start once");Need(hero.GetComponent<RunLoadoutLock>().Locked,"Active loadout lock");
   long before=TotalXp(hero),expectedXp=0;float healthLost=0;int liveBites=0;
   Phase("ten actual waves and reused combat");
   for(int wave=1;wave<=10;wave++)
   {
    await Until(()=>run.Fighting&&run.Wave==wave,"Wave "+wave,25);await Task.Delay(180);
    Need(run.Composition.Points==wave+2&&run.Remaining==run.Composition.Count,"Spawned composition "+wave);Need(run.Wolves.Sum(w=>(int)w.Rank)==wave+2,"Spawned point value "+wave);
    Need(run.Wolves.All(w=>w.GetComponent<DiceFree.Items.FixedWorldDrop>()==null&&w.Actor.Stats.Definition.stableId.StartsWith("enemy.wolf-survival.")),"No overworld drops or quest identity leakage");
    Need(run.Wolves.All(w=>w.Actor.Motor.Ready&&w.GetComponent<IronpineCreaturePresentation>().ModelAnimator!=null),"Navigating animated wolves");
    if(wave==1)
    {
     var wolf=run.Wolves.First();var previous=wolf.transform.position;await Task.Delay(1600);Need(Vector3.Distance(wolf.transform.position,previous)>1,"Wolves must pursue the player across the arena");
     hero.Stats.SetEncounterMaximumHp(1000000);hero.Health.Restore();hero.Health.Invulnerable=false;
     void Hit(CombatActor a,DamageResult d){healthLost+=d.mitigated;liveBites++;}hero.Health.Damaged+=Hit;
     await Until(()=>wolf.GetComponent<BasicAttack>().Target==hero,"Live wolf attack acquisition");await Until(()=>liveBites>0,"Natural bite against invulnerable test hero",15);hero.Health.Damaged-=Hit;
     await Capture("wave-1-combat");hero.Health.Invulnerable=true;
    }
    if(wave==10){Need(run.Wolves.Count==4&&run.Wolves.All(w=>w.Rank==WolfRank.Alpha),"Four alpha finale");await Capture("wave-10-four-alphas");}
    expectedXp+=run.Wolves.Sum(w=>w.Actor.Stats.Definition.experienceReward);
    foreach(var wolf in run.Wolves.ToArray())wolf.Actor.Health.ApplyPacket(hero,new DamageResult{mitigated=1000000});
    await Until(()=>run.Phase==WolfRunPhase.Intermission||run.Phase==WolfRunPhase.Victory,"Wave clear");Need(run.WavesCleared==wave&&run.Remaining==0,"Clear only after last wolf");
    waves.Add(new{wave,points=run.Composition.Points,weak=run.Composition.Weak,strong=run.Composition.Strong,alpha=run.Composition.Alpha});Record(false);
   }
   Need(TotalXp(hero)-before==expectedXp,"Normal wolf kill XP must be awarded once");Need(run.Phase==WolfRunPhase.Victory&&run.WavesCleared==10,"Victory after ten waves");await Capture("survival-victory");run.Abandon();await Until(()=>WolfDungeonRun.Current==null&&!SceneManager.GetSceneByName("WolfSurvival").isLoaded,"Victory cleanup");Need(!hero.GetComponent<RunLoadoutLock>().InsideDungeon&&!hero.GetComponent<RunLoadoutLock>().Locked,"Exit clears run lock");Need(clock.enabled==clockEnabled&&outsideLight.All(p=>p.Key!=null&&p.Key.enabled==p.Value),"Lighting restored");
   Phase("wipe and repeat entry");run=await Enter();Need(run.StartNow(hero),"Fresh run starts");await Until(()=>run.Fighting,"Fresh first wave");Need(run.Wave==1,"New run resets waves");hero.Health.Invulnerable=false;hero.Health.ApplyPacket(null,new DamageResult{mitigated=1000000});await Until(()=>WolfDungeonRun.Current==null&&!SceneManager.GetSceneByName("WolfSurvival").isLoaded,"Solo wipe cleanup");Need(hero.Alive&&!hero.GetComponent<RunLoadoutLock>().InsideDungeon,"Wipe returns to resurrection anchor");hero.Health.Invulnerable=true;
   run=await Enter();run.Abandon();await Until(()=>WolfDungeonRun.Current==null,"Staging abandon");Need(!LoadingScreen.Busy,"No loading overlay leak");
   Phase("active-run menu exit and lease release");run=await Enter();Need(run.StartNow(hero),"Active exit fixture");Need(hero.GetComponent<ReturnToStartMenu>().Return(),"Return to menu in active run");await Until(()=>SceneManager.GetActiveScene().name=="StartMenu"&&!LoadingScreen.Busy,"Menu load");Need(WolfDungeonRun.Current==null&&!SceneManager.GetSceneByName("WolfSurvival").isLoaded,"Active-run scene cleanup");
   Need(errors.Count==0,"Unity errors "+string.Join(";",errors));Phase("passed");Record(true);Debug.Log("DICEFREE_WOLF_SURVIVAL_OK");
  }
  catch(Exception e){Record(false,e.ToString());Debug.LogError(e);}
  finally{Application.logMessageReceived-=Log;}
 }
}
