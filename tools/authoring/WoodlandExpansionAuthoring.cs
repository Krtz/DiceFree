using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Core;
using DiceFree.EditorTools;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

// Add both trails to the current overworld while keeping the original terrain edges and content.
public static class WoodlandExpansionAuthoring
{
    private const string World = "Assets/_DiceFree/Art/World/";
    private const string Art = World + "WoodlandExpansion";
    private const string Models = World + "VisualBatch01/Models/";
    private const string Trees = World + "CornbergBatch/Trees/Tree_";
    private const string RootName = "Fernhollow and Reedscale Eastwatch";
    private static readonly HashSet<Object> Changes = new();
    private static readonly List<MeshCollider> Floors = new();
    private static readonly System.Random Random = new(93504);
    private static readonly Vector2[] SouthTrail =
    {
        new(-34,305), new(-36,296), new(-42,285), new(-45,274), new(-43,264),
        new(-44,254), new(-50,243), new(-53,232), new(-46,221), new(-33,223),
        new(-27,234), new(-29,247), new(-36,255), new(-43,264)
    };
    private static readonly Vector2[] EastTrail =
    {
        new(-37,381), new(-31,379), new(-24,378), new(-17,382), new(-11,388),
        new(0,392), new(11,394), new(22,400), new(31,406), new(25,414),
        new(13,417), new(3,413), new(-5,406), new(0,400), new(11,394)
    };
    private static readonly (string name, Vector2 point)[] SouthMonsters =
    {
        ("Thornlash (Lv 31)", new(-55,242)), ("Sporecap (Lv 33)", new(-50,219)),
        ("Thornlash (Lv 32)", new(-28,242)), ("Lanterncap (Lv 32)", new(-34,228))
    };
    private static readonly (string name, Vector2 point)[] EastMonsters =
    {
        ("Reedscale Spear Guard (Lv 39)", new(2,397)),
        ("Reedscale Ridgebow Sentry (Lv 39)", new(22,405)),
        ("Reedscale Spear Guard (Lv 40)", new(13,412))
    };

    public static void Batch()
    {
        var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
        if (GameObject.Find(RootName) != null || AssetDatabase.IsValidFolder(Art))
            throw new InvalidOperationException("Woodland expansion is already present.");
        AssetDatabase.CreateFolder(World.TrimEnd('/'), "WoodlandExpansion");
        var verdant = Find("Rolling moss banks and spring basin");
        var lookout = Find("Rising forest banks and lookout ridge");
        Floors.Add(verdant.GetComponent<MeshCollider>());
        Floors.Add(lookout.GetComponent<MeshCollider>());

        var root = Group(null, RootName, Vector3.zero);
        var south = Group(root, "Fernhollow - southern Verdant Wilds", Vector3.zero);
        var east = Group(root, "Eastwatch - eastern Reedscale Lookout", Vector3.zero);
        var southGround = ExtendSouth(verdant);
        var eastGround = ExtendEast(lookout);
        var southMaterial = verdant.GetComponent<MeshRenderer>().sharedMaterial;
        var eastMaterial = lookout.GetComponent<MeshRenderer>().sharedMaterial;
        AddGround(south, "Fernhollow moss banks", southGround, southMaterial);
        AddGround(east, "Eastwatch hillside", eastGround, eastMaterial);
        Physics.SyncTransforms();

        TrimOverlappingScenery();
        Trail(south, "Fernhollow loop", SouthTrail, Find("Worn trail around Lanternmere").GetComponent<MeshRenderer>().sharedMaterial);
        Trail(east, "Eastwatch patrol loop", EastTrail, Find("North trail and lookout approach").GetComponent<MeshRenderer>().sharedMaterial);
        DressFernhollow(south);
        DressEastwatch(east);
        DressBoundary(south, "Fernhollow outer slope", SouthRim(southGround), new Vector3(-43,0,237), southMaterial);
        DressBoundary(east, "Eastwatch outer slope", EastRim(eastGround), new Vector3(13,0,399), eastMaterial);
        Physics.SyncTransforms();

        RebuildNavigation();
        var data = Object.FindAnyObjectByType<WorldNavigation>().Data;
        var navigation = NavMesh.AddNavMeshData(data);
        try
        {
            CheckTrail(SouthTrail);
            CheckTrail(EastTrail);
            Populate(south, Find("Wilds creatures"), SouthMonsters, SouthTrail[0]);
            Populate(east, Find("Reedscale defenders"), EastMonsters, EastTrail[0]);
            RequirePath(new Vector2(-34,305), EastTrail[0], "Lanternmere through the existing footbridge to Reedscale");
        }
        finally { navigation.Remove(); }

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("The expanded world could not be saved.");
        AssetDatabase.SaveAssets();
        File.WriteAllLines("Logs/WoodlandExpansionSceneIds.txt", Changes.Select(o =>
            GlobalObjectId.GetGlobalObjectIdSlow(o).targetObjectId.ToString()));
        File.WriteAllText("Logs/WoodlandExpansion.ok", "Both trail loops and all seven creature homes connect to the existing world.");
    }

