using System;
using System.IO;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Skills;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.EditorTools
{
    public static class NoviceSkillsAuthoring
    {
        public const string DataPath = "Assets/_DiceFree/Settings/Skills";

        public const string StrengthId = "skill.novice.strength-melee-stun";
        public const string MagicSandId = "skill.novice.magic-sand";
        public const string AgilityId = "skill.novice.agility-attack-speed";
        public const string SpiritId = "skill.novice.spirit-heal";

        [CliCommand("dicefree.novice-skills.prepare", "Author the Tier-0 Novice skill kit, inputs and Cornberg presentation.", Tags = new[] { "skills", "authoring" })]
        public static object Prepare()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Novice skill authoring requires Edit Mode.");

            ControlsAuthoring.Prepare();
            EnsureFolder(DataPath);

            var strengthAttack = Attack("Novice STR melee stun attack", "attack.novice.strength-melee-stun",
                DamageChannel.Physical, new AttributeValues { strength = 1 });
            var sandAttack = Attack("Magic Sand attack", "attack.novice.magic-sand",
                DamageChannel.Magical, new AttributeValues { intelligence = 1 });

            var strength = Skill("Novice STR melee stun", StrengthId, "STR melee stun (provisional)",
                NoviceSkillKind.StrengthMeleeStun, 10, 0.7f, 0, strengthAttack, 10);
            var sand = Skill("Magic Sand", MagicSandId, "Magic Sand",
                NoviceSkillKind.MagicSand, 12, 12, 5, sandAttack, 10);
            var agility = Skill("Novice AGI attack-speed buff", AgilityId, "AGI attack-speed buff (provisional)",
                NoviceSkillKind.AgilityAttackSpeedBuff, 15, 12, 5, null, 10);
            var spirit = Skill("Novice SPI heal", SpiritId, "SPI heal (provisional)",
                NoviceSkillKind.SpiritHeal, 20, 12, 0, null, 10);
            var passive = Skill("Novice all-stat passive", NoviceSkillProgression.PassiveStableId,
                "All-stat passive (descriptive)", NoviceSkillKind.AllStatPassive, 0, 0, 0, null, 160);

            EnsureInputActions();

            foreach (string guid in AssetDatabase.FindAssets("t:ActorDefinition", new[] { "Assets/_DiceFree/Settings" }))
            {
                var actorDefinition = AssetDatabase.LoadAssetAtPath<ActorDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (actorDefinition?.basicAttack == null) continue;
                actorDefinition.basicAttack.requiresAccuracy = true;
                EditorUtility.SetDirty(actorDefinition.basicAttack);
            }

            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var traversal = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
            if (traversal == null) throw new InvalidOperationException("Existing Cornberg player is required.");

            var player = traversal.gameObject;
            var progression = Ensure<NoviceSkillProgression>(player);
            progression.Configure(new[] { strength, sand, agility, spirit, passive });
            var caster = Ensure<NoviceSkillCaster>(player);
            Ensure<CombatStatusController>(player);

            var bar = Ensure<NoviceSkillBar>(player);
            bar.Configure(progression, caster);
            var panel = Ensure<NoviceSkillPanel>(player);
            panel.Configure(progression);

            foreach (var actor in UnityEngine.Object.FindObjectsByType<CombatActor>(FindObjectsSortMode.None))
                Ensure<CombatStatusController>(actor.gameObject);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("DICEFREE_NOVICE_SKILLS_AUTHORED_OK: five Novice definitions, 1-4 active slots, K allocation menu, accuracy-tagged basic attacks.");
            return new
            {
                success = true,
                finalMarker = "DICEFREE_NOVICE_SKILLS_AUTHORED_OK",
                strengthCooldown = strength.cooldownSeconds,
                magicSandCooldown = sand.cooldownSeconds,
                agilityCooldown = agility.cooldownSeconds,
                spiritCooldown = spirit.cooldownSeconds
            };
        }

        private static NoviceSkillDefinition Skill(string assetName, string stableId, string displayName,
            NoviceSkillKind kind, float cooldown, float range, float duration, AttackDefinition attack, int maxRank)
        {
            string path = DataPath + "/" + assetName + ".asset";
            var value = AssetDatabase.LoadAssetAtPath<NoviceSkillDefinition>(path);
            if (value == null)
            {
                value = ScriptableObject.CreateInstance<NoviceSkillDefinition>();
                AssetDatabase.CreateAsset(value, path);
            }

            value.stableId = stableId;
            value.displayName = displayName;
            value.kind = kind;
            value.maxRank = maxRank;
            value.cooldownSeconds = cooldown;
            value.range = range;
            value.durationSeconds = duration;
            value.attack = attack;
            EditorUtility.SetDirty(value);
            return value;
        }

        private static AttackDefinition Attack(string assetName, string stableId, DamageChannel channel, AttributeValues weights)
        {
            string path = DataPath + "/" + assetName + ".asset";
            var value = AssetDatabase.LoadAssetAtPath<AttackDefinition>(path);
            if (value == null)
            {
                value = ScriptableObject.CreateInstance<AttackDefinition>();
                AssetDatabase.CreateAsset(value, path);
            }

            value.stableId = stableId;
            value.displayName = assetName;
            value.channel = channel;
            value.element = null;
            value.requiresAccuracy = false;
            value.scaling = AttributeScaling.Weighted;
            value.weights = weights;
            value.baseDamage = 0;
            value.coefficient = 2;
            EditorUtility.SetDirty(value);
            return value;
        }

        private static void EnsureInputActions()
        {
            var imported = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsAuthoring.AssetPath);
            if (imported == null) throw new InvalidOperationException("Canonical DiceFreeControls asset is missing.");

            var clone = UnityEngine.Object.Instantiate(imported);
            bool changed = false;
            try
            {
                var gameplay = clone.FindActionMap("Gameplay", true);
                var ui = clone.FindActionMap("UI", true);
                changed |= EnsureButton(gameplay, "Novice skill 1", "<Keyboard>/1");
                changed |= EnsureButton(gameplay, "Novice skill 2", "<Keyboard>/2");
                changed |= EnsureButton(gameplay, "Novice skill 3", "<Keyboard>/3");
                changed |= EnsureButton(gameplay, "Novice skill 4", "<Keyboard>/4");
                changed |= EnsureButton(ui, "Novice skills menu", "<Keyboard>/k");

                if (changed)
                {
                    File.WriteAllText(ControlsAuthoring.AssetPath, clone.ToJson());
                    AssetDatabase.ImportAsset(ControlsAuthoring.AssetPath, ImportAssetOptions.ForceSynchronousImport);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(clone);
            }
        }

        private static bool EnsureButton(InputActionMap map, string name, string path)
        {
            if (map.FindAction(name, false) != null) return false;
            var action = map.AddAction(name, InputActionType.Button);
            action.expectedControlType = "Button";
            action.AddBinding(path);
            return true;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || !AssetDatabase.IsValidFolder(parent))
                throw new InvalidOperationException("Missing parent folder for " + path);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static T Ensure<T>(GameObject obj) where T : Component =>
            obj.TryGetComponent<T>(out var value) ? value : obj.AddComponent<T>();
    }
}
