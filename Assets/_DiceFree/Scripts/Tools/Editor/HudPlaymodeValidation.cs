using System;
using System.Collections;
using System.IO;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Input;
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
    public static class HudPlaymodeValidation
    {
        private const string Prefix = "DiceFree.HudPlaymode.";
        private const string SettingsRootVariable = "DICEFREE_EDITOR_SETTINGS_ROOT";

        private static IEnumerator routine;
        private static bool ticking;
        private static float deadline;
        private static Keyboard keyboard;
        private static InputSettings.BackgroundBehavior priorBackgroundBehavior;
        private static InputSettings.EditorInputBehaviorInPlayMode priorEditorInputBehavior;
        private static bool inputSettingsCaptured;

        static HudPlaymodeValidation() =>
            EditorApplication.playModeStateChanged += OnPlayState;

        [CliCommand(
            "dicefree.hud.playmode-test",
            "Validate customizable HUD persistence/edit suppression and real locomotion animation while moving.",
            Tags = new[] { "tests", "ui", "hud", "animation" })]
        private static object Start()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,
                "HUD Play Mode validation requires idle Edit Mode.");

            string root = Path.Combine(Path.GetTempPath(), "DiceFree-Hud-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            Environment.SetEnvironmentVariable(SettingsRootVariable, root);
            PersistenceTestGuard.DisableForNextPlay();

            SessionState.SetString(Prefix + "root", root);
            SessionState.SetString(Prefix + "status", "running");
            SessionState.SetString(Prefix + "error", "");
            SessionState.SetInt(Prefix + "phase", 1);

            EditorSceneManager.OpenScene(HudAuthoring.ScenePath);
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand(
            "dicefree.hud.playmode-status",
            "Read/continue the customizable HUD + locomotion Play Mode validation.",
            Tags = new[] { "tests", "ui", "hud", "animation" })]
        private static object Status()
        {
            string status = SessionState.GetString(Prefix + "status", "idle");
            if (status == "running" &&
                SessionState.GetInt(Prefix + "phase", 0) == 2 &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
                BeginReload();

            return new
            {
                status,
                success = status == "passed",
                finalMarker = status == "passed" ? "DICEFREE_HUD_PLAYMODE_OK" : "",
                error = SessionState.GetString(Prefix + "error", ""),
                phase = SessionState.GetInt(Prefix + "phase", 0),
                settingsRoot = SessionState.GetString(Prefix + "root", "")
            };
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running") return;

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.isPaused = false;
                Time.timeScale = 1f;

                if (SessionState.GetInt(Prefix + "phase", 0) == 1)
                    StartSyntheticInput();

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
                StopSyntheticInput();
                if (SessionState.GetInt(Prefix + "phase", 0) == 2)
                    EditorApplication.delayCall += BeginReload;
                else
                    Cleanup();
            }
        }

        private static void BeginReload()
        {
            if (SessionState.GetString(Prefix + "status", "") != "running" ||
                SessionState.GetInt(Prefix + "phase", 0) != 2 ||
                EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            Environment.SetEnvironmentVariable(
                SettingsRootVariable,
                SessionState.GetString(Prefix + "root", ""));
            PersistenceTestGuard.DisableForNextPlay();
            EditorSceneManager.OpenScene(HudAuthoring.ScenePath);
            EditorApplication.EnterPlaymode();
        }

        private static void Tick()
        {
            try
            {
                Require(Time.realtimeSinceStartup < deadline, "HUD Play Mode validation timed out.");
                if (!EditorApplication.isPlaying) return;

                int phase = SessionState.GetInt(Prefix + "phase", 0);
                if (phase == 1)
                {
                    routine ??= PhaseOne();
                    if (!routine.MoveNext()) routine = null;
                }
                else if (phase == 2)
                {
                    var manager = UnityEngine.Object.FindAnyObjectByType<HudLayoutManager>();
                    var character = UnityEngine.Object.FindAnyObjectByType<CharacterHudPanel>();
                    if (manager == null || character == null) return;
                    RunReload(manager, character);
                }
            }
            catch (Exception error)
            {
                Fail(error);
            }
        }

        private static IEnumerator PhaseOne()
        {
            HudLayoutManager manager = null;
            CharacterHudPanel character = null;
            TraversalInput traversal = null;
            TraversalMotor motor = null;
            NovicePresentationDriver driver = null;
            HudPortraitRenderer portrait = null;

            while (manager == null || character == null || traversal == null || motor == null ||
                   driver == null || portrait == null || !motor.Ready || driver.VisualAnimator == null)
            {
                manager = UnityEngine.Object.FindAnyObjectByType<HudLayoutManager>();
                character = UnityEngine.Object.FindAnyObjectByType<CharacterHudPanel>();
                traversal = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
                motor = traversal == null ? null : traversal.GetComponent<TraversalMotor>();
                driver = traversal == null ? null : traversal.GetComponent<NovicePresentationDriver>();
                portrait = traversal == null ? null : traversal.GetComponent<HudPortraitRenderer>();
                yield return null;
            }

            var widgets = UnityEngine.Object.FindObjectsByType<CustomizableHudWidget>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            Require(widgets.Length >= 8, "Runtime HUD did not load all customizable widgets.");
            Require(
                widgets.Select(value => value.LayoutId).Distinct(StringComparer.Ordinal).Count() == widgets.Length,
                "Runtime HUD layout IDs are not unique.");

            var minimap = widgets.OfType<MinimapHud>().Single();
            Require(Mathf.Abs(minimap.Bounds.width - minimap.Bounds.height) <= 1.5f,
                "Runtime minimap is not square.");

            manager.SetEditMode(true);
            Require(manager.EditMode && InputBindings.Current.Suppressed,
                "HUD Edit Mode did not suppress gameplay input.");

            var desired = new Rect(0.12f, 0.11f, 0.26f, 0.30f);
            manager.SetNormalizedBounds(character, desired, true);
            Rect changed = manager.NormalizedBounds(character);
            Near(changed.x, desired.x, "character layout x");
            Near(changed.y, desired.y, "character layout y");
            Near(changed.width, desired.width, "character layout width");
            Near(changed.height, desired.height, "character layout height");

            Require(
                HudThemes.All.Length >= 5 &&
                HudThemes.All.Select(value => value.id).Distinct(StringComparer.Ordinal).Count() ==
                HudThemes.All.Length,
                "HUD theme catalogue is incomplete or has duplicate IDs.");

            string originalTheme = manager.ThemeId;
            manager.CycleTheme();
            Require(manager.ThemeId != originalTheme, "HUD theme did not change.");

            var tint = new Color(0.31f, 0.57f, 0.83f, 1f);
            manager.SetHudTint(tint);
            Require(manager.CustomTintEnabled, "HUD custom tint was not enabled.");
            Near(manager.HudTint.r, tint.r, "HUD tint R");
            Near(manager.HudTint.g, tint.g, "HUD tint G");
            Near(manager.HudTint.b, tint.b, "HUD tint B");

            SessionState.SetString(Prefix + "theme", manager.ThemeId);
            SessionState.SetFloat(Prefix + "tint-r", tint.r);
            SessionState.SetFloat(Prefix + "tint-g", tint.g);
            SessionState.SetFloat(Prefix + "tint-b", tint.b);
            SessionState.SetFloat(Prefix + "x", changed.x);
            SessionState.SetFloat(Prefix + "y", changed.y);
            SessionState.SetFloat(Prefix + "w", changed.width);
            SessionState.SetFloat(Prefix + "h", changed.height);

            manager.SetEditMode(false);
            float inputDeadline = Time.realtimeSinceStartup + 2f;
            while (InputBindings.Current.Suppressed && Time.realtimeSinceStartup < inputDeadline)
                yield return null;
            Require(!InputBindings.Current.Suppressed,
                "Gameplay input did not resume after leaving HUD Edit Mode.");

            Require(portrait.Texture != null && portrait.Texture.IsCreated(),
                "Live 3D HUD portrait RenderTexture was not created.");

            var animator = driver.VisualAnimator;
            Require(animator.runtimeAnimatorController != null && animator.avatar != null &&
                    animator.avatar.isValid && animator.avatar.isHuman,
                "Runtime player Animator/Avatar is invalid.");

            // Exercise the same F6 -> WASD path the player uses, rather than calling the motor directly.
            if (traversal.Mode != TraversalInput.ControlMode.Direct)
            {
                Press(Key.F6);
                traversal.SendMessage("Update");
                ReleaseKeyboard();
                yield return null;
            }
            Require(traversal.Mode == TraversalInput.ControlMode.Direct,
                "Synthetic F6 did not switch the real TraversalInput into Direct/WASD mode.");

            // Hold W beyond twice the old 0.5-second source take. Each validation step invokes the same
            // TraversalInput.Update -> TraversalMotor path used by gameplay, then the presentation LateUpdate.
            Press(Key.W);
            float movingUntil = Time.time + 1.15f;
            float peakSpeed = 0f;
            float highestNormalizedLocomotionTime = 0f;
            while (Time.time < movingUntil)
            {
                traversal.SendMessage("Update");
                driver.SendMessage("LateUpdate");

                peakSpeed = Mathf.Max(peakSpeed, driver.LastDrivenSpeed);
                var state = animator.GetCurrentAnimatorStateInfo(0);
                if (state.IsName("Base Layer.Locomotion"))
                    highestNormalizedLocomotionTime = Mathf.Max(highestNormalizedLocomotionTime, state.normalizedTime);
                yield return null;
            }
            ReleaseKeyboard();
            traversal.SendMessage("Update");
            driver.SendMessage("LateUpdate");

            Require(peakSpeed > 0.2f, "Presentation driver never observed movement through real WASD input.");
            var movingState = animator.GetCurrentAnimatorStateInfo(0);
            Require(
                movingState.IsName("Base Layer.Locomotion") || animator.IsInTransition(0),
                "Animator did not enter/stay in Locomotion while WASD moved the actor.");
            Require(highestNormalizedLocomotionTime >= 1f,
                "Locomotion never survived long enough to pass one complete 0.5-second source cycle.");

            float idleAt = Time.time + 0.35f;
            while (Time.time < idleAt) yield return null;
            Require(driver.LastDrivenSpeed < 0.05f, "Presentation speed did not settle after movement stopped.");
            var idleState = animator.GetCurrentAnimatorStateInfo(0);
            Require(idleState.IsName("Base Layer.Idle") || animator.IsInTransition(0),
                "Animator did not return toward Idle after movement stopped.");

            Debug.Log(
                "DICEFREE_HUD_PHASE1_OK: movable/resizable layout, theme/input suppression, live 3D portrait and >1s looping locomotion passed.");

            SessionState.SetInt(Prefix + "phase", 2);
            EditorApplication.ExitPlaymode();
        }

        private static void RunReload(HudLayoutManager manager, CharacterHudPanel character)
        {
            Rect loaded = manager.NormalizedBounds(character);
            Near(loaded.x, SessionState.GetFloat(Prefix + "x", -1), "reloaded character layout x");
            Near(loaded.y, SessionState.GetFloat(Prefix + "y", -1), "reloaded character layout y");
            Near(loaded.width, SessionState.GetFloat(Prefix + "w", -1), "reloaded character layout width");
            Near(loaded.height, SessionState.GetFloat(Prefix + "h", -1), "reloaded character layout height");
            Require(manager.ThemeId == SessionState.GetString(Prefix + "theme", ""),
                "HUD theme selection did not persist across reload.");
            Require(manager.CustomTintEnabled, "HUD custom tint did not persist across reload.");
            Near(manager.HudTint.r, SessionState.GetFloat(Prefix + "tint-r", -1f), "reloaded HUD tint R");
            Near(manager.HudTint.g, SessionState.GetFloat(Prefix + "tint-g", -1f), "reloaded HUD tint G");
            Near(manager.HudTint.b, SessionState.GetFloat(Prefix + "tint-b", -1f), "reloaded HUD tint B");

            Debug.Log(
                "DICEFREE_HUD_PLAYMODE_OK: customizable HUD layout/theme/tint persisted across reload and locomotion remained animated beyond the old clip length.");
            SessionState.SetString(Prefix + "status", "passed");
            SessionState.SetString(Prefix + "error", "");
            SessionState.SetInt(Prefix + "phase", 3);
            StopTick();
            EditorApplication.ExitPlaymode();
        }

        private static void Near(float actual, float expected, string label)
        {
            Require(Mathf.Abs(actual - expected) <= 0.005f,
                label + ": expected " + expected + ", got " + actual);
        }

        private static void StartSyntheticInput()
        {
            if (!inputSettingsCaptured)
            {
                priorBackgroundBehavior = InputSystem.settings.backgroundBehavior;
                priorEditorInputBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
                inputSettingsCaptured = true;
            }

            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;

            if (keyboard == null || !keyboard.added)
                keyboard = InputSystem.AddDevice<Keyboard>();
        }

        private static void StopSyntheticInput()
        {
            if (keyboard != null && keyboard.added)
                InputSystem.RemoveDevice(keyboard);
            keyboard = null;

            if (!inputSettingsCaptured) return;
            InputSystem.settings.backgroundBehavior = priorBackgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode = priorEditorInputBehavior;
            inputSettingsCaptured = false;
        }

        private static void Press(Key key)
        {
            Require(keyboard != null && keyboard.added,
                "Synthetic keyboard is not available.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();
        }

        private static void ReleaseKeyboard()
        {
            if (keyboard == null || !keyboard.added) return;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
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

        private static void Fail(Exception error)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running") return;
            SessionState.SetString(Prefix + "status", "failed");
            SessionState.SetString(Prefix + "error", error.ToString());
            SessionState.SetInt(Prefix + "phase", 3);
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

        private static void Cleanup()
        {
            Environment.SetEnvironmentVariable(SettingsRootVariable, null);
            string root = SessionState.GetString(Prefix + "root", "");
            try
            {
                if (!string.IsNullOrEmpty(root) && Directory.Exists(root)) Directory.Delete(root, true);
            }
            catch
            {
                // Best-effort cleanup after result is already recorded.
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
