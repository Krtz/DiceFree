using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.UI
{
    [DefaultExecutionOrder(600),RequireComponent(typeof(Animator))]
    public sealed class FireMageStaffPresentation:MonoBehaviour,IPortraitBonePose
    {
        [SerializeField]private Transform staff;
        private Animator animator;
        private CombatActor actor;
        private void Awake(){animator=GetComponent<Animator>();actor=GetComponentInParent<CombatActor>();}
        private void LateUpdate()=>Pose();
        private void Pose()
        {
            if(animator==null)animator=GetComponent<Animator>();
            if(staff==null||animator==null||!animator.isHuman)return;
            var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);if(hand==null)return;
            if(actor!=null&&!actor.Alive||animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
            {
                staff.SetPositionAndRotation(hand.position,hand.rotation);
                return;
            }

            var upper=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            var lower=animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            var head=animator.GetBoneTransform(HumanBodyBones.Head);
            if(upper==null||lower==null||head==null)return;
            var forward=Vector3.ProjectOnPlane(transform.forward,Vector3.up).normalized;
            var frame=Quaternion.LookRotation(forward,Vector3.up);
            var right=frame*Vector3.right;
            float side=Vector3.Dot(upper.position-head.position,right)>=0?1:-1;
            right*=side;

            // The shared casting clips bring both hands in front of the chest.
            // Hold the long staff beside the body instead, leaving the other
            // arm's casting animation intact. Rotate joints without stretching.
            float first=Vector3.Distance(upper.position,lower.position);
            float second=Vector3.Distance(lower.position,hand.position);
            float reach=first+second;
            if(first<.0001f||second<.0001f)return;
            var target=upper.position+reach*(right*.68f-Vector3.up*.54f+forward*.32f);
            var direction=(target-upper.position).normalized;
            float distance=Mathf.Clamp(Vector3.Distance(target,upper.position),Mathf.Abs(first-second)+.0001f,reach*.995f);
            target=upper.position+direction*distance;
            var bend=Vector3.ProjectOnPlane(right*.6f-forward,direction).normalized;
            float along=(first*first-second*second+distance*distance)/(2*distance);
            float height=Mathf.Sqrt(Mathf.Max(0,first*first-along*along));
            var elbow=upper.position+direction*along+bend*height;
            var wristRotation=hand.rotation;
            upper.rotation=Quaternion.FromToRotation(lower.position-upper.position,elbow-upper.position)*upper.rotation;
            lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,target-lower.position)*lower.rotation;
            hand.rotation=wristRotation;
            staff.SetPositionAndRotation(hand.position,frame*Quaternion.Euler(0,0,-side*25));
        }
        public void ApplyPortraitPose()=>Pose();
        public void Configure(Transform value)=>staff=value;
    }
}
