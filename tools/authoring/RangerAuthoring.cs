using System;
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

public static class RangerAuthoring
{
    private const string Data = "Assets/_DiceFree/Settings/Ranger/";
    private const string Art = "Assets/_DiceFree/Art/Characters/IronpineRanger/";
    private const string Motion = "Assets/_DiceFree/Resources/PlayerMotion/TierTwo/Ranger.controller";
    private const string Catalog = "Assets/_DiceFree/Settings/Progression/Class catalog.asset";
    private const string Development = "Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset";
    private const string Menu = "Assets/_DiceFree/Resources/MenuPresentationCatalog.asset";
    private const string Model = Art + "IronpineRanger.fbx";
    private const string QuestTitle = "The Ironpine Hunt";

    public static void Batch()
    {
        Directory.CreateDirectory(Data);
        AssetDatabase.Refresh();
        var controller = PriestCompleteAuthoring.PrepareArt(Model, Art, Motion, Art + "IronpineRangerImport.json");
        var catalog = Load<ClassCatalog>(Catalog);
        var development = Load<ClassCatalog>(Development);
        var parent = catalog.Resolve(RangerMentor.ParentClassId);
        var ranger = Asset<ActorDefinition>("Ranger class");
        var basic = Attack("Ranger bow basic", "attack.ranger.bow", true);
        basic.baseDamage = 5;
        basic.weights = new AttributeValues { agility = 1.05f };
        basic.reach = 11f;
        basic.interval = 1.42f;
        basic.windup = .47f;
        ranger.stableId = RangerSkillProgression.RangerClassId;
        ranger.displayName = "Ranger";
        ranger.tuning = parent.tuning;
        ranger.baseAttributes = new AttributeValues { vitality = 13, strength = 8, agility = 18, intelligence = 7, spirit = 9 };
        ranger.growth = new AttributeValues { vitality = 1.5f, strength = .7f, agility = 3.1f, intelligence = .8f, spirit = 1.15f };
        ranger.baseHp = 10;
        ranger.moveSpeed = parent.moveSpeed;
        ranger.basicAttack = basic;

        var mana = Asset<ClassResourceProfile>("Ranger Mana");
        EditorUtility.CopySerialized(Load<ClassResourceProfile>("Assets/_DiceFree/Settings/Resources/Physically Blessed Mana.asset"), mana);
        mana.stableId = "resource-profile.ranger.mana";
        mana.classId = ranger.stableId;

        var gun = Asset<ElementDefinition>("Gun");
        gun.stableId = "element.gun";
        gun.displayName = "Gun";
        var aimed = Attack("Aimed Shot", "skill.ranger.aimed-shot", true);
        var explosive = Attack("Exploding Arrow", "skill.ranger.exploding-arrow");
        explosive.element = gun;
        var rain = Attack("Arrow Rain", "skill.ranger.arrow-rain");
        var hawk = Attack("Hawk Dive", "skill.ranger.hawk-dive");
        var icons = Enumerable.Range(0, 6).Select(i => Icon(RangerSkillProgression.Name((RangerSkill)i), i)).ToArray();
        var bows = new[] { Bow(false), Bow(true) };
        var quest = AuthorQuest();
        var edge = Asset<AdvancementDefinition>("Ironpine Hunt advancement");
        edge.stableId = "advance.physically-blessed.ranger";
        edge.displayName = "Ranger";
        edge.sourceClass = parent;
        edge.targetClass = ranger;
        edge.requiredLevel = 30;
        edge.requiredQuestId = quest.stableId;
        foreach (var classes in new[] { catalog, development })
        {
            classes.Configure(classes.Classes.Where(value => value.stableId != ranger.stableId).Append(ranger).ToArray());
            EditorUtility.SetDirty(classes);
        }
        foreach (var data in new Object[] { ranger, basic, mana, gun, explosive, quest, edge }) EditorUtility.SetDirty(data);
        AddMenu(ranger, controller, bows);
        AssetDatabase.SaveAssets();

        foreach (string sceneName in new[] { "Cornberg", "DevelopmentRoom" })
        {
            bool overworld = sceneName == "Cornberg";
            var scene = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/" + sceneName + ".unity");
            var advancement = Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();
            var player = advancement.GetComponent<CombatActor>();
            var classes = Load<ClassCatalog>(overworld ? Catalog : Development);
            edge = Load<AdvancementDefinition>(Data + "Ironpine Hunt advancement.asset");
            mana = Load<ClassResourceProfile>(Data + "Ranger Mana.asset");
            aimed = Load<AttackDefinition>(Data + "Aimed Shot.asset");
            explosive = Load<AttackDefinition>(Data + "Exploding Arrow.asset");
            rain = Load<AttackDefinition>(Data + "Arrow Rain.asset");
            hawk = Load<AttackDefinition>(Data + "Hawk Dive.asset");
            quest = Load<QuestDefinition>(Data + QuestTitle + ".asset");
            bows = new[] { Load<ItemDefinition>(Data + "Ironpine Scout Shortbow.asset"), Load<ItemDefinition>(Data + "Ironpine Yew Longbow.asset") };
            icons = Enumerable.Range(0, 6).Select(i => Load<Texture2D>(Data + RangerSkillProgression.Name((RangerSkill)i) + " icon.asset")).ToArray();
            controller = Load<UnityEditor.Animations.AnimatorController>(Motion);
            advancement.Configure(classes, advancement.Definitions.Where(value => value != edge).Append(edge).ToArray());
            player.GetComponent<ManifestationPersistence>().ConfigureClassCatalog(classes);
            var resources = player.GetComponent<ActorResourceController>();
            resources.Configure(resources.Profiles.Where(value => value != mana).Append(mana).ToArray());
            var caster = player.GetComponent<RangerSkillCaster>() ?? player.gameObject.AddComponent<RangerSkillCaster>();
            caster.Configure(aimed, explosive, rain, hawk);
            var provider = player.GetComponent<RangerAbilityProvider>() ?? player.gameObject.AddComponent<RangerAbilityProvider>();
            provider.Configure(icons);
            AddInventory(player.GetComponent<CarriedInventory>(), bows);
            AddVisual(player, controller);
            if (overworld) AddTrial(player, quest, edge, bows, controller);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save " + sceneName);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("IRONPINE_RANGER_AUTHOR_OK");
    }

    private static AttackDefinition Attack(string title, string id, bool accuracy = false)
    {
        var attack = Asset<AttackDefinition>(title);
        attack.stableId = id;
        attack.displayName = title;
        attack.channel = DamageChannel.Physical;
        attack.requiresAccuracy = accuracy;
        attack.scaling = AttributeScaling.Weighted;
        attack.baseDamage = 0;
        attack.coefficient = 1;
        EditorUtility.SetDirty(attack);
        return attack;
    }

    private static QuestDefinition AuthorQuest()
    {
        var quest = Asset<QuestDefinition>(QuestTitle);
        quest.stableId = RangerMentor.QuestId;
        quest.title = QuestTitle;
        quest.requiredClassId = RangerMentor.ParentClassId;
        quest.requiredLevel = 30;
        quest.offer = "The Alpha Wolf nearly took my bow arm. The healer can mend it, but that wolf is still on the Ironpine trail. Bring it down and return to me. I'll teach you how we hunt.";
        quest.activeDialogue = "Head to Ironpine and defeat the Alpha Wolf out in the woods. Come back to me here by the priest in Trimery when the trail is clear.";
        quest.locationHint = "Find the level 20 Alpha Wolf out in Ironpine.";
        quest.turnInHint = "Return to Ilyra, the wounded Ranger by Trimery's priest.";
        quest.stages = new[] { new QuestObjective
        {
            kind = ObjectiveKind.Kill, objectiveId = "ironpine-alpha", familyId = "ironpine.alpha-wolf",
            instruction = "Defeat the Alpha Wolf in Ironpine", count = 1
        } };
        return quest;
    }

    private static void AddTrial(CombatActor player, QuestDefinition trial, AdvancementDefinition edge,
        ItemDefinition[] bows, RuntimeAnimatorController controller)
    {
        var journal = player.GetComponent<QuestJournal>();
        journal.Configure(journal.Definitions.Where(value => value != trial).Append(trial).ToArray());
        var trader = Object.FindObjectsByType<ItemShop>(FindObjectsInactive.Include).Single(shop => shop.DisplayName == "Trimery lumber trader");
        trader.Configure(trader.Offers.Where(offer => !bows.Contains(offer.item))
            .Concat(bows.Select((item, i) => new ItemShop.Offer { item = item, goldPrice = i == 0 ? 22 : 31 })).ToArray());
        EditorUtility.SetDirty(trader);

        var priest = Object.FindObjectsByType<HealerInteraction>(FindObjectsInactive.Include)
            .Single(healer => healer.DisplayName == "Trimery Priest - free healing");
        var navigation = NavMesh.AddNavMeshData(Object.FindAnyObjectByType<WorldNavigation>().Data);
        try
        {
            Physics.SyncTransforms();
            Vector3 facing = Vector3.ProjectOnPlane(priest.ApproachPosition - priest.transform.position, Vector3.up).normalized;
            if (facing.sqrMagnitude < .1f) facing = Vector3.forward;
            var side = Vector3.Cross(Vector3.up, facing);
            var existing = GameObject.Find("The Ironpine Hunt - Trimery Ranger");
            if (existing != null) Object.DestroyImmediate(existing);
            var root = new GameObject("The Ironpine Hunt - Trimery Ranger");
            var mentor = root.transform;
            mentor.position = OpenPoint(priest.ApproachPosition + side * 4f + facing * .8f);
            mentor.rotation = Quaternion.LookRotation(facing);
            root.layer = 11;
            var capsule = root.AddComponent<CapsuleCollider>();
            capsule.radius = .42f;
            capsule.height = 1.8f;
            capsule.center = Vector3.up * .9f;
            AttachModel(mentor, controller, false);
            var talk = root.AddComponent<RangerMentor>();
            talk.Configure(trial, edge);
            talk.ConfigureName("Ilyra, Wounded Ranger");
            var stand = new GameObject("Ilyra approach").transform;
            stand.SetParent(mentor, false);
            stand.position = OpenPoint(mentor.position + facing * 1.45f);
            talk.ConfigureApproach(stand);
            root.AddComponent<InteractionPrompt>().Configure(talk, player);
            Debug.Log($"IRONPINE_RANGER_MENTOR {mentor.position} APPROACH {stand.position}");
        }
        finally { navigation.Remove(); }
    }

    private static Vector3 OpenPoint(Vector3 wanted)
    {
        for (int ring = 0; ring <= 6; ring++)
            for (int direction = 0; direction < (ring == 0 ? 1 : 8); direction++)
            {
                Vector3 p = wanted + Quaternion.Euler(0, direction * 45, 0) * Vector3.forward * ring * .8f;
                if (NavMesh.SamplePosition(p, out var hit, .8f, 1) && Mathf.Abs(hit.position.y - wanted.y) < 1 &&
                    !Physics.CheckCapsule(hit.position + Vector3.up * .32f, hit.position + Vector3.up * 1.34f, .30f, 1 << 9))
                    return hit.position;
            }
        throw new InvalidOperationException("No open spot for the Ranger near " + wanted);
    }

    private static void AddVisual(CombatActor player, RuntimeAnimatorController motion)
    {
        var switcher = new SerializedObject(player.GetComponent<ClassPresentationSwitcher>());
        var bindings = switcher.FindProperty("bindings");
        int index = Enumerable.Range(0, bindings.arraySize).Where(i =>
            bindings.GetArrayElementAtIndex(i).FindPropertyRelative("classId").stringValue == RangerSkillProgression.RangerClassId).DefaultIfEmpty(-1).First();
        if (index < 0)
        {
            index = bindings.arraySize;
            bindings.InsertArrayElementAtIndex(index);
        }
        else Object.DestroyImmediate(bindings.GetArrayElementAtIndex(index).FindPropertyRelative("visualRoot").objectReferenceValue is Transform old
            ? old.gameObject : null);
        var visual = new GameObject("Ironpine Ranger").transform;
        visual.SetParent(player.transform, false);
        var animator = AttachModel(visual, motion, false);
        visual.gameObject.SetActive(false);
        var entry = bindings.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("classId").stringValue = RangerSkillProgression.RangerClassId;
        entry.FindPropertyRelative("visualRoot").objectReferenceValue = visual;
        entry.FindPropertyRelative("animator").objectReferenceValue = animator;
        switcher.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddMenu(ActorDefinition ranger, RuntimeAnimatorController motion, ItemDefinition[] bows)
    {
        var catalog = Load<MenuPresentationCatalog>(Menu);
        var root = new GameObject("Ranger menu presentation");
        try
        {
            root.AddComponent<ActorStats>().Configure(ranger);
            root.AddComponent<CarriedInventory>().Configure(catalog.items.Except(bows).Concat(bows).ToArray());
            root.AddComponent<Equipment>();
            root.AddComponent<EquipmentPresentation>();
            var visual = new GameObject("Class visual").transform;
            visual.SetParent(root.transform, false);
            AttachModel(visual, motion, true);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Data + "Ranger menu presentation.prefab");
            catalog.forms = catalog.forms.Where(form => form.classId != ranger.stableId)
                .Append(new MenuPresentationCatalog.Form { classId = ranger.stableId, tier = 2, preview = prefab }).ToArray();
            catalog.items = catalog.items.Except(bows).Concat(bows).ToArray();
            EditorUtility.SetDirty(catalog);
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static Animator AttachModel(Transform parent, RuntimeAnimatorController motion, bool portrait)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(Model), parent);
        var animator = instance.GetComponent<Animator>();
        animator.runtimeAnimatorController = motion;
        animator.applyRootMotion = false;
        var skins = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (animator.avatar == null || !animator.avatar.isValid || skins.Length != 3)
            throw new InvalidOperationException("Expected a Wood Elf body and two rigged yew bows.");
        if (portrait) foreach (var skin in skins) skin.updateWhenOffscreen = true;
        parent.gameObject.AddComponent<RangerBowPresentation>().Configure(
            skins.Single(skin => skin.name.Contains("Shortbow")).gameObject,
            skins.Single(skin => skin.name.Contains("Longbow")).gameObject);
        return animator;
    }

    private static void AddInventory(CarriedInventory inventory, ItemDefinition[] bows)
    {
        var definitions = new SerializedObject(inventory).FindProperty("definitions");
        foreach (var bow in bows)
        {
            bool exists = Enumerable.Range(0, definitions.arraySize)
                .Any(i => definitions.GetArrayElementAtIndex(i).objectReferenceValue == bow);
            if (exists) continue;
            int index = definitions.arraySize;
            definitions.InsertArrayElementAtIndex(index);
            definitions.GetArrayElementAtIndex(index).objectReferenceValue = bow;
        }
        definitions.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static ItemDefinition Bow(bool longbow)
    {
        var item = Asset<ItemDefinition>(longbow ? "Ironpine Yew Longbow" : "Ironpine Scout Shortbow");
        item.stableId = longbow ? RangerBowPresentation.LongbowId : RangerBowPresentation.ShortbowId;
        item.displayName = item.name;
        item.sourceId = "shop.trimery.lumber";
        item.slot = EquipmentSlot.MainHand;
        item.handsRequired = 2;
        item.itemLevel = 1;
        item.allowedClassIds = new[] { RangerSkillProgression.RangerClassId };
        item.flavor = longbow ? "Laminated heartwood trades a slower draw for a heavier arrow." :
            "Ironpine scouts favor this quick-drawing bow for moving between shots.";
        item.stats = new EquipmentStats { attackSpeedPercent = longbow ? 0 : 20, basicAttackFlatBonus = longbow ? 12 : 5 };
        var icon = Icon(item.displayName, longbow ? 7 : 6);
        var sprite = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(icon)).OfType<Sprite>().FirstOrDefault();
        if (sprite == null)
        {
            sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), new Vector2(.5f, .5f));
            sprite.name = item.displayName;
            AssetDatabase.AddObjectToAsset(sprite, icon);
        }
        item.icon = sprite;
        EditorUtility.SetDirty(item);
        return item;
    }

    public static void RefreshIcons()
    {
        for (int i = 0; i < RangerSkillProgression.SkillCount; i++)
            Icon(RangerSkillProgression.Name((RangerSkill)i), i);
        Icon("Ironpine Scout Shortbow", 6);
        Icon("Ironpine Yew Longbow", 7);
        AssetDatabase.SaveAssets();
        Debug.Log("IRONPINE_RANGER_ICONS_OK");
    }

    private static float Fade(float near, float far, float distance) =>
        1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(near, far, distance));

    private static Texture2D Icon(string name, int motif)
    {
        string path = Data + name + " icon.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        const int size = 80;
        var icon = existing ?? new Texture2D(size, size, TextureFormat.RGBA32, false) { name = name, filterMode = FilterMode.Bilinear };
        var colors = new[] { new Color(.91f,.79f,.41f), new Color(1f,.48f,.21f), new Color(.66f,.86f,.55f),
            new Color(.94f,.72f,.46f), new Color(.63f,.88f,.74f), new Color(.86f,.79f,.48f),
            new Color(.88f,.66f,.37f), new Color(.93f,.74f,.46f) };
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new((x + .5f) / size * 2 - 1, (y + .5f) / size * 2 - 1);
                float Segment(float ax, float ay, float bx, float by, float width = .042f)
                {
                    Vector2 a = new(ax, ay), b = new(bx, by);
                    float d = Vector2.Distance(p, a + (b - a) * Mathf.Clamp01(Vector2.Dot(p - a, b - a) / (b - a).sqrMagnitude));
                    return Fade(width * .48f, width, d);
                }
                float Glyph(params Vector2[] points)
                {
                    float value = 0;
                    for (int i = 1; i < points.Length; i++)
                        value = Mathf.Max(value, Segment(points[i-1].x, points[i-1].y, points[i].x, points[i].y));
                    return value;
                }
                float Arrow(float px, float py, float dx, float dy)
                {
                    float tipX = px + dx, tipY = py + dy;
                    return Mathf.Max(Segment(px, py, tipX, tipY),
                        Glyph(new(tipX - dx * .35f - dy * .20f, tipY - dy * .35f + dx * .20f), new(tipX, tipY),
                            new(tipX - dx * .35f + dy * .20f, tipY - dy * .35f - dx * .20f)));
                }
                float radius = p.magnitude;
                float ring = Fade(.014f, .036f, Mathf.Abs(radius - .54f));
                float bow = Glyph(new(-.49f,-.65f), new(-.61f,-.43f), new(-.49f,-.15f), new(-.22f,.13f),
                    new(.07f,.36f), new(.43f,.53f), new(.70f,.43f));
                float line = Segment(-.49f,-.65f,.70f,.43f,.022f);
                float shape = motif switch
                {
                    0 => Mathf.Max(Arrow(-.61f,-.61f,1.17f,1.20f), ring * .56f),
                    1 => Mathf.Max(Arrow(-.65f,-.62f,.79f,.79f), Mathf.Clamp01(1 - Vector2.Distance(p, new(.33f,.37f)) *
                        (1.35f + 4 * Mathf.Abs(Mathf.Sin(Mathf.Atan2(p.y - .37f, p.x - .33f) * 5))))),
                    2 => Mathf.Max(Arrow(-.68f,.55f,.40f,-.77f),
                        Mathf.Max(Arrow(-.24f,.72f,.46f,-1.25f), Arrow(.19f,.65f,.43f,-.89f))),
                    3 => Glyph(new(-.78f,.43f), new(-.50f,-.02f), new(-.19f,-.11f), new(0,-.52f), new(.19f,-.11f),
                        new(.50f,-.02f), new(.78f,.43f), new(.45f,.28f), new(.23f,.27f), new(0,.08f), new(-.23f,.27f),
                        new(-.45f,.28f), new(-.78f,.43f)),
                    4 => Mathf.Max(Arrow(-.42f,-.10f,1.03f,.58f),
                        Mathf.Max(Segment(-.69f,-.37f,.16f,-.20f), Segment(-.52f,-.56f,.31f,-.40f))),
                    5 => Mathf.Max(Mathf.Max(bow, line * .55f), ring * .52f),
                    6 => Mathf.Max(bow, line * .58f),
                    _ => Mathf.Max(Mathf.Max(bow, line * .55f), Segment(-.38f,-.46f,.52f,.46f,.029f))
                };
                float texture = Mathf.PerlinNoise(p.x * 9 + 13, p.y * 9 + 8);
                var dark = Color.Lerp(new Color(.027f,.044f,.033f), new Color(.16f,.21f,.12f),
                    Mathf.Clamp01((1.12f - radius) * (.40f + texture * .35f)));
                float rim = Fade(.008f, .029f, Mathf.Abs(Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y)) - .90f));
                icon.SetPixel(x, y, Color.Lerp(dark, colors[motif], Mathf.Max(shape, rim * .30f)));
            }
        icon.Apply();
        if (existing == null) AssetDatabase.CreateAsset(icon, path);
        else EditorUtility.SetDirty(icon);
        return icon;
    }

    private static T Asset<T>(string name) where T : ScriptableObject
    {
        string path = Data + name + ".asset";
        var current = AssetDatabase.LoadAssetAtPath<T>(path);
        if (current != null) return current;
        var result = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(result, path);
        return result;
    }

    private static T Load<T>(string path) where T : Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("Missing " + path);
}
