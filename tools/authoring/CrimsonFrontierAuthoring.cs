using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.EditorTools;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static class CrimsonFrontierAuthoring
{
    private const string World = "Assets/_DiceFree/Art/World/";
    private const string Art = World + "CrimsonMire";
    private const string Settings = "Assets/_DiceFree/Settings/CrimsonMire";
    private const string Reedscale = "Assets/_DiceFree/Settings/ReedscaleLookout/";
    private const string RootName = "Crimson Mire";
    private static readonly Vector2 Center = new(65, 144);
    private static readonly Vector2 Radii = new(72, 47);
    private static readonly Vector2 Arborium = new(98.04f, 304.74f);
    private static readonly Vector2[] Trail =
    {
        new(30,52), new(32,68), new(36,86), new(47,106), new(52,125), new(65,144),
        new(83,149), new(102,145), new(131,135), new(145,131), new(165,130)
    };
    private static readonly (Vector2 point, Vector2 radii)[] Pools =
    {
        (new(25,143), new(13,7)), (new(64,169), new(18,9)),
        (new(95,119), new(13,6)), (new(113,158), new(9,5))
    };
    private static readonly Vector2[] Homes =
    {
        new(17,136), new(26,153), new(37,146), new(49,162), new(64,180),
        new(78,164), new(82,120), new(96,110), new(103,129), new(104,157),
        new(119,166), new(121,149)
    };
    private static MeshCollider valley;
    private static readonly List<Object> ChangedSceneObjects = new();

    public static void Batch()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
        if (GameObject.Find(RootName) != null) throw new InvalidOperationException("Crimson Mire is already present.");
        AssetDatabase.CreateFolder(World.TrimEnd('/'), "CrimsonMire");
        AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "CrimsonMire");
        ChangedSceneObjects.Clear();
        TrimEastwatchSlope();
        var tuning = RaiseReedscale();
        valley = GameObject.Find("Cornberg valley").GetComponent<MeshCollider>();
        Physics.SyncTransforms();
        var root = Group(null, RootName, Vector3.zero);
        BuildSurface(root);
        Dress(root);
        BuildWarning(root, new Vector2(30,81), 219);
        BuildWarning(root, new Vector2(147,128), 240);

        RebuildNavigation();
        var navigation = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            RequirePath(new Vector2(29,49), Center, "Cornberg north meadow");
            RequirePath(new Vector2(170,130), Center, "Ironpine woodland");
            Populate(root, tuning);
        }
        finally { navigation.Remove(); }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save Cornberg.");
        File.WriteAllLines("Logs/CrimsonFrontierSceneIds.txt", ChangedSceneObjects.Select(item =>
            GlobalObjectId.GetGlobalObjectIdSlow(item).targetObjectId.ToString()));
        Debug.Log("DICEFREE_CRIMSON_FRONTIER_AUTHORING_OK");
    }

    private static void TrimEastwatchSlope()
    {
        var filter = Object.FindObjectsByType<MeshFilter>().Single(item => item.name == "Eastwatch outer slope");
        var mesh = Object.Instantiate(filter.sharedMesh);
        var positions = mesh.vertices.Select(v => Planar(filter.transform.TransformPoint(v)) - Arborium).ToArray();
        int removed = 0;
        for (int slot = 0; slot < mesh.subMeshCount; slot++)
        {
            var indices = mesh.GetTriangles(slot);
            var kept = new List<int>(indices.Length);
            for (int i = 0; i < indices.Length; i += 3)
            {
                var a = positions[indices[i]];
                var b = positions[indices[i + 1]];
                var c = positions[indices[i + 2]];
                if (TouchesCapital(a, b, c)) { removed++; continue; }
                kept.Add(indices[i]); kept.Add(indices[i + 1]); kept.Add(indices[i + 2]);
            }
            mesh.SetTriangles(kept, slot);
        }
        if (removed == 0) throw new InvalidOperationException("No Eastwatch slope triangles crossed the capital footprint.");
        mesh.name = "Eastwatch slope outside Arborium";
        AssetDatabase.CreateAsset(mesh, Art + "/" + mesh.name + ".asset");
        filter.sharedMesh = mesh;
        ChangedSceneObjects.Add(filter);
        Debug.Log($"Trimmed {removed} slope triangles at Arborium; the Eastwatch ground and colliders were kept.");
    }

    private static bool TouchesCapital(Vector2 a, Vector2 b, Vector2 c)
    {
        const float radius = 84;
        float ab = Cross(a, b), bc = Cross(b, c), ca = Cross(c, a);
        bool coversCenter = (ab > 0 && bc > 0 && ca > 0) || (ab < 0 && bc < 0 && ca < 0);
        return coversCenter || SegmentDistance(Vector2.zero, a, b) < radius ||
               SegmentDistance(Vector2.zero, b, c) < radius || SegmentDistance(Vector2.zero, c, a) < radius;
    }

    private static CombatTuning RaiseReedscale()
    {
        var spear = AssetDatabase.LoadAssetAtPath<ActorDefinition>(Reedscale + "Reedscale Spear Guard.asset");
        var bow = AssetDatabase.LoadAssetAtPath<ActorDefinition>(Reedscale + "Reedscale Ridgebow Sentry.asset");
        spear.baseHp = 2640;
        spear.basicAttack.baseDamage = 76;
        bow.baseHp = 2095;
        bow.basicAttack.baseDamage = 65;
        foreach (var item in new Object[] { spear, bow, spear.basicAttack, bow.basicAttack }) EditorUtility.SetDirty(item);
        var defenders = Object.FindObjectsByType<ActorStats>()
            .Where(stats => stats.Definition == spear || stats.Definition == bow).ToArray();
        if (defenders.Length != 11) throw new InvalidOperationException("Expected eleven Reedscale defenders.");
        foreach (var stats in defenders)
        {
            if (stats.Level >= 39) continue;
            int level = stats.Level + (stats.Definition == spear ? 4 : 3);
            stats.Configure(stats.Definition, level);
            stats.name = stats.Definition.displayName + " (Lv " + level + ")";
            ChangedSceneObjects.Add(stats);
            ChangedSceneObjects.Add(stats.gameObject);
        }
        return spear.tuning;
    }

    private static void BuildSurface(Transform root)
    {
        var soil = TexturedGround("Iron-stained peat", new Vector2(-15, 91), new Vector2(145, 189), false);
        var trail = TexturedGround("Crimson Mire footpath", new Vector2(23, 50), new Vector2(168, 155), true);
        trail.renderQueue = soil.renderQueue + 1;
        GroundPatch(root, soil, new Vector2(-15, 91), new Vector2(145, 189), .048f);
        GroundPatch(root, trail, new Vector2(23, 50), new Vector2(168, 155), .085f);
        for (int i = 0; i < Pools.Length; i++)
        {
            var (center, radii) = Pools[i];
            var material = new Material(Shader.Find("DiceFree/Woodland Water")) { name = "Rust pool " + (i + 1) };
            material.SetColor("_DeepColor", new Color(.105f,.041f,.051f));
            material.SetColor("_ShallowColor", new Color(.38f,.16f,.105f));
            material.SetColor("_GlintColor", new Color(.65f,.44f,.31f));
            material.SetVector("_Flow", new Vector4(.23f,.09f,0,0));
            material.SetVector("_Pond", new Vector4(center.x,center.y,radii.x,radii.y));
            AssetDatabase.CreateAsset(material, Art + "/" + material.name + ".mat");
            var points = new List<Vector3> { Ground(center) + Vector3.up * .13f };
            var triangles = new List<int>();
            const int steps = 48;
            for (int j = 0; j < steps; j++)
            {
                float angle = j * Mathf.PI * 2 / steps;
                float edge = 1 + .07f * Mathf.Sin(angle * 5 + i);
                Vector2 bank = center + Vector2.Scale(radii, new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))) * edge;
                points.Add(Ground(bank) + Vector3.up * .12f);
                triangles.Add(0); triangles.Add(1 + (j + 1) % steps); triangles.Add(1 + j);
            }
            Surface(root, material.name, points.ToArray(), null, triangles.ToArray(), material);
        }
    }

    private static Material TexturedGround(string name, Vector2 min, Vector2 max, bool trail)
    {
        const int size = 512;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        for (int z = 0; z < size; z++)
        for (int x = 0; x < size; x++)
        {
            var point = min + Vector2.Scale(max - min, new Vector2(x/(size-1f), z/(size-1f)));
            float broad = Mathf.PerlinNoise(point.x * .084f + 21, point.y * .084f);
            float coarse = Mathf.PerlinNoise(point.x * .39f, point.y * .39f + 8);
            float grain = Mathf.PerlinNoise(point.x * 2.4f + 37, point.y * 2.4f);
            float blend = .46f * broad + .35f * coarse + .19f * grain;
            var dark = trail ? new Color(.25f,.17f,.12f) : new Color(.17f,.085f,.072f);
            var light = trail ? new Color(.59f,.45f,.32f) : new Color(.46f,.24f,.17f);
            var color = Color.Lerp(dark, light, blend);
            if (trail)
            {
                float distance = TrailDistance(point);
                float taper = Mathf.Clamp01(Mathf.Min(point.y-51, 166-point.x) / 4);
                color.a = .94f * taper * (1 - Mathf.SmoothStep(0,1,Mathf.InverseLerp(.85f,2.75f,distance + (coarse-.5f)*.8f)));
            }
            else
            {
                float distance = RegionDistance(point);
                float edge = Mathf.InverseLerp(.73f,.995f,distance + (broad-.5f)*.17f);
                color.a = 1 - Mathf.SmoothStep(0,1,edge);
            }
            pixels[z * size + x] = color;
        }
        texture.SetPixels32(pixels);
        string texturePath = Art + "/" + name + ".png";
        File.WriteAllBytes(texturePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.alphaIsTransparency = true;
        importer.anisoLevel = 4;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        var material = NewMaterial(name, Color.white, .11f);
        material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
        material.SetFloat("_Surface", 1);
        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
        material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetShaderPassEnabled("ShadowCaster", false);
        material.renderQueue = (int)RenderQueue.Transparent - 12;
        return material;
    }

    private static void GroundPatch(Transform root, Material material, Vector2 min, Vector2 max, float lift)
    {
        int width = Mathf.CeilToInt(max.x - min.x) + 1;
        int height = Mathf.CeilToInt(max.y - min.y) + 1;
        var vertices = new Vector3[width * height];
        var uv = new Vector2[vertices.Length];
        var triangles = new List<int>();
        for (int z = 0; z < height; z++)
        for (int x = 0; x < width; x++)
        {
            int i = z * width + x;
            uv[i] = new Vector2(x / (width - 1f), z / (height - 1f));
            vertices[i] = Ground(min + Vector2.Scale(max - min, uv[i])) + Vector3.up * lift;
            if (z == 0 || x == 0) continue;
            triangles.Add(i - width - 1); triangles.Add(i - 1); triangles.Add(i);
            triangles.Add(i - width - 1); triangles.Add(i); triangles.Add(i - width);
        }
        Surface(root, material.name, vertices, uv, triangles.ToArray(), material);
    }

    private static void Dress(Transform root)
    {
        var random = new System.Random(507312);
        var decor = Group(root, "Mire forest and banks", Vector3.zero);
        var forest = new List<Vector2>();
        var rush = NewMaterial("Copper sedges", new Color(.52f,.29f,.12f), .24f);
        var shale = NewMaterial("Ironstone", new Color(.21f,.16f,.16f), .32f);
        var ochre = NewMaterial("Ochre seams", new Color(.38f,.17f,.105f), .29f);
        var basalt = NewMaterial("Wet bank shale", new Color(.14f,.13f,.14f), .45f);
        var bark = NewMaterial("Waterlogged bark", new Color(.21f,.13f,.095f), .21f);
        var sedgeVertices = new List<Vector3>();
        var sedgeTriangles = new List<int>();
        for (int i = 0; i < 250; i++)
        {
            float angle = Next(random, 0, Mathf.PI * 2);
            float radius = Next(random, .28f, .96f);
            var point = Center + Vector2.Scale(Radii, new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))) * radius;
            if (TrailDistance(point) < 3.4f || Homes.Any(home => Vector2.Distance(home,point) < 3.5f) ||
                Pools.Any(pool => PoolDistance(point,pool) < 1.13f)) continue;

            if (radius > .73f && i % 3 == 0 && forest.All(tree => Vector2.Distance(tree, point) > 5.8f))
            {
                forest.Add(point);
                string type = i % 2 == 0 ? "Pine" : "Twisted";
                Model(decor, type, World + "CornbergBatch/Trees/Tree_" + type + ".fbx",
                    Ground(point), Next(random,5.8f,10.5f), angle, null, true);
            }
            else if (i % 3 == 1)
            {
                int variant = 1 + i % 5;
                float height = Next(random,.29f,radius > .81f ? 1.7f : .89f);
                var stone = Model(decor, "Ironstone outcrop", World + "VisualBatch01/Models/rock_0" + variant + ".fbx",
                    Ground(point), height, angle, i % 5 == 0 ? ochre : shale, false);
                if (height > 1.1f) AddObstacle(decor, stone, "Solid ironstone");
            }
            else Sedges(Ground(point), random, sedgeVertices, sedgeTriangles);
        }
        foreach (var pool in Pools)
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = i * 2.39996f;
                var radial = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var point = pool.point + Vector2.Scale(pool.radii, radial) * Next(random,1.02f,1.19f);
                if (TrailDistance(point) < 2.9f) continue;
                if (i % 4 == 0)
                    Model(decor,"Pool edge stone",World+"VisualBatch01/Models/rock_03.fbx",Ground(point),
                        Next(random,.42f,.94f),angle,basalt,false);
                else
                {
                    Sedges(Ground(point), random, sedgeVertices, sedgeTriangles);
                    Sedges(Ground(point + radial * .7f), random, sedgeVertices, sedgeTriangles);
                }
            }
        }
        var outcrops = new[]
        {
            (new Vector2(54.2f,153.4f),2.3f), (new Vector2(53,155.4f),1.5f),
            (new Vector2(52.3f,152.1f),1.05f), (new Vector2(82.4f,136),1.9f),
            (new Vector2(84.3f,134.8f),1.3f), (new Vector2(86.6f,136),.78f),
            (new Vector2(97.4f,173),2.5f), (new Vector2(95.6f,171),1.45f),
            (new Vector2(39.3f,122.4f),2.15f), (new Vector2(38.2f,124.5f),1.1f),
        };
        for (int i = 0; i < outcrops.Length; i++)
        {
            var (point, height) = outcrops[i];
            var stone = Model(decor,"Fractured ironstone",World+"VisualBatch01/Models/rock_0"+(1+i%5)+".fbx",
                Ground(point),height,Next(random,0,360),i%3 == 1 ? ochre : basalt,false);
            if (height > 1.1f) AddObstacle(decor, stone, "Solid fractured ironstone");
            Sedges(Ground(point + new Vector2(.7f,.8f)), random, sedgeVertices, sedgeTriangles);
            Sedges(Ground(point + new Vector2(-1.1f,.7f)), random, sedgeVertices, sedgeTriangles);
        }
        foreach (var point in new[] { new Vector2(43,139), new Vector2(55,164), new Vector2(107,122) })
            Model(decor,"Twisted mire tree",World+"CornbergBatch/Trees/Tree_Twisted.fbx",Ground(point),
                Next(random,6.3f,7.9f),Next(random,0,360),null,true);
        Surface(decor, "Copper sedge beds", sedgeVertices.ToArray(), null, sedgeTriangles.ToArray(), rush);
        var log = Model(decor,"Waterlogged timber",World+"VisualBatch01/Models/log_03.fbx",Ground(new Vector2(46,154)),1.05f,32,bark,false);
        AddObstacle(decor,log,"Solid fallen timber");
        log = Model(decor,"Waterlogged timber",World+"VisualBatch01/Models/log_01.fbx",Ground(new Vector2(87,172)),.84f,72,bark,false);
        AddObstacle(decor,log,"Solid fallen timber");
    }

    private static void AddObstacle(Transform parent, GameObject model, string name)
    {
        var bounds = BoundsOf(model.GetComponentsInChildren<MeshRenderer>());
        var block = Group(parent,name,bounds.center).gameObject;
        block.layer = 9;
        block.AddComponent<BoxCollider>().size = new Vector3(bounds.size.x * .68f, bounds.size.y, bounds.size.z * .68f);
    }

    private static void Sedges(Vector3 at, System.Random random, List<Vector3> vertices, List<int> triangles)
    {
        for (int i = 0; i < 9; i++)
        {
            float angle = Next(random, 0, Mathf.PI * 2);
            var along = new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
            var side = Vector3.Cross(Vector3.up,along) * Next(random,.025f,.066f);
            var foot = at + along * Next(random,.035f,.31f);
            var middle = foot + Vector3.up * Next(random,.25f,.48f) + along * .13f;
            var tip = middle + Vector3.up * Next(random,.16f,.37f) + along * .19f;
            int start = vertices.Count;
            vertices.Add(foot - side); vertices.Add(foot + side);
            vertices.Add(middle - side * .6f); vertices.Add(middle + side * .6f); vertices.Add(tip);
            triangles.AddRange(new[]{start,start+2,start+1,start+1,start+2,start+3,start+2,start+4,start+3});
        }
    }

    private static void BuildWarning(Transform root, Vector2 point, float heading)
    {
        var sign = Group(root, "Crimson Mire warning", Ground(point));
        sign.localRotation = Quaternion.Euler(0, heading, 0);
        var wood = NewMaterial("Weathered warning sign timber", new Color(.30f,.19f,.115f), .16f);
        var band = NewMaterial("Warning sign bronze", new Color(.53f,.35f,.17f), .43f);
        Part(sign,"Sign post",PrimitiveType.Cube,new Vector3(0,1.08f,0),new Vector3(.16f,2.16f,.19f),wood);
        var face = Group(sign,"Tilted warning board",new Vector3(0,1.85f,0));
        face.localRotation = Quaternion.Euler(-24,0,0);
        Part(face,"Sign board",PrimitiveType.Cube,Vector3.zero,new Vector3(2.35f,.68f,.13f),wood);
        Part(face,"Bronze upper rim",PrimitiveType.Cube,new Vector3(0,.316f,.065f),new Vector3(2.35f,.048f,.045f),band);
        Part(face,"Bronze lower rim",PrimitiveType.Cube,new Vector3(0,-.316f,.065f),new Vector3(2.35f,.048f,.045f),band);
        var label = Group(face,"CRIMSON MIRE - LEVEL 50",new Vector3(0,-.01f,.078f)).gameObject.AddComponent<TextMesh>();
        label.transform.localRotation = Quaternion.Euler(0,180,0);
        label.text = "CRIMSON MIRE\nLEVEL 50";
        label.color = new Color(1,.87f,.67f);
        label.alignment = TextAlignment.Center;
        label.anchor = TextAnchor.MiddleCenter;
        label.characterSize = .034f;
        label.fontSize = 42;
        label.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
    }

    private static void Populate(Transform root, CombatTuning tuning)
    {
        var attack = Create<AttackDefinition>("Red Slime attack");
        attack.stableId = "attack.slime.crimson-bump";
        attack.displayName = "Crimson bump";
        attack.channel = DamageChannel.Physical;
        attack.requiresAccuracy = true;
        attack.baseDamage = 104;
        attack.reach = 1.65f;
        attack.windup = .66f;
        attack.interval = 1.95f;
        var species = Create<ActorDefinition>("Red Slime");
        species.stableId = "enemy.slime.red";
        species.displayName = "Red Slime";
        species.familyId = "slime";
        species.tags = new[]{"slime","crimson-mire"};
        species.tuning = tuning;
        species.baseAttributes = new AttributeValues { strength = 17, agility = 19 };
        species.baseHp = 3520;
        species.physicalDefense = 25;
        species.magicalDefense = species.physicalDefense + 17 * tuning.Coefficient(SecondaryStat.PhysicalDefense);
        species.moveSpeed = 4.4f;
        species.experienceReward = 815;
        species.basicAttack = attack;
        species.overrideVitalityHp = species.overrideVitalityRegeneration = true;
        var respawn = Create<RespawnDefinition>("Red Slime respawn");
        respawn.delaySeconds = 75;
        foreach (var asset in new Object[]{attack,species,respawn}) EditorUtility.SetDirty(asset);
        var actors = Group(root, "Red Slimes", Vector3.zero);
        var prefab = SlimeModel();
        for (int i = 0; i < Homes.Length; i++)
        {
            RequirePath(Center,Homes[i],"Red Slime " + (i + 1));
            var point = NavPoint(Homes[i]);
            var obj = Group(actors, "Red Slime (Lv 50)", point).gameObject;
            obj.transform.rotation = Quaternion.Euler(0, i * 137.5f, 0);
            obj.layer = 10;
            var agent = obj.AddComponent<NavMeshAgent>();
            agent.enabled = false;
            agent.radius = .6f;
            agent.height = 1.45f;
            agent.areaMask = 1;
            var capsule = obj.AddComponent<CapsuleCollider>();
            capsule.radius = .6f;
            capsule.height = 1.45f;
            capsule.center = Vector3.up * .725f;
            obj.AddComponent<ActorStats>().Configure(species,50);
            obj.AddComponent<CombatActor>().Configure(1,.6f,"slime");
            obj.AddComponent<CombatStatusController>();
            obj.AddComponent<BasicAttack>();
            obj.AddComponent<DefeatReporter>();
            obj.AddComponent<OverworldRespawn>().Configure(respawn);
            obj.AddComponent<AggroBehaviour>().Configure(6.8f,17f);
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, obj.transform);
            visual.name = "Presentation";
            visual.transform.localScale *= .92f + (i % 4) * .06f;
            obj.AddComponent<ActorFeedback>().Configure(visual.transform);
            obj.AddComponent<SlimeVisualMotion>();
        }
    }

    private static GameObject SlimeModel()
    {
        var gel = NewMaterial("Scarlet gel", new Color(.77f,.047f,.105f), .86f);
        var belly = NewMaterial("Dark red jelly skirt", new Color(.38f,.025f,.065f), .69f);
        var eyes = NewMaterial("Slime dark eyes", new Color(.025f,.008f,.023f), .93f);
        var glint = NewMaterial("Gel sheen", new Color(1,.32f,.39f), .79f);
        var core = NewMaterial("Garnet nodules", new Color(.52f,.023f,.096f), .89f);
        var visual = Group(null,"Presentation",Vector3.zero);
        Part(visual,"Pooled jelly",PrimitiveType.Sphere,new Vector3(0,.19f,0),new Vector3(1.96f,.39f,1.67f),belly);
        Part(visual,"Scarlet dome",PrimitiveType.Sphere,new Vector3(0,.60f,0),new Vector3(1.77f,1.18f,1.55f),gel);
        Part(visual,"Top sheen",PrimitiveType.Sphere,new Vector3(-.25f,1.137f,.13f),new Vector3(.5f,.075f,.36f),glint)
            .localRotation = Quaternion.Euler(6,-17,15);
        var nodules = Group(visual,"GelCore",new Vector3(0,.57f,-.29f));
        for (int i = 0; i < 6; i++)
        {
            float angle = (i + 1) * Mathf.PI / 3;
            var position = new Vector3(Mathf.Cos(angle)*.66f, .22f + .14f*Mathf.Sin(i*2), Mathf.Sin(angle)*.36f);
            float scale = .14f + (i % 3) * .033f;
            Part(nodules,"Garnet nodule",PrimitiveType.Sphere,position,new Vector3(scale,scale,scale)*2,core);
        }
        for (int side = -1; side <= 1; side += 2)
        {
            var eye = Part(visual,side < 0 ? "Left eye" : "Right eye",PrimitiveType.Sphere,
                new Vector3(side*.29f,.696f,.727f),new Vector3(.21f,.28f,.14f),eyes);
            Part(eye,"Catchlight",PrimitiveType.Sphere,new Vector3(-.14f,.18f,.4f),
                new Vector3(.29f,.27f,.25f),glint);
        }
        var prefab = PrefabUtility.SaveAsPrefabAsset(visual.gameObject, Art + "/Red slime presentation.prefab");
        Object.DestroyImmediate(visual.gameObject);
        return prefab;
    }

    private static void RebuildNavigation()
    {
        var route = Object.FindObjectsByType<MonoBehaviour>().Single(item => item.GetType().Name == "OgreRoadwardenRoute");
        using var serialized = new SerializedObject(route);
        var wreckage = serialized.FindProperty("wreckage");
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

    private static Vector3 NavPoint(Vector2 point)
    {
        if (!NavMesh.SamplePosition(Ground(point),out var hit,2,NavMesh.AllAreas))
            throw new InvalidOperationException("Crimson Mire missing nav near " + point);
        return hit.position;
    }

    private static void RequirePath(Vector2 start, Vector2 end, string label)
    {
        var path = new NavMeshPath();
        if (!NavMesh.CalculatePath(NavPoint(start),NavPoint(end),NavMesh.AllAreas,path) ||
            path.status != NavMeshPathStatus.PathComplete)
            throw new InvalidOperationException("Crimson Mire path failed from " + label);
    }

    private static float Cross(Vector2 a, Vector2 b) => a.x*b.y - a.y*b.x;
    private static Vector2 Planar(Vector3 point) => new(point.x,point.z);
    private static float RegionDistance(Vector2 point) => Divide(point - Center,Radii).magnitude;
    private static float PoolDistance(Vector2 point, (Vector2 point, Vector2 radii) pool) =>
        Divide(point-pool.point,pool.radii).magnitude;
    private static Vector2 Divide(Vector2 point, Vector2 radii) => new(point.x/radii.x,point.y/radii.y);
    private static float TrailDistance(Vector2 point)
    {
        float result = float.PositiveInfinity;
        for (int i = 1; i < Trail.Length; i++)
            result = Mathf.Min(result,SegmentDistance(point, Trail[i-1], Trail[i]));
        return result;
    }
    private static float SegmentDistance(Vector2 point, Vector2 a, Vector2 b)
    {
        var edge = b - a;
        if (edge.sqrMagnitude < .0001f) return Vector2.Distance(point,a);
        return Vector2.Distance(point,a+edge*Mathf.Clamp01(Vector2.Dot(point-a,edge)/edge.sqrMagnitude));
    }
    private static Vector3 Ground(Vector2 point)
    {
        if (!valley.Raycast(new Ray(new Vector3(point.x,40,point.y),Vector3.down),out var hit,75))
            throw new InvalidOperationException("Crimson Mire outside valley at " + point);
        return hit.point;
    }
    private static float Next(System.Random random, float min, float max) => Mathf.Lerp(min,max,(float)random.NextDouble());

    private static Transform Group(Transform parent, string name, Vector3 localPosition)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent,false);
        obj.transform.localPosition = localPosition;
        return obj.transform;
    }

    private static Transform Part(Transform parent, string name, PrimitiveType shape, Vector3 position, Vector3 scale, Material material)
    {
        var part = GameObject.CreatePrimitive(shape);
        part.name = name;
        part.transform.SetParent(parent,false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        Object.DestroyImmediate(part.GetComponent<Collider>());
        part.GetComponent<MeshRenderer>().sharedMaterial = material;
        return part.transform;
    }

    private static GameObject Model(Transform root, string name, string path, Vector3 point, float height, float heading, Material material, bool tree)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab,root);
        PrefabUtility.UnpackPrefabInstance(model,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        model.name = name;
        model.transform.SetPositionAndRotation(point,Quaternion.Euler(0,heading,0));
        foreach (var collider in model.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
        var renderers = model.GetComponentsInChildren<MeshRenderer>();
        var bounds = BoundsOf(renderers);
        model.transform.localScale *= height / Mathf.Max(.01f,bounds.size.y);
        bounds = BoundsOf(renderers);
        model.transform.position += new Vector3(point.x-bounds.center.x,point.y-bounds.min.y,point.z-bounds.center.z);
        if (material != null)
            foreach (var renderer in renderers) renderer.sharedMaterials = renderer.sharedMaterials.Select(_ => material).ToArray();
        if (!tree) return model;
        var trunk = Group(root,"Solid trunk",point);
        trunk.gameObject.layer = 9;
        var body = trunk.gameObject.AddComponent<CapsuleCollider>();
        body.height = 2.2f;
        body.radius = .48f;
        body.center = Vector3.up * 1.1f;
        model.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(7);
        return model;
    }

    private static Bounds BoundsOf(MeshRenderer[] renderers)
    {
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }

    private static void Surface(Transform root, string name, Vector3[] points, Vector2[] uv, int[] triangles, Material material)
    {
        var mesh = new Mesh { name = name, vertices = points, triangles = triangles };
        if (uv != null) mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Art+"/"+name+".asset");
        var obj = Group(root,name,Vector3.zero).gameObject;
        obj.layer = 2;
        obj.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    private static Material NewMaterial(string name, Color color, float smoothness)
    {
        string path = Art + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name, color = color };
        material.SetFloat("_Smoothness", smoothness);
        if (name == "Copper sedges") material.SetFloat("_Cull", (float)CullMode.Off);
        AssetDatabase.CreateAsset(material,path);
        return material;
    }

    private static T Create<T>(string name) where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();
        asset.name = name;
        AssetDatabase.CreateAsset(asset,Settings+"/"+name+".asset");
        return asset;
    }
}
