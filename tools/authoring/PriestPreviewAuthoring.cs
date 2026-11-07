using System;
using System.IO;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Combat;
using DiceFree.Development;
using DiceFree.EditorTools;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class PriestPreviewAuthoring
{
    private const string ScenePath = "Assets/_DiceFree/Scenes/DevelopmentRoom.unity";
    private const string Folder = "Assets/_DiceFree/Settings/Development/Priest";

    public static object Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty || AssetDatabase.IsValidFolder(Folder))
            throw new InvalidOperationException("A clean Editor and unauthored Priest preview are required.");
        var setup = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            var persistence = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ManifestationPersistence>(true)).Single();
            var player = persistence.gameObject;
            var advancement = player.GetComponent<AdvancementController>();
            var presentation = player.GetComponent<ClassPresentationSwitcher>();
            var resources = player.GetComponent<ActorResourceController>();
            var map = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<DevelopmentMap>(true)).Single();
            var targets = map.GetComponentsInChildren<CombatActor>(true).Where(a => a.GetComponent<ActorStats>().Definition.stableId == "dev.practice-target").ToArray();
            var parent = AssetDatabase.LoadAssetAtPath<ActorDefinition>(AdvancementAuthoring.MagicalPath);
            var original = AssetDatabase.LoadAssetAtPath<ClassCatalog>(AdvancementAuthoring.CatalogPath);
            if (targets.Length != 3 || parent == null || original == null || presentation.Resolve(parent.stableId) == null)
                throw new InvalidOperationException("Expected development target, caster rig and catalog fixtures.");
            File.Copy(ScenePath, "Logs/DevelopmentRoom-before-priest-preview.unity");
            AssetDatabase.CreateFolder("Assets/_DiceFree/Settings/Development", "Priest");

            var light = Create<ElementDefinition>("Light");
            light.stableId = "element.light";
            light.displayName = "Light";
            var basic = Create<AttackDefinition>("Priest staff");
            basic.stableId = "attack.priest.staff";
            basic.displayName = "Staff Strike";
            basic.channel = DamageChannel.Magical;
            basic.scaling = AttributeScaling.Weighted;
            basic.weights = new AttributeValues { spirit = 1 };
            basic.requiresAccuracy = true;
            basic.baseDamage = 6;
            basic.coefficient = 1.1f;
            basic.interval = 1.45f;
            basic.windup = .32f;
            basic.reach = 2.4f;
            var holyLight = Create<AttackDefinition>("Holy Light");
            holyLight.stableId = "attack.priest.holy-light";
            holyLight.displayName = "Holy Light";
            holyLight.element = light;
            holyLight.channel = DamageChannel.Magical;
            holyLight.scaling = AttributeScaling.Weighted;
            holyLight.weights = new AttributeValues { spirit = 1 };
            holyLight.baseDamage = 14;
            holyLight.coefficient = PriestSkillCaster.Coefficient(1);
            holyLight.reach = PriestSkillCaster.Range;

            var priest = Create<ActorDefinition>("Priest preview");
            EditorUtility.CopySerialized(parent, priest);
            priest.name = "Priest preview";
            priest.stableId = PriestSkillProgression.PriestClassId;
            priest.displayName = "Priest";
            priest.baseAttributes = new AttributeValues { vitality = 12, strength = 5, agility = 6, intelligence = 11, spirit = 18 };
            priest.growth = new AttributeValues { vitality = 1.5f, strength = .5f, agility = .75f, intelligence = 1.5f, spirit = 3 };
            priest.basicAttack = basic;

            var mana = Create<ClassResourceProfile>("Priest Mana");
            mana.stableId = "resource-profile.priest.mana";
            mana.classId = priest.stableId;
            mana.resource = AssetDatabase.LoadAssetAtPath<ResourceDefinition>("Assets/_DiceFree/Settings/Resources/Mana.asset");
            mana.baseMaximum = 70;
            mana.maximumPerSpirit = 4;
            mana.baseRegenPerSecond = .7f;
            mana.regenPerSpirit = .015f;

            var graceResource = Create<ResourceDefinition>("Grace");
            graceResource.stableId = PriestSkillProgression.GraceResourceId;
            graceResource.displayName = "Grace";
            var grace = Create<ClassResourceProfile>("Priest Grace");
            grace.stableId = "resource-profile.priest.grace";
            grace.classId = priest.stableId;
            grace.resource = graceResource;
            grace.baseMaximum = 0;
            grace.baseRegenPerSecond = 0;
            grace.initialFraction = 0;
            grace.loadPolicy = ResourceRetentionPolicy.ResetOnLoad;

            var catalog = Create<ClassCatalog>("Development Priest catalog");
            catalog.Configure(original.Classes.Append(priest).ToArray());
            var edge = Create<AdvancementDefinition>("Development Priest selection");
            edge.stableId = "advancement.development.priest";
            edge.displayName = "Select Priest";
            edge.sourceClass = parent;
            edge.targetClass = priest;
            edge.requiredLevel = 30;
            persistence.ConfigureClassCatalog(catalog);
            advancement.Configure(catalog, advancement.Definitions.Append(edge).ToArray());
            resources.Configure(resources.Profiles.Concat(new[] { mana, grace }).ToArray());
            player.AddComponent<PriestSkillProgression>();
            player.AddComponent<PriestSkillCaster>().Configure(holyLight);
            player.AddComponent<PriestAbilityProvider>();

            var visual = Object.Instantiate(presentation.Resolve(parent.stableId).gameObject, player.transform);
            visual.name = "Priest preview presentation";
            visual.SetActive(false);
            var bindings = new SerializedObject(presentation).FindProperty("bindings");
            bindings.InsertArrayElementAtIndex(bindings.arraySize);
            var binding = bindings.GetArrayElementAtIndex(bindings.arraySize - 1);
            binding.FindPropertyRelative("classId").stringValue = priest.stableId;
            binding.FindPropertyRelative("visualRoot").objectReferenceValue = visual.transform;
            binding.FindPropertyRelative("animator").objectReferenceValue = null;
            bindings.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            var allyStats = Create<ActorDefinition>("Priest practice ally");
            EditorUtility.CopySerialized(targets[0].GetComponent<ActorStats>().Definition, allyStats);
            allyStats.name = "Priest practice ally";
            allyStats.stableId = "dev.priest-ally";
            allyStats.displayName = "Wounded Ally";
            allyStats.baseHp = 300;
            var blue = new Material(targets[0].GetComponentInChildren<Renderer>().sharedMaterial)
            { name = "Practice ally blue", color = new Color(.22f, .51f, .79f) };
            AssetDatabase.CreateAsset(blue, Folder + "/Practice ally blue.mat");
            AddAlly(targets[0], allyStats, blue, targets[1].transform.position + Vector3.back * 4, .35f);
            AddAlly(targets[0], allyStats, blue, targets[2].transform.position + Vector3.back * 4, .55f);
            foreach (var asset in new Object[] { light, basic, holyLight, priest, mana, graceResource, grace, catalog, edge, allyStats })
                EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Development scene save failed.");
            return new { classId = priest.stableId, abilities = 2, allies = 2, map = scene.name, catalogCount = catalog.Classes.Count };
        }
        finally
        {
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }
    }

    private static T Create<T>(string name) where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, Folder + "/" + name + ".asset");
        return asset;
    }

    private static void AddAlly(CombatActor template, ActorDefinition stats, Material color, Vector3 position, float health)
    {
        var ally = Object.Instantiate(template.gameObject, template.transform.parent);
        ally.name = "Priest healing practice ally";
        ally.transform.position = position;
        ally.GetComponent<ActorStats>().Configure(stats);
        ally.GetComponent<CombatActor>().Configure(0, .5f, "dev.priest-ally");
        ally.GetComponentInChildren<Renderer>().sharedMaterial = color;
        ally.AddComponent<PriestPracticeAlly>().Configure(health);
    }
}
