using UnityEngine;

namespace DiceFree.World
{
    /// <summary>
    /// Portable Trimery prop: works with tracked low-poly blockout art when
    /// locally licensed Asset Store prefabs are absent. If available, show
    /// the local model; never put vendor GUID dependencies into public scenes.
    /// </summary>
    public sealed class TrimeryArtSlot : MonoBehaviour
    {
        [SerializeField] private string licensedResource;
        [SerializeField] private Transform fallbackVisual;
        [SerializeField] private Vector3 licensedScale=Vector3.one;
        [SerializeField] private Vector3 licensedOffset=Vector3.zero;
        public string Resource => licensedResource;
        public bool LoadedLicensedArt { get; private set; }
        public void Configure(string resource,Transform fallback,
            Vector3 scale,Vector3 offset)
        {
            licensedResource=resource;
            fallbackVisual=fallback;
            licensedScale=scale;
            licensedOffset=offset;
        }
        private void Start()
        {
            if(string.IsNullOrEmpty(licensedResource))return;
            var prefab=Resources.Load<GameObject>("TrimeryLicensed/"+licensedResource);
            if(prefab==null)return;
            var art=Instantiate(prefab,transform);
            art.name="Licensed "+licensedResource+" (local)";
            art.transform.localPosition=licensedOffset;
            art.transform.localScale=licensedScale;
            art.transform.localRotation=Quaternion.identity;
            foreach(var collider in art.GetComponentsInChildren<Collider>(true))
                collider.enabled=false; // Authored fallback footprints own physics.
            foreach(var behaviour in art.GetComponentsInChildren<MonoBehaviour>(true))
                if(behaviour!=null)behaviour.enabled=false;
            if(fallbackVisual!=null)fallbackVisual.gameObject.SetActive(false);
            LoadedLicensedArt=true;
        }
    }
}
