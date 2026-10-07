using System;
using System.IO;
using DiceFree.Characters;
using DiceFree.Persistence;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    internal static class Issue36ManualValidation
    {
        private const string ActiveKey = "DiceFree.Issue36Manual.Active";
        private const string RootKey = "DiceFree.Issue36Manual.Root";
        private const string ResultKey = "DiceFree.Issue36Manual.Result";
        private const string PhaseKey = "DiceFree.Issue36Manual.SavePhase";
        private const string PendingGitShaKey = "DiceFree.Issue36Manual.PendingGitSha";
        private const string TaskIdKey = "DiceFree.Issue36Manual.TaskId";
        private const string TaskNameKey = "DiceFree.Issue36Manual.TaskName";
        private const string TaskKindKey = "DiceFree.Issue36Manual.TaskKind";
        private const string TaskRootKey = "DiceFree.Issue36Manual.TaskRoot";
        private const string TaskStatusKey = "DiceFree.Issue36Manual.TaskStatus";
        private const string TaskMarkerKey = "DiceFree.Issue36Manual.TaskMarker";
        private const string TaskErrorKey = "DiceFree.Issue36Manual.TaskError";
        private const string TaskGitShaKey = "DiceFree.Issue36Manual.TaskGitSha";
        private const string TaskStartedTicksKey = "DiceFree.Issue36Manual.TaskStartedTicks";
        private const string TaskEndedTicksKey = "DiceFree.Issue36Manual.TaskEndedTicks";
        private const string SavePriorFastPlayEnabledKey = "DiceFree.Issue36Manual.SavePriorFastPlayEnabled";
        private const string SavePriorFastPlayOptionsKey = "DiceFree.Issue36Manual.SavePriorFastPlayOptions";

        private static TraversalMotor motor;
        private static NavMeshAgent agent;
        private static Vector3 sampledStart, sampledDestination;
        private static NavMeshPath routePath;
        private static float routeLength, routeStarted, nextRouteLog;
        private static bool routePrepared, routeFinished;

        static Issue36ManualValidation()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += TickSavePhaseTransition;
        }

        internal static bool IsPipelineValidationBusy() =>
            !string.IsNullOrEmpty(SessionState.GetString(ActiveKey, string.Empty)) ||
            EditorApplication.isPlayingOrWillChangePlaymode;

        internal static void StartFromPipeline(string kind, string testGitSha)
        {
            if (IsPipelineValidationBusy()) throw new InvalidOperationException("Another Issue 36 check or Play Mode transition is already active.");
            SessionState.SetString(PendingGitShaKey, testGitSha ?? string.Empty);
            switch (kind)
            {
                case "route12": RunRoute12(); break;
                case "runner": RunRunner(); break;
                case "save": RunSaveReload(); break;
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown Issue 36 validation.");
            }
        }

        internal static object GetPipelineTaskStatus(string requestedTaskId)
        {
            TickSavePhaseTransition();
            string taskId = SessionState.GetString(TaskIdKey, string.Empty);
            if (!string.IsNullOrEmpty(requestedTaskId) && !string.Equals(taskId, requestedTaskId, StringComparison.Ordinal))
                return new { success = false, status = "not_found", validation = string.Empty, taskId = requestedTaskId,
                    finalMarker = (string)null, elapsedSeconds = 0d, error = "No current or most recently completed task has this ID.",
                    temporarySaveRoot = (string)null, logPath = Application.consoleLogPath, testedGitSha = string.Empty, phase = 0 };

            string status = SessionState.GetString(TaskStatusKey, "idle");
            long startTicks = ParseTicks(SessionState.GetString(TaskStartedTicksKey, string.Empty));
            long endTicks = ParseTicks(SessionState.GetString(TaskEndedTicksKey, string.Empty));
            long elapsedTicks = startTicks == 0 ? 0 : Math.Max(0, (endTicks == 0 ? DateTime.UtcNow.Ticks : endTicks) - startTicks);
            bool? success = status == "passed" ? true : status == "failed" ? false : (bool?)null;
            return new
            {
                success,
                status,
                validation = SessionState.GetString(TaskNameKey, string.Empty),
                taskId,
                finalMarker = SessionState.GetString(TaskMarkerKey, string.Empty),
                elapsedSeconds = TimeSpan.FromTicks(elapsedTicks).TotalSeconds,
                error = SessionState.GetString(TaskErrorKey, string.Empty),
                temporarySaveRoot = SessionState.GetString(TaskRootKey, string.Empty),
                logPath = Application.consoleLogPath,
                testedGitSha = SessionState.GetString(TaskGitShaKey, string.Empty),
                phase = SessionState.GetInt(PhaseKey, 0),
                editorIsPlaying = EditorApplication.isPlaying,
                editorIsChangingPlayMode = EditorApplication.isPlayingOrWillChangePlaymode,
                orchestrationState = SessionState.GetString(ResultKey, string.Empty),
                validationActive = SessionState.GetString(ActiveKey, string.Empty)
            };
        }

        private static long ParseTicks(string value) => long.TryParse(value, out var ticks) ? ticks : 0;

        private static void BeginTask(string displayName, string kind, string root)
        {
            var now = DateTime.UtcNow.Ticks;
            SessionState.SetString(TaskIdKey, Guid.NewGuid().ToString("N"));
            SessionState.SetString(TaskNameKey, displayName);
            SessionState.SetString(TaskKindKey, kind);
            SessionState.SetString(TaskRootKey, root ?? string.Empty);
            SessionState.SetString(TaskStatusKey, "running");
            SessionState.SetString(TaskMarkerKey, string.Empty);
            SessionState.SetString(TaskErrorKey, string.Empty);
            SessionState.SetString(TaskGitShaKey, SessionState.GetString(PendingGitShaKey, string.Empty));
            SessionState.SetString(PendingGitShaKey, string.Empty);
            SessionState.SetString(TaskStartedTicksKey, now.ToString());
            SessionState.SetString(TaskEndedTicksKey, string.Empty);
        }

        private static void UpdateTask(string status, string marker, string error)
        {
            if (string.IsNullOrEmpty(SessionState.GetString(TaskIdKey, string.Empty))) return;
            SessionState.SetString(TaskStatusKey, status);
            SessionState.SetString(TaskMarkerKey, marker ?? string.Empty);
            SessionState.SetString(TaskErrorKey, error ?? string.Empty);
            if (status == "passed" || status == "failed")
                SessionState.SetString(TaskEndedTicksKey, DateTime.UtcNow.Ticks.ToString());
        }

        internal static void RecordPipelineFailure(string error)
        {
            if (string.IsNullOrEmpty(SessionState.GetString(TaskIdKey, string.Empty))) return;
            SessionState.SetString(TaskErrorKey, error ?? string.Empty);
        }

        [MenuItem("DiceFree/Validation/Issue 36/Route 12 - Return to mountain")]
        private static void RunRoute12()
        {
            if (RejectIfBusy("Route 12 - Return to mountain")) return;
            try
            {
                Begin("Route 12 - Return to mountain", "route12", null);
                CornbergValidation.ValidateNavigation();
                PersistenceTestGuard.DisableForNextPlay();
                SessionState.SetBool("DiceFree.Issue36Manual.RouteRunning", true);
                EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Fail("route12", "Route 12", error); }
        }

        [MenuItem("DiceFree/Validation/Issue 36/Runner Returns - Dedicated harness")]
        private static void RunRunner()
        {
            if (RejectIfBusy("Runner Returns - Dedicated harness")) return;
            try
            {
                var root = CreateFreshRoot("Runner");
                Begin("Runner Returns - Dedicated harness", "runner", root);
                PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
                CornbergRunnerValidation.RunManual(root);
            }
            catch (Exception error) { Fail("runner", "Runner Returns", error); }
        }

        [MenuItem("DiceFree/Validation/Issue 36/Save reload - Two phase")]
        private static void RunSaveReload()
        {
            if (RejectIfBusy("Save reload - Two phase")) return;
            try
            {
                var root = CreateFreshRoot("SaveReload");
                Begin("Save reload - Two phase", "save", root);
                // This validator explicitly proves a fresh player-like save/reload lifecycle.
                // Do not let the developer's Fast Play (no domain/scene reload) state leak between its phases.
                SessionState.SetBool(SavePriorFastPlayEnabledKey, EditorSettings.enterPlayModeOptionsEnabled);
                SessionState.SetInt(SavePriorFastPlayOptionsKey, (int)EditorSettings.enterPlayModeOptions);
                EditorSettings.enterPlayModeOptionsEnabled = false;
                SessionState.SetInt(PhaseKey, 1);
                CornbergSaveValidation.RunManualPhase(root, 1);
            }
            catch (Exception error) { Fail("save", "Save reload", error); }
        }

        private static bool RejectIfBusy(string requested)
        {
            var active = SessionState.GetString(ActiveKey, string.Empty);
            if (string.IsNullOrEmpty(active) && !EditorApplication.isPlayingOrWillChangePlaymode) return false;
            Debug.LogError("ISSUE36_MANUAL_START_REJECTED: " + requested + "; active validation=" +
                (string.IsNullOrEmpty(active) ? "Unity Play Mode transition" : active));
            return true;
        }

        private static void Begin(string displayName, string kind, string root)
        {
            if (!string.IsNullOrEmpty(SessionState.GetString(ActiveKey, string.Empty)) ||
                EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("An Issue 36 manual validation is already running, or Unity is changing Play Mode.");

            SessionState.SetString(ActiveKey, kind);
            SessionState.SetString(RootKey, root ?? string.Empty);
            SessionState.SetString(ResultKey, string.Empty);
            BeginTask(displayName, kind, root);
            routePrepared = routeFinished = false;
            Debug.Log("ISSUE36_MANUAL_START: " + displayName + (string.IsNullOrEmpty(root) ? string.Empty : " | isolated root: " + root));
        }

        private static string CreateFreshRoot(string label)
        {
            var root = Path.Combine(Path.GetTempPath(), "DiceFree-Issue36-" + label + "-" + Guid.NewGuid().ToString("N"));
            if (Directory.Exists(root) || File.Exists(root))
                throw new IOException("The unique validation root unexpectedly already exists: " + root);
            Directory.CreateDirectory(root);
            if (Directory.GetFileSystemEntries(root).Length != 0)
                throw new IOException("The new validation root is not empty: " + root);
            return Path.GetFullPath(root);
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            var kind = SessionState.GetString(ActiveKey, string.Empty);
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (kind == "route12") StartRoute12();
                return;
            }

            if (state != PlayModeStateChange.EnteredEditMode || string.IsNullOrEmpty(kind)) return;
            var result = SessionState.GetString(ResultKey, string.Empty);
            if (kind == "save" && result == "phase1-ok")
            {
                SessionState.SetString(ResultKey, "phase2-starting");
                return;
            }

            if (result == "complete" || result == "failed") Cleanup(kind);
        }

        private static void TickSavePhaseTransition()
        {
            if (SessionState.GetString(ActiveKey, string.Empty) != "save" ||
                EditorApplication.isPlayingOrWillChangePlaymode) return;
            var result = SessionState.GetString(ResultKey, string.Empty);
            if (result == "phase2-starting")
            {
                StartSaveReloadPhase2();
                return;
            }
            bool phaseOneCompleted = result == "phase1-ok" ||
                (result.Length == 0 && SessionState.GetInt(PhaseKey, 0) == 1 &&
                 SessionState.GetString(TaskMarkerKey, string.Empty) == "SAVE_FIRST_PROCESS_OK" &&
                 SessionState.GetString(TaskStatusKey, string.Empty) == "running" &&
                 File.Exists(new LocalEchoStore(SessionState.GetString(RootKey, string.Empty)).Path));
            if (!phaseOneCompleted) return;
            SessionState.SetString(ResultKey, "phase2-starting");
        }

        private static void StartRoute12()
        {
            try
            {
                routeFinished = false;
                motor = UnityEngine.Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<TraversalMotor>();
                Require(motor != null, "Cornberg player TraversalMotor was not found.");
                foreach (var enemy in UnityEngine.Object.FindObjectsByType<DiceFree.AI.AggroBehaviour>(FindObjectsSortMode.None))
                    enemy.gameObject.SetActive(false);
                Time.timeScale = 3f;
                routeStarted = Time.realtimeSinceStartup;
                nextRouteLog = routeStarted;
                EditorApplication.update += TickRoute12;
                Application.logMessageReceived += OnRouteLog;
            }
            catch (Exception error) { FinishRoute12(false, error.Message); }
        }

        private static void TickRoute12()
        {
            if (routeFinished || !EditorApplication.isPlaying) return;
            try
            {
                var elapsed = Time.realtimeSinceStartup - routeStarted;
                Require(elapsed <= 90f, "90 real-second diagnostic timeout.");
                if (!motor.Ready) return;
                if (!routePrepared) PrepareRoute12();

                if (Vector3.Distance(motor.transform.position, sampledDestination) < 0.6f)
                {
                    FinishRoute12(true, null);
                    return;
                }
                if (Time.realtimeSinceStartup >= nextRouteLog)
                {
                    Debug.Log(AgentState("ISSUE36_ROUTE12_PROGRESS", elapsed));
                    nextRouteLog = Time.realtimeSinceStartup + 5f;
                }
            }
            catch (Exception error) { FinishRoute12(false, error.Message); }
        }

        private static void PrepareRoute12()
        {
            agent = motor.GetComponent<NavMeshAgent>();
            Require(agent != null && agent.enabled && agent.isOnNavMesh, "Player NavMeshAgent is missing, disabled, or off the NavMesh.");
            sampledStart = Sample(new Vector3(110f, 0f, 37f), agent.areaMask);
            sampledDestination = Sample(new Vector3(-42f, 3f, -30f), agent.areaMask);
            routePath = new NavMeshPath();
            bool calculated = NavMesh.CalculatePath(sampledStart, sampledDestination, agent.areaMask, routePath);
            routeLength = PathLength(routePath.corners);
            Debug.Log("ISSUE36_ROUTE12_STATIC: sampledStart=" + sampledStart + "; sampledDestination=" + sampledDestination +
                "; calculatePath=" + calculated + "; pathStatus=" + routePath.status + "; cornerCount=" + routePath.corners.Length +
                "; pathLength=" + routeLength.ToString("F2") + "m");
            for (int i = 0; i < routePath.corners.Length; i++)
                Debug.Log("ISSUE36_ROUTE12_CORNER " + i + "=" + routePath.corners[i]);
            Require(calculated && routePath.status == NavMeshPathStatus.PathComplete, "Static route-12 path is not complete.");
            Require(motor.Teleport(sampledStart), "TraversalMotor could not warp to route-11 destination.");
            Debug.Log("ISSUE36_ROUTE12_WARPED: " + AgentState("ISSUE36_ROUTE12_STATE", Time.realtimeSinceStartup - routeStarted));
            Require(motor.MoveTo(sampledDestination), "TraversalMotor.MoveTo rejected route-12 destination.");
            routeStarted = Time.realtimeSinceStartup;
            nextRouteLog = routeStarted + 5f;
            routePrepared = true;
        }

        private static string AgentState(string marker, float elapsed)
        {
            var surface = motor.GetComponent<SurfaceTravel>()?.Current;
            string surfaceId = surface == null ? "none" : surface.StableId;
            return marker + ": elapsed=" + elapsed.ToString("F1") + "s; actor=" + motor.transform.position +
                "; nextPosition=" + (agent == null ? Vector3.zero.ToString() : agent.nextPosition.ToString()) +
                "; velocity=" + (agent == null ? Vector3.zero.ToString() : agent.velocity.ToString()) +
                "; remainingDistance=" + (agent == null ? -1f : agent.remainingDistance).ToString("F2") +
                "; pathPending=" + (agent != null && agent.pathPending) + "; hasPath=" + (agent != null && agent.hasPath) +
                "; pathStatus=" + (agent == null ? "no-agent" : agent.pathStatus.ToString()) +
                "; motor.Ready=" + motor.Ready + "; motor.Travelling=" + motor.Travelling +
                "; effectiveSpeed=" + motor.Speed.ToString("F2") + "; surface=" + surfaceId +
                "; surfaceFactor=" + (motor.BaseSpeed > 0 ? motor.Speed / motor.BaseSpeed : 1f).ToString("F2");
        }

        private static void OnRouteLog(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;
            if (message.StartsWith("ArgumentOutOfRangeException", StringComparison.Ordinal) && trace.Contains("UnityEditor.Search.SearchDatabase")) return;
            FinishRoute12(false, "Unity " + type + ": " + message + "\n" + trace);
        }

        private static void FinishRoute12(bool passed, string reason)
        {
            if (routeFinished) return;
            routeFinished = true;
            EditorApplication.update -= TickRoute12;
            Application.logMessageReceived -= OnRouteLog;
            Time.timeScale = 1f;
            string state = motor == null ? "motor=missing" : AgentState("final", Time.realtimeSinceStartup - routeStarted);
            if (passed)
            {
                Debug.Log("ISSUE36_ROUTE12_OK: elapsed=" + (Time.realtimeSinceStartup - routeStarted).ToString("F2") +
                    "s; pathLength=" + routeLength.ToString("F2") + "m; " + state);
                SessionState.SetString(ResultKey, "complete");
                UpdateTask("passed", "ISSUE36_ROUTE12_OK", string.Empty);
            }
            else
            {
                Debug.LogError("ISSUE36_ROUTE12_FAIL: " + reason + "; pathLength=" + routeLength.ToString("F2") + "m; " + state);
                SessionState.SetString(ResultKey, "failed");
                UpdateTask("failed", "ISSUE36_ROUTE12_FAIL", reason);
            }
            EditorApplication.ExitPlaymode();
        }

        private static void StartSaveReloadPhase2()
        {
            if (SessionState.GetString(ActiveKey, string.Empty) != "save" ||
                SessionState.GetInt(PhaseKey, 0) != 1) return;
            try
            {
                var root = SessionState.GetString(RootKey, string.Empty);
                Require(!string.IsNullOrEmpty(root) && Path.IsPathFullyQualified(root), "Saved phase-one root is missing or invalid.");
                Require(File.Exists(new LocalEchoStore(root).Path), "Phase 1 did not leave a persisted save file on disk.");
                SessionState.SetInt(PhaseKey, 2);
                Debug.Log("ISSUE36_SAVE_PHASE2_START: reloading from disk at " + root);
                CornbergSaveValidation.RunManualPhase(root, 2);
            }
            catch (Exception error) { Fail("save", "Save reload phase 2", error); }
        }

        internal static void CompleteRunner(int code)
        {
            if (code == 0)
            {
                Debug.Log("ISSUE36_RUNNER_OK: " + SessionState.GetString(RootKey, string.Empty));
                SessionState.SetString(ResultKey, "complete");
                UpdateTask("passed", "ISSUE36_RUNNER_OK", string.Empty);
            }
            else
            {
                Debug.LogError("ISSUE36_RUNNER_FAIL: dedicated Runner validation failed; root=" + SessionState.GetString(RootKey, string.Empty));
                SessionState.SetString(ResultKey, "failed");
                UpdateTask("failed", "ISSUE36_RUNNER_FAIL", SessionState.GetString(TaskErrorKey, "Dedicated Runner assertion failed; inspect the Editor log."));
            }
            EditorApplication.ExitPlaymode();
        }

        internal static void CompleteSavePhase(int phase, int code)
        {
            if (code != 0)
            {
                Debug.LogError("ISSUE36_SAVE_RELOAD_FAIL: phase=" + phase + "; root=" + SessionState.GetString(RootKey, string.Empty));
                SessionState.SetString(ResultKey, "failed");
                UpdateTask("failed", "ISSUE36_SAVE_RELOAD_FAIL", SessionState.GetString(TaskErrorKey, "Save/reload assertion failed; inspect the Editor log."));
                EditorApplication.ExitPlaymode();
                return;
            }

            if (phase == 1)
            {
                Debug.Log("ISSUE36_SAVE_PHASE1_OK: " + SessionState.GetString(RootKey, string.Empty));
                SessionState.SetString(ResultKey, "phase1-ok");
                SessionState.SetString(TaskMarkerKey, "SAVE_FIRST_PROCESS_OK");
            }
            else
            {
                Debug.Log("ISSUE36_SAVE_RELOAD_OK: " + SessionState.GetString(RootKey, string.Empty));
                SessionState.SetString(ResultKey, "complete");
                UpdateTask("passed", "ISSUE36_SAVE_RELOAD_OK", string.Empty);
            }
            EditorApplication.ExitPlaymode();
        }

        internal static void Fail(string expectedKind, string testName, Exception error)
        {
            if (error != null) Debug.LogException(error);
            var kind = SessionState.GetString(ActiveKey, string.Empty);
            var root = SessionState.GetString(RootKey, string.Empty);
            string marker = expectedKind == "route12" ? "ISSUE36_ROUTE12_FAIL" : expectedKind == "runner" ? "ISSUE36_RUNNER_FAIL" : "ISSUE36_SAVE_RELOAD_FAIL";
            if (!string.IsNullOrEmpty(kind) && kind != expectedKind)
            {
                Debug.LogError("ISSUE36_MANUAL_START_REJECTED: " + testName + "; active validation=" + kind + ". Existing run left alone.");
                return;
            }
            Debug.LogError(marker + ": " + testName + " setup/orchestration failure; root=" + root);
            SessionState.SetString(ResultKey, "failed");
            if (string.IsNullOrEmpty(SessionState.GetString(TaskIdKey, string.Empty))) BeginTask(testName, expectedKind, root);
            UpdateTask("failed", marker, error == null ? testName + " failed; inspect the Editor log." : error.ToString());
            SessionState.SetString(PendingGitShaKey, string.Empty);
            if (EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.ExitPlaymode();
            else if (!string.IsNullOrEmpty(kind)) Cleanup(kind);
        }

        private static void Cleanup(string kind)
        {
            var root = SessionState.GetString(RootKey, string.Empty);
            if (kind == "save")
            {
                EditorSettings.enterPlayModeOptions =
                    (EnterPlayModeOptions)SessionState.GetInt(
                        SavePriorFastPlayOptionsKey,
                        (int)EditorSettings.enterPlayModeOptions);
                EditorSettings.enterPlayModeOptionsEnabled =
                    SessionState.GetBool(
                        SavePriorFastPlayEnabledKey,
                        EditorSettings.enterPlayModeOptionsEnabled);
            }
            PersistenceTestGuard.ClearValidationOverrides();
            SessionState.SetBool("DiceFree.Issue36Manual.RouteRunning", false);
            SessionState.SetBool("DiceFree.RunnerValidation", false);
            SessionState.SetBool("DiceFree.RunnerValidation.Manual", false);
            SessionState.SetBool("DiceFree.SaveValidation", false);
            SessionState.SetBool("DiceFree.SaveValidation.Manual", false);
            SessionState.SetInt(PhaseKey, 0);
            Debug.Log("ISSUE36_MANUAL_END: " + kind + "; result=" + SessionState.GetString(ResultKey, string.Empty) +
                (string.IsNullOrEmpty(root) ? string.Empty : "; isolated root retained=" + root));
            SessionState.SetString(ActiveKey, string.Empty);
            SessionState.SetString(RootKey, string.Empty);
        }

        private static Vector3 Sample(Vector3 point, int areaMask)
        {
            Require(NavMesh.SamplePosition(point, out var hit, 5f, areaMask), "No walkable ground near " + point);
            return hit.position;
        }

        private static float PathLength(Vector3[] corners)
        {
            float length = 0;
            for (int i = 1; i < corners.Length; i++) length += Vector3.Distance(corners[i - 1], corners[i]);
            return length;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
