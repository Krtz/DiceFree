using System;
using System.IO;
using System.Linq;
using DiceFree.Input;
using Unity.Pipeline.Commands;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace DiceFree.EditorTools
{
    public static class ControlsValidation
    {
        [CliCommand("dicefree.controls.validate", "Validate defaults, rebinding, composites, conflicts, persistence/reset and modal map suppression.", Tags = new[] { "tests", "input" })]
        public static object Run()
        {
            ControlsAuthoring.Require(!UnityEditor.EditorApplication.isPlaying, "Controls validation requires Edit Mode.");
            var authored = Resources.Load<InputActionAsset>("DiceFreeControls");
            ControlsAuthoring.Require(authored != null, "Missing Resources/DiceFreeControls.");

            string directory = Path.Combine(Path.GetTempPath(), "DiceFree-Controls-" + Guid.NewGuid().ToString("N"));
            var priorMode = InputSystem.settings.updateMode;
            Keyboard keyboard = null;
            Mouse mouse = null;

            try
            {
                InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
                keyboard = InputSystem.AddDevice<Keyboard>();
                mouse = InputSystem.AddDevice<Mouse>();

                using (var bindings = new InputBindings(authored, directory))
                {
                    Default(bindings, "Gameplay/Direct movement", "Up", "<Keyboard>/w");
                    Default(bindings, "Gameplay/Direct movement", "Down", "<Keyboard>/s");
                    Default(bindings, "Gameplay/Direct movement", "Left", "<Keyboard>/a");
                    Default(bindings, "Gameplay/Direct movement", "Right", "<Keyboard>/d");
                    Default(bindings, "Gameplay/Move destination", null, "<Mouse>/rightButton");
                    Default(bindings, "Gameplay/Switch control mode", null, "<Keyboard>/f6");
                    Default(bindings, "Gameplay/Stop", null, "<Keyboard>/space");
                    Default(bindings, "Gameplay/Interact", null, "<Keyboard>/i");
                    Default(bindings, "Gameplay/Close interaction", null, "<Keyboard>/escape");
                    Default(bindings, "Gameplay/Select target", null, "<Mouse>/leftButton");
                    Default(bindings, "Gameplay/Cycle hostile", null, "<Keyboard>/tab");
                    Default(bindings, "Gameplay/Attack selected", null, "<Keyboard>/x");
                    Default(bindings, "Gameplay/Clear target", null, "<Keyboard>/escape");
                    Default(bindings, "Gameplay/Return to anchor", null, "<Keyboard>/r");
                    Default(bindings, "Gameplay/Inventory", null, "<Keyboard>/b");
                    Default(bindings, "Camera/Look toward World 1", null, "<Keyboard>/v");
                    Default(bindings, "Camera/Camera pan", "Up", "<Keyboard>/upArrow");
                    Default(bindings, "Camera/Camera pan", "Down", "<Keyboard>/downArrow");
                    Default(bindings, "Camera/Camera pan", "Left", "<Keyboard>/leftArrow");
                    Default(bindings, "Camera/Camera pan", "Right", "<Keyboard>/rightArrow");
                    Default(bindings, "Camera/Camera drag", null, "<Mouse>/middleButton");
                    Default(bindings, "Camera/Recenter", null, "<Keyboard>/home");
                    Default(bindings, "Camera/Toggle follow", null, "<Keyboard>/f");
                    Default(bindings, "Camera/Rotate camera", "Negative", "<Keyboard>/q");
                    Default(bindings, "Camera/Rotate camera", "Positive", "<Keyboard>/e");
                    Default(bindings, "UI/Options menu toggle", null, "<Keyboard>/f10");
                    Default(bindings, "UI/Close options", null, "<Keyboard>/escape");
                    Debug.Log("DICEFREE_CONTROLS_DEFAULTS_OK");

                    var interact = Entry(bindings, "Gameplay/Interact");
                    bindings.ApplyOverride(interact, "<Keyboard>/j");
                    bindings.Save();
                    ControlsAuthoring.Require(interact.Path == "<Keyboard>/j", "Simple override path did not apply immediately.");
                    ControlsAuthoring.Require(interact.Action.controls.Contains(keyboard.jKey),
                        "Simple rebind did not resolve the J key on the live action.");

                    var up = Entry(bindings, "Gameplay/Direct movement", "Up");
                    bindings.ApplyOverride(up, "<Keyboard>/t");
                    ControlsAuthoring.Require(up.Path == "<Keyboard>/t", "Composite override path did not apply immediately.");
                    ControlsAuthoring.Require(bindings.Action("Gameplay/Direct movement").controls.Contains(keyboard.tKey),
                        "Composite rebind did not resolve the T key on the live movement action.");

                    var destination = Entry(bindings, "Gameplay/Move destination");
                    bindings.ApplyOverride(destination, "<Mouse>/middleButton");
                    ControlsAuthoring.Require(destination.Path == "<Mouse>/middleButton", "Mouse override path did not apply immediately.");
                    ControlsAuthoring.Require(destination.Action.controls.Contains(mouse.middleButton),
                        "Mouse-button rebind did not resolve middle mouse on the live action.");

                    var stop = Entry(bindings, "Gameplay/Stop");
                    bindings.ApplyOverride(stop, "<Keyboard>/j");
                    ControlsAuthoring.Require(bindings.Conflict(interact).Contains("Stop"), "Shared binding conflict feedback missing.");
                    bindings.Save();
                    bindings.Save();
                    Debug.Log("DICEFREE_CONTROLS_REBIND_CONFLICT_OK");
                }

                using (var restored = new InputBindings(authored, directory))
                {
                    ControlsAuthoring.Require(Entry(restored, "Gameplay/Interact").Path == "<Keyboard>/j", "Saved simple override not restored.");
                    ControlsAuthoring.Require(Entry(restored, "Gameplay/Direct movement", "Up").Path == "<Keyboard>/t", "Saved composite override not restored.");
                    ControlsAuthoring.Require(Entry(restored, "Gameplay/Move destination").Path == "<Mouse>/middleButton", "Saved mouse override not restored.");
                }

                string primary = Path.Combine(directory, "bindings.json");
                File.WriteAllText(primary, "{broken");
                using (var recovered = new InputBindings(authored, directory))
                {
                    ControlsAuthoring.Require(recovered.Status.Contains("backup"), "Corrupt primary did not recover from backup.");
                    ControlsAuthoring.Require(Entry(recovered, "Gameplay/Interact").Path == "<Keyboard>/j", "Backup recovery lost overrides.");
                    recovered.ResetDefaults();
                }

                using (var defaults = new InputBindings(authored, directory))
                {
                    Default(defaults, "Gameplay/Interact", null, "<Keyboard>/i");
                    Default(defaults, "Gameplay/Direct movement", "Up", "<Keyboard>/w");
                    Default(defaults, "Gameplay/Move destination", null, "<Mouse>/rightButton");
                    ControlsAuthoring.Require(Entry(defaults, "Gameplay/Close interaction").Path == "<Keyboard>/escape" &&
                                            Entry(defaults, "Gameplay/Clear target").Path == "<Keyboard>/escape" &&
                                            defaults.Action("UI/Close options").bindings[0].effectivePath == "<Keyboard>/escape",
                        "Closed-state Escape gameplay defaults changed.");
                    Debug.Log("DICEFREE_CONTROLS_PERSIST_RESET_ESCAPE_OK");
                }

                using (var modal = new InputBindings(authored, null))
                {
                    modal.SetModal(true);
                    ControlsAuthoring.Require(!modal.Asset.FindActionMap("Gameplay", true).enabled, "Gameplay map remained active under Options.");
                    ControlsAuthoring.Require(!modal.Asset.FindActionMap("Camera", true).enabled, "Camera map remained active under Options.");
                    ControlsAuthoring.Require(modal.Asset.FindActionMap("UI", true).enabled, "UI map was disabled under Options.");
                    modal.SetModal(false);
                    ControlsAuthoring.Require(modal.Asset.FindActionMap("Gameplay", true).enabled &&
                                            modal.Asset.FindActionMap("Camera", true).enabled,
                        "Gameplay/Camera maps did not resume after modal close.");
                    Debug.Log("DICEFREE_CONTROLS_MODAL_OK");
                }

                Debug.Log("DICEFREE_CONTROLS_OK");
                return new { success = true, finalMarker = "DICEFREE_CONTROLS_OK", temporarySettingsRoot = directory };
            }
            finally
            {
                InputSystem.settings.updateMode = priorMode;
                if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
                if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
                try { if (Directory.Exists(directory)) Directory.Delete(directory, true); } catch { }
            }
        }

        private static InputBindings.Entry Entry(InputBindings bindings, string actionPath, string part = null)
        {
            var action = bindings.Action(actionPath);
            return bindings.Entries.Single(entry =>
                entry.Action == action &&
                (part == null
                    ? !action.bindings[entry.Index].isPartOfComposite
                    : string.Equals(action.bindings[entry.Index].name, part, StringComparison.OrdinalIgnoreCase)));
        }

        private static void Default(InputBindings bindings, string actionPath, string part, string expected)
        {
            string actual = part == null
                ? bindings.Action(actionPath).bindings.First(binding => !binding.isComposite && !binding.isPartOfComposite).effectivePath
                : Entry(bindings, actionPath, part).Path;
            ControlsAuthoring.Require(string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
                actionPath + (part == null ? "" : "/" + part) + " default changed: " + actual);
        }

        private static void Release(Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
        }

        private static void Press(Keyboard keyboard, Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();
        }

        private static void Release(Mouse mouse)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
        }

        private static void PressMiddle(Mouse mouse)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 4 });
            InputSystem.Update();
        }
    }
}
