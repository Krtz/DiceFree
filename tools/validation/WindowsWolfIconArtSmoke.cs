// Pipeline eval_file against a fresh development Player with an isolated QA/Profile.
var buildRoot=System.IO.Directory.GetParent(Application.dataPath).FullName;
var output=System.IO.Path.Combine(buildRoot,"QA");var report=System.IO.Path.Combine(output,"wolf-icon-art-smoke.json");
Application.runInBackground=true;var errors=new System.Collections.Generic.List<string>();
void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}Application.logMessageReceived+=Log;
void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
Type T(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
object Get(object o,string name){var t=o.GetType();var p=t.GetProperty(name);return p!=null?p.GetValue(o):t.GetField(name).GetValue(o);}
void Set(object o,string name,object value){var t=o.GetType();var p=t.GetProperty(name);if(p!=null)p.SetValue(o,value);else t.GetField(name).SetValue(o,value);}
object Call(object o,string name,params object[] args){var t=o is Type?(Type)o:o.GetType();var m=t.GetMethods().Where(v=>v.Name==name).First(v=>{var p=v.GetParameters();return p.Length>=args.Length&&p.Take(args.Length).Select((a,i)=>args[i]==null||a.ParameterType.IsInstanceOfType(args[i])).All(b=>b)&&p.Skip(args.Length).All(a=>a.IsOptional);});return m.Invoke(o is Type?null:o,m.GetParameters().Select((p,i)=>i<args.Length?args[i]:p.DefaultValue).ToArray());}
Component[] All(string name)=>UnityEngine.Object.FindObjectsByType(T(name)).Cast<Component>().ToArray();
Component Find(string name)=>All(name).FirstOrDefault();Component Part(Component c,string name)=>c.GetComponent(T(name));
bool Busy()=>(bool)T("DiceFree.ApplicationFlow.LoadingScreen").GetProperty("Busy").GetValue(null);
object Packet(){var p=Activator.CreateInstance(T("DiceFree.Combat.DamageResult"));Set(p,"mitigated",1000000f);return p;}
long Xp(Component p){var xp=Part(p,"DiceFree.Progression.ExperienceProgression");long sum=(int)Get(xp,"CurrentXp");for(int rank=1;rank<(int)Get(xp,"Level");rank++)sum+=30+3L*(rank-1)*(rank-1);return sum;}
void Phase(string name)=>System.IO.File.WriteAllText(report,"{\"status\":\"running\",\"phase\":\""+name+"\"}");
async System.Threading.Tasks.Task Until(Func<bool> f,string text,int seconds=50){var end=DateTime.UtcNow.AddSeconds(seconds);while(!f()){Need(DateTime.UtcNow<end,"Timeout: "+text);await System.Threading.Tasks.Task.Delay(25);}}
async System.Threading.Tasks.Task Capture(string name){await System.Threading.Tasks.Task.Delay(200);ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(output,name+".png"));await System.Threading.Tasks.Task.Delay(120);}
async System.Threading.Tasks.Task<Component> Menu(){Component menu=null;await Until(()=>(menu=Find("DiceFree.Advancement.StartMenuController"))!=null&&!Busy(),"Menu");return menu;}
async System.Threading.Tasks.Task<Component> Player(){Component input=null;await Until(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Cornberg"&&!Busy()&&(input=Find("DiceFree.Characters.TraversalInput"))!=null&&(bool)Get(Part(input,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Ready overworld");var p=Part(input,"DiceFree.Combat.CombatActor");((Behaviour)input).enabled=false;((Behaviour)Part(p,"DiceFree.Characters.CombatInput")).enabled=false;((Behaviour)Part(p,"DiceFree.Combat.BasicAttack")).enabled=false;Set(Get(p,"Health"),"Invulnerable",true);return p;}
void Move(Component p,Vector3 at){Need(UnityEngine.AI.NavMesh.SamplePosition(at,out var hit,1,-1)&&(bool)Call(Get(p,"Motor"),"TeleportAcrossMaps",hit.position),"Fixture travel "+at);Physics.SyncTransforms();Call(Find("DiceFree.Core.ExplorationCamera"),"RecenterForTravel",p.transform);}
Component Room()=>Find("DiceFree.Dungeons.DungeonRewardRoom");
Component[] Chests(Component p)=>All("DiceFree.Dungeons.DungeonRewardChest").Where(c=>(bool)Call(c,"Available",p)).ToArray();
bool naturalBite=false;
async System.Threading.Tasks.Task<Component> ClearWolf(Component p,bool natural=false)
{
 var portal=Find("DiceFree.Dungeons.WolfDungeonPortal");Move(p,(Vector3)Get(portal,"ApproachPosition"));Need((bool)Call(portal,"CanInteract",p),"Physical wolf entrance");Call(portal,"Interact",p);Component run=null;await Until(()=>(run=Find("DiceFree.Dungeons.WolfDungeonRun"))!=null&&(bool)Get(run,"StagingReady")&&!Busy(),"Wolf staging");Need(!(bool)Call(run,"ClaimRewards",p),"No reward before all ten waves");Need((bool)Call(run,"StartNow",p),"Wolf start");
 for(int wave=1;wave<=10;wave++)
 {
  await Until(()=>(bool)Get(run,"Fighting")&&(int)Get(run,"Wave")==wave,"Wave "+wave);var wolves=((System.Collections.IEnumerable)Get(run,"Wolves")).Cast<Component>().ToArray();Need(wolves.Sum(w=>Convert.ToInt32(Get(w,"Rank")))==wave+2,"Unchanged exact wave budget");if(wave==10)Need(wolves.Length==4&&wolves.All(w=>Get(w,"Rank").ToString()=="Alpha"),"Four final alphas");
  if(natural&&wave==1){Call(Get(p,"Stats"),"SetEncounterMaximumHp",1000000f);Call(Get(p,"Health"),"Restore");Set(Get(p,"Health"),"Invulnerable",false);float hp=(float)Get(Get(p,"Health"),"Current");await Until(()=>(float)Get(Get(p,"Health"),"Current")<hp,"Natural wolf attack",25);Set(Get(p,"Health"),"Invulnerable",true);naturalBite=true;}
  foreach(var wolf in wolves)Call(Get(Get(wolf,"Actor"),"Health"),"ApplyPacket",p,Packet());await Until(()=>Get(run,"Phase").ToString()=="Intermission"||Get(run,"Phase").ToString()=="Victory","Wave clear");if(wave<10)Need((bool)Call(run,"StartNextWave",p),"Advance exact pack");
 }
 Need(!(bool)Call(run,"ClaimRewards",(object)null)&&(bool)Call(run,"ClaimRewards",p)&&!(bool)Call(run,"ClaimRewards",p),"One guarded reward transition");await Until(()=>Get(run,"Phase").ToString()=="Rewards"&&!Busy()&&!UnityEngine.SceneManagement.SceneManager.GetSceneByName("WolfSurvival").isLoaded,"Treasury arrival");Need((bool)Get(Part(p,"DiceFree.Foundation.RunLoadoutLock"),"InsideDungeon")&&!(bool)Get(Part(p,"DiceFree.Foundation.RunLoadoutLock"),"Locked"),"Reward gear preparation and ownership");return run;
}
async System.Threading.Tasks.Task Claim(Component p,Component chest)
{Need((bool)Call(Part(p,"DiceFree.World.Interactor"),"Order",chest),"Interact with numbered physical chest");await Until(()=>!UnityEngine.SceneManagement.SceneManager.GetSceneByName("DungeonRewardRoom").isLoaded&&Find("DiceFree.Dungeons.WolfDungeonRun")==null,"Claim cleanup");Need(p.transform.position.x<1000&&!(bool)Get(Part(p,"DiceFree.Foundation.RunLoadoutLock"),"InsideDungeon"),"Return and release run lock");}
async System.Threading.Tasks.Task<Component> Resume(Component p)
{Need((bool)Call(Part(p,"DiceFree.Dungeons.ReturnToStartMenu"),"Return"),"Menu exit");var menu=await Menu();var entry=((System.Collections.IEnumerable)Get(menu,"Entries")).Cast<object>().First(e=>(bool)Get(e,"active"));Need((bool)Call(menu,"TryPlayManifestation",Get(entry,"classId")),"Saved resume");return await Player();}
async System.Threading.Tasks.Task Execute()
{
 try
 {
  string root=(string)Call(T("DiceFree.Persistence.EchoSaveLocation"),"ResolveRoot");Need(System.IO.Path.GetFullPath(root)==System.IO.Path.GetFullPath(System.IO.Path.Combine(output,"Profile")),"Isolated art QA profile");var menu=await Menu();Need((bool)Call(menu,"TryStartNewEcho","Wolf Art QA"),"Fresh Echo");var p=await Player();
  Phase("six wolf illustrations in inventory");var items=Resources.LoadAll("DiceFree/DungeonRewards/Wolf",T("DiceFree.Items.ItemDefinition"));Need(items.Length==6,"Six existing wolf items");var inventory=Part(p,"DiceFree.Items.CarriedInventory");foreach(var item in items){var icon=(Sprite)Get(item,"icon");Need(icon!=null&&icon.texture.width==512&&icon.texture.height==512,"Existing icon references and imported size");Call(inventory,"Grant",Get(item,"stableId"));}
  var panel=Part(p,"DiceFree.UI.InventoryPanel");Call(panel,"Show");await Capture("wolf-art-inventory");Call(panel,"Close");
  Phase("wolf art in normal reward choices");await ClearWolf(p);var chest=Chests(p).First(c=>Get(c,"OfferedItem")!=null);int before=((Array)Get(inventory,"Items")).Length;await Capture("wolf-art-reward-cards");await Claim(p,chest);Need(((Array)Get(inventory,"Items")).Length==before+1,"Ordinary item reward still grants once");
  p=await Resume(p);inventory=Part(p,"DiceFree.Items.CarriedInventory");foreach(var item in Resources.LoadAll("DiceFree/DungeonRewards/Wolf",T("DiceFree.Items.ItemDefinition"))){Need(Call(inventory,"Resolve",Get(item,"stableId"))!=null&&(Sprite)Get(item,"icon")!=null,"Artwork resolves after saved resume");}Need(((Array)Get(inventory,"Items")).Length==7,"Art QA grants and earned reward persist");Call(Part(p,"DiceFree.UI.InventoryPanel"),"Show");await Capture("wolf-art-saved-inventory");Call(Part(p,"DiceFree.UI.InventoryPanel"),"Close");
  Need((bool)Call(Part(p,"DiceFree.Dungeons.ReturnToStartMenu"),"Return"),"Final menu return");await Menu();Need(errors.Count==0,"Unity errors: "+string.Join(";",errors));System.IO.File.WriteAllText(report,"{\"status\":\"passed\",\"icons\":6,\"importSize\":512,\"inventoryCaptured\":true,\"rewardCardsCaptured\":true,\"normalClaim\":true,\"savedResume\":true,\"unityErrors\":0,\"quitRequested\":true}");Debug.Log("DICEFREE_WOLF_ICON_ART_OK");Need((bool)Call(Find("DiceFree.UI.OptionsPanel"),"RequestQuitToDesktop"),"Normal Quit");
 }
 catch(Exception e){System.IO.File.WriteAllText(report,"{\"status\":\"failed\",\"error\":\""+e.ToString().Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r"," ").Replace("\n"," ")+"\"}");Debug.LogError(e);}
 finally{Application.logMessageReceived-=Log;}
}
_ = Execute();return "Started wolf icon art acceptance; inspect QA/wolf-icon-art-smoke.json.";