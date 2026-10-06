using System;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Progression;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class PhysicalBlessedValidation
    {
        [CliCommand("dicefree.physical.validate", "Validate Physically Blessed Tier-1 data, resource, presentation and Cornberg composition.", Tags = new[] { "tests", "physical" })]
        public static object Validate()
        {
            var physical = RequireAsset<ActorDefinition>(PhysicalBlessedAuthoring.PhysicalClassPath);
            Require(physical.stableId == PhysicalBlessedAuthoring.ClassId, "Physical class stable ID changed.");
            Attributes(physical.baseAttributes, 12, 12, 12, 5, 5, "Physical base");
            Attributes(physical.growth, 1, 2, 2, .5f, .5f, "Physical growth");

            var basic = RequireAsset<AttackDefinition>(PhysicalBlessedAuthoring.BasicAttackPath);
            Require(physical.basicAttack == basic && basic.channel == DamageChannel.Physical &&
                    basic.scaling == AttributeScaling.HighestSelected && basic.requiresAccuracy &&
                    Mathf.Approximately(basic.weights.strength, 1) && Mathf.Approximately(basic.weights.agility, 1),
                "Physical adaptive basic attack is not authored from higher STR/AGI.");
            var probe = new AttributeValues { strength = 20, agility = 30, intelligence = 100 };
            Require(Mathf.Approximately(basic.RawDamage(probe), 1 + 1.4f * 30f),
                "Physical basic attack used the wrong attribute.");

            var mana = RequireAsset<ResourceDefinition>(PhysicalBlessedAuthoring.ManaPath);
            var resource = RequireAsset<ClassResourceProfile>(PhysicalBlessedAuthoring.PhysicalManaPath);
            resource.Validate();
            Require(mana.stableId == PhysicalBlessedAuthoring.ManaId &&
                    resource.classId == PhysicalBlessedAuthoring.ClassId &&
                    resource.resource == mana &&
                    resource.loadPolicy == ResourceRetentionPolicy.Persist &&
                    Mathf.Approximately(resource.Maximum(physical.baseAttributes), 65) &&
                    Mathf.Approximately(resource.Regeneration(physical.baseAttributes), 4.5f),
                "Physical Mana profile changed from the documented prototype tuning.");

            var heavy = Skill(PhysicalBlessedAuthoring.HeavyPath, PhysicalBlessedAuthoring.HeavyId, PhysicalSkillKind.HeavyStrike);
            var guard = Skill(PhysicalBlessedAuthoring.GuardPath, PhysicalBlessedAuthoring.GuardId, PhysicalSkillKind.Guard);
            var quick = Skill(PhysicalBlessedAuthoring.QuickeningPath, PhysicalBlessedAuthoring.QuickeningId, PhysicalSkillKind.Quickening);
            var rain = Skill(PhysicalBlessedAuthoring.ArrowRainPath, PhysicalBlessedAuthoring.ArrowRainId, PhysicalSkillKind.ArrowRain);
            var martial = Skill(PhysicalBlessedAuthoring.MartialPath, PhysicalBlessedAuthoring.MartialId, PhysicalSkillKind.MartialAptitude);
            var skills = new[] { heavy, guard, quick, rain, martial };

            Require(skills.All(value => value.maxRank == 6) && skills.Count(value => value.Active) == 4,
                "Physical kit must remain four actives plus one passive, six ranks each.");
            Near(heavy.HeavyStunSeconds(1), .4f, "Heavy Strike R1 stun");
            Near(heavy.HeavyStunSeconds(6), 1.4f, "Heavy Strike R6 stun");
            Near(guard.GuardReduction(1), .15f, "Guard R1 reduction");
            Near(guard.GuardReduction(6), .40f, "Guard R6 reduction");
            Near(quick.QuickeningAttackSpeedPercent(1), 12, "Quickening R1 attack speed");
            Near(quick.QuickeningAttackSpeedPercent(6), 32, "Quickening R6 attack speed");
            Near(quick.QuickeningMoveSpeedPercent(1), 4, "Quickening R1 move speed");
            Near(quick.QuickeningMoveSpeedPercent(6), 9, "Quickening R6 move speed");
            Near(martial.MartialBasicAttackPercent(6), 30, "Martial Aptitude R6 basic attack");
            Near(martial.MartialPhysicalDefense(6), 9, "Martial Aptitude R6 physical defense");
            Require(rain.attack != null && rain.attack.channel == DamageChannel.Physical &&
                    rain.arrowHitCount == 3 && Mathf.Approximately(rain.arrowRadius, 3) &&
                    Mathf.Approximately(rain.range, 12),
                "Arrow Rain short-burst AoE tuning changed.");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PhysicalBlessedAuthoring.PhysicalPrefabPath);
            Require(prefab != null, "Physical presentation prefab is missing.");
            Require(prefab.GetComponentsInChildren<Collider>(true).Length == 0,
                "Physical presentation must not own gameplay colliders.");
            var prefabAnimator = prefab.GetComponentInChildren<Animator>(true);
            Require(prefabAnimator != null && prefabAnimator.avatar != null && prefabAnimator.avatar.isValid &&
                    prefabAnimator.runtimeAnimatorController != null && !prefabAnimator.applyRootMotion,
                "Physical presentation must retain the valid Novice-family humanoid animator.");
            Require(prefab.transform.localScale.x > 1.04f,
                "Physical presentation is not visibly larger than the Novice family baseline.");

            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
            Require(player != null, "Cornberg player is missing.");
            var go = player.gameObject;
            var runtimeResource = go.GetComponent<ActorResourceController>();
            var progression = go.GetComponent<PhysicalSkillProgression>();
            var caster = go.GetComponent<PhysicalSkillCaster>();
            var bar = go.GetComponent<PhysicalSkillBar>();
            var panel = go.GetComponent<PhysicalSkillPanel>();
            var resourceBar = go.GetComponent<ResourceBar>();
            var switcher = go.GetComponent<ClassPresentationSwitcher>();
            Require(runtimeResource != null && progression != null && caster != null && bar != null &&
                    panel != null && resourceBar != null && switcher != null,
                "Cornberg Physical runtime/UI composition is incomplete.");
            Require(runtimeResource.Profiles.Count == 1 && runtimeResource.Profiles[0] == resource,
                "Cornberg player is not wired to Physical Mana.");
            Require(progression.Definitions.Count == 5 &&
                    progression.Definitions.All(value => skills.Contains(value)),
                "Cornberg player is not wired to the five Physical skills.");

            var noviceVisual = player.transform.Find("NovicePresentation");
            var physicalVisual = player.transform.Find("PhysicallyBlessedPresentation");
            Require(noviceVisual != null && noviceVisual.gameObject.activeSelf &&
                    physicalVisual != null && !physicalVisual.gameObject.activeSelf,
                "Authored Cornberg scene must stay Novice by default with Physical presentation dormant.");
            Require(switcher.Resolve(NoviceSkillProgression.ClassStableId) == noviceVisual &&
                    switcher.Resolve(PhysicalBlessedAuthoring.ClassId) == physicalVisual,
                "Class presentation switcher is not wired to Novice/Physical visuals.");
            Require(physicalVisual.GetComponentsInChildren<Collider>(true).Length == 0,
                "Physical scene presentation added a gameplay collider.");

            var buildScenes = EditorBuildSettings.scenes.Where(value => value.enabled).ToArray();
            Require(buildScenes.Length >= 2 &&
                    buildScenes[0].path == AdvancementAuthoring.StartMenuScenePath &&
                    buildScenes[1].path == CornbergSceneBuilder.ScenePath,
                "Physical work must preserve StartMenu -> Cornberg build order.");

            Debug.Log("DICEFREE_PHYSICAL_DATA_OK: class stats, adaptive basic, Mana, five-skill formulas, presentation and Cornberg composition passed.");
            return new
            {
                success = true,
                finalMarker = "DICEFREE_PHYSICAL_DATA_OK",
                classId = physical.stableId,
                skillCount = skills.Length,
                manaMaximumAtLevel1 = resource.Maximum(physical.baseAttributes),
                manaRegenAtLevel1 = resource.Regeneration(physical.baseAttributes)
            };
        }

        private static PhysicalSkillDefinition Skill(string path, string id, PhysicalSkillKind kind)
        {
            var value = RequireAsset<PhysicalSkillDefinition>(path);
            Require(value.stableId == id && value.kind == kind, "Physical skill identity changed: " + id);
            return value;
        }

        private static T RequireAsset<T>(string path) where T : UnityEngine.Object
        {
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            Require(value != null, "Missing asset: " + path);
            return value;
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

        public static void Near(float actual, float expected, string label)
        {
            Require(Mathf.Abs(actual - expected) <= .001f, label + ": expected " + expected + ", got " + actual);
        }

        public static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
