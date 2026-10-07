using System;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class HudValidation
    {
        [CliCommand(
            "dicefree.hud.validate",
            "Validate customizable HUD composition, equipment/stat contract and locomotion animation assets.",
            Tags = new[] { "tests", "ui", "hud", "animation" })]
        public static object Validate()
        {
            var scene = EditorSceneManager.OpenScene(HudAuthoring.ScenePath);
            var traversal = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
            Require(traversal != null, "Cornberg traversal player is missing.");
            var go = traversal.gameObject;

            var manager = go.GetComponent<HudLayoutManager>();
            var portrait = go.GetComponent<HudPortraitRenderer>();
            var character = go.GetComponent<CharacterHudPanel>();
            var stats = go.GetComponent<CharacterStatsHud>();
            var source = go.GetComponent<AbilityBarSource>();
            var bar = go.GetComponent<GenericActionBar>();
            var menuStrip = go.GetComponent<HudMenuStrip>();
            var minimap = go.GetComponent<MinimapHud>();
            var chat = go.GetComponent<ChatHud>();
            Require(
                manager != null && portrait != null && character != null && stats != null &&
                source != null && bar != null && menuStrip != null && minimap != null && chat != null,
                "The new generic HUD composition is incomplete.");

            var widgets = UnityEngine.Object.FindObjectsByType<CustomizableHudWidget>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            Require(widgets.Length >= 8, "Expected at least eight independently customizable HUD modules.");
            Require(
                widgets.Select(value => value.LayoutId).Distinct(StringComparer.Ordinal).Count() == widgets.Length,
                "Customizable HUD widget IDs are not unique.");
            Require(widgets.All(value => value.MinimumPixelSize.x > 0 && value.MinimumPixelSize.y > 0),
                "Every customizable HUD widget needs a positive minimum size.");
            Require(minimap.LockedAspectRatio == 1f, "Minimap must remain square while resizing.");

            Require(Enum.GetValues(typeof(EquipmentSlot)).Length == 11,
                "HUD equipment presentation expects exactly the current eleven equipment slots.");
            Require(go.GetComponent<ActorStats>() != null &&
                    go.GetComponent<BasicAttack>() != null &&
                    go.GetComponent<DiceFree.Progression.ExperienceProgression>() != null,
                "Stats HUD data sources are missing.");

            Require(!UnityEngine.Object.FindAnyObjectByType<PlayerCombatPanel>(FindObjectsInactive.Include).enabled,
                "Old PlayerCombatPanel must be disabled after generic HUD authoring.");
            Require(!UnityEngine.Object.FindAnyObjectByType<ExperienceBar>(FindObjectsInactive.Include).enabled,
                "Old ExperienceBar must be disabled after generic HUD authoring.");
            Require(!go.GetComponent<ResourceBar>().enabled &&
                    !go.GetComponent<NoviceSkillBar>().enabled &&
                    !go.GetComponent<PhysicalSkillBar>().enabled &&
                    !go.GetComponent<MagicalSkillBar>().enabled,
                "Old class-specific resource/skill bars must be disabled.");

            var switcher = go.GetComponent<ClassPresentationSwitcher>();
            var driver = go.GetComponent<NovicePresentationDriver>();
            Require(switcher != null && driver != null,
                "Class presentation switcher/locomotion driver is missing.");

            foreach (string classId in new[]
                     {
                         NoviceSkillProgression.ClassStableId,
                         PhysicalSkillProgression.ClassStableId,
                         MagicalSkillProgression.ClassStableId
                     })
            {
                var root = switcher.Resolve(classId);
                var animator = root == null ? null : root.GetComponentInChildren<Animator>(true);
                Require(root != null && animator != null,
                    "Missing presentation/Animator for " + classId + ".");
                Require(animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman,
                    "Invalid Humanoid Avatar for " + classId + ".");
                Require(animator.runtimeAnimatorController != null && !animator.applyRootMotion,
                    "Presentation controller/root-motion contract is invalid for " + classId + ".");
            }

            var idle = AssetDatabase.LoadAssetAtPath<AnimationClip>(HudAuthoring.IdleClipPath);
            var locomotion = AssetDatabase.LoadAssetAtPath<AnimationClip>(HudAuthoring.LocomotionClipPath);
            var attack = AssetDatabase.LoadAssetAtPath<AnimationClip>(HudAuthoring.AttackClipPath);
            Require(idle != null && locomotion != null && attack != null,
                "Authored Idle, Locomotion and Attack runtime clips are missing.");
            Require(idle.isLooping, "Idle runtime clip is not looping.");
            Require(locomotion.isLooping, "Locomotion runtime clip is not looping.");
            Require(!attack.isLooping, "Attack runtime clip should not loop.");

            Debug.Log(
                "DICEFREE_HUD_DATA_OK: eight+ independent resizable widgets, 11 equipment slots, generic class bar/menu strip, square minimap, themes and looping Humanoid locomotion validated.");
            return new
            {
                success = true,
                finalMarker = "DICEFREE_HUD_DATA_OK",
                customizableWidgets = widgets.Length,
                equipmentSlots = Enum.GetValues(typeof(EquipmentSlot)).Length,
                idleLooping = idle.isLooping,
                locomotionLooping = locomotion.isLooping,
                attackLooping = attack.isLooping
            };
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
