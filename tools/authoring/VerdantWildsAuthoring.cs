using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.EditorTools;
using DiceFree.Items;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static class VerdantWildsAuthoring
{
    private const string RootName = "The Verdant Wilds - Lanternmere trail";
    private const string Art = "Assets/_DiceFree/Art/World/VerdantWilds";
    private const string Settings = "Assets/_DiceFree/Settings/VerdantWilds";
    private const string Models = "Assets/_DiceFree/Art/World/VisualBatch01/Models/";
    private const string Trees = "Assets/_DiceFree/Art/World/CornbergBatch/Trees/";
    private const string Region = "Assets/_DiceFree/Art/World/BriarbackRegion/";
    private const string Thornlash = "verdant.thornlash";
    private const string Sporecap = "verdant.sporecap";
    private const string Lanterncap = "verdant.lanterncap";
    private static readonly Vector2 Spring = new(-33, 320);
    private static readonly Vector2 Warden = new(-8, 309);
    private static readonly Vector2[] Trail =
    {
        new(6,305), new(-5,305), new(-15,305), new(-24,305), new(-35,305), new(-44,309),
        new(-49,318), new(-48,328), new(-42,335), new(-31,337), new(-23,332), new(-18,324),
        new(-17,314), new(-15,305)
    };
    private static readonly Vector2[] Thorns = { new(-24,298), new(-38,298), new(-52,307), new(-58,319), new(-48,342) };
    private static readonly Vector2[] Spores = { new(-52,331), new(-39,341), new(-26,344), new(-18,335) };
    private static readonly Vector2[] Lanterns = { new(-24,313), new(-43,320), new(-26,330), new(-10,319) };
    private static Material soil, path, rock, water, foam, leaf, thorn, red, amber, ivory, blue, brass;

    public static void Batch()
    {
        var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
        if (GameObject.Find(RootName) != null || AssetDatabase.IsValidFolder(Art) || AssetDatabase.IsValidFolder(Settings))
            throw new InvalidOperationException("Verdant Wilds already exists. Use the pre-authoring scene to regenerate it.");
        Directory.CreateDirectory(Art);
        Directory.CreateDirectory(Settings);
        AssetDatabase.Refresh();
        BuildPalette();
        var root = Group(null, RootName, Vector3.zero);
        MeshObject(root, "Rolling moss banks and spring basin", GroundMesh(), soil, 8);
        MeshObject(root, "Worn trail around Lanternmere", TrailMesh(), path);
        BuildSpring(root);
        BuildForest(root);

        var journal = Object.FindObjectsByType<QuestJournal>().Single(j => j.Definitions.Any(q => q.stableId == "quest.arborium.roadwarden"));
        var template = GameObject.Find("Adventurers Guild Registrar");
        if (template == null) throw new InvalidOperationException("Arborium guard appearance missing.");
        var tuning = AssetDatabase.LoadAssetAtPath<CombatTuning>("Assets/_DiceFree/Settings/Combat/Cornberg provisional tuning.asset");
        var nature = AssetDatabase.LoadAssetAtPath<ElementDefinition>("Assets/_DiceFree/Settings/Combat/Elements/Nature.asset");
        if (tuning == null || nature == null) throw new InvalidOperationException("Verdant combat assets missing.");
        var respawn = Asset<RespawnDefinition>("Wilds respawn");
        respawn.delaySeconds = 70;

        var lash = Species(Thornlash, "Thornlash", "Briar lash", tuning, null, 2050, 53, .9f, 4.2f, 395);
        var puff = Species(Sporecap, "Sporecap", "Spore puff", tuning, nature, 1820, 66, 4.8f, 2.6f, 430);
        var bump = Species(Lanterncap, "Lanterncap", "Cap bump", tuning, null, 1570, 42, .7f, 2.3f, 0);
        var lanternmereQuest = BuildQuest();
        var plantBounty = BuildPlantBounty(lash);
        journal.Configure(journal.Definitions.Concat(new[] { lanternmereQuest, plantBounty }).ToArray());
        EditorUtility.SetDirty(journal);
        var giver = BuildWarden(root, template, lanternmereQuest, plantBounty);
        var enemies = Group(root, "Wilds creatures", Vector3.zero);
        var actors = Thorns.Select((at,i) => Spawn(enemies, lash, 31+i, at, respawn))
            .Concat(Spores.Select((at,i) => Spawn(enemies, puff, 33+i, at, respawn)))
            .Concat(Lanterns.Select(at => Spawn(enemies, bump, 32, at, respawn))).ToArray();

        var bounds = Object.FindAnyObjectByType<OverworldMapBounds>();
        if (bounds == null) throw new InvalidOperationException("Overworld camera bounds missing.");
        var area = bounds.Bounds;
        area.xMin = Mathf.Min(area.xMin, -82);
        bounds.Configure(area);
        EditorUtility.SetDirty(bounds);
        EditorUtility.SetDirty(respawn);

        // Match the previous highway bake so the live roadblock can still carve and reopen.
        var highway = Object.FindObjectsByType<MonoBehaviour>().Single(c => c.GetType().Name == "OgreRoadwardenRoute");
        var wreckage = new SerializedObject(highway).FindProperty("wreckage");
        var closed = Enumerable.Range(0, wreckage.arraySize)
            .Select(i => (GameObject)wreckage.GetArrayElementAtIndex(i).objectReferenceValue)
            .Where(item => item.activeSelf).ToArray();
        try
        {
            foreach (var piece in closed) piece.SetActive(false);
            CornbergSceneBuilder.Rebake();
        }
        finally
        {
            foreach (var piece in closed) piece.SetActive(true);
        }

        using (var navigation = new Navigation())
        {
            var gate = Sample(new Vector3(29, .07f, 305));
            var wardenApproach = Sample(giver.ApproachPosition);
            var route = new NavMeshPath();
            RequirePath(gate, wardenApproach, route, "West Gate to Warden Lethen");
            foreach (var stop in Trail.Skip(1))
                RequirePath(wardenApproach, Sample(Ground(stop)), route, "Lanternmere trail " + stop);
            foreach (var actor in actors)
            {
                Vector3 home = Sample(actor.transform.position);
                RequirePath(wardenApproach, home, route, actor.name);
                actor.transform.position = home;
                actor.GetComponent<AggroBehaviour>().ConfigureHomePosition(home);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save Cornberg.");
        AssetDatabase.SaveAssets();
        File.WriteAllText("Logs/VerdantAuthoring.ok", $"West Gate connected, trail points {Trail.Length - 1}, creatures {actors.Length}.");
    }

    private sealed class Navigation : IDisposable
    {
        private readonly NavMeshDataInstance instance = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        public void Dispose() => instance.Remove();
    }

    private static Vector3 Sample(Vector3 point)
    {
        if (!NavMesh.SamplePosition(point, out var hit, 1.25f, 1))
            throw new InvalidOperationException("No walkable ground at " + point);
        return hit.position;
    }

    private static void RequirePath(Vector3 from, Vector3 to, NavMeshPath path, string label)
    {
        if (!NavMesh.CalculatePath(from, to, 1, path) || path.status != NavMeshPathStatus.PathComplete)
            throw new InvalidOperationException("No connected route for " + label);
    }

    private static void BuildPalette()
    {
        soil = MaterialAsset("Moss and forest litter", "7C9E74");
        soil.SetTexture("_BaseMap", MossTexture());
        path = AssetDatabase.LoadAssetAtPath<Material>(Region + "Trampled trail.mat");
        rock = MaterialAsset("Blue shale", "4E6663");
        water = MaterialAsset("Lanternmere deep water", "227D78", smoothness: .84f);
        foam = MaterialAsset("Spring shallows", "54BBB0", smoothness: .72f);
        leaf = MaterialAsset("Thorn and reed green", "42745B");
        thorn = MaterialAsset("New shoots", "B5C875");
        red = MaterialAsset("Sporecap umber", "993F66");
        amber = MaterialAsset("Lanterncap amber", "E3A749", "835020");
        ivory = MaterialAsset("Ivory gills", "E3C8AB");
        blue = MaterialAsset("Mere light", "84F4CD", "36A883");
        brass = MaterialAsset("Weathered brass", "8F7552");
    }

    private static Texture2D MossTexture()
    {
        const int size = 256;
        var pixels = new Color32[size*size];
        var dark = ColorFrom("697958");
        var bright = ColorFrom("C7D8A4");
        for (int z = 0; z < size; z++)
        for (int x = 0; x < size; x++)
        {
            float broad = Mathf.PerlinNoise(x*.027f,z*.027f);
            float flecks = Mathf.PerlinNoise(15+x*.36f,7+z*.34f);
            float grain = Mathf.PerlinNoise(44+x*.78f,55+z*.76f);
            float fade = Mathf.Clamp01(Mathf.Min(x,z,size-1-x,size-1-z)/16f);
            pixels[z*size+x] = Color.Lerp(dark,bright,Mathf.Lerp(.56f,.72f*broad+.18f*flecks+.10f*grain,fade));
        }
        var texture = new Texture2D(size,size,TextureFormat.RGBA32,true) { name="Moss and leaf litter detail", wrapMode=TextureWrapMode.Repeat };
        texture.SetPixels32(pixels);
        texture.Apply(true);
        AssetDatabase.CreateAsset(texture,Art+"/Moss and leaf litter detail.asset");
        return texture;
    }

    private static Material MaterialAsset(string name, string tint, string emission = null, float smoothness = .14f)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
        material.SetColor("_BaseColor", ColorFrom(tint));
        material.SetFloat("_Smoothness", smoothness);
        if (emission != null)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", ColorFrom(emission));
        }
        AssetDatabase.CreateAsset(material, Art + "/" + name + ".mat");
        return material;
    }

    private static Color ColorFrom(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out var color);
        return color;
    }

    private static Vector3 P(float x, float y, float z) => new(x, y, z);
    private static Vector3 Ground(Vector2 p) => P(p.x, Height(p), p.y);

    private static float Height(Vector2 p)
    {
        float approach = Mathf.Clamp01(-p.x / 16);
        float hill = .28f + .47f * Mathf.Sin((p.x + 12) * .121f) * Mathf.Cos((p.y - 311) * .118f);
        float basin = 1.65f * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(6.4f, 11.9f, PondDistance(p))));
        float rim = Mathf.Max(Mathf.InverseLerp(-62, -76, p.x),
            Mathf.Max(Mathf.InverseLerp(280,264,p.y), Mathf.InverseLerp(341,353,p.y)));
        return -.025f + approach * (hill - basin + rim * rim * 5.2f);
    }

    private static float PondDistance(Vector2 p)
    {
        var offset = p - Spring;
        return new Vector2(offset.x, offset.y * 1.12f).magnitude;
    }

    private static Mesh GroundMesh()
    {
        const int columns = 72, rows = 80;
        var verts = new Vector3[(columns+1)*(rows+1)];
        var uv = new Vector2[verts.Length];
        var triangles = new List<int>(columns*rows*6);
        for (int z = 0; z <= rows; z++)
        {
            float worldZ = Mathf.Lerp(264, 353, z / (float)rows);
            float opening = 1 - Mathf.SmoothStep(0,1,Mathf.InverseLerp(9,22,Mathf.Abs(worldZ-305)));
            float east = Mathf.Lerp(-7, 7, opening);
            for (int x = 0; x <= columns; x++)
            {
                float worldX = Mathf.Lerp(-77, east, x / (float)columns);
                int i = z*(columns+1)+x;
                verts[i] = Ground(new Vector2(worldX,worldZ));
                uv[i] = new Vector2(worldX,worldZ)*.105f;
                if (x < columns && z < rows) Quad(triangles, i, i+columns+1, i+1, i+columns+2);
            }
        }
        return BakeMesh("Lanternmere terrain", verts, uv, triangles);
    }

    private static Mesh TrailMesh()
    {
        var centers = new List<Vector2>();
        for (int i = 0; i < Trail.Length-1; i++)
        {
            int steps = Mathf.CeilToInt(Vector2.Distance(Trail[i],Trail[i+1]) / .8f);
            for (int n = 0; n < steps; n++) centers.Add(Vector2.Lerp(Trail[i],Trail[i+1],n/(float)steps));
        }
        centers.Add(Trail[^1]);
        const int cross = 6;
        var verts = new Vector3[centers.Count*(cross+1)];
        var uv = new Vector2[verts.Length];
        var triangles = new List<int>();
        float run = 0;
        for (int j = 0; j < centers.Count; j++)
        {
            var direction = (centers[Mathf.Min(j+1,centers.Count-1)]-centers[Mathf.Max(j-1,0)]).normalized;
            var normal = new Vector2(-direction.y,direction.x);
            if (j > 0) run += Vector2.Distance(centers[j-1],centers[j]);
            float width = 2.15f + .30f * Mathf.Sin(run*.74f);
            for (int i = 0; i <= cross; i++)
            {
                float t = i/(float)cross;
                var vertex = Ground(centers[j] + normal*((t*2-1)*width));
                int index = j*(cross+1)+i;
                verts[index] = vertex + Vector3.up*.045f;
                uv[index] = new Vector2(t,run*.21f);
                if (i < cross && j < centers.Count-1)
                    Quad(triangles,index,index+1,index+cross+1,index+cross+2);
            }
        }
        return BakeMesh("Lanternmere trail ribbon",verts,uv,triangles);
    }

    private static void Quad(List<int> triangles,int a,int b,int c,int d)
    {
        triangles.Add(a); triangles.Add(b); triangles.Add(c);
        triangles.Add(c); triangles.Add(b); triangles.Add(d);
    }

    private static Mesh BakeMesh(string name, Vector3[] vertices, Vector2[] uv, List<int> indices)
    {
        var mesh = new Mesh { name = name, vertices = vertices, uv = uv };
        mesh.SetTriangles(indices,0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Art+"/"+name+".asset");
        return mesh;
    }

    private static Transform Group(Transform parent,string name,Vector3 at)
    {
        var child = new GameObject(name).transform;
        child.SetParent(parent,false);
        child.localPosition = at;
        return child;
    }

    private static void MeshObject(Transform parent,string name,Mesh mesh,Material material,int layer=0)
    {
        var node = Group(parent,name,Vector3.zero).gameObject;
        node.layer = layer;
        node.AddComponent<MeshFilter>().sharedMesh = mesh;
        node.AddComponent<MeshRenderer>().sharedMaterial = material;
        if (layer != 8) return;
        using var meshSettings = new SerializedObject(mesh);
        var triangleCollision = meshSettings.FindProperty("m_PreBakeTriangleCollisionMesh");
        if (triangleCollision != null)
        {
            triangleCollision.boolValue = true;
            meshSettings.ApplyModifiedPropertiesWithoutUndo();
        }
        node.AddComponent<MeshCollider>().sharedMesh = mesh;
    }

    private static Transform Shape(Transform parent,string name,PrimitiveType type,Vector3 point,Vector3 size,Material material)
    {
        var obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent,false);
        obj.transform.localPosition = point;
        obj.transform.localScale = size;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(obj.GetComponent<Collider>());
        return obj.transform;
    }

    private static Transform Model(Transform parent,string path,string label,Vector3 at,float height,float angle,Material[] paint=null)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (source == null) throw new InvalidOperationException("Missing scenery: " + path);
        var pivot = Group(parent,label,at);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(source,pivot);
        foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
        {
            // Keep prefab links while excluding visual-only model colliders from world picking and baking.
            collider.enabled = false;
        }
        var renderers = instance.GetComponentsInChildren<MeshRenderer>();
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        if (bounds.size.y < .01f) throw new InvalidOperationException("Degenerate scenery model: " + path);
        float scale = height / bounds.size.y;
        instance.transform.localPosition = (at - new Vector3(bounds.center.x,bounds.min.y,bounds.center.z))*scale;
        instance.transform.localScale *= scale;
        pivot.localRotation = Quaternion.Euler(0,angle,0);
        if (paint != null)
            foreach (var renderer in renderers)
            {
                var finish = renderer.name.Contains("stem",StringComparison.OrdinalIgnoreCase) ? paint[1] : paint[0];
                renderer.sharedMaterials = renderer.sharedMaterials.Select(_ => finish).ToArray();
            }
        return pivot;
    }

    private static void Trunk(Transform tree,float radius,float height)
    {
        var solid = Group(tree,"Solid trunk",Vector3.up*(height*.5f)).gameObject;
        solid.layer = 9;
        var collision = solid.AddComponent<CapsuleCollider>();
        collision.radius = radius;
        collision.height = height;
    }

    private static void BuildSpring(Transform root)
    {
        var mere = Group(root,"Lanternmere - luminous spring",P(Spring.x, -.21f, Spring.y));
        var shallows = Shape(mere,"Jade shallows",PrimitiveType.Cylinder,Vector3.zero,P(19.8f,.035f,17.7f),foam);
        shallows.gameObject.layer = 9;
        shallows.gameObject.AddComponent<MeshCollider>().sharedMesh = shallows.GetComponent<MeshFilter>().sharedMesh;
        Shape(mere,"Dark reflective water",PrimitiveType.Cylinder,P(.45f,.045f,-.2f),P(17.4f,.018f,15.25f),water);
        Shoreline(root);
        for (int i=0; i<17; i++)
        {
            float a = i*2.39996f;
            float radius = 4.8f + (i%4)*1.1f;
            var at = P(Mathf.Sin(a)*radius,.083f,Mathf.Cos(a)*radius*.86f);
            Shape(mere,"Floating lily pad",PrimitiveType.Sphere,at,P(.53f+(i%3)*.19f,.026f,.42f),leaf);
            if (i%3 == 0)
            {
                for (int j=0; j<5; j++)
                {
                    float petal = j*72*Mathf.Deg2Rad;
                    var shape = Shape(mere,"Lily petal",PrimitiveType.Sphere,
                        at+P(Mathf.Sin(petal)*.11f,.075f,Mathf.Cos(petal)*.11f),P(.13f,.05f,.23f),ivory);
                    shape.localRotation = Quaternion.Euler(0,j*72,0);
                }
                Shape(mere,"Lily lantern",PrimitiveType.Sphere,at+Vector3.up*.109f,Vector3.one*.11f,blue);
            }
        }
        var willow = Model(root,Trees+"Tree_Willow.fbx","Lanternmere willow",Ground(new Vector2(-33,329)),12.3f,22);
        Trunk(willow,1.06f,6);
        for (int i = 0; i < 7; i++)
        {
            float angle = i*137.5f;
            var position = willow.localPosition + Quaternion.Euler(0,angle,0)*P(2.4f,1.4f+i*.39f,0);
            Shape(root,"Willow hanging wisp",PrimitiveType.Sphere,position,Vector3.one*.21f,blue);
        }
    }

    private static void Shoreline(Transform root)
    {
        for (int bank=0; bank<5; bank++)
        {
            float angle = (37+bank*71)*Mathf.Deg2Rad;
            for (int stone=0; stone<4; stone++)
            {
                float arc = angle + .10f*(stone-1.5f);
                var at = Spring+new Vector2(Mathf.Sin(arc)*9.85f,Mathf.Cos(arc)*8.75f);
                var boulder = Model(root,Models+"rock_0"+(stone+1)+".fbx","Spring bank shale",Ground(at),
                    .72f+.32f*Mathf.Sin(stone*2.1f+bank),bank*73+stone*21,new[]{rock,rock});
                Fern(root,boulder.position+P(Mathf.Sin(arc)*.7f,0,Mathf.Cos(arc)*.7f),.78f,bank*83);
                var reed = at + new Vector2(Mathf.Sin(arc)*.87f,Mathf.Cos(arc)*.77f);
                for (int i=0; i<4; i++)
                {
                    var point = Ground(reed+new Vector2((i-1.5f)*.13f,Mathf.Sin(i*1.4f)*.15f));
                    var frond = Shape(root,"Spring rush",PrimitiveType.Sphere,point+Vector3.up*(.34f+i*.064f),
                        P(.055f,.72f+i*.12f,.10f),i%2==0?leaf:thorn);
                    frond.localRotation=Quaternion.Euler(14+i*7,bank*71,(i-1.5f)*10);
                }
            }
        }
        foreach (var cap in new[]{(new Vector2(-43,326),2.35f),(new Vector2(-38,330),2.60f),(new Vector2(-31,330),2.1f)})
            Model(root,Models+"mushroom_03.fbx","Old lantern fungi",Ground(cap.Item1),cap.Item2,72,new[]{amber,ivory});
    }

    private static void BuildForest(Transform root)
    {
        var forest = Group(root,"Willow, ferns and fungal shelves",Vector3.zero);
        var random = new System.Random(42719);
        float Range(float min,float max) => Mathf.Lerp(min,max,(float)random.NextDouble());
        var avoid = Thorns.Concat(Spores).Concat(Lanterns).Append(Warden).ToArray();
        bool Clear(Vector2 at,float clearance) =>
            Trail.Zip(Trail.Skip(1),(a,b) => SegmentDistance(at,a,b)).Min() > clearance &&
            avoid.All(home => Vector2.Distance(at,home)>clearance) && PondDistance(at)>11.2f;
        var planted = new List<Vector2>();
        for (int i=0; i<102; i++)
        {
            var at = new Vector2(Range(-75,-9),Range(267,350));
            if (!Clear(at,4.4f) || planted.Any(other => Vector2.Distance(other,at)<4.5f)) continue;
            planted.Add(at);
            var type = i%4==0 ? "Willow" : i%3==0 ? "Twisted" : i%3==1 ? "Maple" : "Oak";
            float height = Range(4.8f,9.3f);
            var tree = Model(forest,Trees+"Tree_"+type+".fbx",type,Ground(at),height,Range(0,360));
            Trunk(tree,Mathf.Lerp(.34f,.64f,height/9),height*.48f);
        }
        for (int i=0; i<125; i++)
        {
            var at = new Vector2(Range(-74,-8),Range(269,348));
            if (!Clear(at,1.55f)) continue;
            var point = Ground(at);
            if (i%4 == 0)
                Model(forest,Models+"mushroom_0"+(i%3+1)+".fbx","Bank mushrooms",point,Range(.42f,1.40f),
                    Range(0,360),new[]{i%3==0?amber:red,ivory});
            else if (i%7 == 0)
                Model(forest,Models+"rock_0"+(i%4+1)+".fbx","Mossy shale",point,Range(.45f,1.2f),Range(0,360),new[]{rock,rock});
            else
                Fern(forest,point,Range(.46f,1.0f),Range(0,360));
        }
        foreach (var at in new[]{new Vector2(-10,302),new Vector2(-23,307),new Vector2(-43,306),
            new Vector2(-51,322),new Vector2(-40,336),new Vector2(-19,327)})
            Waylight(forest,Ground(at));
    }

    private static float SegmentDistance(Vector2 point,Vector2 a,Vector2 b)
    {
        var line = b-a;
        return Vector2.Distance(point,a+line*Mathf.Clamp01(Vector2.Dot(point-a,line)/line.sqrMagnitude));
    }

    private static void Fern(Transform parent,Vector3 at,float scale,float turn)
    {
        var plant = Group(parent,"Fern fronds",at);
        plant.localRotation = Quaternion.Euler(0,turn,0);
        for (int i=0; i<5; i++)
        {
            var ray = Quaternion.Euler(0,i*72,0);
            var part = Shape(plant,"Frond",PrimitiveType.Sphere,ray*P(0,.25f,.34f)*scale,
                P(.16f,.072f,.96f)*scale,i%3==0?thorn:leaf);
            part.localRotation = ray*Quaternion.Euler(-32,0,0);
        }
    }

    private static void Waylight(Transform parent, Vector3 position)
    {
        var fixture = Group(parent,"Lanternmere waylight",position);
        Shape(fixture,"Stone footing",PrimitiveType.Cylinder,P(0,.09f,0),P(.50f,.09f,.50f),rock);
        Shape(fixture,"Bronze stem",PrimitiveType.Cylinder,P(0,.66f,0),P(.086f,.58f,.086f),brass);
        Shape(fixture,"Glowing glass",PrimitiveType.Sphere,P(0,1.26f,0),P(.29f,.34f,.29f),blue);
        Shape(fixture,"Lantern crown",PrimitiveType.Cylinder,P(0,1.44f,0),P(.35f,.048f,.35f),brass);
        var light = fixture.gameObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = ColorFrom("A3FFCF");
        light.range = 5;
        light.intensity = 1.9f;
        light.shadows = LightShadows.None;
        light.cullingMask = ~(1<<31);
    }

    private static T Asset<T>(string name) where T : ScriptableObject
    {
        var value = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(value, Settings+"/"+name+".asset");
        return value;
    }

    private static QuestDefinition BuildQuest()
    {
        var reward = Asset<FixedRewardDefinition>("Lanternmere reward");
        reward.gold = 110;
        var quest = Asset<QuestDefinition>("Keep the Lanterns Burning");
        quest.stableId = "quest.verdant.lanternmere";
        quest.title = "Keep the Lanterns Burning";
        quest.requiredLevel = 30;
        quest.offer = "I tend the lamps around Lanternmere. Thornlashes have crept onto the lower bank, and Sporecaps are puffing into the upper trail. Thin them out so I can make the circuit. The amber Lanterncaps leave us alone if we leave them alone.";
        quest.activeDialogue = "Follow the lamplit trail around the spring. Clear three Thornlashes and two Sporecaps, then come back to me.";
        quest.locationHint = "Leave Arborium's West Gate and follow the lanterns to the forest trailhead.";
        quest.turnInHint = "Return to Warden Lethen at the Lanternmere trailhead.";
        quest.rewardXp = 2800;
        quest.reward = reward;
        var targets = new[]{Kill(Thornlash,3,"Defeat 3 Thornlashes"),Kill(Sporecap,2,"Defeat 2 Sporecaps")};
        quest.stages = new[]
        {
            new QuestObjective { kind=ObjectiveKind.All, count=targets.Sum(target => target.count),
                instruction="Clear the Lanternmere trail", alternatives=targets }
        };
        EditorUtility.SetDirty(quest);
        EditorUtility.SetDirty(reward);
        return quest;
    }

    private static QuestDefinition BuildPlantBounty(ActorDefinition thornlash)
    {
        var quest = Asset<QuestDefinition>("Another 15 Plant Monsters");
        quest.stableId = "quest.verdant.repeat-plant-hunt";
        quest.title = "Another 15 Plant Monsters";
        quest.requiredLevel = 30;
        quest.offer = "The Thornlashes keep creeping back onto the trail. Defeat fifteen plant monsters, then return to me. The bounty pays the experience of twenty Thornlashes.";
        quest.activeDialogue = "Clear fifteen plant monsters and return for your bounty. Thornlashes count toward the hunt.";
        quest.locationHint = "Warden Lethen stands at the Lanternmere trailhead beyond Arborium's West Gate.";
        quest.turnInHint = "Return to Warden Lethen at the Lanternmere trailhead.";
        quest.stages = new[]
        {
            new QuestObjective { kind = ObjectiveKind.Kill, requiredTag = "plant", count = 15,
                instruction = "Defeat 15 plant monsters", creditMode = QuestCreditMode.NearbyParty }
        };
        quest.rewardXp = 20 * thornlash.experienceReward;
        quest.repeatable = true;
        EditorUtility.SetDirty(quest);
        return quest;
    }

    private static QuestLeaf Kill(string species,int count,string instruction) =>
        new() { kind=ObjectiveKind.Kill, objectiveId=species, familyId=species, count=count,
            instruction=instruction, creditMode=QuestCreditMode.NearbyParty };

    private static QuestGiver BuildWarden(Transform root,GameObject template,QuestDefinition quest,QuestDefinition plantBounty)
    {
        var npc = Object.Instantiate(template,root);
        npc.name = "Warden Lethen - Lanternmere trailhead";
        npc.transform.SetPositionAndRotation(Ground(Warden),Quaternion.Euler(0,163,0));
        if (PrefabUtility.IsPartOfPrefabInstance(npc))
            PrefabUtility.UnpackPrefabInstance(npc,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        foreach (var old in npc.GetComponentsInChildren<InteractionTarget>(true)) Object.DestroyImmediate(old);
        foreach (var old in npc.GetComponentsInChildren<QuestWorldMarker>(true)) Object.DestroyImmediate(old);
        var giver = npc.AddComponent<QuestGiver>();
        giver.ConfigureName("Warden Lethen");
        giver.Configure(quest);
        giver.ConfigureAdditional(plantBounty);
        giver.ConfigureApproach(Group(root,"Warden Lethen interaction approach",Ground(Warden+new Vector2(0,-1.7f))));
        return giver;
    }

    private static ActorDefinition Species(string id,string label,string attackLabel,CombatTuning tuning,
        ElementDefinition element,float health,float damage,float reach,float speed,int xp)
    {
        var attack = Asset<AttackDefinition>(attackLabel);
        attack.stableId = "attack."+id;
        attack.displayName = attackLabel;
        attack.channel = element != null ? DamageChannel.Magical : DamageChannel.Physical;
        attack.element = element;
        attack.requiresAccuracy = element == null;
        attack.baseDamage = damage;
        attack.coefficient = 0;
        attack.interval = element != null ? 2.25f : 1.55f;
        attack.windup = element != null ? .85f : .37f;
        attack.reach = reach;

        var actor = Asset<ActorDefinition>(label);
        actor.stableId = "enemy."+id;
        actor.displayName = label;
        actor.familyId = id;
        actor.tags = new[]{ "verdant-wilds", id == Thornlash ? "plant" : "fungus" };
        actor.tuning = tuning;
        actor.baseAttributes = new AttributeValues { strength=9, agility=10, intelligence=9, vitality=8 };
        actor.growth = default;
        actor.baseHp = health;
        actor.overrideVitalityHp = actor.overrideVitalityRegeneration = true;
        actor.hpPerVitality = actor.regenerationPerVitality = 0;
        actor.physicalDefense = id == Thornlash ? 19 : 10;
        actor.magicalDefense = id == Sporecap ? 18 : 9;
        actor.moveSpeed = speed;
        actor.basicAttack = attack;
        actor.experienceReward = xp;
        EditorUtility.SetDirty(attack);
        EditorUtility.SetDirty(actor);
        return actor;
    }

    private static GameObject Spawn(Transform parent,ActorDefinition definition,int level,Vector2 at,RespawnDefinition respawn)
    {
        bool plant = definition.familyId == Thornlash;
        bool passive = definition.familyId == Lanterncap;
        var unit = Group(parent,definition.displayName+" (Lv "+level+")",Ground(at));
        var obj = unit.gameObject;
        obj.layer = 10;
        var agent = obj.AddComponent<NavMeshAgent>();
        agent.enabled = false;
        agent.areaMask = 1;
        agent.height = plant ? 2.0f : 1.7f;
        var hitbox = obj.AddComponent<CapsuleCollider>();
        hitbox.height = agent.height;
        hitbox.center = Vector3.up*(agent.height*.5f);
        hitbox.radius = .47f;
        obj.AddComponent<ActorStats>().Configure(definition,level);
        obj.AddComponent<CombatActor>().Configure(1,.47f);
        obj.AddComponent<CombatStatusController>();
        obj.AddComponent<BasicAttack>();
        obj.AddComponent<DefeatReporter>();
        obj.AddComponent<OverworldRespawn>().Configure(respawn);
        var aggro = obj.AddComponent<AggroBehaviour>();
        aggro.Configure(plant ? 7.0f : 8.0f, passive ? 12 : 17);
        aggro.ConfigureAutoAggro(new AutoAggroPolicy { retaliationOnly = passive });
        var form = Group(unit,"Creature body",Vector3.zero);
        if (plant) ThornBody(form);
        else FungusBody(form,passive);
        obj.AddComponent<ActorFeedback>().Configure(form);
        return obj;
    }

    private static void ThornBody(Transform form)
    {
        Shape(form,"Gnarled root torso",PrimitiveType.Capsule,P(0,.79f,0),P(.51f,.72f,.49f),leaf);
        Shape(form,"Thorn seed head",PrimitiveType.Sphere,P(0,1.54f,.14f),P(.70f,.57f,.62f),leaf);
        for (int i=0; i<6; i++)
        {
            var spin = Quaternion.Euler(0,i*60,0);
            var petal = Shape(form,"Raised barb",PrimitiveType.Sphere,P(0,1.52f,.14f)+spin*P(0,.24f,.33f),
                P(.15f,.12f,.53f),i%2==0?thorn:leaf);
            petal.localRotation = spin*Quaternion.Euler(-35,0,0);
            var root = Shape(form,"Walking root",PrimitiveType.Capsule,spin*P(0,.25f,.33f),P(.16f,.45f,.16f),leaf);
            root.localRotation = spin*Quaternion.Euler(57,0,0);
        }
        for (int side=-1; side<=1; side+=2)
        {
            var arm = Shape(form,"Lashing thorn arm",PrimitiveType.Capsule,P(side*.43f,1.02f,.17f),P(.19f,.54f,.22f),leaf);
            arm.localRotation = Quaternion.Euler(18,0,side*-43);
            Shape(form,"Thorn tip",PrimitiveType.Sphere,P(side*.69f,.72f,.35f),P(.13f,.24f,.19f),thorn);
            Shape(form,"Sap eye",PrimitiveType.Sphere,P(side*.13f,1.55f,.429f),P(.11f,.089f,.085f),amber);
        }
    }

    private static void FungusBody(Transform form,bool passive)
    {
        var cap = passive ? amber : red;
        Shape(form,"Fungus stem",PrimitiveType.Capsule,P(0,.68f,0),P(.53f,.64f,.51f),ivory);
        Shape(form,"Cream gill rim",PrimitiveType.Sphere,P(0,1.20f,0),P(1.46f,.25f,1.26f),ivory);
        Shape(form,"Fungus canopy",PrimitiveType.Sphere,P(0,1.34f,0),P(1.51f,passive?.55f:.66f,1.29f),cap);
        for (int i=0; i<9; i++)
        {
            float angle = i*2.39996f;
            float ring = .26f + (i%3)*.13f;
            Shape(form,"Cap spots",PrimitiveType.Sphere,P(Mathf.Sin(angle)*ring,1.60f-(i%3)*.035f,Mathf.Cos(angle)*ring*.86f),
                P(.16f,.069f,.14f),passive?blue:ivory);
        }
        for (int side=-1; side<=1; side+=2)
        {
            Shape(form,"Fungal foot",PrimitiveType.Sphere,P(side*.24f,.19f,.17f),P(.33f,.29f,.54f),ivory);
            Shape(form,"Eyes beneath the cap",PrimitiveType.Sphere,P(side*.106f,.92f,.245f),Vector3.one*.085f,passive?brass:amber);
        }
    }
}
