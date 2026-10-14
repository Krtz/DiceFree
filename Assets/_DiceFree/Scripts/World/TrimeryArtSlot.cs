using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.World
{
    /// <summary>Optional local town art with portable geometry and collision.</summary>
    public sealed class TrimeryArtSlot : MonoBehaviour
    {
        [SerializeField] private string licensedResource;
        [SerializeField] private Transform fallbackVisual;
        [SerializeField] private Vector3 licensedScale=Vector3.one;
        [SerializeField] private Vector3 licensedOffset=Vector3.zero;
        readonly List<Material> ownedMaterials=new();
        public string Resource => licensedResource;
        public bool LoadedLicensedArt { get; private set; }
        public int ConvertedMaterials { get; private set; }
        public void Configure(string resource,Transform fallback,Vector3 scale,Vector3 offset)
        {
            licensedResource=resource;fallbackVisual=fallback;
            licensedScale=scale;licensedOffset=offset;
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
            if(!PrepareMaterials(art)){Destroy(art);return;}
            foreach(var collider in art.GetComponentsInChildren<Collider>(true))
                collider.enabled=false; // Portable authored footprints own physics.
            foreach(var behaviour in art.GetComponentsInChildren<MonoBehaviour>(true))
                if(behaviour!=null)behaviour.enabled=false;
            if(fallbackVisual!=null)fallbackVisual.gameObject.SetActive(false);
            LoadedLicensedArt=true;
        }
        bool PrepareMaterials(GameObject art)
        {
            var lit=Shader.Find("Universal Render Pipeline/Lit");
            if(lit==null || !lit.isSupported)return false;
            var converted=new Dictionary<Material,Material>();
            foreach(var renderer in art.GetComponentsInChildren<Renderer>(true))
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    var original=materials[i];
                    if(original!=null && original.shader!=null && original.shader.isSupported &&
                        original.shader.name.StartsWith("Universal Render Pipeline/",System.StringComparison.Ordinal))continue;
                    if(original!=null && converted.TryGetValue(original,out var existing))
                    {materials[i]=existing;continue;}
                    var material=new Material(lit){name=(original!=null?original.name:"Timber")+" (URP)",enableInstancing=true};
                    Texture texture=original!=null?original.mainTexture:null;
                    material.SetTexture("_BaseMap",texture);
                    Color tint=original!=null && original.HasProperty("_Tint")?original.GetColor("_Tint"):
                        original!=null && original.HasProperty("_Color")?original.GetColor("_Color"):Color.white;
                    material.SetColor("_BaseColor",tint);
                    material.SetFloat("_Smoothness",.15f);
                    material.SetFloat("_Metallic",0f);
                    if(original!=null)
                    {
                        material.SetTextureScale("_BaseMap",original.mainTextureScale);
                        material.SetTextureOffset("_BaseMap",original.mainTextureOffset);
                        if(original.HasProperty("_BumpMap") && original.GetTexture("_BumpMap")!=null)
                        {
                            material.SetTexture("_BumpMap",original.GetTexture("_BumpMap"));
                            material.EnableKeyword("_NORMALMAP");
                        }
                        converted.Add(original,material);
                    }
                    ownedMaterials.Add(material);ConvertedMaterials++;materials[i]=material;
                }
                renderer.sharedMaterials=materials;
            }
            return true;
        }
        void OnDestroy()
        {
            foreach(var material in ownedMaterials)if(material!=null)Destroy(material);
            ownedMaterials.Clear();
        }
    }
}
