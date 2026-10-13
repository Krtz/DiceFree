using UnityEngine;
namespace DiceFree.Development
{
    public sealed class DevelopmentSign : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (Camera.main != null) transform.rotation = Camera.main.transform.rotation;
        }
    }
}
