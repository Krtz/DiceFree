using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DiceFree.Core
{
    [DisallowMultipleComponent]
    public sealed class CameraOcclusionFader : MonoBehaviour
    {
        [SerializeField, Range(0.05f, 0.95f)] private float fadedAlpha = 0.22f;
        [SerializeField, Min(0.05f)] private float castRadius = 0.42f;
        [SerializeField] private LayerMask obstructionMask = 1 << 9;

        private sealed class FadeState
        {
            public Material[] originals;
            public Material[] transparentCopies;
        }

        private readonly Dictionary<Renderer, FadeState> faded = new();
        private readonly HashSet<Renderer> visibleThisFrame = new();

        public void UpdateOcclusion(Vector3 focus, Vector3 cameraPosition)
        {
            visibleThisFrame.Clear();

            Vector3 delta = cameraPosition - focus;
            float distance = delta.magnitude;
            if (distance > 0.01f)
            {
                var hits = Physics.SphereCastAll(
                    focus,
                    castRadius,
                    delta / distance,
                    distance,
                    obstructionMask,
                    QueryTriggerInteraction.Ignore);

                foreach (var hit in hits)
                    Collect(hit.collider);

                var ray = new Ray(focus, delta / distance);
                foreach (var tree in TreeCameraOccluder.Active)
                {
                    bool blocks = false;
                    foreach (var mesh in tree.Renderers)
                    {
                        if (mesh == null || !mesh.enabled) continue;
                        var bounds = mesh.bounds; bounds.Expand(castRadius * 2);
                        if (bounds.IntersectRay(ray, out var along) && along < distance) { blocks = true; break; }
                    }
                    if (blocks) foreach (var mesh in tree.Renderers) if (mesh != null) visibleThisFrame.Add(mesh);
                }
            }

            foreach (var renderer in visibleThisFrame)
                Fade(renderer);

            var restore = new List<Renderer>();
            foreach (var pair in faded)
                if (pair.Key == null || !visibleThisFrame.Contains(pair.Key))
                    restore.Add(pair.Key);

            foreach (var renderer in restore)
                Restore(renderer);
        }

        private void Collect(Collider collider)
        {
            if (collider == null) return;

            Renderer direct = collider.GetComponent<Renderer>();
            if (direct != null) visibleThisFrame.Add(direct);

            if (direct == null)
            {
                Renderer parent = collider.GetComponentInParent<Renderer>();
                if (parent != null) visibleThisFrame.Add(parent);
            }

            // Common tree/rock authoring uses a collider on the root and one or more child meshes.
            if (direct == null)
                foreach (var child in collider.GetComponentsInChildren<Renderer>(true))
                    if (child != null) visibleThisFrame.Add(child);
        }

        private void Fade(Renderer renderer)
        {
            if (renderer == null) return;
            if(faded.TryGetValue(renderer,out var prior))
            {
                var current=renderer.sharedMaterials;bool unchanged=current.Length==prior.transparentCopies.Length;
                for(int i=0;unchanged&&i<current.Length;i++)unchanged=current[i]==prior.transparentCopies[i];
                if(unchanged)return;
                // Puzzle enchantment or other authored appearance changes can happen while ghosted.
                renderer.sharedMaterials=ResolveOriginals(renderer,prior);
                foreach(var copy in prior.transparentCopies)if(copy!=null)Destroy(copy);
                faded.Remove(renderer);
            }

            Material[] originals = renderer.sharedMaterials;
            var copies = new Material[originals.Length];
            for (int i = 0; i < originals.Length; i++)
            {
                if (originals[i] == null) continue;
                var material = new Material(originals[i])
                {
                    name = originals[i].name + " (camera fade)"
                };
                MakeTransparent(material, fadedAlpha);
                copies[i] = material;
            }

            renderer.materials = copies;
            faded[renderer] = new FadeState
            {
                originals = originals,
                transparentCopies = copies
            };
        }

        private static void MakeTransparent(Material material, float alpha)
        {
            if (material == null) return;

            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);

            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            material.renderQueue = (int)RenderQueue.Transparent;

            if (material.HasProperty("_BaseColor"))
            {
                Color color = material.GetColor("_BaseColor");
                color.a = alpha;
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                Color color = material.GetColor("_Color");
                color.a = alpha;
                material.SetColor("_Color", color);
            }
        }

        private void Restore(Renderer renderer)
        {
            if (renderer == null)
            {
                if (faded.TryGetValue(renderer, out var dead))
                    foreach (var material in dead.transparentCopies) if (material != null) Destroy(material);
                faded.Remove(renderer);
                return;
            }

            if (!faded.TryGetValue(renderer, out var state)) return;
            renderer.sharedMaterials = ResolveOriginals(renderer,state);
            foreach (var material in state.transparentCopies)
                if (material != null) Destroy(material);
            faded.Remove(renderer);
        }
        private static Material[] ResolveOriginals(Renderer renderer,FadeState state)
        {
            var current=renderer.sharedMaterials;
            for(int i=0;i<current.Length;i++)
                if(i<state.transparentCopies.Length&&current[i]==state.transparentCopies[i])current[i]=state.originals[i];
            return current;
        }

        private void OnDisable()
        {
            var renderers = new List<Renderer>(faded.Keys);
            foreach (var renderer in renderers)
                Restore(renderer);
        }

        private void OnDestroy() => OnDisable();
    }
}
