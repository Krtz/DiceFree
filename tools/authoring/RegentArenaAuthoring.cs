using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Dungeons;
using DiceFree.Items;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static class RegentArenaAuthoring
{
    const string Data = "Assets/_DiceFree/Settings/Dungeons/";
    const string Content = Data + "Regent Court";
    const string Loot = "Assets/_DiceFree/Resources/DiceFree/DungeonRewards/SlimeRegent";
    const string ScenePath = "Assets/_DiceFree/Scenes/SlimeDungeon.unity";
    static readonly Rect Court = Rect.MinMaxRect(30, -11, 80, 29);
    static Material azure, darkStone, trim, paleCrystal;

    public static void Batch()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        Directory.CreateDirectory(Content);
        Directory.CreateDirectory(Loot);
        AssetDatabase.Refresh();
        var root = scene.GetRootGameObjects().Single(go => go.GetComponent<WorldNavigation>()).transform;
        var tuning = Load<SlimeDungeonTuning>(Data + "Slime playtest tuning.asset");
        Rebalance(tuning);
        PrepareMaterials();
        var court = WidenCourt(root);
        var gate = root.Find("Enchanted Regent barrier");
        ColorGate(gate);
        DressCourt(court);
        gate.gameObject.SetActive(false);
        try { Bake(); }
        finally { gate.gameObject.SetActive(true); }
        EditorUtility.SetDirty(tuning);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save the Regent Court.");
    }

    static void Rebalance(SlimeDungeonTuning tuning)
    {
        tuning.regentSlam.baseDamage = 300;
        tuning.regentSlamRadius = 5.5f;
        tuning.regentTrailMultiplier = .18f;
        tuning.regentAddAttack.baseDamage = 21;
        tuning.regentXp = 3600;
        tuning.regentGold = 280;
        tuning.regentBonusXp = 1800;
        tuning.regentBonusGold = 140;
        var bump = ScriptableObject.CreateInstance<AttackDefinition>();
        bump.stableId = "attack.dungeon.regent-bump";
        bump.displayName = "Regent's Crush";
        bump.baseDamage = 66;
        bump.coefficient = 0;
        bump.interval = 1.5f;
        bump.windup = .37f;
        bump.reach = .9f;
        AssetDatabase.CreateAsset(bump, Content + "/Regent crush.asset");
        tuning.regent.basicAttack = bump;
        EditorUtility.SetDirty(tuning.regent);
        EditorUtility.SetDirty(tuning.regentAddAttack);
        EditorUtility.SetDirty(tuning.regentSlam);
        var pool = tuning.regentRewards;
        pool.items = new[]
        {
            Item("Regent's Signet", "signet", EquipmentSlot.Ring, "rarity.uncommon",
                new EquipmentStats { attributes=new AttributeValues(5), physicalDefense=6, magicalDefense=6 }),
            Item("Azure Slime Orb", "orb", EquipmentSlot.OffHand, "rarity.uncommon",
                new EquipmentStats { attributes=new AttributeValues{ intelligence=11,spirit=9,vitality=6 },
                    magicalDefense=8,basicAttackMaximumBonus=6 },
                new[] { "class.magically-touched-novice","class.mage","class.psion","class.priest","class.acolyte" }),
            Item("Crownguard Gauntlets", "gauntlets", EquipmentSlot.Hands, "rarity.uncommon",
                new EquipmentStats { attributes=new AttributeValues{ strength=7,agility=9,vitality=8 },
                    attackSpeedPercent=8,physicalDefense=6 })
        };
        pool.rare = Item("Slime Regent's Crown", "crown", EquipmentSlot.Head, "rarity.rare",
            new EquipmentStats { attributes=new AttributeValues(7), physicalDefense=12, magicalDefense=12, regeneration=13 });
        EditorUtility.SetDirty(pool);
    }

    static ItemDefinition Item(string title, string id, EquipmentSlot slot, string rarity, EquipmentStats stats, string[] classes=null)
    {
        var item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.stableId = "item.slime-regent." + id;
        item.displayName = title;
        item.slot = slot;
        item.rarityId = rarity;
        item.itemLevel = 50;
        item.sourceId = "boss.slime-regent";
        item.stats = stats;
        if (classes != null) item.allowedClassIds = classes;
        item.icon = Icon(id);
        AssetDatabase.CreateAsset(item, Loot + "/" + title + ".asset");
        return item;
    }

    static Sprite Icon(string id)
    {
        const int size=80;
        var tex=new Texture2D(size,size,TextureFormat.RGB24,false);
        var gold=new Color(.97f,.75f,.34f);
        for (int y=0;y<size;y++)
        for (int x=0;x<size;x++)
        {
            float u=(x-39.5f)/36, v=(y-39.5f)/36;
            float ax=Mathf.Abs(u);
            float distance=new Vector2(u,v).magnitude;
            var color=Color.Lerp(new Color(.026f,.043f,.09f),new Color(.06f,.21f,.32f),Mathf.Clamp01(1-distance));
            bool metal = id switch
            {
                "signet" => Mathf.Abs(new Vector2(u,(v+.20f)*1.24f).magnitude-.49f)<.085f,
                "orb" => Mathf.Abs(new Vector2(u,(v-.05f)*1.10f).magnitude-.59f)<.045f || ax<.44f&&v is >-.74f and <-.55f,
                "gauntlets" => ax<.68f && v>-.68f && v<.52f-Mathf.Abs(ax-.41f)*1.3f && ax>.11f,
                _ => ax<.76f && v>-.44f && v<.62f-(Mathf.Cos(u*9f)+1)*.22f+ax*.20f
            };
            if(metal)color=Color.Lerp(new Color(.27f,.20f,.12f),gold,Mathf.Clamp01(.62f+v*.6f+Mathf.Sin(u*25)*.095f));
            float gem = id switch
            {
                "crown" => Mathf.Abs(u)*1.4f + Mathf.Abs(v+.16f),
                "gauntlets" => Mathf.Abs(ax-.39f)*2.2f+Mathf.Abs(v+.16f),
                "signet" => ax*1.4f+Mathf.Abs(v-.35f),
                _ => new Vector2(u,(v-.04f)*1.1f).magnitude*.61f
            };
            if(gem<.32f)
            {
                float glint=Mathf.Clamp01(.20f+.43f*Mathf.Max(0,-u)+.30f*Mathf.Max(0,v)+.14f*(Mathf.Abs(Mathf.Sin(u*13+v*12))));
                color=Color.Lerp(new Color(.03f,.31f,.74f),new Color(.65f,.98f,1f),glint+(gem>.29f?.18f:0));
            }
            if(Mathf.Max(ax,Mathf.Abs(v)) is >1.02f and <1.07f)color=new Color(.34f,.29f,.24f);
            tex.SetPixel(x,y,color*(.92f+.15f*Mathf.PerlinNoise(x*.3f,y*.45f)));
        }
        tex.Apply();
        string path=Content+"/"+id+"-icon.png";
        File.WriteAllBytes(path,tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var imp=(TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType=TextureImporterType.Sprite;
        imp.spriteImportMode=SpriteImportMode.Single;
        imp.mipmapEnabled=false;
        imp.SaveAndReimport();
        return Load<Sprite>(path);
    }

    static void PrepareMaterials()
    {
        azure = new Material(Load<Material>(Data+"Dungeon Foliage Blue.mat")) { color=new Color(.065f,.37f,.94f) };
        azure.EnableKeyword("_EMISSION");
        azure.SetColor("_EmissionColor", new Color(.025f,.085f,.31f));
        AssetDatabase.CreateAsset(azure, Content+"/Enchanted azure leaves.mat");
        darkStone = Mat("Cool basalt", new Color(.20f,.27f,.35f));
        trim = Mat("Tarnished gilt inlay", new Color(.60f,.43f,.18f));
        paleCrystal = Mat("Blue gel crystal", new Color(.13f,.55f,1));
        paleCrystal.EnableKeyword("_EMISSION");
        paleCrystal.SetColor("_EmissionColor", new Color(.045f,.18f,.53f));
        paleCrystal.SetFloat("_Smoothness", .67f);
        EditorUtility.SetDirty(paleCrystal);
    }

    static Material Mat(string name, Color color)
    {
        var material=new Material(Shader.Find("Universal Render Pipeline/Lit")) { color=color };
        AssetDatabase.CreateAsset(material,Content+"/"+name+".mat");
        return material;
    }
    
    static Transform WidenCourt(Transform root)
    {
        var origin=SlimeDungeonRun.Origin;
        var floor=root.Cast<Transform>().Single(t=>t.name=="Forest corridor" &&
            Mathf.Abs(t.position.x-origin.x-52.5f)<.08f && Mathf.Abs(t.position.z-origin.z-17)<.08f);
        floor.position=origin+new Vector3(Court.center.x,-.25f,Court.center.y);
        floor.localScale=new Vector3(Court.width,.5f,Court.height);
        floor.name="Regent Court - expanded ground";

        var clear = Rect.MinMaxRect(Court.xMin-1.2f,Court.yMin-1.2f,Court.xMax+1.2f,Court.yMax+1.2f);
        using var log=new StreamWriter("Logs/RegentCourtAuthoring.txt");
        foreach(var tree in root.Cast<Transform>().Where(t=>t.name.StartsWith("Tree_",StringComparison.Ordinal)).ToArray())
        {
            var p=tree.position-origin;
            if(!clear.Contains(new Vector2(p.x,p.z)))continue;
            log.WriteLine("Cleared space for Regent Court: "+tree.name+" "+p);
            Object.DestroyImmediate(tree.gameObject);
        }
        var court=Group(root,"Regent Court perimeter and crown seal",SlimeDungeonRun.RegentArenaCenter);
        return court;
    }

    static void ColorGate(Transform gate)
    {
        int slots=0;
        foreach(var renderer in gate.GetComponentsInChildren<Renderer>(true))
        {
            var mats=renderer.sharedMaterials;
            for(int i=0;i<mats.Length;i++)
            {
                if(mats[i]==null || !(mats[i].name.Contains("Leaf",StringComparison.OrdinalIgnoreCase) ||
                    mats[i].name.Contains("Foliage",StringComparison.OrdinalIgnoreCase)))continue;
                mats[i]=azure;
                slots++;
            }
            renderer.sharedMaterials=mats;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }
        if(slots==0)throw new InvalidOperationException("Regent barrier leaf materials were not found.");
        for(int i=-2;i<=2;i++)
            Part(gate,"Visible blue ward crystal",PrimitiveType.Sphere,new Vector3(i*2.3f,2.4f,.14f),
                new Vector3(.28f,.44f,.28f),paleCrystal);
    }

    static void DressCourt(Transform court)
    {
        var min=Court.min-Court.center;
        var max=Court.max-Court.center;
        // Keep the central 12-metre entry and the whole combat floor clear.
        for(int i=0;i<44;i++)
        {
            bool across=i<24, end=i%2==0;
            float t=(i/2%(across?12:10)+.5f)/(across?12:10);
            var p=across?new Vector3(Mathf.Lerp(min.x,max.x,t),.55f,end?min.y-.7f:max.y+.7f):
                new Vector3(end?min.x-.67f:max.x+.67f,.55f,Mathf.Lerp(min.y,max.y,t));
            if(Mathf.Abs(p.x)<6 && p.z>0)continue;
            var stone=Part(court,"Weathered court edging",PrimitiveType.Cube,p,
                new Vector3(1.75f+(i%4)*.22f,1.0f+(i%3)*.24f,1.5f+(i%5)*.18f),darkStone);
            stone.transform.localRotation=Quaternion.Euler(i%7*4,i*31,i%5*6);
            if(i%3==0)
            {
                var shard=Part(court,"Azure mineral growth",PrimitiveType.Sphere,p+Vector3.up*.72f,
                    new Vector3(.42f,.98f,.46f),paleCrystal);
                shard.transform.localRotation=Quaternion.Euler(14,i*25,32);
            }
        }

        var seal=Group(court,"Crown inlaid in the turf",Vector3.up*.023f);
        for(int i=0;i<20;i++)
        {
            float angle=(i+.5f)*UnityEngine.Mathf.PI/10;
            var outward=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));
            var line=Part(seal,"Inlaid ring stone",PrimitiveType.Cube,outward*6.6f,
                new Vector3(2.07f,.042f,i%5==2?.20f:.09f),i%5==2?paleCrystal:trim);
            line.transform.localRotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg,0);
        }
        var points=new Vector3[] {new(-3,0,-1),new(-4,0,2),new(-1.6f,0,.9f),new(0,0,3.2f),
            new(1.6f,0,.9f),new(4,0,2),new(3,0,-1),new(-3,0,-1)};
        for(int i=1;i<points.Length;i++)
        {
            var delta=points[i]-points[i-1];
            var line=Part(seal,"Crown line",PrimitiveType.Cube,(points[i]+points[i-1])*.5f,
                new Vector3(.17f,.038f,delta.magnitude+.08f),trim);
            line.transform.localRotation=Quaternion.LookRotation(delta);
        }
    }

    static void Bake()
    {
        var bounds=new Bounds(SlimeDungeonRun.Origin+new Vector3(41,0,39),new Vector3(118,32,131));
        var sources=new List<NavMeshBuildSource>();
        Physics.SyncTransforms();
        NavMeshBuilder.CollectSources(bounds,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);
        var settings=NavMesh.GetSettingsByID(0);
        settings.agentRadius=.5f;
        settings.agentHeight=1.8f;
        settings.agentClimb=.4f;
        var fresh=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);
        if(fresh==null)throw new InvalidOperationException("Slime Dungeon navigation bake failed.");
        var registered=NavMesh.AddNavMeshData(fresh);
        try
        {
            var path=new NavMeshPath();
            var origin=SlimeDungeonRun.Origin;
            if(!NavMesh.SamplePosition(origin+new Vector3(52,0,34),out var gate,1,1))
                throw new InvalidOperationException("Regent corridor has no navigation.");
            foreach(var p in new[]{Court.center,new Vector2(32,-8),new Vector2(77,-8),new Vector2(32,26),new Vector2(77,26)})
                if(!NavMesh.SamplePosition(origin+new Vector3(p.x,0,p.y),out var hit,1,1) ||
                    !NavMesh.CalculatePath(gate.position,hit.position,1,path) || path.status!=NavMeshPathStatus.PathComplete)
                    throw new InvalidOperationException($"Regent Court has an isolated corner at {p}. Gate at {gate.position:F2}, last path {path.status}.");
            var saved=Load<NavMeshData>(Data+"SlimeDungeonNavMesh.asset");
            EditorUtility.CopySerialized(fresh,saved);
            EditorUtility.SetDirty(saved);
        }
        finally{registered.Remove();Object.DestroyImmediate(fresh);}
    }

    static T Load<T>(string path) where T:Object => AssetDatabase.LoadAssetAtPath<T>(path) ??
        throw new FileNotFoundException("Missing Regent source.",path);

    static Transform Group(Transform parent,string name,Vector3 worldPoint)
    {
        var go=new GameObject(name).transform;
        go.SetParent(parent);
        go.position=worldPoint;
        return go;
    }
    static GameObject Part(Transform parent,string name,PrimitiveType type,Vector3 local,Vector3 scale,Material material)
    {
        var go=GameObject.CreatePrimitive(type);
        go.name=name;
        go.layer=2;
        go.transform.SetParent(parent,false);
        go.transform.localPosition=local;
        go.transform.localScale=scale;
        go.GetComponent<Renderer>().sharedMaterial=material;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }
}

