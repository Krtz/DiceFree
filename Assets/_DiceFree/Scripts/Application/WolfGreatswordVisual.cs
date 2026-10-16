using System;
using System.Linq;
using DiceFree.Items;
using DiceFree.UI;
using UnityEngine;
namespace DiceFree.Visuals
{
    [DefaultExecutionOrder(500),DisallowMultipleComponent]
    public sealed class WolfGreatswordVisual : MonoBehaviour
    {
        public const string ItemId="item.alpha-wolf.greatsword";
        Transform previous;Material steel,leather,bone;
        void Start()=>Refresh();
        void LateUpdate(){if(GetComponent<ClassPresentationSwitcher>()?.ActiveVisualRoot!=previous)Refresh();}
        void Refresh()=>PrepareVisual(GetComponent<ClassPresentationSwitcher>()?.ActiveVisualRoot,
            GetComponent<EquipmentPresentation>(),GetComponent<CarriedInventory>());
        Material Mat(string name,Color color)=>new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};
        public void PrepareVisual(Transform root,EquipmentPresentation presentation,CarriedInventory inventory)
        {
            previous=root;if(root==null||presentation==null||inventory==null)return;
            var item=inventory.Resolve(ItemId);if(item==null)return;
            var bindings=presentation.Bindings.ToList();var binding=bindings.FirstOrDefault(b=>b.definition==item);
            if(binding!=null&&binding.visuals.Any(v=>v!=null&&v.transform.IsChildOf(root)))return;
            steel??=Mat("Wolf-Fang dull steel",new Color(.49f,.55f,.59f));
            leather??=Mat("Wolf-Fang dark leather",new Color(.16f,.13f,.11f));
            bone??=Mat("Wolf-Fang ivory",new Color(.78f,.75f,.62f));
            var group=new GameObject("Wolf-Fang Greatsword");group.transform.SetParent(root,false);
            void Piece(string name,PrimitiveType type,Vector3 point,Vector3 size,Material material,float angle=0){
                var p=GameObject.CreatePrimitive(type);p.name=name;p.transform.SetParent(group.transform,false);
                p.transform.localPosition=point;p.transform.localScale=size;p.transform.localRotation=Quaternion.Euler(0,0,angle);
                var collider=p.GetComponent<Collider>();collider.enabled=false;Destroy(collider);p.GetComponent<Renderer>().sharedMaterial=material;
            }
            Piece("Plain broad steel blade",PrimitiveType.Cube,new Vector3(0,.83f,0),new Vector3(.16f,1.22f,.05f),steel);
            Piece("Leather two-hand grip",PrimitiveType.Cylinder,new Vector3(0,-.08f,0),new Vector3(.065f,.20f,.065f),leather);
            Piece("Plain crossguard",PrimitiveType.Cube,new Vector3(0,.19f,0),new Vector3(.43f,.07f,.085f),steel);
            for(int side=-1;side<=1;side+=2){
                Piece("Fang guard "+side,PrimitiveType.Capsule,new Vector3(side*.19f,.12f,0),new Vector3(.045f,.09f,.045f),bone,side*18);
                Piece("Wolf pommel ear "+side,PrimitiveType.Cube,new Vector3(side*.055f,-.30f,0),new Vector3(.04f,.075f,.05f),steel,side*22);
            }
            Piece("Simple wolf-head pommel",PrimitiveType.Cube,new Vector3(0,-.35f,0),new Vector3(.13f,.12f,.09f),steel);
            Piece("Wolf muzzle",PrimitiveType.Cube,new Vector3(0,-.37f,.06f),new Vector3(.07f,.05f,.08f),bone);
            var animator=ClassPresentationSwitcher.FindVisualAnimator(root);
            var pose=group.AddComponent<WolfGreatswordPose>();pose.Configure(root,animator);pose.ApplyPortraitPose();group.SetActive(false);
            if(binding==null){binding=new EquipmentPresentation.Binding{definition=item,slot=EquipmentSlot.MainHand};bindings.Add(binding);}
            binding.visuals=binding.visuals.Concat(new[]{group}).ToArray();
            var grip=root.GetComponentInChildren<GreatbladeTwoHandGrip>(true);
            var baseline=root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Caster Staff"||t.name=="Mage Wand"||t.name=="Mage Orb"||
                t.name.IndexOf("greatsword",StringComparison.OrdinalIgnoreCase)>=0&&!t.IsChildOf(group.transform)&&t!=group.transform).Select(t=>t.gameObject);
            if(grip?.Weapon!=null)baseline=baseline.Concat(new[]{grip.Weapon.gameObject});
            binding.baselineVisuals=binding.baselineVisuals.Concat(baseline).Distinct().ToArray();
            presentation.Configure(bindings.ToArray());GetComponent<HudPortraitRenderer>()?.RequestRefresh();
        }
        void OnDestroy(){if(steel!=null)Destroy(steel);if(leather!=null)Destroy(leather);if(bone!=null)Destroy(bone);}
    }
}
