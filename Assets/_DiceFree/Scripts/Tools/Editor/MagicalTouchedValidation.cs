using System;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class MagicalTouchedValidation
    {
        [CliCommand("dicefree.magical.validate", "Validate Magically Touched Tier-1 data, elements, Mana, presentation and Cornberg composition.", Tags = new[] { "tests", "magical" })]
        public static object Validate()
        {
            var magical = AssetDatabase.LoadAssetAtPath<ActorDefinition>(MagicalTouchedAuthoring.MagicalClassPath);
            Require(magical != null && magical.stableId == MagicalTouchedAuthoring.ClassId,
                "Magical class definition is missing/wrong.");
            Attributes(magical.baseAttributes, 10, 5, 5, 13, 13, "Magical base stats");
            Attributes(magical.growth, 1, .5f, .5f, 2, 2, "Magical growth");
            Require(Near(magical.basicAttackMinimumOffset,-4) &&
                Near(magical.basicAttackMaximumOffset,4),
                "Magically Touched basic attacks must vary from -4 to +4");

            var basic = AssetDatabase.LoadAssetAtPath<AttackDefinition>(MagicalTouchedAuthoring.BasicAttackPath);
            Require(basic != null && magical.basicAttack == basic &&
                    basic.channel == DamageChannel.Magical &&
                    basic.scaling == AttributeScaling.HighestSelected &&
                    Near(basic.weights.intelligence, 1) && Near(basic.weights.spirit, 1) &&
                    Near(basic.weights.strength, 0) && Near(basic.weights.agility, 0) &&
                    Near(basic.reach, 8) && basic.requiresAccuracy,
                "Magical ranged higher-INT/SPI basic attack is misconfigured.");

            var manaProfile = AssetDatabase.LoadAssetAtPath<ClassResourceProfile>(MagicalTouchedAuthoring.MagicalManaPath);
            var physicalProfile = AssetDatabase.LoadAssetAtPath<ClassResourceProfile>(PhysicalBlessedAuthoring.PhysicalManaPath);
            Require(manaProfile != null && physicalProfile != null &&
                    manaProfile.classId == MagicalTouchedAuthoring.ClassId &&
                    manaProfile.resource != null && manaProfile.resource.stableId == ResourceIds.Mana &&
                    manaProfile.loadPolicy == ResourceRetentionPolicy.Persist,
                "Magical Mana profile is missing/misconfigured.");
            Require(Near(manaProfile.Maximum(magical.baseAttributes), 119) &&
                    Near(manaProfile.Regeneration(magical.baseAttributes), 8.9f) &&
                    manaProfile.Maximum(magical.baseAttributes) > physicalProfile.Maximum(
                        AssetDatabase.LoadAssetAtPath<ActorDefinition>(PhysicalBlessedAuthoring.PhysicalClassPath).baseAttributes),
                "Magical Mana does not exceed the Physical Tier-1 economy as designed.");

            Element(MagicalTouchedAuthoring.NaturePath, "element.nature", "Nature");
            Element(MagicalTouchedAuthoring.FireElementPath, "element.fire", "Fire");
            Element(MagicalTouchedAuthoring.IceElementPath, "element.ice", "Ice");

            var sand = Skill(MagicalTouchedAuthoring.SandPath, MagicalTouchedAuthoring.SandId, MagicalSkillKind.MagicSand);
            Require(Near(sand.ManaCost(1), 10) && Near(sand.ManaCost(6), 100) &&
                    Near(sand.MagicSandMissChance(1), .075f) && Near(sand.MagicSandMissChance(6), .45f) &&
                    Near(sand.durationSeconds, 5) && sand.attack != null &&
                    sand.attack.element != null && sand.attack.element.stableId == "element.nature" &&
                    sand.attack.scaling == AttributeScaling.Weighted &&
                    Near(sand.attack.weights.intelligence, 1),
                "Magic Sand tuning/element/INT scaling changed.");

            var mend = Skill(MagicalTouchedAuthoring.MendPath, MagicalTouchedAuthoring.MendId, MagicalSkillKind.Mend);
            Require(Near(mend.cooldownSeconds, 10) && Near(mend.ManaCost(1), 12) &&
                    Near(mend.HealAmount(new AttributeValues { spirit = 10 }, 1), 80) &&
                    Near(mend.HealAmount(new AttributeValues { spirit = 10 }, 6), 180),
                "Mend SPI-only tuning changed.");

            var fire = Skill(MagicalTouchedAuthoring.FirePath, MagicalTouchedAuthoring.FireId, MagicalSkillKind.FireImbuement);
            Require(fire.secondaryAttack != null && fire.secondaryAttack.element != null &&
                    fire.secondaryAttack.element.stableId == "element.fire" &&
                    Near(fire.durationSeconds, 8) && Near(fire.ManaCost(1), 15) &&
                    Near(fire.ManaCost(6), 75) &&
                    Near(fire.ImbuementCoefficient(1), .25f) &&
                    Near(fire.ImbuementCoefficient(6), .5f),
                "Fire Imbuement tuning/element changed.");

            var ice = Skill(MagicalTouchedAuthoring.IcePath, MagicalTouchedAuthoring.IceId, MagicalSkillKind.IceBurst);
            Require(ice.attack != null && ice.attack.element != null &&
                    ice.attack.element.stableId == "element.ice" &&
                    Near(ice.ManaCost(1), 18) && Near(ice.ManaCost(6), 100) &&
                    Near(ice.IceSlowPercent(1), 2) && Near(ice.IceSlowPercent(6), 12) &&
                    Near(ice.iceDelaySeconds, .75f) && Near(ice.iceRadius, 3.5f) &&
                    Near(ice.slowDurationSeconds, 4),
                "Ice Burst delay/radius/slow/cost tuning changed.");

            var attune = Skill(MagicalTouchedAuthoring.AttunementPath, MagicalTouchedAuthoring.AttunementId, MagicalSkillKind.ManaAttunement);
            Require(!attune.Active &&
                    Near(attune.MaximumManaBonus(1), 20) && Near(attune.MaximumManaBonus(6), 120) &&
                    Near(attune.PersonalRegenBonus(1), .75f) && Near(attune.PersonalRegenBonus(6), 4.5f) &&
                    Near(attune.AuraRegenBonus(1), .25f) && Near(attune.AuraRegenBonus(6), 1.5f) &&
                    Near(attune.auraRadius, 8),
                "Mana Attunement tuning changed.");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MagicalTouchedAuthoring.MagicalPrefabPath);
            Require(prefab != null && prefab.GetComponentInChildren<Animator>(true) != null,
                "Magical presentation prefab/Animator is missing.");
            Require(prefab.GetComponentsInChildren<Collider>(true).Length == 0,
                "Magical presentation owns gameplay colliders.");
            Require(prefab.transform.localScale.x < 1,
                "Magical presentation is not visibly smaller/frailer than the Novice family.");

            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
            Require(player != null, "Cornberg player is missing.");
            var stats = player.GetComponent<ActorStats>();
            Require(stats != null && stats.Definition != null &&
                    stats.Definition.stableId == NoviceSkillProgression.ClassStableId,
                "Authored Cornberg scene must remain Novice by default.");

            var resources = player.GetComponent<ActorResourceController>();
            var progression = player.GetComponent<MagicalSkillProgression>();
            var caster = player.GetComponent<MagicalSkillCaster>();
            var aura = player.GetComponent<ResourceRegenerationAura>();
            var bar = player.GetComponent<MagicalSkillBar>();
            var targeting = player.GetComponent<SkillTargetingController>();
            var panel = player.GetComponent<MagicalSkillPanel>();
            var resourceBar = player.GetComponent<ResourceBar>();
            var switcher = player.GetComponent<ClassPresentationSwitcher>();
            Require(resources != null && resources.Profiles.Count == 2 &&
                    resources.Profiles.Any(value => value.classId == PhysicalBlessedAuthoring.ClassId) &&
                    resources.Profiles.Any(value => value.classId == MagicalTouchedAuthoring.ClassId),
                "Cornberg resource controller does not contain both Tier-1 Mana profiles.");
            Require(progression != null && progression.Definitions.Count == 5 &&
                    caster != null && aura != null && bar != null && targeting != null &&
                    panel != null && resourceBar != null,
                "Cornberg Magical runtime/UI/targeting composition is incomplete.");
            Require(switcher != null &&
                    switcher.Resolve(NoviceSkillProgression.ClassStableId) != null &&
                    switcher.Resolve(PhysicalBlessedAuthoring.ClassId) != null &&
                    switcher.Resolve(MagicalTouchedAuthoring.ClassId) != null,
                "Class presentation switcher does not contain all three implemented manifestations.");
            Require(!switcher.Resolve(MagicalTouchedAuthoring.ClassId).gameObject.activeSelf &&
                    switcher.Resolve(NoviceSkillProgression.ClassStableId).gameObject.activeSelf,
                "Authored scene presentation state is not Novice-by-default.");

            Debug.Log("DICEFREE_MAGICAL_DATA_OK: class stats, ranged basic, larger Mana, five skills/elements, presentation and Cornberg composition passed.");
            return new
            {
                success = true,
                finalMarker = "DICEFREE_MAGICAL_DATA_OK",
                classId = magical.stableId,
                skillCount = progression.Definitions.Count,
                manaMaximumAtLevel1 = manaProfile.Maximum(magical.baseAttributes),
                manaRegenAtLevel1 = manaProfile.Regeneration(magical.baseAttributes),
                sandRank6Cost = sand.ManaCost(6),
                iceRank6Cost = ice.ManaCost(6)
            };
        }

        public static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static MagicalSkillDefinition Skill(string path, string id, MagicalSkillKind kind)
        {
            var value = AssetDatabase.LoadAssetAtPath<MagicalSkillDefinition>(path);
            Require(value != null && value.stableId == id && value.kind == kind && value.maxRank == 6,
                "Magical skill asset is missing/wrong: " + id);
            return value;
        }

        private static void Element(string path, string id, string display)
        {
            var value = AssetDatabase.LoadAssetAtPath<ElementDefinition>(path);
            Require(value != null && value.stableId == id && value.displayName == display,
                "Element asset is missing/wrong: " + id);
        }

        private static void Attributes(
            AttributeValues actual,
            float vit, float str, float agi, float intel, float spirit,
            string label)
        {
            Require(Near(actual.vitality, vit) && Near(actual.strength, str) &&
                    Near(actual.agility, agi) && Near(actual.intelligence, intel) &&
                    Near(actual.spirit, spirit),
                label + " changed.");
        }

        private static bool Near(float actual, float expected) => Mathf.Abs(actual - expected) <= .01f;
    }
}
