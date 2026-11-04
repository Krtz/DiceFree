using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Core;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DiceFree.EditorTools
{
    public static class BriarbackFireLandmark
    {
        const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        const string Folder = "Assets/_DiceFree/Art/World/BriarbackFire";
        const string Site = "Fire Shaman settlement - provisional";
        const string Landmark = "Ember oath clearing";
        const string Legacy = "Original fire blockout (hidden)";
        const string Recovery = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-fire-landmark-20261104.unity";
        const string RemainingRecovery = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-other-landmarks-20261104.unity";
        static readonly (string element, string title)[] OtherSites =
        {
            ("Earth", "Splitroot cairn"),
            ("Lightning", "Stormcall stakes"),
            ("Ice", "Rimeglass vigil")
        };

        static readonly (string name, Color color, float emission)[] Palette =
        {
            ("Ash basalt", new Color(.16f,.145f,.13f),0),
            ("Broken edge", new Color(.29f,.23f,.18f),0),
            ("Oxidized copper", new Color(.48f,.26f,.105f),0),
            ("Cinder ash", new Color(.091f,.063f,.049f),0),
            ("Oath cloth", new Color(.44f,.085f,.055f),0),
            ("Ritual ochre", new Color(.88f,.56f,.23f),0),
            ("Amber flame", new Color(1,.29f,.037f),1.25f),
            ("Golden flame", new Color(1,.68f,.13f),2.0f),
            ("Old timber", new Color(.23f,.124f,.068f),0),
        };

        static void Need(bool condition, string problem)
        {
            if (!condition) throw new InvalidOperationException(problem);
        }

        static Transform FindSite(string element = "Fire")
        {
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath && !scene.isDirty, "Open clean Cornberg before using the fire landmark tools.");
            return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Single(t => t.name == element + " Shaman settlement - provisional" &&
                    t.parent != null && t.parent.name.StartsWith("Briarback elemental settlements", StringComparison.Ordinal));
        }

        [CliCommand("dicefree.briarback.fire.author", "Author the Ember Oath Clearing visual pass without touching gameplay.")]
        public static object Author()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "Stop Play Mode before authoring.");
            var site = FindSite();
            Need(site.Find(Landmark) == null && site.Find(Legacy) == null, "The fire landmark already exists.");
            Need(!AssetDatabase.IsValidFolder(Folder) && !File.Exists(Recovery), "Existing art or recovery data would be overwritten.");
            var original = site.Cast<Transform>().ToArray();
            Need(original.Length >= 14 && original.All(t => t.GetComponent<Renderer>() != null) &&
                 site.GetComponentsInChildren<Collider>(true).Length == 0,
                 "The original fire visuals don't match the expected collider-free blockout.");
            Need(Shader.Find("Universal Render Pipeline/Lit") != null, "URP Lit not available.");
            var art = new Clearing(site.position);
            art.Design();
            Need(EditorSceneManager.SaveScene(site.gameObject.scene, Recovery, true), "Could not save recovery scene.");
            AssetDatabase.CreateFolder("Assets/_DiceFree/Art/World", "BriarbackFire");
            var root = Child(site, Landmark);
            art.Install(root);
            var legacy = Child(site, Legacy);
            foreach (var item in original) item.SetParent(legacy, true);
            legacy.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(site.gameObject.scene);
            AssetDatabase.SaveAssets();
            Need(EditorSceneManager.SaveScene(site.gameObject.scene), "Could not save the landmark.");
            return Validate();
        }

        [CliCommand("dicefree.briarback.landmarks.author", "Apply the earth, lightning and ice landmarks without changing gameplay.")]
        public static object AuthorRemaining()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "Stop Play Mode before authoring.");
            var sites = OtherSites.Select(s => FindSite(s.element)).ToArray();
            var originals = sites.Select(s => s.Cast<Transform>().ToArray()).ToArray();
            Need(!File.Exists(RemainingRecovery), "Remaining site recovery already exists.");
            for (int i=0;i<sites.Length;i++)
                Need(!AssetDatabase.IsValidFolder(ArtFolder(OtherSites[i].element)) && originals[i].Length >= 14 &&
                     originals[i].All(t => t.GetComponent<Renderer>() != null) &&
                     sites[i].GetComponentsInChildren<Collider>(true).Length == 0,
                     "Original site isn't an untouched visual blockout: " + sites[i].name);
            var art = sites.Select((s,i) => new Clearing(s.position,OtherSites[i].element)).ToArray();
            foreach (var clearing in art) clearing.Design();
            var scene = sites[0].gameObject.scene;
            Need(EditorSceneManager.SaveScene(scene,RemainingRecovery,true),"Could not create remaining-site recovery.");
            for(int i=0;i<sites.Length;i++)
            {
                var (element,title) = OtherSites[i];
                AssetDatabase.CreateFolder("Assets/_DiceFree/Art/World","Briarback"+element);
                art[i].Install(Child(sites[i],title));
                var legacy = Child(sites[i],"Original "+element.ToLowerInvariant()+" blockout (hidden)");
                foreach(var item in originals[i]) item.SetParent(legacy,true);
                legacy.gameObject.SetActive(false);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            Need(EditorSceneManager.SaveScene(scene), "Could not save the three new landmarks.");
            return ValidateRemaining();
        }

        [CliCommand("dicefree.briarback.landmarks.validate", "Check the three elemental art variants and retained blockouts.")]
        public static object ValidateRemaining()
        {
            var report=OtherSites.Select(s =>
            {
                var site=FindSite(s.element);
                var root=site.Find(s.title);
                var legacy=site.Find("Original "+s.element.ToLowerInvariant()+" blockout (hidden)");
                Need(root!=null && legacy!=null && !legacy.gameObject.activeSelf &&
                     site.GetComponentsInChildren<Collider>(true).Length==0, "Invalid landmark at "+s.element);
                var meshes=root.GetComponentsInChildren<MeshFilter>();
                Need(meshes.Length>=5 && meshes.All(m=>m.sharedMesh!=null &&
                     AssetDatabase.GetAssetPath(m.sharedMesh).StartsWith(ArtFolder(s.element),StringComparison.Ordinal)) &&
                     root.GetComponentsInChildren<MeshRenderer>().All(r=>r.sharedMaterial!=null),
                     "Missing art references at "+s.element);
                return new { s.element, meshes=meshes.Length, retainedVisuals=legacy.childCount };
            }).ToArray();
            return new { marker="DICEFREE_ELEMENTAL_LANDMARKS_OK", report };
        }

        static string ArtFolder(string element) => "Assets/_DiceFree/Art/World/Briarback"+element;

        [CliCommand("dicefree.briarback.fire.validate", "Verify the saved ember landmark and retained original visuals.")]
        public static object Validate()
        {
            var site = FindSite();
            var root = site.Find(Landmark);
            var legacy = site.Find(Legacy);
            Need(root != null && legacy != null && !legacy.gameObject.activeSelf, "Missing landmark or hidden original visuals.");
            Need(site.GetComponentsInChildren<Collider>(true).Length == 0, "Visual site gained collision.");
            var meshes = root.GetComponentsInChildren<MeshFilter>();
            Need(meshes.Length == Palette.Length && meshes.All(m => m.sharedMesh != null &&
                AssetDatabase.GetAssetPath(m.sharedMesh).StartsWith(Folder, StringComparison.Ordinal)), "Landmark mesh references are incomplete.");
            Need(root.GetComponentsInChildren<Renderer>().All(r => r.sharedMaterial != null), "Landmark materials are incomplete.");
            return new { marker = "DICEFREE_FIRE_LANDMARK_OK", meshes = meshes.Length,
                triangles = meshes.Sum(m => m.sharedMesh.triangles.Length / 3), originalVisuals = legacy.childCount, colliders = 0 };
        }

        [CliCommand("dicefree.briarback.fire.capture", "Capture the fire site with the authored gameplay camera pose.")]
        public static object Capture()
        {
            return CaptureSite("Fire", Landmark);
        }

        [CliCommand("dicefree.briarback.landmarks.capture", "Capture earth, lightning and ice with the authored gameplay camera.")]
        public static object CaptureRemaining() => OtherSites.Select(s => CaptureSite(s.element,s.title)).ToArray();

        static object CaptureSite(string element,string title)
        {
            var site = FindSite(element);
            var gameCamera = Object.FindAnyObjectByType<ExplorationCamera>();
            Need(gameCamera != null, "Cornberg gameplay camera missing.");
            var output = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs", "Briarback"+element);
            Directory.CreateDirectory(output);
            var go = new GameObject("Temporary fire site review camera");
            var marker = new GameObject("Temporary approach camera target");
            var rt = new RenderTexture(1600, 900, 24);
            var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                var camera = go.AddComponent<Camera>();
                camera.CopyFrom(gameCamera.GetComponent<Camera>());
                camera.targetTexture = rt;
                bool authored = site.Find(title) != null;
                var files = new List<string>();
                foreach (var (name, offset) in new[] { ("approach", new Vector2(2,-7)), ("clearing", new Vector2(0,1)) })
                {
                    marker.transform.position = CornbergLandscape.Ground(site.position.x + offset.x, site.position.z + offset.y);
                    gameCamera.GetFollowPose(marker.transform, out var p, out var r, out var fov);
                    camera.transform.SetPositionAndRotation(p, r);
                    camera.fieldOfView = fov;
                    camera.Render();
                    RenderTexture.active = rt;
                    image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
                    image.Apply();
                    var file = Path.Combine(output, (authored ? "after-" : "before-") + name + ".png");
                    File.WriteAllBytes(file, image.EncodeToPNG());
                    files.Add(file);
                }
                return new { element, authored, files };
            }
            finally
            {
                RenderTexture.active = previous;
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(marker);
                Object.DestroyImmediate(image);
                rt.Release();
                Object.DestroyImmediate(rt);
            }
        }

        static Transform Child(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.layer = 2;
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        sealed class Surface
        {
            readonly List<Vector3> vertices = new();
            readonly List<int> triangles = new();
            public bool Empty => triangles.Count == 0;
            public void Tri(Vector3 a, Vector3 b, Vector3 c)
            {
                int index = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c);
                triangles.Add(index); triangles.Add(index + 1); triangles.Add(index + 2);
            }
            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                Tri(a,b,c); Tri(a,c,d);
            }
            public Mesh Bake(string name)
            {
                var mesh = new Mesh { name = name };
                mesh.SetVertices(vertices);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                return mesh;
            }
        }

        sealed class Clearing
        {
            readonly Vector3 origin;
            readonly string element;
            readonly Surface[] surfaces = Palette.Select(_ => new Surface()).ToArray();
            readonly System.Random random = new(67037);
            public Clearing(Vector3 origin,string element="Fire")
            {
                this.origin=origin;
                this.element=element;
            }
            float Range(float min, float max) => Mathf.Lerp(min, max, (float)random.NextDouble());
            Vector3 Ground(float x, float z) => CornbergLandscape.Ground(origin.x+x,origin.z+z) - origin;

            // Build faceted cross sections with explicit bevels rather than scaled primitives.
            void Profile(Vector3 at, int n, float xScale, float zScale, float angle,
                (float y, float r, float drift)[] sections, int face, int accent, float irregular = .08f)
            {
                float[] shape = Enumerable.Range(0, n).Select(_ => Range(1-irregular,1+irregular)).ToArray();
                Vector3 Point(int level, int i)
                {
                    var (y,r,drift) = sections[level];
                    var theta = angle + i * 2 * Mathf.PI / n;
                    return at + new Vector3(Mathf.Cos(theta)*r*xScale*shape[i]+drift,y,Mathf.Sin(theta)*r*zScale*shape[i]);
                }
                for (int j = 0; j < sections.Length-1; j++)
                for (int i = 0; i < n; i++)
                {
                    int next = (i+1)%n;
                    surfaces[(i+j)%5==0 ? accent : face].Quad(Point(j,i),Point(j+1,i),Point(j+1,next),Point(j,next));
                }
                var top = sections.Length-1;
                var center = at + new Vector3(sections[top].drift,sections[top].y,0);
                for (int i=0;i<n;i++) surfaces[face].Tri(center, Point(top,(i+1)%n), Point(top,i));
            }

            void Ember(Vector3 p, float width, float height)
            {
                Profile(p,5,width,width,.3f,new[] {(0f,1f,0f),(.35f*height,1.12f,0f),
                    (.73f*height,.58f,.14f*width),(height,.045f,.48f*width)},6,7,.17f);
                Profile(p+new Vector3(-width*.14f,0,-width*.43f),5,width*.59f,width*.64f,.8f,
                    new[] {(0f,1f,0f),(.31f*height,.93f,0f),(.71f*height,.03f,-.24f*width)},7,7);
            }

            void Brazier(float x, float z, float size)
            {
                var p = Ground(x,z);
                Profile(p,7,size,size,.25f,new[] {(-.11f,.57f,0f),(.13f,.65f,0f),(.51f,.42f,0f),
                    (.62f,.78f,0f),(.84f,.99f,0f),(.89f,.91f,0f)},0,2);
                Profile(p+Vector3.up*.885f,7,size,size,.25f,new[] {(0f,.79f,0f),(.018f,.75f,0f)},3,3);
                Ember(p+Vector3.up*.9f,size*.48f,size*1.6f);
                Ember(p+new Vector3(size*.30f,.89f,size*.18f),size*.23f,size*.84f);
            }

            void Banner(float x,float z,float height)
            {
                var p = Ground(x,z);
                Profile(p,5,.13f,.13f,.4f,new[] {(-.2f,1.6f,0f),(.3f,1.25f,0f),(height,1f,.08f),(height+.20f,.08f,.08f)},8,2);
                float left=.12f, right=1.36f, top=height-.3f, low=top-1.65f;
                Vector3 V(float u,float v,float front=0) => p+new Vector3(u,v,.18f+front);
                for(int i=0;i<4;i++)
                {
                    float a=Mathf.Lerp(left,right,i/4f), b=Mathf.Lerp(left,right,(i+1)/4f);
                    float fold=(i%2)*.08f, next=((i+1)%2)*.08f;
                    var p0=V(a,top,fold); var p1=V(b,top,next);
                    var p2=V(b,low-Mathf.Sin((i+1)*Mathf.PI/4)*.4f,next);
                    var p3=V(a,low-Mathf.Sin(i*Mathf.PI/4)*.4f,fold);
                    surfaces[4].Quad(p0,p1,p2,p3); surfaces[4].Quad(p3,p2,p1,p0);
                }
                var a0=V(.72f,top-.29f,-.025f);var b0=V(1.0f,top-.9f,-.025f);
                var c0=V(.72f,top-1.32f,-.025f);var d0=V(.44f,top-.9f,-.025f);
                surfaces[5].Quad(a0,b0,c0,d0);
            }

            public void Design()
            {
                // Maintain a low, open center. Make the sanctuary read from the southern approach.
                for (int i=0;i<20;i++)
                {
                    float a=i*Mathf.PI/10, r=4.3f+Range(-.18f,.18f);
                    var p=Ground(Mathf.Cos(a)*r,Mathf.Sin(a)*r);
                    Profile(p,5,Range(.53f,.82f),Range(.55f,.82f),a,
                        new[] {(-.11f,1f,0f),(.075f,1f,0f),(.13f,.78f,0f)},i%3==0?1:3,1);
                }
                for (int i=0;i<6;i++)
                {
                    var p=Ground(Range(-.42f,.42f),-5.5f-i*1.1f);
                    Profile(p,6,Range(.68f,1.03f),.38f,Range(-.3f,.3f),
                        new[] {(-.12f,1f,0f),(.022f,1f,0f),(.065f,.86f,0f)},1,3);
                }
                if (element!="Fire")
                {
                    DesignVariant();
                    return;
                }
                var altar=Ground(0,-.7f);
                Profile(altar,8,1.75f,1.16f,.23f,new[] {(-.12f,1.05f,0f),(.14f,1.05f,0f),
                    (.51f,.72f,.02f),(.64f,.95f,.02f),(.80f,.86f,.02f)},0,1);
                Profile(altar+Vector3.up*.795f,8,1.74f,1.16f,.23f,new[] {(0f,.65f,0f),(.035f,.59f,0f)},2,2);
                Ember(altar+Vector3.up*.83f,.46f,1.10f);
                Brazier(-3.65f,-3.4f,.78f); Brazier(3.65f,-3.4f,.78f);
                Brazier(-5.25f,1.6f,.67f); Brazier(5.25f,2.4f,.67f);
                Banner(-4.45f,-5.25f,3.8f); Banner(3.55f,-5.50f,3.25f);
                foreach (var (x,z,h) in new[] {(-6.1f,2.6f,4.5f),(-5.55f,3.85f,3.30f),(-6.55f,3.9f,2.62f),
                    (5.85f,3.25f,3.60f),(6.4f,4.25f,2.38f),(5.95f,-.5f,1.35f),(-5.75f,-1.4f,1.5f)})
                {
                    var p=Ground(x,z);
                    Profile(p,6,.81f,.73f,Range(-.7f,.7f),
                        new[] {(-.35f,1.14f,0f),(.18f,1.09f,0f),(h*.55f,.84f,-.19f),
                            (h*.88f,.76f,-.31f),(h,.42f,-.42f)},0,1,.22f);
                }
            }

            void DesignVariant()
            {
                Banner(-4.45f,-5.25f,3.75f);
                Banner(3.55f,-5.50f,3.25f);
                var middle=Ground(0,-.85f);
                Profile(middle,8,1.62f,1.14f,.19f,new[] {(-.14f,1.02f,0f),(.08f,1f,0f),
                    (.51f,.79f,0f),(.64f,.95f,0f),(.71f,.82f,0f)},0,1);
                Profile(middle+Vector3.up*.7f,6,1.1f,.75f,.25f,new[] {(0f,1f,0f),(.075f,.8f,0f)},2,2);
                switch (element)
                {
                    case "Earth":
                        foreach(var (x,z,h,w) in new[] {(-5.8f,2.1f,3.7f,1.3f),(-6.75f,3.8f,2.1f,.9f),
                            (5.6f,3.4f,2.9f,1.15f),(6.6f,1.8f,1.45f,.78f),(-3.75f,-2.7f,1.25f,.65f),(3.7f,-2.5f,1.1f,.63f)})
                        {
                            var p=Ground(x,z);
                            Profile(p,7,w,w*.79f,Range(0,1),new[] {(-.22f,1f,0f),(.16f,1.05f,0f),
                                (h*.4f,.94f,-.19f),(h*.44f,1f,-.24f),(h*.77f,.71f,-.29f),
                                (h,.43f,-.43f)},0,1,.24f);
                            Profile(p+Vector3.up*(h*.42f),7,w,w*.79f,0,new[] {(0f,.99f,-.21f),(.075f,.93f,-.22f)},5,2);
                        }
                        foreach(var (x,z) in new[] {(-.65f,-.7f),(.5f,-1.1f),(.13f,-.65f)})
                            Crystal(middle+new Vector3(x,.73f,z+.85f),.31f,Range(.45f,.89f),.2f);
                        break;
                    case "Lightning":
                        foreach(var (x,z,h,lean) in new[] {(-5.2f,2.4f,4.9f,.61f),(5.1f,3.3f,4.4f,-.62f),
                            (-4.2f,-2.6f,2.2f,.2f),(4.2f,-2.4f,2f,-.24f)})
                        {
                            var p=Ground(x,z);
                            Profile(p,6,.52f,.54f,.4f,new[] {(-.18f,1.6f,0f),(.22f,1.4f,0f),
                                (.5f,.79f,0f),(h-.4f,.62f,lean),(h,.28f,lean)},8,0);
                            for(int j=0;j<3;j++)
                                Profile(p+new Vector3(lean*(.8f+j*.09f),h-.98f+j*.28f,0),6,.46f,.45f,.4f,
                                    new[] {(0f,1f,0f),(.105f,.96f,0f)},2,2);
                            Crystal(p+new Vector3(lean,h-.08f,0),.29f,.92f,-lean*.65f);
                        }
                        Crystal(middle+Vector3.up*.71f,.37f,1.25f,.24f);
                        break;
                    case "Ice":
                        foreach(var (x,z,h,w,lean) in new[] {(-5.35f,2.5f,4.5f,.9f,-.64f),
                            (-6.2f,3.2f,2.9f,.6f,-.85f),(-4.6f,3.7f,2.3f,.63f,.55f),
                            (5.6f,2.9f,3.8f,.76f,.62f),(6.3f,3.8f,2.47f,.59f,.9f),
                            (4.85f,4.1f,1.8f,.42f,-.4f),(-3.7f,-2.6f,1.43f,.43f,-.23f),(3.6f,-2.6f,1.15f,.4f,.21f)})
                            Crystal(Ground(x,z),w,h,lean);
                        Crystal(middle+Vector3.up*.71f,.55f,1.40f,-.2f);
                        Crystal(middle+new Vector3(.59f,.71f,-.02f),.27f,.82f,.25f);
                        break;
                }
            }

            void Crystal(Vector3 p,float width,float h,float lean)
            {
                Profile(p,5,width,width*.76f,Range(-.65f,.65f),new[] {(-.08f,.93f,0f),
                    (h*.20f,1f,0f),(h*.66f,.84f,lean*.64f),(h,.025f,lean)},6,7,.17f);
            }

            Color VariantColor(int i,Color fallback) => (element,i) switch
            {
                ("Earth",0) => new Color(.32f,.245f,.145f), ("Earth",1) => new Color(.50f,.37f,.19f),
                ("Earth",3) => new Color(.17f,.125f,.07f), ("Earth",4) => new Color(.36f,.24f,.105f),
                ("Earth",5) => new Color(.72f,.55f,.25f), ("Earth",6) => new Color(.24f,.51f,.25f),
                ("Earth",7) => new Color(.55f,.69f,.30f),
                ("Lightning",0) => new Color(.18f,.19f,.27f), ("Lightning",1) => new Color(.29f,.27f,.38f),
                ("Lightning",3) => new Color(.10f,.11f,.19f), ("Lightning",4) => new Color(.25f,.15f,.41f),
                ("Lightning",5) => new Color(.73f,.66f,.95f), ("Lightning",6) => new Color(.31f,.36f,.95f),
                ("Lightning",7) => new Color(.66f,.71f,1f),
                ("Ice",0) => new Color(.24f,.34f,.42f), ("Ice",1) => new Color(.47f,.67f,.73f),
                ("Ice",2) => new Color(.48f,.70f,.81f), ("Ice",3) => new Color(.14f,.24f,.31f),
                ("Ice",4) => new Color(.19f,.33f,.46f), ("Ice",5) => new Color(.78f,.94f,1f),
                ("Ice",6) => new Color(.24f,.65f,.83f), ("Ice",7) => new Color(.64f,.92f,.97f),
                _ => fallback
            };

            public void Install(Transform root)
            {
                for (int i=0;i<Palette.Length;i++)
                {
                    if (surfaces[i].Empty) continue;
                    var (name,color,emission) = Palette[i];
                    color=VariantColor(i,color);
                    if(element!="Fire" && emission>0)
                        emission=element=="Earth"?.04f:element=="Ice"?.16f:i==6?.38f:.7f;
                    if(element!="Fire")
                        name=new[] {"Stone","Broken edge","Trim","Ring pavers","Trail cloth","Trail emblem","Element core","Element facet","Old timber"}[i];
                    var folder=ArtFolder(element);
                    var mesh=surfaces[i].Bake(name);
                    AssetDatabase.CreateAsset(mesh, folder+"/"+name+".asset");
                    var material=new Material(Shader.Find("Universal Render Pipeline/Lit")) { name=name };
                    material.SetColor("_BaseColor",color);
                    material.SetFloat("_Smoothness",i==2?.44f:.12f);
                    if (i==2) material.SetFloat("_Metallic",.38f);
                    if (emission>0)
                    {
                        material.EnableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", color*emission);
                        material.globalIlluminationFlags=MaterialGlobalIlluminationFlags.None;
                    }
                    AssetDatabase.CreateAsset(material, folder+"/"+name+".mat");
                    var section=Child(root,name);
                    section.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                    section.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
                }
                if(element!="Fire") return;
                foreach (var (x,z) in new[] {(-3.65f,-3.4f),(3.65f,-3.4f),(0f,-.7f)})
                {
                    var lightRoot = Child(root,"Contained ember light");
                    lightRoot.localPosition = Ground(x,z) + Vector3.up * 1.35f;
                    var light = lightRoot.gameObject.AddComponent<Light>();
                    light.type=LightType.Point;
                    light.color=new Color(1f,.43f,.15f);
                    light.intensity=1.15f;
                    light.range=4f;
                    light.shadows=LightShadows.None;
                }
            }
        }
    }
}
