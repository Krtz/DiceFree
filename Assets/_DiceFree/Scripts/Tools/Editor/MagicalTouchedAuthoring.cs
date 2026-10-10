using System;
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
    public static class MagicalTouchedAuthoring
    {
        public const string ClassId = "class.magically-touched-novice";
        public const string Root = "Assets/_DiceFree";
        public const string SkillFolder = Root + "/Settings/Skills/Magical";
        public const string ElementFolder = Root + "/Settings/Combat/Elements";
        public const string ResourceFolder = Root + "/Settings/Resources";
        public const string ArtFolder = Root + "/Art/Characters/MagicallyTouched";
        public const string MagicalClassPath = Root + "/Settings/Progression/Magically Touched Novice shell.asset";
        public const string MagicalManaPath = ResourceFolder + "/Magically Touched Mana.asset";

        public const string BasicAttackPath = SkillFolder + "/Magical Basic Attack.asset";
        public const string SandAttackPath = SkillFolder + "/Nature Magic Sand Attack.asset";
        public const string FireAugmentAttackPath = SkillFolder + "/Fire Imbuement Attack.asset";
        public const string IceAttackPath = SkillFolder + "/Ice Burst Attack.asset";
        public const string SandPath = SkillFolder + "/Magic Sand.asset";
        public const string MendPath = SkillFolder + "/Mend.asset";
        public const string FirePath = SkillFolder + "/Fire Elemental Imbuement.asset";
        public const string IcePath = SkillFolder + "/Ice Burst.asset";
        public const string AttunementPath = SkillFolder + "/Mana Attunement.asset";

        public const string NaturePath = ElementFolder + "/Nature.asset";
        public const string FireElementPath = ElementFolder + "/Fire.asset";
        public const string IceElementPath = ElementFolder + "/Ice.asset";

        public const string MagicalPrefabPath = ArtFolder + "/Prefabs/MagicallyTouchedPresentation.prefab";
        public const string MagicalTeePath = ArtFolder + "/Materials/MagicallyTouched_Tee.mat";
        private const string NovicePrefabPath = Root + "/Art/Characters/Novice/Prefabs/NovicePresentation.prefab";

        public const string SandId = "skill.magical.magic-sand";
        public const string MendId = "skill.magical.mend";
        public const string FireId = "skill.magical.fire-imbuement";
        public const string IceId = "skill.magical.ice-burst";
        public const string AttunementId = "skill.magical.mana-attunement";

        [MenuItem("DiceFree/Progression/Author Magically Touched")]
        [CliCommand("dicefree.magical.prepare", "Author the Magically Touched Tier-1 data, presentation and Cornberg runtime composition.", Tags = new[] { "progression", "magical" })]
        public static object Prepare()
        {
            EnsureFolders();

            var magical = AssetDatabase.LoadAssetAtPath<ActorDefinition>(MagicalClassPath);
            if (magical == null) throw new InvalidOperationException("Magical class shell from #50 is missing.");

            var mana = AssetDatabase.LoadAssetAtPath<ResourceDefinition>(PhysicalBlessedAuthoring.ManaPath);
            if (mana == null) throw new InvalidOperationException("Shared Mana definition from #51 is missing.");

            var profile = Asset<ClassResourceProfile>(MagicalManaPath);
            profile.stableId = "resource-profile.magical.mana";
            profile.classId = ClassId;
            profile.resource = mana;
            profile.baseMaximum = 80;
            profile.maximumPerIntelligence = 2;
            profile.maximumPerSpirit = 1;
            profile.baseRegenPerSecond = 5;
            profile.regenPerIntelligence = 0.15f;
            profile.regenPerSpirit = 0.15f;
            profile.initialFraction = 1;
            profile.loadPolicy = ResourceRetentionPolicy.Persist;
            EditorUtility.SetDirty(profile);

            var nature = Element(NaturePath, "element.nature", "Nature");
            var fire = Element(FireElementPath, "element.fire", "Fire");
            var ice = Element(IceElementPath, "element.ice", "Ice");

            var basic = Attack(
                BasicAttackPath, "attack.magical.basic", "Arcane Spark",
                DamageChannel.Magical, null, AttributeScaling.HighestSelected,
                new AttributeValues { intelligence = 1, spirit = 1 },
                baseDamage: 1, coefficient: 1.25f, interval: 1.15f, windup: 0.3f, reach: 8, requiresAccuracy: true);
            magical.basicAttackMinimumOffset = -4f;
            magical.basicAttackMaximumOffset = 4f;
            magical.basicAttack = basic;
            EditorUtility.SetDirty(magical);

            var sandAttack = Attack(
                SandAttackPath, "attack.magical.magic-sand", "Nature Magic Sand",
                DamageChannel.Magical, nature, AttributeScaling.Weighted,
                new AttributeValues { intelligence = 1 },
                baseDamage: 1, coefficient: 1.4f, interval: 1, windup: 0, reach: 12, requiresAccuracy: false);

            var fireAugment = Attack(
                FireAugmentAttackPath, "attack.magical.fire-imbuement", "Fire Imbuement",
                DamageChannel.Magical, fire, AttributeScaling.Highest,
                default,
                baseDamage: 0, coefficient: 0.25f, interval: 1, windup: 0, reach: 0, requiresAccuracy: false);

            var iceAttack = Attack(
                IceAttackPath, "attack.magical.ice-burst", "Ice Burst",
                DamageChannel.Magical, ice, AttributeScaling.Weighted,
                new AttributeValues { intelligence = 1 },
                baseDamage: 1, coefficient: 1, interval: 1, windup: 0, reach: 12, requiresAccuracy: false);

            var sand = Skill(SandPath, SandId, "Magic Sand", MagicalSkillKind.MagicSand);
            Tune(sand, cooldown: 7, duration: 5, range: 12,
                mana1: 10, linear: 6, quadratic: 2.4f,
                attack: sandAttack, coefficient: 1.4f, coefficientPerRank: 0.18f);

            var mend = Skill(MendPath, MendId, "Mend", MagicalSkillKind.Mend);
            Tune(mend, cooldown: 10, duration: 0, range: 12,
                mana1: 12, linear: 4, quadratic: 1,
                healCoefficient: 8, healCoefficientPerRank: 2);

            var imbuement = Skill(FirePath, FireId, "Fire Elemental Imbuement", MagicalSkillKind.FireImbuement);
            Tune(imbuement, cooldown: 18, duration: 8, range: 12,
                mana1: 15, linear: 5, quadratic: 1.4f,
                secondaryAttack: fireAugment, coefficient: 0.25f, coefficientPerRank: 0.05f);

            var iceBurst = Skill(IcePath, IceId, "Ice Burst", MagicalSkillKind.IceBurst);
            Tune(iceBurst, cooldown: 12, duration: 4, range: 12,
                mana1: 18, linear: 6, quadratic: 2.08f,
                attack: iceAttack, coefficient: 1, coefficientPerRank: 0.15f);
            iceBurst.iceDelaySeconds = 0.75f;
            iceBurst.iceRadius = 3.5f;
            iceBurst.slowDurationSeconds = 4;
            EditorUtility.SetDirty(iceBurst);

            var attunement = Skill(AttunementPath, AttunementId, "Mana Attunement", MagicalSkillKind.ManaAttunement);
            Tune(attunement, cooldown: 0, duration: 0, range: 0,
                mana1: 0, linear: 0, quadratic: 0);
            attunement.maximumManaPerRank = 20;
            attunement.personalRegenPerRank = 0.75f;
            attunement.auraRegenPerRank = 0.25f;
            attunement.auraRadius = 8;
            EditorUtility.SetDirty(attunement);

            CreateMagicalPresentation();

            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
            if (player == null) throw new InvalidOperationException("Cornberg player is missing.");
            var playerObject = player.gameObject;

            var resourceController = Ensure<ActorResourceController>(playerObject);
            var physicalProfile = AssetDatabase.LoadAssetAtPath<ClassResourceProfile>(PhysicalBlessedAuthoring.PhysicalManaPath);
            if (physicalProfile == null) throw new InvalidOperationException("Physical Mana profile from #51 is missing.");
            resourceController.Configure(physicalProfile, profile);
            EditorUtility.SetDirty(resourceController);

            Ensure<ResourceRegenerationAura>(playerObject);

            var progression = Ensure<MagicalSkillProgression>(playerObject);
            progression.Configure(sand, mend, imbuement, iceBurst, attunement);
            EditorUtility.SetDirty(progression);

            var caster = Ensure<MagicalSkillCaster>(playerObject);
            EditorUtility.SetDirty(caster);
            var targeting = Ensure<SkillTargetingController>(playerObject);
            targeting.Configure(player.WorldCamera);
            EditorUtility.SetDirty(targeting);

            var bar = Ensure<MagicalSkillBar>(playerObject);
            bar.Configure(progression, caster, player.WorldCamera, targeting);
            EditorUtility.SetDirty(bar);

            var panel = Ensure<MagicalSkillPanel>(playerObject);
            panel.Configure(progression);
            EditorUtility.SetDirty(panel);

            var resourceBar = Ensure<ResourceBar>(playerObject);
            resourceBar.Configure(resourceController);
            EditorUtility.SetDirty(resourceBar);

            var noviceVisual = player.transform.Find("NovicePresentation");
            var physicalVisual = player.transform.Find("PhysicallyBlessedPresentation");
            if (noviceVisual == null || physicalVisual == null)
                throw new InvalidOperationException("Accepted Novice/Physical presentations are missing.");

            var magicalVisual = player.transform.Find("MagicallyTouchedPresentation");
            if (magicalVisual == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MagicalPrefabPath);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.name = "MagicallyTouchedPresentation";
                instance.transform.SetParent(player.transform, false);
                magicalVisual = instance.transform;
            }
            magicalVisual.gameObject.SetActive(false);

            var switcher = Ensure<ClassPresentationSwitcher>(playerObject);
            switcher.Configure(
                Binding(NoviceSkillProgression.ClassStableId, noviceVisual),
                Binding(PhysicalBlessedAuthoring.ClassId, physicalVisual),
                Binding(ClassId, magicalVisual));
            EditorUtility.SetDirty(switcher);

            noviceVisual.gameObject.SetActive(true);
            physicalVisual.gameObject.SetActive(false);
            magicalVisual.gameObject.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("DICEFREE_MAGICAL_AUTHORED_OK: larger Mana economy, five skills, magical basic attack, dedicated presentation and Cornberg composition.");
            return new
            {
                success = true,
                finalMarker = "DICEFREE_MAGICAL_AUTHORED_OK",
                skillCount = 5,
                manaMaximumAtLevel1 = profile.Maximum(magical.baseAttributes),
                manaRegenAtLevel1 = profile.Regeneration(magical.baseAttributes),
                magicSandRank6Cost = sand.ManaCost(6),
                iceBurstRank6Cost = iceBurst.ManaCost(6)
            };
        }

        private static ClassPresentationSwitcher.Binding Binding(string classId, Transform root) => new()
        {
            classId = classId,
            visualRoot = root,
            animator = root.GetComponentInChildren<Animator>(true)
        };

        private static ElementDefinition Element(string path, string id, string display)
        {
            var element = Asset<ElementDefinition>(path);
            element.stableId = id;
            element.displayName = display;
            EditorUtility.SetDirty(element);
            return element;
        }

        private static AttackDefinition Attack(
            string path,
            string id,
            string display,
            DamageChannel channel,
            ElementDefinition element,
            AttributeScaling scaling,
            AttributeValues weights,
            float baseDamage,
            float coefficient,
            float interval,
            float windup,
            float reach,
            bool requiresAccuracy)
        {
            var attack = Asset<AttackDefinition>(path);
            attack.stableId = id;
            attack.displayName = display;
            attack.channel = channel;
            attack.element = element;
            attack.requiresAccuracy = requiresAccuracy;
            attack.scaling = scaling;
            attack.weights = weights;
            attack.baseDamage = baseDamage;
            attack.coefficient = coefficient;
            attack.interval = interval;
            attack.windup = windup;
            attack.reach = reach;
            EditorUtility.SetDirty(attack);
            return attack;
        }

        private static MagicalSkillDefinition Skill(
            string path,
            string id,
            string display,
            MagicalSkillKind kind)
        {
            var skill = Asset<MagicalSkillDefinition>(path);
            skill.stableId = id;
            skill.displayName = display;
            skill.kind = kind;
            skill.maxRank = 6;
            EditorUtility.SetDirty(skill);
            return skill;
        }

        private static void Tune(
            MagicalSkillDefinition skill,
            float cooldown,
            float duration,
            float range,
            float mana1,
            float linear,
            float quadratic,
            AttackDefinition attack = null,
            AttackDefinition secondaryAttack = null,
            float coefficient = 0,
            float coefficientPerRank = 0,
            float healCoefficient = 0,
            float healCoefficientPerRank = 0)
        {
            skill.cooldownSeconds = cooldown;
            skill.durationSeconds = duration;
            skill.range = range;
            skill.manaCostRank1 = mana1;
            skill.manaCostLinear = linear;
            skill.manaCostQuadratic = quadratic;
            skill.attack = attack;
            skill.secondaryAttack = secondaryAttack;
            skill.coefficientRank1 = coefficient;
            skill.coefficientPerRank = coefficientPerRank;
            skill.healCoefficientRank1 = healCoefficient;
            skill.healCoefficientPerRank = healCoefficientPerRank;
            EditorUtility.SetDirty(skill);
        }

        private static void CreateMagicalPresentation()
        {
            var novice = AssetDatabase.LoadAssetAtPath<GameObject>(NovicePrefabPath);
            if (novice == null) throw new InvalidOperationException("Novice presentation prefab is missing.");

            var root = PrefabUtility.LoadPrefabContents(NovicePrefabPath);
            try
            {
                root.name = "MagicallyTouchedPresentation";
                root.transform.localScale = Vector3.Scale(root.transform.localScale, new Vector3(0.965f, 0.965f, 0.965f));

                var animator = root.GetComponentInChildren<Animator>(true);
                if (animator == null) throw new InvalidOperationException("Novice-derived Magical presentation has no Animator.");

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
                    if (bone != null)
                        bone.localScale = Vector3.Scale(bone.localScale, new Vector3(0.985f, 0.985f, 0.985f));
                }

                var sourceTee = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/_DiceFree/Art/Characters/Novice/Materials/Novice_Baseline_Tee.mat");
                var magicalTee = AssetDatabase.LoadAssetAtPath<Material>(MagicalTeePath);
                if (magicalTee == null)
                {
                    magicalTee = new Material(sourceTee) { name = "MagicallyTouched_Tee" };
                    var tone = new Color(0.18f, 0.23f, 0.38f, 1);
                    if (magicalTee.HasProperty("_BaseColor")) magicalTee.SetColor("_BaseColor", tone);
                    else magicalTee.color = tone;
                    AssetDatabase.CreateAsset(magicalTee, MagicalTeePath);
                }

                foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    if (renderer.name.IndexOf("TShirt", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        var materials = renderer.sharedMaterials;
                        for (int i = 0; i < materials.Length; i++) materials[i] = magicalTee;
                        renderer.sharedMaterials = materials;
                    }

                PrefabUtility.SaveAsPrefabAsset(root, MagicalPrefabPath);
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
            EnsureFolder(Root + "/Settings/Skills", "Magical");
            EnsureFolder(Root + "/Settings/Combat", "Elements");
            EnsureFolder(Root + "/Art/Characters", "MagicallyTouched");
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
