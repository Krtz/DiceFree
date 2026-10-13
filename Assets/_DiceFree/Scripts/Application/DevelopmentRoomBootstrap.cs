using UnityEngine;
namespace DiceFree.Development
{
    public sealed class DevelopmentRoomBootstrap : MonoBehaviour
    {
        public GameObject sessionTemplate;
        private void Awake()
        {
            if (DevelopmentRoomController.Current != null) { Destroy(sessionTemplate); return; }
            DiceFree.Foundation.DevelopmentSession.Begin();
            string root = DiceFree.Persistence.EchoSaveLocation.ResolveRoot();
            if (root != null) DiceFree.Persistence.EchoIdentity.QueueNewEchoName("Developer", root);
            sessionTemplate.SetActive(true);
        }
    }
}
