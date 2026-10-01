using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    [RequireComponent(typeof(CombatActor),typeof(BasicAttack))]
    public sealed class Interactor : MonoBehaviour
    {
        private CombatActor actor;
        private BasicAttack attack;
        private InteractionTarget pending;
        public InteractionTarget Active { get; private set; }
        private void Awake()
        {
            actor = GetComponent<CombatActor>(); attack = GetComponent<BasicAttack>();
        }
        private void OnDisable() => Cancel();
        private void Update()
        {
            if (!actor.Alive || attack.Target != null) { Cancel(); return; }
            if (Active != null && !Active.CanInteract(actor)) Active = null;
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
        public bool InteractNearest()
        {
            InteractionTarget nearest = null; float distance = float.PositiveInfinity;
            foreach (var target in FindObjectsByType<InteractionTarget>())
            {
                if (target.WorldTarget != target) continue;
                float d = (target.transform.position - transform.position).sqrMagnitude;
                if (target.CanInteract(actor) && d < distance) { nearest = target; distance = d; }
            }
            return nearest != null && Order(nearest);
        }
        public bool ContextInteract(Ray ray)
        {
            if (!Physics.Raycast(ray,out var hit,1500,
                (1<<8)|(1<<9)|(1<<10)|(1<<11),QueryTriggerInteraction.Ignore)) return false;
            var target = hit.collider.GetComponentInParent<InteractionTarget>();
            return target != null && Order(target);
        }
    }
}
