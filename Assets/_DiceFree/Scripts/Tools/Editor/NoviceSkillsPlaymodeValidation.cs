using System;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class NoviceSkillsPlaymodeValidation
    {
        private const string Prefix = "DiceFree.NoviceSkillsPlaymode.";
        private const string BatchFlag = Prefix + "batch";
        private const string PriorFastPlayFlag = Prefix + "prior-fast-play";
        private const string PriorPlayOptionsKey = Prefix + "prior-play-options";
        private const string SaveRootVariable = "DICEFREE_EDITOR_SAVE_ROOT";

        private static float deadline;
        private static bool runningTick;

        private sealed class FixedMissRoll : IMissRollSource
        {
            private readonly float value;
            public FixedMissRoll(float result) => value = result;
            public float NextUnit() => value;
        }

        static NoviceSkillsPlaymodeValidation() => EditorApplication.playModeStateChanged += OnPlayState;

        public static void RunLegacyBatch()
        {
            SessionState.SetBool(BatchFlag, true);
            Start();
        }

        [CliCommand("dicefree.novice-skills.playmode-test", "Run real Novice skill casts plus two-phase skill save/reload; poll dicefree.novice-skills.playmode-status.", Tags = new[] { "tests", "skills" })]
        private static object Start()
        {
            NoviceSkillsValidation.Require(!EditorApplication.isPlayingOrWillChangePlaymode,
                "Novice skill Play Mode validation requires idle Edit Mode.");

            SessionState.SetBool(PriorFastPlayFlag, EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetInt(PriorPlayOptionsKey, (int)EditorSettings.enterPlayModeOptions);
            EditorSettings.enterPlayModeOptionsEnabled = false;

            var args = Environment.GetCommandLineArgs();
            int saveRootIndex = Array.IndexOf(args, "-diceFreeSaveRoot");
            string root = saveRootIndex >= 0 && saveRootIndex + 1 < args.Length && Path.IsPathRooted(args[saveRootIndex + 1])
                ? args[saveRootIndex + 1]
                : Path.Combine(Path.GetTempPath(), "DiceFree-NoviceSkills-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            Environment.SetEnvironmentVariable(SaveRootVariable, root);
            SessionState.SetString(Prefix + "root", root);
            SessionState.SetString(Prefix + "status", "running");
            SessionState.SetString(Prefix + "error", "");
            SessionState.SetInt(Prefix + "phase", 1);
            EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.novice-skills.playmode-status", "Read the current or most recent Novice skill Play Mode result.", Tags = new[] { "tests", "skills" })]
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
                finalMarker = status == "passed" ? "DICEFREE_NOVICE_SKILLS_PLAYMODE_OK" : "",
                error = SessionState.GetString(Prefix + "error", ""),
                temporarySaveRoot = SessionState.GetString(Prefix + "root", ""),
                phase = SessionState.GetInt(Prefix + "phase", 0)
            };
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running")
            {
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    CleanupRoot();
                    ExitBatchIfRequested();
                }
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.isPaused = false;
                Time.timeScale = 1f;
                deadline = Time.realtimeSinceStartup + 20f;
                if (!runningTick)
                {
                    runningTick = true;
                    EditorApplication.update += Tick;
                    Application.logMessageReceived += OnLog;
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                StopTick();
                int phase = SessionState.GetInt(Prefix + "phase", 0);
                if (phase == 2)
                {
                    // PersistenceTestGuard also clears editor-only overrides on EnteredEditMode.
                    // Reapply this harness root on the next editor tick so callback ordering cannot erase it.
                    EditorApplication.delayCall += BeginPhaseTwo;
                }
                else
                {
                    CleanupRoot();
                }
            }
        }

        private static void BeginPhaseTwo()
        {
            if (SessionState.GetString(Prefix + "status", "") != "running" ||
                SessionState.GetInt(Prefix + "phase", 0) != 2 ||
                EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(SessionState.GetString(Prefix + "root", ""));
            EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            EditorApplication.EnterPlaymode();
        }

        private static void Tick()
        {
            try
            {
                NoviceSkillsValidation.Require(Time.realtimeSinceStartup < deadline,
                    "Novice skill Play Mode validation timed out. " + DescribeWaitState());
                if (!EditorApplication.isPlaying) return;

                var skills = UnityEngine.Object.FindAnyObjectByType<NoviceSkillProgression>();
                if (skills == null) return;
                var player = skills.GetComponent<CombatActor>();
                var xp = skills.GetComponent<ExperienceProgression>();
                var caster = skills.GetComponent<NoviceSkillCaster>();
                var selection = skills.GetComponent<TargetSelection>();
                var persistence = skills.GetComponent<ManifestationPersistence>();
                if (player == null || xp == null || caster == null || selection == null ||
                    persistence == null || !persistence.Ready)
                    return;

                if (SessionState.GetInt(Prefix + "phase", 0) == 1)
                    RunPhaseOne(skills, player, xp, caster, selection, persistence);
                else
                    RunPhaseTwo(skills, xp, persistence);
            }
            catch (Exception error)
            {
                Fail(error);
            }
        }

        private static void RunPhaseOne(
            NoviceSkillProgression skills,
            CombatActor player,
            ExperienceProgression xp,
            NoviceSkillCaster caster,
            TargetSelection selection,
            ManifestationPersistence persistence)
        {
            StopTick();

            xp.RestoreState(5, 0);
            var beforePassive = player.Stats.Attributes;
            skills.RestoreState(new[]
            {
                new SkillRankState(NoviceSkillsAuthoring.StrengthId, 1),
                new SkillRankState(NoviceSkillsAuthoring.MagicSandId, 1),
                new SkillRankState(NoviceSkillsAuthoring.AgilityId, 1),
                new SkillRankState(NoviceSkillsAuthoring.SpiritId, 1),
                new SkillRankState(NoviceSkillProgression.PassiveStableId, 1)
            });
            NoviceSkillsValidation.Require(skills.SpentPoints == 5 && skills.UnspentPoints == 0,
                "Level-5 Novice did not resolve five spent skill points.");
            var afterPassive = player.Stats.Attributes;
            Approximately(afterPassive.vitality, beforePassive.vitality + 1, "passive VIT");
            Approximately(afterPassive.strength, beforePassive.strength + 1, "passive STR");
            Approximately(afterPassive.agility, beforePassive.agility + 1, "passive AGI");
            Approximately(afterPassive.intelligence, beforePassive.intelligence + 1, "passive INT");
            Approximately(afterPassive.spirit, beforePassive.spirit + 1, "passive SPI");

            bool pointNotified = false;
            skills.SkillPointAvailable += OnPoint;
            void OnPoint() => pointNotified = true;
            int required = xp.RequiredXp;
            xp.Grant(required);
            skills.SkillPointAvailable -= OnPoint;
            NoviceSkillsValidation.Require(xp.Level == 6 && pointNotified && skills.UnspentPoints == 1,
                "Level-up did not grant/notify one additional Novice skill point.");

            var enemy = CombatActor.All
                .Where(actor => player.IsHostileTo(actor) && actor.Effects != null && actor.GetComponent<BasicAttack>() != null)
                .OrderByDescending(actor => actor.Health.Maximum)
                .FirstOrDefault();
            NoviceSkillsValidation.Require(enemy != null, "No live Cornberg hostile with status/attack support found.");
            int originalFaction = enemy.Faction;
            float originalRadius = enemy.Radius;
            string originalFamily = enemy.FamilyId;
            var ai = enemy.GetComponent<AggroBehaviour>();
            if (ai != null) ai.enabled = false;
            enemy.GetComponent<BasicAttack>()?.Cancel();
            enemy.Health.Restore(1);

            Vector3 nearby = player.transform.position + player.transform.forward * 1.1f;
            NoviceSkillsValidation.Require(enemy.Motor.Teleport(nearby), "Could not place validation enemy in Novice skill range.");

            var strength = skills.Definition(NoviceSkillsAuthoring.StrengthId);
            enemy.Effects.ConfigureControl(0.95f, false);
            float before = enemy.Health.Current;
            NoviceSkillsValidation.Require(caster.Cast(strength, enemy), "STR melee stun failed to cast.");
            NoviceSkillsValidation.Require(enemy.Health.Current < before, "STR melee stun dealt no damage.");
            NoviceSkillsValidation.Require(enemy.Effects.Stunned, "STR melee stun did not apply stun.");
            Approximately(enemy.Effects.ResolveStunDuration(strength.StunDuration(1), true), 0.2f,
                "ordinary-resistance bypass");
            enemy.Effects.ConfigureControl(0.95f, true);
            NoviceSkillsValidation.Require(!enemy.Effects.TryApplyStun(1f, true),
                "Hard Stun Immunity did not block the Novice stun.");
            enemy.Effects.ConfigureControl(0, false);
            enemy.Effects.ClearTransient();
            caster.ResetCooldowns();

            enemy.Health.Restore(1);
            var sand = skills.Definition(NoviceSkillsAuthoring.MagicSandId);
            before = enemy.Health.Current;
            NoviceSkillsValidation.Require(caster.Cast(sand, enemy), "Magic Sand failed to cast.");
            NoviceSkillsValidation.Require(enemy.Health.Current < before, "Magic Sand dealt no damage.");
            Approximately(enemy.Effects.AccuracyMissChance, 0.075f, "Magic Sand rank-1 penalty");
            var enemyAttack = enemy.GetComponent<BasicAttack>().Definition;
            NoviceSkillsValidation.Require(enemyAttack.requiresAccuracy, "Enemy basic attack is not accuracy-tagged.");
            NoviceSkillsValidation.Require(AccuracyResolver.Resolve(enemy, enemyAttack, new FixedMissRoll(0.01f)).Hit.missed,
                "Magic Sand did not miss with a roll below 7.5%.");
            NoviceSkillsValidation.Require(!AccuracyResolver.Resolve(enemy, enemyAttack, new FixedMissRoll(0.50f)).Hit.missed,
                "Magic Sand incorrectly missed with a roll above 7.5%.");
            enemy.Effects.ApplyAccuracyPenalty(sand.stableId, 0.075f, sand.durationSeconds);
            Approximately(enemy.Effects.AccuracyMissChance, 0.075f, "Magic Sand refresh-not-stack semantics");
            enemy.Effects.ClearTransient();
            caster.ResetCooldowns();

            enemy.Health.Restore(1);
            enemy.Configure(player.Faction, originalRadius, originalFamily);
            selection.Select(enemy);
            NoviceSkillsValidation.Require(selection.Selected == enemy && selection.ValidFriendly(enemy),
                "Normal target selection did not retain a friendly support target.");

            var agility = skills.Definition(NoviceSkillsAuthoring.AgilityId);
            float baseAttackSpeed = enemy.Stats.AttackSpeed;
            float bonus = agility.AttackSpeedBonusPercent(player.Stats.Attributes, 1);
            NoviceSkillsValidation.Require(caster.Cast(agility, enemy), "AGI support buff failed on explicit ally target.");
            Approximately(enemy.Stats.AttackSpeed, baseAttackSpeed * (1 + bonus / 100f), "AGI ally attack-speed buff");
            enemy.Effects.ApplyAttackSpeedBuff(agility.stableId, bonus, agility.durationSeconds);
            Approximately(enemy.Stats.AttackSpeed, baseAttackSpeed * (1 + bonus / 100f), "AGI refresh-not-stack semantics");
            enemy.Effects.ClearTransient();
            caster.ResetCooldowns();

            enemy.Health.Restore(1);
            float damage = Mathf.Min(5f, enemy.Health.Maximum * 0.25f);
            enemy.Health.ApplyDamage(player, new DamageResult { mitigated = damage, channel = DamageChannel.Physical });
            NoviceSkillsValidation.Require(enemy.Alive && enemy.Health.Current < enemy.Health.Maximum,
                "Could not create a living wounded ally for SPI heal validation.");
            float wounded = enemy.Health.Current;
            var spirit = skills.Definition(NoviceSkillsAuthoring.SpiritId);
            float expectedHeal = spirit.HealAmount(player.Stats.Attributes, 1) *
                                 player.Stats.HealingDone * enemy.Stats.HealingReceived;
            NoviceSkillsValidation.Require(caster.Cast(spirit, enemy), "SPI heal failed on explicit ally target.");
            Approximately(enemy.Health.Current,
                Mathf.Min(enemy.Health.Maximum, wounded + expectedHeal), "SPI ally heal");

            enemy.Configure(originalFaction, originalRadius, originalFamily);
            selection.Select(null);
            if (ai != null) ai.enabled = true;

            NoviceSkillsValidation.Require(caster.CooldownRemaining(spirit) > 0,
                "Skill cooldown did not begin after a successful cast.");
            NoviceSkillsValidation.Require(persistence.Flush(), "Could not flush Novice skill allocation.");
            var profile = persistence.CaptureProfile();
            NoviceSkillsValidation.Require(profile != null && profile.schemaVersion == 5,
                "Saved Echo did not use schema v5.");
            var savedSection = profile.sections.Single(value => value.id == "manifestation:" + player.Stats.Definition.stableId);
            NoviceSkillsValidation.Require(savedSection.version == 5, "Saved manifestation did not use v5.");
            var saved = JsonUtility.FromJson<ManifestationSave>(savedSection.json);
            RequireRanks(saved.classSkills);

            Debug.Log("DICEFREE_NOVICE_SKILLS_PHASE1_OK: passive/point notification, four real casts, friendly targeting, status rules and v5 save passed.");
            SessionState.SetInt(Prefix + "phase", 2);
            EditorApplication.ExitPlaymode();
        }

        private static void RunPhaseTwo(
            NoviceSkillProgression skills,
            ExperienceProgression xp,
            ManifestationPersistence persistence)
        {
            StopTick();

            string expectedRoot = SessionState.GetString(Prefix + "root", "");
            string actualRoot = string.IsNullOrEmpty(persistence.SavePath) ? "" : Path.GetDirectoryName(persistence.SavePath);
            NoviceSkillsValidation.Require(!string.IsNullOrEmpty(expectedRoot) && !string.IsNullOrEmpty(actualRoot) &&
                                            string.Equals(Path.GetFullPath(actualRoot), Path.GetFullPath(expectedRoot), StringComparison.OrdinalIgnoreCase),
                "Reload used the wrong isolated save root. expected=" + expectedRoot + "; actual=" + actualRoot);
            var diskProfile = new LocalEchoStore(expectedRoot).Load();
            NoviceSkillsValidation.Require(diskProfile != null, "Reload root has no committed Echo profile.");
            var diskSection = diskProfile.sections.Single(value => value.id == "manifestation:" + skills.GetComponent<CombatActor>().Stats.Definition.stableId);
            var diskState = JsonUtility.FromJson<ManifestationSave>(diskSection.json);
            NoviceSkillsValidation.Require(diskState.level == 6,
                "Committed Novice level changed before reload. disk=" + diskState.level + "; runtime=" + xp.Level +
                "; revision=" + diskProfile.revision + "; path=" + persistence.SavePath);
            NoviceSkillsValidation.Require(xp.Level == 6,
                "Reloaded Novice level changed. runtime=" + xp.Level + "; disk=" + diskState.level +
                "; revision=" + persistence.Revision + "; path=" + persistence.SavePath);
            RequireRanks(skills.CaptureState());
            NoviceSkillsValidation.Require(skills.SpentPoints == 5 && skills.UnspentPoints == 1,
                "Reloaded Novice skill point accounting changed.");
            var profile = persistence.CaptureProfile();
            NoviceSkillsValidation.Require(profile != null && profile.schemaVersion == 5,
                "Reloaded Echo is not schema v5.");

            skills.ResetAllocatedRanks();
            NoviceSkillsValidation.Require(skills.SpentPoints == 0 && skills.UnspentPoints == 6,
                "Free Novice respec did not refund all known skill points.");

            Debug.Log("DICEFREE_NOVICE_SKILLS_PLAYMODE_OK: all four skills, status rules, support targeting, passive progression, save/reload and free respec passed.");
            SessionState.SetString(Prefix + "status", "passed");
            SessionState.SetString(Prefix + "error", "");
            SessionState.SetInt(Prefix + "phase", 3);
            EditorApplication.ExitPlaymode();
        }

        private static void RequireRanks(SkillRankState[] state)
        {
            NoviceSkillsValidation.Require(state != null && state.Length == 5, "Expected five saved Novice rank records.");
            foreach (string id in new[]
                     {
                         NoviceSkillsAuthoring.StrengthId,
                         NoviceSkillsAuthoring.MagicSandId,
                         NoviceSkillsAuthoring.AgilityId,
                         NoviceSkillsAuthoring.SpiritId,
                         NoviceSkillProgression.PassiveStableId
                     })
                NoviceSkillsValidation.Require(state.Single(value => value.stableId == id).rank == 1,
                    "Saved/reloaded rank changed for " + id);
        }

        private static void Approximately(float actual, float expected, string label)
        {
            NoviceSkillsValidation.Require(Mathf.Abs(actual - expected) < 0.01f,
                label + " expected " + expected + " but got " + actual);
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
            SessionState.SetInt(Prefix + "phase", 3);
            Debug.LogException(error);
            StopTick();
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
            else
                CleanupRoot();
        }

        private static void StopTick()
        {
            if (!runningTick) return;
            runningTick = false;
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;
        }

        private static string DescribeWaitState()
        {
            var skills = UnityEngine.Object.FindAnyObjectByType<NoviceSkillProgression>();
            if (skills == null) return "phase=" + SessionState.GetInt(Prefix + "phase", 0) + "; skills=missing";
            var persistence = skills.GetComponent<ManifestationPersistence>();
            return "phase=" + SessionState.GetInt(Prefix + "phase", 0) +
                   "; skills=present; persistence=" + (persistence == null ? "missing" : persistence.Status) +
                   "; ready=" + (persistence != null && persistence.Ready);
        }

        private static void ExitBatchIfRequested()
        {
            if (!SessionState.GetBool(BatchFlag, false)) return;
            SessionState.EraseBool(BatchFlag);
            EditorApplication.Exit(SessionState.GetString(Prefix + "status", "failed") == "passed" ? 0 : 1);
        }

        private static void CleanupRoot()
        {
            EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)SessionState.GetInt(PriorPlayOptionsKey, (int)EditorSettings.enterPlayModeOptions);
            EditorSettings.enterPlayModeOptionsEnabled = SessionState.GetBool(PriorFastPlayFlag, EditorSettings.enterPlayModeOptionsEnabled);
            Environment.SetEnvironmentVariable(SaveRootVariable, null);
            string root = SessionState.GetString(Prefix + "root", "");
            try
            {
                if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
                    Directory.Delete(root, true);
            }
            catch
            {
                // Best-effort cleanup after result has already been recorded.
            }
        }
    }
}
