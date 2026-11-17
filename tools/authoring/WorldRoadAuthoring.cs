using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.EditorTools;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Use the same terrain-aligned footprints for the road art and its travel effect.
public static class WorldRoadAuthoring
{
    private const string World = "Assets/_DiceFree/Art/World/";
    private const string Output = World + "WorldRoads";
    private const string SurfaceData = "Assets/_DiceFree/Settings/Travel/Cornberg dirt road.asset";
    private const string RootName = "World 1 connected roads";
    private const float RoadLift = .074f;
    private static readonly HashSet<Object> ExistingChanges = new();
    private static readonly List<string> Verification = new();
    private static Transform[] sceneObjects;
    private static TravelSurfaceDefinition road;
    private static Transform roads;
    private static readonly Vector2[] CartTrack =
    {
        new(248,183),new(257,182.2f),new(263,183),new(267,185.4f),new(272,185.4f),
        new(274.6f,184.3f),new(278,180),new(285,180),new(301,180)
    };

    public static void Batch()
    {
        var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
        if (GameObject.Find(RootName) != null || AssetDatabase.IsValidFolder(Output))
            throw new InvalidOperationException("Restore the pre-road scene before authoring this pass again.");
        sceneObjects = scene.GetRootGameObjects().SelectMany(o => o.GetComponentsInChildren<Transform>(true)).ToArray();
        AssetDatabase.CreateFolder(World.TrimEnd('/'), "WorldRoads");
        Physics.SyncTransforms();
        road = Load<TravelSurfaceDefinition>(SurfaceData);
        roads = new GameObject(RootName).transform;

        var reference = Load<Material>(World + "BriarbackRegion/Trampled trail.mat");
        var farm = MaterialCopy(reference, "Cornberg packed gravel", new Color(1.04f, 1.02f, .95f));
        var forest = MaterialCopy(reference, "Ironpine earth and gravel", new Color(.84f, .81f, .76f));
        var lumber = MaterialCopy(reference, "Trimery cart road gravel", new Color(1.06f, .98f, .86f));
        var junction = MaterialCopy(Load<Material>(World + "BriarbackRegion/Feathered trail junctions.mat"),
            "Worn junction gravel", new Color(.94f, .90f, .82f));

        var nav = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            CornbergAndIronpine(farm, forest, junction);
            RepairLegacyPaths(farm,forest);
            Trimery(lumber, junction);
            OtherRegions();
            VerifyRoads();
        }
        finally { nav.Remove(); }

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the road update.");
        AssetDatabase.SaveAssets();
        File.WriteAllLines("Logs/WorldRoadSceneIds.txt", ExistingChanges.Select(o =>
            GlobalObjectId.GetGlobalObjectIdSlow(o).targetObjectId.ToString()));
        File.WriteAllLines("Logs/WorldRoadAuthoring.ok", Verification);
    }

    private static void RepairLegacyPaths(Material farm,Material forest)
    {
        // The root-level copy offset by 2.7 m follows the same woodland loop.
        // Keep one road at that fork and preserve the other copies' distinct routes.
        var duplicate = sceneObjects.Single(t => t.parent==null && t.name=="North woodland loop");
        Hide(duplicate);
        var paths = sceneObjects.Where(t =>
            (t.parent==null && t.name=="North woodland loop (1)") ||
            (t.parent!=null && t.parent.name=="Cornberg eastern forest expansion v1" &&
                (t.name=="North meadow farm path" || t.name.StartsWith("Eastern woodland fighting path",StringComparison.Ordinal))))
            .OrderBy(t => t.name,StringComparer.Ordinal).ToArray();
        if (paths.Length!=5) throw new InvalidOperationException("Expected the four separate woodland paths and the meadow road.");
        for (int i=0;i<paths.Length;i++)
        {
            var path=paths[i];
            var mesh=path.GetComponent<MeshFilter>().sharedMesh;
            var vertices=mesh.vertices.Select(path.TransformPoint).Select(v=>new Vector2(v.x,v.z)).ToArray();
            if (vertices.Length<4 || vertices.Length%4!=0 || mesh.triangles.Length!=vertices.Length/4*6)
                throw new InvalidOperationException("Expected a road made of paired segments at " + path.name);
            var controls=new List<Vector2>{(vertices[0]+vertices[1])*.5f};
            Vector2 previousDirection=Vector2.zero;
            Vector2 end=default;
            for (int k=0;k<vertices.Length;k+=4)
            {
                var start=(vertices[k]+vertices[k+1])*.5f;
                var next=(vertices[k+2]+vertices[k+3])*.5f;
                var direction=(next-start).normalized;
                if (k>0 && (Vector2.Dot(direction,previousDirection)<.9999f || Vector2.Distance(start,end)>.08f))
                    controls.Add((start+end)*.5f);
                previousDirection=direction;
                end=next;
            }
            controls.Add(end);
            float width=Vector2.Distance(vertices[0],vertices[1])*.52f;
            string name=path.name=="North meadow farm path" ? "North meadow road" :
                path.parent==null ? "Western Ironpine woodland road" : "Eastern woodland trail "+(i+1);
            var ribbon=new RoadMesh();
            ribbon.Strip(controls.ToArray(),width,softenEnd:true);
            Hide(path);
            var filter=Surface(name,ribbon.Save(name),path.name=="North meadow farm path"?farm:forest);
            Bind(filter,"travel.road."+name.ToLowerInvariant().Replace(' ','-'));
        }
    }

    private static void CornbergAndIronpine(Material farm, Material forest, Material junction)
    {
        var west = new Vector2[] { new(-45,-33),new(-36,-29),new(-30,-22),new(-26,-15),new(-26,-9),new(-23.59f,-9) };
        var east = new Vector2[] { new(-14.61f,-9),new(-12,-9),new(0,0),new(25,0),new(54,6),new(72,18),new(95,34),new(114,43) };
        var main = new RoadMesh();
        main.Strip(west,2.42f,bridgeEnd: -23.59f);
        main.Strip(east,2.42f,bridgeEnd: -14.61f);
        var mainSurface = Surface("Cornberg main road",main.Save("Cornberg main road"),farm);
        Hide(Find("Cornberg packed earth - Mountain to east road"));
        Transfer(Find("Mountain to east road").GetComponent<TravelSurface>(), mainSurface);

        Renew("Cornberg packed earth - North woodland loop","Ironpine north woodland loop road",forest,1.72f,
            new(62,11),new(64,25),new(64,35),new(76,47),new(91,46),new(95,34));
        Renew("Cornberg packed earth - South woodland loop","Ironpine south woodland loop road",forest,1.72f,
            new(73,19),new(83,-3),new(103,1),new(109,26),new(95,34));
        Renew("Cornberg packed earth - Dungeon approach","Slime dungeon approach",forest,1.63f,
            new(76,47),new(78,51),new(80,56));

        var pine = Find("Winding pine forest road");
        var oldSpeed = pine.GetComponent<TravelSurface>();
        var rebuilt = CreateRibbon("Ironpine northbound road",forest,2.42f,
            new(114,43),new(140,44),new(158,45),new(170,46),new(175,51),
            new(174,64),new(180,80),new(183,98),new(196,112),new(208,130),new(223,149));
        Hide(pine);
        Transfer(oldSpeed,rebuilt);

        foreach (var bridge in sceneObjects.Where(t => t.name=="Timber footbridge" &&
            t.gameObject.activeInHierarchy && t.GetComponent<Collider>() != null))
        {
            var filter = bridge.GetComponent<MeshFilter>();
            if (filter != null) Bind(filter, bridge.position.z < 0 ? "travel.cornberg.south-bridge" : "travel.cornberg.north-bridge");
        }

        Junction("Woodland road fork",new Vector2(95,34),3.25f,junction);
        Junction("North loop junction",new Vector2(76,47),2.37f,junction);
        Junction("Ironpine road transition",new Vector2(114,43),2.69f,junction);
    }

    private static void Trimery(Material material, Material junction)
    {
        Renew("Trimery packed earth - Trimery southern caravan approach","Trimery caravan approach",material,2.92f,
            new(223,149),new(237,151),new(248,154),new(248,166),new(248,180));
        Renew("Trimery packed earth - Trimery western lumber-cart highway","Trimery west road",material,3.48f,
            new(248,180),new(232,180),new(220,180),new(195,180));
        Renew("Trimery packed earth - Trimery northern quilboar route","Trimery north road",material,2.44f,
            new(248,180),new(248,207),new(248,223),new(248,243));
        Hide(Find("Eastern timber cart trail"));
        var cart=new RoadMesh();
        cart.Strip(CartTrack,2.10f,softenEnd:true);
        Bind(Surface("Trimery east cart track",cart.Save("Trimery east cart track"),material),"travel.road.trimery-east-cart-track");
        Junction("Trimery crossroads",new Vector2(248,180),4.4f,junction);
        Junction("Ironpine to Trimery junction",new Vector2(223,149),3.2f,junction);
    }

    private static void OtherRegions()
    {
        foreach (string name in new[]
        {
            "Briarback northern detour", "Briarback western scouting loop", "Harrowtusk eastern camp trail",
            "Briarback circular warband road", "North approach to the open stronghold gate"
        })
        {
            var baseRoad = Find(name);
            var visible = Find(name + " worn surface");
            var filter = Align(visible, .095f);
            Transfer(baseRoad.GetComponent<TravelSurface>(), filter);
        }

        foreach (var (name,id) in new[]
        {
            ("Worn trail around Lanternmere", "travel.verdant.lanternmere"),
            ("North trail and lookout approach", "travel.reedscale.lookout"),
            ("Fernhollow loop", "travel.verdant.fernhollow"),
            ("Eastwatch patrol loop", "travel.reedscale.eastwatch")
        })
            Bind(Align(Find(name),RoadLift), id);

        Bind(Find("Continuous footbridge deck").GetComponent<MeshFilter>(), "travel.reedscale.footbridge");
        Bind(Find("Raised timber bridge deck over lower road").GetComponent<MeshFilter>(), "travel.briarback.stronghold-bridge");
        foreach (var (name,id) in new[]
        {
            ("West gate approach", "travel.arborium.west-gate"),
            ("East gate approach", "travel.arborium.east-gate"),
            ("Guild forecourt - patterned stone and arrival banners", "travel.arborium.guild-forecourt"),
            ("Merchant quarter - striped cloth and goods", "travel.arborium.merchant-quarter")
        })
        {
            var filter = Find(name).Find("radial granite paving").GetComponent<MeshFilter>();
            Bind(filter,id);
        }

        var highway = Find("Guild trail from Arborium to the wreck").Find("Compacted roadbed");
        Bind(highway.GetComponent<MeshFilter>(), "travel.roadwarden.approach");
    }

    private static void Renew(string previous,string label,Material material,float halfWidth,params Vector2[] route)
    {
        Hide(Find(previous));
        var filter = CreateRibbon(label,material,halfWidth,route);
        Bind(filter, "travel.road." + label.ToLowerInvariant().Replace(' ','-'));
    }

    private static MeshFilter CreateRibbon(string name,Material material,float halfWidth,params Vector2[] route)
    {
        var mesh = new RoadMesh();
        mesh.Strip(route,halfWidth);
        return Surface(name,mesh.Save(name),material);
    }

    private static void Junction(string name,Vector2 center,float radius,Material material)
    {
        const int spokes = 24, rings = 5;
        var v = new List<Vector3> { Ground(center) + Vector3.up * .127f };
        var uv = new List<Vector2> { new(.5f,.5f) };
        var triangles = new List<int>();
        for (int ring = 1; ring <= rings; ring++)
        for (int i = 0; i < spokes; i++)
        {
            float angle = i * Mathf.PI * 2 / spokes;
            var offset = new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)) * (ring/(float)rings);
            v.Add(Ground(center + offset*radius) + Vector3.up * .127f);
            uv.Add(new Vector2(.5f,.5f)+offset*.5f);
        }
        for (int ring = 1; ring <= rings; ring++)
        for (int i = 0; i < spokes; i++)
        {
            int a = 1 + (ring-1)*spokes + i;
            int b = 1 + (ring-1)*spokes + (i+1)%spokes;
            if (ring==1) Triangle(v,triangles,0,b,a);
            else
            {
                Triangle(v,triangles,a-spokes,b,a);
                Triangle(v,triangles,a-spokes,b-spokes,b);
            }
        }
        var filter = Surface(name,SaveMesh(name,v,uv,triangles),material);
        Bind(filter, "travel.junction." + name.ToLowerInvariant().Replace(' ','-'));
    }

    private static MeshFilter Align(Transform node,float lift)
    {
        var filter = node.GetComponent<MeshFilter>();
        var mesh = Object.Instantiate(filter.sharedMesh);
        mesh.name = node.name + " grounded";
        var vertices = mesh.vertices;
        for (int i=0;i<vertices.Length;i++)
        {
            var p = node.TransformPoint(vertices[i]);
            if (!Physics.Raycast(new Vector3(p.x,65,p.z),Vector3.down,out var hit,105,1<<8)) continue;
            if (Mathf.Abs(hit.point.y-p.y) > 1f) continue;
            vertices[i] = node.InverseTransformPoint(new Vector3(p.x,hit.point.y+lift,p.z));
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, Output+"/"+mesh.name+".asset");
        filter.sharedMesh = mesh;
        ExistingChanges.Add(filter);
        return filter;
    }

    private static void Hide(Transform original)
    {
        var renderer = original.GetComponent<MeshRenderer>();
        if (!renderer.enabled) return;
        renderer.enabled = false;
        ExistingChanges.Add(renderer);
    }

    private static void Transfer(TravelSurface existing,MeshFilter footprint)
    {
        if (existing == null) throw new InvalidOperationException("Expected the old road effect for " + footprint.name);
        existing.Configure(existing.StableId,existing.Definition,footprint,BoundsFor(footprint));
        ExistingChanges.Add(existing);
    }

    private static void Bind(MeshFilter filter,string id)
    {
        if (filter.TryGetComponent<TravelSurface>(out _)) throw new InvalidOperationException("Already a travel surface: " + filter.name);
        var value = filter.gameObject.AddComponent<TravelSurface>();
        value.Configure(id,road,filter,BoundsFor(filter));
        if (!filter.transform.IsChildOf(roads)) ExistingChanges.Add(filter.gameObject);
    }

    private static Bounds BoundsFor(MeshFilter filter)
    {
        var bounds = filter.sharedMesh.bounds;
        bounds.Expand(new Vector3(.004f,1.5f,.004f));
        return bounds;
    }

    private static MeshFilter Surface(string name,Mesh mesh,Material material)
    {
        var go = new GameObject(name);
        go.layer = 2;
        go.transform.SetParent(roads,false);
        var filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        return filter;
    }

    private static Material MaterialCopy(Material source,string name,Color tint)
    {
        var material = new Material(source) { name=name };
        material.SetColor("_BaseColor",tint);
        AssetDatabase.CreateAsset(material,Output+"/"+name+".mat");
        return material;
    }

    private static void VerifyRoads()
    {
        var surfaces = Object.FindObjectsByType<TravelSurface>();
        var tests = new (string,Vector2[])[]
        {
            ("Cornberg south bridge and road", new Vector2[]{new(-26,-10),new(-19,-9),new(-13,-9),new(0,0)}),
            ("Cornberg village to Ironpine", new Vector2[]{new(17,0),new(38,2),new(62,11),new(95,34),new(114,43),new(140,44),new(170,46),new(175,52)}),
            ("North woodland loop", new Vector2[]{new(62,11),new(64,25),new(64,35),new(76,47),new(91,46),new(95,34)}),
            ("South woodland loop", new Vector2[]{new(73,19),new(83,-3),new(103,1),new(109,26),new(95,34)}),
            ("Ironpine to Trimery",new Vector2[]{new(175,52),new(174,64),new(180,80),new(183,98),new(196,112),new(208,130),new(223,149),new(248,154),new(248,171)}),
            ("Trimery east cart track",CartTrack),
            ("Trimery north road",new Vector2[]{new(248,191),new(248,206),new(248,223),new(248,238)}),
            ("Verdant Wilds through the Larch bridge", new Vector2[]{new(12,305),new(-8,305),new(-35,305),new(-36,370),new(-42,388)}),
            ("Fernhollow loop",new Vector2[]{new(-43,264),new(-50,243),new(-46,221),new(-27,234)}),
            ("Eastwatch patrol loop",new Vector2[]{new(-11,388),new(11,394),new(31,406),new(13,417)})
        };
        foreach (var (label,points) in tests)
        {
            Vector3 previous = default;
            for (int i=0;i<points.Length;i++)
            {
                var sample = Ground(points[i]);
                if (!NavMesh.SamplePosition(sample,out var hit,1.65f,1))
                    throw new InvalidOperationException(label+" cannot be reached at "+points[i]);
                if (!surfaces.Any(surface => surface.Contains(hit.position) && surface.Definition.speedBonusPercent == 15))
                    throw new InvalidOperationException(label+" has no road bonus at "+points[i]+" (nav "+hit.position+")");
                if (i>0)
                {
                    var path = new NavMeshPath();
                    if (!NavMesh.CalculatePath(previous,hit.position,1,path) || path.status!=NavMeshPathStatus.PathComplete)
                        throw new InvalidOperationException(label+" is disconnected at "+points[i]);
                }
                previous = hit.position;
            }
            Verification.Add(label + " | " + points.Length + " connected road bonus samples");
        }
    }

    private static Vector3 Ground(Vector2 p)
    {
        if (!Physics.Raycast(new Vector3(p.x,65,p.y),Vector3.down,out var hit,105,1<<8))
            throw new InvalidOperationException("No ground under road at " + p);
        return hit.point;
    }

    private static Mesh SaveMesh(string name,List<Vector3> vertices,List<Vector2> uv,List<int> triangles)
    {
        var mesh = new Mesh { name=name, indexFormat=IndexFormat.UInt32 };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0,uv);
        mesh.SetTriangles(triangles,0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Output+"/"+name+".asset");
        return mesh;
    }

    private static void Triangle(List<Vector3> v,List<int> triangles,int a,int b,int c)
    {
        if (Vector3.Cross(v[b]-v[a],v[c]-v[a]).y<0) (b,c)=(c,b);
        triangles.Add(a); triangles.Add(b); triangles.Add(c);
    }

    private static Transform Find(string name) => sceneObjects.Single(t => t.name==name);
    private static T Load<T>(string path) where T : Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new FileNotFoundException(path);

    private sealed class RoadMesh
    {
        private readonly List<Vector3> vertices = new();
        private readonly List<Vector2> uv = new();
        private readonly List<int> triangles = new();

        public Mesh Save(string name) => SaveMesh(name,vertices,uv,triangles);

        public void Strip(Vector2[] route,float halfWidth,float bridgeEnd=float.NaN,bool softenEnd=false)
        {
            var centers = new List<Vector2>();
            for (int i=1;i<route.Length;i++)
            {
                var p0=route[Mathf.Max(0,i-2)];
                var p1=route[i-1];
                var p2=route[i];
                var p3=route[Mathf.Min(i+1,route.Length-1)];
                int steps=Mathf.CeilToInt(Vector2.Distance(p1,p2)/.55f);
                for (int step=0;step<steps;step++)
                {
                    float t=step/(float)steps, t2=t*t, t3=t2*t;
                    centers.Add(.5f*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t2+(-p0+3*p1-3*p2+p3)*t3));
                }
            }
            centers.Add(route[^1]);
            const int lanes=9;
            float distance=0;
            bool west = bridgeEnd < -20;
            for (int row=0;row<centers.Count;row++)
            {
                var point=centers[row];
                if (row>0) distance+=Vector2.Distance(point,centers[row-1]);
                var tangent=(centers[Mathf.Min(centers.Count-1,row+1)]-centers[Mathf.Max(0,row-1)]).normalized;
                var side=new Vector2(-tangent.y,tangent.x);
                float width=halfWidth+.105f*Mathf.Sin(distance*.89f)+.09f*Mathf.Sin(distance*.27f);
                for (int col=0;col<lanes;col++)
                {
                    float across=col/(lanes-1f);
                    var xz=point+side*((across*2-1)*width);
                    if (!float.IsNaN(bridgeEnd) && Mathf.Abs(point.x-bridgeEnd)<6f)
                        xz.x=west?Mathf.Min(xz.x,bridgeEnd):Mathf.Max(xz.x,bridgeEnd);
                    var v=Ground(xz)+Vector3.up*RoadLift;
                    if (!float.IsNaN(bridgeEnd))
                    {
                        float contact=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,4,Mathf.Abs(xz.x-bridgeEnd)));
                        v.y=Mathf.Lerp(v.y,.225f,contact);
                    }
                    int index=vertices.Count;
                    vertices.Add(v);
                    float fade = softenEnd ? Mathf.SmoothStep(0,1,Mathf.Clamp01((centers.Count-1-row)*.20f)) : 1;
                    uv.Add(new Vector2(Mathf.Lerp(1,across,fade),distance*.21f));
                    if(row==0||col==0) continue;
                    Triangle(vertices,triangles,index-lanes-1,index-lanes,index-1);
                    Triangle(vertices,triangles,index-lanes,index,index-1);
                }
            }
        }
    }
}
