using UnityEngine;

namespace DiceFree.World
{
    /// <summary>
    /// Warcraft-inspired three-state exploration fog. LOS/exploration use a gameplay
    /// grid; a smoothly filtered WORLD-ALIGNED transparent mesh shades terrain.
    /// Unlike the old screen-space overlay it cannot stamp black ground silhouettes
    /// onto the roofs of buildings, the camera, or the HUD.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public sealed class WorldFogOfWar : MonoBehaviour
    {
        [SerializeField] private Transform revealer;
        [SerializeField] private Camera worldCamera;
        [SerializeField, Range(8f, 65f)] private float visionRadius = 24f;
        [SerializeField, Range(.05f, .3f)] private float refreshSeconds = .15f;
        [SerializeField] private Vector2 mapMin = new Vector2(-70f, -60f);
        [SerializeField] private Vector2 mapMax = new Vector2(275f, 190f);
        [SerializeField] private float cellSize = .85f;

        // Do not serialize these dimensions: earlier scenes had a 160x90 screen mask
        // and would silently override a higher-resolution default.
        private const int TextureWidth = 512;
        private const int TextureHeight = 384;
        private const float SurfaceHeight = 1.2f;
        private static readonly int FogTexId = Shader.PropertyToID("_FogTex");

        private bool[] explored;
        private bool[] blocked;
        private bool[] visibleCells;
        private int columns, rows;
        private byte[] baseAlpha, horizontalAlpha;
        private Color32[] pixels;
        private Texture2D fogTexture;
        private Material runtimeMaterial;
        private GameObject overlay;
        private Mesh overlayMesh;
        private MeshRenderer overlayRenderer;
        private float nextRefresh;
        private Vector3 lastPosition;
        private bool hasPosition;
        private bool hasPaintPosition;
        private Vector3 lastPaintPosition;
        private bool initialized;
        private int blockersCount;

        public float VisionRadius => visionRadius;
        public int ExploredCellCount { get; private set; }
        public int BlockingObjectsCount => blockersCount;
        public int FogTextureWidth => fogTexture != null ? fogTexture.width : 0;
        public int FogTextureHeight => fogTexture != null ? fogTexture.height : 0;
        public bool UsesWorldOverlay => overlayRenderer != null && overlayRenderer.enabled;

        public void Configure(Transform player, Camera camera, Vector2 min,
            Vector2 max, float radius)
        {
            revealer = player;
            worldCamera = camera;
            mapMin = min;
            mapMax = max;
            visionRadius = Mathf.Max(8f, radius);
            if(Application.isPlaying) Initialize();
        }

        private void Awake()
        {
            if(Application.isPlaying) Initialize();
        }

        private void Initialize()
        {
            DisposeRuntime();
            columns = Mathf.CeilToInt((mapMax.x - mapMin.x) / Mathf.Max(.5f,cellSize));
            rows = Mathf.CeilToInt((mapMax.y - mapMin.y) / Mathf.Max(.5f,cellSize));
            explored = new bool[columns * rows];
            blocked = new bool[columns * rows];
            visibleCells = new bool[columns * rows];
            ExploredCellCount = 0;
            RebuildObstructions();

            fogTexture = new Texture2D(TextureWidth, TextureHeight,
                TextureFormat.RGBA32, false, true);
            fogTexture.name = "World 1 exploration map (smooth)";
            fogTexture.filterMode = FilterMode.Bilinear;
            fogTexture.wrapMode = TextureWrapMode.Clamp;
            baseAlpha = new byte[TextureWidth * TextureHeight];
            horizontalAlpha = new byte[baseAlpha.Length];
            pixels = new Color32[baseAlpha.Length];

            Material template = Resources.Load<Material>("WorldFogOverlay");
            if(template == null || template.shader == null || !template.shader.isSupported)
            {
                Debug.LogError("DiceFree: missing or unsupported Resources/WorldFogOverlay fog material.");
                initialized = false;
                return;
            }
            runtimeMaterial = new Material(template) { name = "World fog (runtime)" };
            runtimeMaterial.SetTexture(FogTexId, fogTexture);

            overlay = new GameObject("World-aligned fog surface (runtime)");
            overlay.layer = 29; // Dedicated fog-render layer, no collider: safe to exclude on minimap
            // without hiding all the decorative roads/trees on Ignore Raycast (layer 2).
            overlay.transform.SetParent(transform, false);
            overlay.transform.localPosition = Vector3.zero;
            overlay.transform.localRotation = Quaternion.identity;
            overlay.transform.localScale = Vector3.one;
            var filter = overlay.AddComponent<MeshFilter>();
            overlayMesh = new Mesh { name = "World fog ground quad" };
            overlayMesh.vertices = new[]
            {
                new Vector3(mapMin.x, SurfaceHeight, mapMin.y),
                new Vector3(mapMax.x, SurfaceHeight, mapMin.y),
                new Vector3(mapMin.x, SurfaceHeight, mapMax.y),
                new Vector3(mapMax.x, SurfaceHeight, mapMax.y)
            };
            overlayMesh.uv = new[]
            {
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 1), new Vector2(1, 1)
            };
            overlayMesh.triangles = new[] {0, 2, 1, 2, 3, 1};
            overlayMesh.RecalculateNormals();
            overlayMesh.RecalculateBounds();
            filter.sharedMesh = overlayMesh;
            overlayRenderer = overlay.AddComponent<MeshRenderer>();
            overlayRenderer.sharedMaterial = runtimeMaterial;
            overlayRenderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            overlayRenderer.receiveShadows = false;

            for(int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(9, 14, 22, 245);
            fogTexture.SetPixels32(pixels);
            fogTexture.Apply(false, false);
            nextRefresh = 0f;
            initialized = true;
            hasPosition = false;
            hasPaintPosition = false;
            RefreshVisionNow();
        }

        public bool IsVisible(Vector3 location)
        {
            if(revealer == null) return true;
            float dx = location.x - revealer.position.x;
            float dz = location.z - revealer.position.z;
            if(dx*dx + dz*dz > visionRadius*visionRadius) return false;
            int x = Mathf.FloorToInt((location.x-mapMin.x)/cellSize);
            int z = Mathf.FloorToInt((location.z-mapMin.y)/cellSize);
            return visibleCells != null && x >= 0 && z >= 0 &&
                x < columns && z < rows && visibleCells[z*columns+x];
        }

        // UI minimap samples the SAME softened map as the world overlay,
        // rather than sampling the jagged, authoritative gameplay LOS grid.
        public byte SmoothedOpacityAt(Vector3 position)
        {
            if(pixels==null || !initialized) return OpacityAt(position);
            float u = Mathf.Clamp01((position.x-mapMin.x)/(mapMax.x-mapMin.x));
            float v = Mathf.Clamp01((position.z-mapMin.y)/(mapMax.y-mapMin.y));
            float fx = Mathf.Clamp(u*TextureWidth-.5f,0f,TextureWidth-1f);
            float fz = Mathf.Clamp(v*TextureHeight-.5f,0f,TextureHeight-1f);
            int x0 = Mathf.FloorToInt(fx), z0 = Mathf.FloorToInt(fz);
            int x1 = Mathf.Min(TextureWidth-1,x0+1);
            int z1 = Mathf.Min(TextureHeight-1,z0+1);
            float sx=fx-x0,sz=fz-z0;
            float a0=Mathf.Lerp(pixels[z0*TextureWidth+x0].a,
                pixels[z0*TextureWidth+x1].a,sx);
            float a1=Mathf.Lerp(pixels[z1*TextureWidth+x0].a,
                pixels[z1*TextureWidth+x1].a,sx);
            return (byte)Mathf.RoundToInt(Mathf.Lerp(a0,a1,sz));
        }

        public bool IsExplored(Vector3 location)
        {
            if(!initialized && explored == null) return false;
            int x = Mathf.FloorToInt((location.x-mapMin.x)/cellSize);
            int z = Mathf.FloorToInt((location.z-mapMin.y)/cellSize);
            return explored != null && x >= 0 && z >= 0 &&
                x < columns && z < rows && explored[z*columns+x];
        }

        public byte OpacityAt(Vector3 position)
        {
            if(revealer == null) return 0;
            float dx = position.x-revealer.position.x;
            float dz = position.z-revealer.position.z;
            float radius = Mathf.Sqrt(dx*dx+dz*dz);
            byte outside = IsExplored(position) ? (byte)155 : (byte)245;
            if(!IsVisible(position)) return outside;
            if(radius <= visionRadius-3f) return 0;
            if(radius >= visionRadius) return outside;
            return (byte)Mathf.RoundToInt(outside *
                Mathf.SmoothStep(0f, 1f, (radius-visionRadius+3f)/3f));
        }

        private void RebuildObstructions()
        {
            System.Array.Clear(blocked, 0, blocked.Length);
            blockersCount = 0;
            foreach(var collider in FindObjectsByType<Collider>(
                FindObjectsSortMode.None))
            {
                if(collider == null || !collider.enabled || collider.isTrigger ||
                    !collider.gameObject.activeInHierarchy) continue;
                bool wall = collider.name == "Timber and lime walls";
                bool trunk = collider.name == "Playtest trunk collision" ||
                    collider.name == "Trunk";
                if(!wall && !trunk) continue;
                Bounds bounds = collider.bounds;
                if(bounds.size.y < .5f) continue;
                int minX = Mathf.Max(0, Mathf.FloorToInt(
                    (bounds.min.x-mapMin.x)/cellSize));
                int maxX = Mathf.Min(columns-1, Mathf.FloorToInt(
                    (bounds.max.x-mapMin.x)/cellSize));
                int minZ = Mathf.Max(0, Mathf.FloorToInt(
                    (bounds.min.z-mapMin.y)/cellSize));
                int maxZ = Mathf.Min(rows-1, Mathf.FloorToInt(
                    (bounds.max.z-mapMin.y)/cellSize));
                for(int z=minZ;z<=maxZ;z++)
                for(int x=minX;x<=maxX;x++)
                    blocked[z*columns+x]=true;
                blockersCount++;
            }
        }

        private bool HasLineOfSight(int x0, int z0, int x1, int z1)
        {
            int dx = Mathf.Abs(x1-x0), dz = Mathf.Abs(z1-z0);
            int stepX=x0<x1?1:-1, stepZ=z0<z1?1:-1;
            int error=dx-dz, x=x0,z=z0;
            for(int i=0;i<dx+dz+2;i++)
            {
                if(x==x1 && z==z1) return true;
                if((x!=x0 || z!=z0) && x>=0 && z>=0 &&
                    x<columns && z<rows && blocked[z*columns+x]) return false;
                int twice = error*2;
                if(twice>-dz) { error-=dz; x+=stepX; }
                if(twice<dx) { error+=dx; z+=stepZ; }
            }
            return false;
        }

        private void RefreshVisibility()
        {
            if(revealer==null || visibleCells==null) return;
            System.Array.Clear(visibleCells, 0, visibleCells.Length);
            var origin = revealer.position;
            int cx = Mathf.FloorToInt((origin.x-mapMin.x)/cellSize);
            int cz = Mathf.FloorToInt((origin.z-mapMin.y)/cellSize);
            int reach = Mathf.CeilToInt(visionRadius/cellSize)+1;
            for(int z=Mathf.Max(0,cz-reach);z<=Mathf.Min(rows-1,cz+reach);z++)
            for(int x=Mathf.Max(0,cx-reach);x<=Mathf.Min(columns-1,cx+reach);x++)
            {
                float wx = mapMin.x+(x+.5f)*cellSize-origin.x;
                float wz = mapMin.y+(z+.5f)*cellSize-origin.z;
                if(wx*wx+wz*wz>visionRadius*visionRadius ||
                   !HasLineOfSight(cx,cz,x,z)) continue;
                int i = z*columns+x;
                visibleCells[i] = true;
                if(!explored[i]) { explored[i]=true; ExploredCellCount++; }
            }
        }

        public void RefreshVisionNow()
        {
            RefreshVisibility();
            if(!initialized || revealer==null) return;
            // Repaint the old reveal area (it may now be explored but unlit),
            // and the new one. Even a long-distance teleport affects only
            // two small texture rectangles, never the whole world map.
            if(hasPaintPosition) PaintRegion(lastPaintPosition);
            PaintRegion(revealer.position);
            lastPaintPosition = revealer.position;
            hasPaintPosition = true;
        }

        private void PaintRegion(Vector3 center)
        {
            if(fogTexture==null || revealer==null)return;
            const int blurRadius=3;
            float stepX=(mapMax.x-mapMin.x)/TextureWidth;
            float stepZ=(mapMax.y-mapMin.y)/TextureHeight;
            float padding=visionRadius+4f;
            if(center.x+padding<mapMin.x || center.x-padding>mapMax.x ||
               center.z+padding<mapMin.y || center.z-padding>mapMax.y) return;

            int left=Mathf.Clamp(Mathf.FloorToInt(
                (center.x-padding-mapMin.x)/stepX),0,TextureWidth-1);
            int right=Mathf.Clamp(Mathf.CeilToInt(
                (center.x+padding-mapMin.x)/stepX),0,TextureWidth-1);
            int bottom=Mathf.Clamp(Mathf.FloorToInt(
                (center.z-padding-mapMin.y)/stepZ),0,TextureHeight-1);
            int top=Mathf.Clamp(Mathf.CeilToInt(
                (center.z+padding-mapMin.y)/stepZ),0,TextureHeight-1);
            if(left>right || bottom>top)return;

            // Blur needs a 3-pixel halo around the affected rectangle.
            for(int z=Mathf.Max(0,bottom-blurRadius);
                z<=Mathf.Min(TextureHeight-1,top+blurRadius);z++)
            {
                float wz=mapMin.y+(z+.5f)*stepZ;
                for(int x=Mathf.Max(0,left-blurRadius);
                    x<=Mathf.Min(TextureWidth-1,right+blurRadius);x++)
                    baseAlpha[z*TextureWidth+x]=OpacityAt(new Vector3(
                        mapMin.x+(x+.5f)*stepX,0f,wz));
            }

            for(int z=Mathf.Max(0,bottom-blurRadius);
                z<=Mathf.Min(TextureHeight-1,top+blurRadius);z++)
            for(int x=left;x<=right;x++)
            {
                int sum=0;
                for(int k=-blurRadius;k<=blurRadius;k++)
                    sum+=(4-Mathf.Abs(k))*baseAlpha[
                        z*TextureWidth+Mathf.Clamp(x+k,0,TextureWidth-1)];
                horizontalAlpha[z*TextureWidth+x]=(byte)((sum+8)/16);
            }

            int uploadWidth=right-left+1,uploadHeight=top-bottom+1;
            var region=new Color32[uploadWidth*uploadHeight];
            for(int z=bottom;z<=top;z++)
            for(int x=left;x<=right;x++)
            {
                int sum=0;
                for(int k=-blurRadius;k<=blurRadius;k++)
                    sum+=(4-Mathf.Abs(k))*horizontalAlpha[
                        Mathf.Clamp(z+k,0,TextureHeight-1)*TextureWidth+x];
                var result=new Color32(9,14,22,(byte)((sum+8)/16));
                pixels[z*TextureWidth+x]=result;
                region[(z-bottom)*uploadWidth+(x-left)]=result;
            }
            fogTexture.SetPixels32(left,bottom,uploadWidth,uploadHeight,region);
            fogTexture.Apply(false,false);
        }

        private void LateUpdate()
        {
            if(!Application.isPlaying || !initialized || revealer==null) return;
            if(Time.unscaledTime<nextRefresh) return;
            nextRefresh=Time.unscaledTime+refreshSeconds;
            if(hasPosition && (revealer.position-lastPosition).sqrMagnitude<.0025f)
                return;
            lastPosition=revealer.position;
            hasPosition=true;
            RefreshVisionNow();
        }

        private void DisposeRuntime()
        {
            if(overlay!=null) DestroyObject(overlay);
            if(overlayMesh!=null) DestroyObject(overlayMesh);
            if(runtimeMaterial!=null) DestroyObject(runtimeMaterial);
            if(fogTexture!=null) DestroyObject(fogTexture);
            overlay=null;
            overlayMesh=null;
            runtimeMaterial=null;
            fogTexture=null;
            overlayRenderer=null;
        }
        private static void DestroyObject(Object item)
        {
            if(item==null) return;
            if(Application.isPlaying) Destroy(item);
            else DestroyImmediate(item);
        }
        private void OnDestroy() => DisposeRuntime();
    }
}
