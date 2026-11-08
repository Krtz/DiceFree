using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Core;
using DiceFree.EditorTools;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Scene-native Arborium art. The site and its keeper are ready for a later advancement quest.
public static partial class ArboriumCemeteryAuthoring
{
    public const string RootName = "Lanternrest Cemetery - Arborium";
    public const string KeeperId = "npc.arborium.cemetery-keeper";
    private const string Folder = "Assets/_DiceFree/Art/World/ArboriumCemetery";
    private static readonly Vector3 City = new(98.04f, 0, 304.74f);
    private static readonly Quaternion Orientation = Quaternion.Euler(0, 225, 0);
    private static Mesh box, bevel, sphere, cylinder, tablet, petal;
    private static Material mortar, stone, pale, weathered, slate, iron, bronze, amber, soil, turf,
        moss, needles, bloom, linen, cloth, skin, wood;

    public static void Batch() => Debug.Log(Author());

    public static object Author()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Author from saved Edit Mode.");

        var scene = SceneManager.GetActiveScene();
        if (scene.path != CornbergSceneBuilder.ScenePath)
            scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
        if (GameObject.Find(RootName) != null)
            throw new InvalidOperationException("Lanternrest is already installed.");

        var center = City + Orientation * (Vector3.forward * 54.5f);
        Physics.SyncTransforms();
        if (Physics.CheckBox(center + Vector3.up * 1.7f, P(11.8f,1.6f,9), Orientation, 1<<9, QueryTriggerInteraction.Ignore))
            throw new InvalidOperationException("The cemetery plot intersects an existing city solid.");
        for (int x = -10; x <= 10; x += 5)
        for (int z = -10; z <= 8; z += 3)
        {
            var point = center + Orientation * P(x,0,z);
            if (!Physics.Raycast(point + Vector3.up * 2, Vector3.down, out var hit, 3, 1<<8, QueryTriggerInteraction.Ignore) ||
                Mathf.Abs(hit.point.y) > .18f)
                throw new InvalidOperationException("The selected plot needs continuous level city ground.");
        }

        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        Palette();
        var root = new GameObject(RootName).transform;
        root.SetPositionAndRotation(center, Orientation);
        Layout(root);
        Entrance(root);
        Graves(root);
        Memorial(root);
        Planting(root);
        Keeper(root);
        CornbergSceneBuilder.Rebake();
        AssetDatabase.SaveAssets();
        Validate(root);
        return new { scene = scene.path, center, graves = 12, keeper = KeeperId, linkedToCity = true };
    }

    private static void Validate(Transform root)
    {
        var nav = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            Vector3 At(float x, float z)
            {
                if (!NavMesh.SamplePosition(root.TransformPoint(P(x,0,z)), out var hit, .65f, 1))
                    throw new InvalidOperationException("Cemetery checkpoint is off the ground navigation.");
                return hit.position;
            }

            void Connect(Vector3 a, Vector3 b)
            {
                var path = new NavMeshPath();
                if (!NavMesh.CalculatePath(a,b,1,path) || path.status != NavMeshPathStatus.PathComplete)
                    throw new InvalidOperationException($"Cemetery route unavailable from {a} to {b}.");
            }

            var street = At(0,-13);
            var gate = At(0,-8.5f);
            var court = At(0,2.5f);
            Connect(street, gate);
            Connect(gate, court);
            Connect(court, At(-3.35f,1.6f));
            Connect(court, At(-3.4f,-2));
            Connect(court, At(3.4f,-2));
            if (!NavMesh.Raycast(At(8,-6.4f),At(8,-10.3f),out _,1))
                throw new InvalidOperationException("The cemetery wall does not block movement.");
            var keeper = root.GetComponentInChildren<ConversationTarget>();
            if (keeper.NpcId != KeeperId || keeper.GetComponent<Collider>() == null)
                throw new InvalidOperationException("The keeper needs a registered dialogue and click target.");
        }
        finally { nav.Remove(); }
    }

    private static Vector3 P(float x, float y, float z) => new(x,y,z);

    private static Transform Node(Transform parent, string name, Vector3 position = default, float yaw = 0)
    {
        var result = new GameObject(name).transform;
        result.SetParent(parent,false);
        result.localPosition = position;
        result.localRotation = Quaternion.Euler(0,yaw,0);
        return result;
    }

    private static void Solid(Transform parent, string name, Vector3 center, Vector3 size, float yaw = 0)
    {
        var node = Node(parent,name,center,yaw);
        node.gameObject.layer = 9;
        node.gameObject.AddComponent<BoxCollider>().size = size;
    }

    private static Material Paint(string name, string hex, float metal = 0, bool glow = false)
    {
        string path = Folder + "/" + name + ".mat";
        var result = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (result == null)
        {
            result = new Material(Shader.Find(glow ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(result,path);
        }
        ColorUtility.TryParseHtmlString("#" + hex,out var color);
        result.SetColor("_BaseColor",color);
        if (!glow)
        {
            result.SetFloat("_Metallic",metal);
            result.SetFloat("_Smoothness",metal > 0 ? .34f : .16f);
        }
        EditorUtility.SetDirty(result);
        return result;
    }

    private static Mesh Primitive(PrimitiveType kind)
    {
        var temporary = GameObject.CreatePrimitive(kind);
        var mesh = temporary.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(temporary);
        return mesh;
    }

    private static void Palette()
    {
        mortar = Paint("Dark mortar","494647");
        stone = Paint("Weathered blue limestone","777780");
        pale = Paint("Ivory memorial stone","B0ABA3");
        weathered = Paint("Warm headstone","91827C");
        slate = Paint("Lichen slate","626B70");
        iron = Paint("Blackened iron","383740",.37f);
        bronze = Paint("Patinated bronze","A08662",.42f);
        amber = Paint("Votive flame","FFD29B",glow:true);
        soil = Paint("Grave soil","574C43");
        turf = Paint("Shaded cemetery lawn","617451");
        moss = Paint("Muted leaf green","73805B");
        needles = Paint("Deep yew","344D41");
        bloom = Paint("Lavender flowers","B59BB9");
        linen = Paint("Candle and ivory linen","DDD2B6");
        cloth = Paint("Keeper's plum wool","574459");
        skin = Paint("Keeper's warm skin","AD8065");
        wood = Paint("Dark oiled oak","523C35");
        box = Primitive(PrimitiveType.Cube);
        cylinder = Primitive(PrimitiveType.Cylinder);
        sphere = Primitive(PrimitiveType.Sphere);
        bevel = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_DiceFree/Settings/Trimery/Roadwarden/Roadwarden cut stone.asset");
        if (bevel == null) throw new InvalidOperationException("Shared bevelled stone mesh is missing.");
        tablet = Profile("Rounded headstone profile",new[]
        {
            new Vector2(-.5f,0),new Vector2(.5f,0),new Vector2(.5f,.67f),new Vector2(.39f,.87f),
            new Vector2(.19f,.97f),new Vector2(0,1),new Vector2(-.19f,.97f),new Vector2(-.39f,.87f),new Vector2(-.5f,.67f)
        });
        petal = Profile("Forged spear profile",new[]
        {
            new Vector2(-.29f,0),new Vector2(.29f,0),new Vector2(.44f,.40f),new Vector2(0,1),new Vector2(-.44f,.40f)
        });
    }

    private static Mesh Profile(string name, Vector2[] outline)
    {
        var mesh = MeshAsset(name);
        var vertices = new List<Vector3>();
        var indices = new List<int>();
        void Face(params Vector3[] points)
        {
            int first = vertices.Count;
            vertices.AddRange(points);
            for (int i = 1; i < points.Length-1; i++) indices.AddRange(new[]{first,first+i,first+i+1});
        }
        Vector3 At(int i, float z) => P(outline[i].x,outline[i].y,z);
        Face(outline.Select(p=>P(p.x,p.y,.5f)).ToArray());
        Face(outline.Reverse().Select(p=>P(p.x,p.y,-.5f)).ToArray());
        for (int i = 0; i < outline.Length; i++)
        {
            int j = (i+1)%outline.Length;
            Face(At(i,.5f),At(i,-.5f),At(j,-.5f),At(j,.5f));
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices,0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        EditorUtility.SetDirty(mesh);
        return mesh;
    }

    private static Mesh MeshAsset(string name)
    {
        string path = Folder + "/" + name + ".asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh != null) return mesh;
        mesh = new Mesh { name = name };
        AssetDatabase.CreateAsset(mesh,path);
        return mesh;
    }

    private sealed class Section
    {
        private readonly Dictionary<Material,List<CombineInstance>> parts = new();
        public void Shape(Mesh mesh, Material mat, Vector3 point, Vector3 size, Vector3 euler = default)
        {
            if (!parts.TryGetValue(mat,out var items)) parts.Add(mat,items=new List<CombineInstance>());
            items.Add(new CombineInstance { mesh = mesh, transform = Matrix4x4.TRS(point,Quaternion.Euler(euler),size) });
        }
        public void Block(Material mat, Vector3 point, Vector3 size, Vector3 euler = default) => Shape(box,mat,point,size,euler);
        public void Stone(Material mat, Vector3 point, Vector3 size, Vector3 euler = default) => Shape(bevel,mat,point,size,euler);
        public void Oval(Material mat, Vector3 point, Vector3 size, Vector3 euler = default) => Shape(sphere,mat,point,size,euler);
        public void Rod(Material mat, Vector3 a, Vector3 b, float diameter)
        {
            var direction = b-a;
            var rotation = Quaternion.FromToRotation(Vector3.up,direction.normalized);
            Shape(cylinder,mat,(a+b)*.5f,P(diameter,direction.magnitude*.5f,diameter),rotation.eulerAngles);
        }

        public void Save(Transform root, string title, bool fade = false)
        {
            var group = Node(root,title);
            foreach (var pair in parts)
            {
                var mesh = MeshAsset(title + " - " + pair.Key.name);
                mesh.Clear();
                mesh.indexFormat = IndexFormat.UInt32;
                mesh.CombineMeshes(pair.Value.ToArray());
                EditorUtility.SetDirty(mesh);
                var node = Node(group,pair.Key.name);
                node.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                node.gameObject.AddComponent<MeshRenderer>().sharedMaterial = pair.Key;
                if (fade) node.gameObject.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(20);
            }
        }
    }
}

public static partial class ArboriumCemeteryAuthoring
{
    private static void Layout(Transform root)
    {
        var s = new Section();
        s.Block(turf,P(0,.092f,.1f),P(21.7f,.045f,16.7f));
        s.Block(mortar,P(0,.14f,-3.85f),P(4.5f,.055f,18.4f));
        s.Block(mortar,P(0,.14f,3.8f),P(7.35f,.055f,5.1f));
        var rng = new System.Random(84);
        for (int row = 0; row < 25; row++)
        {
            float z = -12.75f + row*.72f;
            bool court = z > 1.32f;
            int half = court ? 4 : 2;
            float offset = row%2 == 0 ? .025f : -.065f;
            for (int column = -half; column <= half; column++)
                s.Stone(rng.NextDouble() < .23 ? weathered : rng.NextDouble() < .50 ? stone : pale,
                    P(column*.81f+offset,.155f,z),P(.91f,.105f,.77f),P(0,rng.Next(-4,5),0));
        }
        for (int side = -1; side <= 1; side += 2)
        {
            for (int row = 0; row < 22; row++)
                s.Stone(stone,P(side*2.15f,.16f,-12.8f+row*.68f),P(.29f,.15f,.73f),P(0,row%3*3,0));
            // Low benches flank the court and leave the gravel aisle open.
            float x = side * 3.1f;
            s.Stone(weathered,P(x,.52f,3.9f),P(.58f,.26f,1.7f));
            foreach (float z in new[]{3.38f,4.42f})
                s.Stone(stone,P(x,.24f,z),P(.44f,.51f,.36f));
        }
        s.Save(root,"Stone approach and memorial court");

        const float edge = 10.9f, front = -8.4f, back = 8.5f;
        var fence = new Section();
        void Wall(Vector3 a, Vector3 b)
        {
            var direction = (b-a).normalized;
            float yaw = Quaternion.LookRotation(direction).eulerAngles.y;
            float length = Vector3.Distance(a,b);
            var middle = (a+b)*.5f;
            fence.Block(mortar,middle+P(0,.45f,0),P(.65f,.9f,length),P(0,yaw,0));
            fence.Rod(iron,a+P(0,1.15f,0),b+P(0,1.15f,0),.09f);
            fence.Rod(iron,a+P(0,1.74f,0),b+P(0,1.74f,0),.075f);
            int blocks = Mathf.CeilToInt(length/1.15f);
            for (int i=0;i<blocks;i++)
            {
                Vector3 center = Vector3.Lerp(a,b,(i+.5f)/blocks);
                fence.Stone(i%4==0 ? weathered : stone,center+P(0,.42f,0),P(.84f,.87f,length/blocks+.09f),P(0,yaw,0));
                fence.Stone(pale,center+P(0,.90f,0),P(.94f,.22f,length/blocks+.085f),P(0,yaw,0));
            }
            int bars = Mathf.CeilToInt(length/.57f);
            for (int i=0;i<bars;i++)
            {
                Vector3 center = Vector3.Lerp(a,b,(i+.5f)/bars);
                fence.Rod(iron,center+P(0,.94f,0),center+P(0,1.89f,0),.079f);
                fence.Shape(petal,iron,center+P(0,1.83f,0),P(.17f,.30f,.15f),P(0,yaw+90,0));
            }
            Solid(root,"Solid cemetery boundary",middle+P(0,.93f,0),P(.68f,1.89f,length),yaw);
        }
        Wall(P(-edge,0,front),P(-2.63f,0,front));
        Wall(P(2.63f,0,front),P(edge,0,front));
        Wall(P(-edge,0,front),P(-edge,0,back));
        Wall(P(edge,0,front),P(edge,0,back));
        Wall(P(-edge,0,back),P(edge,0,back));
        fence.Save(root,"Limestone and wrought iron enclosure");
    }

    private static void Arch(Section s, float z, float spring, float radius, float band, float depth, int count)
    {
        for (int i=0;i<count;i++)
        {
            float angle = Mathf.PI*(i+.5f)/count;
            float x = Mathf.Cos(angle)*radius;
            float y = spring+Mathf.Sin(angle)*radius;
            float turn = angle*Mathf.Rad2Deg;
            s.Stone(i==count/2 ? weathered : pale,P(x,y,z),P(band,Mathf.PI*radius/count*1.24f,depth),P(0,0,turn));
        }
    }

    private static void Lamp(Section s, Vector3 p, float size)
    {
        s.Stone(iron,p,P(size*.74f,size*.25f,size*.74f));
        s.Block(amber,p+P(0,size*.41f,0),P(size*.50f,size*.67f,size*.50f));
        foreach (int x in new[]{-1,1})
        foreach (int z in new[]{-1,1})
            s.Rod(iron,p+P(x*size*.29f,.06f,z*size*.29f),p+P(x*size*.29f,size*.77f,z*size*.29f),size*.09f);
        s.Stone(bronze,p+P(0,size*.81f,0),P(size*.88f,size*.40f,size*.88f));
        s.Oval(bronze,p+P(0,size*1.02f,0),Vector3.one*size*.19f);
    }

    private static void Entrance(Transform root)
    {
        const float z = -8.42f;
        var s = new Section();
        for (int sign=-1;sign<=1;sign+=2)
        {
            float x = sign*2.53f;
            s.Stone(stone,P(x,.28f,z),P(1.35f,.62f,1.48f));
            for (int course=0;course<5;course++)
                s.Stone(course%3==0 ? pale : stone,P(x,.73f+course*.52f,z),P(.97f,.57f,1.04f),P(0,course%2*4,0));
            s.Stone(pale,P(x,3.17f,z),P(1.21f,.35f,1.36f));
            Lamp(s,P(x,2.11f,z-.72f),.55f);
            s.Rod(bronze,P(x,2.02f,z-.83f),P(x,1.84f,z-.39f),.10f);
            Solid(root,"Gate pier",P(x,1.52f,z),P(.98f,3.10f,1.03f));

            // The two iron leaves are pinned open toward the gardens.
            var hinge = P(sign*2.09f,.13f,z+.22f);
            var tip = P(sign*2.31f,.13f,z+1.98f);
            s.Rod(iron,hinge+P(0,.41f,0),tip+P(0,.41f,0),.098f);
            s.Rod(iron,hinge+P(0,1.77f,0),tip+P(0,1.62f,0),.095f);
            for(int i=0;i<5;i++)
            {
                Vector3 p=Vector3.Lerp(hinge,tip,i/4f);
                float height=1.84f-.11f*i/4f;
                s.Rod(iron,p+P(0,.24f,0),p+P(0,height,0),.09f);
                s.Shape(petal,bronze,p+P(0,height-.02f,0),P(.18f,.27f,.15f),P(0,90,0));
            }
            Solid(root,"Open iron gate leaf",(hinge+tip)*.5f+P(0,.94f,0),P(.16f,1.84f,1.76f),sign*7);
        }
        Arch(s,z,3.12f,2.47f,.49f,.91f,13);
        s.Stone(iron,P(0,5.08f,z-.60f),P(3.14f,.50f,.22f));
        s.Stone(bronze,P(0,5.81f,z),P(.51f,.48f,.49f),P(0,0,45));
        s.Save(root,"Lanternrest open gate");

        var plate = Node(root,"Cemetery name",P(0,5.08f,z-.729f));
        Letter(plate,"LANTERNREST",.039f);
        var way = Node(root,"Ring road waymarker",P(-2.82f,0,-11.8f),-9);
        var woodwork = new Section();
        woodwork.Stone(stone,P(0,.21f,0),P(.64f,.43f,.56f));
        woodwork.Block(wood,P(0,.92f,0),P(.16f,1.80f,.17f));
        woodwork.Stone(wood,P(0,1.56f,0),P(1.94f,.53f,.21f));
        woodwork.Stone(bronze,P(-.78f,1.57f,-.12f),P(.18f,.25f,.07f),P(0,0,-45));
        woodwork.Save(way,"Cemetery waypost");
        Letter(Node(way,"Waypost lettering",P(.06f,1.57f,-.13f)),"CEMETERY",.028f);
    }

    private static void Letter(Transform node, string value, float size)
    {
        var label = node.gameObject.AddComponent<TextMesh>();
        label.text=value;
        label.fontSize=48;
        label.characterSize=size;
        label.anchor=TextAnchor.MiddleCenter;
        label.alignment=TextAlignment.Center;
        label.color=new Color(.93f,.83f,.67f);
        label.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
    }

    private static void Graves(Transform root)
    {
        var s = new Section();
        var random = new System.Random(614);
        for (int side=-1;side<=1;side+=2)
        for (int row=0;row<3;row++)
        for (int column=0;column<2;column++)
        {
            float x = side*(5.15f+column*3.22f)+random.Next(-13,14)*.01f;
            float z = -4.65f+row*4.46f+random.Next(-21,22)*.01f;
            float yaw = side*random.Next(-7,8);
            var at = P(x,0,z);
            var rot = Quaternion.Euler(0,yaw,0);
            Vector3 T(float a,float b,float c) => at+rot*P(a,b,c);
            var stoneColor = ((row+column+(side>0?1:0))%3) switch {0=>pale,1=>weathered,_=>slate};
            s.Stone(soil,T(0,.20f,0),P(1.76f,.35f,2.73f),P(0,yaw,0));
            for (int i=0;i<4;i++)
            {
                float d=-.92f+i*.61f;
                s.Stone(stone,T(-.76f,.26f,d),P(.25f,.28f,.66f),P(0,yaw+i,0));
                s.Stone(stone,T(.76f,.26f,d),P(.25f,.28f,.66f),P(0,yaw-i,0));
            }
            s.Stone(stone,T(0,.24f,-1.16f),P(1.44f,.31f,.31f),P(0,yaw,0));
            s.Stone(stone,T(0,.36f,.87f),P(1.29f,.39f,.57f),P(0,yaw,0));
            float height=.93f+random.Next(0,37)*.01f;
            s.Shape(tablet,stoneColor,T(0,.52f,.89f),P(.93f,height,.36f),P(0,yaw,random.Next(-4,5)));
            s.Shape(tablet,bronze,T(0,.79f,.69f),P(.42f,.34f,.028f),P(0,yaw,0));
            for(int n=0;n<2;n++)
                s.Block(linen,T(0,.91f+n*.09f,.67f),P(.26f-n*.047f,.018f,.023f),P(0,yaw,0));
            s.Stone(moss,T(.47f,.50f,.80f),P(.47f,.14f,.31f),P(12,yaw+28,0));
            for(int i=0;i<5;i++)
                Sprig(s,T(-.47f+i*.17f,.35f,-.58f+random.Next(-12,13)*.026f),.70f+random.Next(0,5)*.15f);
            if ((row+column)%2==0)
                Lamp(s,T(-.62f,.32f,-.93f),.24f);
            Solid(root,"Raised grave",T(0,.43f,.05f),P(1.56f,.86f,2.45f),yaw);
        }
        s.Save(root,"Twelve carved graves and offerings");
    }
}


public static partial class ArboriumCemeteryAuthoring
{
    private static void Memorial(Transform root)
    {
        var s = new Section();
        const float z = 7.46f;
        s.Stone(stone,P(0,.23f,6.4f),P(5.72f,.45f,2.49f));
        s.Stone(pale,P(0,.39f,6.39f),P(5.46f,.22f,2.40f));
        s.Block(mortar,P(0,1.84f,z+.02f),P(5.39f,3.21f,.51f));
        for (int row=0;row<7;row++)
        for (int column=-2;column<=2;column++)
        {
            float x=column*1.02f+(row%2==0?.08f:-.08f);
            s.Stone(row%3==0 ? weathered : stone,P(x,.52f+row*.46f,z),P(1.15f,.49f,.55f),P(0,row%2*3,0));
        }
        for(int side=-1;side<=1;side+=2)
        {
            s.Stone(pale,P(side*2.29f,1.99f,z-.22f),P(.53f,3.53f,.67f));
            s.Stone(weathered,P(side*2.29f,3.67f,z-.22f),P(.69f,.33f,.77f));
            s.Stone(weathered,P(side*2.29f,.54f,z-.22f),P(.68f,.39f,.76f));
            Lamp(s,P(side*1.99f,.67f,5.83f),.57f);
        }
        Arch(s,z-.29f,3.40f,2.28f,.45f,.58f,13);
        s.Shape(tablet,slate,P(0,1.01f,z-.42f),P(2.04f,3.70f,.18f));
        s.Shape(tablet,mortar,P(0,1.19f,z-.54f),P(1.76f,3.32f,.08f));
        // The World Tree emblem is wrought in bronze in the open-air memorial.
        var basePoint=P(0,2.12f,z-.62f);
        var trunk=basePoint+P(0,.72f,0);
        s.Rod(bronze,basePoint,trunk,.105f);
        for(int side=-1;side<=1;side+=2)
        {
            s.Rod(bronze,basePoint,basePoint+P(side*.33f,-.20f,0),.065f);
            s.Rod(bronze,basePoint+P(0,.32f,0),basePoint+P(side*.52f,.66f,0),.083f);
            s.Rod(bronze,basePoint+P(0,.57f,0),basePoint+P(side*.30f,.96f,0),.072f);
            s.Shape(petal,bronze,basePoint+P(side*.51f,.66f,0),P(.25f,.44f,.11f),P(0,0,side*-39));
            s.Shape(petal,bronze,basePoint+P(side*.29f,.93f,0),P(.22f,.40f,.11f),P(0,0,side*-23));
        }
        s.Shape(petal,bronze,trunk+P(0,.27f,0),P(.22f,.44f,.10f));
        s.Stone(weathered,P(0,.62f,5.88f),P(2.87f,.45f,1.13f));
        s.Stone(pale,P(0,.91f,5.88f),P(2.94f,.24f,1.17f));
        s.Stone(linen,P(0,1.045f,5.88f),P(1.48f,.048f,.70f));
        for (int i=0;i<11;i++)
        {
            float x=(i%6-2.5f)*.19f;
            float depth=6.75f+(i/6)*.21f;
            float height=.12f+(i%3)*.068f;
            s.Rod(linen,P(x,1.015f,depth),P(x,1.015f+height,depth),.10f);
            s.Oval(amber,P(x,1.047f+height,depth),P(.067f,.112f,.064f));
        }
        s.Save(root,"Open air vigil memorial");
        Solid(root,"Memorial stone and burial bier",P(0,.45f,6.41f),P(5.25f,.89f,2.40f));
        Solid(root,"Memorial rear masonry",P(0,1.92f,z),P(5.39f,3.65f,.58f));
    }

    private static void Sprig(Section s, Vector3 p, float size)
    {
        var tip=p+P(.08f,.33f,-.04f)*size;
        s.Rod(moss,p,tip,.033f*size);
        s.Oval(moss,p+P(-.05f,.14f,0)*size,P(.20f,.05f,.075f)*size,P(0,0,22));
        s.Oval(moss,p+P(.12f,.19f,-.02f)*size,P(.18f,.052f,.078f)*size,P(0,0,-29));
        for(int i=0;i<3;i++)
            s.Oval(i==1 ? linen : bloom,tip+P((i-1)*.043f,i*.047f,0)*size,P(.102f,.087f,.085f)*size);
    }

    private static void Planting(Transform root)
    {
        var random=new System.Random(4552);
        var garden=new Section();
        for (int side=-1;side<=1;side+=2)
        {
            for (int i=0;i<24;i++)
            {
                float x=side*(9.85f+random.Next(-30,33)*.01f);
                float z=-7.4f+i*.63f+random.Next(-23,24)*.01f;
                garden.Stone(moss,P(x,.18f,z),P(.41f,.21f,.54f),P(0,i*39,0));
                if(i%3!=1) Sprig(garden,P(x,.17f,z),.8f+(float)random.NextDouble()*.71f);
            }
            for(int i=0;i<8;i++)
                Sprig(garden,P(side*(3.9f+i*.64f),.19f,7.0f+random.Next(-30,31)*.017f),.83f+random.Next(0,5)*.14f);

            var tree=Node(root,"Low branched cemetery yew",P(side*8.65f,0,6.49f),side*22);
            var trunk=new Section();
            trunk.Rod(wood,P(0,.03f,0),P(.22f,3.81f,.04f),.47f);
            trunk.Stone(weathered,P(-.18f,.15f,-.12f),P(1.14f,.34f,1.02f));
            trunk.Rod(wood,P(.14f,1.53f,0),P(-.85f,2.58f,.18f),.23f);
            trunk.Rod(wood,P(.17f,2.15f,0),P(1.02f,3.27f,-.36f),.21f);
            trunk.Save(tree,side<0 ? "West yew bark" : "East yew bark",true);
            Solid(tree,"Yew trunk",P(.03f,.81f,0),P(.55f,1.62f,.57f));
            var leaves=new Section();
            for(int i=0;i<15;i++)
            {
                float a=i*Mathf.PI*2/7;
                float y=2.34f+i*.155f;
                float spread=1.10f-i*.046f;
                var cluster=P(Mathf.Cos(a)*spread+.11f,y,Mathf.Sin(a)*spread*.72f);
                leaves.Stone(i%3==0?moss:needles,cluster,P(1.37f-i*.031f,.87f,1.20f-i*.021f),P(8,i*23,11));
            }
            leaves.Save(tree,side<0 ? "West yew crown" : "East yew crown",true);
        }
        garden.Save(root,"Yew borders and remembrance flowers");
    }

    private static void Keeper(Transform root)
    {
        var node=Node(root,"Mara - Lanternrest cemetery keeper",P(-3.36f,.08f,2.66f),25);
        var s=new Section();
        s.Shape(tablet,cloth,P(0,.23f,0),P(.62f,1.25f,.43f),P(-3,0,0));
        s.Oval(cloth,P(0,1.04f,.025f),P(.55f,.81f,.41f));
        s.Shape(tablet,linen,P(0,.40f,-.218f),P(.37f,.65f,.09f));
        s.Rod(wood,P(-.22f,.76f,-.205f),P(.22f,.76f,-.205f),.084f);
        s.Stone(bronze,P(0,.76f,-.27f),P(.14f,.14f,.05f));
        s.Oval(cloth,P(0,1.56f,.075f),P(.48f,.60f,.46f));
        s.Oval(skin,P(0,1.54f,-.116f),P(.30f,.37f,.26f));
        s.Oval(skin,P(0,1.51f,-.25f),P(.083f,.13f,.063f));
        s.Oval(linen,P(0,1.35f,-.132f),P(.29f,.13f,.25f));
        for (int side=-1;side<=1;side+=2)
        {
            s.Rod(cloth,P(side*.228f,1.26f,0),P(side*.36f,.83f,-.15f),.20f);
            s.Oval(skin,P(side*.355f,.80f,-.17f),P(.17f,.19f,.18f));
            s.Stone(wood,P(side*.153f,.12f,-.075f),P(.24f,.24f,.39f));
            s.Oval(iron,P(side*.061f,1.579f,-.235f),P(.040f,.026f,.031f));
        }
        s.Rod(wood,P(.41f,.06f,-.26f),P(.37f,1.48f,-.13f),.085f);
        s.Stone(bronze,P(.37f,1.49f,-.13f),P(.18f,.19f,.16f));
        s.Save(node,"Mara the grave keeper");

        node.gameObject.layer=11;
        var body=node.gameObject.AddComponent<CapsuleCollider>();
        body.height=1.86f;
        body.radius=.36f;
        body.center=P(0,.93f,0);
        var conversation=node.gameObject.AddComponent<ConversationTarget>();
        conversation.ConfigureName("Mara, Cemetery Keeper");
        conversation.ConfigureApproach(Node(root,"Keeper approach",P(-3.35f,0,1.60f)));
        conversation.Configure(KeeperId,"conversation.arborium.lanternrest",
            "Welcome to Lanternrest. We keep a lamp for those whose families live too far to tend a grave. When someone from Cornberg or Trimery is brought here, we light the first candle at the stone together.");
    }
}
