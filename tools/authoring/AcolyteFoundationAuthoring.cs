using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public static partial class AcolyteFoundationAuthoring
{
    private const string Folder = "Assets/_DiceFree/Settings/Acolyte";
    private const string CatalogPath = "Assets/_DiceFree/Settings/Progression/Class catalog.asset";
    private const string DevelopmentCatalogPath = "Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset";
    private const string MenuPath = "Assets/_DiceFree/Resources/MenuPresentationCatalog.asset";
    private const string CornbergId = "work.cornberg.acolyte-remains";
    private const string TrimeryId = "work.trimery.acolyte-remains";
    private static Material cloth, leather, bronze, beacon;

    public static void Batch() => Debug.Log(Run());

    public static object Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Use clean Edit Mode.");
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();

        var classes = Load<ClassCatalog>(CatalogPath);
        if (classes.Resolve(AcolyteSkillProgression.AcolyteClassId) != null)
            throw new InvalidOperationException("The Acolyte is already installed.");
        var parent = classes.Resolve(AcolyteMentor.ParentClassId);
        var acolyte = Asset<ActorDefinition>("Acolyte class");
        var wand = Asset<AttackDefinition>("Acolyte wand basic");
        EditorUtility.CopySerialized(Load<AttackDefinition>("Assets/_DiceFree/Settings/Skills/Mage/Mage wand basic.asset"), wand);
        wand.stableId = "attack.acolyte.wand";
        wand.displayName = "Vigil Spark";
        acolyte.stableId = AcolyteSkillProgression.AcolyteClassId;
        acolyte.displayName = "Acolyte";
        acolyte.tuning = parent.tuning;
        acolyte.baseAttributes = new AttributeValues { vitality=12, strength=5, agility=7, intelligence=16, spirit=14 };
        acolyte.growth = new AttributeValues { vitality=1.5f, strength=.5f, agility=.75f, intelligence=2.5f, spirit=2 };
        acolyte.baseHp = 10;
        acolyte.moveSpeed = parent.moveSpeed;
        acolyte.basicAttack = wand;

        var element = Asset<ElementDefinition>("Void");
        element.stableId = "element.void";
        element.displayName = "Void";
        var drain = Spell("Drain Life","skill.acolyte.drain-life",element,4,.20f,.10f);
        var coil = Spell("Mortal Coil","skill.acolyte.mortal-coil",element,10,.60f,.30f);
        var drainIcon = SpellIcon("Drain Life icon",false);
        var coilIcon = SpellIcon("Mortal Coil icon",true);
        var mana = Asset<ClassResourceProfile>("Acolyte Mana");
        EditorUtility.CopySerialized(Load<ClassResourceProfile>("Assets/_DiceFree/Settings/Resources/Magically Touched Mana.asset"),mana);
        mana.stableId = "resource-profile.acolyte.mana";
        mana.classId = acolyte.stableId;
        var soul = Asset<ResourceDefinition>("Soul Charge");
        soul.stableId = AcolyteSkillProgression.SoulResourceId;
        soul.displayName = "Soul Charges";
        var soulProfile = Asset<ClassResourceProfile>("Acolyte Soul Charges");
        soulProfile.stableId = "resource-profile.acolyte.soul-charge";
        soulProfile.classId = acolyte.stableId;
        soulProfile.resource = soul;
        soulProfile.baseMaximum = 8;
        soulProfile.baseRegenPerSecond = 0;
        soulProfile.initialFraction = 0;
        soulProfile.loadPolicy = ResourceRetentionPolicy.ResetOnLoad;

        var trial = Trial();
        var edge = Asset<AdvancementDefinition>("Lanternrest Acolyte advancement");
        edge.stableId = "advance.magically-touched.acolyte";
        edge.displayName = "Acolyte";
        edge.sourceClass = parent;
        edge.targetClass = acolyte;
        edge.requiredLevel = 30;
        edge.requiredQuestId = trial.stableId;
        classes.Configure(classes.Classes.Append(acolyte).ToArray());
        var developmentClasses = Load<ClassCatalog>(DevelopmentCatalogPath);
        developmentClasses.Configure(developmentClasses.Classes.Append(acolyte).ToArray());
        EditorUtility.SetDirty(developmentClasses);
        foreach (var asset in new Object[]{wand,acolyte,element,mana,soul,soulProfile,edge,classes})
            EditorUtility.SetDirty(asset);

        cloth = Tint("Vigil linen",null,new Color(.53f,.49f,.41f));
        leather = Tint("Vigil leather",null,new Color(.22f,.15f,.16f));
        bronze = Tint("Vigil bronze",null,new Color(.64f,.44f,.24f));
        beacon = Tint("Vigil marker",null,new Color(.87f,.66f,.97f),true);
        InstallMenu(acolyte.stableId);
        AssetDatabase.SaveAssets();
        foreach (string name in new[]{"Cornberg","DevelopmentRoom"})
        {
            var scene = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/" + name + ".unity");
            // Single-scene loads unload unused native asset instances. Resolve the saved objects in each scene.
            classes = Load<ClassCatalog>(name == "DevelopmentRoom" ? DevelopmentCatalogPath : CatalogPath);
            acolyte = Load<ActorDefinition>(Folder + "/Acolyte class.asset");
            edge = Load<AdvancementDefinition>(Folder + "/Lanternrest Acolyte advancement.asset");
            trial = Load<QuestDefinition>(Folder + "/The Lanternrest Vigil.asset");
            mana = Load<ClassResourceProfile>(Folder + "/Acolyte Mana.asset");
            soulProfile = Load<ClassResourceProfile>(Folder + "/Acolyte Soul Charges.asset");
            drain = Load<AttackDefinition>(Folder + "/Drain Life.asset");
            coil = Load<AttackDefinition>(Folder + "/Mortal Coil.asset");
            drainIcon = Load<Texture2D>(Folder + "/Drain Life icon.asset");
            coilIcon = Load<Texture2D>(Folder + "/Mortal Coil icon.asset");
            var controller = Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();
            var player = controller.GetComponent<CombatActor>();
            controller.Configure(classes, controller.Definitions.Append(edge).ToArray());
            player.GetComponent<ManifestationPersistence>().ConfigureClassCatalog(classes);
            var resources = player.GetComponent<ActorResourceController>();
            resources.Configure(resources.Profiles.Concat(new[]{mana,soulProfile}).ToArray());
            player.gameObject.AddComponent<AcolyteSkillCaster>().Configure(drain,coil);
            player.gameObject.AddComponent<AcolyteAbilityProvider>().Configure(drainIcon,coilIcon);
            player.gameObject.AddComponent<SoulChargeHud>();
            InstallVisual(player, acolyte.stableId);
            if (name == "Cornberg") InstallVigil(player,trial,edge);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save "+name);
        }
        AssetDatabase.SaveAssets();
        return new { acolyte = acolyte.stableId, trial = trial.stableId, sites = 3, spells = AcolyteSkillProgression.SkillCount };
    }

    private static T Load<T>(string path) where T : Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("Missing asset " + path);

    private static T Asset<T>(string name) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(Folder + "/" + name + ".asset");
        if (existing != null) return existing;
        var value = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(value, Folder + "/" + name + ".asset");
        return value;
    }

    private static AttackDefinition Spell(string title,string id,ElementDefinition element,float flat,float intelligence,float spirit)
    {
        var attack = Asset<AttackDefinition>(title);
        attack.stableId = id;
        attack.displayName = title;
        attack.channel = DamageChannel.Magical;
        attack.element = element;
        attack.requiresAccuracy = false;
        attack.scaling = AttributeScaling.Weighted;
        attack.weights = new AttributeValues { intelligence = intelligence, spirit = spirit };
        attack.baseDamage = flat;
        attack.coefficient = 1;
        attack.reach = AcolyteSkillCaster.Range;
        EditorUtility.SetDirty(attack);
        return attack;
    }

    private static QuestDefinition Trial()
    {
        var quest = Asset<QuestDefinition>("The Lanternrest Vigil");
        quest.stableId = AcolyteMentor.QuestId;
        quest.title = "The Lanternrest Vigil";
        quest.requiredClassId = AcolyteMentor.ParentClassId;
        quest.requiredLevel = 30;
        quest.offer = "Two travelers died far from their kin. Their remains have been prepared in Cornberg and Trimery. Bring them to Lanternrest and light their candles at the burial stone. Then we can speak of the Acolyte's Way.";
        quest.activeDialogue = "Find one wrapped bundle near the magical well in Cornberg and one near the healer in Trimery. Carry both to the stone bier at Lanternrest and complete the burial rite.";
        quest.locationHint = "Cornberg magical well and Trimery healer. Bring both bundles to Lanternrest Cemetery in southwest Arborium.";
        quest.turnInHint = "Speak to Mara at Lanternrest after the burial rite.";
        quest.stages = new[]
        {
            new QuestObjective { kind = ObjectiveKind.All, count = 2, alternatives = new[]
            {
                new QuestLeaf { kind=ObjectiveKind.Interact, objectiveId="cornberg-remains", contentId=CornbergId,
                    instruction="Collect the prepared remains near Cornberg's magical well" },
                new QuestLeaf { kind=ObjectiveKind.Interact, objectiveId="trimery-remains", contentId=TrimeryId,
                    instruction="Collect the prepared remains near the Trimery healer" }
            }},
            new QuestObjective { kind=ObjectiveKind.Interact, contentId=AcolyteBurialGate.RiteId,
                instruction="Lay the remains to rest and light the candles at Lanternrest" }
        };
        EditorUtility.SetDirty(quest);
        return quest;
    }
}

