using System;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.AI
{
    // Authored acquisition eligibility only, never threat cancellation or retaliation.
    // SerializeReference permits a future scaled policy without rewriting the AI loop.
    [Serializable]
    public class AutoAggroPolicy
    {
        public bool alwaysAutoAggro;
        [Min(1)] public int trivialLevelGap = 10; // Provisional working default.
        public virtual bool Allows(CombatActor enemy, CombatActor candidate) =>
            alwaysAutoAggro || (long)candidate.Stats.Level - enemy.Stats.Level < Mathf.Max(1, trivialLevelGap);
    }
}
