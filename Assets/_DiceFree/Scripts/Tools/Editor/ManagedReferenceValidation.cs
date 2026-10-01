using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    /// <summary>
    /// Fails closed when a first-party SerializeReference cannot resolve its recorded type.
    /// This reports data only; it never clears, resets, or recreates a missing reference.
    /// </summary>
    public static class ManagedReferenceValidation
    {
        private const string FirstPartyRoot = "Assets/_DiceFree";
        private const string CornbergScene = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string EliteVariant = "Assets/_DiceFree/Settings/Enemies/Elite Forest Slime variant.asset";
        private const long EliteVariantPolicyId = 6556558292968538112L;
        private const long CropSlimePolicyId = 6556558292968538114L;
        private const long EliteScenePolicyId = 6556558292968538113L;

        [MenuItem("DiceFree/Validation/Missing managed references")]
        public static void Run()
        {
            var errors = new List<string>();
            Validate(errors);
            if (errors.Count > 0)
                throw new InvalidOperationException("DiceFree managed-reference validation failed:\n - " + string.Join("\n - ", errors));
            Debug.Log("DICEFREE_MANAGED_REFERENCE_OK: no first-party SerializeReference types are missing; AutoAggroPolicy compatibility records resolved with preserved IDs and values.");
        }

        internal static void Validate(List<string> errors)
        {
            ValidateScriptableObjects(errors);
            ValidatePrefabs(errors);
            ValidateScenes(errors);
            ValidateAutoAggroCompatibility(errors);
        }

        private static void ValidateScriptableObjects(List<string> errors)
        {
            foreach (var path in FindPaths("t:ScriptableObject"))
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset != null) ReportMissing(path, asset, errors);
        }

        private static void ValidatePrefabs(List<string> errors)
        {
            foreach (var path in FindPaths("t:Prefab"))
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    foreach (var host in root.GetComponentsInChildren<Component>(true))
                        if (host != null) ReportMissing(path, host, errors);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }

        private static void ValidateScenes(List<string> errors)
        {
            foreach (var path in FindPaths("t:Scene"))
            {
                var scene = OpenForInspection(path, out var opened);
                try
                {
                    foreach (var root in scene.GetRootGameObjects())
                    foreach (var host in root.GetComponentsInChildren<Component>(true))
                        if (host != null) ReportMissing(path, host, errors);
                }
                finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
            }
        }

        private static IEnumerable<string> FindPaths(string filter) =>
            AssetDatabase.FindAssets(filter, new[] { FirstPartyRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal);

        private static Scene OpenForInspection(string path, out bool opened)
        {
            var scene = SceneManager.GetSceneByPath(path);
            opened = !scene.IsValid() || !scene.isLoaded;
            return opened ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive) : scene;
        }

        private static void ReportMissing(string path, UnityEngine.Object host, List<string> errors)
        {
            if (host is not MonoBehaviour && host is not ScriptableObject) return;
            if (!SerializationUtility.HasManagedReferencesWithMissingTypes(host)) return;
            foreach (var missing in SerializationUtility.GetManagedReferencesWithMissingTypes(host))
            {
                string qualified = string.IsNullOrEmpty(missing.namespaceName)
                    ? missing.className : missing.namespaceName + "." + missing.className;
                errors.Add($"{path}: host '{host.name}' ({host.GetType().FullName}), reference ID {missing.referenceId}, missing {qualified} from assembly {missing.assemblyName}");
            }
        }

        private static void ValidateAutoAggroCompatibility(List<string> errors)
        {
            var variant = AssetDatabase.LoadAssetAtPath<EnemyVariantDefinition>(EliteVariant);
            if (variant == null) { errors.Add("missing authored Elite Forest Slime variant asset"); return; }
            ValidatePolicy(EliteVariant, variant, "autoAggroPolicy", EliteVariantPolicyId, variant.autoAggroPolicy, errors);

            var scene = OpenForInspection(CornbergScene, out var opened);
            try
            {
                ValidateScenePolicy(scene, "Crop Slime - combat placeholder", CropSlimePolicyId, errors);
                ValidateScenePolicy(scene, "Elite Forest Slime - combat placeholder", EliteScenePolicyId, errors);
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }

        private static void ValidateScenePolicy(Scene scene, string objectName, long expectedId, List<string> errors)
        {
            var host = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<AggroBehaviour>(true))
                .FirstOrDefault(candidate => candidate.gameObject.name == objectName);
            if (host == null) { errors.Add($"{CornbergScene}: missing expected AggroBehaviour host '{objectName}'"); return; }
            ValidatePolicy(CornbergScene + " / " + objectName, host, "autoAggroPolicy", expectedId, host.AutoAggroPolicy, errors);
        }

        private static void ValidatePolicy(string location, UnityEngine.Object host, string propertyName, long expectedId, AutoAggroPolicy policy, List<string> errors)
        {
            if (policy == null) { errors.Add(location + ": AutoAggroPolicy did not resolve"); return; }
            if (policy.alwaysAutoAggro || policy.trivialLevelGap != 10)
                errors.Add(location + ": AutoAggroPolicy values changed during assembly migration");
            var property = new SerializedObject(host).FindProperty(propertyName);
            if (property == null || property.managedReferenceId != expectedId)
                errors.Add(location + $": AutoAggroPolicy managed reference ID changed (expected {expectedId})");
        }
    }
}
