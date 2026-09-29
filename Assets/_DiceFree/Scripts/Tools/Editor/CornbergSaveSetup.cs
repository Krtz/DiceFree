using DiceFree.Characters;
using DiceFree.Persistence;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class CornbergSaveSetup
    {
        [MenuItem("DiceFree/Persistence/Add local autosave to saved Cornberg")]
        public static void Install()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player = Object.FindAnyObjectByType<TraversalInput>().gameObject;
            var anchorObject = GameObject.Find("Cornberg resurrection anchor");
            if (anchorObject == null) throw new System.InvalidOperationException("Cornberg anchor missing.");
            if (!anchorObject.TryGetComponent<ResurrectionAnchor>(out var anchor)) anchor = anchorObject.AddComponent<ResurrectionAnchor>();
            anchor.Configure("anchor.cornberg");
            if (!player.GetComponent<ManifestationPersistence>()) player.AddComponent<ManifestationPersistence>();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("CORNBERG_SAVE_SETUP_OK");
        }
    }
}
