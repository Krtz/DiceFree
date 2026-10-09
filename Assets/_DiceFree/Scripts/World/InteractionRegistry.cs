using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceFree.World
{
    /// <summary>Session-owned catalog of root interaction targets for the local actor.</summary>
    [DisallowMultipleComponent]
    public sealed class InteractionRegistry : MonoBehaviour
    {
        private readonly List<InteractionTarget> targets = new();
        private bool bootstrapped;

        public bool Contains(InteractionTarget target) => target != null && targets.Contains(target);

        public void Register(InteractionTarget target)
        {
            if (target == null || target.WorldTarget != target || targets.Contains(target)) return;
            targets.Add(target);
            target.BindRegistry(this);
        }

        public void Unregister(InteractionTarget target)
        {
            if (target == null) return;
            targets.Remove(target);
            target.UnbindRegistry(this);
        }

        // Lifecycle removal keeps the binding so re-enabling the component restores membership.
        internal void UnregisterForLifetime(InteractionTarget target)
        {
            if (target != null) targets.Remove(target);
        }

        public InteractionTarget NearestInRange(Combat.CombatActor actor)
        {
            InteractionTarget nearest = null;
            float distance = float.PositiveInfinity;
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                var target = targets[i];
                if (target == null) { targets.RemoveAt(i); continue; }
                float candidateDistance = (target.ApproachPosition - actor.transform.position).sqrMagnitude;
                if (candidateDistance < distance && target.CanInteract(actor))
                { nearest = target; distance = candidateDistance; }
            }
            return nearest;
        }

        // Existing saved scenes predate explicit catalogs. Discover once at actor/session composition,
        // then input lookups use this owned registry and target enable/disable controls membership.
        public void RegisterExistingSceneTargets()
        {
            if (bootstrapped) return;
            bootstrapped = true;
            foreach (var target in FindObjectsByType<InteractionTarget>()) Register(target);
        }

        private void OnDisable()
        {
            foreach (var target in targets.ToArray()) target.UnbindRegistry(this);
            targets.Clear(); bootstrapped = false;
        }
    }
}
