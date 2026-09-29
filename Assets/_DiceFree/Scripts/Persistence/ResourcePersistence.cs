using System;
using System.Collections.Generic;

namespace DiceFree.Persistence
{
    public enum ResourceLoadPolicy { ResetOnLoad, Persist }

    // A policy seam for future independent resource definitions, not a resource simulation.
    [Serializable] public sealed class ResourceLoadRule
    {
        public string resourceId;
        public ResourceLoadPolicy policy = ResourceLoadPolicy.ResetOnLoad;
        public double initialValue;
    }
    [Serializable] public sealed class ResourceSaveValue
    {
        public string resourceId;
        public double value;
    }
    public static class ResourcePersistence
    {
        public static ResourceSaveValue Capture(ResourceLoadRule rule, double current)
        {
            if (rule.policy != ResourceLoadPolicy.Persist) return null;
            if (string.IsNullOrEmpty(rule.resourceId) || double.IsNaN(current) || double.IsInfinity(current))
                throw new ArgumentException("Invalid durable resource.");
            return new ResourceSaveValue { resourceId = rule.resourceId, value = current };
        }
        public static double Resolve(ResourceLoadRule rule, IReadOnlyList<ResourceSaveValue> saved)
        {
            if (rule.policy == ResourceLoadPolicy.Persist && saved != null)
                foreach (var record in saved)
                    if (record.resourceId == rule.resourceId && !double.IsNaN(record.value) && !double.IsInfinity(record.value))
                        return record.value;
            return rule.initialValue;
        }
    }
}
