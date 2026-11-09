using System;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class PsionFoundationAuthoring
{
    private const string Folder = "Assets/_DiceFree/Settings/Psion/";
    private const string Catalog = "Assets/_DiceFree/Settings/Development/Priest/Development Priest catalog.asset";
    private const string ParentId = "class.magically-touched-novice";
    private const string ClassId = PsionSkillProgression.PsionClassId;

    public static void Batch()
    {
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        var scene = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/DevelopmentRoom.unity");
        var advancement = Object.FindObjectsByType<AdvancementController>(FindObjectsInactive.Include).Single();
        var player = advancement.GetComponent<CombatActor>();
        var catalog = Load<ClassCatalog>(Catalog);
        var parent = catalog.Resolve(ParentId);
        var metal = Asset<ElementDefinition>("Metal");
        metal.stableId = "element.metal";
        metal.displayName = "Metal";
        var basic = Attack("Psionic blade", "attack.psion.blade", DamageChannel.Physical, null, true);
        basic.weights = new AttributeValues { agility = .45f, intelligence = .45f };
        basic.reach = 5;
        basic.interval = 1.18f;
        basic.windup = .34f;
        var bolt = Attack("Psychic Bolt", "skill.psion.psychic-bolt", DamageChannel.Magical, metal);
        var storm = Attack("Lightning Storm", "skill.psion.lightning-storm", DamageChannel.Magical,
            Load<ElementDefinition>("Assets/_DiceFree/Settings/Combat/Elements/Lightning.asset"));
        var imageBlade = Attack("Mental Image blade", "skill.psion.image-blade", DamageChannel.Physical, null, true);
        var rupture = Attack("Mental Image rupture", "skill.psion.image-rupture", DamageChannel.Magical,
            Load<ElementDefinition>("Assets/_DiceFree/Settings/Acolyte/Void.asset"));
        var psion = Asset<ActorDefinition>("Psion class");
        psion.stableId = ClassId;
        psion.displayName = "Psion";
        psion.tuning = parent.tuning;
        psion.baseAttributes = new AttributeValues { vitality = 12, strength = 7, agility = 14, intelligence = 18, spirit = 10 };
        psion.growth = new AttributeValues { vitality = 1.55f, strength = .65f, agility = 2.1f, intelligence = 2.95f, spirit = 1.1f };
        psion.baseHp = 10;
        psion.moveSpeed = 5;
        psion.basicAttack = basic;
        var mana = Asset<ClassResourceProfile>("Psion Mana");
        EditorUtility.CopySerialized(Load<ClassResourceProfile>("Assets/_DiceFree/Settings/Resources/Magically Touched Mana.asset"), mana);
        mana.name = "Psion Mana";
        mana.stableId = "resource-profile.psion.mana";
        mana.classId = ClassId;
        mana.baseMaximum = 84;
        mana.maximumPerIntelligence = 1.45f;
        mana.baseRegenPerSecond = 4.5f;
        var edge = Asset<AdvancementDefinition>("Psion development edge");
        edge.stableId = "advance.magically-touched.psion";
        edge.displayName = "Psion";
        edge.sourceClass = parent;
        edge.targetClass = psion;
        edge.requiredLevel = 30;
        // This edge belongs to the test room. It uses the existing development
        // bypass while the World 1 quest and mentor are still being planned.
        edge.requiredQuestId = "quest.psion.advancement";
        foreach (var data in new Object[] { metal, basic, psion, mana, edge }) EditorUtility.SetDirty(data);

        catalog.Configure(catalog.Classes.Where(value => value.stableId != ClassId).Append(psion).ToArray());
        EditorUtility.SetDirty(catalog);
        advancement.Configure(catalog, advancement.Definitions.Where(value => value != edge).Append(edge).ToArray());
        player.GetComponent<ManifestationPersistence>().ConfigureClassCatalog(catalog);
        var resources = player.GetComponent<ActorResourceController>();
        resources.Configure(resources.Profiles.Where(value => value != mana).Append(mana).ToArray());
        var caster = player.GetComponent<PsionSkillCaster>() ?? player.gameObject.AddComponent<PsionSkillCaster>();
        caster.Configure(bolt, storm, imageBlade, rupture, metal);
        var provider = player.GetComponent<PsionAbilityProvider>() ?? player.gameObject.AddComponent<PsionAbilityProvider>();
        provider.Configure(Enumerable.Range(0, PsionSkillProgression.SkillCount).Select(Icon).ToArray());
        BindTrainingRig(player);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save the Psion test setup.");
        AssetDatabase.SaveAssets();
        Debug.Log("DICEFREE_PSION_FOUNDATION_AUTHOR_OK");
    }

    private static void BindTrainingRig(CombatActor player)
    {
        var switcher = new SerializedObject(player.GetComponent<ClassPresentationSwitcher>());
        var bindings = switcher.FindProperty("bindings");
        int parent = Enumerable.Range(0, bindings.arraySize).Single(i =>
            bindings.GetArrayElementAtIndex(i).FindPropertyRelative("classId").stringValue == ParentId);
        var form = bindings.GetArrayElementAtIndex(parent);
        var visual = form.FindPropertyRelative("visualRoot").objectReferenceValue;
        var animator = form.FindPropertyRelative("animator").objectReferenceValue;
        int index = Enumerable.Range(0, bindings.arraySize).Where(i =>
            bindings.GetArrayElementAtIndex(i).FindPropertyRelative("classId").stringValue == ClassId).DefaultIfEmpty(-1).First();
        if (index < 0)
        {
            index = bindings.arraySize;
            bindings.InsertArrayElementAtIndex(index);
        }
        var entry = bindings.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("classId").stringValue = ClassId;
        entry.FindPropertyRelative("visualRoot").objectReferenceValue = visual;
        entry.FindPropertyRelative("animator").objectReferenceValue = animator;
        switcher.ApplyModifiedPropertiesWithoutUndo();
    }

    private static AttackDefinition Attack(string name, string id, DamageChannel channel, ElementDefinition element, bool accuracy = false)
    {
        var attack = Asset<AttackDefinition>(name);
        attack.stableId = id;
        attack.displayName = name;
        attack.channel = channel;
        attack.element = element;
        attack.requiresAccuracy = accuracy;
        attack.scaling = AttributeScaling.Weighted;
        attack.baseDamage = 0;
        attack.coefficient = 1;
        EditorUtility.SetDirty(attack);
        return attack;
    }

    private static Texture2D Icon(int index)
    {
        string name = PsionSkillProgression.Name((PsionSkill)index);
        string path = Folder + name + " icon.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (existing != null) return existing;
        const int size = 72;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = name };
        var palette = new[] { new Color(.80f,.84f,1), new Color(.72f,.47f,.96f), new Color(.62f,.47f,.87f),
            new Color(.42f,.82f,1), new Color(.79f,.80f,.88f) };
        var glyphs = new[]
        {
            new[] {new Vector2(-.66f,-.27f),new Vector2(.25f,.20f),new Vector2(.13f,.28f),new Vector2(.59f,.47f),new Vector2(.36f,.05f)},
            new[] {new Vector2(-.44f,-.54f),new Vector2(.54f,.62f),new Vector2(.38f,.13f)},
            new[] {new Vector2(-.44f,-.53f),new Vector2(-.34f,-.01f),new Vector2(-.19f,.15f),new Vector2(.19f,.15f),
                new Vector2(.34f,-.01f),new Vector2(.44f,-.53f),new Vector2(-.44f,-.53f)},
            new[] {new Vector2(.22f,.65f),new Vector2(-.29f,.03f),new Vector2(.20f,.12f),new Vector2(-.19f,-.63f)},
            new[] {new Vector2(-.52f,.48f),new Vector2(.52f,.48f),new Vector2(.48f,-.09f),new Vector2(.23f,-.43f),
                new Vector2(0,-.61f),new Vector2(-.23f,-.43f),new Vector2(-.48f,-.09f),new Vector2(-.52f,.48f)}
        };
        for (int y=0; y<size; y++)
            for (int x=0; x<size; x++)
            {
                var point = new Vector2((x+.5f)*2/size-1, (y+.5f)*2/size-1);
                float Fade(float distance) => 1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.02f,.052f,distance));
                float Segment(Vector2 a,Vector2 b) => Fade(Vector2.Distance(point,
                    a+(b-a)*Mathf.Clamp01(Vector2.Dot(point-a,b-a)/(b-a).sqrMagnitude)));
                float value = 0;
                for (int i=1;i<glyphs[index].Length;i++)
                    value = Mathf.Max(value,Segment(glyphs[index][i-1],glyphs[index][i]));
                if (index==1) value = Mathf.Max(value,Segment(new(-.43f,.51f),new(.55f,-.48f)));
                if (index==2) value = Mathf.Max(value,Fade(Mathf.Abs(Vector2.Distance(point,new Vector2(0,.39f))-.18f)));
                if (index==4) value = Mathf.Max(value,Fade(Mathf.Abs(point.magnitude-.25f)));
                var background=Color.Lerp(new Color(.03f,.041f,.071f),new Color(.18f,.13f,.24f),Mathf.Clamp01(1-point.magnitude));
                texture.SetPixel(x,y,Color.Lerp(background,palette[index],value));
            }
        texture.Apply();
        AssetDatabase.CreateAsset(texture,path);
        return texture;
    }

    private static T Asset<T>(string name) where T : ScriptableObject
    {
        string path = Folder + name + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var value = ScriptableObject.CreateInstance<T>();
        value.name = name;
        AssetDatabase.CreateAsset(value,path);
        return value;
    }

    private static T Load<T>(string path) where T:Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("Missing " + path);
}