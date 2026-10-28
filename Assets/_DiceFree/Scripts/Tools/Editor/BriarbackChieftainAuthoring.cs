using System;
using System.Linq;
using DiceFree.AI;
using DiceFree.Briarback;
using DiceFree.Combat;
using DiceFree.Core;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    /// <summary>Additive, preflighted Unity-API authoring for the approved Chieftain encounter.</summary>
    public static class BriarbackChieftainAuthoring
    {
        const string ScenePath = "Assets/_DiceFree/Scenes/Cornberg.unity";
        const string RegionName = "14 - Briarback quilboar frontier";
        const string StrongholdName = "Briarback northern mountain stronghold - additive";
        const string GroupName = "Briarback Chieftain encounter - additive";
        const string Folder = "Assets/_DiceFree/Settings/Briarback";
        const string BackupPath = "Assets/_DiceFree/Settings/Recovery/Cornberg-before-chieftain-encounter-20261028.unity";

        static void Need(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("BRIARBACK CHIEFTAIN: " + message);
        }

        static Transform ExistingGroup(Scene scene) => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .FirstOrDefault(t => t.name == StrongholdName);

        [CliCommand("dicefree.briarback.chieftain.preview", "Preflight Chieftain and guard spawn points in the saved upper Briarback court; no changes.")]
        public static object Preview()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath && !scene.isDirty, "activate and save Cornberg before authoring.");
            var stronghold = ExistingGroup(scene);
            Need(stronghold != null, "the existing additive cliff stronghold is missing.");
            Need(stronghold.Find(GroupName) == null, "an encounter group already exists; inspect it instead of duplicating it.");
            Need(AssetDatabase.LoadMainAssetAtPath(BackupPath) == null, "the recovery path is occupied; inspect it and choose a new dated path.");

            var region = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).First(t => t.name == RegionName);
            var templates = region.Find("Approved Quillboar role prototypes - additive");
            Need(templates != null, "the approved role prototypes are missing.");
            var warrior = templates.Cast<Transform>().FirstOrDefault(t => t.GetComponent<QuilboarCombat>()?.role == QuilboarRole.Warrior);
            var brute = templates.Cast<Transform>().FirstOrDefault(t => t.GetComponent<QuilboarCombat>()?.role == QuilboarRole.Brute);
            Need(warrior != null && brute != null, "the exact elite Warrior/Brute templates are missing.");

            var navigation = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldNavigation>(true)).Single();
            var nav = NavMesh.AddNavMeshData(navigation.Data);
            Vector2[] locations = { new(282, 484), new(277.6f, 482.7f), new(286.4f, 482.7f) };
            try
            {
                var samples = locations.Select(point =>
                {
                    var desired = CornbergLandscape.Ground(point.x, point.y) + Vector3.up * 5.2f;
                    Need(NavMesh.SamplePosition(desired, out var hit, 1.5f, NavMesh.AllAreas), "arena spawn is outside saved NavMesh at " + point);
                    Need(!Physics.CheckCapsule(hit.position + Vector3.up * .5f, hit.position + Vector3.up * 1.65f, .45f,
                        Physics.AllLayers, QueryTriggerInteraction.Ignore), "a sampled spawn has static collision at " + point);
                    return new { requested = point.ToString(), position = hit.position.ToString("F2") };
                }).ToArray();

                return new
                {
                    marker = "DICEFRE_BRIARBACK_CHIEFTAIN_PREFLIGHT_OK",
                    scene = scene.path,
                    roles = new[] { "one level-40 Chieftain", "one level-35 elite Warrior", "one level-35 elite Brute" },
                    spawns = samples,
                    dimensions = new { approvedDiameter = 25, clearFootprint = "25m x 23m" },
                    recoveryCopyWillBeMade = BackupPath,
                    sceneDirty = scene.isDirty
                };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        static T Asset<T>(string name) where T : ScriptableObject
        {
            string path = Folder + "/" + name + ".asset";
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            if (value != null) return value;
            value = ScriptableObject.CreateInstance<T>();
            value.name = name;
            AssetDatabase.CreateAsset(value, path);
            return value;
        }

        static AttackDefinition Attack(string name, string id, string display, float baseDamage, float windup, float reach)
        {
            var attack = Asset<AttackDefinition>(name);
            attack.stableId = id;
            attack.displayName = display;
            attack.channel = DamageChannel.Physical;
            attack.element = null;
            attack.requiresAccuracy = false;
            attack.scaling = AttributeScaling.Highest;
            attack.weights = default;
            attack.baseDamage = baseDamage;
            attack.coefficient = 0;
            attack.minimumDamageMultiplier = 1;
            attack.maximumDamageMultiplier = 1;
            attack.interval = Mathf.Max(.1f, windup);
            attack.windup = windup;
            attack.reach = reach;
            EditorUtility.SetDirty(attack);
            return attack;
        }

        static ActorDefinition Definition(ActorDefinition template, string name, string id, int level, float hp, float damage, float pdef, float mdef)
        {
            var definition = Asset<ActorDefinition>(name);
            definition.stableId = id;
            definition.familyId = "briarback.quilboar";
            definition.displayName = name;
            definition.tags = new[] { "briarback", "quilboar", "elite" };
            definition.experienceReward = 0; // Boss XP is owned by the future approved quest/reward implementation.
            definition.tuning = template.tuning;
            definition.baseAttributes = new AttributeValues { vitality = 10, strength = 10 };
            definition.growth = default;
            definition.baseHp = hp;
            definition.overrideVitalityRegeneration = true;
            definition.regenerationPerVitality = 0;
            definition.moveSpeed = name.Contains("Brute") ? 3.1f : 3.6f;
            definition.physicalDefense = pdef;
            definition.magicalDefense = mdef;
            var basic = Asset<AttackDefinition>(name + " basic attack");
            basic.stableId = "attack." + id + ".basic";
            basic.displayName = "Heavy two-handed axe";
            basic.channel = DamageChannel.Physical;
            basic.element = null;
            basic.requiresAccuracy = true;
            basic.scaling = AttributeScaling.Highest;
            basic.weights = default;
            basic.baseDamage = damage;
            basic.coefficient = 0;
            basic.minimumDamageMultiplier = .9f;
            basic.maximumDamageMultiplier = 1.1f;
            basic.interval = name.Contains("Brute") ? 2.6f : 2.2f;
            basic.windup = name.Contains("Brute") ? 1.2f : .9f;
            basic.reach = name.Contains("Chieftain") ? 1.65f : 1.2f;
            definition.basicAttack = basic;
            EditorUtility.SetDirty(basic);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        static void EquipVisual(Transform model)
        {
            var old = model.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name.Contains("weapon"));
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var hand = model.GetComponentsInChildren<Transform>(true).First(t => t.name == "RightHand");
            var axe = new GameObject("Oversized single-bladed bone-and-metal two-handed axe");
            axe.transform.SetParent(hand, false);
            axe.transform.localPosition = new Vector3(.06f, .52f, .1f);
            axe.transform.localRotation = Quaternion.Euler(0, 0, -17);
            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.name = "Axe haft"; shaft.transform.SetParent(axe.transform, false);
            shaft.transform.localPosition = new Vector3(0, .14f, 0); shaft.transform.localScale = new Vector3(.055f, .66f, .055f);
            var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Single broad metal blade"; blade.transform.SetParent(axe.transform, false);
            blade.transform.localPosition = new Vector3(.14f, .46f, 0); blade.transform.localScale = new Vector3(.29f, .25f, .12f);
            var bone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bone.name = "Bone blade spine"; bone.transform.SetParent(axe.transform, false);
            bone.transform.localPosition = new Vector3(.02f, .46f, 0); bone.transform.localScale = new Vector3(.13f, .38f, .15f);
            foreach (var col in axe.GetComponentsInChildren<Collider>()) UnityEngine.Object.DestroyImmediate(col);
            var metal = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.34f, .37f, .35f, 1) };
            var wood = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.26f, .15f, .08f, 1) };
            shaft.GetComponent<Renderer>().sharedMaterial = wood;
            blade.GetComponent<Renderer>().sharedMaterial = metal;
            bone.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(.79f, .68f, .45f, 1) };
        }

        static GameObject CloneElite(Transform template, Transform group, Vector3 position, string name, ActorDefinition definition, float scale, float respawnSeconds)
        {
            bool isChief = definition.displayName == "Quillboar Chieftain";
            bool isBrute = definition.displayName == "Chieftain elite Brute";
            var clone = UnityEngine.Object.Instantiate(template.gameObject, position, template.rotation, group);
            clone.name = name;
            clone.transform.localScale = Vector3.one * scale;
            var actor = clone.GetComponent<CombatActor>();
            actor.Configure(1, .65f, "briarback.quilboar");
            clone.GetComponent<ActorStats>().Configure(definition, isChief ? 40 : 35);
            clone.GetComponent<CombatStatusController>().ConfigureControl(.2f, false);
            clone.GetComponent<AggroBehaviour>().Configure(isChief ? 12 : 8, isChief ? 30 : 20);
            clone.GetComponent<AggroBehaviour>().ConfigureAutoAggro(new AutoAggroPolicy { alwaysAutoAggro = true });
            var respawn = Asset<RespawnDefinition>(isChief ? "Chieftain respawn" :
                isBrute ? "Chieftain elite Brute respawn" : "Chieftain elite Warrior respawn");
            respawn.delaySeconds = respawnSeconds;
            EditorUtility.SetDirty(respawn);
            var overworld = clone.GetComponent<OverworldRespawn>();
            if (overworld == null) overworld = clone.AddComponent<OverworldRespawn>();
            overworld.Configure(respawn);
            var combat = clone.GetComponent<QuilboarCombat>();
            combat.role = isChief ? QuilboarRole.Chieftain : isBrute ? QuilboarRole.Brute : QuilboarRole.Warrior;
            var agent = clone.GetComponent<NavMeshAgent>();
            agent.enabled = false;
            agent.radius = .65f * scale;
            agent.height = 3.2f * scale;
            var capsule = clone.GetComponent<CapsuleCollider>();
            capsule.radius = .65f; capsule.height = 3.2f; capsule.center = Vector3.up * 1.6f;
            clone.GetComponent<QuilboarPresentation>().model = clone.GetComponentInChildren<Animator>(true);
            clone.SetActive(true);
            return clone;
        }

        [CliCommand("dicefree.briarback.chieftain.author", "Add the approved Chieftain and two elite guards to the saved upper arena using a recovery copy.")]
        public static object Author()
        {
            var preview = Preview();
            var scene = SceneManager.GetActiveScene();
            var region = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).First(t => t.name == RegionName);
            var stronghold = ExistingGroup(scene);
            var templates = region.Find("Approved Quillboar role prototypes - additive");
            var warriorTemplate = templates.Cast<Transform>().First(t => t.GetComponent<QuilboarCombat>()?.role == QuilboarRole.Warrior);
            var bruteTemplate = templates.Cast<Transform>().First(t => t.GetComponent<QuilboarCombat>()?.role == QuilboarRole.Brute);
            var templateDefinition = warriorTemplate.GetComponent<ActorStats>().Definition;
            var warriorDefinition = Definition(templateDefinition, "Chieftain elite Warrior", "briarback.chieftain.guard-warrior", 35, 5600, 180, 26, 18);
            var bruteDefinition = Definition(templateDefinition, "Chieftain elite Brute", "briarback.chieftain.guard-brute", 35, 9000, 270, 22, 14);
            var chiefDefinition = Definition(templateDefinition, "Quillboar Chieftain", "briarback.chieftain", 40, 32000, 320, 38, 30);
            var smash = Attack("Chieftain overhead smash", "attack.briarback.chieftain.overhead-smash", "Ground-Splitting Overhead Smash", 0, 1.5f, 6);
            var axeThrow = Attack("Chieftain axe throw", "attack.briarback.chieftain.axe-throw", "Homing Axe Throw", 0, .65f, 24);

            Need(AssetDatabase.CopyAsset(ScenePath, BackupPath), "could not preserve the complete pre-encounter scene.");
            var navigation = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldNavigation>(true)).Single();
            var nav = NavMesh.AddNavMeshData(navigation.Data);
            Vector3[] positions;
            try
            {
                positions = new[] { new Vector2(282, 484), new Vector2(277.6f, 482.7f), new Vector2(286.4f, 482.7f) }
                    .Select(p =>
                    {
                        Need(NavMesh.SamplePosition(CornbergLandscape.Ground(p.x, p.y) + Vector3.up * 5.2f,
                            out var hit, 1.5f, NavMesh.AllAreas), "spawn sample disappeared at " + p);
                        return hit.position;
                    }).ToArray();
            }
            finally { if (nav.valid) nav.Remove(); }

            var groupObject = new GameObject(GroupName);
            Undo.RegisterCreatedObjectUndo(groupObject, "Author Briarback Chieftain encounter");
            groupObject.transform.SetParent(stronghold, false);
            var chief = CloneElite(warriorTemplate, groupObject.transform, positions[0], "Quillboar Chieftain (Lv 40)", chiefDefinition, 1.7f, 600);
            var warrior = CloneElite(warriorTemplate, groupObject.transform, positions[1], "Elite Warrior bodyguard (Lv 35)", warriorDefinition, 1.25f, 300);
            var brute = CloneElite(bruteTemplate, groupObject.transform, positions[2], "Elite Brute bodyguard (Lv 35)", bruteDefinition, 1.35f, 300);
            var ai = chief.AddComponent<BriarbackChieftainEncounter>();
            ai.overheadSmash = smash; ai.axeThrow = axeThrow;
            ai.eliteWarrior = warrior.GetComponent<CombatActor>(); ai.eliteBrute = brute.GetComponent<CombatActor>();
            ai.smashDamage = 520; ai.axeDamage = 320; ai.axeSpeed = 16; ai.axeMaxRange = 24;
            EquipVisual(chief.GetComponentInChildren<Animator>(true).transform);
            EditorUtility.SetDirty(ai);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            Need(EditorSceneManager.SaveScene(scene), "could not save the authored encounter.");
            return new { marker = "DICEFRE_BRIARBACK_CHIEFTAIN_AUTHORED", scene = scene.path, recoveryCopy = BackupPath,
                actors = new[] { chief.name, warrior.name, brute.name }, positions = positions.Select(p => p.ToString("F2")).ToArray(),
                abilityTuning = "Smash/axe flat values are explicit provisional tuning placeholders; playtest before release.", preflight = preview };
        }

        [CliCommand("dicefree.briarback.chieftain.repair", "Repair only the three authored encounter positions from the verified saved NavMesh; keep the original recovery copy.")]
        public static object RepairPositions()
        {
            Need(!EditorApplication.isPlayingOrWillChangePlaymode, "stop Play Mode first.");
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath, "activate Cornberg first.");
            var region = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).First(t => t.name == RegionName);
            var group = region.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == GroupName);
            Need(group != null && AssetDatabase.LoadMainAssetAtPath(BackupPath) != null, "encounter or original recovery copy missing; no changes made.");
            var chief = group.GetComponentsInChildren<BriarbackChieftainEncounter>(true).Single();
            var guards = new[] { chief.eliteWarrior, chief.eliteBrute };
            Need(chief.GetComponent<CombatActor>() != null && guards.All(g => g != null), "exact three encounter actors are incomplete.");
            var points = new[] { new Vector2(282, 484), new Vector2(277.6f, 482.7f), new Vector2(286.4f, 482.7f) };
            var navigation = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldNavigation>(true)).Single();
            var nav = NavMesh.AddNavMeshData(navigation.Data);
            try
            {
                var samples = points.Select(p =>
                {
                    Need(NavMesh.SamplePosition(CornbergLandscape.Ground(p.x, p.y) + Vector3.up * 5.2f, out var hit, 1.5f, NavMesh.AllAreas),
                        "spawn sample failed at " + p + "; existing recovery copy is preserved.");
                    return hit.position;
                }).ToArray();
                var actors = new[] { chief.GetComponent<CombatActor>(), guards[0], guards[1] };
                Need(actors.Select((actor, index) => (actor, index)).All(pair =>
                    !Physics.CheckCapsule(samples[pair.index] + Vector3.up * .55f, samples[pair.index] + Vector3.up * 1.5f,
                        .35f, 1 << 8, QueryTriggerInteraction.Ignore)), "static geometry blocks a verified spawn point.");
                for (var i = 0; i < actors.Length; i++)
                {
                    Undo.RecordObject(actors[i].transform, "Repair Briarback encounter spawn");
                    actors[i].transform.position = samples[i];
                    EditorUtility.SetDirty(actors[i].transform);
                    bool isChief = i == 0;
                    bool isBrute = i == 2;
                    string definitionName = isChief ? "Quillboar Chieftain" : isBrute ? "Chieftain elite Brute" : "Chieftain elite Warrior";
                    var definition = AssetDatabase.LoadAssetAtPath<ActorDefinition>(Folder + "/" + definitionName + ".asset");
                    Need(definition != null, "saved actor definition missing: " + definitionName);
                    actors[i].GetComponent<ActorStats>().Configure(definition, isChief ? 40 : 35);
                    actors[i].Configure(1, .65f, "briarback.quilboar");
                    actors[i].GetComponent<AggroBehaviour>().Configure(isChief ? 12 : 8, isChief ? 30 : 20);
                    var aggro = actors[i].GetComponent<AggroBehaviour>();
                    aggro?.ConfigureHomePosition(samples[i]);
                    if (actors[i].TryGetComponent(out OverworldRespawn respawn))
                    {
                        var respawnAssetName = isChief ? "Chieftain respawn" : isBrute ? "Chieftain elite Brute respawn" : "Chieftain elite Warrior respawn";
                        var respawnDefinition = Asset<RespawnDefinition>(respawnAssetName);
                        respawnDefinition.delaySeconds = isChief ? 600 : 300;
                        EditorUtility.SetDirty(respawnDefinition);
                        respawn.Configure(respawnDefinition);
                        respawn.ConfigureHomePosition(samples[i]);
                        EditorUtility.SetDirty(respawn);
                    }
                }
                Physics.SyncTransforms();
                EditorSceneManager.MarkSceneDirty(scene);
                Need(EditorSceneManager.SaveScene(scene), "could not save the repaired encounter positions.");
                return new { marker = "DICEFRE_BRIARBACK_CHIEFTAIN_POSITIONS_REPAIRED", positions = samples.Select(p => p.ToString("F2")).ToArray(),
                    recoveryCopyPreserved = BackupPath };
            }
            finally { if (nav.valid) nav.Remove(); }
        }

        [CliCommand("dicefree.briarback.chieftain.validate", "Validate the saved Chieftain encounter roster, attack setup, collision and independent respawn timers.")]
        public static object Validate()
        {
            var scene = SceneManager.GetActiveScene();
            Need(scene.path == ScenePath && !scene.isDirty, "save Cornberg before validating.");
            var region = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).First(t => t.name == RegionName);
            var group = region.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == GroupName);
            Need(group != null, "the additive encounter group is missing.");
            var chief = group.GetComponentsInChildren<BriarbackChieftainEncounter>(true).Single();
            var boss = chief.GetComponent<CombatActor>();
            var bossStats = chief.GetComponent<ActorStats>();
            Need(boss != null && bossStats != null, "the Chieftain is missing its CombatActor or ActorStats component.");
            Need(bossStats.Definition != null && bossStats.Level == 40 && bossStats.Definition.familyId == "briarback.quilboar" && chief.overheadSmash != null && chief.axeThrow != null,
                "boss identity, level, family or attacks are wrong.");
            Need(Mathf.Approximately(chief.overheadSmash.windup, 1.5f) && Mathf.Approximately(chief.SmashCooldownRemaining, 0) &&
                 Mathf.Approximately(chief.axeThrow.interval, .65f), "the authored smash/axe definitions changed unexpectedly.");
            var bossRespawn = boss.GetComponent<OverworldRespawn>();
            Need(bossRespawn != null && bossRespawn.Definition != null, "Chieftain is missing its respawn definition.");
            Need(Mathf.Approximately(bossRespawn.Definition.delaySeconds, 600), "boss respawn must be ten minutes.");
            var guards = new[] { chief.eliteWarrior, chief.eliteBrute };
            Need(guards.All(g => g != null && g.GetComponent<ActorStats>() != null && g.GetComponent<Health>() != null &&
                g.GetComponent<ActorStats>().Level == 35 && g.GetComponent<OverworldRespawn>() != null),
                "both level-35 elite bodyguards must exist with independent respawn components.");
            Need(guards.All(g => g.GetComponent<OverworldRespawn>().Definition != null), "a bodyguard is missing its respawn definition.");
            Need(Mathf.Approximately(guards[0].GetComponent<OverworldRespawn>().Definition.delaySeconds, 300) &&
                 Mathf.Approximately(guards[1].GetComponent<OverworldRespawn>().Definition.delaySeconds, 300), "each bodyguard must respawn after five minutes.");
            var navigation = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<WorldNavigation>(true)).Single();
            var nav = NavMesh.AddNavMeshData(navigation.Data);
            try
            {
            foreach (var actor in new[] { boss, guards[0], guards[1] })
                {
                    Need(NavMesh.SamplePosition(actor.transform.position, out var sample, .25f, NavMesh.AllAreas), "an actor is off NavMesh: " + actor.name);
                    Need(!Physics.CheckCapsule(actor.transform.position + Vector3.up * .55f, actor.transform.position + Vector3.up * 1.5f,
                        .35f, 1 << 8, QueryTriggerInteraction.Ignore), "static collision overlaps actor: " + actor.name);
                }
            }
            finally { if (nav.valid) nav.Remove(); }
            Need(AssetDatabase.LoadMainAssetAtPath(BackupPath) != null, "pre-encounter scene recovery copy is missing.");
            // ActorStats.Stats is only initialized during Play Mode; Edit Mode authoring reads serialized ActorStats directly.
            return new { marker = "DICEFRE_BRIARBACK_CHIEFTAIN_VALIDATION_OK", actors = 3, bossLevel = bossStats.Level,
                smashTelegraphSeconds = chief.overheadSmash.windup, smashCooldownSeconds = 12, axeCooldownSeconds = 15,
                respawns = new { chief = 600, eachElite = 300 }, recoveryCopy = BackupPath,
                damageTuning = new { smash = chief.smashDamage, axe = chief.axeDamage, provisional = true } };
        }
    }
}
