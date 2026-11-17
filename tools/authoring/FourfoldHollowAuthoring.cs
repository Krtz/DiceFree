using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.Items;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class FourfoldHollowAuthoring
{
    const string World = "Assets/_DiceFree/Scenes/Cornberg.unity";
    const string ScenePath = "Assets/_DiceFree/Scenes/FourfoldHollow.unity";
    const string Art = "Assets/_DiceFree/Settings/Briarback/FourfoldHollow";
    const string Items = "Assets/_DiceFree/Resources/DiceFree/DungeonRewards/FourfoldHollow";
    static readonly Vector3 Origin = ShamanDungeonRun.Origin;
    static readonly string[] Elements = { "Fire", "Earth", "Lightning", "Ice" };
    static readonly string[] Titles = { "Ember Warden", "Splitroot Warden", "Stormcall Warden", "Rimeglass Warden", "Hollow Hierophant" };
    static readonly Color[] Tints = { new(1f,.32f,.075f), new(.66f,.83f,.25f), new(.70f,.35f,1f), new(.21f,.79f,.97f) };
    static Material slate, deepSlate, tile, tileDark, bronze, black, warning;
    static Material[] sigils, crystal;
    static Mesh boulder;
    
    public static void Batch()
    {
        if (File.Exists(ScenePath)) throw new InvalidOperationException("Fourfold Hollow is already authored.");
        var world = EditorSceneManager.OpenScene(World);
        var originals = Elements.Select(element => Object.FindObjectsByType<QuilboarCombat>()
            .Single(shaman => shaman.name.StartsWith(element + "Shaman guardian - "))).ToArray();
        var nav = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            MoveLedgerTrees();
            Directory.CreateDirectory(Art);
            Directory.CreateDirectory(Items);
            AssetDatabase.Refresh();
            CreateMaterials();
            Entrance();
        }
        finally { nav.Remove(); }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        var root = new GameObject("Fourfold Hollow - ritual chamber").transform;
        root.position = Origin;
        var arena = root.gameObject.AddComponent<ShamanDungeonArena>();
        arena.rewards = CreateLoot();
        arena.arrival = Group(root, "Arrival", new Vector3(0, .05f, -20.5f));
        arena.braziers = BuildChamber(root);
        arena.startPrompt = CreateAltar(root);
        var exit = Group(root, "Path back to Briarback", new Vector3(0,0,-24.3f));
        Portal(exit, "Return to Briarback", Vector3.zero, ShamanDungeonPortal.Action.Leave, 1.05f);
        Torch(exit, new Vector3(-2.2f,0,.4f), 0);
        Torch(exit, new Vector3(2.2f,0,.4f), 0);
        Label(exit, "BRIARBACK", new Vector3(0,1.9f,0));

        var spells = CreateSpells(originals);
        var starts = new Vector3[] { new(-8,.08f,5), new(8,.08f,5), new(-8,.08f,14), new(8,.08f,14), new(0,.08f,15) };
        var opponents = Group(root, "Ritual opponents", Vector3.zero);
        arena.opponents = Titles.Select((_,i)=>MakeEnemy(opponents,originals[i%4],i,starts[i],spells)).ToArray();
        Bake(root, arena.arrival.position, arena.opponents.Select(actor => actor.transform.position));
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Could not save Fourfold Hollow.");
        EditorBuildSettings.scenes = EditorBuildSettings.scenes.Append(new EditorBuildSettingsScene(ScenePath, true)).ToArray();
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(world);
        if (!EditorSceneManager.SaveScene(world)) throw new InvalidOperationException("Could not save the Briarback entrance.");
    }

    static void MoveLedgerTrees()
    {
        var camps = GameObject.Find("Highland pines and watch camps").transform;
        var pairs = new[] { (new Vector2(82.920f,434.716f), new Vector2(78.1f,442.5f)),
                            (new Vector2(129.638f,425.832f), new Vector2(137,422)) };
        using var edited = new StreamWriter("Logs/FourfoldMovedTreeIds.txt");
        foreach (var (old, destination) in pairs)
        {
            var tree = camps.GetComponentsInChildren<TreeCameraOccluder>().Single(candidate =>
                Vector2.Distance(old, new Vector2(candidate.transform.position.x,candidate.transform.position.z)) < .08f).transform;
            if (!Physics.Raycast(new Vector3(destination.x,60,destination.y),Vector3.down,out var hit,80,1<<8) ||
                hit.collider.name != "Reedscale highlands walkable terrain")
                throw new InvalidOperationException("Ledger tree placement has no highland floor.");
            tree.position = hit.point;
            edited.WriteLine(GlobalObjectId.GetGlobalObjectIdSlow(tree).targetObjectId);
            EditorUtility.SetDirty(tree);
        }
    }

    static void Entrance()
    {
        if (!Physics.Raycast(new Vector3(257,55,417),Vector3.down,out var ground,90,1<<8) ||
            !NavMesh.SamplePosition(ground.point,out var hit,2.5f,1))
            throw new InvalidOperationException("Fourfold Hollow entrance is off the Briarback NavMesh.");
        var root = new GameObject("Fourfold Hollow - Briarback cave entrance").transform;
        root.position = hit.position;
        root.rotation = Quaternion.Euler(0,194,0);
        Portal(root,"Enter Fourfold Hollow",Vector3.zero,ShamanDungeonPortal.Action.Enter,1.05f);
        for (int side=-1;side<=1;side+=2)
        {
            Rock(root,new Vector3(side*5.15f,2,-3.4f),new Vector3(6.6f,6.5f,6.5f),side*28,deepSlate);
            Rock(root,new Vector3(side*2.75f,1.6f,-.9f),new Vector3(3.5f,5.15f,3.9f),side*62,slate);
            Rock(root,new Vector3(side*7.35f,1.1f,-4.7f),new Vector3(4.4f,5,7),side*87,slate);
            Torch(root,new Vector3(side*2.03f,0,1.36f),side==-1?0:3);
        }
        Rock(root,new Vector3(0,4.25f,-2.05f),new Vector3(7.4f,3.65f,6.2f),11,slate);
        Rock(root,new Vector3(0,2.8f,-5.2f),new Vector3(11.6f,7.1f,5.7f),88,deepSlate);
        Part(root,"Cavern shadow",PrimitiveType.Cube,new Vector3(0,1.4f,.20f),new Vector3(3.65f,3.18f,.12f),black);
        // Keep the actual doorway clear and steer world paths around the far side of the bluff.
        var cliff = Group(root,"Cave back and navigation obstruction",new Vector3(0,1.4f,-4.55f)).gameObject;
        cliff.layer=9;
        var blocker=cliff.AddComponent<BoxCollider>();
        blocker.size=new Vector3(10.8f,3.6f,4.75f);
        var obstacle=cliff.AddComponent<NavMeshObstacle>();
        obstacle.shape=NavMeshObstacleShape.Box;
        obstacle.size=blocker.size;
        obstacle.carving=true;
        var seal=Group(root,"Carved lintel",new Vector3(0,3.75f,1));
        seal.localRotation=Quaternion.Euler(-90,0,0);
        Seal(seal,Vector3.zero,.68f,0);
        Label(root,"FOURFOLD HOLLOW",new Vector3(0,4.55f,1.5f));
    }

    static GameObject[] BuildChamber(Transform root)
    {
        Disk(root,"Continuous slate floor",0,24,29,slate,true);
        Disk(root,"Lower cavern shelves",-.28f,35,38,deepSlate,false);
        Disk(root,"Dark cavern beyond the walls",-1.2f,100,110,black,false);
        Paving(root);
        var caverns=Group(root,"Basalt walls and mineral seams",Vector3.zero);
        for (int n=0;n<46;n++)
        {
            float angle=n*2*Mathf.PI/46;
            float x=Mathf.Cos(angle)*(24.6f+Mathf.Sin(n*2.3f)),z=Mathf.Sin(angle)*(30.2f+Mathf.Cos(n*2.3f));
            float height=(x+z<-7?2.8f:8.8f)+2.3f*Mathf.Abs(Mathf.Sin(n*3.31f));
            Rock(caverns,new Vector3(x,.32f*height,z),new Vector3(5+n%4,height,5.9f+n%3),n*37,n%4==0?slate:deepSlate);
            if(n%2==0)Rock(caverns,new Vector3(x*.93f,.33f,z*.93f),new Vector3(3.1f,1.8f,2.3f),n*47,tileDark);
            if(n%4==1)Crystal(caverns,new Vector3(x*.88f,0,z*.88f),n/4%4,.72f);
        }

        var wardLights=new GameObject[4];
        var pads=new Vector3[] { new(-10,0,6),new(10,0,6),new(-10,0,16),new(10,0,16) };
        for (int i=0;i<pads.Length;i++)
        {
            var shrine=Group(root,Elements[i]+" shrine",pads[i]);
            Seal(shrine,Vector3.up*.069f,2.75f,i);
            float side=Mathf.Sign(pads[i].x);
            Rock(shrine,new Vector3(side*3.7f,1.1f,1.45f),new Vector3(2.3f,3.7f,2.1f),43+i*58,slate);
            Crystal(shrine,new Vector3(side*2.8f,0,-.6f),i,1);
            Torch(shrine,new Vector3(side*2.87f,0,1.15f),i);
            wardLights[i]=Group(shrine,"Awakened ward",Vector3.zero).gameObject;
            Rock(wardLights[i].transform,new Vector3(side*2.87f,2.15f,1.15f),new Vector3(.56f,1.06f,.56f),30,sigils[i]);
            PointLight(wardLights[i].transform,new Vector3(side*2.87f,2.2f,1.15f),Tints[i],12,3.4f);
            wardLights[i].SetActive(false);
        }

        var dais=Group(root,"Hierophant's ritual throne",new Vector3(0,0,20));
        Seal(dais,new Vector3(0,.075f,-4.6f),4.1f,2);
        Rock(dais,new Vector3(0,2.3f,1.5f),new Vector3(4.6f,5.9f,2.7f),38,tileDark);
        for(int side=-1;side<=1;side+=2)
        {
            Rock(dais,new Vector3(side*3.6f,1.43f,0),new Vector3(2.1f,4.7f,2.6f),side*33,slate);
            Crystal(dais,new Vector3(side*2.7f,0,-.5f),side==-1?2:3,1.23f);
        }
        var glyph=Group(dais,"Throne relief",new Vector3(0,2.5f,-.05f));
        glyph.localRotation=Quaternion.Euler(-90,0,0);
        Seal(glyph,Vector3.zero,1.25f,2);
        PointLight(dais,new Vector3(0,4,-2.8f),new Color(.64f,.47f,1),16,3.4f);

        var key=Group(root,"Cavern reflected skylight",Vector3.zero).gameObject.AddComponent<Light>();
        key.type=LightType.Directional;
        key.color=new Color(.73f,.82f,1);
        key.intensity=1.4f;
        key.shadows=LightShadows.Soft;
        key.transform.rotation=Quaternion.Euler(49,-38,0);
        return wardLights;
    }

    static GameObject CreateAltar(Transform root)
    {
        var altar=Group(root,"Central ritual dais",new Vector3(0,0,2));
        Seal(altar,Vector3.up*.053f,4.8f,0);
        Part(altar,"Basalt altar",PrimitiveType.Cylinder,Vector3.up*.24f,new Vector3(2.42f,.24f,2.42f),deepSlate,9);
        Part(altar,"Gilded altar rim",PrimitiveType.Cylinder,Vector3.up*.49f,new Vector3(2.54f,.04f,2.54f),bronze);
        for(int i=0;i<4;i++)
        {
            var pos=Quaternion.Euler(0,i*90+45,0)*Vector3.right*.75f;
            Rock(altar,pos+Vector3.up*.73f,new Vector3(.34f,.67f,.34f),i*45,sigils[i]);
        }
        var prompt=Portal(altar,"Begin the Fourfold Ritual",new Vector3(0,0,-1.85f),ShamanDungeonPortal.Action.BeginRitual,.7f);
        Label(prompt.transform,"BEGIN THE RITUAL",Vector3.up*2);
        return prompt.gameObject;
    }

    static AttackDefinition[] CreateSpells(QuilboarCombat[] sources)
    {
        string[] ids={"ember","earth","storm","rime"};
        string[] names={"Ember Eruption","Splitroot Quake","Stormcall Chain","Rimeglass Burst"};
        return sources.Select((source,i)=>
        {
            var spell=Object.Instantiate(source.GetComponent<ActorStats>().Definition.basicAttack);
            spell.stableId="fourfold."+ids[i];
            spell.displayName=names[i];
            spell.baseDamage=235+i*15;
            spell.coefficient=0;
            spell.requiresAccuracy=false;
            AssetDatabase.CreateAsset(spell,Art+"/"+names[i]+".asset");
            return spell;
        }).ToArray();
    }

    static CombatActor MakeEnemy(Transform parent,QuilboarCombat source,int index,Vector3 at,AttackDefinition[] spells)
    {
        bool boss=index==4;
        var go=Object.Instantiate(source.gameObject,parent);
        if(PrefabUtility.IsPartOfPrefabInstance(go))
            PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        go.SetActive(false);
        go.name=Titles[index];
        go.transform.localPosition=at;
        go.transform.localRotation=Quaternion.Euler(0,180,0);
        foreach(var behaviour in go.GetComponents<MonoBehaviour>())
            if(behaviour is not (TraversalMotor or ActorStats or Health or CombatActor or BasicAttack or AggroBehaviour or
                CombatStatusController or DefeatReporter or QuilboarCombat or QuilboarPresentation))
                Object.DestroyImmediate(behaviour);

        var stat=go.GetComponent<ActorStats>();
        var definition=Object.Instantiate(stat.Definition);
        var attack=Object.Instantiate(definition.basicAttack);
        definition.displayName=Titles[index];
        definition.stableId=boss?"boss.hollow-hierophant":"enemy.fourfold."+Elements[index].ToLowerInvariant();
        definition.familyId="fourfold.wardens";
        definition.tags=new[]{"fourfold-hollow","quilboar"};
        definition.baseHp=boss?4900:1580+index*210;
        definition.experienceReward=0;
        definition.physicalDefense=boss?22:12;
        definition.magicalDefense=boss?22:14;
        definition.basicAttack=attack;
        attack.stableId=definition.stableId+".bolt";
        attack.displayName=boss?"Hierophant's Hex":attack.displayName;
        attack.baseDamage=boss?124:75+index*8;
        AssetDatabase.CreateAsset(attack,Art+"/"+Titles[index]+" attack.asset");
        AssetDatabase.CreateAsset(definition,Art+"/"+Titles[index]+" stats.asset");
        stat.Configure(definition,boss?40:34+index);
        var aggro=go.GetComponent<AggroBehaviour>();
        aggro.Configure(60,95);
        aggro.ConfigurePatrolMovement(false);
        aggro.ConfigureHomePosition(go.transform.position);
        go.GetComponent<NavMeshAgent>().enabled=false;
        if(boss)
        {
            var visual=go.transform.Cast<Transform>().FirstOrDefault(t=>t.GetComponentInChildren<Animator>(true)!=null);
            if(visual!=null)visual.localScale*=1.26f;
        }

        var ability=go.AddComponent<ShamanRitualCasts>();
        ability.spells=boss?spells:new[]{spells[index]};
        ability.warningMaterial=warning;
        return go.GetComponent<CombatActor>();
    }

    static DungeonRewardPool CreateLoot()
    {
        var pool=ScriptableObject.CreateInstance<DungeonRewardPool>();
        pool.stableId=ShamanDungeonRun.DungeonId+".treasure";
        pool.items=new[]
        {
            Item("Fourfold Band","band",EquipmentSlot.Ring,38,"rarity.uncommon",2,
                new EquipmentStats{attributes=new AttributeValues{strength=4,agility=4,intelligence=4,spirit=4,vitality=4},magicalDefense=5},
                "Four slivers of ritual stone share one hammered setting."),
            Item("Rimeglass Pendant","rimeglass",EquipmentSlot.Amulet,38,"rarity.uncommon",3,
                new EquipmentStats{attributes=new AttributeValues{intelligence=8,spirit=8,vitality=3},magicalDefense=6},
                "A chip from the ice altar. It keeps its winter frost even in the warmest room.")
        };
        pool.rare=Item("Stormbound Mantle","stormbound",EquipmentSlot.Shoulders,40,"rarity.rare",2,
            new EquipmentStats{attributes=new AttributeValues{agility=9,spirit=6,vitality=8},
                physicalDefense=10,magicalDefense=12,movementSpeedPercent=3},
            "Silver wire stitched through dyed hide carries a charge from the Stormcall ward.");
        AssetDatabase.CreateAsset(pool,Art+"/Fourfold treasure.asset");
        return pool;
    }

    static ItemDefinition Item(string name,string id,EquipmentSlot slot,int level,string rarity,int tint,
        EquipmentStats stats,string flavor)
    {
        var item=ScriptableObject.CreateInstance<ItemDefinition>();
        item.stableId="item.fourfold."+id;
        item.sourceId=ShamanDungeonRun.DungeonId;
        item.displayName=name;
        item.slot=slot;
        item.itemLevel=level;
        item.rarityId=rarity;
        item.stats=stats;
        item.flavor=flavor;
        item.icon=PaintItemIcon(id,slot,tint);
        AssetDatabase.CreateAsset(item,Items+"/"+name+".asset");
        return item;
    }

    static Sprite PaintItemIcon(string id,EquipmentSlot slot,int tint)
    {
        const int size=96;
        var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
        for(int y=0;y<size;y++)
        for(int x=0;x<size;x++)
        {
            float u=(x-47.5f)/45f,v=(y-47.5f)/45f;
            float radial=new Vector2(u,v).magnitude;
            var color=Color.Lerp(new Color(.054f,.041f,.083f),new Color(.16f,.13f,.24f),Mathf.Clamp01(1-radial));
            if(Mathf.Max(Mathf.Abs(u),Mathf.Abs(v)) is >1.02f and <1.06f)color=new Color(.52f,.37f,.20f);
            bool ring=slot==EquipmentSlot.Ring && Mathf.Abs(new Vector2(u*.94f,(v+.18f)*1.24f).magnitude-.47f)<.08f;
            bool chain=slot==EquipmentSlot.Amulet && Mathf.Abs(new Vector2(u*.83f,(v-.53f)*1.07f).magnitude-.45f)<.028f && v>.16f;
            if(ring || chain)color=Color.Lerp(new Color(.37f,.22f,.09f),new Color(.97f,.76f,.39f),Mathf.Clamp01(.51f+v-u*.3f));
            float gemX=slot==EquipmentSlot.Shoulders?Mathf.Abs(u)-.38f:u;
            float gemY=slot==EquipmentSlot.Ring?v-.28f:v+.18f;
            if(slot==EquipmentSlot.Shoulders && Mathf.Abs(u)<.73f && v<.46f-Mathf.Abs(u)*.35f && v>-.59f+Mathf.Abs(u)*.21f)
                color=Color.Lerp(new Color(.17f,.12f,.27f),new Color(.65f,.59f,.77f),Mathf.Clamp01(v+.40f+Mathf.Cos(u*50)*.11f));
            float jewel=Mathf.Abs(gemX)*1.25f+Mathf.Abs(gemY)*.75f;
            if(jewel<.34f)
            {
                int facet=(gemX>0?1:0)+(gemY>0?2:0);
                var jewelColor=slot==EquipmentSlot.Ring?Tints[facet]:Tints[tint];
                color=Color.Lerp(jewelColor*.55f,Color.white,.13f+facet*.14f+Mathf.Max(0,gemY)*.22f);
                if(jewel>.289f)color=Color.Lerp(color,new Color(.93f,.82f,.57f),.64f);
            }
            float grain=Mathf.PerlinNoise(x*.31f+9,y*.39f+7)*.17f+.86f;
            texture.SetPixel(x,y,new Color(color.r*grain,color.g*grain,color.b*grain,1));
        }
        texture.Apply();
        string path=Art+"/"+id+"-icon.png";
        File.WriteAllBytes(path,texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Single;
        importer.mipmapEnabled=false;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void CreateMaterials()
    {
        slate=Material("Pitted cavern slate",new Color(.31f,.29f,.35f));
        deepSlate=Material("Basalt in shadow",new Color(.16f,.15f,.22f));
        tile=Material("Dusky slate pavers",new Color(.31f,.29f,.34f));
        tileDark=Material("Blue slate pavers",new Color(.22f,.26f,.34f));
        bronze=Material("Hammered old bronze",new Color(.64f,.44f,.23f));
        bronze.SetFloat("_Metallic",.36f);
        bronze.SetFloat("_Smoothness",.37f);
        black=Material("Shadow at cavern mouth",new Color(.016f,.013f,.026f),true);
        warning=Material("Ritual warning inlay",Color.white,true);
        sigils=Tints.Select((tint,i)=>Material(Elements[i]+" lit sigils",tint*1.32f,true)).ToArray();
        crystal=Tints.Select((tint,i)=>
        {
            var mat=Material(Elements[i]+" crystals",tint*.55f);
            mat.SetFloat("_Smoothness",.7f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor",tint*.48f);
            return mat;
        }).ToArray();
        AddStoneTexture();
        boulder=MakeBoulder();
    }

    static void AddStoneTexture()
    {
        const int n=128;
        var texture=new Texture2D(n,n,TextureFormat.RGB24,true);
        for(int y=0;y<n;y++)
        for(int x=0;x<n;x++)
        {
            float grain=Mathf.PerlinNoise(x*.37f+4,y*.51f);
            float vein=Mathf.PerlinNoise(x*.037f,y*.07f+Mathf.Sin(x*.098f)*.9f);
            float value=.62f+vein*.29f+grain*.19f;
            texture.SetPixel(x,y,new Color(value,value*.98f,value*1.05f));
        }
        texture.Apply();
        string path=Art+"/Slate grain.png";
        File.WriteAllBytes(path,texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var saved=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        foreach(var mat in new[]{slate,deepSlate,tile,tileDark})
        {
            mat.mainTexture=saved;
            EditorUtility.SetDirty(mat);
        }
    }

    static Material Material(string name,Color color,bool unlit=false)
    {
        var mat=new Material(Shader.Find(unlit?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));
        mat.color=color;
        if(!unlit)mat.SetFloat("_Smoothness",.17f);
        AssetDatabase.CreateAsset(mat,Art+"/"+name+".mat");
        return mat;
    }

    static Mesh MakeBoulder()
    {
        const int sides=8,rings=4;
        Vector3 Point(int ring,int i)
        {
            i%=sides;
            float angle=i*2*Mathf.PI/sides+Mathf.Sin(ring*2.3f)*.30f;
            float r=(ring is 0 or 3?.31f:.52f)*(1+Mathf.Sin(i*7.71f+ring*4.31f)*.20f);
            return new Vector3(Mathf.Cos(angle)*r,-.5f+ring/3f+Mathf.Sin(i*5.3f+ring)*.10f,Mathf.Sin(angle)*r);
        }
        var vertices=new List<Vector3>();
        void Face(Vector3 a,Vector3 b,Vector3 c)=>vertices.AddRange(new[]{a,b,c});
        for(int i=0;i<sides;i++)
        {
            for(int ring=0;ring<rings-1;ring++)
            {
                Face(Point(ring,i),Point(ring+1,i),Point(ring+1,i+1));
                Face(Point(ring,i),Point(ring+1,i+1),Point(ring,i+1));
            }
            Face(new Vector3(0,.49f,0),Point(rings-1,i+1),Point(rings-1,i));
            Face(new Vector3(0,-.5f,0),Point(0,i),Point(0,i+1));
        }
        var mesh=CreateMesh("Original faceted basalt",vertices,
            Enumerable.Range(0,vertices.Count).ToArray(),v=>new Vector2(v.x+v.z,v.y)*1.13f);
        return mesh;
    }

    static void Bake(Transform root,Vector3 arrival,IEnumerable<Vector3> starts)
    {
        var bounds=new Bounds(Origin+Vector3.up*5,new Vector3(76,24,85));
        var sources=new List<NavMeshBuildSource>();
        var blockers=root.GetComponentsInChildren<Collider>().Where(c=>c.gameObject.layer==9)
            .Select(c=>new NavMeshBuildMarkup{root=c.transform,overrideArea=true,area=1}).ToList();
        Physics.SyncTransforms();
        NavMeshBuilder.CollectSources(bounds,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,blockers,sources);
        var settings=NavMesh.GetSettingsByID(0);
        settings.agentRadius=.53f;
        settings.agentHeight=1.85f;
        settings.agentClimb=.37f;
        settings.overrideVoxelSize=true;
        settings.voxelSize=.15f;
        var data=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);
        if(data==null)throw new InvalidOperationException("Cavern NavMesh build failed.");
        AssetDatabase.CreateAsset(data,Art+"/Fourfold Hollow NavMesh.asset");
        root.gameObject.AddComponent<WorldNavigation>().Configure(data);
        var handle=NavMesh.AddNavMeshData(data);
        try
        {
            var path=new NavMeshPath();
            if(!NavMesh.SamplePosition(arrival,out var a,1,1) || starts.Any(point=>
                !NavMesh.SamplePosition(point,out var b,1,1) || !NavMesh.CalculatePath(a.position,b.position,1,path) ||
                path.status!=NavMeshPathStatus.PathComplete))
                throw new InvalidOperationException("A ritual site or arrival is disconnected.");
        }
        finally{handle.Remove();}
    }

    static void Paving(Transform parent)
    {
        var paving=Group(parent,"Worn paving",Vector3.zero);
        for(int row=-12;row<=11;row++)
        for(int col=-10;col<=10;col++)
        {
            float x=col*2.06f+(row%2)*.55f,z=row*2.12f;
            if(x*x/390+z*z/625>1 || x*x+(z-2)*(z-2)<44)continue;
            var cell=Part(paving,"Paver",PrimitiveType.Cube,new Vector3(x,.026f,z),
                new Vector3(1.99f,.056f,2.04f),(row*11+col*3)%7==0?tileDark:tile);
            cell.transform.localRotation=Quaternion.Euler(0,Mathf.Sin(row*11+col*83)*.86f,0);
        }
        var groups=paving.GetComponentsInChildren<MeshFilter>().GroupBy(m=>m.GetComponent<Renderer>().sharedMaterial).ToArray();
        foreach(var group in groups)
        {
            var mesh=new Mesh{name=group.Key.name+" floor",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
            mesh.CombineMeshes(group.Select(filter=>new CombineInstance
                {mesh=filter.sharedMesh,transform=paving.worldToLocalMatrix*filter.transform.localToWorldMatrix}).ToArray());
            AssetDatabase.CreateAsset(mesh,Art+"/"+mesh.name+".asset");
            Surface(paving,mesh.name,Vector3.zero,mesh,group.Key);
            foreach(var filter in group)Object.DestroyImmediate(filter.gameObject);
        }
    }

    static void Disk(Transform parent,string name,float y,float x,float z,Material material,bool walkable)
    {
        var vertices=new List<Vector3>{Vector3.zero};
        var triangles=new List<int>();
        const int sides=96;
        for(int i=0;i<sides;i++)
        {
            float angle=i*2*Mathf.PI/sides;
            float scale=1+Mathf.Sin(i*.57f)*.031f;
            vertices.Add(new Vector3(Mathf.Sin(angle)*x,0,Mathf.Cos(angle)*z)*scale);
            triangles.AddRange(new[]{0,i+1,(i+1)%sides+1});
        }
        var mesh=CreateMesh(name,vertices,triangles,v=>new Vector2(v.x,v.z)/4);
        var go=Surface(parent,name,Vector3.up*y,mesh,material).gameObject;
        if(!walkable)return;
        go.layer=8;
        go.AddComponent<MeshCollider>().sharedMesh=mesh;
    }

    static Mesh CreateMesh(string name,List<Vector3> vertices,IList<int> triangles,Func<Vector3,Vector2> uv)
    {
        var mesh=new Mesh{name=name};
        mesh.SetVertices(vertices);
        mesh.SetUVs(0,vertices.Select(uv).ToList());
        mesh.SetTriangles(triangles.ToArray(),0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,Art+"/"+name+".asset");
        return mesh;
    }

    static ShamanDungeonPortal Portal(Transform parent,string title,Vector3 local,ShamanDungeonPortal.Action action,float radius)
    {
        var node=Group(parent,title,local).gameObject;
        node.layer=11;
        var collider=node.AddComponent<CapsuleCollider>();
        collider.center=Vector3.up*1.25f;
        collider.height=2.5f;
        collider.radius=radius;
        var portal=node.AddComponent<ShamanDungeonPortal>();
        portal.action=action;
        portal.ConfigureName(title);
        return portal;
    }

    static Transform Group(Transform parent,string name,Vector3 local)
    {
        var group=new GameObject(name).transform;
        group.SetParent(parent,false);
        group.localPosition=local;
        return group;
    }

    static Transform Surface(Transform parent,string name,Vector3 p,Mesh mesh,Material material)
    {
        var go=Group(parent,name,p);
        go.gameObject.layer=2;
        go.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        go.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
        return go;
    }

    static GameObject Part(Transform root,string name,PrimitiveType shape,Vector3 p,Vector3 size,Material material,int layer=2)
    {
        var go=GameObject.CreatePrimitive(shape);
        go.name=name;
        go.layer=layer;
        go.transform.SetParent(root,false);
        go.transform.localPosition=p;
        go.transform.localScale=size;
        go.GetComponent<Renderer>().sharedMaterial=material;
        if(layer==2)Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static void Rock(Transform root,Vector3 p,Vector3 scale,float yaw,Material material)
    {
        var stone=Surface(root,material.name,p,boulder,material);
        stone.localScale=scale;
        stone.localRotation=Quaternion.Euler(Mathf.Sin(yaw*.2f)*9,yaw,Mathf.Sin(yaw*.14f)*7);
    }

    static void Crystal(Transform parent,Vector3 p,int element,float scale)
    {
        for(int i=0;i<5;i++)
        {
            float angle=i*2.4f;
            var at=p+new Vector3(Mathf.Sin(angle)*i*.13f,.59f*scale,Mathf.Cos(angle)*i*.13f);
            Rock(parent,at,new Vector3(.34f+i*.063f,1.42f-i*.18f,.39f)*scale,i*173+28,crystal[element]);
        }
    }

    static void Torch(Transform root,Vector3 p,int element)
    {
        var post=Group(root,Elements[element]+" offering brazier",p);
        Part(post,"Carved stone base",PrimitiveType.Cylinder,Vector3.up*.12f,new Vector3(.83f,.12f,.83f),slate);
        Part(post,"Bronze brazier stem",PrimitiveType.Cylinder,Vector3.up*.64f,new Vector3(.19f,.54f,.19f),bronze);
        Part(post,"Worked bronze dish",PrimitiveType.Cylinder,Vector3.up*1.27f,new Vector3(.91f,.16f,.91f),bronze);
        Rock(post,Vector3.up*1.66f,new Vector3(.41f,.62f,.44f),24,sigils[element]);
        PointLight(post,Vector3.up*1.82f,Tints[element],7.5f,1.6f);
    }

    static void PointLight(Transform root,Vector3 p,Color color,float range,float intensity)
    {
        var light=Group(root,"Ritual light",p).gameObject.AddComponent<Light>();
        light.type=LightType.Point;
        light.color=color;
        light.range=range;
        light.intensity=intensity;
    }

    static void Seal(Transform root,Vector3 p,float radius,int style)
    {
        var seal=Group(root,"Incised offering seal",p);
        int sides=style==2?8:12;
        for(int i=0;i<sides;i++)
        {
            float angle=(i+.5f)*2*Mathf.PI/sides;
            var direction=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));
            var band=Part(seal,"Bronze seal band",PrimitiveType.Cube,direction*(radius*Mathf.Cos(Mathf.PI/sides)),
                new Vector3(2*radius*Mathf.Sin(Mathf.PI/sides)+.018f,.032f,.085f),bronze);
            band.transform.localRotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg,0);
            var rune=Part(seal,"Lit glyph",PrimitiveType.Cube,direction*(radius*.71f)+Vector3.up*.018f,
                new Vector3(.075f+radius*.034f,.038f,radius*.22f),sigils[style]);
            rune.transform.localRotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg+28*((i%2)*2-1),0);
        }
    }

    static void Label(Transform root,string value,Vector3 p)
    {
        var text=Group(root,value,p).gameObject.AddComponent<TextMesh>();
        text.transform.rotation=Quaternion.Euler(52,45,0);
        text.gameObject.layer=2;
        text.text=value;
        text.anchor=TextAnchor.MiddleCenter;
        text.alignment=TextAlignment.Center;
        text.characterSize=.086f;
        text.fontSize=46;
        text.color=new Color(.96f,.79f,.53f);
    }
}

