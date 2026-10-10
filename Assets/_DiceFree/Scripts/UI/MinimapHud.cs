using DiceFree.Combat;
using DiceFree.World;
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
        private WorldFogOfWar fog;
        private Texture2D fogMap;
        private Color32[] fogPixels;
        private float nextFogRefresh;
        private const int FogResolution = 96;

        public override string LayoutId => "minimap";
        public override string DisplayName => "Minimap";
        public override Rect DefaultNormalizedBounds => new(0.805f, 0.018f, 0.18f, 0.27f);
        public override Vector2 MinimumPixelSize => new(150, 150);
        public override float LockedAspectRatio => 1f;

        public void Configure(CombatActor actor) => player = actor;
        public float WorldRadius=>worldRadius;
        public void Zoom(float factor){worldRadius=Mathf.Clamp(worldRadius*factor,8,60);if(mapCamera!=null)mapCamera.orthographicSize=worldRadius;}

        private void Awake()
        {
            if (player == null) player = GetComponent<CombatActor>();
            CreateCamera();
            fog = FindFirstObjectByType<WorldFogOfWar>();
            fogMap = new Texture2D(FogResolution,FogResolution,TextureFormat.RGBA32,false)
            {
                name="Minimap exploration mask",filterMode=FilterMode.Bilinear,
                wrapMode=TextureWrapMode.Clamp
            };
            fogPixels = new Color32[FogResolution*FogResolution];
        }

        private void OnDestroy()
        {
            if(fogMap!=null)Destroy(fogMap);
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
            if(fog == null)fog = FindFirstObjectByType<WorldFogOfWar>();
            if(fog!=null && fogMap!=null && Time.unscaledTime>=nextFogRefresh)
            {
                nextFogRefresh=Time.unscaledTime+.16f;
                Vector3 center=player.transform.position;
                for(int y=0;y<FogResolution;y++)
                for(int x=0;x<FogResolution;x++)
                {
                    float dx=((x+.5f)/FogResolution*2f-1f)*worldRadius;
                    float dz=((y+.5f)/FogResolution*2f-1f)*worldRadius;
                    byte alpha=fog.OpacityAt(center+new Vector3(dx,0,dz));
                    fogPixels[y*FogResolution+x]=new Color32(6,12,19,alpha);
                }
                fogMap.SetPixels32(fogPixels);
                fogMap.Apply(false,false);
            }
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
            if(fog!=null && fogMap!=null)
                GUI.DrawTexture(mapRect,fogMap,ScaleMode.StretchToFill,true);

            DrawCompass(mapRect);
            GUI.Label(
                mapRect,
                "+",
                new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = Mathf.RoundToInt(Mathf.Clamp(mapRect.height * 0.08f, 12, 24))
                });
            if(GUI.Button(new Rect(mapRect.xMax-52,mapRect.yMax-26,24,24),"+"))Zoom(.8f);
            if(GUI.Button(new Rect(mapRect.xMax-26,mapRect.yMax-26,24,24),"-"))Zoom(1.25f);
        }

        // ASCII labels and a drawn arrow avoid font-dependent compass glyphs.
        private static void DrawCompass(Rect map)
        {
            float size = Mathf.Clamp(map.width * .12f, 18, 30);
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold, fontSize = Mathf.RoundToInt(size * .65f) };
            style.normal.textColor = Color.white;
            void Label(string text, Rect rect) { GUI.Box(rect, GUIContent.none); GUI.Label(rect, text, style); }
            Label("N", new Rect(map.center.x-size/2, map.y+2, size, size));
            Label("S", new Rect(map.center.x-size/2, map.yMax-size-2, size, size));
            Label("W", new Rect(map.x+2, map.center.y-size/2, size, size));
            Label("E", new Rect(map.xMax-size-2, map.center.y-size/2, size, size));
            var old = GUI.color; GUI.color = new Color(1, .85f, .35f);
            GUI.DrawTexture(new Rect(map.center.x-1, map.y+size+6, 2, 10), Texture2D.whiteTexture);
            for (int row=0; row<5; row++)
                GUI.DrawTexture(new Rect(map.center.x-row, map.y+size+3+row, 1+row*2, 1), Texture2D.whiteTexture);
            GUI.color = old;
        }
    }
}
