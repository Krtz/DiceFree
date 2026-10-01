using System;
using System.IO;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class CornbergSaveValidation
    {
        private const string Running = "DiceFree.SaveValidation";
        private static float deadline;
        private static bool autosaveRequested;
        static CornbergSaveValidation() => EditorApplication.playModeStateChanged += OnPlay;
        public static void Run()
        {
            try
            {
                ValidateStore();
                SaveMigrationValidation.Run();
                var args = Environment.GetCommandLineArgs();
                if (Array.IndexOf(args, "-diceFreeVerifyLegacyReload") >= 0)
                {
                    int index = Array.IndexOf(args, "-diceFreeSaveRoot");
                    Require(index >= 0 && index + 1 < args.Length && Path.IsPathRooted(args[index + 1]), "Legacy test requires isolated root.");
                    Directory.CreateDirectory(args[index + 1]);
                    File.Copy(SaveMigrationValidation.Fixture, Path.Combine(args[index + 1], "primary-echo.json"));
                }
                CornbergValidation.ValidateNavigation();
                SessionState.SetBool(Running, true); EditorApplication.EnterPlaymode();
            }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }
        private static void ValidateStore()
        {
            var root = Path.Combine(Path.GetTempPath(), "DiceFree-save-test-" + Guid.NewGuid().ToString("N"));
            var store = new LocalEchoStore(root);
            var save = new EchoSave();
            Require(store.Load() == null, "Empty storage must not fabricate a saved Echo.");
            save.sections.Add(new SaveSection { id = "future-content", version = 27, json = "{\"opaque\":true}" });
            for (int i = 1; i <= 4; i++) { save.revision = i; store.Commit(save); }
            var loaded = store.Load();
            Require(loaded.revision == 4 && loaded.echoId == save.echoId && loaded.userId == save.userId, "Identity/revision did not round-trip.");
            Require(loaded.sections[0].json == save.sections[0].json, "Unknown section lost.");
            bool staleRejected = false;
            try { store.Commit(loaded); } catch (InvalidOperationException) { staleRejected = true; }
            Require(staleRejected && store.Load().revision == 4, "Stale writer overwrote a committed revision.");
            Require(File.Exists(store.Path + ".bak3"), "Three rotating backups missing.");
            File.WriteAllText(store.Path + ".pending", "interrupted candidate");
            Require(store.Load().revision == 4, "Uncommitted candidate was loaded.");
            File.WriteAllText(store.Path, "corrupt primary");
            Require(store.Load().revision == 3 && store.RecoveryMessage != null, "Backup recovery failed.");
            save.revision = 5; store.Commit(save);
            Require(store.Load().revision == 5 && File.Exists(store.Path + ".bak1"), "Recovery write destroyed valid backup.");
            bool rejected = false;
            save.schemaVersion = 99; save.revision = 6;
            try { store.Commit(save); } catch (NotSupportedException) { rejected = true; }
            Require(rejected && store.Load().revision == 5, "Unsupported schema damaged committed revision.");
            foreach (var suffix in new[] { "", ".bak1", ".bak2", ".bak3" }) File.WriteAllText(store.Path + suffix, "corrupt");
            rejected = false;
            try { store.Load(); } catch (InvalidDataException) { rejected = true; }
            Require(rejected, "Entirely unreadable profile must fail closed, not create a new Echo.");
            Debug.Log("SAVE_STORAGE_OK: atomic candidate, checksum recovery, backups, unknown sections and unsupported schema.");
        }
        private static void OnPlay(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Running, false) || state != PlayModeStateChange.EnteredPlayMode) return;
            deadline = Time.realtimeSinceStartup + 30;
            autosaveRequested = false;
            Application.logMessageReceived += OnLog;
            EditorApplication.update += Tick;
        }
        private static void OnLog(string message, string stack, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error && type != LogType.Assert) return;
            if (stack.Contains("UnityEditor.Search.SearchDatabase")) return; // Existing issue #17 only.
            SessionState.SetBool(Running, false); EditorApplication.update -= Tick; EditorApplication.Exit(1);
        }
        private static void Tick()
        {
            try
            {
                var player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
                var persistence = player.GetComponent<ManifestationPersistence>();
                if (!persistence.Ready)
                {
                    Require(Time.realtimeSinceStartup < deadline, "Save initialization timed out; use an isolated -diceFreeSaveRoot.");
                    return;
                }
                var xp = player.GetComponent<ExperienceProgression>();
                var journal = player.GetComponent<QuestJournal>();
                var quest = journal.Definitions[0];
                var args = Environment.GetCommandLineArgs();
                bool deadReload = Array.IndexOf(args, "-diceFreeVerifyDeadReload") >= 0;
                bool legacyReload = Array.IndexOf(args, "-diceFreeVerifyLegacyReload") >= 0;
                bool reload = Array.IndexOf(args, "-diceFreeVerifyReload") >= 0 || deadReload || legacyReload;
                if (!reload)
                {
                    if (!File.Exists(persistence.SavePath))
                    {
                        Require(Time.realtimeSinceStartup < deadline, "Initial autosave timed out.");
                        return;
                    }
                    if (!autosaveRequested)
                    {
                        Require(xp.Level == 1 && xp.CurrentXp == 0, "Fresh profile not level one.");
                        journal.Accept(quest); xp.Grant(10); autosaveRequested = true;
                        return;
                    }
                    var autosaved = new LocalEchoStore(Path.GetDirectoryName(persistence.SavePath)).Load();
                    var autosavedState = JsonUtility.FromJson<ManifestationSave>(autosaved.sections[0].json);
                    if (autosavedState.xp != 10)
                    {
                        Require(Time.realtimeSinceStartup < deadline, "Event-driven autosave timed out.");
                        return;
                    }
                    Require(autosavedState.quests[0].status == QuestStatus.Active, "Autosave split quest and XP operation.");
                    xp.RestoreState(1, 0);
                    Require(xp.Level == 1 && xp.CurrentXp == 0, "Fresh profile not level one.");
                    Require(player.transform.position.x < -30, "Fresh Echo must retain emergence start.");
                    int rewards = 0; xp.LeveledUp += _ => rewards++;
                    xp.RestoreState(3, 7);
                    Require(rewards == 0 && player.Stats.Attributes.Highest == 3, "Restore replayed rewards or missed stats.");
                    var records = new[] {
                        new QuestProgress { questId = quest.stableId, definitionVersion = quest.version,
                            status = QuestStatus.Completed, stage = 1, count = 2 },
                        new QuestProgress { questId = "quest.unresolved", definitionVersion = 99,
                            status = QuestStatus.Active, stage = 41, count = 8 }
                    };
                    foreach (var state in new[] {
                        new QuestProgress { questId = quest.stableId, definitionVersion = quest.version, status = QuestStatus.Active, stage = 0, count = 2 },
                        new QuestProgress { questId = quest.stableId, definitionVersion = quest.version, status = QuestStatus.Active, stage = 1, count = 1 },
                        new QuestProgress { questId = quest.stableId, definitionVersion = quest.version, status = QuestStatus.ReadyToTurnIn, stage = 1, count = 2 }
                    })
                    {
                        journal.RestoreState(new[] { state });
                        Require(journal.GetProgress(quest.stableId).count == state.count && xp.CurrentXp == 7, "Quest restore changed XP or objective state.");
                    }
                    journal.RestoreState(records);
                    Require(!journal.TurnIn(quest), "Completed reward replayed after restore.");
                    records[0].count = 99;
                    Require(journal.GetProgress(quest.stableId).count == 2 && !journal.CanRestore(records), "Quest snapshot alias or invalid count accepted.");
                    player.Health.ApplyDamage(null, new DamageResult { mitigated = 5 });
                    Require(player.Alive && player.Health.Current < player.Health.Maximum, "Injured save fixture not injured.");
                    Require(persistence.Flush(), "Profile commit failed.");
                    // Exercise unknown-anchor fallback on the next real process launch.
                    var store = new LocalEchoStore(Path.GetDirectoryName(persistence.SavePath));
                    var save = store.Load();
                    var section = save.sections.Find(value => value.id == "manifestation:" + player.Stats.Definition.stableId);
                    var data = JsonUtility.FromJson<ManifestationSave>(section.json);
                    Require(!section.json.Contains("healthFraction"), "Transient HP was serialized.");
                    data.anchorId = "anchor.removed-content";
                    section.json = JsonUtility.ToJson(data);
                    save.revision++; store.Commit(save);
                    // Prevent shutdown callback from replacing this deliberate test fixture.
                    UnityEngine.Object.DestroyImmediate(persistence);
                    Debug.Log("SAVE_FIRST_PROCESS_OK");
                }
                else
                {
                    Require(xp.Level == 3 && xp.CurrentXp == 7 && player.Stats.Attributes.Highest == 3, "Progression reload failed.");
                    Require(journal.GetProgress(quest.stableId).status == QuestStatus.Completed && !journal.TurnIn(quest), "Quest reload/reward idempotency failed.");
                    Require(journal.GetProgress("quest.unresolved").stage == 41, "Unresolved quest lost.");
                    Require(Vector3.Distance(player.transform.position, new Vector3(8, 0, 6)) < 1, "Load did not use Cornberg fallback.");
                    Require(player.Alive && !player.InCombat && player.GetComponent<BasicAttack>().Target == null, "Transient combat state survived load.");
                    Require(Mathf.Approximately(player.Health.Current, player.Health.Maximum), "Loaded injured/dead/legacy manifestation was not full HP.");
                    var enemy = CombatValidationActors.Find("enemy.crop-slime");
                    Require(enemy.Health.Current == enemy.Health.Maximum && !enemy.InCombat, "Enemy session state resumed.");
                    var temporary = new GameObject("Validation registered anchor");
                    temporary.transform.position = new Vector3(5, 0, -8);
                    temporary.AddComponent<ResurrectionAnchor>().Configure("anchor.validation");
                    var respawn = player.GetComponent<RespawnAtAnchor>();
                    Require(player.Motor.Teleport(enemy.transform.position + Vector3.back), "Attack-reset fixture could not reach enemy.");
                    var attack = player.GetComponent<BasicAttack>();
                    attack.Order(enemy);
                    player.GetComponent<TargetSelection>().Select(enemy);
                    attack.SendMessage("Update"); // Exercise the real wind-up transition before applying load reset.
                    Require(attack.Target == enemy && player.GetComponent<TargetSelection>().Selected == enemy && attack.CooldownRemaining > 0,
                        "Load-reset fixture did not establish a target and cooldown.");
                    player.Stats.SetSecondaryModifier("test.load", new SecondaryScalingModifier { stat = SecondaryStat.AttackSpeed, add = 1 });
                    player.Stats.SetDefenseModifier("test.load", new DefenseModifier { buffFlat = 100 });
                    player.Stats.SetVitalityModifier("test.load", new VitalityModifier { hpPerVitalityAdd = 10 });
                    player.Stats.SetResistanceCapModifier("test.load", ResistanceCapModifier.Global(.5f));
                    Require(respawn.LoadAtAnchor("anchor.validation") && Vector3.Distance(player.transform.position, temporary.transform.position) < 1,
                        "Registered anchor resolution failed.");
                    ResistanceCapValidation.RequireDefaultCap(player.Stats);
                    Require(player.Health.Maximum == 55 && player.Health.Current == 55 &&
                        Mathf.Approximately(player.Stats.AttackSpeed, 1.00075f) && player.Stats.DefenseModifiers(DamageChannel.Physical).buffFlat == 0,
                        "Load retained transient stat modifiers.");
                    Require(attack.Target == null && attack.CooldownRemaining == 0 && player.GetComponent<TargetSelection>().Selected == null,
                        "Load retained attack/selection state.");
                    temporary.transform.position = new Vector3(10000, 10000, 10000);
                    Require(respawn.LoadAtAnchor("anchor.validation") && respawn.AnchorId == "anchor.cornberg", "Unnavigable saved anchor did not fall back.");
                    Require(respawn.LoadAtAnchor("anchor.unavailable") && Vector3.Distance(player.transform.position, new Vector3(8,0,6)) < 1,
                        "Fallback was replaced by the previous selected anchor.");
                    UnityEngine.Object.DestroyImmediate(temporary);
                    Require(persistence.Flush(), "Reloaded save could not commit.");
                    var inspection = ProfileInspection.Capture(persistence);
                    Require(inspection.userId == persistence.CaptureProfile().userId && inspection.echoId == persistence.CaptureProfile().echoId &&
                        inspection.schema == SaveMigrations.CurrentSchema && inspection.revision == persistence.Revision && inspection.level == 3 && inspection.xp == 7 &&
                        inspection.anchorId == "anchor.cornberg" && inspection.files[0].valid && inspection.files[1].exists &&
                        inspection.path == persistence.SavePath && inspection.autosaveReady && !string.IsNullOrEmpty(inspection.savedUtc) &&
                        inspection.quests[0].status == QuestStatus.Completed && inspection.preserved.Length > 0,
                        "Profile Inspector diagnostics do not match the loaded profile.");
                    if (deadReload)
                    {
                        var competingStore = new LocalEchoStore(Path.GetDirectoryName(persistence.SavePath));
                        var competing = competingStore.Load(); competing.revision++; competingStore.Commit(competing);
                        long lastSuccessful = persistence.Revision;
                        Require(!persistence.Flush(), "Stale runtime writer did not stop autosave.");
                        inspection = ProfileInspection.Capture(persistence);
                        Require(!inspection.autosaveReady && inspection.revision == lastSuccessful && inspection.status.Contains("Autosave stopped"),
                            "Inspector hides stopped autosave or reports uncommitted revision as successful.");
                    }
                    if (!deadReload && !legacyReload)
                    {
                        player.Health.ApplyDamage(null, new DamageResult { mitigated = 10000 });
                        Require(!player.Alive && persistence.Flush(), "Dead save fixture failed.");
                        UnityEngine.Object.DestroyImmediate(persistence);
                    }
                    Debug.Log(legacyReload ? "SAVE_LEGACY_PROCESS_OK" : deadReload ? "SAVE_DEAD_RELOAD_OK" : "SAVE_SECOND_PROCESS_OK");
                }
                SessionState.SetBool(Running, false); EditorApplication.update -= Tick; EditorApplication.Exit(0);
            }
            catch (Exception error)
            { Debug.LogException(error); SessionState.SetBool(Running, false); EditorApplication.update -= Tick; EditorApplication.Exit(1); }
        }
    }
}
