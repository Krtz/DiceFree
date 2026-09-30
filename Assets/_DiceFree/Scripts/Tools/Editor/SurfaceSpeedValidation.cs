using System;
using System.Collections;
using System.Linq;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class SurfaceSpeedValidation
    {
        private const string Running = "DiceFree.SurfaceValidation";
        private static CombatActor player, enemy;
        private static TraversalMotor motor;
        private static TravelSurface road;
        private static SurfaceTravel travel;
        private static IEnumerator routine;
        private static float deadline;
        private static Keyboard keyboard;
        private static Mouse mouse;
        private static readonly Vector3 OnRoad = new(36, 0, 66f / 29f);
        private static readonly Vector3 OffRoad = new(36, 0, 8);
        static SurfaceSpeedValidation() => EditorApplication.playModeStateChanged += OnPlay;
        public static void Run()
        {
            try
            {
                CornbergValidation.ValidateNavigation(); SessionState.SetBool("DiceFree.DisablePersistence", true);
                SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Running, false) || state != PlayModeStateChange.EnteredPlayMode) return;
            player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>(); motor = player.Motor;
            enemy = CombatValidationActors.Find("enemy.crop-slime");
            foreach (var ai in UnityEngine.Object.FindObjectsByType<AggroBehaviour>()) ai.enabled = false;
            road = TravelSurface.Active.Single(); travel = player.GetComponent<SurfaceTravel>();
            keyboard = InputSystem.AddDevice<Keyboard>(); mouse = InputSystem.AddDevice<Mouse>();
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            deadline = Time.realtimeSinceStartup + 90; Time.timeScale = 1;
            routine = Flow(); Application.logMessageReceived += OnLog; EditorApplication.update += Tick;
        }
        private static void CheckSpeed(float factor, string message)
        {
            Require(Mathf.Approximately(motor.BaseSpeed, player.Stats.MoveSpeed) &&
                Mathf.Approximately(motor.Speed, player.Stats.MoveSpeed * factor) &&
                Mathf.Approximately(motor.GetComponent<NavMeshAgent>().speed, motor.Speed), message);
        }
        private static void Place(Vector3 point) { Require(motor.Teleport(point), "Surface fixture teleport."); }
        private static IEnumerator Flow()
        {
            while (!motor.Ready || !enemy.Motor.Ready) yield return null;
            Require(road.Definition.speedBonusPercent == 15 && road.StableId == "surface-area.cornberg.east-road", "Road data.");
            Require(!road.Contains(OnRoad + Vector3.up * 5), "Surface ignored standing height.");
            Place(OffRoad); CheckSpeed(1, "Off-road baseline.");
            for (int i = 0; i < 6; i++)
            {
                Place(OnRoad); CheckSpeed(1.15f, "Enter/reentry accumulated or missed road.");
                Require(travel.Current == road, "Standing mesh detection.");
                Place(OffRoad); CheckSpeed(1, "Exit/teleport retained road.");
            }
            Place(OnRoad); float oldBase = motor.BaseSpeed;
            player.Stats.SetLevel(3); CheckSpeed(1.15f, "Level change lost road.");
            Require(motor.BaseSpeed > oldBase, "Level did not change AGI baseline.");
            player.Stats.SetSecondaryModifier("validation.surface", new SecondaryScalingModifier { stat = SecondaryStat.MoveSpeed, add = .01f });
            CheckSpeed(1.15f, "Coefficient update retained stale baseline.");
            Place(OffRoad); CheckSpeed(1, "Leaving after stat change lost new baseline.");
            player.Stats.RemoveSecondaryModifier("validation.surface"); player.Stats.SetLevel(1);
            motor.SetSpeedFactor("validation.independent", 1.1f); Place(OnRoad); CheckSpeed(1.1f * 1.15f, "Independent source overwritten.");
            Place(OffRoad); CheckSpeed(1.1f, "Road removal erased another source."); motor.RemoveSpeedFactor("validation.independent");
            // Overlaps select one surface by priority then stable area ID; never duplicate the category.
            var data = ScriptableObject.CreateInstance<TravelSurfaceDefinition>(); data.stableId = "surface.fixture";
            data.priority = 10; data.speedBonusPercent = 30;
            var overlap = new GameObject("Overlap fixture").AddComponent<TravelSurface>();
            overlap.Configure("surface-area.fixture", data, road.GetComponent<MeshFilter>(), new Bounds(Vector3.zero, Vector3.one * 1000));
            Place(OnRoad); CheckSpeed(1.3f, "Overlap priority/stacking.");
            road.enabled = false; CheckSpeed(1.3f, "Removing nonwinner erased winning surface.");
            road.enabled = true; overlap.enabled = false; CheckSpeed(1.15f, "Disabled winner did not reveal underlying surface.");
            overlap.enabled = true; data.priority = road.Definition.priority;
            overlap.Configure("a.fixture", data, road.GetComponent<MeshFilter>(), new Bounds(Vector3.zero, Vector3.one * 1000));
            CheckSpeed(1.3f, "Stable tie ordering.");
            overlap.Configure("z.fixture", data, road.GetComponent<MeshFilter>(), new Bounds(Vector3.zero, Vector3.one * 1000));
            CheckSpeed(1.15f, "Stable tie ordering reverse.");
            overlap.enabled = false; UnityEngine.Object.Destroy(overlap.gameObject); UnityEngine.Object.Destroy(data);
            road.enabled = false; CheckSpeed(1, "Disabled surface retained bonus."); road.enabled = true;
            travel.enabled = false; CheckSpeed(1, "Consumer disable retained surface."); travel.enabled = true; CheckSpeed(1.15f, "Consumer reenable.");
            Place(OffRoad); Require(motor.MoveTo(OnRoad), "Walk-on path rejected.");
            while (motor.Travelling) yield return null;
            CheckSpeed(1.15f, "Walking onto road missed bonus.");
            Require(motor.MoveTo(OffRoad), "Walk-off path rejected.");
            while (motor.Travelling) yield return null;
            CheckSpeed(1, "Walking off road retained bonus."); Place(OnRoad);
            enemy.GetComponent<BasicAttack>().Order(player);
            Require(player.InCombat, "Combat fixture failed."); CheckSpeed(1.15f, "Combat suppressed road.");
            enemy.GetComponent<BasicAttack>().Cancel();
            float enemyBase = enemy.Stats.MoveSpeed; enemy.Motor.Teleport(new Vector3(48, 0, 138f / 29f));
            Require(enemy.GetComponent<SurfaceTravel>() == null && Mathf.Approximately(enemy.Motor.Speed, enemyBase), "Enemy silently opted into road.");
            enemy.Motor.Teleport(new Vector3(47, 0, -10));
            // Real Classic mouse binding, with the destination centered clear of HUD panels.
            var camera = Camera.main; camera.GetComponent<ExplorationCamera>().enabled = false;
            var destination = new Vector3(49, 0, 144f / 29f);
            camera.transform.rotation = Quaternion.Euler(70, 0, 0); camera.transform.position = destination - camera.transform.forward * 25;
            var screen = camera.WorldToScreenPoint(destination);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = screen, buttons = 2 });
            while (!motor.Travelling) yield return null;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
            float start = Time.time;
            while (Time.time - start < .4f) yield return null;
            CheckSpeed(1.15f, "Classic road speed assignment.");
            Require(motor.GetComponent<NavMeshAgent>().velocity.magnitude > motor.BaseSpeed * 1.08f, "Classic path did not actually move faster.");
            motor.Stop(); Place(OnRoad);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.F6));
            while (player.GetComponent<TraversalInput>().Mode != TraversalInput.ControlMode.Direct) yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D)); yield return null; yield return null;
            Vector3 origin = motor.transform.position; start = Time.time;
            while (Time.time - start < .5f) yield return null;
            float directRate = Vector3.Distance(origin, motor.transform.position) / (Time.time - start);
            Require(directRate > motor.BaseSpeed * 1.05f && directRate < motor.Speed * 1.1f, "WASD did not use same effective speed.");
            CheckSpeed(1.15f, "Direction affected road eligibility.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null; motor.Stop();
            Place(OnRoad); player.Health.ApplyDamage(null, new DamageResult { mitigated = 10000 });
            Require(!player.Alive && travel.Current == null, "Death retained road state."); CheckSpeed(1, "Death retained road contribution.");
            var respawn = player.GetComponent<RespawnAtAnchor>(); while (!respawn.CanReturn) yield return null;
            Require(respawn.Return(), "Combat Return failed."); CheckSpeed(1, "Return retained road contribution.");
            Place(OnRoad); CheckSpeed(1.15f, "Restored actor cannot regain road.");
            Require(respawn.LoadAtAnchor("anchor.cornberg"), "Load reset failed."); CheckSpeed(1, "Load/anchor teleport retained road.");
            Debug.Log("SURFACE_SPEED_PLAYMODE_OK: exact base/enter/exit/repeat, Classic click velocity, WASD displacement, combat, stats, independent source, overlap/disable/tie, teleport/death/Return/load, player-only eligibility.");
        }
        private static void Tick()
        {
            try { Require(Time.realtimeSinceStartup < deadline, "Surface validation timeout."); if (!routine.MoveNext()) Finish(0); }
            catch (Exception error) { Debug.LogException(error); Finish(1); }
        }
        private static void OnLog(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;
            if (message.StartsWith("ArgumentOutOfRangeException") && trace.Contains("UnityEditor.Search.SearchDatabase") &&
                trace.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup")) return;
            Finish(1);
        }
        private static void Finish(int code)
        {
            SessionState.SetBool(Running, false); EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog;
            if (keyboard != null) InputSystem.RemoveDevice(keyboard); if (mouse != null) InputSystem.RemoveDevice(mouse);
            Time.timeScale = 1; EditorApplication.Exit(code);
        }
    }
}
