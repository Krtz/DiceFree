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
using DiceFree.Items;

namespace DiceFree.Dungeons
{
    public enum WolfRunPhase { Loading, Staging, Warning, Fighting, Intermission, Victory, Leaving, LoadingRewards, Rewards }
    public sealed class WolfDungeonRun : MonoBehaviour, IDungeonRewardRun
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
        float started,clearDuration;bool completed;
        string announcement;float announcementUntil;IronpineCreatureAudio announcementAudio;
        public string Announcement=>Time.unscaledTime<announcementUntil?announcement:null;
        public float RunSeconds=>completed?clearDuration:Active?Mathf.Max(0,Time.time-started):0;
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
        public WolfWaveComposition RemainingComposition=>new WolfWaveComposition(
            wolves.Count(w=>w!=null&&w.Actor.Alive&&w.Rank==WolfRank.Weak),
            wolves.Count(w=>w!=null&&w.Actor.Alive&&w.Rank==WolfRank.Strong),
            wolves.Count(w=>w!=null&&w.Actor.Alive&&w.Rank==WolfRank.Alpha));
        public float SecondsRemaining=>Mathf.Max(0,deadline-Time.time);
        public string Status=>Phase switch
        {
            WolfRunPhase.Loading=>"Travelling to the moonlit clearing…",
            WolfRunPhase.Staging=>$"Prepare your party • {Mathf.CeilToInt(SecondsRemaining)}s • 10 waves",
            WolfRunPhase.Warning=>$"Wave {Wave} / 10 • {Composition.Points} points • Incoming in {Mathf.CeilToInt(SecondsRemaining)}s",
            WolfRunPhase.Fighting=>$"Wave {Wave} / 10 • {Composition.Points} points • Wolves remaining: {Remaining}",
            WolfRunPhase.Intermission=>$"Wave {Wave} cleared • Next wave: {Wave+3} points • {Mathf.CeilToInt(SecondsRemaining)}s",
            WolfRunPhase.Victory=>$"All 10 waves survived! • Treasure room in {Mathf.CeilToInt(SecondsRemaining)}s",
            WolfRunPhase.LoadingRewards=>"Travelling to your treasure choices…",
            WolfRunPhase.Rewards=>"Choose one reward chest",
            _=>"Leaving the clearing…"
        };
        void OnEnable()=>HudPointerBlocker.OverlayCovers+=CoversPointer;
        void OnDisable()=>HudPointerBlocker.OverlayCovers-=CoversPointer;
        public string Enter(CombatActor actor,WolfDungeonTuning data,Vector3 returnPoint)
        {
            if(actor==null||!actor.Alive||actor.GetComponent<DiceFree.Characters.TraversalInput>()==null)return "Only a living player can enter.";
            if(data==null||data.definition==null)return "This entrance is not ready.";
            if(LoadingScreen.Busy||Phase==WolfRunPhase.Leaving||Phase==WolfRunPhase.LoadingRewards||Phase==WolfRunPhase.Rewards)return "Please wait for the current journey.";
            var state=actor.GetComponent<RunLoadoutLock>();if(state?.Owner!=null&&state.Owner!=this)return "Resolve your current dungeon run first.";
            if(Participants.Contains(actor))return "You have already joined this run.";
            var party=actor.GetComponent<LocalPartyMember>()?.partyId;
            if(string.IsNullOrEmpty(party))party="solo:"+actor.GetEntityId();
            if(!registry.TryClaimDungeonStaging(data.definition.stableId,party,actor.GetEntityId().ToString(),DateTime.UtcNow,out var claim,out var error))return error;
            Participants.Add(actor);var hub=new SemanticGameEventHub();hubs.Add(actor,hub);actor.GetComponent<CodexProgression>()?.Bind(hub);actor.GetComponent<QuestJournal>()?.BindDungeonEvents(hub);
            var map=actor.GetComponent<MinimapHud>();mapNames[actor]=map?.MapName;
            Action died=()=>{deaths.Add(actor);hub.Publish(new DungeonPlayerDiedEvent(data.definition.stableId));};deathHandlers.Add(actor,died);actor.Health.Died+=died;
            state=state??actor.gameObject.AddComponent<RunLoadoutLock>();state.Owner=this;state.InsideDungeon=true;state.Locked=false;
            state.DeathReturnHandler=()=>{withdrawn.Add(actor);if(Phase==WolfRunPhase.Rewards)FindAnyObjectByType<DungeonRewardRoom>()?.DiscardForSessionExit(actor);else ReturnPlayer(actor,false);return true;};
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
        public bool StartNextWave(CombatActor requester)
        {
            if(Phase!=WolfRunPhase.Intermission||requester==null||requester!=Initiator||!Living.Contains(requester))return false;
            NextWave();return true;
        }
        void Update()
        {
            if(tuning==null||Phase==WolfRunPhase.Loading||Phase==WolfRunPhase.Leaving||Phase==WolfRunPhase.LoadingRewards||Phase==WolfRunPhase.Rewards)return;
            registry.TouchDungeon(tuning.definition.stableId,lease.leaseToken,DateTime.UtcNow,out _);
            if(!Living.Any()&&Phase!=WolfRunPhase.Victory){Publish(new DungeonWipedEvent(tuning.definition.stableId));StartCoroutine(ExitAll(false));return;}
            if(!Living.Contains(Initiator)&&StagingReady)Initiator=Living.First();
            foreach(var a in Living){var map=a.GetComponent<MinimapHud>();if(map!=null)map.MapName="Moonlit Clearing";}
            if(StagingReady&&Time.time>=deadline)StartNow(Initiator);
            else if(Phase==WolfRunPhase.Warning&&Time.time>=deadline)SpawnWave();
            else if(Fighting&&Remaining==0)ClearWave();
            else if(Phase==WolfRunPhase.Intermission&&Time.time>=deadline)NextWave();
            else if(Phase==WolfRunPhase.Victory&&Time.time>=deadline)ClaimRewards(Initiator);
        }
        void NextWave()
        {
            ClearWolves();Wave++;Composition=WolfWavePlan.Create(Wave,Seed);Phase=WolfRunPhase.Warning;deadline=Time.time+tuning.warningSeconds;
            announcement=Wave==10?"FINAL WAVE — FOUR ALPHAS":$"WAVE {Wave} / 10  ·  {Composition.Points} POINTS";announcementUntil=Time.unscaledTime+2;
            if(announcementAudio==null){announcementAudio=gameObject.AddComponent<IronpineCreatureAudio>();GetComponent<AudioSource>().spatialBlend=0;}
            announcementAudio.Cue("Howl");
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
                var entrance=PackEntrance(index);
                if(!NavMesh.SamplePosition(entrance,out var spawn,2,-1))throw new InvalidOperationException("Wolf trail has no navigation at "+entrance);
                var go=Instantiate(tuning.Template(rank),spawn.position,Quaternion.identity);go.name=rank==WolfRank.Weak?"Weak Wolf":rank==WolfRank.Strong?"Strong Wolf":"Alpha Wolf";
                var member=go.AddComponent<WolfDungeonEnemy>();member.Initialize(this,rank,index);wolves.Add(member);index++;
            }
            IronpineRealisticWolfRuntime.ApplyToWorld();Phase=WolfRunPhase.Fighting;
        }
        static Vector3 PackEntrance(int index)
        {
            int gate=index%3,slot=index/3;
            var trail=gate==0?new Vector3(-23,0,9):gate==1?new Vector3(23,0,9):new Vector3(0,0,25);
            var direction=trail.normalized;var side=new Vector3(-direction.z,0,direction.x);
            return Origin+trail+direction*5.5f+side*((slot%2==0?-1:1)*(.8f+slot/2*1.2f));
        }
        void ClearWave()
        {
            WolvesDefeated+=Composition.Count;WavesCleared=Wave;
            if(Wave<WolfWavePlan.WaveCount){Phase=WolfRunPhase.Intermission;deadline=Time.time+tuning.intermissionSeconds;return;}
            if(completed)return;completed=true;clearDuration=Time.time-started;Phase=WolfRunPhase.Victory;deadline=Time.time+tuning.victorySeconds;
            registry.TryCompleteDungeon(tuning.definition.stableId,lease.leaseToken,DateTime.UtcNow,out _);
            Publish(new DungeonCompletedEvent(tuning.definition.stableId,"ten-waves",Participants.Count,Time.time-started,deaths.Count==0));
        }
        void Publish<T>(T e) where T:struct,ISemanticGameEvent{foreach(var hub in hubs.Values)hub.Publish(e);}
        public void Abandon()
        {
            if(Phase==WolfRunPhase.Victory){ClaimRewards(Initiator);return;}
            if(Phase==WolfRunPhase.Rewards||Phase==WolfRunPhase.LoadingRewards)return;
            if(Phase!=WolfRunPhase.Leaving)StartCoroutine(ExitAll(StagingReady));
        }
        public bool ClaimRewards(CombatActor requester)
        {
            if(Phase!=WolfRunPhase.Victory||requester!=Initiator||requester==null)return false;
            if(tuning.rewards==null){Debug.LogError("Wolf Survival reward pool is missing.");StartCoroutine(ExitAll(true));return false;}
            Phase=WolfRunPhase.LoadingRewards;
            if(SceneManager.GetSceneByName("DungeonRewardRoom").isLoaded)RewardsLoaded(SceneManager.GetSceneByName("DungeonRewardRoom"));
            else if(!LoadingScreen.TryLoadAdditive("DungeonRewardRoom","Opening your wolf dungeon rewards",RewardsLoaded,LoadFailed))LoadFailed(new InvalidOperationException("Reward travel is busy."));
            return true;
        }
        void RewardsLoaded(Scene rewardScene)
        {
            if(this==null||Phase!=WolfRunPhase.LoadingRewards)return;
            var room=rewardScene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<DungeonRewardRoom>()).Single();
            foreach(var a in Participants.Where(a=>a!=null&&!withdrawn.Contains(a)).ToArray())
            {
                if(!a.Alive){ReturnPlayer(a,false);withdrawn.Add(a);continue;}
                a.GetComponent<RunLoadoutLock>().Locked=false;
                Relocate(a,SlimeDungeonRun.RewardOrigin+new Vector3(Participants.IndexOf(a)*2,0,0),false);
                room.Offer(a,tuning.rewards,this,tuning.bonusXp,tuning.bonusGold);
            }
            Phase=WolfRunPhase.Rewards;
            registry.ReleaseDungeon(tuning.definition.stableId,lease.leaseToken,"world.face.1",out _);
            var arena=SceneManager.GetSceneByName(tuning.definition.sceneId);if(arena.isLoaded)SceneManager.UnloadSceneAsync(arena);
            if(!room.HasChoice(Initiator)&&!Participants.Any(room.HasChoice))Finish();
        }
        public void ObserveReward(CombatActor actor,ItemDefinition item)=>hubs[actor].Publish(new DropObservedEvent(tuning.definition.stableId,item.stableId,true));
        public void Drop(CombatActor actor,ItemDefinition item)
        {var inv=actor.GetComponent<CarriedInventory>();inv.Configure(inv.Definitions.Concat(new[]{item}).Distinct().ToArray());inv.Grant(item.stableId);}
        public void Finish()
        {
            Phase=WolfRunPhase.Leaving;
            var room=FindAnyObjectByType<DungeonRewardRoom>();var scene=SceneManager.GetSceneByName("DungeonRewardRoom");
            if(scene.isLoaded&&(room==null||room.Pending==0))SceneManager.UnloadSceneAsync(scene);
            if(Current==this)Current=null;Destroy(gameObject);
        }
        void ClearWolves(){foreach(var w in wolves)if(w!=null)Destroy(w.gameObject);wolves.Clear();foreach(var w in warnings)if(w!=null)Destroy(w);warnings.Clear();}
        IEnumerator ExitAll(bool atEntrance)
        {
            if(Phase==WolfRunPhase.Leaving)yield break;Phase=WolfRunPhase.Leaving;ClearWolves();if(dummy!=null)Destroy(dummy);
            foreach(var a in Participants)if(a!=null&&!withdrawn.Contains(a))ReturnPlayer(a,atEntrance);
            registry.ReleaseDungeon(tuning.definition.stableId,lease.leaseToken,"world.face.1",out _);
            var scene=SceneManager.GetSceneByName(tuning.definition.sceneId);if(scene.isLoaded)yield return SceneManager.UnloadSceneAsync(scene);
            if(Current==this)Current=null;Destroy(gameObject);
        }
        public void ReturnPlayer(CombatActor a,bool atEntrance)
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
            if(point.x>29000&&point.x<31000){FindAnyObjectByType<WorldDayNightCycle>()?.SetInterior(true);FindAnyObjectByType<ExplorationCamera>()?.ConfigureMapBounds(new Rect(29988,29988,24,24));FindAnyObjectByType<ExplorationCamera>()?.RecenterForTravel(a.transform);var map=a.GetComponent<MinimapHud>();if(map!=null)map.MapName="Dungeon Treasury";}
        }
        static void ConfigureCamera(CombatActor a,bool inside)
        {
            var camera=FindAnyObjectByType<ExplorationCamera>();if(camera==null)return;
            if(!inside)FindAnyObjectByType<WorldDayNightCycle>()?.SetInterior(false);
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
        public static bool CoversPointer(Vector2 p)=>Current!=null&&Current.tuning!=null&&Current.Phase!=WolfRunPhase.Leaving&&Current.Phase!=WolfRunPhase.Rewards&&Current.Phase!=WolfRunPhase.LoadingRewards&&(new Rect(15,80,550,100).Contains(new Vector2(p.x,Screen.height-p.y))||((Current.Announcement!=null||Current.Phase==WolfRunPhase.Victory)&&new Rect(Screen.width*.5f-220,200,440,100).Contains(new Vector2(p.x,Screen.height-p.y))));
        GUIStyle heading,body;
        void OnGUI()
        {
            if(tuning==null||LoadingScreen.Busy||Phase==WolfRunPhase.Leaving||Phase==WolfRunPhase.LoadingRewards||Phase==WolfRunPhase.Rewards||HudPointerBlocker.ModalOpen)return;
            if(heading==null){heading=new GUIStyle(GUI.skin.label){fontSize=17,fontStyle=FontStyle.Bold};heading.normal.textColor=new Color(.72f,.87f,1);body=new GUIStyle(GUI.skin.label){fontSize=13};body.normal.textColor=new Color(.9f,.93f,.96f);}
            var old=GUI.color;GUI.color=new Color(.08f,.13f,.2f,.94f);GUI.DrawTexture(new Rect(15,80,550,100),Texture2D.whiteTexture);GUI.color=old;
            GUI.Label(new Rect(27,85,520,23),"WOLF SURVIVAL  ·  MOONLIT CLEARING",heading);
            GUI.Label(new Rect(27,109,520,22),Status,body);
            if(Announcement!=null||Phase==WolfRunPhase.Victory)
            {
                GUI.color=new Color(.06f,.11f,.17f,.92f);GUI.DrawTexture(new Rect(Screen.width*.5f-220,200,440,Phase==WolfRunPhase.Victory?90:50),Texture2D.whiteTexture);GUI.color=old;
                var banner=new GUIStyle(heading){alignment=TextAnchor.MiddleCenter,fontSize=20};
                GUI.Label(new Rect(Screen.width*.5f-215,207,430,28),Phase==WolfRunPhase.Victory?"MOONLIT CLEARING — CLEARED":Announcement,banner);
                if(Phase==WolfRunPhase.Victory){int seconds=Mathf.CeilToInt(RunSeconds);GUI.Label(new Rect(Screen.width*.5f-205,241,410,34),$"10 waves  ·  {WolvesDefeated} wolves  ·  {seconds/60}:{seconds%60:00}\nKill XP earned throughout the run",new GUIStyle(body){alignment=TextAnchor.MiddleCenter});}
            }
            GUI.color=new Color(.2f,.29f,.38f);GUI.DrawTexture(new Rect(27,135,520,4),Texture2D.whiteTexture);GUI.color=new Color(.4f,.75f,.9f);GUI.DrawTexture(new Rect(27,135,520*WavesCleared/10f,4),Texture2D.whiteTexture);GUI.color=old;
            if(Phase!=WolfRunPhase.Loading&&GUI.Button(new Rect(27,148,150,23),Phase==WolfRunPhase.Victory?"Claim rewards":"Leave dungeon"))Abandon();
            if(StagingReady){if(GUI.Button(new Rect(185,148,125,23),"Start waves"))StartNow(FindAnyObjectByType<DiceFree.Characters.TraversalInput>()?.GetComponent<CombatActor>());GUI.Label(new Rect(320,149,230,23),"Weak 1  ·  Strong 2  ·  Alpha 3",body);}
            else
            {
                var counts=Fighting?RemainingComposition:Composition;
                var detail=Phase==WolfRunPhase.Intermission?$"Next: {Wave+3} points":Phase==WolfRunPhase.Victory?"10 / 10 waves cleared":$"Weak {counts.Weak}  ·  Strong {counts.Strong}  ·  Alpha {counts.Alpha}";
                GUI.Label(new Rect(320,149,230,23),detail,body);
                if(Phase==WolfRunPhase.Intermission&&GUI.Button(new Rect(185,148,125,23),"Next wave"))StartNextWave(FindAnyObjectByType<DiceFree.Characters.TraversalInput>()?.GetComponent<CombatActor>());
            }
        }
    }
}
