using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Dungeons;
using DiceFree.Items;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

public static class CrucibleLootAuthoring
{
    public const string Data="Assets/_DiceFree/Settings/CrimsonCrucible";
    public const string Rewards="Assets/_DiceFree/Resources/DiceFree/DungeonRewards/CrimsonCrucible";
    const string Armor="Assets/Polytope Studio/Lowpoly_Characters/Sources/Modular_Armors/Meshes/PT_Lowpoly_Armors_Male_Moduar_Free.fbx";
    const string Icons="Assets/_DiceFree/Art/ItemIcons/CrimsonCrucible";
    static readonly List<Object> temporary=new();
    public static void Batch()=>Author();
    public static DungeonRewardPool Author()
    {
        foreach(var folder in new[]{Data,Rewards,Icons})Directory.CreateDirectory(folder);AssetDatabase.Refresh();
        var result=new List<ItemDefinition>();var slots=new[]{EquipmentSlot.Head,EquipmentSlot.Shoulders,EquipmentSlot.Legs,EquipmentSlot.Back};
        var prefixes=new[]{"Bristleforged","Moonfang","Stormscale","Bloomkeeper"};
        var names=new[]{new[]{"Warhelm","Pauldrons","Cuisses","Warcloak"},new[]{"Cowl","Mantle","Legwraps","Hunting Cape"},new[]{"Hood","Epaulets","Leggings","Spellcloak"},new[]{"Veil","Shawl","Legwraps","Prayer Cape"}};
        try
        {
            for(int role=0;role<4;role++)for(int slot=0;slot<4;slot++)
            {
                string name=prefixes[role]+" "+names[role][slot];var item=Item(name,prefixes[role].ToLowerInvariant()+"-"+slots[slot].ToString().ToLowerInvariant(),slots[slot],Stats(role,slot));
                item.flavor=role switch{0=>"Iron and bristlehide, earned in the third cell of the Crimson Crucible.",1=>"Moonlit leather and a hunter's patience, carried beyond the wolf's cell.",2=>"Lightning-blue scale and deep cloth, marked by the stormcaller trial.",_=>"Soft cloth and living green thread, gathered after the thornwarden's fall."};
                Render(item,role,slot);result.Add(item);
            }
            var rare=Item("Crucible Victor's Shroud","victors-shroud",EquipmentSlot.Back,new EquipmentStats{attributes=new AttributeValues(12){vitality=20},physicalDefense=18,magicalDefense=18,attackSpeedPercent=4,movementSpeedPercent=3,regeneration=3});
            rare.flavor="Six conquered cells are embroidered around a single crimson gem.";Render(rare,4,3);
            var pool=Asset<DungeonRewardPool>(Data+"/Crucible treasury.asset");pool.stableId="loot.crimson-crucible";pool.rare=rare;pool.items=result.ToArray();EditorUtility.SetDirty(pool);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Logs");File.WriteAllText("Logs/CrucibleLoot.json","{\"itemLevel\":55,\"ordinary\":16,\"rare\":1,\"roles\":[\"STR\",\"AGI\",\"INT\",\"SPI\"],\"slots\":[\"Head\",\"Shoulders\",\"Legs\",\"Back\"]}");return pool;
        }
        finally{foreach(var item in temporary)if(item!=null)Object.DestroyImmediate(item);temporary.Clear();}
    }
    static EquipmentStats Stats(int role,int slot)
    {
        float primary=slot==2?24:slot==3?16:18,vit=slot==2?18:slot==3?12:14;
        var value=new EquipmentStats();
        value.attributes=role switch{0=>new(){strength=primary,vitality=vit+6,spirit=4},1=>new(){agility=primary,strength=6,vitality=vit},2=>new(){intelligence=primary,spirit=8,vitality=vit},_=>new(){spirit=primary,intelligence=8,vitality=vit}};
        value.physicalDefense=role==0?(slot==2?30:20):role==1?(slot==2?22:14):(slot==2?14:9);
        value.magicalDefense=role>=2?(slot==2?26:20):(slot==2?18:12);
        if(role==0)value.regeneration=2;if(role==1){value.attackSpeedPercent=slot==2?4:2;value.basicAttackFlatBonus=slot==2?4:2;}if(role==3)value.regeneration=slot==2?4:3;
        if(slot==3)value.movementSpeedPercent=2;return value;
    }
    static ItemDefinition Item(string name,string id,EquipmentSlot slot,EquipmentStats stats)
    {
        var item=Asset<ItemDefinition>(Rewards+"/"+name+".asset");item.name=item.displayName=name;item.stableId="item.crimson-crucible."+id;item.itemLevel=55;item.slot=slot;item.rarityId="rarity.rare";
        item.sourceId=CrimsonCruciblePortal.DungeonId;item.stats=stats;item.allowedClassIds=Array.Empty<string>();EditorUtility.SetDirty(item);return item;
    }
    public static T Asset<T>(string path)where T:ScriptableObject
    {var result=AssetDatabase.LoadAssetAtPath<T>(path);if(result==null){result=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(result,path);}return result;}
    static Material Tint(Color color,bool gem=false)
    {var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color};m.SetFloat("_Cull",0);m.SetFloat("_Metallic",gem ? .3f : .1f);m.SetFloat("_Smoothness",.36f);m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*.7f);temporary.Add(m);return m;}
    static Mesh MeshFor(int role,int slot)
    {
        string name=slot switch{0=>role==0?"PT_Male_Armor_05_C_helmet":"PT_Male_Armor_01_A_helmet",1=>"PT_Male_Armor_05_C_body",2=>role==0?"PT_Male_Armor_05_C_legs":role==1?"PT_Male_Armor_01_A_legs":"PT_Male_Armor_cloth_00_legs",_=>role==1?"PT_Male_Armor_01_A_cape":"PT_Male_Armor_05_C_cape"};
        var source=AssetDatabase.LoadAllAssetsAtPath(Armor).OfType<Mesh>().Single(m=>m.name==name);var mesh=Object.Instantiate(source);temporary.Add(mesh);
        if(slot==1)
        {
            var v=mesh.vertices;for(int sub=0;sub<mesh.subMeshCount;sub++){var input=source.GetTriangles(sub);var triangles=new List<int>();for(int i=0;i<input.Length;i+=3)if(input.Skip(i).Take(3).All(j=>v[j].y>1.29f&&Mathf.Abs(v[j].x)<.50f))triangles.AddRange(new[]{input[i],input[i+1],input[i+2]});mesh.SetTriangles(triangles,sub);}
            var used=mesh.triangles.Select(i=>v[i]).ToArray();if(used.Length==0)throw new Exception("Empty shoulder crop");var bounds=new Bounds(used[0],Vector3.zero);foreach(var vertex in used)bounds.Encapsulate(vertex);mesh.bounds=bounds;
        }
        return mesh;
    }
    static void Render(ItemDefinition item,int role,int slot)
    {
        var color=role switch{0=>new Color(.40f,.24f,.17f),1=>new Color(.14f,.30f,.25f),2=>new Color(.24f,.25f,.44f),3=>new Color(.66f,.64f,.49f),_=>new Color(.36f,.09f,.13f)};
        var root=new GameObject(item.displayName);var model=new GameObject("Library armour");model.transform.SetParent(root.transform,false);var mesh=MeshFor(role,slot);model.AddComponent<MeshFilter>().sharedMesh=mesh;var r=model.AddComponent<MeshRenderer>();r.sharedMaterials=Enumerable.Repeat(Tint(color),mesh.subMeshCount).ToArray();
        // A small trial gem, rather than a large floating ornament, keeps the slot readable.
        var gem=GameObject.CreatePrimitive(PrimitiveType.Cube);Object.DestroyImmediate(gem.GetComponent<Collider>());gem.transform.SetParent(root.transform,false);var b=mesh.bounds;
        gem.transform.localPosition=b.center+new Vector3(0,b.extents.y*.45f,b.extents.z+.018f);gem.transform.localScale=Vector3.one*Mathf.Min(.07f,b.size.y*.10f);gem.transform.localRotation=Quaternion.Euler(0,0,45);gem.GetComponent<Renderer>().sharedMaterial=Tint(role==3?new Color(.24f,.76f,.58f):role==2?new Color(.38f,.68f,1):new Color(.82f,.20f,.17f),true);
        root.transform.rotation=Quaternion.Euler(0,-17,0);var preview=new PreviewRenderUtility();var previous=RenderTexture.active;Texture2D image=null;bool async=ShaderUtil.allowAsyncCompilation;
        try
        {
            ShaderUtil.allowAsyncCompilation=false;preview.AddSingleGO(root);var bounds=r.bounds;bounds.Encapsulate(gem.GetComponent<Renderer>().bounds);
            var camera=preview.camera;camera.orthographic=true;camera.orthographicSize=Mathf.Max(bounds.extents.y,bounds.extents.x)*1.2f;camera.nearClipPlane=.01f;camera.farClipPlane=30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.045f,.05f);
            camera.transform.SetPositionAndRotation(bounds.center+(slot==3?Vector3.back:Vector3.forward)*8,Quaternion.Euler(0,slot==3?0:180,0));
            preview.lights[0].intensity=1.4f;preview.lights[0].transform.rotation=Quaternion.Euler(30,slot==3?25:155,0);preview.lights[1].intensity=.9f;preview.lights[1].transform.rotation=Quaternion.Euler(-15,slot==3?-35:215,0);preview.ambientColor=new(.38f,.4f,.42f);
            preview.BeginPreview(new Rect(0,0,512,512),GUIStyle.none);preview.Render(true);RenderTexture.active=(RenderTexture)preview.EndPreview();image=new Texture2D(512,512,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,512,512),0,0);image.Apply();string path=Icons+"/"+item.stableId+".png";File.WriteAllBytes(path,image.EncodeToPNG());AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.SaveAndReimport();item.icon=AssetDatabase.LoadAssetAtPath<Sprite>(path);EditorUtility.SetDirty(item);
        }
        finally{ShaderUtil.allowAsyncCompilation=async;RenderTexture.active=previous;preview.Cleanup();if(root!=null)Object.DestroyImmediate(root);if(image!=null)Object.DestroyImmediate(image);}
    }
}
