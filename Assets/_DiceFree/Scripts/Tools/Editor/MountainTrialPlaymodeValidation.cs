using System;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class MountainTrialPlaymodeValidation
    {
        private const string Prefix = "DiceFree.MountainTrialSmoke.";
        private const string SaveRootVariable = "DICEFREE_EDITOR_SAVE_ROOT";
        private static bool ticking;
        private static float deadline;

        static MountainTrialPlaymodeValidation() =>
            EditorApplication.playModeStateChanged += OnPlayState;

        [CliCommand("dicefree.mountain.playmode-test",
            "Test isolated fresh Novice at L9/L10, separate mountain travel, advancement and return.",
            Tags = new[] { "tests", "world", "advancement", "art" })]
        private static object Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Mountain test requires idle Edit Mode.");
            SessionState.SetBool(Prefix+"prior-fast",EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetInt(Prefix+"prior-options",(int)EditorSettings.enterPlayModeOptions);
            EditorSettings.enterPlayModeOptionsEnabled=false;
            string root=Path.Combine(Path.GetTempPath(),"DiceFree-Mountain-"+Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            SessionState.SetString(Prefix+"root",root);
            SessionState.SetString(Prefix+"status","running");
            SessionState.SetString(Prefix+"error","");
            SessionState.SetInt(Prefix+"phase",0);
            EditorSceneManager.OpenScene(AdvancementAuthoring.StartMenuScenePath);
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.mountain.playmode-status",
            "Read the isolated mountain trial travel smoke result.",
            Tags = new[] { "tests", "world", "advancement" })]
        private static object Status()
        {
            string status=SessionState.GetString(Prefix+"status","idle");
            return new {status,success=status=="passed",
                finalMarker=status=="passed"?"DICEFREE_MOUNTAIN_PLAYMODE_OK":"",
                phase=SessionState.GetInt(Prefix+"phase",0),
                error=SessionState.GetString(Prefix+"error","")};
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (SessionState.GetString(Prefix+"status","")!="running")
            {
                if (state==PlayModeStateChange.EnteredEditMode) Cleanup();
                return;
            }
            if (state==PlayModeStateChange.EnteredPlayMode)
            {
                deadline=Time.realtimeSinceStartup+70f;
                if(!ticking)
                {
                    ticking=true;
                    EditorApplication.update+=Tick;
                }
            }
            else if(state==PlayModeStateChange.EnteredEditMode)
            {
                StopTick();
                Cleanup();
            }
        }

        private static void Tick()
        {
            try
            {
                if (!EditorApplication.isPlaying)return;
                if (Time.realtimeSinceStartup>=deadline)
                {
                    var probe=UnityEngine.Object.FindAnyObjectByType<MountainTrialTraveller>();
                    var probeMap=SceneManager.GetSceneByName("NoviceMountainTrial");
                    Require(false, "Mountain Play Mode timed out in phase " +
                        SessionState.GetInt(Prefix+"phase",0) +
                        "; diagnostic=" + (probe!=null?probe.TravelDiagnostic:"missing traveller") +
                        "; loaded=" + probeMap.isLoaded +
                        "; position=" + (probe!=null?probe.transform.position.ToString():"unknown"));
                }
                int phase=SessionState.GetInt(Prefix+"phase",0);
                if (phase==0)
                {
                    var menu=UnityEngine.Object.FindAnyObjectByType<StartMenuController>();
                    if(menu==null || !menu.ReadyForNewEcho)return;
                    Require(menu.TryStartNewEcho(),"Could not create a fresh Novice.");
                    SessionState.SetInt(Prefix+"phase",1);
                    return;
                }

                var controller=UnityEngine.Object.FindAnyObjectByType<AdvancementController>();
                if(controller==null || controller.GetComponent<ManifestationPersistence>()?.Ready!=true)return;
                var traveller=controller.GetComponent<MountainTrialTraveller>();
                var xp=controller.GetComponent<ExperienceProgression>();
                Require(traveller!=null && xp!=null,"Mountain traveller/progression missing from Cornberg player.");

                if(phase==1)
                {
                    var door=UnityEngine.Object.FindAnyObjectByType<MountainAdvancementGate>();
                    Require(door!=null,"Mountain gate missing at runtime.");
                    xp.RestoreState(9,0);
                    Require(!traveller.Eligible,"Novice L9 unexpectedly admitted to mountain.");
                    xp.RestoreState(10,0);
                    Require(traveller.Eligible,"Novice L10 cannot enter advancement mountain.");
                    traveller.Enter("NoviceMountainTrial");
                    SessionState.SetInt(Prefix+"phase",2);
                    return;
                }
                if(phase==2)
                {
                    if(traveller.Busy || !traveller.InsideTrial)return;
                    Require(SceneManager.GetSceneByName("NoviceMountainTrial").isLoaded,
                        "Trial scene is not loaded additively.");
                    Require(controller.transform.position.x>9000,
                        "Player did not teleport to physically separate trial map.");
                    var physical=controller.Definitions.FirstOrDefault(d=>
                        d.targetClass!=null && d.targetClass.stableId==AdvancementAuthoring.PhysicalId);
                    Require(physical!=null && controller.CanAdvance(physical),
                        "Level-10 Novice cannot choose Physically Blessed inside trial.");
                    Require(controller.TryAdvance(physical),
                        "Trial advancement fork failed: "+controller.Feedback);
                    Require(controller.CurrentClassId==AdvancementAuthoring.PhysicalId &&
                        xp.Level==1,"Advancement trial did not retain level-reset class fork.");
                    Require(traveller.Exit(),"Cannot request return to Cornberg.");
                    SessionState.SetInt(Prefix+"phase",3);
                    return;
                }
                if (phase==3)
                {
                    if(traveller.Busy || traveller.InsideTrial ||
                       SceneManager.GetSceneByName("NoviceMountainTrial").isLoaded)return;
                    Require(controller.transform.position.x<0,
                        "Player did not return to Cornberg.");
                    Require(controller.GetComponent<ManifestationPersistence>().Flush(),
                        "Advancement save failed after returning.");
                    SessionState.SetString(Prefix+"status","passed");
                    SessionState.SetInt(Prefix+"phase",4);
                    Debug.Log("DICEFREE_MOUNTAIN_PLAYMODE_OK: L9 locked, L10 entered separate scene, forked, returned, saved.");
                    StopTick();
                    EditorApplication.ExitPlaymode();
                }
            }
            catch(Exception e){Fail(e);}
        }

        private static void Require(bool condition,string message)
        {
            if(!condition)throw new InvalidOperationException(message);
        }

        private static void Fail(Exception e)
        {
            if(SessionState.GetString(Prefix+"status","")!="running")return;
            SessionState.SetString(Prefix+"status","failed");
            SessionState.SetString(Prefix+"error",e.ToString());
            Debug.LogError("MOUNTAIN_TEST_FAILED: "+e);
            StopTick();
            if(EditorApplication.isPlaying)EditorApplication.ExitPlaymode();
            else Cleanup();
        }

        private static void StopTick()
        {
            if(!ticking)return;
            ticking=false;
            EditorApplication.update-=Tick;
        }

        private static void Cleanup()
        {
            EditorSettings.enterPlayModeOptions=
                (EnterPlayModeOptions)SessionState.GetInt(Prefix+"prior-options",
                    (int)EditorSettings.enterPlayModeOptions);
            EditorSettings.enterPlayModeOptionsEnabled=
                SessionState.GetBool(Prefix+"prior-fast",EditorSettings.enterPlayModeOptionsEnabled);
            PersistenceTestGuard.ClearValidationOverrides();
            Environment.SetEnvironmentVariable(SaveRootVariable,null);
            string dir=SessionState.GetString(Prefix+"root","");
            try{if(!string.IsNullOrEmpty(dir)&&Directory.Exists(dir))Directory.Delete(dir,true);}
            catch{/* best effort */ }
        }
    }
}