public static partial class AcolyteFoundationAuthoring
{
    private static void InstallVigil(CombatActor player,QuestDefinition quest,AdvancementDefinition edge)
    {
        var journal = player.GetComponent<QuestJournal>();
        journal.Configure(journal.Definitions.Append(quest).ToArray());
        var ambient = Object.FindObjectsByType<ConversationTarget>(FindObjectsInactive.Include)
            .Single(target => target.NpcId == "npc.arborium.cemetery-keeper");
        var keeper = ambient.gameObject;
        var approach = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Single(t=>t.name=="Keeper approach");
        var mentor = keeper.AddComponent<AcolyteMentor>();
        mentor.Configure(quest,edge);
        mentor.ConfigureName("Mara, Acolyte mentor");
        mentor.ConfigureApproach(approach);
        var npc = keeper.AddComponent<ContextualNpc>();
        npc.ConfigureName("Mara, Cemetery Keeper");
        npc.ConfigureApproach(approach);
        npc.Configure(ambient.NpcId,mentor,ambient);
        keeper.AddComponent<InteractionPrompt>().Configure(npc,player);

        var sites = new GameObject("Lanternrest Vigil - advancement sites").transform;
        var navigation = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            Physics.SyncTransforms();
            AddBundle(sites,player,quest,new Vector3(3.6f,0,5.2f),"Cornberg","cornberg-remains",CornbergId);
            AddBundle(sites,player,quest,new Vector3(243,0,185.4f),"Trimery","trimery-remains",TrimeryId);
            var cemetery = keeper.transform.parent;
            var burial = Work(sites,"Lanternrest burial bier",cemetery.TransformPoint(new Vector3(0,.1f,5.72f)),
                AcolyteBurialGate.RiteId,"Holding the burial vigil",4f,
                GroundNav(cemetery.TransformPoint(new Vector3(0,0,4.36f))));
            burial.transform.rotation = cemetery.rotation;
            burial.transform.Find("Safe approach").position = GroundNav(cemetery.TransformPoint(new Vector3(0,0,4.36f)));
            var box = burial.GetComponent<BoxCollider>();
            box.center = new Vector3(0,.68f,0);
            box.size = new Vector3(2.65f,1.24f,1.10f);
            var flame = Node(burial.transform,"Vigil flames after burial",Vector3.zero);
            for (int side=-1;side<=1;side+=2)
            {
                var candle = new Vector3(side*.63f,.97f,-.09f);
                Shape(burial.transform,"Unlit vigil candle",PrimitiveType.Cylinder,candle, new Vector3(.115f,.12f,.115f),cloth);
                Shape(flame,"Lit vigil candle",PrimitiveType.Sphere,candle+Vector3.up*.17f,
                    new Vector3(.076f,.17f,.073f),beacon);
            }
            flame.gameObject.SetActive(false);
            burial.gameObject.AddComponent<AcolyteBurialGate>().Configure(journal,flame.gameObject);
        }
        finally { navigation.Remove(); }
    }

    private static Vector3 GroundNav(Vector3 preferred)
    {
        if (!NavMesh.SamplePosition(preferred,out var hit,1.2f,1))
            throw new InvalidOperationException("The vigil interaction needs a walkable approach near " + preferred);
        return hit.position;
    }

    private static void AddBundle(Transform root, CombatActor player, QuestDefinition quest, Vector3 preferred,
        string village, string objectiveId, string workId)
    {
        Vector3 approach = GroundNav(preferred);
        var spot = approach + new Vector3(.68f,0,.54f);
        if (Physics.CheckBox(spot+Vector3.up*.5f,new Vector3(.65f,.47f,.85f),Quaternion.identity,1<<9))
            throw new InvalidOperationException("The bundle overlaps an existing obstruction in "+village);
        var work = Work(root,village+" prepared remains",spot,workId,"Gathering the prepared remains",2.3f,approach);
        var litter = Node(work.transform,"Covered burial bundle",Vector3.zero);
        Shape(litter,"Timber litter",PrimitiveType.Cube,new Vector3(0,.16f,0),new Vector3(.86f,.13f,1.43f),leather);
        Shape(litter,"Wrapped linen",PrimitiveType.Capsule,new Vector3(0,.34f,0),new Vector3(.58f,.69f,.46f),cloth).localRotation=Quaternion.Euler(90,0,0);
        for(int i=0;i<3;i++)
            Shape(litter,"Respectfully tied linen strap",PrimitiveType.Cube,new Vector3(0,.48f,-.30f+i*.30f),
                new Vector3(.56f,.057f,.082f),leather);
        var memorial = Node(work.transform,"Empty litter and votive",Vector3.zero);
        Shape(memorial,"Empty timber litter",PrimitiveType.Cube,new Vector3(0,.13f,0),new Vector3(.86f,.13f,1.43f),leather);
        Shape(memorial,"Memorial candle",PrimitiveType.Cylinder,new Vector3(.25f,.26f,.39f),new Vector3(.09f,.085f,.09f),cloth);
        Shape(memorial,"Memorial flame",PrimitiveType.Sphere,new Vector3(.25f,.39f,.39f),new Vector3(.052f,.11f,.052f),beacon);
        memorial.gameObject.SetActive(false);
        var marker = Node(work.transform,"Vigil marker",Vector3.up*1.65f);
        Shape(marker,"Floating lavender beacon",PrimitiveType.Cube,Vector3.zero,Vector3.one*.23f,beacon).localRotation=Quaternion.Euler(26,45,45);
        marker.gameObject.SetActive(false);
        var gate = work.gameObject.AddComponent<QuestWorkGate>();
        gate.Configure(quest,objectiveId);
        work.gameObject.AddComponent<GuildWorkSitePresentation>().Configure(player,gate,litter.gameObject,memorial.gameObject,marker.gameObject);
    }

    private static TimedInteractionTarget Work(Transform parent,string name,Vector3 world,string id,string verb,float duration,Vector3 approach)
    {
        var root = Node(parent,name,world);
        root.gameObject.layer = 11;
        var box = root.gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3(0,.38f,0);
        box.size = new Vector3(.91f,.77f,1.51f);
        var target = root.gameObject.AddComponent<TimedInteractionTarget>();
        target.Configure(id,verb,duration);
        target.ConfigureName(name);
        var where = Node(root,"Safe approach",Vector3.zero);
        where.position = approach;
        target.ConfigureApproach(where);
        return target;
    }

    private static Transform Node(Transform parent,string name,Vector3 position)
    {
        var result = new GameObject(name).transform;
        result.SetParent(parent,false);
        result.localPosition = position;
        return result;
    }

    private static Transform Shape(Transform parent,string name,PrimitiveType type,Vector3 point,Vector3 size,Material material)
    {
        var go = GameObject.CreatePrimitive(type);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.name = name;
        go.transform.SetParent(parent,false);
        go.transform.localPosition = point;
        go.transform.localScale = size;
        go.GetComponent<MeshRenderer>().sharedMaterial = material;
        return go.transform;
    }
}

