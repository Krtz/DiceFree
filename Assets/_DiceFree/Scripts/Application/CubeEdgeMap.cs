using DiceFree.World;
using UnityEngine;

namespace DiceFree.Development
{
    /// <summary>Bounded dev-face link. Visual surrogates never own physics, navigation or actors.</summary>
    [DisallowMultipleComponent]
    public sealed class CubeEdgeMap : MonoBehaviour
    {
        public GameObject facePreview;
        public GameObject adjacentPreview;
        public WorldEdgeLink link=new();
        public string destinationScene;
        public string arrivalSpawnId;
        public int outwardSign=1;
        public float halfSize=24f;
        public Vector3 Origin {get {var m=GetComponent<DevelopmentMap>();return new Vector3(m.bounds.center.x,0,m.bounds.center.y);}}
        public int Sign=>outwardSign>=0?1:-1;
        public Vector3 Pivot=>Origin+Vector3.forward*(Sign*halfSize);
        public Vector3 Approach=>Origin+Vector3.forward*(Sign*(halfSize-3f));
        public bool AtCrossing(Vector3 point)
        {
            var local=point-Origin;
            return Mathf.Abs(local.x)<2.4f&&Sign*local.z>=halfSize-3.5f;
        }
    }
}
