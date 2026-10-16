using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.UI;
using DiceFree.Visuals;
using UnityEngine;
namespace DiceFree.Advancement
{
    /// <summary>Visual-only save previews. No CombatActor, persistence, AI, input or scene travel.</summary>
    public sealed class MenuPortraitStage : MonoBehaviour
    {
        public sealed class Portrait {
            public string classId; public GameObject root; public RenderTexture texture;
            public Camera camera; public bool rendered; internal int readyFrame;
            public string[] equippedIds;
        }
        readonly Dictionary<string,Portrait> portraits=new(StringComparer.Ordinal);
        readonly HashSet<string> unavailable=new(StringComparer.Ordinal);
        public IReadOnlyCollection<Portrait> Portraits=>portraits.Values;
        public RenderTexture Get(string id,ManifestationSave state,MenuPresentationCatalog catalog)
        {
            if(unavailable.Contains(id))return null;
            if(portraits.TryGetValue(id,out var old))return old.texture;
            var form=catalog?.Resolve(id);if(form?.preview==null)return null;
            GameObject stage=null;RenderTexture texture=null;
            try {
            stage=new GameObject("Menu portrait: "+id);stage.transform.SetParent(transform,false);
            stage.transform.position=new Vector3(portraits.Count*30,-1200,0);
            var root=Instantiate(form.preview,stage.transform);root.name="Read-only equipped preview";
            root.SetActive(true);
            var inv=root.GetComponent<CarriedInventory>();var equipment=root.GetComponent<Equipment>();
            inv.Configure(catalog.items);
            inv.Restore(state?.inventory??Array.Empty<ItemInstance>());
            var plain=root.GetComponent<PlainEquipmentVisuals>()??root.AddComponent<PlainEquipmentVisuals>();
            plain.enabled=false;
            var visual=root.transform.Find("Class visual");
            plain.PrepareVisual(visual,root.GetComponent<EquipmentPresentation>(),inv);
            var wolf=root.GetComponent<WolfGreatswordVisual>()??root.AddComponent<WolfGreatswordVisual>();
            wolf.enabled=false;wolf.PrepareVisual(visual,root.GetComponent<EquipmentPresentation>(),inv);
            // Procedural class defaults exist before gear bindings are generated. Register them now.
            foreach(var mage in visual.GetComponentsInChildren<MageOutfit>(true))mage.BindEquipmentBaselines();
            equipment.Restore(state?.equipment??Array.Empty<EquippedItem>());
            foreach(var a in visual.GetComponentsInChildren<Animator>(true)) {
                if(!a.isActiveAndEnabled)continue;
                a.applyRootMotion=false;a.cullingMode=AnimatorCullingMode.AlwaysAnimate;a.Rebind();
                int idle=Animator.StringToHash("Base Layer.Idle");
                if(!a.HasState(0,idle))idle=Animator.StringToHash("Base Layer.Greatblade two-hand ready");
                if(a.HasState(0,idle))a.Play(idle,0,0);a.Update(0);
            }
            var cameraObject=new GameObject("Menu portrait camera");cameraObject.transform.SetParent(stage.transform,false);
            var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
            camera.cullingMask=1<<31;camera.nearClipPlane=.05f;camera.farClipPlane=30;
            texture=new RenderTexture(384,512,16,RenderTextureFormat.ARGB32){name="Equipped "+id,antiAliasing=2};
            texture.Create();camera.targetTexture=texture;
            if(portraits.Count==0) {
            var light=new GameObject("Portrait light");light.transform.SetParent(stage.transform,false);
            var l=light.AddComponent<Light>();l.type=LightType.Directional;l.intensity=1.2f;l.cullingMask=1<<31;l.shadows=LightShadows.None;
            light.transform.rotation=Quaternion.Euler(35,150,0);
            }
            foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
            foreach(var col in root.GetComponentsInChildren<Collider>(true))col.enabled=false;
            var p=new Portrait{classId=id,root=stage,texture=texture,camera=camera,readyFrame=Time.frameCount+3,
                equippedIds=equipment.Slots.Select(s=>inv.Find(s.instanceId)?.definitionId).Where(x=>x!=null).ToArray()};
            portraits.Add(id,p);return texture;
            } catch(Exception error) {
                if(texture!=null){texture.Release();Destroy(texture);}
                if(stage!=null)Destroy(stage);
                unavailable.Add(id);
                Debug.LogWarning("Menu preview unavailable for "+id+": "+error.Message);
                return null;
            }
        }
        void LateUpdate() {
            foreach(var p in portraits.Values)if(!p.rendered&&Time.frameCount>=p.readyFrame) {
                foreach(var grip in p.root.GetComponentsInChildren<GreatbladeTwoHandGrip>())grip.ApplyPose();
                foreach(var b in p.root.GetComponentsInChildren<MonoBehaviour>())if(b is IPortraitPoseable pose)pose.ApplyPortraitPose();
                Frame(p);p.camera.Render();p.rendered=true;
                foreach(var a in p.root.GetComponentsInChildren<Animator>())a.enabled=false;
            }
        }
        static void Frame(Portrait p) {
            bool found=false;Bounds bounds=default;var mesh=new Mesh();
            try{foreach(var renderer in p.root.GetComponentsInChildren<Renderer>()) {
                if(!renderer.enabled)continue;var world=renderer.bounds;
                if(renderer is SkinnedMeshRenderer skin) {
                    skin.BakeMesh(mesh);var b=mesh.bounds;world=new Bounds(skin.transform.TransformPoint(b.center),Vector3.zero);
                    for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
                        world.Encapsulate(skin.transform.TransformPoint(b.center+Vector3.Scale(b.extents,new Vector3(x,y,z))));
                }
                if(!found){bounds=world;found=true;}else bounds.Encapsulate(world);
            }}finally{Destroy(mesh);}
            if(!found)return;
            p.camera.orthographicSize=Mathf.Max(bounds.extents.y,bounds.extents.x/p.camera.aspect)*1.15f;
            p.camera.transform.position=bounds.center+Vector3.forward*(bounds.extents.z+8);p.camera.transform.LookAt(bounds.center);
        }
        public void Clear() {
            foreach(var p in portraits.Values){if(p.texture!=null){p.texture.Release();Destroy(p.texture);}if(p.root!=null)Destroy(p.root);}
            portraits.Clear();unavailable.Clear();
        }
        void OnDestroy()=>Clear();
    }
}
