using System;
using System.Linq;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.EditorTools
{
    // Adds content to the saved scene. Never recreates geography or bakes navigation.
    public static class CornbergSurgeSetup
    {
        public const string QuestId = "quest.cornberg.break-slime-surge";
        public const string FarmerId = "npc.cornberg.surge-farmer";
        public const string EliteId = "enemy.forest-elite-slime";
        public const string EligibleTag = "quest-credit.cornberg.surge-slime";
        private const string Data = "Assets/_DiceFree/Settings/Enemies/";
        public static void Install()
        {
            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
            var journal = player.GetComponent<QuestJournal>();
            var road = AssetDatabase.LoadAssetAtPath<EnemyVariantDefinition>(Data + "Road Slime variant.asset");
            var forest = AssetDatabase.LoadAssetAtPath<EnemyVariantDefinition>(Data + "Named Forest Slime variant.asset");
            foreach (var stats in new[] { road.stats, forest.stats })
            {
                stats.tags = (stats.tags ?? Array.Empty<string>()).Concat(new[] { EligibleTag }).Distinct().ToArray();
                EditorUtility.SetDirty(stats);
            }
            var gloves = AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/_DiceFree/Settings/Items/Cornberg Work Gloves.asset");
            var shoes = AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/_DiceFree/Settings/Items/Forest Travel Shoes.asset");
            var reward = Asset<FixedRewardDefinition>("Surge reward", value => { value.gold = 100; value.items = new[] { gloves }; });
            var quest = Asset<QuestDefinition>("Cornberg break Slime surge", value => {
                value.stableId = QuestId; value.title = "Break the Slime Surge";
                value.completedQuestIds = new[] { CornbergRunnerSetup.BeatId };
                value.rewardXp = 200; value.reward = reward;
                value.offer = "The runner couldn't get through. We need to thin these Slimes so the road is safer again. Defeat the dangerous Slime somewhere in the Slime Forest, or take down thirty of the stronger Slimes beyond the crop fields. Either will help. Then come back to me.";
                value.locationHint = "Speak to Mira in Cornberg.";
                value.stages = new[] { new QuestObjective { kind = ObjectiveKind.Any, objectiveId = "surge.any", count = 1,
                    alternatives = new[] {
                        new QuestObjective { objectiveId = "surge.elite", contentId = EliteId, count = 1,
                            instruction = "Defeat the dangerous Slime somewhere in the Slime Forest", creditMode = QuestCreditMode.NearbyParty, creditRadius = 50 },
                        new QuestObjective { objectiveId = "surge.population", requiredTag = EligibleTag, count = 30,
                            instruction = "Defeat stronger Slimes", creditMode = QuestCreditMode.NearbyParty, creditRadius = 50 }
                    } } };
            });
            if (!journal.Definitions.Contains(quest)) journal.Configure(journal.Definitions.Concat(new[] { quest }).ToArray());
            var attack = Asset<AttackDefinition>("Elite Slime bump", value => {
                value.stableId = "attack.forest-elite-slime.bump"; value.displayName = "Bump";
                value.baseDamage = 12; value.coefficient = 0; value.interval = 1.6f; value.windup = .5f; value.reach = .6f;
            });
            var eliteStats = Asset<ActorDefinition>("Elite Forest Slime combat stats", value => {
                value.stableId = EliteId; value.displayName = "Dangerous Forest Slime"; value.familyId = road.archetype.familyId;
                value.tuning = road.stats.tuning; value.baseAttributes = new AttributeValues(0); value.growth = new AttributeValues(0);
                value.baseHp = 240; value.moveSpeed = 3.4f; value.basicAttack = attack; value.experienceReward = 80;
                value.tags = new[] { EligibleTag };
            });
            var variant = Asset<EnemyVariantDefinition>("Elite Forest Slime variant", value => {
                value.archetype = road.archetype; value.stats = eliteStats; value.level = 8; value.bodyScale = 2;
                value.overrideAwareness = value.overrideLeash = true; value.awareness = 8; value.leash = 16;
            });
            var respawn = Asset<RespawnDefinition>("Elite Forest Slime respawn", value => value.delaySeconds = 450);
            var nav = NavMesh.AddNavMeshData(UnityEngine.Object.FindAnyObjectByType<WorldNavigation>().Data);
            try
            {
                if (!NavMesh.SamplePosition(new Vector3(77, 0, 53), out var eliteHit, 2, 1)) throw new InvalidOperationException("Deep forest clearing not navigable.");
                if (GameObject.Find("Elite Forest Slime - combat placeholder") == null)
                {
                    var root = (GameObject)PrefabUtility.InstantiatePrefab(road.archetype.prefab);
                    root.name = "Elite Forest Slime - combat placeholder"; root.transform.position = eliteHit.position;
                    root.GetComponent<EnemyVariant>().Configure(variant); root.GetComponent<OverworldRespawn>().Configure(respawn);
                    var drop = root.AddComponent<FixedWorldDrop>(); drop.item = shoes; drop.chance = .2f;
                    foreach (var component in root.GetComponentsInChildren<Component>()) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                }
                else
                {
                    var eliteRoot = GameObject.Find("Elite Forest Slime - combat placeholder");
                    eliteRoot.transform.position = eliteHit.position;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(eliteRoot.transform);
                }
                if (GameObject.Find("Mira - provisional surge farmer") == null)
                {
                    if (!NavMesh.SamplePosition(new Vector3(12, 0, -8), out var hit, 2, 1)) throw new InvalidOperationException("Farmer position not navigable.");
                    var root = new GameObject("Mira - provisional surge farmer"); root.transform.position = hit.position;
                    var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); body.transform.SetParent(root.transform);
                    body.transform.localPosition = Vector3.up * .9f; body.transform.localScale = new Vector3(.8f, .9f, .8f); body.layer = 11;
                    body.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Materials/Blockout/Timber.mat");
                    var giver = root.AddComponent<QuestGiver>(); giver.Configure(quest); giver.ConfigureName("Mira"); giver.ConfigureAmbient(true);
                    var bark = root.AddComponent<AmbientBark>(); bark.npcId = FarmerId; bark.listener = player;
                    bark.lines = new[] { "Looks like a good day for the fields.", "There's always another fence to mend.", "The soil could use a little rain." };
                    root.AddComponent<InteractionPrompt>().Configure(giver, player);
                }
            }
            finally { nav.Remove(); }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("CORNBERG_SURGE_SETUP_OK: additive Q4 farmer and elite; navigation preserved.");
        }
        private static T Asset<T>(string name, Action<T> initialize) where T : ScriptableObject
        {
            string path = Data + name + ".asset";
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            if (value != null) return value;
            value = ScriptableObject.CreateInstance<T>(); initialize(value); AssetDatabase.CreateAsset(value, path); return value;
        }
    }
}
