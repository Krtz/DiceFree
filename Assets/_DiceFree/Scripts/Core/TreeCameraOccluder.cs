using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Core
{
    // Canopies need visibility tests, not physical canopy colliders.
    [DisallowMultipleComponent]
    public sealed class TreeCameraOccluder : MonoBehaviour
    {
        [SerializeField, Min(1)] private float focusDistance = 8;
        public float FocusDistance => focusDistance;
        public void ConfigureFocusDistance(float distance) => focusDistance = Mathf.Max(1, distance);
        internal static readonly HashSet<TreeCameraOccluder> Active = new();
        internal Renderer[] Renderers { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Active.Clear();
        void OnEnable() { Renderers = GetComponentsInChildren<Renderer>(); Active.Add(this); }
        void OnDisable() => Active.Remove(this);
    }
}
