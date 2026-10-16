using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Combat
{
    /// <summary>Nearby hostile resistance reduction. Overlapping copies use the strongest aura.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor))]
    public sealed class ElementalResistanceAura : MonoBehaviour
    {
        static readonly HashSet<ElementalResistanceAura> Active = new();
        readonly HashSet<string> schools = new(StringComparer.Ordinal);
        CombatActor owner;
        float reduction, radius;
        public float Reduction => reduction;
        public float Radius => radius;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetRegistry() => Active.Clear();
        void Awake() => owner = GetComponent<CombatActor>();
        void OnEnable() => Active.Add(this);
        void OnDisable() => Active.Remove(this);
        public void Configure(ElementDefinition[] elements, float fraction, float distance)
        {
            if (!float.IsFinite(fraction) || !float.IsFinite(distance) || fraction < 0 || distance < 0)
                throw new ArgumentOutOfRangeException(nameof(fraction));
            schools.Clear();
            foreach (var element in elements ?? Array.Empty<ElementDefinition>())
                if (element != null) schools.Add(element.stableId);
            reduction = fraction;
            radius = distance;
        }
        public void Clear() { schools.Clear(); reduction = radius = 0; }
        public static float StrongestAgainst(ActorStats targetStats, ElementDefinition element)
        {
            if (targetStats == null || element == null || !targetStats.TryGetComponent<CombatActor>(out var target))
                return 0;
            float strongest = 0;
            foreach (var aura in Active)
            {
                if (aura == null || !aura.isActiveAndEnabled || aura.reduction <= 0 || aura.radius <= 0 ||
                    aura.owner == null || !aura.owner.IsHostileTo(target) || !aura.schools.Contains(element.stableId))
                    continue;
                if (Vector3.Distance(aura.transform.position, target.transform.position) <= aura.radius)
                    strongest = Mathf.Max(strongest, aura.reduction);
            }
            return strongest;
        }
    }
}
