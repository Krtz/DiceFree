using System;
using UnityEngine;

namespace DiceFree.Combat
{
    public enum ResourceRetentionPolicy
    {
        ResetOnLoad,
        Persist
    }

    [Serializable]
    public sealed class RuntimeResourceValue
    {
        public string resourceId;
        public double value;
    }

    [CreateAssetMenu(menuName = "DiceFree/Combat/Resource")]
    public sealed class ResourceDefinition : ScriptableObject
    {
        public string stableId;
        public string displayName;
    }
}
