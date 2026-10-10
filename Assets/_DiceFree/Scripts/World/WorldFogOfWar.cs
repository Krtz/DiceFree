using UnityEngine;
namespace DiceFree.World
{
    /// <summary>
    /// First World-1 Warcraft-style fog. The camera-space mask keeps far scenery, units
    /// and trees covered, unlike a flat fog plane that characters poke through.
    /// Three states: unseen black, previously explored dim, current vision clear.
    /// This is intentionally a local-player, per-scene prototype (no party/shared LOS yet).
    /// </summary>
    [DefaultExecutionOrder(200)]
    public sealed class WorldFogOfWar : MonoBehaviour
    {
        [SerializeField] private Transform revealer;
        [SerializeField] private Camera worldCamera;
        [SerializeField, Range(8f, 65f)] private float visionRadius = 24f;
        [SerializeField, Range(.05f,.3f)] private float refreshSeconds = .15f;
        [SerializeField] private Vector2 mapMin = new Vector2(-70,-60);
        [SerializeField] private Vector2 mapMax = new Vector2(275,190);
        [SerializeField] private float cellSize = 2.0f;
        [SerializeField] private int maskWidth = 160;
        [SerializeField] private int maskHeight = 90;
        private bool[] explored;
        private Texture2D screenMask;
        private Color32[] pixels;
        private int columns, rows;
        private int lastWidth, lastHeight;
        private Vector3 lastEye, lastTarget;
        private Quaternion lastCameraRotation;
        private float nextRefresh;
        private bool initialized;
        public float VisionRadius => visionRadius;
        public int ExploredCellCount { get; private set; }

        public void Configure(Transform player, Camera camera, Vector2 min, Vector2 max, float radius)
        {
            revealer = player; worldCamera = camera;
            mapMin = min; mapMax = max;
            visionRadius = Mathf.Max(8f,radius);
            Initialize();
        }

        private void Awake() => Initialize();

        private void Initialize()
        {
            if (screenMask != null)
            {
                if (Application.isPlaying) Destroy(screenMask);
                else DestroyImmediate(screenMask);
            }
            columns = Mathf.CeilToInt((mapMax.x - mapMin.x) / Mathf.Max(.5f,cellSize));
            rows = Mathf.CeilToInt((mapMax.y - mapMin.y) / Mathf.Max(.5f,cellSize));
            explored = new bool[columns*rows];
            maskWidth = Mathf.Clamp(maskWidth,80,320);
            maskHeight = Mathf.Clamp(maskHeight,45,180);
            pixels = new Color32[maskWidth*maskHeight];
            screenMask=new Texture2D(maskWidth,maskHeight,TextureFormat.RGBA32,false);
            screenMask.name="World 1 fog of war display";
            screenMask.filterMode=FilterMode.Bilinear;
            screenMask.wrapMode=TextureWrapMode.Clamp;
            initialized=true;
            nextRefresh=0;
            ExploredCellCount=0;
        }

        public bool IsVisible(Vector3 location)
        {
            if (revealer == null) return true;
            float dx=location.x-revealer.position.x;
            float dz=location.z-revealer.position.z;
            return dx*dx+dz*dz<=visionRadius*visionRadius;
        }
        public byte OpacityAt(Vector3 position)
        {
            if (revealer == null) return 0;
            float dx=position.x-revealer.position.x;
            float dz=position.z-revealer.position.z;
            float r=Mathf.Sqrt(dx*dx+dz*dz);
            if(r<=visionRadius-2f)return 0;
            byte outside=IsExplored(position)?(byte)155:(byte)245;
            if(r>=visionRadius+1f)return outside;
            return (byte)Mathf.RoundToInt(outside *
                Mathf.SmoothStep(0f,1f,(r-visionRadius+2f)/3f));
        }
        public bool IsExplored(Vector3 location)
        {
            if (!initialized) return false;
            int x=Mathf.FloorToInt((location.x-mapMin.x)/cellSize);
            int z=Mathf.FloorToInt((location.z-mapMin.y)/cellSize);
            return x>=0&&z>=0&&x<columns&&z<rows&&explored[z*columns+x];
        }

