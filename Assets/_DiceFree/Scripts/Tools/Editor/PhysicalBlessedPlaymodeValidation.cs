using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class PhysicalBlessedPlaymodeValidation
    {
        private const string Prefix = "DiceFree.PhysicalPlaymode.";
        private const string SaveRootVariable = "DICEFREE_EDITOR_SAVE_ROOT";
        private const string PriorFastPlayFlag = Prefix + "prior-fast-play";
        private const string PriorPlayOptionsKey = Prefix + "prior-play-options";
        private const double PersistedMana = 23.25;

        private static float deadline;
        private static bool ticking;
        private static IEnumerator routine;

        static PhysicalBlessedPlaymodeValidation() => EditorApplication.playModeStateChanged += OnPlayState;

        [CliCommand("dicefree.physical.playmode-test", "Run real Physically Blessed advancement, five-skill combat, Mana and menu-reload validation; poll dicefree.physical.playmode-status.", Tags = new[] { "tests", "physical" })]
        private static object Start()
        {
            PhysicalBlessedValidation.Require(!EditorApplication.isPlayingOrWillChangePlaymode,
                "Physical Play Mode validation requires idle Edit Mode.");

            SessionState.SetBool(PriorFastPlayFlag, EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetInt(PriorPlayOptionsKey, (int)EditorSettings.enterPlayModeOptions);
            EditorSettings.enterPlayModeOptionsEnabled = false;

            string root = Path.Combine(Path.GetTempPath(), "DiceFree-Physical-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);
            SessionState.SetString(Prefix + "root", root);
            SessionState.SetString(Prefix + "status", "running");
            SessionState.SetString(Prefix + "error", "");
            SessionState.SetInt(Prefix + "phase", 1);

            EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.physical.playmode-status", "Read or continue the current Physically Blessed Play Mode validation.", Tags = new[] { "tests", "physical" })]
        private static object Status()
        {
            string status = SessionState.GetString(Prefix + "status", "idle");
            if (status == "running" && SessionState.GetInt(Prefix + "phase", 0) == 2 &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
                BeginMenuPhase();

            return new
            {
                status,
                success = status == "passed",
                finalMarker = status == "passed" ? "DICEFREE_PHYSICAL_PLAYMODE_OK" : "",
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
                EditorApplication.isPaused = false;
                Time.timeScale = 1f;
                deadline = Time.realtimeSinceStartup + 45f;
                routine = null;
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
                    EditorApplication.delayCall += BeginMenuPhase;
                else
                    Cleanup();
            }
        }

        private static void BeginMenuPhase()
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
                PhysicalBlessedValidation.Require(Time.realtimeSinceStartup < deadline,
                    "Physical Play Mode validation timed out: " + DescribeState());
                if (!EditorApplication.isPlaying) return;

                int phase = SessionState.GetInt(Prefix + "phase", 0);
                if (phase == 2)
                {
                    var menu = UnityEngine.Object.FindAnyObjectByType<StartMenuController>();
                    if (menu == null || !menu.HasLoadedProfile || menu.LoadBlocked) return;
                    RunMenuPhase(menu);
                    return;
                }

                var progression = UnityEngine.Object.FindAnyObjectByType<PhysicalSkillProgression>();
                if (progression == null) return;
                var persistence = progression.GetComponent<ManifestationPersistence>();
                if (persistence == null || !persistence.Ready) return;

                if (phase == 1)
                {
                    routine ??= PhaseOne(progression, persistence);
                    if (!routine.MoveNext()) routine = null;
                }
                else if (phase == 3)
                {
                    StopTick();
                    RunReloadPhase(progression, persistence);
                }
            }
            catch (Exception error)
            {
                Fail(error);
            }
        }

        private static IEnumerator PhaseOne(
            PhysicalSkillProgression physicalSkills,
            ManifestationPersistence persistence)
        {
            var actor = physicalSkills.GetComponent<CombatActor>();
            var xp = physicalSkills.GetComponent<ExperienceProgression>();
            var controller = physicalSkills.GetComponent<AdvancementController>();
            var noviceSkills = physicalSkills.GetComponent<NoviceSkillProgression>();
            var resources = physicalSkills.GetComponent<ActorResourceController>();
            var caster = physicalSkills.GetComponent<PhysicalSkillCaster>();
            var switcher = physicalSkills.GetComponent<ClassPresentationSwitcher>();
            var basic = physicalSkills.GetComponent<BasicAttack>();

            PhysicalBlessedValidation.Require(actor != null && xp != null && controller != null &&
                    noviceSkills != null && resources != null && caster != null && switcher != null && basic != null,
                "Physical gameplay fixture is incomplete.");
            PhysicalBlessedValidation.Require(controller.CurrentClassId == NoviceSkillProgression.ClassStableId,
                "Fresh Physical fixture did not begin as Novice.");

            var edge = controller.Definitions.Single(value =>
                value.targetClass.stableId == PhysicalBlessedAuthoring.ClassId);
            xp.RestoreState(10, 0);
            PhysicalBlessedValidation.Require(controller.CanAdvance(edge) && controller.TryAdvance(edge),
                "Could not advance the level-10 Novice into Physical: " + controller.Feedback);

            yield return null;
            switcher.SendMessage("Update");

            PhysicalBlessedValidation.Require(controller.CurrentClassId == PhysicalBlessedAuthoring.ClassId &&
                    xp.Level == 1 && xp.CurrentXp == 0 && physicalSkills.ActiveForCurrentClass &&
                    !noviceSkills.ActiveForCurrentClass,
                "Physical advancement did not activate the correct level-1 class systems.");
            PhysicalBlessedValidation.Require(resources.Has(PhysicalBlessedAuthoring.ManaId),
                "Physical child did not activate Mana.");
            Near(resources.Maximum(PhysicalBlessedAuthoring.ManaId), 65, "level-1 Mana maximum");
            Near(resources.Current(PhysicalBlessedAuthoring.ManaId), 65, "level-1 initial Mana");
            PhysicalBlessedValidation.Require(switcher.CurrentClassId == PhysicalBlessedAuthoring.ClassId &&
                    switcher.ActiveVisualRoot != null &&
                    switcher.ActiveVisualRoot.name == "PhysicallyBlessedPresentation",
                "Physical advancement did not activate the dedicated presentation.");
            PhysicalBlessedValidation.Require(basic.Definition != null &&
                    basic.Definition.stableId == "attack.physical.basic",
                "Physical adaptive basic attack did not become active.");

            xp.RestoreState(5, 0);
            resources.RefillAll();
            Near(resources.Maximum(PhysicalBlessedAuthoring.ManaId), 67, "level-5 Mana maximum");
            Near(resources.Regeneration(PhysicalBlessedAuthoring.ManaId), 4.7f, "level-5 Mana regeneration");

            var heavy = Definition(physicalSkills, PhysicalBlessedAuthoring.HeavyId);
            var guard = Definition(physicalSkills, PhysicalBlessedAuthoring.GuardId);
            var quick = Definition(physicalSkills, PhysicalBlessedAuthoring.QuickeningId);
            var rain = Definition(physicalSkills, PhysicalBlessedAuthoring.ArrowRainId);
            var martial = Definition(physicalSkills, PhysicalBlessedAuthoring.MartialId);

            physicalSkills.RestoreState(new[]
            {
                new SkillRankState(heavy.stableId, 1),
                new SkillRankState(guard.stableId, 1),
                new SkillRankState(quick.stableId, 1),
                new SkillRankState(rain.stableId, 1)
            });
            PhysicalBlessedValidation.Require(physicalSkills.SpentPoints == 4 && physicalSkills.UnspentPoints == 1,
                "Level-5 Physical did not expose the fifth skill point.");

            float defenseBeforePassive = actor.Stats.Defense(DamageChannel.Physical);
            Near(actor.Stats.BasicAttackDamageMultiplier, 1, "pre-passive basic multiplier");
            PhysicalBlessedValidation.Require(physicalSkills.Spend(martial),
                "Could not spend the fifth point into Martial Aptitude.");
            Near(actor.Stats.BasicAttackDamageMultiplier, 1.05f, "Martial Aptitude R1 basic multiplier");
            Near(actor.Stats.Defense(DamageChannel.Physical), defenseBeforePassive + 1.5f,
                "Martial Aptitude R1 physical defense");
            PhysicalBlessedValidation.Require(physicalSkills.SpentPoints == 5 && physicalSkills.UnspentPoints == 0,
                "Physical five-point allocation accounting changed.");

            var enemies = CombatActor.All
                .Where(value => value != null && actor.IsHostileTo(value) && value.Effects != null &&
                                value.GetComponent<BasicAttack>() != null)
                .OrderByDescending(value => value.Health.Maximum)
                .Take(2)
                .ToArray();
            PhysicalBlessedValidation.Require(enemies.Length == 2,
                "Physical validation needs two real Cornberg hostiles.");
            foreach (var enemy in enemies)
            {
                var ai = enemy.GetComponent<AggroBehaviour>();
                if (ai != null) ai.enabled = false;
                enemy.GetComponent<BasicAttack>()?.Cancel();
                enemy.Health.Restore();
                enemy.Effects.ClearTransient();
            }

            Vector3 heavyPoint = actor.transform.position + actor.transform.forward * 1.05f;
            PhysicalBlessedValidation.Require(enemies[0].Motor.Teleport(heavyPoint),
                "Could not place Heavy Strike validation target.");
            enemies[0].Effects.ConfigureControl(.5f, false);
            float manaBefore = resources.Current(PhysicalBlessedAuthoring.ManaId);
            float healthBefore = enemies[0].Health.Current;
            PhysicalBlessedValidation.Require(caster.Cast(heavy, enemies[0]), "Heavy Strike failed.");
            Near(manaBefore - resources.Current(PhysicalBlessedAuthoring.ManaId), 5, "Heavy Strike Mana cost");
            PhysicalBlessedValidation.Require(enemies[0].Health.Current < healthBefore,
                "Heavy Strike dealt no damage.");
            PhysicalBlessedValidation.Require(enemies[0].Effects.Stunned,
                "Heavy Strike did not apply its ordinary-resistance stun.");
            Near(enemies[0].Effects.ResolveStunDuration(heavy.HeavyStunSeconds(1), false), .2f,
                "Heavy Strike 50% resisted stun");
            PhysicalBlessedValidation.Require(caster.CooldownRemaining(heavy) > 0,
                "Heavy Strike cooldown did not start.");
            enemies[0].Effects.ClearTransient();
            enemies[0].Effects.ConfigureControl(0, false);
            caster.ResetForManifestationLoad();

            resources.RefillAll();
            manaBefore = resources.Current(PhysicalBlessedAuthoring.ManaId);
            PhysicalBlessedValidation.Require(caster.Cast(guard), "Guard failed.");
            Near(manaBefore - resources.Current(PhysicalBlessedAuthoring.ManaId), 8, "Guard Mana cost");
            Near(actor.Stats.IncomingDamageMultiplier(DamageChannel.Physical), .85f, "Guard physical mitigation");
            Near(actor.Stats.IncomingDamageMultiplier(DamageChannel.Magical), .85f, "Guard magical mitigation");
            actor.Effects.ApplyDamageReduction(guard.stableId, guard.GuardReduction(1), guard.durationSeconds);
            Near(actor.Stats.IncomingDamageMultiplier(DamageChannel.Physical), .85f,
                "Guard refresh-not-stack semantics");
            actor.Effects.ClearTransient();
            caster.ResetForManifestationLoad();

            resources.RefillAll();
            float attackSpeed = actor.Stats.AttackSpeed;
            float moveSpeed = actor.Stats.MoveSpeed;
            manaBefore = resources.Current(PhysicalBlessedAuthoring.ManaId);
            PhysicalBlessedValidation.Require(caster.Cast(quick), "Quickening failed.");
            Near(manaBefore - resources.Current(PhysicalBlessedAuthoring.ManaId), 6, "Quickening Mana cost");
            Near(actor.Stats.AttackSpeed, attackSpeed * 1.12f, "Quickening R1 attack speed");
            Near(actor.Stats.MoveSpeed, moveSpeed * 1.04f, "Quickening R1 move speed");
            actor.Effects.ClearTransient();
            caster.ResetForManifestationLoad();

            resources.RefillAll();
            Vector3 center = actor.transform.position + actor.transform.forward * 2.5f;
            PhysicalBlessedValidation.Require(enemies[0].Motor.Teleport(center),
                "Could not place first Arrow Rain target.");
            PhysicalBlessedValidation.Require(enemies[1].Motor.Teleport(center + actor.transform.right * 1.2f),
                "Could not place second Arrow Rain target.");
            foreach (var enemy in enemies) enemy.Health.Restore();

            float firstBefore = enemies[0].Health.Current;
            float secondBefore = enemies[1].Health.Current;
            manaBefore = resources.Current(PhysicalBlessedAuthoring.ManaId);
            PhysicalBlessedValidation.Require(caster.CastArrowRain(rain, center), "Arrow Rain failed.");
            Near(manaBefore - resources.Current(PhysicalBlessedAuthoring.ManaId), 10, "Arrow Rain Mana cost");
            PhysicalBlessedValidation.Require(enemies[0].Health.Current < firstBefore &&
                                              enemies[1].Health.Current < secondBefore,
                "Arrow Rain first wave did not hit multiple enemies.");
            float afterFirstWave = enemies[0].Health.Current;
            ResolveRemainingArrowRainWaves(caster, rain, 1, center);
            PhysicalBlessedValidation.Require(enemies[0].Health.Current < afterFirstWave,
                "Arrow Rain did not continue through its short multi-hit burst.");
            PhysicalBlessedValidation.Require(caster.CooldownRemaining(rain) > 0,
                "Arrow Rain cooldown did not start.");

            foreach (var enemy in enemies)
            {
                enemy.Effects.ClearTransient();
                enemy.Health.Restore();
            }
            actor.Effects.ClearTransient();

            resources.SetCurrentForValidation(PhysicalBlessedAuthoring.ManaId, (float)PersistedMana);
            PhysicalBlessedValidation.Require(persistence.Flush(),
                "Could not save the fully configured Physical manifestation.");
            var saved = ReadPhysicalState(persistence.CaptureProfile());
            RequireRanks(saved.classSkills);
            PhysicalBlessedValidation.Require(saved.level == 5,
                "Saved Physical level changed.");
            PhysicalBlessedValidation.Require(saved.resources != null &&
                    saved.resources.Single(value => value.resourceId == PhysicalBlessedAuthoring.ManaId).value == PersistedMana,
                "Saved Physical Mana did not capture the exact persisted value.");

            Debug.Log("DICEFREE_PHYSICAL_PHASE1_OK: advancement, presentation, Mana, Martial passive and four real active casts passed.");
            SessionState.SetInt(Prefix + "phase", 2);
            EditorApplication.ExitPlaymode();
        }

        private static void RunMenuPhase(StartMenuController menu)
        {
            var physical = menu.Entries.SingleOrDefault(value => value.classId == PhysicalBlessedAuthoring.ClassId);
            PhysicalBlessedValidation.Require(physical != null && physical.available && physical.active && physical.level == 5,
                "Start menu did not show the saved level-5 Physical manifestation as active.");

            string root = SessionState.GetString(Prefix + "root", "");
            var menuDisk = new LocalEchoStore(root).Load();
            double menuMana = ReadPhysicalState(menuDisk).resources
                .Single(value => value.resourceId == PhysicalBlessedAuthoring.ManaId).value;
            PhysicalBlessedValidation.Require(menuMana >= PersistedMana && menuMana <= 67.001,
                "Quit-time persisted Mana is outside the valid regenerated range: " + menuMana);
            SessionState.SetFloat(Prefix + "menu-mana", (float)menuMana);

            SessionState.SetInt(Prefix + "phase", 3);
            deadline = Time.realtimeSinceStartup + 30f;
            PhysicalBlessedValidation.Require(menu.TryPlayManifestation(PhysicalBlessedAuthoring.ClassId),
                "Start menu could not load the Physical manifestation: " + menu.Status);
        }

        private static void RunReloadPhase(
            PhysicalSkillProgression skills,
            ManifestationPersistence persistence)
        {
            var actor = skills.GetComponent<CombatActor>();
            var xp = skills.GetComponent<ExperienceProgression>();
            var resources = skills.GetComponent<ActorResourceController>();
            var caster = skills.GetComponent<PhysicalSkillCaster>();
            var switcher = skills.GetComponent<ClassPresentationSwitcher>();

            PhysicalBlessedValidation.Require(persistence.CurrentClassId == PhysicalBlessedAuthoring.ClassId &&
                    xp.Level == 5 && skills.ActiveForCurrentClass,
                "Menu reload did not restore the saved Physical class/level.");
            RequireRanks(skills.CaptureState());
            PhysicalBlessedValidation.Require(skills.SpentPoints == 5 && skills.UnspentPoints == 0,
                "Reloaded Physical skill accounting changed.");

            string root = SessionState.GetString(Prefix + "root", "");
            var disk = new LocalEchoStore(root).Load();
            var saved = ReadPhysicalState(disk);
            double diskMana = saved.resources.Single(value => value.resourceId == PhysicalBlessedAuthoring.ManaId).value;
            float menuMana = SessionState.GetFloat(Prefix + "menu-mana", -1);
            PhysicalBlessedValidation.Require(Math.Abs(diskMana - menuMana) < .001,
                "StartMenu selection changed the persisted Physical Mana value.");
            PhysicalBlessedValidation.Require(resources.Has(PhysicalBlessedAuthoring.ManaId) &&
                    resources.Current(PhysicalBlessedAuthoring.ManaId) >= menuMana &&
                    resources.Current(PhysicalBlessedAuthoring.ManaId) <= resources.Maximum(PhysicalBlessedAuthoring.ManaId),
                "Runtime Mana did not restore from the persisted Physical value.");

            switcher.SendMessage("Update");
            PhysicalBlessedValidation.Require(switcher.CurrentClassId == PhysicalBlessedAuthoring.ClassId &&
                    switcher.ActiveVisualRoot != null &&
                    switcher.ActiveVisualRoot.name == "PhysicallyBlessedPresentation",
                "Physical presentation did not survive menu reload.");
            foreach (var definition in skills.Definitions.Where(value => value.Active))
                Near(caster.CooldownRemaining(definition), 0, "transient cooldown reset");

            Near(actor.Stats.BasicAttackDamageMultiplier, 1.05f,
                "reloaded Martial Aptitude basic multiplier");

            Debug.Log("DICEFREE_PHYSICAL_PLAYMODE_OK: full Tier-1 kit, Mana, dedicated presentation and StartMenu save/reload passed.");
            SessionState.SetString(Prefix + "status", "passed");
            SessionState.SetString(Prefix + "error", "");
            SessionState.SetInt(Prefix + "phase", 4);
            EditorApplication.ExitPlaymode();
        }

        private static void ResolveRemainingArrowRainWaves(
            PhysicalSkillCaster caster,
            PhysicalSkillDefinition definition,
            int rank,
            Vector3 point)
        {
            caster.StopAllCoroutines();
            var resolver = typeof(PhysicalSkillCaster).GetMethod(
                "ResolveArrowWave",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            PhysicalBlessedValidation.Require(resolver != null,
                "PhysicalSkillCaster Arrow Rain resolver could not be resolved by the validation harness.");
            int remaining = Mathf.Max(0, definition.arrowHitCount - 1);
            for (int wave = 0; wave < remaining; wave++)
                resolver.Invoke(caster, new object[] { definition, rank, point });
        }

        private static PhysicalSkillDefinition Definition(PhysicalSkillProgression skills, string id) =>
            skills.Definitions.Single(value => value.stableId == id);

        private static ManifestationSave ReadPhysicalState(EchoSave profile) =>
            JsonUtility.FromJson<ManifestationSave>(
                profile.sections.Single(value =>
                    value.id == ManifestationRoster.SectionIdFor(PhysicalBlessedAuthoring.ClassId)).json);

        private static void RequireRanks(SkillRankState[] state)
        {
            PhysicalBlessedValidation.Require(state != null && state.Length == 5,
                "Expected five saved Physical skill ranks.");
            foreach (string id in new[]
                     {
                         PhysicalBlessedAuthoring.HeavyId,
                         PhysicalBlessedAuthoring.GuardId,
                         PhysicalBlessedAuthoring.QuickeningId,
                         PhysicalBlessedAuthoring.ArrowRainId,
                         PhysicalBlessedAuthoring.MartialId
                     })
                PhysicalBlessedValidation.Require(state.Single(value => value.stableId == id).rank == 1,
                    "Saved/reloaded Physical rank changed for " + id);
        }

        private static void Near(float actual, float expected, string label)
        {
            PhysicalBlessedValidation.Require(Mathf.Abs(actual - expected) <= .02f,
                label + ": expected " + expected + ", got " + actual);
        }

        private static void OnLog(string message, string stack, LogType type)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running") return;
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            if (stack.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup") &&
                stack.Contains("UnityEditor.Search.SearchDatabase"))
                return;
            Fail(new InvalidOperationException(message + "\n" + stack));
        }

        private static void Fail(Exception error)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running") return;
            SessionState.SetString(Prefix + "status", "failed");
            SessionState.SetString(Prefix + "error", error.ToString());
            SessionState.SetInt(Prefix + "phase", 4);
            Debug.LogException(error);
            StopTick();
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
            else Cleanup();
        }

        private static void StopTick()
        {
            if (!ticking) return;
            ticking = false;
            routine = null;
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
        }

        private static string DescribeState()
        {
            int phase = SessionState.GetInt(Prefix + "phase", 0);
            var skills = UnityEngine.Object.FindAnyObjectByType<PhysicalSkillProgression>();
            if (skills == null) return "phase=" + phase + "; physical-skills=missing";
            var persistence = skills.GetComponent<ManifestationPersistence>();
            return "phase=" + phase + "; class=" + persistence?.CurrentClassId +
                   "; persistence=" + (persistence == null ? "missing" : persistence.Status) +
                   "; ready=" + (persistence != null && persistence.Ready);
        }

        private static void Cleanup()
        {
            EditorSettings.enterPlayModeOptions =
                (EnterPlayModeOptions)SessionState.GetInt(PriorPlayOptionsKey, (int)EditorSettings.enterPlayModeOptions);
            EditorSettings.enterPlayModeOptionsEnabled =
                SessionState.GetBool(PriorFastPlayFlag, EditorSettings.enterPlayModeOptionsEnabled);
            Environment.SetEnvironmentVariable(SaveRootVariable, null);
            string root = SessionState.GetString(Prefix + "root", "");
            try
            {
                if (!string.IsNullOrEmpty(root) && Directory.Exists(root)) Directory.Delete(root, true);
            }
            catch
            {
                // Best-effort cleanup after the result is already recorded.
            }
        }
    }
}
