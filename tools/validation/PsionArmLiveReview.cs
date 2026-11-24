var qa=System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName,"QA");
var report=System.IO.Path.Combine(qa,"psion-arm-live.json");
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


async System.Threading.Tasks.Task Review()
{
 try{
 traversal=Find("DiceFree.Characters.TraversalInput");player=Part(traversal,combat+"CombatActor");var controller=Part(player,"DiceFree.Advancement.AdvancementController");var camera=Find("DiceFree.Core.ExplorationCamera");HealthFlag(true);Debug.developerConsoleVisible=false;
 foreach(var id in new[]{"class.psionic-knight","class.mirage-mage"}){
 Need((bool)Invoke(controller,"TrySwitch",id),"Switch live hero "+id);await System.Threading.Tasks.Task.Delay(180);Warp(Ground(new Vector3(40,0,4)));player.transform.rotation=Quaternion.identity;
 var root=(Transform)Read(Part(player,"DiceFree.UI.ClassPresentationSwitcher"),"ActiveVisualRoot");Need(root.GetComponentInChildren(T("DiceFree.Visuals.PsionEvolutionHandPose"))!=null,"Retargeting active on live hero");
 var p=player.transform.position+new Vector3(1.5f,2.2f,4.8f);Invoke(camera,"SetPresentationPose",p,Quaternion.LookRotation(player.transform.position+Vector3.up*.9f-p),42f);await Capture(id+"-live-idle");
 Need((bool)Invoke(Read(player,"Motor"),"MoveTo",Ground(player.transform.position+Vector3.right*2)),"Live navigation order");await Until(()=>(bool)Read(Read(player,"Motor"),"Travelling"),"Hero walking",5);await Capture(id+"-live-walking");Invoke(Read(player,"Motor"),"Stop");player.transform.rotation=Quaternion.identity;p=player.transform.position+new Vector3(1.5f,2.2f,4.8f);Invoke(camera,"SetPresentationPose",p,Quaternion.LookRotation(player.transform.position+Vector3.up*.9f-p),42f);
 var caster=Part(player,"DiceFree.Skills.PsionEvolutionCaster");var mana=Part(player,combat+"ActorResourceController");Invoke(mana,"Restore","resource.mana",100000f);Array.Clear((Array)Read(caster,"cooldowns"),0,5);Need((bool)Invoke(caster,"CastSelf",id.EndsWith("knight")?2:3),"Normal hero spell cast");await Capture(id+"-live-casting");await Until(()=>!(bool)Read(caster,"IsChanneling"),"Live cast completed");
 }
 Invoke(camera,"ClearPresentationPose");Need(errors.Count==0,"No live runtime errors");phase="complete";Write("passed");
 }catch(Exception e){failure=e.ToString();Write("failed");Debug.LogWarning(failure);}finally{Application.logMessageReceived-=Log;}
}
Application.logMessageReceived+=Log;_ = Review();return "Live hero walking and casting review scheduled.";
