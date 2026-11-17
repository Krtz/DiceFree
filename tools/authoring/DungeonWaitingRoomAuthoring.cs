using System;
using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.Dungeons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static class DungeonWaitingRoomAuthoring
{
    private const string Prefab = "Assets/_DiceFree/Resources/DungeonWaitingRoom.prefab";
    private const string Navigation = "Assets/_DiceFree/Settings/Dungeons/DungeonWaitingRoomNavMesh.asset";
    private const string Stone = "Assets/_DiceFree/Art/World/World1Handcrafted/";

    public static void Batch()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        var room = new GameObject("Dungeon Waiting Room");
        try
        {
            var ground = Material(Stone + "Slate.mat");
            var walls = Material(Stone + "SlateDark.mat");
            var trim = Material(Stone + "PaleStone.mat");
            var wood = Material(Stone + "DarkOak.mat");
            var copper = Material(Stone + "Brass.mat");
            var glow = Material(Stone + "Ember.mat");

            Piece(room, "Walkable floor", new Vector3(0,-.2f,0), new Vector3(23,.4f,19), ground, 8);
            Piece(room, "Foundation", new Vector3(0,-.85f,0), new Vector3(24.4f,.8f,20.4f), walls);
            for (int i = -4; i <= 4; i++)
            {
                float x = i * 2.49f;
                Piece(room, "Entry stonework", new Vector3(x,1.05f,-9.45f), new Vector3(2.45f,2.10f,.94f), walls, 9);
                Piece(room, "Back wall", new Vector3(x,2.15f,9.37f), new Vector3(2.47f,4.35f,1.03f), walls, 9);
                Piece(room, "Back-wall cap", new Vector3(x,4.35f,9.32f), new Vector3(2.42f,.34f,1.22f), trim);
            }

            foreach (int side in new[] { -1, 1 })
            {
                for (int i = -3; i <= 3; i++)
                {
                    float z = i*2.72f;
                    Piece(room, "Side wall", new Vector3(side*11.2f,1.54f,z), new Vector3(1,3.1f,2.69f), walls, 9);
                    Piece(room, "Stone caps", new Vector3(side*11.13f,3.17f,z), new Vector3(1.21f,.25f,2.62f), trim);
                }
                for (int i = 0; i < 3; i++)
                {
                    float z = -5.3f+i*5;
                    Piece(room, "Inset granite paver", new Vector3(side*3.38f,.023f,z),new Vector3(1.6f,.055f,1.78f),trim);
                    Piece(room, "Pillar", new Vector3(side*10.4f,1.55f,z),new Vector3(1.03f,3.15f,1.14f),ground,9);
                    if (i == 1) continue;
                    Piece(room, "Bench seat", new Vector3(side*8.8f,.49f,z),new Vector3(.87f,.19f,2.9f),wood);
                    foreach (int end in new[] { -1,1 })
                        Piece(room, "Bench leg", new Vector3(side*8.8f,.24f,z+end*.99f),new Vector3(.72f,.49f,.21f),ground);
                }
                Torch(room, new Vector3(side*7.55f,0,5.4f), walls, copper, glow);
                Torch(room, new Vector3(side*7.55f,0,-5), walls, copper, glow);
            }
            Piece(room, "Training circle", new Vector3(-4.6f,.028f,3.85f), new Vector3(3.4f,.033f,3.4f),
                trim, shape: PrimitiveType.Cylinder);
            var light = Node(room, "Warm chamber light", new Vector3(0,8,0)).gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f,.83f,.62f);
            light.intensity = 20;
            light.range = 26;

            var arrival = Node(room, "Party arrival", new Vector3(0,0,-3.9f));
            var dummy = Node(room, "Practice target", new Vector3(-4.6f,.07f,3.85f));
            var nav = Bake(room, arrival.position, dummy.position);
            var saved = AssetDatabase.LoadAssetAtPath<NavMeshData>(Navigation);
            if (saved == null) { AssetDatabase.CreateAsset(nav, Navigation); saved = nav; }
            else { EditorUtility.CopySerialized(nav,saved); EditorUtility.SetDirty(saved); Object.DestroyImmediate(nav); }
            var definition = AssetDatabase.LoadAssetAtPath<ActorDefinition>(
                "Assets/_DiceFree/Settings/Dungeons/Practice target — no rewards.asset");
            if (definition == null) throw new InvalidOperationException("Missing the Slime Dungeon practice target.");
            room.AddComponent<DungeonWaitingRoom>().Configure(saved, definition, arrival, dummy);
            if (PrefabUtility.SaveAsPrefabAsset(room, Prefab) == null)
                throw new InvalidOperationException("Could not save the waiting room.");
            AssetDatabase.SaveAssets();
        }
        finally { Object.DestroyImmediate(room); }
    }

    private static NavMeshData Bake(GameObject room, Vector3 arrival, Vector3 dummy)
    {
        Physics.SyncTransforms();
        var sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(room.transform, (1 << 8) | (1 << 9), NavMeshCollectGeometry.PhysicsColliders,
            0, new List<NavMeshBuildMarkup>(), sources);
        var settings = NavMesh.GetSettingsByID(0);
        settings.agentRadius = .675f;
        var data = NavMeshBuilder.BuildNavMeshData(settings, sources, new Bounds(Vector3.up, new Vector3(28,7,24)),
            Vector3.zero, Quaternion.identity);
        if (data == null) throw new InvalidOperationException("No navigation data.");
        var instance = NavMesh.AddNavMeshData(data);
        try
        {
            var path = new NavMeshPath();
            if (!NavMesh.SamplePosition(arrival, out var from, .7f, 1) ||
                !NavMesh.SamplePosition(dummy, out var to, .7f, 1) ||
                !NavMesh.CalculatePath(from.position, to.position, 1, path) || path.status != NavMeshPathStatus.PathComplete)
                throw new InvalidOperationException("The party arrival and practice target are disconnected.");
        }
        finally { instance.Remove(); }
        return data;
    }

    private static void Torch(GameObject room, Vector3 point, Material stone, Material copper, Material fire)
    {
        Piece(room, "Brazier pedestal", point + Vector3.up*.56f,new Vector3(.62f,1.1f,.60f),stone);
        Piece(room, "Copper bowl", point + Vector3.up*1.13f,new Vector3(1.12f,.19f,1.12f),copper,shape:PrimitiveType.Cylinder);
        Piece(room, "Coal flame", point + Vector3.up*1.38f,new Vector3(.51f,.55f,.51f),fire,shape:PrimitiveType.Sphere);
        var flame = Node(room,"Brazier light",point+Vector3.up*1.62f).gameObject.AddComponent<Light>();
        flame.type=LightType.Point;
        flame.color=new Color(1f,.57f,.26f);
        flame.range=6.2f;
        flame.intensity=3.8f;
    }

    private static Transform Node(GameObject room, string name, Vector3 position)
    {
        var node = new GameObject(name).transform;
        node.SetParent(room.transform, false);
        node.localPosition = position;
        return node;
    }

    private static void Piece(GameObject room, string name, Vector3 position, Vector3 dimensions, Material material,
        int layer = 2, PrimitiveType shape = PrimitiveType.Cube)
    {
        var piece = GameObject.CreatePrimitive(shape);
        piece.name = name;
        piece.layer = layer;
        piece.transform.SetParent(room.transform, false);
        piece.transform.localPosition = position;
        piece.transform.localScale = dimensions;
        piece.GetComponent<Renderer>().sharedMaterial = material;
        if (layer == 2) Object.DestroyImmediate(piece.GetComponent<Collider>());
    }

    private static Material Material(string path) =>
        AssetDatabase.LoadAssetAtPath<Material>(path) ?? throw new InvalidOperationException("Missing material "+path);
}
