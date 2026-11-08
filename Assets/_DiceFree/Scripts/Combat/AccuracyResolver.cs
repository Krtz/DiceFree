using System;
using UnityEngine;

namespace DiceFree.Combat
{
    public sealed class UnityMissRollSource : IMissRollSource, ICriticalRollSource
    {
        public float NextUnit() => Mathf.Min(UnityEngine.Random.value, .99999994f);
    }

    public static class AccuracyResolver
    {
        private static readonly UnityMissRollSource UnityRolls = new();

        public static ActionResolution Resolve(CombatActor source, AttackDefinition attack, IMissRollSource rolls = null,
            CriticalRule critical = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (attack == null) throw new ArgumentNullException(nameof(attack));
            var origin = new ActionProvenance(attack.stableId, source.Stats.Definition.stableId);
            bool basic = ReferenceEquals(attack, source.Stats.Definition.basicAttack);
            float smoke = basic ? DiceFree.Skills.ThiefSmokeCloud.BasicMissChance(source) : 0;
            if (!attack.requiresAccuracy && smoke == 0 && critical == null)
                return ActionResolution.Resolve(origin);
            float status = attack.requiresAccuracy ? source.Effects?.AccuracyMissChance ?? 0 : 0;
            var miss = new MissChance(0, status, smoke);
            if (miss.Effective > 0 && miss.Effective < 1) rolls ??= UnityRolls;
            return ActionResolution.Resolve(origin, miss, rolls, critical == null ? null : new[] { critical }, UnityRolls);
        }
    }
}
