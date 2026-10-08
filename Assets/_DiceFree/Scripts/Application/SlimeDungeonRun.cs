using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.World;
using DiceFree.Foundation;
using DiceFree.Gameplay;
using DiceFree.Progression;
using DiceFree.Items;
using DiceFree.Quests;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace DiceFree.Dungeons
{
    public sealed class SlimeDungeonRun : MonoBehaviour
    {
        public static SlimeDungeonRun Current {get;private set;}
        private static SessionMapRegistry registry=new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetHost(){Current=null;registry=new();}
        public readonly List<CombatActor> Participants=new();
        public SlimeDungeonTuning Tuning {get;private set;}
        public bool Active {get;private set;}
        public bool Completing {get;private set;}
        public bool SecretEligible {get;private set;}
        public bool MinibossDead {get;private set;}
        public bool PuzzleSolved {get;private set;}
        public bool SecretSolved {get;private set;}
        public string Status {get;private set;}="Loading staging";
        public static readonly Vector3 Origin=new(20000,0,20000);
        public static readonly Vector3 RewardOrigin=new(30000,0,30000);
        DungeonDefinition definition; DungeonOccupancySnapshot lease;
        readonly SemanticGameEventHub events=new();
        readonly Dictionary<CombatActor,SemanticGameEventHub> playerEvents=new();
        readonly HashSet<CombatActor> deaths=new();
        readonly HashSet<CombatActor> withdrawn=new();
        readonly Dictionary<CombatActor,Action> deathHandlers=new();
        readonly Dictionary<CombatActor,Vector3> home=new();
        readonly List<GameObject> spawned=new();
        Vector3 entrance; float stagingEnd, started, completionEnd;
        bool loading, regentVictory, exiting;
        SlimeTreePuzzle puzzle; GameObject stagingGate,puzzleGate,bossGate,secretGate;
        public string Enter(CombatActor actor,DungeonDefinition data,SlimeDungeonTuning tuning,Vector3 returnPoint)
        {
            if(actor==null || !actor.Alive || actor.GetComponent<DiceFree.Characters.TraversalInput>()==null) return "Only a living player can enter.";
            if(actor.GetComponent<RunLoadoutLock>()?.Owner!=null && actor.GetComponent<RunLoadoutLock>().Owner!=this) return "Resolve your current dungeon participation first.";
            string party=actor.GetComponent<LocalPartyMember>()?.partyId;
            if(string.IsNullOrEmpty(party)) party="solo:"+actor.GetEntityId();
            if(!registry.TryClaimDungeonStaging(data.stableId,party,actor.GetEntityId().ToString(),DateTime.UtcNow,out var state,out var error)) return error;
            UnityEngine.Application.runInBackground=true;
            if(Participants.Contains(actor)) return "Already in staging.";
            Participants.Add(actor); home[actor]=actor.transform.position;
            var hub=new SemanticGameEventHub(); actor.GetComponent<CodexProgression>()?.Bind(hub);playerEvents[actor]=hub;
            deathHandlers[actor]=()=>PlayerDied(actor);actor.Health.Died+=deathHandlers[actor];
            var loadout=actor.GetComponent<RunLoadoutLock>()??actor.gameObject.AddComponent<RunLoadoutLock>();
            loadout.InsideDungeon=true;
            loadout.Owner=this;
            loadout.DeathReturnHandler=()=>{withdrawn.Add(actor);ReturnPlayer(actor,false);return true;};
            if(definition==null)
            {
                definition=data; Tuning=tuning; entrance=returnPoint;lease=state;
                stagingEnd=Time.time+data.stagingSeconds; Current=this; StartCoroutine(Load());
            }
            else if(!loading) Relocate(actor,Origin+new Vector3(8,0,5));
            return "Joined shared staging.";
        }
        IEnumerator Load()
        {
            loading=true;
            yield return SceneManager.LoadSceneAsync(definition.sceneId,LoadSceneMode.Additive);
            var scene=SceneManager.GetSceneByName(definition.sceneId);
            Transform Find(string name)=>scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).First(t=>t.name==name);
            stagingGate=Find("Staging gate").gameObject;puzzleGate=Find("Miniboss gate").gameObject;
            bossGate=Find("Normal route gate").gameObject;secretGate=Find("Enchanted Regent barrier").gameObject;
            puzzle=Find("Five ring puzzle").GetComponent<SlimeTreePuzzle>();puzzle.Run=this;
            foreach(var actor in Participants) Relocate(actor,Origin+new Vector3(8,0,5));
            Spawn(Tuning.dummy,Origin+new Vector3(8,0,10),1.1f,SlimeRole.Dummy);
            loading=false;
        }
        void Update()
        {
            if(loading||definition==null||exiting)return;
            registry.TouchDungeon(definition.stableId,lease.leaseToken,DateTime.UtcNow,out _);
            if(!Active&&!Completing)
            {
                Status=$"Shared staging: {Mathf.CeilToInt(Mathf.Max(0,stagingEnd-Time.time))}s — practice, prepare gear/class";
                if(Time.time>=stagingEnd) Begin();
            }
            if(Active && !Living.Any()) {Publish(new DungeonWipedEvent(definition.stableId));StartCoroutine(ExitAll(false));}
            if(Completing){Status=$"Clear! Resurrection window: {Mathf.CeilToInt(Mathf.Max(0,completionEnd-Time.time))}s";if(Time.time>=completionEnd)StartCoroutine(Rewards());}
        }
        void Begin()
        {
            var context=new DungeonConditionContext{participants=Participants.Select(a=>new DungeonParticipantSnapshot{participantId=a.GetEntityId().ToString(),level=a.Stats.Level,classId=a.Stats.Definition.stableId}).ToArray()};
            var variant=DungeonConditionEvaluator.Evaluate(definition,context);
            if(!registry.TryBeginDungeon(definition.stableId,lease.leaseToken,variant,DateTime.UtcNow,out lease,out var error))throw new InvalidOperationException(error);
            SecretEligible=variant.HasRoute("route.slime-regent");Active=true;started=Time.time;
            foreach(var a in Participants)a.GetComponent<RunLoadoutLock>().Locked=true;
            foreach(var s in spawned.ToArray())if(s!=null && s.GetComponent<DungeonSlime>()?.Role==SlimeRole.Dummy)Destroy(s);
            stagingGate.SetActive(false);Status="Defeat Big Slime in the northwest clearing";
            for(int i=0;i<6;i++)Spawn(Tuning.green,Origin+new Vector3(7+(i%2)*4,0,24+i*5),1,SlimeRole.Trash);
            Spawn(Tuning.miniboss,Origin+new Vector3(12,0,76),2.3f,SlimeRole.Miniboss);
        }
        public DungeonSlime Spawn(ActorDefinition stats,Vector3 at,float size,SlimeRole role,bool blue=false)
        {
            spawned.RemoveAll(s=>s==null);
            var obj=Instantiate(Tuning.slimeTemplate,at,Quaternion.identity);obj.name=stats.displayName;
            // Template has no drop/respawn/defeat reporter: this run grants rewards exactly once.
            obj.transform.localScale=Vector3.one*size;
            var model=role==SlimeRole.Boss?Tuning.bossPresentation:role==SlimeRole.Regent?Tuning.regentPresentation:null;
            if(model!=null){var old=obj.transform.Find("Presentation");if(old!=null){old.gameObject.SetActive(false);Destroy(old.gameObject);}var visual=Instantiate(model,obj.transform);visual.name="Presentation";visual.transform.localPosition=Vector3.zero;}
            var actor=obj.GetComponent<CombatActor>();actor.Stats.Configure(stats);actor.Health.Restore();actor.Configure(1,.65f*size,stats.familyId);
            var slime=obj.AddComponent<DungeonSlime>();slime.Initialize(this,role,blue);spawned.Add(obj);
            return slime;
        }
        public IEnumerable<CombatActor> Living=>Participants.Where(a=>a!=null&&a.Alive&&!withdrawn.Contains(a));
        public void Witness(string boss,string mechanic)=>Publish(new BossMechanicWitnessedEvent(boss,mechanic));
        public void Killed(DungeonSlime slime)
        {
            if(!Active)return;
            if(slime.Role==SlimeRole.Trash){Grant(Tuning.trashXp,Tuning.trashGold);Publish(new MonsterKilledEvent(slime.Actor.Stats.Definition.stableId));}
            if(slime.Role==SlimeRole.Miniboss)
            {
                MinibossDead=true;puzzleGate.SetActive(false);Grant(Tuning.minibossXp,Tuning.minibossGold);Publish(new DungeonBossKilledEvent(definition.stableId,"boss.big-slime"));puzzle.Begin(SecretEligible);Status="Lure slimes into all five green rings";
            }
            if(slime.Role==SlimeRole.Boss||slime.Role==SlimeRole.Regent)
            {
                regentVictory=slime.Role==SlimeRole.Regent;Active=false;Completing=true;completionEnd=Time.time+Tuning.victorySeconds;
                Grant(regentVictory?Tuning.regentXp:Tuning.bossXp,regentVictory?Tuning.regentGold:Tuning.bossGold);
                Publish(new DungeonBossKilledEvent(definition.stableId,regentVictory?"boss.slime-regent":"boss.slime"));
                Publish(new DungeonCompletedEvent(definition.stableId,regentVictory?"regent":"normal",Participants.Count,Time.time-started,deaths.Count==0));
                registry.TryCompleteDungeon(definition.stableId,lease.leaseToken,DateTime.UtcNow,out _);
                foreach(var s in spawned)if(s!=null)s.GetComponent<DungeonSlime>()?.Cease();
            }
        }
        public void SolvePuzzle(bool secret)
        {
            PuzzleSolved=true;SecretSolved=secret;bossGate.SetActive(false);Grant(secret?Tuning.bluePuzzleXp:Tuning.puzzleXp,0);
            Spawn(Tuning.boss,Origin+new Vector3(68,0,76),3.2f,SlimeRole.Boss);
            if(secret){secretGate.SetActive(false);Spawn(Tuning.regent,Origin+new Vector3(52,0,22),4.4f,SlimeRole.Regent);Publish(new DungeonSecretDiscoveredEvent(definition.stableId,"secret.slime-regent"));}
            Status=secret?"Both final routes open — defeat either boss":"Normal Slime Boss route open";
        }
        void PlayerDied(CombatActor a){if(!Active&&!Completing)return;deaths.Add(a);playerEvents[a].Publish(new DungeonPlayerDiedEvent(definition.stableId));}
        public void Grant(int xp,int gold){foreach(var a in Participants){if(a==null)continue;a.GetComponent<ExperienceProgression>()?.Grant(xp);a.GetComponent<GoldWallet>()?.Grant(gold);}}
        void Publish<T>(T e) where T:struct,ISemanticGameEvent{events.Publish(e);foreach(var hub in playerEvents.Values)hub.Publish(e);}
        IEnumerator Rewards()
        {
            Completing=false;exiting=true;
            if(!SceneManager.GetSceneByName("DungeonRewardRoom").isLoaded)yield return SceneManager.LoadSceneAsync("DungeonRewardRoom",LoadSceneMode.Additive);
            var room=FindFirstObjectByType<DungeonRewardRoom>();
            var pool=regentVictory?Tuning.regentRewards:Tuning.normalRewards;
            foreach(var a in Participants.ToArray())
            {
                if(a==null)continue;a.GetComponent<RunLoadoutLock>().Locked=false;
                if(a.Alive&&!withdrawn.Contains(a)){Relocate(a,RewardOrigin+new Vector3(Participants.IndexOf(a)*2,0,0));room.Offer(a,pool,this,regentVictory?Tuning.regentBonusXp:Tuning.bonusXp,regentVictory?Tuning.regentBonusGold:Tuning.bonusGold);}
                else ReturnPlayer(a,false);
            }
            registry.ReleaseDungeon(definition.stableId,lease.leaseToken,"world.face.1",out _);
            if(Current==this)Current=null;
            CleanupDungeon();
            if(room.Pending==0)Finish();
        }
        public void ReturnPlayer(CombatActor a,bool atEntrance)
        {
            a.GetComponent<RunLoadoutLock>().Locked=false;
            a.GetComponent<RunLoadoutLock>().DeathReturnHandler=null;
            a.GetComponent<RunLoadoutLock>().InsideDungeon=false;
            a.GetComponent<RunLoadoutLock>().Owner=null;
            a.Health.Died-=deathHandlers[a];
            if(atEntrance)Relocate(a,entrance+Vector3.back*3);
            else {a.GetComponent<RespawnAtAnchor>()?.LoadAtAnchor(null);if(!a.Alive)a.Health.Restore();}
            if(a.TryGetComponent<RespawnAtAnchor>(out var respawn))respawn.enabled=true;
            ConfigureCamera(a,a.transform.position);
        }
        public void ObserveReward(CombatActor a,ItemDefinition item)=>playerEvents[a].Publish(new DropObservedEvent(definition.stableId,item.stableId,true));
        public void Drop(CombatActor a,ItemDefinition item){var inv=a.GetComponent<CarriedInventory>();inv.Configure(inv.Definitions.Concat(new[]{item}).Distinct().ToArray());inv.Grant(item.stableId);}
        public void Abandon()=>StartCoroutine(ExitAll(false));
        IEnumerator ExitAll(bool entranceReturn)
        {
            if(exiting)yield break;exiting=true;Active=false;Completing=false;
            foreach(var a in Participants)if(a!=null)ReturnPlayer(a,entranceReturn);
            registry.ReleaseDungeon(definition.stableId,lease.leaseToken,"world.face.1",out _);CleanupDungeon();yield return null;Finish();
        }
        void CleanupDungeon(){foreach(var obj in spawned)if(obj!=null)Destroy(obj);SceneManager.UnloadSceneAsync(definition.sceneId);}
        public void Finish(){var room=FindFirstObjectByType<DungeonRewardRoom>();if(room!=null&&room.Pending==0&&SceneManager.GetSceneByName("DungeonRewardRoom").isLoaded)SceneManager.UnloadSceneAsync("DungeonRewardRoom");if(Current==this)Current=null;Destroy(gameObject);}
        void Relocate(CombatActor a,Vector3 point)
        {
            a.GetComponent<BasicAttack>()?.Cancel();a.GetComponent<TargetSelection>()?.Select(null);a.GetComponent<Interactor>()?.Cancel();
            if(a.TryGetComponent<RespawnAtAnchor>(out var respawn))respawn.enabled=false;
            if(!a.Motor.TeleportAcrossMaps(point))throw new InvalidOperationException("Dungeon relocation failed at "+point);
            ConfigureCamera(a,point);
            a.GetComponent<InteractionRegistry>()?.RegisterExistingSceneTargets();
        }
        static void ConfigureCamera(CombatActor actor,Vector3 point)
        {
            var camera=FindFirstObjectByType<DiceFree.Core.ExplorationCamera>();if(camera==null)return;
            camera.ConfigureMapBounds(point.x>29000?new Rect(29988,29988,24,24):point.x>19000?new Rect(19994,19994,96,108):new Rect(-56,-44,181,116));
            camera.RecenterForTravel(actor.transform);
        }
        void OnGUI(){if(definition==null||exiting)return;GUI.Box(new Rect(15,80,540,65),Status);if(!loading&&GUI.Button(new Rect(25,115,150,25),"Abandon dungeon"))Abandon();}
        void OnDestroy(){if(Current==this)Current=null;foreach(var a in Participants)if(a!=null){a.Health.Died-=deathHandlers[a];var state=a.GetComponent<RunLoadoutLock>();if(state.Owner==this){state.Locked=false;state.InsideDungeon=false;state.DeathReturnHandler=null;state.Owner=null;}}}
    }
}
