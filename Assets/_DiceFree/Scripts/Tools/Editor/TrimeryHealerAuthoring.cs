using System;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.UI;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DiceFree.EditorTools
{
    public static class TrimeryHealerAuthoring
    {
        public const string Name = "Trimery priest - placeholder healer";
        private const string ModelPath = "Assets/_DiceFree/Art/Characters/Dawnwarden/Dawnwarden.fbx";
        private const string ControllerPath = "Assets/_DiceFree/Resources/PlayerMotion/TierTwo/Priest.controller";

        private static void InstallVisual(Transform healer)
        {
            const string visualName = "Dawnwarden Healer";
            var visual = healer.Find(visualName);
            if (visual == null)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
                if (model == null) throw new InvalidOperationException("Dawnwarden model is missing.");
                visual = ((GameObject)PrefabUtility.InstantiatePrefab(model, healer)).transform;
                visual.name = visualName;
            }

            var animator = visual.GetComponent<Animator>();
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            if (animator == null || animator.avatar == null || !animator.avatar.isValid || controller == null)
                throw new InvalidOperationException("Dawnwarden rig or idle controller is missing.");

            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.Euler(0, 90, 0);
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            if (healer.TryGetComponent<VillageVisualIdle>(out var oldIdle))
                Object.DestroyImmediate(oldIdle);
            var placeholder = healer.Find("Soft priest blob");
            if (placeholder != null) Object.DestroyImmediate(placeholder.gameObject);
        }

        public static HealerInteraction InstallInto(Scene scene)
        {
            var all = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
            var parent = all.Single(t => t.name == TrimeryVillageAuthoring.RootName);
            var old = parent.Find(Name);
            if (old != null)
            {
                var existing = old.GetComponent<HealerInteraction>();
                if (existing == null) throw new InvalidOperationException("The Trimery healer interaction is missing.");
                InstallVisual(old);
                return existing;
            }
            var go = new GameObject(Name);
            go.transform.SetParent(parent, false);
            go.transform.position = CornbergLandscape.Ground(244, 190);
            // Interaction selection uses layer 10; not layer 9 (solid world/navigation).
            go.layer = 10;
            var selection = go.AddComponent<CapsuleCollider>();
            selection.center = Vector3.up * .85f;
            selection.radius = .6f;
            selection.height = 1.8f;
            ConfigureStationaryNpcObstacle(go, selection);
            InstallVisual(go.transform);
            var approach = new GameObject("Roadside treatment point").transform;
            approach.SetParent(go.transform, false);
            approach.position = CornbergLandscape.Ground(246, 190);
            var healer = go.AddComponent<HealerInteraction>();
            healer.ConfigureName("Trimery Priest - free healing");
            healer.ConfigureApproach(approach);
            var player = all.Select(t => t.GetComponent<TraversalInput>()).First(t => t != null).GetComponent<CombatActor>();
            go.AddComponent<InteractionPrompt>().Configure(healer, player);
            return healer;
        }

        private static NavMeshObstacle ConfigureStationaryNpcObstacle(GameObject root, CapsuleCollider selection)
        {
            var obstacle = root.GetComponent<NavMeshObstacle>();
            if (obstacle == null) obstacle = root.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Capsule;
            obstacle.center = selection.center;
            obstacle.radius = selection.radius;
            obstacle.height = selection.height;
            obstacle.carving = root.GetComponent<VillageHomeWander>() == null;
            EditorUtility.SetDirty(obstacle);
            return obstacle;
        }

        [CliCommand("dicefree.trimery.healer.repair-obstacle", "Add the matching stationary-NPC NavMeshObstacle to the existing Trimery healer only.")]
        public static object RepairObstacle()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode only");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save modified scenes first");
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene(TrimeryVillageAuthoring.ScenePath);
                var roots = scene.GetRootGameObjects()
                    .SelectMany(g => g.GetComponentsInChildren<Transform>(true));
                var healer = roots.Single(t => t.name == Name);
                var selection = healer.GetComponent<CapsuleCollider>();
                if (selection == null || !selection.enabled)
                    throw new InvalidOperationException("Trimery healer selection collider is missing or disabled");
                var obstacle = ConfigureStationaryNpcObstacle(healer.gameObject, selection);
                EditorUtility.SetDirty(healer.gameObject);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed");
                AssetDatabase.SaveAssets();
                return new
                {
                    repaired = true,
                    name = healer.name,
                    colliderCenter = selection.center.ToString(),
                    radius = obstacle.radius,
                    height = obstacle.height,
                    carving = obstacle.carving
                };
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
        }

        [CliCommand("dicefree.trimery.healer.install", "Install the Trimery healer or refresh her Dawnwarden visual in place.")]
        public static object Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode only");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save modified scenes first");
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene(TrimeryVillageAuthoring.ScenePath);
                var healer = InstallInto(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed");
                AssetDatabase.SaveAssets();
                return new { installed = true, name = healer.DisplayName, position = healer.transform.position.ToString(), approach = healer.ApproachPosition.ToString() };
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
        }
    }
}
