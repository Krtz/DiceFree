using System;
using DiceFree.AI;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Progression;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class CornbergQuestSetup
    {
        private const string Data="Assets/_DiceFree/Settings/Progression";
        [MenuItem("DiceFree/Quests/Add Q1 to saved Cornberg")]
        public static void Install()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene=EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player=UnityEngine.Object.FindAnyObjectByType<TraversalInput>().gameObject;
            var enemy=UnityEngine.Object.FindAnyObjectByType<AggroBehaviour>().gameObject;
            if (!AssetDatabase.IsValidFolder(Data)) AssetDatabase.CreateFolder("Assets/_DiceFree/Settings","Progression");
            var curve=Asset<ExperienceCurve>("Starter XP curve",_=>{});
            var respawn=Asset<RespawnDefinition>("Ordinary overworld respawn",_=>{});
            var quest=Asset<QuestDefinition>("Cornberg crop Slimes",value=> {
                value.stableId="quest.cornberg.crop-slimes"; value.title="Crop Slimes"; value.rewardXp=20;
                value.offer="Those Slimes are ruining the northern fields, north of Cornberg. Follow the north path, clear three first, then two more, and come back to me.";
                value.locationHint="Follow the north path to the northern fields north of Cornberg. Return to the former swordswoman/farmer in town.";
                value.stages=new[] {
                    new QuestObjective { contentId="enemy.crop-slime",count=3,instruction="Defeat crop Slimes north of Cornberg" },
                    new QuestObjective { contentId="enemy.crop-slime",count=2,instruction="Defeat two more crop Slimes north of Cornberg" }
                };
            });
            var enemyData=enemy.GetComponent<ActorStats>().Definition;
            enemyData.familyId="enemy-family.slime"; enemyData.experienceReward=10; EditorUtility.SetDirty(enemyData);
            Ensure<DefeatReporter>(player); Ensure<DefeatReporter>(enemy);
            Ensure<OverworldRespawn>(enemy).Configure(respawn);
            Ensure<KillCreditReceiver>(player);
            var xp=Ensure<ExperienceProgression>(player); xp.Configure(curve);
            var journal=Ensure<QuestJournal>(player); journal.Configure(quest);
            var interactor=Ensure<Interactor>(player);
            ConfigureLayer();
            var farmer=GameObject.Find("Former swordswoman - farmer");
            if (farmer == null)
            {
                farmer=GameObject.CreatePrimitive(PrimitiveType.Capsule); farmer.name="Former swordswoman - farmer";
                farmer.transform.position=new Vector3(32,0.9f,2); farmer.transform.localScale=new Vector3(0.8f,0.9f,0.8f);
                // Primitive material is an explicit placeholder; no combat actor or bespoke click input.
                farmer.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Materials/Blockout/Timber.mat");
            }
            farmer.layer=11;
            var giver=Ensure<QuestGiver>(farmer); giver.ConfigureName("Former swordswoman / farmer"); giver.Configure(quest);
            var approach=farmer.transform.Find("Interaction approach");
            if(approach==null)
            {
                approach=new GameObject("Interaction approach").transform; approach.SetParent(farmer.transform);
                approach.localPosition=new Vector3(0,-1,0);
            }
            giver.ConfigureApproach(approach);
            Ensure<InteractionPrompt>(farmer).Configure(giver,player.GetComponent<CombatActor>());
            Ensure<ExperienceBar>(Named("HUD - experience")).Configure(xp);
            Ensure<QuestTracker>(Named("HUD - quests")).Configure(journal);
            Ensure<QuestInteractionPanel>(Named("HUD - quest interaction")).Configure(interactor);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("CORNBERG_QUEST_SETUP_OK: additive Q1, XP, defeat credit and timed respawn; combat tuning preserved.");
        }
        private static T Ensure<T>(GameObject obj) where T:Component => obj.TryGetComponent<T>(out var value)?value:obj.AddComponent<T>();
        private static GameObject Named(string name)=>GameObject.Find(name)??new GameObject(name);
        private static T Asset<T>(string name,Action<T> initialize) where T:ScriptableObject
        {
            var path=Data+"/"+name+".asset"; var value=AssetDatabase.LoadAssetAtPath<T>(path);
            if(value!=null)return value;
            value=ScriptableObject.CreateInstance<T>(); initialize(value); AssetDatabase.CreateAsset(value,path); return value;
        }
        private static void ConfigureLayer()
        {
            var manager=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layer=manager.FindProperty("layers").GetArrayElementAtIndex(11);
            if(!string.IsNullOrEmpty(layer.stringValue)&&layer.stringValue!="Interactable")throw new InvalidOperationException("Layer 11 in use.");
            layer.stringValue="Interactable"; manager.ApplyModifiedProperties();
        }
    }
}
