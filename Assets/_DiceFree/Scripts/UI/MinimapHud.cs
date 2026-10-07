using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.UI
{
    public sealed class MinimapHud : CustomizableHudWidget
    {
        private const int PortraitLayer = 31;

        [SerializeField] private CombatActor player;
        [SerializeField] private float worldRadius = 22f;

        private Camera mapCamera;
        private RenderTexture mapTexture;

        public override string LayoutId => "minimap";
        public override string DisplayName => "Minimap";
        public override Rect DefaultNormalizedBounds => new(0.805f, 0.018f, 0.18f, 0.27f);
        public override Vector2 MinimumPixelSize => new(150, 150);
        public override float LockedAspectRatio => 1f;

        public void Configure(CombatActor actor) => player = actor;

        private void Awake()
        {
            if (player == null) player = GetComponent<CombatActor>();
            CreateCamera();
        }

        private void OnDestroy()
        {
            if (mapTexture != null)
            {
                mapTexture.Release();
                Destroy(mapTexture);
            }
            if (mapCamera != null) Destroy(mapCamera.gameObject);
        }

        private void LateUpdate()
        {
            if (mapCamera == null || player == null) return;
            mapCamera.transform.position = player.transform.position + Vector3.up * 45f;
            mapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        private void CreateCamera()
        {
            var go = new GameObject("HUD Minimap Camera");
            mapCamera = go.AddComponent<Camera>();
            mapCamera.orthographic = true;
            mapCamera.orthographicSize = worldRadius;
            mapCamera.nearClipPlane = 0.1f;
            mapCamera.farClipPlane = 100f;
            mapCamera.clearFlags = CameraClearFlags.SolidColor;
            mapCamera.backgroundColor = new Color(0.025f, 0.035f, 0.025f, 1f);
            mapCamera.cullingMask = ~(1 << PortraitLayer);
            mapCamera.depth = -20;

            mapTexture = new RenderTexture(384, 384, 16, RenderTextureFormat.ARGB32)
            {
                name = "DiceFree HUD Minimap",
                filterMode = FilterMode.Bilinear
            };
            mapTexture.Create();
            mapCamera.targetTexture = mapTexture;
        }

        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen) return;

            Rect panel = Bounds;
            DrawPanel(panel);
            Rect inner = Inner(panel);
            float header = Mathf.Clamp(inner.height * 0.10f, 18, 28);
            var headerRect = new Rect(inner.x, inner.y, inner.width, header);
            HudChrome.DrawHeader(headerRect, Theme, "Cornberg");

            var mapRect = new Rect(inner.x, inner.y + header, inner.width, inner.height - header);
            if (mapTexture != null)
                GUI.DrawTexture(mapRect, mapTexture, ScaleMode.StretchToFill, false);
            else
                GUI.Box(mapRect, "Map");

            GUI.Label(
                mapRect,
                "▲",
                new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = Mathf.RoundToInt(Mathf.Clamp(mapRect.height * 0.08f, 12, 24))
                });
        }
    }
}
