using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Apply once to the existing Cornberg world. All added geometry is presentation only.
public static class WorldArtPassAuthoring
{
    private const string World = "Assets/_DiceFree/Art/World/";
    private const string Output = World + "WoodlandAndSettlements";
    private const string Nature = World + "NaturePass03/";
    private const string BatchModels = World + "VisualBatch01/Models/";
    private const string RootName = "Woodland and settlement details";
    private static readonly HashSet<Object> Changes = new();
    private static readonly System.Random Random = new(62721);
    private static Mesh fern, grass, wildflowers, gardenFlowers;

    public static void Batch()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
        if (GameObject.Find(RootName) || AssetDatabase.IsValidFolder(Output))
            throw new InvalidOperationException("Restore the original scene and remove the previous output before applying this pass.");
        AssetDatabase.CreateFolder(World.TrimEnd('/'), "WoodlandAndSettlements");
        fern = ExtractPlant("Fern", "ForestFerns.asset", 100);
        grass = ExtractPlant("Grass tuft", "GrassClumps_1.asset", 20);
        wildflowers = ExtractPlant("Meadow flower", "WildflowerPetals_1.asset", 15, .28f);
        gardenFlowers = ExtractPlant("Garden flower", "WildflowerPetals_0.asset", 15, .32f);
        Physics.SyncTransforms();

