using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Persistence;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class BriarbackPatrolValidation
    {
        private const string SessionKey = "DiceFree.Briarback.Patrols.";
        private static readonly Stack<IEnumerator> Steps = new();
        private static double deadline;
        private static int checks;

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string error;
            public int checks;
            public int groups;
            public int members;
            public float walkedMeters;
            public float minimumPause;
            public float maximumPause;
        }

        private static Report report;

        static BriarbackPatrolValidation() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        private static void Need(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
            checks++;
        }

        [CliCommand("dicefree.briarback.patrols.test",
            "Run both three-member Briarback road patrols in Play Mode and verify travel, collision-safe navigation and randomized camp pauses with an isolated save.")]
        public static object Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before starting the isolated patrol regression.");

            SessionState.SetString(SessionKey + "scene", EditorSceneManager.GetActiveScene().path);
            SessionState.SetBool(SessionKey + "options", EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetInt(SessionKey + "flags", (int)EditorSettings.enterPlayModeOptions);
            SessionState.SetBool(SessionKey + "cleanup", true);
            SessionState.SetString(SessionKey + "status", "running");
            SessionState.SetString(SessionKey + "error", "");
            SessionState.SetInt(SessionKey + "checks", 0);
            string isolatedRoot = Path.Combine(Path.GetTempPath(), "DiceFree-Briarback-Patrols-" + Guid.NewGuid().ToString("N"));
            SessionState.SetString(SessionKey + "root", isolatedRoot);
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(isolatedRoot);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.briarback.patrols.status", "Read the isolated road-patrol Play Mode regression status.")]
        public static object Status() => new
        {
            status = SessionState.GetString(SessionKey + "status", "idle"),
            checks = SessionState.GetInt(SessionKey + "checks", 0),
            error = SessionState.GetString(SessionKey + "error", "")
        };

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetString(SessionKey + "status", "") == "running")
            {
                checks = 0;
                report = new Report { groups = 2, members = 6 };
                Steps.Clear();
                Steps.Push(Flow());
                deadline = EditorApplication.timeSinceStartup + 180;
                EditorApplication.update += Tick;
                Application.logMessageReceived += OnLog;
            }

            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(SessionKey + "cleanup", false))
            {
                SessionState.SetBool(SessionKey + "cleanup", false);
                EditorApplication.update -= Tick;
                Application.logMessageReceived -= OnLog;
                PersistenceTestGuard.ClearValidationOverrides();
                EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)SessionState.GetInt(SessionKey + "flags", 0);
                EditorSettings.enterPlayModeOptionsEnabled = SessionState.GetBool(SessionKey + "options", false);
                string previousScene = SessionState.GetString(SessionKey + "scene", "");
                if (!string.IsNullOrWhiteSpace(previousScene)) EditorSceneManager.OpenScene(previousScene);
            }
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
                Finish(false, condition + "\n" + stackTrace);
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            try
            {
                if (EditorApplication.timeSinceStartup > deadline)
                    throw new TimeoutException("Road patrol Play Mode test timed out.");
                while (Steps.Count > 0)
                {
                    var current = Steps.Peek();
                    if (!current.MoveNext()) { Steps.Pop(); continue; }
                    if (current.Current is IEnumerator child) { Steps.Push(child); continue; }
                    return;
                }
                Finish(true, null);
            }
            catch (Exception exception) { Finish(false, exception.ToString()); }
        }

        private static void Finish(bool passed, string error)
        {
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
            report ??= new Report();
            report.status = passed ? "passed" : "failed";
            report.error = error;
            report.checks = checks;
            SessionState.SetString(SessionKey + "status", report.status);
            SessionState.SetString(SessionKey + "error", error ?? "");
            SessionState.SetInt(SessionKey + "checks", checks);
            File.WriteAllText("Library/BriarbackPatrolResults.json", JsonUtility.ToJson(report, true));
            EditorApplication.ExitPlaymode();
        }

        private static IEnumerator WaitFor(Func<bool> predicate, float timeout, string description)
        {
            float end = Time.realtimeSinceStartup + timeout;
            while (!predicate() && Time.realtimeSinceStartup < end) yield return null;
            Need(predicate(), "Timed out waiting for " + description + ".");
        }

        private static string Describe(QuilboarRoadPatrol group)
        {
            return string.Join(" | ", group.Members.Select((actor, i) =>
            {
                var agent = actor.GetComponent<NavMeshAgent>();
                var attack = actor.GetComponent<BasicAttack>();
                var aggro = actor.GetComponent<DiceFree.AI.AggroBehaviour>();
                return actor.name + " pos=" + actor.transform.position.ToString("F1") +
                       " target=" + group.RouteTargets[group.CurrentStop * 3 + i].ToString("F1") +
                       " agent=" + (agent == null ? "missing" : "on=" + agent.isOnNavMesh + ",path=" + agent.hasPath + ",state=" + agent.pathStatus + ",remaining=" + agent.remainingDistance.ToString("F1") + ",speed=" + agent.velocity.magnitude.ToString("F1")) +
                       " combat=" + (attack.Target == null ? "none" : attack.Target.name) +
                       " aggro=" + (aggro == null ? "missing" : aggro.State + ",returning=" + aggro.Returning +
                           ",home=" + aggro.Home.ToString("F1") + ",leash=" + aggro.Leash);
            }));
        }

        private static IEnumerator WaitRealtime(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end) yield return null;
        }

        private static IEnumerator ExerciseGroup(QuilboarRoadPatrol group)
        {
            var starts = group.Members.Select(actor => actor.transform.position).ToArray();
            float arrivalEnd = Time.realtimeSinceStartup + 40;
            while (!(group.IsWaitingAtStop && group.CurrentStop == 0) && Time.realtimeSinceStartup < arrivalEnd)
                yield return null;
            Need(group.IsWaitingAtStop && group.CurrentStop == 0,
                group.name + " did not reach its first camp stop: " + Describe(group));
            Need(group.CurrentPauseSeconds >= 3f && group.CurrentPauseSeconds <= 8f,
                group.name + " did not choose a 3-8 second randomized stop.");
            Need(group.PauseRemaining > 0, group.name + " pause elapsed before arrival could be inspected.");
            float firstPauseDuration = group.CurrentPauseSeconds;
            float firstPauseObservedAt = Time.realtimeSinceStartup;
            RecordPause(firstPauseDuration);
            yield return WaitRealtime(.4f);
            Need(group.IsWaitingAtStop && group.CurrentStop == 0, group.name + " did not remain at its first camp during the pause.");
            for (int i = 0; i < 3; i++)
                Need(Vector3.Distance(group.Members[i].transform.position, group.RouteTargets[i]) < 1.2f,
                    group.name + " member " + i + " did not reach its first route target.");
            report.walkedMeters += group.Members.Select((actor, i) => Vector3.Distance(starts[i], actor.transform.position)).Sum();

            var pausedPositions = group.Members.Select(actor => actor.transform.position).ToArray();
            yield return WaitRealtime(.45f);
            Need(group.IsWaitingAtStop && group.CurrentStop == 0, group.name + " left its camp before the pause ended.");
            float pauseDrift = group.Members.Select((actor, i) => Vector3.Distance(pausedPositions[i], actor.transform.position)).Max();
            Need(pauseDrift < .15f, group.name + " members continued moving during the camp pause; max drift=" + pauseDrift.ToString("F2") + "m.");

            yield return WaitFor(() => group.CurrentStop == 1 && !group.IsWaitingAtStop, 9,
                group.name + " to finish its camp pause and depart for the second stop");
            float firstPauseElapsed = Time.realtimeSinceStartup - firstPauseObservedAt;
            Need(firstPauseElapsed >= firstPauseDuration - .15f && firstPauseElapsed <= firstPauseDuration + .5f,
                group.name + " did not hold its first camp pause for the selected duration; selected=" +
                firstPauseDuration.ToString("F2") + "s elapsed=" + firstPauseElapsed.ToString("F2") + "s.");
            yield return WaitRealtime(.25f);
            int traveling = group.Members.Count(actor => actor.GetComponent<NavMeshAgent>().hasPath);
            Need(traveling >= 2, group.name + " did not issue real NavMesh movement to the next camp.");
            foreach (var member in group.Members)
                Need(member.GetComponent<BasicAttack>().Target == null,
                    member.name + " acquired an unexpected combat target during the patrol fixture.");
            yield return WaitFor(() => group.IsWaitingAtStop && group.CurrentStop == 1, 35,
                group.name + " to reach its second camp stop");
            Need(group.CurrentPauseSeconds >= 3f && group.CurrentPauseSeconds <= 8f,
                group.name + " second camp pause is outside 3-8 seconds.");
            RecordPause(group.CurrentPauseSeconds);
            for (int i = 0; i < 3; i++)
                Need(Vector3.Distance(group.Members[i].transform.position, group.RouteTargets[3 + i]) < 1.2f,
                    group.name + " member " + i + " missed its second stop.");
        }

        private static void RecordPause(float seconds)
        {
            report.minimumPause = report.minimumPause == 0 ? seconds : Mathf.Min(report.minimumPause, seconds);
            report.maximumPause = Mathf.Max(report.maximumPause, seconds);
        }

        private static IEnumerator Flow()
        {
            CombatActor player = null;
            while (player == null)
            {
                player = Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();
                yield return null;
            }
            var save = player.GetComponent<ManifestationPersistence>();
            while (!save.Ready || !player.Motor.Ready) yield return null;
            player.GetComponent<TraversalInput>().enabled = false;
            player.GetComponent<CombatInput>().enabled = false;
            player.GetComponent<BasicAttack>().enabled = false;
            player.Health.Invulnerable = true;

            var groups = Object.FindObjectsByType<QuilboarRoadPatrol>();
            Need(groups.Length == 2, "The saved scene did not load exactly two patrol groups.");
            var allMembers = groups.SelectMany(group => group.Members).ToArray();
            Need(allMembers.Length == 6 && allMembers.All(actor => actor != null), "The patrol groups do not have six live actor references.");
            var patrolActors = new HashSet<CombatActor>(allMembers);
            foreach (var other in CombatActor.All.ToArray())
                if (other != player && !patrolActors.Contains(other))
                    other.gameObject.SetActive(false);
            foreach (var group in groups)
            {
                Need(group.MinimumStopSeconds == 3 && group.MaximumStopSeconds == 8, "Patrol pause bounds changed at runtime.");
                foreach (var member in group.Members)
                {
                    var aggro = member.GetComponent<DiceFree.AI.AggroBehaviour>();
                    Need(aggro != null && aggro.Awareness == 8 && aggro.Leash == 20 && aggro.PatrolControlled,
                        member.name + " did not retain ordinary aggro/leash behavior.");
                    Need(member.GetComponent<BasicAttack>().Target == null, member.name + " was unexpectedly in combat at patrol start.");
                }
            }

            foreach (var group in groups)
            {
                group.enabled = false;
            }
            foreach (var group in groups)
            {
                group.enabled = true;
                yield return ExerciseGroup(group);
                group.enabled = false;
            }
            Need(report.walkedMeters > 35f, "The six patrol members did not make a real route walk; moved " + report.walkedMeters.ToString("F1") + "m.");
        }
    }
}
