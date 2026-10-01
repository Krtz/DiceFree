using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    // One discoverable world target; authored action order defines the default, never component enumeration.
    public sealed class ContextualNpc : InteractionTarget
    {
        [SerializeField] private string npcId;
        [SerializeField] private InteractionTarget[] actions = Array.Empty<InteractionTarget>();
        public string NpcId => npcId;
        public override InteractionTarget Resolve(CombatActor actor)
        {
            if (!isActiveAndEnabled || actor == null || !actor.Alive) return null;
            foreach (var action in actions)
                if (action != null && action != this && action.Available(actor)) return action;
            return null;
        }
        public override bool Available(CombatActor actor) => Resolve(actor) != null;
        public override bool CanInteract(CombatActor actor) => base.CanInteract(actor) && Resolve(actor).CanInteract(actor);
        public void Configure(string id, params InteractionTarget[] values)
        {
            npcId = id; actions = values;
            foreach (var action in actions) action.ConfigureContext(this);
        }
    }
}
