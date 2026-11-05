using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Core
{
    // Canopies need visibility tests, not physical canopy colliders.
    [DisallowMultipleComponent]
    public sealed class TreeCameraOccluder : MonoBehaviour
    {
        [SerializeField, Min(1)] private float focusDistance = 8;
        [SerializeField] private Transform exteriorFrame;
        [SerializeField] private Vector2 exteriorFootprint;
        public float FocusDistance => focusDistance;
        public void ConfigureFocusDistance(float distance) => focusDistance = Mathf.Max(1, distance);
        public void ConfigureExteriorOnly(Transform frame, Vector2 footprint)
        { exteriorFrame=frame; exteriorFootprint=footprint; }
        internal bool AllowsFade(Vector3 focus)
        {
            if(exteriorFrame==null)return true;
            var p=exteriorFrame.InverseTransformPoint(focus);
            return Mathf.Abs(p.x)>exteriorFootprint.x*.5f+.1f || Mathf.Abs(p.z)>exteriorFootprint.y*.5f+.1f;
        }
        internal static readonly HashSet<TreeCameraOccluder> Active = new();
        internal Renderer[] Renderers { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Active.Clear();
        void OnEnable() { Renderers = GetComponentsInChildren<Renderer>(); Active.Add(this); }
        void OnDisable() => Active.Remove(this);
    }
}
