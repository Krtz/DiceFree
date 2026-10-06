using System;
using UnityEngine;

namespace DiceFree.Combat
{
    public sealed class UnityMissRollSource : IMissRollSource
    {
        public float NextUnit() => UnityEngine.Random.value;
    }

    public static class AccuracyResolver
    {
        private static readonly UnityMissRollSource UnityRolls = new();

        public static ActionResolution Resolve(CombatActor source, AttackDefinition attack, IMissRollSource rolls = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (attack == null) throw new ArgumentNullException(nameof(attack));
            var origin = new ActionProvenance(attack.stableId, source.Stats.Definition.stableId);
            if (!attack.requiresAccuracy) return ActionResolution.Resolve(origin);

            var miss = new MissChance(0, source.Effects?.AccuracyMissChance ?? 0);
            if (miss.Effective > 0 && miss.Effective < 1) rolls ??= UnityRolls;
            return ActionResolution.Resolve(origin, miss, rolls);
        }
    }
}
