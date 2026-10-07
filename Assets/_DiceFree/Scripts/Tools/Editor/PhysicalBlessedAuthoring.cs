using System;
using System.IO;
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
    public static class PhysicalBlessedAuthoring
    {
        public const string ClassId = "class.physically-blessed-novice";
        public const string ManaId = "resource.mana";

        public const string Root = "Assets/_DiceFree";
        public const string SkillFolder = Root + "/Settings/Skills/Physical";
        public const string ResourceFolder = Root + "/Settings/Resources";
        public const string ArtFolder = Root + "/Art/Characters/PhysicallyBlessed";
        public const string PhysicalClassPath = "Assets/_DiceFree/Settings/Progression/Physically Blessed Novice shell.asset";
        public const string ManaPath = ResourceFolder + "/Mana.asset";
        public const string PhysicalManaPath = ResourceFolder + "/Physically Blessed Mana.asset";
        public const string BasicAttackPath = SkillFolder + "/Physical Basic Attack.asset";
        public const string HeavyAttackPath = SkillFolder + "/Heavy Strike Attack.asset";
        public const string ArrowAttackPath = SkillFolder + "/Arrow Rain Attack.asset";
        public const string HeavyPath = SkillFolder + "/Heavy Strike.asset";
        public const string GuardPath = SkillFolder + "/Guard.asset";
        public const string QuickeningPath = SkillFolder + "/Quickening.asset";
        public const string ArrowRainPath = SkillFolder + "/Arrow Rain.asset";
        public const string MartialPath = SkillFolder + "/Martial Aptitude.asset";
        public const string PhysicalPrefabPath = ArtFolder + "/Prefabs/PhysicallyBlessedPresentation.prefab";
        public const string PhysicalTeePath = ArtFolder + "/Materials/PhysicallyBlessed_Tee.mat";
        private const string NovicePrefabPath = "Assets/_DiceFree/Art/Characters/Novice/Prefabs/NovicePresentation.prefab";

        public const string HeavyId = "skill.physical.heavy-strike";
        public const string GuardId = "skill.physical.guard";
        public const string QuickeningId = "skill.physical.quickening";
        public const string ArrowRainId = "skill.physical.arrow-rain";
        public const string MartialId = "skill.physical.martial-aptitude";

        [MenuItem("DiceFree/Progression/Author Physically Blessed")]
        [CliCommand("dicefree.physical.prepare", "Author the Physically Blessed Tier-1 data, presentation and Cornberg runtime composition.", Tags = new[] { "progression", "physical" })]
        public static object Prepare()
        {
            EnsureFolders();

            var physical = AssetDatabase.LoadAssetAtPath<ActorDefinition>(PhysicalClassPath);
            if (physical == null) throw new InvalidOperationException("Physical class shell from #50 is missing.");

            var mana = Asset<ResourceDefinition>(ManaPath);
            mana.stableId = ManaId;
            mana.displayName = "Mana";
            EditorUtility.SetDirty(mana);

            var profile = Asset<ClassResourceProfile>(PhysicalManaPath);
            profile.stableId = "resource-profile.physical.mana";
            profile.classId = ClassId;
            profile.resource = mana;
            // Prototype tuning: deliberately usable despite Physical's low INT/SPI.
            profile.baseMaximum = 60;
            profile.maximumPerIntelligence = 1;
            profile.maximumPerSpirit = 0;
            profile.baseRegenPerSecond = 4;
            profile.regenPerIntelligence = 0;
            profile.regenPerSpirit = 0.1f;
            profile.initialFraction = 1;
            profile.loadPolicy = ResourceRetentionPolicy.Persist;
            EditorUtility.SetDirty(profile);

            var basic = Attack(BasicAttackPath, "attack.physical.basic", "Adaptive Strike", 1.4f, 1.05f, 0.24f, 0.7f, true);
            physical.basicAttack = basic;
            EditorUtility.SetDirty(physical);

            var heavyAttack = Attack(HeavyAttackPath, "attack.physical.heavy-strike", "Heavy Strike", 1.5f, 1, 0, 0.7f, false);
            var arrowAttack = Attack(ArrowAttackPath, "attack.physical.arrow-rain", "Arrow Rain", 0.5f, 1, 0, 12, false);

            var heavy = Skill(HeavyPath, HeavyId, "Heavy Strike", PhysicalSkillKind.HeavyStrike);
            Tune(heavy, cooldown: 6, duration: 0, range: 0.7f, mana: 5, manaPerRank: 1,
                attack: heavyAttack, coefficient: 1.5f, coefficientPerRank: 0.2f);

            var guard = Skill(GuardPath, GuardId, "Guard", PhysicalSkillKind.Guard);
            Tune(guard, cooldown: 16, duration: 3, range: 0, mana: 8, manaPerRank: 2);

            var quickening = Skill(QuickeningPath, QuickeningId, "Quickening", PhysicalSkillKind.Quickening);
            Tune(quickening, cooldown: 14, duration: 5, range: 0, mana: 6, manaPerRank: 2);

            var arrow = Skill(ArrowRainPath, ArrowRainId, "Arrow Rain", PhysicalSkillKind.ArrowRain);
            Tune(arrow, cooldown: 12, duration: 0.8f, range: 12, mana: 10, manaPerRank: 3,
                attack: arrowAttack, coefficient: 0.5f, coefficientPerRank: 0.08f);
            arrow.arrowRadius = 3;
            arrow.arrowHitCount = 3;
            arrow.arrowHitInterval = 0.35f;
            EditorUtility.SetDirty(arrow);

            var martial = Skill(MartialPath, MartialId, "Martial Aptitude", PhysicalSkillKind.MartialAptitude);
            Tune(martial, cooldown: 0, duration: 0, range: 0, mana: 0, manaPerRank: 0);

            CreatePhysicalPresentation();

            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
            if (player == null) throw new InvalidOperationException("Cornberg player is missing.");
            var playerObject = player.gameObject;

            var resourceController = Ensure<ActorResourceController>(playerObject);
            resourceController.Configure(profile);
            EditorUtility.SetDirty(resourceController);

            var progression = Ensure<PhysicalSkillProgression>(playerObject);
            progression.Configure(heavy, guard, quickening, arrow, martial);
            EditorUtility.SetDirty(progression);

            var caster = Ensure<PhysicalSkillCaster>(playerObject);
            EditorUtility.SetDirty(caster);
            var targeting = Ensure<SkillTargetingController>(playerObject);
            targeting.Configure(player.WorldCamera);
            EditorUtility.SetDirty(targeting);

            var bar = Ensure<PhysicalSkillBar>(playerObject);
            bar.Configure(progression, caster, player.WorldCamera, targeting);
            EditorUtility.SetDirty(bar);

            var panel = Ensure<PhysicalSkillPanel>(playerObject);
            panel.Configure(progression);
            EditorUtility.SetDirty(panel);

            var resourceBar = Ensure<ResourceBar>(playerObject);
            resourceBar.Configure(resourceController);
            EditorUtility.SetDirty(resourceBar);

            var noviceVisual = player.transform.Find("NovicePresentation");
            if (noviceVisual == null) throw new InvalidOperationException("Accepted Novice presentation is missing.");
            var physicalVisual = player.transform.Find("PhysicallyBlessedPresentation");
            if (physicalVisual == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PhysicalPrefabPath);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.name = "PhysicallyBlessedPresentation";
                instance.transform.SetParent(player.transform, false);
                physicalVisual = instance.transform;
            }
            physicalVisual.gameObject.SetActive(false);

            var switcher = Ensure<ClassPresentationSwitcher>(playerObject);
            switcher.Configure(
                new ClassPresentationSwitcher.Binding
                {
                    classId = NoviceSkillProgression.ClassStableId,
                    visualRoot = noviceVisual,
                    animator = noviceVisual.GetComponentInChildren<Animator>(true)
                },
                new ClassPresentationSwitcher.Binding
                {
                    classId = ClassId,
                    visualRoot = physicalVisual,
                    animator = physicalVisual.GetComponentInChildren<Animator>(true)
                });
            EditorUtility.SetDirty(switcher);

            // The authored scene is Novice by default. Ensure presentation state reflects that before save.
            noviceVisual.gameObject.SetActive(true);
            physicalVisual.gameObject.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("DICEFREE_PHYSICAL_AUTHORED_OK: Mana, five skills, adaptive basic attack, dedicated presentation and Cornberg composition.");
            return new
            {
                success = true,
                finalMarker = "DICEFREE_PHYSICAL_AUTHORED_OK",
                skillCount = 5,
                manaMaximumAtLevel1 = profile.Maximum(physical.baseAttributes),
                manaRegenAtLevel1 = profile.Regeneration(physical.baseAttributes)
            };
        }

        private static AttackDefinition Attack(
            string path,
            string id,
            string display,
            float coefficient,
            float interval,
            float windup,
            float reach,
            bool requiresAccuracy)
        {
            var attack = Asset<AttackDefinition>(path);
            attack.stableId = id;
            attack.displayName = display;
            attack.channel = DamageChannel.Physical;
            attack.element = null;
            attack.requiresAccuracy = requiresAccuracy;
            attack.scaling = AttributeScaling.HighestSelected;
            attack.weights = new AttributeValues { strength = 1, agility = 1 };
            attack.baseDamage = 1;
            attack.coefficient = coefficient;
            attack.interval = interval;
            attack.windup = windup;
            attack.reach = reach;
            EditorUtility.SetDirty(attack);
            return attack;
        }

        private static PhysicalSkillDefinition Skill(string path, string id, string display, PhysicalSkillKind kind)
        {
            var skill = Asset<PhysicalSkillDefinition>(path);
            skill.stableId = id;
            skill.displayName = display;
            skill.kind = kind;
            skill.maxRank = 6;
            EditorUtility.SetDirty(skill);
            return skill;
        }

        private static void Tune(
            PhysicalSkillDefinition skill,
            float cooldown,
            float duration,
            float range,
            float mana,
            float manaPerRank,
            AttackDefinition attack = null,
            float coefficient = 0,
            float coefficientPerRank = 0)
        {
            skill.cooldownSeconds = cooldown;
            skill.durationSeconds = duration;
            skill.range = range;
            skill.manaBase = mana;
            skill.manaPerAdditionalRank = manaPerRank;
            skill.attack = attack;
            skill.damageCoefficientRank1 = coefficient;
            skill.damageCoefficientPerAdditionalRank = coefficientPerRank;
            EditorUtility.SetDirty(skill);
        }

        private static void CreatePhysicalPresentation()
        {
            var novice = AssetDatabase.LoadAssetAtPath<GameObject>(NovicePrefabPath);
            if (novice == null) throw new InvalidOperationException("Novice presentation prefab is missing.");

            var root = PrefabUtility.LoadPrefabContents(NovicePrefabPath);
            try
            {
                root.name = "PhysicallyBlessedPresentation";
                root.transform.localScale = Vector3.Scale(root.transform.localScale, new Vector3(1.055f, 1.055f, 1.055f));

                var animator = root.GetComponentInChildren<Animator>(true);
                if (animator == null) throw new InvalidOperationException("Novice-derived Physical presentation has no Animator.");

                foreach (var boneId in new[]
                         {
                             HumanBodyBones.Chest,
                             HumanBodyBones.LeftUpperArm,
                             HumanBodyBones.RightUpperArm,
                             HumanBodyBones.LeftUpperLeg,
                             HumanBodyBones.RightUpperLeg
                         })
                {
                    var bone = animator.GetBoneTransform(boneId);
                    if (bone != null) bone.localScale = Vector3.Scale(bone.localScale, new Vector3(1.025f, 1.025f, 1.025f));
                }

                var sourceTee = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/_DiceFree/Art/Characters/Novice/Materials/Novice_Baseline_Tee.mat");
                var physicalTee = AssetDatabase.LoadAssetAtPath<Material>(PhysicalTeePath);
                if (physicalTee == null)
                {
                    physicalTee = new Material(sourceTee) { name = "PhysicallyBlessed_Tee" };
                    if (physicalTee.HasProperty("_BaseColor"))
                        physicalTee.SetColor("_BaseColor", new Color(0.36f, 0.19f, 0.16f, 1));
                    else
                        physicalTee.color = new Color(0.36f, 0.19f, 0.16f, 1);
                    AssetDatabase.CreateAsset(physicalTee, PhysicalTeePath);
                }

                foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    if (renderer.name.IndexOf("TShirt", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var materials = renderer.sharedMaterials;
                        for (int i = 0; i < materials.Length; i++) materials[i] = physicalTee;
                        renderer.sharedMaterials = materials;
                    }

                PrefabUtility.SaveAsPrefabAsset(root, PhysicalPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
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

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/_DiceFree/Settings", "Skills");
            EnsureFolder("Assets/_DiceFree/Settings/Skills", "Physical");
            EnsureFolder("Assets/_DiceFree/Settings", "Resources");
            EnsureFolder("Assets/_DiceFree/Art/Characters", "PhysicallyBlessed");
            EnsureFolder(ArtFolder, "Materials");
            EnsureFolder(ArtFolder, "Prefabs");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
