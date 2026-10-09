using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Core
{
    // Canopies need visibility tests, not physical canopy colliders.
    [DisallowMultipleComponent]
    public sealed class TreeCameraOccluder : MonoBehaviour
    {
        internal static readonly HashSet<TreeCameraOccluder> Active = new();
        internal Renderer[] Renderers { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Active.Clear();
        void OnEnable() { Renderers = GetComponentsInChildren<Renderer>(); Active.Add(this); }
        void OnDisable() => Active.Remove(this);
    }
}
