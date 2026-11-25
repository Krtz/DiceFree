using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.ApplicationFlow;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class IronpineSouthPassAuthoring
{
    private const string WorldPath = "Assets/_DiceFree/Scenes/Cornberg.unity";
    private const string LandingPath = "Assets/_DiceFree/Scenes/WorldTwoArrival.unity";
    private const string Art = "Assets/_DiceFree/Art/World/";
    private const string Output = Art + "AshfallReach/";
    private const string Handcrafted = Art + "World1Handcrafted/";
    private const string Dragon = "Assets/_DiceFree/Settings/AncientRoadDragon/";
    private static readonly Vector3 LandingOrigin = new(64000, 0, 64000);
    private static readonly Vector2[] IronpineTrail =
    {
        new(166,44), new(165,38), new(164,33), new(163,27), new(163,13)
    };

    private static GameObject pine, spruce;
    private static Mesh boulder;
    private static Material path, basalt, stone, moss, bark, ash, lamp, grass, ledge, copper;
    private static System.Random random;

    public static void Batch()
    {
        if (!AssetDatabase.IsValidFolder(Output.TrimEnd('/'))) AssetDatabase.CreateFolder(Art.TrimEnd('/'), "AshfallReach");
        pine = Load<GameObject>(Art + "CornbergBatch/Trees/Tree_Pine.fbx");
        spruce = Load<GameObject>(Art + "CornbergBatch/Trees/Tree_Spruce.fbx");
        boulder = Load<Mesh>("Assets/_DiceFree/Settings/Briarback/FourfoldHollow/Original faceted basalt.asset");
        path = Load<Material>(Dragon + "Dust on the mountain path.mat");
        basalt = Load<Material>(Dragon + "Charcoal mountain rock.mat");
        stone = Load<Material>(Dragon + "Old worked granite.mat");
        moss = Load<Material>(Dragon + "Deep moss at roots.mat");
        bark = Load<Material>(Handcrafted + "DarkOak.mat");
        copper = Load<Material>(Dragon + "Stained bronze inlay.mat");
        ash = Load<Material>(Art + "BriarbackFire/Cinder ash.mat");
        lamp = Load<Material>(Handcrafted + "Ember.mat");
        grass = Tint("Ashfall heath", moss, new Color(.32f,.38f,.24f));
        ledge = Tint("Ashfall slate", basalt, new Color(.29f,.30f,.32f));
        random = new System.Random(4231);

        var world = EditorSceneManager.OpenScene(WorldPath);
        var worldNav = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            Physics.SyncTransforms();
            BuildIronpine(world);
            EditorSceneManager.MarkSceneDirty(world);
            if (!EditorSceneManager.SaveScene(world)) throw new IOException("Could not save southern Ironpine.");
        }
        finally { worldNav.Remove(); }

        var landing = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BuildLanding();
        if (!EditorSceneManager.SaveScene(landing, LandingPath)) throw new IOException("Could not save the World 2 landing.");
        if (!EditorBuildSettings.scenes.Any(scene => scene.path == LandingPath))
            EditorBuildSettings.scenes = EditorBuildSettings.scenes.Append(new EditorBuildSettingsScene(LandingPath, true)).ToArray();
        AssetDatabase.SaveAssets();
    }

    private static void BuildIronpine(Scene world)
    {
        if (world.GetRootGameObjects().Any(root => root.name == "Ironpine southern pass to World 2"))
            throw new InvalidOperationException("The Ironpine passage is already authored.");
        var gridTest = new NavMeshPath();
        if (!NavMesh.CalculatePath(Ground(166,44), Ground(163,14.5f), 1, gridTest) ||
            gridTest.status != NavMeshPathStatus.PathComplete)
            throw new InvalidOperationException("Southern Ironpine is disconnected from the existing road.");

        var root = Group(null, "Ironpine southern pass to World 2", Vector3.zero);
        Trail(root, "Ironpine south trail", IronpineTrail, TerrainHeight, 1.97f);
        var trees = Group(root, "Closely grown pines and fallen timber", Ground(163,21));
        for (int i = 0; i < 5; i++)
        {
            float x = (i - 2) * 1.63f;
            var p = Ground(163+x,21+Mathf.Sin(i*3.2f)*.68f);
            Tree(trees, p-trees.position, i % 2 == 0 ? pine : spruce, 1.22f+i*.12f);
        }
        for (int i = 0; i < 3; i++)
        {
            Part(trees,"Fallen pine across the trail",new Vector3((i-1)*2.35f,.44f,-.18f-i*.29f),
                new Vector3(.26f,1.64f,.26f),bark,PrimitiveType.Cylinder,Quaternion.Euler(6,i*10,76+i*4));
        }
        Block(trees, "Tangled trunks", new Vector3(0,.95f,0), new Vector3(8.8f,2f,2.2f), true);

        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 6; i++)
            {
                float z = 12.4f+i*2.83f;
                float x = 163+side*(4.4f+.08f*i+Range(-.25f,.25f));
                var p = Ground(x,z);
                var scale = new Vector3(Range(2.9f,3.5f),Range(1.6f,3.3f),Range(3.1f,3.9f));
                Rock(root, p+Vector3.up*(scale.y*.22f),scale,i%3==0?moss:basalt);
                if (i < 5)
                    Block(root, "Rock bank", p+Vector3.up*.75f,new Vector3(2.65f,1.9f,3.05f),true);
                if (i % 2 == 1) Tree(root,p+new Vector3(side*1.15f,.37f,-.22f),i%3==0?pine:spruce,Range(.8f,1.22f));
            }
        }
        Block(root, "Ridge behind the old stones", Ground(163,11.4f)+Vector3.up*1.02f,new Vector3(8.8f,2.35f,1.8f),true);

        var fire = Group(root, "Small fire clearing the pines", Ground(163,21));
        var flame = FlameMesh();
        var orange = Load<Material>(Art + "BriarbackFire/Amber flame.mat");
        var gold = Load<Material>(Art + "BriarbackFire/Golden flame.mat");
        for (int i = -1; i <= 1; i++)
        {
            var source = Group(fire, "Fire in the fallen timber", new Vector3(i*1.67f-.36f,.37f,-2.16f+i*.34f));
            for (int tongue = 0; tongue < 3; tongue++)
            {
                var outer = Surface(source,"Emissive flame",flame,tongue == 1 ? gold : orange);
                outer.transform.localPosition = new Vector3((tongue-1)*.36f,0,tongue*.17f-.14f);
                outer.transform.localRotation = Quaternion.Euler(0,tongue*100+i*40,0);
                outer.transform.localScale = new Vector3(.64f,1.35f+.38f*tongue,.70f);
            }
            Particles(source, "Flames", new Color(1,.43f,.065f,.88f),new Color(1,.12f,.025f,.61f), .85f, .8f, 25, 1.7f, .48f);
            Particles(source, "Sparks", new Color(1,.72f,.22f,1),new Color(1,.23f,.025f,.70f), .12f, 1.17f, 17, 2.7f, .39f);
            Particles(source, "Smoke", new Color(.28f,.25f,.24f,.33f),new Color(.31f,.28f,.27f,.18f), 1.85f, 2.7f, 12, 2.7f, .66f);
        }
        var glow = Group(fire, "Warm firelight", Vector3.up*1.23f).gameObject.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(1,.43f,.15f);
        glow.range = 9;
        glow.intensity = 9;
        fire.gameObject.SetActive(false);

        var cleared = Group(root, "Char and stumps after the burn", Vector3.zero);
        for (int side = -1; side <= 1; side += 2)
        for (int i = 0; i < 3; i++)
        {
            var p = Ground(163+side*(2.44f+i*.37f),20.0f+i*.85f);
            Part(cleared,"Blackened stump",p+Vector3.up*.26f,new Vector3(.47f,.26f,.52f),ash,
                PrimitiveType.Cylinder,Quaternion.Euler(Range(-12,12),0,Range(-13,13)));
            Rock(cleared,p+new Vector3(-side*.26f,.046f,.18f),new Vector3(1.23f,.10f,.88f),ash);
        }
        cleared.gameObject.SetActive(false);

        var doorway = Group(root,"Old south pass waystones",Ground(163,12.8f));
        Gate(doorway,"WORLD 2");
        var portal = doorway.gameObject.AddComponent<IronpineSouthPass>();
        portal.ConfigureName("Cross to World 2 · Ashfall Reach");
        portal.ConfigureApproach(Group(doorway, "Pass approach", Ground(163,14.8f)-doorway.position));
        var returnPoint = Group(root, "Return to the Ironpine trail", Ground(163,17.6f));
        portal.ConfigureEntrance(trees.gameObject, fire.gameObject, cleared.gameObject, returnPoint);
        var sign = Group(root, "Ironpine south trail sign", Ground(166.5f,31.5f));
        sign.localRotation = Quaternion.Euler(0,225,0);
        Part(sign,"Hewn cedar post",new Vector3(0,1.08f,0),new Vector3(.18f,2.15f,.20f),bark);
        Part(sign,"Wayboard",new Vector3(0,1.89f,.04f),new Vector3(2.55f,.60f,.14f),bark);
        Label(sign,"SOUTH PASS",new Vector3(0,1.90f,.15f),1.46f);
    }

    private static void BuildLanding()
    {
        var root = Group(null, "World 2 · Ashfall Reach", LandingOrigin);
        var visibleGround = Landscape("Ashfall Reach valley", 96, 2);
        var floor = Surface(root,"Slate ridges and alpine heath",visibleGround,grass,ledge);
        floor.layer = 8;
        floor.AddComponent<MeshCollider>().sharedMesh = Landscape("Ashfall Reach walkable valley", 32, 1);
        var scenery = Group(root,"High ridges beyond Ashfall Reach",Vector3.zero);
        for(int side = -1; side <= 1; side += 2)
        {
            for(int i=0;i<26;i++)
            {
                float z = -74+i*5.62f+Range(-1.8f,1.8f);
                float x = side*(39+Mathf.Sin(i*.30f)*7+Range(-3,6));
                var size=new Vector3(Range(7,14),Range(7,17),Range(8,13));
                Rock(scenery,new Vector3(x,ValleyHeight(x,z)+size.y*.27f,z),size,i%3==0?ledge:basalt);
            }
            for(int i=0;i<58;i++)
            {
                float x = Range(-72,72), z = side*Range(31,76);
                Tree(scenery,new Vector3(x,ValleyHeight(x,z)-.12f,z),i%4==0?pine:spruce,Range(.85f,1.65f));
            }
        }
        var trail = new Vector2[] { new(0,25),new(1,19),new(-1.5f,11),new(-5,5),new(-5,-3),
            new(-2,-12),new(3,-19),new(1,-25) };
        Trail(root, "Ashfall trail", trail, ValleyHeight, 2.55f);

        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 16; i++)
            {
                float z = 28-i*3.66f+Range(-.65f,.65f);
                float x = side*(21.6f+Mathf.Sin(i*.47f)*2.8f+Range(-1.7f,2.2f));
                var scale = new Vector3(Range(5.4f,9),Range(4.5f,10.8f),Range(5.6f,8.5f));
                Rock(root,new Vector3(x,ValleyHeight(x,z)+scale.y*.18f,z),scale,i%4==0?basalt:ledge);
                Block(root,"Slate ridge",new Vector3(x,ValleyHeight(x,z)+1.7f,z),new Vector3(scale.x*.72f,4f,scale.z*.79f),false);
            }
            for (int i = 0; i < 23; i++)
            {
                float z = Range(-29,28), x = side*Range(9.5f,23.8f);
                float y = ValleyHeight(x,z);
                if (i % 4 == 0)
                    Rock(root,new Vector3(x,y+.57f,z),new Vector3(Range(1.2f,4.7f),Range(1.05f,2.7f),Range(1.2f,4.4f)),i%3==0?basalt:moss);
                else
                    Tree(root,new Vector3(x,y-.12f,z),i%3==0?pine:spruce,Range(.82f,1.55f));
            }
        }
        var overlooks = Group(root,"Weathered waystones along the reach",Vector3.zero);
        for(int i=0;i<4;i++)
        {
            var p=trail[i+2];
            p.x+=i%2==0?-3.4f:3.8f;
            var post=Group(overlooks,"Worn brass-capped waystone",new Vector3(p.x,ValleyHeight(p.x,p.y),p.y));
            Rock(post,Vector3.up*.16f,new Vector3(1.22f,.44f,1.18f),basalt);
            Part(post,"Standing slab",new Vector3(0,.70f,0),new Vector3(.44f,1.3f,.46f),stone,rotation:Quaternion.Euler(0,i*29,0));
            Part(post,"Brass cap",new Vector3(0,1.38f,0),new Vector3(.56f,.14f,.55f),copper);
        }
        for (int i = -3; i <= 3; i++)
        {
            var position = new Vector3(i*4.45f,1.25f,30+Mathf.Abs(i)*.7f);
            var size = new Vector3(6.5f,Range(4,7.3f),5.7f);
            Rock(root,position,size,i%2==0?basalt:ledge);
            Block(root,"Northern rock face",position+Vector3.up,new Vector3(5.3f,4.1f,3.8f),false);
        }
        var gate = Group(root, "North gate back to Ironpine", new Vector3(0,ValleyHeight(0,25),25));
        Gate(gate,"IRONPINE");
        gate.gameObject.layer=9;
        var portal = gate.gameObject.AddComponent<IronpineSouthPass>();
        portal.ConfigureName("Return to Ironpine");
        portal.ConfigureApproach(Group(gate,"Return approach",new Vector3(0,ValleyHeight(0,23)-gate.localPosition.y,-2)));
        portal.ConfigureExit(Group(root,"World 2 arrival",new Vector3(1,ValleyHeight(1,19),19)));

        var end = Group(root,"Ashfall overlook",new Vector3(1,ValleyHeight(1,-26),-26));
        var overlook = Group(end,"Ashfall Reach overlook cairn",new Vector3(-3.5f,0,.8f));
        overlook.localRotation = Quaternion.Euler(0,225,0);
        Rock(overlook,Vector3.up*.35f,new Vector3(1.5f,.95f,1.45f),ledge);
        Part(overlook,"Weathered wayboard",new Vector3(0,1.05f,.06f),new Vector3(3.5f,.68f,.20f),bark);
        Label(overlook,"ASHFALL REACH",new Vector3(0,1.06f,.18f),2.02f);
        Rock(end,new Vector3(0,-.07f,-2.8f),new Vector3(8.7f,1.9f,2.4f),ledge);
        for (int i = -2; i <= 2; i++)
            Rock(end,new Vector3(i*3.8f,1.3f,-5.1f),new Vector3(5.5f,Range(3.4f,6.5f),4.9f),i%2==0?basalt:ledge);

        BakeLanding(root);
    }

    private static void Gate(Transform parent, string label)
    {
        const int opening = -1;
        for (int side = -1; side <= 1; side += 2)
        {
            Rock(parent,new Vector3(side*2.85f,.57f,0),new Vector3(2.3f,1.75f,2.02f),basalt);
            Part(parent,"Worked stone pier",new Vector3(side*1.94f,1.74f,0),new Vector3(.83f,3.45f,.86f),stone,
                rotation:Quaternion.Euler(0,side*5,side*-2));
            Part(parent,"Pillar cornice",new Vector3(side*1.94f,3.47f,0),new Vector3(1.05f,.24f,1.05f),ledge);
            Part(parent,"Copper lantern cradle",new Vector3(side*2.10f,1.06f,opening*.82f),new Vector3(.51f,.36f,.49f),copper);
            Part(parent,"Warm lantern",new Vector3(side*2.10f,1.33f,opening*.82f),new Vector3(.29f,.37f,.30f),lamp,PrimitiveType.Sphere);
        }
        Part(parent,"Stone lintel", new Vector3(0,3.74f,0), new Vector3(4.96f,.51f,1.06f), stone);
        Part(parent,"Lintel cap", new Vector3(0,4.03f,-.05f), new Vector3(5.19f,.18f,1.16f),ledge);
        Part(parent,"Wayboard", new Vector3(0,3.72f,opening*.56f), new Vector3(2.58f,.45f,.13f),bark);
        Label(parent,label,new Vector3(0,3.73f,opening*.65f),1.58f,true);
        for(int i=0;i<4;i++)
            Part(parent,"Worn threshold paver",new Vector3((i-1.5f)*.71f,.035f,opening*.62f),new Vector3(.67f,.10f,1.18f),stone);
        var hit = Group(parent,"Gate interaction",new Vector3(0,2.03f,.04f)).gameObject;
        hit.layer=11;
        var click=hit.AddComponent<BoxCollider>();
        click.size=new Vector3(4.5f,3.9f,1.12f);
    }

    private static void Label(Transform parent,string text,Vector3 p,float width,bool reverse=false)
    {
        var label = Group(parent,text,p);
        label.localRotation=Quaternion.Euler(0,reverse?0:180,0);
        var mesh=label.gameObject.AddComponent<TextMesh>();
        mesh.text=text;
        mesh.anchor=TextAnchor.MiddleCenter;
        mesh.alignment=TextAlignment.Center;
        mesh.fontSize=48;
        mesh.characterSize=width/(text.Length*1.32f);
        mesh.color=new Color(.99f,.84f,.56f);
    }

    private static void Particles(Transform parent,string name,Color hot,Color warm,float size,float lifetime,float rate,float speed,float radius)
    {
        var system=Group(parent,name,Vector3.zero).gameObject.AddComponent<ParticleSystem>();
        var main=system.main;
        main.startColor=new ParticleSystem.MinMaxGradient(hot,warm);
        main.startSize=new ParticleSystem.MinMaxCurve(size*.55f,size);
        main.startLifetime=new ParticleSystem.MinMaxCurve(lifetime*.55f,lifetime);
        main.startSpeed=new ParticleSystem.MinMaxCurve(speed*.64f,speed);
        main.maxParticles=110;
        main.simulationSpace=ParticleSystemSimulationSpace.World;
        var shape=system.shape;
        shape.shapeType=ParticleSystemShapeType.Cone;
        shape.radius=radius;
        shape.angle=18;
        system.transform.localRotation=Quaternion.Euler(-90,0,0);
        var emission=system.emission;
        emission.rateOverTime=rate;
        var fade=system.colorOverLifetime;
        fade.enabled=true;
        var gradient=new Gradient();
        gradient.SetKeys(new [] { new GradientColorKey(Color.white,0),new GradientColorKey(new Color(.64f,.55f,.51f),1) },
            new [] { new GradientAlphaKey(0,0),new GradientAlphaKey(1,.12f),new GradientAlphaKey(.7f,.48f),new GradientAlphaKey(0,1) });
        fade.color=gradient;
        var renderer=system.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial=Load<Material>("Assets/_DiceFree/Resources/EarlySkillVfx/SoftSparks.mat");
    }

    private static Mesh FlameMesh()
    {
        var mesh = new Mesh { name = "Split ember flame" };
        mesh.vertices = new[]
        {
            new Vector3(-.78f,0,-.35f),new Vector3(.64f,0,-.51f),new Vector3(.64f,0,.42f),new Vector3(-.49f,0,.53f),
            new Vector3(-.55f,.55f,-.16f),new Vector3(.5f,.57f,-.35f),new Vector3(.29f,.85f,.35f),new Vector3(-.32f,.68f,.24f),
            new Vector3(.21f,1.39f,-.05f),new Vector3(-.18f,.86f,.07f)
        };
        mesh.triangles = new[]
        {
            0,4,1,1,4,5, 1,5,2,2,5,6, 2,6,3,3,6,7, 3,7,0,0,7,4,
            4,8,5,5,8,6, 6,8,9,6,9,7, 7,9,4,4,9,8
        };
        SaveMesh(mesh);
        return Load<Mesh>(Output+"Split ember flame.asset");
    }

    private static Mesh Landscape(string name, int radius, int step)
    {
        var mesh = new Mesh { name = name };
        int width = radius*2/step;
        var vertices = new List<Vector3>();
        var uv = new List<Vector2>();
        var heath = new List<int>();
        var rock = new List<int>();
        for (int z = 0; z <= width; z++)
        for (int x = 0; x <= width; x++)
        {
            float localX = x*step-radius, localZ = z*step-radius;
            vertices.Add(new Vector3(localX,ValleyHeight(localX,localZ),localZ));
            uv.Add(new Vector2(localX*.20f,localZ*.20f));
            if (x == width || z == width) continue;
            var triangles = ValleyHeight(localX+step*.5f,localZ+step*.5f)>2.1f ? rock : heath;
            int a=z*(width+1)+x;
            triangles.AddRange(new [] { a,a+width+1,a+1,a+1,a+width+1,a+width+2 });
        }
        mesh.SetVertices(vertices);
        mesh.SetUVs(0,uv);
        mesh.subMeshCount=2;
        mesh.SetTriangles(heath,0);
        mesh.SetTriangles(rock,1);
        SaveMesh(mesh);
        return Load<Mesh>(Output+name+".asset");
    }
    private static void Trail(Transform parent,string name,Vector2[] points,Func<float,float,float> height,float halfWidth)
    {
        var samples=new List<Vector2> { points[0] };
        for (int i=1;i<points.Length;i++)
        {
            int count=Mathf.CeilToInt(Vector2.Distance(points[i-1],points[i])/.75f);
            for (int j=1;j<=count;j++) samples.Add(Vector2.Lerp(points[i-1],points[i],j/(float)count));
        }
        var vertices=new List<Vector3>();
        var uv=new List<Vector2>();
        var triangles=new List<int>();
        for (int i=0;i<samples.Count;i++)
        {
            var direction=(samples[Mathf.Min(i+1,samples.Count-1)]-samples[Mathf.Max(i-1,0)]).normalized;
            var normal=new Vector2(-direction.y,direction.x)*(halfWidth+.14f*Mathf.Sin(i*.64f));
            for (int j=0;j<3;j++)
            {
                var p=samples[i]+normal*(j-1);
                vertices.Add(new Vector3(p.x,height(p.x,p.y)+.065f,p.y));
                uv.Add(new Vector2(j*.5f,i*.18f));
            }
            if (i==0) continue;
            for (int j=0;j<2;j++)
            {
                int a=(i-1)*3+j;
                triangles.AddRange(new [] { a,a+1,a+3,a+1,a+4,a+3 });
            }
        }
        var mesh=new Mesh { name=name };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0,uv);
        mesh.SetTriangles(triangles,0);
        SaveMesh(mesh);
        var surface=Surface(parent,name,Load<Mesh>(Output+name+".asset"),path);
        if(name=="Ashfall trail")WorldTwoRouteAuthoring.Configure(surface.GetComponent<MeshFilter>());
    }

    private static void BakeLanding(Transform root)
    {
        Physics.SyncTransforms();
        var sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(root, (1<<8)|(1<<9), NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);
        var settings = NavMesh.GetSettingsByID(0);
        var data=NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(LandingOrigin+Vector3.up*3,new Vector3(70,21,70)),Vector3.zero,Quaternion.identity);
        if(data==null) throw new InvalidOperationException("Could not bake Ashfall Reach navigation.");
        var instance=NavMesh.AddNavMeshData(data);
        try
        {
            var pathCheck=new NavMeshPath();
            if (!NavMesh.SamplePosition(LandingOrigin+new Vector3(1,0,19),out var start,2,1) ||
                !NavMesh.SamplePosition(LandingOrigin+new Vector3(1,0,-25),out var end,2,1) ||
                !NavMesh.CalculatePath(start.position,end.position,1,pathCheck) || pathCheck.status!=NavMeshPathStatus.PathComplete)
                throw new InvalidOperationException("The arrival path to the Ashfall overlook is disconnected.");
        }
        finally { instance.Remove(); }
        var file = Output+"Ashfall Reach navigation.asset";
        SaveAsset(data,file);
        root.gameObject.AddComponent<WorldNavigation>().Configure(Load<NavMeshData>(file));
    }

    private static float ValleyHeight(float x,float z)
    {
        float edge = Mathf.SmoothStep(0,1,Mathf.InverseLerp(11,30,Mathf.Abs(x+Mathf.Sin(z*.085f)*2.1f)));
        return (Mathf.PerlinNoise(51+x*.115f,70+z*.115f)-.5f)*.65f + edge*(5.4f+1.55f*Mathf.Sin(z*.22f));
    }

    private static float TerrainHeight(float x,float z) => Ground(x,z).y;
    private static Vector3 Ground(float x,float z)
    {
        if (!Physics.Raycast(new Vector3(x,40,z),Vector3.down,out var hit,90,1<<8))
            throw new InvalidOperationException($"Missing Ironpine ground at {x}, {z}.");
        return hit.point;
    }

    private static void Tree(Transform parent,Vector3 p,GameObject model,float scale)
    {
        var tree=(GameObject)PrefabUtility.InstantiatePrefab(model,parent);
        tree.transform.localPosition=p;
        tree.transform.localRotation=Quaternion.Euler(0,Range(0,360),0);
        tree.transform.localScale=Vector3.one*scale;
        foreach(var collider in tree.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(collider);
    }

    private static void Block(Transform parent,string name,Vector3 center,Vector3 size,bool carve)
    {
        var go=Group(parent,name,center).gameObject;
        go.layer=9;
        go.AddComponent<BoxCollider>().size=size;
        if (!carve) return;
        var obstacle=go.AddComponent<NavMeshObstacle>();
        obstacle.shape=NavMeshObstacleShape.Box;
        obstacle.size=size;
        obstacle.carving=true;
        obstacle.carveOnlyStationary=false;
    }

    private static void Rock(Transform parent,Vector3 p,Vector3 size,Material material)
    {
        var go=Surface(parent,"Fractured ridge stone",boulder,material);
        go.transform.localPosition=p;
        go.transform.localRotation=Quaternion.Euler(Range(-16,16),Range(0,360),Range(-14,14));
        go.transform.localScale=size;
    }

    private static GameObject Surface(Transform parent,string name,Mesh mesh,params Material[] materials)
    {
        var go=Group(parent,name,Vector3.zero).gameObject;
        go.layer=2;
        go.AddComponent<MeshFilter>().sharedMesh=mesh;
        go.AddComponent<MeshRenderer>().sharedMaterials=materials;
        return go;
    }

    private static void SaveMesh(Mesh mesh)
    {
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        SaveAsset(mesh,Output+mesh.name+".asset");
    }

    private static Transform Group(Transform parent,string name,Vector3 p)
    {
        var group=new GameObject(name).transform;
        group.SetParent(parent,false);
        group.localPosition=p;
        return group;
    }

    private static void Part(Transform parent,string name,Vector3 p,Vector3 size,Material material,
        PrimitiveType type=PrimitiveType.Cube,Quaternion? rotation=null)
    {
        var go=GameObject.CreatePrimitive(type);
        go.name=name;
        go.layer=2;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent,false);
        go.transform.localPosition=p;
        go.transform.localRotation=rotation ?? Quaternion.identity;
        go.transform.localScale=size;
        go.GetComponent<MeshRenderer>().sharedMaterial=material;
    }

    private static T Load<T>(string path) where T : Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new IOException("Missing asset "+path);

    private static Material Tint(string name,Material template,Color color)
    {
        var material=new Material(template) { name=name };
        material.SetColor("_BaseColor",color);
        SaveAsset(material,Output+name+".mat");
        return Load<Material>(Output+name+".mat");
    }

    private static void SaveAsset<T>(T created,string path) where T : Object
    {
        var current = AssetDatabase.LoadAssetAtPath<T>(path);
        if (current == null) AssetDatabase.CreateAsset(created,path);
        else
        {
            EditorUtility.CopySerialized(created,current);
            EditorUtility.SetDirty(current);
            Object.DestroyImmediate(created);
        }
    }

    private static float Range(float min,float max) => min+(float)random.NextDouble()*(max-min);
}
