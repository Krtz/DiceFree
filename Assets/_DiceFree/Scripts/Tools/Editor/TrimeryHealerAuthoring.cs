using System;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.UI;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DiceFree.EditorTools
{
    public static class TrimeryHealerAuthoring
    {
        public const string Name = "Trimery priest - placeholder healer";
        public const string Art = "Assets/_DiceFree/Art/TrimeryHealer";
        static void Folder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            Folder(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
        static Material Mat(string name, Color color)
        {
            string path = Art + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            material.SetFloat("_Smoothness", .2f);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }
        static void Shape(Transform parent, string name, PrimitiveType type, Vector3 at, Vector3 size, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = at;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.layer = 2;
        }
        public static HealerInteraction InstallInto(Scene scene)
        {
            var all = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToArray();
            var parent = all.Single(t => t.name == TrimeryVillageAuthoring.RootName);
            var old = parent.Find(Name);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Folder(Art);
            var ivory = Mat("Ivory robe", new Color(.88f, .85f, .72f));
            var blue = Mat("Soft blue vestment", new Color(.28f, .57f, .72f));
            var gold = Mat("Warm gold", new Color(.93f, .70f, .25f));
            var dark = Mat("Gentle dark eyes", new Color(.12f, .16f, .20f));
            var wood = Mat("Staff wood", new Color(.37f, .25f, .15f));
            var go = new GameObject(Name);
            go.transform.SetParent(parent, false);
            go.transform.position = CornbergLandscape.Ground(244, 190);
            // Interaction selection uses layer 10; not layer 9 (solid world/navigation).
            go.layer = 10;
            var selection = go.AddComponent<CapsuleCollider>();
            selection.center = Vector3.up * .85f;
            selection.radius = .6f;
            selection.height = 1.8f;
            var visual = new GameObject("Soft priest blob").transform;
            visual.SetParent(go.transform, false);
            visual.localRotation = Quaternion.Euler(0, 90, 0);
            Shape(visual, "Round ivory body", PrimitiveType.Sphere, new Vector3(0, .68f, 0), new Vector3(1.1f, 1.35f, .95f), ivory);
            Shape(visual, "Blue stole", PrimitiveType.Cube, new Vector3(0, .78f, .43f), new Vector3(.26f, .95f, .08f), blue);
            Shape(visual, "Head", PrimitiveType.Sphere, new Vector3(0, 1.35f, 0), Vector3.one * .65f, ivory);
            foreach (float x in new[] { -.13f, .13f })
                Shape(visual, "Kind eye", PrimitiveType.Sphere, new Vector3(x, 1.41f, .29f), new Vector3(.085f, .1f, .055f), dark);
            Shape(visual, "Wooden staff", PrimitiveType.Cylinder, new Vector3(.68f, .8f, .1f), new Vector3(.065f, .8f, .065f), wood);
            Shape(visual, "Gold staff focus", PrimitiveType.Sphere, new Vector3(.68f, 1.7f, .1f), Vector3.one * .24f, gold);
            Shape(visual, "Gold healing emblem", PrimitiveType.Cube, new Vector3(0, .9f, .5f), new Vector3(.17f, .17f, .05f), gold);
            go.AddComponent<VillageVisualIdle>().Configure(visual, 401);
            var approach = new GameObject("Roadside treatment point").transform;
            approach.SetParent(go.transform, false);
            approach.position = CornbergLandscape.Ground(246, 190);
            var healer = go.AddComponent<HealerInteraction>();
            healer.ConfigureName("Trimery Priest - free healing");
            healer.ConfigureApproach(approach);
            var player = all.Select(t => t.GetComponent<TraversalInput>()).First(t => t != null).GetComponent<CombatActor>();
            go.AddComponent<InteractionPrompt>().Configure(healer, player);
            return healer;
        }
        [CliCommand("dicefree.trimery.healer.install", "Install or refresh only the Trimery placeholder healer, preserving other village work.")]
        public static object Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode only");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save modified scenes first");
            var previous = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene = EditorSceneManager.OpenScene(TrimeryVillageAuthoring.ScenePath);
                var healer = InstallInto(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed");
                AssetDatabase.SaveAssets();
                return new { installed = true, name = healer.DisplayName, position = healer.transform.position.ToString(), approach = healer.ApproachPosition.ToString() };
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
        }
    }
}
