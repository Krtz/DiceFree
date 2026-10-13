using UnityEngine;
using DiceFree.World;
namespace DiceFree.Development
{
    public sealed class DevelopmentMap : MonoBehaviour
    {
        public string worldId;
        public Vector3 spawn;
        public Rect bounds;
        public ResurrectionAnchor anchor;
    }
}