        var root = Group(null, RootName, Vector3.zero);
        Woodland(root);
        Settlements(root);
        Capital(root);
        if (root.GetComponentsInChildren<Collider>(true).Length != 0)
            throw new InvalidOperationException("The decorative layer contains colliders.");

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("World art scene did not save.");
        AssetDatabase.SaveAssets();
        File.WriteAllLines("Logs/WorldArtPassSceneIds.txt", Changes.Select(o =>
            GlobalObjectId.GetGlobalObjectIdSlow(o).targetObjectId.ToString()));
        File.WriteAllText("Logs/WorldArtPass.ok", root.GetComponentsInChildren<Renderer>().Length + " decorative renderers.");
    }

    private static void Woodland(Transform root)
    {
        var woods = Group(root, "Lanternmere and Reedscale", Vector3.zero);
        var creek = Water("Reedscale moving creek", "1D5354", "739C77", "BBE2D5", new Vector4(.78f, .12f, 0, 0), Vector4.zero);
        var pond = Water("Lanternmere spring", "174B61", "6DAD8C", "BAE8CD", new Vector4(.13f, -.16f, 0, 0),
            new Vector4(-33, 320, 9.9f, 8.85f));
        var original = Find("Creek shallows").GetComponent<MeshFilter>().sharedMesh;
        var vertices = original.vertices;
        var uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uv[i] = new Vector2(vertices[i].x, i % 2);
            vertices[i].y += .035f;
        }
        Surface(woods, "Continuous creek surface", MakeMesh("Reedscale water ribbon", vertices, uv, original.triangles), creek, false);
        Hide(Find("Creek shallows").GetComponent<MeshRenderer>());
        Hide(Find("Creek channel").GetComponent<MeshRenderer>());

        var jade = Find("Jade shallows").GetComponent<MeshRenderer>();
        ReplaceMaterial(jade, pond);
        jade.shadowCastingMode = ShadowCastingMode.Off;
        Hide(Find("Dark reflective water").GetComponent<MeshRenderer>());

        var fernLeaves = Asset<Material>(Nature + "FernLeaves.mat");
        var grassLeaves = Asset<Material>(Nature + "GrassBlade_1.mat");
        for (int i = 0; i < 34; i++)
        {
            float x = Mathf.Lerp(-71, -15, i / 33f);
            if (Mathf.Abs(x + 36) < 3.7f) continue;
            for (int side = -1; side <= 1; side += 2)
            {
                float z = CreekZ(x) + side * Between(2.85f, 4.25f);
                var foot = Floor(x + Between(-.48f, .48f), z);
                Plant(woods, "Creek bank fern", fern, fernLeaves, foot, Between(.72f, 1.42f));
                if (i % 2 == 0)
                    Plant(woods, "Creek bank grass", grass, grassLeaves, foot + new Vector3(.4f, 0, .18f), 1.55f);
                if (i % 4 == 0)
                    Model(woods, "Wet shale", BatchModels + "rock_0" + (1 + i % 5) + ".fbx", foot, Between(.32f, .74f));
            }
        }

        for (int i = 0; i < 35; i++)
        {
            float angle = i * Mathf.PI * 2 / 35;
            float x = -33 + Mathf.Cos(angle) * Between(9.8f, 11.4f);
            float z = 320 + Mathf.Sin(angle) * Between(8.9f, 10.1f);
            var foot = Floor(x, z);
            if (foot.y < -.12f) continue;
            Plant(woods, "Lanternmere shore fern", fern, fernLeaves, foot, Between(.9f, 1.6f));
            if (i % 3 == 0)
                Model(woods, "Mossy spring stone", BatchModels + "rock_0" + (1 + i % 5) + ".fbx", foot, Between(.3f, .72f));
        }

        // Break up the artificial end of the river under a layered outcrop.
        foreach (float end in new[] { -75f, -14f })
        {
            float inward = end < -50 ? 1 : -1;
            float exposed = end;
            for (int step = 0; step < 36; step++)
            {
                exposed = end + inward * step * .25f;
                if (Floor(exposed, CreekZ(exposed)).y < 5.32f) break;
            }
            for (int i = 0; i < 5; i++)
            {
                float x = exposed + inward * (.22f + .57f * (i % 2));
                var foot = Floor(x, CreekZ(x) + (i - 2) * 1.02f);
                var rock = new Vector3(x, Mathf.Max(5.0f, foot.y - .30f), foot.z);
                Model(woods, "Creek rock overhang", BatchModels + "rock_0" + (1 + i) + ".fbx", rock, Between(1.25f, 2.05f));
            }
        }
        for (int i = 0; i < 11; i++)
        {
            float x = Mathf.Lerp(-57, -29, i / 10f);
            var foot = Floor(x, 416 + Mathf.Sin(i * .9f) * 1.45f);
            Plant(woods, "Ridge fern", fern, fernLeaves, foot, 1.7f);
            if (i % 2 == 0) Model(woods, "Ridge pine", World + "CornbergBatch/Trees/Tree_Pine.fbx", foot, Between(5.2f, 8.5f));
        }
        foreach (var item in new[]
        {
            ("crate_02", -32.9f, 391.1f, 1.15f), ("barrel_03", -29.6f, 391.7f, 1.12f),
            ("log_02", -56.8f, 400.7f, .78f), ("crate_01", -51.8f, 408.2f, .86f),
            ("barrel_01", -48.7f, 410.9f, .92f), ("crate_03", -54.2f, 398.4f, .87f)
        })
            Model(woods, "Reedscale field supplies", BatchModels + item.Item1 + ".fbx", Floor(item.Item2, item.Item3), item.Item4);

        RidgeBackdrop(woods);
    }

    private static void RidgeBackdrop(Transform root)
    {
        var backdrop = Group(root, "Wooded ridge beyond the lookout", Vector3.zero);
        var terrain = Find("Rising forest banks and lookout ridge");
        var source = terrain.GetComponent<MeshFilter>().sharedMesh;
        var sourceVertices = source.vertices;
        const int columns = 65;
        const int rows = 89;
        const int firstRow = 9;
        var rim = new List<Vector3>();
        for (int row = firstRow; row < rows; row++) rim.Add(sourceVertices[row * columns]);
        for (int column = 1; column < columns; column++) rim.Add(sourceVertices[(rows - 1) * columns + column]);
        for (int row = rows - 2; row >= firstRow; row--) rim.Add(sourceVertices[row * columns + columns - 1]);

        const int bands = 4;
        var verts = new Vector3[rim.Count * bands];
        var uv = new Vector2[verts.Length];
        var tris = new List<int>();
        var center = new Vector3(-45, 0, 391);
        for (int i = 0; i < rim.Count; i++)
        {
            var point = rim[i];
            var outward = point - center;
            outward.y = 0;
            outward.Normalize();
            for (int band = 0; band < bands; band++)
            {
                float width = band * (7.4f + 1.7f * Mathf.Sin(i * .20f));
                var p = point + outward * width;
                p.y += band * (1.18f + .64f * Mathf.Sin(i * .12f + band * .9f)) - .025f;
                verts[i * bands + band] = p;
                uv[i * bands + band] = new Vector2(p.x, p.z) * .105f;
                if (i == 0 || band == 0) continue;
                int a = (i - 1) * bands + band - 1;
                AddTriangle(tris, verts, a, a + 1, a + bands);
                AddTriangle(tris, verts, a + 1, a + bands + 1, a + bands);
            }
            if (i % 6 == 1)
            {
                int band = i % 4 < 2 ? 1 : 2;
                var p = verts[i * bands + band];
                string tree = i % 5 < 2 ? "Spruce" : "Pine";
                Model(backdrop, "Distant " + tree, World + "CornbergBatch/Trees/Tree_" + tree + ".fbx", p, Between(6.4f, 11f));
            }
        }

        var ground = terrain.GetComponent<MeshRenderer>().sharedMaterial;
        Surface(backdrop, "Continuous ridge scenery", MakeMesh("Lookout wooded rim", verts, uv, tris.ToArray()), ground, true);
    }

    private static void Settlements(Transform root)
    {
        var cornberg = Group(root, "Cornberg village garden and supplies", Vector3.zero);
        var trimery = Group(root, "Trimery timber yards and doorstep flowers", Vector3.zero);
        var briarback = Group(root, "Briarback pathside heath", Vector3.zero);
        var green = Asset<Material>(Nature + "FernLeaves.mat");
        var blooms = Asset<Material>(Nature + "Wildflower_1.mat");

        foreach (var p in new[] { new Vector2(2, 1.65f), new Vector2(2.75f, 1.34f), new Vector2(3.15f, 1.9f),
            new Vector2(2.45f, 4.85f), new Vector2(1.7f, 4.45f), new Vector2(3.2f, 4.7f),
            new Vector2(4.1f, 9.2f), new Vector2(4.6f, 9.6f), new Vector2(5.1f, 10.1f) })
            Plant(cornberg, "Well lane wildflowers", wildflowers, blooms, Floor(p.x, p.y), Between(1.25f, 1.8f));

        Model(cornberg, "Village produce barrow", BatchModels + "cart_01.fbx", Floor(13.8f, 15.7f), 1.23f, 24);
        Model(cornberg, "Village crate", BatchModels + "crate_01.fbx", Floor(14.7f, 15.1f), .61f, 9);
        Model(cornberg, "Village barrel", BatchModels + "barrel_02.fbx", Floor(14.8f, 16.0f), .7f, 14);

        foreach (var spot in new[] { new Vector2(240, 187), new Vector2(255, 189), new Vector2(241, 177), new Vector2(256, 176) })
        {
            var foot = Floor(spot.x, spot.y);
            Plant(trimery, "Doorstep flowers", wildflowers, blooms, foot, 1.5f);
            Plant(trimery, "Doorstep fern", fern, green, foot + new Vector3(.45f, 0, -.35f), .8f);
        }
        Model(trimery, "Lumber delivery cart", BatchModels + "cart_02.fbx", Floor(243, 199.2f), 1.37f, -17);
        Model(trimery, "Trimery sawn logs", BatchModels + "log_02.fbx", Floor(241.9f, 198.5f), .67f, 76);
        Model(trimery, "Trimery sawn logs", BatchModels + "log_01.fbx", Floor(242.2f, 197.7f), .63f, 74);
        Model(trimery, "Trimery storage barrel", BatchModels + "barrel_01.fbx", Floor(243.6f, 200.2f), .85f);

        var straw = Asset<Material>(Nature + "GrassBlade_0.mat");
        for (int i = 0; i < 18; i++)
        {
            var point = Floor(257 + Between(0, 22), 300 + Between(0, 20));
            Plant(briarback, "Frontier bunchgrass", grass, straw, point, Between(.8f, 1.5f));
            if (i % 4 == 0) Model(briarback, "Pathside shale", BatchModels + "rock_03.fbx", point, Between(.32f, .60f));
        }
    }

    private static void Capital(Transform root)
    {
        var city = Find("Arborium - World Tree capital");
        var gardens = Group(root, "Arborium World Tree garden", city.position);
        var leaves = Asset<Material>(Nature + "FernLeaves.mat");
        var blooms = Asset<Material>(Nature + "Wildflower_0.mat");
        var limestone = Asset<Material>(World + "ArboriumArrival/ivory carved stone.mat");
        var earth = Lit("Garden bed earth", "42583A");

        // Low beds sit between the four main approach paths and stay below the camera cutaway.
        for (int bed = 0; bed < 8; bed++)
        {
            float angle = (bed * 45 + 22.5f) * Mathf.Deg2Rad;
            var direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
            var center = city.position + direction * 22.4f;
            center.y = Floor(center.x, center.z).y + .05f;
            var strip = Group(gardens, "Planted limestone bed", center, bed * 45 + 22.5f);
            var curve = new[] { new Vector2(-2.5f, -.65f), new Vector2(-1.5f, -.32f), new Vector2(0, -.2f),
                new Vector2(1.5f, -.32f), new Vector2(2.5f, -.65f) };
            var verts = new Vector3[curve.Length * 2];
            var uv = new Vector2[verts.Length];
            var tris = new List<int>();
            for (int i = 0; i < curve.Length; i++)
            {
                var p = new Vector3(curve[i].x, .08f, curve[i].y);
                verts[2 * i] = p - Vector3.forward * .72f;
                verts[2 * i + 1] = p + Vector3.forward * .72f;
                if (i == 0) continue;
                AddTriangle(tris, verts, 2 * i - 2, 2 * i - 1, 2 * i);
                AddTriangle(tris, verts, 2 * i - 1, 2 * i + 1, 2 * i);
                foreach (float side in new[] { -1f, 1f })
                {
                    var a = strip.TransformPoint(verts[2 * i - (side < 0 ? 2 : 1)]);
                    var b = strip.TransformPoint(verts[2 * i + (side < 0 ? 0 : 1)]);
                    Edging(strip, limestone, a, b);
                }
            }
            Edging(strip, limestone, strip.TransformPoint(verts[0]), strip.TransformPoint(verts[1]));
            Edging(strip, limestone, strip.TransformPoint(verts[^2]), strip.TransformPoint(verts[^1]));
            Surface(strip, "Garden bed soil", MakeMesh("Garden bed " + (bed + 1), verts, uv, tris.ToArray()), earth, false);
            for (int i = 0; i < 9; i++)
            {
                float x = -2.15f + i * .54f;
                var p = strip.TransformPoint(new Vector3(x, .12f, Between(-.62f, .12f)));
                Plant(strip, i % 3 == 0 ? "Garden fern" : "Garden flowers", i % 3 == 0 ? fern : gardenFlowers,
                    i % 3 == 0 ? leaves : blooms, p, Between(1.1f, 1.85f));
                if (i % 3 != 0) Plant(strip, "Garden flower foliage", grass, leaves, p, 1.3f);
            }
        }

        RestoreCityTopiary(gardens, city, leaves, blooms);
        DressCapitalSurfaces(city);

        var brass = Asset<Material>(World + "ArboriumArrival/old gilded bronze.mat");
        // These physical emblems stay behind buildings and the World Tree.
        foreach (var text in city.GetComponentsInChildren<TextMesh>(true))
        {
            string name = text.gameObject.name;
            if (name == "Sign - ARBORIUM" || name == "Sign - Northern Gate" ||
                name == "Sign - Adventurers' Guild" || name == "Sign - Northern Exchange")
            {
                Hide(text.GetComponent<MeshRenderer>());
                continue;
            }
            if (text.transform.parent == null || !text.transform.parent.name.StartsWith("Wayfinding - ", StringComparison.Ordinal))
                continue;

            Hide(text.GetComponent<MeshRenderer>());
            var emblem = Group(gardens, text.text + " wayfinding emblem", text.transform.position);
            emblem.rotation = text.transform.rotation;
            if (text.text == "WORLD TREE")
            {
                SignRelief(emblem, brass, new Vector2(0,-.10f), new Vector2(.10f,.49f), 0);
                SignRelief(emblem, brass, new Vector2(-.13f,.14f), new Vector2(.13f,.38f), 45);
                SignRelief(emblem, brass, new Vector2(.13f,.14f), new Vector2(.13f,.38f), -45);
            }
            else if (text.text == "GUILD")
            {
                SignRelief(emblem, brass, Vector2.zero, new Vector2(.12f,.76f), 38);
                SignRelief(emblem, brass, Vector2.zero, new Vector2(.12f,.76f), -38);
                SignRelief(emblem, brass, new Vector2(0,.1f), new Vector2(.25f,.25f), 45);
            }
            else
            {
                SignRelief(emblem, brass, new Vector2(-.34f,-.12f), new Vector2(.28f,.28f), 45);
                SignRelief(emblem, brass, new Vector2(0,.11f), new Vector2(.28f,.28f), 45);
                SignRelief(emblem, brass, new Vector2(.34f,-.12f), new Vector2(.28f,.28f), 45);
            }
        }
    }

    private static void SignRelief(Transform root, Material material, Vector2 position, Vector2 size, float angle)
    {
        var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(piece.GetComponent<Collider>());
        piece.name = "Cast brass";
        piece.layer = 2;
        piece.transform.SetParent(root, false);
        piece.transform.localPosition = new Vector3(position.x, position.y, -.025f);
        piece.transform.localRotation = Quaternion.Euler(0, 0, angle);
        piece.transform.localScale = new Vector3(size.x, size.y, .065f);
        piece.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    private static void DressCapitalSurfaces(Transform city)
    {
        var sourceGround = Asset<Material>(World + "ArboriumArrival/capital meadow.mat");
        var ground = new Material(sourceGround) { name = "Arborium garden lawn" };
        ground.SetTexture("_BaseMap", Asset<Texture2D>(World + "World1Handcrafted/Packed road grain.png"));
        ground.SetColor("_BaseColor", sourceGround.GetColor("_BaseColor") * 1.12f);
        ground.SetTextureScale("_BaseMap", new Vector2(1.25f, 1.25f));
        AssetDatabase.CreateAsset(ground, Output + "/Arborium garden lawn.mat");

        var sourceGlass = Asset<Material>(World + "ArboriumArrival/practical enchanted glass.mat");
        var glass = new Material(sourceGlass) { name = "Arborium evening glass" };
        glass.EnableKeyword("_EMISSION");
        // URP uses the emissive GI flag to keep the forward emission keyword on import.
        glass.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
        glass.SetColor("_EmissionColor", new Color(.001f,.001f,.001f));
        AssetDatabase.CreateAsset(glass, Output + "/Arborium evening glass.mat");

        var buildings = Find("Arborium - gates and inhabited floors");
        var surfaces = city.GetComponentsInChildren<MeshRenderer>(true)
            .Concat(buildings.GetComponentsInChildren<MeshRenderer>(true));
        foreach (var renderer in surfaces)
        {
            var materials = renderer.sharedMaterials;
            bool replaced = false;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == sourceGround) materials[i] = ground;
                else if (materials[i] == sourceGlass) materials[i] = glass;
                else continue;
                replaced = true;
            }
            if (!replaced) continue;
            Changes.Add(renderer);
            renderer.sharedMaterials = materials;
        }
    }

    private static void RestoreCityTopiary(Transform gardens, Transform city, Material leaves, Material blooms)
    {
        var filter = city.Find("12 - Sacred civic gardens and World Tree roots/sacred garden foliage").GetComponent<MeshFilter>();
        var source = filter.sharedMesh;
        var vertices = source.vertices;
        var triangles = source.triangles;
        var keep = new List<int>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            var v = vertices[triangles[i]];
            if (new Vector2(v.x, v.z).sqrMagnitude > 28 * 28) continue;
            keep.Add(triangles[i]);
            keep.Add(triangles[i + 1]);
            keep.Add(triangles[i + 2]);
        }
        var mesh = MakeMesh("Arborium garden planter greenery", vertices, source.uv, keep.ToArray());
        mesh.normals = source.normals;
        filter.sharedMesh = mesh;
        EditorUtility.SetDirty(mesh);
        Changes.Add(filter);

        for (int i = 0; i < 6; i++)
        {
            float angle = (i * 60 + 30) * Mathf.Deg2Rad;
            var point = city.position + new Vector3(Mathf.Sin(angle) * 36, .15f, Mathf.Cos(angle) * 36);
            Model(gardens, "City garden cypress", World + "CornbergBatch/Trees/Tree_Cypress.fbx", point, Between(3.6f, 4.5f));
            for (int j = 0; j < 8; j++)
            {
                float turn = j * Mathf.PI * .25f;
                var p = point + new Vector3(Mathf.Sin(turn) * 1.92f, .015f, Mathf.Cos(turn) * 1.92f);
                Plant(gardens, "Cypress planter foliage", grass, leaves, p, 1.35f);
                if (j % 2 == 0) Plant(gardens, "Cypress planter flowers", gardenFlowers, blooms, p, 1.25f);
            }
        }
    }

    private static float CreekZ(float x) => 370.2f + Mathf.Sin((x + 36) * .16f) * 1.3f;
    private static float Between(float a, float b) => Mathf.Lerp(a, b, (float)Random.NextDouble());

    private static Transform Find(string name) =>
        GameObject.Find(name)?.transform ?? throw new InvalidOperationException("Missing scene object " + name);

    private static T Asset<T>(string path) where T : Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new FileNotFoundException(path);

    private static Vector3 Floor(float x, float z)
    {
        var start = new Vector3(x, 48, z);
        if (!Physics.Raycast(start, Vector3.down, out var hit, 70, 1 << 8))
            throw new InvalidOperationException("No world surface at " + new Vector2(x, z));
        return hit.point;
    }

    private static Transform Group(Transform parent, string name, Vector3 point, float yaw = 0)
    {
        var go = new GameObject(name);
        go.layer = 2;
        go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(point, Quaternion.Euler(0, yaw, 0));
        return go.transform;
    }

    private static void Plant(Transform root, string name, Mesh mesh, Material mat, Vector3 point, float size)
    {
        var plant = Group(root, name, point, Between(0, 360));
        plant.localScale = Vector3.one * size;
        Surface(plant, name, mesh, mat, false);
    }

    private static void Edging(Transform root, Material mat, Vector3 a, Vector3 b)
    {
        var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(piece.GetComponent<Collider>());
        piece.name = "Dressed limestone edging";
        piece.layer = 2;
        piece.transform.SetParent(root, false);
        piece.transform.position = (a + b) * .5f + Vector3.up * .034f;
        piece.transform.rotation = Quaternion.LookRotation(b - a);
        piece.transform.localScale = new Vector3(.15f, .25f, Vector3.Distance(a, b) + .12f);
        piece.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static void Model(Transform root, string name, string path, Vector3 point, float height, float yaw = float.NaN)
    {
        var model = (GameObject)PrefabUtility.InstantiatePrefab(Asset<GameObject>(path), root);
        PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        model.name = name;
        model.transform.SetPositionAndRotation(point, Quaternion.Euler(0, float.IsNaN(yaw) ? Between(0, 360) : yaw, 0));
        foreach (var part in model.GetComponentsInChildren<Transform>(true)) part.gameObject.layer = 2;
        foreach (var collider in model.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
        Bounds bounds = BoundsOf(model);
        if (bounds.size.y < .001f) throw new InvalidOperationException("Model has no height " + path);
        model.transform.localScale *= height / bounds.size.y;
        bounds = BoundsOf(model);
        model.transform.position += new Vector3(point.x - bounds.center.x, point.y - bounds.min.y, point.z - bounds.center.z);
    }

    private static Bounds BoundsOf(GameObject model)
    {
        var renderers = model.GetComponentsInChildren<MeshRenderer>();
        var result = renderers.First().bounds;
        foreach (var renderer in renderers.Skip(1)) result.Encapsulate(renderer.bounds);
        return result;
    }

    private static void Surface(Transform root, string name, Mesh mesh, Material mat, bool shadow)
    {
        var go = new GameObject(name);
        go.layer = 2;
        go.transform.SetParent(root, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = shadow ? ShadowCastingMode.On : ShadowCastingMode.Off;
    }

    private static Mesh MakeMesh(string name, Vector3[] verts, Vector2[] uv, int[] indices)
    {
        var mesh = new Mesh { name = name, vertices = verts, uv = uv, triangles = indices };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, Output + "/" + name + ".asset");
        return mesh;
    }

    // CornbergNaturePass emits these pieces consecutively into its region-wide meshes.
    // One fern uses 5 fronds of 5 quads, each blossom has 5 triangles, and grass has at least 4 blades.
    private static Mesh ExtractPlant(string name, string asset, int count, float lift = 0)
    {
        var source = Asset<Mesh>(Nature + asset);
        var vertices = source.vertices.Take(count).ToArray();
        var bounds = new Bounds(vertices[0], Vector3.zero);
        foreach (var vertex in vertices) bounds.Encapsulate(vertex);
        var origin = new Vector3(bounds.center.x, bounds.min.y - lift, bounds.center.z);
        for (int i = 0; i < vertices.Length; i++) vertices[i] -= origin;

        var triangles = source.triangles;
        var local = new List<int>();
        for (int i = 0; i < triangles.Length; i += 3)
            if (triangles[i] < count && triangles[i + 1] < count && triangles[i + 2] < count)
                local.AddRange(new[] { triangles[i], triangles[i + 1], triangles[i + 2] });
        var mesh = MakeMesh(name, vertices, source.uv.Take(count).ToArray(), local.ToArray());
        mesh.normals = source.normals.Take(count).ToArray();
        EditorUtility.SetDirty(mesh);
        return mesh;
    }

    private static void AddTriangle(List<int> indices, Vector3[] verts, int a, int b, int c)
    {
        if (Vector3.Cross(verts[b] - verts[a], verts[c] - verts[a]).y < 0) (b, c) = (c, b);
        indices.Add(a);
        indices.Add(b);
        indices.Add(c);
    }

    private static void Hide(MeshRenderer renderer)
    {
        Changes.Add(renderer);
        renderer.enabled = false;
    }

    private static void ReplaceMaterial(MeshRenderer renderer, Material material)
    {
        Changes.Add(renderer);
        renderer.sharedMaterial = material;
    }

    private static Material Water(string name, string deep, string shallow, string glint, Vector4 flow, Vector4 pond)
    {
        var material = new Material(Asset<Shader>("Assets/_DiceFree/Shaders/WoodlandWater.shader")) { name = name };
        material.SetColor("_DeepColor", ColorFrom(deep));
        material.SetColor("_ShallowColor", ColorFrom(shallow));
        material.SetColor("_GlintColor", ColorFrom(glint));
        material.SetVector("_Flow", flow);
        material.SetVector("_Pond", pond);
        AssetDatabase.CreateAsset(material, Output + "/" + name + ".mat");
        return material;
    }

    private static Material Lit(string name, string color)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
        material.SetColor("_BaseColor", ColorFrom(color));
        material.SetFloat("_Smoothness", .1f);
        AssetDatabase.CreateAsset(material, Output + "/" + name + ".mat");
        return material;
    }

    private static Color ColorFrom(string hex)
    {
        if (!ColorUtility.TryParseHtmlString("#" + hex, out var color)) throw new ArgumentException(hex);
        return color;
    }
}
