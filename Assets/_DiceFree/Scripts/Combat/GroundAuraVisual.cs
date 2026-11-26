using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DiceFree.Combat
{
    public interface IAuraGroundSurface { bool TryGetHeight(Vector3 position,out float height); }
    public enum GroundAuraStyle { Mana, Elements, Defender, Speed, Frost, Radiation, Ranged, Coffee, Infernal }

    // Presentation only. The ability owns eligibility, duration, range and stacking.
    [DefaultExecutionOrder(650)]
    public sealed class GroundAuraVisual : MonoBehaviour
    {
        static readonly HashSet<IAuraGroundSurface> groundSurfaces=new();
        public static void RegisterGround(IAuraGroundSurface surface)=>groundSurfaces.Add(surface);
        public static void UnregisterGround(IAuraGroundSurface surface)=>groundSurfaces.Remove(surface);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetGround()=>groundSurfaces.Clear();
        private const int Grid = 8;
        private Behaviour source;
        private CombatActor recipient;
        private Func<bool> eligible;
        private Mesh mesh;
        private MeshRenderer surface;
        private Material material;
        private Vector3[] vertices;
        private float nextGround, opacity;
        private Vector3 sampledAt;
        private bool sampled;
        public GroundAuraStyle Style { get; private set; }
        public bool Visible => surface != null && surface.enabled;
        public float Radius { get; private set; }
        public CombatActor Recipient => recipient;

        public static GroundAuraVisual Create(Behaviour source, CombatActor recipient, GroundAuraStyle style, Func<bool> eligible)
        {
            if(source == null || recipient == null) return null;
            var root = new GameObject("Aura sigil - " + style) { layer = 30 };
            root.transform.SetParent(recipient.transform, false);
            var visual = root.AddComponent<GroundAuraVisual>();
            visual.source = source; visual.recipient = recipient; visual.eligible = eligible; visual.Style = style;
            visual.Radius = style switch { GroundAuraStyle.Elements => 1.75f, GroundAuraStyle.Defender => 1.65f,
                GroundAuraStyle.Speed => 1.65f, GroundAuraStyle.Frost => 1.6f, _ => 1.5f };
            visual.Initialize();
            return visual;
        }

        private void Initialize()
        {
            var template = Resources.Load<Material>("AuraVfx/GroundSigil");
            if(template == null) { Destroy(gameObject); return; }
            material = new Material(template) { name = "Aura sigil - " + Style, hideFlags = HideFlags.DontSave };
            material.renderQueue=3040; // Draw after transparent road overlays; depth still hides it beneath actors.
            material.SetFloat("_Style", (int)Style);
            var colors = Style switch
            {
                GroundAuraStyle.Mana => (new Color(.12f,.62f,1), new Color(.64f,.94f,1)),
                GroundAuraStyle.Elements => (new Color(.40f,.68f,1), new Color(1,.64f,.20f)),
                GroundAuraStyle.Defender => (new Color(1,.68f,.18f), new Color(.42f,.77f,1)),
                GroundAuraStyle.Speed => (new Color(.61f,.30f,1), new Color(.84f,.90f,1)),
                GroundAuraStyle.Frost => (new Color(.22f,.76f,1), new Color(.87f,.98f,1)),
                GroundAuraStyle.Ranged => (new Color(.35f,.85f,.45f),new Color(1,.85f,.35f)),
                GroundAuraStyle.Coffee => (new Color(.86f,.43f,.12f),new Color(1,.84f,.48f)),
                GroundAuraStyle.Infernal => (new Color(1,.20f,.025f),new Color(1,.78f,.13f)),
                _ => (new Color(.28f,1,.10f), new Color(.80f,1,.29f))
            };
            material.SetColor("_TintA", colors.Item1); material.SetColor("_TintB", colors.Item2);
            vertices = new Vector3[(Grid+1)*(Grid+1)];
            var uv = new Vector2[vertices.Length]; var triangles = new int[Grid*Grid*6];
            for(int z=0;z<=Grid;z++) for(int x=0;x<=Grid;x++) uv[z*(Grid+1)+x]=new Vector2(x/(float)Grid,z/(float)Grid);
            int index=0;
            for(int z=0;z<Grid;z++) for(int x=0;x<Grid;x++)
            {
                int a=z*(Grid+1)+x,b=a+1,c=a+Grid+1,d=c+1;
                triangles[index++]=a;triangles[index++]=c;triangles[index++]=b;
                triangles[index++]=b;triangles[index++]=c;triangles[index++]=d;
            }
            mesh = new Mesh { name="Ground-conforming aura sigil",hideFlags=HideFlags.DontSave };
            mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;
            gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            surface=gameObject.AddComponent<MeshRenderer>();surface.enabled=false;surface.sharedMaterial=material;
            surface.shadowCastingMode=ShadowCastingMode.Off;surface.receiveShadows=false;
            surface.lightProbeUsage=LightProbeUsage.Off;surface.reflectionProbeUsage=ReflectionProbeUsage.Off;
        }

        private void LateUpdate()
        {
            if(source == null || recipient == null) { Remove(); return; }
            if(surface == null) return;
            bool active=source.isActiveAndEnabled && recipient.Alive && !recipient.Concealed && eligible?.Invoke()==true;
            // Immediately hide invalid/dead sources. Fade-in keeps learning an aura subtle.
            opacity=active?Mathf.MoveTowards(opacity,1,Time.unscaledDeltaTime*4):0;
            surface.enabled=active && opacity>0;
            if(!surface.enabled) return;
            var position=recipient.transform.position;
            transform.SetPositionAndRotation(position,Quaternion.identity);
            var scale=recipient.transform.lossyScale;
            transform.localScale=new Vector3(1/Mathf.Max(.001f,Mathf.Abs(scale.x)),1/Mathf.Max(.001f,Mathf.Abs(scale.y)),1/Mathf.Max(.001f,Mathf.Abs(scale.z)));
            if(!sampled || Time.unscaledTime>=nextGround && (position-sampledAt).sqrMagnitude>.0025f)
            {
                sampled=true;sampledAt=position;nextGround=Time.unscaledTime+.10f;
                for(int z=0;z<=Grid;z++) for(int x=0;x<=Grid;x++)
                {
                    var p=position+new Vector3((x/(float)Grid*2-1)*Radius,0,(z/(float)Grid*2-1)*Radius);
                    if(Physics.Raycast(p+Vector3.up*.7f,Vector3.down,out var ground,2.1f,1<<8,QueryTriggerInteraction.Ignore)) p.y=ground.point.y;
                    foreach(var road in groundSurfaces)
                        if(road.TryGetHeight(p,out float roadHeight))p.y=Mathf.Max(p.y,roadHeight);
                    vertices[z*(Grid+1)+x]=p-position+Vector3.up*.045f;
                }
                mesh.vertices=vertices;mesh.RecalculateBounds();
            }
            material.SetFloat("_Opacity",opacity);
            material.SetFloat("_Phase",Time.time);
        }

        public void Remove()
        {
            if(surface != null) surface.enabled=false;
            Destroy(gameObject);
        }
        private void OnDisable() { if(surface != null) surface.enabled=false; opacity=0; }
        private void OnDestroy()
        {
            if(material != null) Destroy(material);
            if(mesh != null) Destroy(mesh);
        }
    }
}
