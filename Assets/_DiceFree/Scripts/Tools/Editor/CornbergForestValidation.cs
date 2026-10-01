using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.AI;
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
    public static class CornbergForestValidation
    {
        private const string Running = "DiceFree.ForestValidation";
        private static CombatActor player, named;
        private static ManifestationPersistence persistence;
        private static QuestJournal journal;
        private static QuestDefinition quest;
        private static QuestGiver giver;
        private static ReachArea area;
        private static ExperienceProgression xp;
        private static IEnumerator routine;
        private static float deadline;
        private static int reports;
        private static string echoId;
        private static QuestProgress State => journal.GetProgress(quest.stableId);
        static CornbergForestValidation() => EditorApplication.playModeStateChanged += OnPlay;
        public static void Run()
        {
            try
            {
                var args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-diceFreeSaveRoot");
                Require(index >= 0 && index + 1 < args.Length && Path.IsPathRooted(args[index + 1]), "Q3 needs isolated root.");
                var store = new LocalEchoStore(args[index + 1]); Require(store.Load() == null, "Q3 needs a fresh root.");
                CornbergValidation.ValidateNavigation();
                // A genuine pre-Q3 representation: schema/record v2, completed Q1/Q2, no Q3 record.
                var actor = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
                var definitions = actor.GetComponent<QuestJournal>().Definitions;
                var prior = definitions.Where(q => q.stableId == "quest.cornberg.crop-slimes" ||
                    q.stableId == "quest.cornberg.investigate-road").Select(q => new QuestProgress {
                    questId = q.stableId, definitionVersion = q.version, status = QuestStatus.Completed,
                    stage = q.stages.Length - 1, count = q.stages.Last().count }).ToList();
                prior.Add(new QuestProgress { questId = "quest.future-unknown", definitionVersion = 99, stage = 12, count = 9 });
                var data = new ManifestationSave { classId = actor.GetComponent<ActorStats>().Definition.stableId,
                    level = 5, xp = 0, anchorId = "anchor.cornberg", quests = prior.ToArray() };
                var save = new EchoSave { schemaVersion = 2, revision = 1 }; echoId = save.echoId;
                save.sections.Add(new SaveSection { id = "manifestation:" + data.classId, version = 2, json = JsonUtility.ToJson(data) });
                SaveMigrationValidation.WriteFixture(store.Path, save);
                SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Running, false) || state != PlayModeStateChange.EnteredPlayMode) return;
            reports = 0; Time.timeScale = 5; deadline = Time.realtimeSinceStartup + 240;
            DefeatEvents.Reported += OnDefeat; Application.logMessageReceived += OnLog;
            routine = Flow(); EditorApplication.update += Tick;
        }
        private static void OnDefeat(ActorDefeated report)
        {
            if (report.contentId != "enemy.named-forest-slime") return;
            Require(report.familyId == "enemy-family.slime" && report.creditOwner == player, "Q3 defeat attribution."); reports++;
        }
        private static IEnumerator Bind()
        {
            do { yield return null; persistence = UnityEngine.Object.FindAnyObjectByType<ManifestationPersistence>(); }
            while (persistence == null || !persistence.Ready);
            player = persistence.GetComponent<CombatActor>(); journal = player.GetComponent<QuestJournal>(); xp = player.GetComponent<ExperienceProgression>();
            named = CombatValidationActors.Find("enemy.named-forest-slime"); giver = CombatValidationActors.OriginalFarmer();
            quest = journal.Definitions.Single(q => q.stableId == "quest.cornberg.named-slime");
            area = UnityEngine.Object.FindObjectsByType<ReachArea>().Single(a => a.StableId == "area.cornberg.forest-clearing");
        }
        private static void Kill(CombatActor victim)
        { victim.Health.ApplyDamage(player, new DamageResult { mitigated = 10000 }); victim.Health.ApplyDamage(player, new DamageResult { mitigated = 10000 }); }
        private static IEnumerator Checkpoint(string label)
        {
            var state = State; int level = xp.Level, remainder = xp.CurrentXp, before = reports;
            Require(persistence.Flush(), "Q3 save failed.");
            EditorSceneManager.LoadSceneInPlayMode(CornbergSceneBuilder.ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            for (var wait = Bind(); wait.MoveNext();) yield return null;
            Require(State.status == state.status && State.stage == state.stage && State.count == state.count && xp.Level == level && xp.CurrentXp == remainder,
                "Q3 reload lost stage/reward: " + label);
            Require(reports == before && persistence.CaptureProfile().echoId == echoId && journal.GetProgress("quest.future-unknown").stage == 12,
                "Q3 reload replaced identity, lost unknown data or replayed credit.");
            Require(player.Alive && player.Health.Current == player.Health.Maximum && player.GetComponent<RespawnAtAnchor>().AnchorId == "anchor.cornberg", "Q3 reload spawn/HP.");
            Debug.Log("Q3_CHECKPOINT_OK " + label);
        }
        private static IEnumerator Flow()
        {
            for (var wait = Bind(); wait.MoveNext();) yield return null;
            Require(State.status == QuestStatus.Available && xp.Level == 5 && xp.CurrentXp == 0, "Pre-Q3 profile failed to acquire unstarted Q3.");
            var q2 = journal.Definitions.Single(q => q.stableId == "quest.cornberg.investigate-road");
            var snapshot = journal.CaptureState();
            var locked = journal.CaptureState(); var previous = Array.Find(locked, q => q.questId == q2.stableId);
            previous.status = QuestStatus.Available; previous.stage = previous.count = 0; journal.RestoreState(locked);
            Require(!journal.CanAccept(quest) && !journal.Accept(quest), "Q3 unlocked before Q2 completion."); journal.RestoreState(snapshot);
            Require(!giver.Accept(player, quest), "Remote NPC acceptance allowed.");
            player.Motor.Teleport(area.transform.position); yield return null; yield return null;
            Kill(named); Require(reports == 1 && xp.CurrentXp == 35 && State.status == QuestStatus.Available, "Pre-accept kill policy.");
            named.GetComponent<AggroBehaviour>().ResetEncounter(); xp.RestoreState(5, 0);
            var interaction = player.GetComponent<Interactor>(); Require(interaction.Order(giver), "NPC approach failed.");
            while (interaction.Active != giver) { Require(player.Alive, "Approach died."); yield return null; }
            Require(giver.CurrentQuest(player) == quest && giver.Accept(player, quest) && !giver.Accept(player, quest), "NPC Q3 offer/acceptance.");
            Require(State.stage == 0, "Pre-accept area credited retroactively.");
            for (var wait = Checkpoint("accepted"); wait.MoveNext();) yield return null;
            Require(player.Motor.MoveTo(area.transform.position), "Forest path not reachable.");
            while (State.stage == 0) { Require(player.Alive, "Forest approach died."); yield return null; }
            player.Motor.Stop(); Require(State.stage == 1 && State.count == 0, "Locate objective failed.");
            player.Motor.Teleport(giver.ApproachPosition); yield return null;
            for (var wait = Checkpoint("located"); wait.MoveNext();) yield return null;
            Kill(CombatValidationActors.Find("enemy.crop-slime")); Kill(CombatValidationActors.Find("enemy.road-slime"));
            Require(State.count == 0 && xp.CurrentXp == 30, "Wrong Slime advanced Q3 or XP pipeline changed."); xp.RestoreState(5, 0);
            Require(named.Stats.Level == 5 && named.Health.Maximum == 60 && named.Stats.Definition.basicAttack.element == null, "Named authored stats.");
            player.Stats.SetLevel(20); Require(named.Stats.Level == 5 && named.Health.Maximum == 60, "Named scaled to player."); xp.RestoreState(5, 0); player.Health.Restore();
            var brain = named.GetComponent<AggroBehaviour>(); int hits = named.GetComponent<BasicAttack>().Hits, before = reports;
            Require(player.Motor.Teleport(brain.Home + Vector3.left * 3), "Named duel placement.");
            player.GetComponent<BasicAttack>().Order(named);
            while (named.Alive) { Require(player.Alive, "Level-five Novice lost named duel."); yield return null; }
            Require(player.Alive && named.GetComponent<BasicAttack>().Hits > hits && reports == before + 1 && State.status == QuestStatus.ReadyToTurnIn && xp.CurrentXp == 35,
                "Named actual duel, credit or reward failed.");
            Kill(named); Require(reports == before + 1 && xp.CurrentXp == 35, "Corpse duplicate credit.");
            var identity = named.GetEntityId(); int actors = CombatActor.All.Count; float died = Time.time;
            player.Motor.Teleport(new Vector3(8, 0, 6));
            while (!named.Alive) yield return null;
            Require(named.GetEntityId().Equals(identity) && CombatActor.All.Count == actors && named.Health.Current == named.Health.Maximum &&
                Time.time - died >= 9.8f && !named.InCombat && named.GetComponent<BasicAttack>().Target == null, "Named clean timed respawn.");
            for (var wait = Checkpoint("ready"); wait.MoveNext();) yield return null;
            Require(!giver.TurnIn(player, quest), "Remote turn-in allowed.");
            interaction = player.GetComponent<Interactor>(); Require(interaction.Order(giver), "Return approach failed.");
            while (interaction.Active != giver) yield return null;
            Require(giver.TurnIn(player, quest) && !giver.TurnIn(player, quest) && xp.Level == 6 && xp.CurrentXp == 0, "Q3 turn-in/level-six pacing.");
            Require(giver.CurrentQuest(player) == quest, "Completed conversation jumped back to Q1.");
            for (var wait = Checkpoint("completed"); wait.MoveNext();) yield return null;
            Require(!journal.Accept(quest) && !journal.TurnIn(quest) && xp.Level == 6 && xp.CurrentXp == 0, "Q3 reward replay.");
            Kill(named); Require(State.status == QuestStatus.Completed && State.count == 1 && xp.CurrentXp == 35, "Post-completion credit policy.");
            xp.RestoreState(6, 0); Require(persistence.Flush(), "Final Q3 fixture save.");
            Debug.Log("CORNBERG_Q3_PLAYMODE_OK: old profile, NPC offer/approach, locate, named duel, wrong/pre/post credit, respawn, level six, four reload checkpoints.");
        }
        private static void Tick()
        {
            try { Require(Time.realtimeSinceStartup < deadline, "Q3 validation timeout."); if (!routine.MoveNext()) Finish(0); }
            catch (Exception error) { Debug.LogException(error); Finish(1); }
        }
        private static void OnLog(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;
            if (trace.Contains("UnityEditor.Search.SearchDatabase")) return;
            Finish(1);
        }
        private static void Finish(int code)
        {
            SessionState.SetBool(Running, false); EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog;
            DefeatEvents.Reported -= OnDefeat; Time.timeScale = 1; EditorApplication.Exit(code);
        }
    }
}
