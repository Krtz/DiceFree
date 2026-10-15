using UnityEngine;

namespace DiceFree.Visuals
{
    /// <summary>
    /// Keeps both palms on the measured handle after Humanoid retargeting.
    /// Adjusts visual bones only; actor movement and combat remain authoritative.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Animator))]
    public sealed class GreatbladeTwoHandGrip : MonoBehaviour
    {
        [SerializeField] private Transform weapon,dominantGrip,supportGrip;
        // A03 palm vertices centre near X=-.060; the closed fingers wrap
        // on the positive-Y side of the hand, around the handle centre.
        public static readonly Vector3 HandPalm=new Vector3(-.060f,.020f,0);
        [SerializeField] private Vector3 palmInHand=HandPalm;
        [SerializeField] private Quaternion weaponInHand=Quaternion.identity;
        [SerializeField] private Quaternion supportWristInWeapon=Quaternion.identity;
        private Animator animator;

        public Transform Weapon=>weapon;
        public Transform DominantGrip=>dominantGrip;
        public Transform SupportGrip=>supportGrip;
        public Vector3 PalmInHand=>palmInHand;

        public void Configure(Transform sword,Transform upperGrip,
            Transform lowerGrip,Quaternion wrist)
        {
            weapon=sword;dominantGrip=upperGrip;supportGrip=lowerGrip;
            palmInHand=HandPalm;
            weaponInHand=sword.localRotation;
            supportWristInWeapon=wrist;
        }
        private void Awake(){animator=GetComponent<Animator>();}
        private void LateUpdate(){ApplyPose();}
        public Vector3 SupportPalm
        {
            get
            {
                if(animator==null)animator=GetComponent<Animator>();
                var hand=animator!=null && animator.isHuman?
                    animator.GetBoneTransform(HumanBodyBones.LeftHand):null;
                return hand!=null?hand.TransformPoint(palmInHand):transform.position;
            }
        }

        public static void SolveArm(Transform upper,Transform elbow,
            Transform hand,Vector3 target,Quaternion rotation,Vector3 fallbackBend)
        {
            Vector3 origin=upper.position;
            float a=Vector3.Distance(origin,elbow.position);
            float b=Vector3.Distance(elbow.position,hand.position);
            Vector3 reach=target-origin;
            if(a<.001f || b<.001f || reach.sqrMagnitude<.000001f)return;
            Vector3 direction=reach.normalized;
            float distance=Mathf.Clamp(reach.magnitude,
                Mathf.Abs(a-b)+.001f,a+b-.001f);
            Vector3 bend=Vector3.ProjectOnPlane(elbow.position-origin,direction);
            if(bend.sqrMagnitude<.000001f)
                bend=Vector3.ProjectOnPlane(fallbackBend,direction);
            if(bend.sqrMagnitude<.000001f)
                bend=Vector3.ProjectOnPlane(Vector3.forward,direction);
            bend.Normalize();
            float along=(a*a+distance*distance-b*b)/(2f*distance);
            float away=Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
            Vector3 elbowTarget=origin+direction*along+bend*away;
            upper.rotation=Quaternion.FromToRotation(elbow.position-origin,
                elbowTarget-origin)*upper.rotation;
            Vector3 handTarget=origin+direction*distance;
            elbow.rotation=Quaternion.FromToRotation(hand.position-elbow.position,
                handTarget-elbow.position)*elbow.rotation;
            hand.rotation=rotation;
        }

        private static Vector3 Reachable(Vector3 point,Vector3 centre,float radius)
        {
            Vector3 delta=point-centre;
            return delta.sqrMagnitude>radius*radius?
                centre+delta.normalized*radius:point;
        }

        // Read the authored FK pose first. Fit the fixed handle into both
        // arm reach spheres, then solve actual mesh bones after animation.
        // This avoids using Mecanim IK-goal axes as vendor hand-bone axes.
        public void ApplyPose()
        {
            if(animator==null)animator=GetComponent<Animator>();
            if(weapon==null || dominantGrip==null || supportGrip==null ||
                animator==null || !animator.isHuman || !animator.isInitialized)return;
            var ru=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            var re=animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            var rh=animator.GetBoneTransform(HumanBodyBones.RightHand);
            var lu=animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            var le=animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            var lh=animator.GetBoneTransform(HumanBodyBones.LeftHand);
            if(ru==null || re==null || rh==null || lu==null || le==null || lh==null)return;

            Vector3 palm=rh.TransformPoint(palmInHand);
            Vector3 axis=palm-lh.TransformPoint(palmInHand);
            Quaternion baseRotation=rh.rotation*weaponInHand;
            Quaternion swordRotation=axis.sqrMagnitude>.0001f?
                Quaternion.FromToRotation(baseRotation*Vector3.up,axis.normalized)*
                baseRotation:baseRotation;
            Quaternion rightRotation=swordRotation*Quaternion.Inverse(weaponInHand);
            Quaternion leftRotation=swordRotation*supportWristInWeapon;
            Vector3 separation=swordRotation*Vector3.Scale(
                supportGrip.localPosition-dominantGrip.localPosition,weapon.lossyScale);
            float rightReach=Vector3.Distance(ru.position,re.position)+
                Vector3.Distance(re.position,rh.position)-.001f;
            float leftReach=Vector3.Distance(lu.position,le.position)+
                Vector3.Distance(le.position,lh.position)-.001f;
            Vector3 rightCentre=ru.position+rightRotation*palmInHand;
            Vector3 leftCentre=lu.position+leftRotation*palmInHand-separation;
            for(int i=0;i<8;i++)
            {
                palm=Reachable(palm,leftCentre,leftReach);
                palm=Reachable(palm,rightCentre,rightReach);
            }
            SolveArm(ru,re,rh,palm-rightRotation*palmInHand,
                rightRotation,transform.right);
            weapon.rotation=swordRotation;
            weapon.position=palm-swordRotation*
                Vector3.Scale(dominantGrip.localPosition,weapon.lossyScale);
            SolveArm(lu,le,lh,palm+separation-leftRotation*palmInHand,
                leftRotation,-transform.right);
        }
    }
}
