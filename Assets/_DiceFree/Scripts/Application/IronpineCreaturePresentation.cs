using System;
using DiceFree.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Ironpine
{
    // Controls authored quadruped FBX animation clips. Locomotion uses actual agent
    // velocity; specials are called at the moment wildlife AI commits to an action.
    // No root motion: movement/damage remain authoritative in DiceFree's combat system.
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(IronpineWildlife))]
    public sealed class IronpineCreaturePresentation : MonoBehaviour
    {
        [SerializeField] private Animator modelAnimator;
        private CombatActor actor;
        private BasicAttack basicAttack;
        private NavMeshAgent nav;
        private float lockedUntil;
        private float nextIdleBehavior;
        private string current;
        private bool windingLastFrame;
        private bool dead;
        private float woundedFlashUntil;
        private Vector3 priorPosition;
        private bool hasPriorPosition;

        public Animator ModelAnimator => modelAnimator;
        public string CurrentState => current;

        public void Configure(Animator visual)
        {
            modelAnimator = visual;
            if (modelAnimator != null)
            {
                modelAnimator.applyRootMotion = false;
                modelAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                modelAnimator.updateMode = AnimatorUpdateMode.Normal;
            }
        }
        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            basicAttack = GetComponent<BasicAttack>();
            nav = GetComponent<NavMeshAgent>();
            nextIdleBehavior = Time.time + UnityEngine.Random.Range(7f, 16f);
            priorPosition = transform.position; hasPriorPosition = true;
        }
        private void OnEnable()
        {
            if (actor == null) actor = GetComponent<CombatActor>();
            actor.Health.Damaged += Damaged;
            actor.Health.Restored += Restored;
        }
        private void OnDisable()
        {
            if (actor == null) return;
            actor.Health.Damaged -= Damaged;
            actor.Health.Restored -= Restored;
        }
        private void Restored()
        {
            dead = false;
            lockedUntil = 0;
            windingLastFrame = false;
            current = null;
            Transition("Idle", .05f);
        }
        private void Damaged(CombatActor source, DamageResult result)
        {
            if (result.applied <= 0f || !actor.Alive) return;
            woundedFlashUntil = Time.time + .3f;
            if (Time.time >= lockedUntil && UnityEngine.Random.value < .45f)
                PlayAction("Hurt", .45f);
        }
        public void PlayAction(string state, float length)
        {
            if (modelAnimator == null || !actor.Alive || string.IsNullOrEmpty(state)) return;
            lockedUntil = Time.time + Mathf.Max(.06f, length);
            Transition(state, .08f, true);
            GetComponent<IronpineCreatureAudio>()?.Cue(state);
        }
        private void Transition(string state, float fade = .15f, bool replay = false)
        {
            if (modelAnimator == null || modelAnimator.runtimeAnimatorController == null) return;
            if (!replay && current == state) return;
            // Our authored AnimatorController maps actions to these stable state names.
            int hash = Animator.StringToHash(state);
            if (!modelAnimator.HasState(0, hash)) return;
            modelAnimator.CrossFade(hash, fade, 0, 0f);
            current = state;
        }
        private void Update()
        {
            if (modelAnimator == null || actor == null) return;
            if (!actor.Alive)
            {
                if (!dead)
                {
                    dead = true;
                    lockedUntil = float.PositiveInfinity;
                    Transition("Death", .12f, true);
                    GetComponent<IronpineCreatureAudio>()?.Cue("Death");
                }
                return;
            }
            if (dead) Restored();
            if (Time.time < lockedUntil)
            {
                priorPosition = transform.position;
                hasPriorPosition = true;
                return;
            }
            bool winding = basicAttack != null && basicAttack.State == "Wind-up";
            if (winding && !windingLastFrame)
            {
                windingLastFrame = true;
                PlayAction("Bite", .48f);
                return;
            }
            windingLastFrame = winding;
            float speed = nav != null && nav.enabled && nav.isOnNavMesh
                ? nav.velocity.magnitude : 0f;
            if(hasPriorPosition && Time.deltaTime > .0001f)
            {
                float moved = Vector3.ProjectOnPlane(transform.position-priorPosition,
                    Vector3.up).magnitude / Time.deltaTime;
                if(moved < 18f) speed = Mathf.Max(speed,moved);
            }
            priorPosition = transform.position;
            hasPriorPosition = true;
            if (speed > 3.4f)
            {
                nextIdleBehavior = Time.time + UnityEngine.Random.Range(6f, 14f);
                Transition("Run");
            }
            else if (speed > .13f)
            {
                nextIdleBehavior = Time.time + UnityEngine.Random.Range(6f, 14f);
                Transition("Walk");
            }
            else if (Time.time >= nextIdleBehavior && !actor.InCombat)
            {
                var kind = GetComponent<IronpineWildlife>().Kind;
                PlayAction(UnityEngine.Random.value < .68f
                    ? (kind == WildlifeKind.Quillrat ? "Forage" : "Sniff")
                    : "Scratch", 1.5f);
                nextIdleBehavior = Time.time + UnityEngine.Random.Range(8f, 22f);
            }
            else Transition("Idle");
        }
    }
}
