using System;
using System.Collections.Generic;

namespace DiceFree.Combat
{
    // One immutable result per action, shared by all its eligible damage packets.
    // An eventual on-crit publisher belongs to the action owner, never packet calculation.
    public sealed class ActionCriticalResolution
    {
        public CriticalResolution Winner { get; }
        public ActionProvenance Origin { get; }
        public bool Triggered => Winner.triggered;
        public IReadOnlyList<CriticalResolution> Evaluated { get; }
        internal ActionCriticalResolution(CriticalResolution winner, CriticalResolution[] evaluated, ActionProvenance origin)
        {
            Origin = origin;
            Winner = winner; Evaluated = Array.AsReadOnly(evaluated);
        }
        public float ApplyToRaw(float raw) => Triggered ? raw * Winner.requestedMultiplier.Value : raw;
    }
}
