var qa=System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName,"QA");
var report=System.IO.Path.Combine(qa,"psion-tier3-combat.json");
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
System.Collections.Generic.List<object[]> packets=new();
Component Dummy(Vector3 position)
{
    var root=new GameObject("Tier III controlled target"){layer=10};root.SetActive(false);root.transform.position=Ground(position);
    var definition=UnityEngine.Object.Instantiate((ScriptableObject)Read(Read(player,"Stats"),"Definition"));
    Set(definition,"stableId","fixture.target");Set(definition,"physicalDefense",100f);Set(definition,"magicalDefense",80f);
    var stats=root.AddComponent(T(combat+"ActorStats"));Invoke(stats,"Configure",definition);
    var actor=root.AddComponent(T(combat+"CombatActor"));root.AddComponent(T(combat+"CombatStatusController"));Invoke(actor,"Configure",1,.35f,null);
    root.SetActive(true);Invoke(stats,"SetEncounterMaximumHp",100000f);Invoke(Read(actor,"Health"),"SetEncounterHp",100000f);
    var health=Read(actor,"Health");var ev=health.GetType().GetEvent("PacketResolved");
    var args=ev.EventHandlerType.GetMethod("Invoke").GetParameters().Select(a=>System.Linq.Expressions.Expression.Parameter(a.ParameterType,a.Name)).ToArray();
    var record=System.Linq.Expressions.Expression.NewArrayInit(typeof(object),args.Select(a=>System.Linq.Expressions.Expression.Convert(a,typeof(object))));
    var call=System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Constant(packets),packets.GetType().GetMethod("Add"),record);
    ev.AddEventHandler(health,System.Linq.Expressions.Expression.Lambda(ev.EventHandlerType,call,args).Compile());return actor;
}
Component[] Images()=>((System.Collections.IEnumerable)Read(caster,"Images")).Cast<Component>().Where(i=>i!=null&&(bool)Read(i,"Alive")).ToArray();
float Mana()=>(float)Invoke(resources,"Current","resource.mana");
void Near(float actual,float expected,string label,float tolerance=.05f)=>Need(Mathf.Abs(actual-expected)<tolerance,label+": "+actual+" / "+expected);
async System.Threading.Tasks.Task CastSelf(int slot)
{ClearCooldowns();FullMana();Need((bool)Invoke(caster,"CastSelf",slot),"Cast self slot "+slot);await Until(()=>!(bool)Read(caster,"IsChanneling"),"Self cast committed");}
async System.Threading.Tasks.Task<Component> Image()
{
    ClearCooldowns();FullMana();int before=Images().Length;
    Need((bool)Invoke(caster,"CastImage",Ground(player.transform.position+Vector3.right*3)),"Image cast accepted");
    await Until(()=>Images().Length>before,"Image committed");var image=Images().Last();((Behaviour)image).enabled=false;return image;
}
void Reset()
{Invoke(caster,"ResetForManifestationLoad");Invoke(Part(player,combat+"BasicAttack"),"Cancel");Invoke(Read(player,"Health"),"Restore",1f);FullMana();packets.Clear();}
async System.Threading.Tasks.Task Check()
{
    try
    {
        foreach(var old in UnityEngine.Object.FindObjectsByType<Transform>().Where(t=>t.name=="Tier III controlled target").ToArray())UnityEngine.Object.DestroyImmediate(old.gameObject);
        traversal=Find("DiceFree.Characters.TraversalInput");player=Part(traversal,combat+"CombatActor");
        foreach(var other in All(combat+"CombatActor").Where(a=>a!=player))other.gameObject.SetActive(false);
        controller=Part(player,adv+"AdvancementController");persistence=Part(player,"DiceFree.Persistence.ManifestationPersistence");caster=Part(player,skills+"PsionEvolutionCaster");resources=Part(player,combat+"ActorResourceController");
        ((Behaviour)resources).enabled=false;HealthFlag(true);
        Phase("Knight funded, shielded, insufficient Mana and missed impacts");Need((bool)Invoke(controller,"TrySwitch","class.psionic-knight"),"Knight manifestation resumes");Reset();Warp(Ground(new Vector3(40,0,4)));
        var target=Dummy(player.transform.position+Vector3.forward*2);var basic=Part(player,combat+"BasicAttack");
        await CastSelf(3);int charges=(int)Read(caster,"Charges");float mana=Mana();packets.Clear();Invoke(basic,"ResolveImpact",target);
        Need(packets.Count==2,"Exactly Physical and Magical Void packets");Need(Read(packets[0][1],"channel").ToString()=="Physical"&&Read(packets[1][1],"channel").ToString()=="Magical","Two independently mitigated channels");
        Need(Read(packets[1][1],"critical")==null,"Empowered Void never crits");Near((float)Read(packets[1][1],"raw"),(float)Invoke(caster,"Preview",3,34),"One approved rank34 Void formula");
        Need((float)Read(packets[1][1],"defense")>(float)Read(packets[0][1],"defense")-30,"Void uses Magical Defense");Near(Mana(),mana-72,"One landed strike costs 72 Mana");Need((int)Read(caster,"Charges")==charges-1,"Landed strike spends one charge");
        Invoke(Read(target,"Health"),"AddTemporaryShield","fixture.shield",100000f,20f,null);mana=Mana();charges=(int)Read(caster,"Charges");Invoke(basic,"ResolveImpact",target);Near(Mana(),mana-72,"Absorbed hit still spends Mana");Need((int)Read(caster,"Charges")==charges-1,"Absorbed hit spends charge");
        Invoke(resources,"SetCurrentForValidation","resource.mana",0f);charges=(int)Read(caster,"Charges");packets.Clear();Invoke(basic,"ResolveImpact",target);Need(packets.Count==1,"Insufficient Mana resolves only Physical");Need((int)Read(caster,"Charges")==charges&&Mana()==0,"No partial Mana or charge consumed");
        var attack=Read(basic,"Definition");bool accuracy=(bool)Read(attack,"requiresAccuracy");Set(attack,"requiresAccuracy",true);FullMana();mana=Mana();Invoke(Read(player,"Effects"),"ApplyAccuracyPenalty","fixture.miss",1f,3f);packets.Clear();Need((bool)Invoke(basic,"ResolveImpact",target),"Forced miss resolved");Need(packets.Count==0&&(int)Read(caster,"Charges")==charges,"Miss has no connected packet or charge");Near(Mana(),mana,"Miss spends no Mana");Set(attack,"requiresAccuracy",accuracy);Invoke(Read(player,"Effects"),"ClearTransient");
        Phase("Knight image local empowerment, shields, haste and owner death");await CastSelf(2);Invoke(Read(player,"Effects"),"ApplyAttackSpeedBuff","fixture.attack",40f,2f);Invoke(Read(player,"Effects"),"ApplyMovementSpeedBuff","fixture.move",20f,2f);float shield=(float)Read(caster,"ShieldRemaining");var image=await Image();var actor=(Component)Read(image,"Actor");
        var inherited=(System.Collections.IDictionary)Read(Read(actor,"Stats"),"attackSpeedPercentModifiers");Near((float)inherited["status:image-copy:fixture.attack"],20,"Knight copies temporary attack-speed potency once at 50 percent");var movement=(System.Collections.IDictionary)Read(Read(actor,"Stats"),"movementSpeedPercentModifiers");Near((float)movement["status:image-copy:fixture.move"],10,"Knight copies temporary movement potency once at 50 percent");Invoke(Read(player,"Effects"),"ClearTransient");Near((float)Read(Read(actor,"Health"),"TemporaryShield"),shield*.5f,"Rank25 image gets half-strength temporary personal shield");charges=(int)Read(caster,"Charges");mana=Mana();int local=(int)Read(image,"Charges");Invoke(image,"Hit",target);Need((int)Read(image,"Charges")==local-1&&(int)Read(caster,"Charges")==charges,"Image spends only local charges");Near(Mana(),mana,"Image attack spends no owner Mana");
        Invoke(caster,"GrantImageHaste",25);float until=(float)Read(caster,"hasteExpire");Invoke(caster,"GrantImageHaste",1);Near((float)Read(caster,"hasteExpire"),until,"Weaker haste ignored");Near((float)Read(caster,"HastePercent"),50,"Maximum image haste 50 percent");await System.Threading.Tasks.Task.Delay(100);Invoke(caster,"GrantImageHaste",25);Need((float)Read(caster,"hasteExpire")>until,"Equal haste refreshes");
        var health=Read(player,"Health");health.GetType().GetProperty("Current").SetValue(health,0f);Invoke(caster,"OnDeath");Need((bool)Read(image,"Alive"),"Image survives owner death");Invoke(image,"End");Need((float)Read(caster,"HastePercent")==0,"Image-end haste lost while owner dead");Invoke(health,"Restore",1f);Reset();UnityEngine.Object.Destroy(target.gameObject);
        Phase("Mage invalid casts and interrupted channel");Need((bool)Invoke(controller,"TrySwitch","class.mirage-mage"),"Mage manifestation resumes");Reset();Warp(Ground(new Vector3(40,0,4)));target=Dummy(player.transform.position+Vector3.forward*3);
        Need(!(bool)Invoke(caster,"CastBolt",(object)null),"Invalid target rejected");Near((float)Invoke(caster,"Remaining",0),0,"Invalid target has no cooldown");mana=Mana();Need((bool)Invoke(caster,"CastBolt",target),"Legal bolt begins");Invoke(Read(player,"Effects"),"TryApplyStun",1f,true);await Until(()=>!(bool)Read(caster,"IsChanneling"),"Stun cancels cast");Near(Mana(),mana,"Precommit cancellation spends no Mana");Near((float)Invoke(caster,"Remaining",0),0,"Precommit cancellation spends no cooldown");Invoke(Read(player,"Effects"),"ClearTransient");
        ClearCooldowns();FullMana();Need((bool)Invoke(caster,"CastStorm",Ground(target.transform.position)),"Storm begins");await Until(()=>(int)Read(caster,"wave")==1,"First storm wave");Invoke(Read(player,"Effects"),"TryApplyStun",1f,true);await System.Threading.Tasks.Task.Delay(650);Need((int)Read(caster,"wave")==1&&!(bool)Read(caster,"IsChanneling"),"Interrupt cancels remaining storm waves");Invoke(Read(player,"Effects"),"ClearTransient");
        Phase("Mage target fallback, spell snapshots and no copied CC");Reset();image=await Image();actor=(Component)Read(image,"Actor");var fallback=Dummy(actor.transform.position+Vector3.right*2);FullMana();ClearCooldowns();float expected=(float)Invoke(caster,"Preview",0,34);packets.Clear();Need((bool)Invoke(caster,"CastBolt",target),"Original bolt begins");await Until(()=>packets.Any(p=>p[0] is Component c&&c==player),"Original bolt hit");Hit(target,1000000f);var attribute=Activator.CreateInstance(T(combat+"AttributeValues"));Set(attribute,"intelligence",1000f);Invoke(Read(player,"Stats"),"SetAttributeContribution","fixture.snapshot",attribute);await Until(()=>packets.Any(p=>p[0] is Component c&&c==actor),"Copy retargeted after original enemy death");
        var copied=packets.Last(p=>p[0] is Component c&&c==actor)[1];Near((float)Read(copied,"raw"),expected*.25f,"Mirror applies one 25 percent raw multiplier");Need(!(bool)Read(Read(fallback,"Effects"),"Stunned"),"Copied bolt has no stun");
        Invoke(Read(player,"Stats"),"RemoveAttributeContribution","fixture.snapshot");Phase("Independent copied pools and bounded secondary images");Reset();image=await Image();await CastSelf(3);float original=(float)Read(caster,"ShieldRemaining");await System.Threading.Tasks.Task.Delay(500);Near((float)Read(health,"TemporaryShield"),original*1.25f,"Original and independent 25 percent shared pools");Invoke(image,"End");Need((float)Read(health,"TemporaryShield")>=original*1.249f,"Applied copied shield survives source image death");
        Reset();image=await Image();ClearCooldowns();FullMana();Need((bool)Invoke(caster,"CastImage",Ground(player.transform.position+Vector3.back*3)),"Second original image cast");await Until(()=>Images().Any(i=>(bool)Read(i,"Secondary")),"Secondary image echo");var secondary=Images().Single(i=>(bool)Read(i,"Secondary"));
        Near((float)Read(Read((Component)Read(secondary,"Actor"),"Health"),"Maximum"),(float)Read(Read((Component)Read(image,"Actor"),"Health"),"Maximum")*.5f,"Secondary has half primary maximum HP");Near((float)Read(secondary,"ExplosionRaw"),(float)Invoke(caster,"Preview",1,25),"Secondary keeps full original Void explosion");Need((float)Read(secondary,"Remaining")<=2.5f,"Secondary lifetime is bounded to half");Invoke(image,"End");Need((bool)Read(secondary,"Alive"),"Secondary survives primary destruction");
        Reset();image=await Image();await CastSelf(3);original=(float)Read(caster,"ShieldRemaining");Invoke(image,"End");await System.Threading.Tasks.Task.Delay(500);Near((float)Read(health,"TemporaryShield"),original,"Image death during echo delay cancels pending shield");
        Phase("Speed Presence restoration and persistence");Reset();await System.Threading.Tasks.Task.Delay(250);var mods=(System.Collections.IDictionary)Read(Read(player,"Stats"),"attackSpeedPercentModifiers");Near((float)mods["psion3.speed-presence"],3.2f,"Aura reapplied after same-class reset");
        Need((bool)Invoke(persistence,"Flush"),"Both class saves flushed");Need(errors.Count==0,"No Unity errors");Phase("complete");Write("passed");
    }
    catch(Exception error){failure=error.ToString();Write("failed");Debug.LogError(error);}
    finally{if(player!=null)Invoke(Read(player,"Stats"),"RemoveAttributeContribution","fixture.snapshot");if(resources is Behaviour b)b.enabled=true;Application.logMessageReceived-=Log;}
}
Application.logMessageReceived+=Log;_ = Check();return "Detailed Tier III combat verification scheduled.";




