using DiceFree.Combat;
using DiceFree.Input;
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
        private WorldAtlasHud atlas;
        private RenderTexture mapTexture;
        private WorldFogOfWar fog;
        private IMinimapNavigator navigation;
        private SkillTargetingController targeting;
        private InteractionRegistry interactions;
        private Texture2D fogMap;
        private Color32[] fogPixels;
        private float nextFogRefresh;
        private const int FogResolution = 192;
        public bool CameraUp => GameplayPreferences.Current.MinimapOrientation ==
            MinimapOrientation.CameraUp;
        public static Vector3 MapUpDirection(Vector3 cameraForward)
        {
            Vector3 planar=Vector3.ProjectOnPlane(cameraForward,Vector3.up);
            return planar.sqrMagnitude>.001f?planar.normalized:Vector3.forward;
        }
        public Vector3 MapUp => CameraUp && Camera.main!=null?
            MapUpDirection(Camera.main.transform.forward):Vector3.forward;
        private Vector3 MapRight=>Vector3.Cross(Vector3.up,MapUp).normalized;

        public override string LayoutId => "minimap";
        public override string DisplayName => "Minimap";
        public string MapName { get; set; } = "Cornberg";
        public override Rect DefaultNormalizedBounds => new(0.805f, 0.018f, 0.18f, 0.27f);
        public override Vector2 MinimumPixelSize => new(150, 150);
        public override float LockedAspectRatio => 1f;

        public void Configure(CombatActor actor) => player = actor;
        public float WorldRadius=>worldRadius;
        public void Zoom(float factor){worldRadius=Mathf.Clamp(worldRadius*factor,8,60);if(mapCamera!=null)mapCamera.orthographicSize=worldRadius;}

        private void Awake()
        {
            if (player == null) player = GetComponent<CombatActor>();
            foreach(var behaviour in GetComponents<MonoBehaviour>())
                if(behaviour is IMinimapNavigator navigator)
                {navigation=navigator;break;}
            targeting = GetComponent<SkillTargetingController>();
            interactions=player!=null?player.GetComponent<InteractionRegistry>():null;
            if (gameObject.scene.name == "Cornberg") atlas = gameObject.AddComponent<WorldAtlasHud>();
            CreateCamera();
            fog = FindAnyObjectByType<WorldFogOfWar>();
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
            mapCamera.transform.rotation =
                Quaternion.LookRotation(Vector3.down,MapUp);
            if(fog == null)fog = FindAnyObjectByType<WorldFogOfWar>();
            if(fog!=null && fogMap!=null && Time.unscaledTime>=nextFogRefresh)
            {
                nextFogRefresh=Time.unscaledTime+.16f;
                Vector3 center=player.transform.position;
                for(int y=0;y<FogResolution;y++)
                for(int x=0;x<FogResolution;x++)
                {
                    float dx=((x+.5f)/FogResolution*2f-1f)*worldRadius;
                    float dz=((y+.5f)/FogResolution*2f-1f)*worldRadius;
                    byte alpha=fog.SmoothedOpacityAt(center+
                        MapRight*dx+MapUp*dz);
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
            // Exclude ONLY the dedicated world-fog renderer and portrait clones.
            // Decorative roads, bridges and vegetation legitimately use layer 2.
            mapCamera.cullingMask = ~((1 << PortraitLayer) | (1 << 29));
            mapCamera.depth = -20;

            mapTexture = new RenderTexture(384, 384, 16, RenderTextureFormat.ARGB32)
            {
                name = "DiceFree HUD Minimap",
                filterMode = FilterMode.Bilinear
            };
            mapTexture.Create();
            mapCamera.targetTexture = mapTexture;
        }

        /// <summary>Map north is world +Z; supports any movable/resized map rectangle.</summary>
        public static Vector3 WorldPointForMapClick(Vector3 playerPosition,
            float radius, Rect mapRect, Vector2 guiPoint)
        {
            float u=Mathf.Clamp01((guiPoint.x-mapRect.x)/mapRect.width);
            float v=Mathf.Clamp01((guiPoint.y-mapRect.y)/mapRect.height);
            return new Vector3(playerPosition.x+(u-.5f)*radius*2f,
                playerPosition.y,playerPosition.z+(.5f-v)*radius*2f);
        }

        public static Vector3 WorldPointForMapClick(Vector3 playerPosition,
            float radius,Rect mapRect,Vector2 guiPoint,Vector3 up)
        {
            float u=Mathf.Clamp01((guiPoint.x-mapRect.x)/mapRect.width);
            float v=Mathf.Clamp01((guiPoint.y-mapRect.y)/mapRect.height);
            up=MapUpDirection(up);
            Vector3 right=Vector3.Cross(Vector3.up,up).normalized;
            return playerPosition+right*((u-.5f)*radius*2f)+
                up*((.5f-v)*radius*2f);
        }

        public bool TryMoveFromMapClick(Rect mapRect, Vector2 guiPoint)
        {
            if(player==null||navigation==null||!mapRect.Contains(guiPoint) ||
                HudPointerBlocker.ModalOpen ||
                (HudLayoutManager.Current?.EditMode ?? false))
                return false;
            Vector3 destination=WorldPointForMapClick(
                player.transform.position,worldRadius,mapRect,guiPoint,MapUp);
            // The minimap is a top-down image of the world: sample the actual
            // ground below the requested XZ before seeking a NavMesh path.
            if(!Physics.Raycast(destination+Vector3.up*80f,Vector3.down,
                out var ground,160f,1<<8,QueryTriggerInteraction.Ignore))
                return false;
            bool append=UnityEngine.InputSystem.Keyboard.current!=null &&
                UnityEngine.InputSystem.Keyboard.current.shiftKey.isPressed;
            // While X is armed the minimap is another legitimate attack-move
            // ground picker. Shift appends; ordinary click replaces orders.
            return TryIssueWorldPoint(ground.point,append);
        }

        /// <summary>
        /// Confirm a minimap point using the same command mode as the world click.
        /// Public for deterministic playtest coverage of Shift-queued map orders.
        /// </summary>
        public bool TryIssueWorldPoint(Vector3 worldPoint, bool append)
        {
            if(player==null || navigation==null || HudPointerBlocker.ModalOpen ||
               (HudLayoutManager.Current?.EditMode ?? false))return false;
            if(targeting==null)targeting=GetComponent<SkillTargetingController>();
            if(targeting!=null && targeting.Mode==SkillTargetingMode.AttackMove)
                return targeting.ConfirmAttackMoveFromMinimap(worldPoint,append);
            return navigation.TryMoveFromMinimap(worldPoint,append);
        }

        public static Vector2 MapPointForWorld(Vector3 center,Vector3 point,float radius,Rect rect,Vector3 up)
        {
            up=MapUpDirection(up);var right=Vector3.Cross(Vector3.up,up).normalized;var delta=point-center;
            return new Vector2(rect.center.x+Vector3.Dot(delta,right)*rect.width/(radius*2),rect.center.y-Vector3.Dot(delta,up)*rect.height/(radius*2));
        }

        public bool TryProjectWorkMarker(TimedInteractionTarget target,Rect rect,out Vector2 point)
        {
            point=default;
            if(player==null||target==null||!target.Available(player)||Mathf.Abs(target.ApproachPosition.y-player.transform.position.y)>2.5f||
                (fog!=null&&!fog.IsExplored(target.ApproachPosition)))return false;
            point=MapPointForWorld(player.transform.position,target.ApproachPosition,worldRadius,rect,MapUp);
            return new Rect(rect.x+8,rect.y+8,rect.width-16,rect.height-16).Contains(point);
        }

        private void DrawWorkMarkers(Rect rect)
        {
            if(interactions==null&&player!=null)interactions=player.GetComponent<InteractionRegistry>();
            if(interactions==null)return;
            string hovered=null;Vector2 hover=default;
            foreach(var item in interactions.Targets)
            {
                if(item is not TimedInteractionTarget target||!TryProjectWorkMarker(target,rect,out var at))continue;
                var matrix=GUI.matrix;var color=GUI.color;
                try
                {
                    GUIUtility.RotateAroundPivot(45,at);GUI.color=new Color(.02f,.07f,.10f,.95f);
                    GUI.DrawTexture(new Rect(at.x-5,at.y-5,10,10),Texture2D.whiteTexture);
                    GUI.color=new Color(.20f,.80f,1);GUI.DrawTexture(new Rect(at.x-3,at.y-3,6,6),Texture2D.whiteTexture);
                }
                finally{GUI.matrix=matrix;GUI.color=color;}
                if((Event.current.mousePosition-at).sqrMagnitude<=81){hovered=target.DisplayName;hover=at;}
            }
            if(hovered!=null)
            {
                float width=Mathf.Min(rect.width,220);float x=Mathf.Clamp(hover.x-width*.5f,rect.x,rect.xMax-width);
                GUI.Box(new Rect(x,Mathf.Clamp(hover.y-31,rect.y,rect.yMax-28),width,27),hovered);
            }
        }

        private void OnGUI()
        {
            if (HudPointerBlocker.ModalOpen) return;

            Rect panel = Bounds;
            DrawPanel(panel);
            Rect inner = Inner(panel);
            float header = Mathf.Clamp(inner.height * 0.10f, 18, 28);
            var headerRect = new Rect(inner.x, inner.y, inner.width, header);
            bool isAttackMap=targeting!=null &&
                targeting.Mode==SkillTargetingMode.AttackMove;
            var titleRect = atlas == null ? headerRect : new Rect(headerRect.x, headerRect.y, headerRect.width-64, headerRect.height);
            HudChrome.DrawHeader(titleRect, Theme,
                isAttackMap?MapName+"  /  ATTACK-MOVE":MapName);

            if (atlas != null && GUI.Button(new Rect(headerRect.xMax-63,headerRect.y+1,62,header-2),"World [M]"))
                atlas.Toggle();

            var mapRect = new Rect(inner.x, inner.y + header, inner.width, inner.height - header);
            if (mapTexture != null)
                GUI.DrawTexture(mapRect, mapTexture, ScaleMode.StretchToFill, false);
            else
                GUI.Box(mapRect, "Map");
            if(fog!=null && fogMap!=null)
                GUI.DrawTexture(mapRect,fogMap,ScaleMode.StretchToFill,true);
            DrawWorkMarkers(mapRect);

            var corner=GameplayPreferences.Current.MinimapCompassCorner;
            var compassBounds=MinimapCompass.BoundsFor(mapRect,corner);
            MinimapCompass.Draw(mapRect,MapUp,corner);
            if(isAttackMap)
                HudChrome.Border(mapRect,2f,new Color(.94f,.30f,.18f,.88f));
            GUI.Label(
                mapRect,
                "+",
                new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = Mathf.RoundToInt(Mathf.Clamp(mapRect.height * 0.08f, 12, 24))
                });
            float zoomX=corner==MinimapCompassCorner.BottomRight?mapRect.x:mapRect.xMax-52;
            var plus=new Rect(zoomX,mapRect.yMax-26,24,24);
            var minus=new Rect(zoomX+26,mapRect.yMax-26,24,24);
            if(GUI.Button(plus,"+"))Zoom(.8f);
            if(GUI.Button(minus,"-"))Zoom(1.25f);

            // Left or right-click on the actual map image issues movement.
            // Keep zoom buttons interactive and never move in HUD Edit Mode.
            var evt=Event.current;
            if(evt.type==EventType.MouseDown && (evt.button==0||evt.button==1) &&
                mapRect.Contains(evt.mousePosition) &&
                !plus.Contains(evt.mousePosition) &&
                !minus.Contains(evt.mousePosition) &&
                !(HudLayoutManager.Current?.EditMode ?? false))
            {
                if(!compassBounds.Contains(evt.mousePosition))
                    TryMoveFromMapClick(mapRect,evt.mousePosition);
                evt.Use(); // A minimap click must never leak to world interactions.
            }
        }


    }
}
