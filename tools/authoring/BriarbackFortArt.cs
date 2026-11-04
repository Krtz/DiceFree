using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Original procedural fort artwork. All gameplay surfaces/colliders remain the authored originals.
public static class BriarbackFortArt
{
    const string Folder="Assets/_DiceFree/Art/World/BriarbackFort";
    const string Root="Briarback fort finished timber and stone";
    const string Recovery="Assets/_DiceFree/Settings/Recovery/Cornberg-before-fort-art-20261104.unity";
    static readonly string[] Names={"Heartwood","Worn timber","Cut endgrain","Cliff slate","Stone strata","Copper straps","Oxblood cloth","Carved ivory","Packed earth","Moss"};
    static readonly Color[] Colors={new(.19f,.115f,.065f),new(.35f,.23f,.12f),new(.48f,.34f,.19f),new(.23f,.25f,.24f),new(.38f,.37f,.31f),new(.43f,.27f,.13f),new(.34f,.07f,.048f),new(.72f,.64f,.44f),new(.30f,.265f,.18f),new(.29f,.32f,.15f)};
    static float Height(float x,float z)=>.22f*Mathf.Sin(x*.09f)*Mathf.Sin(z*.08f);
    static Vector3 Ground(float x,float z,float y=0)=>new(x,Height(x,z)+y,z);
    static void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
    static Transform Fort()=>GameObject.Find("Briarback northern mountain stronghold - additive").transform;
    static Scene Ready(){var s=SceneManager.GetActiveScene();Need(s.path=="Assets/_DiceFree/Scenes/Cornberg.unity"&&!s.isDirty&&!EditorApplication.isPlayingOrWillChangePlaymode,"Open clean Cornberg Edit Mode.");return s;}
    public static object Author()
    {
        var scene=Ready();var fort=Fort();
        Need(fort.Find(Root)==null&&!AssetDatabase.IsValidFolder(Folder)&&!File.Exists(Recovery),"Fort art or recovery already exists.");
        Need(EditorSceneManager.SaveScene(scene,Recovery,true),"Recovery failed.");
        AssetDatabase.CreateFolder("Assets/_DiceFree/Art/World","BriarbackFort");
        var root=new GameObject(Root).transform;root.SetParent(fort,false);
        var art=new Art();art.Design(fort);art.Install(root);
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Scene save failed.");
        return Validate();
    }
    public static object Validate()
    {
        Ready();var root=Fort().Find(Root);Need(root!=null,"Fort art missing.");
        var meshes=root.GetComponentsInChildren<MeshFilter>();
        Need(meshes.Length>=20&&meshes.All(m=>m.gameObject.layer==2&&m.sharedMesh!=null&&m.sharedMesh.normals.All(n=>n.sqrMagnitude>.8f)),"Incomplete fort mesh art.");
        Need(root.GetComponentsInChildren<Collider>(true).Length==0&&root.GetComponentsInChildren<MonoBehaviour>(true).Length==0,"Fort art contains gameplay components.");
        return new{marker="DICEFREE_FORT_ART_OK",batches=meshes.Length,triangles=meshes.Sum(m=>m.sharedMesh.triangles.Length/3),collidersAdded=0};
    }
    static Batch CourtSurface()
    {
        var surface=new Batch();var collider=Fort().Find("Raised upper stronghold terrace").GetComponent<Collider>();
        Vector3 At(int x,int z){Need(collider.Raycast(new Ray(new Vector3(x,30,z),Vector3.down),out var hit,40),"Missing terrace surface at "+x+","+z);return hit.point+Vector3.up*.025f;}
        // The original large terrace triangles interpolate their corner heights.
        // Sample that actual collision surface, rather than the analytic valley height.
        for(int z=470;z<502;z++)for(int x=234;x<306;x++)
            surface.Quad(At(x,z),At(x,z+1),At(x+1,z+1),At(x+1,z));
        return surface;
    }
    public static object RefreshCourtSurface()
    {
        Ready();var path=Folder+"/Chieftain court and longhouse Packed earth.asset";
        var destination=AssetDatabase.LoadAssetAtPath<Mesh>(path);Need(destination!=null,"Court skin missing.");
        var mesh=CourtSurface().Mesh("Chieftain court packed earth");EditorUtility.CopySerialized(mesh,destination);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(destination);AssetDatabase.SaveAssetIfDirty(destination);return Validate();
    }
    sealed class Batch
    {
        readonly List<Vector3> vertices=new();readonly List<int> indices=new();readonly List<Vector2> uv=new();
        public bool Empty=>vertices.Count==0;
        public void Tri(Vector3 a,Vector3 b,Vector3 c){int n=vertices.Count;vertices.AddRange(new[]{a,b,c});indices.AddRange(new[]{n,n+1,n+2});uv.AddRange(new[]{new Vector2(a.x,a.z)*.1f,new Vector2(b.x,b.z)*.1f,new Vector2(c.x,c.z)*.1f});}
        public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){Tri(a,b,c);Tri(a,c,d);}
        public Mesh Mesh(string name){var m=new Mesh{name=name,indexFormat=IndexFormat.UInt32};m.SetVertices(vertices);m.SetUVs(0,uv);m.SetTriangles(indices,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
    }
    sealed class Art
    {
        readonly Dictionary<string,Batch[]> sections=new();Batch[] batch;readonly System.Random rng=new(6110401);
        float R(float a,float b)=>Mathf.Lerp(a,b,(float)rng.NextDouble());
        void Section(string name){batch=Enumerable.Range(0,Names.Length).Select(_=>new Batch()).ToArray();sections.Add(name,batch);}
        void Box(Vector3 p,Vector3 s,int mat,Quaternion? rotation=null)
        {
            var q=rotation??Quaternion.identity;Vector3 V(float x,float y,float z)=>p+q*Vector3.Scale(s,new Vector3(x,y,z))*.5f;
            var a=V(-1,-1,-1);var b=V(1,-1,-1);var c=V(1,1,-1);var d=V(-1,1,-1);var e=V(-1,-1,1);var f=V(1,-1,1);var g=V(1,1,1);var h=V(-1,1,1);
            batch[mat].Quad(a,d,c,b);batch[mat].Quad(f,g,h,e);batch[mat].Quad(e,h,d,a);batch[mat].Quad(b,c,g,f);batch[mat].Quad(d,h,g,c);batch[mat].Quad(e,a,b,f);
        }
        void Beam(Vector3 a,Vector3 b,float width,float depth,int mat)=>Box((a+b)*.5f,new Vector3(width,(a-b).magnitude,depth),mat,Quaternion.FromToRotation(Vector3.up,b-a));
        void Log(Vector3 a,Vector3 b,float radius,int mat,bool pointed=false)
        {
            var rot=Quaternion.FromToRotation(Vector3.up,b-a);float length=(b-a).magnitude;
            Vector3 P(float h,float r,int i)=>a+rot*new Vector3(Mathf.Cos(i*Mathf.PI/3)*r,h,Mathf.Sin(i*Mathf.PI/3)*r);
            for(int i=0;i<6;i++)
            {
                var p=P(0,radius,i);var q=P(0,radius,i+1);var s=P(length*(pointed?.88f:1),radius*.87f,i);var t=P(length*(pointed?.88f:1),radius*.87f,i+1);
                batch[i%3==0?1:mat].Quad(p,s,t,q);
                if(pointed)batch[2].Tri(s,b,t);else batch[2].Tri(b,s,t);
            }
        }
        void Rock(Vector3 p,Vector3 scale,float yaw=0)
        {
            var rot=Quaternion.Euler(0,yaw,0);Vector3 V(float a,float y,float r)=>p+rot*new Vector3(Mathf.Cos(a)*scale.x*r,y*scale.y,Mathf.Sin(a)*scale.z*r);
            for(int i=0;i<7;i++)
            {
                float a=i*Mathf.PI*2/7,b=(i+1)*Mathf.PI*2/7;
                var p0=V(a,-.07f,1);var q0=V(b,-.07f,1);var p1=V(a,.38f,.94f);var q1=V(b,.38f,.94f);var p2=V(a,.82f,.68f);var q2=V(b,.82f,.68f);
                batch[3].Quad(p0,p1,q1,q0);batch[4].Quad(p1,p2,q2,q1);batch[3].Tri(p2,p+Vector3.up*scale.y,q2);
            }
        }
        void Standard(Vector3 p,float h=3.5f)
        {
            Log(p,p+Vector3.up*h,.10f,0,true);
            var a=p+new Vector3(.1f,h-.35f,.04f);var b=a+Vector3.right*1.3f;var c=b+new Vector3(0,-1.75f,.08f);var d=a+new Vector3(0,-1.4f,0);
            batch[6].Quad(a,b,c,d);batch[6].Quad(d,c,b,a);
            var top=a+new Vector3(.6f,-.25f,-.015f);var low=top+Vector3.down*.95f;
            batch[7].Quad(top,top+new Vector3(.21f,-.5f,0),low,top+new Vector3(-.21f,-.5f,0));
        }
        void Tower(Vector3 p)
        {
            // Existing solid foundation, legs and fighting deck remain authoritative.
            for(int side=-1;side<=1;side+=2)
            {
                Beam(p+new Vector3(side*2.72f,1,-2.72f),p+new Vector3(side*2.72f,5.7f,2.72f),.25f,.24f,1);
                Beam(p+new Vector3(-2.72f,1,side*2.72f),p+new Vector3(2.72f,5.7f,side*2.72f),.25f,.24f,1);
                for(int i=0;i<6;i++)Box(p+new Vector3(-3+i*1.18f,5.92f,side*3.2f),new Vector3(1.1f,.16f,.5f),2);
            }
            for(int i=0;i<12;i++)Box(p+new Vector3(-3.3f+i*.6f,6.02f,0),new Vector3(.56f,.12f,6.8f),i%3==0?2:1);
            for(int side=-1;side<=1;side+=2)
            {
                var a=p+new Vector3(-4.2f,8.6f,side*4.1f);var b=p+new Vector3(4.2f,8.6f,side*4.1f);var c=p+new Vector3(4.2f,10.2f,0);var d=p+new Vector3(-4.2f,10.2f,0);
                batch[0].Quad(a,b,c,d);batch[0].Quad(d,c,b,a);
                for(int i=0;i<9;i++)Beam(p+new Vector3(-4+i,8.62f,side*4.1f),p+new Vector3(-4+i,10.23f,0),.10f,.10f,1);
                Beam(p+new Vector3(-4.2f,8.6f,side*4.1f),p+new Vector3(4.2f,8.6f,side*4.1f),.24f,.24f,2);
            }
            Beam(p+new Vector3(-4.4f,10.3f,0),p+new Vector3(4.4f,10.3f,0),.25f,.3f,5);
            Standard(p+new Vector3(-2,6,-3.7f),2.6f);
        }
        public void Design(Transform fort)
        {
            Section("Gate and stockade");
            var original=fort.GetComponentsInChildren<MeshRenderer>().ToArray();
            foreach(var renderer in original)
            {
                var t=renderer.transform;
                if(t.name.EndsWith(" - timber palisade",StringComparison.Ordinal))
                {
                    renderer.enabled=false;
                    float bottom=renderer.bounds.min.y,top=renderer.bounds.max.y;
                    Log(new Vector3(t.position.x,bottom,t.position.z),new Vector3(t.position.x,top+.38f,t.position.z),.31f,0,true);
                    for(int j=0;j<2;j++)Box(new Vector3(t.position.x,bottom+1.3f+j*2,t.position.z),new Vector3(.66f,.14f,.66f),5);
                }
                else if(t.name.EndsWith(" - sharpened crown",StringComparison.Ordinal))renderer.enabled=false;
                else if(t.name.Contains("fieldstone foot")||t.name.Contains("support pier")||t.name=="Raised upper stronghold terrace")
                    renderer.sharedMaterial=Material(3);
                else if(t.name.Contains("timber leg")||t.name.Contains("fighting deck")||t.name.Contains("parapet"))renderer.sharedMaterial=Material(0);
            }
            foreach(var point in new[]{new Vector2(258,438),new Vector2(278,438),new Vector2(234,497),new Vector2(306,497)})
                Tower(Ground(point.x,point.y,point.y>490?5.2f:0));
            foreach(var (a,b) in new[]{(new Vector2(220,438),new Vector2(253,438)),(new Vector2(283,438),new Vector2(316,438)),(new Vector2(220,438),new Vector2(220,469)),(new Vector2(316,438),new Vector2(316,469)),(new Vector2(234,497),new Vector2(306,497)),(new Vector2(234,474),new Vector2(234,497)),(new Vector2(306,474),new Vector2(306,497))})
            {
                float level=a.y>470?5.2f:0;
                for(int j=0;j<2;j++)Log(Ground(a.x,a.y,level+1.35f+j*1.9f),Ground(b.x,b.y,level+1.35f+j*1.9f),.14f,1);
            }
            // Header is above tower height and leaves the entire original open gate clear.
            Beam(Ground(262,438,8.3f),Ground(274,438,8.3f),.55f,.65f,0);
            for(int i=0;i<5;i++)Box(Ground(264+i*2,437.7f,8.3f),new Vector3(.20f,.66f,.1f),5);
            Standard(Ground(264,433),3.2f);Standard(Ground(273,433),3.2f);
            for(int i=0;i<16;i++)
            {
                float z=414+i*1.2f;
                foreach(int sign in new[]{-1,1})Rock(Ground(270+sign*3.1f,z),new Vector3(R(.22f,.5f),R(.12f,.29f),R(.2f,.5f)),R(0,360));
            }

            Section("Bridge and cliff strata");
            var deck=fort.Find("Raised timber bridge deck over lower road").GetComponent<MeshFilter>();
            // Top surface is copied from its actual mesh endpoints; rails and supports stay outside the lane.
            var dv=deck.sharedMesh.vertices;
            var left=deck.transform.TransformPoint(dv[0]);var right=deck.transform.TransformPoint(dv[2]);
            for(float x=242.3f;x<294;x+=.78f)
            {
                float y=Mathf.Lerp(left.y,right.y,(x-242)/52)+.03f;
                Box(new Vector3(x,y,460),new Vector3(.72f,.08f,5.12f),((int)(x*10))%3==0?2:1);
                for(int s=-1;s<=1;s+=2)Box(new Vector3(x,y+.055f,460+s*2.1f),new Vector3(.07f,.025f,.09f),5);
            }
            foreach(var rampName in new[]{"West ramp to upper timber bridge","East ramp to upper timber bridge","Walkable ramp through the cliff cut"})
            {
                var f=fort.Find(rampName).GetComponent<MeshFilter>();var v=f.sharedMesh.vertices.Select(p=>f.transform.TransformPoint(p)).ToArray();
                var a=(v[0]+v[1])*.5f;var b=(v[2]+v[3])*.5f;
                float length=(a-b).magnitude;int steps=Mathf.CeilToInt(length/.7f);
                for(int i=0;i<=steps;i++)
                {
                    var p=Vector3.Lerp(a,b,i/(float)steps)+Vector3.up*.045f;
                    Box(p,new Vector3(4.05f,.11f,.58f),i%3==0?2:1,Quaternion.FromToRotation(Vector3.forward,(b-a).normalized));
                }
                for(int side=-1;side<=1;side+=2)
                {
                    var offset=Vector3.right*side*2.04f;
                    for(int i=0;i<=4;i++){var p=Vector3.Lerp(a,b,i/4f)+offset;Log(p,p+Vector3.up*1.18f,.10f,0);}
                    Beam(a+offset+Vector3.up*.9f,b+offset+Vector3.up*.9f,.11f,.12f,1);
                }
            }
            for(int side=-1;side<=1;side+=2)
            {
                for(float x=243;x<=293;x+=3.8f){var p=Ground(x,460+side*2.45f,5.2f);Log(p,p+Vector3.up*1.55f,.13f,0,true);}
                Log(Ground(242,460+side*2.45f,6.5f),Ground(294,460+side*2.45f,6.5f),.14f,1);
            }
            for(int i=0;i<4;i++)
            {
                float x=new[]{244f,252f,284f,292f}[i];
                // The central 12m underpass has no decorative supports.
                for(int side=-1;side<=1;side+=2)
                    Beam(Ground(x,460+side*2.1f,.4f),Ground(x+2,460+side*2.1f,4.7f),.35f,.35f,0);
            }
            foreach(var renderer in original.Where(r=>r.name=="Natural cliff face below upper terrace"))
            {
                renderer.enabled=false;
                var p=renderer.transform.position;
                for(int i=0;i<3;i++)Rock(new Vector3(p.x-2.1f+i*2.1f,Height(p.x,472),472.1f),new Vector3(1.28f,4.9f+R(-.2f,.2f),.65f),R(-8,8));
            }
            // Dress only the rock shoulders already outside the fort's traversable interior.
            foreach(var renderer in original.Where(r=>r.name=="Natural northern cliff spine"||r.name.EndsWith("mountain shoulder",StringComparison.Ordinal)))
            {
                renderer.enabled=false;
                var b=renderer.bounds;
                Rock(new Vector3(b.center.x,b.min.y,b.center.z),new Vector3(b.extents.x,b.size.y,b.extents.z),R(0,360));
            }

            Section("Chieftain court and longhouse");
            // Calm textured soil, keeping all telegraph space unoccupied.
            batch[8]=CourtSurface();
            var house=fort.Find("Chieftain longhouse - timber hall");var hp=house.position;
            foreach(var renderer in original.Where(r=>r.name.StartsWith("Chieftain longhouse",StringComparison.Ordinal)))renderer.sharedMaterial=Material(renderer.name.Contains("foundation")?3:0);
            for(int i=-5;i<=5;i++)
            {
                var p=new Vector3(hp.x+i*1.6f,hp.y,hp.z-6.6f);
                Box(p,new Vector3(1.5f,6.4f,.2f),i%3==0?1:0);
            }
            for(int s=-1;s<=1;s+=2)
            {
                var a=hp+new Vector3(-10.7f,3.5f,s*7.8f);var b=hp+new Vector3(10.7f,3.5f,s*7.8f);var c=hp+new Vector3(10.7f,6.4f,0);var d=hp+new Vector3(-10.7f,6.4f,0);
                batch[0].Quad(a,b,c,d);batch[0].Quad(d,c,b,a);
                for(int i=-10;i<=10;i++)Beam(hp+new Vector3(i,3.55f,s*7.8f),hp+new Vector3(i,6.45f,0),.11f,.12f,1);
            }
            Standard(Ground(266,496,5.2f),4.2f);Standard(Ground(299,496,5.2f),4.2f);
            foreach(var p in new[]{new Vector2(269,497),new Vector2(296,498),new Vector2(303,487)})
                for(int i=0;i<4;i++)Rock(Ground(p.x+R(-1,1),p.y+R(-1,1),5.2f),new Vector3(.4f,.16f,.3f),R(0,360));
        }
        readonly Dictionary<int,Material> materials=new();
        Material Material(int i)
        {
            if(materials.TryGetValue(i,out var m))return m;
            m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=Names[i]};m.SetColor("_BaseColor",Colors[i]);m.SetFloat("_Smoothness",.07f);m.SetFloat("_SpecularHighlights",0);m.SetFloat("_EnvironmentReflections",0);
            if(i==8)
            {
                const int size=512;var tex=new Texture2D(size,size,TextureFormat.RGB24,true);var pixels=new Color[size*size];
                for(int z=0;z<size;z++)for(int x=0;x<size;x++){float broad=Mathf.PerlinNoise(x*.025f+31,z*.025f+17),grain=Mathf.PerlinNoise(x*.38f+12,z*.38f+7);pixels[z*size+x]=Color.Lerp(new Color(.70f,.65f,.52f),Color.white,broad*.6f+grain*.25f);}
                tex.SetPixels(pixels);tex.Apply();string p=Folder+"/Court soil.png";File.WriteAllBytes(p,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(p);importer.wrapMode=TextureWrapMode.Repeat;importer.mipmapEnabled=true;importer.anisoLevel=4;importer.SaveAndReimport();m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(p));
            }
            AssetDatabase.CreateAsset(m,Folder+"/"+Names[i]+".mat");materials.Add(i,m);return m;
        }
        public void Install(Transform root)
        {
            foreach(var pair in sections)
            {
                var section=new GameObject(pair.Key).transform;section.SetParent(root,false);section.gameObject.layer=2;
                for(int i=0;i<Names.Length;i++)if(!pair.Value[i].Empty)
                {
                    var mesh=pair.Value[i].Mesh(pair.Key+" "+Names[i]);var path=Folder+"/"+pair.Key+" "+Names[i]+".asset";AssetDatabase.CreateAsset(mesh,path);
                    var go=new GameObject(Names[i]);go.layer=2;go.transform.SetParent(section,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
                    var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=Material(i);r.shadowCastingMode=i==8?ShadowCastingMode.Off:ShadowCastingMode.On;
                }
            }
        }
    }
    public static object CaptureAfter()=>Capture("after");
    public static object EnsureEffectMaterials()
    {
        const string path="Assets/_DiceFree/Resources/BriarbackFx";
        if(!AssetDatabase.IsValidFolder(path))AssetDatabase.CreateFolder("Assets/_DiceFree/Resources","BriarbackFx");
        foreach(var type in new[]{"Lit","Unlit","LitTransparent","UnlitTransparent"})
        {
            string p=path+"/"+type+".mat";
            var existing=AssetDatabase.LoadAssetAtPath<Material>(p);
            var shader=Shader.Find("Universal Render Pipeline/"+type.Replace("Transparent",""));Need(shader!=null,"Missing FX shader "+type);
            var material=existing!=null?existing:new Material(shader){name="Briarback "+type+" effects"};material.SetFloat("_Smoothness",.15f);
            if(type.EndsWith("Transparent",StringComparison.Ordinal))
            {
                if(material.HasProperty("_BlendModePreserveSpecularLighting"))material.SetFloat("_BlendModePreserveSpecularLighting",0);
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.SetFloat("_Surface",1);material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);material.SetFloat("_ZWrite",0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.SetOverrideTag("RenderType","Transparent");material.renderQueue=3000;
            }
            if(existing==null)AssetDatabase.CreateAsset(material,p);else EditorUtility.SetDirty(material);
        }
        AssetDatabase.SaveAssets();return new{resourceMaterials=4};
    }
    public static object CaptureBefore()=>Capture("before");
    static object Capture(string phase)
    {
        Ready();var game=Object.FindAnyObjectByType<ExplorationCamera>();var go=new GameObject("Temporary fort review camera");var target=new GameObject("Temporary fort review target");var rt=new RenderTexture(1600,900,24);var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);var old=RenderTexture.active;string output="VisualBaselines/BriarbackFort";Directory.CreateDirectory(output);
        try{var c=go.AddComponent<Camera>();c.CopyFrom(game.GetComponent<Camera>());c.targetTexture=rt;foreach(var p in new[]{("gate",new Vector3(270,0,430)),("bridge",new Vector3(270,0,452)),("arena",new Vector3(282,5.2f,482))}){target.transform.position=p.Item2;game.GetFollowPose(target.transform,out var pos,out var rot,out var fov);c.transform.SetPositionAndRotation(pos,rot);c.fieldOfView=fov;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(output+"/"+p.Item1+"-"+phase+".png",tex.EncodeToPNG());}return new{phase,output};}
        finally{RenderTexture.active=old;Object.DestroyImmediate(go);Object.DestroyImmediate(target);Object.DestroyImmediate(tex);rt.Release();Object.DestroyImmediate(rt);}
    }
}
