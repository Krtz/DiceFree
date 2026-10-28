using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class BriarbackElementalQuestPlaytest
    {
        private const string Key = "DiceFree.Briarback.ElementsQuest.";
        private static readonly Stack<IEnumerator> Steps = new();
        private static double deadline;
        private static int checks;

        [Serializable]
        private sealed class Report
        {
            public string status, error;
            public int checks, outOfOrderKills, orderedElementKills;
            public bool nearbyPartyCredit, turnInRequired, completionPersisted, isolatedSave;
            public int xpAwarded;
            public long goldAwarded;
        }

        private static Report report;
        static BriarbackElementalQuestPlaytest() => EditorApplication.playModeStateChanged += Changed;

        private static void Need(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            checks++;
        }

        [CliCommand("dicefree.briarback.elements-quest.test",
            "Playtest four ordered Shaman kills, nearby-party credit, required scholar turn-in and isolated-save persistence.")]
        public static object Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before starting this isolated quest test.");
            SessionState.SetString(Key + "scene", EditorSceneManager.GetActiveScene().path);
            SessionState.SetBool(Key + "options", EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetInt(Key + "flags", (int)EditorSettings.enterPlayModeOptions);
            SessionState.SetBool(Key + "cleanup", true);
            SessionState.SetString(Key + "status", "running");
            SessionState.SetString(Key + "error", "");
            SessionState.SetInt(Key + "checks", 0);
            var saveRoot = Path.Combine(Path.GetTempPath(), "DiceFree-Briarback-Elements-" + Guid.NewGuid().ToString("N"));
            SessionState.SetString(Key + "root", saveRoot);
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(saveRoot);
            EditorSettings.enterPlayModeOptionsEnabled = false;
            EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.briarback.elements-quest.status",
            "Read the isolated four-element quest Play Mode test status.")]
        public static object Status() => new
        {
            status = SessionState.GetString(Key + "status", "idle"),
            checks = SessionState.GetInt(Key + "checks", 0),
            error = SessionState.GetString(Key + "error", "")
        };

        private static void Changed(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetString(Key + "status", "") == "running")
            {
                checks = 0;
                report = new Report { isolatedSave = true };
                Steps.Clear();
                Steps.Push(Flow());
                deadline = EditorApplication.timeSinceStartup + 180;
                EditorApplication.update += Tick;
                Application.logMessageReceived += OnLog;
            }
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Key + "cleanup", false))
            {
                SessionState.SetBool(Key + "cleanup", false);
                EditorApplication.update -= Tick;
                Application.logMessageReceived -= OnLog;
                PersistenceTestGuard.ClearValidationOverrides();
                EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)SessionState.GetInt(Key + "flags", 0);
                EditorSettings.enterPlayModeOptionsEnabled = SessionState.GetBool(Key + "options", false);
                var scene = SessionState.GetString(Key + "scene", "");
                if (!string.IsNullOrWhiteSpace(scene)) EditorSceneManager.OpenScene(scene);
            }
        }

        private static void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception) Finish(false, message + "\n" + stack);
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            try
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Elemental quest test timed out.");
                while (Steps.Count > 0)
                {
                    var step = Steps.Peek();
                    if (!step.MoveNext()) { Steps.Pop(); continue; }
                    if (step.Current is IEnumerator child) { Steps.Push(child); continue; }
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
            SessionState.SetString(Key + "status", report.status);
            SessionState.SetString(Key + "error", error ?? "");
            SessionState.SetInt(Key + "checks", checks);
            File.WriteAllText("Library/BriarbackElementalQuestResults.json", JsonUtility.ToJson(report, true));
            EditorApplication.ExitPlaymode();
        }

        private static IEnumerator Frames(int count)
        {
            for (var i = 0; i < count; i++) yield return null;
        }

        private static void Defeat(CombatActor source, CombatActor victim)
        {
            victim.Health.Invulnerable = false;
            victim.Health.ApplyPacket(source, new DamageResult { mitigated = 1000000 });
            Need(!victim.Alive, "Elemental guardian did not enter its ordinary defeated state: " + victim.name);
        }

        private static IEnumerator Flow()
        {
            CombatActor player = null;
            while (player == null)
            {
                player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();
                yield return null;
            }
            var save = player.GetComponent<ManifestationPersistence>();
            while (!save.Ready || !player.Motor.Ready) yield return null;
            player.GetComponent<TraversalInput>().enabled = false;
            player.GetComponent<CombatInput>().enabled = false;
            player.GetComponent<BasicAttack>().enabled = false;
            player.Health.Invulnerable = true;

            var patrolGroup = GameObject.Find("Briarback road patrols - additive");
            if (patrolGroup != null) patrolGroup.SetActive(false);
            var guardianGroup = GameObject.Find("Briarback elemental guardians - additive");
            Need(guardianGroup != null, "Elemental guardian group is missing in Play Mode.");
            foreach (var c in UnityEngine.Object.FindObjectsByType<QuilboarCombat>())
            {
                var attack = c.GetComponent<BasicAttack>();
                if (attack != null) attack.enabled = false;
                var aggro = c.GetComponent<AggroBehaviour>();
                if (aggro != null) aggro.enabled = false;
                c.enabled = false;
            }

            var guardians = guardianGroup.GetComponentsInChildren<QuilboarCombat>()
                .ToDictionary(value => value.role, value => value.GetComponent<CombatActor>());
            var giver = UnityEngine.Object.FindObjectsByType<QuestGiver>()
                .Single(value => value.name == "Trimery Elemental Scholar - Briarback");
            var quest = giver.Quest;
            var journal = player.GetComponent<QuestJournal>();
            Need(journal.CanAccept(quest), "Fresh isolated character could not accept the scholar quest.");
            Need(NavMesh.SamplePosition(giver.ApproachPosition, out var approach, 2, NavMesh.AllAreas),
                "Scholar interaction point has no live NavMesh coverage.");
            Need(player.Motor.Teleport(approach.position), "Could not reach scholar on NavMesh.");
            Physics.SyncTransforms();
            yield return Frames(2);
            Need(giver.CanInteract(player) && giver.Accept(player, quest), "Scholar quest interaction/acceptance failed.");
            var progress = journal.GetProgress(quest.stableId);
            Need(progress.status == QuestStatus.Active && progress.stage == 0 && progress.count == 0,
                "Quest did not begin at the Fire stage.");

            // Credit the test kill to a separate same-party CombatActor. This exercises
            // the existing NearbyParty policy instead of a direct-owner shortcut.
            var roleGroup = GameObject.Find("Approved Quillboar role prototypes - additive");
            Need(roleGroup != null, "The approved Warrior credit fixture is missing.");
            var warrior = roleGroup.GetComponentsInChildren<QuilboarCombat>(true)
                .Single(value => value.role == QuilboarRole.Warrior);
            var partyOwner = warrior.GetComponent<CombatActor>();
            var playerParty = player.GetComponent<LocalPartyMember>() ?? player.gameObject.AddComponent<LocalPartyMember>();
            var ownerParty = partyOwner.GetComponent<LocalPartyMember>() ?? partyOwner.gameObject.AddComponent<LocalPartyMember>();
            playerParty.partyId = "briarback-elements-validation";
            ownerParty.partyId = playerParty.partyId;

            Need(NavMesh.SamplePosition(guardians[QuilboarRole.IceShaman].transform.position + Vector3.forward * 2,
                    out var icePoint, 3, NavMesh.AllAreas) && player.Motor.Teleport(icePoint.position),
                "Could not position the local party member within the Ice guardian's credit radius.");
            // The out-of-order kill is intentionally valid credit proximity but
            // must not satisfy the currently selected Fire stage.
            Physics.SyncTransforms();
            Defeat(partyOwner, guardians[QuilboarRole.IceShaman]);
            report.outOfOrderKills++;
            progress = journal.GetProgress(quest.stableId);
            Need(progress.status == QuestStatus.Active && progress.stage == 0 && progress.count == 0,
                "Out-of-order Ice Shaman kill was incorrectly counted at the Fire stage.");

            var ordered = new[] { QuilboarRole.FireShaman, QuilboarRole.EarthShaman,
                QuilboarRole.LightningShaman, QuilboarRole.IceShaman };
            for (int i = 0; i < ordered.Length; i++)
            {
                var target = guardians[ordered[i]];
                Need(NavMesh.SamplePosition(target.transform.position + Vector3.forward * 2,
                        out var killPoint, 3, NavMesh.AllAreas) && player.Motor.Teleport(killPoint.position),
                    "Could not place the local party member within the " + ordered[i] + " credit radius.");
                Physics.SyncTransforms();
                target.Health.Restore();
                Defeat(partyOwner, target);
                report.orderedElementKills++;
                progress = journal.GetProgress(quest.stableId);
                if (i < ordered.Length - 1)
                    Need(progress.status == QuestStatus.Active && progress.stage == i + 1 && progress.count == 0,
                        "Elemental stage did not advance in order after " + ordered[i] +
                        "; status=" + progress.status + ", stage=" + progress.stage + ", count=" + progress.count);
                else
                    Need(progress.status == QuestStatus.ReadyToTurnIn && progress.stage == ordered.Length - 1 && progress.count == 1,
                        "Four qualifying party kills did not ready the scholar quest for turn-in.");
            }
            report.nearbyPartyCredit = true;
            Need(!giver.TurnIn(player, quest), "Quest awarded without returning to the scholar.");
            report.turnInRequired = true;

            Need(player.Motor.Teleport(approach.position), "Could not return to the scholar.");
            Physics.SyncTransforms();
            yield return Frames(2);
            var xp = player.GetComponent<ExperienceProgression>();
            var wallet = player.GetComponent<GoldWallet>();
            int beforeLevel = xp.Level, beforeXp = xp.CurrentXp;
            long beforeGold = wallet.Gold;
            Need(giver.CanInteract(player) && giver.TurnIn(player, quest), "Quest turn-in at the Trimery scholar failed.");
            progress = journal.GetProgress(quest.stableId);
            Need(progress.status == QuestStatus.Completed, "Turn-in did not permanently complete the character quest.");
            Need(wallet.Gold == beforeGold + 50, "Scholar quest gold tuning placeholder was not granted exactly once.");
            Need(xp.Level > beforeLevel || xp.CurrentXp - beforeXp == 400,
                "Scholar quest XP tuning placeholder was not granted.");
            Need(!giver.TurnIn(player, quest) && wallet.Gold == beforeGold + 50,
                "Completed one-time scholar quest granted rewards twice.");
            Need(save.Flush(), "Completed scholar quest did not flush to the isolated save.");
            var disk = new LocalEchoStore(Path.GetDirectoryName(save.SavePath)).Load();
            var manifestation = ManifestationRoster.ReadManifestation(disk, "class.novice");
            var saved = manifestation.quests.Single(value => value.questId == quest.stableId);
            Need(saved.status == QuestStatus.Completed && saved.definitionVersion == quest.version,
                "Completed four-element eligibility marker did not persist in the character save.");
            report.completionPersisted = true;
            report.xpAwarded = 400;
            report.goldAwarded = 50;
        }
    }
}
