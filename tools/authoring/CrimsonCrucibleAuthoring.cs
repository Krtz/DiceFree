using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class CrimsonCrucibleAuthoring
{
    const string Data=CrucibleLootAuthoring.Data;
    const string ScenePath="Assets/_DiceFree/Scenes/CrimsonCrucible.unity";
    static Transform root;
    static Material stone,trim,iron,gold,ruby;
    static Material[] floors;
    static readonly string[] Names={"Verdigris Globule","Moonfang Alpha","Bristlebreaker","Bloomheart Thornwarden","Stormscale Hierophant","Crimson Sovereign"};
    static readonly string[] Ids={"verdigris-globule","moonfang-alpha","bristlebreaker","bloomheart","stormscale","crimson-sovereign"};
    static readonly Color[] Colors={new(.32f,.43f,.26f),new(.30f,.37f,.46f),new(.45f,.31f,.20f),new(.23f,.42f,.30f),new(.28f,.42f,.44f),new(.44f,.19f,.22f)};
    static Vector3 P(float x,float y,float z)=>new(x,y,z);
    public static void Batch()
    {
        Directory.CreateDirectory(Data);AssetDatabase.Refresh();var pool=CrucibleLootAuthoring.Author();
        var world=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");var actors=world.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<CombatActor>(true)).Where(a=>a.GetComponent<ActorStats>()?.Definition!=null).ToArray();
        CombatActor Pick(string family)=>actors.Where(a=>a.GetComponent<ActorStats>().Definition.familyId==family).OrderByDescending(a=>a.GetComponent<ActorStats>().Level).First();
        var templates=new[]{Pick("enemy-family.slime"),Pick("ironpine.wolf"),Pick("briarback.quilboar"),Pick("verdant.thornlash"),Pick("reedscale.lizardman"),Pick("slime")};
        stone=Mat("Crucible dark masonry",new(.18f,.21f,.24f));trim=Mat("Crucible cut stone",new(.37f,.40f,.40f));iron=Mat("Crucible black iron",new(.085f,.095f,.115f));gold=Mat("Crucible old gold",new(.66f,.44f,.17f));ruby=Mat("Crucible living ruby",new(.85f,.07f,.12f),true);
        floors=Colors.Select((c,i)=>Mat("Cell "+(i+1)+" floor",c)).ToArray();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);root=new GameObject("Crimson Crucible - Six Cells").transform;root.position=CrimsonCruciblePortal.Origin;
        var arena=root.gameObject.AddComponent<CrucibleArena>();arena.rewards=pool;arena.bosses=new CrucibleBoss[6];arena.gates=new CrucibleGate[5];
        for(int i=0;i<6;i++)
        {
            float z=i*48;var cell=Group(root,"Cell "+(i+1)+" - "+Names[i],P(0,0,z));
            Box(cell,"Walkable arena floor",P(0,-.35f,0),P(36,.7f,36),floors[i],8);
            Wall(cell,P(-18.6f,1.7f,0),P(1.2f,4.4f,38));Wall(cell,P(18.6f,1.7f,0),P(1.2f,4.4f,38));
            foreach(int side in new[]{-1,1})
            {
                Wall(cell,P(side*11.75f,1.7f,18.6f),P(13.5f,4.4f,1.2f));Wall(cell,P(side*11.75f,1.7f,-18.6f),P(13.5f,4.4f,1.2f));
                Torch(cell,P(side*15,0,-12),i);Torch(cell,P(side*15,0,12),i);
            }
            if(i==0)Wall(cell,P(0,1.7f,-18.6f),P(10,4.4f,1.2f));if(i==5)Wall(cell,P(0,1.7f,18.6f),P(10,4.4f,1.2f));
            Ring(cell,12,Colors[i]);Label(cell,P(0,3.9f,-16),$"CELL {i+1} / 6\n{Names[i]}",.085f);
            Dress(cell,i);arena.bosses[i]=Boss(cell,templates[i],i);
            if(i<5)
            {
                var corridor=Group(root,"Passage "+(i+1),P(0,0,z+24));Box(corridor,"Walkable passage",P(0,-.35f,0),P(10,.7f,12.1f),trim,8);
                Wall(corridor,P(-5.6f,1.7f,0),P(1.2f,4.4f,12));Wall(corridor,P(5.6f,1.7f,0),P(1.2f,4.4f,12));arena.gates[i]=Gate(corridor,i);
            }
        }
        var exit=Group(root,"Crucible exit",P(-11,0,-12));exit.gameObject.layer=11;var portal=exit.gameObject.AddComponent<CrimsonCruciblePortal>();portal.ConfigureAsExit();portal.ConfigureName("Leave the Crimson Crucible");var select=exit.gameObject.AddComponent<CapsuleCollider>();select.height=3;select.center=Vector3.up*1.5f;select.radius=.8f;Box(exit,"Exit marker",P(0,1.3f,0),P(.8f,2.6f,.8f),ruby);Label(exit,P(0,3.3f,0),"RETURN TO THE MIRE",.055f);
        var sun=new GameObject("Crucible skylight").AddComponent<Light>();sun.transform.SetParent(root,false);sun.type=LightType.Directional;sun.intensity=.95f;sun.color=new(.77f,.84f,1);sun.transform.rotation=Quaternion.Euler(62,-30,0);sun.shadows=LightShadows.Soft;
        foreach(var gate in arena.gates)gate.GetComponent<NavMeshObstacle>().enabled=false;
        var nav=Bake(arena.gates);root.gameObject.AddComponent<WorldNavigation>().Configure(nav);var instance=NavMesh.AddNavMeshData(nav);
        try{var start=CrimsonCruciblePortal.Origin+P(0,0,-12);for(int i=0;i<6;i++){var path=new NavMeshPath();if(!NavMesh.CalculatePath(start,CrucibleArena.CellCenter(i),1,path)||path.status!=NavMeshPathStatus.PathComplete){Directory.CreateDirectory("Logs");File.WriteAllText("Logs/CrucibleNavFailure.txt","Cell "+i+" status "+path.status+"\n"+string.Join("\n",path.corners.Select(c=>c.ToString("F3")))+"\n"+string.Join("\n",root.GetComponentsInChildren<Collider>().Where(c=>c.gameObject.layer==8||c.gameObject.layer==9).Select(c=>c.name+" "+c.bounds.ToString())));throw new Exception("Disconnected cell floor "+i);}}}
        finally{instance.Remove();foreach(var gate in arena.gates)gate.GetComponent<NavMeshObstacle>().enabled=true;}
        EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new IOException("Crucible scene save failed");EditorSceneManager.CloseScene(scene,true);
        SceneManager.SetActiveScene(world);InstallEntrance(world);
        var scenes=EditorBuildSettings.scenes.ToList();if(!scenes.Any(s=>s.path==ScenePath))scenes.Add(new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();
        AssetDatabase.SaveAssets();Directory.CreateDirectory("Logs");File.WriteAllText("Logs/CrucibleAuthoring.json","{\"cells\":6,\"gates\":5,\"level\":55,\"items\":17,\"entrance\":[80,149],\"bossOrder\":[\"slime\",\"wolf\",\"bristleboar\",\"plant\",\"lizard\",\"red slime\"]}");Debug.Log("DICEFREE_CRIMSON_CRUCIBLE_AUTHORED_OK");
    }
    static Material Mat(string name,Color color,bool glow=false)
    {
        string path=Data+"/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}material.color=color;material.SetFloat("_Smoothness",.25f);if(glow){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*2.3f);}else{material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_DiceFree/Settings/Briarback/FourfoldHollow/Slate grain.png"));material.SetTextureScale("_BaseMap",new(3,3));}EditorUtility.SetDirty(material);return material;
    }
    static Transform Group(Transform parent,string name,Vector3 at){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=at;return t;}
    static Transform Box(Transform parent,string name,Vector3 at,Vector3 size,Material material,int layer=2)
    {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.layer=layer;go.transform.SetParent(parent,false);go.transform.localPosition=at;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;if(layer==2)Object.DestroyImmediate(go.GetComponent<Collider>());return go.transform;}
    static void Wall(Transform parent,Vector3 at,Vector3 size)
    {
        var wall=Box(parent,"Cell masonry wall",at,size,stone,9);wall.gameObject.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(9);
        Box(parent,"Stone coping",at+Vector3.up*2.3f,new(size.x+.15f,.23f,size.z+.15f),trim);
        int count=Mathf.CeilToInt(Mathf.Max(size.x,size.z)/4);
        for(int i=0;i<count;i++){var p=at+(size.z>size.x?Vector3.forward:Vector3.right)*(i-(count-1)*.5f)*3.8f;p.y=.9f;Box(parent,"Wall buttress",p,size.z>size.x?P(1.8f,3,1):P(1,3,1.8f),trim);}
    }
    static void Torch(Transform parent,Vector3 at,int index)
    {var t=Group(parent,"Cell brazier",at);Box(t,"Brazier pedestal",P(0,.7f,0),P(.8f,1.4f,.8f),iron);Box(t,"Living trial crystal",P(0,1.7f,0),P(.5f,.7f,.5f),ruby).localRotation=Quaternion.Euler(0,0,45);var l=Group(t,"Warm cell light",P(0,2,0)).gameObject.AddComponent<Light>();l.type=LightType.Point;l.range=12;l.intensity=2.2f;l.color=Color.Lerp(Colors[index],Color.white,.4f);l.shadows=LightShadows.None;}
    static void Label(Transform parent,Vector3 at,string text,float size)
    {var label=Group(parent,"Trial inscription",at).gameObject.AddComponent<TextMesh>();label.text=text;label.fontSize=42;label.characterSize=size;label.anchor=TextAnchor.MiddleCenter;label.color=new(.95f,.84f,.61f);label.transform.localRotation=Quaternion.Euler(52,45,0);}
    static void Ring(Transform parent,float radius,Color color)
    {
        var line=Group(parent,"Arena inlay",Vector3.up*.035f).gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=72;line.widthMultiplier=.13f;line.sharedMaterial=trim;line.startColor=line.endColor=Color.Lerp(color,Color.white,.35f);
        for(int i=0;i<72;i++)line.SetPosition(i,P(Mathf.Sin(i*Mathf.PI/36)*radius,0,Mathf.Cos(i*Mathf.PI/36)*radius));
    }
    static CrucibleGate Gate(Transform parent,int index)
    {
        var go=Group(parent,"Sealed gate "+(index+1),Vector3.zero);go.gameObject.layer=9;var collision=go.gameObject.AddComponent<BoxCollider>();collision.center=P(0,2,0);collision.size=P(10.8f,4,1);
        var obstacle=go.gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.center=P(0,2,0);obstacle.size=P(10.8f,4,1.2f);obstacle.carving=true;obstacle.carvingTimeToStationary=.1f;
        var bars=Group(go,"Rising iron portcullis",Vector3.zero);for(int i=0;i<11;i++)Box(bars,"Iron gate bar",P(i-5,2,0),P(.17f,4,.25f),iron);foreach(float y in new[]{.6f,3.5f})Box(bars,"Riveted gate binding",P(0,y,0),P(10.4f,.27f,.35f),gold);
        var result=go.gameObject.AddComponent<CrucibleGate>();result.Configure(bars,collision,obstacle);Box(parent,"Gate lintel",P(0,5,0),P(12,1.2f,1.9f),trim);return result;
    }
    static void Prop(Transform parent,string model,Vector3 at,float scale,Material tint=null)
    {
        string path="Assets/_DiceFree/Art/World/VisualBatch01/Models/"+model+".fbx";var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(source==null)throw new Exception("Missing existing model "+path);
        var go=(GameObject)PrefabUtility.InstantiatePrefab(source,parent);PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);go.name="Trial decoration - "+model;go.transform.localPosition=at;
        var rr=go.GetComponentsInChildren<Renderer>();var bounds=rr[0].bounds;foreach(var r in rr)bounds.Encapsulate(r.bounds);go.transform.localScale*=scale/Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));bounds=rr[0].bounds;foreach(var r in rr){bounds.Encapsulate(r.bounds);if(tint!=null)r.sharedMaterial=tint;}go.transform.position+=Vector3.up*(parent.TransformPoint(at).y-bounds.min.y);
        foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.layer=2;foreach(var c in go.GetComponentsInChildren<Collider>())Object.DestroyImmediate(c);
    }
    static void Dress(Transform cell,int index)
    {
        foreach(int side in new[]{-1,1})
        {
            switch(index)
            {
                case 0:Prop(cell,"mushroom_02",P(side*13,0,12),2.5f);Prop(cell,"slimejar_01",P(side*13,0,-8),2);break;
                case 1:Prop(cell,"pine_01",P(side*14,0,12),7);Prop(cell,"rock_03",P(side*14,0,-12),3);break;
                case 2:Prop(cell,"fence_02",P(side*13,0,9),5);Prop(cell,"barrel_02",P(side*13,0,-10),2);Prop(cell,"cart_01",P(side*12,0,13),3);break;
                case 3:Prop(cell,"root_01",P(side*14,0,10),5);Prop(cell,"mushroom_03",P(side*12,0,-10),3);break;
                case 4:Prop(cell,"altar_02",P(side*13,0,12),3.5f);Prop(cell,"rock_05",P(side*14,0,-12),3);break;
                default:Prop(cell,"altar_01",P(side*13,0,12),4);Prop(cell,"rock_04",P(side*14,0,-11),4,stone);Box(cell,"Crimson seep",P(side*11,.03f,7),P(6,.04f,4),ruby);break;
            }
        }
    }
    static CrucibleBoss Boss(Transform cell,CombatActor source,int index)
    {
        var go=new GameObject(Names[index]){layer=10};go.transform.SetParent(cell,false);go.transform.localPosition=P(0,0,5);
        var definition=Object.Instantiate(source.GetComponent<ActorStats>().Definition);definition.name=Names[index];definition.stableId="boss.crucible."+Ids[index];definition.displayName=Names[index];definition.tags=(definition.tags??Array.Empty<string>()).Append("boss").Distinct().ToArray();definition.growth=new AttributeValues(0);definition.baseAttributes=new(){vitality=20,strength=42,agility=28,intelligence=38,spirit=28};definition.baseHp=index==5?13500:9000+index*500;definition.experienceReward=index==5?1500:600;definition.moveSpeed=index==1?5.5f:3.8f;definition.physicalDefense=28;definition.magicalDefense=28;definition.overrideVitalityRegeneration=true;definition.regenerationPerVitality=0;
        var basic=Object.Instantiate(definition.basicAttack);basic.name=Names[index]+" basic";basic.stableId="attack.crucible."+Ids[index]+".basic";basic.baseDamage=85+index*7;basic.coefficient=.8f;basic.interval=1.8f;basic.windup=.4f;basic.reach=1.6f;basic=Save(basic,basic.name);definition.basicAttack=basic;
        var special=Object.Instantiate(basic);special.name=Names[index]+" special";special.stableId="attack.crucible."+Ids[index]+".special";special.baseDamage=150+index*14;special.coefficient=.6f;special.requiresAccuracy=false;special.channel=index is 1 or 2?DamageChannel.Physical:DamageChannel.Magical;special=Save(special,special.name);definition=Save(definition,definition.name);
        go.AddComponent<ActorStats>().Configure(definition,55);go.AddComponent<NavMeshAgent>().radius=1.2f;go.AddComponent<TraversalMotor>();go.AddComponent<Health>();go.AddComponent<CombatActor>().Configure(1,1.3f);go.AddComponent<CombatStatusController>().ConfigureControl(.65f,false);go.AddComponent<BasicAttack>();var ai=go.AddComponent<AggroBehaviour>();ai.Configure(24,31);ai.ConfigureHomePosition(go.transform.position);ai.enabled=false;go.AddComponent<DefeatReporter>();go.AddComponent<ActorFeedback>();
        var collider=go.AddComponent<CapsuleCollider>();collider.height=3.5f;collider.center=P(0,1.75f,0);collider.radius=1.3f;
        Transform visualSource;
        if(index==0)visualSource=source.transform.Cast<Transform>().FirstOrDefault(t=>t.name.StartsWith("Slimes/")&&t.gameObject.activeSelf)??source.transform.GetChild(0);
        else visualSource=source.transform.Cast<Transform>().First(t=>t.gameObject.activeSelf&&t.GetComponentsInChildren<Renderer>().Length>0);
        var visual=Object.Instantiate(visualSource,go.transform,false);visual.name=index is 0 or 5?"Presentation":visualSource.name;visual.gameObject.SetActive(true);var renderers=visual.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);visual.localScale*=5.0f/Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);visual.position+=P(go.transform.position.x-bounds.center.x,go.transform.position.y-bounds.min.y,go.transform.position.z-bounds.center.z);
        foreach(var t in visual.GetComponentsInChildren<Transform>())t.gameObject.layer=2;foreach(var component in visual.GetComponentsInChildren<Collider>())Object.DestroyImmediate(component);foreach(var animator in visual.GetComponentsInChildren<Animator>())animator.applyRootMotion=false;
        if(index is 0 or 5)go.AddComponent<SlimeVisualMotion>();
        var warning=AssetDatabase.LoadAssetAtPath<Material>(Data+"/Crucible attack warning.mat");
        if(warning==null)
        {
            warning=new Material(Shader.Find("Universal Render Pipeline/Unlit"));warning.SetFloat("_Surface",1);warning.SetFloat("_ZWrite",0);warning.SetFloat("_SrcBlend",5);warning.SetFloat("_DstBlend",10);warning.SetFloat("_Cull",0);warning.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");warning.SetOverrideTag("RenderType","Transparent");warning.renderQueue=3000;AssetDatabase.CreateAsset(warning,Data+"/Crucible attack warning.mat");
        }
        var boss=go.AddComponent<CrucibleBoss>();boss.Configure(index,visual,special,warning);
        go.SetActive(false);return boss;
    }
    static T Save<T>(T value,string name)where T:Object
    {string path=Data+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old==null)AssetDatabase.CreateAsset(value,path);else{EditorUtility.CopySerialized(value,old);Object.DestroyImmediate(value);value=old;}return value;}
    static NavMeshData Bake(CrucibleGate[] gates)
    {
        Physics.SyncTransforms();var sources=new List<NavMeshBuildSource>();var blocked=new List<NavMeshBuildSource>();var marks=gates.Select(g=>new NavMeshBuildMarkup{root=g.transform,ignoreFromBuild=true}).ToList();
        NavMeshBuilder.CollectSources(root,1<<8,NavMeshCollectGeometry.PhysicsColliders,0,marks,sources);NavMeshBuilder.CollectSources(root,1<<9,NavMeshCollectGeometry.PhysicsColliders,1,marks,blocked);sources.AddRange(blocked);
        var settings=NavMesh.GetSettingsByID(0);settings.overrideVoxelSize=true;settings.voxelSize=.20f;settings.agentSlope=40;settings.agentClimb=.4f;var data=NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(CrimsonCruciblePortal.Origin+P(0,10,120),P(54,40,300)),Vector3.zero,Quaternion.identity);if(data==null)throw new Exception("Crucible navigation bake failed");return Save(data,"Crucible navigation");
    }
    static void InstallEntrance(Scene scene)
    {
        if(scene.GetRootGameObjects().Any(g=>g.name=="Crimson Crucible entrance"))throw new Exception("Crucible entrance already authored; preserve existing work");
        var navigation=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<WorldNavigation>()).First();var instance=NavMesh.AddNavMeshData(navigation.Data);Vector3 at;
        try{if(!NavMesh.SamplePosition(P(80,0,144),out var nav,5,1))throw new Exception("No Crimson Mire approach");at=nav.position;}finally{instance.Remove();}
        var entrance=new GameObject("Crimson Crucible entrance"){layer=11};entrance.transform.position=P(at.x,at.y,149);var portal=entrance.AddComponent<CrimsonCruciblePortal>();portal.ConfigureName("Crimson Crucible - Six Boss Gauntlet");var approach=Group(entrance.transform,"Crucible entrance approach",Vector3.zero);approach.position=at;portal.ConfigureApproach(approach);
        var select=entrance.AddComponent<BoxCollider>();select.center=P(0,2.4f,0);select.size=P(8,5,2);
        foreach(int side in new[]{-1,1}){Box(entrance.transform,"Crucible gatepost",P(side*3.4f,1.7f,0),P(1.1f,3.5f,1.5f),stone);Box(entrance.transform,"Crimson entry crystal",P(side*3.4f,3.8f,0),P(.8f,1,.8f),ruby).localRotation=Quaternion.Euler(0,0,45);}
        Box(entrance.transform,"Crucible stone lintel",P(0,4.5f,0),P(8.3f,.8f,1.8f),trim);Label(entrance.transform,P(0,5.7f,0),"CRIMSON CRUCIBLE\nSix bosses · Recommended level 55",.065f);
        EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new IOException("Crucible entrance save failed");
    }
    public static void PolishEntrance()
    {
        var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");var entry=scene.GetRootGameObjects().Single(g=>g.name=="Crimson Crucible entrance").transform;
        ruby=AssetDatabase.LoadAssetAtPath<Material>(Data+"/Crucible living ruby.mat");gold=AssetDatabase.LoadAssetAtPath<Material>(Data+"/Crucible old gold.mat");
        foreach(var t in entry.Cast<Transform>().Where(t=>t.name=="Crimson entry crystal"))t.localPosition=new(t.localPosition.x,5.3f,t.localPosition.z);
        if(entry.Find("Six-trial crest")==null)
        {
            var crest=Group(entry,"Six-trial crest",P(0,5.45f,-.2f));
            for(int i=0;i<6;i++){float angle=i*Mathf.PI/3;Box(crest,"Crucible ruby seal",P(Mathf.Sin(angle)*1.05f,Mathf.Cos(angle)*.6f,0),P(.33f,.33f,.28f),ruby).localRotation=Quaternion.Euler(0,0,45);}
            Box(crest,"Crest bronze backing",Vector3.zero,P(2.8f,1.6f,.16f),gold);
            var light=Group(entry,"Crimson gate glow",P(0,4.8f,-1.5f)).gameObject.AddComponent<Light>();light.type=LightType.Point;light.range=10;light.intensity=1.8f;light.color=new(1,.12f,.18f);light.shadows=LightShadows.None;
        }
        foreach(var text in entry.GetComponentsInChildren<TextMesh>()){text.transform.localPosition=P(0,7,0);text.characterSize=.085f;}
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
    }
}
