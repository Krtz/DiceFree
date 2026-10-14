using System;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Development;
using DiceFree.Foundation;
using DiceFree.Items;
using DiceFree.Persistence;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class DevelopmentRoomValidation
    {
        const string Key="DiceFree.DevelopmentValidation.";
        static float deadline;
        static bool ticking;
        static DevelopmentRoomValidation()=>EditorApplication.playModeStateChanged+=OnPlayState;

        [CliCommand("dicefree.development.playmode-test","Validate menu entry, isolated test Echo, class/item controls, state-preserving World 2 round trip and menu return.")]
        public static object Begin()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            SessionState.SetBool(Key+"old-fast",EditorSettings.enterPlayModeOptionsEnabled);
            EditorSettings.enterPlayModeOptionsEnabled=false;
            string root=Path.Combine(Path.GetTempPath(),"DiceFree-DevRoom-"+Guid.NewGuid().ToString("N"));
            var sentinel=new EchoSave{revision=1,writtenUtc=DateTime.UtcNow.ToString("O")};
            EchoIdentity.WriteName(sentinel,"Normal Echo sentinel");
            var store=new LocalEchoStore(root);store.Commit(sentinel);
            SessionState.SetString(Key+"normal-bytes",File.ReadAllText(store.Path));
            SessionState.SetString(Key+"root",root);
            SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");
            SessionState.SetInt(Key+"phase",0);
            SessionState.SetBool(Key+"needs-cleanup",true);
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.development.playmode-status","Read the development room and World 2 transition validation.")]
        public static object Status()=>new{
            status=SessionState.GetString(Key+"status","idle"),
            phase=SessionState.GetInt(Key+"phase",0),
            marker=SessionState.GetString(Key+"status","")=="passed"?"DICEFREE_DEVELOPMENT_ROOM_PLAYMODE_OK":"",
            error=SessionState.GetString(Key+"error","")
        };

        static void OnPlayState(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredEditMode){if(SessionState.GetBool(Key+"needs-cleanup",false))Cleanup();return;}
            if(SessionState.GetString(Key+"status","")!="running")return;
            if(state==PlayModeStateChange.EnteredPlayMode){
                deadline=Time.realtimeSinceStartup+45;
                ticking=true;EditorApplication.update+=Tick;
                Application.logMessageReceived+=OnLog;
            }
        }

        static void Check(bool ok,string text){if(!ok)throw new InvalidOperationException("Development room: "+text);}
        static void Tick()
        {
            if(!EditorApplication.isPlaying)return;
            try
            {
                Check(Time.realtimeSinceStartup<deadline,"validation timed out in phase "+SessionState.GetInt(Key+"phase",0));
                int phase=SessionState.GetInt(Key+"phase",0);
                if(phase==0){
                    var menu=UnityEngine.Object.FindAnyObjectByType<StartMenuController>();
                    if(menu==null||!menu.HasLoadedProfile)return;
                    SessionState.SetInt(Key+"phase",1);
                    Check(menu.TryOpenDevelopmentRoom(),"menu could not open the dev room");
                    return;
                }
                if(phase==4){
                    var menu=UnityEngine.Object.FindAnyObjectByType<StartMenuController>();
                    if(menu==null||!menu.HasLoadedProfile)return;
                    Check(!DevelopmentSession.Active && DevelopmentRoomController.Current==null,"dev session leaked into the main menu");
                    string root=SessionState.GetString(Key+"root","");
                    Check(File.ReadAllText(new LocalEchoStore(root).Path)==SessionState.GetString(Key+"normal-bytes",""),"normal Echo save was changed");
                    var saved=new LocalEchoStore(Path.Combine(root,"DevelopmentRoom")).Load();
                    Check(EchoIdentity.ReadName(saved)=="Developer","test Echo name was not saved");
                    SessionState.SetString(Key+"status","passed");
                    Debug.Log("DICEFREE_DEVELOPMENT_ROOM_PLAYMODE_OK: menu, isolated saves, real classes/items, same-player World2 round trip, health/resource continuity and clean menu return.");
                    Finish();return;
                }
                var room=DevelopmentRoomController.Current;
                if(room==null||room.Map==null||room.Frames<5||room.Busy)return;
                var player=room.player;
                var persistence=player.GetComponent<ManifestationPersistence>();
                if(!persistence.Ready)return;
                if(phase==1){
                    string root=SessionState.GetString(Key+"root","");
                    Check(Path.GetDirectoryName(persistence.SavePath)==Path.Combine(root,"DevelopmentRoom"),"test Echo is not isolated");
                    Check(persistence.EchoName=="Developer","test Echo name is wrong");
                    Check(CombatActor.All.Count(a=>a.Stats.Definition.stableId=="dev.practice-target")==3,"training targets are missing");
                    Check(room.SelectClass(AdvancementAuthoring.PhysicalId),"Physical class control failed");
                    Check(room.SelectClass(AdvancementAuthoring.MagicalId),"Magical class control failed");
                    Check(room.SetLevel(42)&&player.Stats.Level==42,"level control failed");
                    room.RestorePlayer();
                    var inv=player.GetComponent<CarriedInventory>();
                    var item=inv.Definitions.First(value=>value!=null&&(value.allowedClassIds==null||value.allowedClassIds.Length==0));
                    var owned=inv.Grant(item.stableId);
                    Check(player.GetComponent<Equipment>().Equip(owned.instanceId,item.slot),"test equipment could not be equipped");
                    SessionState.SetString(Key+"item",owned.instanceId);
                    player.GetComponent<GoldWallet>().Grant(123);
                    player.Health.SetEncounterHp(player.Health.Maximum*.5f);
                    SessionState.SetFloat(Key+"hp-start",player.Health.Current);
                    SessionState.SetFloat(Key+"hp-regen",player.Stats.Regeneration*player.Stats.HealingReceived);
                    var resources=player.GetComponent<ActorResourceController>();
                    Check(resources.Has(ResourceIds.Mana),"Magical test class lacks Mana");
                    Check(resources.TrySpend(ResourceIds.Mana,resources.Maximum(ResourceIds.Mana)*.5f),"test Mana setup failed");
                    SessionState.SetFloat(Key+"mana-start",resources.Current(ResourceIds.Mana));
                    SessionState.SetFloat(Key+"time-start",Time.time);
                    SessionState.SetFloat(Key+"mana-regen",resources.Profiles.Single(p=>p.classId==player.Stats.Definition.stableId&&p.resource.stableId==ResourceIds.Mana).Regeneration(player.Stats.Attributes));
                    SessionState.SetString(Key+"player",player.GetEntityId().ToString());
                    SessionState.SetInt(Key+"phase",2);
                    Check(room.Travel("World2TransitionTest"),"World2 travel was rejected");
                    deadline=Time.realtimeSinceStartup+45;return;
                }
                if(phase==2){
                    if(room.CompletedTransitions<1)return;
                    Check(room.Map.worldId=="world.test.2" &&
                        !UnityEngine.SceneManagement.SceneManager.GetSceneByName("DevelopmentRoom").isLoaded,"World1 map was not unloaded");
                    CheckState(room);
                    SessionState.SetInt(Key+"phase",3);
                    Check(room.Travel("DevelopmentRoom"),"return travel was rejected");return;
                }
                if(phase==3){
                    if(room.CompletedTransitions<2)return;
                    Check(room.Map.worldId=="world.test.1" &&
                        !UnityEngine.SceneManagement.SceneManager.GetSceneByName("World2TransitionTest").isLoaded,"World2 map was not unloaded");
                    CheckState(room);
                    Check(UnityEngine.Object.FindObjectsByType<TraversalInput>(FindObjectsSortMode.None).Length==1,"return created a duplicate player");
                    Check(player.Motor.Ready,"player agent was not rebound to the new map");
                    SessionState.SetInt(Key+"phase",4);
                    Check(player.GetComponent<DiceFree.Dungeons.ReturnToStartMenu>().Return(),"main menu return failed");
                }
            }
            catch(Exception error){SessionState.SetString(Key+"status","failed");SessionState.SetString(Key+"error",error.ToString());Debug.LogException(error);Finish();}
        }

        static void CheckState(DevelopmentRoomController room)
        {
            var player=room.player;
            Check(player.GetEntityId().ToString()==SessionState.GetString(Key+"player",""),"travel replaced the player");
            Check(player.Stats.Definition.stableId==AdvancementAuthoring.MagicalId && player.Stats.Level==42,"class/level state was lost");
            Check(player.GetComponent<GoldWallet>().Gold==123,"gold state was lost");
            string item=SessionState.GetString(Key+"item","");
            Check(player.GetComponent<CarriedInventory>().Items.Any(value=>value.instanceId==item) &&
                player.GetComponent<Equipment>().IsEquipped(item),"carried/equipped item identity was lost");
            float hpRate=SessionState.GetFloat(Key+"hp-regen",0);
            float expectedHp=Mathf.Min(player.Health.Maximum,SessionState.GetFloat(Key+"hp-start",0)+hpRate*(Time.time-SessionState.GetFloat(Key+"time-start",Time.time)));
            Check(Mathf.Abs(player.Health.Current-expectedHp)<Mathf.Max(.25f,hpRate*.15f),"travel changed health beyond natural regeneration");
            var res=player.GetComponent<ActorResourceController>();
            float rate=SessionState.GetFloat(Key+"mana-regen",0);
            float expected=Mathf.Min(res.Maximum(ResourceIds.Mana),SessionState.GetFloat(Key+"mana-start",0)+rate*(Time.time-SessionState.GetFloat(Key+"time-start",Time.time)));
            Check(Mathf.Abs(res.Current(ResourceIds.Mana)-expected)<Mathf.Max(.25f,rate*.15f),"travel changed Mana beyond natural regeneration");
        }

        static void OnLog(string message,string stack,LogType type)
        {
            if(type!=LogType.Error&&type!=LogType.Exception)return;
            if(stack.Contains("DevelopmentRoomValidation")||stack.Contains("UnityEditor.Search.SearchDatabase"))return;
            SessionState.SetString(Key+"status","failed");SessionState.SetString(Key+"error",message+"\n"+stack);Finish();
        }
        static void Finish(){if(ticking){ticking=false;EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;}EditorApplication.ExitPlaymode();}
        static void Cleanup(){
            if(EditorApplication.isPlayingOrWillChangePlaymode || !SessionState.GetBool(Key+"needs-cleanup",false))return;
            SessionState.SetBool(Key+"needs-cleanup",false);
            EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"old-fast",false);
            PersistenceTestGuard.ClearValidationOverrides();DevelopmentSession.End();
            string root=SessionState.GetString(Key+"root","");
            if(!string.IsNullOrEmpty(root)&&Directory.Exists(root))Directory.Delete(root,true);
            EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
        }
    }
}
