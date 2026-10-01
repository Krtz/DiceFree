using DiceFree.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.World
{
    [RequireComponent(typeof(CombatActor),typeof(BasicAttack))]
    public sealed class Interactor : MonoBehaviour
    {
        private CombatActor actor;
        private BasicAttack attack;
        private InteractionTarget pending;
        private InputAction interact, close;
        public InteractionTarget Active { get; private set; }
        private void Awake()
        {
            actor = GetComponent<CombatActor>(); attack = GetComponent<BasicAttack>();
            interact = new InputAction("Interact nearby",binding:"<Keyboard>/i");
            close = new InputAction("Close interaction",binding:"<Keyboard>/escape");
        }
        private void OnEnable() { interact.Enable(); close.Enable(); }
        private void OnDisable() { interact.Disable(); close.Disable(); Cancel(); }
        private void OnDestroy() { interact.Dispose(); close.Dispose(); }
        private void Update()
        {
            if (!actor.Alive || attack.Target != null || close.WasPressedThisFrame()) { Cancel(); return; }
            if (Active != null && !Active.CanInteract(actor)) Active = null;
            if (interact.WasPressedThisFrame())
            {
                InteractionTarget nearest = null; float distance = float.PositiveInfinity;
                foreach (var target in FindObjectsByType<InteractionTarget>())
                {
                    if (target.WorldTarget != target) continue;
                    float d = (target.transform.position-transform.position).sqrMagnitude;
                    if (target.CanInteract(actor) && d < distance) { nearest=target; distance=d; }
                }
                if (nearest != null) Order(nearest);
            }
            if (pending != null && pending.CanInteract(actor))
            {
                actor.Motor.Stop(); Active = pending.Resolve(actor); pending = null;
                if (Active != null) Active.Interact(actor);
            }
            else if (pending != null && !actor.Motor.Travelling) pending = null;
        }
        public bool Order(InteractionTarget target)
        {
            if (!actor.Alive || target == null) return false;
            target = target.WorldTarget;
            if (!target.Available(actor)) return false;
            attack.Cancel(); Active = null; pending = null;
            if (!target.CanInteract(actor) && !actor.Motor.MoveTo(target.ApproachPosition)) return false;
            pending = target; return true;
        }
        public void Cancel() { pending = null; Active = null; }
        public bool ContextInteract(Vector2 point)
        {
            if (Camera.main == null || !Physics.Raycast(Camera.main.ScreenPointToRay(point),out var hit,1500,
                (1<<8)|(1<<9)|(1<<10)|(1<<11),QueryTriggerInteraction.Ignore)) return false;
            var target = hit.collider.GetComponentInParent<InteractionTarget>();
            return target != null && Order(target);
        }
    }
}
