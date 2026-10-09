using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DiceFree.Items;
using DiceFree.UI;
using UnityEditor;
using UnityEngine;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    public static class ItemIconAuthoring
    {
        const string Root = "Assets/_DiceFree/Art/ItemIcons";
        const int Size = 192;
        static readonly string[] Names = { "Bronze Dagger", "Cornberg Work Gloves", "Farmer's Pants", "Forest Travel Shoes", "Slime Orb", "Slime Shield", "Slimy Farmer's Gloves", "Slimy Tophat" };
        static readonly string[] Sources = {
            "Assets/_DiceFree/Art/Weapons/BronzeDagger/BronzeDagger.prefab",
            "Assets/_DiceFree/Art/Characters/CornbergWorkGloves/Prefabs/CornbergWorkGlovesPresentation.prefab",
            "Assets/_DiceFree/Art/Characters/FarmersPants/Prefabs/FarmersPantsPresentation.prefab", "",
            "Assets/_DiceFree/Art/Equipment/SlimeDungeon/SlimeOrb.prefab",
            "Assets/_DiceFree/Art/Equipment/SlimeDungeon/SlimeShield.prefab",
            "Assets/_DiceFree/Art/Equipment/SlimeDungeon/SlimyFarmersGloves.prefab",
            "Assets/_DiceFree/Art/Equipment/SlimeDungeon/SlimyTophat.prefab" };
        static ItemDefinition[] Items() => AssetDatabase.FindAssets("t:ItemDefinition").Select(g => AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        [CliCommand("dicefree.item-icons.gui-open", "Open a lightweight real IMGUI rendering check without gameplay or save access.")]
        public static object OpenGUI()
        {
            var window = EditorWindow.GetWindow<ItemIconGUIValidation>(true, "Item icon GUI validation");
            window.items = Items(); window.drawn = 0;
            window.minSize = new Vector2(540, 240); window.Show(); window.Repaint();
            return new { success = true, message = "Poll dicefree.item-icons.gui-result after the window repaints." };
        }
        [CliCommand("dicefree.item-icons.gui-result", "Check real IMGUI repaint results and close the isolated validation window.")]
        public static object GUIResult()
        {
            var window = Resources.FindObjectsOfTypeAll<ItemIconGUIValidation>().FirstOrDefault();
            Require(window != null && window.drawn >= 16 && window.fallback, "GUI repaint has not completed or failed.");
            int count = window.drawn; window.Close();
            return new { success = true, marker = "ITEM_ICONS_GUI_VALIDATED", rendered = count, fallback = true };
        }
        static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }

        [CliCommand("dicefree.item-icons.install", "Render isolated equipment PNG sprites and persist only ItemDefinition icon references.")]
        public static object Install()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Requires stable Edit Mode.");
            var items = Items();
            // Check every source before writing anything. Unknown future items require an explicit mapping.
            Require(items.Length >= 8, "Expected at least eight definitions.");
            foreach (var item in items) Require(Array.IndexOf(Names, item.name) >= 0, "Add art mapping for " + item.name);
            foreach (string path in Sources.Where(p => p.Length > 0)) Require(AssetDatabase.LoadAssetAtPath<GameObject>(path) != null, "Missing genuine source: " + path);
            Directory.CreateDirectory(Root);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var item in items)
            {
                int index = Array.IndexOf(Names, item.name);
                string path = Root + "/" + item.stableId + ".png";
                Render(index, path);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = true;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = false;
                importer.isReadable = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.maxTextureSize = 256;
                importer.spritePixelsPerUnit = Size;
                importer.SaveAndReimport();
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Require(sprite != null, "Sprite import failed: " + path);
                // Targeted serialized field update preserves stats, IDs and all other authoring data.
                var serialized = new SerializedObject(item);
                serialized.FindProperty("icon").objectReferenceValue = sprite;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssetIfDirty(item);
            }
            return Validate();
        }

        static void Render(int index, string path)
        {
            var preview = new PreviewRenderUtility();
            var owned = new List<UnityEngine.Object>();
            GameObject source = null;
            Texture2D image = null;
            var previous = RenderTexture.active;
            bool previousAsync = ShaderUtil.allowAsyncCompilation;
            try
            {
                ShaderUtil.allowAsyncCompilation = false;
                GameObject model;
                if (Sources[index].Length > 0)
                {
                    source = PrefabUtility.LoadPrefabContents(Sources[index]);
                    model = UnityEngine.Object.Instantiate(source);
                    PrefabUtility.UnloadPrefabContents(source); source = null;
                }
                else model = Boots(owned);
                preview.AddSingleGO(model);
                model.SetActive(true);
                // Bake the source rig's rest pose; no character, animation or scene binding is changed.
                foreach (var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    var mesh = new Mesh(); skin.BakeMesh(mesh); owned.Add(mesh);
                    var part = new GameObject(skin.name + " icon bake");
                    part.transform.SetParent(skin.transform, false);
                    part.AddComponent<MeshFilter>().sharedMesh = mesh;
                    part.AddComponent<MeshRenderer>().sharedMaterials = skin.sharedMaterials;
                    if (index == 1) part.transform.position += Vector3.right * (skin.name.StartsWith("Left") ? -.32f : .32f);
                    skin.enabled = false;
                }
                if (index == 0) model.transform.rotation = Quaternion.Euler(-90, 0, -30);
                var renderers = model.GetComponentsInChildren<Renderer>(true).Where(r => r.enabled && r.gameObject.activeInHierarchy).ToArray();
                Require(renderers.Length > 0, "Source has no visible art: " + Names[index]);
                // Preview scenes have no baked ambient probe. Gentle material fill keeps dark
                // leather/canvas readable while the directional lights retain the sculpted shading.
                foreach (var renderer in renderers)
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(original =>
                    {
                        if (original == null) return null;
                        var material = new Material(original); owned.Add(material);
                        if (index != 4 && material.HasProperty("_EmissionColor") && material.HasProperty("_BaseColor"))
                        {
                            material.EnableKeyword("_EMISSION");
                            material.SetColor("_EmissionColor", material.GetColor("_BaseColor") * .55f);
                        }
                        return material;
                    }).ToArray();
                Bounds bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                var camera = preview.camera;
                camera.orthographic = true;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                camera.allowHDR = false; camera.allowMSAA = true;
                camera.nearClipPlane = .001f; camera.farClipPlane = 100;
                camera.transform.rotation = Quaternion.Euler(14, 160, 0);
                camera.transform.position = bounds.center - camera.transform.forward * (bounds.size.magnitude * 3 + 1);
                float extent = 0;
                for (int mask = 0; mask < 8; mask++)
                {
                    Vector3 point = bounds.center + Vector3.Scale(bounds.extents, new Vector3((mask & 1) == 0 ? -1 : 1, (mask & 2) == 0 ? -1 : 1, (mask & 4) == 0 ? -1 : 1));
                    Vector3 local = camera.transform.InverseTransformPoint(point);
                    extent = Mathf.Max(extent, Mathf.Abs(local.x), Mathf.Abs(local.y));
                }
                camera.orthographicSize = Mathf.Max(.01f, extent * 1.16f);
                preview.lights[0].intensity = index == 4 ? 1.9f : 3f;
                preview.lights[0].transform.rotation = Quaternion.Euler(35, 140, 0);
                preview.lights[1].intensity = index == 4 ? .95f : 1.5f;
                preview.lights[1].transform.rotation = Quaternion.Euler(25, 215, 0);
                preview.ambientColor = new Color(.32f, .32f, .36f);
                // Render at 2x then downsample for smooth transparent silhouettes, independent of project MSAA.
                preview.BeginPreview(new Rect(0, 0, Size * 2, Size * 2), GUIStyle.none);
                preview.Render(true);
                var texture = preview.EndPreview();
                RenderTexture.active = (RenderTexture)texture;
                image = new Texture2D(Size * 2, Size * 2, TextureFormat.RGBA32, false);
                image.ReadPixels(new Rect(0, 0, Size * 2, Size * 2), 0, 0); image.Apply();
                var pixels = image.GetPixels();
                var output = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
                owned.Add(output);
                var result = new Color[Size * Size];
                for (int y = 0; y < Size; y++) for (int x = 0; x < Size; x++)
                {
                    Color sum = Color.clear; float alpha = 0;
                    for (int dy = 0; dy < 2; dy++) for (int dx = 0; dx < 2; dx++)
                    {
                        Color p = pixels[(y * 2 + dy) * Size * 2 + x * 2 + dx];
                        sum.r += p.r * p.a; sum.g += p.g * p.a; sum.b += p.b * p.a; alpha += p.a;
                    }
                    result[y * Size + x] = alpha > 0 ? new Color(sum.r / alpha, sum.g / alpha, sum.b / alpha, alpha / 4) : Color.clear;
                }
                // Subtle offset silhouette shadow, composited behind the art with alpha (no opaque floor).
                for (int y = 0; y < Size; y++) for (int x = 0; x < Size; x++)
                {
                    float shadow = 0;
                    for (int dy = -2; dy <= 2; dy++) for (int dx = -2; dx <= 2; dx++)
                    {
                        int sx = x - 2 + dx, sy = y + 3 + dy;
                        if (sx >= 0 && sy >= 0 && sx < Size && sy < Size) shadow += result[sy * Size + sx].a / 25f;
                    }
                    Color c = result[y * Size + x]; float a = shadow * .18f * (1 - c.a);
                    float combined = c.a + a;
                    output.SetPixel(x, y, combined > 0 ? new Color(c.r * c.a / combined, c.g * c.a / combined, c.b * c.a / combined, combined) : Color.clear);
                }
                output.Apply(); File.WriteAllBytes(path, output.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                ShaderUtil.allowAsyncCompilation = previousAsync;
                if (source != null) PrefabUtility.UnloadPrefabContents(source);
                preview.Cleanup();
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                foreach (var obj in owned) UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        static GameObject Boots(List<UnityEngine.Object> owned)
        {
            var root = new GameObject("Forest travel boots - icon only");
            var leather = new Material(Shader.Find("Universal Render Pipeline/Lit")); owned.Add(leather);
            leather.color = new Color(.28f, .17f, .09f);
            var sole = new Material(leather); owned.Add(sole); sole.color = new Color(.075f, .065f, .045f);
            var cuff = new Material(leather); owned.Add(cuff); cuff.color = new Color(.42f, .39f, .22f);
            for (int i = 0; i < 2; i++)
            {
                float x = (i - .5f) * .40f;
                BootPart(root, "Rounded leather toe", PrimitiveType.Sphere, new Vector3(x, .14f, .12f), new Vector3(.30f, .23f, .52f), leather);
                BootPart(root, "Leather shaft", PrimitiveType.Cylinder, new Vector3(x, .32f, -.03f), new Vector3(.24f, .19f, .26f), leather);
                BootPart(root, "Dark sole", PrimitiveType.Cube, new Vector3(x, .045f, .10f), new Vector3(.30f, .07f, .49f), sole);
                BootPart(root, "Olive canvas cuff", PrimitiveType.Cylinder, new Vector3(x, .50f, -.03f), new Vector3(.27f, .035f, .29f), cuff);
                for (int j = 0; j < 3; j++) BootPart(root, "Lace", PrimitiveType.Cube, new Vector3(x, .25f + j * .055f, .103f), new Vector3(.15f, .016f, .018f), cuff);
            }
            return root;
        }
        static void BootPart(GameObject root, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(type); part.name = name;
            part.transform.SetParent(root.transform, false); part.transform.localPosition = position; part.transform.localScale = scale;
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>()); part.GetComponent<Renderer>().sharedMaterial = material;
        }

        [CliCommand("dicefree.item-icons.validate", "Validate every item sprite, unique PNG content, import settings and all four UI integrations.")]
        public static object Validate()
        {
            var items = Items(); Require(items.Length >= 8, "Expected eight or more items.");
            var ids = new HashSet<string>(); var sprites = new HashSet<Sprite>(); var hashes = new HashSet<string>();
            foreach (var item in items)
            {
                Require(ids.Add(item.stableId), "Duplicate item ID.");
                Require(item.icon != null && sprites.Add(item.icon), "Missing/shared sprite: " + item.name);
                string path = AssetDatabase.GetAssetPath(item.icon);
                Require(path.EndsWith(".png") && File.Exists(path) && new FileInfo(path).Length > 0, "PNG missing: " + path);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Require(importer != null && importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single && importer.alphaIsTransparency && importer.sRGBTexture, "Invalid sprite importer: " + path);
                Require(item.icon.rect.width >= 64 && item.icon.rect.height >= 64, "Icon too small.");
                using (var sha = System.Security.Cryptography.SHA256.Create()) Require(hashes.Add(Convert.ToBase64String(sha.ComputeHash(File.ReadAllBytes(path)))), "Identical icon images.");
                var image = new Texture2D(2, 2);
                try
                {
                    Require(image.LoadImage(File.ReadAllBytes(path)), "Unreadable PNG.");
                    var pixels = image.GetPixels(); int visible = pixels.Count(p => p.a > .5f);
                    Require(visible > 100 && visible < pixels.Length * .9f && pixels.Any(p => p.a == 0), "Blank/opaque icon: " + item.name);
                    var rect = ItemIconGUI.ImageRect(new Rect(0, 0, 58, 40), item.icon);
                    Require(rect.x >= 3 && rect.y >= 3 && rect.xMax <= 55 && rect.yMax <= 37, "GUI inset/fit regression.");
                }
                finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            foreach (string file in new[] { "UI/InventoryPanel", "UI/CharacterHudPanel", "Application/ShopPanel", "Application/DungeonRewardRoom" })
            {
                string code = File.ReadAllText("Assets/_DiceFree/Scripts/" + file + ".cs");
                Require(code.Contains("ItemIconGUI.Draw") && code.Contains("GUI.Button") && code.Contains("HudTooltip.Item"), "UI integration missing: " + file);
            }
            Require(!ItemIconGUI.Draw(default, null), "Missing icon fallback failed.");
            return new { success = true, marker = "ITEM_ICONS_VALIDATED", items = items.Length, uniqueSprites = sprites.Count, uniquePNGs = hashes.Count, uiSurfaces = 4, resolution = Size };
        }
    }
    public sealed class ItemIconGUIValidation : EditorWindow
    {
        public ItemDefinition[] items;
        public int drawn;
        public bool fallback;
        void OnGUI()
        {
            if (items == null) return;
            int count = 0;
            for (int row = 0; row < 2; row++) for (int i = 0; i < items.Length; i++)
            {
                var rect = new Rect(10 + i * 65, 30 + row * 80, 58, row == 0 ? 58 : 36);
                GUI.Box(rect, GUIContent.none);
                GUI.Button(rect, new GUIContent("", HudTooltip.Item(items[i])), GUIStyle.none);
                if (ItemIconGUI.Draw(rect, items[i])) count++;
            }
            fallback = !ItemIconGUI.Draw(new Rect(10, 200, 58, 36), null);
            GUI.Label(new Rect(10, 190, 520, 30), "192px source sprites: inventory and compact equipped slot framing");
            if (Event.current.type == EventType.Repaint) drawn = count;
        }
    }
}
