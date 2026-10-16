using System;using System.Collections.Generic;using System.Linq;using UnityEngine;using DiceFree.Items;using DiceFree.UI;
namespace DiceFree.Visuals
{
 /// <summary>Original replaceable Mage placeholder; does not contain the commercial reference pack.</summary>
 [DefaultExecutionOrder(80),DisallowMultipleComponent] public sealed class MageOutfit:MonoBehaviour
 {
  readonly List<Material> materials=new();readonly List<Mesh> meshes=new();
  Transform staff;GameObject hat,cape,coat,wand,orb;[SerializeField,HideInInspector] bool built;
  Material Mat(string name,Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};m.SetFloat("_Smoothness",.22f);materials.Add(m);return m;}
  void Awake()=>Build();
  public void Build(){
   if(built)return;var animator=ClassPresentationSwitcher.FindVisualAnimator(transform);if(animator==null||!animator.isHuman)return;
   built=true;animator.Rebind();int idle=Animator.StringToHash("Base Layer.Idle");if(animator.HasState(0,idle))animator.Play(idle,0,0);animator.Update(0);
   var head=animator.GetBoneTransform(HumanBodyBones.Head);var hips=animator.GetBoneTransform(HumanBodyBones.Hips);var spine=animator.GetBoneTransform(HumanBodyBones.Spine);
   var left=animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);var right=animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);var foot=animator.GetBoneTransform(HumanBodyBones.LeftFoot);
   if(head==null||hips==null||spine==null||left==null||right==null||foot==null)return;
   float height=Mathf.Max(1,head.position.y-foot.position.y),floor=foot.position.y-.04f;
   var blue=Mat("Mage midnight blue cloth",new Color(.10f,.15f,.31f));var trim=Mat("Mage muted brass",new Color(.53f,.39f,.17f));
   blue.SetFloat("_Cull",0);
   var dark=Mat("Mage hat and cape",new Color(.12f,.10f,.22f));var leather=Mat("Mage leather",new Color(.18f,.10f,.06f));
   var glow=Mat("Mage neutral crystal",new Color(.45f,.75f,.88f));glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",new Color(.15f,.25f,.35f));
   GameObject Group(string name){var g=new GameObject(name);g.transform.SetParent(transform,false);return g;}
   GameObject Primitive(string name,PrimitiveType type,Transform parent,Vector3 local,Vector3 size,Material material){
    var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=local;g.transform.localScale=size;
    var c=g.GetComponent<Collider>();c.enabled=false;Destroy(c);g.GetComponent<Renderer>().sharedMaterial=material;return g;
   }
   hat=Group("Mage Hat");hat.transform.position=head.position+Vector3.up*(height*.155f);
   Primitive("Broad pointed-hat brim",PrimitiveType.Cylinder,hat.transform,Vector3.zero,new Vector3(height*.56f,height*.012f,height*.48f),dark);
   var verts=new List<Vector3>();var triangles=new List<int>();
   for(int ring=0;ring<3;ring++)for(int i=0;i<12;i++){float a=i*Mathf.PI/6,r=ring==0?.18f:ring==1?.12f:0;verts.Add(new Vector3(Mathf.Cos(a)*r-(ring==2?.07f:0),ring*.15f,Mathf.Sin(a)*r)*height);}
   for(int ring=0;ring<2;ring++)for(int i=0;i<12;i++){int a=ring*12+i,b=ring*12+(i+1)%12,c=(ring+1)*12+i,d=(ring+1)*12+(i+1)%12;triangles.AddRange(new[]{a,c,b,b,c,d});}
   var mesh=new Mesh{name="Original bent Mage hat"};mesh.SetVertices(verts);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();meshes.Add(mesh);
   var crown=new GameObject("Bent pointed crown");crown.transform.SetParent(hat.transform,false);crown.AddComponent<MeshFilter>().sharedMesh=mesh;crown.AddComponent<MeshRenderer>().sharedMaterial=dark;
   Primitive("Hat band",PrimitiveType.Cylinder,hat.transform,new Vector3(0,height*.035f,0),new Vector3(height*.375f,height*.025f,height*.375f),trim);hat.transform.SetParent(head,true);
   coat=Group("Mage layered coat");
   verts.Clear();triangles.Clear();var weights=new List<BoneWeight>();
   float[] levels={1.06f,.85f,.66f,.46f,.25f};float[] widths={.075f,.12f,.115f,.14f,.20f};
   Vector3 origin=transform.position;
   for(int ring=0;ring<levels.Length;ring++)for(int i=0;i<12;i++){
    float a=i*Mathf.PI/6;var world=origin+transform.right*(Mathf.Cos(a)*height*widths[ring])+transform.forward*(Mathf.Sin(a)*height*widths[ring]*.90f);world.y=floor+height*levels[ring];
    verts.Add(transform.InverseTransformPoint(world));
    weights.Add(new BoneWeight{boneIndex0=ring<2?0:ring<4?1:(Mathf.Cos(a)<0?2:3),weight0=1});
   }
   for(int ring=0;ring<levels.Length-1;ring++)for(int i=0;i<12;i++){
    if(ring==3&&(i==2||i==8))continue;int a=ring*12+i,b=ring*12+(i+1)%12,c=(ring+1)*12+i,d=(ring+1)*12+(i+1)%12;
    triangles.AddRange(new[]{a,b,c,b,d,c});
   }
   mesh=new Mesh{name="Original split Mage coat"};mesh.SetVertices(verts);mesh.SetTriangles(triangles,0);mesh.boneWeights=weights.ToArray();
   Transform[] bones={spine,hips,left,right};mesh.bindposes=bones.Select(b=>b.worldToLocalMatrix*coat.transform.localToWorldMatrix).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
   var skin=coat.AddComponent<SkinnedMeshRenderer>();skin.sharedMesh=mesh;skin.bones=bones;skin.rootBone=hips;skin.sharedMaterial=blue;skin.updateWhenOffscreen=true;
   var belt=Primitive("Mage belt",PrimitiveType.Cylinder,transform,Vector3.zero,new Vector3(height*.27f,height*.022f,height*.24f),leather);belt.transform.position=hips.position+Vector3.up*.06f;belt.transform.SetParent(hips,true);
   cape=Group("Mage short cape");cape.transform.position=spine.position-transform.forward*.17f;
   var capeMesh=new Mesh{name="Original tapered Mage cape"};
   capeMesh.vertices=new[]{new Vector3(-.10f,.16f,0)*height,new Vector3(.10f,.16f,0)*height,new Vector3(-.20f,-.28f,-.045f)*height,new Vector3(.20f,-.28f,-.045f)*height};
   capeMesh.triangles=new[]{0,2,1,1,2,3};capeMesh.RecalculateNormals();capeMesh.RecalculateBounds();meshes.Add(capeMesh);
   cape.AddComponent<MeshFilter>().sharedMesh=capeMesh;cape.AddComponent<MeshRenderer>().sharedMaterial=dark;dark.SetFloat("_Cull",0);cape.transform.SetParent(spine,true);
   wand=Group("Mage Wand");Primitive("Mage wand grip",PrimitiveType.Cylinder,wand.transform,new Vector3(0,.10f,0),new Vector3(.035f,.18f,.035f),leather);
   Primitive("Mage wand brass neck",PrimitiveType.Cylinder,wand.transform,new Vector3(0,.30f,0),new Vector3(.05f,.025f,.05f),trim);
   var crystal=Primitive("Mage wand crystal",PrimitiveType.Cube,wand.transform,new Vector3(0,.38f,0),new Vector3(.075f,.13f,.075f),glow);crystal.transform.localRotation=Quaternion.Euler(0,0,30);
   orb=Group("Mage Orb");Primitive("Floating crystal orb",PrimitiveType.Sphere,orb.transform,Vector3.zero,Vector3.one*.20f,glow);
   var pose=gameObject.AddComponent<MageFocusPose>();pose.visualRoot=transform;pose.model=animator;pose.wand=wand.transform;pose.orb=orb.transform;pose.ApplyPortraitPose();
   staff=GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="Caster Staff");if(staff!=null)staff.gameObject.SetActive(false);
   foreach(var r in GetComponentsInChildren<Renderer>(true))if(r.name=="PT_Male_Armor_01_A_cape")r.enabled=false;
   BindEquipmentBaselines();
  }
  public void BindEquipmentBaselines(){
   var presentation=GetComponentInParent<EquipmentPresentation>();
   if(presentation!=null){
    foreach(var b in presentation.Bindings){
     GameObject baseline=b.slot switch{EquipmentSlot.Head=>hat,EquipmentSlot.Back=>cape,EquipmentSlot.Chest=>coat,EquipmentSlot.MainHand=>wand,EquipmentSlot.OffHand=>orb,_=>null};
     if(baseline!=null)b.baselineVisuals=b.baselineVisuals.Concat(new[]{baseline}).Distinct().ToArray();
    }
    presentation.Configure(presentation.Bindings);
   }
  }
  void LateUpdate(){if(staff!=null)staff.gameObject.SetActive(false);}
  void OnDestroy(){foreach(var m in materials)if(m!=null)Destroy(m);foreach(var m in meshes)if(m!=null)Destroy(m);}
 }
}
