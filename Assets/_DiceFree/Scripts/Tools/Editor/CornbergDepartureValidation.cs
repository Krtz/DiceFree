using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class CornbergDepartureValidation
    {
        private const string Running = "DiceFree.DepartureValidation";
        private static IEnumerator flow;
        private static float deadline;
        private static CombatActor player;
        private static QuestJournal journal;
        private static QuestDefinition quest, q4;
        private static QuestGiver giver;
        private static ContextualNpc runner;
        private static ConversationTarget story;
        private static ManifestationPersistence persistence;
        private static Keyboard keyboard;
        private static string identity;
        private static QuestProgress State => journal.GetProgress(CornbergDepartureSetup.QuestId);
        static CornbergDepartureValidation() => EditorApplication.playModeStateChanged += OnPlay;
        public static void Run()
        {
            try
            {
                var args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-diceFreeSaveRoot");
                Require(index >= 0 && index + 1 < args.Length && Path.IsPathRooted(args[index + 1]), "Isolated profile required");
                var store = new LocalEchoStore(args[index + 1]); Require(store.Load() == null, "Fresh profile required");
                CornbergValidation.ValidateNavigation();
                var actor = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
                var records = actor.GetComponent<QuestJournal>().Definitions.Where(d => d.stableId != CornbergDepartureSetup.QuestId)
                    .Select(d => new QuestProgress { questId = d.stableId, definitionVersion = d.version,
                        status = QuestStatus.Completed, stage = d.stages.Length - 1, count = d.stages.Last().count }).ToList();
                var surge = records.Single(r => r.questId == CornbergSurgeSetup.QuestId);
                surge.status = QuestStatus.ReadyToTurnIn; surge.alternatives = new[] { new ObjectiveCount { objectiveId = "surge.elite", count = 1 }, new ObjectiveCount { objectiveId = "surge.population", count = 1 } };
                records.Add(new QuestProgress { questId = "quest.future.departure-test", definitionVersion = 99, stage = 17, count = 8 });
                var state = new ManifestationSave { classId = actor.GetComponent<ActorStats>().Definition.stableId,
                    level = 7, xp = 0, anchorId = "anchor.cornberg", quests = records.ToArray() };
                var save = new EchoSave { revision = 1 };
                save.sections.Add(new SaveSection { id = "manifestation:" + state.classId, version = SaveMigrations.ManifestationVersion, json = JsonUtility.ToJson(state) });
                save.sections.Add(new SaveSection { id = "unknown:departure", version = 91, json = "opaque" });
                store.Commit(save);
                SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Running, false)) return;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>(); deadline = Time.realtimeSinceStartup + 120;
            flow = Flow(); Application.logMessageReceived += OnLog; EditorApplication.update += Tick;
        }
        private static IEnumerator Bind()
        {
            do { yield return null; persistence = UnityEngine.Object.FindAnyObjectByType<ManifestationPersistence>(); }
            while (persistence == null || !persistence.Ready);
            player = persistence.GetComponent<CombatActor>(); journal = player.GetComponent<QuestJournal>();
            quest = journal.Definitions.Single(d => d.stableId == CornbergDepartureSetup.QuestId);
            q4 = journal.Definitions.Single(d => d.stableId == CornbergSurgeSetup.QuestId);
            runner = UnityEngine.Object.FindObjectsByType<ContextualNpc>().Single(n => n.NpcId == CornbergRunnerSetup.NpcId);
            giver = runner.GetComponentsInChildren<QuestGiver>().Single(); story = runner.GetComponentsInChildren<ConversationTarget>().Single();
            foreach (var brain in UnityEngine.Object.FindObjectsByType<AggroBehaviour>()) brain.enabled = false;
        }
        private static IEnumerator Checkpoint(string label)
        {
            string quests = JsonUtility.ToJson(State), surge = JsonUtility.ToJson(journal.GetProgress(q4.stableId));
            int level = player.Stats.Level, xp = player.GetComponent<ExperienceProgression>().CurrentXp;
            long gold = player.GetComponent<GoldWallet>().Gold;
            string items = string.Join(",", player.GetComponent<CarriedInventory>().Items.Select(i => i.instanceId));
            Require(persistence.Flush(), "Checkpoint write");
            EditorSceneManager.LoadSceneInPlayMode(CornbergSceneBuilder.ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            for (var wait = Bind(); wait.MoveNext();) yield return null;
            var profile = persistence.CaptureProfile();
            Require(profile.echoId == identity && profile.schemaVersion == 4 && profile.sections.Single(s => s.id == "unknown:departure").json == "opaque", "Identity/schema/unknown section");
            Require(journal.GetProgress("quest.future.departure-test").stage == 17, "Unknown quest lost");
            Require(JsonUtility.ToJson(State) == quests && JsonUtility.ToJson(journal.GetProgress(q4.stableId)) == surge, "Quest reload altered state");
            Require(player.Stats.Level == level && player.GetComponent<ExperienceProgression>().CurrentXp == xp && player.GetComponent<GoldWallet>().Gold == gold &&
                string.Join(",", player.GetComponent<CarriedInventory>().Items.Select(i => i.instanceId)) == items, "Reward replay or item loss");
            Debug.Log("DEPARTURE_CHECKPOINT_OK " + label);
        }
        private static IEnumerator Approach(InteractionTarget target)
        {
            var interactor = player.GetComponent<Interactor>(); Require(interactor.Order(target), "Approach request");
            while (interactor.Active != target.Resolve(player)) yield return null;
        }
        private static IEnumerator Flow()
        {
            for (var wait = Bind(); wait.MoveNext();) yield return null;
            identity = persistence.CaptureProfile().echoId;
            Require(!journal.CanAccept(quest) && runner.Resolve(player) == story, "Q5 before Q4 completion");
            Require(runner.GetComponents<InteractionTarget>().Length == 1 && runner.GetComponentsInChildren<InteractionTarget>().Count(t => t.WorldTarget == t) == 1, "Competing world targets");
            Require(story.NpcId == CornbergRunnerSetup.NpcId && story.ConversationId == CornbergRunnerSetup.ConversationId && story.Dialogue.Contains("usual weekly run"), "Original story data lost");
            for (var wait = Approach(runner); wait.MoveNext();) yield return null;
            Require(story.Acknowledge(player) && !giver.Accept(player), "Pre-Q4 story/offer behavior");
            for (var wait = Checkpoint("before-available"); wait.MoveNext();) yield return null;
            var farmer = UnityEngine.Object.FindObjectsByType<QuestGiver>().Single(g => g.Quest == q4);
            player.Motor.Teleport(farmer.ApproachPosition); yield return null;
            Require(farmer.TurnIn(player) && player.Stats.Level == 9 && player.GetComponent<ExperienceProgression>().CurrentXp == 10, "Unchanged elite Q4 reward");
            Require(journal.CanAccept(quest) && runner.Resolve(player) == giver && !giver.Accept(player), "Q5 availability/remote rejection");
            Require(quest.completedQuestIds.SequenceEqual(new[] { q4.stableId }) && quest.rewardXp == 0 && quest.reward == null, "Q5 prerequisite/reward authoring");
            Require(UnityEngine.Object.FindObjectsByType<QuestGiver>().Count(g => g.Offers.Contains(quest)) == 1, "Duplicate Q5 giver");
            for (var wait = Checkpoint("available-level-9"); wait.MoveNext();) yield return null;
            yield return null; // Let presentation gating observe the restored quest state before clicking its collider.
            var camera = Camera.main; camera.GetComponent<ExplorationCamera>().enabled = false;
            camera.transform.rotation = Quaternion.Euler(42, 45, 0); camera.transform.position = runner.transform.position + Vector3.up - camera.transform.forward * 25;
            var screen = camera.WorldToScreenPoint(runner.transform.position + Vector3.up * .9f);
            Physics.SyncTransforms();
            Require(Physics.Raycast(camera.ScreenPointToRay(screen), out var clickHit, 1500, (1<<8)|(1<<9)|(1<<10)|(1<<11)), "Runner click collider");
            Debug.Log("DEPARTURE_CLICK_HIT " + clickHit.collider.name);
            Require(player.GetComponent<Interactor>().ContextInteract(camera.ScreenPointToRay(screen)), "Click ray/approach");
            while (player.GetComponent<Interactor>().Active != giver) yield return null;
            Require(giver.Accept(player) && !giver.Accept(player) && State.status == QuestStatus.Active, "Level-nine acceptance/idempotency");
            Require(player.Stats.Level == 9 && player.GetComponent<ExperienceProgression>().CurrentXp == 10 && player.GetComponent<GoldWallet>().Gold == 100 && player.GetComponent<CarriedInventory>().Items.Length == 1,
                "Q5 acceptance granted reward");
            Require(!story.Acknowledge(player) && !journal.TurnIn(quest), "Inactive story or Q5 completion exploit");
            for (var wait = Checkpoint("active-level-9"); wait.MoveNext();) yield return null;
            Require(!UnityEngine.Object.FindObjectsByType<ReachArea>().Any(a => a.StableId == CornbergDepartureSetup.DestinationId), "Fake destination in Cornberg");
            foreach (var area in UnityEngine.Object.FindObjectsByType<ReachArea>())
            { Require(player.Motor.Teleport(area.transform.position), "Existing area reachable"); yield return null; yield return null; }
            player.Motor.Teleport(new Vector3(110, 0, 37)); yield return null; yield return null;
            Require(State.status == QuestStatus.Active && State.count == 0, "Cornberg completed Q5");
            var reset = journal.CaptureState(); Array.Find(reset, r => r.questId == quest.stableId).status = QuestStatus.Available;
            journal.RestoreState(reset); player.GetComponent<ExperienceProgression>().RestoreState(13, 30);
            Require(player.Motor.Teleport(runner.ApproachPosition + Vector3.left), "Keyboard approach placement"); yield return null;
            Require(runner.CanInteract(player) && giver.CanInteract(player) && journal.CanAccept(quest), "Keyboard interaction prerequisites");
            InputSystem.RemoveDevice(keyboard); keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.I));
            float keyDeadline = Time.realtimeSinceStartup + 3;
            while (player.GetComponent<Interactor>().Active != giver)
            { Require(Time.realtimeSinceStartup < keyDeadline, "Keyboard did not open contextual quest action"); yield return null; }
            InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
            Require(player.GetComponent<Interactor>().Active == giver && giver.Accept(player), "Level-thirteen keyboard/default action");
            Require(!journal.CanAccept(quest) && runner.Resolve(player) == giver && State.status == QuestStatus.Active, "Active contextual role");
            for (var wait = Checkpoint("active-level-13"); wait.MoveNext();) yield return null;
            for (var wait = Approach(runner); wait.MoveNext();) yield return null;
            Require(!giver.Accept(player) && !giver.TurnIn(player) && State.status == QuestStatus.Active, "Repeat acceptance/completion");
            Require(player.GetComponent<GoldWallet>().Gold == 100 && player.GetComponent<CarriedInventory>().Items.Length == 1 && player.GetComponent<ExperienceProgression>().CurrentXp == 30, "Repeat reward");
            Require(quest.stages[0].areaId == CornbergDepartureSetup.DestinationId && story.Dialogue.Contains("usual weekly run"), "Future destination/story identity");
            for (var wait = Checkpoint("active-repeated"); wait.MoveNext();) yield return null;
            Debug.Log("CORNBERG_DEPARTURE_OK: contextual single target, story preserved, Q4-only prerequisite, levels 9/13, click/keyboard/range, future Active objective, five reloads, no reward or schema change.");
        }
        private static void Tick()
        {
            try { Require(Time.realtimeSinceStartup < deadline, "Departure timeout"); if (!flow.MoveNext()) Finish(0); }
            catch (Exception error) { File.AppendAllText("T:/TEMP/msq5-validation-errors.txt", error + "\n"); Debug.LogException(error); Finish(1); }
        }
        private static void OnLog(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;
            if (message.StartsWith("ArgumentOutOfRangeException") && trace.Contains("UnityEditor.Search.SearchDatabase") && trace.Contains("IndexationOnStartup")) return;
            File.AppendAllText("T:/TEMP/msq5-validation-errors.txt", message + "\n" + trace + "\n"); Finish(1);
        }
        private static void Finish(int code)
        {
            SessionState.SetBool(Running, false); EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog;
            if (keyboard != null) InputSystem.RemoveDevice(keyboard); EditorApplication.Exit(code);
        }
    }
}
