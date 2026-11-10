using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.World
{
    // Rendering and picking follow the occupied storey. Navigation remains the baked,
    // continuous staircase; hiding a ceiling never changes the agent's path.
    [DefaultExecutionOrder(600)]
    public sealed class ArboriumInterior : MonoBehaviour
    {
        public enum PartKind { Floor, Stair, Wall, Roof }
        [Serializable] public sealed class Part
        {
            public GameObject root;
            public PartKind kind;
            public int floor;
            public Vector3 outward;
            [NonSerialized] public Renderer[] renderers;
            [NonSerialized] public Collider[] colliders;
            [NonSerialized] public Light[] lights;
        }
        [SerializeField] private string buildingName;
        [SerializeField] private Vector2 footprint;
        [SerializeField] private float storeyHeight;
        [SerializeField] private int floorCount;
        [SerializeField] private Part[] parts;
        [SerializeField] private string[] floorNames=Array.Empty<string>();
        private WorldFogOfWar fog;
        private Camera worldCamera;
        private static readonly HashSet<Collider> transparentWalls = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPicking() => transparentWalls.Clear();
        public bool Inside { get; private set; }
        public int CurrentFloor { get; private set; }
        public int FloorCount => floorCount;
        public float StoreyHeight => storeyHeight;
        public string BuildingName => buildingName;
        public Vector2 Footprint => footprint;
        public string CurrentFloorName => CurrentFloor>=0&&CurrentFloor<floorNames.Length?floorNames[CurrentFloor]:"Floor "+(CurrentFloor+1);
        public string[] FloorNames => (string[])floorNames.Clone();
        public void ConfigureFloorNames(params string[] names) => floorNames=names??Array.Empty<string>();
        public void Configure(string title, Vector2 size, float height, int count, Part[] surfaces)
        { buildingName=title; footprint=size; storeyHeight=height; floorCount=count; parts=surfaces; }
        private void Awake()
        {
            foreach(var part in parts)
            { part.renderers=part.root.GetComponentsInChildren<Renderer>(true); part.colliders=part.root.GetComponentsInChildren<Collider>(true);part.lights=part.root.GetComponentsInChildren<Light>(true); }
        }
        private void LateUpdate()
        {
            if(fog==null) fog=FindAnyObjectByType<WorldFogOfWar>();
            if(worldCamera==null) worldCamera=Camera.main;
            if(fog?.Revealer==null) return;
            var p=transform.InverseTransformPoint(fog.Revealer.position);
            Inside=Mathf.Abs(p.x)<footprint.x*.5f+.1f && Mathf.Abs(p.z)<footprint.y*.5f+.1f && p.y>-.6f && p.y<floorCount*storeyHeight;
            CurrentFloor=Inside?FloorAtHeight(p.y,storeyHeight,floorCount):-1;
            var toCamera=worldCamera!=null?transform.InverseTransformDirection(worldCamera.transform.position-transform.position):new Vector3(-1,1,-1);
            foreach(var part in parts)
            {
                bool visible=!Inside || part.kind switch
                {
                    PartKind.Roof=>false,
                    PartKind.Floor=>part.floor<=CurrentFloor,
                    PartKind.Stair=>part.floor<=CurrentFloor && part.floor>=CurrentFloor-1,
                    PartKind.Wall=>part.floor<CurrentFloor || part.floor==CurrentFloor && Vector3.Dot(part.outward,toCamera)<=0,
                    _=>true
                };
                foreach(var renderer in part.renderers) renderer.enabled=visible;
                foreach(var light in part.lights)light.enabled=visible;
                foreach(var collider in part.colliders)
                {
                    // Solid walls/towers retain collision even when cut away; clicks pass through.
                    if(collider.gameObject.layer==9)
                    { if(visible) transparentWalls.Remove(collider); else transparentWalls.Add(collider); }
                    else collider.enabled=visible;
                }
            }
        }
        public static int FloorAtHeight(float height, float storeyHeight, int count) =>
            Mathf.Clamp(Mathf.FloorToInt(height / storeyHeight + .5f), 0, count - 1);

        public static bool RaycastWalkableGround(Ray ray, out RaycastHit hit) =>
            Physics.Raycast(ray, out hit, 1500f, 1 << 8, QueryTriggerInteraction.Ignore);

        public static bool RaycastGround(Ray ray,out RaycastHit hit)
        {
            var hits=Physics.RaycastAll(ray,1500f,(1<<8)|(1<<9),QueryTriggerInteraction.Ignore);
            Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            foreach(var candidate in hits) if(!transparentWalls.Contains(candidate.collider)){hit=candidate;return true;}
            hit=default;return false;
        }
        private void OnDisable()
        {
            if(parts!=null)foreach(var part in parts)
            {
                if(part.renderers!=null)foreach(var renderer in part.renderers)if(renderer!=null)renderer.enabled=true;
                if(part.lights!=null)foreach(var light in part.lights)if(light!=null)light.enabled=true;
                if(part.colliders!=null)foreach(var collider in part.colliders){transparentWalls.Remove(collider);if(collider!=null)collider.enabled=true;}
            }
            Inside=false;CurrentFloor=-1;
        }
        private void OnGUI()
        {
            if(!Inside)return;
            GUI.Box(new Rect(Screen.width*.5f-230,78,460,27),buildingName+"  ·  "+CurrentFloorName+"  ("+(CurrentFloor+1)+" / "+floorCount+")");
        }
    }
}
