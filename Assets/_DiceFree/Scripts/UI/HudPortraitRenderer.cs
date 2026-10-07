using DiceFree.Items;
using UnityEngine;

namespace DiceFree.UI
{
    [DisallowMultipleComponent]
    public sealed class HudPortraitRenderer : MonoBehaviour
    {
        private const int PortraitLayer = 31;

        [SerializeField] private ClassPresentationSwitcher presentation;

        private Transform studio;
        private Camera portraitCamera;
        private Light portraitLight;
        private GameObject clone;
        private Transform source;
        private Equipment equipment;
        private bool rebuildRequested = true;

        public RenderTexture Texture { get; private set; }

        private void Awake()
        {
            if (presentation == null) presentation = GetComponent<ClassPresentationSwitcher>();
            equipment = GetComponent<Equipment>();
            if (equipment != null) equipment.Changed += RequestRebuild;
            CreateStudio();
        }

        private void Start() => Rebuild();

        private void Update()
        {
            if (presentation != null && presentation.ActiveVisualRoot != source)
                rebuildRequested = true;

            if (rebuildRequested) Rebuild();
        }

        private void OnDestroy()
        {
            if (equipment != null) equipment.Changed -= RequestRebuild;
            if (Texture != null)
            {
                Texture.Release();
                Destroy(Texture);
            }
            if (studio != null) Destroy(studio.gameObject);
        }

        private void RequestRebuild() => rebuildRequested = true;

        private void CreateStudio()
        {
            var root = new GameObject("HUD Portrait Studio");
            root.transform.SetParent(transform, false);
            root.transform.position = new Vector3(0, -1000, 0);
            studio = root.transform;

            var cameraObject = new GameObject("HUD Portrait Camera");
            cameraObject.transform.SetParent(studio, false);
            cameraObject.transform.localPosition = new Vector3(0, 1.05f, 3.45f);
            portraitCamera = cameraObject.AddComponent<Camera>();
            portraitCamera.clearFlags = CameraClearFlags.SolidColor;
            portraitCamera.backgroundColor = new Color(0.025f, 0.024f, 0.022f, 1f);
            portraitCamera.cullingMask = 1 << PortraitLayer;
            portraitCamera.fieldOfView = 23f;
            portraitCamera.nearClipPlane = 0.05f;
            portraitCamera.farClipPlane = 20f;
            portraitCamera.enabled = true;
            cameraObject.transform.LookAt(studio.TransformPoint(new Vector3(0, 1.0f, 0)));

            Texture = new RenderTexture(384, 512, 16, RenderTextureFormat.ARGB32)
            {
                name = "DiceFree HUD Portrait",
                antiAliasing = 2,
                filterMode = FilterMode.Bilinear
            };
            Texture.Create();
            portraitCamera.targetTexture = Texture;

            var lightObject = new GameObject("HUD Portrait Light");
            lightObject.transform.SetParent(studio, false);
            lightObject.transform.localRotation = Quaternion.Euler(35f, 150f, 0);
            portraitLight = lightObject.AddComponent<Light>();
            portraitLight.type = LightType.Directional;
            portraitLight.intensity = 1.35f;
            portraitLight.cullingMask = 1 << PortraitLayer;
        }

        private void Rebuild()
        {
            rebuildRequested = false;
            if (presentation == null) return;
            var next = presentation.ActiveVisualRoot;
            if (next == null) return;

            source = next;
            if (clone != null) Destroy(clone);

            clone = Instantiate(next.gameObject, studio);
            clone.name = next.name + " HUD Portrait";
            clone.SetActive(true);
            clone.transform.localPosition = Vector3.zero;
            clone.transform.localRotation = Quaternion.identity;

            SetLayerRecursive(clone.transform, PortraitLayer);
            foreach (var collider in clone.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
            foreach (var behaviour in clone.GetComponentsInChildren<MonoBehaviour>(true))
                behaviour.enabled = false;

            var animator = clone.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                animator.enabled = true;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.speed = 1f;
                animator.Play(Animator.StringToHash("Base Layer.Idle"), 0, 0f);
            }
        }

        private static void SetLayerRecursive(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            for (int i = 0; i < root.childCount; i++)
                SetLayerRecursive(root.GetChild(i), layer);
        }
    }
}
