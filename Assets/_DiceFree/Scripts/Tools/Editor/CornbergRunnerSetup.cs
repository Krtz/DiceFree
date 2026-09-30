using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.UI;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiceFree.EditorTools
{
    public static class CornbergRunnerSetup
    {
        public const string BeatId = "story.cornberg.runner-returns";
        public const string NpcId = "npc.cornberg.runner";
        public const string ConversationId = "conversation.cornberg.runner-blocked-road";
        [MenuItem("DiceFree/Quests/Add Runner to saved Cornberg")]
        public static void Install()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player = Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
            var journal = player.GetComponent<QuestJournal>();
            var q3 = journal.Definitions.Single(q => q.stableId == "quest.cornberg.named-slime");
            const string path = "Assets/_DiceFree/Settings/Progression/Cornberg runner returns.asset";
            var beat = AssetDatabase.LoadAssetAtPath<QuestDefinition>(path);
            if (beat == null)
            {
                beat = ScriptableObject.CreateInstance<QuestDefinition>();
                beat.stableId = BeatId; beat.title = "Runner Returns";
                beat.completedQuestIds = new[] { q3.stableId }; beat.acceptOnTalk = true; beat.completeOnObjectives = true;
                beat.locationHint = "Speak to Cornberg Runner by the village green, near the east road.";
                beat.stages = new[] { new QuestObjective { kind = ObjectiveKind.TalkTo, contentId = NpcId,
                    conversationId = ConversationId, instruction = "Hear why the weekly runner returned", count = 1 } };
                AssetDatabase.CreateAsset(beat, path);
            }
            if (!journal.Definitions.Contains(beat)) journal.Configure(journal.Definitions.Concat(new[] { beat }).ToArray());
            if (GameObject.Find("Cornberg Runner - placeholder") == null)
            {
                var root = new GameObject("Cornberg Runner - placeholder"); root.transform.position = new Vector3(18, 0, 6);
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); body.name = "Runner presentation";
                body.transform.SetParent(root.transform); body.transform.localPosition = Vector3.up * .9f;
                body.transform.localScale = new Vector3(.8f, .9f, .8f); body.layer = 11;
                body.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_DiceFree/Materials/Blockout/Timber.mat");
                var target = root.AddComponent<ConversationTarget>(); target.ConfigureName("Cornberg Runner");
                target.Configure(NpcId, ConversationId,
                    "I set out on my usual weekly run to the next town, but I couldn't get through. There were too many Slimes along the road. I had to turn back to Cornberg.");
                root.AddComponent<QuestConversationGate>().Configure(beat, player, body);
                root.AddComponent<InteractionPrompt>().Configure(target, player);
            }
            if (Object.FindAnyObjectByType<ConversationPanel>() == null)
                new GameObject("HUD - conversation").AddComponent<ConversationPanel>().Configure(player.GetComponent<Interactor>());
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("CORNBERG_RUNNER_SETUP_OK: additive NPC and TalkTo story beat; no navigation bake.");
        }
    }
}
