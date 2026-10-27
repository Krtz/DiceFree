using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Dungeons;
using DiceFree.Input;
using DiceFree.Ironpine;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class OctoberPlaytestValidation
    {
        const string Key="DiceFree.OctoberPlaytest.";
        static IEnumerator flow;
        static CombatActor player;
        static DungeonSlime observedBoss;
        static float lastBumpRaw,lastSlamRaw;
        static double deadline;
        static int walked,converted;
        static OctoberPlaytestValidation()=>EditorApplication.playModeStateChanged+=Changed;

        static void Need(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        [CliCommand("dicefree.october-playtest.validate","Validate approved damage endpoints, compass placement and v3 settings migration.")]
        public static object ValidateData()
        {
            var constructor=typeof(GameplayPreferences).GetConstructor(BindingFlags.NonPublic|BindingFlags.Instance,
                null,new[]{typeof(string)},null);
            string directory=Path.Combine(Path.GetTempPath(),"DiceFree-Compass-"+Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory,"gameplay.json"),
                "{\"version\":3,\"autoRetaliate\":false,\"outOfRangeSkillBehavior\":0,\"showCastRange\":false,\"selfHealthBar\":2,\"allyHealthBar\":1,\"enemyHealthBar\":0,\"minimapOrientation\":1}");
            var preferences=(GameplayPreferences)constructor.Invoke(new object[]{directory});
            Need(!preferences.AutoRetaliate && !preferences.ShowCastRange &&
                preferences.MinimapOrientation==MinimapOrientation.CameraUp &&
                preferences.MinimapCompassCorner==MinimapCompassCorner.BottomLeft &&
                preferences.QuickCastMode==QuickCastMode.Reticle &&
                preferences.SelfHealthBar==OverheadHealthVisibility.Never,"v3 settings migration lost an existing preference.");
            var map=new Rect(30,40,240,210);
            foreach(MinimapCompassCorner corner in Enum.GetValues(typeof(MinimapCompassCorner)))
            {
                preferences.SetMinimapCompassCorner(corner);
                var reloaded=(GameplayPreferences)constructor.Invoke(new object[]{directory});
                Need(reloaded.MinimapCompassCorner==corner && !reloaded.AutoRetaliate &&
                    reloaded.MinimapOrientation==MinimapOrientation.CameraUp,"Compass preference did not persist.");
                Rect bounds=DiceFree.UI.MinimapCompass.BoundsFor(map,corner);
                Need(corner==MinimapCompassCorner.Off?bounds.width==0:
                    map.Contains(bounds.min)&&map.Contains(bounds.max),"Compass escapes the map corner.");
            }
            foreach(QuickCastMode mode in Enum.GetValues(typeof(QuickCastMode)))
            {
                preferences.SetQuickCastMode(mode);
                var reloaded=(GameplayPreferences)constructor.Invoke(new object[]{directory});
                Need(reloaded.QuickCastMode==mode,"Quickcast mode did not persist.");
            }
            Need(Vector2.Distance(DiceFree.UI.MinimapCompass.DirectionOnMap(Vector3.forward,Vector3.right),
                Vector2.left)<.001f,"Camera-up compass north is incorrect.");
            Directory.Delete(directory,true);
            int count=0;
            foreach(string guid in AssetDatabase.FindAssets("t:AttackDefinition",new[]{"Assets/_DiceFree/Settings/Ironpine"}))
            {
                var attack=AssetDatabase.LoadAssetAtPath<AttackDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                Vector2 expected;
                if(attack.name.StartsWith("Quillrat-"))expected=new Vector2(5.1f*4,5.1f*5);
                else if(attack.name.StartsWith("DarkWolf-"))expected=new Vector2(6.9f*3.5f,6.9f*5);
                else if(attack.name.StartsWith("AlphaWolf-"))expected=new Vector2(9f*6,9f*8);
                else continue;
                Need(Vector2.Distance(attack.RawDamageRange(default),expected)<.002f,"Incorrect damage endpoints: "+attack.name);
                count++;
            }
            Need(count==9,"Wildlife attack inventory.");
            return new{marker="DICEFREE_OCTOBER_PLAYTEST_DATA_OK",attackDefinitions=count,compassChoices=5,settingsMigration=true};
        }

        [CliCommand("dicefree.october-playtest.playmode-test","Test Trimery walking/revive/materials, compass, and level-24 boss pursuit/damage in isolated saves.")]
        public static object Start()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode,"Exit Play Mode first.");
            ValidateData();
            SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);
            SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
            SessionState.SetBool(Key+"cleanup",true);
            SessionState.SetBool(Key+"resume",false);
            SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");
            string root=Path.Combine(Path.GetTempPath(),"DiceFree-OctoberPlaytest-"+Guid.NewGuid().ToString("N"));
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            EditorSettings.enterPlayModeOptionsEnabled=false;
            EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            EditorApplication.EnterPlaymode();
            return Status();
        }
        [CliCommand("dicefree.october-playtest.status","Read the focused Oct 13 gameplay validation.")]
        public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),
            error=SessionState.GetString(Key+"error",""),walked,converted,
            bumpRaw=lastBumpRaw,slamRaw=lastSlamRaw,phase=SessionState.GetString(Key+"phase","")};
        [CliCommand("dicefree.october-playtest.resume","Continue the boss test after the town screenshot checkpoint.")]
        public static object Resume(){SessionState.SetBool(Key+"resume",true);return Status();}
        static void Changed(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetString(Key+"status","")=="running")
            {
                UnityEngine.Application.runInBackground=true;Time.timeScale=2;
                walked=converted=0;lastBumpRaw=lastSlamRaw=0;
                deadline=EditorApplication.timeSinceStartup+240;
                flow=Flow();EditorApplication.update+=Tick;UnityEngine.Application.logMessageReceived+=Log;
            }
            if(state==PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Key+"cleanup",false))
            {
                SessionState.SetBool(Key+"cleanup",false);
                EditorApplication.update-=Tick;UnityEngine.Application.logMessageReceived-=Log;
                EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);
                EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);
                PersistenceTestGuard.ClearValidationOverrides();
                string scene=SessionState.GetString(Key+"scene","");
                if(!string.IsNullOrEmpty(scene))EditorSceneManager.OpenScene(scene);
            }
        }
        static void Log(string message,string stack,LogType type)
        {
            if(type==LogType.Error || type==LogType.Exception)Finish(false,message);
        }
        static void Tick()
        {
            if(!EditorApplication.isPlaying)return;
            try
            {
                Need(EditorApplication.timeSinceStartup<deadline,"Focused gameplay test timed out.");
                if(flow!=null&&!flow.MoveNext())Finish(true,null);
            }
            catch(Exception error){Finish(false,error.ToString());}
        }
        static IEnumerator Flow()
        {
            SessionState.SetString(Key+"phase","initialize");
            while(player==null)
            {player=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();yield return null;}
            while(!player.Motor.Ready || !player.GetComponent<ManifestationPersistence>().Ready)yield return null;
            player.GetComponent<TraversalInput>().enabled=false;
            player.GetComponent<CombatInput>().enabled=false;
            GameplayPreferences.Current.SetAutoRetaliate(false);
            player.GetComponent<ExperienceProgression>().RestoreState(24,0);player.Health.Restore();
            Need(player.Stats.Level==24,"Level-24 Novice fixture.");
            foreach(var animal in Object.FindObjectsByType<IronpineWildlife>())
            {
                Vector2 expected=animal.Kind==WildlifeKind.Quillrat?new Vector2(20.4f,25.5f):
                    animal.Kind==WildlifeKind.DarkWolf?new Vector2(36.225f,51.75f):
                    animal.Kind==WildlifeKind.AlphaWolf?new Vector2(109.35f,145.8f):Vector2.zero;
                if(expected==Vector2.zero)continue;
                Need(Vector2.Distance(animal.GetComponent<BasicAttack>().DamageRange,expected)<.02f,
                    "Runtime bite/quill range mismatch: "+animal.name);
            }
            var town=Object.FindObjectsByType<TrimeryArtSlot>().Where(slot=>slot.LoadedLicensedArt).ToArray();
            Need(town.Length>0,"Installed local town art did not initialize.");
            foreach(var slot in town)
            {
                converted+=slot.ConvertedMaterials;
                var art=slot.transform.Find("Licensed "+slot.Resource+" (local)");
                Need(art!=null && art.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials)
                    .All(m=>m!=null&&m.shader.isSupported&&m.shader.name.StartsWith("Universal Render Pipeline/")),
                    "Legacy/pink town material: "+slot.Resource);
            }
            Need(converted>0,"Legacy timber material was not converted.");
            SessionState.SetString(Key+"phase","town walking");
            Need(player.Motor.Teleport(new Vector3(248,0,161)),"Town south-gate placement.");
            foreach(var point in new[]{new Vector3(226,0,176),new Vector3(232,0,185),new Vector3(239,0,192),
                new Vector3(235,0,205),new Vector3(248,0,204),new Vector3(268,0,186),new Vector3(260,0,183),
                new Vector3(260,0,178),new Vector3(237,0,178.5f)})
            {
                Need(UnityEngine.AI.NavMesh.SamplePosition(point,out var sample,.75f,UnityEngine.AI.NavMesh.AllAreas),
                    "Street point is not on navigation: "+point);
                Need(player.Motor.MoveTo(sample.position),"Street path rejected: "+point);
                float end=Time.time+24f;
                while(Vector3.Distance(player.transform.position,sample.position)>1.35f && Time.time<end)yield return null;
                Need(Vector3.Distance(player.transform.position,sample.position)<=1.35f,"Actual player could not walk to "+point+"; player="+player.transform.position+"; remaining="+player.GetComponent<UnityEngine.AI.NavMeshAgent>().remainingDistance+"; path="+player.GetComponent<UnityEngine.AI.NavMeshAgent>().pathStatus);
                walked++;
            }
            SessionState.SetString(Key+"phase","revive");
            var anchor=Object.FindObjectsByType<ResurrectionAnchor>().Single(a=>a.StableId=="anchor.trimery");
            Need(player.Motor.Teleport(anchor.ActivationPosition+Vector3.back*3.8f),"Visible beacon approach.");
            Need(anchor.CanActivateFrom(player.transform.position),"Beacon activation reach.");
            float activationEnd=Time.time+2;
            var respawn=player.GetComponent<RespawnAtAnchor>();
            while(respawn.AnchorId!="anchor.trimery" && Time.time<activationEnd)yield return null;
            Need(respawn.AnchorId=="anchor.trimery","Visible stone did not activate automatically.");
            player.Health.ApplyDamage(null,new DamageResult{mitigated=player.Health.Maximum*2});
            while(!respawn.CanReturn)yield return null;
            Need(respawn.Return(),"Death return to Trimery failed.");
            Need(player.Alive && Vector3.Distance(player.transform.position,anchor.transform.position)<1 &&
                respawn.AnchorId=="anchor.trimery","Trimery death return destination.");
            player.Health.Restore();
            var map=player.GetComponent<DiceFree.UI.MinimapHud>();
            GameplayPreferences.Current.SetMinimapOrientation(MinimapOrientation.NorthUp);
            GameplayPreferences.Current.SetMinimapCompassCorner(MinimapCompassCorner.BottomLeft);
            Need(map!=null,"Minimap missing.");
            player.Motor.Teleport(new Vector3(248,0,184));
            Object.FindAnyObjectByType<DiceFree.Core.ExplorationCamera>()?.RecenterForTravel(player.transform);
            for(int i=0;i<8;i++)yield return null;
            SessionState.SetString(Key+"status","visual_ready");SessionState.SetString(Key+"phase","town screenshot");
            while(!SessionState.GetBool(Key+"resume",false))yield return null;
            SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"phase","boss pursuit");
            player.Stats.SetEncounterMaximumHp(100000);player.Health.Restore();
            var portal=Object.FindObjectsByType<DungeonPortal>().Single(p=>p.isActiveAndEnabled);
            Need(player.Motor.Teleport(portal.ApproachPosition+Vector3.back*1.5f),"Dungeon portal approach.");
            portal.Interact(player);
            while(SlimeDungeonRun.Current==null || !SlimeDungeonRun.Current.StagingReady)yield return null;
            var run=SlimeDungeonRun.Current;
            Need(run.StartNow(player),"Dungeon Start Now failed.");
            player.Health.Damaged+=OnPlayerDamaged;
            foreach(var role in new[]{SlimeRole.Miniboss,SlimeRole.Boss})
            {
                var boss=Object.FindObjectsByType<DungeonSlime>().Single(s=>s.Role==role);
                observedBoss=boss;lastBumpRaw=lastSlamRaw=0;
                Need(player.Motor.Teleport(boss.transform.position+Vector3.back*10f),"Boss approach fixture.");
                yield return null;yield return null;
                Physics.SyncTransforms();
                var capsule=boss.GetComponent<CapsuleCollider>();
                Need(Mathf.Abs(capsule.bounds.extents.x-boss.Actor.Radius)<.05f,"Scaled boss collider exceeds combat radius.");
                Vector3 before=boss.transform.position;
                float limit=Time.time+14f, nextTrace=Time.time;
                string trace="";
                while((boss.PursuitOrders==0 || boss.BossBumpHits==0) && Time.time<limit)
                {
                    if(Time.time>=nextTrace)
                    {
                        var nav=boss.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        trace+=" t="+Mathf.RoundToInt(Time.time)+" p="+boss.transform.position+
                            " dst="+Vector3.Distance(boss.transform.position,player.transform.position).ToString("F1")+
                            " busy="+boss.Busy+" state="+boss.PursuitState+
                            " order="+boss.PursuitOrders+" bumps="+boss.BossBumpAttempts+
                            " nav="+nav.isOnNavMesh+"/"+nav.hasPath+"/"+nav.isStopped+
                            " speed="+nav.speed.ToString("F1")+" radius="+nav.radius.ToString("F1")+
                            " scale="+boss.transform.lossyScale.x.ToString("F1")+
                            " vel="+nav.velocity.magnitude.ToString("F1")+
                            " desired="+nav.desiredVelocity.magnitude.ToString("F1")+
                            " remaining="+nav.remainingDistance.ToString("F1")+
                            " destination="+nav.destination+
                            " separations="+boss.GetComponent<ActorFootprintSeparation>().Corrections+";";
                        nextTrace+=1f;
                    }
                    yield return null;
                }
                Need(boss.PursuitTarget==player && boss.PursuitOrders>0 &&
                    Vector3.Distance(before,boss.transform.position)>.25f && boss.BossBumpHits>0,
                    role+" failed to pursue and attack level-24 Novice; "+boss.PursuitState+
                    "; boss="+boss.transform.position+" player="+player.transform.position+
                    "; orders="+boss.PursuitOrders+" bumps="+boss.BossBumpHits+"/"+boss.BossBumpAttempts+
                    "; bumpState="+boss.BossBumpState+" busy="+boss.Busy+
                    "; agent="+boss.GetComponent<UnityEngine.AI.NavMeshAgent>().pathStatus+
                    "; velocity="+boss.GetComponent<UnityEngine.AI.NavMeshAgent>().velocity+
                    "; trace="+trace);
                float expectedBump=DamageResolver.Calculate(boss.Actor.Stats,player.Stats,boss.Actor.Stats.Definition.basicAttack).baseRaw*boss.DamageMultiplier;
                Need(Mathf.Abs(lastBumpRaw-expectedBump)<.02f,"Boss melee damage multiplier incorrect.");
                if(role==SlimeRole.Boss)
                {
                    float specialLimit=Time.time+18f;
                    while(lastSlamRaw==0 && Time.time<specialLimit)yield return null;
                    Need(lastSlamRaw>0,"Normal boss Slam did not damage the player.");
                    float expectedSlam=DamageResolver.Calculate(boss.Actor.Stats,player.Stats,run.Tuning.slam).baseRaw*10f;
                    Need(Mathf.Abs(lastSlamRaw-expectedSlam)<.02f,"Normal boss Slam is not 10x.");
                }
                boss.Actor.Health.ApplyDamage(player,new DamageResult{mitigated=boss.Actor.Health.Maximum*2});
                if(role==SlimeRole.Miniboss)run.SolvePuzzle(false);
                yield return null;
            }
            player.Health.Damaged-=OnPlayerDamaged;
            SessionState.SetString(Key+"phase","complete");
        }
        static void OnPlayerDamaged(CombatActor source,DamageResult result)
        {
            if(observedBoss==null || source!=observedBoss.Actor)return;
            if(result.baseRaw<=0)return;
            float expected=DamageResolver.Calculate(source.Stats,player.Stats,source.Stats.Definition.basicAttack).baseRaw*observedBoss.DamageMultiplier;
            if(Mathf.Abs(result.baseRaw-expected)<.02f)lastBumpRaw=result.baseRaw;
            else lastSlamRaw=result.baseRaw;
        }
        static void Finish(bool passed,string error)
        {
            if(flow==null)return;
            flow=null;EditorApplication.update-=Tick;UnityEngine.Application.logMessageReceived-=Log;
            if(player!=null)player.Health.Damaged-=OnPlayerDamaged;
            SessionState.SetString(Key+"status",passed?"passed":"failed");SessionState.SetString(Key+"error",error??"");
            if(passed)Debug.Log("DICEFREE_OCTOBER_PLAYTEST_PLAYMODE_OK");
            Time.timeScale=1;EditorApplication.ExitPlaymode();
        }
    }
}
