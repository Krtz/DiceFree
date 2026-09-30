using DiceFree.Characters;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class CornbergSurfaceSetup
    {
        [MenuItem("DiceFree/Travel/Add road speed to saved Cornberg")]
        public static void Install()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var road = GameObject.Find("Mountain to east road");
            if (road == null || road.GetComponent<MeshFilter>() == null) throw new System.InvalidOperationException("Existing road mesh required.");
            const string folder = "Assets/_DiceFree/Settings/Travel";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "Travel");
            var data = AssetDatabase.LoadAssetAtPath<TravelSurfaceDefinition>(folder + "/Cornberg dirt road.asset");
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<TravelSurfaceDefinition>();
                data.stableId = "surface.cornberg.dirt-road"; data.displayName = "Dirt road";
                data.speedBonusPercent = 15; // Explicitly provisional authored balance.
                AssetDatabase.CreateAsset(data, folder + "/Cornberg dirt road.asset");
            }
            if (!road.TryGetComponent<TravelSurface>(out var surface))
            {
                surface = road.AddComponent<TravelSurface>();
                surface.Configure("surface-area.cornberg.east-road", data, road.GetComponent<MeshFilter>(),
                    new Bounds(new Vector3(72, 0, 25), new Vector3(94, 10, 70)));
            }
            var player = Object.FindAnyObjectByType<TraversalInput>();
            if (!player.TryGetComponent<SurfaceTravel>(out _)) player.gameObject.AddComponent<SurfaceTravel>();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("CORNBERG_SURFACE_SETUP_OK: existing east-road mesh, player opt-in, provisional +15%; no geometry/nav changes.");
        }
    }
}
