using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Visuals
{
    // The shared caster idle clasps both hands inside these narrower bodies.
    // Retarget the arms from this mesh's own reference rotations. The shared
    // caster clips contain arm twists that do not fit these vendor skeletons.
    // Body/leg animation and gameplay timing remain driven by the controller.
    [DefaultExecutionOrder(365),DisallowMultipleComponent,RequireComponent(typeof(Animator))]
    public sealed class PsionEvolutionHandPose : MonoBehaviour,IPortraitBonePose
    {
        [SerializeField] private Quaternion leftWrist=Quaternion.identity,rightWrist=Quaternion.identity;
        [SerializeField] private Vector3 leftPalm,rightPalm,leftPalmNormal;
        [SerializeField] private Quaternion leftUpper,leftLower,rightUpper,rightLower;
        private Animator animator;
        public void Configure(Quaternion leftLocal,Quaternion rightLocal,Vector3 leftPalmPoint,Vector3 rightPalmPoint,Vector3 palmNormal,
            Quaternion leftUpperReference,Quaternion leftLowerReference,Quaternion rightUpperReference,Quaternion rightLowerReference)
        {leftWrist=leftLocal;rightWrist=rightLocal;leftPalm=leftPalmPoint;rightPalm=rightPalmPoint;leftPalmNormal=palmNormal;
            leftUpper=leftUpperReference;leftLower=leftLowerReference;rightUpper=rightUpperReference;rightLower=rightLowerReference;}
        public Vector3 PalmPoint(bool left)
        {
            animator??=GetComponent<Animator>();
            return animator.GetBoneTransform(left?HumanBodyBones.LeftHand:HumanBodyBones.RightHand).TransformPoint(left?leftPalm:rightPalm);
        }
        public void ApplyPortraitPose()=>Apply(false);
        private void LateUpdate()=>Apply(false);
        public void PoseForFitting()=>Apply(true);
        private void Apply(bool fitting)
        {
            animator??=GetComponent<Animator>();
            if(!gameObject.activeInHierarchy||!animator.isHuman||!animator.isInitialized)return;
            var actor=GetComponentInParent<CombatActor>();if(!fitting&&actor!=null&&!actor.Alive)return;
            var equipment=GetComponentInParent<Equipment>();
            if(!fitting&&equipment?.UsesTwoHandedStance==true)return;
            float gesture=0;
            if(!fitting)
            {
                var state=animator.IsInTransition(0)?animator.GetNextAnimatorStateInfo(0):animator.GetCurrentAnimatorStateInfo(0);
                if(state.IsName("Cast"))gesture=Mathf.Sin(Mathf.Clamp01(state.normalizedTime)*Mathf.PI);
                else if(!state.IsName("Idle")&&!state.IsName("Locomotion"))return;
            }
            bool focus=fitting||equipment?.ResolvedDefinition(EquipmentSlot.OffHand)!=null;
            bool main=fitting||equipment?.ResolvedDefinition(EquipmentSlot.MainHand)!=null;
            Arm(true,focus,gesture);Arm(false,main,gesture);
        }
        private void Arm(bool left,bool held,float gesture)
        {
            var upper=animator.GetBoneTransform(left?HumanBodyBones.LeftUpperArm:HumanBodyBones.RightUpperArm);
            var lower=animator.GetBoneTransform(left?HumanBodyBones.LeftLowerArm:HumanBodyBones.RightLowerArm);
            var hand=animator.GetBoneTransform(left?HumanBodyBones.LeftHand:HumanBodyBones.RightHand);
            if(upper==null||lower==null||hand==null)return;
            upper.localRotation=left?leftUpper:rightUpper;
            lower.localRotation=left?leftLower:rightLower;
            hand.localRotation=left?leftWrist:rightWrist;
            float a=Vector3.Distance(upper.position,lower.position),b=Vector3.Distance(lower.position,hand.position),sign=left?-1:1;
            if(a<.001f||b<.001f)return;
            var offset=held?(left?new Vector3(sign*.72f,-.55f,.35f):new Vector3(sign*.60f,-.78f,.16f)):new Vector3(sign*.60f,-.78f,.10f);
            var delta=transform.rotation*offset*(a+b);float distance=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.001f,a+b-.001f);var direction=delta.normalized;
            var bend=Vector3.ProjectOnPlane(transform.right*sign+transform.forward*.8f,direction).normalized;
            float along=(a*a+distance*distance-b*b)/(2*distance),away=Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
            var elbow=upper.position+direction*along+bend*away;
            var target=upper.position+direction*distance;
            upper.rotation=Quaternion.FromToRotation(lower.position-upper.position,elbow-upper.position)*upper.rotation;
            lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,target-lower.position)*lower.rotation;
            hand.localRotation=left?leftWrist:rightWrist;
            if(left&&held&&leftPalm.sqrMagnitude>.000001f)
                hand.rotation=Quaternion.LookRotation(transform.forward,transform.up)*Quaternion.Inverse(Quaternion.LookRotation(leftPalm.normalized,leftPalmNormal));
            if(gesture>0)upper.rotation=Quaternion.AngleAxis(-25*gesture,transform.right)*upper.rotation;
        }
    }
}
