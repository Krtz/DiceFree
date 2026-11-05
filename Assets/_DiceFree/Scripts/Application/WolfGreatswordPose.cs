using DiceFree.UI;
using DiceFree.Items;
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
            if(visualRoot==null||!gameObject.activeInHierarchy)return;
            var equipment=GetComponentInParent<Equipment>();
            if(equipment!=null&&!equipment.UsesTwoHandedStance)return;
            if(grip!=null&&grip.DominantGrip!=null&&grip.Weapon!=null&&grip.Weapon.gameObject.activeInHierarchy){
                transform.SetPositionAndRotation(grip.DominantGrip.position,grip.Weapon.rotation);return;
            }
            var hand=animator!=null&&animator.isHuman?animator.GetBoneTransform(HumanBodyBones.RightHand):null;
            transform.SetPositionAndRotation(hand!=null?hand.position:visualRoot.TransformPoint(new Vector3(.4f,1,0)),visualRoot.rotation);
            if(hand==null)return;
            var upper=animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);var elbow=animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);var left=animator.GetBoneTransform(HumanBodyBones.LeftHand);
            var rightUpper=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);var rightElbow=animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            if(upper==null||elbow==null||left==null||rightUpper==null||rightElbow==null)return;
            // Fit the fixed handle into both arm reach spheres before solving either arm.
            // A lowered one-hand FK pose may leave the other hand unable to reach it.
            var separation=transform.TransformVector(new Vector3(0,-.22f,0));var position=hand.position;var wrist=hand.rotation;
            float rightReach=Vector3.Distance(rightUpper.position,rightElbow.position)+Vector3.Distance(rightElbow.position,hand.position)-.001f;
            float leftReach=Vector3.Distance(upper.position,elbow.position)+Vector3.Distance(elbow.position,left.position)-.001f;
            Vector3 Fit(Vector3 p,Vector3 origin,float reach){var delta=p-origin;return delta.sqrMagnitude>reach*reach?origin+delta.normalized*reach:p;}
            for(int i=0;i<8;i++){position=Fit(position,upper.position-separation,leftReach);position=Fit(position,rightUpper.position,rightReach);}
            GreatbladeTwoHandGrip.SolveArm(rightUpper,rightElbow,hand,position,wrist,visualRoot.right);
            transform.position=hand.position;
            GreatbladeTwoHandGrip.SolveArm(upper,elbow,left,transform.position+separation,wrist,-visualRoot.right);
        }
    }
}
