using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DiceFree.Briarback
{
    /// <summary>Original cosmetic geometry, with bounded lifetimes and owned resource cleanup.</summary>
    public sealed class BriarbackChieftainEffects : MonoBehaviour
    {
        readonly List<Mesh> meshes = new();
        readonly List<Material> materials = new();
        readonly List<Renderer> fading = new();
        Transform spinner;
        float born, lifetime;
        int mode;
        MaterialPropertyBlock block;

        static readonly Color Red = new(1,.085f,.025f,.78f);
        static readonly Color Gold = new(1,.64f,.14f,1);
        static readonly Color Steel = new(.67f,.73f,.72f);

        public static GameObject Warning(Vector3 position,Vector3 forward)
        {
            var fx = New("Chieftain smash - red fracture warning",position,forward,0,1.5f);
            // Exactly the existing 6m x 1m center footprint. Victim-radius rules stay in combat.
            fx.Quad("Danger footprint",new Vector3(-.5f,0,0),new Vector3(-.5f,0,6),new Vector3(.5f,0,6),new Vector3(.5f,0,0),Red);
            fx.Strip("Left boundary",new(-.48f,.012f,0),new(-.48f,.012f,6),.055f,Gold);
            fx.Strip("Right boundary",new(.48f,.012f,0),new(.48f,.012f,6),.055f,Gold);
            fx.Strip("Far boundary",new(-.5f,.012f,5.97f),new(.5f,.012f,5.97f),.055f,Gold);
            for(int i=0;i<4;i++)
            {
                float z=.55f+i*1.4f;
                fx.Strip("Advance chevron",new(-.24f,.016f,z),new(0,.016f,z+.35f),.045f,Gold);
                fx.Strip("Advance chevron",new(.24f,.016f,z),new(0,.016f,z+.35f),.045f,Gold);
            }
            return fx.gameObject;
        }

        public static void DressAxe(GameObject projectile)
        {
            var fx=projectile.AddComponent<BriarbackChieftainEffects>();
            fx.born=Time.time;fx.mode=1;
            fx.spinner=new GameObject("Spinning forged axe").transform;fx.spinner.SetParent(projectile.transform,false);
            fx.Box("Ash haft",new(0,-.12f,0),new(.10f,1.0f,.11f),new Color(.27f,.14f,.065f),fx.spinner);
            fx.Box("Grip binding",new(0,-.30f,0),new(.13f,.30f,.14f),new Color(.12f,.085f,.055f),fx.spinner);
            fx.Box("Copper eye",new(0,.19f,0),new(.19f,.28f,.18f),new Color(.51f,.31f,.12f),fx.spinner);
            foreach(int sign in new[]{-1,1})
            {
                var points=new[]{new Vector3(sign*.08f,.32f,0),new Vector3(sign*.37f,.43f,0),new Vector3(sign*.58f,.22f,0),new Vector3(sign*.47f,-.08f,0),new Vector3(sign*.08f,.07f,0)};
                fx.Polygon("Forged blade",points,.10f,Steel,fx.spinner);
            }
            var trail=projectile.AddComponent<TrailRenderer>();trail.time=.16f;trail.minVertexDistance=.08f;
            trail.startWidth=.14f;trail.endWidth=0;trail.numCapVertices=2;trail.autodestruct=false;
            trail.sharedMaterial=fx.MakeMaterial(new Color(.94f,.67f,.23f,.7f),true);
            trail.startColor=new Color(1,.75f,.32f,.7f);trail.endColor=new Color(1,.5f,.1f,0);
            trail.shadowCastingMode=ShadowCastingMode.Off;trail.receiveShadows=false;
        }

        public static void SmashImpact(Vector3 position,Vector3 forward)
        {
            var fx=New("Chieftain smash - dust and fracture",position,forward,2,.48f);
            for(int i=0;i<6;i++)
            {
                float z=.45f+i*.92f,sign=i%2==0?1:-1;
                fx.Strip("Stone fracture",new(-.1f*sign,.008f,z-.35f),new(.2f*sign,.008f,z+.18f),.045f,new Color(.10f,.075f,.045f,.75f));
                var shard=new GameObject("Low stone splinter").transform;shard.SetParent(fx.transform,false);shard.localPosition=new Vector3(sign*.34f,.08f,z);
                fx.Box("Stone chip",Vector3.zero,new(.13f,.16f,.14f),new Color(.52f,.43f,.29f,.8f),shard);
            }
        }
        public static void AxeImpact(Vector3 position)
        {
            var fx=New("Chieftain axe - impact sparks",position,Vector3.forward,3,.22f);
            for(int i=0;i<6;i++)
            {
                var q=Quaternion.Euler(i*31,i*60,0);var direction=q*Vector3.up;
                fx.Strip("Impact fleck",direction*.07f,direction*.3f,.025f,Gold);
            }
        }
        static BriarbackChieftainEffects New(string name,Vector3 position,Vector3 forward,int mode,float duration)
        {
            var go=new GameObject(name);go.layer=2;go.transform.SetPositionAndRotation(position,Quaternion.LookRotation(forward));
            var fx=go.AddComponent<BriarbackChieftainEffects>();fx.mode=mode;fx.born=Time.time;fx.lifetime=duration;return fx;
        }
        Material MakeMaterial(Color color,bool unlit)
        {
            // Resource materials keep both shaders available in the Windows build.
            var template=Resources.Load<Material>("BriarbackFx/"+(unlit?"Unlit":"Lit")+(color.a<1?"Transparent":""));
            var mat=template!=null?new Material(template):new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor",color);
            if(color.a<1)
            {
                if(mat.HasProperty("_BlendModePreserveSpecularLighting"))mat.SetFloat("_BlendModePreserveSpecularLighting",0);
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.SetFloat("_Surface",1);mat.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);mat.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);mat.SetFloat("_ZWrite",0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");mat.SetOverrideTag("RenderType","Transparent");mat.renderQueue=3000;
            }
            materials.Add(mat);return mat;
        }
        void Geometry(string name,Vector3[] vertices,int[] triangles,Color color,bool unlit,Transform parent=null)
        {
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
            var go=new GameObject(name);go.layer=2;go.transform.SetParent(parent!=null?parent:transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=MakeMaterial(color,unlit);renderer.shadowCastingMode=ShadowCastingMode.Off;
            if(color.a<1)fading.Add(renderer);
        }
        void Quad(string name,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color)=>Geometry(name,new[]{a,b,c,d},new[]{0,1,2,0,2,3},color,true);
        void Strip(string name,Vector3 a,Vector3 b,float width,Color color)
        {
            var side=Vector3.Cross(b-a,Vector3.up).normalized*width*.5f;
            if(side.sqrMagnitude<.00001f)side=Vector3.right*width*.5f;
            Quad(name,a+side,b+side,b-side,a-side,color);
        }
        void Box(string name,Vector3 center,Vector3 size,Color color,Transform parent)
        {
            Vector3 V(int x,int y,int z)=>center+Vector3.Scale(size,new Vector3(x,y,z))*.5f;
            Geometry(name,new[]{V(-1,-1,-1),V(1,-1,-1),V(1,1,-1),V(-1,1,-1),V(-1,-1,1),V(1,-1,1),V(1,1,1),V(-1,1,1)},
                new[]{0,3,2,0,2,1,4,5,6,4,6,7,0,4,7,0,7,3,1,2,6,1,6,5,3,7,6,3,6,2,0,1,5,0,5,4},color,false,parent);
        }
        void Polygon(string name,Vector3[] outline,float depth,Color color,Transform parent)
        {
            var v=new List<Vector3>();var t=new List<int>();
            for(int side=-1;side<=1;side+=2)foreach(var p in outline)v.Add(p+Vector3.forward*side*depth*.5f);
            int n=outline.Length;for(int i=1;i<n-1;i++){t.AddRange(new[]{0,i+1,i,n,n+i,n+i+1});}
            for(int i=0;i<n;i++){int j=(i+1)%n;t.AddRange(new[]{i,j,n+j,i,n+j,n+i});}
            Geometry(name,v.ToArray(),t.ToArray(),color,false,parent);
        }
        void LateUpdate()
        {
            float age=Time.time-born;
            if(mode==1){if(spinner!=null)spinner.localRotation=Quaternion.Euler(0,0,age*1000);return;}
            if(mode>=2&&age>=lifetime){Destroy(gameObject);return;}
            block??=new MaterialPropertyBlock();
            foreach(var renderer in fading)
            {
                var color=renderer.sharedMaterial.GetColor("_BaseColor");
                color.a*=mode==0?.86f+.14f*Mathf.Sin(age*12):1-age/lifetime;
                block.SetColor("_BaseColor",color);renderer.SetPropertyBlock(block);
            }
            if(mode==2)foreach(Transform child in transform)if(child.name=="Low stone splinter")child.localPosition+=Vector3.up*(.65f-age*3)*Time.deltaTime;
        }
        void OnDestroy(){foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);foreach(var material in materials)if(material!=null)Destroy(material);}
    }
}
