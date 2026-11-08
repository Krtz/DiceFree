using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Core;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Original project artwork authored over the existing world collision.
public static class WorldVisualAuthoring
{
    private const string Folder = "Assets/_DiceFree/Art/World/World1Handcrafted";
    private const string WaterShaderPath = "Assets/_DiceFree/Shaders/CornbergWater.shader";
    private const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
    private enum Surface { Oak, Sapwood, DarkOak, Iron, Brass, Slate, PaleStone, WarmStone, Moss,
        Lantern, Water, WaterDeep, Cedar, CedarLight, Clay, SlateBlue, SlateLight, SlateDark,
        MossCedar, MossLight, MossDark, Gravel, Bark, LetterPaint, PineDark, Pine, PineTip, PineGold, PineFrost, Thatch, ThatchTip, ThatchShadow, Brick, BrickDark, Ember }
    private static readonly Dictionary<Surface, Material> Materials = new();
    private static readonly Surface[][] Roofing =
    {
        new[] { Surface.Cedar, Surface.CedarLight, Surface.Clay },
        new[] { Surface.SlateBlue, Surface.SlateLight, Surface.SlateDark },
        new[] { Surface.MossCedar, Surface.MossLight, Surface.MossDark }
    };
    private static Vector3 P(float x, float y, float z) => new(x, y, z);

    public static object Trimery()
    {
        var scene = OpenWorld();
        Palette();
        var town = Find(scene, "13 - Trimery lumber village");
        var art = Rebuild(town, "Trimery handcrafted presentation");
        int index = 0;
        foreach (Transform house in town.Find("Buildings - safe inner village")) DressHouse(art, house, index++);
        DressMill(art, town.Find("Working sawmill and lumber yard"));
        var palisade = town.Find("Believable log palisade with three guarded openings");
        DressGate(art, palisade, "SOUTH", "TRIMERY", 248, 153, 0);
        DressGate(art, palisade, "NORTH", "BRIARBACK", 248, 207, 0);
        DressGate(art, palisade, "WEST", "TRADE ROAD", 220, 180, 90);
        DressPalisade(art, palisade);
        DressRoad(art, town);
        DressRoadSurface(art,town.Find("SOUTH, WEST and NORTH roads"),"Trimery",new Color(.93f,.86f,.74f));
        DressForest(scene, art);
        Save(scene);
        return Report(art);
    }

    private static object Report(Transform art)
    {
        var meshes = art.GetComponentsInChildren<MeshFilter>();
        return new { root=art.name, meshes=meshes.Length,
            vertices=meshes.Sum(m=>m.sharedMesh.vertexCount),
            decorativeColliders=art.GetComponentsInChildren<Collider>().Length };
    }

