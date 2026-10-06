using System;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class AdvancementPlaymodeValidation
    {
        private const string Prefix = "DiceFree.AdvancementPlaymode.";
        private const string SaveRootVariable = "DICEFREE_EDITOR_SAVE_ROOT";
        private const string PriorFastPlayFlag = Prefix + "prior-fast-play";
        private const string PriorPlayOptionsKey = Prefix + "prior-play-options";

        private static float deadline;
        private static bool ticking;

        static AdvancementPlaymodeValidation() => EditorApplication.playModeStateChanged += OnPlayState;

        [CliCommand("dicefree.advancement.playmode-test", "Run the real Novice -> two Tier-1 manifestation fork/switch/reload validation; poll dicefree.advancement.playmode-status.", Tags = new[] { "tests", "progression", "advancement" })]
        private static object Start()
        {
            AdvancementValidation.Require(!EditorApplication.isPlayingOrWillChangePlaymode,
                "Advancement Play Mode validation requires idle Edit Mode.");

            SessionState.SetBool(PriorFastPlayFlag, EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetInt(PriorPlayOptionsKey, (int)EditorSettings.enterPlayModeOptions);
            EditorSettings.enterPlayModeOptionsEnabled = false;

            string root = Path.Combine(Path.GetTempPath(), "DiceFree-Advancement-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            SessionState.SetString(Prefix + "root", root);
            SessionState.SetString(Prefix + "status", "running");
            SessionState.SetString(Prefix + "error", "");
            SessionState.SetInt(Prefix + "phase", 0);

            EditorSceneManager.OpenScene(AdvancementAuthoring.StartMenuScenePath);
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.advancement.playmode-status", "Read or continue the current Advancement Play Mode validation.", Tags = new[] { "tests", "progression", "advancement" })]
        private static object Status()
        {
            string status = SessionState.GetString(Prefix + "status", "idle");
            if (status == "running" && SessionState.GetInt(Prefix + "phase", 0) == 2 &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
                BeginPhaseTwo();

            return new
            {
                status,
                success = status == "passed",
                finalMarker = status == "passed" ? "DICEFREE_ADVANCEMENT_PLAYMODE_OK" : "",
                error = SessionState.GetString(Prefix + "error", ""),
                temporarySaveRoot = SessionState.GetString(Prefix + "root", ""),
                phase = SessionState.GetInt(Prefix + "phase", 0)
            };
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running")
            {
                if (state == PlayModeStateChange.EnteredEditMode) Cleanup();
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                deadline = Time.realtimeSinceStartup + 30f;
                if (!ticking)
                {
                    ticking = true;
                    EditorApplication.update += Tick;
                    Application.logMessageReceived += OnLog;
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                StopTick();
                if (SessionState.GetInt(Prefix + "phase", 0) == 2)
                    EditorApplication.delayCall += BeginPhaseTwo;
                else
                    Cleanup();
            }
        }

        private static void BeginPhaseTwo()
        {
            if (SessionState.GetString(Prefix + "status", "") != "running" ||
                SessionState.GetInt(Prefix + "phase", 0) != 2 ||
                EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(SessionState.GetString(Prefix + "root", ""));
            EditorSceneManager.OpenScene(AdvancementAuthoring.StartMenuScenePath);
            EditorApplication.EnterPlaymode();
        }

        private static void Tick()
        {
            try
            {
                AdvancementValidation.Require(Time.realtimeSinceStartup < deadline,
                    "Advancement Play Mode validation timed out: " + DescribeState());
                if (!EditorApplication.isPlaying) return;

                int phase = SessionState.GetInt(Prefix + "phase", 0);
                if (phase == 0)
                {
                    var freshMenu = UnityEngine.Object.FindAnyObjectByType<StartMenuController>();
                    if (freshMenu == null || !freshMenu.ReadyForNewEcho) return;
                    SessionState.SetInt(Prefix + "phase", 1);
                    deadline = Time.realtimeSinceStartup + 30f;
                    AdvancementValidation.Require(freshMenu.TryStartNewEcho(),
                        "Fresh start menu could not begin as Novice: " + freshMenu.Status);
                    return;
                }
                if (phase == 2)
                {
                    var menu = UnityEngine.Object.FindAnyObjectByType<StartMenuController>();
                    if (menu == null || !menu.HasLoadedProfile || menu.LoadBlocked) return;
                    RunStartMenuPhase(menu);
                    return;
                }

                var controller = UnityEngine.Object.FindAnyObjectByType<AdvancementController>();
                if (controller == null) return;
                var persistence = controller.GetComponent<ManifestationPersistence>();
                if (persistence == null || !persistence.Ready) return;

                StopTick();
                if (phase == 1)
                    RunPhaseOne(controller, persistence);
                else if (phase == 3)
                    RunPhaseTwo(controller, persistence);
            }
            catch (Exception error)
            {
                Fail(error);
            }
        }

        private static void RunPhaseOne(AdvancementController controller, ManifestationPersistence persistence)
        {
            var actor = controller.GetComponent<CombatActor>();
            var xp = controller.GetComponent<ExperienceProgression>();
            var skills = controller.GetComponent<NoviceSkillProgression>();
            var inventory = controller.GetComponent<CarriedInventory>();
            var equipment = controller.GetComponent<Equipment>();
            var wallet = controller.GetComponent<GoldWallet>();
            var journal = controller.GetComponent<QuestJournal>();

            AdvancementValidation.Require(actor != null && xp != null && skills != null && inventory != null &&
                    equipment != null && wallet != null && journal != null,
                "Advancement fixture is missing player progression components.");
            AdvancementValidation.Require(controller.CurrentClassId == AdvancementAuthoring.NoviceId,
                "Fresh advancement fixture did not begin as Novice.");

            var physicalEdge = controller.Definitions.Single(value => value.targetClass.stableId == AdvancementAuthoring.PhysicalId);
            var magicalEdge = controller.Definitions.Single(value => value.targetClass.stableId == AdvancementAuthoring.MagicalId);

            xp.RestoreState(9, 0);
            AdvancementValidation.Require(!controller.CanAdvance(physicalEdge) && !controller.CanAdvance(magicalEdge),
                "Level-9 Novice was incorrectly eligible for Tier-1 advancement.");

            xp.RestoreState(10, 0);
            AdvancementValidation.Require(controller.CanAdvance(physicalEdge) && controller.CanAdvance(magicalEdge),
                "Level-10 Novice did not expose both first branches.");

            skills.RestoreState(new[]
            {
                new SkillRankState(NoviceSkillsAuthoring.MagicSandId, 1),
                new SkillRankState(NoviceSkillProgression.PassiveStableId, 1)
            });
            AdvancementValidation.Require(skills.SpentPoints == 2 && skills.UnspentPoints == 8,
                "Novice fork fixture skill allocation is invalid.");

            wallet.Restore(41);

            var quest = journal.Definitions.FirstOrDefault(journal.CanAccept);
            if (quest != null)
                AdvancementValidation.Require(journal.Accept(quest), "Could not establish active quest fork fixture.");
            string parentQuests = QuestSnapshot(journal);

            var itemDefinition = inventory.Definitions.FirstOrDefault(value => value != null);
            AdvancementValidation.Require(itemDefinition != null, "Cornberg player has no item definition for fork validation.");
            var parentItem = inventory.Grant(itemDefinition.stableId);
            AdvancementValidation.Require(equipment.Equip(parentItem.instanceId, itemDefinition.slot),
                "Could not equip fork validation item.");
            AdvancementValidation.Require(persistence.Flush(), "Could not save parent Novice before advancement.");

            var parentProfile = persistence.CaptureProfile();
            var parentState = ReadState(parentProfile, AdvancementAuthoring.NoviceId);
            string parentSectionBeforeFork = SectionJson(parentProfile, AdvancementAuthoring.NoviceId);
            AdvancementValidation.Require(parentState.level == 10 && parentState.xp == 0 &&
                    parentState.gold == 41 && parentState.inventory.Any(value => value.instanceId == parentItem.instanceId) &&
                    parentState.equipment.Any(value => value.instanceId == parentItem.instanceId),
                "Saved Novice parent fixture is incomplete.");

            AdvancementValidation.Require(controller.TryAdvance(physicalEdge),
                "Physically Blessed fork failed: " + controller.Feedback);
            AdvancementValidation.Require(controller.CurrentClassId == AdvancementAuthoring.PhysicalId &&
                    xp.Level == 1 && xp.CurrentXp == 0,
                "Physical child did not become the active level-1 manifestation.");
            AdvancementValidation.Require(!skills.ActiveForCurrentClass && skills.SpentPoints == 0,
                "Novice skill system remained active on Physical child.");
            AdvancementValidation.Require(wallet.Gold == 41 && QuestSnapshot(journal) == parentQuests,
                "Physical child did not inherit parent currency/quest snapshot.");

            var physicalItems = inventory.Items;
            AdvancementValidation.Require(physicalItems.Length == parentState.inventory.Length,
                "Physical child inventory count changed during fork.");
            var physicalCopiedItem = physicalItems.Single(value => value.definitionId == itemDefinition.stableId);
            AdvancementValidation.Require(physicalCopiedItem.instanceId != parentItem.instanceId &&
                    equipment.Slots.Any(value => value.instanceId == physicalCopiedItem.instanceId),
                "Physical fork reused parent item identity or failed to remap equipped child item.");

            var profileAfterPhysical = persistence.CaptureProfile();
            AdvancementValidation.Require(SectionJson(profileAfterPhysical, AdvancementAuthoring.NoviceId) == parentSectionBeforeFork,
                "Creating Physical child mutated the preserved Novice section.");
            ValidateRoster(profileAfterPhysical, AdvancementAuthoring.PhysicalId, 1);

            wallet.Grant(9);
            AdvancementValidation.Require(wallet.Gold == 50 && persistence.Flush(),
                "Could not establish divergent Physical state.");
            string physicalItemId = physicalCopiedItem.instanceId;

            AdvancementValidation.Require(controller.TrySwitch(AdvancementAuthoring.NoviceId),
                "Could not return to preserved Novice: " + controller.Feedback);
            AdvancementValidation.Require(controller.CurrentClassId == AdvancementAuthoring.NoviceId &&
                    xp.Level == 10 && wallet.Gold == 41 && skills.ActiveForCurrentClass &&
                    skills.Rank(NoviceSkillsAuthoring.MagicSandId) == 1 &&
                    skills.Rank(NoviceSkillProgression.PassiveStableId) == 1 &&
                    QuestSnapshot(journal) == parentQuests,
                "Returning to parent did not restore the exact pre-blessing Novice state.");
            AdvancementValidation.Require(inventory.Items.Any(value => value.instanceId == parentItem.instanceId) &&
                    equipment.Slots.Any(value => value.instanceId == parentItem.instanceId),
                "Parent Novice lost its original item identity/equipment.");

            string physicalBeforeDuplicate = SectionJson(persistence.CaptureProfile(), AdvancementAuthoring.PhysicalId);
            AdvancementValidation.Require(!controller.CanAdvance(physicalEdge) && !controller.TryAdvance(physicalEdge),
                "Existing Physical manifestation could be created/overwritten a second time.");
            AdvancementValidation.Require(SectionJson(persistence.CaptureProfile(), AdvancementAuthoring.PhysicalId) == physicalBeforeDuplicate,
                "Duplicate Physical advancement attempt changed the existing child.");

            AdvancementValidation.Require(controller.CanAdvance(magicalEdge) && controller.TryAdvance(magicalEdge),
                "Preserved Novice could not create the other first branch.");
            AdvancementValidation.Require(controller.CurrentClassId == AdvancementAuthoring.MagicalId &&
                    xp.Level == 1 && xp.CurrentXp == 0 && !skills.ActiveForCurrentClass &&
                    wallet.Gold == 41 && QuestSnapshot(journal) == parentQuests,
                "Magical child did not inherit/reset the expected fork state.");

            var magicalCopiedItem = inventory.Items.Single(value => value.definitionId == itemDefinition.stableId);
            AdvancementValidation.Require(magicalCopiedItem.instanceId != parentItem.instanceId &&
                    magicalCopiedItem.instanceId != physicalItemId &&
                    equipment.Slots.Any(value => value.instanceId == magicalCopiedItem.instanceId),
                "Magical fork did not receive an independent copied item/equipment identity.");
            ValidateRoster(persistence.CaptureProfile(), AdvancementAuthoring.MagicalId, 2);

            wallet.Grant(20);
            AdvancementValidation.Require(wallet.Gold == 61 && persistence.Flush(),
                "Could not establish divergent Magical state.");

            AdvancementValidation.Require(controller.TrySwitch(AdvancementAuthoring.PhysicalId) && wallet.Gold == 50,
                "Physical manifestation did not preserve its independent currency state.");
            AdvancementValidation.Require(controller.TrySwitch(AdvancementAuthoring.NoviceId) &&
                    wallet.Gold == 41 && xp.Level == 10 &&
                    inventory.Items.Any(value => value.instanceId == parentItem.instanceId),
                "Novice parent diverged after child mutations.");
            AdvancementValidation.Require(controller.TrySwitch(AdvancementAuthoring.MagicalId) && wallet.Gold == 61,
                "Magical manifestation did not preserve its independent currency state.");
            AdvancementValidation.Require(persistence.Flush(), "Could not save final active Magical manifestation.");

            SessionState.SetString(Prefix + "parent-item", parentItem.instanceId);
            SessionState.SetString(Prefix + "physical-item", physicalItemId);
            SessionState.SetString(Prefix + "magical-item", magicalCopiedItem.instanceId);
            SessionState.SetString(Prefix + "quests", parentQuests);

            Debug.Log("DICEFREE_ADVANCEMENT_PHASE1_OK: level gate, Physical fork, parent restore, duplicate rejection, Magical fork and timeline divergence passed.");
            SessionState.SetInt(Prefix + "phase", 2);
            EditorApplication.ExitPlaymode();
        }

        private static void RunStartMenuPhase(StartMenuController menu)
        {
            var entries = menu.Entries;
            AdvancementValidation.Require(entries.Length == 3,
                "Start menu did not expose all three saved manifestations.");
            AdvancementValidation.Require(entries.All(value => value.available),
                "Start menu reported an implemented prototype manifestation as unavailable.");
            AdvancementValidation.Require(entries.Single(value => value.classId == AdvancementAuthoring.NoviceId).level == 10,
                "Start menu displayed the wrong Novice level.");
            AdvancementValidation.Require(entries.Single(value => value.classId == AdvancementAuthoring.PhysicalId).level == 1,
                "Start menu displayed the wrong Physical level.");
            AdvancementValidation.Require(entries.Single(value => value.classId == AdvancementAuthoring.MagicalId).level == 1,
                "Start menu displayed the wrong Magical level.");
            AdvancementValidation.Require(entries.Single(value => value.classId == AdvancementAuthoring.MagicalId).active,
                "Start menu did not mark the last active Magical manifestation.");

            SessionState.SetInt(Prefix + "phase", 3);
            deadline = Time.realtimeSinceStartup + 30f;
            AdvancementValidation.Require(menu.TryPlayManifestation(AdvancementAuthoring.PhysicalId),
                "Start menu could not select the preserved Physical manifestation: " + menu.Status);
        }

        private static void RunPhaseTwo(AdvancementController controller, ManifestationPersistence persistence)
        {
            var xp = controller.GetComponent<ExperienceProgression>();
            var skills = controller.GetComponent<NoviceSkillProgression>();
            var inventory = controller.GetComponent<CarriedInventory>();
            var wallet = controller.GetComponent<GoldWallet>();

            string root = SessionState.GetString(Prefix + "root", "");
            AdvancementValidation.Require(Path.GetDirectoryName(persistence.SavePath) == root,
                "Reload phase did not use the original isolated save root.");
            AdvancementValidation.Require(controller.CurrentClassId == AdvancementAuthoring.PhysicalId &&
                    xp.Level == 1 && wallet.Gold == 50 && !skills.ActiveForCurrentClass &&
                    inventory.Items.Any(value => value.instanceId == SessionState.GetString(Prefix + "physical-item", "")),
                "Start-menu selection did not load the preserved Physical manifestation.");

            var profile = persistence.CaptureProfile();
            ValidateRoster(profile, AdvancementAuthoring.PhysicalId, 2);
            AdvancementValidation.Require(
                    ManifestationRoster.HasManifestation(profile, AdvancementAuthoring.NoviceId) &&
                    ManifestationRoster.HasManifestation(profile, AdvancementAuthoring.PhysicalId) &&
                    ManifestationRoster.HasManifestation(profile, AdvancementAuthoring.MagicalId),
                "Reloaded Echo lost one or more manifestation sections.");

            AdvancementValidation.Require(controller.TrySwitch(AdvancementAuthoring.NoviceId) &&
                    wallet.Gold == 41 && xp.Level == 10 && skills.ActiveForCurrentClass &&
                    skills.Rank(NoviceSkillsAuthoring.MagicSandId) == 1 &&
                    skills.Rank(NoviceSkillProgression.PassiveStableId) == 1 &&
                    inventory.Items.Any(value => value.instanceId == SessionState.GetString(Prefix + "parent-item", "")) &&
                    QuestSnapshot(controller.GetComponent<QuestJournal>()) == SessionState.GetString(Prefix + "quests", ""),
                "Reloaded preserved Novice state is wrong.");

            AdvancementValidation.Require(controller.TrySwitch(AdvancementAuthoring.MagicalId) && wallet.Gold == 61,
                "Could not return to Magical manifestation after reload.");

            var disk = new LocalEchoStore(root).Load();
            ValidateRoster(disk, AdvancementAuthoring.MagicalId, 2);
            AdvancementValidation.Require(disk.schemaVersion == 5,
                "Advancement roster unexpectedly changed the Echo schema.");

            Debug.Log("DICEFREE_ADVANCEMENT_PLAYMODE_OK: two real branches, independent timelines, roster reload and parent return passed.");
            SessionState.SetString(Prefix + "status", "passed");
            SessionState.SetString(Prefix + "error", "");
            SessionState.SetInt(Prefix + "phase", 3);
            EditorApplication.ExitPlaymode();
        }

        private static void ValidateRoster(EchoSave profile, string activeClassId, int branchCount)
        {
            var roster = ManifestationRoster.Read(profile, AdvancementAuthoring.NoviceId);
            AdvancementValidation.Require(roster.activeClassId == activeClassId &&
                    roster.branches.Length == branchCount,
                "Manifestation roster active class/branch count changed.");
            if (branchCount >= 1)
                AdvancementValidation.Require(roster.branches.Any(value =>
                        value.parentClassId == AdvancementAuthoring.NoviceId &&
                        value.targetClassId == AdvancementAuthoring.PhysicalId),
                    "Physical branch history is missing.");
            if (branchCount >= 2)
                AdvancementValidation.Require(roster.branches.Any(value =>
                        value.parentClassId == AdvancementAuthoring.NoviceId &&
                        value.targetClassId == AdvancementAuthoring.MagicalId),
                    "Magical branch history is missing.");
        }

        private static ManifestationSave ReadState(EchoSave profile, string classId) =>
            JsonUtility.FromJson<ManifestationSave>(
                profile.sections.Single(value => value.id == ManifestationRoster.SectionIdFor(classId)).json);

        private static string SectionJson(EchoSave profile, string classId) =>
            profile.sections.Single(value => value.id == ManifestationRoster.SectionIdFor(classId)).json;

        private static string QuestSnapshot(QuestJournal journal) =>
            string.Join("|", journal.CaptureState()
                .OrderBy(value => value.questId, StringComparer.Ordinal)
                .Select(JsonUtility.ToJson));

        private static string DescribeState()
        {
            var controller = UnityEngine.Object.FindAnyObjectByType<AdvancementController>();
            if (controller == null) return "controller=missing";
            var persistence = controller.GetComponent<ManifestationPersistence>();
            return "phase=" + SessionState.GetInt(Prefix + "phase", 0) +
                   "; class=" + controller.CurrentClassId +
                   "; persistence=" + (persistence == null ? "missing" : persistence.Status) +
                   "; ready=" + (persistence != null && persistence.Ready);
        }

        private static void OnLog(string message, string stack, LogType type)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running") return;
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            if (stack.Contains("UnityEditor.Search.SearchDatabase")) return;
            Fail(new InvalidOperationException(message + "\n" + stack));
        }

        private static void Fail(Exception error)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running") return;
            SessionState.SetString(Prefix + "status", "failed");
            SessionState.SetString(Prefix + "error", error.ToString());
            SessionState.SetInt(Prefix + "phase", 3);
            Debug.LogException(error);
            StopTick();
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
            else
                Cleanup();
        }

        private static void StopTick()
        {
            if (!ticking) return;
            ticking = false;
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
        }

        private static void Cleanup()
        {
            EditorSettings.enterPlayModeOptions =
                (EnterPlayModeOptions)SessionState.GetInt(PriorPlayOptionsKey, (int)EditorSettings.enterPlayModeOptions);
            EditorSettings.enterPlayModeOptionsEnabled =
                SessionState.GetBool(PriorFastPlayFlag, EditorSettings.enterPlayModeOptionsEnabled);
            PersistenceTestGuard.ClearValidationOverrides();
            Environment.SetEnvironmentVariable(SaveRootVariable, null);

            string root = SessionState.GetString(Prefix + "root", "");
            try
            {
                if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
                    Directory.Delete(root, true);
            }
            catch
            {
                // Best effort after result is recorded.
            }
        }
    }
}
