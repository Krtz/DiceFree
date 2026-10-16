using System.Collections.Generic;using System.Linq;using DiceFree.Items;using DiceFree.UI;using DiceFree.Visuals;using UnityEngine;
namespace DiceFree.Briarback
{
    [DefaultExecutionOrder(350)]
    public sealed class BriarbackMantleVisual:MonoBehaviour
    {
        public const string ItemId="item.briarback.bristlehide-mantle";
        readonly HashSet<Transform> prepared=new();readonly List<Mesh> meshes=new();
        ClassPresentationSwitcher classes;Transform previous;EquipmentPresentation presentation;CarriedInventory inventory;
        Material hide,bone;
        void Start(){classes=GetComponent<ClassPresentationSwitcher>();presentation=GetComponent<EquipmentPresentation>();inventory=GetComponent<CarriedInventory>();Prepare();}
        void LateUpdate(){if(classes!=null&&classes.ActiveVisualRoot!=previous)Prepare();}
        void Prepare(){previous=classes?.ActiveVisualRoot;PrepareVisual(previous,presentation,inventory);}
        public void PrepareVisual(Transform root,EquipmentPresentation target,CarriedInventory items){
            if(root==null||target==null||items==null||!prepared.Add(root))return;
            var definition=items.Resolve(ItemId);if(definition==null)return;
            hide??=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Original bristlehide mantle",color=new Color(.30f,.22f,.13f)};
            bone??=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Original mantle bone trim",color=new Color(.75f,.67f,.43f)};
            hide.SetFloat("_Cull",0);hide.SetFloat("_Smoothness",.1f);
            var animator=ClassPresentationSwitcher.FindVisualAnimator(root);if(animator==null||!animator.isHuman)return;
            var spine=animator.GetBoneTransform(HumanBodyBones.Spine);if(spine==null)return;
            var piece=new GameObject("Bristlehide Mantle");piece.transform.SetParent(root,false);
            piece.transform.SetPositionAndRotation(spine.position-root.forward*.17f,root.rotation);
            var mesh=new Mesh{name="Original scalloped hide mantle",
                vertices=new[]{new Vector3(-.24f,.18f,0),new Vector3(.24f,.18f,0),new Vector3(-.29f,-.47f,-.04f),new Vector3(-.15f,-.55f,-.06f),new Vector3(0,-.48f,-.06f),new Vector3(.15f,-.55f,-.06f),new Vector3(.29f,-.47f,-.04f)},
                triangles=new[]{0,2,3,0,3,4,0,4,1,1,4,5,1,5,6}};
            mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
            piece.AddComponent<MeshFilter>().sharedMesh=mesh;piece.AddComponent<MeshRenderer>().sharedMaterial=hide;
            for(int i=0;i<5;i++){
                var stud=GameObject.CreatePrimitive(PrimitiveType.Capsule);stud.name="Bone mantle quill";
                stud.transform.SetParent(piece.transform,false);stud.transform.localPosition=new Vector3((i-2)*.09f,.16f,-.06f);
                stud.transform.localScale=new Vector3(.035f,.10f,.035f);stud.transform.localRotation=Quaternion.Euler(35,0,(i-2)*-12);
                var collider=stud.GetComponent<Collider>();collider.enabled=false;Destroy(collider);stud.GetComponent<Renderer>().sharedMaterial=bone;stud.layer=2;
            }
            piece.layer=2;piece.AddComponent<PlainGearAttachment>().Configure(spine);piece.SetActive(false);
            var bindings=target.Bindings.ToList();var binding=bindings.FirstOrDefault(b=>b.definition==definition);
            if(binding==null){binding=new EquipmentPresentation.Binding{slot=EquipmentSlot.Back,definition=definition};bindings.Add(binding);}
            binding.visuals=binding.visuals.Concat(new[]{piece}).ToArray();
            binding.baselineVisuals=binding.baselineVisuals.Concat(bindings.Where(b=>b.slot==EquipmentSlot.Back&&b!=binding).SelectMany(b=>b.baselineVisuals)).Distinct().ToArray();
            target.Configure(bindings.ToArray());
        }
        void OnDestroy(){if(hide!=null)Destroy(hide);if(bone!=null)Destroy(bone);foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);}
    }
}
