using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DiceFree.AI;
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
    public static class CornbergRoadValidation
    {
        private const string Running = "DiceFree.RoadValidation";
        private static CombatActor player, crop, road;
        private static QuestJournal journal;
        private static QuestDefinition q1, q2;
        private static QuestGiver giver;
        private static ReachArea area;
        private static ExperienceProgression xp;
        private static ManifestationPersistence persistence;
        private static IEnumerator routine;
        private static float deadline;
        private static int defeats, entries;
        private static readonly HashSet<long> defeatIds = new();
        static CornbergRoadValidation() => EditorApplication.playModeStateChanged += OnPlay;
        public static void Run()
        {
            try
            {
                var args = Environment.GetCommandLineArgs(); int index = Array.IndexOf(args, "-diceFreeSaveRoot");
                Require(index >= 0 && index + 1 < args.Length && Path.IsPathRooted(args[index + 1]), "Q2 validation needs isolated save root.");
                Require(!File.Exists(Path.Combine(args[index + 1], "primary-echo.json")), "Q2 validation needs a fresh test profile.");
                CornbergValidation.ValidateNavigation(); SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Running, false) || state != PlayModeStateChange.EnteredPlayMode) return;
            defeats = entries = 0; defeatIds.Clear(); Time.timeScale = 5; deadline = Time.realtimeSinceStartup + 240;
            DefeatEvents.Reported += OnDefeat; AreaEvents.Entered += OnArea;
            Application.logMessageReceived += OnLog; routine = Flow(); EditorApplication.update += Tick;
        }
        private static void OnDefeat(ActorDefeated value)
        {
            if (value.contentId != "enemy.road-slime") return;
            Require(value.familyId == "enemy-family.slime" && value.creditOwner == player && defeatIds.Add(value.sequence), "Road semantic credit invalid/duplicated.");
            defeats++;
        }
        private static void OnArea(AreaEntered value) { if (value.actor == player && value.areaId == area.StableId) entries++; }
        private static IEnumerator Bind()
        {
            do { yield return null; persistence = UnityEngine.Object.FindAnyObjectByType<ManifestationPersistence>(); }
            while (persistence == null || !persistence.Ready);
            player = persistence.GetComponent<CombatActor>(); xp = player.GetComponent<ExperienceProgression>(); journal = player.GetComponent<QuestJournal>();
            crop = CombatValidationActors.Find("enemy.crop-slime"); road = CombatValidationActors.Find("enemy.road-slime");
            giver = UnityEngine.Object.FindAnyObjectByType<QuestGiver>(); area = UnityEngine.Object.FindAnyObjectByType<ReachArea>();
            q1 = giver.Quest;
            foreach (var definition in journal.Definitions) if (definition.stableId == "quest.cornberg.investigate-road") q2 = definition;
        }
        private static QuestProgress State => journal.GetProgress(q2.stableId);
        private static void Fatal(CombatActor actor)
        {
            actor.Health.ApplyDamage(player, new DamageResult { mitigated = 10000 });
            actor.Health.ApplyDamage(player, new DamageResult { mitigated = 10000 });
        }
        private static IEnumerator Checkpoint(string label)
        {
            var state = State; int level = xp.Level, remainder = xp.CurrentXp, reports = defeats;
            string echo = persistence.CaptureProfile().echoId;
            Require(persistence.Flush(), "Q2 checkpoint save failed: " + label);
            EditorSceneManager.LoadSceneInPlayMode(CornbergSceneBuilder.ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            for (var wait = Bind(); wait.MoveNext();) yield return null;
            var loaded = State;
            Require(loaded.status == state.status && loaded.stage == state.stage && loaded.count == state.count && xp.Level == level && xp.CurrentXp == remainder,
                "Q2 checkpoint lost progression: " + label);
            Require(persistence.CaptureProfile().echoId == echo && defeats == reports, "Reload replayed credit or replaced profile.");
            Require(player.Health.Current == player.Health.Maximum && Vector3.Distance(player.transform.position, new Vector3(8,0,6)) < 1,
                "Q2 reload was not full HP at registered anchor.");
            Debug.Log("Q2_CHECKPOINT_OK " + label);
        }
        private static IEnumerator Flow()
        {
            for (var wait = Bind(); wait.MoveNext();) yield return null;
            Require(xp.Level == 1 && !journal.CanAccept(q2) && !journal.Accept(q2), "Q2 unlocked before Q1.");
            var cropVariant = crop.GetComponent<EnemyVariant>().Definition;
            var roadVariant = road.GetComponent<EnemyVariant>().Definition;
            Require(cropVariant.archetype == roadVariant.archetype && crop.FamilyId == road.FamilyId &&
                crop.Stats.Definition != road.Stats.Definition && road.Stats.Level == 3 && road.Health.Maximum == 30 &&
                road.Stats.Definition.basicAttack.baseDamage == 6 && road.Stats.Definition.basicAttack.element == null,
                "Authored Slime family/variant boundary failed.");
            player.Stats.SetLevel(20);
            Require(road.Stats.Level == 3 && road.Health.Maximum == 30, "Road Slime scaled to player.");
            xp.RestoreState(1, 0);
            Require(player.Motor.Teleport(area.transform.position), "Pre-accept area placement.");
            while (entries == 0) yield return null;
            Require(State.stage == 0 && State.count == 0, "Pre-accept area credit was retroactive.");
            player.Motor.Teleport(giver.ApproachPosition); yield return null;
            Require(giver.Accept(player, q1), "Q1 acceptance failed.");
            for (int i = 0; i < 5; i++) { crop.GetComponent<AggroBehaviour>().ResetEncounter(); Fatal(crop); }
            Require(giver.TurnIn(player, q1) && xp.Level == 3 && xp.CurrentXp == 0 && journal.CanAccept(q2), "Q1 -> Q2 unlock/XP failed.");
            Fatal(road);
            Require(xp.CurrentXp == 20 && State.count == 0 && defeats == 1, "Road XP outside quest/duplicate corpse credit failed.");
            road.GetComponent<AggroBehaviour>().ResetEncounter(); xp.RestoreState(3, 0); // Separate pre-quest XP check from fresh-route balance.
            Require(giver.Accept(player, q2) && !giver.Accept(player, q2) && State.stage == 0, "Q2 acceptance or pre-entry history failed.");
            for (var wait = Checkpoint("accepted"); wait.MoveNext();) yield return null;
            var wrongArea = new GameObject("Wrong-area fixture"); wrongArea.transform.position = player.transform.position;
            wrongArea.AddComponent<ReachArea>().Configure("area.wrong", 4); yield return null; yield return null;
            Require(State.stage == 0, "Wrong area advanced Q2."); UnityEngine.Object.Destroy(wrongArea);
            int beforeEntry = entries;
            Require(player.Motor.MoveTo(area.transform.position), "Investigation road path failed.");
            while (State.stage == 0) yield return null;
            player.Motor.Stop();
            Require(entries == beforeEntry + 1 && State.stage == 1 && State.count == 0, "ReachArea did not advance exactly one objective.");
            yield return null; yield return null;
            Require(entries == beforeEntry + 1, "Remaining inside repeated entry credit.");
            player.Motor.Teleport(giver.ApproachPosition); yield return null; yield return null;
            player.Motor.Teleport(area.transform.position);
            while (entries == beforeEntry + 1) yield return null;
            Require(State.stage == 1 && State.count == 0, "Repeated enter/exit duplicated one-time objective progress.");
            for (var wait = Checkpoint("investigated"); wait.MoveNext();) yield return null;
            Fatal(crop);
            Require(State.count == 0 && xp.CurrentXp == 10, "Crop Slime incorrectly counted as Road Slime.");
            xp.RestoreState(3, 0); // Restore route balance after independent wrong-content XP check.
            for (int kill = 1; kill <= 3; kill++)
            {
                int before = defeats;
                if (kill == 1)
                {
                    var brain = road.GetComponent<AggroBehaviour>(); var bump = road.GetComponent<BasicAttack>();
                    int hits = bump.Hits;
                    Require(player.Motor.Teleport(brain.Home + Vector3.left * 3), "Road duel placement.");
                    while (bump.Hits < hits + 2) yield return null;
                    Require(player.Health.Current < player.Health.Maximum && player.InCombat, "Road aggro/damage missing.");
                    player.GetComponent<BasicAttack>().Order(road);
                    while (road.Alive) { Require(player.Alive, "Level-three Novice lost Road duel."); yield return null; }
                }
                else Fatal(road);
                road.Health.ApplyDamage(player, new DamageResult { mitigated = 10000 });
                Require(defeats == before + 1 && State.count == kill, "Road kill credited more than once.");
                Require((kill < 3 && xp.Level == 3 && xp.CurrentXp == 20 * kill) || (kill == 3 && xp.Level == 4 && xp.CurrentXp == 10), "Road XP duplicated/lost.");
                if (kill == 1)
                {
                    object identity = road.GetEntityId(); float died = Time.time;
                    player.Motor.Teleport(giver.ApproachPosition);
                    while (!road.Alive) yield return null;
                    Require(Time.time - died >= road.GetComponent<OverworldRespawn>().Definition.delaySeconds - 0.2f &&
                        road.GetEntityId().Equals(identity) && CombatActor.All.Count == 3 && road.Health.Current == road.Health.Maximum &&
                        !road.InCombat && !road.GetComponent<AggroBehaviour>().Returning && road.GetComponent<BasicAttack>().CooldownRemaining == 0,
                        "Road timed respawn did not cleanly reuse the actor.");
                    // Same respawned actor must produce another valid report, separately from route tuning.
                    Fatal(road); Require(defeats == before + 2, "Respawn did not permit later defeat credit.");
                    journal.RestoreState(new[] { journal.GetProgress(q1.stableId), new QuestProgress { questId = q2.stableId,
                        definitionVersion = q2.version, status = QuestStatus.Active, stage = 1, count = 1 } });
                    xp.RestoreState(3, 20);
                }
                for (var wait = Checkpoint(kill == 3 ? "ready" : "kill-" + kill); wait.MoveNext();) yield return null;
            }
            Require(State.status == QuestStatus.ReadyToTurnIn && !giver.TurnIn(player, q2), "Q2 ready state or remote turn-in failed.");
            var interaction = player.GetComponent<Interactor>(); interaction.Order(giver);
            while (interaction.Active != giver) yield return null;
            Require(giver.CurrentQuest(player) == q2 && giver.TurnIn(player, q2) && !giver.TurnIn(player, q2), "Q2 turn-in not once-only.");
            Require(xp.Level == 5 && xp.CurrentXp == 0 && player.Health.Maximum == 85, "Q1 -> Q2 route did not reach level five.");
            for (var wait = Checkpoint("completed"); wait.MoveNext();) yield return null;
            Require(!journal.TurnIn(q2) && !journal.Accept(q2) && xp.Level == 5 && xp.CurrentXp == 0, "Completed Q2 replayed after reload.");
            Fatal(road);
            Require(xp.CurrentXp == 20 && State.status == QuestStatus.Completed && State.count == 3, "Post-quest road XP/progress failed.");
            Debug.Log("CORNBERG_Q2_PLAYMODE_OK: unlock, semantic area, variants, duel, credit/XP, respawn, level five and six scene-reload checkpoints.");
        }
        private static void Tick()
        {
            try { Require(Time.realtimeSinceStartup < deadline, "Q2 validation timeout."); if (!routine.MoveNext()) Finish(0); }
            catch (Exception error) { Debug.LogException(error); Finish(1); }
        }
        private static void OnLog(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;
            if (trace.Contains("UnityEditor.Search.SearchDatabase")) return;
            File.AppendAllText("Logs/Cornberg/road-errors.txt", message + "\n" + trace + "\n"); Finish(1);
        }
        private static void Finish(int code)
        {
            SessionState.SetBool(Running, false); EditorApplication.update -= Tick; Application.logMessageReceived -= OnLog;
            DefeatEvents.Reported -= OnDefeat; AreaEvents.Entered -= OnArea; Time.timeScale = 1; EditorApplication.Exit(code);
        }
    }
}