        private void MarkExplored()
        {
            if (revealer==null||!initialized)return;
            Vector3 point=revealer.position;
            int cx=Mathf.FloorToInt((point.x-mapMin.x)/cellSize);
            int cz=Mathf.FloorToInt((point.z-mapMin.y)/cellSize);
            int radius=Mathf.CeilToInt(visionRadius/cellSize)+1;
            for(int z=Mathf.Max(0,cz-radius);z<=Mathf.Min(rows-1,cz+radius);z++)
            for(int x=Mathf.Max(0,cx-radius);x<=Mathf.Min(columns-1,cx+radius);x++)
            {
                float wx=mapMin.x+(x+.5f)*cellSize-point.x;
                float wz=mapMin.y+(z+.5f)*cellSize-point.z;
                if(wx*wx+wz*wz>visionRadius*visionRadius)continue;
                int index=z*columns+x;
                if(explored[index])continue;
                explored[index]=true;
                ExploredCellCount++;
            }
        }

        private void LateUpdate()
        {
            if(!Application.isPlaying||!initialized||worldCamera==null||revealer==null)return;
            MarkExplored();
            if(Time.unscaledTime<nextRefresh &&
                lastCameraRotation==worldCamera.transform.rotation &&
                (worldCamera.transform.position-lastEye).sqrMagnitude<.00001f &&
                (revealer.position-lastTarget).sqrMagnitude<.0001f) return;
            nextRefresh=Time.unscaledTime+refreshSeconds;
            lastWidth=Screen.width;lastHeight=Screen.height;
            lastCameraRotation=worldCamera.transform.rotation;
            lastEye=worldCamera.transform.position;
            lastTarget=revealer.position;
            // The screen-to-world ray intersects the horizontal World 1 floor.
            // Unlike a world plane, this masks high trees / units as well.
            Plane ground=new Plane(Vector3.up,Vector3.zero);
            for(int y=0;y<maskHeight;y++)
            for(int x=0;x<maskWidth;x++)
            {
                Ray ray=worldCamera.ScreenPointToRay(new Vector3(
                    (x+.5f)*lastWidth/maskWidth,(y+.5f)*lastHeight/maskHeight,0));
                byte alpha=245;
                if(ground.Raycast(ray,out float distance)&&distance>=0)
                {
                    Vector3 hit=ray.GetPoint(distance);
                    float dx=hit.x-lastTarget.x;
                    float dz=hit.z-lastTarget.z;
                    float r=Mathf.Sqrt(dx*dx+dz*dz);
                    // Fade in over 3 metres at the edge of current vision.
                    if(r<=visionRadius-2f)alpha=0;
                    else if(r<visionRadius+1f)
                    {
                        float t=Mathf.SmoothStep(0,1,(r-visionRadius+2f)/3f);
                        alpha=(byte)Mathf.RoundToInt(t*(IsExplored(hit)?155:245));
                    }
                    else alpha=IsExplored(hit)?(byte)155:(byte)245;
                }
                pixels[y*maskWidth+x]=new Color32(9,14,22,alpha);
            }
            screenMask.SetPixels32(pixels);
            screenMask.Apply(false,false);
        }

        private void OnGUI()
        {
            if(!Application.isPlaying||screenMask==null||worldCamera==null||
               revealer==null||Event.current.type!=EventType.Repaint)return;
            // Higher GUI.depth draws BEHIND normal HUD, so it remains readable.
            int saved=GUI.depth;
            Color original=GUI.color;
            GUI.depth=900;
            GUI.color=Color.white;
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),screenMask,
                ScaleMode.StretchToFill,true);
            GUI.color=original;
            GUI.depth=saved;
        }
        private void OnDestroy()
        {
            if(screenMask!=null)
            {
                if(Application.isPlaying)Destroy(screenMask);
                else DestroyImmediate(screenMask);
            }
        }
    }
}