    private static Mesh ExtendSouth(Transform original)
    {
        const int columns = 73;
        var source = original.GetComponent<MeshFilter>().sharedMesh.vertices;
        return Grid("Fernhollow walkable terrain", columns, 53, (column, row, cross, t) =>
        {
            var seam = original.TransformPoint(source[column]);
            float z = seam.z - 59 * t + Mathf.Sin(cross * Mathf.PI * 2) * t * 3.4f;
            float x = Mathf.Lerp(seam.x, -43 + (seam.x + 43) * .76f + 4 * Mathf.Sin(t * 2.5f), t);
            float banks = .8f * Mathf.Sin(x * .16f + z * .05f) * Mathf.Cos(z * .13f) +
                3.9f * Mathf.Max(Ramp(.82f,1,Mathf.Abs(cross * 2 - 1)),Ramp(.85f,1,t));
            float y = Mathf.Lerp(seam.y, 1.1f + banks, Ramp(0,.38f,t));
            return new Vector3(x,y,z);
        });
    }

    private static Mesh ExtendEast(Transform original)
    {
        const int oldColumns = 65;
        const int firstRow = 28;
        const int lastRow = 88;
        var source = original.GetComponent<MeshFilter>().sharedMesh.vertices;
        return Grid("Eastwatch walkable terrain", 57, lastRow - firstRow + 1, (column, row, t, along) =>
        {
            var seam = original.TransformPoint(source[(firstRow + row) * oldColumns + oldColumns - 1]);
            float opening = .025f + .975f * Ramp(0,.28f,along) * (1 - Ramp(.74f,1,along));
            float x = seam.x + 61 * t * opening;
            float z = seam.z + t * Mathf.Sin(along * 8.7f) * 2.3f * opening;
            float hills = Mathf.Sin(x * .16f + z * .13f) * .83f + Ramp(.62f,1,t) * 3.9f;
            float y = Mathf.Lerp(seam.y, 8.8f + hills, Ramp(0,.42f,t) * opening);
            return new Vector3(x,y,z);
        });
    }

