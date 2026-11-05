using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.UI;
using UnityEngine;
namespace DiceFree.Visuals
{
    // Keep a held shield/focus on its own side; do not use the sword support grip.
    // Runs before bone-following equipment attachments, after animation evaluation.
    [DefaultExecutionOrder(350),DisallowMultipleComponent,RequireComponent(typeof(Animator))]
    public sealed class IndependentOffhandPose : MonoBehaviour,IPortraitBonePose
    {
        public void ApplyPortraitPose()
        {
            if(!gameObject.activeInHierarchy)return;
            var equipment=GetComponentInParent<Equipment>();
            if(equipment==null||equipment.UsesTwoHandedStance||equipment.ResolvedDefinition(EquipmentSlot.OffHand)==null)return;
            var actor=GetComponentInParent<CombatActor>();if(actor!=null&&!actor.Alive)return;
            var model=GetComponent<Animator>();if(!model.isHuman||!model.isInitialized)return;
            var upper=model.GetBoneTransform(HumanBodyBones.LeftUpperArm);var elbow=model.GetBoneTransform(HumanBodyBones.LeftLowerArm);var hand=model.GetBoneTransform(HumanBodyBones.LeftHand);
            if(upper==null||elbow==null||hand==null)return;
            float reach=Vector3.Distance(upper.position,elbow.position)+Vector3.Distance(elbow.position,hand.position);
            var target=upper.position-transform.right*reach*.20f-transform.up*reach*.50f+transform.forward*reach*.40f;
            GreatbladeTwoHandGrip.SolveArm(upper,elbow,hand,target,hand.rotation,-transform.right);
        }
        void LateUpdate()=>ApplyPortraitPose();
    }
}
