using DiceFree.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.UI
{
    /// <summary>Drives the active class presentation from gameplay state without owning movement.</summary>
    [DefaultExecutionOrder(250)]
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor))]
    public sealed class NovicePresentationDriver : MonoBehaviour
    {
        private static readonly int SpeedId = Animator.StringToHash("Speed");
        private static readonly int AttackId = Animator.StringToHash("Attack");
        private static readonly int IdleId = Animator.StringToHash("Base Layer.Idle");

        [SerializeField] private Transform visualRoot;
        [SerializeField] private Animator animator;

        private CombatActor actor;
        private BasicAttack attack;
        private NavMeshAgent agent;
        private bool wasWinding;
        private bool wasAlive;
        private Vector3 previousPosition;
        private bool hasPreviousPosition;
        private Transform leftUpperArm;
        private Transform rightUpperArm;
        private Transform leftLowerArm,rightLowerArm;

        public Transform VisualRoot => visualRoot;
        public Animator VisualAnimator => animator;
        public float LastDrivenSpeed { get; private set; }

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            attack = GetComponent<BasicAttack>();
            agent = GetComponent<NavMeshAgent>();
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
            if (visualRoot == null && animator != null) visualRoot = animator.transform;
            if (animator == null)
            {
                Debug.LogError("Player presentation has no Animator.", this);
                enabled = false;
                return;
            }

            PrepareAnimator(animator, false);
            CacheHumanoidBones();
            previousPosition = transform.position;
            hasPreviousPosition = true;
            wasAlive = actor.Alive;
        }

        private void LateUpdate()
        {
            if (animator == null || actor == null) return;

            float speed = ResolveMovementSpeed();
            bool alive = actor.Alive;
            if (!alive)
            {
                LastDrivenSpeed = 0;
                animator.SetFloat(SpeedId, 0f);
                animator.speed = 0f;
                wasWinding = false;
                wasAlive = false;
                return;
            }

            if (!wasAlive)
            {
                animator.speed = 1f;
                animator.ResetTrigger(AttackId);
                animator.Play(IdleId, 0, 0f);
                wasAlive = true;
            }
            else
            {
                animator.speed = 1f;
            }

            LastDrivenSpeed = speed;
            animator.SetFloat(SpeedId, speed, 0.06f, Mathf.Max(0.0001f, Time.deltaTime));

            bool winding = attack != null && attack.State == "Wind-up";
            if (winding && !wasWinding) animator.SetTrigger(AttackId);
            wasWinding = winding;

            bool attacking = winding || animator.GetCurrentAnimatorStateInfo(0).IsName("UnarmedAttack");
            if (!attacking)
            {
                if(speed<=.03f)ApplyRelaxedIdleArms();
                else ApplyLocomotionArmSwing(speed);
            }
        }

        private float ResolveMovementSpeed()
        {
            Vector3 current = transform.position;
            if (!hasPreviousPosition)
            {
                previousPosition = current;
                hasPreviousPosition = true;
                return 0;
            }

            float deltaTime = Time.deltaTime;
            Vector3 displacement = Vector3.ProjectOnPlane(current - previousPosition, Vector3.up);
            previousPosition = current;

            float measured = deltaTime > 0.0001f ? displacement.magnitude / deltaTime : 0f;
            float expected = actor?.Stats == null ? 5f : Mathf.Max(0.1f, actor.Stats.MoveSpeed);

            // Warp/load/respawn should not look like one enormous locomotion frame.
            if (displacement.magnitude > Mathf.Max(1.5f, expected * Mathf.Max(deltaTime, 0.02f) * 4f))
                measured = 0f;

            float nav = agent != null && agent.enabled && agent.isOnNavMesh
                ? Vector3.ProjectOnPlane(agent.velocity, Vector3.up).magnitude
                : 0f;

            float result = Mathf.Max(measured, nav);
            return result < 0.03f ? 0f : result;
        }

        public void Configure(Transform modelRoot, Animator modelAnimator)
        {
            visualRoot = modelRoot;
            animator = modelAnimator;
            hasPreviousPosition = false;
            LastDrivenSpeed = 0;

            if (animator != null)
            {
                PrepareAnimator(animator, Application.isPlaying);
                CacheHumanoidBones();
            }
        }

        public void PlayAttackAnimation()
        {
            if (animator == null || !animator.isActiveAndEnabled) return;
            animator.ResetTrigger(AttackId);
            animator.SetTrigger(AttackId);
        }

        private void CacheHumanoidBones()
        {
            leftUpperArm = animator != null && animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.LeftUpperArm)
                : null;
            rightUpperArm = animator != null && animator.isHuman
                ? animator.GetBoneTransform(HumanBodyBones.RightUpperArm)
                : null;
            leftLowerArm=animator!=null&&animator.isHuman?animator.GetBoneTransform(HumanBodyBones.LeftLowerArm):null;
            rightLowerArm=animator!=null&&animator.isHuman?animator.GetBoneTransform(HumanBodyBones.RightLowerArm):null;
        }
        private void ApplyRelaxedIdleArms()
        {
            float breath=Mathf.Sin(Time.time*1.6f);
            RelaxArm(leftUpperArm,leftLowerArm,transform.TransformDirection(new Vector3(-.24f,-1,.08f+breath*.035f)));
            RelaxArm(rightUpperArm,rightLowerArm,transform.TransformDirection(new Vector3(.24f,-1,.08f-breath*.035f)));
        }
        private static void RelaxArm(Transform upper,Transform lower,Vector3 desired)
        {
            if(upper==null||lower==null)return;
            var current=lower.position-upper.position;
            if(current.sqrMagnitude>.000001f)upper.rotation=Quaternion.FromToRotation(current,desired)*upper.rotation;
        }

        private void ApplyLocomotionArmSwing(float speed)
        {
            if (leftUpperArm == null || rightUpperArm == null || speed <= 0.03f) return;

            float expected = actor?.Stats == null ? 5f : Mathf.Max(0.1f, actor.Stats.MoveSpeed);
            float movement = Mathf.Clamp01(speed / expected);
            float frequency = Mathf.Lerp(4.5f, 7.5f, movement);
            float amplitude = Mathf.Lerp(7f, 20f, movement);
            float swing = Mathf.Sin(Time.time * frequency) * amplitude;
            Quaternion left = Quaternion.AngleAxis(swing, transform.right);
            Quaternion right = Quaternion.AngleAxis(-swing, transform.right);
            leftUpperArm.rotation = left * leftUpperArm.rotation;
            rightUpperArm.rotation = right * rightUpperArm.rotation;
        }

        private static void PrepareAnimator(Animator value, bool rebind)
        {
            value.applyRootMotion = false;
            value.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            value.updateMode = AnimatorUpdateMode.Normal;
            value.speed = 1f;

            if (!rebind) return;
            value.enabled = true;
            value.Rebind();
            value.Update(0f);
            value.Play(IdleId, 0, 0f);
            value.Update(0f);
        }
    }
}
