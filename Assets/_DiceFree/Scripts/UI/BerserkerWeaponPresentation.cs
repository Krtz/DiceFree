using DiceFree.Combat;
using DiceFree.Items;
using UnityEngine;

namespace DiceFree.UI
{
    // A shared shaft follows both animated hands. The left hand is constrained
    // onto the handle rather than leaving an offhand floating beside the axe.
    [RequireComponent(typeof(Animator))]
    public sealed class BerserkerWeaponPresentation : MonoBehaviour
    {
        [SerializeField] private Transform weaponRoot;
        [SerializeField] private GameObject axe,sword;
        private Animator animator;
        private Equipment equipment;
        private CombatActor actor;
        private void Awake(){animator=GetComponent<Animator>();equipment=GetComponentInParent<Equipment>();actor=GetComponentInParent<CombatActor>();}
        private void LateUpdate()=>Pose();
        private void Pose()
        {
            if(animator==null||!animator.isHuman||weaponRoot==null)return;
            var right=animator.GetBoneTransform(HumanBodyBones.RightHand);
            var left=animator.GetBoneTransform(HumanBodyBones.LeftHand);
            if(right==null||left==null)return;
            bool useSword=equipment?.ResolvedDefinition(EquipmentSlot.MainHand)?.greatWeaponFamily==GreatWeaponFamily.TwoHandedSword;
            if(axe!=null)axe.SetActive(!useSword);if(sword!=null)sword.SetActive(useSword);
            Vector3 shaft=left.position-right.position;
            if(shaft.sqrMagnitude<.01f)shaft=transform.up;
            shaft.Normalize();
            var face=Vector3.ProjectOnPlane(transform.forward,shaft);
            if(face.sqrMagnitude<.01f)face=Vector3.ProjectOnPlane(transform.right,shaft);
            weaponRoot.SetPositionAndRotation(right.position,Quaternion.LookRotation(face,shaft));
        }
        private void OnAnimatorIK(int layer)
        {
            if(animator==null||!animator.isHuman||weaponRoot==null||actor!=null&&!actor.Alive)return;
            Pose();
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand,weaponRoot.TransformPoint(Vector3.up*.32f));
        }
        public void Configure(Transform root,GameObject axeVisual,GameObject swordVisual){weaponRoot=root;axe=axeVisual;sword=swordVisual;}
    }
}
