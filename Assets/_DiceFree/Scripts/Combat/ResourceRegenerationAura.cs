using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor))]
    public sealed class ResourceRegenerationAura : MonoBehaviour
    {
        private static readonly HashSet<ResourceRegenerationAura> Active = new();

        private CombatActor actor;
        private string resourceId = "";
        private float bonusPerSecond;
        private float radius;

        public string ResourceId => resourceId;
        public float BonusPerSecond => bonusPerSecond;
        public float Radius => radius;
        public bool Contributing => actor != null && actor.Alive && bonusPerSecond > 0 && radius > 0 &&
                                    !string.IsNullOrWhiteSpace(resourceId);

        private void Awake() => actor = GetComponent<CombatActor>();
        private void OnEnable() => Active.Add(this);
        private void OnDisable() => Active.Remove(this);

        public void Configure(string stableResourceId, float regenerationBonus, float auraRadius)
        {
            if (string.IsNullOrWhiteSpace(stableResourceId) && (regenerationBonus > 0 || auraRadius > 0))
                throw new ArgumentException("Resource aura requires a stable resource ID.");
            if (float.IsNaN(regenerationBonus) || float.IsInfinity(regenerationBonus) || regenerationBonus < 0 ||
                float.IsNaN(auraRadius) || float.IsInfinity(auraRadius) || auraRadius < 0)
                throw new ArgumentOutOfRangeException(nameof(regenerationBonus));

            resourceId = stableResourceId ?? "";
            bonusPerSecond = regenerationBonus;
            radius = auraRadius;
        }

        public void Clear() => Configure("", 0, 0);

        public static float StrongestFor(CombatActor target, string stableResourceId)
        {
            if (target == null || string.IsNullOrWhiteSpace(stableResourceId)) return 0;
            float strongest = 0;
            foreach (var aura in Active)
            {
                if (aura == null || !aura.Contributing || aura.actor == target ||
                    aura.resourceId != stableResourceId || !aura.actor.IsFriendlyTo(target))
                    continue;
                if (Vector3.Distance(aura.transform.position, target.transform.position) > aura.radius)
                    continue;
                strongest = Mathf.Max(strongest, aura.bonusPerSecond);
            }
            return strongest;
        }
    }
}
