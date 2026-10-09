using UnityEngine;

namespace DiceFree.UI
{
    // Wrist height follows animation; torso-space side/front limits prevent a bent arm
    // from carrying the shield through the back. This affects presentation only.
    [DefaultExecutionOrder(200)]
    public sealed class ShieldSideAttachment : MonoBehaviour
    {
        [SerializeField] private Transform hero;
        [SerializeField] private Animator rig;
        public void Configure(Transform actor, Animator animator) { hero = actor; rig = animator; Align(); }
        private void LateUpdate() => Align();
        public void Align()
        {
            if (hero == null || rig == null || !rig.isHuman) return;
            var hand = rig.GetBoneTransform(HumanBodyBones.LeftHand);
            if (hand == null) return;
            var p = hero.InverseTransformPoint(hand.position);
            p.x = Mathf.Min(p.x - .12f, -.55f);
            p.y = Mathf.Clamp(p.y, .70f, 1.45f);
            p.z = Mathf.Max(p.z + .15f, .35f);
            transform.position = hero.TransformPoint(p);
            transform.rotation = Quaternion.LookRotation(hero.forward * .8f - hero.right * .6f, hero.up);
        }
    }
}
