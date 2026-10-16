using UnityEngine;
namespace DiceFree.World
{
    // Optional licensed resource, never a serialized dependency in the public scene.
    // The authored original remains a complete fallback; all gameplay colliders stay put.
    [DisallowMultipleComponent, DefaultExecutionOrder(-500)]
    public sealed class DetailedTreeVisual : MonoBehaviour
    {
        [SerializeField] int variant;
        [SerializeField] Renderer[] originals;
        [SerializeField] Vector3 localBase;
        [SerializeField] float localHeight = 5;
        GameObject detailed;
        public GameObject DetailedRoot => detailed;
        public Renderer[] Originals => originals;
        public void Configure(int palette, Renderer[] renderers, Vector3 origin, float height)
        { variant = palette; originals = renderers; localBase = origin; localHeight = height; }
        void Awake() => Install();
        public bool Install()
        {
            if (detailed != null) return true;
            var prefab = Resources.Load<GameObject>("TreesLicensed/Oak" + (variant % 4 + 1));
            if (prefab == null) return false;
            detailed = Instantiate(prefab, transform, false);
            detailed.name = "Detailed Canopy - licensed oak";
            detailed.transform.localPosition = localBase;
            // Prefab is normalized to 1m in the authoring step.
            detailed.transform.localScale = new Vector3(.72f, 1, .72f) * localHeight;
            detailed.transform.localRotation = Quaternion.Euler(0, variant * 47.3f, 0);
            foreach (var r in originals) if (r != null) r.enabled = false;
            return true;
        }
    }
}
