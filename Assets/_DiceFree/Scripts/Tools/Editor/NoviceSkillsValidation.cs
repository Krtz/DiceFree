using System;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Input;
using DiceFree.Items;
using DiceFree.Persistence;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.EditorTools
{
    public static class NoviceSkillsValidation
    {
        [CliCommand("dicefree.novice-skills.validate", "Validate authored Novice skill data, input, scene wiring and v4->v5 save migration.", Tags = new[] { "tests", "skills" })]
        public static object Run()
        {
            Require(!EditorApplication.isPlaying, "Novice skill validation requires Edit Mode.");

            var definitions = AssetDatabase.FindAssets("t:NoviceSkillDefinition", new[] { NoviceSkillsAuthoring.DataPath })
                .Select(guid => AssetDatabase.LoadAssetAtPath<NoviceSkillDefinition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(value => value != null)
                .ToArray();
            Require(definitions.Length == 5, "Expected exactly five authored Novice skill definitions.");

            var strength = Definition(definitions, NoviceSkillsAuthoring.StrengthId);
            var sand = Definition(definitions, NoviceSkillsAuthoring.MagicSandId);
            var agility = Definition(definitions, NoviceSkillsAuthoring.AgilityId);
            var spirit = Definition(definitions, NoviceSkillsAuthoring.SpiritId);
            var passive = Definition(definitions, NoviceSkillProgression.PassiveStableId);

            Require(strength.maxRank == 10 && sand.maxRank == 10 && agility.maxRank == 10 && spirit.maxRank == 10,
                "Normal Novice actives must cap at rank 10.");
            Require(passive.maxRank == 160 && !passive.Active, "Novice all-stat passive must cap at 160 and remain passive.");
            Approximately(strength.StunDuration(1), 0.2f, "Rank-1 stun duration");
            Approximately(strength.StunDuration(10), 2f, "Rank-10 stun duration");
            Approximately(sand.MagicSandMissChance(1), 0.075f, "Rank-1 Magic Sand miss chance");
            Approximately(sand.MagicSandMissChance(10), 0.75f, "Rank-10 Magic Sand miss chance");
            var sample = new AttributeValues { agility = 10, spirit = 7 };
            Approximately(agility.AttackSpeedBonusPercent(sample, 1), 12f, "Rank-1 AGI buff at 10 AGI");
            Approximately(agility.AttackSpeedBonusPercent(sample, 10), 30f, "Rank-10 AGI buff at 10 AGI");
            Approximately(spirit.HealAmount(sample, 1), 70f, "SPI heal at 7 SPI");
            Require(sand.durationSeconds == 5f && agility.durationSeconds == 5f, "Magic Sand and AGI buff must use the current 5s duration.");
            Require(strength.attack != null && sand.attack != null, "Offensive Novice skills require authored attack definitions.");
            Require(!strength.attack.requiresAccuracy && !sand.attack.requiresAccuracy,
                "Novice skill attacks are not implicitly accuracy-gated; accuracy tagging stays explicit.");

            var controls = Resources.Load<InputActionAsset>("DiceFreeControls");
            Require(controls != null, "Canonical DiceFree controls asset missing.");
            Default(controls, "Gameplay/Novice skill 1", "<Keyboard>/1");
            Default(controls, "Gameplay/Novice skill 2", "<Keyboard>/2");
            Default(controls, "Gameplay/Novice skill 3", "<Keyboard>/3");
            Default(controls, "Gameplay/Novice skill 4", "<Keyboard>/4");
            Default(controls, "UI/Novice skills menu", "<Keyboard>/k");

            foreach (string guid in AssetDatabase.FindAssets("t:ActorDefinition", new[] { "Assets/_DiceFree/Settings" }))
            {
                var actor = AssetDatabase.LoadAssetAtPath<ActorDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (actor?.basicAttack != null)
                    Require(actor.basicAttack.requiresAccuracy, actor.name + " basic attack is not explicitly tagged as requiring accuracy.");
            }

            EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var progression = UnityEngine.Object.FindAnyObjectByType<NoviceSkillProgression>();
            Require(progression != null, "Cornberg player is missing NoviceSkillProgression.");
            var player = progression.gameObject;
            Require(player.GetComponent<NoviceSkillCaster>() != null, "Cornberg player is missing NoviceSkillCaster.");
            Require(player.GetComponent<CombatStatusController>() != null, "Cornberg player is missing CombatStatusController.");
            Require(player.GetComponent<NoviceSkillBar>() != null, "Cornberg player is missing NoviceSkillBar.");
            Require(player.GetComponent<NoviceSkillPanel>() != null, "Cornberg player is missing NoviceSkillPanel.");
            foreach (var actor in UnityEngine.Object.FindObjectsByType<CombatActor>())
                Require(actor.GetComponent<CombatStatusController>() != null, actor.name + " is missing CombatStatusController.");

            var legacyQuest = new QuestProgress
            {
                questId = "quest.validation",
                definitionVersion = 1,
                status = QuestStatus.Active,
                alternatives = new[] { new ObjectiveCount { objectiveId = "route-a", count = 7 } }
            };
            var legacyState = new ManifestationSave
            {
                classId = "class.novice",
                level = 5,
                xp = 0,
                anchorId = "anchor.validation",
                quests = new[] { legacyQuest },
                resources = Array.Empty<ResourceSaveValue>(),
                inventory = Array.Empty<ItemInstance>(),
                equipment = Array.Empty<EquippedItem>(),
                gold = 123
            };
            string legacyJson = JsonUtility.ToJson(legacyState).Replace(",\"classSkills\":[]", "");
            var legacy = new EchoSave { schemaVersion = 4 };
            legacy.sections.Add(new SaveSection
            {
                id = "manifestation:class.novice",
                version = 4,
                json = legacyJson
            });
            var upgraded = SaveMigrations.Upgrade(legacy);
            Require(upgraded.schemaVersion == 5, "Echo schema did not migrate v4 -> v5.");
            var section = upgraded.sections.Single(value => value.id == "manifestation:class.novice");
            Require(section.version == 5, "Manifestation record did not migrate v4 -> v5.");
            var migrated = JsonUtility.FromJson<ManifestationSave>(section.json);
            Require(migrated.classSkills != null && migrated.classSkills.Length == 0, "Legacy v4 migration did not initialize empty class skills.");
            Require(migrated.gold == 123, "v4 -> v5 migration changed gold.");
            Require(migrated.quests.Length == 1 && migrated.quests[0].alternatives.Length == 1 &&
                    migrated.quests[0].alternatives[0].count == 7,
                "v4 -> v5 migration changed existing Q4-style alternative progress.");

            Debug.Log("DICEFREE_NOVICE_SKILLS_DATA_OK: formulas, rank caps, explicit accuracy tags, 1-4/K bindings, Cornberg composition and schema v5 migration passed.");
            return new
            {
                success = true,
                finalMarker = "DICEFREE_NOVICE_SKILLS_DATA_OK",
                definitions = definitions.Length,
                schema = SaveMigrations.CurrentSchema
            };
        }

        private static NoviceSkillDefinition Definition(NoviceSkillDefinition[] values, string id)
        {
            var result = values.SingleOrDefault(value => value.stableId == id);
            Require(result != null, "Missing Novice skill definition " + id);
            return result;
        }

        private static void Default(InputActionAsset asset, string path, string expected)
        {
            var action = asset.FindAction(path, true);
            string actual = action.bindings.First(binding => !binding.isComposite && !binding.isPartOfComposite).effectivePath;
            Require(string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
                path + " default changed: " + actual);
        }

        private static void Approximately(float actual, float expected, string label)
        {
            Require(Mathf.Abs(actual - expected) < 0.0001f, label + " expected " + expected + " but got " + actual);
        }

        internal static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
