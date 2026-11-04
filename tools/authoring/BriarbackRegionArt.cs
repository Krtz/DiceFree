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

// Original, seeded regional scenery. Original ground, roads, trees and collision remain intact.
public static class BriarbackRegionArt
{
    const string Folder="Assets/_DiceFree/Art/World/BriarbackRegion";
    const string Group="Briarback connected woodland and worn trails";
    const string Recovery="Assets/_DiceFree/Settings/Recovery/Cornberg-before-region-art-20261104.unity";
    static readonly string[] Roads={"Briarback northern detour","Harrowtusk eastern camp trail","Briarback western scouting loop","Briarback circular warband road","North approach to the open stronghold gate"};
    static readonly Vector2[] Sites={new(230,378),new(286,378),new(230,426),new(286,426)};
    static void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
    static Scene Ready(){var s=SceneManager.GetActiveScene();Need(s.name=="Cornberg"&&!s.isDirty&&!EditorApplication.isPlayingOrWillChangePlaymode,"Clean Cornberg Edit Mode required.");return s;}
    static float H(float x,float z)=>.22f*Mathf.Sin(x*.09f)*Mathf.Sin(z*.08f);
    static float Smooth(float a,float b,float v)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,v));
    sealed class Surface
    {
        readonly List<Vector3> v=new();readonly List<int> t=new();readonly List<Vector2> uv=new();
        public bool Empty=>v.Count==0;
        public void Tri(Vector3 a,Vector3 b,Vector3 c,Vector2 ua=default,Vector2 ub=default,Vector2 uc=default)
        {int n=v.Count;v.AddRange(new[]{a,b,c});t.AddRange(new[]{n,n+1,n+2});uv.AddRange(new[]{ua,ub,uc});}
        public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector2 ua=default,Vector2 ub=default,Vector2 uc=default,Vector2 ud=default){Tri(a,b,c,ua,ub,uc);Tri(a,c,d,ua,uc,ud);}
        public Mesh Bake(string name){var m=new Mesh{name=name,indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetTriangles(t,0);m.SetUVs(0,uv);m.RecalculateNormals();m.RecalculateBounds();return m;}
    }
    static Material Material(string name,Color color,bool alpha=false,Texture texture=null,int queue=3000)
    {
        var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.035f);m.SetFloat("_SpecularHighlights",0);m.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");m.SetFloat("_EnvironmentReflections",0);m.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        if(texture!=null)m.SetTexture("_BaseMap",texture);
        if(alpha){m.SetFloat("_Surface",1);m.SetFloat("_BlendModePreserveSpecularLighting",0);m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.DisableKeyword("_ALPHAPREMULTIPLY_ON");m.SetOverrideTag("RenderType","Transparent");m.renderQueue=queue;m.SetShaderPassEnabled("ShadowCaster",false);}
        AssetDatabase.CreateAsset(m,Folder+"/"+name+".mat");return m;
    }
    static Texture2D Texture(string name,int w,int h,Func<float,float,Color> pixel,bool repeat)
    {
        var tex=new Texture2D(w,h,TextureFormat.RGBA32,true);var p=new Color32[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++)p[y*w+x]=pixel(x/(w-1f),y/(h-1f));tex.SetPixels32(p);tex.Apply();string path=Folder+"/"+name+".png";File.WriteAllBytes(path,tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.alphaIsTransparency=true;importer.wrapMode=repeat?TextureWrapMode.Repeat:TextureWrapMode.Clamp;importer.anisoLevel=4;importer.maxTextureSize=Math.Max(w,h);importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static void Install(Transform parent,string name,Surface surface,Material material,bool shadow=true)
    {
        if(surface.Empty)return;var mesh=surface.Bake(name);AssetDatabase.CreateAsset(mesh,Folder+"/"+name+".asset");var go=new GameObject(name);go.layer=2;go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=shadow?ShadowCastingMode.On:ShadowCastingMode.Off;
    }
    public static object Author()
    {
        var scene=Ready();var region=GameObject.Find("14 - Briarback quilboar frontier").transform;
        Need(region.Find(Group)==null&&!AssetDatabase.IsValidFolder(Folder)&&!File.Exists(Recovery),"Regional art already exists.");
        Need(EditorSceneManager.SaveScene(scene,Recovery,true),"Recovery failed.");AssetDatabase.CreateFolder("Assets/_DiceFree/Art/World","BriarbackRegion");
        var root=new GameObject(Group).transform;root.SetParent(region,false);root.gameObject.layer=2;
        var roads=region.GetComponentsInChildren<MeshFilter>().Where(f=>Roads.Contains(f.name)).ToArray();Need(roads.Length==5,"Expected five original regional trail meshes.");
        var roadTexture=Texture("Rutted trail shoulders",512,256,(u,v)=>{
            float across=Mathf.Abs(u-.5f)*2,noise=Mathf.PerlinNoise(u*19+51,v*12+31),fine=Mathf.PerlinNoise(u*113+2,v*73+8);
            var c=new Color(.62f,.54f,.38f)*(.85f+noise*.18f+fine*.09f);
            float rut=Mathf.Exp(-Mathf.Pow((across-.34f)*12,2))*.07f;c*=1-rut;
            c.a=1-Smooth(.59f,.99f,across+(noise-.5f)*.06f);return c;
        },true);
        var roadMaterial=Material("Trampled trail",Color.white,true,roadTexture,2980);
        var grass=new Surface();var dry=new Surface();var ferns=new Surface();var rubble=new Surface();var strata=new Surface();var wood=new Surface();
        var rng=new System.Random(6110408);float R(float a,float b)=>Mathf.Lerp(a,b,(float)rng.NextDouble());
        Vector3 At(float x,float z,float up=.02f)=>new(x,H(x,z)+up,z);
        bool SiteClear(Vector3 p)=>Sites.All(s=>Vector2.Distance(new Vector2(p.x,p.z),s)>19.5f);
        bool Clear(Vector3 p)=>SiteClear(p)&&!Physics.CheckSphere(p+Vector3.up*.8f,.6f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore);
        void Blade(Vector3 p,float height,float yaw,Surface s)
        {
            var dir=new Vector3(Mathf.Cos(yaw),0,Mathf.Sin(yaw));var side=new Vector3(-dir.z,0,dir.x)*.075f;var tip=p+Vector3.up*height+dir*height*.4f;
            s.Tri(p-side,tip,p+side);s.Tri(p+side,tip,p-side);
        }
        void Rock(Vector3 p,Vector3 size,float angle=0)
        {
            var q=Quaternion.Euler(0,angle,0);Vector3 P(int i,float y,float r)=>p+q*Vector3.Scale(size,new Vector3(Mathf.Cos(i*Mathf.PI/3)*r,y,Mathf.Sin(i*Mathf.PI/3)*r));
            for(int i=0;i<6;i++){var a=P(i,0,.5f);var b=P(i+1,0,.5f);var c=P(i,.42f,.48f);var d=P(i+1,.42f,.48f);var e=P(i,.85f,.34f);var f=P(i+1,.85f,.34f);rubble.Quad(a,c,d,b);strata.Quad(c,e,f,d);rubble.Tri(e,p+Vector3.up*size.y,f);}
        }
        foreach(var road in roads)
        {
            var surface=new Surface();var vertices=road.sharedMesh.vertices.Select(v=>road.transform.TransformPoint(v)).ToArray();Need(vertices.Length%4==0,"Unexpected trail topology.");float distance=0;
            for(int i=0;i<vertices.Length;i+=4)
            {
                var a=(vertices[i]+vertices[i+1])*.5f;var b=(vertices[i+2]+vertices[i+3])*.5f;var side=(vertices[i+1]-vertices[i]).normalized;float half=Vector3.Distance(vertices[i],vertices[i+1])*.5f;float length=Vector3.Distance(a,b);
                // Match the old road's sampled height plus a thin skin; shoulders descend to terrain.
                Vector3 Edge(Vector3 p,float x){var r=p+side*x;bool start=(p-a).sqrMagnitude<.00001f;int n=start?i:i+2;float roadY=Mathf.LerpUnclamped(vertices[n].y,vertices[n+1].y,(x+half)/(2*half));float y=Mathf.Abs(x)<=half?roadY+.014f:H(r.x,r.z)+.018f;return new Vector3(r.x,y,r.z);}
                var widths=new[]{-half-1.55f,-half,0,half,half+1.55f};var uv=new[]{0f,.23f,.5f,.77f,1f};
                for(int k=0;k<4;k++)surface.Quad(Edge(a,widths[k]),Edge(a,widths[k+1]),Edge(b,widths[k+1]),Edge(b,widths[k]),new(uv[k],distance/5),new(uv[k+1],distance/5),new(uv[k+1],(distance+length)/5),new(uv[k],(distance+length)/5));
                distance+=length;
                if(i%12==0)
                {
                    for(int sign=-1;sign<=1;sign+=2)
                    {
                        var p=At(a.x+side.x*sign*(half+R(2,4)),a.z+side.z*sign*(half+R(2,4)));
                        if(p.z<198||p.z>430||!Clear(p))continue;
                        for(int n=0;n<5;n++){var s=p+new Vector3(R(-.35f,.35f),0,R(-.35f,.35f));s.y=H(s.x,s.z)+.02f;Blade(s,R(.3f,.7f),R(0,6.28f),n%3==0?dry:grass);}
                        if(i%36==0)Rock(p+Vector3.right*.7f,new Vector3(R(.2f,.6f),R(.13f,.32f),R(.2f,.6f)),R(0,360));
                        if(i%24==0)for(int n=0;n<6;n++){float angle=n*Mathf.PI/3;var dir=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));var right=new Vector3(-dir.z,0,dir.x);var end=p+dir*.7f+Vector3.up*.25f;ferns.Tri(p,end+right*.16f,end-right*.16f);ferns.Tri(p,end-right*.16f,end+right*.16f);}
                    }
                }
            }
            Install(root,road.name+" worn surface",surface,roadMaterial,false);
        }
        var woodlandTexture=Texture("Woodland floor",2048,2048,(u,v)=>{
            float x=196+144*u,z=246+188*v;float n=Mathf.PerlinNoise(x*.15f+6,z*.15f+82),grain=Mathf.PerlinNoise(x*4.3f,z*4.3f+17);
            var c=Color.Lerp(new Color(.60f,.62f,.39f),new Color(.68f,.65f,.44f),n)*(.91f+grain*.13f);
            float edge=Mathf.Min(x-196,340-x,z-246,434-z);c.a=Smooth(0,10,edge)*.82f;
            foreach(var site in Sites)c.a*=Smooth(16,21,Vector2.Distance(new Vector2(x,z),site));return c;
        },false);
        var floor=new Surface();
        for(int z=246;z<434;z+=2)for(int x=196;x<340;x+=2)
            floor.Quad(At(x,z,.019f),At(x,z+2,.019f),At(x+2,z+2,.019f),At(x+2,z,.019f),new((x-196)/144f,(z-246)/188f),new((x-196)/144f,(z+2-246)/188f),new((x+2-196)/144f,(z+2-246)/188f),new((x+2-196)/144f,(z-246)/188f));
        Install(root,"Dry woodland floor",floor,Material("Dry woodland blend",Color.white,true,woodlandTexture,2970),false);
        // Replace only the old spherical boundary visuals. Every original collider stays in place.
        var originals=region.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Wooded cliff boundary"||r.name=="Northern wooded ridge"||r.name=="Grounded mossy rock").ToArray();
        foreach(var r in originals){var b=r.bounds;Rock(new Vector3(b.center.x,b.min.y,b.center.z),b.size*.99f,r.transform.eulerAngles.y);r.enabled=false;}
        // Dress existing camp roofs/stockades with an earthier palette and low original stones.
        foreach(var camp in region.Cast<Transform>().Where(t=>new[]{"Southern raider camp","Western spear camp","Upper totem camp","Eastern warband stockade","Harrowtusk's stockade"}.Contains(t.name)))
        {
            var renderers=camp.GetComponentsInChildren<MeshRenderer>();var hide=Material("Camp roof "+camp.name,new Color(.31f,.23f,.13f));foreach(var r in renderers)if(r.name.Contains("roof")||r.name=="Quilboar hide shelter")r.sharedMaterial=hide;
            var center=renderers.Aggregate(new Bounds(renderers[0].bounds.center,Vector3.zero),(b,r)=>{b.Encapsulate(r.bounds);return b;}).center;
            for(int i=0;i<12;i++){var p=At(center.x+R(-8,8),center.z+R(-8,8));if(Clear(p))Rock(p,new Vector3(.35f,.16f,.3f),R(0,360));}
        }
        Install(root,"Meadow tufts",grass,Material("Woodland green",new(.28f,.33f,.15f)),false);
        Install(root,"Dry trail grass",dry,Material("Dry ochre grass",new(.46f,.40f,.24f)),false);
        Install(root,"Trail ferns",ferns,Material("Fern green",new(.23f,.33f,.17f)),false);
        Install(root,"Weathered boundary rock",rubble,Material("Grey green rock",new(.28f,.29f,.23f)));
        Install(root,"Broken stone strata",strata,Material("Ochre stone seams",new(.42f,.39f,.29f)));
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Scene save failed.");
        FeatherTrailJunctions();
        return Validate();
    }
    public static object Validate()
    {
        Ready();var root=GameObject.Find("14 - Briarback quilboar frontier").transform.Find(Group);Need(root!=null,"Regional art missing.");var meshes=root.GetComponentsInChildren<MeshFilter>();
        Need(root.GetComponentsInChildren<Collider>(true).Length==0&&root.GetComponentsInChildren<MonoBehaviour>(true).Length==0,"New scenery gained gameplay components.");
        Need((meshes.Length==11||meshes.Length==12)&&meshes.All(f=>f.gameObject.layer==2&&f.sharedMesh.normals.All(n=>n.sqrMagnitude>.9f)),"Regional art references or normals invalid.");
        return new{marker="DICEFREE_REGION_ART_OK",meshBatches=meshes.Length,triangles=meshes.Sum(m=>m.sharedMesh.triangles.Length/3),collidersAdded=0};
    }
    public static object RefreshCampPalette()
    {
        var scene=Ready();var region=GameObject.Find("14 - Briarback quilboar frontier").transform;int shelters=0;
        foreach(var camp in region.Cast<Transform>().Where(t=>new[]{"Southern raider camp","Western spear camp","Upper totem camp","Eastern warband stockade","Harrowtusk's stockade"}.Contains(t.name)))
        {
            string path=Folder+"/Camp roof "+camp.name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null)material=Material("Camp roof "+camp.name,new Color(.31f,.23f,.13f));
            foreach(var renderer in camp.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name=="Quilboar hide shelter")){renderer.sharedMaterial=material;shelters++;}
        }
        Need(shelters==15,"Expected the fifteen existing camp shelters.");EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Scene save failed.");return new{shelters,originalCollisionPreserved=true};
    }
    public static object FeatherTrailJunctions()
    {
        var scene=Ready();var region=GameObject.Find("14 - Briarback quilboar frontier").transform;var root=region.Find(Group);Need(root.Find("Feathered trail junctions")==null,"Junction artwork already exists.");
        var terrain=region.GetComponentsInChildren<MeshCollider>().Single(c=>c.name=="Continuous Briarback walkable terrain");
        var roads=region.GetComponentsInChildren<MeshFilter>().Where(f=>Roads.Contains(f.name)).ToArray();
        var roadTriangles=roads.SelectMany(f=>{var m=f.sharedMesh;var v=m.vertices.Select(p=>f.transform.TransformPoint(p)).ToArray();return m.triangles.Select((n,i)=>i).Where(i=>i%3==0).Select(i=>new[]{v[m.triangles[i]],v[m.triangles[i+1]],v[m.triangles[i+2]]});}).ToArray();
        float SurfaceY(Vector3 p)
        {
            float y=H(p.x,p.z);if(terrain.Raycast(new Ray(new Vector3(p.x,20,p.z),Vector3.down),out var hit,30))y=hit.point.y;
            foreach(var tri in roadTriangles)
            {
                Vector2 a=new(tri[0].x,tri[0].z),b=new(tri[1].x,tri[1].z),c=new(tri[2].x,tri[2].z),q=new(p.x,p.z);float d=(b.y-c.y)*(a.x-c.x)+(c.x-b.x)*(a.y-c.y);if(Mathf.Abs(d)<.00001f)continue;
                float u=((b.y-c.y)*(q.x-c.x)+(c.x-b.x)*(q.y-c.y))/d,v=((c.y-a.y)*(q.x-c.x)+(a.x-c.x)*(q.y-c.y))/d,w=1-u-v;if(u>=-.001f&&v>=-.001f&&w>=-.001f)y=Mathf.Max(y,tri[0].y*u+tri[1].y*v+tri[2].y*w);
            }
            return y+.035f;
        }
        foreach(var filter in root.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.EndsWith(" worn surface",StringComparison.Ordinal)))
        {var mesh=filter.sharedMesh;var vertices=mesh.vertices;for(int i=0;i<vertices.Length;i++)vertices[i].y=Mathf.Max(vertices[i].y,SurfaceY(vertices[i]));mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssetIfDirty(mesh);}
        var texture=Texture("Trail junction feathering",512,512,(u,v)=>{float radius=Vector2.Distance(new(u,v),new(.5f,.5f))*2,n=Mathf.PerlinNoise(u*19+51,v*19+31),grain=Mathf.PerlinNoise(u*113,v*113+8);var color=new Color(.62f,.54f,.38f)*(.85f+n*.18f+grain*.09f);color.a=(1-Smooth(.65f,1,radius+(n-.5f)*.08f))*.92f;return color;},false);
        var junctions=new Surface();var seen=new System.Collections.Generic.HashSet<string>();int count=0;
        foreach(var road in roads)
        {
            var points=road.sharedMesh.vertices.Select(p=>road.transform.TransformPoint(p)).ToArray();
            for(int i=0;i<points.Length;i+=4)foreach(int k in new[]{i,i+2})
            {
                var center=(points[k]+points[k+1])*.5f;string key=Mathf.RoundToInt(center.x*10)+":"+Mathf.RoundToInt(center.z*10);if(!seen.Add(key))continue;float radius=Vector3.Distance(points[k],points[k+1])*.5f+1.4f;
                // A small grid follows the real ground and road triangles instead of bridging their slopes.
                const int steps=8;for(int z=0;z<steps;z++)for(int x=0;x<steps;x++)
                {
                    Vector3 P(int xx,int zz){var p=center+new Vector3((xx/(float)steps*2-1)*radius,0,(zz/(float)steps*2-1)*radius);p.y=SurfaceY(p)+.008f;return p;}
                    junctions.Quad(P(x,z),P(x,z+1),P(x+1,z+1),P(x+1,z),new(x/(float)steps,z/(float)steps),new(x/(float)steps,(z+1)/(float)steps),new((x+1)/(float)steps,(z+1)/(float)steps),new((x+1)/(float)steps,z/(float)steps));
                }
                count++;
            }
        }
        Install(root,"Feathered trail junctions",junctions,Material("Feathered trail junctions",Color.white,true,texture,2985),false);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Scene save failed.");return new{junctions=count,validation=Validate()};
    }
    public static object RepairTrailNormals()
    {
        Ready();var root=GameObject.Find("14 - Briarback quilboar frontier").transform.Find(Group);
        foreach(var f in root.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.EndsWith(" worn surface",StringComparison.Ordinal)))
        {
            Need(AssetDatabase.GetAssetPath(f.sharedMesh).StartsWith(Folder+"/",StringComparison.Ordinal),"Foreign trail mesh.");
            if(f.sharedMesh.normals.Average(n=>n.y)>0)continue;var t=f.sharedMesh.triangles;
            for(int i=0;i<t.Length;i+=3){int a=t[i+1];t[i+1]=t[i+2];t[i+2]=a;}f.sharedMesh.triangles=t;f.sharedMesh.RecalculateNormals();EditorUtility.SetDirty(f.sharedMesh);AssetDatabase.SaveAssetIfDirty(f.sharedMesh);
        }
        return Validate();
    }
    public static object MatchTrailCrossSlopes()
    {
        Ready();var region=GameObject.Find("14 - Briarback quilboar frontier").transform;var root=region.Find(Group);
        foreach(var name in Roads)
        {
            var original=region.GetComponentsInChildren<MeshFilter>().Single(f=>f.name==name);var source=original.sharedMesh.vertices.Select(p=>original.transform.TransformPoint(p)).ToArray();
            var mesh=root.Find(name+" worn surface").GetComponent<MeshFilter>().sharedMesh;var vertices=mesh.vertices;
            Need(vertices.Length==source.Length/4*24,"Trail output layout changed.");
            for(int segment=0;segment<source.Length/4;segment++)
            {
                int n=segment*4;var a=(source[n]+source[n+1])*.5f;var b=(source[n+2]+source[n+3])*.5f;var side=(source[n+1]-source[n]).normalized;float half=Vector3.Distance(source[n],source[n+1])*.5f;
                for(int j=segment*24;j<(segment+1)*24;j++)
                {
                    var p=vertices[j];var center=Vector3.ProjectOnPlane(p-a,Vector3.up).sqrMagnitude<Vector3.ProjectOnPlane(p-b,Vector3.up).sqrMagnitude?a:b;bool first=(center-a).sqrMagnitude<.001f;float across=Vector3.Dot(p-center,side);int k=first?n:n+2;
                    if(Mathf.Abs(across)<=half+.02f)p.y=Mathf.LerpUnclamped(source[k].y,source[k+1].y,(across+half)/(2*half))+.014f;vertices[j]=p;
                }
            }
            mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssetIfDirty(mesh);
        }
        return Validate();
    }
    public static object Before()=>Capture("before");public static object After()=>Capture("after");
    static object Capture(string phase)
    {
        Ready();string output="VisualBaselines/BriarbackRegion";Directory.CreateDirectory(output);var source=Object.FindAnyObjectByType<ExplorationCamera>();var go=new GameObject("Temporary region review camera");var target=new GameObject("Temporary region target");var rt=new RenderTexture(1600,900,24);var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);var old=RenderTexture.active;
        try{var c=go.AddComponent<Camera>();c.CopyFrom(source.GetComponent<Camera>());c.targetTexture=rt;foreach(var p in new[]{("trimery-road",new Vector3(248,0,229)),("watchpost",new Vector3(245,0,284)),("crossroads",new Vector3(270,0,369)),("western-boundary",new Vector3(207,0,355))}){target.transform.position=p.Item2;source.GetFollowPose(target.transform,out var pos,out var rot,out var fov);c.transform.SetPositionAndRotation(pos,rot);c.fieldOfView=fov;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes(output+"/"+p.Item1+"-"+phase+".png",tex.EncodeToPNG());}return new{phase,output};}
        finally{RenderTexture.active=old;Object.DestroyImmediate(go);Object.DestroyImmediate(target);Object.DestroyImmediate(tex);rt.Release();Object.DestroyImmediate(rt);}
    }
}
