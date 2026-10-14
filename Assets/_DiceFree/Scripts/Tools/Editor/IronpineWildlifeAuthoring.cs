using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.Ironpine;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DiceFree.EditorTools
{
    // Idempotent authored Ironpine wildlife pass. Does not rebuild or replace Cornberg/road.
    internal static class IronpineWildlifeAuthoring
    {
        private const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string Root = "12 - Ironpine Forest pioneer route";
        private const string WildlifeRoot = "Ironpine wildlife v1.4";
        private const string AssetsPath = "Assets/_DiceFree/Settings/Ironpine";
        private static readonly Vector2[] Road = {
            new(140,44),new(158,45),new(175,47),new(174,64),
            new(180,80),new(183,98),new(196,112),new(208,130),new(223,149)
        };
        private static readonly Vector2[] AlphaGrounds = {
            new(167,57), new(197,90), new(201,118), new(212,143)
        };
        private static readonly string[] Colors = { "Moss", "Amber", "Slate" };
        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("IRONPINE WILDLIFE: " + message);
        }
        private static T[] All<T>(Scene scene) where T : Component =>
            scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
        private static Transform Find(Scene scene, string name) =>
            All<Transform>(scene).FirstOrDefault(t => t.name == name);
        private static void Folder()
        {
            if (!AssetDatabase.IsValidFolder(AssetsPath))
                AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "Ironpine");
        }
        private static T SaveAsset<T>(string label, Func<T> create) where T : Object
        {
            string path = AssetsPath + "/" + label + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            T obj = create();
            AssetDatabase.CreateAsset(obj, path);
            return obj;
        }
        private static Material Material(string label, Color color)
        {
            string path = AssetsPath + "/" + label + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Require(shader != null, "URP/Standard shader unavailable");
            m = new Material(shader) { name = label };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            m.color = color;
            AssetDatabase.CreateAsset(m, path);
            return m;
        }
        private static GameObject Part(string name, PrimitiveType shape, Transform parent,
            Vector3 pos, Vector3 scale, Material material)
        {
            GameObject g = GameObject.CreatePrimitive(shape);
            g.name = name; g.transform.SetParent(parent, false);
            g.transform.localPosition = pos; g.transform.localScale = scale;
            foreach (var c in g.GetComponents<Collider>()) Object.DestroyImmediate(c);
            g.GetComponent<Renderer>().sharedMaterial = material;
            return g;
        }
        private static Transform Model(WildlifeKind kind, int variant, Transform owner)
        {
            var root = new GameObject("Ironpine stylized wildlife model").transform;
            root.SetParent(owner, false);
            bool rat = kind == WildlifeKind.Quillrat, alpha = kind == WildlifeKind.AlphaWolf;
            float size = alpha ? 1.7f : rat ? .72f : 1.15f;
            root.localScale = Vector3.one * size;
            var body = rat ? Material("rat-"+Colors[variant],
                variant == 0 ? new Color(.40f,.57f,.28f) :
                variant == 1 ? new Color(.83f,.51f,.22f) : new Color(.43f,.55f,.77f)) :
                Material(kind == WildlifeKind.GreyWolf ? "grey-wolf" :
                    alpha ? "alpha-wolf" : "dark-wolf",
                    kind == WildlifeKind.GreyWolf ? new Color(.52f,.55f,.54f) :
                    alpha ? new Color(.16f,.19f,.20f) : new Color(.26f,.28f,.32f));
            var shadow = Material("wildlife-shadow", new Color(.21f,.19f,.18f));
            var pale = Material("wildlife-cream", new Color(.84f,.76f,.59f));
            var eye = Material("wildlife-eyes", new Color(.94f,.80f,.27f));
            Part("Torso", PrimitiveType.Sphere, root, new Vector3(0,.65f,0),
                rat ? new Vector3(1.1f,.9f,1.38f) : new Vector3(.9f,.84f,1.55f), body);
            Part("Chest", PrimitiveType.Sphere, root, new Vector3(0,.73f,.52f),
                rat ? new Vector3(.57f,.57f,.63f) : new Vector3(.82f,.87f,.76f), pale);
            Part("Head", PrimitiveType.Sphere, root, new Vector3(0,rat?.72f:1.04f,rat?.64f:.91f),
                rat ? new Vector3(.65f,.60f,.64f) : new Vector3(.69f,.66f,.72f), body);
            Part("Muzzle",PrimitiveType.Sphere,root,new Vector3(0,rat?.61f:.90f,rat?1.01f:1.35f),
                rat ? new Vector3(.28f,.22f,.38f) : new Vector3(.36f,.32f,.60f),pale);
            Part("Nose",PrimitiveType.Sphere,root,new Vector3(0,rat?.66f:.97f,rat?1.31f:1.68f),
                Vector3.one*(rat?.17f:.21f),shadow);
            foreach (int sign in new[]{-1,1})
            {
                Part("Eye",PrimitiveType.Sphere,root,new Vector3(sign*(rat?.26f:.27f),
                    rat?.86f:1.16f,rat?.89f:1.27f),Vector3.one*.105f,eye);
                var ear = Part("Ear",PrimitiveType.Cube,root,new Vector3(sign*.30f,
                    rat?1.06f:1.47f,rat?.52f:.80f),
                    new Vector3(.25f,rat?.24f:.43f,.24f),body);
                ear.transform.localRotation=Quaternion.Euler(0,0,-sign*23);
                foreach(float z in new[]{-.46f,.55f})
                {
                    Part("Leg",PrimitiveType.Capsule,root,new Vector3(sign*.34f,.29f,z),
                        new Vector3(.22f,.38f,.22f),body);
                    Part("Paw",PrimitiveType.Sphere,root,new Vector3(sign*.34f,.11f,z+.13f),
                        new Vector3(.27f,.16f,.31f),shadow);
                }
            }
            if (rat)
            {
                var spikes = Material("quill-"+Colors[variant],
                    variant == 0 ? new Color(.80f,.86f,.36f) :
                    variant == 1 ? new Color(.97f,.76f,.29f) : new Color(.58f,.85f,.94f));
                for(int i=0;i<25;i++)
                {
                    float t=i/25f*Mathf.PI*2;
                    float column=(i%5)/4f;
                    var g=Part("Raised quill",PrimitiveType.Cylinder,root,
                        new Vector3(Mathf.Sin(t)*.38f,.83f+column*.17f,
                            -.18f+Mathf.Cos(t)*.43f),
                        new Vector3(.045f,.40f+column*.10f,.045f),spikes);
                    g.transform.localRotation=Quaternion.Euler(45f*Mathf.Cos(t)+25,
                        0, -42f*Mathf.Sin(t));
                }
            }
            else
            {
                var tail=Part("Tail",PrimitiveType.Capsule,root,
                    new Vector3(0,.65f,-.89f),new Vector3(.30f,.47f,.28f),body);
                tail.transform.localRotation=Quaternion.Euler(-50f,0,0);
                if(alpha)
                {
                    for(int i=0;i<5;i++)
                    {
                        var mane=Part("Heavy neck mane",PrimitiveType.Sphere,root,
                            new Vector3(Mathf.Sin(i*1.3f)*.36f,.9f+Mathf.Cos(i*1.3f)*.17f,.46f),
                            new Vector3(.37f,.50f,.51f),body);
                    }
                }
            }
            return root;
        }
        private static Vector3 Nav(Vector2 point, float search = 7f)
        {
            var desired = new Vector3(point.x, 0, point.y);
            Require(NavMesh.SamplePosition(desired, out var hit, search, NavMesh.AllAreas),
                "No NavMesh at "+point);
            return hit.position;
        }
        private static float RoadDistance(Vector2 p)
        {
            float closest = float.MaxValue;
            for(int i=1;i<Road.Length;i++)
            {
                var a=Road[i-1]; var b=Road[i]; var delta=b-a;
                float t=Mathf.Clamp01(Vector2.Dot(p-a,delta)/Mathf.Max(.01f,delta.sqrMagnitude));
                closest=Mathf.Min(closest,Vector2.Distance(p,a+delta*t));
            }
            return closest;
        }
        private static Vector3 Spawn(int i)
        {
            // Author one home per creature, not random population spawning.
            // Either side of the woodland road; collision/navigation are checked below.
            var random = new System.Random(60314+i*379);
            for(int attempt=0;attempt<70;attempt++)
            {
                int section=(i*3+attempt)% (Road.Length-1);
                Vector2 a=Road[section],b=Road[section+1];
                float t=.14f+.72f*(float)random.NextDouble();
                Vector2 along=a+(b-a)*t;
                Vector2 direction=(b-a).normalized;
                var normal=new Vector2(-direction.y,direction.x);
                float offset=(i%2==0?1:-1)*(7f+(float)random.NextDouble()*12f);
                var candidate=along+normal*offset;
                if(candidate.x<139||candidate.x>231||candidate.y<42||candidate.y>168||
                   RoadDistance(candidate)<5.2f||Vector2.Distance(candidate,new Vector2(223,149))<12f)continue;
                if(NavMesh.SamplePosition(new Vector3(candidate.x,0,candidate.y),out var hit,3f,NavMesh.AllAreas))
                    return hit.position;
            }
            throw new InvalidOperationException("No viable wildlife home for slot "+i);
        }
        private static ActorDefinition Definition(ActorDefinition template, WildlifeKind kind, int level)
        {
            string name=kind+"-Lv"+level;
            return SaveAsset(name+"-stats",()=>
            {
                var def=Object.Instantiate(template);
                def.name=name+" combat";
                def.displayName=kind switch {
                    WildlifeKind.Quillrat=>"Quillrat", WildlifeKind.GreyWolf=>"Grey Wolf",
                    WildlifeKind.DarkWolf=>"Dark Wolf", _=>"Alpha Wolf"};
                def.stableId = kind switch {
                    WildlifeKind.Quillrat=>"ironpine.quillrat",WildlifeKind.AlphaWolf=>"ironpine.alpha-wolf",
                    _=>"ironpine.wolf"};
                def.familyId=def.stableId;
                def.tags = new[]{"ironpine","wildlife","no-loot"};
                int baseline=Mathf.Max(1,template.experienceReward);
                def.experienceReward=Mathf.Max(1,
                    Mathf.RoundToInt(baseline*(1+level*.42f)*
                        (kind==WildlifeKind.AlphaWolf?4f:1f)));
                def.moveSpeed=kind==WildlifeKind.AlphaWolf?5.25f:
                    kind==WildlifeKind.Quillrat?4.4f:5.3f;
                var attack=SaveAsset(name+"-attack",()=>
                {
                    var a=Object.Instantiate(template.basicAttack);
                    a.name=name+" bite or quill";
                    a.stableId="attack.ironpine."+name.ToLowerInvariant();
                    a.displayName=kind==WildlifeKind.Quillrat?"Homing quill":"Bite";
                    float multiplier=kind==WildlifeKind.Quillrat?.85f:
                        kind==WildlifeKind.DarkWolf?1.15f:
                        kind==WildlifeKind.AlphaWolf?1.5f:1f;
                    float minimumScale=kind==WildlifeKind.Quillrat?4f:
                        kind==WildlifeKind.DarkWolf?3.5f:kind==WildlifeKind.AlphaWolf?6f:1f;
                    float maximumScale=kind==WildlifeKind.Quillrat?5f:
                        kind==WildlifeKind.DarkWolf?5f:kind==WildlifeKind.AlphaWolf?8f:1f;
                    a.baseDamage*=multiplier*minimumScale; a.coefficient*=multiplier*minimumScale;
                    a.maximumDamageMultiplier*=maximumScale/minimumScale;
                    a.interval=kind==WildlifeKind.GreyWolf?2.2f:
                        kind==WildlifeKind.DarkWolf?2f:kind==WildlifeKind.AlphaWolf?2.4f:2.8f;
                    a.windup=.25f;
                    a.reach=kind==WildlifeKind.Quillrat?15f:1.25f;
                    return a;
                });
                def.basicAttack=attack;
                return def;
            });
        }
        [CliCommand("dicefree.ironpine-wildlife.install","Author 22 Ironpine creatures, stat assets, home territories and alpha hunting grounds.")]
        public static object Install()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Stop Play Mode before authoring");
            for(int i=0;i<SceneManager.sceneCount;i++)
                Require(!SceneManager.GetSceneAt(i).isDirty,"Save current user scene first");
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var root=Find(scene,Root);
                Require(root!=null,"Forest environment has not been installed");
                var existing=root.Find(WildlifeRoot);
                if(existing!=null)
                    return new{alreadyInstalled=true,creatures=existing.GetComponentsInChildren<IronpineWildlife>(true).Length};
                var navSource=All<WorldNavigation>(scene).FirstOrDefault();
                Require(navSource!=null&&navSource.Data!=null,"Saved Cornberg navigation missing");
                var nav=NavMesh.AddNavMeshData(navSource.Data);
                try
                {
                    Folder();
                    var template=All<CombatActor>(scene).FirstOrDefault(a=>
                        a.GetComponent<ActorStats>()?.Definition?.stableId=="enemy.road-slime" &&
                        a.GetComponent<EnemyVariant>()!=null);
                    Require(template!=null,"Existing authored road slime enemy template unavailable");
                    var group=new GameObject(WildlifeRoot);
                    group.transform.SetParent(root,false);
                    group.AddComponent<IronpinePartyCredit>();
                    var ground=AlphaGrounds.Select(p=>Nav(p,12f)).ToArray();
                    var kinds=new List<WildlifeKind>();
                    kinds.AddRange(Enumerable.Repeat(WildlifeKind.Quillrat,10));
                    kinds.AddRange(Enumerable.Repeat(WildlifeKind.GreyWolf,7));
                    kinds.AddRange(Enumerable.Repeat(WildlifeKind.DarkWolf,4));
                    kinds.Add(WildlifeKind.AlphaWolf);
                    var homes=new List<Vector3>();
                    for(int i=0;i<21;i++)
                    {
                        Vector3 h=Spawn(i);
                        Require(homes.All(v=>Vector3.Distance(h,v)>2.5f),
                            "Wildlife homes overlap for slot "+i);
                        homes.Add(h);
                    }
                    int count=0;
                    for(int i=0;i<kinds.Count;i++)
                    {
                        var kind=kinds[i];
                        int level=kind switch {
                            WildlifeKind.Quillrat=>12+i%5,
                            WildlifeKind.GreyWolf=>14+(i-10)%3,
                            WildlifeKind.DarkWolf=>17+(i-17)%3,
                            _=>20};
                        var position=i<21?homes[i]:ground[0];
                        var go=Object.Instantiate(template.gameObject,position,Quaternion.identity,group.transform);
                        if(PrefabUtility.IsPartOfPrefabInstance(go))
                            PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
                        go.name=kind+" "+(i+1).ToString("D2")+" (Lv "+level+")";
                        foreach(Transform child in go.transform.Cast<Transform>().ToArray())
                            Object.DestroyImmediate(child.gameObject);
                        foreach(var component in go.GetComponents<MonoBehaviour>())
                        {
                            if (component is ActorStats || component is Health ||
                                component is CombatActor || component is DiceFree.Characters.TraversalMotor ||
                                component is BasicAttack || component is AggroBehaviour ||
                                component is CombatStatusController || component is DefeatReporter)
                                continue;
                            Object.DestroyImmediate(component);
                        }
                        var stats=go.GetComponent<ActorStats>();
                        stats.Configure(Definition(template.GetComponent<ActorStats>().Definition,kind,level),level);
                        if(go.GetComponent<DefeatReporter>()==null)go.AddComponent<DefeatReporter>();
                        var variant=(kind==WildlifeKind.Quillrat?i%5/2:0);
                        var visual=Model(kind,variant,go.transform);
                        Vector3[] sites;
                        if(kind==WildlifeKind.AlphaWolf)sites=ground;
                        else
                        {
                            var seed=homes[i];
                            var list=new List<Vector3>{seed};
                            for(int s=0;s<8;s++)
                            {
                                var offset=new Vector3(Mathf.Sin(s*2.399f)*5f,0,Mathf.Cos(s*2.399f)*5f);
                                if(NavMesh.SamplePosition(seed+offset,out var hit,2f,NavMesh.AllAreas))
                                    list.Add(hit.position);
                            }
                            sites=list.ToArray();
                        }
                        var wildlife=go.AddComponent<IronpineWildlife>();
                        wildlife.Configure(kind,visual,sites,kind==WildlifeKind.AlphaWolf?ground:null);
                        var agent=go.GetComponent<NavMeshAgent>();
                        if(agent!=null)
                        {
                            agent.radius=kind==WildlifeKind.AlphaWolf?.75f:kind==WildlifeKind.Quillrat?.35f:.52f;
                            agent.height=kind==WildlifeKind.AlphaWolf?2.2f:kind==WildlifeKind.Quillrat?.9f:1.45f;
                            agent.enabled=false; // TraversalMotor enables it after WorldNavigation is ready.
                        }
                        var capsule=go.GetComponent<CapsuleCollider>();
                        if(capsule!=null)
                        {
                            capsule.radius=kind==WildlifeKind.AlphaWolf?.75f:kind==WildlifeKind.Quillrat?.35f:.52f;
                            capsule.height=kind==WildlifeKind.AlphaWolf?2.2f:kind==WildlifeKind.Quillrat?.9f:1.45f;
                            capsule.center=Vector3.up*capsule.height*.5f;
                        }
                        count++;
                    }
                    EditorSceneManager.MarkSceneDirty(scene);
                    Require(EditorSceneManager.SaveScene(scene),"Failed to save wildlife to Cornberg scene");
                    AssetDatabase.SaveAssets();
                    return new{success=true,created=count,quillrats=10,grey=7,dark=4,alpha=1,alphaGrounds=ground.Length};
                }
                finally {nav.Remove();}
            }
            finally {if(previous.Length>0) EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }

        [CliCommand("dicefree.ironpine-wildlife.validate","Validate authored Ironpine wildlife quotas, levels, navigation and unique identities.")]
        public static object Validate()
        {
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var root=Find(scene,WildlifeRoot);
                Require(root!=null,"Wildlife group not yet installed");
                var all=root.GetComponentsInChildren<IronpineWildlife>(true);
                Require(all.Length==22,"Expected 22 wildlife, found "+all.Length);
                Require(all.Count(a=>a.Kind==WildlifeKind.Quillrat)==10,"Quillrat count");
                Require(all.Count(a=>a.Kind==WildlifeKind.GreyWolf)==7,"Grey Wolf count");
                Require(all.Count(a=>a.Kind==WildlifeKind.DarkWolf)==4,"Dark Wolf count");
                Require(all.Count(a=>a.Kind==WildlifeKind.AlphaWolf)==1,"Alpha count");
                Require(all.All(a=>a.GetComponent<ActorStats>()?.Level>=12 &&
                    a.GetComponent<ActorStats>().Level<=20),"Level range invalid");
                Require(all.All(a=>a.GetComponent<ActorStats>()?.Definition!=null &&
                    a.GetComponent<DefeatReporter>()!=null),"Missing combat/credit setup");
                Require(root.GetComponent<IronpinePartyCredit>()!=null,"Party credit bridge missing");
                Require(all.Select(a=>a.transform.position).Distinct().Count()==22,
                    "Duplicate wildlife homes");
                return new {success=true,counts=new[]{10,7,4,1},navigation="Spawn positions authored from NavMesh"};
            }
            finally{if(previous.Length>0) EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }
    }
}
