using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    public abstract class InteractionTarget : MonoBehaviour
    {
        [SerializeField] private string displayName;
        [SerializeField, Min(0.5f)] private float range = 2.5f;
        [SerializeField] private Transform approachPoint;
        public string DisplayName => displayName;
        public Vector3 ApproachPosition => approachPoint != null ? approachPoint.position : transform.position;
        public virtual bool CanInteract(CombatActor actor) => isActiveAndEnabled && actor != null && actor.Alive &&
            Vector3.Distance(actor.transform.position,ApproachPosition) <= range &&
            !Physics.Linecast(actor.transform.position+Vector3.up,transform.position+Vector3.up,1<<9);
        public virtual void Interact(CombatActor actor) { }
        public void ConfigureName(string value) => displayName = value;
        public void ConfigureApproach(Transform value) => approachPoint = value;
    }
}
