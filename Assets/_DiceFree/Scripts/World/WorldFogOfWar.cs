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
        private bool[] blocked;
        private bool[] visibleCells;
        private int blockersCount;
        public int BlockingObjectsCount => blockersCount;
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
            blocked = new bool[columns*rows];
            visibleCells = new bool[columns*rows];
            if(Application.isPlaying) RebuildObstructions();
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
            if(revealer==null)return true;
            float dx=location.x-revealer.position.x;
            float dz=location.z-revealer.position.z;
            if(dx*dx+dz*dz>visionRadius*visionRadius)return false;
            int x=Mathf.FloorToInt((location.x-mapMin.x)/cellSize);
            int z=Mathf.FloorToInt((location.z-mapMin.y)/cellSize);
            if(visibleCells==null||x<0||z<0||x>=columns||z>=rows)return false;
            return visibleCells[z*columns+x];
        }

        private void RebuildObstructions()
        {
            System.Array.Clear(blocked,0,blocked.Length);
            blockersCount=0;
            // The physical gameplay colliders are the source of truth: no phantom
            // forest visibility blockers once trees are actually deactivated.
            // House walls are boxes; only the narrow wooden tree trunks block vision,
            // never the broad decorative canopies.
            foreach(var collider in FindObjectsByType<Collider>(FindObjectsSortMode.None))
            {
                if(collider==null||!collider.enabled||collider.isTrigger||
                   !collider.gameObject.activeInHierarchy)continue;
                bool wall=collider.name=="Timber and lime walls";
                bool trunk=collider.name=="Playtest trunk collision" || collider.name=="Trunk";
                if(!wall&&!trunk)continue;
                Bounds box=collider.bounds;
                if(box.size.y<.5f)continue;
                int left=Mathf.Max(0,Mathf.FloorToInt((box.min.x-mapMin.x)/cellSize));
                int right=Mathf.Min(columns-1,Mathf.FloorToInt((box.max.x-mapMin.x)/cellSize));
                int low=Mathf.Max(0,Mathf.FloorToInt((box.min.z-mapMin.y)/cellSize));
                int high=Mathf.Min(rows-1,Mathf.FloorToInt((box.max.z-mapMin.y)/cellSize));
                for(int z=low;z<=high;z++)
                for(int x=left;x<=right;x++)
                {
                    // Solid rectangles intersecting a grid cell block its sight ray.
                    // Even slender tree trunks must occupy at least one fog cell.
                    blocked[z*columns+x]=true;
                }
                blockersCount++;
            }
        }

        private bool HasLineOfSight(int x0,int z0,int x1,int z1)
        {
            // Discrete supercover ray from the player to target grid cell.
            // The first occupied cell is visible (near face of house/tree), but cells
            // BEHIND it are not. This is independent of camera position/zoom.
            int dx=Mathf.Abs(x1-x0),dz=Mathf.Abs(z1-z0);
            int stepX=x0<x1?1:-1,stepZ=z0<z1?1:-1;
            int err=dx-dz, x=x0,z=z0;
            int limit=dx+dz+2;
            for(int i=0;i<limit;i++)
            {
                if(x==x1&&z==z1)return true;
                if((x!=x0||z!=z0)&&x>=0&&z>=0&&x<columns&&z<rows &&
                   blocked[z*columns+x])return false;
                int e2=err*2;
                if(e2>-dz){err-=dz;x+=stepX;}
                if(e2<dx){err+=dx;z+=stepZ;}
            }
            return false;
        }

        private void RefreshVisibility()
        {
            if(revealer==null||visibleCells==null)return;
            System.Array.Clear(visibleCells,0,visibleCells.Length);
            Vector3 eye=revealer.position;
            int cx=Mathf.FloorToInt((eye.x-mapMin.x)/cellSize);
            int cz=Mathf.FloorToInt((eye.z-mapMin.y)/cellSize);
            int radius=Mathf.CeilToInt(visionRadius/cellSize)+1;
            for(int z=Mathf.Max(0,cz-radius);z<=Mathf.Min(rows-1,cz+radius);z++)
            for(int x=Mathf.Max(0,cx-radius);x<=Mathf.Min(columns-1,cx+radius);x++)
            {
                float dx=mapMin.x+(x+.5f)*cellSize-eye.x;
                float dz=mapMin.y+(z+.5f)*cellSize-eye.z;
                if(dx*dx+dz*dz>visionRadius*visionRadius)continue;
                if(!HasLineOfSight(cx,cz,x,z))continue;
                int index=z*columns+x;
                visibleCells[index]=true;
                if(!explored[index]){explored[index]=true;ExploredCellCount++;}
            }
        }
        public void RefreshVisionNow() => RefreshVisibility();
        public byte OpacityAt(Vector3 position)
        {
            if (revealer == null) return 0;
            float dx=position.x-revealer.position.x;
            float dz=position.z-revealer.position.z;
            float r=Mathf.Sqrt(dx*dx+dz*dz);
            if(!IsVisible(position))return IsExplored(position)?(byte)155:(byte)245;
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

        private void LateUpdate()
        {
            if(!Application.isPlaying||!initialized||worldCamera==null||revealer==null)return;
            if(Time.unscaledTime<nextRefresh)return;
            if(lastWidth==Screen.width&&lastHeight==Screen.height&&
                lastCameraRotation==worldCamera.transform.rotation &&
                (worldCamera.transform.position-lastEye).sqrMagnitude<.00001f &&
                (revealer.position-lastTarget).sqrMagnitude<.0001f)
            {nextRefresh=Time.unscaledTime+refreshSeconds;return;}
            // LOS and exploration are recomputed together so blocked regions aren't
            // accidentally marked explored by a simple circular distance test.
            RefreshVisibility();
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
                    alpha=OpacityAt(hit);
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
