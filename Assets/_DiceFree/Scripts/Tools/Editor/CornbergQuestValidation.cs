using System;
using System.IO;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class CornbergQuestValidation
    {
        private const string Running="DiceFree.QuestValidation";
        private static CombatActor player,enemy;
        private static AggroBehaviour brain;
        private static OverworldRespawn respawn;
        private static ExperienceProgression xp;
        private static QuestJournal journal;
        private static QuestGiver giver;
        private static Interactor interactor;
        private static Keyboard keyboard;
        private static Mouse mouse;
        private static int stage,kills,reports,credits;
        private static object actorId;
        private static float deadline,diedAt;
        private static long lastSequence;
        private static string questId;
        static CornbergQuestValidation()=>EditorApplication.playModeStateChanged+=OnPlay;
        [MenuItem("DiceFree/Validation/Run Cornberg Q1 progression (exits editor)")]
        public static void Run()
        {
            SessionState.SetBool("DiceFree.DisablePersistence", true);
            try { CornbergValidation.ValidateNavigation(); SessionState.SetBool(Running,true); EditorApplication.EnterPlaymode(); }
            catch(Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if(!SessionState.GetBool(Running,false)||state!=PlayModeStateChange.EnteredPlayMode)return;
            player=UnityEngine.Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
            brain=UnityEngine.Object.FindAnyObjectByType<AggroBehaviour>(); enemy=brain.GetComponent<CombatActor>();
            respawn=enemy.GetComponent<OverworldRespawn>(); xp=player.GetComponent<ExperienceProgression>();
            journal=player.GetComponent<QuestJournal>(); giver=UnityEngine.Object.FindAnyObjectByType<QuestGiver>();
            interactor=player.GetComponent<Interactor>(); questId=giver.Quest.stableId; actorId=enemy.GetEntityId();
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard=InputSystem.AddDevice<Keyboard>(); mouse=InputSystem.AddDevice<Mouse>();
            stage=kills=reports=credits=0; lastSequence=0; Time.timeScale=4; deadline=Time.realtimeSinceStartup+60;
            DefeatEvents.Reported+=OnDefeat; player.GetComponent<KillCreditReceiver>().Credited+=OnCredit;
            Application.logMessageReceived+=OnLog; EditorApplication.update+=Tick;
        }
        private static void OnCredit(ActorDefeated value)=>credits++;
        private static void OnDefeat(ActorDefeated value)
        {
            if(value.victim!=enemy)return;
            Require(value.sequence>lastSequence,"Defeat identity must be unique per life"); lastSequence=value.sequence;
            Require(value.contentId==enemy.Stats.Definition.stableId && value.familyId=="enemy-family.slime","Semantic victim IDs missing");
            Require(value.killer==player && value.creditOwner==player,"Killer/owner credit missing");
            Require(Vector3.Distance(value.position,enemy.transform.position)<0.01f,"Death location missing"); reports++;
        }
        private static void Kill()
        {
            var before=reports; var beforeCredit=credits;
            enemy.GetComponent<BasicAttack>().Order(player);
            player.GetComponent<TargetSelection>().Select(enemy);
            enemy.Health.ApplyDamage(player,new DamageResult{raw=1000,mitigated=1000});
            enemy.Health.ApplyDamage(player,new DamageResult{raw=1000,mitigated=1000});
            Require(!enemy.Alive && reports==before+1 && credits==beforeCredit+1,"Death/credit emitted more than once");
            diedAt=Time.time;
        }
        private static bool Respawned()
        {
            if(!enemy.Alive)
            {
                Require(Time.time-diedAt<respawn.Definition.delaySeconds+2,"Respawn missed its deadline"); return false;
            }
            Require(Time.time-diedAt>=respawn.Definition.delaySeconds,"Early respawn");
            Require(enemy.Health.Current==enemy.Health.Maximum,"Respawn HP incomplete");
            Require(UnityEngine.AI.NavMesh.SamplePosition(brain.Home,out var home,2,1) &&
                Vector3.Distance(enemy.transform.position,home.position)<0.1f,"Respawn moved away from authored ground point");
            Require(!brain.Returning && !enemy.InCombat && enemy.GetComponent<BasicAttack>().Target==null,"Stale respawn combat state");
            Require(player.GetComponent<TargetSelection>().Selected==null,"Old target survived death/respawn");
            Require(CombatActor.All.Count==2 && actorId.Equals(enemy.GetEntityId()),"Respawn duplicated actors"); return true;
        }
        private static void Tick()
        {
            try
            {
                if(!EditorApplication.isPlaying)return;
                Require(Time.realtimeSinceStartup<deadline,"Q1 timeout at stage "+stage);
                if(!player.Motor.Ready||!enemy.Motor.Ready)return;
                switch(stage)
                {
                    case 0:
                        Require(!giver.Accept(player),"Remote acceptance allowed");
                        player.Motor.Teleport(new Vector3(30,0,2));
                        Kill(); Require(xp.CurrentXp==10 && journal.GetProgress(questId).status==QuestStatus.Available,"Pre-accept credit policy");
                        // Reset only the test fixture's XP so the requested fresh five-kill pacing can be measured.
                        var serialized=new SerializedObject(xp); serialized.FindProperty("currentXp").intValue=0; serialized.ApplyModifiedPropertiesWithoutUndo();
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.I)); stage++; break;
                    case 1:
                        if(interactor.Active!=giver)return;
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                        Require(giver.Accept(player) && !giver.Accept(player),"Accept must be one-time");
                        Require(journal.GetProgress(questId).count==0,"Pre-accept kills counted retroactively");
                        interactor.Cancel(); stage++; break;
                    case 2:
                        if(!Respawned())return;
                        if(kills==2)player.Health.ApplyDamage(enemy,new DamageResult{raw=4,mitigated=4});
                        float missingHp=player.Health.Maximum-player.Health.Current;
                        Kill(); kills++;
                        var progress=journal.GetProgress(questId);
                        if(kills<3) Require(progress.stage==0&&progress.count==kills&&xp.Level==1&&xp.CurrentXp==kills*10,"Stage-one count/XP");
                        if(kills==3)
                        {
                            Require(progress.stage==1&&progress.count==0&&xp.Level==2&&xp.CurrentXp==0,"Third kill must advance quest and level");
                            Require(Mathf.Approximately(player.Health.Maximum-player.Health.Current,missingHp),"Level-up must preserve missing HP");
                            CheckLevel(2); Debug.Log("CORNBERG_Q1_STAGE_ONE_OK: 3 kills -> level 2 and stage 2 0/2.");
                        }
                        if(kills==4)Require(progress.stage==1&&progress.count==1&&xp.CurrentXp==10,"Fourth kill credit");
                        if(kills==5)
                        {
                            Require(progress.status==QuestStatus.ReadyToTurnIn&&progress.count==2&&xp.Level==2&&xp.CurrentXp==20,"Fifth kill must await turn-in");
                            player.Motor.Teleport(new Vector3(47,0,-8));
                            Require(!giver.TurnIn(player),"Remote turn-in allowed");
                            Focus(); var point=Camera.main.WorldToScreenPoint(giver.transform.position);
                            InputSystem.QueueStateEvent(mouse,new MouseState{position=new Vector2(point.x,point.y),buttons=2});
                            stage++; deadline=Time.realtimeSinceStartup+60;
                        }
                        break;
                    case 3:
                        if(interactor.Active!=giver)return;
                        InputSystem.QueueStateEvent(mouse,new MouseState());
                        Require(giver.TurnIn(player)&&!giver.TurnIn(player),"Turn-in/reward must happen once");
                        Require(xp.Level==3&&xp.CurrentXp==0&&journal.GetProgress(questId).status==QuestStatus.Completed,"Turn-in level-three target");
                        CheckLevel(3);
                        Require(journal.CaptureState()[0].questId==questId,"Persistence record missing stable quest ID");
                        Debug.Log("CORNBERG_Q1_TURNIN_OK: 2/2, return interaction, one reward -> level 3."); stage++; break;
                    case 4:
                        if(!Respawned())return;
                        Kill();
                        Require(xp.CurrentXp==10&&journal.GetProgress(questId).status==QuestStatus.Completed&&journal.GetProgress(questId).count==2,"Completed quest must not restart; kills still grant XP");
                        Require(reports==7&&credits==7,"Repeated lives lost/duplicated defeat credit");
                        player.Health.ApplyDamage(enemy,new DamageResult{raw=1000,mitigated=1000});
                        xp.Grant(xp.RequiredXp-xp.CurrentXp);
                        Require(xp.Level==4&&!player.Alive&&player.Health.Current==0,"Level-up resurrected a dead actor");
                        Debug.Log("CORNBERG_Q1_PLAYMODE_OK: seven lives, once-only semantic credit/XP, pre/post quest policy, timed full-state respawn, 3+2 stages, turn-in, stats/scaling, I and right-click interaction.");
                        Finish(0); break;
                }
            }
            catch(Exception error){Debug.LogException(error);Finish(1);}
        }
        private static void CheckLevel(int level)
        {
            var a=player.Stats.Attributes;
            Require(a.vitality==level&&a.strength==level&&a.agility==level&&a.intelligence==level&&a.spirit==level,"All five Novice stats must grow");
            Require(player.Health.Maximum==10+15*level&&player.Health.Current>0&&player.Health.Current<=player.Health.Maximum,"Level-up HP bounds");
            Require(Mathf.Approximately(player.Stats.Regeneration, 0.1f*level), "Level-up regeneration");
            Require(player.Stats.Definition.basicAttack.RawDamage(a)==1+2*level,"Fists must read post-level attributes");
        }
        private static void Focus()
        {
            var camera=Camera.main; camera.GetComponent<ExplorationCamera>().enabled=false;
            camera.transform.rotation=Quaternion.Euler(42,45,0);
            camera.transform.position=new Vector3(37,1,0)-camera.transform.forward*30;
        }
        private static void OnLog(string message,string trace,LogType type)
        {
            if(type!=LogType.Error&&type!=LogType.Exception)return;
            if(Application.isBatchMode&&trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup")&&trace.Contains("UnityEditor.Search.SearchDatabase"))return;
            Directory.CreateDirectory("Logs/Cornberg"); File.AppendAllText("Logs/Cornberg/quest-errors.txt",message+"\n"+trace+"\n"); Finish(1);
        }
        private static void Finish(int code)
        {
            EditorApplication.update-=Tick; Application.logMessageReceived-=OnLog; DefeatEvents.Reported-=OnDefeat;
            player.GetComponent<KillCreditReceiver>().Credited-=OnCredit;
            SessionState.SetBool(Running,false); Time.timeScale=1;
            if(keyboard!=null)InputSystem.RemoveDevice(keyboard); if(mouse!=null)InputSystem.RemoveDevice(mouse);
            EditorApplication.Exit(code);
        }
    }
}
