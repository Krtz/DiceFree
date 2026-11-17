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

public static class ReedscaleLinkAuthoring
{
    private const string World = "Assets/_DiceFree/Art/World/";
    private const string Art = World + "ReedscaleHighlands";
    private const string RootName = "Reedscale Highlands - Briarback pass";
    private const int FirstSeamRow = 20, LastSeamRow = 48, Columns = 126;
    private const int Rows = LastSeamRow - FirstSeamRow + 1;
    private static readonly System.Random Random = new(53371);
    private static readonly List<Object> Changed = new();
    private static readonly Vector2 Arborium = new(98.04f,304.74f);
    private static readonly (Vector2 point, bool bow, int level)[] Patrols =
    {
        (new(62,400),false,39), (new(73,429),true,40), (new(87,413),false,40),
        (new(98,441),false,41), (new(118,419),true,40), (new(129,450),false,41),
        (new(145,424),true,41), (new(149,445),false,40)
    };
    private static Material moss, slate, timber, banner;
    private static Vector3[] ground;

    public static void Batch()
    {
        var scene=EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
        if(AssetDatabase.IsValidFolder(Art))throw new InvalidOperationException("Reedscale Highlands already exists.");
        AssetDatabase.CreateFolder(World.TrimEnd('/'),"ReedscaleHighlands");
        Changed.Clear();
        var eastwatch=Find("Eastwatch hillside").GetComponent<MeshFilter>();
        moss=eastwatch.GetComponent<MeshRenderer>().sharedMaterial;
        slate=Load<Material>(World+"World1TrailPresentation/Wet blue shale.mat");
        timber=Load<Material>(World+"ReedscaleLookout/Hewn larch.mat");
        banner=Load<Material>(World+"ReedscaleLookout/Faded ochre canvas.mat");
        Physics.SyncTransforms();
        var root=Group(null,RootName,Vector3.zero);
        BuildHighlands(root,eastwatch);
        Physics.SyncTransforms();
        TrimEastwatchBackdrop();
        BuildSlopes(root);
        var travel=BuildRoad(root);
        Dress(root);
        Waypost(root,new Vector2(35,406.8f),215,"BRIARBACK");
        Waypost(root,new Vector2(247.5f,380),227,"REEDSCALE");
        RebuildNavigation();
        var nav=NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            VerifyRoad(travel);
            AddPatrols(root);
        }
        finally {nav.Remove();}
        EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene))throw new IOException("Could not save the Reedscale Highlands.");
        AssetDatabase.SaveAssets();
        File.WriteAllLines("Logs/ReedscaleLinkIds.txt",Changed.Select(item=>GlobalObjectId.GetGlobalObjectIdSlow(item).targetObjectId.ToString()));
        Debug.Log("DICEFREE_REEDSCALE_LINK_OK");
    }

    private static void BuildHighlands(Transform root,MeshFilter eastwatch)
    {
        var seam=eastwatch.sharedMesh.vertices;
        ground=new Vector3[Columns*Rows];
        for(int row=0;row<Rows;row++)
        for(int col=0;col<Columns;col++)
        {
            float t=col/(Columns-1f), side=row/(Rows-1f)*2-1;
            var start=eastwatch.transform.TransformPoint(seam[(row+FirstSeamRow)*57+56]);
            float x=Mathf.Lerp(start.x,202,t);
            float center=CenterZ(x);
            float width=Mathf.Lerp(14,29,Smooth(Mathf.InverseLerp(48,87,x)));
            width=Mathf.Lerp(width,12,Smooth(Mathf.InverseLerp(138,185,x)));
            float z=Mathf.Lerp(start.z,center+side*width,Smooth(t*6.6f));
            z+=Mathf.Sin(x*.23f)*Mathf.Pow(Mathf.Abs(side),2)*1.5f*Mathf.Sin(t*Mathf.PI);
            float minimumNorth=Arborium.y+Mathf.Sqrt(Mathf.Max(0,86*86-(x-Arborium.x)*(x-Arborium.x)));
            if(Mathf.Abs(x-Arborium.x)<86)
                z=Mathf.Max(z,minimumNorth);
            float foothill=0;
            if(x>174&&Physics.Raycast(new Vector3(x,45,z),Vector3.down,out var landing,70,1<<8))
                foothill=landing.point.y-.095f;
            float flatten=Smooth(Mathf.InverseLerp(153,199,x));
            float flatHeight=10.2f+.69f*Mathf.Sin(x*.083f)+.28f*Mathf.Cos(x*.14f+z*.08f);
            float bank=Smooth(Mathf.InverseLerp(.36f,1,Mathf.Abs(side)));
            float rise=(2.1f+1.55f*Mathf.Sin(x*.132f+side))*bank;
            float y=Mathf.Lerp(start.y,flatHeight+rise,Smooth(Mathf.InverseLerp(48,72,x)));
            y=Mathf.Lerp(y,foothill,flatten);
            ground[row*Columns+col]=new Vector3(x,y,z);
        }
        var mesh=Grid("Reedscale highlands walkable terrain",ground,Columns,Rows);
        var renderer=Surface(root,mesh,moss,8);
        renderer.gameObject.AddComponent<MeshCollider>().sharedMesh=mesh;
        mesh.SetPreBakeCollisionMesh(false,true);
        EditorUtility.SetDirty(mesh);
    }

    private static float CenterZ(float x)
    {
        var points=ReedscalePass.Points;
        for(int i=1;i<points.Count;i++)
            if(x<=points[i].x)return Mathf.Lerp(points[i-1].y,points[i].y,Mathf.InverseLerp(points[i-1].x,points[i].x,x));
        return points[^1].y;
    }

    private static void TrimEastwatchBackdrop()
    {
        var filter=Object.FindObjectsByType<MeshFilter>().Single(item=>item.name=="Eastwatch outer slope");
        var mesh=Object.Instantiate(filter.sharedMesh);
        mesh.name="Eastwatch scenery outside highlands";
        var vertices=mesh.vertices.Select(filter.transform.TransformPoint).ToArray();
        bool Overlap(Vector3 p)=>p.x>40&&p.x<95&&p.z>388&&p.z<453&&
            Mathf.Abs(p.z-CenterZ(p.x))<Mathf.Lerp(15,32,(p.x-44)/42);
        var keep=new List<int>();
        var indices=mesh.triangles;
        for(int i=0;i<indices.Length;i+=3)
        {
            var a=vertices[indices[i]];var b=vertices[indices[i+1]];var c=vertices[indices[i+2]];
            if(Overlap((a+b+c)/3)||Overlap(a)||Overlap(b)||Overlap(c))continue;
            keep.Add(indices[i]);keep.Add(indices[i+1]);keep.Add(indices[i+2]);
        }
        mesh.SetTriangles(keep,0);
        Save(mesh);
        filter.sharedMesh=mesh;
        Changed.Add(filter);
        foreach(Transform prop in filter.transform.parent)
        {
            if(prop==filter.transform || !Overlap(prop.position))continue;
            if(!prop.gameObject.activeSelf)continue;
            prop.gameObject.SetActive(false);
            Changed.Add(prop.gameObject);
        }
    }

    private static void BuildSlopes(Transform root)
    {
        const int bands=10;
        for(int side=0;side<2;side++)
        {
            var vertices=new Vector3[Columns*bands];
            for(int col=0;col<Columns;col++)
            for(int band=0;band<bands;band++)
            {
                float t=band/(bands-1f);
                var p=ground[(side==0?0:Rows-1)*Columns+col];
                float distance=(side==0?7:16)*t*Mathf.Sin(Mathf.PI*col/(Columns-1f));
                p.z+=(side==0?-1:1)*distance;
                p.y-=.045f+t*t*10;
                float north=Arborium.y+Mathf.Sqrt(Mathf.Max(0,85*85-(p.x-Arborium.x)*(p.x-Arborium.x)));
                if(Mathf.Abs(p.x-Arborium.x)<85)p.z=Mathf.Max(north,p.z);
                vertices[col*bands+band]=p;
            }
            Surface(root,Grid(side==0?"Capital-facing escarpment":"Northern highland banks",vertices,bands,Columns),slate,2);
        }
    }

    private static TravelSurface BuildRoad(Transform root)
    {
        var stops=ReedscalePass.Points;
        var points=new List<Vector2>();
        for(int i=1;i<stops.Count;i++)
        {
            var a=stops[Mathf.Max(0,i-2)];var b=stops[i-1];var c=stops[i];var d=stops[Mathf.Min(stops.Count-1,i+1)];
            int samples=Mathf.CeilToInt(Vector2.Distance(b,c)/.66f);
            for(int j=0;j<samples;j++)
            {
                float t=j/(float)samples;
                points.Add(.5f*(2*b+(c-a)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));
            }
        }
        points.Add(stops[^1]);
        const int columns=9;
        var vertices=new Vector3[columns*points.Count];
        var uv=new Vector2[vertices.Length];
        float distance=0;
        for(int row=0;row<points.Count;row++)
        {
            if(row>0)distance+=Vector2.Distance(points[row-1],points[row]);
            var along=(points[Mathf.Min(row+1,points.Count-1)]-points[Mathf.Max(0,row-1)]).normalized;
            var across=new Vector2(-along.y,along.x);
            float width=1.88f+.16f*Mathf.Sin(distance*.41f);
            for(int col=0;col<columns;col++)
            {
                int index=row*columns+col;
                float u=col/(columns-1f);
                vertices[index]=Floor(points[row]+across*((2*u-1)*width))+Vector3.up*.147f;
                uv[index]=new Vector2(u,distance/270);
            }
        }
        var mesh=Grid("Reedscale highland trail",vertices,columns,points.Count,uv);
        var material=new Material(Load<Material>(World+"MirewildConnection/Mirewild packed earth.mat"))
            {name="Reedscale highland trail gravel",renderQueue=2990};
        material.SetColor("_Tint",new Color(.89f,.94f,.86f));
        Save(material);
        var renderer=Surface(root,mesh,material,2);
        renderer.shadowCastingMode=ShadowCastingMode.Off;
        var filter=renderer.GetComponent<MeshFilter>();
        var bounds=mesh.bounds; bounds.Expand(1.4f);
        var travel=renderer.gameObject.AddComponent<TravelSurface>();
        travel.Configure(ReedscalePass.SurfaceId,Load<TravelSurfaceDefinition>("Assets/_DiceFree/Settings/Travel/Cornberg dirt road.asset"),filter,bounds);
        return travel;
    }

    private static void Dress(Transform root)
    {
        var props=Group(root,"Highland pines and watch camps",Vector3.zero);
        var points=ReedscalePass.Points;
        var trees=new List<Vector2>();
        var ferns=new List<CombineInstance>();
        var fern=Load<Mesh>(World+"WoodlandAndSettlements/Fern.asset");
        for(int i=0;i<220;i++)
        {
            float x=Between(52,177);
            var p=new Vector2(x,CenterZ(x)+Between(-23,24));
            float distance=float.PositiveInfinity;
            for(int n=1;n<points.Count;n++)
                distance=Mathf.Min(distance,MirewildTrail.DistanceToSegment(p,points[n-1],points[n]));
            if(distance<2.9f||!TryFloor(p,out var at)||Patrols.Any(home=>Vector2.Distance(home.point,p)<3))continue;
            if(i%4==0&&distance>5&&trees.All(t=>Vector2.Distance(t,p)>4.7f))
            {
                trees.Add(p);
                string type=i%3==0?"Pine":"Spruce";
                var tree=Model(props,World+"CornbergBatch/Trees/Tree_"+type+".fbx",at,Between(6.2f,9.7f),null);
                tree.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(7);
            }
            else if(i%5==0)
                Model(props,World+"VisualBatch01/Models/rock_04.fbx",at,Between(.38f,1.35f),slate);
            else
                ferns.Add(new CombineInstance{mesh=fern,transform=Matrix4x4.TRS(at,Quaternion.Euler(0,Between(0,360),0),Vector3.one*Between(.76f,1.34f))});
        }
        var patch=new Mesh{name="Highland fern beds"};
        patch.CombineMeshes(ferns.ToArray());
        Save(patch);
        Surface(props,patch,Load<Material>(World+"NaturePass03/FernLeaves.mat"),2).shadowCastingMode=ShadowCastingMode.Off;
        var camp=Find("Reedscale lookout camp");
        CampProp(props,camp.Find("Sentry hide shelter"),new Vector2(82,435),45);
        CampProp(props,camp.Find("Reedscale split banner"),new Vector2(79,429),28);
        CampProp(props,camp.Find("Reedscale split banner"),new Vector2(121,439),55);
        CampProp(props,camp.Find("Sentry hide shelter"),new Vector2(130,425),-35);
        CampProp(props,camp.Find("Reedscale split banner"),new Vector2(160,442),54);
        Model(props,World+"VisualBatch01/Models/crate_02.fbx",Floor(new Vector2(84,433)),.96f,null);
        Model(props,World+"VisualBatch01/Models/barrel_03.fbx",Floor(new Vector2(128,426)),.88f,null);
        foreach(var p in new[]{new Vector2(100,424),new Vector2(115,440),new Vector2(145,430),new Vector2(170,440)})
            Model(props,World+"VisualBatch01/Models/rock_05.fbx",Floor(p),Between(.78f,1.67f),slate);
    }

    private static void CampProp(Transform parent,Transform template,Vector2 point,float yaw)
    {
        var prop=Object.Instantiate(template.gameObject,parent);
        prop.name=template.name;
        prop.transform.SetPositionAndRotation(Floor(point),Quaternion.Euler(0,yaw,0));
        foreach(var collider in prop.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(collider);
    }

    private static void Waypost(Transform root,Vector2 at,float yaw,string text)
    {
        var sign=Group(root,"Highland waypost",Floor(at));
        sign.localRotation=Quaternion.Euler(0,yaw,0);
        Block(sign,"Larch post",new Vector3(0,.94f,0),new Vector3(.16f,1.88f,.16f),timber);
        var board=Group(sign,"Trail board",new Vector3(0,1.74f,0));
        board.localRotation=Quaternion.Euler(-29,0,0);
        Block(board,"Larch trail sign",Vector3.zero,new Vector3(2.32f,.55f,.12f),timber);
        Block(board,"Ochre lower band",new Vector3(0,-.21f,.069f),new Vector3(2.23f,.09f,.025f),banner);
        var label=Group(board,"Trail lettering",new Vector3(0,.036f,.074f)).gameObject.AddComponent<TextMesh>();
        label.transform.localRotation=Quaternion.Euler(0,180,0);
        label.text=text;label.fontSize=42;label.characterSize=.057f;
        label.anchor=TextAnchor.MiddleCenter;
        label.color=new Color(1,.87f,.65f);
        label.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
    }

    private static void Block(Transform parent,string name,Vector3 at,Vector3 size,Material material)
    {
        var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name=name;obj.layer=2;
        obj.transform.SetParent(parent,false);
        obj.transform.localPosition=at;obj.transform.localScale=size;
        Object.DestroyImmediate(obj.GetComponent<Collider>());
        obj.GetComponent<MeshRenderer>().sharedMaterial=material;
    }

    private static GameObject Model(Transform parent,string path,Vector3 at,float height,Material material)
    {
        var prop=(GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(path),parent);
        PrefabUtility.UnpackPrefabInstance(prop,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        prop.transform.SetPositionAndRotation(at,Quaternion.Euler(0,Between(0,360),0));
        foreach(var collider in prop.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(collider);
        var renderers=prop.GetComponentsInChildren<MeshRenderer>();
        Bounds Measure()
        {
            var bounds=renderers[0].bounds;
            foreach(var renderer in renderers.Skip(1))bounds.Encapsulate(renderer.bounds);
            return bounds;
        }
        prop.transform.localScale*=height/Mathf.Max(.01f,Measure().size.y);
        var measured=Measure();
        prop.transform.position+=new Vector3(at.x-measured.center.x,at.y-measured.min.y,at.z-measured.center.z);
        foreach(var renderer in renderers)
        {
            renderer.gameObject.layer=2;
            if(material!=null)renderer.sharedMaterials=renderer.sharedMaterials.Select(_=>material).ToArray();
        }
        return prop;
    }

    private static void RebuildNavigation()
    {
        var route=Object.FindObjectsByType<MonoBehaviour>().Single(component=>component.GetType().Name=="OgreRoadwardenRoute");
        using var data=new SerializedObject(route);
        var wreckage=data.FindProperty("wreckage");
        var closed=Enumerable.Range(0,wreckage.arraySize)
            .Select(i=>(GameObject)wreckage.GetArrayElementAtIndex(i).objectReferenceValue)
            .Where(piece=>piece.activeSelf).ToArray();
        try
        {
            foreach(var piece in closed)piece.SetActive(false);
            CornbergSceneBuilder.Rebake();
        }
        finally {foreach(var piece in closed)piece.SetActive(true);}
    }

    private static void VerifyRoad(TravelSurface travel)
    {
        var points=ReedscalePass.Points;
        for(int i=1;i<points.Count;i++)
        {
            var start=NavPoint(points[i-1]);
            var end=NavPoint(points[i]);
            RequirePath(start,end,Vector3.Distance(start,end)*1.75f+3);
            if(i<points.Count-1&&!travel.Contains(end))throw new InvalidOperationException("Travel surface gap near "+points[i]);
        }
        RequirePath(NavPoint(new Vector2(11,394)),NavPoint(points[^1]),305);
        RequirePath(NavPoint(points[^1]),NavPoint(new Vector2(264,320)),155);
    }

    private static void AddPatrols(Transform root)
    {
        var defenders=Find("Reedscale defenders");
        var templates=defenders.GetComponentsInChildren<ActorStats>();
        var guards=Group(root,"Highland Reedscale defenders",Vector3.zero);
        foreach(var (point,bow,level) in Patrols)
        {
            var home=NavPoint(point);
            RequirePath(home,NavPoint(new Vector2(96,429)),105);
            string role=bow?"ridgebow-sentry":"spear-guard";
            var template=templates.First(actor=>actor.Definition.stableId=="enemy.reedscale."+role);
            var copy=Object.Instantiate(template.gameObject,guards);
            copy.name=template.Definition.displayName+" (Lv "+level+")";
            copy.GetComponent<ActorStats>().Configure(template.Definition,level);
            copy.transform.SetPositionAndRotation(home,Quaternion.Euler(0,Between(0,360),0));
            copy.GetComponent<AggroBehaviour>().ConfigureHomePosition(home);
        }
    }

    private static Vector3 NavPoint(Vector2 point)
    {
        if(!NavMesh.SamplePosition(Floor(point),out var hit,1.5f,1))throw new InvalidOperationException("No navigation near "+point);
        return hit.position;
    }
    private static void RequirePath(Vector3 a,Vector3 b,float maximum)
    {
        var path=new NavMeshPath();
        if(!NavMesh.CalculatePath(a,b,1,path)||path.status!=NavMeshPathStatus.PathComplete)
            throw new InvalidOperationException("Incomplete trail from "+a+" to "+b);
        float length=0;
        var corners=path.corners;
        for(int i=1;i<corners.Length;i++)length+=Vector3.Distance(corners[i-1],corners[i]);
        if(length>maximum)throw new InvalidOperationException($"Trail detour from {a} to {b}: {length:F1} m.");
    }
    private static bool TryFloor(Vector2 p,out Vector3 at)
    {
        bool found=Physics.Raycast(new Vector3(p.x,65,p.y),Vector3.down,out var hit,95,1<<8);
        at=hit.point;
        return found;
    }
    private static Vector3 Floor(Vector2 p)=>TryFloor(p,out var at)?at:throw new InvalidOperationException("Missing trail ground at "+p);
    private static float Between(float min,float max)=>Mathf.Lerp(min,max,(float)Random.NextDouble());
    private static float Smooth(float t)=>Mathf.SmoothStep(0,1,t);
    private static Transform Find(string name)=>GameObject.Find(name)?.transform??throw new InvalidOperationException("Missing "+name);
    private static T Load<T>(string path) where T:Object=>AssetDatabase.LoadAssetAtPath<T>(path)??throw new IOException("Missing "+path);
    private static void Save(Object item)=>AssetDatabase.CreateAsset(item,Art+"/"+item.name+(item is Material?".mat":".asset"));
    private static Transform Group(Transform parent,string name,Vector3 at)
    {
        var node=new GameObject(name);
        node.layer=2;
        node.transform.SetParent(parent,false);
        node.transform.localPosition=at;
        return node.transform;
    }
    private static MeshRenderer Surface(Transform parent,Mesh mesh,Material material,int layer)
    {
        var node=Group(parent,mesh.name,Vector3.zero).gameObject;
        node.layer=layer;
        node.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=node.AddComponent<MeshRenderer>();
        renderer.sharedMaterial=material;
        return renderer;
    }
    private static Mesh Grid(string name,Vector3[] vertices,int columns,int rows,Vector2[] uv=null)
    {
        var triangles=new List<int>();
        for(int row=1;row<rows;row++)
        for(int col=1;col<columns;col++)
        {
            int i=row*columns+col;
            Triangle(i-columns-1,i-columns,i-1);
            Triangle(i-columns,i,i-1);
        }
        void Triangle(int a,int b,int c)
        {
            triangles.Add(a);
            if(Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]).y>=0)
            {triangles.Add(b);triangles.Add(c);}
            else {triangles.Add(c);triangles.Add(b);}
        }
        var mesh=new Mesh{name=name,vertices=vertices,triangles=triangles.ToArray(),
            uv=uv??vertices.Select(p=>new Vector2(p.x,p.z)*.105f).ToArray()};
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Save(mesh);
        return mesh;
    }
}
