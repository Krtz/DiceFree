using System;
using System.IO;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.EditorTools
{
    public static class ControlsAuthoring
    {
        internal const string AssetPath = "Assets/_DiceFree/Resources/DiceFreeControls.inputactions";

        [CliCommand("dicefree.controls.prepare", "Create/verify the canonical DiceFree Input System action asset.", Tags = new[] { "input", "authoring" })]
        public static object Prepare()
        {
            if (!File.Exists(AssetPath))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(AssetPath));
                var asset = ScriptableObject.CreateInstance<InputActionAsset>();
                asset.name = "DiceFreeControls";

                var gameplay = new InputActionMap("Gameplay");
                asset.AddActionMap(gameplay);
                var movement = gameplay.AddAction("Direct movement", InputActionType.Value);
                movement.expectedControlType = "Vector2";
                movement.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/w")
                    .With("Down", "<Keyboard>/s")
                    .With("Left", "<Keyboard>/a")
                    .With("Right", "<Keyboard>/d");
                Button(gameplay, "Move destination", "<Mouse>/rightButton");
                Button(gameplay, "Switch control mode", "<Keyboard>/f6");
                Button(gameplay, "Stop", "<Keyboard>/space");
                Button(gameplay, "Interact", "<Keyboard>/i");
                Button(gameplay, "Close interaction", "<Keyboard>/escape");
                Button(gameplay, "Select target", "<Mouse>/leftButton");
                Button(gameplay, "Cycle hostile", "<Keyboard>/tab");
                Button(gameplay, "Attack selected", "<Keyboard>/x");
                Button(gameplay, "Clear target", "<Keyboard>/escape");
                Button(gameplay, "Return to anchor", "<Keyboard>/r");
                Button(gameplay, "Inventory", "<Keyboard>/b");

                var camera = new InputActionMap("Camera");
                asset.AddActionMap(camera);
                var zoom = camera.AddAction("Zoom", InputActionType.Value);
                zoom.expectedControlType = "Axis";
                zoom.AddBinding("<Mouse>/scroll/y");
                Button(camera, "Look toward World 1", "<Keyboard>/v");
                var pan = camera.AddAction("Camera pan", InputActionType.Value);
                pan.expectedControlType = "Vector2";
                pan.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/upArrow")
                    .With("Down", "<Keyboard>/downArrow")
                    .With("Left", "<Keyboard>/leftArrow")
                    .With("Right", "<Keyboard>/rightArrow");
                Button(camera, "Camera drag", "<Mouse>/middleButton");
                var pointerDelta = camera.AddAction("Camera pointer delta", InputActionType.Value);
                pointerDelta.expectedControlType = "Vector2";
                pointerDelta.AddBinding("<Mouse>/delta");
                Button(camera, "Recenter", "<Keyboard>/home");
                Button(camera, "Toggle follow", "<Keyboard>/f");
                var rotate = camera.AddAction("Rotate camera", InputActionType.Value);
                rotate.expectedControlType = "Axis";
                rotate.AddCompositeBinding("1DAxis")
                    .With("Negative", "<Keyboard>/q")
                    .With("Positive", "<Keyboard>/e");

                var ui = new InputActionMap("UI");
                asset.AddActionMap(ui);
                Button(ui, "Options menu toggle", "<Keyboard>/f10");
                Button(ui, "Close options", "<Keyboard>/escape");

                File.WriteAllText(AssetPath, asset.ToJson());
                UnityEngine.Object.DestroyImmediate(asset);
            }

            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceSynchronousImport);
            var imported = AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetPath);
            Require(imported != null, "DiceFreeControls input asset did not import.");
            Require(imported.FindActionMap("Gameplay", true) != null &&
                    imported.FindActionMap("Camera", true) != null &&
                    imported.FindActionMap("UI", true) != null,
                "DiceFreeControls is missing required maps.");
            AssetDatabase.SaveAssets();

            Debug.Log("DICEFREE_CONTROLS_ASSET_OK: Gameplay/Camera/UI maps authored; F10 Options, fixed Escape close, shared gameplay Escape defaults preserved.");
            return new { success = true, finalMarker = "DICEFREE_CONTROLS_ASSET_OK", assetPath = AssetPath };
        }

        private static InputAction Button(InputActionMap map, string name, string binding)
        {
            var action = map.AddAction(name, InputActionType.Button);
            action.expectedControlType = "Button";
            action.AddBinding(binding);
            return action;
        }

        internal static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }
    }
}
