using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    /// <summary>Living-player town treatment. Does not resurrect or refill class resources.</summary>
    [DisallowMultipleComponent]
    public sealed class HealerInteraction : InteractionTarget
    {
        public override bool Available(CombatActor actor) =>
            base.Available(actor) && actor.Faction == 0;

        public override void Interact(CombatActor actor)
        {
            if (!CanInteract(actor)) return;
            actor.Health.Heal(actor.Health.Maximum);
        }
    }
}
