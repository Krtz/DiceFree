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
void Report(string status)=>System.IO.File.WriteAllText(System.IO.Path.Combine(qa,"anvilreach-review.json"),"{\"status\":"+Q(status)+",\"phase\":"+Q(phase)+",\"checks\":"+checks.Count+",\"passed\":["+string.Join(",",checks.Select(Q))+"],\"error\":"+Q(failure)+",\"unityErrors\":["+string.Join(",",errors.Select(Q))+"]}");
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
Vector3 P(float x,float z)=>new Vector3(64000+x,0,64000+z);
async System.Threading.Tasks.Task Walk(float x,float z){var at=Nav(P(x,z));Need((bool)Call(Read(hero,"Motor"),"MoveTo",at),"Issue ordinary walking order to "+x+", "+z);await Until(()=>Vector3.Distance(hero.transform.position,at)<1.2f,"Walking route completes",45);Need(Vector3.Distance(hero.transform.position,at)<1.2f,"Walk reaches "+x+", "+z);}
async System.Threading.Tasks.Task ReviewIndustry()
{
 try{
 Phase("Start copied save and cross to World Two");Component menu=null;await Until(()=>(menu=All("DiceFree.Advancement.StartMenuController").FirstOrDefault())!=null&&!Busy(),"Menu");Need((bool)Call(menu,"TryPlayManifestation","class.psionic-knight"),"Load isolated saved Knight");await Until(()=>(hero=All("DiceFree.Characters.TraversalInput").FirstOrDefault())!=null&&!Busy()&&(bool)Read(Part(hero,"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"Hero ready");hero=Part(hero,"DiceFree.Combat.CombatActor");journal=Part(hero,"DiceFree.Quests.QuestJournal");inventory=Part(hero,"DiceFree.Items.CarriedInventory");wallet=Part(hero,"DiceFree.Items.GoldWallet");Quiet();
 ((Behaviour)Part(hero,"DiceFree.Characters.CombatInput")).enabled=false;((Behaviour)Part(hero,"DiceFree.UI.WorldHoverFeedback")).enabled=false;
 if(hero.transform.position.x<63000){var pass=All("DiceFree.ApplicationFlow.IronpineSouthPass").Single(p=>p.gameObject.scene.name=="Cornberg");Warp((Vector3)Read(pass,"ApproachPosition"));Call(pass,"Interact",hero);await Until(()=>hero.transform.position.x>63000&&!Busy(),"Real World Two transition",100);}Quiet();Need(hero.transform.position.x>63000&&!Busy(),"Existing rotating world crossing reaches expanded World Two");
 var world=All("DiceFree.World.WorldNavigation").Single(c=>c.gameObject.scene.name=="WorldTwoArrival");var city=world.transform.Find("Anvilreach and the southern biomes/Anvilreach - city of the hearth");Need(city!=null,"Saved Anvilreach city is present");Need(city.Cast<Transform>().Count(t=>t.name.StartsWith("Anvilreach courtyard workshop"))==10,"Ten additional courtyard workshops bring the city to 26 buildings");Need(city.Find("Miners Guildhall")!=null&&city.Find("Copper Kettle inn")!=null&&city.Find("Exchange and counting house")!=null&&city.Find("Anvilreach foundry hall")!=null,"Guildhall, inn, exchange and foundry are authored");
 Need(city.Cast<Transform>().Count(t=>t.name.StartsWith("Working blast furnace"))==3&&city.GetComponentsInChildren<ParticleSystem>().Length==3,"Three furnaces have smoke systems");Need(city.Cast<Transform>().Count(t=>t.name=="Anvilreach open gate")==3,"Three city gates remain open");
 var region=T("DiceFree.World.AshfallReach").GetMethod("RegionName");foreach(var e in new[]{(92f,-205f,"Anvilreach"),(105f,-120f,"Coppervein Highlands"),(126f,-520f,"Glasswind Barrens"),(-111f,-444f,"Deepmoss Fen"),(5f,-47f,"Ashfall Reach"),(-150f,-62f,"Steamroot Vale")})Need((string)region.Invoke(null,new object[]{P(e.Item1,e.Item2)})==e.Item3,"Minimap region identity: "+e.Item3);
 var names=(string[])T("DiceFree.UI.WorldAtlasHud").GetField("AshfallNames",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).GetValue(null);Need(names.Length==9&&names.Contains("Anvilreach")&&names.Contains("Deepmoss Fen"),"Atlas lists the city and all three new biomes");
 var road=All("DiceFree.World.TravelSurface").Single(t=>((string)Read(t,"StableId")).EndsWith("Anvilreach foundry avenue"));Need((bool)Call(road,"Contains",Nav(P(92,-205))),"City avenue has the ordinary road movement surface");
 Phase("City routes and services");Warp(P(23,-205));Call(All("DiceFree.World.WorldDayNightCycle").Single(),"SetHour",15f,false);await Walk(42,-205);await Walk(92,-205);await Capture("anvilreach-market",34);await Walk(92,-244);await Walk(92,-290);Warp(P(92,-154));await Walk(92,-128);Need(Physics.Linecast(Nav(P(24,-250))+Vector3.up*2,Nav(P(36,-250))+Vector3.up*2,1<<9),"Masonry wall has a physical blocking collider");Need(All("DiceFree.Combat.CombatActor").Where(a=>a.GetComponent(T("DiceFree.AI.AggroBehaviour"))!=null).All(a=>!new Rect(64030,63718,140,146).Contains(new Vector2(a.transform.position.x,a.transform.position.z))),"Enemy spawns are outside the city walls");
 foreach(var target in city.GetComponentsInChildren(T("DiceFree.World.InteractionTarget")).Cast<Component>()){var path=new UnityEngine.AI.NavMeshPath();Need(UnityEngine.AI.NavMesh.CalculatePath(Nav(P(92,-205)),Nav((Vector3)Read(target,"ApproachPosition")),1,path)&&path.status==UnityEngine.AI.NavMeshPathStatus.PathComplete,"Reachable service approach: "+target.name);}
 var shop=All("DiceFree.Items.ItemShop").Single(s=>s.name=="Master smith Branna");Warp((Vector3)Read(shop,"ApproachPosition"));Need((bool)Call(shop,"Available",hero),"Smiths cooperative is available through normal shop interaction");var offer=((Array)Read(shop,"Offers")).GetValue(0);long price=(int)Read(offer,"goldPrice");Call(wallet,"Grant",price);long gold=Gold();int owned=((Array)Read(inventory,"Items")).Length;Need((bool)Call(shop,"Purchase",hero,0,null)&&Gold()==gold-price&&((Array)Read(inventory,"Items")).Length==owned+1,"Normal merchant purchase grants one item and debits its price");
 var banker=All("DiceFree.Banking.BankInteraction").Single(b=>b.name=="Anvilreach Banker");Warp((Vector3)Read(banker,"ApproachPosition"));Need((bool)Call(banker,"Available",hero),"Shared banker is available");Call(banker,"Interact",hero);Need((bool)Read(Part(hero,"DiceFree.Banking.BankPanel"),"open"),"Bank interaction opens the shared bank panel");Set(Part(hero,"DiceFree.Banking.BankPanel"),"open",false);
 var spring=All("DiceFree.World.HealingArea").Single(s=>s.name=="Anvilreach healing spring");Need((float)Read(spring,"HpPerSecond")==125,"City spring has the World Two healing rate");Warp(P(80,-211));var health=Read(hero,"Health");Set(health,"Current",(float)Read(health,"Maximum")*.5f);float before=(float)Read(health,"Current");await System.Threading.Tasks.Task.Delay(1000);Need((float)Read(health,"Current")>before+60,"Spring actually heals the damaged hero");
 var anchor=All("DiceFree.World.ResurrectionAnchor").Single(a=>(string)Read(a,"StableId")=="anchor.anvilreach");Warp((Vector3)Read(anchor,"ActivationPosition"));await System.Threading.Tasks.Task.Delay(800);Need((string)Read(Part(hero,"DiceFree.World.RespawnAtAnchor"),"AnchorId")=="anchor.anvilreach","Approaching the city dice activates its resurrection anchor");
 Phase("Foundry lighting and visual review");var atmosphere=All("DiceFree.World.FoundryAtmosphere").Single();var lamps=((Array)Read(atmosphere,"streetLights")).Cast<Light>().ToArray();var fires=((Array)Read(atmosphere,"hearthLights")).Cast<Light>().ToArray();var clock=All("DiceFree.World.WorldDayNightCycle").Single();Call(clock,"SetHour",15f,false);await System.Threading.Tasks.Task.Delay(200);Need(lamps.Length>=12&&lamps.All(l=>!l.enabled),"City street lights are off during daytime");Warp(P(119,-211));await Capture("anvilreach-foundry",30);Call(clock,"SetHour",22f,false);await System.Threading.Tasks.Task.Delay(250);Need(lamps.All(l=>l.enabled)&&fires.All(l=>l.intensity>2),"Night lights turn on while the foundry keeps its firelight");await Capture("anvilreach-night",30);Call(clock,"SetHour",15f,false);
 foreach(var point in new[]{(48f,-119f,"coppervein-highlands"),(23f,-460f,"glasswind-barrens"),(-145f,-387f,"deepmoss-fen")}){Warp(P(point.Item1,point.Item2));await Capture(point.Item3,40);}
 Need(new[]{"quest.world2.first-foothold","quest.world2.two-watchfires","quest.world2.unfinished-record"}.All(id=>Quest(id)!=null),"Existing expedition quest definitions remain registered");Need(All("DiceFree.World.TimedInteractionTarget").Count(c=>(string)Read(c,"ContentId") is "work.world2.needle-survey" or "work.world2.crown-survey")==2,"Both pip survey sites remain authored");Need(errors.Count==0,"No Unity runtime errors during city review");Report("passed");
 }catch(Exception e){failure=e.ToString();Report("failed");}finally{Application.logMessageReceived-=Log;}
}
Report("scheduled");Application.runInBackground=true;Application.logMessageReceived+=Log;_ = ReviewIndustry();return "Anvilreach gameplay and biome review scheduled.";