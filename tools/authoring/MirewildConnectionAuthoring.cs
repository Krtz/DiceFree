using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.EditorTools;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static class MirewildConnectionAuthoring
{
    private const string World = "Assets/_DiceFree/Art/World/";
    private const string Output = World + "MirewildConnection";
    private const int Columns = 73;
    private const int Rows = 31;
    private static readonly System.Random Random = new(77301);
    private static readonly List<Object> Changed = new();
    private static readonly List<MeshCollider> Floors = new();
    private static MeshCollider valley;
    private static Vector3[] ridge;
    private static Material moss, shale, roots;
    private static readonly (Vector2 point, Vector2 radius)[] Ponds =
    {
        (new(9,175),new(8.5f,4.9f)), (new(-29,191),new(6.4f,3.1f))
    };
    private static readonly (string name, Vector2 point)[] ForestMonsters =
    {
        ("Sporecap (Lv 33)",new(-46,202)),
        ("Thornlash (Lv 32)",new(-22,212)),
        ("Lanterncap (Lv 32)",new(-15,201))
    };
    private static readonly Vector2[] Slimes = {new(2,169),new(27,169),new(45,181)};

    public static void Batch()
    {
        var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
        if (AssetDatabase.IsValidFolder(Output)) throw new InvalidOperationException("The Mirewild connection is already authored.");
        AssetDatabase.CreateFolder(World.TrimEnd('/'), "MirewildConnection");
        Changed.Clear();
        Floors.Clear();
        var forest = Find("Fernhollow moss banks").GetComponent<MeshCollider>();
        var mainForest = Find("Rolling moss banks and spring basin").GetComponent<MeshCollider>();
        valley = Find("Cornberg valley").GetComponent<MeshCollider>();
        Floors.AddRange(new[]{forest,mainForest,valley});
        moss = forest.GetComponent<MeshRenderer>().sharedMaterial;
        shale = Load<Material>(World+"World1TrailPresentation/Wet blue shale.mat");
        roots = Load<Material>(World+"World1TrailPresentation/Silvered deadwood.mat");
        var root = Group(null,"Mirewild Trail - Fernhollow to Crimson Mire",Vector3.zero);
        ridge = ExtendForest(root,forest);
        Physics.SyncTransforms();
        ClearOldSkirt();
        BuildSideSlopes(root);
        PaintWetland(root);
        var trail = BuildTrail(root);
        PopulatePonds(root);
        PlantVerge(root);
        MakeSign(root,new Vector2(-30,220),224,"CRIMSON MIRE\nLEVEL 50");
        MakeSign(root,new Vector2(53,153),222,"FERNHOLLOW\nVERDANT WILDS");

        RebuildNavigation();
        var nav = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            VerifyTrail(trail);
            var source = Find("Wilds creatures");
            var actors = Group(root,"Southern Fernhollow creatures",Vector3.zero);
            foreach (var (name,point) in ForestMonsters) Spawn(source.Find(name),actors,point);
            source = Find("Crimson Mire").Find("Red Slimes");
            actors = Group(root,"Northern Crimson Mire red slimes",Vector3.zero);
            foreach (var point in Slimes) Spawn(source.GetChild(0),actors,point);
        }
        finally { nav.Remove(); }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the connected overworld.");
        File.WriteAllLines("Logs/MirewildConnectionIds.txt", Changed.Select(item=>GlobalObjectId.GetGlobalObjectIdSlow(item).targetObjectId.ToString()));
        Debug.Log("DICEFREE_MIREWILD_CONNECTION_AUTHORING_OK");
    }

    private static Vector3[] ExtendForest(Transform root,MeshCollider source)
    {
        var north = source.sharedMesh.vertices.TakeLast(Columns).Select(source.transform.TransformPoint).ToArray();
        var vertices = new Vector3[Columns * Rows];
        for (int row=0;row<Rows;row++)
        for (int column=0;column<Columns;column++)
        {
            float t=row/(Rows-1f), across=column/(Columns-1f);
            var seam=north[column];
            float x=Mathf.Lerp(seam.x,Mathf.Lerp(-49,41,across),t);
            float z=Mathf.Lerp(seam.z,189.95f,t);
            float bank=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.72f,.99f,Mathf.Abs(across*2-1)));
            float hills=(.52f*Mathf.Sin(x*.33f+z*.28f)+.47f*Mathf.Sin(z*.39f-x*.13f)+1.6f*bank)*Mathf.Sin(t*Mathf.PI);
            float floor=0;
            if(valley.Raycast(new Ray(new Vector3(x,30,189.9f),Vector3.down),out var hit,65)) floor=hit.point.y;
            float y=Mathf.Lerp(seam.y,floor,Mathf.SmoothStep(0,1,t))+hills;
            vertices[row*Columns+column]=new Vector3(x,y,z);
        }
        var mesh=Grid("Fernhollow southern ridge",vertices,Columns,Rows);
        var ground=Surface(root,mesh,moss,8);
        var collider=ground.gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh=mesh;
        mesh.SetPreBakeCollisionMesh(false,true);
        EditorUtility.SetDirty(mesh);
        Floors.Add(collider);
        return vertices;
    }

    private static bool InRidge(Vector3 p,float margin)
    {
        if(p.z<183 || p.z>210) return false;
        float along=Mathf.Clamp01((207-p.z)/22);
        return p.x >= Mathf.Lerp(-66,-49,along)-margin && p.x <= Mathf.Lerp(-13,41,along)+margin;
    }

    private static void ClearOldSkirt()
    {
        var root=Find("Fernhollow - southern Verdant Wilds").Find("Fernhollow outer slope");
        var filter=root.GetComponentInChildren<MeshFilter>();
        var mesh=Object.Instantiate(filter.sharedMesh);
        mesh.name="Fernhollow skirt outside new ridge";
        var kept=new List<int>();
        var triangles=mesh.triangles;
        const int bands=7;
        const int firstSouthRow=RowsInFernhollow-1;
        const int lastSouthRow=firstSouthRow+Columns-1;
        for(int i=0;i<triangles.Length;i+=3)
        {
            int row=Mathf.Min(triangles[i],Mathf.Min(triangles[i+1],triangles[i+2]))/bands;
            if(row>=firstSouthRow && row<lastSouthRow)continue;
            kept.Add(triangles[i]);kept.Add(triangles[i+1]);kept.Add(triangles[i+2]);
        }
        if(kept.Count==triangles.Length)throw new InvalidOperationException("No old scenery intersected the new ridge.");
        mesh.SetTriangles(kept,0);
        Save(mesh);
        filter.sharedMesh=mesh;
        Changed.Add(filter);
        foreach(Transform child in root)
        {
            var p=child.position;
            if(child==filter.transform || p.z>=208 || p.x<=-67 || p.x>=-12)continue;
            child.gameObject.SetActive(false);
            Changed.Add(child.gameObject);
        }
    }
    private const int RowsInFernhollow=53;

    private static void BuildSideSlopes(Transform root)
    {
        const int bands=11;
        for(int side=0;side<2;side++)
        {
            var positions=new Vector3[bands*Rows];
            for(int row=0;row<Rows;row++)
            for(int band=0;band<bands;band++)
            {
                float t=band/(bands-1f), along=row/(Rows-1f);
                var p=ridge[row*Columns+(side==0?0:Columns-1)];
                p.x+=(side==0?-1:1)*t*(13+5*Mathf.Sin(along*Mathf.PI));
                p.z-=t*Mathf.Sin(along*Mathf.PI)*2;
                p.y-=.047f+6.4f*t*t;
                positions[row*bands+band]=p;
            }
            Surface(root,Grid(side==0?"West forest slope":"East forest slope",positions,bands,Rows),moss,2);
        }
    }

    private static void PaintWetland(Transform root)
    {
        const int width=105, height=54;
        var origin=new Vector2(-37,151);
        var vertices=new Vector3[width*height];
        var uv=new Vector2[vertices.Length];
        var onGround=new bool[vertices.Length];
        var texture=new Texture2D(width*3,height*3,TextureFormat.RGBA32,false);
        var pixels=new Color32[texture.width*texture.height];
        for(int y=0;y<texture.height;y++)
        for(int x=0;x<texture.width;x++)
        {
            var p=origin+new Vector2(x*(width-1f)/(texture.width-1),y*(height-1f)/(texture.height-1));
            float noise=Mathf.PerlinNoise(31+p.x*.09f,47+p.y*.075f);
            float distance=TrailDistance(p,3);
            float fill=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(12,32,distance+(noise-.5f)*14));
            float northFade=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(188,203,p.y+(noise-.5f)*8));
            float southFade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(151,163,p.y));
            float edgeFade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-37,-29,p.x))*
                (1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(57,67,p.x)));
            var color=Color.Lerp(new Color(.25f,.15f,.11f),new Color(.43f,.30f,.20f),noise);
            color.a=fill*northFade*southFade*edgeFade*.91f;
            pixels[y*texture.width+x]=color;
        }
        texture.SetPixels32(pixels);
        var path=Output+"/Wetland transition.png";
        File.WriteAllBytes(path,texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.wrapMode=TextureWrapMode.Clamp;
        importer.alphaIsTransparency=true;
        importer.SaveAndReimport();

        for(int row=0;row<height;row++)
        for(int col=0;col<width;col++)
        {
            int index=row*width+col;
            var p=origin+new Vector2(col,row);
            onGround[index]=TryFloor(p,out var hit);
            vertices[index]=onGround[index]?hit+Vector3.up*.088f:new Vector3(p.x,0,p.y);
            uv[index]=new Vector2(col/(width-1f),row/(height-1f));
        }
        var material=new Material(Load<Material>(World+"World1TrailPresentation/Iron-stained peat detailed.mat"))
            { name="Northern mire peat", renderQueue=2988 };
        material.SetTexture("_BaseMap",Load<Texture2D>(path));
        Save(material);
        Surface(root,Grid("Northern mire peat",vertices,width,height,uv,onGround),material,2).shadowCastingMode=ShadowCastingMode.Off;
    }

    private static TravelSurface BuildTrail(Transform root)
    {
        var trail=MirewildTrail.Points;
        var points=new List<Vector2>();
        for(int i=1;i<trail.Count;i++)
        {
            var a=trail[Mathf.Max(0,i-2)];var b=trail[i-1];var c=trail[i];var d=trail[Mathf.Min(i+1,trail.Count-1)];
            int count=Mathf.CeilToInt(Vector2.Distance(b,c)/.65f);
            for(int j=0;j<count;j++)
            {
                float t=j/(float)count;
                points.Add(.5f*((2*b)+(c-a)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));
            }
        }
        points.Add(trail[^1]);
        const int columns=7;
        var vertices=new Vector3[columns*points.Count];
        var uv=new Vector2[vertices.Length];
        float distance=0;
        for(int row=0;row<points.Count;row++)
        {
            if(row>0)distance+=Vector2.Distance(points[row-1],points[row]);
            var heading=(points[Mathf.Min(row+1,points.Count-1)]-points[Mathf.Max(0,row-1)]).normalized;
            var across=new Vector2(-heading.y,heading.x);
            float width=1.79f+.17f*Mathf.Sin(distance*.31f)+.11f*Mathf.Sin(distance*.69f);
            for(int col=0;col<columns;col++)
            {
                float u=col/(columns-1f);
                int index=row*columns+col;
                vertices[index]=Floor(points[row]+across*((2*u-1)*width))+Vector3.up*.151f;
                uv[index]=new Vector2(u,distance/154);
            }
        }
        var renderer=Surface(root,Grid("Mirewild footpath",vertices,columns,points.Count,uv),
            TrailMaterial(),2);
        renderer.shadowCastingMode=ShadowCastingMode.Off;
        var filter=renderer.GetComponent<MeshFilter>();
        var area=filter.sharedMesh.bounds;
        area.Expand(1.5f);
        var surface=renderer.gameObject.AddComponent<TravelSurface>();
        surface.Configure(MirewildTrail.SurfaceId,Load<TravelSurfaceDefinition>("Assets/_DiceFree/Settings/Travel/Cornberg dirt road.asset"),filter,area);
        return surface;
    }

    private static Material TrailMaterial()
    {
        const int width=64,height=512;
        var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);
        var pixels=new Color32[width*height];
        for(int y=0;y<height;y++)
        for(int x=0;x<width;x++)
        {
            float across=Mathf.Abs(2*x/(width-1f)-1);
            float noise=Mathf.PerlinNoise(11+x*.091f,35+y*.094f);
            var color=Color.Lerp(new Color(.45f,.30f,.20f),new Color(.69f,.52f,.33f),noise);
            color.a=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.62f,1,across+(noise-.5f)*.22f)))*.95f;
            pixels[y*width+x]=color;
        }
        texture.SetPixels32(pixels);
        var path=Output+"/Mirewild path shoulders.png";
        File.WriteAllBytes(path,texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.wrapMode=TextureWrapMode.Clamp;
        importer.alphaIsTransparency=true;
        importer.SaveAndReimport();
        var material=new Material(Load<Material>(World+"World1TrailPresentation/Crimson Mire footpath detailed.mat"))
            { name="Mirewild packed earth",renderQueue=2990 };
        material.SetTexture("_BaseMap",Load<Texture2D>(path));
        Save(material);
        return material;
    }

    private static void PopulatePonds(Transform root)
    {
        var mats=Load<Material>(World+"World1TrailPresentation/Ironwater pool 1.mat");
        for(int i=0;i<Ponds.Length;i++)
        {
            var (center,radius)=Ponds[i];
            var material=new Material(mats){name="Mirewild pool "+(i+1)};
            material.SetVector("_Pond",new Vector4(center.x,center.y,radius.x,radius.y));
            Save(material);
            const int steps=46, rows=11;
            var vertices=new Vector3[(steps+1)*rows];
            for(int ring=0;ring<rows;ring++)
            for(int step=0;step<=steps;step++)
            {
                float angle=step*Mathf.PI*2/steps;
                var p=center+PoolEdge(angle,radius)*(ring/(rows-1f));
                vertices[ring*(steps+1)+step]=Floor(p)+Vector3.up*.121f;
                if(ring==rows-1&&step<steps&&step%4==0)
                {
                    var bank=center+PoolEdge(angle,radius)*1.12f;
                    Place(root,"Bank shale",World+"VisualBatch01/Models/rock_03.fbx",Floor(bank),Between(.34f,.83f),shale);
                }
            }
            Surface(root,Grid(material.name,vertices,steps+1,rows),material,2);
        }
    }

    private static Vector2 PoolEdge(float angle,Vector2 radius) => Vector2.Scale(radius,
        new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)))*(1+.07f*Mathf.Sin(angle*5+.8f));

    private static void PlantVerge(Transform root)
    {
        var plants=Group(root,"Moss, wetland grasses and sheltered forest",Vector3.zero);
        var trunks=new List<Vector2>();
        var ferns=Load<Mesh>(World+"WoodlandAndSettlements/Fern.asset");
        var grasses=Load<Mesh>(World+"WoodlandAndSettlements/Grass tuft.asset");
        var fernMat=Load<Material>(World+"NaturePass03/FernLeaves.mat");
        var grassMat=Load<Material>(World+"World1TrailPresentation/Sedge tips.mat");
        var fernMatrices=new List<Matrix4x4>();
        var grassMatrices=new List<Matrix4x4>();
        for(int i=0;i<390;i++)
        {
            var p=new Vector2(Between(-58,57),Between(165,216));
            float offset=TrailDistance(p);
            if(offset<2.3f || !TryFloor(p,out var at) || (p.y>195 && !InRidge(at,0)))continue;
            if(Ponds.Any(pond=>Vector2.Scale(p-pond.point,new Vector2(1/pond.radius.x,1/pond.radius.y)).sqrMagnitude<1.15f))continue;
            if(ForestMonsters.Any(home=>Vector2.Distance(home.point,p)<2.2f) ||
                Slimes.Any(home=>Vector2.Distance(home,p)<2.2f))continue;
            if(p.y>192 && offset>5 && i%7==0 && trunks.All(other=>Vector2.Distance(other,p)>5f))
            {
                trunks.Add(p);
                var tree=Place(plants,"Fernhollow pine",World+"CornbergBatch/Trees/Tree_"+(i%2==0?"Pine":"Twisted")+".fbx",at,Between(5.6f,8.5f),null);
                tree.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(7);
                continue;
            }
            if(offset<10 && i%13==0)
            {
                Place(plants,"Verge stone",World+"VisualBatch01/Models/rock_04.fbx",at,Between(.22f,.71f),shale);
                continue;
            }
            var matrix=Matrix4x4.TRS(at,Quaternion.Euler(0,Between(0,360),0),Vector3.one*Between(.71f,1.65f));
            (p.y>193 && i%3==0?fernMatrices:grassMatrices).Add(matrix);
        }
        foreach(var (center,radius) in Ponds)
        for(int i=0;i<27;i++)
        {
            var p=center+PoolEdge(i*2.399963f,radius)*Between(1.02f,1.23f);
            if(TrailDistance(p)<2.5f)continue;
            grassMatrices.Add(Matrix4x4.TRS(Floor(p),Quaternion.Euler(0,Between(0,360),0),Vector3.one*Between(1.4f,2.15f)));
        }
        BatchPlants(plants,"Woodland ferns",ferns,fernMatrices,fernMat);
        BatchPlants(plants,"Mire sedge beds",grasses,grassMatrices,grassMat);
        Place(plants,"Half-buried ridge root",World+"VisualBatch01/Models/root_02.fbx",Floor(new Vector2(-42,198)),.61f,roots);
        Place(plants,"Bogfall timber",World+"VisualBatch01/Models/log_01.fbx",Floor(new Vector2(21,172)),.62f,roots);
    }

    private static void BatchPlants(Transform root,string name,Mesh source,List<Matrix4x4> matrices,Material material)
    {
        var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};
        mesh.CombineMeshes(matrices.Select(matrix=>new CombineInstance{mesh=source,transform=matrix}).ToArray());
        Save(mesh);
        Surface(root,mesh,material,2).shadowCastingMode=ShadowCastingMode.Off;
    }

    private static void MakeSign(Transform root,Vector2 point,float heading,string text)
    {
        var at=Group(root,"Mirewild waypost",Floor(point));
        at.localRotation=Quaternion.Euler(0,heading,0);
        var timber=Load<Material>(World+"CrimsonMire/Weathered warning sign timber.mat");
        var brass=Load<Material>(World+"CrimsonMire/Warning sign bronze.mat");
        Block(at,"Timber post",new Vector3(0,1,0),new Vector3(.18f,2,.18f),timber);
        var board=Group(at,"Waypost board",new Vector3(0,1.78f,0));
        board.localRotation=Quaternion.Euler(-29,0,0);
        Block(board,"Bronze board rim",Vector3.zero,new Vector3(2.65f,.73f,.12f),brass);
        Block(board,"Timber face",new Vector3(0,0,.03f),new Vector3(2.54f,.62f,.11f),timber);
        var label=Group(board,"Waypost lettering",new Vector3(0,0,.093f)).gameObject.AddComponent<TextMesh>();
        label.transform.localRotation=Quaternion.Euler(0,180,0);
        label.text=text;
        label.anchor=TextAnchor.MiddleCenter;
        label.alignment=TextAlignment.Center;
        label.characterSize=.062f;
        label.fontSize=42;
        label.color=new Color(1,.87f,.65f);
        label.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
    }

    private static void Block(Transform root,string name,Vector3 at,Vector3 scale,Material material)
    {
        var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name=name;
        obj.layer=2;
        obj.transform.SetParent(root,false);
        obj.transform.localPosition=at;
        obj.transform.localScale=scale;
        Object.DestroyImmediate(obj.GetComponent<Collider>());
        obj.GetComponent<MeshRenderer>().sharedMaterial=material;
    }

    private static GameObject Place(Transform root,string name,string asset,Vector3 at,float height,Material material)
    {
        var model=(GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(asset),root);
        PrefabUtility.UnpackPrefabInstance(model,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        model.name=name;
        model.transform.SetPositionAndRotation(at,Quaternion.Euler(0,Between(0,360),0));
        foreach(var collider in model.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
        var renderers=model.GetComponentsInChildren<MeshRenderer>();
        Bounds BoundsOf()
        {
            var bounds=renderers[0].bounds;
            foreach(var renderer in renderers.Skip(1))bounds.Encapsulate(renderer.bounds);
            return bounds;
        }
        model.transform.localScale*=height/Mathf.Max(.01f,BoundsOf().size.y);
        var measured=BoundsOf();
        model.transform.position+=new Vector3(at.x-measured.center.x,at.y-measured.min.y,at.z-measured.center.z);
        foreach(var renderer in renderers)
        {
            renderer.gameObject.layer=2;
            if(material!=null)renderer.sharedMaterials=renderer.sharedMaterials.Select(_=>material).ToArray();
        }
        return model;
    }

    private static void RebuildNavigation()
    {
        var route=Object.FindObjectsByType<MonoBehaviour>().Single(item=>item.GetType().Name=="OgreRoadwardenRoute");
        using var serialized=new SerializedObject(route);
        var wreckage=serialized.FindProperty("wreckage");
        var closed=Enumerable.Range(0,wreckage.arraySize)
            .Select(i=>(GameObject)wreckage.GetArrayElementAtIndex(i).objectReferenceValue)
            .Where(piece=>piece.activeSelf).ToArray();
        try
        {
            foreach(var piece in closed)piece.SetActive(false);
            CornbergSceneBuilder.Rebake();
        }
        finally { foreach(var piece in closed)piece.SetActive(true); }
    }

    private static void VerifyTrail(TravelSurface trail)
    {
        var points=MirewildTrail.Points;
        for(int i=1;i<points.Count;i++)
        {
            var start=NavPoint(points[i-1]);
            var end=NavPoint(points[i]);
            RequirePath(start,end,Vector3.Distance(start,end)*1.6f+2);
            if(i<points.Count-1 && !trail.Contains(end))throw new InvalidOperationException("No travel effect at "+end);
        }
        RequirePath(NavPoint(new Vector2(-43,264)),NavPoint(points[^1]),180);
    }

    private static void Spawn(Transform template,Transform parent,Vector2 point)
    {
        if(template==null)throw new InvalidOperationException("Missing monster template for "+point);
        var home=NavPoint(point);
        RequirePath(NavPoint(MirewildTrail.Points[4]),home,100);
        var actor=Object.Instantiate(template.gameObject,parent);
        actor.name=template.name;
        actor.transform.position=home;
        actor.transform.rotation=Quaternion.Euler(0,Between(0,360),0);
        actor.GetComponent<AggroBehaviour>().ConfigureHomePosition(home);
    }

    private static void RequirePath(Vector3 start,Vector3 end,float maximum)
    {
        var path=new NavMeshPath();
        if(!NavMesh.CalculatePath(start,end,1,path)||path.status!=NavMeshPathStatus.PathComplete)
            throw new InvalidOperationException("No direct route from "+start+" to "+end);
        float distance=0;
        var corners=path.corners;
        for(int i=1;i<corners.Length;i++)distance+=Vector3.Distance(corners[i-1],corners[i]);
        if(distance>maximum)throw new InvalidOperationException($"Unexpected route detour from {start} to {end}: {distance:F1} m.");
    }
    private static Vector3 NavPoint(Vector2 p)
    {
        if(!NavMesh.SamplePosition(Floor(p),out var hit,1.3f,1))throw new InvalidOperationException("Missing navmesh at "+p);
        return hit.position;
    }
    private static bool TryFloor(Vector2 p,out Vector3 result)
    {
        var ray=new Ray(new Vector3(p.x,70,p.y),Vector3.down);
        result=new Vector3(p.x,float.NegativeInfinity,p.y);
        foreach(var floor in Floors)
            if(floor.Raycast(ray,out var hit,100) && hit.point.y>result.y)result=hit.point;
        return float.IsFinite(result.y);
    }
    private static Vector3 Floor(Vector2 p)=>TryFloor(p,out var result)?result:
        throw new InvalidOperationException("No trail ground at "+p);
    private static float TrailDistance(Vector2 p,int start=1)
    {
        float distance=float.PositiveInfinity;
        var points=MirewildTrail.Points;
        for(int i=start;i<points.Count;i++)distance=Mathf.Min(distance,MirewildTrail.DistanceToSegment(p,points[i-1],points[i]));
        return distance;
    }
    private static float Between(float min,float max)=>Mathf.Lerp(min,max,(float)Random.NextDouble());

    private static Mesh Grid(string name,Vector3[] vertices,int columns,int rows,Vector2[] uv=null,bool[] onGround=null)
    {
        var indices=new List<int>((columns-1)*(rows-1)*6);
        for(int row=1;row<rows;row++)
        for(int col=1;col<columns;col++)
        {
            int i=row*columns+col;
            AddTriangle(i-columns-1,i-columns,i-1);
            AddTriangle(i-columns,i,i-1);
        }
        void AddTriangle(int a,int b,int c)
        {
            if(onGround!=null && (!onGround[a] || !onGround[b] || !onGround[c]))return;
            indices.Add(a);
            if(Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]).y>=0)
            { indices.Add(b);indices.Add(c); }
            else { indices.Add(c);indices.Add(b); }
        }
        var mesh=new Mesh{name=name,vertices=vertices,triangles=indices.ToArray(),
            uv=uv??vertices.Select(v=>new Vector2(v.x,v.z)*.105f).ToArray()};
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Save(mesh);
        return mesh;
    }
    private static MeshRenderer Surface(Transform root,Mesh mesh,Material material,int layer)
    {
        var group=Group(root,mesh.name,Vector3.zero).gameObject;
        group.layer=layer;
        group.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=group.AddComponent<MeshRenderer>();
        renderer.sharedMaterial=material;
        return renderer;
    }
    private static Transform Find(string name)=>GameObject.Find(name)?.transform??throw new InvalidOperationException("Missing "+name);
    private static Transform Group(Transform parent,string name,Vector3 at)
    {
        var obj=new GameObject(name);
        obj.layer=2;
        obj.transform.SetParent(parent,false);
        obj.transform.localPosition=at;
        return obj.transform;
    }
    private static T Load<T>(string path) where T:Object => AssetDatabase.LoadAssetAtPath<T>(path)??throw new IOException("Missing "+path);
    private static void Save(Object item)=>AssetDatabase.CreateAsset(item,Output+"/"+item.name+(item is Material?".mat":".asset"));
}
