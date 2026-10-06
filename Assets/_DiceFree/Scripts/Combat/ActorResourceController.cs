using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent, RequireComponent(typeof(ActorStats))]
    public sealed class ActorResourceController : MonoBehaviour
    {
        [SerializeField] private ClassResourceProfile[] profiles = Array.Empty<ClassResourceProfile>();

        private readonly Dictionary<string, float> current = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ClassResourceProfile> active = new(StringComparer.Ordinal);
        private ActorStats stats;

        public string ActiveClassId { get; private set; }
        public IReadOnlyList<ClassResourceProfile> Profiles => profiles;
        public event Action<string, float, float> Changed;
        public event Action DurableChanged;

        private void Awake()
        {
            stats = GetComponent<ActorStats>();
            ValidateProfiles();
        }

        private void OnEnable()
        {
            if (stats == null) stats = GetComponent<ActorStats>();
            stats.Changed += OnStatsChanged;
        }

        private void OnDisable()
        {
            if (stats != null) stats.Changed -= OnStatsChanged;
        }

        private void Update()
        {
            if (active.Count == 0 || Time.deltaTime <= 0) return;
            foreach (var pair in active)
            {
                float value = Current(pair.Key);
                float maximum = Maximum(pair.Key);
                if (value >= maximum) continue;
                float next = Mathf.Min(maximum, value + pair.Value.Regeneration(stats.Attributes) * Time.deltaTime);
                if (!Mathf.Approximately(next, value))
                {
                    current[pair.Key] = next;
                    Changed?.Invoke(pair.Key, next, maximum);
                }
            }
        }

        public void Configure(params ClassResourceProfile[] values)
        {
            profiles = values == null ? Array.Empty<ClassResourceProfile>() : (ClassResourceProfile[])values.Clone();
            ValidateProfiles();
        }

        public bool Has(string resourceId) => active.ContainsKey(resourceId);

        public float Current(string resourceId) =>
            current.TryGetValue(resourceId, out float value) ? value : 0;

        public float Maximum(string resourceId) =>
            active.TryGetValue(resourceId, out var profile) ? profile.Maximum(stats.Attributes) : 0;

        public float Regeneration(string resourceId) =>
            active.TryGetValue(resourceId, out var profile) ? profile.Regeneration(stats.Attributes) : 0;

        public bool CanSpend(string resourceId, float amount) =>
            amount >= 0 && Has(resourceId) && Current(resourceId) + 0.0001f >= amount;

        public bool TrySpend(string resourceId, float amount)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount < 0 || !CanSpend(resourceId, amount))
                return false;
            float value = Mathf.Max(0, Current(resourceId) - amount);
            current[resourceId] = value;
            Changed?.Invoke(resourceId, value, Maximum(resourceId));
            DurableChanged?.Invoke();
            return true;
        }

        public float Restore(string resourceId, float amount)
        {
            if (!Has(resourceId) || float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0) return 0;
            float before = Current(resourceId);
            float after = Mathf.Min(Maximum(resourceId), before + amount);
            current[resourceId] = after;
            if (!Mathf.Approximately(before, after))
            {
                Changed?.Invoke(resourceId, after, Maximum(resourceId));
                DurableChanged?.Invoke();
            }
            return after - before;
        }

        public void SetCurrentForValidation(string resourceId, float value)
        {
            if (!Has(resourceId)) throw new InvalidOperationException("Inactive resource: " + resourceId);
            current[resourceId] = Mathf.Clamp(value, 0, Maximum(resourceId));
            Changed?.Invoke(resourceId, current[resourceId], Maximum(resourceId));
            DurableChanged?.Invoke();
        }

        public void RefillAll()
        {
            foreach (string id in active.Keys.ToArray())
            {
                current[id] = Maximum(id);
                Changed?.Invoke(id, current[id], Maximum(id));
                DurableChanged?.Invoke();
            }
        }

        public bool CanRestoreForClass(string classId, IReadOnlyList<RuntimeResourceValue> saved, ActorDefinition definition, int level)
        {
            if (definition == null || definition.stableId != classId || level < 1) return false;
            var known = ProfilesForClass(classId).ToDictionary(value => value.resource.stableId, StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (saved == null) return true;
            foreach (var record in saved)
            {
                if (record == null || string.IsNullOrWhiteSpace(record.resourceId) ||
                    double.IsNaN(record.value) || double.IsInfinity(record.value) || !seen.Add(record.resourceId))
                    return false;
                if (!known.TryGetValue(record.resourceId, out var profile)) continue; // opaque future/unknown record
                if (profile.loadPolicy != ResourceRetentionPolicy.Persist) continue;
                var attributes = AttributeValues.AtLevel(definition.baseAttributes, definition.growth, level);
                if (record.value < 0 || record.value > profile.Maximum(attributes) + 0.001f) return false;
            }
            return true;
        }

        public void RestoreForClass(string classId, IReadOnlyList<RuntimeResourceValue> saved)
        {
            ActiveClassId = classId;
            active.Clear();
            current.Clear();

            foreach (var profile in ProfilesForClass(classId))
            {
                profile.Validate();
                string id = profile.resource.stableId;
                active[id] = profile;
                float maximum = profile.Maximum(stats.Attributes);
                float initial = maximum * Mathf.Clamp01(profile.initialFraction);
                if (profile.loadPolicy == ResourceRetentionPolicy.Persist && saved != null)
                {
                    foreach (var record in saved)
                        if (record != null && record.resourceId == id &&
                            !double.IsNaN(record.value) && !double.IsInfinity(record.value))
                        {
                            initial = (float)record.value;
                            break;
                        }
                }
                current[id] = Mathf.Clamp(initial, 0, maximum);
                Changed?.Invoke(id, current[id], maximum);
            }
        }

        public RuntimeResourceValue[] CaptureForActive(IReadOnlyList<RuntimeResourceValue> preserved)
        {
            var result = new List<RuntimeResourceValue>();
            var ownedIds = new HashSet<string>(
                ProfilesForClass(ActiveClassId).Select(value => value.resource.stableId),
                StringComparer.Ordinal);

            if (preserved != null)
                foreach (var record in preserved)
                    if (record != null && !ownedIds.Contains(record.resourceId))
                        result.Add(new RuntimeResourceValue { resourceId = record.resourceId, value = record.value });

            foreach (var pair in active.OrderBy(value => value.Key, StringComparer.Ordinal))
                if (pair.Value.loadPolicy == ResourceRetentionPolicy.Persist)
                    result.Add(new RuntimeResourceValue { resourceId = pair.Key, value = Current(pair.Key) });
            return result.ToArray();
        }

        private IEnumerable<ClassResourceProfile> ProfilesForClass(string classId) =>
            profiles.Where(value => value != null && value.classId == classId);

        private void ValidateProfiles()
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var profile in profiles)
            {
                if (profile == null) throw new InvalidOperationException("Resource profile array contains null.");
                profile.Validate();
                string key = profile.classId + "|" + profile.resource.stableId;
                if (!ids.Add(key)) throw new InvalidOperationException("Duplicate class/resource profile: " + key);
            }
        }

        private void OnStatsChanged(float _)
        {
            foreach (string id in active.Keys.ToArray())
            {
                float maximum = Maximum(id);
                if (Current(id) > maximum)
                {
                    current[id] = maximum;
                    Changed?.Invoke(id, maximum, maximum);
                }
            }
        }
    }
}
