using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Items
{
    /// <summary>Shared physical loot chest for fixed drops and dungeon rewards.</summary>
    public static class WorldLootVisual
    {
        private const string Path = "WorldLoot/TreasureChestModel";
        private static readonly Dictionary<string,Material> fallbackMaterials = new();

        public static GameObject Create(Vector3 point, string name, float width = .8f,
            bool forceOriginal = false)
        {
            var root = new GameObject(name);
            root.layer = 11;
            root.transform.position = point;
            var collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.0f, .8f, 1.0f);
            collider.center = Vector3.up * .4f;
            var art = forceOriginal ? null : Resources.Load<GameObject>(Path);
            // The source repo is public: commercially licensed raw asset files
            // remain local to licensed developer machines and are gitignored.
            // This original, hand-built fallback makes clean checkouts playable.
            var body = art == null ? BuildOriginalChest(root.transform) :
                UnityEngine.Object.Instantiate(art, root.transform);
            body.name = art == null ? "Original DiceFree treasure chest" :
                "Licensed animated cartoon treasure chest (visual)";
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = Vector3.one;

            foreach(var child in body.GetComponentsInChildren<Collider>(true))
                child.enabled = false;
            foreach(var child in body.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = 2; // model cannot steal chest interaction raycasts
            foreach(var behaviour in body.GetComponentsInChildren<MonoBehaviour>(true))
                behaviour.enabled = false; // imported demo logic must never drive gameplay

            var renderers = body.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                throw new InvalidOperationException("Chest asset has no renderers");
            Bounds bounds = renderers[0].bounds;
            foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            float largest = Mathf.Max(bounds.size.x, bounds.size.z, .001f);
            float scale = width / largest;
            body.transform.localScale = Vector3.one * scale;
            // Ground the imported chest at Y=0 relative to the loot root.
            bounds = renderers[0].bounds;
            foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            body.transform.position += Vector3.up * (point.y - bounds.min.y);
            return root;
        }
        // Completely original geometric chest, never copied from an Asset Store FBX.
        // This branch is used by public clean checkouts without licensed packages.
        private static GameObject BuildOriginalChest(Transform parent)
        {
            var root = new GameObject("Original DiceFree Chest");
            root.transform.SetParent(parent,false);
            Material red = Solid("Mahogany wood",new Color(.36f,.095f,.085f),.14f);
            Material gold = Solid("Dull brass",new Color(.78f,.50f,.14f),.48f);
            Material dark = Solid("Iron latch",new Color(.12f,.13f,.15f),.36f);
            void Part(string name,Vector3 location,Vector3 size,Material material,
                Quaternion rotation=default)
            {
                var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.name=name;
                obj.transform.SetParent(root.transform,false);
                obj.transform.localPosition=location;
                obj.transform.localRotation=rotation==default?Quaternion.identity:rotation;
                obj.transform.localScale=size;
                var col=obj.GetComponent<Collider>();
                col.enabled=false;
                if(Application.isPlaying)UnityEngine.Object.Destroy(col);
                else UnityEngine.Object.DestroyImmediate(col);
                obj.GetComponent<Renderer>().sharedMaterial=material;
            }
            Part("Rounded lower mahogany chest",new Vector3(0,.33f,0),
                new Vector3(.98f,.57f,.75f),red);
            Part("Lower brass rim",new Vector3(0,.075f,0),
                new Vector3(1.06f,.065f,.81f),gold);
            Part("Lid lower brass seam",new Vector3(0,.616f,0),
                new Vector3(1.04f,.056f,.83f),gold);
            // Twenty curved solid wooden roof ribs create a rounded trunk profile.
            for(int i=0;i<20;i++)
            {
                float t=(i+.5f)/20f;
                float angle=t*Mathf.PI;
                float z=.36f*Mathf.Cos(angle);
                float y=.615f+.275f*Mathf.Sin(angle);
                float tangent=Mathf.Atan2(.275f*Mathf.Cos(angle),
                    -.36f*Mathf.Sin(angle))*Mathf.Rad2Deg;
                var bend=Quaternion.Euler(tangent-90f,0,0);
                Part("Curved oak lid segment "+i,new Vector3(0,y,z),
                    new Vector3(.98f,.065f,.085f),red,bend);
                if(i==0||i==19||i==10)
                    Part("Curved brass reinforcing band "+i,new Vector3(0,y+.025f,z),
                        new Vector3(1.06f,.045f,.070f),gold,bend);
            }
            for(int side=-1;side<=1;side+=2)
            {
                Part("Side brass corner "+side,new Vector3(side*.475f,.34f,0),
                    new Vector3(.073f,.60f,.77f),gold);
                for(int j=0;j<3;j++)
                {
                    Part("Copper corner rivet "+side+"-"+j,
                        new Vector3(side*.52f,.19f+j*.16f,-.35f),
                        new Vector3(.049f,.05f,.036f),gold);
                }
            }
            Part("Lockplate",new Vector3(0,.42f,-.407f),new Vector3(.20f,.21f,.052f),gold);
            Part("Iron keyhole",new Vector3(0,.43f,-.45f),new Vector3(.06f,.11f,.012f),dark);
            Part("Front wooden panel",new Vector3(0,.34f,-.391f),
                new Vector3(.78f,.38f,.020f),red);
            Part("Chest front lock foreground",new Vector3(0,.43f,-.46f),
                new Vector3(.20f,.20f,.042f),gold);
            Part("Keyhole foreground",new Vector3(0,.43f,-.487f),
                new Vector3(.055f,.09f,.014f),dark);
            return root;
        }
        private static Material Solid(string name,Color tint,float shine)
        {
            if(fallbackMaterials.TryGetValue(name,out var cached) && cached!=null) return cached;
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};
            material.SetColor("_BaseColor",tint);
            material.SetFloat("_Smoothness",shine);
            fallbackMaterials[name]=material;
            return material;
        }
    }
}
