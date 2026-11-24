using System.Collections.Generic;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.World
{
    // Pointer geometry is separate from the bodies used for combat and navigation.
    // Readable meshes use their actual triangles, including the current skinned pose.
    [DisallowMultipleComponent]
    public sealed class WorldSelectionShape : MonoBehaviour
    {
        private sealed class Geometry
        {
            public readonly List<Vector3> vertices = new();
            public int[] triangles;
            public Mesh baked;
            public Mesh source;
            public int frame = -1;
        }
        private readonly Dictionary<Renderer, Geometry> geometry = new();
        private Renderer[] renderers;
        private float refreshAt;
        private CombatActor actor;
        public static WorldSelectionShape For(Component target)
        {
            var shape = target.GetComponent<WorldSelectionShape>();
            if(shape == null) shape = target.gameObject.AddComponent<WorldSelectionShape>();
            return shape;
        }
        public Renderer[] Renderers
        {
            get
            {
                if(renderers == null || Time.unscaledTime >= refreshAt)
                {
                    actor = GetComponent<CombatActor>();
                    renderers = GetComponentsInChildren<Renderer>(true);
                    refreshAt = Time.unscaledTime + 1;
                }
                return renderers;
            }
        }
        public bool Visible(Renderer renderer) => renderer != null && renderer.enabled &&
            renderer.gameObject.activeInHierarchy && renderer.gameObject.layer != 31 && renderer.gameObject.layer != 30 &&
            (renderer is MeshRenderer || renderer is SkinnedMeshRenderer) && renderer.GetComponent<TextMesh>() == null &&
            (actor == null || renderer.GetComponentInParent<CombatActor>() == actor);
        public bool TryBounds(out Bounds bounds)
        {
            bounds = default; bool found = false, hasModel = false;
            foreach(var renderer in Renderers)
            {
                if(renderer != null && renderer.gameObject.layer != 30 && renderer.gameObject.layer != 31 &&
                   (renderer is MeshRenderer || renderer is SkinnedMeshRenderer) && renderer.GetComponent<TextMesh>() == null) hasModel = true;
                if(!Visible(renderer)) continue;
                if(!found) bounds = renderer.bounds; else bounds.Encapsulate(renderer.bounds);
                found = true;
            }
            // Some dungeon entrances intentionally have a collider and a world
            // label, with their scenery authored outside the interaction root.
            // Keep these clickable, without exposing models hidden on another floor.
            if(!found && !hasModel) foreach(var collider in GetComponentsInChildren<Collider>())
            {
                if(!collider.enabled || collider.isTrigger || !collider.gameObject.activeInHierarchy) continue;
                if(!found) bounds=collider.bounds; else bounds.Encapsulate(collider.bounds);
                found=true;
            }
            return found;
        }
        public bool Raycast(Ray ray, float limit, out float distance)
        {
            distance = limit; bool found = false;
            foreach(var renderer in Renderers)
            {
                if(!Visible(renderer) || !renderer.bounds.IntersectRay(ray, out var broad) || broad > distance) continue;
                Mesh mesh = renderer is SkinnedMeshRenderer skin ? skin.sharedMesh : renderer.GetComponent<MeshFilter>()?.sharedMesh;
                if(mesh == null) continue;
                float hit;
                if(mesh.isReadable)
                {
                    if(!geometry.TryGetValue(renderer, out var data) || data.source != mesh)
                    {
                        if(data?.baked != null) Destroy(data.baked);
                        data = new Geometry { triangles = mesh.triangles, source = mesh };
                        geometry[renderer]=data;
                        if(renderer is SkinnedMeshRenderer) data.baked = new Mesh { name = "Pointer posed mesh", hideFlags = HideFlags.DontSave };
                        else mesh.GetVertices(data.vertices);
                    }
                    if(renderer is SkinnedMeshRenderer skinned && data.frame != Time.frameCount)
                    {
                        skinned.BakeMesh(data.baked, false); data.baked.GetVertices(data.vertices); data.frame = Time.frameCount;
                    }
                    var localRay = new Ray(renderer.transform.InverseTransformPoint(ray.origin), renderer.transform.InverseTransformVector(ray.direction));
                    if(!Triangles(localRay, data, out var localDistance)) continue;
                    hit = Vector3.Distance(ray.origin, renderer.transform.TransformPoint(localRay.GetPoint(localDistance)));
                }
                else
                {
                    // Imported meshes may deliberately omit CPU vertex data. An
                    // ellipsoid avoids the empty corners of their world-space box.
                    if(!Ellipsoid(ray, renderer.bounds, out hit)) continue;
                }
                if(hit < 0 || hit > distance) continue;
                distance = hit; found = true;
            }
            return found;
        }
        private static bool Triangles(Ray ray, Geometry mesh, out float nearest)
        {
            nearest = float.PositiveInfinity;
            for(int i = 0; i + 2 < mesh.triangles.Length; i += 3)
            {
                var a = mesh.vertices[mesh.triangles[i]]; var edge1 = mesh.vertices[mesh.triangles[i+1]] - a;
                var edge2 = mesh.vertices[mesh.triangles[i+2]] - a; var cross = Vector3.Cross(ray.direction, edge2);
                float determinant = Vector3.Dot(edge1, cross);
                if(Mathf.Abs(determinant) < .0000001f) continue;
                float inverse = 1 / determinant; var from = ray.origin - a;
                float u = Vector3.Dot(from, cross) * inverse; if(u < 0 || u > 1) continue;
                var q = Vector3.Cross(from, edge1); float v = Vector3.Dot(ray.direction, q) * inverse;
                if(v < 0 || u + v > 1) continue;
                float hit = Vector3.Dot(edge2, q) * inverse;
                if(hit >= 0 && hit < nearest) nearest = hit;
            }
            return !float.IsPositiveInfinity(nearest);
        }
        private static bool Ellipsoid(Ray ray, Bounds bounds, out float distance)
        {
            var size = bounds.extents; size.x = Mathf.Max(.06f,size.x); size.y = Mathf.Max(.06f,size.y); size.z = Mathf.Max(.06f,size.z);
            var origin = ray.origin - bounds.center;
            origin = new Vector3(origin.x/size.x,origin.y/size.y,origin.z/size.z);
            var direction = new Vector3(ray.direction.x/size.x,ray.direction.y/size.y,ray.direction.z/size.z);
            float a = Vector3.Dot(direction,direction), b = Vector3.Dot(origin,direction), c = Vector3.Dot(origin,origin)-1;
            float discriminant = b*b-a*c; distance = 0;
            if(discriminant < 0 || a < .0000001f) return false;
            distance = (-b-Mathf.Sqrt(discriminant))/a;
            if(distance < 0) distance = (-b+Mathf.Sqrt(discriminant))/a;
            return distance >= 0;
        }
        private void OnDestroy()
        {
            foreach(var data in geometry.Values) if(data.baked != null) Destroy(data.baked);
        }
        private void OnTransformChildrenChanged() { renderers=null; }
    }
}
