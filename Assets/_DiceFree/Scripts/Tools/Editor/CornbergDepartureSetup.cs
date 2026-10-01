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
    public static class CornbergDepartureSetup
    {
        public const string QuestId = "quest.cornberg.become-runner";
        public const string DestinationId = "area.world1.next-settlement-arrival";
        public static void Install()
        {
            var scene = EditorSceneManager.OpenScene(CornbergSceneBuilder.ScenePath);
            var player = Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();
            var journal = player.GetComponent<QuestJournal>();
            const string path = "Assets/_DiceFree/Settings/Progression/Cornberg become runner.asset";
            var quest = AssetDatabase.LoadAssetAtPath<QuestDefinition>(path);
            if (quest == null)
            {
                quest = ScriptableObject.CreateInstance<QuestDefinition>();
                quest.stableId = QuestId; quest.title = "Become Cornberg's Runner";
                quest.completedQuestIds = new[] { CornbergSurgeSetup.QuestId };
                quest.locationHint = "Speak to Cornberg Runner by the village green.";
                quest.offer = "You've helped with the Slimes, but I keep thinking about that road. I'm not ready to try again yet. Would you take Cornberg's message to the next settlement for me? It's our usual village news. When there's important news to bring back, we count on our runner for that too.";
                quest.activeDialogue = "Thank you for taking the message. Take care on the road. I hope the trip goes well.";
                quest.stages = new[] { new QuestObjective { kind = ObjectiveKind.ReachArea, objectiveId = "runner.deliver-message",
                    areaId = DestinationId, count = 1, instruction = "Travel to the next settlement and deliver Cornberg's message." } };
                AssetDatabase.CreateAsset(quest, path);
            }
            if (!journal.Definitions.Contains(quest)) journal.Configure(journal.Definitions.Concat(new[] { quest }).ToArray());
            var root = GameObject.Find("Cornberg Runner - placeholder");
            if (root.GetComponent<ContextualNpc>() == null)
            {
                var previous = root.GetComponent<ConversationTarget>();
                var oldGate = root.GetComponent<QuestConversationGate>();
                var storyObject = new GameObject("Runner Returns action"); storyObject.transform.SetParent(root.transform, false);
                var story = storyObject.AddComponent<ConversationTarget>(); EditorUtility.CopySerialized(previous, story);
                var gate = storyObject.AddComponent<QuestConversationGate>(); EditorUtility.CopySerialized(oldGate, gate);
                Object.DestroyImmediate(oldGate); Object.DestroyImmediate(previous);
                var questObject = new GameObject("Runner departure action"); questObject.transform.SetParent(root.transform, false);
                var giver = questObject.AddComponent<QuestGiver>(); giver.Configure(quest); giver.ConfigureName("Cornberg Runner"); giver.ConfigureAmbient(true);
                var context = root.AddComponent<ContextualNpc>(); context.ConfigureName("Cornberg Runner");
                context.Configure(CornbergRunnerSetup.NpcId, giver, story);
                root.GetComponent<InteractionPrompt>().Configure(context, player);
            }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("CORNBERG_DEPARTURE_SETUP_OK: existing Runner, one world target, future destination only; no navigation bake.");
        }
    }
}
