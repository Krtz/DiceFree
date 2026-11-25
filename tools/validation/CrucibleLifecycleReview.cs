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
void Report(string status)=>System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"crucible-lifecycle.json"),"{\"status\":"+Q(status)+",\"phase\":"+Q(phase)+",\"checks\":"+checks.Count+",\"passed\":["+string.Join(",",checks.Select(Q))+"],\"error\":"+Q(failure)+",\"unityErrors\":["+string.Join(",",errors.Select(Q))+"]}");
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
async System.Threading.Tasks.Task Lifecycle()
{
 try{
 Component menu=null;await Until(()=>(menu=All("DiceFree.Advancement.StartMenuController").FirstOrDefault())!=null&&!Busy(),"Menu ready");Need((bool)Call(menu,"TryPlayManifestation","class.psionic-knight"),"Reload copied manifestation from disk after full gauntlet review");await Until(()=>(hero=All("DiceFree.Characters.TraversalInput").FirstOrDefault())!=null&&!Busy()&&(bool)Read(Part(hero,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Hero ready");hero=Part(hero,"DiceFree.Combat.CombatActor");inventory=Part(hero,"DiceFree.Items.CarriedInventory");Quiet();((Behaviour)Part(hero,"DiceFree.Characters.CombatInput")).enabled=false;
 var defs=Resources.LoadAll("DiceFree/DungeonRewards/CrimsonCrucible",T("DiceFree.Items.ItemDefinition"));var items=((Array)Read(inventory,"Items")).Cast<object>().ToArray();Need(defs.Length==17&&defs.All(d=>items.Any(i=>(string)Read(i,"definitionId")== (string)Read(d,"stableId"))),"All seventeen owned rewards survived actual disk reload");Need(defs.All(d=>((Array)Read(d,"allowedClassIds")).Length==0),"All four role sets remain usable by every class");
 var portal=All("DiceFree.Dungeons.CrimsonCruciblePortal").Single(p=>p.gameObject.scene.name=="Cornberg");Warp((Vector3)Read(portal,"ApproachPosition"));Call(portal,"Interact",hero);object run=null;await Until(()=>(run=Static("DiceFree.Dungeons.CrimsonCrucibleRun","Current"))!=null&&(bool)Read(run,"StagingReady"),"Wait room");Need((bool)Call(run,"StartNow",hero),"Delivered build starts the real gauntlet");Component arena=null;await Until(()=>(arena=All("DiceFree.Dungeons.CrucibleArena").FirstOrDefault())!=null&&!Busy()&&(bool)Read(((Array)Read(arena,"bosses")).GetValue(0),"Armed"),"Arena ready");Quiet();var boss=(Component)((Array)Read(arena,"bosses")).GetValue(0);var opponent=(Component)Read(boss,"Actor");((Behaviour)Part(opponent,"DiceFree.Combat.BasicAttack")).enabled=false;Warp(opponent.transform.position+Vector3.back*5);Call(boss,"BeginCast",hero);Need((float)Read(boss,"CastRemaining")>0,"Boss is winding up before stun");Need((bool)Call(Read(opponent,"Effects"),"TryApplyStun",2f,true),"Ordinary combat status applies stun to the boss");await System.Threading.Tasks.Task.Delay(150);Need((float)Read(boss,"CastRemaining")==0,"Real stun cancels the special without a delayed impact");
 int count=((Array)Read(inventory,"Items")).Length;Set(Read(hero,"Health"),"Invulnerable",false);var packet=Activator.CreateInstance(T("DiceFree.Combat.DamageResult"));Set(packet,"raw",1000000f);Set(packet,"mitigated",1000000f);Call(Read(hero,"Health"),"ApplyPacket",opponent,packet);await Until(()=>Read(run,"Phase").ToString()=="Defeated","Wipe phase");Need(((Array)Read(arena,"bosses")).Cast<Component>().All(b=>!b.gameObject.activeSelf),"Wipe stops every encounter");Need(((Array)Read(arena,"gates")).Cast<object>().All(g=>!(bool)Read(g,"Open")),"Wiping before a kill opens no gates");Need(((Array)Read(inventory,"Items")).Length==count,"Wipe grants no equipment");await System.Threading.Tasks.Task.Delay(1700);var handler=Read(Part(hero,"DiceFree.Foundation.RunLoadoutLock"),"DeathReturnHandler");Need((bool)Call(handler,"Invoke"),"Dungeon death-return handler returns the player");await Until(()=>Static("DiceFree.Dungeons.CrimsonCrucibleRun","Current")==null&&!Busy(),"Wipe cleanup");Need((bool)Read(hero,"Alive")&&Read(Part(hero,"DiceFree.Foundation.RunLoadoutLock"),"Owner")==null,"Death return revives the player and clears the run lock");
 Warp((Vector3)Read(portal,"ApproachPosition"));Call(portal,"Interact",hero);await Until(()=>(run=Static("DiceFree.Dungeons.CrimsonCrucibleRun","Current"))!=null&&(bool)Read(run,"StagingReady"),"Fresh waiting room");Need((bool)Call(run,"StartNow",hero),"A fresh attempt starts after the wipe");await Until(()=>All("DiceFree.Dungeons.CrucibleArena").Length>0&&!Busy(),"Fresh arena");Need((int)Read(run,"Stage")==0,"Fresh attempt begins at the first cell");Need((bool)Call(Part(hero,"DiceFree.Dungeons.ReturnToStartMenu"),"Return"),"Delivered build supports returning to the menu mid-run");await Until(()=>All("DiceFree.Advancement.StartMenuController").Length>0&&!Busy(),"Final menu");Need(Static("DiceFree.Dungeons.CrimsonCrucibleRun","Current")==null&&!UnityEngine.SceneManagement.SceneManager.GetSceneByName("CrimsonCrucible").isLoaded,"Menu return unloads the arena and releases occupancy");Need(errors.Count==0,"No runtime errors in delivered-build lifecycle review");Report("passed");
 }catch(Exception e){failure=e.ToString();Report("failed");}finally{Application.logMessageReceived-=Log;}
}
Report("scheduled");Application.runInBackground=true;Application.logMessageReceived+=Log;_ = Lifecycle();return "Crucible actual reload, stun, wipe and delivered-build cleanup scheduled.";