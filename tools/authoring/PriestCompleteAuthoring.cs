using System;
using System.Linq;
using DiceFree.Development;
using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class PriestCompleteAuthoring
{
    private const string Art = "Assets/_DiceFree/Art/Characters/Dawnwarden/";
    private const string SkillAssets = "Assets/_DiceFree/Settings/Development/Priest/";
    private const string Elements = "Assets/_DiceFree/Settings/Combat/Elements/";
    private const string Model = Art + "Dawnwarden.fbx";
    private const string Controller = "Assets/_DiceFree/Resources/PlayerMotion/TierTwo/Priest.controller";

    #pragma warning disable CS0649 // Populated by the Blender manifest.
    [Serializable] private sealed class Export { public Mat[] materials; public Clip[] clips; }
    [Serializable] private sealed class Mat
    {
        public string name, baseMap, normalMap;
        public float[] color, emission;
        public float metallic, smoothness, emissionStrength;
    }
    [Serializable] private sealed class Clip
    {
        public string name;
        public float first, last;
        public bool loop;
    }

    #pragma warning restore CS0649

    public static object Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
            throw new InvalidOperationException("Open a clean scene in Edit Mode first.");

        var setup = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            AssetDatabase.Refresh();
            var controller = PrepareArt(Model, Art, Controller, Art + "DawnwardenImport.json");
            var burst = Attack(PriestSkill.LightBurst, "skill.priest.light-burst", SkillAssets + "Light.asset", 10);
            var aegis = Attack(PriestSkill.IceAegis, "skill.priest.ice-aegis", Elements + "Ice.asset", .65f);
            var growth = Attack(PriestSkill.WildGrowth, "skill.priest.wild-growth", Elements + "Nature.asset", .9f);

            foreach (var name in new[] { "Cornberg", "DevelopmentRoom" })
            {
                var scene = EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/" + name + ".unity");
                var caster = Object.FindObjectsByType<PriestSkillCaster>(FindObjectsInactive.Include).Single();
                caster.ConfigureComplete(burst, aegis, growth);
                InstallVisual(caster, controller);
                if (name == "DevelopmentRoom") PreparePracticeParty(caster);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save " + name);
            }
            AssetDatabase.SaveAssets();
            return new { scenes = 2, abilitySlots = PriestSkillProgression.SkillCount, motion = controller.animationClips.Select(c => new { c.name, c.length }).ToArray() };
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
    }

    public static AnimatorController PrepareArt(string modelPath, string artFolder, string controllerPath, string manifestPath)
    {
        var export = Newtonsoft.Json.JsonConvert.DeserializeObject<Export>(Load<TextAsset>(manifestPath).text);
        var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation = true;
        importer.importNormals = ModelImporterNormals.Import;
        importer.importTangents = ModelImporterTangents.CalculateMikk;
        importer.importCameras = false;
        importer.importLights = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        importer.preserveHierarchy = true;
        importer.skinWeights = ModelImporterSkinWeights.Custom;
        importer.maxBonesPerVertex = 8;
        var take = importer.defaultClipAnimations.FirstOrDefault();
        if (take == null) throw new InvalidOperationException(modelPath + " is missing its action take.");
        importer.clipAnimations = export.clips.Select(c => new ModelImporterClipAnimation
        {
            name = c.name, takeName = take.takeName,
            firstFrame = take.firstFrame + c.first - 1, lastFrame = take.firstFrame + c.last - 1,
            loopTime = c.loop, loopPose = c.loop,
            lockRootRotation = true, lockRootHeightY = true, lockRootPositionXZ = true
        }).ToArray();
        if (!AssetDatabase.IsValidFolder(artFolder + "Materials"))
            AssetDatabase.CreateFolder(artFolder.TrimEnd('/'), "Materials");
        foreach (var entry in export.materials)
        {
            string path = artFolder + "Materials/" + entry.name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            var baseMap = Texture(artFolder, entry.baseMap, false);
            material.SetColor("_BaseColor", baseMap == null ? ColorOf(entry.color).gamma : Color.white);
            material.SetFloat("_Metallic", entry.metallic);
            material.SetFloat("_Smoothness", entry.smoothness);
            material.SetTexture("_BaseMap", baseMap);
            var normal = Texture(artFolder, entry.normalMap, true);
            material.SetTexture("_BumpMap", normal);
            if (normal != null) material.EnableKeyword("_NORMALMAP");
            if (entry.emissionStrength > 0)
            {
                material.SetColor("_EmissionColor", ColorOf(entry.emission).gamma * entry.emissionStrength);
                material.EnableKeyword("_EMISSION");
            }
            EditorUtility.SetDirty(material);
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), entry.name), material);
        }
        importer.SaveAndReimport();
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) ??
            AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        var stateMachine = controller.layers[0].stateMachine;
        foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__")))
        {
            var state = stateMachine.states.SingleOrDefault(s => s.state.name == clip.name).state ?? stateMachine.AddState(clip.name);
            state.motion = clip;
            if (clip.name == "Idle") stateMachine.defaultState = state;
        }
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static Texture2D Texture(string artFolder, string file, bool normal)
    {
        if (string.IsNullOrEmpty(file)) return null;
        if (normal && AssetImporter.GetAtPath(artFolder + file) is TextureImporter importer &&
            importer.textureType != TextureImporterType.NormalMap)
        {
            importer.textureType = TextureImporterType.NormalMap;
            importer.SaveAndReimport();
        }
        return Load<Texture2D>(artFolder + file);
    }

    private static Color ColorOf(float[] values) => new(values[0], values[1], values[2], values[3]);

    private static AttackDefinition Attack(PriestSkill skill, string id, string element, float baseDamage)
    {
        string name = PriestSpellRules.Name(skill);
        string path = SkillAssets + name + ".asset";
        var definition = AssetDatabase.LoadAssetAtPath<AttackDefinition>(path);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<AttackDefinition>();
            AssetDatabase.CreateAsset(definition, path);
        }
        definition.stableId = id;
        definition.displayName = name;
        definition.channel = DamageChannel.Magical;
        definition.element = Load<ElementDefinition>(element);
        definition.requiresAccuracy = false;
        definition.scaling = AttributeScaling.Weighted;
        definition.weights = new AttributeValues { spirit = 1 };
        definition.baseDamage = baseDamage;
        definition.coefficient = PriestSpellRules.Coefficient(skill, 1);
        definition.reach = PriestSpellRules.Range(skill);
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static void InstallVisual(PriestSkillCaster caster, AnimatorController controller)
    {
        var switcher = new SerializedObject(caster.GetComponent<ClassPresentationSwitcher>());
        var bindings = switcher.FindProperty("bindings");
        var binding = Enumerable.Range(0, bindings.arraySize).Select(bindings.GetArrayElementAtIndex)
            .Single(b => b.FindPropertyRelative("classId").stringValue == PriestSkillProgression.PriestClassId);
        var root = binding.FindPropertyRelative("visualRoot");
        var previous = (Transform)root.objectReferenceValue;
        var wrapper = caster.transform.Find("Dawnwarden Priest")?.gameObject;
        GameObject visual;
        if (wrapper == null)
        {
            wrapper = new GameObject("Dawnwarden Priest");
            wrapper.transform.SetParent(caster.transform, false);
            visual = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(Model), wrapper.transform);
        }
        else visual = wrapper.transform.GetChild(0).gameObject;
        var animator = visual.GetComponent<Animator>();
        if (animator == null) animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        if (animator.avatar != null && !animator.avatar.isValid) throw new InvalidOperationException("Invalid Dawnwarden rig.");
        root.objectReferenceValue = wrapper.transform;
        binding.FindPropertyRelative("animator").objectReferenceValue = animator;
        switcher.ApplyModifiedPropertiesWithoutUndo();
        wrapper.SetActive(false);
        if (previous != null && previous.name == "Priest preview presentation") Object.DestroyImmediate(previous.gameObject);
    }

    private static void PreparePracticeParty(PriestSkillCaster caster)
    {
        const string party = "development.priest-practice";
        Member(caster.gameObject).partyId = party;
        var allies = Object.FindObjectsByType<PriestPracticeAlly>();
        const string fallenId = "dev.priest-fallen";
        if (!allies.Any(ally => ally.GetComponent<ActorStats>().Definition.stableId == fallenId))
        {
            var template = allies.OrderBy(ally => ally.transform.position.x).First();
            string path = SkillAssets + "Fallen party member.asset";
            var definition = AssetDatabase.LoadAssetAtPath<ActorDefinition>(path);
            if (definition == null)
            {
                definition = Object.Instantiate(template.GetComponent<ActorStats>().Definition);
                definition.stableId = fallenId;
                definition.displayName = "Fallen party member";
                AssetDatabase.CreateAsset(definition, path);
            }
            var fallen = Object.Instantiate(template.gameObject, template.transform.parent);
            fallen.name = "Priest Revive practice target";
            fallen.transform.position = template.transform.position + new Vector3(-3, 0, -3);
            fallen.GetComponent<ActorStats>().Configure(definition);
            fallen.GetComponent<PriestPracticeAlly>().ConfigureFallen();
            allies = allies.Append(fallen.GetComponent<PriestPracticeAlly>()).ToArray();
        }
        foreach (var ally in allies)
        {
            Member(ally.gameObject).partyId = party;
            if (!ally.TryGetComponent<RespawnAtAnchor>(out _)) ally.gameObject.AddComponent<RespawnAtAnchor>();
        }
    }

    private static LocalPartyMember Member(GameObject actor) =>
        actor.GetComponent<LocalPartyMember>() ?? actor.AddComponent<LocalPartyMember>();

    private static T Load<T>(string path) where T : Object =>
        AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("Missing " + path);
}
