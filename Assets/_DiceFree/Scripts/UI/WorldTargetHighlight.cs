using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.World;
using UnityEngine;
using UnityEngine.Rendering;

namespace DiceFree.UI
{
    // Renderer-only overlays share the original meshes and animated bones.
    // They never change source materials, colliders, or navigation footprints.
    internal sealed class WorldTargetHighlight
    {
        private readonly List<(Renderer source, Renderer overlay)> overlays = new();
        private readonly List<Material> materials = new();
        private Component target;
        private GameObject root;
        private LineRenderer ring;
        private Material ringMaterial;
        public void Show(Component next, Color color, bool silhouette, Camera camera)
        {
            if(next == null) { Clear(); return; }
            var shape = WorldSelectionShape.For(next);
            if(!shape.TryBounds(out var bounds)) { Clear(); return; }
            if(target != next || root == null)
            {
                Clear(); target = next;
                root = new GameObject("Target footprint") { layer = 30 };
                root.transform.SetParent(next.transform, false);
                ring = root.AddComponent<LineRenderer>();
                ring.useWorldSpace=true; ring.loop=true; ring.positionCount=64;
                ring.shadowCastingMode=ShadowCastingMode.Off; ring.receiveShadows=false;
                var template=Resources.Load<Material>("WorldHoverFeedback/Ring");
                if(template != null) { ringMaterial=new Material(template); ring.sharedMaterial=ringMaterial; }
                if(silhouette) CreateOutlines(shape);
            }
            if(ringMaterial != null) ringMaterial.SetColor("_BaseColor",color);
            var actor=next as CombatActor;
            float radius=actor != null ? actor.Radius : Mathf.Clamp(Mathf.Max(bounds.extents.x,bounds.extents.z),.4f,4);
            Vector3 center=actor != null ? actor.transform.position : new Vector3(bounds.center.x,next.transform.position.y,bounds.center.z);
            float pixelSize=camera == null ? .025f : camera.orthographic ? camera.orthographicSize*2/Mathf.Max(1,Screen.height) :
                Vector3.Distance(camera.transform.position,center)*Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f)*2/Mathf.Max(1,Screen.height);
            ring.widthMultiplier=Mathf.Clamp(pixelSize*(silhouette?2.5f:1.7f),.025f,.14f);
            for(int i=0;i<ring.positionCount;i++)
            {
                float angle=i/(float)ring.positionCount*Mathf.PI*2;
                var point=center+new Vector3(Mathf.Cos(angle)*radius,.07f,Mathf.Sin(angle)*radius);
                if(Physics.Raycast(point+Vector3.up*.7f,Vector3.down,out var ground,2.2f,1<<8,QueryTriggerInteraction.Ignore))
                    point.y=ground.point.y+.06f;
                ring.SetPosition(i,point);
            }
            foreach(var pair in overlays)
                if(pair.overlay != null) pair.overlay.enabled=shape.Visible(pair.source);
            foreach(var material in materials) if(material.HasProperty("_OutlineColor")) material.SetColor("_OutlineColor",color);
        }
        private void CreateOutlines(WorldSelectionShape shape)
        {
            var template=Resources.Load<Material>("WorldHoverFeedback/Outline");
            var maskTemplate=Resources.Load<Material>("WorldHoverFeedback/Mask");
            if(template == null || maskTemplate == null) return;
            foreach(var source in shape.Renderers)
            {
                if(!shape.Visible(source)) continue;
                Mesh mesh=source is SkinnedMeshRenderer skin ? skin.sharedMesh : source.GetComponent<MeshFilter>()?.sharedMesh;
                if(mesh == null) continue;
                Renderer CreateOverlay(string name)
                {
                    var child=new GameObject(name) { layer=30 };
                    child.transform.SetParent(source.transform,false);
                    Renderer result;
                    if(source is SkinnedMeshRenderer skinned)
                    {
                        var renderer=child.AddComponent<SkinnedMeshRenderer>();
                        renderer.sharedMesh=mesh; renderer.bones=skinned.bones; renderer.rootBone=skinned.rootBone;
                        renderer.localBounds=skinned.localBounds; renderer.updateWhenOffscreen=false;
                        result=renderer;
                    }
                    else
                    {
                        child.AddComponent<MeshFilter>().sharedMesh=mesh;
                        result=child.AddComponent<MeshRenderer>();
                    }
                    result.shadowCastingMode=ShadowCastingMode.Off; result.receiveShadows=false;
                    result.lightProbeUsage=LightProbeUsage.Off; result.reflectionProbeUsage=ReflectionProbeUsage.Off;
                    overlays.Add((source,result)); return result;
                }
                var overlay=CreateOverlay("Hover silhouette");
                var mask=CreateOverlay("Hover silhouette mask");
                var originals=source.sharedMaterials;
                var copies=new Material[mesh.subMeshCount];
                var masks=new Material[mesh.subMeshCount];
                for(int i=0;i<copies.Length;i++)
                {
                    var material=new Material(template); copies[i]=material; materials.Add(material);
                    var maskMaterial=new Material(maskTemplate); masks[i]=maskMaterial; materials.Add(maskMaterial);
                    var original=originals.Length>0?originals[Mathf.Min(i,originals.Length-1)]:null;
                    if(original == null) continue;
                    string map=original.HasProperty("_BaseMap")?"_BaseMap":"_MainTex";
                    if(original.HasProperty(map))
                    {
                        material.SetTexture("_SourceMap",original.GetTexture(map));
                        material.SetTextureScale("_SourceMap",original.GetTextureScale(map));
                        material.SetTextureOffset("_SourceMap",original.GetTextureOffset(map));
                        maskMaterial.SetTexture("_SourceMap",original.GetTexture(map));
                        maskMaterial.SetTextureScale("_SourceMap",original.GetTextureScale(map));
                        maskMaterial.SetTextureOffset("_SourceMap",original.GetTextureOffset(map));
                    }
                    bool clipped=original.IsKeywordEnabled("_ALPHATEST_ON") || (original.HasProperty("_AlphaClip") && original.GetFloat("_AlphaClip")>.5f);
                    material.SetFloat("_UseAlphaClip",clipped?1:0);
                    maskMaterial.SetFloat("_UseAlphaClip",clipped?1:0);
                    if(original.HasProperty("_Cutoff"))
                    {
                        material.SetFloat("_Cutoff",original.GetFloat("_Cutoff"));
                        maskMaterial.SetFloat("_Cutoff",original.GetFloat("_Cutoff"));
                    }
                }
                overlay.sharedMaterials=copies; mask.sharedMaterials=masks;
            }
        }
        public void Clear()
        {
            foreach(var pair in overlays) if(pair.overlay != null) { pair.overlay.enabled=false; Object.Destroy(pair.overlay.gameObject); }
            foreach(var material in materials) if(material != null) Object.Destroy(material);
            overlays.Clear(); materials.Clear();
            if(root != null) { root.SetActive(false); Object.Destroy(root); }
            if(ringMaterial != null) Object.Destroy(ringMaterial);
            root=null; target=null; ring=null; ringMaterial=null;
        }
    }
}
