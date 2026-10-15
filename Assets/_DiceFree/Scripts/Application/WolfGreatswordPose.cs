using DiceFree.UI;
using UnityEngine;
namespace DiceFree.Visuals
{
    [DefaultExecutionOrder(550)]
    public sealed class WolfGreatswordPose : MonoBehaviour,IPortraitPoseable
    {
        [SerializeField] Transform visualRoot;
        [SerializeField] Animator animator;
        [SerializeField] GreatbladeTwoHandGrip grip;
        public void Configure(Transform root,Animator model){visualRoot=root;animator=model;grip=root.GetComponentInChildren<GreatbladeTwoHandGrip>(true);}
        void LateUpdate()=>ApplyPortraitPose();
        public void ApplyPortraitPose(){
            if(visualRoot==null)return;
            if(grip!=null&&grip.DominantGrip!=null){
                transform.SetPositionAndRotation(grip.DominantGrip.position,grip.Weapon.rotation);return;
            }
            var hand=animator!=null&&animator.isHuman?animator.GetBoneTransform(HumanBodyBones.RightHand):null;
            transform.SetPositionAndRotation(hand!=null?hand.position:visualRoot.TransformPoint(new Vector3(.4f,1,0)),visualRoot.rotation);
            if(hand==null)return;
            var upper=animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);var elbow=animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);var left=animator.GetBoneTransform(HumanBodyBones.LeftHand);
            if(upper!=null&&elbow!=null&&left!=null)GreatbladeTwoHandGrip.SolveArm(upper,elbow,left,
                transform.TransformPoint(new Vector3(0,-.22f,0)),hand.rotation,-visualRoot.right);
        }
    }
}
