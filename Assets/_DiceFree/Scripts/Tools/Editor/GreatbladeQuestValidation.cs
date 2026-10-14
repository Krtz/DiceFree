using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Dungeons;
using DiceFree.Input;
using DiceFree.Gameplay;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Visuals;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object=UnityEngine.Object;
namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class GreatbladeQuestValidation
    {
        const string Key="DiceFree.GreatbladeQuest.";
        static IEnumerator flow;static double deadline;static int checks;
        static GreatbladeQuestValidation()=>EditorApplication.playModeStateChanged+=Changed;
        static void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);checks++;}
        [CliCommand("dicefree.greatblade.quest.test","Test mentor eligibility, exact boss credit, persisted progress and atomic quest advancement in isolated saves.")]
        public static object Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            if(EditorSceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save scene first.");
            SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);
            SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetBool(Key+"cleanup",true);SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"error","");
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(Path.Combine(Path.GetTempPath(),"DiceFree-GreatbladeQuest-"+Guid.NewGuid().ToString("N")));
            EditorSettings.enterPlayModeOptionsEnabled=false;
            EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);EditorApplication.EnterPlaymode();return Status();
        }
        [CliCommand("dicefree.greatblade.quest.status","Read Greatblade mentor quest validation.")]
        public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),phase=SessionState.GetString(Key+"phase",""),checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error","")};
        static void Changed(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running")
            {checks=0;deadline=EditorApplication.timeSinceStartup+150;UnityEngine.Application.runInBackground=true;flow=Flow();EditorApplication.update+=Tick;UnityEngine.Application.logMessageReceived+=Log;}
            if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false))
            {
                SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;UnityEngine.Application.logMessageReceived-=Log;
                EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);PersistenceTestGuard.ClearValidationOverrides();
                string scene=SessionState.GetString(Key+"scene","");if(!string.IsNullOrEmpty(scene))EditorSceneManager.OpenScene(scene);
            }
        }
        static void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)Finish(false,text+"\n"+stack);}
        static void Tick()
        {
            if(!EditorApplication.isPlaying)return;
            try{if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Mentor quest timed out.");if(flow!=null&&!flow.MoveNext())Finish(true,null);}
            catch(Exception e){Finish(false,e.ToString());}
        }
        static void Phase(string value)=>SessionState.SetString(Key+"phase",value);
        static IEnumerator Flow()
        {
            Phase("mentor level/class gates");
            CombatActor player=null;while(player==null){player=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();yield return null;}
            var save=player.GetComponent<ManifestationPersistence>();while(!save.Ready||!player.Motor.Ready)yield return null;
            player.GetComponent<TraversalInput>().enabled=false;player.GetComponent<CombatInput>().enabled=false;
            GameplayPreferences.Current.SetAutoRetaliate(false);player.Health.Invulnerable=true;
            var xp=player.GetComponent<ExperienceProgression>();var controller=player.GetComponent<AdvancementController>();var journal=player.GetComponent<QuestJournal>();
            var mentor=Object.FindAnyObjectByType<GreatbladeMentor>();var quest=mentor.TrainingQuest;
            var physical=controller.Definitions.Single(e=>e.targetClass.stableId=="class.physically-blessed-novice");
            var greatblade=controller.Definitions.Single(e=>e.targetClass.stableId==GreatbladeProgressionRules.ClassId);
            xp.RestoreState(30,0);Need(!journal.CanAccept(quest),"Level-30 Novice accepted Physical advancement quest.");
            xp.RestoreState(10,0);Need(controller.TryAdvance(physical),controller.Feedback);
            xp.RestoreState(29,0);Need(!mentor.Available(player)&&!journal.CanAccept(quest),"Quest offered below Physical level 30.");
            xp.RestoreState(30,0);Protect(player);
            Need(player.Motor.Teleport(mentor.ApproachPosition+Vector3.back*1.8f),"Mentor approach unavailable.");
            yield return null;
            Need(mentor.CanInteract(player)&&journal.CanAccept(quest),"Physical level 30 cannot accept mentor quest.");
            Need(!controller.CanAdvance(greatblade)&&!controller.TryAdvance(greatblade),"Uncompleted quest bypassed advancement gate.");
            long before=save.Revision;Need(!mentor.CompleteTraining(player)&&save.Revision==before,"Incomplete turn-in changed durable profile.");
            var tuning=AssetDatabase.LoadAssetAtPath<SlimeDungeonTuning>("Assets/_DiceFree/Settings/Dungeons/Slime playtest tuning.asset");
            Phase("pre-acceptance and nonmatching kills");
            var hub=new SemanticGameEventHub();journal.BindDungeonEvents(hub);
            hub.Publish(new DungeonBossKilledEvent("dungeon.slime","boss.slime"));
            KillFixture(player,tuning.boss);
            Need(journal.GetProgress(quest.stableId).status==QuestStatus.Available&&journal.GetProgress(quest.stableId).count==0,"Earlier normal boss kill counted.");
            Need(mentor.AcceptTraining(player),"Quest acceptance failed.");
            Need(!mentor.AcceptTraining(player),"Repeated acceptance changed quest.");
            KillFixture(player,tuning.miniboss);KillFixture(player,tuning.regent);
            hub.Publish(new DungeonBossKilledEvent("dungeon.slime","boss.big-slime"));
            hub.Publish(new DungeonBossKilledEvent("dungeon.slime","boss.slime-regent"));
            KillFixture(player,tuning.boss);
            Need(journal.GetProgress(quest.stableId).count==0,"Miniboss or Regent counted for normal boss objective.");
            Need(save.Flush(),"Accepted quest persistence failed.");
            Need(controller.TrySwitch("class.novice"),controller.Feedback);KillFixture(player,tuning.boss);
            hub.Publish(new DungeonBossKilledEvent("dungeon.slime","boss.slime"));
            Need(journal.GetProgress(quest.stableId).count==0,"Wrong manifestation gained training credit.");
            Need(controller.TrySwitch("class.physically-blessed-novice"),controller.Feedback);
            Need(journal.GetProgress(quest.stableId).status==QuestStatus.Active&&journal.GetProgress(quest.stableId).count==0,"Accepted Physical quest lost during switching.");
            Protect(player);
            Phase("real dungeon normal-boss credit");
            var portal=Object.FindObjectsByType<DungeonPortal>().Single(p=>p.isActiveAndEnabled&&p.definition!=null&&p.definition.stableId=="dungeon.slime");
            Need(player.Motor.Teleport(portal.ApproachPosition+Vector3.back*1.5f),"Dungeon approach.");
            Need(player.Alive&&player.Health.Invulnerable&&portal.CanInteract(player),"Protected player cannot interact with dungeon portal.");
            portal.Interact(player);while(SlimeDungeonRun.Current==null||!SlimeDungeonRun.Current.StagingReady)yield return null;
            var run=SlimeDungeonRun.Current;Need(run.StartNow(player),"Dungeon start failed.");
            var mini=Object.FindObjectsByType<DungeonSlime>().Single(s=>s.Role==SlimeRole.Miniboss);
            mini.Actor.Health.ApplyDamage(player,new DamageResult{mitigated=mini.Actor.Health.Maximum*2});
            Need(journal.GetProgress(quest.stableId).count==0,"Real miniboss counted.");
            run.SolvePuzzle(false);yield return null;
            var boss=Object.FindObjectsByType<DungeonSlime>().Single(s=>s.Role==SlimeRole.Boss);
            boss.Actor.Health.ApplyDamage(player,new DamageResult{mitigated=boss.Actor.Health.Maximum*2});
            var ready=journal.GetProgress(quest.stableId);
            Need(ready.status==QuestStatus.ReadyToTurnIn&&ready.count==1,"Real normal Slime Boss defeat did not complete objective.");
            Need(save.CurrentClassId=="class.physically-blessed-novice"&&!controller.CanAdvance(greatblade),"Boss kill advanced before return to mentor.");
            run.Abandon();while(SlimeDungeonRun.Current!=null)yield return null;
            Need(save.Flush(),"Ready-to-turn-in persistence failed.");
            Need(controller.TrySwitch("class.novice")&&controller.TrySwitch("class.physically-blessed-novice"),"Ready quest switching failed.");
            Need(journal.GetProgress(quest.stableId).status==QuestStatus.ReadyToTurnIn,"Ready objective was not restored.");
            Protect(player);
            Need(player.Motor.Teleport(mentor.ApproachPosition+Vector3.back*1.8f),"Return to mentor unavailable.");yield return null;
            Phase("atomic turn-in and advancement");
            int parentLevel=player.Stats.Level;before=save.Revision;
            Need(mentor.CompleteTraining(player),controller.Feedback);
            Need(save.Revision==before+1,"Turn-in and advancement must use one durable commit.");
            Need(save.CurrentClassId==GreatbladeProgressionRules.ClassId&&player.Stats.Level==1,"Quest did not advance to level-1 Greatblade.");
            Need(journal.GetProgress(quest.stableId).status==QuestStatus.Completed,"Child quest completion missing.");
            var disk=new LocalEchoStore(Path.GetDirectoryName(save.SavePath)).Load();
            var parent=ManifestationRoster.ReadManifestation(disk,"class.physically-blessed-novice");
            Need(parent.level==parentLevel&&parent.quests.Single(q=>q.questId==quest.stableId).status==QuestStatus.Completed,"Parent level/quest snapshot not preserved.");
            var child=ManifestationRoster.ReadManifestation(disk,GreatbladeProgressionRules.ClassId);
            Need(child.level==1&&child.quests.Single(q=>q.questId==quest.stableId).status==QuestStatus.Completed,"Atomic on-disk child state incomplete.");
            Need(!mentor.CompleteTraining(player),"Duplicate turn-in allowed.");
            Need(controller.TrySwitch("class.physically-blessed-novice"),controller.Feedback);yield return null;
            Need(!mentor.Available(player),"Already unlocked Greatblade offers duplicate training.");
        }
        static void Protect(CombatActor player){player.Stats.SetEncounterMaximumHp(100000);player.Health.Restore();player.Health.Invulnerable=true;}
        static void KillFixture(CombatActor killer,ActorDefinition definition)
        {
            var go=new GameObject("Quest credit fixture "+definition.stableId);go.SetActive(false);
            var agent=go.AddComponent<NavMeshAgent>();agent.enabled=false;
            go.AddComponent<TraversalMotor>();go.AddComponent<ActorStats>().Configure(definition,1);
            go.AddComponent<Health>();var actor=go.AddComponent<CombatActor>();actor.Configure(1,.45f);
            go.AddComponent<DefeatReporter>();go.transform.position=killer.transform.position;
            go.SetActive(true);actor.Health.ApplyDamage(killer,new DamageResult{mitigated=actor.Health.Maximum*2});
            Object.Destroy(go);
        }
        static void Finish(bool ok,string error)
        {
            flow=null;EditorApplication.update-=Tick;UnityEngine.Application.logMessageReceived-=Log;
            SessionState.SetInt(Key+"checks",checks);SessionState.SetString(Key+"status",ok?"passed":"failed");SessionState.SetString(Key+"error",error??"");
            File.WriteAllText("Library/GreatbladeQuestResults.json",JsonUtility.ToJson(new Record{passed=ok,checks=checks,error=error},true));
            if(ok)Debug.Log("DICEFREE_GREATBLADE_QUEST_OK");else Debug.LogWarning("Greatblade quest test failed: "+error);
            EditorApplication.ExitPlaymode();
        }
        [Serializable]class Record{public bool passed;public int checks;public string error;}
    }
}
