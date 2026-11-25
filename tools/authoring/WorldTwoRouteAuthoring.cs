using System;
using System.IO;
using System.Linq;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WorldTwoRouteAuthoring
{
    public const string ScenePath="Assets/_DiceFree/Scenes/WorldTwoArrival.unity";
    public static readonly string[] RouteNames={
        "Ashfall trail","Road to Cinderwatch","Cinderwatch to Willowford",
        "Willowford bridge and riverbank trail","Willowford south street",
        "Road to the Basalt Needle","Road to Ember Crown","Southern world circuit","Western river road",
        "Anvilreach crown trade road","Anvilreach northern trade road","Anvilreach foundry avenue","Anvilreach artisans lane",
        "Anvilreach paved exchange","Anvilreach paved main avenue","Anvilreach paved guild road","Walkable timber bridge"
    };
    public static TravelSurface Configure(MeshFilter mesh)
    {
        if(mesh==null||mesh.sharedMesh==null)throw new InvalidOperationException("A route needs its visible footprint.");
        var road=AssetDatabase.LoadAssetAtPath<TravelSurfaceDefinition>("Assets/_DiceFree/Settings/Travel/Cornberg dirt road.asset");
        if(road==null||!Mathf.Approximately(road.speedBonusPercent,15))throw new InvalidOperationException("Standard road bonus is missing.");
        var surface=mesh.GetComponent<TravelSurface>()??mesh.gameObject.AddComponent<TravelSurface>();
        // Mesh asset GUID plus hierarchy distinguishes the two bridge deck instances.
        string hierarchy=mesh.name;for(var p=mesh.transform.parent;p!=null;p=p.parent)hierarchy=p.name+"/"+hierarchy;
        surface.Configure("travel.world2."+hierarchy,road,mesh,new Bounds(Vector3.zero,Vector3.one*10000));
        EditorUtility.SetDirty(surface);return surface;
    }
    public static void Batch()
    {
        var scene=EditorSceneManager.OpenScene(ScenePath);
        var routes=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshFilter>(true)).Where(f=>RouteNames.Contains(f.name)).ToArray();
        foreach(string name in RouteNames)if(!routes.Any(r=>r.name==name))throw new InvalidOperationException("Missing authored route: "+name);
        int added=routes.Count(r=>r.GetComponent<TravelSurface>()==null);foreach(var route in routes)Configure(route);
        if(routes.Length!=18)throw new InvalidOperationException("Expected sixteen road/paving meshes and two bridge decks.");
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/WorldTwoRoutes.json",JsonUtility.ToJson(new Report{surfaces=routes.Length,added=added,bonusPercent=15,routes=routes.Select(r=>r.name).ToArray()},true));
        Debug.Log("DICEFREE_WORLD_TWO_ROUTES_OK");
    }
    [Serializable]sealed class Report{public int surfaces,added,bonusPercent;public string[] routes;}
}
