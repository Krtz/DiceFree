using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Core;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Original scene artwork. Run through Unity Pipeline; no third-party source assets.
public static class BriarbackGrounding
{
    const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
    const string Folder = "Assets/_DiceFree/Art/World/BriarbackGrounding";
    const string RootName = "Elemental clearing ground and trail dressing";
    const string Recovery = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-clearing-ground-20261104.unity";
    const float HalfSize = 20;
    const int Resolution = 1024;
    static readonly string[] Elements = { "Fire", "Earth", "Lightning", "Ice" };

    static Scene Scene()
    {
        var scene = SceneManager.GetActiveScene();
        Need(!EditorApplication.isPlayingOrWillChangePlaymode && scene.path == ScenePath && !scene.isDirty,
            "Open clean Cornberg in Edit Mode first.");
        return scene;
    }

    static Transform[] Sites(Scene scene) => Elements.Select(e => scene.GetRootGameObjects()
        .SelectMany(o => o.GetComponentsInChildren<Transform>(true))
        .Single(t => t.name == e + " Shaman settlement - provisional")).ToArray();

    static void Need(bool ok, string error) { if (!ok) throw new InvalidOperationException(error); }
    static float Smooth(float low, float high, float value) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(low, high, value));
    static float Noise(float x, float z, float scale, float seed) => Mathf.PerlinNoise(x * scale + seed, z * scale + seed * .731f);

    public static object Author()
    {
        var scene = Scene();
        var sites = Sites(scene);
        Need(!File.Exists(Recovery) && !AssetDatabase.IsValidFolder(Folder), "Grounding art or recovery already exists.");
        Need(sites.All(t => t.Find(RootName) == null), "Existing grounding group would be overwritten.");
        Need(Shader.Find("Universal Render Pipeline/Lit") != null, "URP Lit shader missing.");
        var roads = Roads(scene);
        Need(roads.Count > 20, "Expected the existing northern approach and detour meshes.");
        Need(EditorSceneManager.SaveScene(scene, Recovery, true), "Could not save recovery scene.");
        AssetDatabase.CreateFolder("Assets/_DiceFree/Art/World", "BriarbackGrounding");
        for (int i = 0; i < sites.Length; i++)
        {
            AssetDatabase.CreateFolder(Folder, Elements[i]);
            Build(sites[i], i, roads);
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        Need(EditorSceneManager.SaveScene(scene), "Could not save clearing ground art.");
        return Validate();
    }

    // Rebuilds only this tool's dedicated artwork, preserving asset GUIDs and the recovery copy.
    // Useful for visual iteration. Refuses objects outside the explicitly owned hierarchy/folder.
    public static object Rebuild()
    {
        var scene = Scene();
        Need(File.Exists(Recovery) && AssetDatabase.IsValidFolder(Folder), "Author the first pass before rebuilding.");
        var sites = Sites(scene);
        foreach (var site in sites)
        {
            var root = site.Find(RootName);
            Need(root != null && root.GetComponentsInChildren<Collider>(true).Length == 0 &&
                root.GetComponentsInChildren<MonoBehaviour>(true).Length == 0, "Ground art contains unexpected components.");
            Need(root.GetComponentsInChildren<MeshFilter>().All(f => AssetDatabase.GetAssetPath(f.sharedMesh)
                .StartsWith(Folder + "/", StringComparison.Ordinal)), "Foreign mesh in owned grounding group.");
        }
        var roads = Roads(scene);
        for (int i = 0; i < sites.Length; i++)
        {
            Object.DestroyImmediate(sites[i].Find(RootName).gameObject);
            Build(sites[i], i, roads);
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        Need(EditorSceneManager.SaveScene(scene), "Could not save clearing rebuild.");
        return Validate();
    }

    sealed class Road
    {
        public Vector2 a, b;
        public float radius;
        public Color color;
        public float Distance(Vector2 p)
        {
            var delta = b - a;
            var t = Mathf.Clamp01(Vector2.Dot(p - a, delta) / delta.sqrMagnitude);
            return Vector2.Distance(p, a + delta * t) - radius;
        }
    }

    static List<Road> Roads(Scene scene)
    {
        var result = new List<Road>();
        var names = new[] { "Briarback northern detour", "Briarback circular warband road", "North approach to the open stronghold gate" };
        foreach (var filter in scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<MeshFilter>())
            .Where(f => names.Contains(f.name)))
        {
            var v = filter.sharedMesh.vertices.Select(p => filter.transform.TransformPoint(p)).ToArray();
            Need(v.Length % 4 == 0, "Unexpected road quad layout: " + filter.name);
            for (int j = 0; j < v.Length; j += 4)
            {
                Vector2 Flat(Vector3 p) => new(p.x, p.z);
                var a = (Flat(v[j]) + Flat(v[j + 1])) * .5f;
                var b = (Flat(v[j + 2]) + Flat(v[j + 3])) * .5f;
                if ((a - b).sqrMagnitude > .0001f)
                    result.Add(new Road { a = a, b = b, radius = Vector3.Distance(v[j], v[j + 1]) * .5f,
                        color=filter.GetComponent<Renderer>().sharedMaterial.GetColor("_BaseColor") });
            }
        }
        return result;
    }

    static float RoadDistance(Vector2 p, List<Road> roads) => roads.Min(r => r.Distance(p));

    // Same authored height function as CornbergLandscape, with vertices sampled at 0.5m.
    // No terrain/collision/NavMesh asset is edited.
    static float Height(float x, float z) => .22f * Mathf.Sin(x * .09f) * Mathf.Sin(z * .08f);
    static Vector3 At(Transform site, float x, float z, float above = .018f)
    {
        var world = site.position + new Vector3(x, 0, z);
        return new Vector3(x, Height(world.x, world.z) - site.position.y + above, z);
    }

    static string SiteFolder(int element) => Folder + "/" + Elements[element];
    static void SaveAsset(Object source, string path)
    {
        var previous = AssetDatabase.LoadMainAssetAtPath(path);
        if (previous == null) AssetDatabase.CreateAsset(source, path);
        else
        {
            Need(previous.GetType() == source.GetType(), "Unexpected asset type at " + path);
            EditorUtility.CopySerialized(source, previous);
            EditorUtility.SetDirty(previous);
            Object.DestroyImmediate(source);
        }
    }

    static void Build(Transform site, int element, List<Road> roads)
    {
        var root = new GameObject(RootName).transform;
        root.SetParent(site, false);
        root.gameObject.layer = 2; // Ignore Raycast: dressing cannot intercept player targeting.
        var path = SiteFolder(element);
        var nearbyRoads = roads.Where(r => Vector2.Distance(new Vector2(site.position.x, site.position.z), (r.a + r.b) * .5f) < 50).ToList();
        var texture = MakeTexture(site, element, nearbyRoads);
        var pngPath = path + "/Clearing soil.png";
        File.WriteAllBytes(pngPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.mipmapEnabled = true;
        importer.filterMode = FilterMode.Trilinear;
        importer.maxTextureSize = Resolution;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.anisoLevel = 4;
        importer.SaveAndReimport();
        var mat = Lit("Weathered clearing soil", Color.white);
        mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath));
        mat.SetFloat("_Surface", 1);
        mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.renderQueue = (int)RenderQueue.Transparent - 10;
        mat.SetShaderPassEnabled("ShadowCaster", false);
        SaveAsset(mat, path + "/Weathered clearing soil.mat");
        SaveAsset(GroundMesh(site, nearbyRoads), path + "/Clearing surface.asset");
        Install(root, "Weathered clearing surface", path + "/Clearing surface.asset", path + "/Weathered clearing soil.mat", false);

        var detail = new Detail(site, element, nearbyRoads);
        detail.Build();
        string[] labels = { "Meadow grass", "Dry grass", "Fern leaves", "Weathered stone", "Pale stone faces", "Fallen timber" };
        Color[][] colors = {
            new[] { new Color(.27f,.30f,.12f), new Color(.47f,.38f,.19f), new Color(.25f,.32f,.15f), new Color(.25f,.23f,.20f), new Color(.39f,.36f,.29f), new Color(.22f,.16f,.10f) },
            new[] { new Color(.29f,.36f,.14f), new Color(.50f,.41f,.23f), new Color(.22f,.36f,.16f), new Color(.35f,.30f,.21f), new Color(.48f,.40f,.27f), new Color(.25f,.18f,.10f) },
            new[] { new Color(.29f,.33f,.18f), new Color(.46f,.42f,.29f), new Color(.26f,.32f,.19f), new Color(.28f,.29f,.27f), new Color(.43f,.43f,.37f), new Color(.20f,.17f,.13f) },
            new[] { new Color(.32f,.39f,.28f), new Color(.51f,.55f,.40f), new Color(.24f,.37f,.29f), new Color(.32f,.40f,.39f), new Color(.53f,.62f,.59f), new Color(.25f,.23f,.18f) }
        };
        for (int j = 0; j < labels.Length; j++)
        {
            var name = labels[j];
            SaveAsset(Lit(name, colors[element][j]), path + "/" + name + ".mat");
            SaveAsset(detail.surfaces[j].Bake(name), path + "/" + name + ".asset");
            Install(root, name, path + "/" + name + ".asset", path + "/" + name + ".mat", j >= 3);
        }
    }

    static Material Lit(string name, Color color)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", .06f);
        mat.SetFloat("_SpecularHighlights", 0);
        mat.SetFloat("_EnvironmentReflections", 0);
        return mat;
    }

    static void Install(Transform root, string name, string mesh, string mat, bool shadows)
    {
        var go = new GameObject(name);
        go.layer = 2;
        go.transform.SetParent(root, false);
        go.AddComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(mesh);
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(mat);
        renderer.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        renderer.receiveShadows = true;
    }

    static Texture2D MakeTexture(Transform site, int element, List<Road> roads)
    {
        var texture = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, true) { name = Elements[element] + " eroded earth" };
        var pixels = new Color32[Resolution * Resolution];
        Color[] soil = { new(.28f,.235f,.17f), new(.40f,.32f,.20f), new(.32f,.30f,.24f), new(.35f,.40f,.36f) };
        Color[] mineral = { new(.19f,.165f,.135f), new(.50f,.40f,.25f), new(.26f,.27f,.25f), new(.60f,.68f,.62f) };
        var seed = 181.7f + element * 39.11f;
        // Road masks change on meter scales. Sample once at 0.3125m rather than
        // searching every source segment for each of the million texture pixels.
        const int fieldCells = 128;
        var roadField = new float[fieldCells + 1, fieldCells + 1];
        var roadColor = new Color[fieldCells + 1, fieldCells + 1];
        for (int fz=0;fz<=fieldCells;fz++)
        for (int fx=0;fx<=fieldCells;fx++)
        {
            var point=new Vector2(site.position.x-HalfSize+fx*40f/fieldCells,site.position.z-HalfSize+fz*40f/fieldCells);
            var closest=roads.OrderBy(r=>r.Distance(point)).First();
            roadField[fx,fz]=closest.Distance(point);
            roadColor[fx,fz]=closest.color;
        }
        float SampleRoad(float u,float v)
        {
            float gx=u*fieldCells,gz=v*fieldCells;
            int ix=Math.Min((int)gx,fieldCells-1),iz=Math.Min((int)gz,fieldCells-1);
            return Mathf.Lerp(Mathf.Lerp(roadField[ix,iz],roadField[ix+1,iz],gx-ix),
                Mathf.Lerp(roadField[ix,iz+1],roadField[ix+1,iz+1],gx-ix),gz-iz);
        }
        Color SampleRoadColor(float u,float v)
        {
            float gx=u*fieldCells,gz=v*fieldCells;
            int ix=Math.Min((int)gx,fieldCells-1),iz=Math.Min((int)gz,fieldCells-1);
            return Color.Lerp(Color.Lerp(roadColor[ix,iz],roadColor[ix+1,iz],gx-ix),
                Color.Lerp(roadColor[ix,iz+1],roadColor[ix+1,iz+1],gx-ix),gz-iz);
        }
        for (int row = 0; row < Resolution; row++)
        for (int col = 0; col < Resolution; col++)
        {
            float x = (col / (Resolution - 1f) * 2 - 1) * HalfSize;
            float z = (row / (Resolution - 1f) * 2 - 1) * HalfSize;
            float wx = site.position.x + x, wz = site.position.z + z;
            var n = Noise(wx, wz, .35f, seed);
            var broad = Noise(wx, wz, .12f, seed + 5);
            var grain = Noise(wx, wz, 9, seed + 17);
            var radius = Mathf.Sqrt(x * x / 1.1f + z * z);
            var edge = radius + (broad - .5f) * 5.5f + (n - .5f) * 1.8f;
            var clearing = 1 - Smooth(6.5f, 11.8f, edge);
            var scuff = (1 - Smooth(1.15f, 2.9f, Mathf.Abs(x + Mathf.Sin(z * .3f) * .23f))) *
                Smooth(-17, -12, z) * (1 - Smooth(-4, -2, z));
            var roadDistance = SampleRoad(col/(Resolution-1f),row/(Resolution-1f));
            var shoulder = (1 - Smooth(-.15f, 2.5f, roadDistance + (n - .5f) * .75f));
            var areaFade = 1 - Smooth(16, 19.8f, radius + (broad - .5f) * 2);
            var scatter = Smooth(.42f, .73f, broad) * (1 - Smooth(10, 17, radius)) * .37f;
            float alpha = Mathf.Max(clearing, scuff * .85f, shoulder * .93f, scatter) * areaFade;
            var color = soil[element] * (.84f + n * .21f + grain * .12f);
            var trail = SampleRoadColor(col/(Resolution-1f),row/(Resolution-1f)) * (.88f + broad * .13f + grain * .07f);
            // The route remains a light, quiet strip. The sacred center is worn soil, not a road stripe.
            color = Color.Lerp(color, trail, shoulder * (1 - clearing * .88f));
            float speckle = Noise(wx, wz, 3.7f, seed + 83);
            var grit = Smooth(.66f,.79f,speckle) * (.3f + grain * .4f);
            color = Color.Lerp(color, mineral[element], grit * (.2f + clearing*.8f));
            if (element == 0)
                color = Color.Lerp(color, mineral[0], (1 - Smooth(2.0f, 6.8f, radius + (n-.5f)*2)) * .35f);
            if (element == 3)
            {
                float frost = Smooth(.48f,.72f,Noise(wx,wz,.65f,seed+29)) * (1 - Smooth(6.5f,11,edge));
                color = Color.Lerp(color, mineral[3], frost * .65f);
            }
            // Irregular surface cracks; subtle enough that they do not imply combat hazards.
            var seam = Mathf.Abs(Noise(wx,wz,1.7f,seed+12)-.5f);
            color *= 1 - (1-Smooth(.003f,.022f,seam)) * clearing * .09f;
            color.a = alpha;
            pixels[row * Resolution + col] = color;
        }
        texture.SetPixels32(pixels);
        texture.Apply(true);
        return texture;
    }

    static Mesh GroundMesh(Transform site, List<Road> roads)
    {
        const int cells = 80;
        var vertices = new List<Vector3>();
        var uv = new List<Vector2>();
        var indices = new List<int>();
        for (int z = 0; z <= cells; z++)
        for (int x = 0; x <= cells; x++)
        {
            var p = new Vector2(x / (float)cells * 40 - 20, z / (float)cells * 40 - 20);
            var road = RoadDistance(new Vector2(site.position.x + p.x, site.position.z + p.y), roads);
            // Surface ramps over the existing 0.1m road skin, rather than intersecting it.
            float lift = .018f + (1 - Smooth(-.05f,.55f,road)) * .105f;
            vertices.Add(At(site, p.x, p.y, lift));
            uv.Add(new Vector2(x / (float)cells, z / (float)cells));
            if (x == cells || z == cells) continue;
            var a = z * (cells + 1) + x;
            indices.AddRange(new[] { a, a + cells + 1, a + 1, a + 1, a + cells + 1, a + cells + 2 });
        }
        var mesh = new Mesh { name = "Conforming clearing surface" };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uv);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    sealed class Surface
    {
        readonly List<Vector3> vertices = new();
        readonly List<int> triangles = new();
        public void Tri(Vector3 a, Vector3 b, Vector3 c)
        {
            int start = vertices.Count;
            vertices.AddRange(new[] { a,b,c });
            triangles.AddRange(new[] { start,start+1,start+2 });
        }
        public void Leaf(Vector3 a, Vector3 b, Vector3 c)
        {
            Tri(a,b,c);
            Tri(c,b,a); // Separate vertices so recalculated normals remain valid on both sides.
        }
        public Mesh Bake(string name)
        {
            Need(vertices.Count > 0, "Empty detail mesh: " + name);
            var mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles,0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }

    sealed class Detail
    {
        public readonly Surface[] surfaces = Enumerable.Range(0,6).Select(_=>new Surface()).ToArray();
        readonly Transform site;
        readonly int element;
        readonly List<Road> roads;
        readonly System.Random random;
        public Detail(Transform site, int element, List<Road> roads)
        { this.site=site; this.element=element; this.roads=roads; random=new System.Random(950317+element*79); }
        float Range(float a,float b) => Mathf.Lerp(a,b,(float)random.NextDouble());
        Vector3 Point(float x,float z) => At(site,x,z,.022f);
        bool Clear(float x,float z)
        {
            float r=new Vector2(x,z).magnitude;
            if(r<7.8f || r>17.5f || (z<1 && Mathf.Abs(x)<3.1f)) return false;
            if(RoadDistance(new Vector2(site.position.x+x,site.position.z+z),roads)<1.2f) return false;
            // Keep solid scenery footings and guardian routes visually uncluttered.
            var p=site.position+Point(x,z);
            return !Physics.CheckSphere(p+Vector3.up*.85f,.65f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore);
        }
        public void Build()
        {
            for(int i=0;i<175;i++)
            {
                float x=Range(-17.3f,17.3f),z=Range(-17.3f,17.3f);
                if(!Clear(x,z)) continue;
                var point=Point(x,z);
                if(i%5==0) Fern(point,Range(.65f,1.1f));
                else Grass(point,Range(.35f,.77f),i%3==0?1:0);
            }
            for(int i=0;i<95;i++)
            {
                float x=Range(-16,16),z=Range(-16,16);
                if(!Clear(x,z)) continue;
                Stone(Point(x,z),Range(.12f,.50f),Range(.07f,.25f));
            }
            // Low broken wood on the outer shoulders; never a barrier across the approach.
            foreach(var p in new[] {new Vector2(-10,-4),new Vector2(11,3),new Vector2(-6,12)})
                if(Clear(p.x,p.y)) Branch(Point(p.x,p.y),Range(1.5f,2.3f),Range(0,6.28f));
        }
        void Grass(Vector3 at,float height,int surface)
        {
            for(int i=0;i<7;i++)
            {
                float a=Range(0,6.28f);
                var outward=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                var right=new Vector3(-outward.z,0,outward.x)*Range(.055f,.11f);
                var root=at+outward*Range(.01f,.14f);
                var middle=root+outward*height*.18f+Vector3.up*height*.60f;
                var tip=root+outward*height*.57f+Vector3.up*height*Range(.74f,1.1f);
                surfaces[surface].Leaf(root-right,middle-right*.5f,middle+right*.5f);
                surfaces[surface].Leaf(root-right,middle+right*.5f,root+right);
                surfaces[surface].Leaf(middle-right*.5f,tip,middle+right*.5f);
            }
        }
        void Fern(Vector3 at,float size)
        {
            for(int i=0;i<6;i++)
            {
                float a=i*Mathf.PI/3+Range(-.2f,.2f);
                var outward=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                var right=new Vector3(-outward.z,0,outward.x);
                var end=at+outward*size*.85f+Vector3.up*size*.24f;
                for(int n=0;n<4;n++)
                {
                    float t=(n+1)/5f;
                    var p=Vector3.Lerp(at+Vector3.up*size*.12f,end,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*size*.18f;
                    var forward=outward*size*.21f;
                    for(int sign=-1;sign<=1;sign+=2)
                    {
                        var leaf=p+right*(sign*size*.22f*(1-t*.55f))-outward*size*.08f;
                        surfaces[2].Leaf(p-forward*.25f,leaf,p+forward);
                    }
                }
            }
        }
        void Stone(Vector3 at,float size,float height)
        {
            float angle=Range(0,6.28f);
            var top=at+Vector3.up*height;
            for(int i=0;i<5;i++)
            {
                float a=angle+i*Mathf.PI*2/5,b=angle+(i+1)*Mathf.PI*2/5;
                var p=at+new Vector3(Mathf.Cos(a)*size,0,Mathf.Sin(a)*size*.73f);
                var q=at+new Vector3(Mathf.Cos(b)*size,0,Mathf.Sin(b)*size*.73f);
                surfaces[i%3==0?4:3].Tri(p,top,q);
            }
        }
        void Branch(Vector3 at,float length,float angle)
        {
            var axis=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*length;
            var right=new Vector3(-Mathf.Sin(angle),0,Mathf.Cos(angle))*.11f;
            var end=at+axis+Vector3.up*.05f;
            var up=Vector3.up*.18f;
            surfaces[5].Tri(at-right,at+up,end+up);
            surfaces[5].Tri(at-right,end+up,end-right);
            surfaces[5].Tri(at+up,at+right,end+right);
            surfaces[5].Tri(at+up,end+right,end+up);
            surfaces[4].Tri(end-right,end+up,end+right);
        }
    }

    public static object Validate()
    {
        var scene=Scene();
        var sites=Sites(scene);
        var results=sites.Select(site=>
        {
            var root=site.Find(RootName);
            Need(root!=null && root.gameObject.activeSelf, "Missing ground dressing at "+site.name);
            Need(root.GetComponentsInChildren<Collider>(true).Length==0 && root.GetComponentsInChildren<MonoBehaviour>(true).Length==0,
                "Visual ground dressing contains gameplay components.");
            var filters=root.GetComponentsInChildren<MeshFilter>();
            Need(filters.Length==7 && filters.All(f=>f.gameObject.layer==2 && f.sharedMesh!=null &&
                AssetDatabase.GetAssetPath(f.sharedMesh).StartsWith(Folder+"/",StringComparison.Ordinal)),"Missing ground artwork.");
            Need(root.GetComponentsInChildren<MeshRenderer>().All(r=>r.sharedMaterial!=null && !r.sharedMaterial.shader.name.Contains("Error")),
                "Invalid ground material.");
            Need(filters.All(f=>f.sharedMesh.normals.All(n=>n.sqrMagnitude>.9f)),"Cancelled ground/detail normals.");
            var surface=root.Find("Weathered clearing surface").GetComponent<MeshFilter>();
            float maxLift=surface.sharedMesh.vertices.Max(v=>v.y+site.position.y-Height(v.x+site.position.x,v.z+site.position.z));
            Need(maxLift<.13f && maxLift>=.018f,"Ground skin floats above authored surface.");
            return new {site=site.name,meshes=filters.Length,triangles=filters.Sum(f=>f.sharedMesh.triangles.Length/3),maxSurfaceLift=maxLift};
        }).ToArray();
        return new {marker="DICEFREE_ELEMENTAL_GROUNDING_OK",sites=results,collidersAdded=0,gameplayComponentsAdded=0};
    }

    public static object CaptureBefore() => Capture("before");
    public static object CaptureAfter() => Capture("after");
    static object Capture(string phase)
    {
        var scene=Scene();
        var sites=Sites(scene);
        var output=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"VisualBaselines","BriarbackGrounding");
        Directory.CreateDirectory(output);
        var gameCamera=Object.FindAnyObjectByType<ExplorationCamera>();
        Need(gameCamera!=null,"Gameplay camera missing.");
        var go=new GameObject("Temporary grounding review camera");
        var target=new GameObject("Temporary grounding review target");
        var rt=new RenderTexture(1600,900,24);
        var image=new Texture2D(1600,900,TextureFormat.RGB24,false);
        var previous=RenderTexture.active;
        try
        {
            var camera=go.AddComponent<Camera>();
            camera.CopyFrom(gameCamera.GetComponent<Camera>());
            camera.targetTexture=rt;
            var files=new List<string>();
            for(int i=0;i<sites.Length;i++)
            {
                target.transform.position=sites[i].position+new Vector3(2,0,-7);
                gameCamera.GetFollowPose(target.transform,out var p,out var r,out var fov);
                camera.transform.SetPositionAndRotation(p,r);
                camera.fieldOfView=fov;
                camera.Render();
                RenderTexture.active=rt;
                image.ReadPixels(new Rect(0,0,1600,900),0,0);
                image.Apply();
                var file=Path.Combine(output,Elements[i].ToLowerInvariant()+"-"+phase+".png");
                File.WriteAllBytes(file,image.EncodeToPNG());
                files.Add(file);
            }
            return new {phase,files};
        }
        finally
        {
            RenderTexture.active=previous;
            Object.DestroyImmediate(go); Object.DestroyImmediate(target); Object.DestroyImmediate(image);
            rt.Release(); Object.DestroyImmediate(rt);
        }
    }
}