    private static Scene OpenWorld()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
            throw new InvalidOperationException("Clean Edit Mode is required.");
        var scene = SceneManager.GetActiveScene();
        return scene.path == ScenePath ? scene : EditorSceneManager.OpenScene(ScenePath);
    }

    private static void Save(Scene scene)
    {
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("World scene save failed.");
    }

    private static Transform Find(Scene scene, string name) => scene.GetRootGameObjects()
        .SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).Single(t=>t.name==name);

    private static Transform New(Transform parent, string name, Vector3 position, float yaw = 0)
    {
        var result = new GameObject(name).transform;
        result.gameObject.layer = 2;
        result.SetParent(parent, false);
        result.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
        return result;
    }

    private static Transform Rebuild(Transform parent, string name)
    {
        var old = parent.Find(name);
        if (old != null) Object.DestroyImmediate(old.gameObject);
        return New(parent, name, parent.position);
    }

    private static void Palette()
    {
        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets/_DiceFree/Art/World", "World1Handcrafted");
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) throw new InvalidOperationException("URP Lit shader unavailable.");
        string[] colors = { "5E3B29","C18A53","372A26","3E4A50","B18C51","68716E","86918B",
            "9D8970","567346","F7AD49","5BBEAE","327F85","92563D","A46A4D","744735",
            "4D656D","617A80","425960","626848","7C8051","4D553D","A48D6E","624435","FFE5B2","234535","396746","587E4E","6C7E47","496F68","A0804F","B79761","856841","846D60","5B4A43","D35D22" };
        Materials.Clear();
        foreach (var key in Enum.GetValues(typeof(Surface)).Cast<Surface>())
        {
            string path = Folder + "/" + key + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = "World 1 " + key };
                AssetDatabase.CreateAsset(material, path);
            }
            ColorUtility.TryParseHtmlString("#" + colors[(int)key], out var color);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", key is Surface.Water or Surface.WaterDeep ? .68f : .18f);
            material.SetFloat("_Metallic", key is Surface.Iron or Surface.Brass ? .44f : 0);
            if (key is Surface.Lantern or Surface.Water or Surface.LetterPaint or Surface.Ember)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * (key is Surface.Lantern or Surface.Ember ? 1.45f : .19f));
            }
            EditorUtility.SetDirty(material);
            Materials.Add(key, material);
        }
    }

    private static void DressHouse(Transform parent, Transform house, int index)
    {
        var old = house.Find("Shingled roof");
        var walls = house.Find("Timber structure");
        if (old == null || walls == null) throw new InvalidOperationException("Missing Trimery shell " + house.name);
        var oldRenderer = old.GetComponent<Renderer>();
        float width = old.lossyScale.x + .08f, depth = old.lossyScale.z + .12f;
        float height = width * .235f;
        var anchor = New(parent, house.name + " - carpentry", old.position + Vector3.down * .22f);
        var roof = new Craft();
        ShingleRoof(roof, width, depth, height, Roofing[index % Roofing.Length], index + 73);
        var roofRenderer = roof.Place(anchor, "Trimery house " + index + " - lapped shingles", fade: true);

        var timber = new Craft();
        float floor = walls.position.y - anchor.position.y - walls.lossyScale.y * .5f;
        float xEdge = walls.lossyScale.x * .5f, zEdge = walls.lossyScale.z * .5f;
        for (int front = -1; front <= 1; front += 2)
        {
            float z = (zEdge + .05f) * front;
            timber.Beam(P(-xEdge,floor+.72f,z),P(xEdge,floor+.72f,z),.16f,Surface.DarkOak);
            timber.Beam(P(-xEdge,-.34f,z),P(xEdge,-.34f,z),.21f,Surface.DarkOak);
            foreach (int side in new[] {-1,1})
            {
                float x = side * xEdge * .86f;
                timber.Beam(P(x,floor+.40f,z),P(x,-.22f,z),.23f,Surface.Oak);
                timber.Beam(P(x,-.39f,z),P(x-side*.86f,-1.27f,z),.17f,Surface.Sapwood);
                if (front > 0) continue;
                float wx = xEdge * .54f * side;
                timber.Box(P(wx,-1.61f,z-.07f),P(1.14f,1.18f,.12f),Surface.DarkOak);
                timber.Box(P(wx,-1.60f,z-.15f),P(.91f,.94f,.06f),Surface.Iron);
                timber.Box(P(wx,-1.60f,z-.20f),P(.075f,.97f,.065f),Surface.Sapwood);
                timber.Box(P(wx,-1.60f,z-.20f),P(.95f,.075f,.065f),Surface.Sapwood);
                timber.Box(P(wx,-2.13f,z-.23f),P(1.21f,.12f,.36f),Surface.Oak);
                if (index is 1 or 2 or 3)
                    timber.Box(P(wx-.22f,-1.82f,z-.185f),P(.33f,.30f,.035f),Surface.Lantern);
            }
            if (front < 0)
            {
                timber.Box(P(0,floor+.19f,z-.30f),P(1.78f,.24f,.67f),Surface.WarmStone);
                foreach (int side in new[] {-1,1})
                    timber.Beam(P(side*.83f,floor+.29f,z-.12f),P(side*.83f,floor+2.79f,z-.12f),.15f,Surface.Sapwood);
                timber.Beam(P(-.9f,floor+2.75f,z-.12f),P(.9f,floor+2.75f,z-.12f),.16f,Surface.Sapwood);
                timber.Box(P(.54f,floor+1.35f,z-.18f),P(.14f,.12f,.09f),Surface.Brass);
            }
        }
        for (int side = -1; side <= 1; side += 2)
        {
            float x = (xEdge+.04f)*side;
            timber.Beam(P(x,floor+.72f,-zEdge),P(x,floor+.72f,zEdge),.15f,Surface.DarkOak);
            timber.Beam(P(x,-.34f,-zEdge),P(x,-.34f,zEdge),.20f,Surface.DarkOak);
            for (int bay=0; bay<3; bay++)
            {
                float z = Mathf.Lerp(-zEdge+.1f,zEdge-.1f,bay*.5f);
                timber.Beam(P(x,floor+.56f,z),P(x,-.27f,z),.18f,Surface.Oak);
                if (bay<2) timber.Beam(P(x,floor+.76f,z+.1f),P(x,-.46f,z+zEdge-.15f),.14f,Surface.Sapwood);
            }
        }
        var timberRenderer = timber.Place(anchor,"Trimery house " + index + " - frame");
        oldRenderer.enabled = false;

        // The optional locally installed merchant hall uses the new shell as its portable fallback.
        var slot = house.GetComponentsInChildren<TrimeryArtSlot>(true).SingleOrDefault(s=>s.Resource=="TimberMerchantHall");
        if (slot != null)
        {
            var serialized = new SerializedObject(slot);
            var fallbacks = serialized.FindProperty("fallbackRenderers");
            var previous = Enumerable.Range(0,fallbacks.arraySize)
                .Select(i=>fallbacks.GetArrayElementAtIndex(i).objectReferenceValue as Renderer)
                .Where(r=>r!=null&&r!=oldRenderer).Append(roofRenderer).Append(timberRenderer).Distinct().ToArray();
            fallbacks.arraySize=previous.Length;
            for(int i=0;i<previous.Length;i++)fallbacks.GetArrayElementAtIndex(i).objectReferenceValue=previous[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        else MoveLabel(house,anchor.position+Vector3.up*(height+.56f));
    }

    private static void MoveLabel(Transform source,Vector3 position)
    {
        var label=source.GetComponentInChildren<TextMesh>(true);
        if(label!=null)label.transform.position=position;
    }

    private static void ShingleRoof(Craft mesh,float width,float length,float height,Surface[] colors,int seed,bool coarseThatch=false)
    {
        float half = width * .5f, end = length * .5f;
        var random = new System.Random(seed);
        float Y(float t) => height*(.86f*t+.14f*t*t);
        foreach (int side in new[] {-1,1})
        {
            Vector3 Point(float t,float z,float lift=0) => P(side*half*(1-t),Y(t)+lift,z);
            int rows = Mathf.Clamp(Mathf.RoundToInt(half/(coarseThatch?.71f:.54f)),4,12);
            float step = 1f/rows;
            for (int row = 0; row < rows; row++)
            {
                float first=row*step, last=(row+1)*step;
                mesh.Face(Surface.DarkOak,Vector3.up,Point(first,-end),Point(last,-end),Point(last,end),Point(first,end));
                float boardWidth = length / Mathf.Ceil(length/(coarseThatch?.22f:.62f));
                for (float z=-end-(row%2)*boardWidth*.5f; z<end; z+=boardWidth)
                {
                    float gap=coarseThatch?.006f:.018f; float a=Mathf.Max(-end,z)+gap, b=Mathf.Min(end,z+boardWidth)-gap;
                    if (b-a<.035f) continue;
                    float upper=Mathf.Min(1,last+step*.15f);
                    float lift=.064f+(float)random.NextDouble()*(coarseThatch?.067f:.041f);
                    double tone=random.NextDouble(); var style=colors[tone<.66?0:tone<.81?1:2];
                    Vector3 lowA=Point(first,a,lift),lowB=Point(first,b,lift);
                    Vector3 topA=Point(upper,a,.043f),topB=Point(upper,b,.043f);
                    mesh.Face(style,Vector3.up,lowA,topA,topB,lowB);
                    mesh.Face(colors[2],Vector3.right*side,lowA,lowB,lowB-Vector3.up*.055f,lowA-Vector3.up*.055f);
                    mesh.Face(colors[2],Vector3.back,lowA,topA,topA-Vector3.up*.048f,lowA-Vector3.up*.048f);
                }
            }
            mesh.Beam(Point(0,-end-.10f,-.085f),Point(0,end+.10f,-.085f),.19f,Surface.Oak);
            for (float z=-end+.3f;z<end;z+=.85f)
                mesh.Beam(Point(0,z,-.16f),Point(.17f,z,-.16f),.12f,Surface.Sapwood);
        }
        foreach (int face in new[] {-1,1})
        {
            float z=(end-.52f)*face;
            mesh.Triangle(Surface.Oak,Vector3.forward*face,P(-half,-.12f,z),P(half,-.12f,z),P(0,height,z));
            for (float x=-half+.18f;x<half-.12f;x+=.32f)
            {
                float t=1-Mathf.Max(Mathf.Abs(x),Mathf.Abs(x+.27f))/half;
                float top=Y(Mathf.Clamp01(t))-.04f;
                if(top<.02f)continue;
                mesh.Box(P(x+.13f,(top-.10f)*.5f,z+.016f*face),P(.267f,top+.1f,.038f),
                    random.Next(3)==0?Surface.Sapwood:Surface.Oak);
            }
            foreach (int side in new[] {-1,1})
            {
                float fascia=(end+.025f)*face;
                mesh.Beam(P(side*(half+.06f),-.10f,fascia),P(side*half*.56f,Y(.44f)-.06f,fascia),.17f,Surface.Sapwood);
                mesh.Beam(P(side*half*.56f,Y(.44f)-.06f,fascia),P(0,height-.018f,fascia),.17f,Surface.Sapwood);
            }
            mesh.Beam(P(-half*.70f,.02f,z+.04f*face),P(half*.70f,.02f,z+.04f*face),.14f,Surface.DarkOak);
            mesh.Beam(P(0,.03f,z+.045f*face),P(0,height*.93f,z+.045f*face),.14f,Surface.DarkOak);
            foreach(int side in new[]{-1,1})
                mesh.Beam(P(0,.06f,z+.061f*face),P(side*half*.34f,Y(.65f)-.15f,z+.061f*face),.115f,Surface.Sapwood);
            mesh.Tube(P(0,height-.02f,z),P(0,height+.32f,z),.12f,.055f,Surface.Brass,7);
        }
        mesh.Beam(P(0,height+.061f,-end-.11f),P(0,height+.061f,end+.11f),.17f,Surface.DarkOak);
        for(float z=-end+.25f;z<end;z+=.58f)
            mesh.Box(P(0,height+.11f,z),P(.29f,.12f,.49f),colors[0],Quaternion.Euler(0,0,5));
    }


    private static void DressMill(Transform parent, Transform original)
    {
        var old = original.Find("Sawmill sheltered roof");
        var root = New(parent,"Trimery lumberworks - timber shed",old.position+Vector3.down*.16f);
        var roof = new Craft();
        ShingleRoof(roof,old.lossyScale.x+.30f,old.lossyScale.z+.20f,2.6f,Roofing[0],17);
        roof.Place(root,"Sawmill lapped cedar canopy",fade:true);
        old.GetComponent<Renderer>().enabled=false;
        MoveLabel(original,root.position+Vector3.up*3.1f);

        var frame = new Craft();
        float y = original.Find("Sawmill raised wood platform").position.y - .32f - root.position.y;
        foreach (int x in new[] {-1,1})
        foreach (int z in new[] {-1,1})
        {
            float px=x*5f,pz=z*3.8f;
            frame.Box(P(px,y+.78f,pz),P(.57f,1.56f,.58f),Surface.DarkOak);
            frame.Box(P(px,-.45f,pz),P(.55f,.20f,.57f),Surface.Iron);
            frame.Beam(P(px,-.18f,pz),P(px,y+2.6f,pz),.23f,Surface.Oak);
            frame.Beam(P(px,-1.65f,pz),P(px-x*1.38f,-.23f,pz),.23f,Surface.Sapwood);
            frame.Beam(P(px,-1.65f,pz),P(px,-.23f,pz-z*1.19f),.23f,Surface.Sapwood);
        }
        foreach(int side in new[]{-1,1})
        {
            frame.Beam(P(-5.32f,-.20f,side*3.8f),P(5.32f,-.20f,side*3.8f),.28f,Surface.DarkOak);
            frame.Beam(P(side*5,-.2f,-4.10f),P(side*5,-.2f,4.1f),.24f,Surface.Oak);
        }
        for(int plank=0;plank<16;plank++)
        {
            float x=-5.12f+plank*.68f;
            frame.Box(P(x,y+.645f,0),P(.653f,.058f,8.84f),plank%4==1?Surface.Sapwood:Surface.Oak);
        }
        Lantern(root,frame,P(4.94f,-1.75f,-4.07f),.85f,"Sawmill work light");
        frame.Place(root,"Sawmill pegs braces and deck");

        var wheelAnchor=New(root,"Hand-cranked sawmill flywheel",root.TransformPoint(P(-3.52f,y+1.89f,-4.43f)));
        var stand=new Craft();
        stand.Beam(P(-.82f,y+.06f,-4.25f),P(-2.66f,y+1.90f,-4.25f),.25f,Surface.Oak);
        stand.Beam(P(-4.60f,y+.06f,-4.22f),P(-3.52f,y+1.90f,-4.22f),.25f,Surface.Oak);
        stand.Box(P(-3.50f,y+.16f,-4.24f),P(2.87f,.28f,.48f),Surface.DarkOak);
        stand.Tube(P(-3.52f,y+1.89f,-4.44f),P(-3.52f,y+1.89f,-3.86f),.16f,.16f,Surface.Iron,10);
        stand.Place(root,"Sawmill crank trestle");

        var wheel=new Craft();
        wheel.Ring(Vector3.zero,Vector3.forward,1.13f,.15f,Surface.Iron,24);
        wheel.Ring(Vector3.back*.036f,Vector3.forward,.96f,.083f,Surface.Brass,24);
        wheel.Tube(Vector3.back*.13f,Vector3.forward*.14f,.19f,.19f,Surface.Iron,12);
        for(int i=0;i<8;i++)
        {
            float angle=i*Mathf.PI/4;
            var spoke=P(Mathf.Cos(angle),Mathf.Sin(angle),0);
            wheel.Beam(spoke*.19f,spoke*.99f,.10f,Surface.Sapwood);
        }
        wheel.Beam(P(.13f,0,-.16f),P(.62f,0,-.16f),.087f,Surface.Iron);
        wheel.Tube(P(.62f,0,-.35f),P(.62f,0,-.13f),.068f,.068f,Surface.Oak,10);
        wheel.Place(wheelAnchor,"Sawmill eight-spoke flywheel");
        Spin(wheelAnchor,"Trimery mill slow turn",13f);

        var stock=new Craft();
        var random=new System.Random(9237);
        foreach(var pile in original.Cast<Transform>().Where(t=>t.name.StartsWith("Lumber stock",StringComparison.Ordinal)))
        {
            var logs=pile.Cast<Transform>().Where(t=>t.name=="Cut timber log").ToArray();
            if(logs.Length==0)continue;
            float cx=logs.Average(t=>t.position.x), cz=logs.Average(t=>t.position.z);
            float baseY=logs.Min(t=>t.position.y)-.43f;
            foreach(var r in pile.GetComponentsInChildren<Renderer>())r.enabled=false;
            for(int row=0;row<3;row++)
            for(int i=0;i<3-row;i++)
            {
                float z=cz+(i-(2-row)*.5f)*.52f;
                float ry=baseY+.27f+row*.49f;
                float length=4.75f+(float)random.NextDouble()*.56f;
                stock.Log(P(cx-length*.5f,ry,z)-root.position,P(cx+length*.5f,ry,z+.07f)-root.position,.30f,random);
            }
        }
        stock.Place(root,"Sawmill bark and fresh ring-cut timber");
    }

    private static void Spin(Transform target,string name,float period)
    {
        string path=Folder+"/"+name+".anim";
        var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if(clip==null)
        {
            clip=new AnimationClip { name=name,legacy=true,wrapMode=WrapMode.Loop };
            clip.SetCurve("",typeof(Transform),"localEulerAnglesRaw.z",AnimationCurve.Linear(0,0,period,360));
            AssetDatabase.CreateAsset(clip,path);
        }
        var animation=target.gameObject.AddComponent<Animation>();
        animation.clip=clip;
        animation.AddClip(clip,name);
        animation.wrapMode=WrapMode.Loop;
        animation.playAutomatically=true;
    }

    private static void DressGate(Transform parent,Transform palisade,string prefix,string title,float x,float z,float yaw)
    {
        var original=palisade.Cast<Transform>().Single(t=>t.name.StartsWith(prefix+" -",StringComparison.Ordinal));
        var anchor=New(parent,"Trimery "+prefix.ToLowerInvariant()+" gate - watch carpentry",P(x,Ground(x,z),z),yaw);
        var joinery=new Craft();
        foreach(var tower in original.Cast<Transform>().Where(t=>t.name.StartsWith("Guard post",StringComparison.Ordinal)))
        {
            var old=tower.Find("Canopied tower roof");
            var where=anchor.InverseTransformPoint(old.position)+Vector3.down*.07f;
            var hood=new Craft();
            ShingleRoof(hood,3.8f,3.7f,1.33f,Roofing[2],97+Mathf.RoundToInt(where.x));
            hood.Place(anchor,"Gate "+prefix+" "+tower.name+" - cedar hood",where,true);
            old.GetComponent<Renderer>().enabled=false;
            foreach(int face in new[]{-1,1})
            {
                float zOff=where.z+face*1.30f;
                float y=where.y-1.18f;
                joinery.Beam(P(where.x-1.25f,y,zOff),P(where.x+1.25f,y,zOff),.21f,Surface.Oak);
                joinery.Beam(P(where.x-1.24f,y-.58f,zOff),P(where.x+1.24f,y-.58f,zOff),.19f,Surface.DarkOak);
                for(int bar=-2;bar<=2;bar++)
                    joinery.Beam(P(where.x+bar*.47f,y-.60f,zOff),P(where.x+bar*.47f,y,zOff),.125f,Surface.Sapwood);
            }
            foreach(int side in new[]{-1,1})
            {
                float sideX=where.x+side*1.22f;
                joinery.Beam(P(sideX,where.y-1.73f,where.z-1.33f),P(sideX,where.y-1.73f,where.z+1.33f),.22f,Surface.DarkOak);
                joinery.Beam(P(sideX,where.y-1.10f,where.z-1.33f),P(sideX,where.y-1.10f,where.z+1.33f),.18f,Surface.Oak);
            }
            Lantern(anchor,joinery,P(where.x*.87f,where.y-2.43f,-1.55f),.9f,"Gate "+prefix+" lantern "+tower.name);
        }
        var oldBeam=original.Find("Gate crossbeam");
        float cross=anchor.InverseTransformPoint(oldBeam.position).y;
        oldBeam.GetComponent<Renderer>().enabled=false;
        var overhead=new Craft();
        overhead.Box(P(0,cross,0),P(9.4f,.69f,1.06f),Surface.DarkOak);
        foreach(int side in new[]{-1,1})
        {
            overhead.Beam(P(side*4.50f,cross-1.08f,.44f),P(side*2.6f,cross-.15f,.44f),.20f,Surface.Sapwood);
            overhead.Beam(P(side*4.50f,cross-1.08f,-.44f),P(side*2.6f,cross-.15f,-.44f),.20f,Surface.Sapwood);
            overhead.Box(P(0,cross+.20f,side*.61f),P(4.88f,.81f,.15f),Surface.Oak);
            overhead.Beam(P(-2.48f,cross-.20f,side*.70f),P(2.48f,cross-.20f,side*.70f),.10f,Surface.Brass);
            overhead.Beam(P(-2.48f,cross+.61f,side*.70f),P(2.48f,cross+.61f,side*.70f),.10f,Surface.Brass);
            foreach(float pin in new[]{-2.23f,2.23f})
                overhead.Tube(P(pin,cross+.21f,side*.68f),P(pin,cross+.21f,side*.77f),.085f,.062f,Surface.Brass,8);
        }
        int exterior = prefix=="NORTH"?1:-1;
        float signYaw=exterior<0?0:180;
        Quaternion facing=Quaternion.Euler(0,signYaw,0)*Quaternion.Euler(34,0,0);
        Vector3 signCenter=P(0,cross+.46f,exterior*.68f);
        overhead.Box(signCenter,P(4.90f,.81f,.15f),Surface.Oak,facing);
        Lettering(overhead,title,signCenter+facing*Vector3.back*.095f,signYaw,.070f,34);
        Lettering(overhead,title,P(0,cross+.385f,0),0,.061f,90);
        overhead.Place(anchor,"Gate "+prefix+" - carved lintel",fade:true);
        MoveLabel(original,anchor.position+Vector3.up*7.2f);
        joinery.Place(anchor,"Gate "+prefix+" - rails and lanterns",fade:true);
    }

    private static void Lantern(Transform root,Craft mesh,Vector3 p,float scale,string name)
    {
        mesh.Box(p+P(0,-.43f*scale,0),P(.58f,.10f,.57f)*scale,Surface.Iron);
        mesh.Box(p,P(.41f,.72f,.41f)*scale,Surface.Lantern);
        mesh.Box(p+P(0,.41f*scale,0),P(.65f,.14f,.62f)*scale,Surface.Iron);
        foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})
            mesh.Beam(p+P(x*.23f,-.42f,z*.23f)*scale,p+P(x*.23f,.42f,z*.23f)*scale,.056f*scale,Surface.Iron);
        mesh.Tube(p+Vector3.up*.48f*scale,p+Vector3.up*.72f*scale,.18f*scale,.07f*scale,Surface.Brass,8);
        var lamp=New(root,name,root.TransformPoint(p+P(0,-.20f,-.18f)*scale)).gameObject.AddComponent<Light>();
        lamp.type=LightType.Point;
        lamp.color=new Color(1f,.61f,.27f);
        lamp.range=5.5f*scale;
        lamp.intensity=1.45f*scale;
        lamp.shadows=LightShadows.None;
        lamp.cullingMask=~(1<<31);
    }

    private static void DressPalisade(Transform art,Transform original)
    {
        var stakes=new Craft();
        var random=new System.Random(37);
        foreach(Transform post in original)
        {
            if(post.name!="Trimery pointed timber palisade")continue;
            Vector3 top=post.position+Vector3.up*post.lossyScale.y;
            float height=.24f+(float)random.NextDouble()*.28f;
            stakes.Tube(top-Vector3.up*.07f,top+P(.06f,height,-.06f),post.lossyScale.x*.5f,.036f,
                random.Next(4)==0?Surface.Sapwood:Surface.Bark,10);
        }
        stakes.Place(art,"Palisade hand-hewn stake crowns");
    }

    private static float Ground(float x,float z) =>
        Physics.Raycast(P(x,25,z),Vector3.down,out var hit,45,1<<8,QueryTriggerInteraction.Ignore)?hit.point.y:0;

    private static void DressRoad(Transform parent,Transform town)
    {
        var gravel=new Craft();
        var random=new System.Random(31);
        void Verge(Vector3 from,Vector3 to,float halfWidth)
        {
            var side=Vector3.Cross(Vector3.up,(to-from).normalized);
            int count=Mathf.CeilToInt(Vector3.Distance(from,to)/.49f);
            for(int i=0;i<count;i++)foreach(int sign in new[]{-1,1})
            {
                if(random.NextDouble()<.17)continue;
                var p=Vector3.Lerp(from,to,i/(float)count)+side*(halfWidth+.40f*Mathf.Sin(i*.23f)+.24f*(float)random.NextDouble())*sign;
                p.y=Ground(p.x,p.z)+.045f;
                float size=.16f+.21f*(float)random.NextDouble();
                gravel.Stone(p,size,.074f,size*.81f,random.Next(4)==0?Surface.PaleStone:Surface.Gravel,random.Next(360));
            }
        }
        Verge(P(248,0,142),P(248,0,214),3.55f);
        Verge(P(204,0,180),P(245,0,180),3.55f);
        gravel.Place(parent,"Trimery trade road - cobbled shoulders");
        var gates=new Craft();
        var colors=new[]{Surface.Slate,Surface.WarmStone,Surface.PaleStone,Surface.Gravel};
        foreach(var gate in new[]{(x:248f,z:153f,angle:0f),(x:248f,z:207f,angle:0f),(x:220f,z:180f,angle:90f)})
        {
            var q=Quaternion.Euler(0,gate.angle,0);
            for(int row=-2;row<=2;row++)for(int col=-8;col<=8;col++)
            {
                var p=P(gate.x,0,gate.z)+q*P(col*.43f+(row%2)*.15f,0,row*.32f);
                p.y=Ground(p.x,p.z)+.05f;
                gates.Stone(p,.224f,.061f,.158f,colors[random.Next(colors.Length)],gate.angle+random.Next(-8,9));
            }
        }
        gates.Place(parent,"Trimery gate thresholds - worn setts");

        var old=town.Find("Broken western trade road (no Roadwarden yet)");
        var wreck=new Craft();
        foreach(Transform t in old)
        {
            var renderer=t.GetComponent<MeshRenderer>();
            if(renderer==null)continue;
            if(t.name.StartsWith("Roadwarden smashed paving",StringComparison.Ordinal))
            {
                renderer.enabled=false;
                var size=t.lossyScale;
                wreck.Stone(t.position,size.x*.63f,size.y*.60f,size.z*.62f,
                    random.Next(3)==0?Surface.PaleStone:Surface.Slate,0,t.rotation);
            }
            else if(t.name.Contains("log",StringComparison.OrdinalIgnoreCase))
            {
                renderer.enabled=false;
                var dir=t.lossyScale.z>t.lossyScale.x?t.forward:t.right;
                float length=Mathf.Max(t.lossyScale.x,t.lossyScale.z)*.51f;
                wreck.Log(t.position-dir*length,t.position+dir*length,
                    Mathf.Clamp(t.lossyScale.y*.53f,.19f,.45f),random,true);
            }
        }
        for(int i=0;i<48;i++)
        {
            float x=197.8f+(float)random.NextDouble()*11.8f, z=176f+(float)random.NextDouble()*7.8f;
            float size=.08f+(float)random.NextDouble()*.27f;
            wreck.Stone(P(x,Ground(x,z)+.12f,z),size,size*.55f,size*.64f,Surface.Slate,random.Next(360));
        }
        wreck.Place(parent,"Western highway - fractured setts and split trunks");

        var sign=New(parent,"Western highway warning sign",P(211.9f,Ground(211.9f,177f),177f),63);
        var board=new Craft();
        foreach(int side in new[]{-1,1})
            board.Beam(P(side*1.10f,0,0),P(side*1.10f,1.8f,0),.15f,Surface.DarkOak);
        for(int i=0;i<3;i++)board.Box(P(0,1.07f+i*.225f,0),P(2.76f,.205f,.14f),i==1?Surface.Oak:Surface.DarkOak);
        board.Beam(P(-1.31f,.94f,-.105f),P(1.31f,.94f,-.105f),.063f,Surface.Clay);
        board.Beam(P(-1.31f,1.67f,-.105f),P(1.31f,1.67f,-.105f),.063f,Surface.Clay);
        Lettering(board,"ROAD CLOSED",P(0,1.29f,-.091f),0,.039f);
        board.Place(sign,"Road closure - timber warning board");
    }



    private static readonly Dictionary<char,string[]> Glyphs = new()
    {
        ['A']=new[]{"01110","10001","10001","11111","10001","10001","10001"},
        ['B']=new[]{"11110","10001","10001","11110","10001","10001","11110"},
        ['C']=new[]{"01111","10000","10000","10000","10000","10000","01111"},
        ['D']=new[]{"11110","10001","10001","10001","10001","10001","11110"},
        ['E']=new[]{"11111","10000","10000","11110","10000","10000","11111"},
        ['I']=new[]{"11111","00100","00100","00100","00100","00100","11111"},
        ['K']=new[]{"10001","10010","10100","11000","10100","10010","10001"},
        ['L']=new[]{"10000","10000","10000","10000","10000","10000","11111"},
        ['M']=new[]{"10001","11011","10101","10101","10001","10001","10001"},
        ['O']=new[]{"01110","10001","10001","10001","10001","10001","01110"},
        ['R']=new[]{"11110","10001","10001","11110","10100","10010","10001"},
        ['S']=new[]{"01111","10000","10000","01110","00001","00001","11110"},
        ['T']=new[]{"11111","00100","00100","00100","00100","00100","00100"},
        ['Y']=new[]{"10001","10001","01010","00100","00100","00100","00100"}
    };

    private static void Lettering(Craft craft,string text,Vector3 center,float yaw,float pixel,float tilt=0)
    {
        var facing=Quaternion.Euler(0,yaw,0)*Quaternion.Euler(tilt,0,0);
        float start=-(text.Length*6-2)*pixel*.5f;
        for(int index=0;index<text.Length;index++)
        {
            if(text[index]==' ')continue;
            var glyph=Glyphs[text[index]];
            for(int row=0;row<glyph.Length;row++)
            {
                int column=0;
                while(column<5)
                {
                    if(glyph[row][column]=='0'){column++;continue;}
                    int first=column;
                    while(column<5&&glyph[row][column]=='1')column++;
                    float x=start+(6*index+(first+column-1)*.5f)*pixel;
                    Vector3 position=center+facing*P(x,(3-row)*pixel,0);
                    craft.Box(position,P((column-first)*pixel,.99f*pixel,.023f),Surface.LetterPaint,facing);
                }
            }
        }
    }

    public static object Cornberg()
    {
        var scene=OpenWorld();
        Palette();
        AssetDatabase.ImportAsset(WaterShaderPath,ImportAssetOptions.ForceSynchronousImport);
        var village=Find(scene,"04 - Cornberg village");
        var art=Rebuild(village,"Cornberg handcrafted village green");
        DressSpring(art,village.Find("Village green - well and ancient stone"));
        DressRiver(art,Find(scene,"01 - Valley ground and stream"));
        DressCornbergBuildings(art,village);
        DressRoadSurface(art,Find(scene,"02 - Arrival and connected forest paths"),"Cornberg",new Color(.97f,.88f,.71f));
        int materials=RepairWolfMaterials();
        Save(scene);
        return new { art=Report(art),wolfMaterialsConverted=materials };
    }

    private static void DressSpring(Transform art,Transform source)
    {
        var water=source.Find("Magical well water");
        if(water==null)throw new InvalidOperationException("Cornberg well water missing.");
        var well=New(art,"Magical well - carved masonry",P(-2,Ground(-2,3),3));
        var masonry=new Craft();
        float height=water.position.y-well.position.y+.24f;
        const int segments=20;
        for(int tier=0;tier<3;tier++)
        for(int i=0;i<segments;i++)
        {
            float a=(i+tier*.5f)*360f/segments;
            Vector3 radial=Quaternion.Euler(0,a,0)*Vector3.forward;
            var surface=(i+tier)%5==0?Surface.PaleStone:(i+tier)%3==0?Surface.WarmStone:Surface.Slate;
            float radius=tier==2?1.51f:1.46f;
            masonry.Box(radial*radius+Vector3.up*(.25f+tier*height/3f),
                P(.448f,tier==2?.22f:.27f,tier==2?.47f:.36f),surface,Quaternion.Euler(0,a,0));
        }
        foreach(var stone in source.Cast<Transform>().Where(t=>t.name=="Well ring stone"))
            stone.GetComponent<Renderer>().enabled=false;
        water.GetComponent<Renderer>().enabled=false;
        for(int i=0;i<6;i++)
        {
            float angle=i*Mathf.PI/3;
            var p=P(Mathf.Cos(angle)*1.49f,height+.13f,Mathf.Sin(angle)*1.49f);
            masonry.Box(p,P(.126f,.063f,.126f),Surface.Brass,Quaternion.Euler(0,45,0));
        }
        masonry.Place(well,"Cornberg well - staggered fieldstone");
        var surfaceMesh=new Craft();
        surfaceMesh.Tube(Vector3.up*(height-.065f),Vector3.up*(height-.051f),1.30f,1.30f,Surface.WaterDeep,48);
        var liquid=surfaceMesh.Place(well,"Cornberg well - animated spring");
        liquid.sharedMaterial=WaterMaterial("Cornberg spring water",true);
        liquid.shadowCastingMode=ShadowCastingMode.Off;

        var apron=new Craft();
        var random=new System.Random(122);
        for(int i=0;i<42;i++)
        {
            float a=i*Mathf.PI*2/42;
            float radius=1.89f+.38f*(float)random.NextDouble();
            var p=well.position+P(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);
            p.y=Ground(p.x,p.z)+.065f;
            float size=.12f+.18f*(float)random.NextDouble();
            apron.Stone(p,size,.068f,size*.81f,
                i%3==0?Surface.Moss:i%2==0?Surface.PaleStone:Surface.WarmStone,random.Next(360));
        }
        apron.Place(art,"Cornberg spring apron - weathered stones");
    }

    private static float StreamX(float z)=>-18f+Mathf.Sin(z*.065f)*2f;

    private static void DressRiver(Transform art,Transform source)
    {
        var river=new Craft();
        float first=-53f,last=81.2f;
        foreach(var channel in source.Cast<Transform>().Where(t=>t.name=="Stream channel"))
            channel.GetComponent<Renderer>().enabled=false;
        Vector3 Edge(float z,int sign)=>P(StreamX(z)+sign*2.30f,-.386f,z);
        for(float z=first;z<last;z+=.9f)
        {
            float next=Mathf.Min(z+.9f,last);
            river.Face(Surface.WaterDeep,Vector3.up,Edge(z,-1),Edge(next,-1),Edge(next,1),Edge(z,1));
        }
        var water=river.Place(art,"Cornberg stream - continuous animated surface");
        water.sharedMaterial=WaterMaterial("Cornberg river water",false);
        water.shadowCastingMode=ShadowCastingMode.Off;

        var banks=new Craft();
        var random=new System.Random(7418);
        for(float z=-44f;z<62f;z+=1.6f)
        {
            if(Mathf.Abs(z+9)<4||Mathf.Abs(z-26)<4)continue;
            foreach(int side in new[]{-1,1})
            {
                float x=StreamX(z)+side*(2.6f+(float)random.NextDouble()*.74f);
                var p=P(x,Ground(x,z)+.05f,z);
                float radius=.22f+.25f*(float)random.NextDouble();
                if(random.Next(4)!=0)
                    banks.Stone(p,radius,.17f,radius*.87f,random.Next(3)==0?Surface.Moss:Surface.Slate,random.Next(360));
                if(random.Next(3)==0)Reeds(banks,p+P(side*.33f,-.04f,.28f),random);
            }
        }
        banks.Place(art,"Cornberg stream - reed beds and rounded banks");
        foreach(var bridge in source.Cast<Transform>().Where(t=>t.name=="Timber footbridge"))
            DressBridge(art,source,bridge);
    }

    private static void Reeds(Craft banks,Vector3 origin,System.Random random)
    {
        for(int i=0;i<5;i++)
        {
            float angle=(float)random.NextDouble()*Mathf.PI*2;
            var spread=P(Mathf.Cos(angle),0,Mathf.Sin(angle));
            var basePoint=origin+spread*.13f;
            float height=.42f+.53f*(float)random.NextDouble();
            var direction=Vector3.up*height+spread*(.12f+.10f*(float)random.NextDouble());
            var middle=basePoint+direction*.57f;
            var across=Vector3.Cross(Vector3.up,spread)*.056f;
            var normal=Vector3.Cross(across,direction);
            banks.Face(Surface.Moss,normal,basePoint-across*.4f,basePoint+across*.4f,middle+across,middle-across);
            banks.Triangle(Surface.Moss,normal,middle-across,middle+across,basePoint+direction);
            banks.Face(Surface.MossLight,-normal,basePoint-across*.4f,basePoint+across*.4f,middle+across,middle-across);
            banks.Triangle(Surface.MossLight,-normal,middle-across,middle+across,basePoint+direction);
            if(i%3==0)
                banks.Tube(basePoint+direction*.64f,basePoint+direction*1.08f,.045f,.038f,Surface.Bark,7);
        }
    }

    private static void DressBridge(Transform art,Transform source,Transform bridge)
    {
        float z=bridge.position.z;
        var anchor=New(art,z<0?"Cornberg southern footbridge":"Cornberg northern footbridge",bridge.position);
        var wood=new Craft();
        var size=bridge.lossyScale;
        float deck=size.y*.5f+.025f;
        var random=new System.Random(Mathf.RoundToInt(453+z));
        bridge.GetComponent<Renderer>().enabled=false;
        foreach(var rail in source.Cast<Transform>().Where(t=>t.name=="Bridge rail"&&Mathf.Abs(t.position.z-z)<3f))
            rail.GetComponent<Renderer>().enabled=false;
        for(int i=0;i<22;i++)
        {
            float x=Mathf.Lerp(-size.x*.48f,size.x*.48f,i/21f);
            wood.Box(P(x,deck-.076f,0),P(size.x/22f-.016f,.164f,size.z-.04f+(float)random.NextDouble()*.12f),
                i%6==2?Surface.Sapwood:i%5==1?Surface.Bark:Surface.Oak,Quaternion.Euler(0,random.Next(-1,2)*.4f,0));
            foreach(int side in new[]{-1,1})
                wood.Box(P(x,deck+.01f,side*(size.z*.45f)),P(.065f,.028f,.064f),Surface.Iron);
        }
        foreach(int side in new[]{-1,1})
        {
            float outer=side*(size.z*.5f-.071f);
            wood.Beam(P(-size.x*.5f,deck-.12f,outer),P(size.x*.5f,deck-.12f,outer),.23f,Surface.DarkOak);
            wood.Beam(P(-size.x*.5f,deck+.68f,outer),P(size.x*.5f,deck+.68f,outer),.18f,Surface.Oak);
            wood.Beam(P(-size.x*.46f,deck+.32f,outer),P(size.x*.46f,deck+.32f,outer),.125f,Surface.DarkOak);
            for(int post=-2;post<=2;post++)
            {
                float x=post*size.x*.23f;
                wood.Beam(P(x,deck-.25f,outer),P(x,deck+.84f,outer),.23f,Surface.Oak);
                wood.Box(P(x,deck+.85f,outer),P(.29f,.11f,.30f),Surface.Sapwood);
                if(post<2)wood.Beam(P(x,deck+.05f,outer),P(x+size.x*.23f,deck+.60f,outer),.12f,Surface.Sapwood);
            }
        }
        Lantern(anchor,wood,P(-size.x*.47f,deck+1.30f,-size.z*.5f),.68f,anchor.name+" lantern");
        wood.Place(anchor,anchor.name+" - hewn deck and railings");
    }

    private static Material WaterMaterial(string name,bool basin)
    {
        string path=Folder+"/"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader=AssetDatabase.LoadAssetAtPath<Shader>(WaterShaderPath);
        if(shader==null||!shader.isSupported)throw new InvalidOperationException("Cornberg water shader unavailable.");
        if(material==null)
        {
            material=new Material(shader){name=name};
            AssetDatabase.CreateAsset(material,path);
        }
        material.SetFloat("_BasinMode",basin?1:0);
        material.SetVector("_Basin",new Vector4(-2,3,1.30f,0));
        if(basin)
        {
            material.SetColor("_DeepColor",new Color(.047f,.31f,.33f));
            material.SetColor("_ShallowColor",new Color(.27f,.64f,.54f));
            material.SetColor("_GlintColor",new Color(.68f,.97f,.88f));
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static int RepairWolfMaterials()
    {
        const string ironpine="Assets/_DiceFree/Settings/Ironpine";
        var shader=Shader.Find("Universal Render Pipeline/Lit");
        int count=0;
        foreach(string asset in AssetDatabase.FindAssets("t:Material",new[]{ironpine}).Select(AssetDatabase.GUIDToAssetPath))
        {
            var material=AssetDatabase.LoadAssetAtPath<Material>(asset);
            if(material==null||!material.name.StartsWith("Art_GreyWolf_",StringComparison.Ordinal)||
                material.shader.name!="Standard")continue;
            var tint=material.GetColor("_Color");
            var texture=material.GetTexture("_MainTex");
            var textureOffset=material.GetTextureOffset("_MainTex");
            var textureScale=material.GetTextureScale("_MainTex");
            var smoothness=material.GetFloat("_Glossiness");
            material.shader=shader;
            material.SetColor("_BaseColor",tint);
            material.SetTexture("_BaseMap",texture);
            material.SetTextureOffset("_BaseMap",textureOffset);
            material.SetTextureScale("_BaseMap",textureScale);
            material.SetFloat("_Smoothness",smoothness);
            if(tint.a<.98f)
            {
                material.SetFloat("_Surface",1);
                material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite",0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.SetOverrideTag("RenderType","Transparent");
                material.SetShaderPassEnabled("ShadowCaster",false);
                material.renderQueue=(int)RenderQueue.Transparent;
            }
            else material.renderQueue=(int)RenderQueue.Geometry;
            EditorUtility.SetDirty(material);
            count++;
        }
        return count;
    }


    private static void DressCornbergBuildings(Transform art,Transform village)
    {
        int index=0;
        var thatch=new[]{Surface.Thatch,Surface.ThatchTip,Surface.ThatchShadow};
        foreach(Transform house in village)
        {
            var old=house.Find("Gabled roof");
            if(old==null)continue;
            var walls=house.Find("Timber and lime walls");
            var roof=New(art,"Cornberg "+house.name+" - painted timber",old.position,old.eulerAngles.y);
            var geometry=new Craft();
            var palette=house.name switch
            {
                "Blacksmith"=>Roofing[1],
                "Bank"=>Roofing[0],
                "General goods"=>Roofing[2],
                "Abandoned house - locked"=>Roofing[2],
                _=>thatch
            };
            ShingleRoof(geometry,old.lossyScale.x,old.lossyScale.z,old.lossyScale.y,palette,1200+index,
                coarseThatch:ReferenceEquals(palette,thatch));
            geometry.Place(roof,"Cornberg roof "+index+" - "+house.name,fade:true);
            old.GetComponent<Renderer>().enabled=false;

            var frame=new Craft();
            float floor=roof.InverseTransformPoint(walls.position).y-walls.lossyScale.y*.50f;
            float edgeX=walls.lossyScale.x*.50f,edgeZ=walls.lossyScale.z*.50f;
            foreach(int side in new[]{-1,1})
            {
                float x=(edgeX+.045f)*side;
                frame.Beam(P(x,-.20f,-edgeZ),P(x,-.20f,edgeZ),.15f,Surface.DarkOak);
                frame.Beam(P(x,floor+.52f,-edgeZ),P(x,floor+.52f,edgeZ),.14f,Surface.Oak);
                for(int bay=-1;bay<=1;bay++)
                {
                    float z=bay*(edgeZ-.12f);
                    frame.Beam(P(x,floor+.43f,z),P(x,-.10f,z),.20f,Surface.Oak);
                    if(bay<1)frame.Beam(P(x,floor+.63f,z+.16f),P(x,-.29f,z+edgeZ*.86f),.13f,Surface.Oak);
                }
                float front=-edgeZ-.13f;
                float windowX=side*edgeX*.58f;
                frame.Box(P(windowX,-1.28f,front),P(1.13f,1.23f,.12f),Surface.Oak);
                frame.Box(P(windowX,-1.28f,front-.073f),P(.89f,.95f,.062f),Surface.SlateDark);
                frame.Beam(P(windowX,-1.72f,front-.11f),P(windowX,-.82f,front-.11f),.065f,Surface.Sapwood);
                frame.Beam(P(windowX-.44f,-1.29f,front-.11f),P(windowX+.44f,-1.29f,front-.11f),.065f,Surface.Sapwood);
                frame.Box(P(windowX,-1.89f,front-.13f),P(1.25f,.12f,.36f),Surface.Oak);
            }
            frame.Beam(P(-edgeX,floor+.53f,-edgeZ-.06f),P(edgeX,floor+.53f,-edgeZ-.06f),.13f,Surface.Oak);
            frame.Beam(P(-edgeX,-.20f,-edgeZ-.06f),P(edgeX,-.20f,-edgeZ-.06f),.17f,Surface.DarkOak);
            frame.Box(P(0,floor+.16f,-edgeZ-.28f),P(1.63f,.22f,.52f),Surface.WarmStone);
            if(house.name is "Bank" or "Blacksmith" or "General goods" or "Brewery")
            {
                frame.Beam(P(-.90f,-1.02f,-edgeZ-.06f),P(-.90f,-1.04f,-edgeZ-.49f),.12f,Surface.DarkOak);
                Lantern(roof,frame,P(-.90f,-1.39f,-edgeZ-.46f),.62f,"Cornberg "+house.name+" lantern");
            }
            frame.Place(roof,"Cornberg joinery "+index+" - "+house.name);
            index++;
        }
        DressForge(art,village.Find("Blacksmith"));
    }

    private static void DressForge(Transform art,Transform smith)
    {
        var chimney=smith.Find("Forge chimney");
        var center=chimney.position;
        var size=chimney.lossyScale;
        var stone=new Craft();
        var anchor=New(art,"Cornberg forge - stonework and coals",center);
        float start=-size.y*.5f,top=size.y*.5f;
        const int rows=12;
        for(int row=0;row<rows;row++)
        {
            float y=Mathf.Lerp(start,top,(row+.5f)/rows);
            for(int side=-1;side<=1;side+=2)
            for(int edge=-1;edge<=1;edge+=2)
            {
                float p=(row%2==0?.41f:.35f)*edge;
                var color=(row+edge+side)%4==0?Surface.BrickDark:Surface.Brick;
                stone.Box(P(side*.73f,y,p),P(.36f,.389f,.85f),color);
                stone.Box(P(p,y,side*.73f),P(.82f,.389f,.37f),color);
            }
        }
        stone.Box(P(0,top-.31f,0),P(1.34f,.13f,1.34f),Surface.DarkOak);
        foreach(int side in new[]{-1,1})
        {
            stone.Box(P(side*.76f,top+.06f,0),P(.39f,.25f,1.98f),Surface.PaleStone);
            stone.Box(P(0,top+.06f,side*.78f),P(1.16f,.25f,.37f),Surface.PaleStone);
        }
        chimney.GetComponent<Renderer>().enabled=false;
        var hearth=smith.Find("Forge hearth");
        var coals=smith.Find("Banked coals");
        var bed=anchor.InverseTransformPoint(coals.position);
        coals.GetComponent<Renderer>().enabled=false;
        var edgeOfHearth=anchor.InverseTransformPoint(hearth.position);
        for(int row=0;row<4;row++)
        for(int col=-3;col<=3;col++)
        {
            var p=edgeOfHearth+P(col*.39f+(row%2)*.09f,-.55f+row*.38f,-.91f);
            stone.Box(p,P(.366f,.35f,.15f),(row+col)%3==0?Surface.Slate:Surface.WarmStone);
        }
        var random=new System.Random(8857);
        for(int i=0;i<28;i++)
        {
            var p=bed+P(((float)random.NextDouble()-.5f)*1.70f,
                .009f+(float)random.NextDouble()*.11f,((float)random.NextDouble()-.5f)*.81f);
            stone.Stone(p,.14f,.09f,.12f,i%3==0?Surface.Ember:Surface.BrickDark,random.Next(360));
        }
        stone.Place(anchor,"Cornberg blacksmith - masonry and live embers");
        var light=New(anchor,"Cornberg forge light",coals.position+Vector3.up*.58f).gameObject.AddComponent<Light>();
        light.type=LightType.Point;
        light.color=new Color(1f,.46f,.21f);
        light.range=4.6f;
        light.intensity=1.35f;
        light.shadows=LightShadows.None;
        light.cullingMask=~(1<<31);
    }

    private static void DressRoadSurface(Transform art,Transform source,string prefix,Color tint)
    {
        const string texturePath=Folder+"/Packed road grain.png";
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if(texture==null)
        {
            const string sourcePath="Assets/_DiceFree/Settings/WolfSurvival/Packed earth.png";
            var authored=new Texture2D(2,2,TextureFormat.RGBA32,false);
            if(!authored.LoadImage(System.IO.File.ReadAllBytes(sourcePath)))
                throw new InvalidOperationException("Cannot read existing packed earth grain.");
            var pixels=authored.GetPixels32();
            for(int index=0;index<pixels.Length;index++)
            {
                var pixel=pixels[index];
                float luminance=(.2126f*pixel.r+.7152f*pixel.g+.0722f*pixel.b)/255f;
                byte value=(byte)Mathf.RoundToInt(255*Mathf.Clamp01(.765f+(luminance-.29f)*1.05f));
                pixels[index]=new Color32(value,value,value,255);
            }
            authored.SetPixels32(pixels);
            authored.Apply();
            System.IO.File.WriteAllBytes(texturePath,authored.EncodeToPNG());
            Object.DestroyImmediate(authored);
            AssetDatabase.ImportAsset(texturePath,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.wrapMode=TextureWrapMode.Repeat;
            importer.filterMode=FilterMode.Trilinear;
            importer.anisoLevel=4;
            importer.mipmapEnabled=true;
            importer.SaveAndReimport();
            texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        }

        string materialPath=Folder+"/"+prefix+" worn road.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(material==null)
        {
            material=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=prefix+" worn road"};
            AssetDatabase.CreateAsset(material,materialPath);
        }
        material.SetColor("_BaseColor",tint);
        material.SetTexture("_BaseMap",texture);
        material.SetFloat("_Smoothness",.10f);
        EditorUtility.SetDirty(material);
        foreach(var old in source.GetComponentsInChildren<MeshFilter>())
        {
            var original=old.GetComponent<MeshRenderer>();
            if(original==null)continue;
            string name=prefix+" packed earth - "+old.name;
            string path=Folder+"/"+name+".asset";
            var copied=Object.Instantiate(old.sharedMesh);
            copied.name=name;
            copied.uv=copied.vertices.Select(v=>{
                var world=old.transform.TransformPoint(v);
                return new Vector2(world.x,world.z)*.29f;
            }).ToArray();
            var asset=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(asset==null)
            {
                asset=copied;
                AssetDatabase.CreateAsset(asset,path);
            }
            else
            {
                EditorUtility.CopySerialized(copied,asset);
                Object.DestroyImmediate(copied);
                EditorUtility.SetDirty(asset);
            }
            var visual=New(art,name,old.transform.position);
            visual.rotation=old.transform.rotation;
            visual.localScale=old.transform.lossyScale;
            visual.gameObject.AddComponent<MeshFilter>().sharedMesh=asset;
            var renderer=visual.gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;
            original.enabled=false;
        }
    }
    private static void DressForest(Scene scene,Transform art)
    {
        var forest=New(art,"Trimery edge - layered firs",Vector3.zero);
        var originals=scene.GetRootGameObjects()
            .SelectMany(r=>r.GetComponentsInChildren<MeshFilter>(true))
            .Where(f=>f.sharedMesh!=null&&f.sharedMesh.name=="Sphere"&&f.transform.parent!=null&&
                f.transform.parent.name.StartsWith("Ironpine conifer",StringComparison.Ordinal)&&
                (f.name.StartsWith("Canopy",StringComparison.Ordinal)||f.name.StartsWith("Sunward crown",StringComparison.Ordinal)))
            .GroupBy(f=>f.transform.parent)
            .Where(group=>{
                var p=group.First().transform.position;
                return p.x>188&&p.x<298&&p.z>137&&p.z<227;
            }).OrderBy(group=>group.First().transform.position.x).ThenBy(group=>group.First().transform.position.z).ToArray();

        var random=new System.Random(1578);
        var variants=new MeshRenderer[5];
        foreach(var original in originals)
        {
            var oldRenderers=original.Select(f=>f.GetComponent<MeshRenderer>()).Where(r=>r!=null).ToArray();
            if(oldRenderers.Length==0)continue;
            var bounds=oldRenderers[0].bounds;
            foreach(var old in oldRenderers)
            {
                bounds.Encapsulate(old.bounds);
                old.enabled=false;
            }
            float baseY=Ground(bounds.center.x,bounds.center.z);
            var position=P(bounds.center.x,baseY,bounds.center.z);
            int variant=random.Next(variants.Length);
            var anchor=New(forest,"Fir silhouette - "+original.Key.name,position,random.Next(360));
            anchor.localScale=P(bounds.size.x*.50f,Mathf.Max(4f,bounds.max.y-baseY),bounds.size.z*.50f);
            if(variants[variant]==null)
            {
                var foliage=Fir(variant);
                variants[variant]=foliage.Place(anchor,"Trimery fir canopy "+variant,fade:true);
                continue;
            }
            var instance=New(anchor,"Layered canopy",position);
            instance.localRotation=Quaternion.identity;
            instance.localScale=Vector3.one;
            instance.gameObject.AddComponent<MeshFilter>().sharedMesh=variants[variant].GetComponent<MeshFilter>().sharedMesh;
            instance.gameObject.AddComponent<MeshRenderer>().sharedMaterials=variants[variant].sharedMaterials;
            instance.gameObject.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(32f);
        }
    }

    private static Craft Fir(int variant)
    {
        var mesh=new Craft();
        var random=new System.Random(143+variant*1747);
        const int sprays=15;
        float drift=(variant-2)*.045f;
        for(int tier=0;tier<7;tier++)
        {
            float along=tier/6f;
            float radius=Mathf.Lerp(1.02f,.17f,Mathf.Pow(along,.92f));
            float y=.34f+along*.57f;
            var core=P(drift*along,y+.145f+(1-along)*.07f,drift*along*.60f);
            for(int branch=0;branch<sprays;branch++)
            {
                float angle=(branch+tier*.46f)*Mathf.PI*2/sprays;
                var outward=P(Mathf.Cos(angle),0,Mathf.Sin(angle));
                var across=Vector3.Cross(Vector3.up,outward);
                float length=radius*(.78f+(float)random.NextDouble()*.29f);
                float dip=(float)random.NextDouble()*.061f;
                var basePoint=P(drift*along,y+.086f,drift*along*.60f)+outward*length*.21f;
                var middle=basePoint+outward*length*.40f+Vector3.down*.046f;
                var tip=basePoint+outward*length*.81f+Vector3.down*(.055f+dip);
                float span=length*(.23f+(float)random.NextDouble()*.073f);
                var left=middle-across*span;
                var right=middle+across*span;
                var ridge=middle+Vector3.up*(.039f+.037f*(1-along));
                var color=variant==2?Surface.PineFrost:Surface.Pine;
                var tips=variant==4?Surface.PineGold:Surface.PineTip;
                mesh.Triangle(color,Vector3.up,core,left,ridge);
                mesh.Triangle(color,Vector3.up,core,ridge,right);
                mesh.Triangle(tips,Vector3.up,left,tip,ridge);
                mesh.Triangle(color,Vector3.up,ridge,tip,right);
                mesh.Triangle(Surface.PineDark,Vector3.down,left,right,tip);
                mesh.Triangle(Surface.PineDark,Vector3.down,basePoint,right,left);
                for(int comb=0;comb<4;comb++)
                {
                    float alongBranch=.40f+comb*.138f;
                    var stem=Vector3.Lerp(basePoint,tip,alongBranch);
                    foreach(int side in new[]{-1,1})
                    {
                        float reach=span*(1.48f-comb*.27f);
                        var needle=stem+across*(side*reach)+outward*length*.12f+Vector3.down*.038f;
                        var tipSurface=random.Next(5)==0?tips:color;
                        mesh.Triangle(tipSurface,Vector3.up,stem-outward*length*.14f,
                            stem+outward*length*.15f+Vector3.up*.02f,needle);
                        mesh.Triangle(Surface.PineDark,Vector3.down,stem-outward*length*.14f,
                            stem+outward*length*.15f+Vector3.up*.02f,needle);
                    }
                }
            }
        }
        mesh.Tube(P(drift*.8f,.84f,drift*.5f),P(drift,1.07f,drift*.60f),.14f,.013f,
            variant==2?Surface.PineFrost:Surface.Pine,9);
        return mesh;
    }
    private sealed class Craft
    {
        private readonly List<Vector3> vertices=new();
        private readonly Dictionary<Surface,List<int>> indices=new();

        private void Add(Surface surface,Vector3 facing,params Vector3[] corners)
        {
            if(!indices.TryGetValue(surface,out var target))indices.Add(surface,target=new List<int>());
            int start=vertices.Count;
            vertices.AddRange(corners);
            bool outward=Vector3.Dot(Vector3.Cross(corners[1]-corners[0],corners[2]-corners[0]),facing)>0;
            for(int i=1;i<corners.Length-1;i++)
            {
                target.Add(start);
                target.Add(start+(outward?i:i+1));
                target.Add(start+(outward?i+1:i));
            }
        }

        public void Face(Surface s,Vector3 n,Vector3 a,Vector3 b,Vector3 c,Vector3 d)=>Add(s,n,a,b,c,d);
        public void Triangle(Surface s,Vector3 n,Vector3 a,Vector3 b,Vector3 c)=>Add(s,n,a,b,c);

        public void Box(Vector3 center,Vector3 size,Surface surface,Quaternion? rotation=null)
        {
            var r=rotation??Quaternion.identity;
            Vector3 C(int x,int y,int z)=>center+r*Vector3.Scale(size*.5f,P(x,y,z));
            foreach(int d in new[]{-1,1})
            {
                Face(surface,r*P(d,0,0),C(d,-1,-1),C(d,1,-1),C(d,1,1),C(d,-1,1));
                Face(surface,r*P(0,d,0),C(-1,d,-1),C(1,d,-1),C(1,d,1),C(-1,d,1));
                Face(surface,r*P(0,0,d),C(-1,-1,d),C(1,-1,d),C(1,1,d),C(-1,1,d));
            }
        }

        public void Beam(Vector3 a,Vector3 b,float width,Surface surface)
        {
            var delta=b-a;
            if(delta.sqrMagnitude<.00001f)return;
            Box((a+b)*.5f,P(width,delta.magnitude,width),surface,
                Quaternion.FromToRotation(Vector3.up,delta.normalized));
        }

        private static (Vector3 u,Vector3 v) Plane(Vector3 axis)
        {
            var u=Vector3.Cross(axis,Mathf.Abs(axis.y)<.92f?Vector3.up:Vector3.forward).normalized;
            return (u,Vector3.Cross(axis,u).normalized);
        }

        public void Tube(Vector3 a,Vector3 b,float radiusA,float radiusB,Surface surface,int count,bool caps=true)
        {
            var axis=(b-a).normalized;
            var (u,v)=Plane(axis);
            Vector3 Radial(int i)=>u*Mathf.Cos(i*Mathf.PI*2/count)+v*Mathf.Sin(i*Mathf.PI*2/count);
            for(int i=0;i<count;i++)
            {
                var p=Radial(i);var q=Radial(i+1);
                Face(surface,p+q,a+p*radiusA,a+q*radiusA,b+q*radiusB,b+p*radiusB);
                if(caps)
                {
                    Triangle(surface,-axis,a,a+q*radiusA,a+p*radiusA);
                    Triangle(surface,axis,b,b+p*radiusB,b+q*radiusB);
                }
            }
        }

        public void Ring(Vector3 center,Vector3 axis,float radius,float thickness,Surface surface,int count)
        {
            var (u,v)=Plane(axis);
            Vector3 Rim(int i)=>center+radius*(u*Mathf.Cos(i*Mathf.PI*2/count)+v*Mathf.Sin(i*Mathf.PI*2/count));
            for(int i=0;i<count;i++)Tube(Rim(i),Rim(i+1),thickness*.5f,thickness*.5f,surface,5,false);
        }

        public void Log(Vector3 a,Vector3 b,float radius,System.Random random,bool broken=false)
        {
            var axis=(b-a).normalized;
            var (u,v)=Plane(axis);
            const int count=12;
            var endsA=new Vector3[count];
            var endsB=new Vector3[count];
            for(int i=0;i<count;i++)
            {
                float angle=i*Mathf.PI*2/count;
                var radial=u*Mathf.Cos(angle)+v*Mathf.Sin(angle);
                float r=radius*(.90f+(float)random.NextDouble()*.19f);
                endsA[i]=a+radial*r;
                endsB[i]=b+radial*r+axis*(broken?(float)random.NextDouble()*.49f:0);
            }
            for(int i=0;i<count;i++)
            {
                int next=(i+1)%count;
                Face(i%4==0?Surface.Oak:i%5==0?Surface.DarkOak:Surface.Bark,
                    endsA[i]-a,endsA[i],endsA[next],endsB[next],endsB[i]);
                Triangle(Surface.Sapwood,-axis,a,endsA[next],endsA[i]);
                Triangle(broken&&i%3==0?Surface.Oak:Surface.Sapwood,axis,b,endsB[i],endsB[next]);
            }
            foreach(float ratio in new[]{.29f,.57f,.79f})
            {
                Ring(a-axis*.008f,axis,radius*ratio,.014f,Surface.Oak,20);
                if(!broken)Ring(b+axis*.008f,axis,radius*ratio,.014f,Surface.Oak,20);
            }
        }

        public void Stone(Vector3 center,float rx,float ry,float rz,Surface surface,float yaw,Quaternion? rotation=null)
        {
            var r=rotation??Quaternion.Euler(0,yaw,0);
            const int sides=7;
            Vector3 Perimeter(int i,float height,float scale)
            {
                float a=(i+sides)%sides*Mathf.PI*2/sides;
                return center+r*P(Mathf.Cos(a)*rx*scale,height,Mathf.Sin(a)*rz*scale*(i%3==0?.88f:1f));
            }
            for(int i=0;i<sides;i++)
            {
                var a=Perimeter(i,-ry*.55f,1); var b=Perimeter(i+1,-ry*.55f,1);
                var c=Perimeter(i+1,ry,.76f); var d=Perimeter(i,ry,.76f);
                Face(surface,(a+b)*.5f-center,a,b,c,d);
                Triangle(surface,r*Vector3.up,center+r*Vector3.up*ry,d,c);
            }
        }

        public MeshRenderer Place(Transform parent,string name,Vector3 offset=default,bool fade=false)
        {
            if(vertices.Count==0)throw new InvalidOperationException("Empty mesh "+name);
            var mesh=new Mesh { name=name,indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16 };
            mesh.SetVertices(vertices);
            mesh.subMeshCount=indices.Count;
            int index=0;
            foreach(var faces in indices.Values)mesh.SetTriangles(faces,index++);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            string path=Folder+"/"+name+".asset";
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing==null)AssetDatabase.CreateAsset(mesh,path);
            else
            {
                EditorUtility.CopySerialized(mesh,existing);
                Object.DestroyImmediate(mesh);
                mesh=existing;
                EditorUtility.SetDirty(mesh);
            }
            var obj=new GameObject(name);
            obj.layer=2;
            obj.transform.SetParent(parent,false);
            obj.transform.localPosition=offset;
            obj.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterials=indices.Keys.Select(s=>Materials[s]).ToArray();
            if(fade)obj.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(29f);
            return renderer;
        }
    }
}
