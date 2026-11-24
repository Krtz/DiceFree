var qa=System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName,"QA");
var report=System.IO.Path.Combine(qa,"psion-tier3-reload.json");
const string combat="DiceFree.Combat.", dungeons="DiceFree.Dungeons.", world="DiceFree.World.";
var errors=new System.Collections.Generic.List<string>();
string phase="startup",failure=null;
int checks=0;
Component player=null,traversal=null;
Type T(string n)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(n)).First(t=>t!=null);
Component Find(string n)=>UnityEngine.Object.FindObjectsByType(T(n)).Cast<Component>().FirstOrDefault();
Component Part(Component c,string n)=>c.GetComponent(T(n));
object Read(object c,string n)=>c?.GetType().GetProperty(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)?.GetValue(c)??c?.GetType().GetField(n,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic)?.GetValue(c);
object Invoke(object c,string n,params object[] a)
{
    var method=c.GetType().GetMethods(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).Where(m=>m.Name==n).First(m=>
    {
        var p=m.GetParameters();
        return p.Length>=a.Length&&p.Take(a.Length).Select((v,i)=>a[i]==null||v.ParameterType.IsInstanceOfType(a[i])).All(v=>v)&&
            p.Skip(a.Length).All(v=>v.IsOptional);
    });
    return method.Invoke(c,method.GetParameters().Select((v,i)=>i<a.Length?a[i]:v.DefaultValue).ToArray());
}
bool Busy()=>(bool)T("DiceFree.ApplicationFlow.LoadingScreen").GetProperty("Busy").GetValue(null);
Component SlimeRun()=>(Component)T(dungeons+"SlimeDungeonRun").GetProperty("Current").GetValue(null);
Component[] Slimes()=>UnityEngine.Object.FindObjectsByType(T(dungeons+"DungeonSlime")).Cast<Component>().ToArray();
Component Slime(string role)=>Slimes().FirstOrDefault(s=>Read(s,"Role").ToString()==role);
void Need(bool good,string label){checks++;if(!good)throw new InvalidOperationException(label);}
void Log(string message,string trace,LogType level){if(level is LogType.Error or LogType.Exception or LogType.Assert)errors.Add(message);}
string Quote(string value)=>value==null?"null":"\""+value.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r"," ").Replace("\n"," ")+"\"";
void Write(string status)=>System.IO.File.WriteAllText(report,"{\"status\":"+Quote(status)+",\"phase\":"+Quote(phase)+
    ",\"checks\":"+checks+",\"error\":"+Quote(failure)+",\"unityErrors\":["+string.Join(",",errors.Select(Quote))+"]}");
void Phase(string next){phase=next;Write("running");}
async System.Threading.Tasks.Task Until(Func<bool> done,string reason,float seconds=26)
{
    var deadline=DateTime.UtcNow.AddSeconds(seconds);
    while(!done())
    {
        if(DateTime.UtcNow>deadline)throw new TimeoutException(reason);
        await System.Threading.Tasks.Task.Delay(25);
    }
}
Vector3 Ground(Vector3 p)
{
    if(!UnityEngine.AI.NavMesh.SamplePosition(p,out var hit,1.2f,1))throw new Exception("No navigation at "+p);
    return hit.position;
}
void Warp(Vector3 p)
{
    Need((bool)Invoke(Read(player,"Motor"),"TeleportAcrossMaps",p),"Warp fixture to "+p);
    Invoke(Find("DiceFree.Core.ExplorationCamera"),"RecenterForTravel",player.transform);
}
async System.Threading.Tasks.Task Walk(Vector3 p,float distance=.5f)
{
    var motor=Read(player,"Motor");
    var target=Ground(p);
    Need((bool)Invoke(traversal,"TryMoveFromMinimap",target,false),"Normal move order "+target);
    int retries=0;
    var next=DateTime.UtcNow.AddSeconds(1);
    await Until(()=>
    {
        if(Vector3.Distance(player.transform.position,target)<distance)return true;
        if(DateTime.UtcNow>=next && !(bool)Read(motor,"Travelling") && retries++<3)
        {
            next=DateTime.UtcNow.AddSeconds(1);
            Invoke(traversal,"TryMoveFromMinimap",target,false);
        }
        return false;
    },"Walked to "+target);
    Invoke(motor,"Stop");
}
void Connected(Vector3 start,Vector3 end)
{
    var path=new UnityEngine.AI.NavMeshPath();
    Need(UnityEngine.AI.NavMesh.CalculatePath(Ground(start),Ground(end),1,path)&&
        path.status==UnityEngine.AI.NavMeshPathStatus.PathComplete,"Path from "+start+" to "+end);
}
async System.Threading.Tasks.Task Capture(string name)
{
    await System.Threading.Tasks.Task.Delay(170);
    ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(qa,name+".png"));
    await System.Threading.Tasks.Task.Delay(210);
}
void HealthFlag(bool value)=>Read(player,"Health").GetType().GetProperty("Invulnerable").SetValue(Read(player,"Health"),value);
void Hit(Component target,float amount)
{
    var type=T(combat+"DamageResult");
    object packet=Activator.CreateInstance(type);
    type.GetField("raw").SetValue(packet,amount);
    type.GetField("mitigated").SetValue(packet,amount);
    Invoke(Read(target,"Health"),"ApplyPacket",player,packet);
}
async System.Threading.Tasks.Task Use(Component target,Func<bool> finished)
{
    var interactor=Part(player,world+"Interactor");
    Need((bool)Invoke(interactor,"Order",target),"Order normal interaction with "+target.name);
    await Until(finished,"Interacted with target");
}


