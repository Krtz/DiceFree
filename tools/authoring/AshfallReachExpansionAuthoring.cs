using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.UI;
using DiceFree.Verdant;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static partial class AshfallReachExpansionAuthoring
{
    private const string ScenePath = "Assets/_DiceFree/Scenes/WorldTwoArrival.unity";
    private const string Art = "Assets/_DiceFree/Art/World/";
    private const string Output = Art + "AshfallReach/Expansion/";
    private const string Handcrafted = Art + "World1Handcrafted/";
    private const string Dragon = "Assets/_DiceFree/Settings/AncientRoadDragon/";
    private const string Reedscale = Art + "ReedscaleLookout/";
    private const int WalkableLayer = 8;
    private const int SolidLayer = 9;
    private const int ActorLayer = 10;
    private static readonly Vector2 Well = new(1.6f, -47.5f);
    private static readonly Vector2[] SpearHomes =
    {
        new(-20,-9), new(-24,4), new(-10,-30), new(23,-43), new(16,-66), new(31,-77)
    };
    private static readonly Vector2[] BowHomes =
    {
        new(-28,8), new(-15,-34), new(29,-43), new(24,-77)
    };
    private static readonly string[] ClutterNames = { "Ashfall overlook", "High ridges beyond Ashfall Reach" };

    private static Mesh stoneMesh;
    private static Material basalt, slate, ash, trail, bronze, oak, cedar, roof, light, water, seam, warmRock, heath;
    private static System.Random random;

    public static void Batch()
    {
        LoadMaterials();
        LoadRiverTraderStock();
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var root = scene.GetRootGameObjects().Single(go => go.GetComponent<WorldNavigation>() != null).transform;
        RemoveOldClutter(root);
        if (root.Find("Cinderwatch and the two pips") is Transform previous)
            Object.DestroyImmediate(previous.gameObject);
        var expansion = Group(root, "Cinderwatch and the two pips", Vector3.zero);
        var ground = root.Find("Slate ridges and alpine heath");
        ground.GetComponent<MeshFilter>().sharedMesh = GroundMesh(300, -400, 90, 2, "Expanded valley surface", 150);
        ground.GetComponent<MeshRenderer>().sharedMaterials = new[] { BlendedGround() };
        ground.GetComponent<MeshCollider>().sharedMesh = GroundMesh(230, -155, 35, 1, "Expanded valley collider", 60);

        Road(expansion, "Road to Cinderwatch", AshfallReach.SettlementRoad, 2.57f);
        Road(expansion, "Footpath to the Basalt Needle", AshfallReach.NeedlePath, 1.28f);
        Road(expansion, "Footpath to Ember Crown", AshfallReach.CrownPath, 1.35f);
        BuildSettlement(expansion);
        FortifyCinderwatch(expansion);
        BuildLandmarks(expansion);
        DressValley(expansion);
        BuildRiverlands(expansion);
        BakeNavigation(root, expansion);
        AssetDatabase.SaveAssets();
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save Ashfall Reach.");
        Debug.Log("DICEFREE_ASHFALL_AUTHORING_OK");
    }

    private static void LoadMaterials()
    {
        if (!AssetDatabase.IsValidFolder(Output.TrimEnd('/')))
            AssetDatabase.CreateFolder((Art + "AshfallReach").TrimEnd('/'), "Expansion");
        random = new System.Random(65945);
        stoneMesh = Load<Mesh>("Assets/_DiceFree/Settings/Briarback/FourfoldHollow/Original faceted basalt.asset");
        basalt = Load<Material>(Dragon + "Charcoal mountain rock.mat");
        slate = Load<Material>(Art + "AshfallReach/Ashfall slate.mat");
        heath = Load<Material>(Art + "AshfallReach/Ashfall heath.mat");
        trail = Load<Material>(Dragon + "Dust on the mountain path.mat");
        ash = Tint("Rust and ash", slate, new Color(.36f, .245f, .177f));
        warmRock = Tint("Oxidized tuff", basalt, new Color(.31f, .177f, .119f));
        seam = Tint("Copper mineral seam", slate, new Color(.77f, .31f, .105f));
        bronze = Load<Material>(Dragon + "Stained bronze inlay.mat");
        oak = Load<Material>(Handcrafted + "DarkOak.mat");
        cedar = Load<Material>(Handcrafted + "Cedar.mat");
        roof = Load<Material>(Handcrafted + "SlateDark.mat");
        light = Load<Material>(Handcrafted + "Lantern.mat");
        water = Load<Material>(Handcrafted + "Cornberg spring water.mat");
    }

    private static void RemoveOldClutter(Transform root)
    {
        var overlook = root.Find(ClutterNames[0]);
        foreach (Transform stone in overlook.Cast<Transform>().Where(t => t.name == "Fractured ridge stone").ToArray())
            Object.DestroyImmediate(stone.gameObject);
        var backdrop = root.Find(ClutterNames[1]);
        foreach (Transform t in backdrop.Cast<Transform>().ToArray())
        {
            var p = t.localPosition;
            if (p.z < -29 && p.z > -81 && p.x > -41 && p.x < 45 ||
                p.x < -18 && p.x > -78 && p.z > -62 && p.z < -25)
                Object.DestroyImmediate(t.gameObject);
        }

        // Carve an opening through the existing western ridge where the footpath starts climbing.
        foreach (Transform t in root.Cast<Transform>().ToArray())
        {
            var p = t.localPosition;
            bool ridgePiece = t.name == "Fractured ridge stone" || t.name == "Slate ridge" || PrefabUtility.IsPartOfPrefabInstance(t);
            if (ridgePiece && p.x <= -13 && p.x >= -33 && p.z >= -15 && p.z <= 15)
                Object.DestroyImmediate(t.gameObject);
        }
    }

    private static float Height(float x, float z)
    {
        float edge = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(11, 30, Mathf.Abs(x + Mathf.Sin(z * .085f) * 2.1f)));
        float variation = (Mathf.PerlinNoise(51 + x * .115f, 70 + z * .115f) - .5f) * .65f;
        float oldHeight = variation + edge * (5.4f + 1.55f * Mathf.Sin(z * .22f));
        float south = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-24, -40, z));
        float rim = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(23, 47, Mathf.Abs(x - 2)));
        float ashfall = Mathf.Lerp(oldHeight, variation * .33f + .70f + rim * 5.3f + Mathf.Sin(z * .13f) * .27f, south);
        float riverlands = 1.08f + variation * .40f + Mathf.Sin(z * .027f) * .15f;
        float ground = Mathf.Lerp(ashfall, riverlands, AshfallReach.RiverlandsWeight(x));
        float bank = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(AshfallReach.RiverHalfWidth(z) - .2f,
            AshfallReach.RiverHalfWidth(z) + 5, Mathf.Abs(x - AshfallReach.RiverX(z))));
        return Mathf.Lerp(AshfallReach.RiverWaterHeight - 1.8f, ground, bank);
    }

    private static Vector3 Ground(Vector2 p) => new(p.x, Height(p.x, p.y), p.y);
    private static Vector3 Ground(float x, float z) => Ground(new Vector2(x, z));

    private static Mesh GroundMesh(int west, int south, int north, int stride, string name, int? east = null)
    {
        int columns = ((east ?? west) + west) / stride + 1;
        int rows = (north - south) / stride + 1;
        var vertices = new Vector3[columns * rows];
        var uv = new Vector2[vertices.Length];
        var triangles = new List<int>();
        for (int row = 0; row < rows; row++)
        for (int col = 0; col < columns; col++)
        {
            int a = row * columns + col;
            float x = col * stride - west, z = row * stride + south;
            vertices[a] = Ground(x, z);
            uv[a] = new Vector2((x + 300) / 450f, (z + 400) / 490f);
            if (row == rows - 1 || col == columns - 1) continue;
            triangles.AddRange(new[] { a, a + columns, a + 1, a + 1, a + columns, a + columns + 1 });
        }
        var mesh = new Mesh { name = name, vertices = vertices, uv = uv };
        mesh.SetTriangles(triangles, 0);
        return SaveMesh(mesh);
    }

    private static Material BlendedGround()
    {
        var source = new Texture2D(2, 2);
        if (!ImageConversion.LoadImage(source, File.ReadAllBytes(
            "Assets/_DiceFree/Settings/Briarback/FourfoldHollow/Slate grain.png")))
            throw new IOException("Could not read the existing slate grain for Ashfall ground.");
        const int size = 2048;
        var pixels = new Color[size * size];
        try
        {
            for (int row = 0; row < size; row++)
            for (int col = 0; col < size; col++)
            {
                float x = (col + .5f) / size * 450 - 300;
                float z = (row + .5f) / size * 490 - 400;
                float variation = Mathf.PerlinNoise(x * .14f + 48, z * .14f + 69);
                float edge = -34 + 3 * Mathf.Sin(x * .20f) + (variation - .5f) * 6;
                float transition = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(edge + 5, edge - 5, z));
                Color soil = Color.Lerp(heath.color, ash.color, transition);
                soil = Color.Lerp(soil, new Color(.245f, .365f, .205f), AshfallReach.RiverlandsWeight(x));
                float shore = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(3.5f, 8.5f, Mathf.Abs(x - AshfallReach.RiverX(z))));
                soil = Color.Lerp(soil, new Color(.38f, .345f, .255f), shore);
                float rock = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1.7f, 3.0f, Height(x, z)));
                Color tone = Color.Lerp(soil, slate.color, rock);
                var grain = source.GetPixelBilinear(Mathf.Repeat(x * .2f, 1), Mathf.Repeat(z * .2f, 1));
                float flecks = .91f + variation * .17f;
                pixels[row * size + col] = new Color(tone.r * grain.r * flecks, tone.g * grain.g * flecks,
                    tone.b * grain.b * flecks, 1);
            }
        }
        finally { Object.DestroyImmediate(source); }
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, true)
        {
            name = "Heath fading into volcanic soil",
            wrapMode = TextureWrapMode.Clamp,
            anisoLevel = 4
        };
        texture.SetPixels(pixels);
        texture.Apply();
        var material = new Material(heath) { name = "Ashfall blended ground", color = Color.white };
        material.SetTexture("_BaseMap", Save(texture, Output + texture.name + ".asset"));
        material.SetTextureScale("_BaseMap", Vector2.one);
        material.SetTextureOffset("_BaseMap", Vector2.zero);
        return Save(material, Output + material.name + ".mat");
    }

    private static void Road(Transform parent, string name, IReadOnlyList<Vector2> points, float width)
    {
        var samples = new List<Vector2> { points[0] };
        for (int i = 1; i < points.Count; i++)
        {
            int subdivisions = Mathf.CeilToInt(Vector2.Distance(points[i - 1], points[i]) / .5f);
            for (int j = 1; j <= subdivisions; j++)
                samples.Add(Vector2.Lerp(points[i - 1], points[i], (float)j / subdivisions));
        }
        var vertices = new List<Vector3>();
        var uv = new List<Vector2>();
        var triangles = new List<int>();
        for (int i = 0; i < samples.Count; i++)
        {
            var direction = (samples[Mathf.Min(i + 1, samples.Count - 1)] - samples[Mathf.Max(0, i - 1)]).normalized;
            var normal = new Vector2(-direction.y, direction.x) * (width + .12f * Mathf.Sin(i * .6f));
            for (int j = -2; j <= 2; j++)
            {
                vertices.Add(Ground(samples[i] + normal * (j * .5f)) + Vector3.up * .10f);
                uv.Add(new Vector2(j * .25f, i * .12f));
            }
            if (i == 0) continue;
            for (int j = 0; j < 4; j++)
            {
                int a = (i - 1) * 5 + j;
                triangles.AddRange(new[] { a, a + 1, a + 5, a + 1, a + 6, a + 5 });
            }
        }
        var mesh = new Mesh { name = name };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uv);
        mesh.SetTriangles(triangles, 0);
        Surface(parent, name, SaveMesh(mesh), trail);
    }

    private static void BuildSettlement(Transform parent)
    {
        var village = Group(parent, "Cinderwatch", Vector3.zero);
        Cabin(village, "Wayhouse", 11.6f, -44.7f, 7.6f, 5.5f, -90);
        Cabin(village, "Ridge kitchen", -4.25f, -46.6f, 6.6f, 5.0f, 75);
        Cabin(village, "Supply store", 10.8f, -51, 5.8f, 4.45f, -103);
        Cabin(village, "Watch house", -3, -35.8f, 6, 4.5f, 180);
        Cabin(village, "Hideworker cabin", -4, -56, 6.2f, 4.6f, 70);
        Waypost(village, "CINDERWATCH", Ground(2, -37.0f), 166, 3.3f);
        Lantern(village, Ground(8.1f, -39.5f));
        Lantern(village, Ground(5.1f, -53.5f));
        Lantern(village, Ground(-1.8f, -42.0f));
        Resident(village, "Cinderwatch Keeper", "keeper", "Villager", new(8, -42.3f), new(6.8f, -41.8f), 255,
            "Welcome to Cinderwatch. The spring is safe to drink from. The valley road leads back to Ironpine; the side trails run toward the Basalt Needle and Ember Crown. Watch for Ashscale patrols on the ridges.");
        Resident(village, "Ridge Cook", "cook", "Herbalist", new(-.1f, -43.6f), new(1.1f, -43.4f), 100,
            "Leave the ash on the doorstep. We keep a warm kitchen and a clean spring here. The spearhands travel in loose patrols, and their ridgebows have a long reach.");

        // A recessed spring and a dressed stone rim mark the safe center of the outpost.
        var well = Group(village, "Cinderwatch spring", Ground(Well));
        Part(well, "Spring water", new(0, .59f, 0), new(1.82f, .07f, 1.82f), water, PrimitiveType.Cylinder);
        for (int i = 0; i < 12; i++)
        {
            float angle = i * Mathf.PI / 6;
            var position = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * 1.05f;
            Part(well, "Basalt well ring", position + Vector3.up * .39f, new(.57f, .86f, .42f), i % 3 == 0 ? slate : basalt,
                rotation: Quaternion.Euler(0, i * 30, 0));
            Part(well, "Rounded cap", position + Vector3.up * .85f, new(.57f, .13f, .42f), slate,
                rotation: Quaternion.Euler(0, i * 30, 0));
        }
        Solid(well, "Well collision", Vector3.up * .48f, new(1.74f, 1.0f, 1.74f));
        var healer = well.gameObject.AddComponent<HealerInteraction>();
        healer.ConfigureName("Drink from the Cinderwatch spring");
        healer.ConfigureApproach(Group(village, "Spring approach", Ground(3.33f, -47.5f)));
        // Interaction targets use an outer selectable collider independently from pathing blockers.
        var selectable = well.gameObject.AddComponent<BoxCollider>();
        well.gameObject.layer = 11;
        selectable.center = Vector3.up * .62f;
        selectable.size = new Vector3(2.35f, 1.18f, 2.35f);

        for (int i = 0; i < 17; i++)
        {
            float angle = i * Mathf.PI * 2 / 17;
            var p = Ground(Well + new Vector2(Mathf.Sin(angle) * 2.28f, Mathf.Cos(angle) * 2.25f));
            var stone = Rock(village, p + Vector3.up * .01f, new(.82f, .28f, .68f), i % 3 == 0 ? bronze : slate);
            stone.localRotation = Quaternion.Euler(0, i * 137.5f, 0);
        }
    }

    private static GameObject Resident(Transform parent, string name, string id, string model,
        Vector2 point, Vector2 approach, float facing, string dialogue, string npcPrefix = "npc.ashfall.cinderwatch.")
    {
        var resident = Group(parent, name, Ground(point));
        resident.localRotation = Quaternion.Euler(0, facing, 0);
        resident.gameObject.layer = 11;
        var capsule = resident.gameObject.AddComponent<CapsuleCollider>();
        capsule.radius = .38f;
        capsule.height = 1.90f;
        capsule.center = Vector3.up * .95f;
        var prefab = Load<GameObject>(Art + "WalkingVillagers/Prefabs/" + model + ".prefab");
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab, resident);
        rig.transform.localScale = Vector3.one * 1.08f;
        var talk = resident.gameObject.AddComponent<ConversationTarget>();
        talk.ConfigureName(name);
        talk.Configure(npcPrefix + id, "arrival", dialogue);
        talk.ConfigureApproach(Group(parent, name + " approach", Ground(approach)));
        Solid(resident, "Resident footprint", Vector3.up * .85f, new(.55f, 1.75f, .55f));
        return resident.gameObject;
    }

    private static void Cabin(Transform parent, string name, float x, float z, float width, float depth, float facing,
        Material roofMaterial = null)
    {
        var home = Group(parent, name, Ground(x, z));
        home.localRotation = Quaternion.Euler(0, facing, 0);
        Part(home, "Dressed stone footing", new(0, .03f, 0), new(width + .45f, .88f, depth + .4f), basalt);
        Part(home, "Cedar walls", new(0, 1.65f, 0), new(width, 2.65f, depth), cedar);
        Solid(home, "House footprint", new(0, 1.38f, 0), new(width, 3.1f, depth));
        for (int row = 0; row < 7; row++)
        {
            float y = .52f + row * .35f;
            Part(home, "Lapped front boards", new(0, y, depth / 2), new(width + .08f, .32f, .15f), row % 3 == 0 ? oak : cedar);
            Part(home, "Lapped rear boards", new(0, y, -depth / 2), new(width + .08f, .32f, .15f), row % 3 == 0 ? oak : cedar);
        }
        for (int side = -1; side <= 1; side += 2)
        {
            float edge = width * .5f - .12f;
            Part(home, "Corner timber", new(side * edge, 1.65f, depth * .5f + .06f), new(.25f, 3.05f, .25f), oak);
            Part(home, "Corner timber", new(side * edge, 1.65f, -depth * .5f - .06f), new(.25f, 3.05f, .25f), oak);
            float xWindow = side * width * .31f;
            Part(home, "Window surround", new(xWindow, 1.8f, depth * .5f + .10f), new(.97f, 1.06f, .18f), oak);
            Part(home, "Lit window", new(xWindow, 1.8f, depth * .5f + .21f), new(.70f, .79f, .09f), light);
            Part(home, "Window mullion", new(xWindow, 1.8f, depth * .5f + .28f), new(.075f, .81f, .075f), oak);
            Part(home, "Window sill", new(xWindow, 1.24f, depth * .5f + .21f), new(1.13f, .13f, .39f), slate);
            Gable(home, side, width, depth, roofMaterial ?? roof);
        }
        Part(home, "Door frame", new(0, 1.19f, depth * .5f + .16f), new(1.60f, 2.35f, .23f), oak);
        Part(home, "Door", new(0, 1.16f, depth * .5f + .31f), new(1.18f, 1.9f, .14f), cedar);
        Part(home, "Door fitting", new(.43f, 1.16f, depth * .5f + .40f), new(.13f, .15f, .09f), bronze);
        Part(home, "Front step", new(0, .19f, depth * .5f + .40f), new(2.05f, .43f, .86f), slate);
        Part(home, "Stone chimney", new(width * .31f, 3.65f, -depth * .24f), new(.77f, 2.40f, .77f), basalt);
        Part(home, "Chimney cap", new(width * .31f, 4.87f, -depth * .24f), new(.97f, .20f, .96f), slate);
    }

    private static void Gable(Transform home, int side, float width, float depth, Material cover)
    {
        float half = depth * .5f + .44f;
        const float rise = 1.55f;
        float pitch = Mathf.Atan2(rise, half) * Mathf.Rad2Deg;
        var leaf = Group(home, "Slate roof pitch", new(0, 3.08f + rise * .5f, side * half * .5f));
        leaf.localRotation = Quaternion.Euler(side * pitch, 0, 0);
        float length = Mathf.Sqrt(half * half + rise * rise);
        Part(leaf, "Roof decking", Vector3.zero, new(width + .84f, .20f, length + .16f), cover);
        int columns = Mathf.CeilToInt(width / .75f);
        float tileWidth = (width + .70f) / columns;
        for (int row = 0; row < 5; row++)
        for (int col = 0; col < columns; col++)
        {
            float z = ((row + .5f) / 5 - .5f) * length;
            float x = (col + .5f - columns * .5f) * tileWidth;
            Part(leaf, "Split slate shingle", new(x, .13f + Range(0, .035f), z), new(tileWidth - .021f, .085f, length * .218f),
                (col + row * 3) % 9 == 0 ? slate : cover);
        }
        float inset = depth * .5f;
        for (int row = 0; row < 6; row++)
        {
            float y = 3.03f + (row + .36f) * .24f;
            float fill = inset * (1 - row / 6f);
            Part(home, "Gable cedar infill", new(side * (width * .5f - .045f), y, 0), new(.18f, .27f, fill * 2), cedar);
        }
    }

    private static void BuildLandmarks(Transform parent)
    {
        var needle = Group(parent, "Basalt Needle", Ground(AshfallReach.BasaltNeedle));
        Rock(needle, new(0, .35f, 0), new(11, 4.3f, 9), basalt);
        for (int i = 0; i < 7; i++)
        {
            float a = i * 137.5f * Mathf.Deg2Rad;
            float distance = i == 0 ? 0 : Range(1.2f, 3.1f);
            float height = i == 0 ? 23 : Range(9, 18.5f);
            var p = new Vector3(Mathf.Sin(a) * distance, height * .34f, Mathf.Cos(a) * distance);
            var pillar = Rock(needle, p, new(Range(2.7f, 4.1f), height, Range(2.6f, 3.6f)), i % 3 == 1 ? slate : basalt);
            pillar.localRotation = Quaternion.Euler(Range(-7, 7), i * 43, Range(-7, 7));
        }
        for (int i = 0; i < 5; i++)
        {
            var vein = Rock(needle, new(2.52f + i * .05f, 1.1f + i * 2.0f, 1.68f), new(.19f, 2.4f, .35f), seam);
            vein.localRotation = Quaternion.Euler(5, 30, -11);
        }
        Solid(needle, "Needle footprint", Vector3.up * 2.2f, new(7.8f, 6f, 6.8f));
        Waypost(parent, "BASALT NEEDLE", Ground(-22.6f, -.5f), 145, 3.15f);

        var crown = Group(parent, "Ember Crown", Ground(AshfallReach.EmberCrown));
        Crown(crown);
        for (int i = 0; i < 10; i++)
        {
            float a = i * 137.5f * Mathf.Deg2Rad;
            Rock(crown, new(Mathf.Sin(a) * Range(7.2f, 9), Range(-.25f, .50f), Mathf.Cos(a) * Range(7.2f, 9)),
                new(Range(1.45f, 3.15f), Range(.7f, 2.0f), Range(1.5f, 3.6f)), i % 3 == 0 ? warmRock : basalt);
        }
        Waypost(parent, "EMBER CROWN", Ground(19.1f, -64.5f), 155, 3.3f);
    }

    private static void Crown(Transform crown)
    {
        const int sides = 18;
        float[] radii = { 10.5f, 8.45f, 5.45f, 4.25f, 3.1f, 1.72f };
        float[] heights = { -.85f, 2.8f, 7.2f, 10.55f, 8.3f, 6.7f };
        var vertices = new List<Vector3>();
        var uv = new List<Vector2>();
        var outer = new List<int>();
        var inner = new List<int>();
        for (int ring = 0; ring < radii.Length; ring++)
        for (int i = 0; i < sides; i++)
        {
            float angle = i * 2 * Mathf.PI / sides;
            float crags = Mathf.Sin(angle * 5) * .58f + Mathf.Cos(angle * 3) * .41f;
            float r = radii[ring] + crags * .87f;
            vertices.Add(new Vector3(Mathf.Sin(angle) * r, heights[ring] + crags, Mathf.Cos(angle) * r * .86f));
            uv.Add(new Vector2(i * .25f, heights[ring] * .23f));
            if (ring == 0) continue;
            int a = (ring - 1) * sides + i, b = (ring - 1) * sides + (i + 1) % sides;
            (ring >= 4 ? inner : outer).AddRange(new[] { a, b, a + sides, b, b + sides, a + sides });
        }
        var mesh = new Mesh { name = "Ember Crown crater", subMeshCount = 2 };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uv);
        mesh.SetTriangles(outer, 0);
        mesh.SetTriangles(inner, 1);
        var surface = Surface(crown, "Ribbed volcanic cone", SaveMesh(mesh), warmRock, basalt);
        surface.gameObject.layer = SolidLayer;
        surface.gameObject.AddComponent<MeshCollider>().sharedMesh = surface.GetComponent<MeshFilter>().sharedMesh;
        Part(crown, "Ember pool", new(0, 6.8f, 0), new(3.65f, .14f, 3.3f), Load<Material>(Handcrafted + "Ember.mat"), PrimitiveType.Cylinder);
        var vent = Group(crown, "Steam from the crater", new(0, 8.5f, 0));
        var steam = vent.gameObject.AddComponent<ParticleSystem>();
        var main = steam.main;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(.70f, .57f, .46f, .46f), new Color(.86f, .78f, .61f, .58f));
        main.startSpeed = new ParticleSystem.MinMaxCurve(.72f, 2.3f);
        main.startSize = new ParticleSystem.MinMaxCurve(.75f, 2.15f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(2.3f, 4);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var shape = steam.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.radius = 1.25f;
        shape.angle = 12;
        vent.localRotation = Quaternion.Euler(-83, 0, 0);
        var emission = steam.emission;
        emission.rateOverTime = 7;
        var fog = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")) { name = "Crown steam" };
        fog.SetFloat("_Surface", 1);
        fog.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        fog.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        fog.SetFloat("_ZWrite", 0);
        var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false)
        {
            name = "Soft crater steam",
            wrapMode = TextureWrapMode.Clamp
        };
        var pixels = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        for (int x = 0; x < 64; x++)
        {
            float radius = new Vector2((x + .5f) / 32 - 1, (y + .5f) / 32 - 1).magnitude;
            float alpha = Mathf.SmoothStep(1, 0, Mathf.Clamp01(radius));
            pixels[y * 64 + x] = new Color(1, 1, 1, alpha * alpha);
        }
        texture.SetPixels(pixels);
        texture.Apply();
        fog.SetTexture("_BaseMap", Save(texture, Output + texture.name + ".asset"));
        fog.SetOverrideTag("RenderType", "Transparent");
        fog.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        fog.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        steam.GetComponent<ParticleSystemRenderer>().sharedMaterial = Save(fog, Output + fog.name + ".mat");
    }

    private static void DressValley(Transform parent)
    {
        var terrain = Group(parent, "Ash banks, scree and burned pines", Vector3.zero);
        var pine = Load<GameObject>(Art + "CornbergBatch/Trees/Tree_Spruce.fbx");
        for (int i = 0; i < 165; i++)
        {
            float x = Range(-44, 46), z = Range(-86, -29);
            var p = new Vector2(x, z);
            if ((p - AshfallReach.Cinderwatch).magnitude < 15 ||
                new Rect(-14,-64,36,35).Contains(p) ||
                (p - AshfallReach.EmberCrown).magnitude < 11 ||
                DistanceToRoad(p, AshfallReach.SettlementRoad) < 4.0f ||
                DistanceToRoad(p, AshfallReach.WillowfordRoad) < 4.0f ||
                DistanceToRoad(p, AshfallReach.CrownPath) < 3.2f) continue;
            var ground = Ground(p);
            if (i % 5 == 0 && x < 15)
            {
                var tree = (GameObject)PrefabUtility.InstantiatePrefab(pine, terrain);
                tree.transform.localPosition = ground;
                tree.transform.localScale = Vector3.one * Range(.88f, 1.44f);
                tree.transform.localRotation = Quaternion.Euler(0, Range(0, 360), 0);
            }
            else
            {
                float size = Range(.40f, 1.65f) * (Mathf.Abs(x) > 36 ? 2.1f : 1);
                Rock(terrain, ground + Vector3.up * size * .14f, new(size, size * Range(.25f, .72f), size * .83f), i % 4 == 0 ? ash : basalt);
            }
        }
        for (int i = 0; i < 16; i++)
        {
            float x = -48 + i * 6.4f;
            float z = -88.8f + Mathf.Sin(i * .5f) * 2.3f;
            Rock(terrain, Ground(x, z) + Vector3.up * 2.6f, new(Range(7, 13), Range(5, 12), Range(6, 10)), i % 3 == 0 ? warmRock : basalt);
        }
    }

    private static float DistanceToRoad(Vector2 point, IReadOnlyList<Vector2> road)
    {
        float distance = float.PositiveInfinity;
        for (int i = 1; i < road.Count; i++)
        {
            var a = road[i-1];
            var delta = road[i] - a;
            float fraction = Mathf.Clamp01(Vector2.Dot(point - a, delta) / delta.sqrMagnitude);
            distance = Mathf.Min(distance, Vector2.Distance(point, a + delta * fraction));
        }
        return distance;
    }

    private static void BakeNavigation(Transform root, Transform expansion)
    {
        Physics.SyncTransforms();
        var sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(root, (1 << WalkableLayer) | (1 << SolidLayer),
            NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);
        var data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0), sources,
            new Bounds(AshfallReach.Origin + new Vector3(-85, 4, -60), new Vector3(294, 35, 194)),
            Vector3.zero, Quaternion.identity);
        if (data == null) throw new InvalidOperationException("Ashfall Reach navigation could not bake.");
        data.name = "Cinderwatch navigation";
        var instance = NavMesh.AddNavMeshData(data);
        try
        {
            var start = OnNav(new Vector2(1, 19));
            RequirePath(start, AshfallReach.NorthGate + Vector2.down * 2);
            RequirePath(start, AshfallReach.Cinderwatch);
            RequirePath(start, new Vector2(3.33f, -47.5f));
            RequirePath(start, AshfallReach.NeedlePath[^1]);
            RequirePath(start, AshfallReach.CrownPath[^1]);
            RequirePath(start, AshfallReach.Willowford);
            foreach(var waypoint in AshfallReach.WillowfordRoad)
            {
                RequirePath(start,waypoint);
                var p=AshfallReach.Origin+Ground(waypoint);
                if(Physics.CheckCapsule(p+Vector3.up*.5f,p+Vector3.up*1.6f,.4f,1<<SolidLayer))
                    throw new InvalidOperationException($"A solid object blocks the Willowford road at {waypoint}.");
            }
            RequirePath(start, new Vector2(-170, -62));
            RequirePath(start, new Vector2(-198, -62));
            RequirePath(start, AshfallReach.RiverbankTrail[^1]);
            Populate(expansion, start, SpearHomes, false);
            Populate(expansion, start, BowHomes, true);
            Populate(expansion, start, new[] { new Vector2(-103,-89), new Vector2(-207,-20), new Vector2(-216,-116) }, false);
            Populate(expansion, start, new[] { new Vector2(-114,-96), new Vector2(-207,-118) }, true);
        }
        finally { instance.Remove(); }
        root.GetComponent<WorldNavigation>().Configure(Save(data, Output + data.name + ".asset"));
    }

    private static Vector3 OnNav(Vector2 p)
    {
        if (NavMesh.SamplePosition(AshfallReach.Origin + Ground(p), out var hit, 2, 1)) return hit.position;
        throw new InvalidOperationException($"No Ashfall Reach navigation at {p}.");
    }

    private static Vector3 RequirePath(Vector3 start, Vector2 end)
    {
        var destination = OnNav(end);
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(start, destination, 1, path) || path.status != NavMeshPathStatus.PathComplete)
            throw new InvalidOperationException($"Ashfall Reach route disconnected at {end}.");
        return destination;
    }

    private static void Populate(Transform parent, Vector3 start, Vector2[] homes, bool bow)
    {
        string name = bow ? "Ashscale Ridgebow" : "Ashscale Spearhand";
        string id = bow ? "ridgebow" : "spearhand";
        var attack = ScriptableObject.CreateInstance<AttackDefinition>();
        attack.name = name + " attack";
        attack.stableId = "attack.ashscale." + id;
        attack.displayName = bow ? "Ridgebow shot" : "Obsidian spear thrust";
        attack.channel = DamageChannel.Physical;
        attack.requiresAccuracy = true;
        attack.baseDamage = bow ? 107 : 121;
        attack.reach = bow ? 10.2f : 2.0f;
        attack.windup = bow ? .75f : .58f;
        attack.interval = bow ? 2.35f : 1.95f;
        var definition = ScriptableObject.CreateInstance<ActorDefinition>();
        definition.name = name;
        definition.stableId = "enemy.ashscale." + id;
        definition.displayName = name;
        definition.familyId = "ashscale.lizardman";
        definition.tags = new[] { "lizardman", "ashscale", "ashfall-reach" };
        definition.tuning = Load<CombatTuning>("Assets/_DiceFree/Settings/Combat/Cornberg provisional tuning.asset");
        definition.baseAttributes = new AttributeValues { strength = 19, agility = 24, vitality = 9 };
        definition.growth = default;
        definition.baseHp = bow ? 3610 : 4060;
        definition.physicalDefense = bow ? 22 : 30;
        definition.magicalDefense = bow ? 26 : 22;
        definition.experienceReward = bow ? 950 : 985;
        definition.moveSpeed = bow ? 4.65f : 4.9f;
        definition.overrideVitalityHp = definition.overrideVitalityRegeneration = true;
        definition.hpPerVitality = definition.regenerationPerVitality = 0;
        definition.basicAttack = Save(attack, Output + attack.name + ".asset");
        definition = Save(definition, Output + definition.name + ".asset");
        var respawn = ScriptableObject.CreateInstance<RespawnDefinition>();
        respawn.name = "Ashscale respawn";
        respawn.delaySeconds = 65;
        respawn = Save(respawn, Output + respawn.name + ".asset");
        var scaledHide = Tint("Iron red scales", Load<Material>(Reedscale + "Jade scales.mat"), new Color(.44f, .195f, .106f));
        var scutes = Tint("Pale ash scutes", Load<Material>(Reedscale + "Sage throat and claws.mat"), new Color(.66f, .47f, .32f));
        var cloth = Tint("Charcoal blue cloth", Load<Material>(Reedscale + "Reedscale indigo cloth.mat"), new Color(.18f, .21f, .27f));
        var prefab = Load<GameObject>(Reedscale + (bow ? "Reedscale sentry model" : "Reedscale spear guard model") + ".prefab");
        var patrol = Group(parent, name + " patrols", Vector3.zero);
        for (int i = 0; i < homes.Length; i++)
        {
            int level = homes[i].y > -20 ? 53 : bow ? 54 : 55;
            var actor = Group(patrol, $"{name} (Lv {level})", RequirePath(start, homes[i]) - AshfallReach.Origin);
            actor.gameObject.layer = ActorLayer;
            actor.localRotation = Quaternion.Euler(0, i * 137.5f, 0);
            var agent = actor.gameObject.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            agent.radius = .52f;
            agent.height = 2.35f;
            agent.areaMask = 1;
            var capsule = actor.gameObject.AddComponent<CapsuleCollider>();
            capsule.height = 2.35f;
            capsule.radius = .52f;
            capsule.center = Vector3.up * 1.175f;
            actor.gameObject.AddComponent<ActorStats>().Configure(definition, level);
            actor.gameObject.AddComponent<CombatActor>().Configure(1, .52f);
            actor.gameObject.AddComponent<CombatStatusController>();
            actor.gameObject.AddComponent<BasicAttack>();
            actor.gameObject.AddComponent<DefeatReporter>();
            actor.gameObject.AddComponent<OverworldRespawn>().Configure(respawn);
            actor.gameObject.AddComponent<AggroBehaviour>().Configure(bow ? 9.2f : 7.5f, 10.5f);
            var rig = ((GameObject)PrefabUtility.InstantiatePrefab(prefab, actor)).transform;
            rig.localScale = Vector3.one * (bow ? 1.02f : 1.11f);
            foreach (var renderer in rig.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterials = renderer.sharedMaterials.Select(material => material.name switch
                {
                    "Jade scales" => scaledHide,
                    "Sage throat and claws" => scutes,
                    "Reedscale indigo cloth" => cloth,
                    _ => material
                }).ToArray();
            }
            var grip = rig.Find(bow ? "Left arm/Weapon grip" : "Right arm/Weapon grip");
            actor.gameObject.AddComponent<ReedscaleMotion>().Configure(rig.Find("Left leg"), rig.Find("Right leg"),
                rig.Find("Left arm"), rig.Find("Right arm"), rig.Find("Tail"), grip, grip.Find("Arrow nock"), bow);
            actor.gameObject.AddComponent<ActorFeedback>().Configure(rig);
        }
    }

    private static void Lantern(Transform parent, Vector3 p)
    {
        var lantern = Group(parent, "Copper lantern", p);
        Part(lantern, "Oak post", new(0, 1.12f, 0), new(.24f, 2.24f, .24f), oak);
        Part(lantern, "Footing", new(0, .14f, 0), new(.68f, .4f, .65f), basalt);
        Part(lantern, "Warm glass", new(0, 2.32f, 0), new(.39f, .56f, .39f), light);
        Part(lantern, "Copper cap", new(0, 2.66f, 0), new(.53f, .15f, .53f), bronze);
        var point = Group(lantern, "Lantern light", new(0, 2.40f, 0)).gameObject.AddComponent<Light>();
        point.type = LightType.Point;
        point.color = new Color(1, .62f, .26f);
        point.range = 5;
        point.intensity = 1.6f;
        point.shadows = LightShadows.None;
    }

    private static void Waypost(Transform parent, string title, Vector3 position, float rotation, float width)
    {
        var sign = Group(parent, title, position);
        sign.localRotation = Quaternion.Euler(0, rotation, 0);
        Part(sign, "Oak post", new(0, .97f, 0), new(.24f, 1.98f, .25f), oak);
        Part(sign, "Wayboard", new(0, 1.72f, 0), new(width, .52f, .16f), oak);
        Part(sign, "Copper nail", new(-width * .43f, 1.73f, .11f), new(.075f, .075f, .06f), bronze);
        Part(sign, "Copper nail", new(width * .43f, 1.73f, .11f), new(.075f, .075f, .06f), bronze);
        var label = Group(sign, "Lettering", new(0, 1.72f, .13f));
        label.localRotation = Quaternion.Euler(0, 180, 0);
        var text = label.gameObject.AddComponent<TextMesh>();
        text.text = title;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontSize = 44;
        text.characterSize = .060f;
        text.color = new Color(.97f, .78f, .50f);
    }

    private static Transform Rock(Transform parent, Vector3 p, Vector3 size, Material material)
    {
        var rock = Surface(parent, "Fractured basalt", stoneMesh, material);
        rock.localPosition = p;
        rock.localScale = size;
        rock.localRotation = Quaternion.Euler(Range(-11, 11), Range(0, 360), Range(-12, 12));
        return rock;
    }

    private static void Solid(Transform parent, string name, Vector3 p, Vector3 size)
    {
        var solid = Group(parent, name, p).gameObject;
        solid.layer = SolidLayer;
        solid.AddComponent<BoxCollider>().size = size;
    }

    private static Transform Part(Transform parent, string name, Vector3 p, Vector3 size, Material material,
        PrimitiveType shape = PrimitiveType.Cube, Quaternion? rotation = null)
    {
        var go = GameObject.CreatePrimitive(shape);
        go.name = name;
        go.layer = 2;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = p;
        go.transform.localRotation = rotation ?? Quaternion.identity;
        go.transform.localScale = size;
        go.GetComponent<MeshRenderer>().sharedMaterial = material;
        return go.transform;
    }

    private static Transform Surface(Transform parent, string name, Mesh mesh, params Material[] materials)
    {
        var surface = Group(parent, name, Vector3.zero);
        surface.gameObject.layer = 2;
        surface.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        surface.gameObject.AddComponent<MeshRenderer>().sharedMaterials = materials;
        return surface;
    }

    private static Transform Group(Transform parent, string name, Vector3 position)
    {
        var child = new GameObject(name).transform;
        child.SetParent(parent, false);
        child.localPosition = position;
        return child;
    }

    private static float Range(float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());
    private static T Load<T>(string path) where T : Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new IOException("Missing asset " + path);

    private static Material Tint(string name, Material source, Color color)
    {
        var material = new Material(source) { name = name };
        material.SetColor("_BaseColor", color);
        return Save(material, Output + name + ".mat");
    }

    private static Mesh SaveMesh(Mesh mesh)
    {
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return Save(mesh, Output + mesh.name + ".asset");
    }

    private static T Save<T>(T value, string path) where T : Object
    {
        var current = AssetDatabase.LoadAssetAtPath<T>(path);
        if (current == null)
        {
            AssetDatabase.CreateAsset(value, path);
            return value;
        }
        EditorUtility.CopySerialized(value, current);
        EditorUtility.SetDirty(current);
        Object.DestroyImmediate(value);
        return current;
    }
}
