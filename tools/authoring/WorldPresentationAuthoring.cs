using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static class WorldPresentationAuthoring
{
    private const string World = "Assets/_DiceFree/Art/World/";
    private const string Art = World + "World1TrailPresentation";
    private const string Mire = World + "CrimsonMire/";
    private const string Nature = World + "NaturePass03/";
    private const string Models = World + "VisualBatch01/Models/";
    private static readonly System.Random Random = new(498152);
    private static readonly Vector2[] Pools = { new(25,143), new(64,169), new(95,119), new(113,158) };
    private static readonly Vector2[] Radii = { new(13,7), new(18,9), new(13,6), new(9,5) };
    private static readonly List<MeshRenderer> Changed = new();
    private static MeshCollider valley;
    private static Material shale, ochre, ash, rush, fern, flower, brass, glass;
    private static Mesh fernMesh, grassMesh, flowerMesh;

    public static void Batch()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
        if (AssetDatabase.IsValidFolder(Art)) throw new InvalidOperationException("World presentation has already been authored.");
        AssetDatabase.CreateFolder(World.TrimEnd('/'), "World1TrailPresentation");
        valley = GameObject.Find("Cornberg valley").GetComponent<MeshCollider>();
        Physics.SyncTransforms();
        Changed.Clear();
        var root = Group(null, "World 1 trail and wetland art", Vector3.zero);
        var grain = GroundGrain();
        GroundMaterial("Iron-stained peat",grain,new Color(.91f,.94f,.84f),.53f,(int)RenderQueue.Transparent-12);
        GroundMaterial("Crimson Mire footpath",grain,new Color(1.06f,1.04f,.95f),.30f,(int)RenderQueue.Transparent-11);
        shale = Lit("Wet blue shale", new Color(.25f,.265f,.26f), .39f);
        ochre = Lit("Ironstone ochre", new Color(.43f,.26f,.14f), .29f);
        ash = Lit("Silvered deadwood", new Color(.36f,.29f,.22f), .21f);
        brass = Lit("Weathered waypost brass",new Color(.61f,.42f,.23f),.45f);
        glass = Lit("Mire lantern amber",new Color(.83f,.43f,.17f),.4f);
        glass.EnableKeyword("_EMISSION");
        glass.SetColor("_EmissionColor",new Color(.86f,.35f,.095f) * 1.45f);
        rush = Tinted("Sedge tips", Nature + "GrassBlade_1.mat", new Color(.67f,.54f,.30f));
        fern = Tinted("Wetland ferns", Nature + "FernLeaves.mat", new Color(.62f,.81f,.66f));
        flower = Tinted("Marsh marigold", Nature + "Wildflower_0.mat", new Color(1f,.74f,.26f));
        fernMesh = Asset<Mesh>(World + "WoodlandAndSettlements/Fern.asset");
        grassMesh = Asset<Mesh>(World + "WoodlandAndSettlements/Grass tuft.asset");
        flowerMesh = Asset<Mesh>(World + "WoodlandAndSettlements/Meadow flower.asset");
        BuildPools(Group(root, "Ironwater pools and reed beds", Vector3.zero));
        DressMireTrail(Group(root, "Crimson Mire trail edges", Vector3.zero));
        WoodlandEdges(Group(root, "Verdant and Eastwatch trail edges", Vector3.zero));
        TrailLantern(root,new Vector2(32.4f,84.5f));
        TrailLantern(root,new Vector2(147.7f,128.5f));
        TrailLantern(root,new Vector2(60,139.5f));
        TrailLantern(root,new Vector2(94.7f,144.2f));
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save World 1 presentation.");
        File.WriteAllLines("Logs/WorldPresentationSceneIds.txt",Changed.Select(x => GlobalObjectId.GetGlobalObjectIdSlow(x).targetObjectId.ToString()));
        Debug.Log("DICEFREE_WORLD_PRESENTATION_AUTHORING_OK");
    }

    private static Texture2D GroundGrain()
    {
        const int size = 256;
        var noise = new float[size * size];
        var pixels = new Color32[noise.Length];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float rolling = TileNoise(x,y,8,size);
            float earth = TileNoise(x + 21,y + 39,20,size);
            float grit = TileNoise(x + 67,y + 53,44,size);
            noise[y*size+x] = .23f * rolling + .39f * earth + .38f * grit;
        }
        float Read(int x,int y) => noise[((y+size)%size)*size+(x+size)%size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float flake = Mathf.SmoothStep(0,1,Mathf.InverseLerp(.54f,.78f,TileNoise(x + 11,y + 23,29,size)));
            pixels[y*size+x] = new Color(Read(x,y),.5f + (Read(x-1,y)-Read(x+1,y))*2.8f,
                .5f + (Read(x,y-1)-Read(x,y+1))*2.8f,flake);
        }
        var texture = new Texture2D(size,size,TextureFormat.RGBA32,false,true);
        texture.SetPixels32(pixels);
        string path = Art + "/Peat and iron grit.png";
        File.WriteAllBytes(path,texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.sRGBTexture = false;
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.anisoLevel = 4;
        importer.SaveAndReimport();
        return Asset<Texture2D>(path);
    }

    private static float TileNoise(float x, float y, float frequency, int size)
    {
        float u = Mathf.Repeat(x,size) / size, v = Mathf.Repeat(y,size) / size;
        float a = Mathf.Lerp(Mathf.PerlinNoise(27+u*frequency,18+v*frequency),
            Mathf.PerlinNoise(27+(u-1)*frequency,18+v*frequency),u);
        float b = Mathf.Lerp(Mathf.PerlinNoise(27+u*frequency,18+(v-1)*frequency),
            Mathf.PerlinNoise(27+(u-1)*frequency,18+(v-1)*frequency),u);
        return Mathf.Lerp(a,b,v);
    }

    private static void GroundMaterial(string name, Texture2D grain, Color tint, float relief, int renderQueue)
    {
        var renderer = GameObject.Find(name).GetComponent<MeshRenderer>();
        var original = renderer.sharedMaterial;
        var material = new Material(Shader.Find("DiceFree/Crimson Mire Ground")) { name = name + " detailed" };
        material.SetTexture("_BaseMap",original.GetTexture("_BaseMap"));
        material.SetTexture("_Grain",grain);
        material.SetColor("_Tint",tint);
        material.SetFloat("_Relief",relief);
        material.renderQueue = renderQueue;
        Save(material);
        renderer.sharedMaterial = material;
        Changed.Add(renderer);
    }

    private static void BuildPools(Transform root)
    {
        var pad = Lit("Floating pennywort",new Color(.31f,.43f,.26f),.45f);
        var reedHead = Lit("Cattail heads",new Color(.32f,.17f,.085f),.19f);
        var stems = Lit("Reed stalks",new Color(.47f,.43f,.23f),.26f);
        var reedBatch = new DecorationMesh();
        var headBatch = new DecorationMesh();
        var padBatch = new DecorationMesh();
        var plantBatch = new DecorationMesh();
        for (int pool = 0; pool < Pools.Length; pool++)
        {
            var center = Pools[pool];
            var radii = Radii[pool];
            var old = GameObject.Find("Rust pool " + (pool+1)).GetComponent<MeshRenderer>();
            old.enabled = false;
            Changed.Add(old);
            var material = new Material(Shader.Find("DiceFree/Ironwater")) { name = "Ironwater pool " + (pool+1) };
            material.SetVector("_Pond",new Vector4(center.x,center.y,radii.x,radii.y));
            if (pool % 2 == 0) material.SetColor("_ShallowColor",new Color(.34f,.23f,.14f));
            Save(material);

            const int steps = 72, rings = 12;
            var surface = new DecorationMesh();
            var core = Floor(center,true) + Vector3.up*.108f;
            surface.vertices.Add(core);
            for(int ring=1;ring<=rings;ring++)
            for(int i=0;i<steps;i++)
            {
                float angle = Mathf.PI * 2 * i / steps;
                var edge = PondOffset(angle,radii,pool);
                var p = center + edge * (ring/(float)rings);
                surface.vertices.Add(Floor(p,true)+Vector3.up*.108f);
                int next=(i+1)%steps;
                if(ring==1) surface.triangles.AddRange(new[]{0,1+next,1+i});
                else
                {
                    int outer=1+(ring-1)*steps, inner=outer-steps;
                    surface.triangles.AddRange(new[]{inner+i,outer+next,outer+i,inner+i,inner+next,outer+next});
                }
            }
            AddMesh(root,material.name,surface,material);
            for (int i=0;i<31;i++)
            {
                float angle=i*2.399963f+pool*.71f;
                var edge=PondOffset(angle,radii,pool);
                var ground=Floor(center+edge*Between(.98f,1.13f),true);
                if(i%6==0)
                {
                    Model(root,"Layered bank shale",i%2==0?"rock_02":"rock_05",ground,Between(.44f,.94f),angle*57,shale);
                    continue;
                }
                if (i%4==0) plantBatch.Add(grassMesh,ground,Between(1.1f,1.75f),angle*57);
                else for(int stalk=0;stalk<4;stalk++)
                {
                    var start=ground + new Vector3(Between(-.34f,.34f),.02f,Between(-.34f,.34f));
                    var top=start+new Vector3(Between(-.19f,.19f),Between(.56f,1.40f),Between(-.18f,.18f));
                    reedBatch.Tube(start,top,.025f,.014f,5);
                    headBatch.Tube(top-Vector3.up*.15f,top+Vector3.up*.085f,.058f,.043f,7);
                    var leaf = start + Vector3.up * .25f;
                    var lean = new Vector3(Mathf.Cos(angle+stalk), .48f, Mathf.Sin(angle+stalk)) * Between(.22f,.60f);
                    reedBatch.Leaf(leaf,lean,.059f);
                }
            }
            for(int cluster=0;cluster<5;cluster++)
            {
                float angle = pool + cluster * 2.399963f;
                var spot = center + PondOffset(angle,radii,pool)*Between(.67f,.83f);
                for(int i=0;i<4;i++)
                {
                    var p = spot + new Vector2(Between(-.64f,.64f),Between(-.38f,.38f));
                    padBatch.Fan(Floor(p,true)+Vector3.up*.143f,Between(.18f,.34f),Between(0,6.28f));
                }
            }
            if(pool==1 || pool==2)
            {
                var edge = center + PondOffset(2.24f+pool,radii,pool)*1.07f;
                Model(root,"Half-buried roots","root_02",Floor(edge,true),.64f,45+pool*33,ash);
            }
        }
        AddMesh(root,"Ironwater reeds",reedBatch,stems);
        AddMesh(root,"Rust-brown cattails",headBatch,reedHead);
        AddMesh(root,"Floating pennywort leaves",padBatch,pad);
        AddMesh(root,"Soft grasses around the pools",plantBatch,rush);
    }

    private static Vector2 PondOffset(float angle,Vector2 radii,int pool)
    {
        float bend=1+.09f*Mathf.Sin(angle*3+pool)+.055f*Mathf.Sin(angle*7-pool);
        return Vector2.Scale(radii,new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)))*bend;
    }

    private static void DressMireTrail(Transform root)
    {
        var verge = new[] { new Vector2(35,91), new Vector2(47,108), new Vector2(52,125), new Vector2(65,144),
            new Vector2(83,149), new Vector2(102,145), new Vector2(131,135), new Vector2(150,131), new Vector2(166,130) };
        DressVerge(root,verge,true,2.8f,5.8f);
        foreach(var p in new[]{new Vector2(32,95), new Vector2(42,110), new Vector2(50,130), new Vector2(70,149),
                     new Vector2(99,150), new Vector2(124,130), new Vector2(142,137), new Vector2(157,133)})
            Model(root,"Pathside ironstone","rock_04",Floor(p,true),Between(.28f,.58f),Between(0,360),ochre);
    }

    private static void WoodlandEdges(Transform root)
    {
        DressVerge(Group(root,"Fernhollow trail pockets",Vector3.zero),new[] { new Vector2(-45,274),new(-43,264),
            new(-44,254),new(-50,243),new(-53,232),new(-46,221),new(-33,223),new(-27,234) },false,2.6f,4.3f);
        DressVerge(Group(root,"Eastwatch footpath pockets",Vector3.zero),new[] { new Vector2(-11,388),new(0,392),
            new(11,394),new(22,400),new(31,406),new(25,414),new(13,417) },false,2.8f,4.5f);
    }

    private static void DressVerge(Transform root,Vector2[] trail,bool mire,float min,float max)
    {
        var grass = new DecorationMesh();
        var leaves = new DecorationMesh();
        var blooms = new DecorationMesh();
        for(int i=1;i<trail.Length;i++)
        {
            var along=(trail[i]-trail[i-1]).normalized;
            var across=new Vector2(-along.y,along.x);
            for(float distance=2;distance<Vector2.Distance(trail[i],trail[i-1])-1;distance+=Between(1.2f,3.5f))
            for(int side=-1;side<=1;side+=2)
            {
                var p=trail[i-1]+along*distance+across*(Between(min,max)*side);
                if (mire && Pools.Where((center,k)=>Vector2.Scale(p-center,new Vector2(1/Radii[k].x,1/Radii[k].y)).sqrMagnitude<1.34f).Any())
                    continue;
                if (!TryFloor(p,mire,out var ground)) continue;
                grass.Add(grassMesh,ground,Between(.85f,1.65f),Between(0,360));
                if (Random.Next(3)==0)
                    leaves.Add(fernMesh,ground+new Vector3(.24f,0,.19f),Between(.62f,1.22f),Between(0,360));
                if (Random.Next(5)==0)
                    blooms.Add(flowerMesh,ground+new Vector3(-.26f,0,.22f),Between(.65f,1.04f),Between(0,360));
            }
        }
        AddMesh(root,"Trailside grasses",grass,mire?rush:Asset<Material>(Nature+"GrassBlade_1.mat"));
        AddMesh(root,"Trailside ferns",leaves,fern);
        AddMesh(root,"Trailside flowers",blooms,mire?flower:Asset<Material>(Nature+"Wildflower_1.mat"));
    }

    private static void TrailLantern(Transform root,Vector2 point)
    {
        var post = Group(root,"Amber trail lantern",Floor(point,true));
        Model(post,"Reclaimed post footing","rock_02",post.position,.31f,35,shale);
        Block(post,"Cedar post",new Vector3(0,1.15f,0),new Vector3(.16f,2.3f,.17f),ash);
        Block(post,"Lantern arm",new Vector3(.21f,2.14f,0),new Vector3(.59f,.09f,.12f),ash);
        Block(post,"Amber panes",new Vector3(.45f,1.88f,0),new Vector3(.22f,.32f,.22f),glass);
        Block(post,"Lantern crown",new Vector3(.45f,2.08f,0),new Vector3(.31f,.075f,.30f),brass);
        Block(post,"Lantern base",new Vector3(.45f,1.70f,0),new Vector3(.28f,.06f,.28f),brass);
        for(int x=-1;x<=1;x+=2)
        for(int z=-1;z<=1;z+=2)
            Block(post,"Lantern frame",new Vector3(.45f+x*.106f,1.88f,z*.106f),new Vector3(.031f,.35f,.031f),brass);
        var at = Group(post,"Amber lantern light",new Vector3(.45f,1.91f,0));
        var light = at.gameObject.AddComponent<Light>();
        light.type=LightType.Point;
        light.color=new Color(1,.63f,.28f);
        light.range=5.7f;
        light.intensity=2.2f;
        light.shadows=LightShadows.None;
        light.cullingMask=~(1<<31);
    }

    private static void Block(Transform root,string name,Vector3 position,Vector3 size,Material material)
    {
        var block=GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name=name;
        block.layer=2;
        block.transform.SetParent(root,false);
        block.transform.localPosition=position;
        block.transform.localScale=size;
        Object.DestroyImmediate(block.GetComponent<Collider>());
        block.GetComponent<Renderer>().sharedMaterial=material;
    }

    private static void Model(Transform root,string name,string asset,Vector3 position,float height,float yaw,Material material)
    {
        var model=(GameObject)PrefabUtility.InstantiatePrefab(Asset<GameObject>(Models+asset+".fbx"),root);
        PrefabUtility.UnpackPrefabInstance(model,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        model.name=name;
        model.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
        foreach(var collider in model.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
        var renderers=model.GetComponentsInChildren<MeshRenderer>();
        Bounds BoundsOf()
        {
            var bounds=renderers[0].bounds;
            foreach(var r in renderers.Skip(1)) bounds.Encapsulate(r.bounds);
            return bounds;
        }
        model.transform.localScale*=height/Mathf.Max(.01f,BoundsOf().size.y);
        var placed=BoundsOf();
        model.transform.position+=new Vector3(position.x-placed.center.x,position.y-placed.min.y,position.z-placed.center.z);
        foreach(var renderer in renderers)
        {
            renderer.gameObject.layer=2;
            renderer.sharedMaterials=renderer.sharedMaterials.Select(_=>material).ToArray();
        }
    }

    private static bool TryFloor(Vector2 point,bool mire,out Vector3 world)
    {
        var ray=new Ray(new Vector3(point.x,80,point.y),Vector3.down);
        RaycastHit hit;
        bool found=mire?valley.Raycast(ray,out hit,100):Physics.Raycast(ray,out hit,100,1<<8);
        world=hit.point;
        return found;
    }
    private static Vector3 Floor(Vector2 point,bool mire) => TryFloor(point,mire,out var ground) ? ground :
        throw new InvalidOperationException("No ground for world art at "+point);
    private static float Between(float min,float max)=>Mathf.Lerp(min,max,(float)Random.NextDouble());

    private static Transform Group(Transform parent,string name,Vector3 position)
    {
        var group=new GameObject(name);
        group.layer=2;
        group.transform.SetParent(parent,false);
        group.transform.localPosition=position;
        return group.transform;
    }
    private static void AddMesh(Transform root,string name,DecorationMesh parts,Material material)
    {
        if(parts.vertices.Count==0) return;
        var mesh=new Mesh { name=root.name+" - "+name };
        if(parts.vertices.Count>65535)mesh.indexFormat=IndexFormat.UInt32;
        mesh.SetVertices(parts.vertices);
        mesh.SetTriangles(parts.triangles,0);
        if(parts.uv.Count==parts.vertices.Count)mesh.SetUVs(0,parts.uv);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Save(mesh);
        var node=Group(root,name,Vector3.zero).gameObject;
        node.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=node.AddComponent<MeshRenderer>();
        renderer.sharedMaterial=material;
        renderer.shadowCastingMode=ShadowCastingMode.Off;
    }
    private static Material Lit(string name,Color color,float smoothness)
    {
        var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")) { name=name,color=color };
        mat.SetFloat("_Smoothness",smoothness);
        mat.SetFloat("_Cull",0);
        Save(mat);
        return mat;
    }
    private static Material Tinted(string name,string source,Color tint)
    {
        var mat=new Material(Asset<Material>(source)) { name=name,color=tint };
        Save(mat);
        return mat;
    }
    private static T Asset<T>(string path) where T:Object => AssetDatabase.LoadAssetAtPath<T>(path) ??
        throw new IOException("Missing world art "+path);
    private static void Save(Object item) => AssetDatabase.CreateAsset(item,Art+"/"+item.name+(item is Material?".mat":".asset"));

    private sealed class DecorationMesh
    {
        public readonly List<Vector3> vertices=new();
        public readonly List<int> triangles=new();
        public readonly List<Vector2> uv=new();
        public void Add(Mesh mesh,Vector3 at,float size,float yaw)
        {
            int start=vertices.Count;
            var rotation=Quaternion.Euler(0,yaw,0);
            vertices.AddRange(mesh.vertices.Select(v=>at+rotation*(v*size)));
            triangles.AddRange(mesh.triangles.Select(index=>start+index));
            var coordinates=mesh.uv;
            uv.AddRange(coordinates.Length==mesh.vertexCount?coordinates:new Vector2[mesh.vertexCount]);
        }
        public void Tube(Vector3 a,Vector3 b,float bottom,float top,int sides)
        {
            int start=vertices.Count;
            var rotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);
            for(int i=0;i<sides;i++)
            {
                float angle=Mathf.PI*2*i/sides;
                var radial=rotation*new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                vertices.Add(a+radial*bottom); vertices.Add(b+radial*top);
                int first=start+i*2, next=start+(i+1)%sides*2;
                triangles.AddRange(new[]{first,first+1,next,next,first+1,next+1});
            }
        }
        public void Leaf(Vector3 at,Vector3 lean,float width)
        {
            int start=vertices.Count;
            var side=Vector3.Cross(Vector3.up,lean).normalized*width;
            var middle=at+lean*.45f+Vector3.up*.12f;
            vertices.Add(at);vertices.Add(middle-side);vertices.Add(middle+side);vertices.Add(at+lean);
            triangles.AddRange(new[]{start,start+1,start+2,start+1,start+3,start+2});
        }
        public void Fan(Vector3 at,float radius,float yaw)
        {
            int start=vertices.Count;
            vertices.Add(at);
            const int sides=9;
            for(int i=0;i<=sides;i++)
            {
                float a=yaw+Mathf.Lerp(.35f,Mathf.PI*2-.35f,i/(float)sides);
                vertices.Add(at+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(i*2)*.018f,Mathf.Sin(a)*radius));
                if(i>0)triangles.AddRange(new[]{start,start+i+1,start+i});
            }
        }
    }
}