    private static Mesh Grid(string name, int columns, int rows, Func<int,int,float,float,Vector3> point)
    {
        var vertices = new Vector3[columns * rows];
        var uv = new Vector2[vertices.Length];
        var triangles = new List<int>();
        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                int i = row * columns + column;
                var p = point(column,row,column / (columns - 1f),row / (rows - 1f));
                vertices[i] = p;
                uv[i] = new Vector2(p.x,p.z) * .105f;
                if (row == 0 || column == 0) continue;
                Triangle(vertices, triangles, i - columns - 1, i - columns, i - 1);
                Triangle(vertices, triangles, i - columns, i, i - 1);
            }
        return MeshAsset(name, vertices, uv, triangles);
    }

    private static List<Vector3> SouthRim(Mesh terrain)
    {
        const int columns = 73, rows = 53;
        var v = terrain.vertices;
        var rim = new List<Vector3>();
        for (int row = 0; row < rows; row++) rim.Add(v[row * columns]);
        for (int col = 1; col < columns; col++) rim.Add(v[(rows-1) * columns + col]);
        for (int row = rows-2; row >= 0; row--) rim.Add(v[row * columns + columns-1]);
        return rim;
    }

    private static List<Vector3> EastRim(Mesh terrain)
    {
        const int columns = 57, rows = 61;
        var v = terrain.vertices;
        var rim = new List<Vector3>();
        for (int col = 0; col < columns; col++) rim.Add(v[col]);
        for (int row = 1; row < rows; row++) rim.Add(v[row * columns + columns-1]);
        for (int col = columns-2; col >= 0; col--) rim.Add(v[(rows-1) * columns + col]);
        return rim;
    }

    private static void DressBoundary(Transform region, string name, List<Vector3> rim, Vector3 center, Material ground)
    {
        var root = Group(region,name,Vector3.zero);
        const int bands = 7;
        var mesh = Grid(name,bands,rim.Count,(column,row,t,along) =>
        {
            var lip = rim[row];
            var direction = lip - center;
            direction.y = 0;
            direction.Normalize();
            float fade = Mathf.Lerp(.08f,1f,Mathf.Min(Ramp(0,.12f,along),1-Ramp(.88f,1,along)));
            float width = (35f + 5f * Mathf.Sin(row*.13f)) * fade;
            var point = lip + direction * (width*t);
            point.y += (-.02f - 12.2f*t*t + 2.4f*Mathf.Sin(t*Mathf.PI) *
                Mathf.Sin(row*.10f + t*4)) * fade;
            return point;
        });
        Surface(root,name,mesh,ground,2);

        var vertices = mesh.vertices;
        for (int i = 8; i < rim.Count-7; i += 4)
        {
            int offset = i % 3 == 0 ? 3 : 2;
            var p = vertices[i*bands + offset];
            string tree = i % 3 == 1 ? "Pine" : "Spruce";
            Model(root,"Outer " + tree,Trees + tree + ".fbx",p,Between(6.3f,10.4f));
            if (i % 8 == 0)
                Model(root,"Forest rim boulder",Models + "rock_0" + (i % 5 + 1) + ".fbx",
                    vertices[(i+2)*bands+1],Between(1.55f,3.1f));
        }
    }

    private static void TrimOverlappingScenery()
    {
        var backdrop = Find("Wooded ridge beyond the lookout");
        var filter = backdrop.Find("Continuous ridge scenery").GetComponent<MeshFilter>();
        var old = filter.sharedMesh;
        var vertices = old.vertices;
        var triangles = old.triangles;
        var kept = new List<int>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            var center = (vertices[triangles[i]] + vertices[triangles[i+1]] + vertices[triangles[i+2]]) / 3;
            if (center.x > -13.1f && center.z is > 372 and < 426) continue;
            kept.Add(triangles[i]); kept.Add(triangles[i+1]); kept.Add(triangles[i+2]);
        }
        filter.sharedMesh = MeshAsset("Reedscale distant woodland remaining", vertices, old.uv, kept);
        Changes.Add(filter);

        foreach (Transform child in backdrop)
            if (child.name.StartsWith("Distant ",StringComparison.Ordinal) &&
                child.position.x > -13 && child.position.z is > 372 and < 426 && child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                Changes.Add(child.gameObject);
            }
    }

    private static void Trail(Transform root, string name, Vector2[] route, Material material)
    {
        var points = new List<Vector2>();
        for (int i = 1; i < route.Length; i++)
        {
            var before = route[Mathf.Max(i-2,0)];
            var start = route[i-1];
            var end = route[i];
            var after = route[Mathf.Min(i+1,route.Length-1)];
            int steps = Mathf.CeilToInt(Vector2.Distance(route[i-1],route[i]) / .65f);
            for (int j = 0; j < steps; j++)
            {
                float t = j / (float)steps;
                float square = t*t;
                float cube = square*t;
                points.Add(.5f*((2*start)+(-before+end)*t+
                    (2*before-5*start+4*end-after)*square+(-before+3*start-3*end+after)*cube));
            }
        }
        points.Add(route[^1]);
        float distance = 0;
        const int width = 7;
        var vertices = new Vector3[points.Count * width];
        var uv = new Vector2[vertices.Length];
        var triangles = new List<int>();
        for (int row = 0; row < points.Count; row++)
        {
            if (row > 0) distance += Vector2.Distance(points[row-1],points[row]);
            var tangent = (points[Mathf.Min(row+1,points.Count-1)] - points[Mathf.Max(0,row-1)]).normalized;
            var side = new Vector2(tangent.y,-tangent.x);
            for (int col = 0; col < width; col++)
            {
                float across = col / (width - 1f);
                int i = row * width + col;
                var p = points[row] + side * (across * 2 - 1) * (1.42f + .18f * Mathf.Sin(distance * .64f));
                vertices[i] = Ground(p) + Vector3.up * .062f;
                float fade = Mathf.Min(Ramp(0,3,row*.65f),Ramp(0,2,(points.Count-1-row)*.65f));
                uv[i] = new Vector2(Mathf.Lerp(1,across,fade), distance * .22f);
                if (row == 0 || col == 0) continue;
                Triangle(vertices,triangles,i-width-1,i-width,i-1);
                Triangle(vertices,triangles,i-width,i,i-1);
            }
        }
        Surface(root,name,MeshAsset(name,vertices,uv,triangles),material,2).shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private static void DressFernhollow(Transform root)
    {
        Scatter(root, new Rect(-70,213,54,50), SouthTrail, SouthMonsters, 62, false);
        var heart = new Vector2(-40,235);
        for (int i = 0; i < 9; i++)
        {
            float angle = i * 2.39996f;
            var p = heart + new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)) * (2.8f + i * .43f);
            Model(root,"Fernhollow mushroom",Models + "mushroom_0" + (i%4+1) + ".fbx",Ground(p),Between(.64f,2.3f));
        }
        Model(root,"Fallen grove tree",Models+"log_03.fbx",Ground(new Vector2(-43,232)),1.15f);
        Model(root,"Fernhollow willow",Trees+"Willow.fbx",Ground(heart),10.3f,true);
    }

    private static void DressEastwatch(Transform root)
    {
        Scatter(root, new Rect(-9,384,49,35), EastTrail, EastMonsters, 55, true);
        var oldCamp = Find("Reedscale lookout camp");
        CopyProp(root, oldCamp.Find("Sentry hide shelter"), "Eastwatch supply shelter", new Vector2(26,408), 50);
        CopyProp(root, oldCamp.Find("Reedscale split banner"), "Eastwatch trail banner", new Vector2(30,410), 30);
        CopyProp(root, oldCamp.Find("Reedscale split banner"), "Reedscale fork banner", new Vector2(-5,390), -25);
        Model(root,"Eastwatch barrel",Models+"barrel_03.fbx",Ground(new Vector2(24.6f,409)),.95f);
        Model(root,"Eastwatch crate",Models+"crate_02.fbx",Ground(new Vector2(25.5f,410.2f)),.89f);
        Model(root,"Eastwatch crate",Models+"crate_03.fbx",Ground(new Vector2(26.7f,410.2f)),.82f);
    }

    private static void Scatter(Transform root, Rect area, Vector2[] trail,
        (string name, Vector2 point)[] monsters, int attempts, bool east)
    {
        var trunks = new List<Vector2>();
        for (int i = 0; i < attempts; i++)
        {
            var p = new Vector2(Between(area.xMin,area.xMax),Between(area.yMin,area.yMax));
            if (!TryGround(p, out var floor) || DistanceFromTrail(p,trail) < 3.45f ||
                monsters.Any(monster => Vector2.Distance(p,monster.point) < 2.65f) ||
                Vector2.Distance(p,east ? new Vector2(26,408) : new Vector2(-40,235)) < 5) continue;

            if (i % 3 == 0 && trunks.All(trunk => Vector2.Distance(trunk,p) > 4.5f))
            {
                trunks.Add(p);
                string tree = east ? (i % 2 == 0 ? "Pine" : "Spruce") : (i % 2 == 0 ? "Willow" : "Birch");
                Model(root,tree,Trees+tree+".fbx",floor,Between(5.1f,8.9f),true);
            }
            else if (i % 3 == 1)
                Model(root,"Mossy shale",Models+"rock_0"+(i%5+1)+".fbx",floor,Between(.34f,1.15f));
            else
                Model(root,"Woodland ground cover",Models+"mushroom_0"+(i%4+1)+".fbx",floor,Between(.28f,.78f));
        }
    }

    private static void CopyProp(Transform root, Transform original, string name, Vector2 point, float yaw)
    {
        if (original == null) throw new InvalidOperationException("Missing Reedscale prop " + name);
        var item = Object.Instantiate(original.gameObject, root);
        item.name = name;
        item.transform.SetPositionAndRotation(Ground(point),Quaternion.Euler(0,yaw,0));
        foreach (var collider in item.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
    }

    private static void Model(Transform root, string name, string path, Vector3 point, float height, bool tree = false)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab,root);
        PrefabUtility.UnpackPrefabInstance(obj,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        obj.name = name;
        obj.transform.SetPositionAndRotation(point,Quaternion.Euler(0,Between(0,360),0));
        foreach (var part in obj.GetComponentsInChildren<Transform>(true)) part.gameObject.layer = 2;
        foreach (var collider in obj.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
        var bounds = BoundsOf(obj);
        obj.transform.localScale *= height / Mathf.Max(.01f,bounds.size.y);
        bounds = BoundsOf(obj);
        obj.transform.position += new Vector3(point.x-bounds.center.x,point.y-bounds.min.y,point.z-bounds.center.z);
        if (!tree) return;
        var trunk = Group(root,"Solid trunk",point);
        trunk.gameObject.layer = 9;
        var body = trunk.gameObject.AddComponent<CapsuleCollider>();
        body.height = 2.2f;
        body.radius = .44f;
        body.center = Vector3.up * 1.1f;
        obj.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(7);
    }

    private static Bounds BoundsOf(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<MeshRenderer>();
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }

    private static float DistanceFromTrail(Vector2 p, Vector2[] trail)
    {
        float distance = float.MaxValue;
        for (int i = 1; i < trail.Length; i++)
        {
            var start = trail[i-1];
            var delta = trail[i] - start;
            float t = Mathf.Clamp01(Vector2.Dot(p-start,delta) / delta.sqrMagnitude);
            distance = Mathf.Min(distance,Vector2.Distance(p,start+t*delta));
        }
        return distance;
    }

    private static void Populate(Transform root, Transform original, (string name,Vector2 point)[] monsters, Vector2 entry)
    {
        var camp = Group(root,"Additional patrols",Vector3.zero);
        foreach (var spawn in monsters)
        {
            var template = original.Find(spawn.name);
            if (template == null) throw new InvalidOperationException("Missing patrol template " + spawn.name);
            var home = Sample(spawn.point);
            RequirePath(entry,spawn.point,spawn.name);
            var actor = Object.Instantiate(template.gameObject,camp);
            actor.name = spawn.name;
            actor.transform.position = home;
            actor.GetComponent<AggroBehaviour>().ConfigureHomePosition(home);
        }
    }

    private static void CheckTrail(Vector2[] trail)
    {
        for (int i = 1; i < trail.Length; i++) RequirePath(trail[0],trail[i],"Trail stop " + trail[i]);
    }

    private static void RequirePath(Vector2 from, Vector2 to, string label)
    {
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(Sample(from),Sample(to),1,path) || path.status != NavMeshPathStatus.PathComplete)
            throw new InvalidOperationException("No complete walking route for " + label);
    }

    private static Vector3 Sample(Vector2 point)
    {
        var ground = Ground(point);
        if (!NavMesh.SamplePosition(ground,out var hit,1.4f,1))
            throw new InvalidOperationException("No navigable ground near " + point);
        return hit.position;
    }

    private static void RebuildNavigation()
    {
        var route = Object.FindObjectsByType<MonoBehaviour>().Single(component => component.GetType().Name == "OgreRoadwardenRoute");
        using var data = new SerializedObject(route);
        var wreckage = data.FindProperty("wreckage");
        var closed = Enumerable.Range(0,wreckage.arraySize)
            .Select(i => (GameObject)wreckage.GetArrayElementAtIndex(i).objectReferenceValue)
            .Where(piece => piece.activeSelf).ToArray();
        try
        {
            foreach (var piece in closed) piece.SetActive(false);
            CornbergSceneBuilder.Rebake();
        }
        finally
        {
            foreach (var piece in closed) piece.SetActive(true);
        }
    }

    private static Vector3 Ground(Vector2 p) => TryGround(p,out var result)
        ? result : throw new InvalidOperationException("Missing terrain at " + p);

    private static bool TryGround(Vector2 p, out Vector3 result)
    {
        result = new Vector3(p.x,float.MinValue,p.y);
        var ray = new Ray(new Vector3(p.x,65,p.y),Vector3.down);
        foreach (var terrain in Floors)
            if (terrain.Raycast(ray,out var hit,90) && hit.point.y > result.y) result = hit.point;
        return result.y != float.MinValue;
    }

    private static void AddGround(Transform root, string name, Mesh mesh, Material material)
    {
        var surface = Surface(root,name,mesh,material,8);
        var collider = surface.gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        Floors.Add(collider);
    }

    private static MeshRenderer Surface(Transform root, string name, Mesh mesh, Material material, int layer)
    {
        var group = Group(root,name,Vector3.zero);
        group.gameObject.layer = layer;
        group.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = group.gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        return renderer;
    }

    private static Mesh MeshAsset(string name, Vector3[] vertices, Vector2[] uv, List<int> triangles)
    {
        var mesh = new Mesh { name=name, vertices=vertices, uv=uv, triangles=triangles.ToArray() };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Art+"/"+name+".asset");
        return mesh;
    }

    private static void Triangle(Vector3[] v, List<int> t, int a, int b, int c)
    {
        if (Vector3.Cross(v[b]-v[a],v[c]-v[a]).y < 0) (b,c) = (c,b);
        t.Add(a);t.Add(b);t.Add(c);
    }

    private static Transform Find(string name) =>
        GameObject.Find(name)?.transform ?? throw new InvalidOperationException("Missing scene object " + name);

    private static Transform Group(Transform parent, string name, Vector3 point)
    {
        var transform = new GameObject(name).transform;
        transform.SetParent(parent,false);
        transform.position = point;
        return transform;
    }

    private static float Between(float min,float max) => Mathf.Lerp(min,max,(float)Random.NextDouble());
    private static float Ramp(float min,float max,float at) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(min,max,at));
}
