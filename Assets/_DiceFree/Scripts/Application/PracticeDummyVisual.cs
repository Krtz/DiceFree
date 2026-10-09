using UnityEngine;
namespace DiceFree.Dungeons
{
    public static class PracticeDummyVisual
    {
        static Material wood,straw,red,cream;
        static Material Mat(ref Material cache,string name,Color color)
        {if(cache==null){cache=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};cache.color=color;}return cache;}
        public static Transform Create(Transform owner)
        {
            var root=new GameObject("Presentation").transform;root.SetParent(owner,false);
            var timber=Mat(ref wood,"Practice timber",new Color(.24f,.12f,.04f));var hay=Mat(ref straw,"Practice straw",new Color(.82f,.61f,.18f));
            Part(root,"Stake",PrimitiveType.Cube,new Vector3(0,.7f,0),new Vector3(.16f,1.4f,.16f),timber);
            Part(root,"Crossbar",PrimitiveType.Cube,new Vector3(0,1.15f,0),new Vector3(1.2f,.14f,.14f),timber);
            Part(root,"Straw torso",PrimitiveType.Capsule,new Vector3(0,1.12f,0),new Vector3(.7f,.43f,.45f),hay);
            Part(root,"Straw head",PrimitiveType.Sphere,new Vector3(0,1.78f,0),Vector3.one*.42f,hay);
            var face=Part(root,"Target face",PrimitiveType.Cylinder,new Vector3(0,1.18f,-.27f),new Vector3(.6f,.025f,.6f),Mat(ref cream,"Practice target ivory",new Color(.95f,.88f,.7f)));face.localRotation=Quaternion.Euler(90,0,0);
            var ring=Part(root,"Target red ring",PrimitiveType.Cylinder,new Vector3(0,1.18f,-.31f),new Vector3(.4f,.012f,.4f),Mat(ref red,"Practice target red",new Color(.7f,.06f,.04f)));ring.localRotation=Quaternion.Euler(90,0,0);
            var center=Part(root,"Target center",PrimitiveType.Cylinder,new Vector3(0,1.18f,-.335f),new Vector3(.17f,.008f,.17f),cream);center.localRotation=Quaternion.Euler(90,0,0);
            return root;
        }
        static Transform Part(Transform parent,string name,PrimitiveType kind,Vector3 at,Vector3 scale,Material material)
        {var obj=GameObject.CreatePrimitive(kind);obj.name=name;obj.layer=2;obj.transform.SetParent(parent,false);obj.transform.localPosition=at;obj.transform.localScale=scale;var collider=obj.GetComponent<Collider>();collider.enabled=false;Object.Destroy(collider);obj.GetComponent<Renderer>().sharedMaterial=material;return obj.transform;}
    }
}
