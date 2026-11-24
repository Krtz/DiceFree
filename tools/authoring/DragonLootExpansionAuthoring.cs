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

public static class DragonLootExpansionAuthoring
{
    const string Items="Assets/_DiceFree/Resources/DiceFree/DungeonRewards/AncientRoadDragon";
    const string Icons="Assets/_DiceFree/Art/ItemIcons/DragonRewards";
    const string Armor="Assets/Polytope Studio/Lowpoly_Characters/Sources/Modular_Armors/Meshes/PT_Lowpoly_Armors_Male_Moduar_Free.fbx";
    const string Pool="Assets/_DiceFree/Settings/AncientRoadDragon/Encounter/Ancient Road Dragon treasure.asset";
    static readonly List<Object> temporary=new();

    public static void Batch()
    {
        Directory.CreateDirectory(Icons);AssetDatabase.Refresh();
        var items=new[]
        {
            Item("wyrmfang-cuirass","Wyrmfang Cuirass",EquipmentSlot.Chest,new EquipmentStats {
                attributes=new AttributeValues{agility=24,strength=8,vitality=18},physicalDefense=24,magicalDefense=16,attackSpeedPercent=5,basicAttackFlatBonus=4},
                "Light overlapping jade scales let the wearer strike between a dragon's breaths."),
            Item("roostguard-bulwark","Roostguard Bulwark",EquipmentSlot.Chest,new EquipmentStats {
                attributes=new AttributeValues{strength=24,vitality=28,spirit=4},physicalDefense=36,magicalDefense=20,regeneration=4},
                "A breastplate forged around the oldest, thickest scales of the roost."),
            Item("embermender-vestments","Embermender Vestments",EquipmentSlot.Chest,new EquipmentStats {
                attributes=new AttributeValues{spirit=24,intelligence=10,vitality=18},physicalDefense=14,magicalDefense=30,regeneration=4},
                "Ivory cloth and living jade, stitched for the hands that mend after the fire."),
            Item("jadescale-arcanist-robe","Jadescale Arcanist Robe",EquipmentSlot.Chest,new EquipmentStats {
                attributes=new AttributeValues{intelligence=24,spirit=8,vitality=18},physicalDefense=14,magicalDefense=30,regeneration=2},
                "The dragon's scales hold a cold shimmer between the robe's indigo folds."),
            Item("rooststalkers-grips","Rooststalker's Grips",EquipmentSlot.Hands,new EquipmentStats {
                attributes=new AttributeValues{agility=12,strength=8,vitality=10},physicalDefense=10,magicalDefense=8,attackSpeedPercent=6,basicAttackFlatBonus=3},
                "Jade talons guard the knuckles without slowing a hunter's grip.",false),
            Item("dragontrail-greaves","Dragontrail Greaves",EquipmentSlot.Feet,new EquipmentStats {
                attributes=new AttributeValues(6),physicalDefense=10,magicalDefense=10,movementSpeedPercent=3,regeneration=1},
                "Made for the road beyond the lair, where the ash is still warm.",false)
        };
        try { for(int i=0;i<items.Length;i++) RenderIcon(items[i],i); }
        finally { foreach(var item in temporary)if(item!=null)Object.DestroyImmediate(item);temporary.Clear(); }
        var pool=AssetDatabase.LoadAssetAtPath<DungeonRewardPool>(Pool);
        if(pool==null || pool.rare==null || pool.rare.stableId!="item.ancient-road-dragon.mantle")throw new InvalidOperationException("Existing dragon rewards missing.");
        pool.items=(pool.items??Array.Empty<ItemDefinition>()).Concat(items).GroupBy(i=>i.stableId).Select(g=>g.Last()).ToArray();
        if(pool.items.Length!=8)throw new InvalidOperationException("Expected two existing ordinary rewards and six additions.");
        EditorUtility.SetDirty(pool);AssetDatabase.SaveAssets();
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/DragonLootExpansion.json",JsonUtility.ToJson(new Report {
            addedItems=6,chestItems=4,itemLevel=55,ordinaryPool=pool.items.Length,rare=pool.rare.stableId,iconSource=Armor },true));
        Debug.Log("DICEFREE_DRAGON_LOOT_EXPANSION_OK");
    }
    [Serializable]sealed class Report { public int addedItems,chestItems,itemLevel,ordinaryPool;public string rare,iconSource; }
    static ItemDefinition Item(string id,string title,EquipmentSlot slot,EquipmentStats stats,string flavor,bool rare=true)
    {
        string path=Items+"/"+title+".asset";var item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
        if(item==null){item=ScriptableObject.CreateInstance<ItemDefinition>();AssetDatabase.CreateAsset(item,path);}
        item.name=item.displayName=title;item.stableId="item.ancient-road-dragon."+id;item.itemLevel=55;
        item.slot=slot;item.stats=stats;item.sourceId=AncientRoadDragonLairPortal.BossId;item.allowedClassIds=Array.Empty<string>();
        item.rarityId=rare?"rarity.rare":"rarity.uncommon";item.flavor=flavor;EditorUtility.SetDirty(item);return item;
    }
    static Material Material(string name,Color color,bool glow=false)
    {
        var material=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};
        material.SetFloat("_Smoothness",.38f);material.SetFloat("_Metallic",glow?0:.12f);material.SetFloat("_Cull",0);
        if(glow){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*.12f);}
        temporary.Add(material);return material;
    }
    static Mesh Cropped(string name,Func<Vector3,bool> keep)
    {
        var source=AssetDatabase.LoadAllAssetsAtPath(Armor).OfType<Mesh>().Single(m=>m.name==name);
        var mesh=Object.Instantiate(source);mesh.name="Dragon icon adaptation - "+name;temporary.Add(mesh);
        var vertices=mesh.vertices;
        for(int sub=0;sub<mesh.subMeshCount;sub++)
        {
            var original=source.GetTriangles(sub);var triangles=new List<int>();
            for(int i=0;i<original.Length;i+=3)
                if(keep(vertices[original[i]])&&keep(vertices[original[i+1]])&&keep(vertices[original[i+2]]))
                    triangles.AddRange(new[]{original[i],original[i+1],original[i+2]});
            mesh.SetTriangles(triangles,sub);
        }
        // Unreferenced T-pose arm vertices must not enlarge the icon framing.
        var used=mesh.triangles.Select(i=>vertices[i]).ToArray();
        if(used.Length==0)throw new InvalidOperationException("Empty armour crop: "+name);
        var bounds=new Bounds(used[0],Vector3.zero);foreach(var vertex in used)bounds.Encapsulate(vertex);mesh.bounds=bounds;
        return mesh;
    }
    static void Part(Transform parent,Mesh mesh,Material material)
    {
        var node=new GameObject(mesh.name);node.transform.SetParent(parent,false);node.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=node.AddComponent<MeshRenderer>();renderer.sharedMaterials=Enumerable.Repeat(material,mesh.subMeshCount).ToArray();
    }
    static Mesh Facet()
    {
        var mesh=new Mesh{name="Cut dragon jade"};temporary.Add(mesh);
        mesh.vertices=new[]{new Vector3(0,1,0),new Vector3(-.65f,0,0),new Vector3(0,-1,0),new Vector3(.65f,0,0),new Vector3(0,0,.45f),new Vector3(0,0,-.12f)};
        mesh.triangles=new[]{0,1,4,1,2,4,2,3,4,3,0,4,1,0,5,2,1,5,3,2,5,0,3,5};mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
    }
    static void Badge(Transform parent,Vector3 at,Vector3 scale,Material material)
    {var node=new GameObject("Faceted scale inlay").transform;node.SetParent(parent,false);node.localPosition=at;node.localScale=scale;Part(node,Facet(),material);}
    static void Fang(Transform parent,int side,Material material)
    {
        var vertices=new List<Vector3>();var indices=new List<int>();const int sides=7,rings=5;
        for(int ring=0;ring<rings;ring++)for(int i=0;i<sides;i++)
        {
            float t=ring/(float)(rings-1),a=i*TWO_PI/sides;
            float radius=.042f*(1-t)+.002f;
            vertices.Add(new Vector3(side*.05f*t*t+Mathf.Cos(a)*radius,t*.19f,Mathf.Sin(a)*radius+.04f*t*t));
            if(ring<rings-1){int x=ring*sides+i,y=ring*sides+(i+1)%sides;indices.AddRange(new[]{x,x+sides,y,y,x+sides,y+sides});}
        }
        var mesh=new Mesh{name="Curved dragon fang"};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();temporary.Add(mesh);
        var node=new GameObject(mesh.name).transform;node.SetParent(parent,false);node.localPosition=new Vector3(side*.27f,1.43f,.07f);Part(node,mesh,material);
    }
    const float TWO_PI=Mathf.PI*2;
    static void RenderIcon(ItemDefinition item,int index)
    {
        var primary=Material("Dragon reward primary",index switch {0=>new Color(.32f,.43f,.31f),1=>new Color(.38f,.48f,.45f),2=>new Color(.77f,.73f,.57f),3=>new Color(.27f,.31f,.53f),_=>new Color(.38f,.49f,.41f)});
        var jade=Material("Living jade accents",new Color(.13f,.49f,.31f));
        var trim=Material("Old dragon brass",new Color(.61f,.43f,.20f));
        var ivory=Material("Dragon fang ivory",new Color(.88f,.85f,.68f));
        var gem=Material("Role gemstone",index==2?new Color(.95f,.78f,.30f):index==3?new Color(.35f,.65f,1):new Color(.25f,.92f,.58f),true);
        var root=new GameObject("Library-adapted "+item.displayName);var preview=new PreviewRenderUtility();var old=RenderTexture.active;
        bool previous=ShaderUtil.allowAsyncCompilation;Texture2D image=null;
        try
        {
            ShaderUtil.allowAsyncCompilation=false;
            string mesh=index switch {0=>"PT_Male_Armor_01_A_body",1=>"PT_Male_Armor_05_C_body",2 or 3=>"PT_Male_Armor_cloth_00_body",4=>"PT_Male_Armor_01_A_gauntlets",_=>"PT_Male_Armor_05_C_boots"};
            var baseMesh=Cropped(mesh,v=>index==4?v.x>.50f:index==5?true:Mathf.Abs(v.x)<.34f);
            Part(root.transform,baseMesh,primary);
            if(index is 2 or 3)Part(root.transform,Cropped("PT_Male_Armor_05_C_cape",_=>true),primary);
            if(index<4)
            {
                Badge(root.transform,new(0,1.28f,.195f),new(.10f,.13f,.10f),trim);
                Badge(root.transform,new(0,1.28f,.24f),new(.065f,.085f,.06f),gem);
                for(int side=-1;side<=1;side+=2)for(int row=0;row<3;row++)
                {
                    Badge(root.transform,new(side*(.155f-row*.01f),1.30f-row*.14f,.18f),new(.042f,.052f,.035f),index is 2 or 3?trim:jade);
                }
                if(index<2)foreach(int side in new[]{-1,1})Fang(root.transform,side,ivory);
            }
            else if(index==4)Badge(root.transform,baseMesh.bounds.center+new Vector3(0,0,baseMesh.bounds.extents.z+.01f),new(.06f,.07f,.045f),jade);
            else foreach(int side in new[]{-1,1})Badge(root.transform,new(side*.10f,.31f,.19f),new(.048f,.065f,.035f),trim);
            root.transform.rotation=Quaternion.Euler(0,-18,index==4?-28:0);preview.AddSingleGO(root);
            var renderers=root.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
            var camera=preview.camera;camera.orthographic=true;camera.orthographicSize=Mathf.Max(bounds.extents.x,bounds.extents.y)*1.18f;
            camera.nearClipPlane=.01f;camera.farClipPlane=30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.025f,.055f,.06f,1);
            camera.transform.SetPositionAndRotation(bounds.center+Vector3.forward*8,Quaternion.Euler(0,180,0));
            preview.lights[0].color=Color.white;preview.lights[0].intensity=1.8f;preview.lights[0].transform.rotation=Quaternion.Euler(25,145,0);
            preview.lights[1].color=Color.white;preview.lights[1].intensity=1.2f;preview.lights[1].transform.rotation=Quaternion.Euler(-15,225,0);preview.ambientColor=new Color(.40f,.43f,.46f);
            preview.BeginPreview(new Rect(0,0,512,512),GUIStyle.none);preview.Render(true);RenderTexture.active=(RenderTexture)preview.EndPreview();
            image=new Texture2D(512,512,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,512,512),0,0);image.Apply();
            string path=Icons+"/"+item.stableId+".png";File.WriteAllBytes(path,image.EncodeToPNG());AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            item.icon=AssetDatabase.LoadAssetAtPath<Sprite>(path);EditorUtility.SetDirty(item);
        }
        finally {RenderTexture.active=old;ShaderUtil.allowAsyncCompilation=previous;preview.Cleanup();if(root!=null)Object.DestroyImmediate(root);if(image!=null)Object.DestroyImmediate(image);}
    }
}
