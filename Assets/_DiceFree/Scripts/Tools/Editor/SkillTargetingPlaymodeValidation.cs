using System;
using System.Collections;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    public static class SkillTargetingPlaymodeValidation
    {
        private const string Prefix = "DiceFree.SkillTargetingPlaymode.";

        private static Keyboard keyboard;
        private static Mouse mouse;
        private static IEnumerator routine;
        private static float deadline;
        private static bool ticking;
        private static InputSettings.BackgroundBehavior priorBackgroundBehavior;
        private static InputSettings.EditorInputBehaviorInPlayMode priorEditorInputBehavior;

        static SkillTargetingPlaymodeValidation() =>
            EditorApplication.playModeStateChanged += OnPlayState;

        [CliCommand("dicefree.targeting.playmode-test",
            "Validate WC3/League-style skill targeting: key enters reticle, explicit click confirms, RMB/Esc cancel and selected targets do not auto-cast.",
            Tags = new[] { "tests", "input", "skills" })]
        private static object Start() => StartCore(false);

        [CliCommand("dicefree.targeting.self-test",
            "Verify friendly Mend cast on player through HUD portrait action.")]
        private static object SelfTest() => StartCore(true);

        private static object StartCore(bool selfOnly)
        {
            SessionState.SetBool(Prefix + "selfOnly", selfOnly);
            MagicalTouchedValidation.Require(
                !EditorApplication.isPlayingOrWillChangePlaymode,
                "Skill targeting validation requires idle Edit Mode.");

            SessionState.SetString(Prefix + "status", "running");
            SessionState.SetString(Prefix + "error", "");
            PersistenceTestGuard.DisableForNextPlay();
            EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.targeting.playmode-status",
            "Read the current skill-targeting Play Mode validation result.",
            Tags = new[] { "tests", "input", "skills" })]
        private static object Status()
        {
            string status = SessionState.GetString(Prefix + "status", "idle");
            return new
            {
                status,
                success = status == "passed",
                finalMarker = status == "passed" ? "DICEFREE_SKILL_TARGETING_OK" : "",
                error = SessionState.GetString(Prefix + "error", "")
            };
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running") return;

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                try
                {
                    EditorApplication.isPaused = false;
                    Time.timeScale = 1f;

                    priorBackgroundBehavior = InputSystem.settings.backgroundBehavior;
                    priorEditorInputBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode =
                        InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;

                    keyboard = InputSystem.AddDevice<Keyboard>();
                    mouse = InputSystem.AddDevice<Mouse>();
                    deadline = Time.realtimeSinceStartup + 30f;
                    routine = Run();

                    ticking = true;
                    EditorApplication.update += Tick;
                    Application.logMessageReceived += OnLog;
                }
                catch (Exception error)
                {
                    Fail(error);
                }
            }
        }

        private static void Tick()
        {
            try
            {
                MagicalTouchedValidation.Require(
                    Time.realtimeSinceStartup < deadline,
                    "Skill targeting validation timed out.");
                if (!EditorApplication.isPlaying || routine == null) return;
                if (!routine.MoveNext()) Complete();
            }
            catch (Exception error)
            {
                Fail(error);
            }
        }

        private static IEnumerator Run()
        {
            MagicalSkillProgression skills = null;
            SkillTargetingController targeting = null;
            while (skills == null || targeting == null)
            {
                skills = UnityEngine.Object.FindAnyObjectByType<MagicalSkillProgression>();
                targeting = UnityEngine.Object.FindAnyObjectByType<SkillTargetingController>();
                yield return null;
            }

            var player = skills.GetComponent<CombatActor>();
            var stats = skills.GetComponent<ActorStats>();
            var xp = skills.GetComponent<ExperienceProgression>();
            var resources = skills.GetComponent<ActorResourceController>();
            var caster = skills.GetComponent<MagicalSkillCaster>();
            var bar = skills.GetComponent<MagicalSkillBar>();
            var selection = skills.GetComponent<TargetSelection>();
            var traversal = skills.GetComponent<DiceFree.Characters.TraversalInput>();
            var novice = skills.GetComponent<NoviceSkillProgression>();
            var physical = skills.GetComponent<PhysicalSkillProgression>();

            MagicalTouchedValidation.Require(
                player != null && stats != null && xp != null && resources != null &&
                caster != null && bar != null && selection != null && traversal != null &&
                novice != null && physical != null,
                "Skill targeting runtime fixture is incomplete.");

            var magicalClass = AssetDatabase.LoadAssetAtPath<ActorDefinition>(
                MagicalTouchedAuthoring.MagicalClassPath);
            MagicalTouchedValidation.Require(magicalClass != null,
                "Magical class asset is missing from targeting validation.");

            novice.SetClassActive(false);
            physical.SetClassActive(false);
            stats.Configure(magicalClass, 5);
            xp.RestoreState(5, 0);
            skills.SetClassActive(true);
            resources.RestoreForClass(MagicalTouchedAuthoring.ClassId, Array.Empty<RuntimeResourceValue>());
            skills.RestoreState(new[]
            {
                new SkillRankState(MagicalTouchedAuthoring.SandId, 1),
                new SkillRankState(MagicalTouchedAuthoring.IceId, 1),
                new SkillRankState(MagicalTouchedAuthoring.MendId, 1)
            });
            resources.RefillAll();
            yield return null;

            var enemy = CombatValidationActors.Find("enemy.crop-slime");
            var ai = enemy.GetComponent<AggroBehaviour>();
            if (ai != null) ai.enabled = false;
            enemy.GetComponent<BasicAttack>()?.Cancel();
            enemy.Health.Restore();
            enemy.Effects?.ClearTransient();

            MagicalTouchedValidation.Require(
                enemy.Motor.Teleport(player.transform.position + player.transform.forward * 2.5f),
                "Could not place targeting validation enemy.");

            Physics.SyncTransforms();
            selection.Select(enemy);
            MagicalTouchedValidation.Require(selection.Selected == enemy,
                "Could not establish the pre-existing selected target.");

            float healthBefore = enemy.Health.Current;
            float manaBefore = resources.Current(ResourceIds.Mana);

            Press(Key.Digit1);
            bar.SendMessage("Update");
            ReleaseKeyboard();

            MagicalTouchedValidation.Require(
                targeting.Active && targeting.Mode == SkillTargetingMode.HostileUnit,
                "Pressing Magic Sand did not enter hostile-unit targeting mode.");
            MagicalTouchedValidation.Require(
                Mathf.Approximately(enemy.Health.Current, healthBefore) &&
                Mathf.Approximately(resources.Current(ResourceIds.Mana), manaBefore),
                "Skill auto-cast on the pre-existing selected target before confirmation.");
            MagicalTouchedValidation.Require(selection.Selected == enemy,
                "Entering skill targeting unexpectedly replaced the ordinary selected target.");

            Vector3 enemyScreen3 = traversal.WorldCamera.WorldToScreenPoint(
                enemy.transform.position + Vector3.up * 0.6f);
            var enemyScreen = new Vector2(enemyScreen3.x, enemyScreen3.y);
            MagicalTouchedValidation.Require(
                !HudPointerBlocker.Covers(enemyScreen),
                "Validation enemy projected under a HUD blocker: " + enemyScreen);
            LeftClick(enemyScreen);
            targeting.SendMessage("Update");
            string clickState = "active=" + targeting.Active +
                                "; hover=" + (targeting.HoveredActor == null ? "null" : targeting.HoveredActor.name) +
                                "; valid=" + targeting.HoverValid +
                                "; feedback=" + targeting.Feedback +
                                "; hp=" + enemy.Health.Current + "/" + healthBefore;
            ReleaseMouse();

            MagicalTouchedValidation.Require(
                !targeting.Active && enemy.Health.Current < healthBefore,
                "Explicit hostile click did not confirm Magic Sand. " + clickState);
            MagicalTouchedValidation.Require(
                resources.Current(ResourceIds.Mana) < manaBefore,
                "Confirmed Magic Sand did not spend Mana.");

            caster.ResetForManifestationLoad();
            enemy.Effects?.ClearTransient();
            enemy.Health.Restore();
            resources.RefillAll();

            float groundMana = resources.Current(ResourceIds.Mana);
            Press(Key.Digit4);
            bar.SendMessage("Update");
            ReleaseKeyboard();

            MagicalTouchedValidation.Require(
                targeting.Active && targeting.Mode == SkillTargetingMode.Ground,
                "Pressing Ice Burst did not enter ground-targeting mode.");
            MagicalTouchedValidation.Require(
                Mathf.Approximately(resources.Current(ResourceIds.Mana), groundMana),
                "Ground-targeted skill spent Mana before confirmation.");

            RightClick(Vector2.zero);
            targeting.SendMessage("Update");
            ReleaseMouse();
            MagicalTouchedValidation.Require(
                !targeting.Active &&
                Mathf.Approximately(resources.Current(ResourceIds.Mana), groundMana),
                "Right-click did not cancel targeting without spending Mana.");

            Press(Key.Digit4);
            bar.SendMessage("Update");
            ReleaseKeyboard();
            MagicalTouchedValidation.Require(targeting.Active,
                "Ice Burst did not re-enter targeting mode after RMB cancellation.");

            Press(Key.Escape);
            targeting.SendMessage("Update");
            ReleaseKeyboard();
            MagicalTouchedValidation.Require(
                !targeting.Active &&
                Mathf.Approximately(resources.Current(ResourceIds.Mana), groundMana),
                "Escape did not cancel targeting without spending Mana.");

            if (SessionState.GetBool(Prefix + "selfOnly", false))
            {
                resources.RefillAll();
                var mend = skills.Definitions.First(value =>
                    value.stableId == MagicalTouchedAuthoring.MendId);
                float selfMana = resources.Current(ResourceIds.Mana);
                targeting.BeginFriendly("Mend", target => caster.Cast(mend, target), mend.range);
                MagicalTouchedValidation.Require(targeting.Mode == SkillTargetingMode.FriendlyUnit,
                    "Mend did not begin friendly-unit targeting.");
                MagicalTouchedValidation.Require(targeting.ConfirmSelfFromPortrait(),
                    "Clicking the 3D portrait did not confirm self-target.");
                MagicalTouchedValidation.Require(!targeting.Active &&
                    resources.Current(ResourceIds.Mana) < selfMana &&
                    caster.CooldownRemaining(mend) > 0,
                    "Portrait self-target cast failed to consume mana or start cooldown.");
                Debug.Log("DICEFREE_PORTRAIT_SELF_CAST_OK");
                yield break;
            }

            Press(Key.Digit4);
            bar.SendMessage("Update");
            ReleaseKeyboard();

            Vector3 desiredGround = player.transform.position + player.transform.forward * 3f;
            Vector3 groundScreen3 = traversal.WorldCamera.WorldToScreenPoint(desiredGround);
            var groundScreen = new Vector2(groundScreen3.x, groundScreen3.y);
            MagicalTouchedValidation.Require(
                traversal.TryWorldPoint(groundScreen, out _),
                "Could not resolve a valid visible ground target.");

            LeftClick(groundScreen);
            targeting.SendMessage("Update");
            ReleaseMouse();

            MagicalTouchedValidation.Require(
                !targeting.Active &&
                resources.Current(ResourceIds.Mana) < groundMana,
                "Ground click did not confirm Ice Burst.");
            MagicalTouchedValidation.Require(
                caster.CooldownRemaining(
                    skills.Definitions.First(value => value.stableId == MagicalTouchedAuthoring.IceId)) > 0,
                "Confirmed Ice Burst did not start its cooldown.");

            Debug.Log(
                "DICEFREE_SKILL_TARGETING_OK: selected target no longer auto-casts; hostile and ground reticles require explicit click; RMB/Escape cancel without spending resources.");
            yield break;
        }

        private static void Press(Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();
        }

        private static void ReleaseKeyboard()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
        }

        private static void LeftClick(Vector2 position)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position, buttons = 1 });
            InputSystem.Update();
        }

        private static void RightClick(Vector2 position)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position, buttons = 2 });
            InputSystem.Update();
        }

        private static void ReleaseMouse()
        {
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
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

        private static void Complete()
        {
            SessionState.SetString(Prefix + "status", "passed");
            SessionState.SetString(Prefix + "error", "");
            Stop();
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        }

        private static void Fail(Exception error)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running") return;
            SessionState.SetString(Prefix + "status", "failed");
            SessionState.SetString(Prefix + "error", error.ToString());
            Debug.LogException(error);
            Stop();
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        }

        private static void Stop()
        {
            if (ticking)
            {
                ticking = false;
                EditorApplication.update -= Tick;
                Application.logMessageReceived -= OnLog;
            }

            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
            keyboard = null;
            mouse = null;
            routine = null;

            InputSystem.settings.backgroundBehavior = priorBackgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode = priorEditorInputBehavior;
        }
    }
}
