using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Ironpine;
using DiceFree.Progression;
using DiceFree.UI;
using DiceFree.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
using Object=UnityEngine.Object;
namespace DiceFree.EditorTools
{
    public static class MenuAndWolfAuthoring
    {
        public const string ItemPath="Assets/_DiceFree/Settings/Items/Wolf-Fang Greatsword.asset";
        const string PreviewFolder="Assets/_DiceFree/Art/MenuPreviews";
        static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;Folder(Path.GetDirectoryName(path).Replace('\\','/'));AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\','/'),Path.GetFileName(path));}
        [CliCommand("dicefree.menu-wolf.author","Author tiered equipped menu previews and the Alpha Wolf's 10% two-handed greatsword drop.")]
        public static object Run(){
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            var setup=EditorSceneManager.GetSceneManagerSetup();int inventories=0,alphas=0;
            try{
                Folder(PreviewFolder);Folder("Assets/_DiceFree/Resources");
                var item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(ItemPath);
                if(item==null){item=ScriptableObject.CreateInstance<ItemDefinition>();AssetDatabase.CreateAsset(item,ItemPath);}
                item.stableId=WolfGreatswordVisual.ItemId;item.displayName="Wolf-Fang Greatsword";item.slot=EquipmentSlot.MainHand;
                item.handsRequired=2;item.itemLevel=1;item.rarityId="rarity.uncommon";item.sourceId="ironpine.alpha-wolf";
                item.allowedClassIds=Array.Empty<string>();item.modelTestingItem=false;item.bound=false;item.canSell=true;
                item.flavor="Plain steel, a dark leather grip and a pair of small wolf fangs.";
                item.stats=new EquipmentStats{basicAttackFlatBonus=10,attributes=new AttributeValues{strength=5}};
                item.icon=Icon();EditorUtility.SetDirty(item);AssetDatabase.SaveAssets();
                foreach(var path in EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path)){
                    var scene=EditorSceneManager.OpenScene(path);bool dirty=false;
                    item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(ItemPath);
                    if(item==null)throw new InvalidOperationException("Missing authored wolf greatsword");
                    foreach(var inv in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<CarriedInventory>(true))){
                        var so=new SerializedObject(inv);var values=inv.Definitions.Where(d=>d!=null).Append(item).Distinct().ToArray();
                        var property=so.FindProperty("definitions");property.arraySize=values.Length;
                        for(int i=0;i<values.Length;i++)property.GetArrayElementAtIndex(i).objectReferenceValue=values[i];
                        so.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.RecordPrefabInstancePropertyModifications(inv);inventories++;dirty=true;
                        if(inv.GetComponent<ClassPresentationSwitcher>()!=null&&inv.GetComponent<WolfGreatswordVisual>()==null)inv.gameObject.AddComponent<WolfGreatswordVisual>();
                    }
                    foreach(var alpha in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<IronpineWildlife>(true)).Where(a=>a.Kind==WildlifeKind.AlphaWolf)){
                        var drop=alpha.GetComponent<FixedWorldDrop>()??alpha.gameObject.AddComponent<FixedWorldDrop>();drop.item=item;drop.chance=.1f;EditorUtility.SetDirty(drop);alphas++;dirty=true;
                    }
                    if(dirty){EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Scene save failed "+path);}
                }
                if(alphas!=1)throw new InvalidOperationException("Expected one normal-world Alpha Wolf, found "+alphas);
                var cornberg=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
                var hero=cornberg.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<ClassPresentationSwitcher>(true)).First(g=>g.GetComponent<CarriedInventory>()!=null);
                var catalog=AssetDatabase.LoadAssetAtPath<MenuPresentationCatalog>("Assets/_DiceFree/Resources/MenuPresentationCatalog.asset");
                if(catalog==null){catalog=ScriptableObject.CreateInstance<MenuPresentationCatalog>();AssetDatabase.CreateAsset(catalog,"Assets/_DiceFree/Resources/MenuPresentationCatalog.asset");}
                var advancement=hero.GetComponent<AdvancementController>();
                var definitions=advancement.Classes;
                var edges=AssetDatabase.FindAssets("t:AdvancementDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<AdvancementDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
                int Tier(ActorDefinition def,HashSet<string> seen){
                    if(!seen.Add(def.stableId))throw new InvalidOperationException("Cyclic advancement graph");
                    var parent=edges.FirstOrDefault(e=>e.targetClass==def)?.sourceClass;
                    return parent==null?0:1+Tier(parent,seen);
                }
                var forms=new List<MenuPresentationCatalog.Form>();
                foreach(var def in definitions){
                    var root=new GameObject("Menu visual data - "+def.stableId);root.SetActive(false);
                    try{
                        root.AddComponent<ActorStats>().Configure(def);root.AddComponent<CarriedInventory>().Configure(hero.GetComponent<CarriedInventory>().Definitions.ToArray());
                        root.AddComponent<Equipment>();var presentation=root.AddComponent<EquipmentPresentation>();
                        var mapping=new Dictionary<GameObject,GameObject>();
                        Transform chosen=null;
                        foreach(var klass in definitions){
                            var source=hero.Resolve(klass.stableId);if(source==null)continue;
                            var copy=Object.Instantiate(source.gameObject,root.transform);copy.name=klass==def?"Class visual":source.name;
                            var originals=source.GetComponentsInChildren<Transform>(true);var clones=copy.GetComponentsInChildren<Transform>(true);
                            for(int i=0;i<originals.Length;i++)mapping[originals[i].gameObject]=clones[i].gameObject;
                            copy.SetActive(klass==def);if(klass==def)chosen=copy.transform;
                        }
                        if(chosen==null)throw new InvalidOperationException("No visual for "+def.stableId);
                        GameObject[] Map(GameObject[] refs)=>refs.Where(g=>g!=null&&mapping.ContainsKey(g)).Select(g=>mapping[g]).ToArray();
                        presentation.Configure(hero.GetComponent<EquipmentPresentation>().Bindings.Select(b=>new EquipmentPresentation.Binding{definition=b.definition,slot=b.slot,visuals=Map(b.visuals),baselineVisuals=Map(b.baselineVisuals)}).ToArray());
                        foreach(var col in root.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(col);
                        string path=PreviewFolder+"/"+def.stableId+".prefab";
                        PrefabUtility.SaveAsPrefabAsset(root,path);
                        forms.Add(new MenuPresentationCatalog.Form{classId=def.stableId,tier=Tier(def,new HashSet<string>()),preview=AssetDatabase.LoadAssetAtPath<GameObject>(path)});
                    }finally{Object.DestroyImmediate(root);}
                }
                catalog.forms=forms.ToArray();catalog.items=hero.GetComponent<CarriedInventory>().Definitions.ToArray();EditorUtility.SetDirty(catalog);
                var menuScene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/StartMenu.unity");
                var menu=menuScene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<StartMenuController>(true)).Single();
                var serialized=new SerializedObject(menu);serialized.FindProperty("menuPresentation").objectReferenceValue=catalog;serialized.ApplyModifiedPropertiesWithoutUndo();
                foreach(var extra in menu.GetComponents<OptionsPanel>())Object.DestroyImmediate(extra);
                EditorSceneManager.MarkSceneDirty(menuScene);EditorSceneManager.SaveScene(menuScene);AssetDatabase.SaveAssets();
                return new{marker="DICEFREE_MENU_WOLF_AUTHORED",forms=forms.Count,alphas,inventories,chance=.1f,damage=10,strength=5,hands=2};
            }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
        static Sprite Icon(){
            const string folder="Assets/_DiceFree/Art/Equipment/WolfFang";Folder(folder);string path=folder+"/WolfFang.png";
            var t=new Texture2D(96,96,TextureFormat.RGBA32,false);var p=new Color[96*96];
            for(int y=0;y<96;y++)for(int x=0;x<96;x++){
                float dx=x-48;var c=new Color(.07f,.10f,.13f);
                if(Mathf.Abs(dx)<6&&y>32&&y<86)c=new Color(.66f,.73f,.77f);
                if(Mathf.Abs(dx)<3&&y>12&&y<34)c=new Color(.29f,.20f,.12f);
                if(Mathf.Abs(dx)<21&&y>=31&&y<=36)c=new Color(.48f,.55f,.59f);
                if(Mathf.Abs(Mathf.Abs(dx)-16)<3&&y>25&&y<33)c=new Color(.86f,.82f,.67f);
                if(Mathf.Abs(dx)<6&&y>=7&&y<=14)c=new Color(.62f,.65f,.66f);
                p[y*96+x]=c;
            }
            t.SetPixels(p);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.mipmapEnabled=false;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
