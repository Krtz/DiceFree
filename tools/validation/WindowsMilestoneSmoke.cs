// Run with the QA Player's Pipeline eval_file. Reflection avoids Player-only compiler reference limits.
var buildRoot=System.IO.Directory.GetParent(Application.dataPath).FullName;
var output=System.IO.Path.Combine(buildRoot,"QA");System.IO.Directory.CreateDirectory(output);
var report=System.IO.Path.Combine(output,"player-smoke.json");Application.runInBackground=true;
void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
Type T(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
object Get(object o,string name){var t=o.GetType();var p=t.GetProperty(name);return p!=null?p.GetValue(o):t.GetField(name).GetValue(o);}
void Set(object o,string name,object value){var t=o.GetType();var p=t.GetProperty(name);if(p!=null)p.SetValue(o,value);else t.GetField(name).SetValue(o,value);}
object Call(object o,string name,params object[] args)
{
    var type=o is Type?(Type)o:o.GetType();
    var method=type.GetMethods().Where(m=>m.Name==name).First(m=>{var p=m.GetParameters();return p.Length>=args.Length&&p.Take(args.Length).Select((v,i)=>args[i]==null||v.ParameterType.IsInstanceOfType(args[i])).All(v=>v)&&p.Skip(args.Length).All(v=>v.IsOptional);});
    var values=method.GetParameters().Select((p,i)=>i<args.Length?args[i]:p.DefaultValue).ToArray();return method.Invoke(o is Type?null:o,values);
}
Component[] FindAll(string name)=>UnityEngine.Object.FindObjectsByType(T(name),FindObjectsSortMode.None).Cast<Component>().ToArray();
Component Find(string name)=>FindAll(name).FirstOrDefault();
Component Part(Component owner,string name)=>owner.GetComponent(T(name));
bool Busy()=>(bool)T("DiceFree.ApplicationFlow.LoadingScreen").GetProperty("Busy").GetValue(null);
object Packet(float amount){var p=Activator.CreateInstance(T("DiceFree.Combat.DamageResult"));Set(p,"mitigated",amount);return p;}
void Phase(string phase)=>System.IO.File.WriteAllText(report,"{\"status\":\"running\",\"phase\":\""+phase+"\"}");
async System.Threading.Tasks.Task Until(Func<bool> f,string text){var until=DateTime.UtcNow.AddSeconds(60);while(!f()){Need(DateTime.UtcNow<until,"Timed out: "+text);await System.Threading.Tasks.Task.Delay(25);}}
async System.Threading.Tasks.Task Capture(string name,int delay=150){await System.Threading.Tasks.Task.Delay(delay);ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(output,name+".png"));await System.Threading.Tasks.Task.Delay(100);}
async System.Threading.Tasks.Task<Component> Player(string scene)
{
    await Until(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene().name==scene&&!Busy()&&Find("DiceFree.Characters.TraversalInput")!=null&&(bool)Get(Part(Find("DiceFree.Characters.TraversalInput"),"DiceFree.Persistence.ManifestationPersistence"),"Ready"),"World "+scene);
    return Part(Find("DiceFree.Characters.TraversalInput"),"DiceFree.Combat.CombatActor");
}
async System.Threading.Tasks.Task<Component> Menu(){await Until(()=>Find("DiceFree.Advancement.StartMenuController")!=null&&!Busy(),"Menu");return Find("DiceFree.Advancement.StartMenuController");}
Vector3 Point(Vector3 desired){Need(UnityEngine.AI.NavMesh.SamplePosition(desired,out var hit,2,UnityEngine.AI.NavMesh.AllAreas),"No navigation "+desired);return hit.position;}
void Move(Component actor,Vector3 p){Need((bool)Call(Get(actor,"Motor"),"Teleport",Point(p)),"Travel setup");Physics.SyncTransforms();Call(Find("DiceFree.Core.ExplorationCamera"),"RecenterForTravel",actor.transform);}
async System.Threading.Tasks.Task Run()
{
    try
    {
        var root=(string)Call(T("DiceFree.Persistence.EchoSaveLocation"),"ResolveRoot");
        Need(root!=null&&System.IO.Path.GetFullPath(root)==System.IO.Path.GetFullPath(System.IO.Path.Combine(output,"Profile")),"Use this build's QA/Profile isolated save.");
        Phase("startup and loading");var menu=await Menu();Need((bool)Get(menu,"ReadyForNewEcho"),"QA save must be new");
        var qualityType=T("DiceFree.World.GraphicsQualityRuntime");var presetType=T("DiceFree.World.VisualQuality");
        object Quality(string property)=>qualityType.GetProperty(property).GetValue(null);
        Need((string)Quality("SettingsPath")==System.IO.Path.Combine(output,"Profile","Settings","graphics-quality.json"),"Graphics settings must use the QA profile");
        foreach(var name in new[]{"Low","Medium","High","Ultra"})
        {
            Call(qualityType,"Select",Enum.Parse(presetType,name),true);Call(qualityType,"Reload");Need(Quality("Preset").ToString()==name,"Built preset did not persist: "+name);
            Need(Quality("Pipeline")==QualitySettings.renderPipeline,"Built preset did not apply to the live pipeline");
        }
        Call(qualityType,"Select",Enum.Parse(presetType,"High"),true);
        var graphicsOptions=Find("DiceFree.UI.OptionsPanel");Call(graphicsOptions,"SelectTab",Enum.Parse(T("DiceFree.UI.OptionsPanel+OptionsTab"),"Graphics"));Call(graphicsOptions,"Show");await Capture("graphics-options");Call(graphicsOptions,"Close");
        bool initialized=false;Call(T("DiceFree.ApplicationFlow.LoadingScreen"),"Initialize","Preparing your adventure",(Action)(()=>initialized=true));Need(Busy(),"Initialization overlay missing");await Capture("loading",0);await Until(()=>initialized&&!Busy(),"Initialization cleanup");
        Need((bool)Call(menu,"TryStartNewEcho","Windows milestone"),"New Echo rejected");Need(Busy()&&!(bool)Call(menu,"TryStartNewEcho","Duplicate"),"Duplicate transition accepted");
        var actor=await Player("Cornberg");int firstFrame=Time.frameCount;Set(Get(actor,"Health"),"Invulnerable",true);
        ((Behaviour)Part(actor,"DiceFree.Characters.TraversalInput")).enabled=false;((Behaviour)Part(actor,"DiceFree.Characters.CombatInput")).enabled=false;
        var frozen=new[]{"DiceFree.AI.AggroBehaviour","DiceFree.Combat.BasicAttack","DiceFree.Briarback.QuilboarCombat","DiceFree.Briarback.BriarbackChieftainEncounter"};
        foreach(var b in UnityEngine.Object.FindObjectsByType<MonoBehaviour>().Where(b=>frozen.Contains(b.GetType().FullName)))b.enabled=false;
        foreach(var a in FindAll("DiceFree.Combat.CombatActor")){var motor=Get(a,"Motor");if(motor!=null&&(bool)Get(motor,"Ready"))Call(motor,"Stop");}
        Phase("four elements hunt");var scholar=FindAll("DiceFree.Quests.QuestGiver").Single(g=>g.name=="Trimery Elemental Scholar - Briarback");var quest=Get(scholar,"Quest");var questId=(string)Get(quest,"stableId");var interaction=Part(actor,"DiceFree.World.Interactor");
        Move(actor,(Vector3)Get(scholar,"ApproachPosition"));Need((bool)Call(interaction,"Order",scholar),"Scholar interaction rejected");await Until(()=>Get(interaction,"Active")==scholar,"Scholar");Need((bool)Call(scholar,"Accept",actor,quest),"Quest acceptance rejected");Call(interaction,"Cancel");
        var journal=Part(actor,"DiceFree.Quests.QuestJournal");int stage=0;
        foreach(var element in new[]{"Fire","Earth","Lightning","Ice"})
        {
            Need((int)Get(Call(journal,"GetProgress",questId),"stage")==stage++,"Quest stage order failed");var site=GameObject.Find(element+" Shaman settlement - provisional").transform;Move(actor,site.position+new Vector3(2,0,-7));await Capture(element.ToLowerInvariant());
            var guardian=GameObject.Find("Briarback elemental guardians - additive").GetComponentsInChildren(T("DiceFree.Combat.CombatActor")).Single(g=>g.name.StartsWith(element+"Shaman guardian",StringComparison.Ordinal));Set(Get(guardian,"Health"),"Invulnerable",false);Call(Get(guardian,"Health"),"ApplyPacket",actor,Packet(1000000));Need(!(bool)Get(guardian,"Alive"),"Guardian fixture failed");
        }
        Need(Get(Call(journal,"GetProgress",questId),"status").ToString()=="ReadyToTurnIn","No turn-in state");Move(actor,(Vector3)Get(scholar,"ApproachPosition"));Need((bool)Call(interaction,"Order",scholar),"Scholar return rejected");await Until(()=>Get(interaction,"Active")==scholar,"Turn-in dialogue");Need((bool)Call(scholar,"TurnIn",actor,quest),"Turn-in failed");Call(interaction,"Cancel");
        var elder=FindAll("DiceFree.Quests.QuestGiver").Single(g=>g.name=="Trimery Village Elder - Chieftain's Bane");var elderQuest=Get(elder,"Quest");var elderId=(string)Get(elderQuest,"stableId");Move(actor,(Vector3)Get(elder,"ApproachPosition"));Need((bool)Call(interaction,"Order",elder),"Elder interaction rejected");await Until(()=>Get(interaction,"Active")==elder,"Elder dialogue");await Capture("elder-offer");Need((bool)Call(elder,"Accept",actor,elderQuest),"Chieftain quest rejected");Call(interaction,"Cancel");
        Phase("fort gate and both bridge levels");Move(actor,new Vector3(270,0,430));await Capture("gate");float meters=0;int walks=0;
        foreach(var p in new[]{new Vector3(270,0,450),new Vector3(270,0,460),new Vector3(270,0,466),new Vector3(242,0,441),new Vector3(270,5.2f,460),new Vector3(294,5.2f,460),new Vector3(294,5.2f,476),new Vector3(282,5.2f,484)})
        {
            var target=Point(p);var motor=Get(actor,"Motor");Need((bool)Call(motor,"MoveTo",target),"Move rejected "+p);var last=actor.transform.position;var until=DateTime.UtcNow.AddSeconds(40);
            while(Vector3.Distance(actor.transform.position,target)>.9f){Need(DateTime.UtcNow<until,"Walk blocked "+p);await System.Threading.Tasks.Task.Delay(25);meters+=Vector3.Distance(last,actor.transform.position);last=actor.transform.position;}Call(motor,"Stop");walks++;if(p.x==270&&p.y>4)await Capture("bridge");if(p.x==282)await Capture("arena");
        }
        Phase("built combat effects");var chief=Find("DiceFree.Briarback.BriarbackChieftainEncounter");var boss=Part(chief,"DiceFree.Combat.CombatActor");var attack=Part(chief,"DiceFree.Combat.BasicAttack");
        Move(actor,boss.transform.position+Vector3.forward*4.5f);Call(Get(actor,"Motor"),"SetMotionAllowed",false);((Behaviour)chief).enabled=true;((Behaviour)attack).enabled=true;Need((bool)Call(attack,"Order",actor),"Boss target rejected");await Until(()=>(bool)Get(chief,"Winding"),"Built smash");
        Need(GameObject.Find("Chieftain smash - red fracture warning").GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial.shader.isSupported),"Warning shader unsupported");await Capture("smash-warning",50);await Until(()=>!(bool)Get(chief,"Winding"),"Smash impact");Call(Get(boss,"Health"),"Restore");Call(attack,"Cancel");Call(Get(actor,"Motor"),"SetMotionAllowed",true);
        Move(actor,boss.transform.position+Vector3.forward*12);Call(Get(actor,"Motor"),"SetMotionAllowed",false);Call(Get(boss,"Health"),"ApplyPacket",actor,Packet(10));Need((bool)Call(attack,"Order",actor),"Axe target rejected");GameObject axe=null;await Until(()=>(axe=GameObject.Find("Briarback Chieftain homing axe"))!=null,"Built axe");Need(axe.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial.shader.isSupported),"Axe shader unsupported");await Capture("axe-flight",130);await Until(()=>GameObject.Find("Briarback Chieftain homing axe")==null,"Axe impact");Call(Get(boss,"Health"),"Restore");Call(Get(actor,"Motor"),"SetMotionAllowed",true);
        Call(Get(boss,"Health"),"ApplyPacket",actor,Packet(1000000));Need(Get(Call(journal,"GetProgress",elderId),"status").ToString()=="ReadyToTurnIn","Built Chieftain kill credit missing");Need(!((string)Get(actor,"DisplayName")).EndsWith(" the Chieftain's Bane",StringComparison.Ordinal),"Title granted on kill");Move(actor,(Vector3)Get(elder,"ApproachPosition"));Need((bool)Call(interaction,"Order",elder),"Elder return rejected");await Until(()=>Get(interaction,"Active")==elder,"Elder turn-in dialogue");Need((bool)Call(elder,"TurnIn",actor,elderQuest)&&!(bool)Call(elder,"TurnIn",actor,elderQuest),"Title reward was missing or repeatable");Call(interaction,"Cancel");Need(((string)Get(actor,"DisplayName")).EndsWith(" the Chieftain's Bane",StringComparison.Ordinal),"Built honorific missing");await Capture("elder-title");
        var persistence=Part(actor,"DiceFree.Persistence.ManifestationPersistence");var motion=Part(actor,"DiceFree.UI.PlayerMotionPresentation");
        foreach(var classId in new[]{"class.physically-blessed-novice","class.magically-touched-novice"})
        {
            var catalog=persistence.GetType().GetField("classCatalog",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(persistence);var definition=Call(catalog,"Resolve",classId);Need(definition!=null&&(bool)Call(persistence,"TryForkAndActivate",definition,null),"Built class fixture failed");await Until(()=>(string)Get(motion,"ControllerClassId")==classId,"Built class motion");await Capture(classId.Contains("physically")?"physical-player":"magical-player");
        }
        Need((bool)Call(persistence,"TryActivateExisting","class.novice",null),"Built Novice restoration failed");await Until(()=>(string)Get(motion,"ControllerClassId")=="class.novice","Built Novice motion");
        Need((bool)Call(persistence,"Flush"),"Save failed");Need((bool)Call(Part(actor,"DiceFree.Dungeons.ReturnToStartMenu"),"Return"),"Save/return failed");menu=await Menu();Need((bool)Get(menu,"HasLoadedProfile"),"Saved profile missing");
        Phase("saved resume");var entry=((System.Collections.IEnumerable)Get(menu,"Entries")).Cast<object>().First(e=>(bool)Get(e,"active"));Need(((string)Get(entry,"displayName")).EndsWith(" the Chieftain's Bane",StringComparison.Ordinal),"Built menu title missing");Call(menu,"SelectTier",1);await Capture("titled-class-roster");Call(menu,"SelectTier",0);Need((bool)Call(menu,"TryPlayManifestation",Get(entry,"classId")),"Resume rejected");actor=await Player("Cornberg");Need(Get(Call(Part(actor,"DiceFree.Quests.QuestJournal"),"GetProgress",questId),"status").ToString()=="Completed","Hunt completion not persisted");Need(((string)Get(actor,"DisplayName")).EndsWith(" the Chieftain's Bane",StringComparison.Ordinal),"Built title did not persist");Need((string)Get(Part(actor,"DiceFree.Persistence.ManifestationPersistence"),"EchoName")=="Windows milestone","Identity changed");Need((bool)Call(Part(actor,"DiceFree.Dungeons.ReturnToStartMenu"),"Return"),"Second return failed");menu=await Menu();
        Phase("development room and normal shutdown");Need((bool)Call(menu,"TryOpenDevelopmentRoom"),"Development entry failed");actor=await Player("DevelopmentRoom");Need((bool)Call(Part(actor,"DiceFree.Dungeons.ReturnToStartMenu"),"Return"),"Development return failed");menu=await Menu();await System.Threading.Tasks.Task.Delay(200);Need(FindAll("DiceFree.ApplicationFlow.LoadingScreen").Length==0,"Overlay leaked");Need(Time.frameCount>firstFrame+100,"Player did not advance");
        Need(Quality("Preset").ToString()=="High","Quality selection did not survive scene transitions");
        System.IO.File.WriteAllText(report,"{\"status\":\"passed\",\"chieftainsBane\":true,\"playerMotionClasses\":3,\"graphicsPresets\":4,\"huntTurnedIn\":true,\"resumePersisted\":true,\"fortWalks\":"+walks+",\"walkedMeters\":"+meters.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"builtWarningAndAxe\":true,\"developmentRoom\":true,\"quitRequested\":true}");Debug.Log("DICEFREE_WINDOWS_MILESTONE_SMOKE_OK");Need((bool)Call(Find("DiceFree.UI.OptionsPanel"),"RequestQuitToDesktop"),"Normal quit rejected");
    }
    catch(Exception error){System.IO.File.WriteAllText(report,"{\"status\":\"failed\",\"error\":\""+error.ToString().Replace("\\","\\\\").Replace("\"","\\\"").Replace("\r"," ").Replace("\n"," ")+"\"}");Debug.LogError(error);}
}
_ = Run();return "Started isolated Windows smoke; inspect QA/player-smoke.json.";
