using System;
using System.Linq;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.EditorTools
{
    // Additive content authoring only. Never recreates the scene or bakes navigation.
    public static class CornbergForestSetup
    {
        private const string Data = "Assets/_DiceFree/Settings/Enemies/";
        [MenuItem("DiceFree/Quests/Add Q3 to saved Cornberg")]
        public static void Install()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
            var giver = UnityEngine.Object.FindAnyObjectByType<QuestGiver>();
            var road = AssetDatabase.LoadAssetAtPath<EnemyVariantDefinition>(Data + "Road Slime variant.asset");
            var q2 = AssetDatabase.LoadAssetAtPath<QuestDefinition>(Data + "Cornberg road investigation.asset");
            if (road == null || q2 == null) throw new InvalidOperationException("Existing Q2 required.");
            var attack = Asset<AttackDefinition>("Forest Slime bump", value => {
                value.stableId = "attack.named-forest-slime.bump"; value.displayName = "Bump";
                value.baseDamage = 8; value.coefficient = 0; value.interval = 1.8f; value.windup = .5f; value.reach = .6f;
            });
            var stats = Asset<ActorDefinition>("Named Forest Slime combat stats", value => {
                value.stableId = "enemy.named-forest-slime"; value.displayName = "Named Forest Slime";
                value.familyId = road.archetype.familyId; value.tuning = road.stats.tuning;
                value.baseAttributes = new AttributeValues(0); value.growth = new AttributeValues(0);
                value.baseHp = 60; value.moveSpeed = 3.4f; value.basicAttack = attack; value.experienceReward = 35;
            });
            var variant = Asset<EnemyVariantDefinition>("Named Forest Slime variant", value => {
                value.archetype = road.archetype; value.stats = stats; value.level = 5; value.bodyScale = 1.6f;
                value.overrideAwareness = value.overrideLeash = true; value.awareness = 8; value.leash = 14;
            });
            var nav = NavMesh.AddNavMeshData(UnityEngine.Object.FindAnyObjectByType<WorldNavigation>().Data);
            Vector3 position;
            try
            {
                if (!NavMesh.SamplePosition(new Vector3(86, 0, -3), out var hit, 2, 1)) throw new InvalidOperationException("Existing forest clearing not navigable.");
                position = hit.position;
            }
            finally { nav.Remove(); }
            if (GameObject.Find("Named Forest Slime - combat placeholder") == null)
            {
                var actor = (GameObject)PrefabUtility.InstantiatePrefab(road.archetype.prefab);
                actor.name = "Named Forest Slime - combat placeholder"; actor.transform.position = position;
                actor.GetComponent<EnemyVariant>().Configure(variant);
                foreach (var component in actor.GetComponentsInChildren<Component>()) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            }
            var quest = Asset<QuestDefinition>("Cornberg named forest Slime", value => {
                value.stableId = "quest.cornberg.named-slime"; value.title = "Named Slime";
                value.completedQuestIds = new[] { q2.stableId }; value.rewardXp = 35;
                value.offer = "A larger Slime lurks in the southern forest clearing. Follow the east road, take the southern loop, defeat it, then return.";
                value.locationHint = "East road, southern woodland loop. Return to the farmer afterward.";
                value.stages = new[] {
                    new QuestObjective { kind = ObjectiveKind.ReachArea, areaId = "area.cornberg.forest-clearing", count = 1,
                        instruction = "Locate the southern forest clearing" },
                    new QuestObjective { contentId = stats.stableId, familyId = road.archetype.familyId, count = 1,
                        instruction = "Defeat the Named Forest Slime, then return to the farmer" }
                };
            });
            var journal = player.GetComponent<QuestJournal>();
            if (!journal.Definitions.Contains(quest)) journal.Configure(journal.Definitions.Concat(new[] { quest }).ToArray());
            if (!giver.Offers.Contains(quest)) giver.ConfigureAdditional(giver.Offers.Skip(1).Concat(new[] { quest }).ToArray());
            var marker = GameObject.Find("Forest clearing marker");
            if (marker == null)
            {
                marker = new GameObject("Forest clearing marker"); marker.transform.position = position;
                marker.AddComponent<ReachArea>().Configure("area.cornberg.forest-clearing", 10);
                var label = new GameObject("Forest clearing label"); label.transform.SetParent(marker.transform);
                label.transform.localPosition = new Vector3(-6, 2, 0); label.transform.rotation = Quaternion.Euler(0, 225, 0);
                var text = label.AddComponent<TextMesh>(); text.text = "Forest clearing"; text.anchor = TextAnchor.MiddleCenter;
                text.characterSize = .12f; text.fontSize = 40;
            }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("CORNBERG_FOREST_SETUP_OK: additive Q3 and authored forest variant; navigation preserved.");
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
