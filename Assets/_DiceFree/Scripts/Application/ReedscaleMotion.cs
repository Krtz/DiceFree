using DiceFree.Combat;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.Verdant
{
    [RequireComponent(typeof(CombatActor), typeof(BasicAttack))]
    public sealed class ReedscaleMotion : MonoBehaviour
    {
        [SerializeField] private Transform leftLeg, rightLeg, leftArm, rightArm, tail, weapon, muzzle;
        [SerializeField] private bool bow;

        private CombatActor actor;
        private BasicAttack attack;
        private Vector3 previousPosition;
        private float stride, windupStarted, windupDuration;
        private bool winding, arrowReleased;
        private Quaternion weaponRest;

        public void Configure(Transform legL, Transform legR, Transform armL, Transform armR,
            Transform tailPivot, Transform grip, Transform shotOrigin, bool ranged)
        {
            leftLeg = legL;
            rightLeg = legR;
            leftArm = armL;
            rightArm = armR;
            tail = tailPivot;
            weapon = grip;
            muzzle = shotOrigin;
            bow = ranged;
        }

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            attack = GetComponent<BasicAttack>();
            previousPosition = transform.position;
            weaponRest = weapon.localRotation;
        }

        private void LateUpdate()
        {
            float movement = Vector3.Distance(transform.position, previousPosition);
            previousPosition = transform.position;
            if (!actor.Alive) { winding = false; return; }

            bool moving = movement > .006f && movement < 2f;
            if (moving) stride += movement * 5.2f;
            float step = moving ? Mathf.Sin(stride) * 30 : 0;
            bool startingWindup = attack.State == "Wind-up";
            if (startingWindup && !winding)
            {
                windupStarted = Time.time;
                windupDuration = Mathf.Max(.08f, attack.Definition.windup / actor.Stats.AttackSpeed);
                arrowReleased = false;
            }
            winding = startingWindup;
            float draw = winding ? Mathf.Clamp01((Time.time - windupStarted) / windupDuration) : 0;
            float brace = Mathf.Sin(draw * Mathf.PI);
            Pose(leftLeg, step, 0);
            Pose(rightLeg, -step, 0);
            Pose(leftArm, -step * .32f - brace * (bow ? 34 : 12), bow ? brace * -12 : 0);
            Pose(rightArm, step * .32f + brace * (bow ? 21 : -40), bow ? brace * 18 : 0);
            Pose(tail, 0, moving ? step * .52f : Mathf.Sin(Time.time * 1.5f) * 5);
            weapon.localRotation = weaponRest * Quaternion.Euler(bow ? -brace * 13 : 0, 0, 0);

            if (bow && winding && !arrowReleased && draw >= .5f && attack.Target != null)
            {
                arrowReleased = true;
                RangerVfx.Arrow(muzzle.position, attack.Target.transform.position + Vector3.up,
                    Mathf.Max(.06f, (1 - draw) * windupDuration), .62f, RangerVfx.ArrowTint);
            }
        }

        private static void Pose(Transform joint, float pitch, float yaw)
        {
            joint.localRotation = Quaternion.Slerp(joint.localRotation,
                Quaternion.Euler(pitch, yaw, 0), Mathf.Clamp01(Time.deltaTime * 16));
        }
    }
}
