using System;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Skills;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class AcolyteCompleteAuthoring
{
    private const string Art = "Assets/_DiceFree/Art/Characters/Lanternrest/";
    private const string Data = "Assets/_DiceFree/Settings/Acolyte/";
    private const string Elements = "Assets/_DiceFree/Settings/Combat/Elements/";
    private const string Controller = "Assets/_DiceFree/Resources/PlayerMotion/TierTwo/Acolyte.controller";
    private const string BookId = "item.arborium.lanternrest-grimoire";

    public static void BatchPresentation()
    {
        InstallIcons();
        InstallBook();
        InstallMenu(Load<GameObject>(Art + "LanternrestAcolyte.fbx"), Load<RuntimeAnimatorController>(Controller));
        AssetDatabase.SaveAssets();
    }

    public static void Batch()
    {
        AssetDatabase.Refresh();
        var controller = PriestCompleteAuthoring.PrepareArt(Art + "LanternrestAcolyte.fbx", Art, Controller, Art + "LanternrestImport.json");
        var model = Load<GameObject>(Art + "LanternrestAcolyte.fbx");
        var coffee = Asset<ElementDefinition>(Elements + "Magical Coffee.asset");
        coffee.stableId = "element.magical-coffee";
        coffee.displayName = "Magical Coffee";
        EditorUtility.SetDirty(coffee);
        Attack("Radiation Plague", "skill.acolyte.radiation-plague", Load<ElementDefinition>(Elements + "Radiation.asset"), .2f, .01f);
        Attack("Occult Poison", "skill.acolyte.occult-poison", coffee, 1, .06f);
        InstallIcons();
        InstallBook();
        InstallMenu(model, controller);
        AssetDatabase.SaveAssets();

        foreach (string sceneName in new[] { "Cornberg", "DevelopmentRoom" })
        {
            var scene = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/" + sceneName + ".unity");
            var caster = Object.FindObjectsByType<AcolyteSkillCaster>(FindObjectsInactive.Include).Single();
            caster.ConfigureComplete(Load<AttackDefinition>(Data + "Radiation Plague.asset"), Load<AttackDefinition>(Data + "Occult Poison.asset"));
            caster.GetComponent<AcolyteAbilityProvider>().ConfigureComplete(
                Load<Texture2D>(Data + "Withering Curse icon.asset"), Load<Texture2D>(Data + "Radiation Plague icon.asset"),
                Load<Texture2D>(Data + "Summon Skeleton icon.asset"), Load<Texture2D>(Data + "Occult Adept icon.asset"));
            model = Load<GameObject>(Art + "LanternrestAcolyte.fbx");
            var book = Load<ItemDefinition>(Data + "Lanternrest Grimoire.asset");
            AddItem(caster.GetComponent<CarriedInventory>(), book);
            ReplaceSceneVisual(caster, model, Load<RuntimeAnimatorController>(Controller));
            if (sceneName == "Cornberg")
            {
                var shop = Object.FindObjectsByType<ItemShop>(FindObjectsInactive.Include)
                    .Single(value => value.DisplayName == "Arborium market trader");
                if (!shop.Offers.Any(offer => offer.item == book))
                    shop.Configure(shop.Offers.Append(new ItemShop.Offer { item = book, goldPrice = 24 }).ToArray());
                EditorUtility.SetDirty(shop);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save " + sceneName);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("LANTERNREST_ACOLYTE_AUTHOR_OK");
    }

    private static void ReplaceSceneVisual(AcolyteSkillCaster caster, GameObject model, RuntimeAnimatorController controller)
    {
        var switcher = caster.GetComponent<ClassPresentationSwitcher>();
        var previous = switcher.Resolve(AcolyteSkillProgression.AcolyteClassId) ??
            throw new InvalidOperationException("The Acolyte presentation binding is missing.");
        var replacement = new GameObject("Lanternrest Acolyte");
        replacement.transform.SetParent(previous.parent, false);
        replacement.transform.SetSiblingIndex(previous.GetSiblingIndex());
        var animator = AttachModel(replacement.transform, model, controller, false);
        replacement.SetActive(false);
        var bindings = new SerializedObject(switcher).FindProperty("bindings");
        var entry = Enumerable.Range(0, bindings.arraySize).Select(bindings.GetArrayElementAtIndex)
            .Single(binding => binding.FindPropertyRelative("classId").stringValue == AcolyteSkillProgression.AcolyteClassId);
        entry.FindPropertyRelative("visualRoot").objectReferenceValue = replacement.transform;
        entry.FindPropertyRelative("animator").objectReferenceValue = animator;
        bindings.serializedObject.ApplyModifiedPropertiesWithoutUndo();

        var equipment = caster.GetComponent<EquipmentPresentation>();
        GameObject[] Keep(GameObject[] objects) => objects.Where(value => value != null && !value.transform.IsChildOf(previous)).ToArray();
        equipment.Configure(equipment.Bindings.Select(binding => new EquipmentPresentation.Binding {
            slot = binding.slot, definition = binding.definition,
            visuals = Keep(binding.visuals), baselineVisuals = Keep(binding.baselineVisuals)
        }).Where(binding => binding.visuals.Length != 0 || binding.baselineVisuals.Length != 0).ToArray());
        Object.DestroyImmediate(previous.gameObject);
    }

    private static void InstallMenu(GameObject model, RuntimeAnimatorController controller)
    {
        const string menuPath = Data + "Acolyte menu presentation.prefab";
        var preview = PrefabUtility.LoadPrefabContents(menuPath);
        try
        {
            var previous = preview.transform.Find("Class visual");
            if (previous != null) Object.DestroyImmediate(previous.gameObject);
            var visual = new GameObject("Class visual").transform;
            visual.SetParent(preview.transform, false);
            AttachModel(visual, model, controller, true);
            preview.GetComponent<EquipmentPresentation>().Configure(Array.Empty<EquipmentPresentation.Binding>());
            AddItem(preview.GetComponent<CarriedInventory>(), Load<ItemDefinition>(Data + "Lanternrest Grimoire.asset"));
            PrefabUtility.SaveAsPrefabAsset(preview, menuPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(preview); }
        var catalog = Load<MenuPresentationCatalog>("Assets/_DiceFree/Resources/MenuPresentationCatalog.asset");
        catalog.forms.Single(form => form.classId == AcolyteSkillProgression.AcolyteClassId).preview = Load<GameObject>(menuPath);
        var book = Load<ItemDefinition>(Data + "Lanternrest Grimoire.asset");
        if (!catalog.items.Contains(book)) catalog.items = catalog.items.Append(book).ToArray();
        EditorUtility.SetDirty(catalog);
    }

    private static Animator AttachModel(Transform parent, GameObject model, RuntimeAnimatorController controller, bool menu)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
        var animator = instance.GetComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        var skins = instance.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (animator.avatar == null || !animator.avatar.isValid || skins.Length != 3)
            throw new InvalidOperationException("Lanternrest model is missing its rig or hand props.");
        if (menu)
            foreach (var skin in skins) skin.updateWhenOffscreen = true;
        return animator;
    }

    private static void AddItem(CarriedInventory inventory, ItemDefinition book)
    {
        var definitions = new SerializedObject(inventory).FindProperty("definitions");
        for (int i = 0; i < definitions.arraySize; i++)
            if (definitions.GetArrayElementAtIndex(i).objectReferenceValue == book) return;
        definitions.InsertArrayElementAtIndex(definitions.arraySize);
        definitions.GetArrayElementAtIndex(definitions.arraySize - 1).objectReferenceValue = book;
        definitions.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void InstallBook()
    {
        var book = Asset<ItemDefinition>(Data + "Lanternrest Grimoire.asset");
        book.stableId = BookId;
        book.displayName = "Lanternrest Grimoire";
        book.sourceId = "shop.arborium.market";
        book.slot = EquipmentSlot.OffHand;
        book.itemLevel = 1;
        book.allowedClassIds = new[] { AcolyteSkillProgression.AcolyteClassId };
        book.flavor = "A copper-bound record of the departed, kept by the cemetery watch.";
        book.stats = new EquipmentStats { attributes = new AttributeValues { intelligence = 2, spirit = 3 } };
        var texture = Icon("Lanternrest Grimoire", 4);
        string iconPath = AssetDatabase.GetAssetPath(texture);
        var sprite = AssetDatabase.LoadAllAssetsAtPath(iconPath).OfType<Sprite>().FirstOrDefault();
        if (sprite == null)
        {
            sprite = Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), new Vector2(.5f,.5f));
            sprite.name = "Lanternrest Grimoire";
            AssetDatabase.AddObjectToAsset(sprite, texture);
        }
        book.icon = sprite;
        EditorUtility.SetDirty(book);
    }

    private static void Attack(string title, string id, ElementDefinition element, float flat, float intelligence)
    {
        var attack = Asset<AttackDefinition>(Data + title + ".asset");
        attack.stableId = id;
        attack.displayName = title;
        attack.channel = DamageChannel.Magical;
        attack.element = element;
        attack.requiresAccuracy = false;
        attack.scaling = AttributeScaling.Weighted;
        attack.weights = new AttributeValues { intelligence = intelligence };
        attack.baseDamage = flat;
        attack.coefficient = 1;
        EditorUtility.SetDirty(attack);
    }

    private static void InstallIcons()
    {
        Icon("Withering Curse", 0);
        Icon("Radiation Plague", 1);
        Icon("Summon Skeleton", 2);
        Icon("Occult Adept", 3);
    }

    private static Texture2D Icon(string title, int design)
    {
        string path = Data + title + " icon.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        const int size = 96;
        var texture = existing ?? new Texture2D(size, size, TextureFormat.RGBA32, false) { name = title, filterMode = FilterMode.Bilinear };
        var ink = design switch
        {
            0 => new Color(.79f,.32f,.92f), 1 => new Color(.61f,.96f,.30f),
            2 => new Color(.87f,.79f,.60f), 3 => new Color(.43f,.82f,.96f), _ => new Color(.88f,.66f,.45f)
        };
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = (x+.5f)/size*2-1, v = (y+.5f)/size*2-1;
                float radius = Mathf.Sqrt(u*u + v*v), theta = Mathf.Atan2(v,u);
                float Noise(float p,float q) => Mathf.PerlinNoise(p*4+5,q*4+8);
                float Smooth(float start,float end,float value) => Mathf.SmoothStep(0,1,Mathf.InverseLerp(start,end,value));
                float Ring(float r,float width) => 1-Mathf.SmoothStep(0,1,Mathf.Abs(radius-r)/width);
                float Ellipse(float a,float b,float w,float h) =>
                    1-Smooth(.82f,1.04f,Mathf.Sqrt((u-a)*(u-a)/(w*w)+(v-b)*(v-b)/(h*h)));
                float shape = design switch
                {
                    0 => Mathf.Max(Ring(.60f,.045f)*Mathf.Clamp01(3*Mathf.Abs(Mathf.Sin(theta*3))),
                        Ellipse(0,.02f,.53f,.21f)*(1-Ellipse(0,.02f,.105f,.23f))),
                    1 => Mathf.Max(Ring(.45f,.10f)*Smooth(.12f,.55f,Mathf.Cos(3*theta)),
                        Mathf.Max(Ellipse(0,0,.13f,.13f), Ring(.70f,.08f)*Smooth(.78f,.96f,Mathf.Sin(6*theta)))),
                    2 => Mathf.Max(0, Mathf.Max(Ellipse(0,.15f,.49f,.43f), Ellipse(0,-.29f,.29f,.24f)) -
                        Ellipse(-.17f,.10f,.12f,.10f) - Ellipse(.17f,.10f,.12f,.10f) - Ellipse(0,-.07f,.066f,.087f) -
                        Ellipse(-.10f,-.42f,.04f,.07f) - Ellipse(.10f,-.42f,.04f,.07f)),
                    3 => Mathf.Max(Ring(.65f,.035f), Mathf.Max(0,1-Mathf.Abs(Mathf.Abs(u)*1.5f+Mathf.Abs(v+.02f)-.47f)*17)),
                    _ => Mathf.Max(Ring(.38f,.04f), Mathf.Max(0,1-Mathf.Abs(Mathf.Max(Mathf.Abs(u)*1.24f,Mathf.Abs(v))-.65f)*20))
                };
                float glow = Mathf.Clamp01((1-radius*.89f)*(.18f+Noise(u,v)*.35f));
                var color = Color.Lerp(new Color(.039f,.025f,.072f), ink*.64f, glow);
                color = Color.Lerp(color, Color.Lerp(ink, Color.white, .20f), shape*.93f);
                color.a = 1;
                pixels[x+y*size] = color;
            }
        texture.SetPixels(pixels);
        texture.Apply();
        if (existing == null) AssetDatabase.CreateAsset(texture, path);
        else EditorUtility.SetDirty(texture);
        return texture;
    }

    private static T Asset<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static T Load<T>(string path) where T : Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("Missing " + path);
}
