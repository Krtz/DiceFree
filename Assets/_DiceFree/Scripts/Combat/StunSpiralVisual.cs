using UnityEngine;
using UnityEngine.Rendering;

namespace DiceFree.Combat
{
    // Blue status spirals are intentionally attached to the affected actor,
    // not spawned at the hit location. Pack glow is optional and locally licensed.
    [DisallowMultipleComponent]
    public sealed class StunSpiralVisual : MonoBehaviour
    {
        private static Material lineArt,glowArt;
        private CombatActor actor;
        private Transform root;
        private readonly LineRenderer[] lines=new LineRenderer[2];
        private readonly Transform[] motes=new Transform[3];
        private float until,phase,radius;
        public bool Visible => root!=null && root.gameObject.activeSelf;
        private void Awake()=>actor=GetComponent<CombatActor>();
        public void ShowUntil(float expiry)
        {
            until=Mathf.Max(until,expiry);
            if(root==null)Create();
            root.gameObject.SetActive(true);
        }
        public void Clear()
        {
            until=0;
            if(root!=null)root.gameObject.SetActive(false);
        }
        private static Material MakeMaterial(string name, Texture2D texture)
        {
            Shader shader=Shader.Find("Sprites/Default") ??
                Shader.Find("Universal Render Pipeline/Unlit");
            var material=new Material(shader){name=name};
            if(texture!=null)material.mainTexture=texture;
            material.renderQueue=(int)RenderQueue.Transparent;
            return material;
        }
        private void Create()
        {
            if(lineArt==null)lineArt=MakeMaterial("DiceFree stun swirling blue",null);
            if(glowArt==null)
            {
                glowArt=MakeMaterial("DiceFree stun blue motes",
                    Resources.Load<Texture2D>("StatusVfxLicensed/StunSoftGlow"));
                glowArt.color=new Color(.26f,.66f,1f,.90f);
            }
            radius=Mathf.Clamp((actor!=null?actor.Radius:.7f)*.70f,.35f,1.4f);
            float top=0f;
            foreach(var renderer in GetComponentsInChildren<Renderer>(true))
                if(renderer.enabled)top=Mathf.Max(top,renderer.bounds.max.y-transform.position.y);
            var capsule=GetComponent<CapsuleCollider>();
            top=Mathf.Clamp(Mathf.Max(top,capsule!=null?capsule.height:1.8f)+.16f,1.1f,7f);
            root=new GameObject("DiceFree stun blue swirls (VFX only)").transform;
            root.SetParent(transform,false);
            root.localPosition=Vector3.up*top;
            for(int i=0;i<lines.Length;i++)
            {
                var g=new GameObject("Stun blue helix "+i);
                g.transform.SetParent(root,false);
                var line=g.AddComponent<LineRenderer>();
                line.useWorldSpace=false;
                line.sharedMaterial=lineArt;
                line.positionCount=44;
                line.widthMultiplier=Mathf.Clamp(radius*.12f,.045f,.14f);
                line.numCornerVertices=2;
                line.startColor=new Color(.14f,.72f,1f,.95f);
                line.endColor=new Color(.57f,.85f,1f,.70f);
                line.shadowCastingMode=ShadowCastingMode.Off;
                line.receiveShadows=false;
                lines[i]=line;
            }
            for(int j=0;j<motes.Length;j++)
            {
                var g=GameObject.CreatePrimitive(PrimitiveType.Quad);
                g.name="DiceFree stun glow "+j;
                g.transform.SetParent(root,false);
                var collider=g.GetComponent<Collider>();
                if(collider!=null){collider.enabled=false;Destroy(collider);}
                g.GetComponent<MeshRenderer>().sharedMaterial=glowArt;
                g.transform.localScale=Vector3.one*(radius*.48f);
                motes[j]=g.transform;
            }
            root.gameObject.SetActive(false);
        }
        private void LateUpdate()
        {
            if(!Visible)return;
            if((actor!=null&&!actor.Alive) || Time.time>=until){Clear();return;}
            phase+=Time.deltaTime*4.0f;
            for(int l=0;l<lines.Length;l++)
                for(int i=0;i<lines[l].positionCount;i++)
                {
                    float t=i/(float)(lines[l].positionCount-1);
                    float angle=phase+l*Mathf.PI+t*3.1f*Mathf.PI;
                    float r=radius*(.85f+.12f*Mathf.Sin(t*Mathf.PI));
                    lines[l].SetPosition(i,new Vector3(
                        Mathf.Cos(angle)*r,.07f+t*.42f+l*.06f,
                        Mathf.Sin(angle)*r));
                }
            var camera=Camera.main;
            for(int j=0;j<motes.Length;j++)
            {
                float t=phase+j*2.094395f;
                motes[j].localPosition=new Vector3(Mathf.Sin(t)*radius,
                    .20f+.18f*Mathf.Cos(t*1.3f),Mathf.Cos(t)*radius);
                if(camera!=null)motes[j].rotation=Quaternion.LookRotation(
                    camera.transform.position-motes[j].position,Vector3.up);
            }
        }
        private void OnDisable()=>Clear();
    }
}
