using System;
using System.IO;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class CornbergCombatValidation
    {
        private const string Running = "DiceFree.CombatValidation";
        private static CombatActor player, enemy;
        private static BasicAttack fists, bump;
        private static AggroBehaviour brain;
        private static TargetSelection selection;
        private static Keyboard keyboard;
        private static Mouse mouse;
        private static int stage, hits;
        private static float deadline, since, health;
        private static Vector3 origin;
        static CornbergCombatValidation() => EditorApplication.playModeStateChanged += OnPlay;
        [MenuItem("DiceFree/Validation/Run Cornberg combat loop (exits editor)")]
        public static void Run()
        {
            PersistenceTestGuard.DisableForNextPlay();
            try { CornbergValidation.ValidateNavigation(); SessionState.SetBool(Running,true); EditorApplication.EnterPlaymode(); }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Running,false) || state != PlayModeStateChange.EnteredPlayMode) return;
            player = UnityEngine.Object.FindFirstObjectByType<TraversalInput>().GetComponent<CombatActor>();
            brain = CombatValidationActors.IsolateCropDuel(); enemy = brain.GetComponent<CombatActor>();
            fists = player.GetComponent<BasicAttack>(); bump = enemy.GetComponent<BasicAttack>(); selection = player.GetComponent<TargetSelection>();
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>();
            stage = 0; deadline = Time.realtimeSinceStartup+40; Time.timeScale = 2;
            Application.logMessageReceived += OnLog; EditorApplication.update += Tick;
        }
        private static void Next()
        {
            Debug.Log("CORNBERG_COMBAT_STAGE_OK " + stage); stage++; since = Time.time; deadline = Time.realtimeSinceStartup+40;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState()); InputSystem.QueueStateEvent(mouse,new MouseState());
        }
        private static void Tick()
        {
            try
            {
                if (!EditorApplication.isPlaying) return;
                Require(Time.realtimeSinceStartup < deadline, "Combat validation timeout at " + stage);
                if (!player.Motor.Ready || !enemy.Motor.Ready) return;
                switch (stage)
                {
                    case 0:
                        Check(player,enemy); Require(brain.ResetEncounter(),"Enemy reset");
                        Require(player.Motor.Teleport(brain.Home + Vector3.left*5),"Field placement");
                        health = player.Health.Current; hits = bump.Hits; Next(); break;
                    case 1:
                        if (bump.Hits < hits+2) return;
                        Require(player.Health.Current < health && player.InCombat,"Aggro, repeat damage, combat state");
                        Require(Time.time-since > 1.7f,"Attacks bypassed interval");
                        Focus(); Next(); break;
                    case 2:
                        if (Time.time-since < 0.3f) return;
                        Next(); RightClick(enemy.transform.position+Vector3.up*0.7f); break;
                    case 3:
                        if (Time.time-since < 0.3f) return;
                        Require(fists.Target == enemy,"Context right-click did not start an attack");
                        Require(selection.Selected == enemy,"Context right-click did not select enemy"); Next(); break;
                    case 4:
                        if (enemy.Alive) return;
                        CornbergValidation.Capture("crop-combat",Camera.main);
                        Require(player.Alive && fists.Hits >= 4,"Novice cannot finish the basic duel");
                        Require(enemy.Health.Heal(100) == 0 && !enemy.Alive,"Healing resurrected dead enemy");
                        Next(); break;
                    case 5:
                        if (Time.time-since < 0.3f) return;
                        Require(selection.Selected == null && fists.Target == null,"Dead target not cleared");
                        player.Health.Restore(); brain.ResetEncounter(); player.Motor.Teleport(brain.Home+Vector3.left*3);
                        Next(); break;
                    case 6:
                        if (player.Alive) return;
                        Require(player.Health.Heal(100) == 0,"Well-style healing resurrected dead player");
                        Require(!player.Motor.MoveTo(new Vector3(0,0,0)),"Dead player accepted movement");
                        Next(); break;
                    case 7:
                        if (!player.GetComponent<RespawnAtAnchor>().CanReturn) return;
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.R)); stage++; break;
                    case 8:
                        if (!player.Alive) return;
                        Require(Vector3.Distance(player.transform.position,new Vector3(8,0,6))<1,"Wrong resurrection anchor");
                        Require(player.Health.Current < player.Health.Maximum*0.6f,"Respawn did not use provisional partial HP");
                        Require(player.Motor.MoveTo(new Vector3(-2,0,6)),"Well route failed"); Next(); break;
                    case 9:
                        if (player.Motor.Travelling) return;
                        health = player.Health.Current; Next(); break;
                    case 10:
                        if (Time.time-since < 2) return;
                        Require(player.Health.Current > health+2 || Mathf.Approximately(player.Health.Current,player.Health.Maximum),"Well did not restore HP");
                        player.Health.Restore(); brain.ResetEncounter(); player.Motor.Teleport(brain.Home+Vector3.left*5); Next(); break;
                    case 11:
                        if (bump.Target != player) return;
                        player.Motor.Teleport(brain.Home+Vector3.left*20); Next(); break;
                    case 12:
                        if (Time.time-since < 2 || brain.Returning) return;
                        Require(Vector3.Distance(enemy.transform.position,brain.Home)<0.7f && !player.InCombat,"Leash/reset/combat exit");
                        brain.enabled = false; player.Motor.Teleport(brain.Home+Vector3.left*2);
                        origin = player.transform.position; player.Motor.MoveDirect(Vector3.right,1);
                        Require(Vector3.Distance(player.Motor.GetComponent<UnityEngine.AI.NavMeshAgent>().nextPosition,enemy.transform.position)>1,"Direct movement ghosted through actor");
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F6)); stage++; break;
                    case 13:
                        if (player.GetComponent<TraversalInput>().Mode != TraversalInput.ControlMode.Direct) return;
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Tab)); stage++; break;
                    case 14:
                        if (selection.Selected != enemy) return;
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.X)); stage++; break;
                    case 15:
                        if (fists.Target != enemy) return;
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W)); stage++; since=Time.time; break;
                    case 16:
                        if (Time.time-since < 0.3f) return;
                        Require(fists.Target == null && selection.Selected == enemy,"WASD must cancel attack, preserve selection");
                        Next(); break;
                    case 17:
                        if (Time.time-since < 0.2f) return;
                        player.Health.Restore(); brain.enabled = true; InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.X)); stage++; break;
                    case 18:
                        if (enemy.Alive) return;
                        Require(player.Alive,"Direct-mode duel failed");
                        Debug.Log("CORNBERG_COMBAT_PLAYMODE_OK: aggro, timing, Classic contextual attack, death, R return, well, leash, unit collision, Tab/X, Direct cancellation and duel.");
                        Finish(0); break;
                }
            }
            catch (Exception error) { Debug.LogException(error); Finish(1); }
        }
        private static void Focus()
        {
            var camera = Camera.main; camera.GetComponent<ExplorationCamera>().enabled = false;
            camera.transform.rotation = Quaternion.Euler(42,45,0);
            camera.transform.position = enemy.transform.position+Vector3.up-camera.transform.forward*24;
        }
        private static void RightClick(Vector3 point)
        {
            var screen = Camera.main.WorldToScreenPoint(point);
            InputSystem.QueueStateEvent(mouse,new MouseState { position = new Vector2(screen.x,screen.y), buttons = 2 });
        }
        private static void OnLog(string message,string trace,LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;
            if (Application.isBatchMode && trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup") && trace.Contains("UnityEditor.Search.SearchDatabase"))
            {
                Debug.LogWarning("CORNBERG_EDITOR_SEARCH_WARNING: " + message); return;
            }
            Directory.CreateDirectory("Logs/Cornberg");
            File.AppendAllText("Logs/Cornberg/combat-errors.txt",message+"\n"+trace+"\n");
            Finish(1);
        }
        private static void Finish(int code)
        {
            EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog; SessionState.SetBool(Running,false); Time.timeScale=1;
            if (keyboard != null) InputSystem.RemoveDevice(keyboard); if (mouse != null) InputSystem.RemoveDevice(mouse);
            EditorApplication.Exit(code);
        }
    }
}
