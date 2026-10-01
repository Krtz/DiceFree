using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.World
{
    // Samples the existing visible mesh, without new physics colliders or a navigation bake.
    public sealed class TravelSurface : MonoBehaviour
    {
        private static readonly List<TravelSurface> active = new();
        public static IReadOnlyList<TravelSurface> Active => active;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSessionRegistry() => active.Clear();
        [SerializeField] private string stableId;
        [SerializeField] private TravelSurfaceDefinition definition;
        [SerializeField] private MeshFilter footprint;
        [SerializeField] private Bounds localRegion = new(Vector3.zero, Vector3.one * 10000);
        [SerializeField, Min(.01f)] private float heightTolerance = .75f;
        private Mesh cachedMesh;
        private Vector3[] vertices;
        private int[] triangles;
        public string StableId => stableId;
        public TravelSurfaceDefinition Definition => definition;
        private void OnEnable() { if (!active.Contains(this)) active.Add(this); }
        private void OnDisable() { active.Remove(this); }
        public bool Contains(Vector3 worldPosition)
        {
            if (!isActiveAndEnabled || definition == null || footprint == null || footprint.sharedMesh == null) return false;
            var p = footprint.transform.InverseTransformPoint(worldPosition);
            if (!localRegion.Contains(p)) return false;
            if (cachedMesh != footprint.sharedMesh)
            {
                cachedMesh = footprint.sharedMesh; vertices = cachedMesh.vertices; triangles = cachedMesh.triangles;
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var a = vertices[triangles[i]]; var b = vertices[triangles[i + 1]]; var c = vertices[triangles[i + 2]];
                float denominator = (b.z - c.z) * (a.x - c.x) + (c.x - b.x) * (a.z - c.z);
                if (Mathf.Abs(denominator) < .000001f) continue;
                float u = ((b.z - c.z) * (p.x - c.x) + (c.x - b.x) * (p.z - c.z)) / denominator;
                float v = ((c.z - a.z) * (p.x - c.x) + (a.x - c.x) * (p.z - c.z)) / denominator;
                float w = 1 - u - v;
                if (u >= -.00001f && v >= -.00001f && w >= -.00001f &&
                    Mathf.Abs(p.y - (a.y * u + b.y * v + c.y * w)) <= heightTolerance) return true;
            }
            return false;
        }
        public void Configure(string id, TravelSurfaceDefinition data, MeshFilter mesh, Bounds region)
        { stableId = id; definition = data; footprint = mesh; localRegion = region; cachedMesh = null; }
    }
}
