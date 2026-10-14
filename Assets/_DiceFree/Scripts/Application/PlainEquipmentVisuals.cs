using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Items;
using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Visuals
{
    /// <summary>Plain original fitting pieces, sized to the active Humanoid. Item ownership stays in Equipment.</summary>
    [DefaultExecutionOrder(300), DisallowMultipleComponent]
    public sealed class PlainEquipmentVisuals : MonoBehaviour
    {
        private readonly HashSet<Transform> prepared = new();
        private EquipmentPresentation presentation;
        private ClassPresentationSwitcher classes;
        private CarriedInventory inventory;
        private Transform previous;
        private Material metal,cloth,gold;
        private void Start()
        {
            presentation=GetComponent<EquipmentPresentation>();classes=GetComponent<ClassPresentationSwitcher>();
            inventory=GetComponent<CarriedInventory>();Prepare();
        }
        private void LateUpdate(){if(classes!=null&&classes.ActiveVisualRoot!=previous)Prepare();}
        private void OnDestroy(){if(metal!=null)Destroy(metal);if(cloth!=null)Destroy(cloth);if(gold!=null)Destroy(gold);}
        private Material Mat(string name,Color color)
        {
            var value=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};
            value.SetColor("_BaseColor",color);value.SetFloat("_Smoothness",.18f);return value;
        }
        private void Prepare()
        {
            previous=classes?.ActiveVisualRoot;
            if(previous==null||presentation==null||inventory==null||!prepared.Add(previous))return;
            metal??=Mat("Plain iron",new Color(.46f,.49f,.51f));cloth??=Mat("Plain cloth",new Color(.25f,.36f,.43f));gold??=Mat("Plain brass",new Color(.65f,.48f,.19f));
            var animator=ClassPresentationSwitcher.FindVisualAnimator(previous);
            float headTop=HeadTop(previous,animator);
            var bindings=presentation.Bindings.ToList();
            foreach(var definition in inventory.Definitions.Where(d=>d!=null&&d.modelTestingItem))
            {
                var existing=bindings.FirstOrDefault(b=>b.definition==definition);
                if(existing!=null&&existing.visuals.Any(g=>g!=null&&g.transform.IsChildOf(previous)))continue;
                var visual=Create(definition.slot,previous,animator,headTop);visual.SetActive(false);
                if(existing==null){existing=new EquipmentPresentation.Binding{definition=definition,slot=definition.slot};bindings.Add(existing);}
                existing.visuals=existing.visuals.Concat(new[]{visual}).ToArray();
                if(previous.GetComponent<MagicallyTouchedVisual>()!=null)
                {
                    string baselineName=definition.slot==EquipmentSlot.MainHand?"Caster Staff":
                        definition.slot==EquipmentSlot.Back?"PT_Male_Armor_01_A_cape":null;
                    if(baselineName!=null)existing.baselineVisuals=existing.baselineVisuals.Concat(previous.GetComponentsInChildren<Transform>(true)
                        .Where(t=>t.name==baselineName).Select(t=>t.gameObject)).Distinct().ToArray();
                }
                if(definition.slot==EquipmentSlot.MainHand)
                {
                    var grip=previous.GetComponentInChildren<GreatbladeTwoHandGrip>(true);
                    if(grip!=null&&grip.Weapon!=null)existing.baselineVisuals=existing.baselineVisuals.Concat(new[]{grip.Weapon.gameObject}).ToArray();
                    else existing.baselineVisuals=existing.baselineVisuals.Concat(previous.GetComponentsInChildren<Renderer>(true)
                        .Where(r=>r.name.IndexOf("greatsword",StringComparison.OrdinalIgnoreCase)>=0&&!r.transform.IsChildOf(visual.transform)).Select(r=>r.gameObject)).ToArray();
                }
            }
            presentation.Configure(bindings.ToArray());
            GetComponent<HudPortraitRenderer>()?.RequestRefresh();
        }
        private float HeadTop(Transform root,Animator animator)
        {
            float top=animator!=null&&animator.isHuman&&animator.GetBoneTransform(HumanBodyBones.Head)!=null?
                animator.GetBoneTransform(HumanBodyBones.Head).position.y+.25f:root.position.y+1.85f;
            bool found=false;var mesh=new Mesh();
            try{foreach(var skin in root.GetComponentsInChildren<SkinnedMeshRenderer>()){
                if(!skin.enabled)continue;skin.BakeMesh(mesh);var bounds=mesh.bounds;
                for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2){
                    float height=skin.transform.TransformPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z))).y;
                    if(!found){top=height;found=true;}else top=Mathf.Max(top,height);
                }
            }}finally{Destroy(mesh);}
            return top;
        }
        private GameObject Create(EquipmentSlot slot,Transform root,Animator animator,float headTop)
        {
            Transform Bone(HumanBodyBones id)=>animator!=null&&animator.isHuman?animator.GetBoneTransform(id):null;
            var head=Bone(HumanBodyBones.Head);var foot=Bone(HumanBodyBones.LeftFoot);
            float scale=head!=null&&foot!=null?Mathf.Clamp(Vector3.Distance(head.position,foot.position)/1.55f,.7f,1.8f):1;
            var group=new GameObject("Plain gear "+slot);group.transform.SetParent(root,false);
            Vector3 Point(HumanBodyBones bone,Vector3 fallback)=>Bone(bone)!=null?Bone(bone).position:root.TransformPoint(fallback);
            void Piece(string name,PrimitiveType type,HumanBodyBones bone,Vector3 fallback,Vector3 offset,Vector3 size,Material material)
            {
                var parent=Bone(bone)??root;var piece=GameObject.CreatePrimitive(type);piece.name=name;
                var col=piece.GetComponent<Collider>();col.enabled=false;Destroy(col);
                piece.transform.position=Point(bone,fallback)+root.TransformVector(offset*scale);
                if(slot==EquipmentSlot.Head)piece.transform.position=new Vector3(piece.transform.position.x,headTop-.035f*scale,piece.transform.position.z);
                piece.transform.rotation=root.rotation*(slot==EquipmentSlot.OffHand?Quaternion.Euler(90,0,0):Quaternion.identity);
                piece.transform.localScale=size*scale;
                piece.transform.SetParent(group.transform,true);
                piece.GetComponent<Renderer>().sharedMaterial=material;
                piece.AddComponent<PlainGearAttachment>().Configure(parent);
            }
            switch(slot)
            {
                case EquipmentSlot.Head:Piece("Plain cap",PrimitiveType.Sphere,HumanBodyBones.Head,new Vector3(0,1.7f,0),new Vector3(0,.13f,0),new Vector3(.50f,.18f,.48f),cloth);break;
                case EquipmentSlot.Shoulders:
                    Piece("Left shoulder",PrimitiveType.Cube,HumanBodyBones.LeftUpperArm,new Vector3(-.35f,1.4f,0),new Vector3(0,.05f,0),new Vector3(.26f,.18f,.28f),metal);
                    Piece("Right shoulder",PrimitiveType.Cube,HumanBodyBones.RightUpperArm,new Vector3(.35f,1.4f,0),new Vector3(0,.05f,0),new Vector3(.26f,.18f,.28f),metal);break;
                case EquipmentSlot.Chest:Piece("Plain breastplate",PrimitiveType.Cube,HumanBodyBones.Chest,new Vector3(0,1.25f,0),new Vector3(0,0,.19f),new Vector3(.44f,.40f,.12f),metal);break;
                case EquipmentSlot.Hands:
                    Piece("Left cuff",PrimitiveType.Cube,HumanBodyBones.LeftHand,new Vector3(-.4f,.9f,0),Vector3.zero,new Vector3(.16f,.12f,.18f),cloth);
                    Piece("Right cuff",PrimitiveType.Cube,HumanBodyBones.RightHand,new Vector3(.4f,.9f,0),Vector3.zero,new Vector3(.16f,.12f,.18f),cloth);break;
                case EquipmentSlot.Legs:
                    Piece("Left leg pad",PrimitiveType.Cube,HumanBodyBones.LeftUpperLeg,new Vector3(-.15f,.75f,0),new Vector3(0,-.14f,.12f),new Vector3(.20f,.28f,.10f),cloth);
                    Piece("Right leg pad",PrimitiveType.Cube,HumanBodyBones.RightUpperLeg,new Vector3(.15f,.75f,0),new Vector3(0,-.14f,.12f),new Vector3(.20f,.28f,.10f),cloth);break;
                case EquipmentSlot.Feet:
                    Piece("Left boot",PrimitiveType.Cube,HumanBodyBones.LeftFoot,new Vector3(-.15f,.1f,0),new Vector3(0,0,.06f),new Vector3(.22f,.20f,.36f),cloth);
                    Piece("Right boot",PrimitiveType.Cube,HumanBodyBones.RightFoot,new Vector3(.15f,.1f,0),new Vector3(0,0,.06f),new Vector3(.22f,.20f,.36f),cloth);break;
                case EquipmentSlot.MainHand:
                    Piece("Plain sword blade",PrimitiveType.Cube,HumanBodyBones.RightHand,new Vector3(.4f,.9f,0),new Vector3(0,.43f,0),new Vector3(.085f,.70f,.035f),metal);
                    Piece("Plain sword guard",PrimitiveType.Cube,HumanBodyBones.RightHand,new Vector3(.4f,.9f,0),new Vector3(0,.07f,0),new Vector3(.26f,.045f,.065f),gold);break;
                case EquipmentSlot.OffHand:Piece("Plain buckler",PrimitiveType.Cylinder,HumanBodyBones.LeftHand,new Vector3(-.4f,.9f,0),new Vector3(0,0,.14f),new Vector3(.40f,.06f,.40f),metal);break;
                case EquipmentSlot.Ring:Piece("Plain ring",PrimitiveType.Sphere,HumanBodyBones.RightMiddleProximal,new Vector3(.4f,.9f,0),Vector3.zero,new Vector3(.07f,.06f,.06f),gold);break;
                case EquipmentSlot.Amulet:Piece("Plain pendant",PrimitiveType.Sphere,HumanBodyBones.UpperChest,new Vector3(0,1.4f,0),new Vector3(0,-.07f,.23f),new Vector3(.085f,.10f,.045f),gold);break;
                case EquipmentSlot.Back:Piece("Plain short cape",PrimitiveType.Cube,HumanBodyBones.Chest,new Vector3(0,1.25f,0),new Vector3(0,-.15f,-.20f),new Vector3(.48f,.60f,.045f),cloth);break;
            }
            return group;
        }
    }
}
