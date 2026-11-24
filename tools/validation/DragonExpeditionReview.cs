// Unity CLI eval_file; use a disposable copied save in the development player.
var qa=System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName,"QA");System.IO.Directory.CreateDirectory(qa);
var checks=new System.Collections.Generic.List<string>();var errors=new System.Collections.Generic.List<string>();
Component hero=null,journal=null,inventory=null,wallet=null,xp=null;string phase="startup",failure=null;
Type T(string n)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(n)).First(t=>t!=null);
Component[] All(string n)=>UnityEngine.Object.FindObjectsByType(T(n)).Cast<Component>().ToArray();
Component Part(Component c,string n)=>c.GetComponent(T(n));
object Read(object c,string n)=>c?.GetType().GetProperty(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)?.GetValue(c)??c?.GetType().GetField(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)?.GetValue(c);
void Set(object c,string n,object v){var p=c.GetType().GetProperty(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic);if(p?.CanWrite==true)p.SetValue(c,v);else c.GetType().GetField(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(c,v);}
object Call(object c,string n,params object[] a){var m=c.GetType().GetMethods(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).Where(m=>m.Name==n).First(m=>{var p=m.GetParameters();return p.Length>=a.Length&&p.Take(a.Length).Select((v,i)=>a[i]==null||v.ParameterType.IsInstanceOfType(a[i])).All(v=>v)&&p.Skip(a.Length).All(v=>v.IsOptional);});return m.Invoke(c,m.GetParameters().Select((v,i)=>i<a.Length?a[i]:v.DefaultValue).ToArray());}
object Static(string name,string property)=>T(name).GetProperty(property,System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static).GetValue(null);
object E(string type,string name)=>Enum.Parse(T(type),name);
string Q(string s)=>s==null?"null":"\""+s.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r"," ").Replace("\n"," ")+"\"";
void Report(string status)=>System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"dragon-expedition-review.json"),"{\"status\":"+Q(status)+",\"phase\":"+Q(phase)+",\"checks\":"+checks.Count+",\"passed\":["+string.Join(",",checks.Select(Q))+"],\"error\":"+Q(failure)+",\"unityErrors\":["+string.Join(",",errors.Select(Q))+"]}");
void Need(bool value,string name){if(!value)throw new Exception(name);checks.Add(name);Report("running");}
void Phase(string name){phase=name;Report("running");}
bool Busy()=>(bool)Static("DiceFree.ApplicationFlow.LoadingScreen","Busy")||(bool)Static("DiceFree.ApplicationFlow.WorldFaceCrossing","Busy");
async System.Threading.Tasks.Task Until(Func<bool> condition,string label,int seconds=80){var end=DateTime.UtcNow.AddSeconds(seconds);while(!condition()){if(DateTime.UtcNow>end)throw new TimeoutException(label);await System.Threading.Tasks.Task.Delay(50);}}
object Quest(string id)=>((System.Collections.IEnumerable)Read(journal,"Definitions")).Cast<object>().Single(q=>(string)Read(q,"stableId")==id);
object Progress(string id)=>Call(journal,"GetProgress",id);
string Status(string id)=>Read(Progress(id),"status").ToString();
void SetQuest(string id,string status,int count)
{
 var records=(Array)Call(journal,"CaptureState");var record=records.Cast<object>().Single(r=>(string)Read(r,"questId")==id);
 Set(record,"status",E("DiceFree.Quests.QuestStatus",status));Set(record,"stage",0);Set(record,"count",count);Set(record,"alternatives",Array.CreateInstance(T("DiceFree.Quests.ObjectiveCount"),0));Call(journal,"RestoreState",records);
}
long TotalXp(){long amount=(int)Read(xp,"CurrentXp");var curve=Read(xp,"curve");for(int level=1;level<(int)Read(xp,"Level");level++)amount+=(int)Call(curve,"ToNextLevel",level);return amount;}
long Gold()=>(long)Read(wallet,"Gold");
Component View()=>All("DiceFree.Core.ExplorationCamera").Single();
Vector3 Nav(Vector3 point){if(Physics.Raycast(point+Vector3.up*120,Vector3.down,out var ground,280,1<<8))point=ground.point;if(!UnityEngine.AI.NavMesh.SamplePosition(point,out var result,3,1))throw new Exception("No navigation: "+point);return result.position;}
void Warp(Vector3 point){if(!(bool)Call(Read(hero,"Motor"),"TeleportAcrossMaps",Nav(point)))throw new Exception("Hero placement failed");Call(View(),"RecenterForTravel",hero.transform);Call(All("DiceFree.World.WorldFogOfWar").Single(),"RefreshVisionNow");}
void Quiet(){foreach(var ai in All("DiceFree.AI.AggroBehaviour"))((Behaviour)ai).enabled=false;foreach(var attack in All("DiceFree.Combat.BasicAttack"))Call(attack,"Cancel");Set(Read(hero,"Health"),"Invulnerable",true);}
void Hit(Component victim)
{
 var packet=Activator.CreateInstance(T("DiceFree.Combat.DamageResult"));Set(packet,"raw",1000000f);Set(packet,"mitigated",1000000f);
 Call(Read(victim,"Health"),"ApplyPacket",hero,packet);
}
async System.Threading.Tasks.Task Capture(string name,float zoom=30)
{
 Set(View(),"ZoomDistance",zoom);Call(View(),"RecenterForTravel",hero.transform);await System.Threading.Tasks.Task.Delay(450);
 if(!Application.isBatchMode)ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(qa,name+".png"));
 else
 {
  // Batch players do not refresh the screen buffer. Render world geometry
  // explicitly; screen-space overlay UI is intentionally absent in this path.
  var camera=Camera.main;var target=new RenderTexture(1600,900,24);var old=camera.targetTexture;var active=RenderTexture.active;Texture2D texture=null;
  try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture=new Texture2D(1600,900,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1600,900),0,0);texture.Apply();System.IO.File.WriteAllBytes(System.IO.Path.Combine(qa,name+"-world-only.png"),texture.EncodeToPNG());}
  finally{camera.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.Destroy(target);if(texture!=null)UnityEngine.Object.Destroy(texture);}
 }
 await System.Threading.Tasks.Task.Delay(180);
}
async System.Threading.Tasks.Task Work(Component target)
{
 var interactor=Part(hero,"DiceFree.World.Interactor");Warp((Vector3)Read(target,"ApproachPosition"));await System.Threading.Tasks.Task.Delay(100);
 Need((bool)Read(target,"isActiveAndEnabled")&&(bool)Call(target,"Available",hero),"Survey target is available: "+target.name);
 Need((bool)Call(interactor,"Order",target),"Issue normal survey interaction: "+target.name);
 await Until(()=>Read(target,"Worker") as Component==hero,"Survey work begins");
 await Until(()=>Read(target,"Worker")==null,"Survey work completes",15);
}
void Log(string message,string stack,LogType type){if(type is LogType.Error or LogType.Exception or LogType.Assert)errors.Add(message);}
async System.Threading.Tasks.Task Review()
{
 try
 {
  Component menu=null;await Until(()=>(menu=All("DiceFree.Advancement.StartMenuController").FirstOrDefault())!=null&&!Busy(),"Menu ready");Need((bool)Call(menu,"TryPlayManifestation","class.psionic-knight"),"Load isolated Knight save");
  await Until(()=>(hero=All("DiceFree.Characters.TraversalInput").FirstOrDefault())!=null&&!Busy()&&(bool)Read(Part(hero,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Hero ready");hero=Part(hero,"DiceFree.Combat.CombatActor");
  journal=Part(hero,"DiceFree.Quests.QuestJournal");inventory=Part(hero,"DiceFree.Items.CarriedInventory");wallet=Part(hero,"DiceFree.Items.GoldWallet");xp=Part(hero,"DiceFree.Progression.ExperienceProgression");Quiet();
  ((Behaviour)Part(hero,"DiceFree.Characters.CombatInput")).enabled=false;((Behaviour)Part(hero,"DiceFree.UI.WorldHoverFeedback")).enabled=false;
  Call(All("DiceFree.World.WorldDayNightCycle").Single(),"SetHour",15f,false);
  const string first="quest.world2.first-foothold",survey="quest.world2.two-watchfires",report="quest.world2.unfinished-record",dragonId="quest.arborium.ancient-road-dragon";
  Phase("Dragon reward data and equipment");
  var definitions=Resources.LoadAll("DiceFree/DungeonRewards/AncientRoadDragon",T("DiceFree.Items.ItemDefinition"));
  var ids=new[]{"wyrmfang-cuirass","roostguard-bulwark","embermender-vestments","jadescale-arcanist-robe","rooststalkers-grips","dragontrail-greaves"};
  var additions=ids.Select(id=>definitions.Single(d=>(string)Read(d,"stableId")=="item.ancient-road-dragon."+id)).ToArray();
  Need(definitions.Length==9&&additions.Length==6,"Nine dragon rewards load, including all six additions");
  Need(additions.Take(4).All(d=>Read(d,"slot").ToString()=="Chest")&&additions.All(d=>(int)Read(d,"itemLevel")==55),"Four new chest pieces and both extras are item level 55");
  Need(additions.All(d=>(string)Read(d,"sourceId")=="boss.ancient-road-dragon"&&(Sprite)Read(d,"icon")!=null),"Every new item has its dragon source and shipped icon");
  Need(new[]{"agility","strength","spirit","intelligence"}.Select((attribute,i)=>(float)Read(Read(Read(additions[i],"stats"),"attributes"),attribute)>=24).All(v=>v),"Each requested chest has at least 24 of its intended primary stat");
  var equipment=Part(hero,"DiceFree.Items.Equipment");var owned=new System.Collections.Generic.List<string>();
  foreach(var item in additions)
  {
   var granted=Call(inventory,"Grant",(string)Read(item,"stableId"));var id=(string)Read(granted,"instanceId");owned.Add(id);
   Need((bool)Call(equipment,"Equip",id,Read(item,"slot")),"Equip new reward: "+(string)Read(item,"displayName"));
  }
  Need(owned.All(id=>Call(inventory,"Find",id)!=null)&&additions.All(d=>Call(inventory,"Resolve",(string)Read(d,"stableId"))!=null),"All six new items have durable owned identities and resolvable definitions");
  Phase("Real dragon waiting room and treasury");
  var portal=All("DiceFree.Dungeons.AncientRoadDragonLairPortal").Single(p=>p.gameObject.scene.name=="Cornberg");Warp((Vector3)Read(portal,"ApproachPosition"));await Until(()=>!(bool)Read(hero,"InCombat")&&!Busy(),"Hero ready to enter dragon lair");
  Call(portal,"Interact",hero);await Until(()=>Static("DiceFree.Dungeons.AncientRoadDragonRun","Current")!=null,"Dragon waiting room");var run=Static("DiceFree.Dungeons.AncientRoadDragonRun","Current");
  await Until(()=>(bool)Read(run,"StagingReady"),"Waiting room ready");Need((bool)Call(run,"StartNow",hero),"Start dragon run through its normal waiting room");
  Component encounter=null;await Until(()=>(encounter=All("DiceFree.Dungeons.AncientRoadDragonEncounter").FirstOrDefault())!=null&&!Busy(),"Dragon arena loaded");
  var pool=Read(encounter,"Rewards");var normal=((System.Collections.IEnumerable)Read(pool,"items")).Cast<object>().ToArray();
  Need(normal.Length==8&&(string)Read(Read(pool,"rare"),"stableId")=="item.ancient-road-dragon.mantle","Original rare mantle branch and eight-item ordinary pool are intact");
  var randomState=UnityEngine.Random.state;int rare=0,single=0,doubleDrop=0;var seen=new System.Collections.Generic.HashSet<string>();
  try
  {
   UnityEngine.Random.InitState(551124);
   for(int i=0;i<6000;i++)
   {
    var roll=(Array)Call(pool,"Roll");var rolled=roll.Cast<object>().Select(d=>(string)Read(d,"stableId")).ToArray();
    if(rolled.Contains("item.ancient-road-dragon.mantle")){rare++;if(roll.Length!=1)throw new Exception("Rare branch produced multiple drops");}
    else{if(roll.Length==1)single++;else if(roll.Length==2&&rolled.Distinct().Count()==2)doubleDrop++;else throw new Exception("Normal drop rules changed");}
    foreach(var id in rolled)seen.Add(id);
   }
  }
  finally{UnityEngine.Random.state=randomState;}
  Need(seen.Count==9&&rare>200&&rare<420&&single>3500&&doubleDrop>1000,"Seeded rolls reach every reward and retain rare, single and distinct double drop branches");
  var dragon=Read(encounter,"EncounterActor") as Component;Hit(dragon);Need(!(bool)Read(dragon,"Alive"),"Defeat the real dragon encounter");
  Component room=null;await Until(()=>(room=All("DiceFree.Dungeons.DungeonRewardRoom").FirstOrDefault())!=null&&(bool)Call(room,"HasChoice",hero)&&!Busy(),"Dragon treasury offer");
  int countBefore=((Array)Read(inventory,"Items")).Length;Need((bool)Call(room,"Choose",hero,0),"Claim a normal dragon treasury item");
  Need(((Array)Read(inventory,"Items")).Length==countBefore+1&&!(bool)Call(room,"Choose",hero,0),"Treasury grants exactly one chosen item and rejects a duplicate claim");
  await Until(()=>Static("DiceFree.Dungeons.AncientRoadDragonRun","Current")==null&&!Busy()&&hero.transform.position.x<1000,"Return from treasury");Quiet();
  Phase("World Two quest prerequisites and travel");
  Need(new[]{first,survey,report}.All(id=>Read(Quest(id),"category").ToString()=="Main"),"All three new quests are registered as main-story quests");
  SetQuest(dragonId,"Available",0);Need(!(bool)Call(journal,"CanAccept",Quest(first)),"Expedition requires the completed World One dragon quest");SetQuest(dragonId,"Completed",1);
  Need((bool)Call(journal,"CanAccept",Quest(first))&&!(bool)Call(journal,"CanAccept",Quest(survey)),"Chapter begins after the dragon and later quests stay locked");
  var pass=All("DiceFree.ApplicationFlow.IronpineSouthPass").Single(p=>p.gameObject.scene.name=="Cornberg");Warp((Vector3)Read(pass,"ApproachPosition"));await Until(()=>(bool)Read(pass,"IsOpen")&&!Busy(),"Southern pass opens");Call(pass,"Interact",hero);
  await Until(()=>hero.transform.position.x>63000&&!Busy(),"World Two arrival",100);Quiet();
  var ivara=All("DiceFree.Quests.QuestGiver").Single(g=>g.name=="Expedition Warden Ivara");var tamsin=All("DiceFree.World.ConversationTarget").Single(g=>g.name=="Envoy Tamsin");
  var cases=All("DiceFree.World.TimedInteractionTarget").Where(t=>(string)Read(t,"ContentId") is "work.world2.needle-survey" or "work.world2.crown-survey").ToArray();
  Need(cases.Length==2&&cases.All(c=>!(bool)Call(c,"Available",hero)),"Both survey sites exist and reject work before their quest");
  Warp((Vector3)Read(ivara,"ApproachPosition"));await System.Threading.Tasks.Task.Delay(600);
  Need((bool)Read(Part(ivara,"DiceFree.Quests.QuestWorldMarker"),"IsExclamation"),"Ivara displays the available main-quest marker");await Capture("cinderwatch-expedition");
  Need((bool)Call(ivara,"Accept",hero,Quest(first)),"Accept A Foothold Beyond the Pines through Ivara");
  var bounty=Quest("quest.cinderwatch.repeat-ashscale-hunt");if((bool)Call(journal,"CanAccept",bounty))Call(journal,"Accept",bounty);
  for(int i=0;i<8;i++)
  {
   var enemy=All("DiceFree.Combat.CombatActor").Where(a=>a.gameObject.scene.name=="WorldTwoArrival"&&(bool)Read(a,"Alive")&&(string)Read(a,"FamilyId")=="ashscale.lizardman").OrderBy(a=>Vector3.Distance(hero.transform.position,a.transform.position)).First();
   Warp(enemy.transform.position+new Vector3(-3,0,-3));Hit(enemy);await System.Threading.Tasks.Task.Delay(60);
  }
  Need(Status(first)=="ReadyToTurnIn"&&(int)Read(Progress("quest.cinderwatch.repeat-ashscale-hunt"),"count")==8,"Eight real Ashscale kills credit the story and repeatable bounty together");
  Warp((Vector3)Read(ivara,"ApproachPosition"));long goldBefore=Gold(),xpBefore=TotalXp();Need((bool)Call(ivara,"TurnIn",hero,Quest(first)),"Turn in the first expedition quest");
  Need(Gold()-goldBefore==120&&TotalXp()-xpBefore==8000,"First main quest pays its authored XP and gold exactly once");Need(!(bool)Call(ivara,"TurnIn",hero,Quest(first)),"Completed first quest cannot pay again");
  Phase("Both watchposts and durable progress");Need((bool)Call(ivara,"Accept",hero,Quest(survey)),"Accept The Two Watchfires");
  var crown=cases.Single(c=>(string)Read(c,"ContentId")=="work.world2.crown-survey");var needle=cases.Single(c=>(string)Read(c,"ContentId")=="work.world2.needle-survey");
  await Work(crown);Need((int)Read(Progress(survey),"count")==1,"Ember Crown can be surveyed first");await Capture("ember-crown-survey");
  Need(!(bool)Call(crown,"Available",hero),"An already surveyed site cannot give duplicate progress");
  var saved=(Array)Call(journal,"CaptureState");Need((bool)Call(journal,"CanRestore",saved),"Partially completed unordered survey progress validates for saving");Call(journal,"RestoreState",saved);
  Need((int)Read(Progress(survey),"count")==1&&!(bool)Call(crown,"Available",hero)&&(bool)Call(needle,"Available",hero),"Restoring partial survey progress keeps only the other site available");
  var presentation=Part(needle,"DiceFree.World.GuildWorkSitePresentation");await System.Threading.Tasks.Task.Delay(600);Need(Read(presentation,"player") as Component==hero,"Additive World Two survey markers resolve the actual local hero");
  await Work(needle);Need(Status(survey)=="ReadyToTurnIn","Both different survey sites complete the unordered objective");await Capture("basalt-needle-survey");
  Warp((Vector3)Read(ivara,"ApproachPosition"));goldBefore=Gold();xpBefore=TotalXp();Need((bool)Call(ivara,"TurnIn",hero,Quest(survey)),"Return both surveys to Ivara");Need(Gold()-goldBefore==220&&TotalXp()-xpBefore==18000,"Landmark survey pays its authored XP and gold");
  Phase("Willowford report and chapter completion");Need((bool)Call(ivara,"Accept",hero,Quest(report)),"Accept An Unfinished Record");
  Warp((Vector3)Read(tamsin,"ApproachPosition"));await System.Threading.Tasks.Task.Delay(550);Need((bool)Read(Part(tamsin,"DiceFree.Quests.QuestWorldMarker"),"IsQuestion"),"Tamsin displays the required-conversation quest marker");
  var interactor=Part(hero,"DiceFree.World.Interactor");Need((bool)Call(interactor,"Order",tamsin),"Start Tamsin's real conversation");await Until(()=>Read(interactor,"Active") as Component==tamsin,"Envoy conversation active");await Capture("willowford-expedition-report");
  goldBefore=Gold();xpBefore=TotalXp();Need((bool)Call(tamsin,"Acknowledge",hero),"Acknowledge Tamsin's field-report conversation");
  Need(Status(report)=="Completed"&&Gold()-goldBefore==150&&TotalXp()-xpBefore==10000,"Acknowledging the envoy completes the chapter and grants the final reward");Need(!(bool)Call(tamsin,"Acknowledge",hero),"Repeated conversation acknowledgement cannot duplicate the reward");
  saved=(Array)Call(journal,"CaptureState");Need((bool)Call(journal,"CanRestore",saved),"Completed chapter state validates for persistence");Call(journal,"RestoreState",saved);Need(new[]{first,survey,report}.All(id=>Status(id)=="Completed"),"All three completed quests survive state restoration");
  var atlas=Part(hero,"DiceFree.UI.WorldAtlasHud");Call(atlas,"Toggle");await Capture("world-two-expedition-atlas",40);Call(atlas,"Toggle");
  Need(errors.Count==0,"No Unity runtime errors during dragon and expedition review");Report("passed");
 }
 catch(Exception error){failure=error.ToString();Report("failed");}
 finally{Time.timeScale=1;Application.logMessageReceived-=Log;}
}
Report("scheduled");Application.runInBackground=true;Application.logMessageReceived+=Log;_ = Review();return "Dragon rewards, normal treasury flow, and complete World Two expedition review scheduled.";
