using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Visuals
{
    /// <summary>Follows a bone while remaining inside the item's activation hierarchy. Serializable in authored props.</summary>
    [DefaultExecutionOrder(400)]
    public sealed class PlainGearAttachment : MonoBehaviour, IPortraitPoseable
    {
        [SerializeField] private Transform bone;
        [SerializeField] private Vector3 offset;
        [SerializeField] private Quaternion rotation;
        public void Configure(Transform value)
        {
            bone=value;
            offset=bone.InverseTransformPoint(transform.position);
            rotation=Quaternion.Inverse(bone.rotation)*transform.rotation;
        }
        private void LateUpdate()=>ApplyPortraitPose();
        public void ApplyPortraitPose()
        {
            if(bone!=null)transform.SetPositionAndRotation(bone.TransformPoint(offset),bone.rotation*rotation);
        }
    }
}
