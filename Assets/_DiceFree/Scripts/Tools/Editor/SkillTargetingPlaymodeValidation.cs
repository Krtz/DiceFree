using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.Input;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
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
        private static readonly MethodInfo TargetingUpdateMethod=typeof(SkillTargetingController)
            .GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic);
        private static IEnumerator routine;
        private static float deadline;
        private static bool ticking;
        private static InputSettings.BackgroundBehavior priorBackgroundBehavior;
        private static InputSettings.EditorInputBehaviorInPlayMode priorEditorInputBehavior;
        private static QuickCastMode priorQuickCastMode;

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
            priorQuickCastMode=GameplayPreferences.Current.QuickCastMode;
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
            var explorationCamera = traversal.WorldCamera
                .GetComponent<DiceFree.Core.ExplorationCamera>();
            MagicalTouchedValidation.Require(explorationCamera != null,
                "Exploration camera is required for screen-space targeting validation.");
            explorationCamera.ConfigureZoom(14f, 14f, 0.6f);
            yield return null;

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
            TargetingUpdateMethod.Invoke(targeting,null);
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
            MagicalTouchedValidation.Require(
                enemy.Motor.Teleport(player.transform.position + player.transform.right * 6f),
                "Could not move the validation enemy clear of the ground-target test lane.");
            Physics.SyncTransforms();
            yield return null;

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

            Vector2 groundScreen = default;
            bool foundGroundTarget = false;
            var ice = skills.Definitions.First(value =>
                value.stableId == MagicalTouchedAuthoring.IceId);
            float maxGroundRange = ice.range + player.Radius;
            float nearestCenterDistance = float.MaxValue;
            int coveredSamples = 0, terrainMisses = 0, outOfRangeSamples = 0, offNavMeshSamples = 0;
            var nearGround3 = traversal.WorldCamera.WorldToScreenPoint(
                player.transform.position + player.transform.forward * 3f);
            var nearGroundScreen = new Vector2(nearGround3.x, nearGround3.y);
            if (nearGround3.z > 0f && !HudPointerBlocker.Covers(nearGroundScreen) &&
                traversal.TryWorldPoint(nearGroundScreen, out var nearGroundHit) &&
                Vector3.Distance(player.transform.position, nearGroundHit) <= maxGroundRange &&
                NavMesh.SamplePosition(nearGroundHit, out _, 1.5f, NavMesh.AllAreas))
            {
                groundScreen = nearGroundScreen;
                foundGroundTarget = true;
            }
            for (int column = 1; column <= 9; column++)
            for (int row = 1; row <= 7; row++)
            {
                var screen = new Vector2(Screen.width * column / 10f,
                    Screen.height * row / 8f);
                if (HudPointerBlocker.Covers(screen)) { coveredSamples++; continue; }
                if (!traversal.TryWorldPoint(screen, out var hit)) { terrainMisses++; continue; }
                if (Vector3.Distance(player.transform.position, hit) > maxGroundRange)
                { outOfRangeSamples++; continue; }
                if (!NavMesh.SamplePosition(hit, out _, 1.5f, NavMesh.AllAreas))
                { offNavMeshSamples++; continue; }
                float centerDistance = Vector2.Distance(screen,
                    new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
                if (centerDistance >= nearestCenterDistance) continue;
                nearestCenterDistance = centerDistance;
                groundScreen = screen;
                foundGroundTarget = true;
            }
            MagicalTouchedValidation.Require(foundGroundTarget,
                "Could not resolve a valid visible ground target from the player's clear surroundings. " +
                "screen=" + Screen.width + "x" + Screen.height + "; covered=" + coveredSamples +
                "; noTerrain=" + terrainMisses + "; outOfRange=" + outOfRangeSamples +
                "; offNavMesh=" + offNavMeshSamples + "; player=" + player.transform.position.ToString("F2") +
                "; camera=" + traversal.WorldCamera.name + ".");

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
            MagicalTouchedValidation.Require(
                enemy.Motor.Teleport(player.transform.position + player.transform.forward * 2.5f),
                "Could not restore the visible hostile target for quickcast checks.");
            Physics.SyncTransforms();
            yield return null;

            var preferences=GameplayPreferences.Current;
            var generic=player.GetComponent<GenericActionBar>();
            MagicalTouchedValidation.Require(generic!=null,
                "Generic action bar is required to validate quickcast input.");
            targeting.Configure(traversal.WorldCamera);
            Vector3 hostileScreen3=traversal.WorldCamera.WorldToScreenPoint(
                enemy.transform.position+Vector3.up*.6f);
            var hostileScreen=new Vector2(hostileScreen3.x,hostileScreen3.y);
            MagicalTouchedValidation.Require(!HudPointerBlocker.Covers(hostileScreen),
                "Quickcast target is obstructed by a HUD panel.");

            caster.ResetForManifestationLoad();resources.RefillAll();enemy.Health.Restore();
            QueuePointer(hostileScreen);
            float pressHealth=enemy.Health.Current;
            preferences.SetQuickCastMode(QuickCastMode.OnPress);
            var abilitySource=player.GetComponent<AbilityBarSource>();
            MagicalTouchedValidation.Require(abilitySource!=null,
                "Ability bar input source is required for quickcast validation.");
            bool actionBarEnabled=generic.enabled;
            generic.enabled=false;
            Press(Key.Digit1);
            MagicalTouchedValidation.Require(abilitySource.Activate(0,abilitySource.ActionFor(0)),
                "Magic Sand did not activate through the ability source for quickcast-on-press.");
            TargetingUpdateMethod.Invoke(targeting,null);
            ReleaseKeyboard();
            generic.enabled=actionBarEnabled;
            var quickcastRay = traversal.WorldCamera.ScreenPointToRay(hostileScreen);
            var quickcastHits = Physics.RaycastAll(quickcastRay, 1500f,
                    (1 << 8) | (1 << 9) | (1 << 10), QueryTriggerInteraction.Ignore)
                .OrderBy(hit => hit.distance)
                .Select(hit => hit.collider.name + "@" +
                    LayerMask.LayerToName(hit.collider.gameObject.layer)).Take(5).ToArray();
            MagicalTouchedValidation.Require(!targeting.Active&&enemy.Health.Current<pressHealth,
                "Quickcast-on-press did not cast at the pointer target. active="+targeting.Active+
                "; mode="+targeting.Mode+"; hovered="+(targeting.HoveredActor==null?"null":targeting.HoveredActor.name)+
                "; valid="+targeting.HoverValid+"; feedback="+targeting.Feedback+
                "; pointer="+(Mouse.current==null?"null":Mouse.current.position.ReadValue().ToString())+
                "; targetScreen="+hostileScreen+"; rayHits="+string.Join(",",quickcastHits)+
                "; enemyHp="+enemy.Health.Current+"/"+pressHealth+".");

            caster.ResetForManifestationLoad();resources.RefillAll();enemy.Health.Restore();
            QueuePointer(hostileScreen);
            float releaseHealth=enemy.Health.Current;
            preferences.SetQuickCastMode(QuickCastMode.OnRelease);
            actionBarEnabled=generic.enabled;
            generic.enabled=false;
            Press(Key.Digit1);
            MagicalTouchedValidation.Require(abilitySource.Activate(0,abilitySource.ActionFor(0)),
                "Magic Sand did not activate through the ability source for quickcast-on-release.");
            MagicalTouchedValidation.Require(targeting.Active&&
                Mathf.Approximately(enemy.Health.Current,releaseHealth),
                "Quickcast-on-release cast before the skill key was released.");
            ReleaseKeyboard();
            TargetingUpdateMethod.Invoke(targeting,null);
            generic.enabled=actionBarEnabled;
            MagicalTouchedValidation.Require(!targeting.Active&&enemy.Health.Current<releaseHealth,
                "Quickcast-on-release did not cast at the pointer target on key release.");
            preferences.SetQuickCastMode(priorQuickCastMode);

            Debug.Log(
                "DICEFREE_SKILL_TARGETING_OK: selected target no longer auto-casts; reticle, quickcast-on-press and quickcast-on-release are verified; RMB/Escape cancel without spending resources.");
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

        private static void QueuePointer(Vector2 position)
        {
            InputSystem.QueueStateEvent(mouse,new MouseState{position=position});
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
            GameplayPreferences.Current.SetQuickCastMode(priorQuickCastMode);
        }
    }
}
