using UnityEngine;
using UnityEditor;

namespace DiceFree.EditorTools
{
    internal static class BlockoutShapes
    {
        internal const string Root = "Assets/_DiceFree";
        internal static Material Grass, Path, Stone, Timber, Plaster, Roof, Leaf, LeafLight, Water, Soil, Crop, Glow, Dark;

        internal static void Initialize()
        {
            System.IO.Directory.CreateDirectory(Root + "/Materials/Blockout");
            System.IO.Directory.CreateDirectory(Root + "/Art/Blockout");
            Grass = Material("Meadow", "6B9147"); Path = Material("Warm earth", "B4A078");
            Stone = Material("Fieldstone", "78867F"); Timber = Material("Timber", "65513A");
            Plaster = Material("Limewash", "DDCEAA"); Roof = Material("Weathered roof", "685D4C");
            Leaf = Material("Forest", "315A37"); LeafLight = Material("Sunlit foliage", "658443");
            Water = Material("Mountain water", "4EA7A4"); Soil = Material("Tilled soil", "6B5135");
            Crop = Material("Corn", "A5B856"); Glow = Material("Well light", "A6EAD5"); Dark = Material("Unlit interior", "24332C");
            Glow.EnableKeyword("_EMISSION"); Glow.SetColor("_EmissionColor", new Color(0.25f, 0.8f, 0.55f) * 2f);
            EditorUtility.SetDirty(Glow);
        }

        private static Material Material(string name, string hex)
        {
            var path = Root + "/Materials/Blockout/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            material.SetColor("_BaseColor", color); material.SetFloat("_Smoothness", 0.12f);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        internal static Transform Group(string name, Transform parent = null)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        internal static GameObject Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale,
            Material material, Transform parent, bool solid = true, int layer = 9)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.position = position; obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            obj.layer = solid ? layer : 2;
            if (!solid) Object.DestroyImmediate(obj.GetComponent<Collider>());
            return obj;
        }

        internal static GameObject Box(string name, Vector3 position, Vector3 size, Material material, Transform parent, bool solid = true, int layer = 9)
            => Shape(name, PrimitiveType.Cube, position, size, material, parent, solid, layer);

        internal static GameObject MeshObject(string name, Mesh mesh, Material material, Transform parent, bool collider = false)
        {
            var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<MeshFilter>().sharedMesh = mesh;
            obj.GetComponent<Renderer>().sharedMaterial = material;
            obj.layer = collider ? 8 : 2;
            if (collider) obj.AddComponent<MeshCollider>().sharedMesh = mesh;
            return obj;
        }

        internal static Mesh SaveMesh(string name, Vector3[] vertices, int[] triangles)
        {
            var path = Root + "/Art/Blockout/" + name + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) { mesh = new Mesh { name = name }; AssetDatabase.CreateAsset(mesh, path); }
            mesh.Clear(); mesh.vertices = vertices; mesh.triangles = triangles;
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh);
            return mesh;
        }

        internal static void Label(string name, Vector3 position, Transform parent)
        {
            var obj = new GameObject("Sign - " + name, typeof(TextMesh));
            obj.layer = 2; obj.transform.SetParent(parent, false); obj.transform.position = position;
            obj.transform.rotation = Quaternion.Euler(42, 45, 0);
            var text = obj.GetComponent<TextMesh>();
            text.text = name; text.fontSize = 64; text.characterSize = 0.1f;
            text.anchor = TextAnchor.MiddleCenter; text.color = new Color(1f, 0.94f, 0.77f);
        }

        internal static void Tree(Vector3 ground, float height, Transform parent, bool collision = true)
        {
            var root = Group("Woodland tree", parent);
            Shape("Trunk", PrimitiveType.Cylinder, ground + Vector3.up * height * 0.3f,
                new Vector3(height * 0.09f, height * 0.3f, height * 0.09f), Timber, root, collision);
            Shape("Canopy", PrimitiveType.Sphere, ground + Vector3.up * height * 0.8f,
                new Vector3(height * 0.68f, height * 0.7f, height * 0.65f), Leaf, root, false);
            Shape("Sunward crown", PrimitiveType.Sphere, ground + new Vector3(-height * 0.12f, height * 0.99f, 0),
                new Vector3(height * 0.47f, height * 0.4f, height * 0.48f), LeafLight, root, false);
        }
    }
}
