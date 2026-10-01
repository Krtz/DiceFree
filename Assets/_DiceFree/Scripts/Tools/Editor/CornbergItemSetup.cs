using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DiceFree.EditorTools
{
    public static class CornbergItemSetup
    {
        public const string Folder = "Assets/_DiceFree/Settings/Items";
        public static void Install()
        {
            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "Items");
            var gloves = Create("Cornberg Work Gloves", "item.cornberg.work-gloves", EquipmentSlot.Hands,
                new EquipmentStats { attackSpeedPercent = 5 }, "quest.cornberg.q4");
            var shoes = Create("Forest Travel Shoes", "item.cornberg.forest-shoes", EquipmentSlot.Feet,
                new EquipmentStats { physicalDefense = 1, magicalDefense = 1, movementSpeedPercent = 1 }, "enemy.cornberg.deep-forest-elite");
            var player = Object.FindAnyObjectByType<TraversalInput>().gameObject;
            var inventory = player.GetComponent<CarriedInventory>() ?? player.AddComponent<CarriedInventory>();
            inventory.Configure(new[] { gloves, shoes });
            if (!player.TryGetComponent<Equipment>(out _)) player.AddComponent<Equipment>();
            if (!player.TryGetComponent<GoldWallet>(out _)) player.AddComponent<GoldWallet>();
            if (!player.TryGetComponent<InventoryPanel>(out _)) player.AddComponent<InventoryPanel>();
            EditorUtility.SetDirty(inventory); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("CORNBERG_ITEM_SETUP_OK");
        }
        private static ItemDefinition Create(string name, string id, EquipmentSlot slot, EquipmentStats stats, string source)
        {
            string path = Folder + "/" + name + ".asset";
            var value = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (value != null) return value;
            value = ScriptableObject.CreateInstance<ItemDefinition>(); value.stableId = id; value.displayName = name;
            value.slot = slot; value.stats = stats; value.sourceId = source;
            AssetDatabase.CreateAsset(value, path); return value;
        }
    }
}
