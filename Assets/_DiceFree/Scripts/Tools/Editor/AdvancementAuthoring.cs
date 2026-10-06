using System;
using DiceFree.Advancement;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class AdvancementAuthoring
    {
        public const string NoviceId = "class.novice";
        public const string PhysicalId = "class.physically-blessed-novice";
        public const string MagicalId = "class.magically-touched-novice";

        public const string ProgressionFolder = "Assets/_DiceFree/Settings/Progression";
        public const string StartMenuScenePath = "Assets/_DiceFree/Scenes/StartMenu.unity";
        public const string CatalogPath = ProgressionFolder + "/Class catalog.asset";
        public const string PhysicalPath = ProgressionFolder + "/Physically Blessed Novice shell.asset";
        public const string MagicalPath = ProgressionFolder + "/Magically Touched Novice shell.asset";
        public const string PhysicalAdvancementPath = ProgressionFolder + "/Novice to Physically Blessed.asset";
        public const string MagicalAdvancementPath = ProgressionFolder + "/Novice to Magically Touched.asset";

        [MenuItem("DiceFree/Progression/Author advancement foundation")]
        [CliCommand("dicefree.advancement.prepare", "Author Tier-1 class shells, class catalog, advancement edges and Cornberg fork controls.", Tags = new[] { "progression", "advancement" })]
        public static object Prepare()
        {
            EnsureFolder();

            var novice = AssetDatabase.LoadAssetAtPath<ActorDefinition>("Assets/_DiceFree/Settings/Combat/Novice combat stats.asset");
            if (novice == null) throw new InvalidOperationException("Authored Novice actor definition is required.");

            var physical = Asset<ActorDefinition>(PhysicalPath);
            ConfigureClassShell(
                physical,
                PhysicalId,
                "Physically Blessed Novice",
                new AttributeValues { vitality = 12, strength = 12, agility = 12, intelligence = 5, spirit = 5 },
                new AttributeValues { vitality = 1, strength = 2, agility = 2, intelligence = 0.5f, spirit = 0.5f },
                novice);

            var magical = Asset<ActorDefinition>(MagicalPath);
            ConfigureClassShell(
                magical,
                MagicalId,
                "Magically Touched Novice",
                new AttributeValues { vitality = 10, strength = 5, agility = 5, intelligence = 13, spirit = 13 },
                new AttributeValues { vitality = 1, strength = 0.5f, agility = 0.5f, intelligence = 2, spirit = 2 },
                novice);

            var catalog = Asset<ClassCatalog>(CatalogPath);
            catalog.Configure(novice, physical, magical);
            EditorUtility.SetDirty(catalog);

            var physicalEdge = Asset<AdvancementDefinition>(PhysicalAdvancementPath);
            ConfigureEdge(physicalEdge, "advancement.novice.physical", "Become Physically Blessed", novice, physical);

            var magicalEdge = Asset<AdvancementDefinition>(MagicalAdvancementPath);
            ConfigureEdge(magicalEdge, "advancement.novice.magical", "Become Magically Touched", novice, magical);

            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player = UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            if (player == null) throw new InvalidOperationException("Cornberg traversal player is required.");

            var persistence = player.GetComponent<ManifestationPersistence>();
            if (persistence == null) throw new InvalidOperationException("Cornberg player persistence is required.");
            persistence.ConfigureClassCatalog(catalog);
            EditorUtility.SetDirty(persistence);

            var controller = Ensure<AdvancementController>(player.gameObject);
            controller.Configure(catalog, new[] { physicalEdge, magicalEdge });
            EditorUtility.SetDirty(controller);

            var panel = Ensure<AdvancementPanel>(player.gameObject);
            panel.Configure(controller);
            EditorUtility.SetDirty(panel);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            CreateStartMenu(catalog);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(StartMenuScenePath, true),
                new EditorBuildSettingsScene(CornbergSceneBuilder.ScenePath, true)
            };
            AssetDatabase.SaveAssets();

            Debug.Log("DICEFREE_ADVANCEMENT_AUTHORED_OK: class catalog, two Tier-1 shells, Cornberg fork controls and start-menu manifestation selector.");
            return new
            {
                success = true,
                finalMarker = "DICEFREE_ADVANCEMENT_AUTHORED_OK",
                classes = catalog.Classes.Count,
                advancements = 2
            };
        }

        private static void CreateStartMenu(ClassCatalog catalog)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "StartMenu";

            var cameraObject = new GameObject("Start menu camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.03f, 0.045f, 1f);
            cameraObject.transform.position = new Vector3(0, 0, -10);

            var menuObject = new GameObject("DiceFree start menu");
            var menu = menuObject.AddComponent<StartMenuController>();
            menu.Configure(catalog, "Cornberg");
            EditorUtility.SetDirty(menu);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, StartMenuScenePath);
        }

        private static void ConfigureClassShell(
            ActorDefinition value,
            string stableId,
            string displayName,
            AttributeValues baseAttributes,
            AttributeValues growth,
            ActorDefinition novice)
        {
            value.stableId = stableId;
            value.displayName = displayName;
            value.familyId = "";
            value.tags = Array.Empty<string>();
            value.experienceReward = 0;
            value.baseAttributes = baseAttributes;
            value.growth = growth;

            // #50 is the fork foundation, not the Tier-1 combat implementation.
            // Keep unresolved class-specific combat values inherited from the known-good Novice shell
            // rather than inventing fake Tier-1 balance before #51/#52 implement the real classes.
            value.baseHp = novice.baseHp;
            value.physicalDefense = novice.physicalDefense;
            value.magicalDefense = novice.magicalDefense;
            value.moveSpeed = novice.moveSpeed;
            value.basicAttack = novice.basicAttack;
            value.tuning = novice.tuning;
            value.secondaryOverrides = Array.Empty<SecondaryCoefficientOverride>();
            value.overrideVitalityHp = novice.overrideVitalityHp;
            value.overrideVitalityRegeneration = novice.overrideVitalityRegeneration;
            value.hpPerVitality = novice.hpPerVitality;
            value.regenerationPerVitality = novice.regenerationPerVitality;
            value.resistances = Array.Empty<ElementResistance>();
            EditorUtility.SetDirty(value);
        }

        private static void ConfigureEdge(
            AdvancementDefinition value,
            string stableId,
            string displayName,
            ActorDefinition source,
            ActorDefinition target)
        {
            value.stableId = stableId;
            value.displayName = displayName;
            value.sourceClass = source;
            value.targetClass = target;
            value.requiredLevel = 10;
            EditorUtility.SetDirty(value);
        }

        private static T Asset<T>(string path) where T : ScriptableObject
        {
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            if (value != null) return value;
            value = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(value, path);
            return value;
        }

        private static T Ensure<T>(GameObject value) where T : Component =>
            value.TryGetComponent<T>(out var existing) ? existing : value.AddComponent<T>();

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(ProgressionFolder))
                AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "Progression");
        }
    }
}
