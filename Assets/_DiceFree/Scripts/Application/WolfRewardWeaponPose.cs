using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Visuals
{
    /// <summary>Separate hand props use one hand; the healer staff alone fits a two-hand grip.</summary>
    [DefaultExecutionOrder(375)]
    public sealed class WolfRewardWeaponPose : MonoBehaviour, IPortraitBonePose
    {
        [SerializeField] Transform visualRoot;
        [SerializeField] Animator animator;
        [SerializeField] bool twoHanded;
        [SerializeField] Quaternion gripRotation;
        [SerializeField] Transform rightGrip, leftGrip, leftHand, leftForearm, leftUpperArm;
        public bool TwoHanded=>twoHanded;
        public void Configure(Transform root,Animator model,bool both)
        {
            visualRoot=root;animator=model;twoHanded=both;
            var hand=model.GetBoneTransform(HumanBodyBones.RightHand);
            gripRotation=Quaternion.Inverse(hand.rotation)*root.rotation*Quaternion.Euler(-12,0,-18);
        }
        public void ConfigureSockets(Transform root, Transform right, Transform left, Transform wrist,
            Transform forearm, Transform upperArm, bool both)
        {
            visualRoot = root;
            rightGrip = right;
            leftGrip = left;
            leftHand = wrist;
            leftForearm = forearm;
            leftUpperArm = upperArm;
            twoHanded = both;
        }

        void LateUpdate()=>ApplyPortraitPose();
        public void ApplyPortraitPose()
        {
            if(visualRoot==null||!gameObject.activeInHierarchy)return;
            if(rightGrip!=null)
            {
                transform.SetPositionAndRotation(rightGrip.position,
                    Quaternion.LookRotation(rightGrip.forward, rightGrip.up));
                if(twoHanded && leftGrip!=null && leftHand!=null && leftForearm!=null && leftUpperArm!=null)
                {
                    Vector3 wristPosition = transform.TransformPoint(Vector3.down * .27f) +
                        leftHand.position - leftGrip.position;
                    GreatbladeTwoHandGrip.SolveArm(leftUpperArm,leftForearm,leftHand,wristPosition,
                        leftHand.rotation,-visualRoot.right);
                }
                return;
            }
            if(animator==null||!animator.isHuman)return;
            var right=animator.GetBoneTransform(HumanBodyBones.RightHand);if(right==null)return;
            if(!twoHanded)
            {
                // Bind to animated wrist rather than a fixed world-up prop during an attack.
                transform.SetPositionAndRotation(right.position,right.rotation*gripRotation);
                return;
            }
            transform.SetPositionAndRotation(right.position,visualRoot.rotation*Quaternion.Euler(-8,0,-8));
            var upper=animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            var elbow=animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            var left=animator.GetBoneTransform(HumanBodyBones.LeftHand);
            var ru=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            var re=animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            if(upper==null||elbow==null||left==null||ru==null||re==null)return;
            // Imported rigs can have nonuniform scale. Re-measure after solving rotations,
            // since their world-space arm lengths can change during a cast or attack.
            for(int pass=0;pass<5;pass++)
            {
                var offset=transform.TransformVector(Vector3.down*.22f);
                float rr=Vector3.Distance(ru.position,re.position)+Vector3.Distance(re.position,right.position)-.001f;
                float lr=Vector3.Distance(upper.position,elbow.position)+Vector3.Distance(elbow.position,left.position)-.001f;
                float rightMin=Mathf.Abs(Vector3.Distance(ru.position,re.position)-Vector3.Distance(re.position,right.position))+.001f;
                float leftMin=Mathf.Abs(Vector3.Distance(upper.position,elbow.position)-Vector3.Distance(elbow.position,left.position))+.001f;
                Vector3 Fit(Vector3 p,Vector3 from,float min,float max)
                {
                    var delta=p-from;
                    return from+(delta.sqrMagnitude>.000001f?delta.normalized:visualRoot.forward)*Mathf.Clamp(delta.magnitude,min,max);
                }
                var position=right.position;var wrist=right.rotation;
                // The inner reach matters too: a folded attack can put the support grip closer
                // to the shoulder than the difference between upper and lower arm lengths.
                for(int i=0;i<32;i++){position=Fit(position,upper.position-offset,leftMin,lr);position=Fit(position,ru.position,rightMin,rr);}
                GreatbladeTwoHandGrip.SolveArm(ru,re,right,position,wrist,visualRoot.right);
                transform.position=right.position;
                GreatbladeTwoHandGrip.SolveArm(upper,elbow,left,transform.position+offset,wrist,-visualRoot.right);
            }
        }
    }
}
