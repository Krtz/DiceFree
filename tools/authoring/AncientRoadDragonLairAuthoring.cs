using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Dungeons;
using DiceFree.World;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class AncientRoadDragonLairAuthoring
{
    const string WorldPath = "Assets/_DiceFree/Scenes/Cornberg.unity";
    const string ScenePath = "Assets/_DiceFree/Scenes/AncientRoadDragonLair.unity";
    const string Art = "Assets/_DiceFree/Art/Creatures/AncientRoadDragon";
    const string Set = "Assets/_DiceFree/Settings/AncientRoadDragon";
    const string BoulderPath = "Assets/_DiceFree/Settings/Briarback/FourfoldHollow/Original faceted basalt.asset";
    static readonly Vector3 Origin = AncientRoadDragonLairPortal.Origin;
    static Material cliff, limestone, paving, moss, copper, luminous, black, gravel, pickedStone;
    static Mesh boulder;

    public static void Batch()
    {
        if (File.Exists(ScenePath)) throw new IOException("The dragon lair is already authored.");
        Directory.CreateDirectory(Set);
        AssetDatabase.Refresh();
        PrepareMaterials();

        var world = EditorSceneManager.OpenScene(WorldPath);
        var navigation = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try { AddMountainApproach(); }
        finally { navigation.Remove(); }

        var arena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(arena);
        var root = new GameObject("Ancient Road Dragon lair").transform;
        root.position = Origin;
        BuildTerrain(root);
        BuildDragon(root);
        var exit = Group(root, "Return to mountain pass", new Vector3(0,0,-32));
        Portal(exit, "Return to the mountain pass", true);
        Beacon(exit,new Vector3(-2.4f,0,0),.72f);
        Beacon(exit,new Vector3(2.4f,0,0),.72f);
        Label(exit, "MOUNTAIN PASS", Vector3.up*2.35f);
        Bake(root);

        if (!EditorSceneManager.SaveScene(arena,ScenePath)) throw new IOException("Could not save dragon lair.");
        EditorBuildSettings.scenes=EditorBuildSettings.scenes.Append(new EditorBuildSettingsScene(ScenePath,true)).ToArray();
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(world);
        if (!EditorSceneManager.SaveScene(world)) throw new IOException("Could not save mountain approach.");
    }

    static void AddMountainApproach()
    {
        var nav = new NavMeshPath();
        var entry = SnapTerrain(5,411);
        var trailhead = SnapTerrain(-19,383);
        if (!NavMesh.CalculatePath(trailhead,entry,1,nav) || nav.status!=NavMeshPathStatus.PathComplete)
            throw new InvalidOperationException("The mountain approach does not reach the dragon entrance.");
        var root = new GameObject("Northwest mountain path to the dragon lair").transform;
        root.position = entry;
        Physics.SyncTransforms();
        var points = new List<Vector3> { nav.corners[0] };
        for(int i=1;i<nav.corners.Length;i++)
        {
            var a=nav.corners[i-1];
            var b=nav.corners[i];
            int div=Mathf.Max(1,Mathf.CeilToInt(Vector3.Distance(a,b)/1.1f));
            for(int j=1;j<=div;j++) points.Add(Vector3.Lerp(a,b,j/(float)div));
        }
        var vertices=new List<Vector3>();
        var triangles=new List<int>();
        for(int i=0;i<points.Count;i++)
        {
            var direction=points[Mathf.Min(points.Count-1,i+1)]-points[Mathf.Max(0,i-1)];
            var side=Vector3.Cross(Vector3.up,direction).normalized*(1.13f+.14f*Mathf.Sin(i*.61f));
            foreach(int sign in new[]{-1,1})
            {
                var edge=points[i]+side*sign;
                if(Physics.Raycast(edge+Vector3.up*6,Vector3.down,out var hit,12,1<<8))edge.y=hit.point.y;
                vertices.Add(root.InverseTransformPoint(edge+Vector3.up*.065f));
            }
            if(i==0)continue;
            int k=i*2;
            triangles.AddRange(new[]{k-2,k,k+1,k-2,k+1,k-1});
        }
        var road=MeshAsset("Worn hillside path",vertices,triangles);
        Surface(root,"Stone and dirt trail on the existing ridge",Vector3.zero,road,gravel);
        for(int i=2;i<points.Count-5;i+=7)
        {
            var dir=points[i+1]-points[i-1];
            var p=points[i]+Vector3.Cross(Vector3.up,dir).normalized*(i%2==0?1.88f:-1.88f);
            if(!Physics.Raycast(p+Vector3.up*4,Vector3.down,out var hit,8,1<<8))continue;
            Rock(root,root.InverseTransformPoint(hit.point)+Vector3.up*.26f,new Vector3(.78f,.9f,.86f),i*47,limestone);
        }
        var mouth=Group(root,"Old gate cut into the ridge",Vector3.zero);
        mouth.rotation=Quaternion.Euler(0,205,0);
        Portal(mouth,"Enter the Ancient Road Dragon's Lair",false);
        for(int side=-1;side<=1;side+=2)
        {
            Rock(mouth,new Vector3(side*3.0f,1.8f,-.6f),new Vector3(3.2f,6.3f,4.1f),side*49,cliff);
            Rock(mouth,new Vector3(side*5.5f,1.3f,-2.9f),new Vector3(5.6f,5.4f,5.4f),side*77,cliff);
            Part(mouth,"Carved entry pier",new Vector3(side*2.03f,1.34f,.47f),new Vector3(.58f,2.92f,.72f),limestone,2);
            Beacon(mouth,new Vector3(side*2.6f,0,1.5f),.9f);
        }
        Rock(mouth,new Vector3(0,4.2f,-1.55f),new Vector3(8.5f,4.25f,5.5f),21,cliff);
        Rock(mouth,new Vector3(0,3.5f,-5),new Vector3(11.3f,8.5f,7.7f),164,cliff);
        Part(mouth,"Shadowed passage",new Vector3(0,1.43f,.18f),new Vector3(3.43f,3.25f,.08f),black,2);
        Part(mouth,"Once-gilded lintel",new Vector3(0,3.07f,.56f),new Vector3(4.7f,.49f,.77f),limestone,2);
        Label(mouth,"THE OLD MOUNTAIN ROAD",new Vector3(0,4.36f,.9f));
        var obstacle=Part(mouth,"Ridge gate back obstacle",new Vector3(0,1.4f,-3.9f),
            new Vector3(8.4f,3.8f,3.4f),null,9).AddComponent<NavMeshObstacle>();
        obstacle.shape=NavMeshObstacleShape.Box;
        obstacle.size=Vector3.one;
        obstacle.carving=true;
    }

    static Vector3 SnapTerrain(float x,float z)
    {
        if(!Physics.Raycast(new Vector3(x,100,z),Vector3.down,out var ground,130,1<<8) ||
            !NavMesh.SamplePosition(ground.point,out var surface,1.5f,1))
            throw new InvalidOperationException($"No usable mountain ground at {x}, {z}.");
        return surface.position;
    }

    static void BuildTerrain(Transform root)
    {
        Disk(root,"Shadow beyond the rock",new Vector3(0,-2.9f,0),108,115,black,false);
        Disk(root,"Broken upper terraces",new Vector3(0,-.55f,2),42,48,cliff,false);
        Disk(root,"Walkable mountain arena",Vector3.zero,33,38,moss,true);
        var ruins=Group(root,"Dragon's ruined road and stonework",Vector3.zero);
        var pavers=Group(root,"Hand-cut cobble strips",Vector3.zero);
        for(int z=-16;z<=16;z++)
        for(int x=-12;x<=12;x++)
        {
            float px=x*2.42f+(z%2)*.36f,pz=z*2.07f;
            bool road=Mathf.Abs(px)<5.7f+Mathf.Sin(z*.43f)*.92f;
            bool ring=px*px/390f+(pz-2)*(pz-2)/502f<1;
            if(!road&&!ring)continue;
            if((x*11+z*7)%37==0 && !road)continue;
            var tile=Part(pavers,"Weathered road stone",new Vector3(px,.026f,pz),
                new Vector3(2.37f-.07f*Mathf.Abs(Mathf.Sin(z+x)),.065f,2.01f),road?paving:(x*z%9==0?pickedStone:limestone),2);
            tile.transform.localRotation=Quaternion.Euler(0,Mathf.Sin(x*9+z*7)*1.9f,0);
        }
        Combine(pavers);
        for(int i=0;i<56;i++)
        {
            float angle=(i+.2f)*2*Mathf.PI/56;
            float height=(Mathf.Sin(angle)+Mathf.Cos(angle)<-.25f?3.8f:11)+3.2f*Mathf.Abs(Mathf.Sin(i*6.39f));
            float x=Mathf.Cos(angle)*(34.3f+Mathf.Sin(i*.9f)*1.6f),z=Mathf.Sin(angle)*(38.8f+Mathf.Sin(i*2.2f)*1.9f);
            Rock(root,new Vector3(x,height*.32f,z),new Vector3(5.8f+i%4,height,7.3f+i%4),i*40,cliff);
            if(i%3==1)Rock(root,new Vector3(x*.94f,.47f,z*.95f),new Vector3(3.7f,2.7f,4.1f),i*62,moss);
            if(i%4==1)StoneGrowth(root,new Vector3(x*.91f,.13f,z*.9f),i);
        }
        for(int side=-1;side<=1;side+=2)
        {
            for(int i=0;i<3;i++)
            {
                var at=new Vector3(side*(18+i*1.42f),0,-14+i*11);
                RuinedColumn(ruins,at,i+1);
                Rock(ruins,at+new Vector3(-side*1.1f,.15f,1.22f),new Vector3(2.61f,.96f,1.9f),side*51,limestone);
            }
            Beacon(ruins,new Vector3(side*5.9f,0,-20),1);
            Beacon(ruins,new Vector3(side*20.2f,0,17),1.2f);
        }
        // The front scene stays open for camera travel and future breath-dodge routes.
        Disk(root,"Dragon roost",new Vector3(0,.096f,17),12,7.8f,pickedStone,false);
        Seal(root,new Vector3(0,.082f,1),11.15f);
        var back=Group(root,"Collapsed stone gate and ledge",new Vector3(0,0,28));
        for(int i=-3;i<=3;i++)
            Rock(back,new Vector3(i*2.5f,2.1f+Mathf.Abs(i)*.61f,1.4f),new Vector3(4.62f,6.2f+Mathf.Abs(i),4.4f),i*27,moss);
        RuinedColumn(back,new Vector3(-11.5f,0,-.6f),4);
        RuinedColumn(back,new Vector3(11.5f,0,-.6f),4);

        var key=Group(root,"Hazy light from the mountain opening",Vector3.zero).gameObject.AddComponent<Light>();
        key.type=LightType.Directional;
        key.intensity=1.55f;
        key.color=new Color(.93f,.92f,.70f);
        key.shadows=LightShadows.Soft;
        key.transform.rotation=Quaternion.Euler(49,23,0);
        Light(root,new Vector3(-6,6,12),new Color(.74f,1,.52f),20,3.1f);
        Light(root,new Vector3(13,4,19),new Color(.47f,.71f,1),23,2.7f);
    }

    static void BuildDragon(Transform root)
    {
        var path=Art+"/Winged Road Dragon.fbx";
        var importer=(ModelImporter)AssetImporter.GetAtPath(path);
        importer.animationType=ModelImporterAnimationType.Generic;
        importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation=true;
        importer.importCameras=false;
        importer.importLights=false;
        importer.addCollider=false;
        importer.materialImportMode=ModelImporterMaterialImportMode.None;
        var clips=importer.defaultClipAnimations;
        if(clips.Length!=1)throw new InvalidOperationException("Expected the one source-authored dragon idle animation.");
        clips[0].name="Mountain roost idle";
        clips[0].loopTime=true;
        clips[0].loopPose=true;
        clips[0].lockRootHeightY=true;
        clips[0].lockRootPositionXZ=true;
        clips[0].lockRootRotation=true;
        importer.clipAnimations=clips;
        importer.SaveAndReimport();

        var scales=Mat("Jade scales",new Color(.91f,1,.86f));
        scales.mainTexture=Load<Texture2D>(Art+"/Winged dragon jade scales.png");
        var normalAsset=Art+"/Winged dragon normals.jpg";
        var normal=(TextureImporter)AssetImporter.GetAtPath(normalAsset);
        normal.textureType=TextureImporterType.NormalMap;
        normal.SaveAndReimport();
        scales.EnableKeyword("_NORMALMAP");
        scales.SetTexture("_BumpMap",Load<Texture2D>(normalAsset));
        scales.SetFloat("_BumpScale",.74f);
        scales.SetFloat("_Smoothness",.31f);
        EditorUtility.SetDirty(scales);

        var owner=Group(root,"Ancient Road Dragon - arena preview",new Vector3(0,.13f,15));
        owner.localRotation=Quaternion.Euler(0,158,0);
        var model=(GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(path),owner);
        model.name="Perched dragon, authored idle";
        foreach(var t in model.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer=2;
            PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);
        }
        var renderers=model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if(renderers.Length==0)throw new InvalidOperationException("Dragon rig import has no skinned mesh.");
        for(int index=0;index<renderers.Length;index++)
        {
            var renderer=renderers[index];
            renderer.sharedMaterials=Enumerable.Repeat(scales,renderer.sharedMaterials.Length).ToArray();
            renderer.updateWhenOffscreen=true;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }
        var animator=model.GetComponent<Animator>()??model.AddComponent<Animator>();
        var controller=AnimatorController.CreateAnimatorControllerAtPath(Set+"/Dragon roost preview.controller");
        var idle=controller.layers[0].stateMachine.AddState("Perched Idle");
        idle.motion=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Single(c=>c.name=="Mountain roost idle");
        controller.layers[0].stateMachine.defaultState=idle;
        animator.runtimeAnimatorController=controller;
        animator.applyRootMotion=false;
        animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        PrefabUtility.RecordPrefabInstancePropertyModifications(animator);

        Bounds size=renderers.Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;});
        float width=Mathf.Max(size.size.x,size.size.z);
        if(width<.02f)throw new InvalidOperationException("Empty dragon render bounds.");
        model.transform.localScale=Vector3.one*(21.6f/width);
        size=renderers.Select(r=>r.bounds).Aggregate((a,b)=>{a.Encapsulate(b);return a;});
        var correction=owner.position-size.center;
        correction.y=owner.position.y-size.min.y;
        model.transform.position+=correction;
        PrefabUtility.RecordPrefabInstancePropertyModifications(model.transform);
        Part(root,"Roost ground clearance",new Vector3(0,.8f,15),new Vector3(10,1.8f,5.7f),null,9);
    }

    static void PrepareMaterials()
    {
        boulder=Load<Mesh>(BoulderPath);
        cliff=Mat("Charcoal mountain rock",new Color(.19f,.21f,.18f));
        limestone=Mat("Weather-beaten limestone",new Color(.39f,.43f,.30f));
        paving=Mat("Road stone pale sage",new Color(.42f,.47f,.30f));
        pickedStone=Mat("Old worked granite",new Color(.29f,.34f,.26f));
        moss=Mat("Deep moss at roots",new Color(.20f,.30f,.16f));
        copper=Mat("Stained bronze inlay",new Color(.62f,.48f,.24f));
        gravel=Mat("Dust on the mountain path",new Color(.43f,.40f,.29f));
        var noise=Load<Texture2D>("Assets/_DiceFree/Settings/Briarback/FourfoldHollow/Slate grain.png");
        foreach(var mat in new[]{cliff,limestone,paving,pickedStone,moss,gravel})
        {
            mat.mainTexture=noise;
            EditorUtility.SetDirty(mat);
        }
        luminous=Mat("Guide lamps and emerald crystals",new Color(.26f,.75f,.26f));
        luminous.EnableKeyword("_EMISSION");
        luminous.SetColor("_EmissionColor",new Color(.20f,.72f,.22f));
        EditorUtility.SetDirty(luminous);
        black=new Material(Shader.Find("Universal Render Pipeline/Unlit")){color=new Color(.011f,.017f,.012f)};
        AssetDatabase.CreateAsset(black,Set+"/Cavern depth.mat");
    }

    static void RuinedColumn(Transform parent,Vector3 position,int height)
    {
        var column=Group(parent,"Collapsed wayside column",position);
        Part(column,"Slab footing",Vector3.up*.22f,new Vector3(2.2f,.46f,2.1f),pickedStone,9);
        for(int i=0;i<height;i++)
        {
            var stone=Part(column,"Carved limestone drum",Vector3.up*(.68f+i*.74f),
                new Vector3(1.31f,.75f,1.31f),i%2==0?limestone:pickedStone,2);
            stone.transform.localRotation=Quaternion.Euler(0,i*6+height*8,1+i*2);
        }
        Part(column,"Bronze collar",Vector3.up*(.78f+(height-1)*.74f),new Vector3(1.39f,.063f,1.39f),copper,2);
        Rock(column,Vector3.up*(.91f+height*.64f),new Vector3(1.24f,.63f,1.17f),21+height*43,limestone);
    }

    static void StoneGrowth(Transform root,Vector3 at,int seed)
    {
        for(int j=0;j<4;j++)
        {
            float a=seed*2.37f+j*2.17f;
            Rock(root,at+new Vector3(Mathf.Sin(a)*j*.39f,.53f,Mathf.Cos(a)*j*.39f),
                new Vector3(.32f+j*.09f,1.0f+(j==1?.75f:0),.5f),seed*29+j*97,
                j==1?luminous:moss);
        }
    }

    static void Beacon(Transform parent,Vector3 at,float size)
    {
        var post=Group(parent,"Old road lamp",at);
        Part(post,"Cracked stone lantern foot",Vector3.up*.26f,new Vector3(.7f,.52f,.7f),pickedStone,2);
        Part(post,"Hollow metal stem",Vector3.up*.84f,new Vector3(.23f,1.04f,.23f),copper,2);
        Rock(post,Vector3.up*1.57f,new Vector3(.46f,.69f,.41f)*size,40,luminous);
        Light(post,Vector3.up*1.55f,new Color(.34f,.78f,.37f),8,1.4f);
    }

    static void Seal(Transform root,Vector3 point,float radius)
    {
        var parent=Group(root,"Ancient Road compass inlay",point);
        for(int i=0;i<24;i++)
        {
            float angle=(i+.5f)*2*Mathf.PI/24;
            var p=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));
            var line=Part(parent,"Compass edge",p*radius,new Vector3(2.91f,.026f,.115f),copper,2);
            line.transform.localRotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg,0);
            int length=i%6==0?2:1;
            var marker=Part(parent,"Inset compass mark",p*(radius-.55f),new Vector3(.11f,.035f,length*.54f),
                i%6==0?luminous:limestone,2);
            marker.transform.localRotation=line.transform.localRotation;
        }
        for(int i=0;i<8;i++)
        {
            float angle=i*Mathf.PI/4;
            var p=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));
            var line=Part(parent,"Emblem point",p*1.5f,new Vector3(.15f,.029f,i%2==0?3.7f:2.41f),copper,2);
            line.transform.localRotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg,0);
        }
    }

    static void Bake(Transform root)
    {
        var sources=new List<NavMeshBuildSource>();
        var bounds=new Bounds(Origin+Vector3.up*4,new Vector3(92,26,103));
        Physics.SyncTransforms();
        var blockers=root.GetComponentsInChildren<Collider>().Where(c=>c.gameObject.layer==9)
            .Select(c=>new NavMeshBuildMarkup{root=c.transform,overrideArea=true,area=1}).ToList();
        NavMeshBuilder.CollectSources(bounds,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,blockers,sources);
        var settings=NavMesh.GetSettingsByID(0);
        settings.agentRadius=.55f;
        settings.agentHeight=1.85f;
        settings.agentClimb=.4f;
        settings.overrideVoxelSize=true;
        settings.voxelSize=.16f;
        var data=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);
        if(data==null)throw new InvalidOperationException("Could not bake navigation across the dragon arena.");
        AssetDatabase.CreateAsset(data,Set+"/Ancient Road Dragon NavMesh.asset");
        root.gameObject.AddComponent<WorldNavigation>().Configure(data);
        var handle=NavMesh.AddNavMeshData(data);
        var path=new NavMeshPath();
        try
        {
            if(!NavMesh.SamplePosition(Origin+AncientRoadDragonLairPortal.ArrivalOffset,out var arrival,1,1))
                throw new InvalidOperationException("The arena has no safe arrival.");
            foreach(var local in new[]{new Vector3(0,0,-32),new Vector3(-24,0,1),new Vector3(24,0,1),new Vector3(0,0,24)})
            {
                if(!NavMesh.SamplePosition(Origin+local,out var at,1,1) ||
                   !NavMesh.CalculatePath(arrival.position,at.position,1,path) || path.status!=NavMeshPathStatus.PathComplete)
                    throw new InvalidOperationException("Dragon arena route failed at "+local);
            }
        }
        finally{handle.Remove();}
    }

    static void Portal(Transform parent,string title,bool exit)
    {
        var entrance=Group(parent,title,Vector3.zero).gameObject;
        entrance.layer=11;
        var collider=entrance.AddComponent<CapsuleCollider>();
        collider.height=2.6f;
        collider.center=Vector3.up*1.3f;
        collider.radius=1.05f;
        var portal=entrance.AddComponent<AncientRoadDragonLairPortal>();
        portal.ConfigureName(title);
        if(exit)portal.ConfigureAsExit();
    }

    static void Combine(Transform parent)
    {
        foreach(var group in parent.GetComponentsInChildren<MeshFilter>().GroupBy(f=>f.GetComponent<Renderer>().sharedMaterial).ToArray())
        {
            var mesh=new Mesh{name=group.Key.name+" pavers",indexFormat=IndexFormat.UInt32};
            mesh.CombineMeshes(group.Select(f=>new CombineInstance{mesh=f.sharedMesh,
                transform=parent.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray());
            AssetDatabase.CreateAsset(mesh,Set+"/"+mesh.name+".asset");
            Surface(parent,mesh.name,Vector3.zero,mesh,group.Key);
            foreach(var tile in group)Object.DestroyImmediate(tile.gameObject);
        }
    }

    static void Disk(Transform parent,string name,Vector3 local,float width,float depth,Material material,bool nav)
    {
        var vertices=new List<Vector3>{Vector3.zero};
        var tris=new List<int>();
        const int count=112;
        for(int i=0;i<count;i++)
        {
            float angle=i*2*Mathf.PI/count;
            float warp=1+Mathf.Sin(i*.54f)*.024f;
            vertices.Add(new Vector3(Mathf.Sin(angle)*width,0,Mathf.Cos(angle)*depth)*warp);
            tris.AddRange(new[]{0,i+1,(i+1)%count+1});
        }
        var mesh=MeshAsset(name,vertices,tris);
        var ground=Surface(parent,name,local,mesh,material).gameObject;
        if(!nav)return;
        ground.layer=8;
        ground.AddComponent<MeshCollider>().sharedMesh=mesh;
    }

    static Mesh MeshAsset(string name,List<Vector3> verts,List<int> triangles)
    {
        var mesh=new Mesh{name=name};
        mesh.SetVertices(verts);
        mesh.SetUVs(0,verts.Select(p=>new Vector2(p.x,p.z)*.25f).ToList());
        mesh.SetTriangles(triangles,0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Set+"/"+name+".asset");
        return mesh;
    }

    static void Rock(Transform root,Vector3 point,Vector3 scale,float yaw,Material mat)
    {
        var rock=Surface(root,mat.name,point,boulder,mat);
        rock.localScale=scale;
        rock.localRotation=Quaternion.Euler(Mathf.Sin(yaw*.17f)*9,yaw,Mathf.Sin(yaw*.21f)*9);
    }

    static Material Mat(string name,Color color)
    {
        var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color};
        mat.SetFloat("_Smoothness",.17f);
        AssetDatabase.CreateAsset(mat,Set+"/"+name+".mat");
        return mat;
    }

    static T Load<T>(string path) where T:Object => AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new IOException("Missing art: "+path);

    static Transform Group(Transform parent,string name,Vector3 point)
    {
        var go=new GameObject(name).transform;
        go.SetParent(parent,false);
        go.localPosition=point;
        return go;
    }

    static Transform Surface(Transform parent,string name,Vector3 p,Mesh mesh,Material mat)
    {
        var node=Group(parent,name,p);
        node.gameObject.layer=2;
        node.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        node.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;
        return node;
    }

    static GameObject Part(Transform parent,string name,Vector3 local,Vector3 scale,Material mat,int layer)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name=name;
        go.layer=layer;
        go.transform.SetParent(parent,false);
        go.transform.localPosition=local;
        go.transform.localScale=scale;
        if(layer==2)Object.DestroyImmediate(go.GetComponent<Collider>());
        var renderer=go.GetComponent<Renderer>();
        if(mat!=null)renderer.sharedMaterial=mat;
        else renderer.enabled=false;
        return go;
    }

    static void Light(Transform parent,Vector3 local,Color tint,float range,float intensity)
    {
        var light=Group(parent,"Local rock bounce light",local).gameObject.AddComponent<Light>();
        light.type=LightType.Point;
        light.range=range;
        light.color=tint;
        light.intensity=intensity;
    }

    static void Label(Transform parent,string message,Vector3 point)
    {
        var label=Group(parent,message,point).gameObject.AddComponent<TextMesh>();
        label.gameObject.layer=2;
        label.transform.rotation=Quaternion.Euler(52,45,0);
        label.text=message;
        label.fontSize=46;
        label.characterSize=.092f;
        label.anchor=TextAnchor.MiddleCenter;
        label.alignment=TextAlignment.Center;
        label.color=new Color(.92f,.85f,.58f);
    }
}

