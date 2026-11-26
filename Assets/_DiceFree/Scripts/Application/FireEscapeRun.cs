using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiceFree.ApplicationFlow;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Foundation;
using DiceFree.Persistence;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.Advancement
{
    public sealed class FireEscapeRun:MonoBehaviour
    {
        public static FireEscapeRun Current{get;private set;}
        public FireEscapeSite Site{get;private set;}
        public bool Running{get;private set;}
        public bool Cleared{get;private set;}
        public int Retries{get;private set;}
        public string Feedback{get;private set;}="";
        private CombatActor player;
        private QuestDefinition quest;
        private AttackDefinition hazard;
        private RunLoadoutLock loadout;
        private ExplorationCamera cameraRig;
        private MinimapHud minimap;
        private Rect previousBounds;
        private string previousMapName;
        private Vector3 departure;
        private Scene scene;
        private bool relocated,returning,retrying,inLava;
        private float began,nextLava,nextFire;
        private readonly Dictionary<Renderer,Material> flameMaterials=new();
        private readonly Dictionary<Renderer,ParticleSystem> flameJets=new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetSession()=>Current=null;
        public static bool TryBegin(CombatActor actor,QuestDefinition definition,AttackDefinition damage,Vector3 returnPoint,out string feedback)
        {
            feedback="Finish the current fight or journey first.";
            if(actor==null||!actor.CanAct||actor.InCombat||Current!=null||LoadingScreen.Busy||actor.GetComponent<RunLoadoutLock>()?.Owner!=null||
                actor.Stats.Definition.stableId!="class.mage"||actor.Stats.Level<60||actor.GetComponent<QuestJournal>()?.GetProgress(definition.stableId)?.status!=QuestStatus.Active)return false;
            if(!Application.CanStreamedLevelBeLoaded(FireEscapeSite.SceneName)){feedback="The Ember Walk is unavailable in this build.";return false;}
            var run=new GameObject("Ember Walk solo session").AddComponent<FireEscapeRun>();SceneManager.MoveGameObjectToScene(run.gameObject,actor.gameObject.scene);
            Current=run;run.player=actor;run.quest=definition;run.hazard=damage;run.departure=returnPoint;
            run.cameraRig=FindAnyObjectByType<ExplorationCamera>();run.previousBounds=run.cameraRig.MapBounds;run.minimap=actor.GetComponent<MinimapHud>();run.previousMapName=run.minimap.MapName;
            run.loadout=actor.GetComponent<RunLoadoutLock>()??actor.gameObject.AddComponent<RunLoadoutLock>();run.loadout.Owner=run;
            run.loadout.Locked=run.loadout.InsideDungeon=run.loadout.AbilitiesBlocked=true;run.loadout.DeathReturnHandler=()=>{run.BeginRetry();return true;};
            run.ResetAbilities();actor.Health.Died+=run.BeginRetry;
            if(LoadingScreen.TryLoadAdditive(FireEscapeSite.SceneName,"Entering the Ember Walk",run.Loaded,error=>run.FailLoad(error))){feedback="";return true;}
            run.Release();Current=null;Destroy(run.gameObject);return false;
        }
        private void Loaded(Scene loaded)
        {
            scene=loaded;Site=loaded.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<FireEscapeSite>()).Single();
            if(!Move(Site.entrance))throw new InvalidOperationException("Ember Walk entrance is not navigable.");
            relocated=true;cameraRig.ConfigureMapBounds(Site.mapBounds);cameraRig.RecenterForTravel(player.transform);minimap.MapName="Ember Walk";
            foreach(var flame in Site.flames)if(flame.visual!=null)
            {flameMaterials[flame.visual]=flame.visual.material;flameJets[flame.visual]=FireMageVfx.FlameGate(Site.transform,new Vector3(flame.bounds.center.x,0,flame.bounds.center.y),flame.bounds.size);}
            began=Time.time;Running=true;nextFire=nextLava=0;Feedback="Follow the pale stone path. Amber warns; bright flame burns. Potions are allowed.";
        }
        private void FailLoad(Exception error){Debug.LogError("Ember Walk: "+error.Message);StartCoroutine(Exit());}
        private void Update()
        {
            if(player==null){if(!returning)StartCoroutine(Exit());return;}
            if(!Running||returning||retrying||!player.Alive)return;
            bool fireContact=false;
            foreach(var flame in Site.flames)
            {
                int phase=FireEscapeSite.FlamePhase(Time.time-began,flame.offset);
                if(flame.visual!=null)
                {
                    var material=flameMaterials[flame.visual];var color=phase==1?new Color(1,.20f,.015f):phase==0?new Color(1,.65f,.08f):new Color(.15f,.10f,.07f);
                    material.color=color;if(material.HasProperty("_EmissionColor"))material.SetColor("_EmissionColor",color*(phase==1?2:phase==0?.5f:0));
                    var jets=flameJets[flame.visual];if(phase==1&&!jets.isPlaying)jets.Play();else if(phase!=1&&jets.isPlaying)jets.Stop(true,ParticleSystemStopBehavior.StopEmitting);
                }
                if(phase==1&&flame.bounds.Contains(new Vector2(player.transform.position.x,player.transform.position.z)))fireContact=true;
            }
            bool lava=Site.IsLava(player.transform.position);
            if(lava&&(!inLava||Time.time>=nextLava)){inLava=true;nextLava=Time.time+1;Damage(.15f);}
            else if(!lava)inLava=false;
            if(player.Alive&&fireContact&&Time.time>=nextFire){nextFire=Time.time+1.5f;Damage(.25f);}
            if(player.Alive&&!retrying&&Vector3.Distance(player.transform.position,Site.finish)<1.5f)TryFinish();
        }
        public DamageResult Damage(float maxHpFraction)
        {
            if(!Running||returning||retrying||!player.Alive)return default;
            float raw=player.Health.Maximum*maxHpFraction;
            float defense=DefenseMath.Effective(player.Stats.Defense(hazard.channel),player.Stats.DefenseModifiers(hazard.channel),default);
            var elemental=player.Stats.ResolveResistance(hazard.element,ElementalContext.Damage);
            float after=raw*DefenseMath.DamageMultiplier(defense,player.Stats.Definition.tuning);
            return player.Health.ApplyPacket(null,new DamageResult{raw=raw,baseRaw=raw,channel=hazard.channel,element=hazard.element,defense=defense,afterDefense=after,resistance=elemental.effective,elementalResistance=elemental,mitigated=after*(1-elemental.effective)*player.Stats.IncomingDamageMultiplier(hazard.channel)});
        }
        private void BeginRetry(){if(!returning&&!retrying&&Running)StartCoroutine(Retry());}
        private IEnumerator Retry()
        {
            retrying=true;yield return null;
            if(player==null||returning){retrying=false;yield break;}
            ResetAbilities();
            if(!player.Alive){if(!player.GetComponent<RespawnAtAnchor>().ReturnAtInstanceExit(Site.entrance)){Feedback="Retry entrance unavailable. Leave the trial to return safely.";retrying=false;yield break;}}
            else Move(Site.entrance);
            player.Health.Restore();var resources=player.GetComponent<ActorResourceController>();resources.Restore(ResourceIds.Mana,resources.Maximum(ResourceIds.Mana));
            inLava=false;nextLava=nextFire=0;Retries++;retrying=false;cameraRig.RecenterForTravel(player.transform);Feedback="Try again. HP and Mana restored; spent potions stay spent.";
        }
        public bool TryFinish()
        {
            if(!Running||returning||retrying||player==null||!player.Alive||Vector3.Distance(player.transform.position,Site.finish)>=1.5f)return false;
            var journal=player.GetComponent<QuestJournal>();var before=journal.CaptureState();journal.CreditEscort(FireEscapeSite.TrialId);
            if(journal.GetProgress(quest.stableId)?.status!=QuestStatus.ReadyToTurnIn||!player.GetComponent<ManifestationPersistence>().Flush())
            {journal.RestoreState(before);Feedback="The trial could not be saved. Remain at the exit and retry.";return false;}
            Cleared=true;Running=false;Feedback="Ember Walk complete. Return to your mentor.";StartCoroutine(Exit());return true;
        }
        public void Abandon(){if(!returning)StartCoroutine(Exit());}
        private IEnumerator Exit()
        {
            if(returning)yield break;returning=true;Running=false;
            if(player!=null)
            {
                ResetAbilities();
                if(relocated)
                {
                    bool moved=player.Alive?Move(departure):player.GetComponent<RespawnAtAnchor>().ReturnAtInstanceExit(departure);
                    if(!moved&&!player.GetComponent<RespawnAtAnchor>().LoadAtAnchor(null))Debug.LogError("Ember Walk return point unavailable.");
                }
                cameraRig.ConfigureMapBounds(previousBounds);cameraRig.RecenterForTravel(player.transform);minimap.MapName=previousMapName;
                Release();player.GetComponent<ManifestationPersistence>()?.Flush();
            }
            if(scene.isLoaded)yield return SceneManager.UnloadSceneAsync(scene);Destroy(gameObject);
        }
        private bool Move(Vector3 point)
        {
            player.GetComponent<BasicAttack>()?.Cancel();player.GetComponent<SkillTargetingController>()?.Cancel();player.GetComponent<Interactor>()?.Cancel();player.GetComponent<TargetSelection>()?.Select(null);
            foreach(var c in player.GetComponents<MonoBehaviour>())if(c is IQueuedOrders orders)orders.ClearOrders();
            return player.Motor.TeleportAcrossMaps(point);
        }
        private void ResetAbilities(){if(player!=null)foreach(var c in player.GetComponents<MonoBehaviour>())if(c is IManifestationSessionState state)state.ResetForManifestationLoad();}
        private void Release()
        {
            if(player!=null)player.Health.Died-=BeginRetry;
            if(loadout!=null&&loadout.Owner==this){loadout.Locked=loadout.InsideDungeon=loadout.AbilitiesBlocked=false;loadout.Owner=null;loadout.DeathReturnHandler=null;}
        }
        private void OnDestroy(){Release();foreach(var material in flameMaterials.Values)if(material!=null)Destroy(material);if(Current==this)Current=null;}
        private static Rect Panel=>new(15,80,430,132);
        private bool Covers(Vector2 p)=>Panel.Contains(new Vector2(p.x,Screen.height-p.y));
        private void OnEnable()=>HudPointerBlocker.OverlayCovers+=Covers;
        private void OnDisable()=>HudPointerBlocker.OverlayCovers-=Covers;
        private void OnGUI()
        {
            if(!Running||LoadingScreen.Busy||HudPointerBlocker.ModalOpen)return;
            GUI.Box(Panel,"Ember Walk — reach the far beacon");GUI.Label(new Rect(27,108,405,55),Feedback,new GUIStyle(GUI.skin.label){wordWrap=true});
            GUI.Label(new Rect(27,166,270,30),"Walk carefully · healing potions allowed");if(GUI.Button(new Rect(320,169,110,28),"Leave trial"))Abandon();
        }
    }
}
