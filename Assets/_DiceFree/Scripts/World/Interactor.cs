using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    [RequireComponent(typeof(CombatActor),typeof(BasicAttack),typeof(InteractionRegistry))]
    public sealed class Interactor : MonoBehaviour
    {
        private CombatActor actor;
        private BasicAttack attack;
        private InteractionRegistry registry;
        private InteractionTarget pending;
        public InteractionTarget Active { get; private set; }
        private void Awake()
        {
            actor = GetComponent<CombatActor>(); attack = GetComponent<BasicAttack>();
            registry = GetComponent<InteractionRegistry>();
            if (registry == null) registry = gameObject.AddComponent<InteractionRegistry>();
        }
        private void OnEnable() { if (registry != null) registry.RegisterExistingSceneTargets(); }
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
            if (!registry.Contains(target)) return false;
            if (!target.Available(actor)) return false;
            attack.Cancel(); Active = null; pending = null;
            if (!target.CanInteract(actor) && !actor.Motor.MoveTo(target.ApproachPosition)) return false;
            pending = target; return true;
        }
        public void Cancel() { pending = null; Active = null; }
        public bool InteractNearest()
        {
            var nearest = registry.NearestInRange(actor);
            return nearest != null && Order(nearest);
        }
        public bool ContextInteract(Ray ray)
        {
            if (!Physics.Raycast(ray,out var hit,1500,
                (1<<8)|(1<<9)|(1<<10)|(1<<11),QueryTriggerInteraction.Ignore)) return false;
            var target = hit.collider.GetComponentInParent<InteractionTarget>();
            if (target == null) return false;
            registry.Register(target.WorldTarget); // Explicitly clicked transient targets join this session catalog.
            return Order(target);
        }
    }
}
