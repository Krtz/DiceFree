using UnityEngine;
namespace DiceFree.Visuals
{
    /// <summary>
    /// Both player Greatblade and retired swordswoman use this identical
    /// model source. Licensed art stays local; portable silhouette is fallback.
    /// </summary>
    public sealed class GreatbladePrototypeVisual : MonoBehaviour
    {
        public const string ResourcePath="GreatbladeLicensed/GreatbladeHero";
        [SerializeField] private bool previewSpin;
        private GameObject model;
        private Animator animator;
        public bool LicensedModelLoaded {get;private set;}
        public Transform Model=>model!=null?model.transform:null;

        private static Material Material(Color tint)
        {
            Shader shader=Shader.Find("Universal Render Pipeline/Lit")??
                Shader.Find("Standard");
            var material=new Material(shader){color=tint};
            if(material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor",tint);
            return material;
        }
        private static GameObject Shape(PrimitiveType type,Transform root,
            string label,Vector3 point,Vector3 dimensions,Material mat)
        {
            var go=GameObject.CreatePrimitive(type);
            go.name=label;
            go.transform.SetParent(root,false);
            go.transform.localPosition=point;
            go.transform.localScale=dimensions;
            go.GetComponent<Renderer>().sharedMaterial=mat;
            var col=go.GetComponent<Collider>();
            if(col!=null)Destroy(col);
            return go;
        }
        private GameObject Fallback()
        {
            var root=new GameObject("Original Greatblade silhouette");
            root.transform.SetParent(transform,false);
            var steel=Material(new Color(.55f,.59f,.68f));
            var jacket=Material(new Color(.14f,.24f,.35f));
            var leather=Material(new Color(.3f,.18f,.11f));
            var skin=Material(new Color(.9f,.67f,.51f));
            Shape(PrimitiveType.Capsule,root.transform,"Torso",
                new Vector3(0,1.48f,0),new Vector3(.71f,.95f,.51f),jacket);
            Shape(PrimitiveType.Sphere,root.transform,"Head",
                new Vector3(0,2.2f,0),new Vector3(.44f,.51f,.42f),skin);
            Shape(PrimitiveType.Sphere,root.transform,"Hair",
                new Vector3(0,2.45f,-.1f),new Vector3(.5f,.28f,.46f),leather);
            foreach(int direction in new[]{-1,1})
            {
                Shape(PrimitiveType.Capsule,root.transform,
                    direction<0?"Left leg":"Right leg",
                    new Vector3(direction*.24f,.65f,0),
                    new Vector3(.34f,.62f,.37f),steel);
                Shape(PrimitiveType.Capsule,root.transform,
                    direction<0?"Left shoulder":"Right shoulder",
                    new Vector3(direction*.53f,1.76f,0),
                    new Vector3(.33f,.43f,.49f),steel);
            }
            var sword=Shape(PrimitiveType.Cube,root.transform,
                "Oversized two-handed greatsword",new Vector3(.8f,1.62f,.25f),
                new Vector3(.22f,2.5f,.35f),steel);
            sword.transform.localRotation=Quaternion.Euler(0,0,30);
            Shape(PrimitiveType.Cube,sword.transform,"Wide crossguard",
                new Vector3(0,-.38f,0),new Vector3(2.5f,.10f,1f),leather);
            return root;
        }
        private void Awake()
        {
            var prefab=Resources.Load<GameObject>(ResourcePath);
            model=prefab!=null?Instantiate(prefab,transform):Fallback();
            LicensedModelLoaded=prefab!=null;
            model.name=prefab!=null?"Greatblade shared licensed base":
                "Greatblade original fallback";
            model.transform.localPosition=Vector3.zero;
            model.transform.localRotation=Quaternion.identity;
            animator=model.GetComponentInChildren<Animator>(true);
        }
        public void SetMotion(float speed,bool attack=false,bool spin=false)
        {
            if(animator==null)return;
            animator.SetFloat("Speed",Mathf.Max(0,speed));
            if(attack)animator.SetTrigger("Attack");
            if(spin)animator.SetTrigger("SpinDash");
        }
        private void Update()
        {
            if(previewSpin && animator!=null && Time.frameCount%300==0)
                SetMotion(0,spin:true);
        }
    }
}
