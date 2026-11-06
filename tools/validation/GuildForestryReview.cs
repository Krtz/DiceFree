using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using DiceFree.Advancement;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class GuildForestryReview
{
    const string Commission="quest.arborium.timber-commission",Clearing="quest.arborium.room-to-work",Survey="quest.arborium.road-survey";
    static CombatActor player;static int checks,walks;static string phase;static float meters;
    static readonly List<string> errors=new();
    static void Need(bool ok,string why){checks++;if(!ok)throw new InvalidOperationException(why);}
    static void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(text);}
    static async Task Until(Func<bool> f,string why,int seconds=50){var end=DateTime.UtcNow.AddSeconds(seconds);while(!f()){if(DateTime.UtcNow>end)throw new TimeoutException(why);await Task.Delay(20);}}
    static QuestJournal Journal=>player.GetComponent<QuestJournal>();
    static Interactor Input=>player.GetComponent<Interactor>();
    static QuestDefinition Q(string id)=>Journal.Definitions.Single(q=>q.stableId==id);
    static QuestGiver Steward=>Object.FindObjectsByType<QuestGiver>().Single(g=>g.Offers.Any(q=>q?.stableId==Commission));
    static ConversationTarget Foreman=>Object.FindObjectsByType<ConversationTarget>().Single(t=>t.NpcId=="npc.trimery.guild-foreman");
    static TimedInteractionTarget[] Sites(string id)=>Object.FindObjectsByType<TimedInteractionTarget>().Where(t=>t.GetComponent<QuestWorkGate>().Quest.stableId==id).OrderBy(t=>t.ContentId).ToArray();
    static long Xp(){var xp=player.GetComponent<ExperienceProgression>();long sum=xp.CurrentXp;for(int i=1;i<xp.Level;i++)sum+=30+3L*(i-1)*(i-1);return sum;}
    static void Phase(string name){phase=name;File.WriteAllText("Logs/GuildForestryReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{status="running",phase,checks,walks,meters}));}
    public static string Start()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean Edit Mode required");
        var guard=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("DiceFree.EditorTools.PersistenceTestGuard")).First(t=>t!=null);
        guard.GetMethod("UseIsolatedSaveRootForNextPlay").Invoke(null,new object[]{Path.Combine(Path.GetTempPath(),"DiceFree-GuildForestry-"+Guid.NewGuid())});
        EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");EditorApplication.EnterPlaymode();return "Started isolated guild forestry review";
    }
    public static string Run(){checks=walks=0;meters=0;errors.Clear();Application.logMessageReceived+=Log;_=Execute();return "Started; inspect Logs/GuildForestryReview.json";}
    static async Task Player(){await Until(()=>(player=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>())!=null&&player.Motor.Ready&&player.GetComponent<ManifestationPersistence>().Ready&&!DiceFree.ApplicationFlow.LoadingScreen.Busy,"Ready isolated player");player.GetComponent<TraversalInput>().enabled=false;player.GetComponent<CombatInput>().enabled=false;player.GetComponent<BasicAttack>().enabled=false;player.Health.Invulnerable=true;}
    static void Move(Vector3 p){Need(NavMesh.SamplePosition(p,out var hit,1,-1)&&player.Motor.TeleportAcrossMaps(hit.position),"Fixture travel "+p);Physics.SyncTransforms();Object.FindAnyObjectByType<ExplorationCamera>().RecenterForTravel(player.transform);}
    static async Task Walk(Vector3 p){walks++;Need(player.Motor.MoveTo(p),"Actual walk order "+p);var old=player.transform.position;await Until(()=>{meters+=Vector3.Distance(old,player.transform.position);old=player.transform.position;return Vector3.Distance(old,p)<.6f;},"Actual walk "+p);player.Motor.Stop();}
    static async Task Capture(string name){Directory.CreateDirectory("VisualBaselines/GuildForestry");await Task.Delay(180);ScreenCapture.CaptureScreenshot("VisualBaselines/GuildForestry/"+name+".png");await Task.Delay(150);}
    static async Task Accept(string id){Move(Steward.ApproachPosition);Need(Input.Order(Steward),"Open guild commission");await Until(()=>Input.Active==Steward,"Guild quest UI");var panel=Object.FindAnyObjectByType<QuestInteractionPanel>();Need(panel.SelectOffer(Q(id)),"Select guild quest "+id);Need(Steward.Accept(player,Q(id)),"Accept "+id);Input.Cancel();}
    static async Task Turn(string id){Move(Steward.ApproachPosition);Need(Input.Order(Steward),"Guild turn-in order");await Until(()=>Input.Active==Steward,"Turn-in interaction");long xp=Xp(),gold=player.GetComponent<GoldWallet>().Gold;Need(Steward.TurnIn(player,Q(id)),"Quest turn-in "+id);Need(Xp()-xp==Q(id).rewardXp&&player.GetComponent<GoldWallet>().Gold-gold==Q(id).reward.gold,"Exact reward "+id);Need(!Steward.TurnIn(player,Q(id)),"No duplicate reward");Input.Cancel();}
    static async Task Talk(){Need(Input.Order(Foreman),"Foreman conversation order");await Until(()=>Input.Active==Foreman,"Foreman interaction");Need(Foreman.Acknowledge(player)&&!Foreman.Acknowledge(player),"One acknowledged foreman conversation");Input.Cancel();}
    static async Task Begin(TimedInteractionTarget target){Need(Input.Order(target),"Work order "+target.ContentId);await Until(()=>target.Worker==player,"Work began "+target.ContentId);}
    static async Task Work(TimedInteractionTarget target,bool capture=false){await Task.Delay(220);var map=Object.FindAnyObjectByType<MinimapHud>();Need(map.TryProjectWorkMarker(target,new Rect(0,0,300,300),out _),"Active work shown on minimap");await Begin(target);if(capture){await Task.Delay(700);Need(player.GetComponent<TimedInteractionPanel>().Bounds.width>0,"Visible work progress");await Capture("work-progress");}await Until(()=>target.Worker==null&&!target.Available(player),"Work completed "+target.ContentId);Need(!map.TryProjectWorkMarker(target,new Rect(0,0,300,300),out _),"Completed minimap marker removed");Need(!Input.Order(target),"Completed site cannot be farmed");await Task.Delay(50);var view=target.GetComponent<GuildWorkSitePresentation>();Need(view.transform.Find("Completed work").gameObject.activeSelf&&!view.transform.Find("Before work").gameObject.activeSelf,"Visible completed work");}
    static async Task Resume(){Need(player.GetComponent<DiceFree.Dungeons.ReturnToStartMenu>().Return(),"Mid-progress menu exit");StartMenuController menu=null;await Until(()=>(menu=Object.FindAnyObjectByType<StartMenuController>())!=null&&!DiceFree.ApplicationFlow.LoadingScreen.Busy,"Menu");Need(menu.TryPlayManifestation(menu.Entries.First(e=>e.active).classId),"Resume isolated manifestation");await Player();}
    static void ObjectiveRegression()
    {
        foreach(float yaw in new[]{0f,37f,90f,180f,273f})foreach(float radius in new[]{8f,22f,60f})foreach(float u in new[]{.1f,.5f,.9f})foreach(float v in new[]{.1f,.5f,.9f})
        {
            var rect=new Rect(151,53,277,203);var point=new Vector2(rect.x+u*rect.width,rect.y+v*rect.height);var up=Quaternion.Euler(0,yaw,0)*Vector3.forward;
            var world=MinimapHud.WorldPointForMapClick(new Vector3(42,1,160),radius,rect,point,up);
            Need(Vector2.Distance(MinimapHud.MapPointForWorld(new Vector3(42,1,160),world,radius,rect,up),point)<.001f,"Marker/map-click projection round trip");
        }
        var objective=Q(Clearing).stages[0];Need((int)ObjectiveKind.Kill==0&&(int)ObjectiveKind.SolvePuzzle==4,"Old serialized objective ordinals");
        IEnumerable<int[]> Permute(int[] values)=>values.Length==0?new[]{Array.Empty<int>()}:values.SelectMany(v=>Permute(values.Where(x=>x!=v).ToArray()).Select(t=>new[]{v}.Concat(t).ToArray()));
        foreach(var order in Permute(new[]{0,1,2,3,4}))
        {
            var state=new QuestProgress{status=QuestStatus.Active};
            foreach(int index in order){var id=objective.alternatives[index].objectiveId;int before=state.count;Need(ObjectiveProgress.Apply(objective,state,l=>l.objectiveId==id)&&state.count==before+1,"All group any order");Need(!ObjectiveProgress.Apply(objective,state,l=>l.objectiveId==id)&&state.count==before+1,"All group duplicate rejection");Need(ObjectiveProgress.Valid(objective,state),"Valid durable leaf records");}
            Need(state.count==5&&state.alternatives.Length==5,"All five distinct sites required");
        }
        var any=new QuestObjective{kind=ObjectiveKind.Any,count=1,alternatives=new[]{new QuestLeaf{objectiveId="a",count=2},new QuestLeaf{objectiveId="b",count=1}}};var legacy=new QuestProgress{status=QuestStatus.Active};
        Need(ObjectiveProgress.Apply(any,legacy,l=>l.objectiveId=="a")&&legacy.count==0&&ObjectiveProgress.Valid(any,legacy),"Legacy ANY partial");Need(ObjectiveProgress.Apply(any,legacy,l=>l.objectiveId=="b")&&legacy.count==1&&ObjectiveProgress.Valid(any,legacy),"Legacy ANY alternative completes");
    }
    static async Task Execute()
    {
        var viewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");var view=Resources.FindObjectsOfTypeAll(viewType).FirstOrDefault();var gizmos=viewType.GetProperty("drawGizmos",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);var oldGizmos=view==null?null:gizmos.GetValue(view);
        try
        {
            if(view!=null)gizmos.SetValue(view,false);await Player();Phase("objective compatibility and locked offers");ObjectiveRegression();
            Need(!Journal.CanAccept(Q(Clearing))&&!Journal.CanAccept(Q(Survey)),"Ordered quest prerequisites");Need(Sites(Clearing).Concat(Sites(Survey)).All(t=>!t.Available(player)),"Work locked until corresponding commission");
            Phase("commission and foreman");await Accept(Commission);Move(Foreman.ApproachPosition+Vector3.forward*3);await Walk(Foreman.ApproachPosition);await Talk();Need(Journal.GetProgress(Commission).status==QuestStatus.ReadyToTurnIn,"Foreman report credited");await Turn(Commission);await Accept(Clearing);
            var sites=Sites(Clearing);Phase("work cancellation");Move(sites[4].ApproachPosition);await Begin(sites[4]);await Task.Delay(250);Input.Cancel();await Until(()=>sites[4].Worker==null,"Explicit cancellation");Need(Journal.GetProgress(Clearing).count==0,"Cancellation gives no credit");
            await Begin(sites[4]);Need(player.Motor.MoveTo(player.transform.position+Vector3.right*2),"Movement interrupts work");await Until(()=>sites[4].Worker==null,"Movement cancellation");player.Motor.Stop();Need(Journal.GetProgress(Clearing).count==0,"Movement gives no credit");
            Move(sites[4].ApproachPosition);await Begin(sites[4]);player.Health.Invulnerable=false;player.Health.ApplyPacket(null,new DamageResult{mitigated=1});player.Health.Invulnerable=true;await Until(()=>sites[4].Worker==null,"Damage cancellation");Need(Journal.GetProgress(Clearing).count==0,"Damage gives no credit");
            Phase("out of order work and persistence");await Work(sites[4],true);await Walk(sites[1].ApproachPosition);await Work(sites[1]);Need(Journal.GetProgress(Clearing).count==2,"Two distinct sites");await Resume();sites=Sites(Clearing);Need(Journal.GetProgress(Clearing).count==2&&!sites[4].Available(player)&&!sites[1].Available(player),"Partial work survives saved resume");
            Move(sites[1].ApproachPosition);foreach(int index in new[]{3,0,2}){await Walk(sites[index].ApproachPosition);await Work(sites[index]);}
            Need(Journal.GetProgress(Clearing).stage==1&&Journal.GetProgress(Clearing).status==QuestStatus.Active,"All five sites require foreman report");await Walk(Foreman.ApproachPosition);await Talk();await Turn(Clearing);
            Phase("road survey and guild stairs");await Accept(Survey);var survey=Sites(Survey);foreach(var site in survey.Where(s=>s.ContentId!="work.arborium.survey.3")){Move(site.ApproachPosition+Vector3.back*3);await Walk(site.ApproachPosition);await Work(site);}
            var planning=survey.Single(s=>s.ContentId=="work.arborium.survey.3");Move(Steward.ApproachPosition);Need(!Object.FindAnyObjectByType<MinimapHud>().TryProjectWorkMarker(planning,new Rect(0,0,300,300),out _),"Minimap hides survey on a different floor");await Walk(planning.ApproachPosition);await Task.Delay(220);Need(Object.FindAnyObjectByType<MinimapHud>().TryProjectWorkMarker(planning,new Rect(0,0,300,300),out _),"Minimap shows occupied-floor survey");await Begin(planning);await Capture("guild-planning-survey");await Until(()=>planning.Worker==null&&!planning.Available(player),"Planning survey completed");Need(player.transform.position.y>9,"Planning table reached by actual stairs");await Capture("guild-planning-floor");await Turn(Survey);
            Phase("completed quest reload");await Resume();Need(new[]{Commission,Clearing,Survey}.All(id=>Journal.GetProgress(id).status==QuestStatus.Completed),"All three commissions saved complete");Need(Sites(Clearing).Concat(Sites(Survey)).All(t=>!t.Available(player)),"No farming after reload");Move(new Vector3(249,0,148));await Capture("cleared-timberline");Need(errors.Count==0,"Unity errors: "+string.Join(";",errors));
            var result=new{status="passed",checks,walks,walkedMeters=meters,quests=3,workSites=5,surveySites=3,permutations=120,cancelAndDamage=true,partialResume=true,completedResume=true,allFloorsByStairs=true,unityErrors=errors};File.WriteAllText("Logs/GuildForestryReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));Debug.Log("DICEFREE_GUILD_FORESTRY_OK");
        }
        catch(Exception e){File.WriteAllText("Logs/GuildForestryReview.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{status="failed",phase,checks,walks,meters,error=e.ToString(),unityErrors=errors},Newtonsoft.Json.Formatting.Indented));Debug.LogError(e);}
        finally{if(view!=null&&oldGizmos!=null)gizmos.SetValue(view,oldGizmos);Application.logMessageReceived-=Log;}
    }
}
