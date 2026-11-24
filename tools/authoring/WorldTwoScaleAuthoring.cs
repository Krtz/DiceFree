using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static partial class AshfallReachExpansionAuthoring
{
    const string ScaleOutput="Assets/_DiceFree/Settings/AshfallReach/FullWorld/";
    static readonly IReadOnlyList<Vector2>[] FullRoutes={AshfallReach.NeedlePath,AshfallReach.CrownPath,AshfallReach.SouthernCircuit,AshfallReach.WesternTrail};

    public static void ScaleToWorldOne()
    {
        var one=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
        var bounds=AllIn<OverworldMapBounds>(one).Single().Bounds;
        if(Mathf.Abs(bounds.width-AshfallReach.FaceWidth)>.01f||Mathf.Abs(bounds.height-AshfallReach.FaceHeight)>.01f)
            throw new Exception("World One bounds changed; update the shared face dimensions before authoring.");
        float healing=AllIn<HealingArea>(one).Single(a=>a.name=="Cornberg magical well healing area").HpPerSecond;
        LoadMaterials();
        if(!AssetDatabase.IsValidFolder(ScaleOutput.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/_DiceFree/Settings/AshfallReach","FullWorld");
        var scene=EditorSceneManager.OpenScene(ScenePath);
        var root=scene.GetRootGameObjects().Single(g=>g.GetComponent<WorldNavigation>()!=null).transform;
        if(root.Find("World Two full face")!=null)throw new Exception("Full-world pass already present; preserve authored work.");
        var expansion=root.Find("Cinderwatch and the two pips");
        var full=Group(root,"World Two full face",Vector3.zero);
        var ground=root.Find("Slate ridges and alpine heath");
        ground.GetComponent<MeshFilter>().sharedMesh=FullTerrain(2,"World Two full face terrain");
        ground.GetComponent<MeshCollider>().sharedMesh=FullTerrain(2,"World Two full face collision");
        ground.GetComponent<MeshRenderer>().sharedMaterial=BlendedGround();
        var map=root.GetComponent<OverworldMapBounds>()??root.gameObject.AddComponent<OverworldMapBounds>();map.Configure(AshfallReach.Bounds);

        var needle=expansion.Find("Basalt Needle");needle.localPosition=Ground(AshfallReach.BasaltNeedle);needle.localScale=Vector3.one*3.4f;
        var crown=expansion.Find("Ember Crown");crown.localPosition=Ground(AshfallReach.EmberCrown)+Vector3.up*3.9f;crown.localScale=Vector3.one*4.4f;
        CrownFooting(full,crown);
        expansion.Find("BASALT NEEDLE").localPosition=Ground(AshfallReach.NeedlePath[^1]+new Vector2(2,1));
        expansion.Find("EMBER CROWN").localPosition=Ground(AshfallReach.CrownPath[^1]+new Vector2(-2,0));
        foreach(var name in new[]{"Footpath to the Basalt Needle","Footpath to Ember Crown"})
            if(expansion.Find(name) is Transform previous)Object.DestroyImmediate(previous.gameObject);
        Road(full,"Road to the Basalt Needle",AshfallReach.NeedlePath,2.15f);
        Road(full,"Road to Ember Crown",AshfallReach.CrownPath,2.65f);
        Road(full,"Southern world circuit",AshfallReach.SouthernCircuit,2.15f);
        Road(full,"Western river road",AshfallReach.WesternTrail,2.0f);
        foreach(Transform road in full.Cast<Transform>().Where(t=>t.GetComponent<MeshFilter>()!=null&&t.name.Contains("oad")||t.name=="Southern world circuit"))
        {
            var filter=road.GetComponent<MeshFilter>();if(filter==null)continue;
            road.gameObject.AddComponent<TravelSurface>().Configure("travel.ashfall."+road.name,
                Load<TravelSurfaceDefinition>("Assets/_DiceFree/Settings/Travel/Cornberg dirt road.asset"),filter,new Bounds(Vector3.zero,Vector3.one*10000));
        }
        var vale=expansion.Find("Steamroot Vale");
        Object.DestroyImmediate(vale.Find("Cornberg river continuing across the south face").gameObject);
        BuildRiver(vale);
        var bridge=Object.Instantiate(vale.Find("Willowford timber bridge").gameObject,full);
        bridge.name="Southern Steamroot timber bridge";bridge.transform.localPosition=new Vector3(AshfallReach.RiverX(-318),1.10f,-318);

        var spring=expansion.Find("Cinderwatch/Cinderwatch spring");
        (spring.GetComponent<HealingArea>()??spring.gameObject.AddComponent<HealingArea>()).Configure(healing*5,5);
        var willow=vale.Find("Willowford/Willowford spring");
        (willow.GetComponent<HealingArea>()??willow.gameObject.AddComponent<HealingArea>()).Configure(healing*5,5);
        int relocated=ClearRoutes(root,full);
        ClearLegacyRidgePaths(root);
        int trees=DressFullFace(full);
        AddWorldRim(full);
        var navigation=BakeFullFace(root);
        var instance=NavMesh.AddNavMeshData(navigation);
        int actors=0;
        try
        {
            var start=FullNav(AshfallReach.Cinderwatch);
            foreach(var route in FullRoutes)foreach(var point in route)FullPath(start,point);
            foreach(var p in new[]{new Vector2(170,-550),new Vector2(-226,-550),new Vector2(165,15),new Vector2(-230,10)})FullPath(start,p);
            actors=PopulateFullFace(full,start,root);
            FullPath(start,new Vector2(-145,-64));FullPath(start,new Vector2(-96,-31));
        }
        finally{instance.Remove();}
        root.GetComponent<WorldNavigation>().Configure(navigation);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        var report=new FullFaceReport{worldOneWidth=bounds.width,worldOneHeight=bounds.height,worldTwoWidth=AshfallReach.FaceWidth,
            worldTwoHeight=AshfallReach.FaceHeight,needle=AshfallReach.BasaltNeedle,crown=AshfallReach.EmberCrown,
            needleScale=3.4f,crownScale=4.4f,cornbergHealing=healing,cinderwatchHealing=healing*5,newTrees=trees,newPatrols=actors,relocatedTrees=relocated};
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/WorldTwoFullFaceAuthoring.json",JsonUtility.ToJson(report,true));
        WorldFacePreviewAuthoring.Batch();
        Debug.Log("DICEFREE_WORLD_TWO_FULL_FACE_OK");
    }
    public static void RefreshFullFacePresentation()
    {
        LoadMaterials();var scene=EditorSceneManager.OpenScene(ScenePath);
        var root=scene.GetRootGameObjects().Single(g=>g.GetComponent<WorldNavigation>()!=null).transform;
        ClearLegacyRidgePaths(root);
        foreach(var name in new[]{"Basalt Needle","Ember Crown"})
        {
            var landmark=root.GetComponentsInChildren<Transform>(true).Single(t=>t.name==name);
            var occluder=landmark.GetComponent<TreeCameraOccluder>();if(occluder!=null)Object.DestroyImmediate(occluder);
        }
        var material=BlendedGround();root.Find("Slate ridges and alpine heath").GetComponent<MeshRenderer>().sharedMaterial=material;
        File.WriteAllBytes("Logs/WorldTwoFullFaceGround.png",((Texture2D)material.GetTexture("_BaseMap")).EncodeToPNG());
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    static void ClearLegacyRidgePaths(Transform root)
    {
        var backdrop=root.Find("High ridges beyond Ashfall Reach");if(backdrop==null)return;
        foreach(var child in backdrop.Cast<Transform>().ToArray())
        {
            var renderer=child.GetComponent<Renderer>();
            if(renderer==null||child.GetComponentsInChildren<Collider>().Length!=0)continue;
            var bounds=renderer.bounds;var p=new Vector2(bounds.center.x-AshfallReach.Origin.x,bounds.center.z-AshfallReach.Origin.z);
            if(RouteDistance(p)<5+Mathf.Max(bounds.size.x,bounds.size.z)*.5f)Object.DestroyImmediate(child.gameObject);
        }
    }
    [Serializable]class FullFaceReport{public float worldOneWidth,worldOneHeight,worldTwoWidth,worldTwoHeight,needleScale,crownScale,cornbergHealing,cinderwatchHealing;public Vector2 needle,crown;public int newTrees,newPatrols,relocatedTrees;}
    static T[] AllIn<T>(Scene scene) where T:Component=>scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static Mesh FullTerrain(float stride,string name)
    {
        var rect=AshfallReach.LocalBounds;int columns=Mathf.CeilToInt(rect.width/stride)+1,rows=Mathf.CeilToInt(rect.height/stride)+1;
        var vertices=new Vector3[columns*rows];var uv=new Vector2[vertices.Length];var indices=new int[(columns-1)*(rows-1)*6];int index=0;
        for(int z=0;z<rows;z++)for(int x=0;x<columns;x++)
        {
            int i=z*columns+x;float px=rect.xMin+rect.width*x/(columns-1),pz=rect.yMin+rect.height*z/(rows-1);
            vertices[i]=Ground(px,pz);uv[i]=new Vector2(x/(float)(columns-1),z/(float)(rows-1));
            if(x==columns-1||z==rows-1)continue;
            foreach(int v in new[]{i,i+columns,i+1,i+1,i+columns,i+columns+1})indices[index++]=v;
        }
        var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32,vertices=vertices,uv=uv};mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();
        return Save(mesh,ScaleOutput+name+".asset");
    }
    static void CrownFooting(Transform parent,Transform crown)
    {
        // Join the enlarged, irregular cone lip to the actual ground. Its original
        // rim was built for the small proof-of-concept terrain.
        const int sides=72;var v=new Vector3[sides*2];var uv=new Vector2[v.Length];var triangles=new int[sides*6];
        for(int i=0;i<sides;i++)
        {
            float a=i*Mathf.PI*2/sides,crags=Mathf.Sin(a*5)*.58f+Mathf.Cos(a*3)*.41f;
            float r=(10.5f+crags*.87f)*4.4f;var direction=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a)*.86f);
            v[i*2]=crown.localPosition+direction*r+Vector3.up*((-.85f+crags)*4.4f+.02f);
            var outer=AshfallReach.EmberCrown+new Vector2(direction.x,direction.z)*(r+12);v[i*2+1]=Ground(outer)+Vector3.up*.03f;
            uv[i*2]=new Vector2(i*.5f,0);uv[i*2+1]=new Vector2(i*.5f,2);
            int p=i*2,n=(i+1)%sides*2;int k=i*6;triangles[k]=p;triangles[k+1]=p+1;triangles[k+2]=n;triangles[k+3]=p+1;triangles[k+4]=n+1;triangles[k+5]=n;
        }
        var mesh=new Mesh{name="Ember Crown talus apron",vertices=v,uv=uv,triangles=triangles};mesh.RecalculateNormals();mesh=Save(mesh,ScaleOutput+mesh.name+".asset");
        var filter=Surface(parent,"Crown foot joins the world",mesh,warmRock);filter.gameObject.layer=9;filter.gameObject.AddComponent<MeshCollider>().sharedMesh=mesh;
    }
    static float RouteDistance(Vector2 p)=>FullRoutes.Min(route=>DistanceToRoad(p,route));
    static int ClearRoutes(Transform root,Transform full)
    {
        int moved=0;
        foreach(var node in root.GetComponentsInChildren<Transform>(true).ToArray())
        {
            if(!PrefabUtility.IsAnyPrefabInstanceRoot(node.gameObject)||node.IsChildOf(full))continue;
            string path=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(node.gameObject);
            if(path.IndexOf("/Trees/",StringComparison.OrdinalIgnoreCase)<0)continue;
            var p=new Vector2(node.position.x-AshfallReach.Origin.x,node.position.z-AshfallReach.Origin.z);
            bool occupied=RouteDistance(p)<5||Vector2.Distance(p,AshfallReach.BasaltNeedle)<28||Vector2.Distance(p,AshfallReach.EmberCrown)<64;
            if(!occupied)continue;
            bool placed=false;
            for(int i=0;i<72&&!placed;i++)
            {
                var candidate=p+new Vector2(Mathf.Cos(i*2.4f),Mathf.Sin(i*2.4f))*(7+i*.9f);
                if(RouteDistance(candidate)<6||Vector2.Distance(candidate,AshfallReach.BasaltNeedle)<30||Vector2.Distance(candidate,AshfallReach.EmberCrown)<67||
                    Vector2.Distance(candidate,AshfallReach.Willowford)<34||Mathf.Abs(candidate.x-AshfallReach.RiverX(candidate.y))<9)continue;
                node.position=AshfallReach.Origin+Ground(candidate);placed=true;moved++;
            }
            if(!placed)throw new Exception("Cannot clear new road around "+node.name);
        }
        return moved;
    }
    static int DressFullFace(Transform parent)
    {
        var plants=Group(parent,"Full-world groves and basalt fields",Vector3.zero);
        var broadleaf=Load<GameObject>(Art+"CornbergBatch/Trees/Tree_Oak.fbx");var pine=Load<GameObject>(Art+"CornbergBatch/Trees/Tree_Spruce.fbx");
        int count=0;
        for(int i=0;i<1700;i++)
        {
            var p=new Vector2(Range(-238,180),Range(-565,24));
            if(p.x<55&&p.y>-150||RouteDistance(p)<6||Vector2.Distance(p,AshfallReach.BasaltNeedle)<30||
                Vector2.Distance(p,AshfallReach.EmberCrown)<67||Mathf.Abs(p.x-AshfallReach.RiverX(p.y))<9)continue;
            if(p.x>-60&&i%3!=0)continue;
            var model=(GameObject)PrefabUtility.InstantiatePrefab(p.x<-60?broadleaf:pine,plants);
            model.name=p.x<-60?"Southern Steamroot grove":"Ashfall highland spruce";
            model.transform.position=AshfallReach.Origin+Ground(p);model.transform.localRotation=Quaternion.Euler(0,Range(0,360),0);model.transform.localScale=Vector3.one*Range(.9f,1.7f);
            model.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(9);
            var trunk=new GameObject("Trunk"){layer=9};trunk.transform.SetParent(model.transform,false);var capsule=trunk.AddComponent<CapsuleCollider>();capsule.radius=.45f;capsule.height=3.4f;capsule.center=Vector3.up*1.7f;count++;
            if(i%3==0)Rock(plants,Ground(p+Vector2.right*2),new(Range(1.2f,3.2f),Range(.8f,2.3f),Range(1.2f,2.7f)),p.x<-60?slate:basalt);
        }
        foreach(var route in FullRoutes)
        for(int i=1;i<route.Count;i+=2)
        {
            var p=route[i];Lantern(parent,Ground(p+new Vector2(4,1)));
            if(i%3==0)Waypost(parent,i<route.Count/2?"CINDERWATCH / WILLOWFORD":"WORLD 2 TRAIL",Ground(p+new Vector2(-4,1)),145,4.2f);
        }
        return count;
    }
    static void AddWorldRim(Transform parent)
    {
        var rim=Group(parent,"Outer face ridges",Vector3.zero);var rect=AshfallReach.LocalBounds;
        for(int i=0;i<50;i++)
        {
            float z=Mathf.Lerp(rect.yMin+3,rect.yMax-3,i/49f);
            foreach(float x in new[]{rect.xMin+1,rect.xMax-1})
                Rock(rim,Ground(x,z)+Vector3.up*2,new(Range(7,13),Range(9,18),Range(8,14)),basalt);
        }
        for(int i=0;i<40;i++)
        {
            float x=Mathf.Lerp(rect.xMin+2,rect.xMax-2,i/39f);
            if(Mathf.Abs(x-AshfallReach.RiverX(rect.yMin))<9)continue;
            Rock(rim,Ground(x,rect.yMin+1)+Vector3.up*2,new(12,Range(9,16),11),basalt);
        }
    }
    static NavMeshData BakeFullFace(Transform root)
    {
        Physics.SyncTransforms();var sources=new List<NavMeshBuildSource>();var blocked=new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(root,1<<8,NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);
        NavMeshBuilder.CollectSources(root,1<<9,NavMeshCollectGeometry.PhysicsColliders,1,new List<NavMeshBuildMarkup>(),blocked);sources.AddRange(blocked);
        var rect=AshfallReach.Bounds;var settings=NavMesh.GetSettingsByID(0);settings.overrideVoxelSize=true;settings.voxelSize=.25f;settings.agentSlope=40;settings.agentClimb=.4f;
        var data=NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(new Vector3(rect.center.x,45,rect.center.y),new Vector3(rect.width,140,rect.height)),Vector3.zero,Quaternion.identity);
        if(data==null)throw new Exception("Full-world navigation bake failed");data.name="World Two full face navigation";return Save(data,ScaleOutput+data.name+".asset");
    }
    static Vector3 FullNav(Vector2 p)
    {
        if(NavMesh.SamplePosition(AshfallReach.Origin+Ground(p),out var hit,2,1))return hit.position;
        throw new Exception("No full-world navigation at "+p);
    }
    static void FullPath(Vector3 from,Vector2 to)
    {
        var path=new NavMeshPath();if(!NavMesh.CalculatePath(from,FullNav(to),1,path)||path.status!=NavMeshPathStatus.PathComplete)throw new Exception("Disconnected world route "+to);
    }
    static int PopulateFullFace(Transform parent,Vector3 start,Transform root)
    {
        var templates=root.GetComponentsInChildren<CombatActor>(true).Where(a=>a.GetComponent<ActorStats>().Definition.familyId=="ashscale.lizardman").ToArray();
        var spear=templates.First(a=>a.GetComponent<ActorStats>().Definition.stableId.EndsWith("spearhand"));var bow=templates.First(a=>a.GetComponent<ActorStats>().Definition.stableId.EndsWith("ridgebow"));int count=0;
        var patrol=Group(parent,"Expanded Ashscale patrols",Vector3.zero);
        foreach(var route in FullRoutes)
        for(int i=1;i<route.Count;i++)
        {
            var p=route[i];if(p.y>-135)continue;
            var tangent=(route[i]-route[i-1]).normalized;var normal=new Vector2(-tangent.y,tangent.x);
            var candidate=p+normal*(count%2==0?9:-9);
            if(Mathf.Abs(candidate.x-AshfallReach.RiverX(candidate.y))<10||Vector2.Distance(candidate,AshfallReach.EmberCrown)<62)continue;
            var spawn=FullNav(candidate);FullPath(start,candidate);
            var copy=Object.Instantiate((count%3==0?bow:spear).gameObject,patrol);copy.transform.position=spawn;copy.name="Road patrol — "+copy.GetComponent<ActorStats>().Definition.displayName;
            copy.GetComponent<AggroBehaviour>().ConfigureHomePosition(spawn);count++;
        }
        return count;
    }
}
