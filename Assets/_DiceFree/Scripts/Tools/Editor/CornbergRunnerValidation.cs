using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class CornbergRunnerValidation
    {
        private const string Running = "DiceFree.RunnerValidation";
        private const string Manual = "DiceFree.RunnerValidation.Manual";
        private static CombatActor player;
        private static ManifestationPersistence persistence;
        private static QuestJournal journal;
        private static QuestDefinition beat;
        private static ConversationTarget runner;
        private static IEnumerator routine;
        private static float deadline;
        private static int reports;
        private static string echoId;
        private static long sequence;
        private static QuestProgress State => journal.GetProgress(CornbergRunnerSetup.BeatId);
        static CornbergRunnerValidation() => EditorApplication.playModeStateChanged += OnPlay;
        public static void Run()
        {
            try
            {
                var args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-diceFreeSaveRoot");
                Require(index >= 0 && index + 1 < args.Length && Path.IsPathRooted(args[index + 1]), "Runner requires isolated root.");
                StartRun(index + 1 < args.Length ? args[index + 1] : null, false);
            }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }

        public static void RunManual(string isolatedRoot)
        {
            try { StartRun(isolatedRoot, true); }
            catch (Exception error)
            {
                Debug.LogException(error);
                Issue36ManualValidation.Fail("runner", "Runner Returns", error);
            }
        }

        private static void StartRun(string isolatedRoot, bool manual)
        {
            Require(!string.IsNullOrWhiteSpace(isolatedRoot) &&
                (manual ? Path.IsPathFullyQualified(isolatedRoot) : Path.IsPathRooted(isolatedRoot)), "Runner requires an absolute isolated root.");
            var store = new LocalEchoStore(isolatedRoot); Require(store.Load() == null, "Runner requires fresh root.");
            CornbergValidation.ValidateNavigation();
            var actor = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
            // Old Q3 profile has no runner record. Preserve both opaque sections and unknown quests.
            var records = actor.GetComponent<QuestJournal>().Definitions.Where(q => q.stableId == "quest.cornberg.crop-slimes" ||
                q.stableId == "quest.cornberg.investigate-road" || q.stableId == "quest.cornberg.named-slime")
                .Select(q => new QuestProgress { questId = q.stableId, definitionVersion = q.version,
                    status = QuestStatus.Completed, stage = q.stages.Length - 1, count = q.stages.Last().count }).ToList();
            records.Add(new QuestProgress { questId = "quest.future-runner-test", definitionVersion = 99, stage = 12, count = 9 });
            var data = new ManifestationSave { classId = actor.GetComponent<ActorStats>().Definition.stableId,
                level = 6, xp = 0, anchorId = "anchor.cornberg", quests = records.ToArray() };
            var save = new EchoSave { schemaVersion = 2, revision = 1 }; echoId = save.echoId;
            save.sections.Add(new SaveSection { id = "manifestation:" + data.classId, version = 2, json = JsonUtility.ToJson(data) });
            save.sections.Add(new SaveSection { id = "future-runner-section", version = 99, json = "{\"keep\":true}" });
            SaveMigrationValidation.WriteFixture(store.Path, save);
            if (manual) PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(isolatedRoot);
            SessionState.SetBool(Manual, manual);
            SessionState.SetBool(Running, true);
            EditorApplication.EnterPlaymode();
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Running, false) || state != PlayModeStateChange.EnteredPlayMode) return;
            reports = 0; sequence = 0; Time.timeScale = 3; deadline = Time.realtimeSinceStartup + 120;
            ConversationEvents.Completed += OnConversation; Application.logMessageReceived += OnLog;
            routine = Flow(); EditorApplication.update += Tick;
        }
        private static void OnConversation(ConversationCompleted fact)
        {
            Require(fact.sequence > sequence && fact.actor == player, "Conversation identity/sequence."); sequence = fact.sequence;
            if (fact.npcId == CornbergRunnerSetup.NpcId)
            {
                Require(fact.conversationId == CornbergRunnerSetup.ConversationId, "Wrong runner conversation ID."); reports++;
            }
        }
        private static IEnumerator Bind()
        {
            do { yield return null; persistence = UnityEngine.Object.FindAnyObjectByType<ManifestationPersistence>(); }
            while (persistence == null || !persistence.Ready);
            player = persistence.GetComponent<CombatActor>(); journal = player.GetComponent<QuestJournal>();
            beat = journal.Definitions.Single(q => q.stableId == CornbergRunnerSetup.BeatId);
            runner = UnityEngine.Object.FindAnyObjectByType<ConversationTarget>();
        }
        private static IEnumerator Checkpoint(string label)
        {
            var state = State; int before = reports;
            Require(persistence.Flush(), "Runner save failed.");
            EditorSceneManager.LoadSceneInPlayMode(CornbergSceneBuilder.ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            for (var wait = Bind(); wait.MoveNext();) yield return null;
            var profile = persistence.CaptureProfile();
            Require(State.status == state.status && State.stage == state.stage && State.count == state.count && reports == before, "Runner reload/replay: " + label);
            Require(profile.echoId == echoId && journal.GetProgress("quest.future-runner-test").stage == 12 &&
                profile.sections.Single(s => s.id == "future-runner-section").json == "{\"keep\":true}", "Unknown data/identity lost.");
            var xp = player.GetComponent<ExperienceProgression>();
            Require(xp.Level == 6 && xp.CurrentXp == 0 && player.Health.Current == player.Health.Maximum, "Runner changed XP/load HP.");
            Debug.Log("RUNNER_CHECKPOINT_OK " + label);
        }
        private static IEnumerator Approach(ConversationTarget target)
        {
            var interaction = player.GetComponent<Interactor>(); Require(interaction.Order(target), "Conversation path failed.");
            while (interaction.Active != target) { Require(player.Alive, "Conversation approach died."); yield return null; }
        }
        private static IEnumerator Flow()
        {
            for (var wait = Bind(); wait.MoveNext();) yield return null;
            Require(State.status == QuestStatus.Available && journal.CanAccept(beat), "Old Q3 profile didn't acquire runner beat.");
            var snapshot = journal.CaptureState(); var locked = journal.CaptureState();
            var q3 = Array.Find(locked, q => q.questId == "quest.cornberg.named-slime");
            q3.status = QuestStatus.Available; q3.stage = q3.count = 0; journal.RestoreState(locked);
            player.Motor.Teleport(runner.ApproachPosition); yield return null;
            Require(!runner.Available(player) && !runner.CanInteract(player) && !runner.Acknowledge(player) && !journal.CanAccept(beat), "Runner exposed before Q3.");
            var early = new GameObject("Pre-prerequisite conversation fixture").AddComponent<ConversationTarget>();
            early.transform.position = player.transform.position;
            early.Configure(CornbergRunnerSetup.NpcId, CornbergRunnerSetup.ConversationId, "Fixture");
            player.GetComponent<InteractionRegistry>().Register(early);
            for (var wait = Approach(early); wait.MoveNext();) yield return null;
            Require(early.Acknowledge(player), "Fixture conversation failed.");
            Require(State.status == QuestStatus.Available, "Pre-Q3 semantic event progressed beat.");
            player.GetComponent<Interactor>().Cancel(); UnityEngine.Object.Destroy(early.gameObject);
            journal.RestoreState(snapshot); reports = 0;
            for (var wait = Checkpoint("before-interaction"); wait.MoveNext();) yield return null;
            Require(!runner.Acknowledge(player), "Remote/unopened conversation exploit.");
            for (var wait = Approach(runner); wait.MoveNext();) yield return null;
            Require(State.status == QuestStatus.Available && reports == 0, "Opening dialogue granted credit.");
            player.Motor.Teleport(new Vector3(8, 0, 6));
            Require(!runner.Acknowledge(player) && reports == 0, "Previously opened conversation allowed remote acknowledgment.");
            for (var wait = Approach(runner); wait.MoveNext();) yield return null;
            player.GetComponent<Interactor>().Cancel();
            Require(!runner.Acknowledge(player), "Closed dialogue granted credit.");
            for (var wait = Checkpoint("closed-unacknowledged"); wait.MoveNext();) yield return null;
            // A second generic NPC and a wrong conversation on the correct NPC must not satisfy TalkTo.
            var other = new GameObject("Wrong conversation fixture").AddComponent<ConversationTarget>();
            other.transform.position = player.transform.position;
            other.Configure("npc.other", CornbergRunnerSetup.ConversationId, "Wrong NPC");
            player.GetComponent<InteractionRegistry>().Register(other);
            for (var wait = Approach(other); wait.MoveNext();) yield return null;
            Require(other.Acknowledge(player) && State.status == QuestStatus.Available, "Wrong NPC advanced runner.");
            other.Configure(CornbergRunnerSetup.NpcId, "conversation.other", "Wrong conversation");
            // Test consumer matching directly without treating this fixture as a runner fact in telemetry.
            ConversationEvents.Completed -= OnConversation;
            for (var wait = Approach(other); wait.MoveNext();) yield return null;
            Require(other.Acknowledge(player) && State.status == QuestStatus.Available, "Wrong conversation advanced runner.");
            ConversationEvents.Completed += OnConversation; player.GetComponent<Interactor>().Cancel(); UnityEngine.Object.Destroy(other.gameObject);
            for (var wait = Approach(runner); wait.MoveNext();) yield return null;
            Require(runner.Acknowledge(player) && !runner.Acknowledge(player) && reports == 1 && State.status == QuestStatus.Completed,
                "Runner acknowledgment must emit once and complete.");
            Require(!journal.TurnIn(beat) && !journal.Accept(beat), "Runner reward/progression replay.");
            for (var wait = Checkpoint("completed"); wait.MoveNext();) yield return null;
            for (var wait = Approach(runner); wait.MoveNext();) yield return null;
            Require(runner.Acknowledge(player) && reports == 2 && State.status == QuestStatus.Completed && State.count == 1,
                "Repeat conversation should emit a fresh fact without duplicate progression.");
            for (var wait = Checkpoint("repeated"); wait.MoveNext();) yield return null;
            Debug.Log("CORNBERG_RUNNER_PLAYMODE_OK: gate, approach, remote rejection, semantic IDs, wrong NPC/conversation, once-per-ack, old save/unknowns, four reloads, no XP/reward replay.");
        }
        private static void Tick()
        {
            try { Require(Time.realtimeSinceStartup < deadline, "Runner validation timeout."); if (!routine.MoveNext()) Finish(0); }
            catch (Exception error)
            {
                if (SessionState.GetBool(Manual, false)) Issue36ManualValidation.RecordPipelineFailure(error.ToString());
                Debug.LogException(error); Finish(1);
            }
        }
        private static void OnLog(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;
            if (message.StartsWith("ArgumentOutOfRangeException") && trace.Contains("UnityEditor.Search.SearchDatabase")) return;
            if (SessionState.GetBool(Manual, false)) Issue36ManualValidation.RecordPipelineFailure(message + (string.IsNullOrEmpty(trace) ? string.Empty : "\n" + trace));
            Finish(1);
        }
        private static void Finish(int code)
        {
            if (!SessionState.GetBool(Running, false)) return;
            SessionState.SetBool(Running, false); EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog;
            ConversationEvents.Completed -= OnConversation; Time.timeScale = 1;
            if (SessionState.GetBool(Manual, false))
            {
                SessionState.SetBool(Manual, false);
                Issue36ManualValidation.CompleteRunner(code);
            }
            else EditorApplication.Exit(code);
        }
    }
}
