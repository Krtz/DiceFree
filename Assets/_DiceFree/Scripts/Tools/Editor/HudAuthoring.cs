using System;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class HudAuthoring
    {
        public const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        public const string NoviceModelPath = "Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx";
        public const string ControllerPath = "Assets/_DiceFree/Art/Characters/Novice/Animations/Novice.controller";
        public const string IdleClipPath = "Assets/_DiceFree/Art/Characters/Novice/Animations/NoviceIdle.anim";
        public const string LocomotionClipPath = "Assets/_DiceFree/Art/Characters/Novice/Animations/NoviceLocomotion.anim";
        public const string AttackClipPath = "Assets/_DiceFree/Art/Characters/Novice/Animations/NoviceUnarmedAttack.anim";

        [CliCommand("dicefree.hud.inspect-animations", "Inspect Novice FBX animation take import settings.", Tags = new[] { "tests", "animation" })]
        public static object InspectAnimations()
        {
            var importer = AssetImporter.GetAtPath(NoviceModelPath) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Novice FBX importer is missing.");
            return new
            {
                success = true,
                configured = importer.clipAnimations.Select(value => new { value.name, value.takeName, value.loopTime, value.loopPose }).ToArray(),
                defaults = importer.defaultClipAnimations.Select(value => new { value.name, value.takeName, value.loopTime, value.loopPose, value.firstFrame, value.lastFrame }).ToArray(),
                runtime = AssetDatabase.LoadAllAssetsAtPath(NoviceModelPath).OfType<AnimationClip>()
                    .Where(value => !value.name.StartsWith("__preview__", StringComparison.Ordinal))
                    .Select(value => new { value.name, value.isLooping, value.length, value.frameRate }).ToArray()
            };
        }

        [MenuItem("DiceFree/UI/Prepare Customizable HUD")]
        [CliCommand(
            "dicefree.hud.prepare",
            "Author the customizable WC3-style HUD and repair player locomotion clip looping.",
            Tags = new[] { "ui", "hud", "animation" })]
        public static object Prepare()
        {
            CreateRuntimeAnimationClips();

            var scene = EditorSceneManager.OpenScene(ScenePath);

            var explorationCamera = UnityEngine.Object.FindAnyObjectByType<ExplorationCamera>();
            if (explorationCamera == null) throw new InvalidOperationException("Cornberg exploration camera is missing.");
            var cameraSettings = new SerializedObject(explorationCamera);
            cameraSettings.FindProperty("distance").floatValue = 40f;
            cameraSettings.FindProperty("pitch").floatValue = 52f;
            cameraSettings.ApplyModifiedPropertiesWithoutUndo();
            explorationCamera.ConfigureZoom(14f, 78f, 0.085f);
            var viewCamera = explorationCamera.GetComponent<Camera>();
            viewCamera.fieldOfView = 50f;
            EditorUtility.SetDirty(explorationCamera);
            EditorUtility.SetDirty(viewCamera);

            var traversal = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
            if (traversal == null) throw new InvalidOperationException("Cornberg traversal player is missing.");

            var playerObject = traversal.gameObject;
            var actor = playerObject.GetComponent<CombatActor>();
            if (actor == null) throw new InvalidOperationException("Cornberg player CombatActor is missing.");

            Ensure<HudLayoutManager>(playerObject);
            var portrait = Ensure<HudPortraitRenderer>(playerObject);
            var character = Ensure<CharacterHudPanel>(playerObject);
            character.Configure(actor);
            var stats = Ensure<CharacterStatsHud>(playerObject);
            stats.Configure(actor);
            Ensure<AbilityBarSource>(playerObject);
            Ensure<GenericActionBar>(playerObject);
            Ensure<DiceFree.Gameplay.CodexProgression>(playerObject);
            Ensure<CodexPanel>(playerObject);
            Ensure<HudMenuStrip>(playerObject);

            var minimap = Ensure<MinimapHud>(playerObject);
            minimap.Configure(actor);
            var chat = Ensure<ChatHud>(playerObject);
            chat.Configure(playerObject.GetComponent<DiceFree.Progression.ExperienceProgression>());
            var death = Ensure<DeathRespawnHud>(playerObject);
            death.Configure(actor);

            Disable<PlayerCombatPanel>();
            Disable<CombatCommandsPanel>();
            Disable<ExperienceBar>();
            Disable<TraversalOverlay>();
            var resource = playerObject.GetComponent<ResourceBar>();
            if (resource != null) resource.enabled = false;
            var noviceBar = playerObject.GetComponent<NoviceSkillBar>();
            if (noviceBar != null) noviceBar.enabled = false;
            var physicalBar = playerObject.GetComponent<PhysicalSkillBar>();
            if (physicalBar != null) physicalBar.enabled = false;
            var magicalBar = playerObject.GetComponent<MagicalSkillBar>();
            if (magicalBar != null) magicalBar.enabled = false;

            // Existing target and quest objects become customizable automatically through their new base class.
            var target = UnityEngine.Object.FindAnyObjectByType<TargetFrame>();
            var quests = UnityEngine.Object.FindAnyObjectByType<QuestTracker>();
            if (target == null || quests == null)
                throw new InvalidOperationException("Existing target/quest HUD composition is missing.");

            EditorUtility.SetDirty(playerObject);
            EditorUtility.SetDirty(portrait);
            EditorUtility.SetDirty(character);
            EditorUtility.SetDirty(stats);
            EditorUtility.SetDirty(minimap);
            EditorUtility.SetDirty(chat);
            EditorUtility.SetDirty(death);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
                throw new InvalidOperationException("Could not save Cornberg HUD composition.");
            AssetDatabase.SaveAssets();

            Debug.Log("DICEFREE_HUD_AUTHORED_OK: customizable character/stats/action/minimap/quest/chat/target HUD and looping locomotion clips prepared.");
            return new
            {
                success = true,
                finalMarker = "DICEFREE_HUD_AUTHORED_OK",
                equipmentSlots = Enum.GetValues(typeof(DiceFree.Items.EquipmentSlot)).Length,
                customizableWidgets = UnityEngine.Object.FindObjectsByType<CustomizableHudWidget>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None).Length
            };
        }

        private static void CreateRuntimeAnimationClips()
        {
            var sources = AssetDatabase.LoadAllAssetsAtPath(NoviceModelPath)
                .OfType<AnimationClip>()
                .Where(value => !value.name.StartsWith("__preview__", StringComparison.Ordinal))
                .ToArray();

            var idleSource = sources.SingleOrDefault(value =>
                value.name.EndsWith("Idle", StringComparison.OrdinalIgnoreCase));
            var locomotionSource = sources.SingleOrDefault(value =>
                value.name.EndsWith("Locomotion", StringComparison.OrdinalIgnoreCase));
            var attackSource = sources.SingleOrDefault(value =>
                value.name.IndexOf("Attack", StringComparison.OrdinalIgnoreCase) >= 0);

            if (idleSource == null || locomotionSource == null || attackSource == null)
                throw new InvalidOperationException(
                    "Expected Idle, Locomotion and Attack clips were not all found in the Novice FBX.");

            var idle = CopyClip(idleSource, IdleClipPath, "Novice Idle", true);
            var locomotion = CopyClip(locomotionSource, LocomotionClipPath, "Novice Locomotion", true);
            var attack = CopyClip(attackSource, AttackClipPath, "Novice Unarmed Attack", false);

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null || controller.layers.Length == 0)
                throw new InvalidOperationException("Novice AnimatorController is missing.");

            var states = controller.layers[0].stateMachine.states;
            var idleState = states.Select(value => value.state).SingleOrDefault(value => value.name == "Idle");
            var locomotionState = states.Select(value => value.state).SingleOrDefault(value => value.name == "Locomotion");
            var attackState = states.Select(value => value.state).SingleOrDefault(value => value.name == "UnarmedAttack");
            if (idleState == null || locomotionState == null || attackState == null)
                throw new InvalidOperationException("Novice AnimatorController states are incomplete.");

            idleState.motion = idle;
            locomotionState.motion = locomotion;
            attackState.motion = attack;
            EditorUtility.SetDirty(idleState);
            EditorUtility.SetDirty(locomotionState);
            EditorUtility.SetDirty(attackState);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }

        private static AnimationClip CopyClip(
            AnimationClip source,
            string outputPath,
            string displayName,
            bool loop)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(outputPath);
            if (existing == null)
            {
                existing = new AnimationClip();
                AssetDatabase.CreateAsset(existing, outputPath);
            }

            EditorUtility.CopySerialized(source, existing);
            existing.name = displayName;
            var settings = AnimationUtility.GetAnimationClipSettings(existing);
            settings.loopTime = loop;
            settings.loopBlend = loop;
            AnimationUtility.SetAnimationClipSettings(existing, settings);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static T Ensure<T>(GameObject value) where T : Component =>
            value.TryGetComponent<T>(out var existing) ? existing : value.AddComponent<T>();

        private static void Disable<T>() where T : Behaviour
        {
            foreach (var component in UnityEngine.Object.FindObjectsByType<T>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                component.enabled = false;
                EditorUtility.SetDirty(component);
            }
        }
    }
}
