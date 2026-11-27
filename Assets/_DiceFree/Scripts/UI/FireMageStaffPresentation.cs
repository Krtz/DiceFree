using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.UI
{
    [DefaultExecutionOrder(600),RequireComponent(typeof(Animator))]
    public sealed class FireMageStaffPresentation:MonoBehaviour,IPortraitPoseable
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
            // Keep the long staff upright in the casting hand; follow the hand
            // on death so it falls with the body rather than floating beside it.
            var rotation=actor!=null&&!actor.Alive?hand.rotation:Quaternion.LookRotation(transform.forward,Vector3.up)*Quaternion.Euler(0,0,12);
            staff.SetPositionAndRotation(hand.position,rotation);
        }
        public void ApplyPortraitPose()=>Pose();
        public void Configure(Transform value)=>staff=value;
    }
}
