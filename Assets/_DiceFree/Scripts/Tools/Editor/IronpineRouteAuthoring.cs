using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DiceFree.EditorTools
{
    internal static class IronpineRouteAuthoring
    {
        private const string ScenePath="Assets/_DiceFree/Scenes/Cornberg.unity";
        private const string Root="12 - Ironpine Forest pioneer route";
        private const string Art="Assets/_DiceFree/Art/World/Ironpine";
        private static readonly Vector2[] Route={
            new(112,43),new(140,44),new(158,45),new(175,47),
            new(174,64),new(180,80),new(183,98),new(196,112),
            new(208,130),new(223,149)
        };

        private static void Require(bool condition,string message)
        {
            if(!condition)throw new InvalidOperationException("IRONPINE: "+message);
        }
        private static Vector3 OnGround(Vector2 point) =>
            CornbergLandscape.Ground(point.x,point.y);
        private static float DistanceToTrail(Vector2 p)
        {
            float best=float.MaxValue;
            for(int i=1;i<Route.Length;i++)
            {
                Vector2 a=Route[i-1],b=Route[i];
                Vector2 d=b-a;
                float f=Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);
                best=Mathf.Min(best,Vector2.Distance(p,a+d*f));
            }
            return best;
        }
        private static T[] All<T>(Scene scene) where T:Component =>
            scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<T>(true)).ToArray();
        private static GameObject Group(string name,Transform parent)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            return go;
        }

        private static Vector2 Catmull(Vector2 p0,Vector2 p1,Vector2 p2,
            Vector2 p3,float t)
        {
            float t2=t*t,t3=t2*t;
            return .5f*((2*p1)+(-p0+p2)*t+
                (2*p0-5*p1+4*p2-p3)*t2+
                (-p0+3*p1-3*p2+p3)*t3);
        }

        private static Mesh CreateRoad()
        {
            // One continuous curved ribbon. Averaged tangents at all joins
            // prevent exposed grassy cracks where straight segments met.
            var centres=new List<Vector2>();
            for(int i=0;i<Route.Length-1;i++)
            {
                Vector2 p0=Route[Mathf.Max(0,i-1)],p1=Route[i],p2=Route[i+1],
                    p3=Route[Mathf.Min(Route.Length-1,i+2)];
                int steps=Mathf.CeilToInt(Vector2.Distance(p1,p2)*1.2f);
                for(int j=0;j<steps;j++)
                    centres.Add(Catmull(p0,p1,p2,p3,j/(float)steps));
            }
            centres.Add(Route[Route.Length-1]);
            var verts=new List<Vector3>();
            var uv=new List<Vector2>();
            var triangles=new List<int>();
            float traveled=0;
            for(int i=0;i<centres.Count;i++)
            {
                Vector2 before=centres[Mathf.Max(0,i-1)];
                Vector2 after=centres[Mathf.Min(centres.Count-1,i+1)];
                var tangent=(after-before).normalized;
                var outward=new Vector2(-tangent.y,tangent.x);
                float halfWidth=2.23f+Mathf.Sin(i*.13f)*.15f;
                Vector2 left=centres[i]-outward*halfWidth;
                Vector2 right=centres[i]+outward*halfWidth;
                verts.Add(OnGround(left)+Vector3.up*.285f);
                verts.Add(OnGround(right)+Vector3.up*.285f);
                if(i>0)traveled+=Vector2.Distance(centres[i-1],centres[i]);
                uv.Add(new Vector2(0,traveled/5f));
                uv.Add(new Vector2(1,traveled/5f));
                if(i<centres.Count-1)
                {
                    int v=i*2;
                    triangles.AddRange(new[]{v,v+1,v+2,v+2,v+1,v+3});
                }
            }
            var mesh=new Mesh{name="Ironpine winding continuous road",
                indexFormat=IndexFormat.UInt32};
            mesh.SetVertices(verts);mesh.SetTriangles(triangles,0);
            mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateGroundPatch(float radius,int segments,int seed)
        {
            var rand=new System.Random(seed);
            var vertices=new Vector3[segments+1];
            var uv=new Vector2[vertices.Length];
            vertices[0]=Vector3.zero;uv[0]=new Vector2(.5f,.5f);
            for(int i=0;i<segments;i++)
            {
                float angle=i*Mathf.PI*2f/segments;
                float jitter=.8f+(float)rand.NextDouble()*.38f;
                vertices[i+1]=new Vector3(Mathf.Cos(angle)*radius*jitter,
                    .02f,Mathf.Sin(angle)*radius*jitter);
                uv[i+1]=new Vector2(.5f+Mathf.Cos(angle)*.5f,
                    .5f+Mathf.Sin(angle)*.5f);
            }
            var indices=new int[segments*3];
            for(int i=0;i<segments;i++)
            {
                indices[i*3]=0;indices[i*3+1]=(i+1)%segments+1;
                indices[i*3+2]=i+1;
            }
            var mesh=new Mesh{name="Pine needle ground patch"};
            mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=indices;
            mesh.RecalculateNormals();return mesh;
        }

        private static void SaveMesh(Mesh mesh,string path)
        {
            var previous=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(previous!=null)AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mesh,path);
        }

        private static int OpenRoute(Scene scene)
        {
            int deactivated=0;
            foreach(var t in All<Transform>(scene))
            {
                if(!t.gameObject.activeSelf || t.position.x<136f)continue;
                if(t.name!="Woodland tree" &&
                   !t.name.StartsWith("Trees/Tree_",StringComparison.Ordinal) &&
                   !t.name.StartsWith("East inside",StringComparison.Ordinal))
                    continue;
                if(!t.GetComponentsInChildren<Collider>(true).Any(c=>c.enabled&&!c.isTrigger))
                    continue;
                var p=new Vector2(t.position.x,t.position.z);
                if(DistanceToTrail(p)>5.8f)continue;
                if(p.x>231f)continue; // preserve the outer nature perimeter
                t.gameObject.SetActive(false);
                deactivated++;
            }
            return deactivated;
        }

        private static int PlantPines(Scene scene,Transform group,Mesh patchMesh,Material patchMaterial)
        {
            var templates=All<Transform>(scene)
                .Where(t=>t.gameObject.activeInHierarchy&&t.name=="Woodland tree")
                .Where(t=>t.GetComponentsInChildren<CapsuleCollider>(true).Any(c=>c.enabled)&&
                          t.GetComponentsInChildren<NavMeshObstacle>(true).Any(o=>o.enabled))
                .Select(t=>new{root=t,model=t.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(v=>v.name=="Trees/Tree_Pine - 3D"||
                                       v.name=="Trees/Tree_Spruce - 3D"||
                                       v.name=="Trees/Tree_Cypress - 3D")})
                .Where(x=>x.model!=null)
                .GroupBy(x=>x.model.name).Select(g=>g.First().root).ToArray();
            Require(templates.Length>=2,"no existing collision-ready pine/spruce models");
            var rnd=new System.Random(260611);
            var positions=All<Transform>(scene).Where(t=>t.gameObject.activeInHierarchy&&
                t.GetComponentsInChildren<CapsuleCollider>(true).Any(c=>c.enabled)&&
                t.position.x>128&&t.position.x<237&&t.position.z>48&&t.position.z<175)
                .Select(t=>new Vector2(t.position.x,t.position.z)).ToList();
            int planted=0,patches=0;
            for(float x=143;x<=232;x+=6.1f)
            for(float z=53;z<=166;z+=6.8f)
            {
                if(rnd.NextDouble()<.19)continue;
                float px=x+(float)(rnd.NextDouble()-.5)*4.5f;
                float pz=z+(float)(rnd.NextDouble()-.5)*4.5f;
                var p=new Vector2(px,pz);
                if(DistanceToTrail(p)<7.1f)continue;
                if(Vector2.Distance(p,new Vector2(223,149))<15f)continue;
                if(positions.Any(q=>Vector2.Distance(q,p)<4.1f))continue;
                var target=OnGround(p);
                if(!NavMesh.SamplePosition(target,out var hit,3f,NavMesh.AllAreas))continue;
                // Use the complete existing WC3/Magicka-style tree family:
                // original art, bark, collider, NavMeshObstacle and fog occluder.
                var template=templates[rnd.Next(templates.Length)];
                var instance=Object.Instantiate(template.gameObject,
                    hit.position,Quaternion.Euler(0,(float)rnd.NextDouble()*360,0),group);
                instance.name="Ironpine conifer "+planted.ToString("D3");
                float scale=.91f+(float)rnd.NextDouble()*.45f;
                instance.transform.localScale=template.localScale*scale;
                Require(instance.GetComponentsInChildren<Collider>(true).Any(c=>c.enabled&&!c.isTrigger),
                    "new pine lost physics");
                Require(instance.GetComponentsInChildren<NavMeshObstacle>(true).Any(o=>o.enabled),
                    "new pine lost NavMeshObstacle");
                planted++;
                positions.Add(p);
                // Mossy needles around selected roots: visual only, not collision.
                if(false) // disabled pending a feathered forest-floor shader
                {
                    var patch=Group("Pine-needle carpet "+patches.ToString("D3"),group);
                    patch.layer=2;
                    patch.transform.position=hit.position+Vector3.up*.09f;
                    patch.transform.rotation=Quaternion.Euler(0,(float)rnd.NextDouble()*360,0);
                    patch.transform.localScale=Vector3.one*(.8f+(float)rnd.NextDouble()*.65f);
                    patch.AddComponent<MeshFilter>().sharedMesh=patchMesh;
                    var renderer=patch.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial=patchMaterial;
                    renderer.shadowCastingMode=ShadowCastingMode.Off;
                    patches++;
                }
            }
            Require(planted>=45,"underpopulated Ironpine forest: "+planted);
            return planted;
        }

        [CliCommand("dicefree.ironpine.install","Install navigable northeastern Ironpine road, evergreen woods and provisional Village 2 clearing.")]
        public static object Install()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,"stop Play Mode");
            for(int i=0;i<SceneManager.sceneCount;i++)
                Require(!SceneManager.GetSceneAt(i).isDirty,"save unsaved scene first");
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                Require(!scene.GetRootGameObjects().Any(g=>g.name==Root),"Ironpine is already installed");
                var world=All<Transform>(scene).First(t=>t.name=="Cornberg - World 1 starter pocket");
                var nav=All<WorldNavigation>(scene).FirstOrDefault();
                Require(nav!=null&&nav.Data!=null,"saved navigation asset missing");
                var navInstance=NavMesh.AddNavMeshData(nav.Data);
                try
                {
                    foreach(var p in Route)
                        Require(NavMesh.SamplePosition(OnGround(p),out _,9f,NavMesh.AllAreas),
                            "road point not walkable: "+p);
                    var root=Group(Root,world);
                    var roadGroup=Group("Ironpine Trail â€” toward northern village",root.transform);
                    Directory.CreateDirectory(Art);
                    var mesh=CreateRoad();
                    SaveMesh(mesh,Art+"/IronpineTrail.asset");
                    var road=Group("Winding pine forest road",roadGroup.transform);
                    road.layer=2;
                    var filter=road.AddComponent<MeshFilter>();filter.sharedMesh=mesh;
                    var renderer=road.AddComponent<MeshRenderer>();
                    var oldRoad=All<Transform>(scene).First(t=>t.name=="Mountain to east road");
                    renderer.sharedMaterial=oldRoad.GetComponent<MeshRenderer>().sharedMaterial;
                    renderer.shadowCastingMode=ShadowCastingMode.Off;
                    var speed=oldRoad.GetComponent<TravelSurface>();
                    if(speed!=null && speed.Definition!=null)
                        road.AddComponent<TravelSurface>().Configure("travel.ironpine.northeast",
                            speed.Definition,filter,new Bounds(new Vector3(180,0,100),
                                new Vector3(190,18,200)));

                    int cleared=OpenRoute(scene);
                    var conifers=Group("Dense evergreen groves",root.transform);
                    var needleMesh=CreateGroundPatch(1.45f,11,1181);
                    SaveMesh(needleMesh,Art+"/PineNeedleCarpet.asset");
                    var needleMat=AssetDatabase.LoadAssetAtPath<Material>(Art+"/PineNeedleCarpet.mat");
                    if(needleMat==null)
                    {
                        var shader=Shader.Find("Universal Render Pipeline/Lit");
                        needleMat=new Material(shader);
                        needleMat.SetColor("_BaseColor",new Color(.27f,.35f,.20f,.75f));
                        needleMat.SetFloat("_Smoothness",.02f);
                        needleMat.SetFloat("_Surface",1f);
                        needleMat.SetOverrideTag("RenderType","Transparent");
                        needleMat.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);
                        needleMat.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);
                        needleMat.SetInt("_ZWrite",0);
                        needleMat.renderQueue=3000;
                        AssetDatabase.CreateAsset(needleMat,Art+"/PineNeedleCarpet.mat");
                    }
                    int pines=PlantPines(scene,conifers.transform,needleMesh,needleMat);
                    var arrival=Group("Northern village approach â€” provisional site",root.transform);
                    arrival.transform.position=OnGround(new Vector2(223,149));
                    // A clear footprint reserved for a future village, not actual village yet.
                    var site=Group("Village 2 reserved clearing â€” NO buildings yet",arrival.transform);
                    site.transform.localPosition=Vector3.zero;
                    var sign=AssetDatabase.LoadAssetAtPath<GameObject>(
                        "Assets/_DiceFree/Art/World/VisualBatch01/Models/sign_01.fbx");
                    if(sign!=null)
                    {
                        var waypost=(GameObject)PrefabUtility.InstantiatePrefab(sign);
                        waypost.name="Ironpine northbound waypost";
                        waypost.transform.SetParent(arrival.transform,false);
                        waypost.transform.localPosition=new Vector3(-4f,0,-5f);
                        waypost.transform.localScale=Vector3.one*1.3f;
                        foreach(var c in waypost.GetComponentsInChildren<Collider>())
                            Object.DestroyImmediate(c);
                    }
                    EditorSceneManager.MarkSceneDirty(scene);
                    Require(EditorSceneManager.SaveScene(scene),"scene save failed");
                    AssetDatabase.SaveAssets();
                    return new{success=true,coniferTrees=pines,oldTreesCleared=cleared,
                        roadSegments=Route.Length-1,site="Village 2 reserved clearing",
                        villageLocation=arrival.transform.position.ToString("F1")};
                }finally{navInstance.Remove();}
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }
        private static readonly Dictionary<string,Material> Tinted=new();
        private static Material Evergreen(Material source)
        {
            if(source==null)return null;
            // Always resolve through the canonical Cornberg source. Re-running
            // a palette pass must NEVER create Ironpine_Ironpine_* copies.
            string baseName=source.name;
            while(baseName.StartsWith("Ironpine_",StringComparison.Ordinal))
                baseName=baseName.Substring("Ironpine_".Length);
            if(baseName!=source.name)
            {
                var original=AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/_DiceFree/Materials/CornbergBatch/"+baseName+".mat");
                if(original!=null)source=original;
                else if(baseName=="Sunlit_foliage")
                    return AssetDatabase.LoadAssetAtPath<Material>(
                        Art+"/Materials/Ironpine_Sunlit_foliage.mat");
            }
            var lower=source.name.ToLowerInvariant();
            if(!lower.Contains("foliage")&&!lower.Contains("leafaccent")&&
               !lower.Contains("bark"))return source;
            if(Tinted.TryGetValue(source.name,out var cached))return cached;
            Directory.CreateDirectory(Art+"/Materials");
            string id=source.name.Replace('/','_').Replace(' ','_');
            string path=Art+"/Materials/Ironpine_"+id+".mat";
            var instance=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(instance==null)
            {
                instance=new Material(source);
                AssetDatabase.CreateAsset(instance,path);
            }
            bool bark=lower.Contains("bark");
            bool accent=lower.Contains("leafaccent");
            int variant=Math.Abs(source.name.GetHashCode())%3;
            Color tint=bark
                ? new Color(.39f,.35f,.29f)
                : accent
                   ? (variant==0?new Color(.35f,.53f,.36f):
                      variant==1?new Color(.31f,.50f,.36f):
                                 new Color(.39f,.54f,.34f))
                   : (variant==0?new Color(.27f,.45f,.30f):
                      variant==1?new Color(.23f,.43f,.31f):
                                 new Color(.32f,.48f,.30f));
            instance.shader=Shader.Find("Universal Render Pipeline/Lit");
            Require(instance.shader!=null&&instance.shader.isSupported,
                "palette shader unsupported: "+source.name);
            instance.SetColor("_BaseColor",tint);
            instance.SetFloat("_Smoothness",bark?.06f:.11f);
            instance.enableInstancing=true;
            EditorUtility.SetDirty(instance);
            Tinted[source.name]=instance;
            return instance;
        }

        [CliCommand("dicefree.ironpine.polish",
            "Fix top-facing winding road, give pine trees cool forest palette and clear the approach site.")]
        public static object Polish()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Edit mode only");
            for(int i=0;i<SceneManager.sceneCount;i++)
                Require(!SceneManager.GetSceneAt(i).isDirty,"save unsaved scene first");
            var previous=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var group=All<Transform>(scene).First(t=>t.name==Root);
                var road=group.GetComponentsInChildren<MeshFilter>(true)
                    .First(f=>f.name=="Winding pine forest road");
                var source=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/IronpineTrail.asset");
                Require(source!=null,"Ironpine road asset missing");
                var rebuilt=CreateRoad();
                EditorUtility.CopySerialized(rebuilt,source);
                Object.DestroyImmediate(rebuilt);
                road.sharedMesh=source;
                EditorUtility.SetDirty(source);
                var roadMaterialPath=Art+"/IronpineTrailEarth.mat";
                var oldRoad=road.GetComponent<MeshRenderer>().sharedMaterial;
                var roadMaterial=AssetDatabase.LoadAssetAtPath<Material>(roadMaterialPath);
                if(roadMaterial==null)
                {
                    roadMaterial=new Material(oldRoad);
                    AssetDatabase.CreateAsset(roadMaterial,roadMaterialPath);
                }
                roadMaterial.shader=Shader.Find("Universal Render Pipeline/Unlit");
                Require(roadMaterial.shader!=null&&roadMaterial.shader.isSupported,
                    "Ironpine road shader missing");
                roadMaterial.SetTexture("_BaseMap",null);
                roadMaterial.SetColor("_BaseColor",new Color(.53f,.43f,.30f,1f));
                roadMaterial.SetFloat("_Cull",0f);
                roadMaterial.doubleSidedGI=true;
                roadMaterial.enableInstancing=true;
                EditorUtility.SetDirty(roadMaterial);
                road.GetComponent<MeshRenderer>().sharedMaterial=roadMaterial;
                var carpet=AssetDatabase.LoadAssetAtPath<Material>(
                    Art+"/PineNeedleCarpet.mat");
                if(carpet!=null){carpet.SetFloat("_Cull",0f);EditorUtility.SetDirty(carpet);}
                var patch=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/PineNeedleCarpet.asset");
                var corrected=CreateGroundPatch(1.45f,11,1181);
                EditorUtility.CopySerialized(corrected,patch);
                Object.DestroyImmediate(corrected);
                EditorUtility.SetDirty(patch);

                Tinted.Clear();
                int renderers=0,cleared=0;
                // Hard-edged needle discs looked like dark polygonal stains at
                // game zoom. Preserve terrain/physics; remove only our temporary
                // decorative patches until a properly feathered ground shader exists.
                foreach(var patchObject in group.GetComponentsInChildren<Transform>(true)
                    .Where(t=>t.name.StartsWith("Pine-needle carpet "))
                    .ToArray())
                    Object.DestroyImmediate(patchObject.gameObject);
                var conifers=group.GetComponentsInChildren<Transform>(true)
                    .Where(t=>t.name.StartsWith("Ironpine conifer ")).ToArray();
                // Color new pines, and old trees *inside the Ironpine body*
                // without altering the shared materials in sunny Cornberg.
                var existing=All<Transform>(scene)
                    .Where(t=>t.gameObject.activeInHierarchy&&t.name=="Woodland tree"&&
                        t.position.x>=161f&&t.position.z>=77f&&t.position.z<=171f)
                    .ToArray();
                foreach(var t in conifers.Concat(existing))
                {
                    foreach(var mesh in t.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        if(!mesh.gameObject.activeInHierarchy)continue;
                        var original=mesh.sharedMaterials;
                        var updated=original.Select(Evergreen).ToArray();
                        if(!original.SequenceEqual(updated))
                        {mesh.sharedMaterials=updated;renderers++;}
                    }
                }
                Vector2 goal=new Vector2(223,149);
                foreach(var t in existing)
                {
                    var pos=new Vector2(t.position.x,t.position.z);
                    if(Vector2.Distance(pos,goal)>11f||!t.gameObject.activeSelf)continue;
                    t.gameObject.SetActive(false);
                    cleared++;
                }
                EditorSceneManager.MarkSceneDirty(scene);
                Require(EditorSceneManager.SaveScene(scene),"save failed");
                AssetDatabase.SaveAssets();
                return new{success=true,roadFaceFixed=true,renderersTinted=renderers,
                    customMaterials=Tinted.Count,originalTreesClearedAroundSite=cleared};
            }finally{EditorSceneManager.RestoreSceneManagerSetup(previous);}
        }

        [CliCommand("dicefree.ironpine.validate","Validate Ironpine forest, trail and existing Cornberg NavMesh integrity.")]
        public static object Validate()
        {
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var root=All<Transform>(scene).FirstOrDefault(t=>t.name==Root);
                Require(root!=null,"biome not installed");
                var pines=root.GetComponentsInChildren<Transform>(true).Where(
                    t=>t.name.StartsWith("Ironpine conifer ")&&t.gameObject.activeInHierarchy).ToArray();
                Require(pines.Length>=45,"pines absent");
                Require(pines.All(t=>t.GetComponentsInChildren<Collider>(true)
                    .Any(c=>c.enabled&&!c.isTrigger)&&t.GetComponentsInChildren<NavMeshObstacle>(true)
                    .Any(o=>o.enabled)),"conifer collision/navmesh obstacle missing");
                var road=root.GetComponentsInChildren<Transform>(true)
                    .First(t=>t.name=="Winding pine forest road");
                Require(road.GetComponent<MeshFilter>()?.sharedMesh?.vertexCount>300,
                    "winding road missing");
                var village=root.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t=>t.name.StartsWith("Village 2 reserved"));
                Require(village!=null,"second-village site not reserved");
                var navigation=All<WorldNavigation>(scene).First();
                var nav=NavMesh.AddNavMeshData(navigation.Data);
                try
                {
                    Require(NavMesh.SamplePosition(new Vector3(112,0,43),out var a,8,NavMesh.AllAreas),
                        "existing road inaccessible");
                    Require(NavMesh.SamplePosition(village.position,out var b,10,NavMesh.AllAreas),
                        "future village site inaccessible");
                    var path=new NavMeshPath();
                    Require(NavMesh.CalculatePath(a.position,b.position,NavMesh.AllAreas,path)
                        &&path.status==NavMeshPathStatus.PathComplete,
                        "Village 2 approach cannot be reached by NavMesh");
                    return new{success=true,pines=pines.Length,roadVertices=road.GetComponent<MeshFilter>().sharedMesh.vertexCount,
                        reachable="Cornberg to Ironpine to Village 2 reserved clearing",
                        routeCorners=path.corners.Length,existingNavigationIntact=true};
                }finally{nav.Remove();}
            }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
    }
}
