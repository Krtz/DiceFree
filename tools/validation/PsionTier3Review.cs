var qa=System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName,"QA");
var report=System.IO.Path.Combine(qa,"psion-tier3-review.json");
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
void CompleteFixtureQuest(string id)
{
    var definitions=((System.Collections.IEnumerable)Read(journal,"Definitions")).Cast<object>();var definition=definitions.Single(d=>(string)Read(d,"stableId")==id);
    var records=(Array)Invoke(journal,"CaptureState");var record=records.Cast<object>().Single(r=>(string)Read(r,"questId")==id);var stages=(Array)Read(definition,"stages");var last=stages.GetValue(stages.Length-1);
    Set(record,"status",Enum.Parse(T(quests+"QuestStatus"),"Completed"));Set(record,"stage",stages.Length-1);Set(record,"count",Read(last,"count"));
    if(Read(last,"kind").ToString() is "All" or "Any")
    {
        var leaves=(Array)Read(last,"alternatives");var counts=Array.CreateInstance(T(quests+"ObjectiveCount"),leaves.Length);
        for(int i=0;i<leaves.Length;i++){var count=Activator.CreateInstance(T(quests+"ObjectiveCount"));Set(count,"objectiveId",Read(leaves.GetValue(i),"objectiveId"));Set(count,"count",Read(leaves.GetValue(i),"count"));counts.SetValue(count,i);}Set(record,"alternatives",counts);
    }
    Invoke(journal,"RestoreState",records);
}
void Publish(object hub,string type,params object[] values)
{
    var t=T("DiceFree.Gameplay."+type);var fact=Activator.CreateInstance(t,values);
    hub.GetType().GetMethod("Publish").MakeGenericMethod(t).Invoke(hub,new[]{fact});
}
async System.Threading.Tasks.Task LoadWorldTwo()
{
    if(!UnityEngine.SceneManagement.SceneManager.GetSceneByName("WorldTwoArrival").isLoaded)
    {var op=UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("WorldTwoArrival",UnityEngine.SceneManagement.LoadSceneMode.Additive);await Until(()=>op.isDone,"World 2 fixture loaded",40);}
    var teacher=Teacher(false);Invoke(Part(player,world+"InteractionRegistry"),"Register",teacher);
}
async System.Threading.Tasks.Task GoTeacher(bool knight)
{
    if(!knight)await LoadWorldTwo();var teacher=Teacher(knight);Warp(Ground(teacher.transform.position+Vector3.right*1.35f));await System.Threading.Tasks.Task.Delay(350);
    await Use(teacher,()=>Read(Part(player,world+"Interactor"),"Active")==teacher);
}
async System.Threading.Tasks.Task WolfTrial()
{
    var portal=Find(dungeons+"WolfDungeonPortal");Warp((Vector3)Read(portal,"ApproachPosition"));Invoke(Part(player,combat+"BasicAttack"),"Cancel");Invoke(portal,"Interact",player);await Until(()=>Static(dungeons+"WolfDungeonRun","Current")!=null,"Wolf entrance accepted");
    var run=(Component)Static(dungeons+"WolfDungeonRun","Current");await Until(()=>(bool)Read(run,"StagingReady")&&!Busy(),"Wolf staging",45);
    Need((bool)Invoke(run,"StartNow",player),"Start real solo wolf run after quest acceptance");float old=Time.timeScale;
    try
    {
        Time.timeScale=8;
        for(int guard=0;guard<180&&Read(run,"Phase").ToString()!="Victory";guard++)
        {
            foreach(var wolf in ((System.Collections.IEnumerable)Read(run,"Wolves")).Cast<object>().ToArray())
            {var actor=(Component)Read(wolf,"Actor");if(actor!=null&&(bool)Read(actor,"Alive"))Hit(actor,100000);}
            if(Read(run,"Phase").ToString()=="Intermission")Invoke(run,"StartNextWave",player);
            await System.Threading.Tasks.Task.Delay(120);
        }
    }finally{Time.timeScale=old;}
    Need((int)Read(run,"WavesCleared")==10,"All ten existing wolf waves completed");
    Need(Status("quest.psionic-knight.advancement")=="ReadyToTurnIn","Fresh solo wolf run grants Knight proof");
    Need((string)Read(persistence,"CurrentClassId")=="class.psion","Dungeon victory does not auto-advance");
    Invoke(run,"StartCoroutine",Invoke(run,"ExitAll",true));await Until(()=>Static(dungeons+"WolfDungeonRun","Current")==null&&!Busy(),"Wolf cleanup",40);
}
async System.Threading.Tasks.Task ShamanTrial()
{
    CompleteFixtureQuest("quest.briarback.four-elements-hunt");
    // Call the real entrance handler; the access quest remains checked by production code.
    var method=T(dungeons+"ShamanDungeonRun").GetMethod("Enter");
    object[] args={player,player.transform.position,Vector3.forward};
    var error=method.Invoke(null,args);Need(error==null,"Ordinary Fourfold Hollow access accepted");
    var run=(Component)Static(dungeons+"ShamanDungeonRun","Current");await Until(()=>(bool)Read(run,"StagingReady")&&!Busy(),"Shaman staging",45);
    Need((bool)Invoke(run,"StartNow",player),"Start real solo shaman run after quest acceptance");
    await Until(()=>(bool)Read(run,"CanBegin")&&!Busy(),"Shaman altar ready",45);Invoke(run,"BeginRitual",player);
    float old=Time.timeScale;
    try
    {
        Time.timeScale=8;
        for(int guard=0;guard<180&&Status("quest.mirage-mage.advancement")!="ReadyToTurnIn";guard++)
        {var enemy=(Component)Read(run,"Opponent");if(enemy!=null&&enemy.gameObject.activeInHierarchy&&(bool)Read(enemy,"Alive"))Hit(enemy,100000);await System.Threading.Tasks.Task.Delay(150);}
    }finally{Time.timeScale=old;}
    Need(Status("quest.mirage-mage.advancement")=="ReadyToTurnIn","Fresh solo Fourfold Hollow run grants Mage proof");
    Invoke(run,"Finish");await Until(()=>Static(dungeons+"ShamanDungeonRun","Current")==null&&!Busy(),"Shaman cleanup",40);
}
void SpendThrough150(Component progression)
{
    var xp=Part(player,"DiceFree.Progression.ExperienceProgression");
    for(int level=1;level<=150;level++)
    {
        Invoke(xp,"RestoreState",level,0);
        while((int)Read(progression,"UnspentPoints")>0)
        {int candidate=Enumerable.Range(0,5).Where(i=>(bool)Invoke(progression,"CanSpend",i)).DefaultIfEmpty(-1).First();Need(candidate>=0,"Legal point at class level "+level);Need((bool)Invoke(progression,"Spend",candidate),"Rank purchase at level "+level);}
    }
    Need(((Array)Invoke(progression,"CaptureState")).Cast<object>().Sum(r=>(int)Read(r,"rank"))==150,"All 150 approved ranks purchased");FullMana();
}
async System.Threading.Tasks.Task Check()
{
    try
    {
        Phase("Load copied Psion save");Component menu=null;
        await Until(()=>(menu=Find(adv+"StartMenuController"))!=null&&!Busy(),"Start menu");Need((bool)Invoke(menu,"TryPlayManifestation","class.psion"),"Load existing Psion fixture");
        await Until(()=>(traversal=Find("DiceFree.Characters.TraversalInput"))!=null&&!Busy()&&(persistence=Part(traversal,"DiceFree.Persistence.ManifestationPersistence"))!=null&&(bool)Read(persistence,"Ready"),"Psion save ready",45);
        player=Part(traversal,combat+"CombatActor");journal=Part(player,quests+"QuestJournal");resources=Part(player,combat+"ActorResourceController");controller=Part(player,adv+"AdvancementController");caster=Part(player,skills+"PsionEvolutionCaster");
        Invoke(Find(world+"WorldDayNightCycle"),"SetHour",15f,false);HealthFlag(true);foreach(var ai in All("DiceFree.AI.AggroBehaviour")){((Behaviour)ai).enabled=false;var basic=Part(ai,combat+"BasicAttack");if(basic!=null)Invoke(basic,"Cancel");}
        Need(caster!=null,"Tier III combat composition installed");
        Phase("Hidden teachers and quest eligibility");await GoTeacher(true);
        Need(!(bool)Invoke(Teacher(true),"AcceptTrial",player),"Under-level Psion cannot accept trial");
        var codex=Part(player,"DiceFree.Gameplay.CodexProgression");Need(((string[])Read(codex,"Teachers")).Contains("teacher.psion.psionic-knight"),"Proximity discovers teacher Echo-wide");
        Invoke(Part(player,"DiceFree.Progression.ExperienceProgression"),"RestoreState",60,0);Need((bool)Invoke(Teacher(true),"AcceptTrial",player),"Eligible Psion accepts Knight trial");
        Invoke(Part(player,world+"Interactor"),"Cancel");await GoTeacher(false);Need((bool)Invoke(Teacher(false),"AcceptTrial",player),"Eligible Psion accepts Mage trial concurrently");
        Need(((string[])Read(codex,"Teachers")).Length==2,"Both teachers remembered");
        Invoke(Part(player,world+"Interactor"),"Cancel");
        var hub=Activator.CreateInstance(T("DiceFree.Gameplay.SemanticGameEventHub"));Invoke(journal,"BindDungeonEvents",hub);
        Publish(hub,"DungeonCompletedEvent","dungeon.ironpine.wolf-survival","ten-waves",1,30f,true);
        Need(Status("quest.psionic-knight.advancement")=="Active","Historical completion without qualifying run start rejected");
        Publish(hub,"DungeonStartedEvent","dungeon.ironpine.wolf-survival",2);Publish(hub,"DungeonCompletedEvent","dungeon.ironpine.wolf-survival","ten-waves",2,30f,true);
        Need(Status("quest.psionic-knight.advancement")=="Active","Grouped clear rejected for solo quest");Invoke(journal,"UnbindDungeonEvents");
        Warp(Ground(new Vector3(112,0,18)));Phase("Real ten-wave solo wolf advancement trial");await WolfTrial();
        Phase("Real solo Fourfold Hollow advancement trial");await ShamanTrial();
        Phase("Knight teacher turn-in and parent preservation");await GoTeacher(true);Need((bool)Invoke(Teacher(true),"CompleteTrial",player),"Normal teacher turn-in creates Knight");
        Need((string)Read(persistence,"CurrentClassId")=="class.psionic-knight"&&(int)Read(Read(player,"Stats"),"Level")==1,"New Knight starts at level one");
        Need((bool)Invoke(persistence,"HasManifestation","class.psion"),"Parent Psion retained");
        var knight=Part(player,skills+"PsionicKnightProgression");SpendThrough150(knight);await System.Threading.Tasks.Task.Delay(300);
        Warp(Ground(new Vector3(15,0,11)));await Capture("psionic-knight-gameplay");
        var visual=Read(Part(player,"DiceFree.UI.ClassPresentationSwitcher"),"ActiveVisualRoot") as Transform;Need(visual!=null&&visual.GetComponentInChildren<Animator>(true).isHuman,"Knight uses a valid animated humanoid");
        Need(Math.Abs((float)Read(Part(player,combat+"BasicAttack"),"Reach")-3)<.01,"Knight basic reach 3m");
        ClearCooldowns();Need((bool)Invoke(caster,"CastSelf",3),"Empowerment starts");await Until(()=>!(bool)Read(caster,"IsChanneling"),"Empowerment committed");Need((int)Read(caster,"Charges")==12,"Rank34 grants exactly twelve finite charges");
        ClearCooldowns();FullMana();Need((bool)Invoke(caster,"CastImage",Ground(player.transform.position+Vector3.right*3)),"Knight image cast");await Until(()=>((System.Collections.IEnumerable)Read(caster,"Images")).Cast<object>().Any(),"Mobile image summoned");
        var image=((System.Collections.IEnumerable)Read(caster,"Images")).Cast<Component>().Last();Need((int)Read(image,"Charges")==12,"Illusion snapshots independent charge pool");Need((bool)Read(image,"Knight"),"Knight image is mobile type");
        await Capture("psionic-knight-image");
        Invoke(caster,"ResetForManifestationLoad");Need((bool)Invoke(persistence,"Flush"),"Knight ranks saved");
        Need((bool)Invoke(controller,"TrySwitch","class.psion"),"Preserved Psion can resume");Need(Status("quest.mirage-mage.advancement")=="ReadyToTurnIn","Other completed trial preserved on parent");
        Phase("Mage teacher turn-in and spell kit");await GoTeacher(false);Need((bool)Invoke(Teacher(false),"CompleteTrial",player),"Normal teacher turn-in creates Mirage Mage");
        Need((string)Read(persistence,"CurrentClassId")=="class.mirage-mage"&&(int)Read(Read(player,"Stats"),"Level")==1,"New Mage starts at level one");
        var mage=Part(player,skills+"MirageMageProgression");SpendThrough150(mage);await System.Threading.Tasks.Task.Delay(250);Warp(Ground(new Vector3(15,0,11)));await Capture("mirage-mage-gameplay");
        Need(Math.Abs((float)Read(Part(player,combat+"BasicAttack"),"Reach")-9)<.01,"Mage basic reach 9m");
        ClearCooldowns();FullMana();Need((bool)Invoke(caster,"CastImage",Ground(player.transform.position+Vector3.right*3)),"Mage primary image cast");await Until(()=>((System.Collections.IEnumerable)Read(caster,"Images")).Cast<object>().Any(),"Primary mirage summoned");
        FullMana();ClearCooldowns();Need((bool)Invoke(caster,"CastSelf",3),"Mage group shield cast");await Until(()=>!(bool)Read(caster,"IsChanneling"),"Original shield committed");float original=(float)Read(caster,"ShieldRemaining");await System.Threading.Tasks.Task.Delay(550);
        Need((float)Read(Read(player,"Health"),"TemporaryShield")>original,"Mirror creates an independent shield pool");
        ClearCooldowns();FullMana();Need((bool)Invoke(caster,"CastImage",Ground(player.transform.position+Vector3.back*3)),"Mental Image mirror source cast");await System.Threading.Tasks.Task.Delay(800);
        Need(((System.Collections.IEnumerable)Read(caster,"Images")).Cast<object>().Any(i=>(bool)Read(i,"Secondary")),"Mirroring produces bounded secondary image");await Capture("mirage-mage-images");
        Invoke(caster,"ResetForManifestationLoad");Need((bool)Invoke(persistence,"Flush"),"Mage ranks and Echo discoveries saved");
        Need(errors.Count==0,"No Unity runtime errors");Phase("complete");Write("passed");
    }
    catch(Exception error){failure=error.ToString();Write("failed");Debug.LogError(error);}
    finally{Application.logMessageReceived-=Log;Time.timeScale=1;}
}
Application.runInBackground=true;Application.logMessageReceived+=Log;_ = Check();return "Tier III Psion verification scheduled.";





