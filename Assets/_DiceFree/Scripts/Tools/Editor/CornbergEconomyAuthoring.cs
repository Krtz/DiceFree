using System;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;

namespace DiceFree.EditorTools
{
    internal static class CornbergEconomyAuthoring
    {
        internal const string Id = "item.bronze-dagger";
        internal const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        internal const string Root = "Assets/_DiceFree/Art/Weapons/BronzeDagger";
        internal const string ItemPath = "Assets/_DiceFree/Settings/Items/Bronze Dagger.asset";
        internal static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        [CliCommand("dicefree.economy.install", "Incrementally install the provisional dagger shop and hero bindings; never rebuild terrain/navigation.")]
        public static object Install()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Requires Edit Mode.");
            for (int i=0;i<SceneManager.sceneCount;i++) Require(!SceneManager.GetSceneAt(i).isDirty, "Save scene changes first.");
            var original = EditorSceneManager.GetSceneManagerSetup();
            var scene = SceneManager.GetSceneByPath(ScenePath);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            var merchant = scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true))
                .Single(t=>t.name=="NPCs/NPC_Merchant - 3D").gameObject;
            Require(merchant.GetComponents<DiceFree.World.InteractionTarget>().All(t=>t is ItemShop), "Merchant already has another interaction; preserve it.");
            Folder(Root); Folder(Root+"/Materials");
            var dagger = AssetDatabase.LoadAssetAtPath<ItemDefinition>(ItemPath);
            if (dagger == null) { dagger=ScriptableObject.CreateInstance<ItemDefinition>(); AssetDatabase.CreateAsset(dagger,ItemPath); }
            dagger.stableId=Id; dagger.displayName="BRONZE DAGGER"; dagger.slot=EquipmentSlot.MainHand;
            dagger.sourceId="shop.cornberg.merchant"; dagger.stats=new EquipmentStats {basicAttackFlatBonus=1};
            dagger.flavor="A short bronze blade with a dark wrapped grip.";
            EditorUtility.SetDirty(dagger);
            var prefab = Presentation();
            var shop=merchant.GetComponent<ItemShop>() ?? merchant.AddComponent<ItemShop>();
            shop.ConfigureName("Cornberg Merchant");
            // Preserve serialized balance on later installs.
            if (!shop.Offers.Any(o=>o?.item==dagger)) shop.Configure(shop.Offers.Concat(new[]{new ItemShop.Offer {item=dagger,goldPrice=15,provisionalPrice=true}}).ToArray());
            if (merchant.GetComponent<Collider>()==null) { var c=merchant.AddComponent<CapsuleCollider>(); c.radius=.65f;c.height=2.4f;c.center=Vector3.up*1.2f; }
            merchant.layer=11;
            EditorUtility.SetDirty(shop);
            int catalogs=0, heroes=0;
            // Dungeon maps carry the Cornberg hero/session additively. Update any authored inventory
            // in the relevant maps too, without creating replacement actors or granting ownership.
            foreach(string name in new[]{"Cornberg","SlimeDungeon","DungeonRewardRoom","NoviceMountainTrial"})
            {
                string path="Assets/_DiceFree/Scenes/"+name+".unity";
                var s=SceneManager.GetSceneByPath(path); bool opened=!s.isLoaded;
                if(opened) s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
                bool changed=path==ScenePath;
                foreach(var inv in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<CarriedInventory>(true)))
                {
                    catalogs++;
                    if (!inv.Definitions.Contains(dagger)) { inv.Configure(inv.Definitions.Concat(new[]{dagger}).ToArray()); EditorUtility.SetDirty(inv);changed=true; }
                    var driver=inv.GetComponent<NovicePresentationDriver>();
                    if(driver==null) continue;
                    var presentation=inv.GetComponent<EquipmentPresentation>();
                    Require(presentation!=null,"Missing equipment presentation.");
                    if(!presentation.Bindings.Any(b=>b.definition==dagger))
                    {
                        var socket=driver.VisualRoot.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="RightHandWeapon");
                        var visual=(GameObject)PrefabUtility.InstantiatePrefab(prefab,s);
                        visual.name="BronzeDagger";visual.transform.SetParent(socket,false);visual.SetActive(false);
                        // Preserve all existing bindings, including any starter sword; suppress its visual only while dagger is equipped.
                        var baseline=socket.Cast<Transform>().Where(t=>t!=visual.transform).Select(t=>t.gameObject).ToArray();
                        presentation.Configure(presentation.Bindings.Concat(new[]{new EquipmentPresentation.Binding {slot=EquipmentSlot.MainHand,definition=dagger,visuals=new[]{visual},baselineVisuals=baseline}}).ToArray());
                        EditorUtility.SetDirty(presentation);changed=true;
                    }
                    if(inv.GetComponent<ShopPanel>()==null) {inv.gameObject.AddComponent<ShopPanel>();changed=true;}
                    heroes++;
                }
                if(changed) {EditorSceneManager.MarkSceneDirty(s);Require(EditorSceneManager.SaveScene(s),"Scene save failed.");}
                if(opened && path!=ScenePath) EditorSceneManager.CloseScene(s,true);
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.RestoreSceneManagerSetup(original);
            return new {success=true,marker="CORNBERG_ECONOMY_INSTALLED",catalogs,heroes,price="serialized; default 15 gold, provisional"};
        }
        static void Folder(string path) { if(AssetDatabase.IsValidFolder(path)) return; Folder(System.IO.Path.GetDirectoryName(path).Replace('\\','/'));AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(path).Replace('\\','/'),System.IO.Path.GetFileName(path)); }
        static Material Material(string name,Color color,float metal)
        {
            string path=Root+"/Materials/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat!=null)return mat;
            mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.SetColor("_BaseColor",color);mat.SetFloat("_Metallic",metal);mat.SetFloat("_Smoothness",.27f);AssetDatabase.CreateAsset(mat,path);return mat;
        }
        static GameObject Presentation()
        {
            string path=Root+"/BronzeDagger.prefab";
            var existing=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(existing!=null)return existing;
            var bronze=Material("Warm bronze",new Color(.62f,.32f,.10f),.65f);
            var dark=Material("Dark leather grip",new Color(.065f,.042f,.025f),0);
            var root=new GameObject("BronzeDagger");
            // Closed faceted broad blade, grip pivot, local +Z. Deliberately short chunky silhouette.
            var mesh=new Mesh {name="Handcrafted bronze dagger blade"};
            mesh.vertices=new[]{new Vector3(-.095f,0,.10f),new Vector3(.095f,0,.10f),new Vector3(-.10f,0,.34f),new Vector3(.10f,0,.34f),new Vector3(0,0,.58f),new Vector3(0,.038f,.25f),new Vector3(0,-.038f,.25f)};
            mesh.triangles=new[]{0,5,1,0,2,5,2,4,5,4,3,5,3,1,5,0,1,6,0,6,2,2,6,4,4,6,3,3,6,1};mesh.RecalculateNormals();mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,Root+"/Blade.asset");
            var blade=new GameObject("Bronze blade");blade.transform.SetParent(root.transform,false);blade.AddComponent<MeshFilter>().sharedMesh=mesh;blade.AddComponent<MeshRenderer>().sharedMaterial=bronze;
            Part(root,"Dark grip",new Vector3(0,0,-.035f),new Vector3(.075f,.075f,.23f),dark);
            Part(root,"Short bronze guard",new Vector3(0,0,.095f),new Vector3(.25f,.07f,.055f),bronze);
            Part(root,"Bronze pommel",new Vector3(0,0,-.17f),new Vector3(.09f,.09f,.045f),bronze);
            var result=PrefabUtility.SaveAsPrefabAsset(root,path);UnityEngine.Object.DestroyImmediate(root);return result;
        }
        static void Part(GameObject parent,string name,Vector3 position,Vector3 scale,Material material)
        {var p=GameObject.CreatePrimitive(PrimitiveType.Cube);p.name=name;p.transform.SetParent(parent.transform,false);p.transform.localPosition=position;p.transform.localScale=scale;UnityEngine.Object.DestroyImmediate(p.GetComponent<Collider>());p.GetComponent<Renderer>().sharedMaterial=material;}
        [CliCommand("dicefree.economy.validate", "Validate dagger data, merchant and equipment/catalog wiring.")]
        public static object Validate()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,"Requires Edit Mode.");
            var dagger=AssetDatabase.LoadAssetAtPath<ItemDefinition>(ItemPath);
            Require(dagger!=null && dagger.stableId==Id && dagger.slot==EquipmentSlot.MainHand,"Dagger definition missing.");
            Require(JsonUtility.ToJson(dagger.stats)==JsonUtility.ToJson(new EquipmentStats {basicAttackFlatBonus=1}),"Dagger must grant exactly +1 basic attack damage.");
            var s=SceneManager.GetSceneByPath(ScenePath);if(!s.isLoaded)s=EditorSceneManager.OpenScene(ScenePath);
            var shop=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<ItemShop>(true)).Single();
            Require(shop.name=="NPCs/NPC_Merchant - 3D" && Vector2.Distance(new Vector2(shop.transform.position.x,shop.transform.position.z),new Vector2(17,-18))<2,"Wrong merchant/proximity.");
            Require(shop.gameObject.layer==11 && shop.GetComponent<Collider>()!=null,"Merchant not right-clickable.");
            var inv=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<CarriedInventory>(true)).Single();
            Require(inv.Definitions.Count(d=>d==dagger)==1,"Catalog entry missing/duplicated.");
            var b=inv.GetComponent<EquipmentPresentation>().Bindings.Single(b=>b.definition==dagger);
            Require(b.slot==EquipmentSlot.MainHand && b.visuals.Length==1 && !b.visuals[0].activeSelf && b.visuals[0].transform.parent.name=="RightHandWeapon","Invalid dagger binding.");
            Require(b.visuals[0].GetComponentsInChildren<Renderer>(true).Length==4,"Dagger 3D parts missing.");
            Require(b.visuals[0].GetComponentsInChildren<Collider>(true).Length==0,"Equipment art must not add gameplay colliders.");
            return new {success=true,marker="CORNBERG_ECONOMY_VALIDATED",price=shop.Offers.Single(o=>o.item==dagger).goldPrice,merchant=shop.transform.position.ToString(),renderers=4};
        }
    }
}
