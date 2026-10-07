using System;
using UnityEngine;

namespace DiceFree.Combat
{
    public static class ResourceIds
    {
        public const string Mana = "resource.mana";
    }

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
