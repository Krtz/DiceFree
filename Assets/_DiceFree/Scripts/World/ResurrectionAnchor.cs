using UnityEngine;

namespace DiceFree.World
{
    public sealed class ResurrectionAnchor : MonoBehaviour
    {
        [SerializeField] private string stableId;
        [SerializeField] private Transform activationPoint;
        [SerializeField,Min(.1f)] private float activationRadius=3.5f;
        public string StableId => stableId;
        public Vector3 ActivationPosition => activationPoint!=null?activationPoint.position:transform.position;
        public float ActivationRadius => activationRadius;
        public bool CanActivateFrom(Vector3 position) =>
            Vector3.ProjectOnPlane(ActivationPosition-position,Vector3.up).sqrMagnitude<=activationRadius*activationRadius;
        public void Configure(string id) => stableId=id;
        public void Configure(string id,Transform visibleStone,float reach)
        {
            stableId=id;activationPoint=visibleStone;activationRadius=Mathf.Max(.1f,reach);
        }
    }
}
