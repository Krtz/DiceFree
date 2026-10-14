using System;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.UI;
using DiceFree.Visuals;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    public static class PlainEquipmentAuthoring
    {
        public const string Folder="Assets/_DiceFree/Settings/Items/Plain";
        public const string Icons="Assets/_DiceFree/Art/Equipment/PlainIcons";
        public static string Id(EquipmentSlot slot)=>"item.plain."+slot.ToString().ToLowerInvariant();
        public static string Name(EquipmentSlot slot)=>slot switch{
            EquipmentSlot.Head=>"Plain Cap",EquipmentSlot.Shoulders=>"Plain Shoulder Pads",EquipmentSlot.Chest=>"Plain Breastplate",
            EquipmentSlot.Hands=>"Plain Cuffs",EquipmentSlot.Legs=>"Plain Leg Pads",EquipmentSlot.Feet=>"Plain Boots",
            EquipmentSlot.MainHand=>"Plain Sword",EquipmentSlot.OffHand=>"Plain Buckler",EquipmentSlot.Ring=>"Plain Ring",
            EquipmentSlot.Amulet=>"Plain Pendant",_=>"Plain Short Cape"};
        public static EquipmentStats Stats(EquipmentSlot slot)=>slot switch{
            EquipmentSlot.Head=>new(){attributes=new(){intelligence=1}},
            EquipmentSlot.Shoulders=>new(){attributes=new(){strength=1}},
            EquipmentSlot.Chest or EquipmentSlot.OffHand=>new(){physicalDefense=1},
            EquipmentSlot.Hands=>new(){attributes=new(){agility=1}},
            EquipmentSlot.Legs or EquipmentSlot.Back=>new(){attributes=new(){vitality=1}},
            EquipmentSlot.Feet=>new(){movementSpeedPercent=1},
            EquipmentSlot.MainHand=>new(){basicAttackFlatBonus=1},
            EquipmentSlot.Ring=>new(){attributes=new(){spirit=1}},
            _=>new(){magicalDefense=1}};
        [CliCommand("dicefree.plain-gear.author","Author eleven plain all-class fitting items, 100-gold Cornberg stock, all scene catalogs and extended skill hotkeys.")]
        public static object Run()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            if(setup.Any(s=>SceneManager.GetSceneByPath(s.path).isDirty))throw new InvalidOperationException("Save scene changes first.");
            EnsureFolder(Folder);EnsureFolder(Icons);
            var items=Enum.GetValues(typeof(EquipmentSlot)).Cast<EquipmentSlot>().Select(slot=>{
                string path=Folder+"/"+slot+".asset";var d=AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
                if(d==null){d=ScriptableObject.CreateInstance<ItemDefinition>();AssetDatabase.CreateAsset(d,path);}
                d.stableId=Id(slot);d.displayName=Name(slot);d.slot=slot;d.itemLevel=1;d.rarityId="rarity.common";
                d.sourceId="shop.cornberg.merchant";d.bound=d.questItem=false;d.canSell=true;d.allowedClassIds=Array.Empty<string>();
                d.modelTestingItem=true;d.flavor="A plain fitting piece. Usable by every class.";d.stats=Stats(slot);
                d.icon=Icon(slot);EditorUtility.SetDirty(d);return d;
            }).ToArray();
            ExtendControls();
            int heroes=0,shops=0;
            try
            {
                foreach(var path in EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path))
                {
                    var scene=EditorSceneManager.OpenScene(path);bool changed=false;
                    foreach(var inv in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<CarriedInventory>(true)))
                    {
                        var catalog=inv.Definitions.Concat(items).Where(d=>d!=null).Distinct().ToArray();
                        var serialized=new SerializedObject(inv);var definitions=serialized.FindProperty("definitions");
                        definitions.arraySize=catalog.Length;
                        for(int i=0;i<catalog.Length;i++)definitions.GetArrayElementAtIndex(i).objectReferenceValue=catalog[i];
                        serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(inv);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(inv);changed=true;
                        if(inv.GetComponent<ClassPresentationSwitcher>()==null||inv.GetComponent<EquipmentPresentation>()==null)continue;
                        if(inv.GetComponent<PlainEquipmentVisuals>()==null)inv.gameObject.AddComponent<PlainEquipmentVisuals>();
                        // Legacy serialized bars remain wiring adapters, never competing input/render owners.
                        foreach(var legacy in inv.GetComponents<MonoBehaviour>().Where(c=>c is NoviceSkillBar||c is PhysicalSkillBar||c is MagicalSkillBar))
                            {legacy.enabled=false;EditorUtility.SetDirty(legacy);}
                        heroes++;
                    }
                    if(path=="Assets/_DiceFree/Scenes/Cornberg.unity")
                    {
                        var shop=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<ItemShop>(true)).Single(s=>s.name=="NPCs/NPC_Merchant - 3D");
                        shop.Configure(shop.Offers.Where(o=>o?.item!=null&&!items.Contains(o.item))
                            .Concat(items.Select(d=>new ItemShop.Offer{item=d,goldPrice=100,provisionalPrice=false})).ToArray());
                        EditorUtility.SetDirty(shop);changed=true;shops++;
                    }
                    if(changed){EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Scene save failed: "+path);}
                }
                AssetDatabase.SaveAssets();
                // Reopen saved scenes: editor-time assignment alone is not evidence of a persisted catalog.
                foreach(var path in EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray())
                {
                    var scene=EditorSceneManager.OpenScene(path);
                    foreach(var inv in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<CarriedInventory>(true)))
                        if(items.Any(item=>!inv.Definitions.Contains(item)))
                            throw new InvalidOperationException("Saved plain-item catalog missing in "+path+" on "+inv.name);
                }
            }
            finally {EditorSceneManager.RestoreSceneManagerSetup(setup);}
            return new{success=true,items=items.Length,price=100,heroes,shops,marker="DICEFREE_PLAIN_GEAR_AUTHORED"};
        }
        private static void EnsureFolder(string path)
        {
            if(AssetDatabase.IsValidFolder(path))return;
            string parent=Path.GetDirectoryName(path).Replace('\\','/');EnsureFolder(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
        }
        private static Sprite Icon(EquipmentSlot slot)
        {
            string path=Icons+"/"+slot+".png";var texture=new Texture2D(48,48,TextureFormat.RGBA32,false);
            var pixels=new Color[48*48];Color tint=Color.HSVToRGB((int)slot/11f,.35f,.75f);
            for(int y=0;y<48;y++)for(int x=0;x<48;x++){
                bool border=x<2||x>45||y<2||y>45;bool shape=Mathf.Abs(x-24)<(slot==EquipmentSlot.MainHand?3:11)&&y>9&&y<39;
                if(slot==EquipmentSlot.Ring||slot==EquipmentSlot.Amulet)shape=Vector2.Distance(new Vector2(x,y),new Vector2(24,24))<13&&Vector2.Distance(new Vector2(x,y),new Vector2(24,24))>7;
                pixels[y*48+x]=border?new Color(.45f,.38f,.20f):shape?tint:new Color(.10f,.12f,.15f);
            }
            texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static void ExtendControls()
        {
            const string path="Assets/_DiceFree/Resources/DiceFreeControls.inputactions";
            var clone=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<InputActionAsset>(path));
            try{
                var map=clone.FindActionMap("Gameplay",true);bool changed=false;
                for(int slot=5;slot<=9;slot++){
                    string name=slot<9?"Class skill "+slot:"Class ultimate";
                    if(map.FindAction(name,false)!=null)continue;
                    var action=map.AddAction(name,InputActionType.Button);action.expectedControlType="Button";action.AddBinding(slot<9?"<Keyboard>/"+slot:"<Keyboard>/t");changed=true;
                }
                if(changed){File.WriteAllText(path,clone.ToJson());AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);}
            }finally{UnityEngine.Object.DestroyImmediate(clone);}
        }
    }
}
