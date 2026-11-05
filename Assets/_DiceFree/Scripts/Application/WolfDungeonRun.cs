using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using DiceFree.ApplicationFlow;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Foundation;
using DiceFree.Gameplay;
using DiceFree.Ironpine;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

namespace DiceFree.Dungeons
{
    public enum WolfRunPhase { Loading, Staging, Warning, Fighting, Intermission, Victory, Leaving }
    public sealed class WolfDungeonRun : MonoBehaviour
    {
        public static WolfDungeonRun Current {get;private set;}
        public static readonly Vector3 Origin=new(40000,0,40000);
        static SessionMapRegistry registry=new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset(){Current=null;registry=new();}
        public readonly List<CombatActor> Participants=new();
        readonly List<WolfDungeonEnemy> wolves=new();
        readonly HashSet<CombatActor> withdrawn=new(),deaths=new();
        readonly Dictionary<CombatActor,SemanticGameEventHub> hubs=new();
        readonly Dictionary<CombatActor,Action> deathHandlers=new();
        readonly Dictionary<CombatActor,string> mapNames=new();
        DungeonOccupancySnapshot lease;
        WolfDungeonTuning tuning;Vector3 entrance;float deadline;GameObject dummy;
        readonly List<GameObject> warnings=new();
        float started;bool completed;
        public WolfRunPhase Phase {get;private set;}=WolfRunPhase.Loading;
        public int Wave {get;private set;}
        public int Seed {get;private set;}
        public int WavesCleared {get;private set;}
        public int WolvesDefeated {get;private set;}
        public WolfWaveComposition Composition {get;private set;}
        public CombatActor Initiator {get;private set;}
        public bool Fighting=>Phase==WolfRunPhase.Fighting;
        public bool StagingReady=>Phase==WolfRunPhase.Staging;
        public bool Active=>Phase==WolfRunPhase.Warning||Fighting||Phase==WolfRunPhase.Intermission;
        public IEnumerable<CombatActor> Living=>Participants.Where(a=>a!=null&&a.Alive&&!withdrawn.Contains(a));
        public IReadOnlyList<WolfDungeonEnemy> Wolves=>wolves;
        public int Remaining=>wolves.Count(w=>w!=null&&w.Actor!=null&&w.Actor.Alive);
        public float SecondsRemaining=>Mathf.Max(0,deadline-Time.time);
        public string Status=>Phase switch
        {
            WolfRunPhase.Loading=>"Travelling to the moonlit clearing…",
            WolfRunPhase.Staging=>$"Prepare your party • {Mathf.CeilToInt(SecondsRemaining)}s • 10 waves",
            WolfRunPhase.Warning=>$"Wave {Wave} / 10 • {Composition.Points} points • Incoming in {Mathf.CeilToInt(SecondsRemaining)}s",
            WolfRunPhase.Fighting=>$"Wave {Wave} / 10 • {Composition.Points} points • Wolves remaining: {Remaining}",
            WolfRunPhase.Intermission=>$"Wave {Wave} cleared • Next wave: {Wave+3} points • {Mathf.CeilToInt(SecondsRemaining)}s",
            WolfRunPhase.Victory=>$"All 10 waves survived! • Returning in {Mathf.CeilToInt(SecondsRemaining)}s",
            _=>"Leaving the clearing…"
        };
        void OnEnable()=>HudPointerBlocker.OverlayCovers+=CoversPointer;
        void OnDisable()=>HudPointerBlocker.OverlayCovers-=CoversPointer;
        public string Enter(CombatActor actor,WolfDungeonTuning data,Vector3 returnPoint)
        {
            if(actor==null||!actor.Alive||actor.GetComponent<DiceFree.Characters.TraversalInput>()==null)return "Only a living player can enter.";
            if(data==null||data.definition==null)return "This entrance is not ready.";
            if(LoadingScreen.Busy||Phase==WolfRunPhase.Leaving)return "Please wait for the current journey.";
            var state=actor.GetComponent<RunLoadoutLock>();if(state?.Owner!=null&&state.Owner!=this)return "Resolve your current dungeon run first.";
            if(Participants.Contains(actor))return "You have already joined this run.";
            var party=actor.GetComponent<LocalPartyMember>()?.partyId;
            if(string.IsNullOrEmpty(party))party="solo:"+actor.GetEntityId();
            if(!registry.TryClaimDungeonStaging(data.definition.stableId,party,actor.GetEntityId().ToString(),DateTime.UtcNow,out var claim,out var error))return error;
            Participants.Add(actor);var hub=new SemanticGameEventHub();hubs.Add(actor,hub);actor.GetComponent<CodexProgression>()?.Bind(hub);actor.GetComponent<QuestJournal>()?.BindDungeonEvents(hub);
            var map=actor.GetComponent<MinimapHud>();mapNames[actor]=map?.MapName;
            Action died=()=>{deaths.Add(actor);hub.Publish(new DungeonPlayerDiedEvent(data.definition.stableId));};deathHandlers.Add(actor,died);actor.Health.Died+=died;
            state=state??actor.gameObject.AddComponent<RunLoadoutLock>();state.Owner=this;state.InsideDungeon=true;state.Locked=false;
            state.DeathReturnHandler=()=>{withdrawn.Add(actor);ReturnPlayer(actor,false);return true;};
            if(tuning==null)
            {
                Current=this;Initiator=actor;tuning=data;entrance=returnPoint;lease=claim;
                if(!LoadingScreen.TryLoadAdditive(data.definition.sceneId,"Entering the moonlit clearing",Loaded,LoadFailed))LoadFailed(new InvalidOperationException("Travel is busy."));
            }
            else if(StagingReady)Relocate(actor,Origin+new Vector3(Participants.Count*2,0,-16),true);
            UnityEngine.Application.runInBackground=true;return "Joined Wolf Survival staging.";
        }
        void Loaded(Scene scene)
        {
            if(this==null||Phase==WolfRunPhase.Leaving)return;
            try
            {
                foreach(var a in Participants.Where(a=>a!=null))Relocate(a,Origin+new Vector3((Participants.IndexOf(a)-1.5f)*2,0,-16),true);
                dummy=new GameObject("Practice target — no rewards");dummy.SetActive(false);dummy.layer=10;dummy.transform.position=Origin+new Vector3(-7,0,-17);
                var agent=dummy.AddComponent<NavMeshAgent>();agent.enabled=false;agent.radius=.6f;agent.height=2;agent.areaMask=1;
                var motor=dummy.AddComponent<DiceFree.Characters.TraversalMotor>();dummy.AddComponent<ActorStats>().Configure(tuning.dummy);
                var health=dummy.AddComponent<Health>();health.Invulnerable=true;var actor=dummy.AddComponent<CombatActor>();actor.Configure(1,.6f,"practice-target");
                dummy.AddComponent<BasicAttack>().enabled=false;dummy.AddComponent<CombatStatusController>();
                var capsule=dummy.AddComponent<CapsuleCollider>();capsule.radius=.6f;capsule.height=2;capsule.center=Vector3.up;
                PracticeDummyVisual.Create(dummy.transform);dummy.SetActive(true);motor.SetMotionBlocked("wolf.practice",true);
                Phase=WolfRunPhase.Staging;deadline=Time.time+tuning.definition.stagingSeconds;
            }
            catch(Exception e){LoadFailed(e);}
        }
        void LoadFailed(Exception error){Debug.LogError("Wolf Survival could not load: "+error.Message);StartCoroutine(ExitAll(true));}
        public bool StartNow(CombatActor requester)
        {
            if(!StagingReady||requester==null||requester!=Initiator||!Living.Contains(requester))return false;
            var context=new DungeonConditionContext{participants=Living.Select(a=>new DungeonParticipantSnapshot{participantId=a.GetEntityId().ToString(),classId=a.Stats.Definition.stableId,level=a.Stats.Level}).ToArray()};
            if(!registry.TryBeginDungeon(tuning.definition.stableId,lease.leaseToken,DungeonConditionEvaluator.Evaluate(tuning.definition,context),DateTime.UtcNow,out lease,out var error))return false;
            foreach(var a in Living)a.GetComponent<RunLoadoutLock>().Locked=true;
            if(dummy!=null)Destroy(dummy);Seed=UnityEngine.Random.Range(1,int.MaxValue);started=Time.time;NextWave();return true;
        }
        void Update()
        {
            if(tuning==null||Phase==WolfRunPhase.Loading||Phase==WolfRunPhase.Leaving)return;
            registry.TouchDungeon(tuning.definition.stableId,lease.leaseToken,DateTime.UtcNow,out _);
            if(!Living.Any()&&Phase!=WolfRunPhase.Victory){Publish(new DungeonWipedEvent(tuning.definition.stableId));StartCoroutine(ExitAll(false));return;}
            if(!Living.Contains(Initiator)&&StagingReady)Initiator=Living.First();
            foreach(var a in Living){var map=a.GetComponent<MinimapHud>();if(map!=null)map.MapName="Moonlit Clearing";}
            if(StagingReady&&Time.time>=deadline)StartNow(Initiator);
            else if(Phase==WolfRunPhase.Warning&&Time.time>=deadline)SpawnWave();
            else if(Fighting&&Remaining==0)ClearWave();
            else if(Phase==WolfRunPhase.Intermission&&Time.time>=deadline)NextWave();
            else if(Phase==WolfRunPhase.Victory&&Time.time>=deadline)StartCoroutine(ExitAll(true));
        }
        void NextWave()
        {
            ClearWolves();Wave++;Composition=WolfWavePlan.Create(Wave,Seed);Phase=WolfRunPhase.Warning;deadline=Time.time+tuning.warningSeconds;
            foreach(var p in new[]{new Vector3(-23,0,9),new Vector3(23,0,9),new Vector3(0,0,25)}.Take(Mathf.Min(3,Composition.Count)))
            {
                var go=new GameObject("Incoming wolf trail");go.transform.position=Origin+p+Vector3.up*.1f;var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=40;line.widthMultiplier=.15f;line.sharedMaterial=WolfArenaPresentation.WarningMaterial;
                for(int i=0;i<40;i++){float angle=i*Mathf.PI*2/40;line.SetPosition(i,new Vector3(Mathf.Sin(angle)*2,0,Mathf.Cos(angle)*2));}warnings.Add(go);
            }
        }
        void SpawnWave()
        {
            foreach(var w in warnings)if(w!=null)Destroy(w);warnings.Clear();
            int index=0;foreach(var rank in Composition.Wolves)
            {
                int gate=index%3,slot=index/3;var p=gate==0?new Vector3(-23+slot*2.2f,0,9):gate==1?new Vector3(23-slot*2.2f,0,9):new Vector3((slot-1)*2.3f,0,25);
                var go=Instantiate(tuning.Template(rank),Origin+p,Quaternion.identity);go.name=rank==WolfRank.Weak?"Weak Wolf":rank==WolfRank.Strong?"Strong Wolf":"Alpha Wolf";
                var member=go.AddComponent<WolfDungeonEnemy>();member.Initialize(this,rank);wolves.Add(member);index++;
            }
            IronpineRealisticWolfRuntime.ApplyToWorld();Phase=WolfRunPhase.Fighting;
        }
        void ClearWave()
        {
            WolvesDefeated+=Composition.Count;WavesCleared=Wave;
            if(Wave<WolfWavePlan.WaveCount){Phase=WolfRunPhase.Intermission;deadline=Time.time+tuning.intermissionSeconds;return;}
            if(completed)return;completed=true;Phase=WolfRunPhase.Victory;deadline=Time.time+tuning.victorySeconds;
            registry.TryCompleteDungeon(tuning.definition.stableId,lease.leaseToken,DateTime.UtcNow,out _);
            Publish(new DungeonCompletedEvent(tuning.definition.stableId,"ten-waves",Participants.Count,Time.time-started,deaths.Count==0));
        }
        void Publish<T>(T e) where T:struct,ISemanticGameEvent{foreach(var hub in hubs.Values)hub.Publish(e);}
        public void Abandon(){if(Phase!=WolfRunPhase.Leaving)StartCoroutine(ExitAll(Phase==WolfRunPhase.Victory||StagingReady));}
        void ClearWolves(){foreach(var w in wolves)if(w!=null)Destroy(w.gameObject);wolves.Clear();foreach(var w in warnings)if(w!=null)Destroy(w);warnings.Clear();}
        IEnumerator ExitAll(bool atEntrance)
        {
            if(Phase==WolfRunPhase.Leaving)yield break;Phase=WolfRunPhase.Leaving;ClearWolves();if(dummy!=null)Destroy(dummy);
            foreach(var a in Participants)if(a!=null&&!withdrawn.Contains(a))ReturnPlayer(a,atEntrance);
            registry.ReleaseDungeon(tuning.definition.stableId,lease.leaseToken,"world.face.1",out _);
            var scene=SceneManager.GetSceneByName(tuning.definition.sceneId);if(scene.isLoaded)yield return SceneManager.UnloadSceneAsync(scene);
            if(Current==this)Current=null;Destroy(gameObject);
        }
        void ReturnPlayer(CombatActor a,bool atEntrance)
        {
            a.GetComponent<QuestJournal>()?.UnbindDungeonEvents(hubs[a]);if(deathHandlers.TryGetValue(a,out var handler))a.Health.Died-=handler;
            ClearLock(a);
            if(atEntrance&&a.Alive)Relocate(a,entrance+Vector3.back*2,false);
            else {a.GetComponent<RespawnAtAnchor>()?.LoadAtAnchor(null);if(!a.Alive)a.Health.Restore();ConfigureCamera(a,false);}
            var map=a.GetComponent<MinimapHud>();if(map!=null)map.MapName=mapNames[a]??"Cornberg";
        }
        void Relocate(CombatActor a,Vector3 point,bool inside)
        {
            a.GetComponent<BasicAttack>()?.Cancel();a.GetComponent<TargetSelection>()?.Select(null);a.GetComponent<Interactor>()?.Cancel();
            if(!a.Motor.TeleportAcrossMaps(point))throw new InvalidOperationException("Wolf Survival travel failed at "+point);
            ConfigureCamera(a,inside);a.GetComponent<InteractionRegistry>()?.RegisterExistingSceneTargets();
        }
        static void ConfigureCamera(CombatActor a,bool inside)
        {
            var camera=FindAnyObjectByType<ExplorationCamera>();if(camera==null)return;
            camera.ConfigureMapBounds(inside?new Rect(39965,39965,70,70):OverworldMapBounds.ForScene(a.gameObject.scene,new Rect(-56,-44,181,116)));camera.RecenterForTravel(a.transform);
        }
        void ClearLock(CombatActor a){var state=a.GetComponent<RunLoadoutLock>();if(state==null||state.Owner!=this)return;state.Locked=false;state.InsideDungeon=false;state.Owner=null;state.DeathReturnHandler=null;}
        public void ReleaseForSessionExit()
        {
            if(tuning!=null)registry.ReleaseDungeon(tuning.definition.stableId,lease.leaseToken,"world.face.1",out _);Phase=WolfRunPhase.Leaving;ClearWolves();
        }
        void OnDestroy()
        {
            if(Current==this)Current=null;
            if(tuning!=null)registry.ReleaseDungeon(tuning.definition.stableId,lease.leaseToken,"world.face.1",out _);
            foreach(var a in Participants)if(a!=null){if(deathHandlers.TryGetValue(a,out var h))a.Health.Died-=h;if(hubs.TryGetValue(a,out var hub))a.GetComponent<QuestJournal>()?.UnbindDungeonEvents(hub);ClearLock(a);}
        }
        public static bool CoversPointer(Vector2 p)=>Current!=null&&Current.tuning!=null&&Current.Phase!=WolfRunPhase.Leaving&&new Rect(15,80,550,100).Contains(new Vector2(p.x,Screen.height-p.y));
        GUIStyle heading,body;
        void OnGUI()
        {
            if(tuning==null||LoadingScreen.Busy||Phase==WolfRunPhase.Leaving||HudPointerBlocker.ModalOpen)return;
            if(heading==null){heading=new GUIStyle(GUI.skin.label){fontSize=17,fontStyle=FontStyle.Bold};heading.normal.textColor=new Color(.72f,.87f,1);body=new GUIStyle(GUI.skin.label){fontSize=13};body.normal.textColor=new Color(.9f,.93f,.96f);}
            var old=GUI.color;GUI.color=new Color(.08f,.13f,.2f,.94f);GUI.DrawTexture(new Rect(15,80,550,100),Texture2D.whiteTexture);GUI.color=old;
            GUI.Label(new Rect(27,85,520,23),"WOLF SURVIVAL  ·  MOONLIT CLEARING",heading);
            GUI.Label(new Rect(27,109,520,22),Status,body);
            GUI.color=new Color(.2f,.29f,.38f);GUI.DrawTexture(new Rect(27,135,520,4),Texture2D.whiteTexture);GUI.color=new Color(.4f,.75f,.9f);GUI.DrawTexture(new Rect(27,135,520*WavesCleared/10f,4),Texture2D.whiteTexture);GUI.color=old;
            if(Phase!=WolfRunPhase.Loading&&GUI.Button(new Rect(27,148,150,23),Phase==WolfRunPhase.Victory?"Return to entrance":"Leave dungeon"))Abandon();
            if(StagingReady){if(GUI.Button(new Rect(185,148,125,23),"Start waves"))StartNow(FindAnyObjectByType<DiceFree.Characters.TraversalInput>()?.GetComponent<CombatActor>());GUI.Label(new Rect(320,149,230,23),"Weak 1  ·  Strong 2  ·  Alpha 3",body);}
        }
    }
}