public static partial class AcolyteFoundationAuthoring
{
    private static Material Tint(string title,Material original,Color color,bool unlit=false)
    {
        string path = Folder + "/" + title + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        material = original != null ? new Material(original) :
            new Material(Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit"));
        material.SetColor("_BaseColor",color);
        AssetDatabase.CreateAsset(material,path);
        return material;
    }

    private static void AcolytePalette(GameObject model)
    {
        foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            renderer.sharedMaterials = renderer.sharedMaterials.Select(material => material == null ? null : material.name switch
            {
                "T1_Indigo" => Tint("Acolyte mulberry mantle",material,new Color(.28f,.19f,.33f)),
                "T1_BlueCloth" => Tint("Acolyte violet wool",material,new Color(.37f,.25f,.43f)),
                "T1_WovenSilver" => Tint("Acolyte bronze embroidery",material,new Color(.66f,.49f,.27f)),
                "T1_FocusGem" => Tint("Acolyte violet focus",material,new Color(.65f,.42f,.91f)),
                _ => material
            }).ToArray();
    }

    private static void InstallVisual(CombatActor actor,string classId)
    {
        var switcher = actor.GetComponent<ClassPresentationSwitcher>();
        var source = switcher.Resolve(AcolyteMentor.ParentClassId);
        var visual = Object.Instantiate(source.gameObject,source.parent);
        if (PrefabUtility.IsPartOfPrefabInstance(visual))
            PrefabUtility.UnpackPrefabInstance(visual,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        visual.name = "Acolyte vigil robes";
        AcolytePalette(visual);

        var originals = source.GetComponentsInChildren<Transform>(true);
        var clones = visual.GetComponentsInChildren<Transform>(true);
        if (originals.Length != clones.Length) throw new InvalidOperationException("The Acolyte visual hierarchy changed during cloning.");
        var map = originals.Zip(clones,(a,b)=>(a.gameObject,b.gameObject)).ToDictionary(pair=>pair.Item1,pair=>pair.Item2);
        var equipment = actor.GetComponent<EquipmentPresentation>();
        if (equipment != null)
        {
            GameObject[] Remap(GameObject[] values) => (values ?? Array.Empty<GameObject>())
                .Where(value=>value!=null&&map.ContainsKey(value)).Select(value=>map[value]).ToArray();
            var shared = new List<EquipmentPresentation.Binding>();
            foreach (var binding in equipment.Bindings)
            {
                var visuals = Remap(binding.visuals);
                var baseline = Remap(binding.baselineVisuals);
                if (visuals.Length == 0 && baseline.Length == 0) continue;
                shared.Add(new EquipmentPresentation.Binding { slot=binding.slot, definition=binding.definition,
                    visuals=visuals, baselineVisuals=baseline });
            }
            equipment.Configure(equipment.Bindings.Concat(shared).ToArray());
        }

        var serial = new SerializedObject(switcher);
        var bindings = serial.FindProperty("bindings");
        int index = bindings.arraySize;
        bindings.InsertArrayElementAtIndex(index);
        var added = bindings.GetArrayElementAtIndex(index);
        added.FindPropertyRelative("classId").stringValue = classId;
        added.FindPropertyRelative("visualRoot").objectReferenceValue = visual.transform;
        added.FindPropertyRelative("animator").objectReferenceValue = ClassPresentationSwitcher.FindVisualAnimator(visual.transform);
        serial.ApplyModifiedPropertiesWithoutUndo();
        visual.SetActive(false);
    }

    private static void InstallMenu(string classId)
    {
        var catalog = Load<MenuPresentationCatalog>(MenuPath);
        var parent = catalog.Resolve(AcolyteMentor.ParentClassId)?.preview;
        if (parent == null) throw new InvalidOperationException("Missing magical parent menu art.");
        var copy = Object.Instantiate(parent);
        copy.name = "Acolyte menu presentation";
        AcolytePalette(copy);
        var asset = PrefabUtility.SaveAsPrefabAsset(copy,Folder + "/Acolyte menu presentation.prefab");
        Object.DestroyImmediate(copy);
        if (asset == null) throw new InvalidOperationException("Could not save Acolyte menu presentation.");
        catalog.forms = catalog.forms.Append(new MenuPresentationCatalog.Form { classId=classId, tier=2, preview=asset }).ToArray();
        EditorUtility.SetDirty(catalog);
    }

    private static Texture2D SpellIcon(string title,bool coil)
    {
        const int size=64;
        string path = Folder + "/" + title + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var texture = existing ?? new Texture2D(size,size,TextureFormat.RGBA32,false) { name=title, wrapMode=TextureWrapMode.Clamp };
        var pixels=new Color[size*size];
        for(int y=0;y<size;y++)
        for(int x=0;x<size;x++)
        {
            var p=new Vector2((x+.5f)/size*2-1,(y+.5f)/size*2-1);
            float r=p.magnitude;
            float a=Mathf.Atan2(p.y,p.x);
            float shape;
            if(coil)
            {
                float winding=Mathf.Abs(Mathf.Sin(a*1.5f+r*10.5f));
                shape=Mathf.Exp(-9f*winding*winding)*Mathf.Clamp01((.88f-r)*5);
            }
            else
            {
                float trail=Mathf.Abs(p.y-.46f*Mathf.Sin(p.x*2.8f));
                float wave=Mathf.Abs(p.y+.22f-.25f*Mathf.Sin(p.x*3.7f));
                shape=Mathf.Max(Mathf.Exp(-97f*trail*trail),.5f*Mathf.Exp(-120f*wave*wave))*Mathf.Clamp01((.93f-r)*4);
            }
            float center=Mathf.Exp(-13f*(p-new Vector2(coil?0:.38f,coil?0:.40f)).sqrMagnitude);
            float halo=Mathf.Exp(-3.3f*r*r);
            Color color = new Color(.055f,.027f,.10f) + new Color(.21f,.07f,.34f)*halo +
                (coil ? new Color(.41f,.41f,.87f) : new Color(.67f,.31f,.92f))*shape +
                new Color(.39f,.32f,.55f)*center;
            color.a=1;
            pixels[y*size+x]=color;
        }
        texture.SetPixels(pixels);
        texture.Apply(false);
        if (existing == null) AssetDatabase.CreateAsset(texture,path);
        else EditorUtility.SetDirty(texture);
        return texture;
    }
}
