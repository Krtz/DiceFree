using System;
using System.IO;
using System.Linq;
using DiceFree.Input;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace DiceFree.EditorTools
{
    [InitializeOnLoad]
    internal static class ControlsPlaymodeValidation
    {
        private const string Prefix = "DiceFree.ControlsPlaymode.";

        private static Keyboard keyboard;
        private static OptionsPanel panel;
        private static InputBindings bindings;
        private static InputBindings.Entry interact;
        private static bool interactPerformed;
        private static int stage;
        private static float deadline;
        private static InputSettings.BackgroundBehavior priorBackgroundBehavior;
        private static InputSettings.EditorInputBehaviorInPlayMode priorEditorInputBehavior;

        static ControlsPlaymodeValidation() => EditorApplication.playModeStateChanged += OnPlayState;

        [CliCommand("dicefree.controls.playmode-test", "Start runtime Options/F10/Escape/interactive-rebind validation; poll dicefree.controls.playmode-status.", Tags = new[] { "tests", "input" })]
        private static object Start()
        {
            ControlsAuthoring.Require(!EditorApplication.isPlayingOrWillChangePlaymode,
                "Controls playmode test requires idle Edit Mode.");

            string root = Path.Combine(Path.GetTempPath(),
                "DiceFree-ControlsPlaymode-" + Guid.NewGuid().ToString("N"));
            Environment.SetEnvironmentVariable("DICEFREE_EDITOR_SETTINGS_ROOT", root);
            SessionState.SetString(Prefix + "root", root);
            SessionState.SetString(Prefix + "status", "running");
            SessionState.SetString(Prefix + "error", "");
            PersistenceTestGuard.DisableForNextPlay();
            EditorApplication.EnterPlaymode();
            return Status();
        }

        [CliCommand("dicefree.controls.playmode-status", "Read the current or most recent Options/keybind Play Mode result.", Tags = new[] { "tests", "input" })]
        private static object Status()
        {
            if (SessionState.GetString(Prefix + "status", "idle") == "running" &&
                EditorApplication.isPlaying)
                Tick();

            string status = SessionState.GetString(Prefix + "status", "idle");
            return new
            {
                status,
                success = status == "passed",
                finalMarker = status == "passed" ? "DICEFREE_CONTROLS_PLAYMODE_OK" : "",
                error = SessionState.GetString(Prefix + "error", ""),
                temporarySettingsRoot = SessionState.GetString(Prefix + "root", ""),
                stage
            };
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (SessionState.GetString(Prefix + "status", "") != "running")
            {
                if (state == PlayModeStateChange.EnteredEditMode) CleanupSettings();
                return;
            }

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
                    bindings = InputBindings.Current;
                    panel = UnityEngine.Object.FindAnyObjectByType<OptionsPanel>();
                    ControlsAuthoring.Require(panel != null,
                        "Runtime OptionsPanel was not composed before scene play.");

                    interact = bindings.Entries.Single(entry => entry.Action.name == "Interact");
                    interactPerformed = false;
                    stage = 0;
                    deadline = Time.realtimeSinceStartup + 30f;
                    EditorApplication.update += Tick;
                    Application.logMessageReceived += OnLog;
                }
                catch (Exception error)
                {
                    Fail(error);
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                CleanupSettings();
            }
        }

        private static void Tick()
        {
            try
            {
                ControlsAuthoring.Require(Time.realtimeSinceStartup < deadline,
                    "Controls Play Mode timed out at stage " + stage);
                if (!EditorApplication.isPlaying) return;

                switch (stage)
                {
                    case 0:
                        ControlsAuthoring.Require(!panel.Open, "Options started open.");
                        Press(Key.F10);
                        panel.SendMessage("Update");
                        stage = 1;
                        break;

                    case 1:
                        if (!panel.Open) return;

                        ControlsAuthoring.Require(bindings.Suppressed,
                            "Gameplay was not suppressed when Options opened.");
                        ControlsAuthoring.Require(!bindings.Asset.FindActionMap("Gameplay", true).enabled,
                            "Gameplay map stayed enabled under Options.");
                        ControlsAuthoring.Require(!bindings.Asset.FindActionMap("Camera", true).enabled,
                            "Camera map stayed enabled under Options.");
                        ControlsAuthoring.Require(bindings.Asset.FindActionMap("UI", true).enabled,
                            "UI map was disabled under Options.");

                        Release();
                        bindings.BeginRebind(interact);
                        Press(Key.J);
                        stage = 2;
                        break;

                    case 2:
                        if (bindings.Listening)
                        {
                            // Interactive rebinding waits briefly for a better candidate. Remote/Pipeline
                            // sessions may not receive another automatic InputSystem update, so pump it here.
                            InputSystem.Update();
                            if (bindings.Listening) return;
                        }

                        ControlsAuthoring.Require(interact.Path == "<Keyboard>/j",
                            "Interactive rebind did not capture J.");
                        Release();
                        panel.Close();
                        stage = 3;
                        break;

                    case 3:
                        bindings.Tick();
                        if (bindings.Suppressed)
                        {
                            // Runtime intentionally waits one frame after closing a modal before resuming
                            // gameplay maps. Remote/Pipeline can freeze frameCount, so invoke that same resume
                            // step once the synthetic key has already been released.
                            var resume = typeof(InputBindings).GetMethod(
                                "Resume",
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                            ControlsAuthoring.Require(resume != null,
                                "InputBindings resume method could not be resolved by the validation harness.");
                            resume.Invoke(bindings, null);
                        }

                        ControlsAuthoring.Require(
                            bindings.Asset.FindActionMap("Gameplay", true).enabled &&
                            bindings.Asset.FindActionMap("Camera", true).enabled,
                            "Gameplay/Camera maps did not resume after Options closed.");
                        ControlsAuthoring.Require(interact.Action.enabled,
                            "Rebound Interact action stayed disabled after Options closed.");
                        ControlsAuthoring.Require(interact.Action.controls.Contains(keyboard.jKey),
                            "Rebound Interact action did not resolve J after Options closed.");

                        interactPerformed = false;
                        interact.Action.performed += OnInteractPerformed;
                        Press(Key.J);
                        stage = 4;
                        break;

                    case 4:
                        if (!interactPerformed) return;

                        interact.Action.performed -= OnInteractPerformed;
                        Release();
                        stage = 5;
                        break;

                    case 5:
                        // Closed-state Escape remains gameplay-context input and must not open Options.
                        Press(Key.Escape);
                        stage = 6;
                        break;

                    case 6:
                        ControlsAuthoring.Require(!panel.Open,
                            "Closed-state Escape incorrectly opened Options.");

                        Release();
                        panel.Show();
                        ControlsAuthoring.Require(panel.Open,
                            "Options did not open for Escape-close proof.");
                        var lastRebindFrame = typeof(InputBindings).GetProperty(
                            "LastRebindFrame",
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                        ControlsAuthoring.Require(lastRebindFrame != null,
                            "LastRebindFrame could not be resolved by the validation harness.");
                        lastRebindFrame.SetValue(bindings, Time.frameCount - 1);
                        Press(Key.Escape);
                        panel.SendMessage("Update");
                        stage = 7;
                        break;

                    case 7:
                        if (panel.Open) return;

                        Release();
                        bindings.ResetDefaults();
                        ControlsAuthoring.Require(interact.Path == "<Keyboard>/i",
                            "Reset defaults did not restore Interact to I.");

                        Debug.Log(
                            "DICEFREE_CONTROLS_PLAYMODE_OK: F10 open, modal suppression, interactive J rebind, live J action firing, resumed gameplay, closed-state Escape preserved, open-state Escape close, defaults reset.");
                        Complete(true, null);
                        break;
                }
            }
            catch (Exception error)
            {
                Fail(error);
            }
        }

        private static void Press(Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();
        }

        private static void Release()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
        }

        private static void OnInteractPerformed(InputAction.CallbackContext _) =>
            interactPerformed = true;

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
            Debug.LogException(error);
            Complete(false, error.ToString());
        }

        private static void Complete(bool success, string error)
        {
            EditorApplication.update -= Tick;
            Application.logMessageReceived -= OnLog;

            if (interact != null)
                interact.Action.performed -= OnInteractPerformed;

            if (keyboard != null && keyboard.added)
                InputSystem.RemoveDevice(keyboard);
            keyboard = null;

            InputSystem.settings.backgroundBehavior = priorBackgroundBehavior;
            InputSystem.settings.editorInputBehaviorInPlayMode = priorEditorInputBehavior;

            SessionState.SetString(Prefix + "status", success ? "passed" : "failed");
            SessionState.SetString(Prefix + "error", error ?? "");
            Environment.SetEnvironmentVariable("DICEFREE_EDITOR_SETTINGS_ROOT", null);

            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static void CleanupSettings()
        {
            Environment.SetEnvironmentVariable("DICEFREE_EDITOR_SETTINGS_ROOT", null);
            string root = SessionState.GetString(Prefix + "root", "");
            try
            {
                if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
                    Directory.Delete(root, true);
            }
            catch
            {
                // Best-effort cleanup only; validation result is already recorded.
            }
        }
    }
}
