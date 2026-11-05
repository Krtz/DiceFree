// Pipeline eval_file against a fresh QA Player. Reflection keeps the test independent of Player compiler references.
var buildRoot=System.IO.Directory.GetParent(Application.dataPath).FullName;
var output=System.IO.Path.Combine(buildRoot,"QA");var report=System.IO.Path.Combine(output,"wolf-survival-smoke.json");
Application.runInBackground=true;var observedErrors=new System.Collections.Generic.List<string>();
void OnLog(string message,string stack,LogType kind){if(kind==LogType.Error||kind==LogType.Exception||kind==LogType.Assert)observedErrors.Add(message);}Application.logMessageReceived+=OnLog;
void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
Type T(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
object Get(object o,string name){if(o==null)throw new InvalidOperationException("Missing object reading "+name);var t=o.GetType();var p=t.GetProperty(name);return p!=null?p.GetValue(o):t.GetField(name).GetValue(o);}
void Set(object o,string name,object value){var t=o.GetType();var p=t.GetProperty(name);if(p!=null)p.SetValue(o,value);else t.GetField(name).SetValue(o,value);}
object Call(object o,string name,params object[] args)
{
 var type=o is Type?(Type)o:o.GetType();var m=type.GetMethods().Where(v=>v.Name==name).First(v=>{var p=v.GetParameters();return p.Length>=args.Length&&p.Take(args.Length).Select((a,i)=>args[i]==null||a.ParameterType.IsInstanceOfType(args[i])).All(b=>b)&&p.Skip(args.Length).All(a=>a.IsOptional);});
 return m.Invoke(o is Type?null:o,m.GetParameters().Select((p,i)=>i<args.Length?args[i]:p.DefaultValue).ToArray());
}
Component[] FindAll(string name)=>UnityEngine.Object.FindObjectsByType(T(name),FindObjectsSortMode.None).Cast<Component>().ToArray();
Component Find(string name)=>FindAll(name).FirstOrDefault();Component Part(Component o,string name)=>o.GetComponent(T(name));
bool Busy()=>(bool)T("DiceFree.ApplicationFlow.LoadingScreen").GetProperty("Busy").GetValue(null);
object Packet(float amount){var p=Activator.CreateInstance(T("DiceFree.Combat.DamageResult"));Set(p,"mitigated",amount);return p;}
void Phase(string p)=>System.IO.File.WriteAllText(report,"{\"status\":\"running\",\"phase\":\""+p+"\"}");
async System.Threading.Tasks.Task Until(Func<bool> f,string text,int seconds=60){var end=DateTime.UtcNow.AddSeconds(seconds);while(!f()){Need(DateTime.UtcNow<end,"Timed out: "+text);await System.Threading.Tasks.Task.Delay(25);}}
async System.Threading.Tasks.Task Capture(string name,int delay=180){await System.Threading.Tasks.Task.Delay(delay);ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(output,name+".png"));await System.Threading.Tasks.Task.Delay(120);}
async System.Threading.Tasks.Task<Component> Player(){Component input=null;await Until(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Cornberg"&&!Busy()&&(input=Find("DiceFree.Characters.TraversalInput"))!=null&&(bool)Get(Part(input,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Overworld player");return Part(input,"DiceFree.Combat.CombatActor");}
async System.Threading.Tasks.Task<Component> Menu(){await Until(()=>Find("DiceFree.Advancement.StartMenuController")!=null&&!Busy(),"Start menu");return Find("DiceFree.Advancement.StartMenuController");}
void Move(Component actor,Vector3 point){Need(UnityEngine.AI.NavMesh.SamplePosition(point,out var hit,2,-1)&&(bool)Call(Get(actor,"Motor"),"TeleportAcrossMaps",hit.position),"Fixture travel "+point);Physics.SyncTransforms();Call(Find("DiceFree.Core.ExplorationCamera"),"RecenterForTravel",actor.transform);}
long TotalXp(Component actor){var xp=Part(actor,"DiceFree.Progression.ExperienceProgression");long sum=(int)Get(xp,"CurrentXp");for(int rank=1;rank<(int)Get(xp,"Level");rank++)sum+=30+3L*(rank-1)*(rank-1);return sum;}
Component Run()=>Find("DiceFree.Dungeons.WolfDungeonRun");
Component[] Wolves(Component run)=>((System.Collections.IEnumerable)Get(run,"Wolves")).Cast<Component>().ToArray();
async System.Threading.Tasks.Task<Component> Enter(Component hero)
{
 var portal=Find("DiceFree.Dungeons.WolfDungeonPortal");Move(hero,(Vector3)Get(portal,"ApproachPosition"));Need((bool)Call(portal,"CanInteract",hero),"Built physical entrance reachable");Call(portal,"Interact",hero);Need(Busy(),"Built additive loading overlay");await Capture("wolf-loading",0);await Until(()=>Run()!=null&&(bool)Get(Run(),"StagingReady")&&!Busy(),"Built wolf staging");return Run();
}
async System.Threading.Tasks.Task Walk(Component hero,Vector3 point){Need((bool)Call(Get(hero,"Motor"),"MoveTo",point),"Built arena walk order");await Until(()=>Vector3.Distance(hero.transform.position,point)<.8f,"Built arena walk");Call(Get(hero,"Motor"),"Stop");}
async System.Threading.Tasks.Task RunSmoke()
{
 try
 {
  var root=(string)Call(T("DiceFree.Persistence.EchoSaveLocation"),"ResolveRoot");Need(System.IO.Path.GetFullPath(root)==System.IO.Path.GetFullPath(System.IO.Path.Combine(output,"Profile")),"Isolated QA/Profile required");
  Phase("new echo and physical entrance");var menu=await Menu();Call(T("DiceFree.World.GraphicsQualityRuntime"),"Select",Enum.Parse(T("DiceFree.World.VisualQuality"),"High"),true);Need((bool)Get(menu,"ReadyForNewEcho")&&(bool)Call(menu,"TryStartNewEcho","Wolf Survival QA"),"Fresh profile");var actor=await Player();
  ((Behaviour)Part(actor,"DiceFree.Characters.TraversalInput")).enabled=false;((Behaviour)Part(actor,"DiceFree.Characters.CombatInput")).enabled=false;Set(Get(actor,"Health"),"Invulnerable",true);
  var clock=Find("DiceFree.World.WorldDayNightCycle");Call(clock,"SetHour",11f,false);var priorLights=UnityEngine.Object.FindObjectsByType<Light>().Where(l=>l.type==LightType.Directional&&(l.cullingMask&(1<<8))!=0).ToDictionary(l=>l,l=>l.enabled);
  var portal=Find("DiceFree.Dungeons.WolfDungeonPortal");Move(actor,(Vector3)Get(portal,"ApproachPosition"));await Capture("wolf-ironpine-entrance");var run=await Enter(actor);Need(!((Behaviour)clock).enabled,"Built arena lighting ownership");
  var dummy=GameObject.Find("Practice target — no rewards");Need(dummy!=null&&dummy.GetComponent(T("DiceFree.Ironpine.IronpineWildlife"))==null&&(bool)Get(Part(dummy.transform,"DiceFree.Combat.Health"),"Invulnerable"),"Built real practice target");
  Need((bool)Get(Part(actor,"DiceFree.Foundation.RunLoadoutLock"),"InsideDungeon")&&!(bool)Get(Part(actor,"DiceFree.Foundation.RunLoadoutLock"),"Locked"),"Built staging lock state");
  var inventory=Part(actor,"DiceFree.Items.CarriedInventory");var equipment=Part(actor,"DiceFree.Items.Equipment");var slotType=T("DiceFree.Items.EquipmentSlot");var one=Call(inventory,"Grant","item.plain.mainhand");Need((bool)Call(equipment,"Equip",Get(one,"instanceId"),Enum.Parse(slotType,"MainHand")),"Staging permits gear preparation");
  await Capture("wolf-moonlit-staging");var origin=new Vector3(40000,0,40000);await Walk(actor,origin+new Vector3(-20,0,7));await Walk(actor,origin+new Vector3(0,0,-8));
  Need(!(bool)Call(run,"StartNow",(object)null)&&(bool)Call(run,"StartNow",actor)&&!(bool)Call(run,"StartNow",actor),"Built initiator start once");var off=Call(inventory,"Grant","item.plain.offhand");Need(!(bool)Call(equipment,"Equip",Get(off,"instanceId"),Enum.Parse(slotType,"OffHand")),"Active run must lock equipment");
  long before=TotalXp(actor),expectedXp=0;int totalPoints=0,totalWolves=0;bool naturalBite=false,alphaSpecial=false;
  Phase("ten waves exact budgets and natural combat");
  for(int wave=1;wave<=10;wave++)
  {
   await Until(()=>Run()==run&&(bool)Get(run,"Fighting")&&(int)Get(run,"Wave")==wave,"Built wave "+wave,30);await System.Threading.Tasks.Task.Delay(160);var pack=Wolves(run);int points=pack.Sum(w=>Convert.ToInt32(Get(w,"Rank")));Need(points==wave+2&&pack.Length==(int)Get(run,"Remaining"),"Built exact pack budget "+wave);totalPoints+=points;totalWolves+=pack.Length;
   Need(pack.All(w=>Part(w,"DiceFree.Items.FixedWorldDrop")==null&&((string)Get(Get(Get(Get(w,"Actor"),"Stats"),"Definition"),"stableId")).StartsWith("enemy.wolf-survival.")),"Built dungeon wolves have no overworld drops or world-boss identity");
   if(wave==1)
   {
    Need(pack.All(w=>Get(w,"Rank").ToString()!="Alpha")&&(pack.Length==2||pack.Length==3),"Built first wave choices");Call(Get(actor,"Stats"),"SetEncounterMaximumHp",1000000f);Call(Get(actor,"Health"),"Restore");Set(Get(actor,"Health"),"Invulnerable",false);float initial=(float)Get(Get(actor,"Health"),"Current");await Until(()=>(float)Get(Get(actor,"Health"),"Current")<initial,"Natural wolf bite",20);naturalBite=true;Set(Get(actor,"Health"),"Invulnerable",true);await Capture("wolf-wave-1-combat");
   }
   if(wave==10)
   {
    Need(pack.Length==4&&pack.All(w=>Get(w,"Rank").ToString()=="Alpha"),"Built four-alpha finale");
    await Until(()=>pack.Any(w=>{var wildlife=Part(w,"DiceFree.Ironpine.IronpineWildlife");var t=wildlife.GetType();return (bool)t.GetField("howling",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(wildlife)||(bool)t.GetField("pouncing",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(wildlife);}),"Natural alpha howl/pounce",18);alphaSpecial=true;await Capture("wolf-wave-10-four-alphas");
   }
   expectedXp+=pack.Sum(w=>(int)Get(Get(Get(Get(w,"Actor"),"Stats"),"Definition"),"experienceReward"));
   foreach(var wolf in pack)Call(Get(Get(wolf,"Actor"),"Health"),"ApplyPacket",actor,Packet(1000000));
   await Until(()=>Get(run,"Phase").ToString()=="Intermission"||Get(run,"Phase").ToString()=="Victory","Built wave clear");Need((int)Get(run,"WavesCleared")==wave&&(int)Get(run,"Remaining")==0,"Last wolf gates next wave");
  }
  Need(totalPoints==75&&TotalXp(actor)-before==expectedXp,"75 points and kill XP exactly once");Need(Get(run,"Phase").ToString()=="Victory","Ten-wave victory");await Capture("wolf-victory");Call(run,"Abandon");await Until(()=>Run()==null&&!UnityEngine.SceneManagement.SceneManager.GetSceneByName("WolfSurvival").isLoaded,"Built victory exit");Need(!(bool)Get(Part(actor,"DiceFree.Foundation.RunLoadoutLock"),"InsideDungeon")&&((Behaviour)clock).enabled&&priorLights.All(p=>p.Key!=null&&p.Key.enabled==p.Value),"Built locks and lighting restored");
  Phase("wipe and staging retry");run=await Enter(actor);Need((bool)Call(run,"StartNow",actor),"Built wipe run start");await Until(()=>(bool)Get(run,"Fighting"),"Wipe first wave");Need((int)Get(run,"Wave")==1,"Restart at wave one");Set(Get(actor,"Health"),"Invulnerable",false);Call(Get(actor,"Health"),"ApplyPacket",null,Packet(1000000));await Until(()=>Run()==null,"Built solo wipe");Need((bool)Get(actor,"Alive")&&actor.transform.position.x<1000&&!(bool)Get(Part(actor,"DiceFree.Foundation.RunLoadoutLock"),"InsideDungeon"),"Built anchor return after wipe");Set(Get(actor,"Health"),"Invulnerable",true);
  run=await Enter(actor);Call(run,"Abandon");await Until(()=>Run()==null,"Staging abandon");
  Phase("active menu exit save resume and lease release");run=await Enter(actor);Need((bool)Call(run,"StartNow",actor),"Active exit fixture");Need((bool)Call(Part(actor,"DiceFree.Dungeons.ReturnToStartMenu"),"Return"),"Active menu return");menu=await Menu();Need(Run()==null&&!UnityEngine.SceneManagement.SceneManager.GetSceneByName("WolfSurvival").isLoaded,"Active scene cleanup");var entry=((System.Collections.IEnumerable)Get(menu,"Entries")).Cast<object>().First(e=>(bool)Get(e,"active"));Need((bool)Call(menu,"TryPlayManifestation",Get(entry,"classId")),"Built saved resume");actor=await Player();var resumedLock=Part(actor,"DiceFree.Foundation.RunLoadoutLock");Need(actor.transform.position.x<1000&&(resumedLock==null||!(bool)Get(resumedLock,"InsideDungeon")),"Do not persist dungeon position or lock");
  ((Behaviour)Part(actor,"DiceFree.Characters.TraversalInput")).enabled=false;((Behaviour)Part(actor,"DiceFree.Characters.CombatInput")).enabled=false;Set(Get(actor,"Health"),"Invulnerable",true);run=await Enter(actor);Call(run,"Abandon");await Until(()=>Run()==null,"Lease permits reentry after menu");
  Phase("existing Slime Dungeon regression");var slime=FindAll("DiceFree.Dungeons.DungeonPortal").Single(p=>((Behaviour)p).isActiveAndEnabled);Move(actor,(Vector3)Get(slime,"ApproachPosition")+Vector3.back*1.5f);Need((bool)Call(slime,"CanInteract",actor),"Active Slime cave entrance approach");Call(slime,"Interact",actor);await Until(()=>Find("DiceFree.Dungeons.SlimeDungeonRun")!=null&&(bool)Get(Find("DiceFree.Dungeons.SlimeDungeonRun"),"StagingReady"),"Existing Slime staging");var slimeRun=Find("DiceFree.Dungeons.SlimeDungeonRun");Need((bool)Call(slimeRun,"StartNow",actor),"Existing Slime start");Call(slimeRun,"Abandon");await Until(()=>Find("DiceFree.Dungeons.SlimeDungeonRun")==null&&!UnityEngine.SceneManagement.SceneManager.GetSceneByName("SlimeDungeon").isLoaded,"Existing Slime exit");
  Need((bool)Call(Part(actor,"DiceFree.Dungeons.ReturnToStartMenu"),"Return"),"Final menu return");menu=await Menu();Need(!Busy()&&observedErrors.Count==0,"No overlay or Unity errors: "+string.Join(";",observedErrors.Take(3)));
  System.IO.File.WriteAllText(report,"{\"status\":\"passed\",\"waves\":10,\"totalPoints\":75,\"finalAlphas\":4,\"wolvesDefeated\":"+totalWolves+",\"naturalWolfBite\":"+naturalBite.ToString().ToLowerInvariant()+",\"alphaSpecialObserved\":"+alphaSpecial.ToString().ToLowerInvariant()+",\"killXpOnce\":true,\"wipeReset\":true,\"stagingGearAndActiveLock\":true,\"menuResumeAndReentry\":true,\"lightingRestored\":true,\"slimeDungeonRegression\":true,\"unityErrors\":0,\"quitRequested\":true}");Debug.Log("DICEFREE_WINDOWS_WOLF_SURVIVAL_OK");Need((bool)Call(Find("DiceFree.UI.OptionsPanel"),"RequestQuitToDesktop"),"Normal quit");
 }
 catch(Exception e){System.IO.File.WriteAllText(report,"{\"status\":\"failed\",\"error\":\""+e.ToString().Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r"," ").Replace("\n"," ")+"\"}");Debug.LogError(e);}
 finally{Application.logMessageReceived-=OnLog;}
}
_ = RunSmoke();return "Started isolated Windows wolf acceptance; inspect QA/wolf-survival-smoke.json.";
