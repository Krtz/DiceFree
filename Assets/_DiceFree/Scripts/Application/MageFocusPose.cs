using UnityEngine;using DiceFree.UI;
namespace DiceFree.Visuals
{
 [DefaultExecutionOrder(600)] public sealed class MageFocusPose:MonoBehaviour,IPortraitPoseable
 {
  public Transform visualRoot,wand,orb;public Animator model;
  public void ApplyPortraitPose(){
   if(model==null||!model.isHuman)return;
   var right=model.GetBoneTransform(HumanBodyBones.RightHand);var left=model.GetBoneTransform(HumanBodyBones.LeftHand);
   if(wand!=null&&right!=null)wand.SetPositionAndRotation(right.position,visualRoot.rotation*Quaternion.Euler(-12,0,-18));
   if(orb!=null&&left!=null)orb.position=left.position+Vector3.up*.18f+visualRoot.forward*.12f;
  }
  void LateUpdate()=>ApplyPortraitPose();
 }
}
