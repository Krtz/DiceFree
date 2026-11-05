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
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;

public static class ArboriumArrivalReview
{
 const string QuestId="quest.trimery.road-to-arborium",BaneId="quest.briarback.chieftains-bane";
 static int checks,walks;static float meters;static string phase;static readonly List<object> evidence=new();
 static Vector3 center=new(98.04f,0,304.74f);
 static void Need(bool value,string why){checks++;if(!value)throw new InvalidOperationException(why);}
 static void Phase(string value){phase=value;Write(false,null);}
 static void Write(bool passed,string failure){File.WriteAllText("Logs/ArboriumArrivalReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{passed,phase,checks,walks,meters,failure,evidence},Newtonsoft.Json.Formatting.Indented));}
 static async Task Until(Func<bool> test,string why,int seconds=40){var end=DateTime.UtcNow.AddSeconds(seconds);while(!test()){if(DateTime.UtcNow>end)throw new TimeoutException(why);await Task.Delay(20);}}
 static Vector3 Point(Vector3 p){Need(NavMesh.SamplePosition(p,out var hit,2,-1),"No navigation "+p);return hit.position;}
 static void Place(CombatActor p,Vector3 point){Need(p.Motor.Teleport(Point(point)),"Placement "+point);Physics.SyncTransforms();Object.FindAnyObjectByType<ExplorationCamera>().RecenterForTravel(p.transform);}
 static async Task Capture(string name){Directory.CreateDirectory("VisualBaselines/Arborium");await Task.Delay(250);ScreenCapture.CaptureScreenshot("VisualBaselines/Arborium/"+name+".png");await Task.Delay(100);}
 static long TotalXp(ExperienceProgression xp){long sum=xp.CurrentXp;for(int i=1;i<xp.Level;i++)sum+=30+3L*(i-1)*(i-1);return sum;}
 public static string Start()
 {
  Need(!EditorApplication.isPlayingOrWillChangePlaymode&&!UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty,"Clean Edit Mode required.");
  var guard=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);
  guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,new object[]{Path.Combine(Path.GetTempPath(),"DiceFree-Arborium-"+Guid.NewGuid())});
  UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");EditorApplication.EnterPlaymode();return "Started isolated Arborium review.";
 }
 public static string Run(){checks=walks=0;meters=0;evidence.Clear();_ = Execute();return "Started; inspect Logs/ArboriumArrivalReview.json.";}
 public static string Stop(){EditorApplication.ExitPlaymode();return "Stopped isolated review.";}
 public static async Task<object> Presentation()
 {
  CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready,"Presentation player");
  p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
  foreach(var b in Object.FindObjectsByType<MonoBehaviour>().Where(b=>b is AggroBehaviour||b is BasicAttack||b is QuilboarCombat||b is BriarbackChieftainEncounter))b.enabled=false;
  var camera=Object.FindAnyObjectByType<ExplorationCamera>();var city=GameObject.Find("Arborium - World Tree capital").transform;
  Place(p,center+new Vector3(0,0,72));await Capture("arrival-avenue");
  var guild=city.Find("04 - Adventurers Guild fortified hall");bool faded=guild.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m.color.a<=.2f));Need(faded,"Blocking guild tower must fade on the default gameplay camera");
  var registrar=Object.FindObjectsByType<ConversationTarget>().Single(n=>n.NpcId=="npc.arborium.guild-registrar");Place(p,registrar.ApproachPosition);var interactor=p.GetComponent<Interactor>();Need(interactor.Order(registrar),"Presentation registrar order");await Until(()=>interactor.Active==registrar,"Presentation registrar dialogue");
  var panel=Object.FindAnyObjectByType<ConversationPanel>();var stats=Object.FindAnyObjectByType<CharacterStatsHud>();Need(panel!=null,"Conversation panel exists");Need(panel.Bounds.Contains(panel.AcknowledgeBounds.min)&&panel.Bounds.Contains(panel.AcknowledgeBounds.max),"Acknowledge button inside dialogue");Need(stats==null||!stats.Bounds.Overlaps(panel.AcknowledgeBounds),"Acknowledge button must not be hidden by statistics HUD");await Capture("guild-welcome");Need(registrar.Acknowledge(p),"Visible conversation acknowledgement");interactor.Cancel();
  var anchor=Object.FindObjectsByType<ResurrectionAnchor>().Single(a=>a.StableId=="anchor.arborium");Need(anchor.name=="Arborium resurrection anchor","City revive label");
  Place(p,center+new Vector3(0,0,34));await Capture("central-avenue-gameplay");
  Place(p,center+new Vector3(0,0,80));camera.SetPresentationPose(center+new Vector3(28,28,122),Quaternion.LookRotation((center+new Vector3(0,12,76))-(center+new Vector3(28,28,122))),50);await Capture("north-gate-presentation");camera.ClearPresentationPose();
  var result=new{passed=true,blockingGuildTowerFaded=faded,confirmationVisible=true,acknowledgement=true,cityAnchorNamed=true};File.WriteAllText("Logs/ArboriumPresentationReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
 static async Task Execute()
 {
  try
  {
   CombatActor p=null;await Until(()=>(p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&p.Motor.Ready&&p.GetComponent<ManifestationPersistence>().Ready,"Player");
   p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.Health.Invulnerable=true;
   foreach(var b in Object.FindObjectsByType<MonoBehaviour>().Where(b=>b is AggroBehaviour||b is BasicAttack||b is QuilboarCombat||b is BriarbackChieftainEncounter))b.enabled=false;
   foreach(var actor in CombatActor.All)if(actor.Motor!=null&&actor.Motor.Ready)actor.Motor.Stop();
   var camera=Object.FindAnyObjectByType<ExplorationCamera>();var interactor=p.GetComponent<Interactor>();var journal=p.GetComponent<QuestJournal>();var save=p.GetComponent<ManifestationPersistence>();
   var navType=typeof(EditorWindow).Assembly.GetType("UnityEditor.AI.NavMeshVisualizationSettings");navType?.GetProperty("showNavMesh",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic)?.SetValue(null,false);
   var view=Resources.FindObjectsOfTypeAll(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).FirstOrDefault();if(view!=null)view.GetType().GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)?.SetValue(view,false);
   Phase("first city views");Place(p,center+new Vector3(0,0,83));await Capture("north-gate-gameplay");
   camera.SetPresentationPose(center+new Vector3(28,28,122),Quaternion.LookRotation((center+new Vector3(0,12,76))-(center+new Vector3(28,28,122))),50);await Capture("north-gate-presentation");
   camera.SetPresentationPose(center+new Vector3(125,155,210),Quaternion.LookRotation((center+new Vector3(0,42,0))-(center+new Vector3(125,155,210))),50);await Capture("capital-overview");camera.ClearPresentationPose();
   Phase("well healing and raw damage");
   p.Stats.SetEncounterMaximumHp(2000);p.Health.Restore();p.Health.SetEncounterHp(100);
   var well=Object.FindAnyObjectByType<HealingArea>();Need(new SerializedObject(well).FindProperty("hpPerSecond").floatValue==25,"Well must be 25 HP/sec");
   Place(p,well.transform.position+Vector3.forward*3.5f);Need(well.Contains(p),"Walkable well edge must be in healing radius");float healed=0;void OnHeal(float amount)=>healed+=amount;p.Health.Healed+=OnHeal;float at=Time.time;await Task.Delay(2200);float elapsed=Time.time-at;p.Health.Healed-=OnHeal;float observedRate=healed/elapsed;Need(Mathf.Abs(observedRate-25)<1.5f,"Observed well healing rate "+observedRate);
   Place(p,well.transform.position+Vector3.forward*8);healed=0;p.Health.Healed+=OnHeal;await Task.Delay(350);p.Health.Healed-=OnHeal;Need(healed==0,"Well must stop outside radius");
   var heavy=AssetDatabase.LoadAssetAtPath<PhysicalSkillDefinition>("Assets/_DiceFree/Settings/Skills/Physical/Heavy Strike.asset");
   var target=CombatActor.All.First(a=>p.IsHostileTo(a));
   for(int rank=0;rank<=6;rank++){Need(heavy.FlatDamage(rank)==10*rank,"Heavy flat damage rank "+rank);var baseline=DamageResolver.Calculate(p.Stats,target.Stats,heavy.attack,coefficientOverride:heavy.DamageCoefficient(rank));var scaled=DamageResolver.Calculate(p.Stats,target.Stats,heavy.attack,coefficientOverride:heavy.DamageCoefficient(rank),skillFlatDamage:heavy.FlatDamage(rank));Need(Mathf.Abs(scaled.baseRaw-baseline.baseRaw-10*rank)<.001f,"Raw packet addition rank "+rank);}
   evidence.Add(new{wellHpPerSecond=25,observedHpPerSecond=observedRate,outsideRadiusStopped=true,heavyRank1=10,heavyRank6=60,rawPacketAdditionVerified=true});
   Phase("chieftains bane 10000 XP");
   var bane=journal.Definitions.Single(q=>q.stableId==BaneId);Need(bane.rewardXp==10000&&!bane.repeatable,"Bane reward/identity");var elder=Object.FindObjectsByType<QuestGiver>().Single(g=>g.Quest==bane);Place(p,elder.ApproachPosition);Need(interactor.Order(elder),"Elder order");await Until(()=>interactor.Active==elder,"Elder dialogue");Need(elder.Accept(p,bane),"Bane acceptance");interactor.Cancel();var chief=Object.FindAnyObjectByType<BriarbackChieftainEncounter>().GetComponent<CombatActor>();chief.Health.Invulnerable=false;chief.Health.ApplyPacket(p,new DamageResult{mitigated=1000000});Need(journal.GetProgress(BaneId).status==QuestStatus.ReadyToTurnIn,"Bane kill credit");
   Place(p,elder.ApproachPosition);Need(interactor.Order(elder),"Elder return");await Until(()=>interactor.Active==elder,"Elder return dialogue");var xp=p.GetComponent<ExperienceProgression>();long before=TotalXp(xp);Need(elder.TurnIn(p,bane),"Bane turn-in");Need(TotalXp(xp)-before==10000&&!elder.TurnIn(p,bane),"Exact 10000 XP, once");Need(p.DisplayName.EndsWith(" the Chieftain's Bane",StringComparison.Ordinal),"Earned title retained");await Capture("chieftains-bane-reward");interactor.Cancel();evidence.Add(new{baneXp=10000,exactOnce=true,title=true,scriptedBossFixture=true});
   Phase("Trimery road report acceptance");
   var journey=journal.Definitions.Single(q=>q.stableId==QuestId);Need(journey.completedQuestIds.Length==0&&journey.requiredLevel==1,"City access must not require boss or dungeon");var captain=Object.FindObjectsByType<QuestGiver>().Single(g=>g.Quest==journey);Place(p,captain.ApproachPosition);Need(interactor.Order(captain),"Captain order");await Until(()=>interactor.Active==captain,"Captain dialogue");Need(captain.Accept(p,journey),"Journey acceptance");await Capture("trimery-road-report");interactor.Cancel();
   async Task Walk(Vector3 requested){var dest=Point(requested);var path=new NavMeshPath();Need(NavMesh.CalculatePath(p.transform.position,dest,-1,path)&&path.status==NavMeshPathStatus.PathComplete,"Complete city path "+requested);Need(p.Motor.MoveTo(dest),"Walk order "+dest);var prior=p.transform.position;var end=DateTime.UtcNow.AddSeconds(45);while(Vector3.Distance(p.transform.position,dest)>.9f){Need(DateTime.UtcNow<end,"Walk blocked "+dest+" from "+p.transform.position);await Task.Delay(25);meters+=Vector3.Distance(prior,p.transform.position);prior=p.transform.position;}p.Motor.Stop();walks++;}
   Phase("walking north west south into Arborium");
   foreach(var point in new[]{new Vector3(248,0,250),new Vector3(230,0,367),center+new Vector3(137,0,61),center+new Vector3(114,0,73),center+new Vector3(92,0,86),center+new Vector3(0,0,86),center+new Vector3(0,0,72)})await Walk(point);
   await Until(()=>journal.GetProgress(QuestId).stage==1,"City gate arrival");Need(p.GetComponent<MinimapHud>().MapName=="Arborium","City map name");await Capture("arrival-avenue");
   var registrar=Object.FindObjectsByType<ConversationTarget>().Single(n=>n.NpcId=="npc.arborium.guild-registrar");await Walk(registrar.ApproachPosition);Need(interactor.Order(registrar),"Registrar order");await Until(()=>interactor.Active==registrar,"Registrar dialogue");await Capture("guild-welcome");before=TotalXp(xp);Need(registrar.Acknowledge(p),"Registrar acknowledgement");Need(journal.GetProgress(QuestId).status==QuestStatus.Completed&&TotalXp(xp)-before==1500,"Guild talk must finish road report, once");Need(!registrar.Acknowledge(p),"Duplicate acknowledgement");interactor.Cancel();
   Phase("city services and ring walks");
   foreach(var service in Object.FindObjectsByType<InteractionTarget>().Where(n=>n.name=="Arborium market trader"||n.name=="Arborium exchange banker")){await Walk(service.ApproachPosition);Need(interactor.Order(service),"Service order "+service.name);await Until(()=>interactor.Active==service,"Service dialogue "+service.name);Need(service.CanInteract(p),"Service available "+service.name);await Capture(service.name.Contains("banker")?"city-bank":"city-market");interactor.Cancel();}
   foreach(var point in new[]{center+new Vector3(45,0,0),center+new Vector3(0,0,-24),center+new Vector3(-44,0,0),center+new Vector3(0,0,25)}){await Walk(point);await Capture("ring-walk-"+walks);}
   var anchor=Object.FindObjectsByType<ResurrectionAnchor>().Single(a=>a.StableId=="anchor.arborium");await Walk(anchor.transform.position);await Until(()=>p.GetComponent<RespawnAtAnchor>().AnchorId=="anchor.arborium","City resurrection activation");Need(NavMesh.SamplePosition(anchor.transform.position,out var safe,.5f,-1),"Safe anchor navigation");await Capture("city-resurrection-stone");save.Flush();Need(journal.GetProgress(QuestId).status==QuestStatus.Completed,"Saved arrival complete");
   Need(Object.FindObjectsByType<Transform>().Any(t=>t.name=="Broken western trade road (no Roadwarden yet)"),"Original broken highway preserved");
   evidence.Add(new{journey=QuestId,finishedByGuildConversation=true,xp=1500,northWestSouth=true,shopAndBankAvailable=true,cityAnchor="anchor.arborium",directRoadStillBroken=true,bossAndDungeonNotRequired=true});
   Phase("passed");Write(true,null);Debug.Log("DICEFREE_ARBORIUM_ARRIVAL_OK");
  }
  catch(Exception e){Write(false,e.ToString());Debug.LogError(e);}
 }
}
