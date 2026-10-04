using DiceFree.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.UI
{
    /// <summary>Drives a class-form Animator from existing actor state without owning gameplay behavior.</summary>
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

        public Transform VisualRoot => visualRoot;
        public Animator VisualAnimator => animator;

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            attack = GetComponent<BasicAttack>();
            agent = GetComponent<NavMeshAgent>();
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
            if (visualRoot == null && animator != null) visualRoot = animator.transform;
            if (animator == null)
            {
                Debug.LogError("Novice presentation has no Animator.", this);
                enabled = false;
                return;
            }
            wasAlive = actor.Alive;
        }

        private void Update()
        {
            if (animator == null || actor == null) return;
            bool alive = actor.Alive;
            if (!alive)
            {
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
            else animator.speed = 1f;

            float speed = agent != null && agent.enabled && agent.isOnNavMesh ? agent.velocity.magnitude : 0f;
            animator.SetFloat(SpeedId, speed);
            bool winding = attack != null && attack.State == "Wind-up";
            if (winding && !wasWinding) animator.SetTrigger(AttackId);
            wasWinding = winding;
        }

        public void Configure(Transform modelRoot, Animator modelAnimator)
        {
            visualRoot = modelRoot;
            animator = modelAnimator;
        }
    }
}
