using System.Collections.Generic;
using DiceFree.Combat;
using UnityEngine;
using UnityEngine.Rendering;

namespace DiceFree.UI
{
    [DisallowMultipleComponent]
    public sealed class ThiefStealthPresentation : MonoBehaviour
    {
        private readonly Dictionary<Renderer,(Material[] materials,ShadowCastingMode shadows)> visible = new();
        private readonly Dictionary<Material,Material> translucent = new();
        private CombatActor owner;
        private bool hidden;

        private void OnEnable() => owner = GetComponentInParent<CombatActor>();
        private void OnDisable() => Restore();

        private void LateUpdate()
        {
            bool next = owner != null && owner.Concealed;
            if (next == hidden) return;
            hidden = next;
            if (!hidden) { Restore(); return; }
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                var originals = renderer.sharedMaterials;
                visible.Add(renderer,(originals,renderer.shadowCastingMode));
                var faded = new Material[originals.Length];
                for (int i=0;i<originals.Length;i++)
                    faded[i] = Fade(originals[i]);
                renderer.sharedMaterials = faded;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
        }

        private Material Fade(Material source)
        {
            if (source == null) return null;
            if (translucent.TryGetValue(source,out var faded)) return faded;
            faded = new Material(source) { name=source.name+" · hidden",renderQueue=(int)RenderQueue.Transparent };
            faded.SetOverrideTag("RenderType","Transparent");
            faded.SetFloat("_Surface",1);
            faded.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
            faded.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
            faded.SetFloat("_ZWrite",0);
            faded.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            faded.DisableKeyword("_ALPHATEST_ON");
            faded.DisableKeyword("_EMISSION");
            if (faded.HasProperty("_BaseColor"))
            {
                var color = faded.GetColor("_BaseColor");
                color.a = .28f;
                faded.SetColor("_BaseColor",color);
            }
            translucent.Add(source,faded);
            return faded;
        }

        private void Restore()
        {
            foreach (var pair in visible)
                if (pair.Key != null)
                {
                    pair.Key.sharedMaterials = pair.Value.materials;
                    pair.Key.shadowCastingMode = pair.Value.shadows;
                }
            visible.Clear();
            foreach (var material in translucent.Values) Destroy(material);
            translucent.Clear();
            hidden = false;
        }
    }
}
