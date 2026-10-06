using System;
using System.Linq;
using DiceFree.Advancement;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Persistence;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.Skills;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class AdvancementValidation
    {
        [CliCommand("dicefree.advancement.validate", "Validate Tier-1 class shells, advancement edges, Cornberg wiring and manifestation-roster semantics.", Tags = new[] { "tests", "progression", "advancement" })]
        public static object Validate()
        {
            var novice = AssetDatabase.LoadAssetAtPath<ActorDefinition>("Assets/_DiceFree/Settings/Combat/Novice combat stats.asset");
            var physical = AssetDatabase.LoadAssetAtPath<ActorDefinition>(AdvancementAuthoring.PhysicalPath);
            var magical = AssetDatabase.LoadAssetAtPath<ActorDefinition>(AdvancementAuthoring.MagicalPath);
            var catalog = AssetDatabase.LoadAssetAtPath<ClassCatalog>(AdvancementAuthoring.CatalogPath);
            var physicalEdge = AssetDatabase.LoadAssetAtPath<AdvancementDefinition>(AdvancementAuthoring.PhysicalAdvancementPath);
            var magicalEdge = AssetDatabase.LoadAssetAtPath<AdvancementDefinition>(AdvancementAuthoring.MagicalAdvancementPath);

            Require(novice != null && physical != null && magical != null && catalog != null &&
                    physicalEdge != null && magicalEdge != null, "Advancement assets are incomplete.");
            catalog.Validate();
            Require(catalog.Classes.Count == 3, "Class catalog must contain exactly the current three prototype classes.");
            Require(catalog.Resolve(AdvancementAuthoring.NoviceId) == novice &&
                    catalog.Resolve(AdvancementAuthoring.PhysicalId) == physical &&
                    catalog.Resolve(AdvancementAuthoring.MagicalId) == magical,
                "Class catalog stable-ID resolution failed.");

            Attributes(physical.baseAttributes, 12, 12, 12, 5, 5, "Physical base");
            Attributes(physical.growth, 1, 2, 2, 0.5f, 0.5f, "Physical growth");
            Attributes(magical.baseAttributes, 10, 5, 5, 13, 13, "Magical base");
            Attributes(magical.growth, 1, 0.5f, 0.5f, 2, 2, "Magical growth");

            Edge(physicalEdge, "advancement.novice.physical", novice, physical);
            Edge(magicalEdge, "advancement.novice.magical", novice, magical);

            Require(SaveMigrations.CurrentSchema == 5 && SaveMigrations.ManifestationVersion == 5,
                "Advancement roster must remain an additive v5 section, not force a schema bump.");

            ValidateRosterSemantics();
            ValidateStartMenu(catalog);

            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player = UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
            Require(player != null, "Cornberg player is missing.");
            var controller = player.GetComponent<AdvancementController>();
            var panel = player.GetComponent<AdvancementPanel>();
            var persistence = player.GetComponent<ManifestationPersistence>();
            var stats = player.GetComponent<ActorStats>();
            Require(controller != null && panel != null && persistence != null,
                "Cornberg advancement composition is incomplete.");
            Require(stats != null && stats.Definition != null && stats.Definition.stableId == AdvancementAuthoring.NoviceId,
                "Authored Cornberg scene must remain Novice by default; roster chooses another class only at runtime.");
            Require(controller.Definitions.Count == 2 &&
                    controller.Definitions.Any(value => value == physicalEdge) &&
                    controller.Definitions.Any(value => value == magicalEdge),
                "Cornberg controller does not expose exactly the two Tier-1 edges.");
            Require(controller.Classes.Count == 3, "Cornberg controller is not wired to the three-class catalog.");

            Debug.Log("DICEFREE_ADVANCEMENT_DATA_OK: Tier-1 shells, data-driven edges, optional v5 roster and Cornberg wiring passed.");
            return new
            {
                success = true,
                finalMarker = "DICEFREE_ADVANCEMENT_DATA_OK",
                classes = catalog.Classes.Count,
                advancements = controller.Definitions.Count,
                schema = SaveMigrations.CurrentSchema,
                rosterVersion = ManifestationRoster.Version
            };
        }

        private static void ValidateStartMenu(ClassCatalog catalog)
        {
            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(AdvancementAuthoring.StartMenuScenePath);
            Require(asset != null, "Start menu scene is missing.");
            var buildScenes = EditorBuildSettings.scenes.Where(value => value.enabled).ToArray();
            Require(buildScenes.Length >= 2 &&
                    buildScenes[0].path == AdvancementAuthoring.StartMenuScenePath &&
                    buildScenes[1].path == CornbergSceneBuilder.ScenePath,
                "Build Settings must boot StartMenu before Cornberg.");

            var scene = EditorSceneManager.OpenScene(AdvancementAuthoring.StartMenuScenePath);
            var menu = UnityEngine.Object.FindFirstObjectByType<StartMenuController>();
            Require(menu != null && menu.Catalog == catalog && menu.GameplaySceneName == "Cornberg",
                "Start menu is not wired to the current class catalog/Cornberg scene.");
            Require(UnityEngine.Object.FindFirstObjectByType<CombatActor>() == null,
                "Start menu must not instantiate a gameplay CombatActor.");
        }

        private static void ValidateRosterSemantics()
        {
            var profile = new EchoSave { schemaVersion = SaveMigrations.CurrentSchema, revision = 1 };
            var novice = new ManifestationSave
            {
                classId = AdvancementAuthoring.NoviceId,
                level = 10,
                xp = 0,
                quests = Array.Empty<QuestProgress>(),
                resources = Array.Empty<ResourceSaveValue>(),
                inventory = Array.Empty<DiceFree.Items.ItemInstance>(),
                equipment = Array.Empty<DiceFree.Items.EquippedItem>(),
                classSkills = Array.Empty<SkillRankState>(),
                gold = 7
            };
            profile.sections.Add(new SaveSection
            {
                id = ManifestationRoster.SectionIdFor(AdvancementAuthoring.NoviceId),
                version = SaveMigrations.ManifestationVersion,
                json = JsonUtility.ToJson(novice)
            });

            var legacyCompatible = ManifestationRoster.Read(profile, AdvancementAuthoring.NoviceId);
            Require(legacyCompatible.activeClassId == AdvancementAuthoring.NoviceId &&
                    legacyCompatible.branches.Length == 0,
                "A v5 profile without roster metadata did not fall back to the authored current class.");

            var forked = ManifestationRoster.AddBranch(
                profile, legacyCompatible, AdvancementAuthoring.NoviceId, AdvancementAuthoring.PhysicalId);
            Require(forked.activeClassId == AdvancementAuthoring.PhysicalId &&
                    forked.branches.Length == 1 &&
                    forked.branches[0].parentClassId == AdvancementAuthoring.NoviceId &&
                    forked.branches[0].targetClassId == AdvancementAuthoring.PhysicalId,
                "Manifestation branch metadata is incorrect.");

            var child = JsonUtility.FromJson<ManifestationSave>(JsonUtility.ToJson(novice));
            child.classId = AdvancementAuthoring.PhysicalId;
            child.level = 1;
            child.classSkills = Array.Empty<SkillRankState>();
            profile.sections.Add(new SaveSection
            {
                id = ManifestationRoster.SectionIdFor(AdvancementAuthoring.PhysicalId),
                version = SaveMigrations.ManifestationVersion,
                json = JsonUtility.ToJson(child)
            });
            ManifestationRoster.Write(profile, forked);
            var roundTrip = ManifestationRoster.Read(profile, AdvancementAuthoring.NoviceId);
            Require(roundTrip.activeClassId == AdvancementAuthoring.PhysicalId &&
                    roundTrip.branches.Length == 1 &&
                    profile.schemaVersion == 5,
                "Roster round-trip changed active manifestation or schema.");

            bool duplicateRejected = false;
            try
            {
                ManifestationRoster.AddBranch(
                    profile, roundTrip, AdvancementAuthoring.NoviceId, AdvancementAuthoring.PhysicalId);
            }
            catch (InvalidOperationException)
            {
                duplicateRejected = true;
            }
            Require(duplicateRejected, "Duplicate target-class manifestation was not rejected.");
        }

        private static void Edge(
            AdvancementDefinition definition,
            string id,
            ActorDefinition source,
            ActorDefinition target)
        {
            Require(definition.stableId == id && definition.sourceClass == source &&
                    definition.targetClass == target && definition.requiredLevel == 10,
                "Advancement edge is not the authored level-10 Novice branch: " + id);
        }

        private static void Attributes(
            AttributeValues value,
            float vitality,
            float strength,
            float agility,
            float intelligence,
            float spirit,
            string label)
        {
            Require(Mathf.Approximately(value.vitality, vitality) &&
                    Mathf.Approximately(value.strength, strength) &&
                    Mathf.Approximately(value.agility, agility) &&
                    Mathf.Approximately(value.intelligence, intelligence) &&
                    Mathf.Approximately(value.spirit, spirit),
                label + " attributes changed.");
        }

        public static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }
    }
}
