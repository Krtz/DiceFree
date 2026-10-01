using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    public abstract class InteractionTarget : MonoBehaviour
    {
        [SerializeField] private string displayName;
        [SerializeField, Min(0.5f)] private float range = 2.5f;
        [SerializeField] private Transform approachPoint;
        [SerializeField] private ContextualNpc context;
        public InteractionTarget WorldTarget => context != null ? context : this;
        public virtual InteractionTarget Resolve(CombatActor actor) => Available(actor) ? this : null;
        public virtual bool Available(CombatActor actor) => isActiveAndEnabled && actor != null && actor.Alive;
        public string DisplayName => displayName;
        public Vector3 ApproachPosition => approachPoint != null ? approachPoint.position : transform.position;
        public virtual bool CanInteract(CombatActor actor) => Available(actor) &&
            (context == null || context.Resolve(actor) == this) &&
            Vector3.Distance(actor.transform.position,ApproachPosition) <= range &&
            !Physics.Linecast(actor.transform.position+Vector3.up,transform.position+Vector3.up,1<<9);
        public virtual void Interact(CombatActor actor) { }
        public void ConfigureName(string value) => displayName = value;
        public void ConfigureApproach(Transform value) => approachPoint = value;
        public void ConfigureContext(ContextualNpc value) => context = value;
    }
}