const string skills="DiceFree.Skills.",adv="DiceFree.Advancement.",quests="DiceFree.Quests.";
Component caster=null,journal=null,resources=null,controller=null,persistence=null;
void Set(object o,string name,object value)=>o.GetType().GetField(name,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic).SetValue(o,value);
object Static(string type,string name)=>T(type).GetProperty(name).GetValue(null);
Component[] All(string type)=>UnityEngine.Object.FindObjectsByType(T(type)).Cast<Component>().ToArray();
Component Teacher(bool knight)=>All(adv+"PsionEvolutionMentor").Single(c=>(string)Read(c,"TeacherId")=="teacher.psion."+(knight?"psionic-knight":"mirage-mage"));
string Status(string id)=>Read(Invoke(journal,"GetProgress",id),"status")?.ToString();
void FullMana()=>Invoke(resources,"Restore","resource.mana",100000f);
void ClearCooldowns()=>Array.Clear((Array)Read(caster,"cooldowns"),0,5);
async System.Threading.Tasks.Task Check()
{
 try
 {
  Phase("Reload saved Echo in a fresh process");Component menu=null;
  await Until(()=>(menu=Find(adv+"StartMenuController"))!=null&&!Busy()&&(bool)Read(menu,"HasLoadedProfile"),"Saved menu ready",45);
  var entries=((System.Collections.IEnumerable)Read(menu,"Entries")).Cast<object>().ToArray();
  int parentLevel=(int)Read(entries.Single(e=>(string)Read(e,"classId")=="class.psion"),"level");var profile=Read(menu,"profile");var manifests=((System.Collections.IEnumerable)Invoke(menu,"ManifestationEntries",profile)).Cast<object>().Select(row=>Read(row,"Item1")).ToArray();
  var stage=Part(menu,adv+"MenuPortraitStage");var catalog=Resources.Load("MenuPresentationCatalog");
  foreach(var id in new[]{"class.psionic-knight","class.mirage-mage"})
  {
   var entry=entries.Single(e=>(string)Read(e,"classId")==id);Need((bool)Read(entry,"available")&&(int)Read(entry,"tier")==3,"Saved Tier III entry "+id);Need((int)Read(entry,"level")==150,"Saved class level150 "+id);
   var state=manifests.Single(m=>(string)Read(m,"classId")==id);var texture=(RenderTexture)Invoke(stage,"Get",id,state,catalog);Need(texture!=null,"Equipped menu preview "+id);
  }
  await System.Threading.Tasks.Task.Delay(400);
  foreach(var portrait in ((System.Collections.IEnumerable)Read(stage,"Portraits")).Cast<object>())
  {
   Need((bool)Read(portrait,"rendered"),"Menu preview rendered");var texture=(RenderTexture)Read(portrait,"texture");var previous=RenderTexture.active;RenderTexture.active=texture;var image=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);image.Apply();RenderTexture.active=previous;System.IO.File.WriteAllBytes(System.IO.Path.Combine(qa,(string)Read(portrait,"classId")+"-menu.png"),image.EncodeToPNG());UnityEngine.Object.Destroy(image);
  }
  Need((bool)Invoke(menu,"TryPlayManifestation","class.psionic-knight"),"Load saved Knight through menu");
  await Until(()=>(traversal=Find("DiceFree.Characters.TraversalInput"))!=null&&!Busy()&&(persistence=Part(traversal,"DiceFree.Persistence.ManifestationPersistence"))!=null&&(bool)Read(persistence,"Ready"),"Saved Knight ready",45);
  player=Part(traversal,combat+"CombatActor");controller=Part(player,adv+"AdvancementController");caster=Part(player,skills+"PsionEvolutionCaster");resources=Part(player,combat+"ActorResourceController");
  HealthFlag(true);Debug.developerConsoleVisible=false;Invoke(Find(world+"WorldDayNightCycle"),"SetHour",15f,false);
  Need(((string[])Read(Part(player,"DiceFree.Gameplay.CodexProgression"),"Teachers")).Length==2,"Both hidden teacher discoveries survived restart");
  foreach(var id in new[]{"class.psionic-knight","class.mirage-mage"})
  {
   Need((bool)Invoke(controller,"TrySwitch",id),"Switch persisted "+id);Need((int)Read(Read(player,"Stats"),"Level")==150,"Runtime restored level150");
   var progression=Part(player,skills+(id.EndsWith("knight")?"PsionicKnightProgression":"MirageMageProgression"));Need(((Array)Invoke(progression,"CaptureState")).Cast<object>().Sum(r=>(int)Read(r,"rank"))==150,"All 150 ranks survived restart");
   Warp(Ground(new Vector3(40,0,4)));player.transform.rotation=Quaternion.identity;
   var camera=Find("DiceFree.Core.ExplorationCamera");Invoke(camera,"ClearPresentationPose");await Capture(id+"-verified-gameplay");
  }
  Need((bool)Invoke(controller,"TrySwitch","class.psion"),"Parent still playable");Need((int)Read(Read(player,"Stats"),"Level")==parentLevel,"Parent level preserved separately");
  Need(errors.Count==0,"No runtime errors on restart");Phase("complete");Write("passed");
 }
 catch(Exception error){failure=error.ToString();Write("failed");Debug.LogError(error);}
 finally{Application.logMessageReceived-=Log;}
}
Application.logMessageReceived+=Log;_ = Check();return "Persistent class and menu verification scheduled.";



