using System;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class CornbergRoadSetup
    {
        private const string Data = "Assets/_DiceFree/Settings/Enemies";
        [MenuItem("DiceFree/Quests/Add Q2 to saved Cornberg")]
        public static void Install()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player = UnityEngine.Object.FindAnyObjectByType<TraversalInput>();
            var giver = UnityEngine.Object.FindAnyObjectByType<QuestGiver>();
            GameObject crop = null;
            foreach (var actor in UnityEngine.Object.FindObjectsByType<ActorStats>())
                if (actor.Definition.stableId == "enemy.crop-slime") crop = actor.gameObject;
            if (crop == null) throw new InvalidOperationException("Existing Crop Slime required.");
            if (!AssetDatabase.IsValidFolder(Data)) AssetDatabase.CreateFolder("Assets/_DiceFree/Settings", "Enemies");
            if (!AssetDatabase.IsValidFolder("Assets/_DiceFree/Prefabs")) AssetDatabase.CreateFolder("Assets/_DiceFree", "Prefabs");
            var archetype = Asset<EnemyArchetype>("Slime archetype", value => value.familyId = "enemy-family.slime");
            var cropVariant = Asset<EnemyVariantDefinition>("Crop Slime variant", value => {
                value.archetype = archetype; value.stats = crop.GetComponent<ActorStats>().Definition;
            });
            var bump = Asset<AttackDefinition>("Road Slime bump", value => {
                value.stableId = "attack.road-slime.bump"; value.displayName = "Bump";
                value.baseDamage = 6; value.coefficient = 0; value.interval = 1.7f; value.windup = 0.45f; value.reach = 0.6f;
            });
            var roadStats = Asset<ActorDefinition>("Road Slime combat stats", value => {
                value.stableId = "enemy.road-slime"; value.displayName = "Road Slime"; value.familyId = archetype.familyId;
                value.baseAttributes = new AttributeValues(0); value.growth = new AttributeValues(0);
                value.baseHp = 30; value.moveSpeed = 3.6f; value.basicAttack = bump; value.experienceReward = 20;
                value.tuning = crop.GetComponent<ActorStats>().Definition.tuning;
            });
            var roadVariant = Asset<EnemyVariantDefinition>("Road Slime variant", value => {
                value.archetype = archetype; value.stats = roadStats; value.level = 3; value.bodyScale = 1.25f;
                value.overrideAwareness = value.overrideLeash = true; value.awareness = 8; value.leash = 16;
            });
            if (!crop.TryGetComponent<EnemyVariant>(out var cropBinding))
            {
                cropBinding = crop.AddComponent<EnemyVariant>();
                cropBinding.Configure(cropVariant, crop.transform.Find("Slime body"));
            }
            if (archetype.prefab == null)
            {
                var template = UnityEngine.Object.Instantiate(crop); template.name = "Slime base";
                archetype.prefab = PrefabUtility.SaveAsPrefabAsset(template, "Assets/_DiceFree/Prefabs/Slime base.prefab");
                UnityEngine.Object.DestroyImmediate(template); EditorUtility.SetDirty(archetype);
            }
            var road = GameObject.Find("Road Slime - combat placeholder");
            if (road == null)
            {
                road = (GameObject)PrefabUtility.InstantiatePrefab(archetype.prefab);
                road.name = "Road Slime - combat placeholder"; road.transform.position = new Vector3(64, 0, 34);
                road.GetComponent<EnemyVariant>().Configure(roadVariant);
                PrefabUtility.RecordPrefabInstancePropertyModifications(road.GetComponent<EnemyVariant>());
            }
            road.GetComponent<EnemyVariant>().Configure(roadVariant);
            foreach (var component in road.GetComponentsInChildren<Component>())
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            var quest = Asset<QuestDefinition>("Cornberg road investigation", value => {
                value.stableId = "quest.cornberg.investigate-road"; value.title = "Check the Road";
                value.completedQuestIds = new[] { giver.Quest.stableId }; value.rewardXp = 50;
                value.offer = "These Slimes are behaving strangely. Check the marked bend along the northeast road, defeat three Road Slimes, then return.";
                value.locationHint = "Speak to the farmer, then follow the northeast road to the trail marker.";
                value.stages = new[] {
                    new QuestObjective { kind = ObjectiveKind.ReachArea, areaId = "area.cornberg.road-investigation", count = 1,
                        instruction = "Reach the marked northeast road bend" },
                    new QuestObjective { contentId = roadStats.stableId, familyId = archetype.familyId, count = 3,
                        instruction = "Defeat Road Slimes, then return to the farmer" }
                };
            });
            player.GetComponent<QuestJournal>().Configure(giver.Quest, quest);
            giver.ConfigureAdditional(quest);
            var marker = GameObject.Find("Road investigation marker");
            if (marker == null)
            {
                marker = new GameObject("Road investigation marker"); marker.transform.position = new Vector3(57, 0, 26);
                marker.AddComponent<ReachArea>().Configure("area.cornberg.road-investigation", 4);
                var post = GameObject.CreatePrimitive(PrimitiveType.Cube); post.name = "Trail marker post";
                post.transform.SetParent(marker.transform); post.transform.localPosition = new Vector3(2, 0.8f, 0);
                post.transform.localScale = new Vector3(0.2f, 1.6f, 0.2f); post.layer = 2;
                UnityEngine.Object.DestroyImmediate(post.GetComponent<Collider>());
                post.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Materials/Blockout/Timber.mat");
                var label = new GameObject("Road marker label"); label.transform.SetParent(marker.transform);
                label.transform.localPosition = new Vector3(2, 2, 0); label.transform.rotation = Quaternion.Euler(0, 225, 0);
                var text = label.AddComponent<TextMesh>(); text.text = "Investigate road"; text.anchor = TextAnchor.MiddleCenter;
                text.characterSize = 0.12f; text.fontSize = 40;
            }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("CORNBERG_ROAD_SETUP_OK: additive Q2, ReachArea and shared Slime archetype/variant.");
        }
        private static T Asset<T>(string name, Action<T> initialize) where T : ScriptableObject
        {
            string path = Data + "/" + name + ".asset";
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            if (value != null) return value;
            value = ScriptableObject.CreateInstance<T>(); initialize(value); AssetDatabase.CreateAsset(value, path); return value;
        }
    }
}
