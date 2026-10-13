using System;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.Quests;
using DiceFree.World;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
    public static class CornbergSlimeRepeatAuthoring
    {
        public const string QuestId="quest.cornberg.repeat-slime-hunt";
        private const string AssetPath=
            "Assets/_DiceFree/Settings/Enemies/Cornberg repeat slime hunt.asset";
        private const string ScenePath="Assets/_DiceFree/Scenes/Cornberg.unity";
        private static T[] All<T>(Scene scene) where T:Component =>
            scene.GetRootGameObjects()
                .SelectMany(root=>root.GetComponentsInChildren<T>(true)).ToArray();
        [CliCommand("dicefree.cornberg.repeat-slime.install",
            "Install 30-Slime repeatable hunt on Mira, unlocked by Break the Slime Surge.")]
        public static object Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before scene authoring.");
            for(int i=0;i<SceneManager.sceneCount;i++)
                if(SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save modified scenes first.");
            var backup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var journal=All<QuestJournal>(scene).Single();
                var mira=All<QuestGiver>(scene).Single(q=>q.Quest!=null &&
                    q.Quest.stableId==CornbergSurgeSetup.QuestId);
                var quest=AssetDatabase.LoadAssetAtPath<QuestDefinition>(AssetPath);
                if(quest==null)
                {
                    quest=ScriptableObject.CreateInstance<QuestDefinition>();
                    AssetDatabase.CreateAsset(quest,AssetPath);
                }
                quest.stableId=QuestId;
                quest.title="Another Thirty Slimes";
                quest.repeatable=true;
                quest.completedQuestIds=new[]{CornbergSurgeSetup.QuestId};
                quest.offer="Those Slimes keep coming back. Bring down thirty Slimes outside or around Cornberg, then report back. There's always more work for a willing adventurer.";
                quest.activeDialogue="Thirty Slimes. I'll have your next reward ready.";
                quest.locationHint="Return to Mira in Cornberg after killing 30 Slimes.";
                quest.rewardXp=120;
                quest.stages=new[]{new QuestObjective
                {
                    kind=ObjectiveKind.Kill, familyId="enemy-family.slime",
                    count=30,creditMode=QuestCreditMode.NearbyParty,
                    creditRadius=50,
                    instruction="Defeat thirty Slimes"
                }};
                EditorUtility.SetDirty(quest);
                if(!journal.Definitions.Contains(quest))
                    journal.Configure(journal.Definitions.Concat(new[]{quest}).ToArray());
                if(!mira.Offers.Contains(quest))
                    mira.ConfigureAdditional(mira.Offers.Skip(1).Concat(new[]{quest}).ToArray());
                if(mira.GetComponent<QuestWorldMarker>()==null)
                    mira.gameObject.AddComponent<QuestWorldMarker>();
                EditorUtility.SetDirty(journal);
                EditorUtility.SetDirty(mira);
                EditorSceneManager.MarkSceneDirty(scene);
                if(!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Couldn't save Cornberg.");
                AssetDatabase.SaveAssets();
                return new{success=true,questId=QuestId,
                    repeatable=quest.repeatable,kills=30,
                    rewardXp=quest.rewardXp,questNpc=mira.name,
                    prerequisites=quest.completedQuestIds};
            }
            finally{if(backup.Length>0)EditorSceneManager.RestoreSceneManagerSetup(backup);}
        }
        [CliCommand("dicefree.cornberg.oct13.scene-inspect",
            "Inspect village merchant positions and Ironpine road colliders.")]
        public static object Inspect()
        {
            var backup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var all=All<Transform>(scene);
                var interactions=All<InteractionTarget>(scene)
                    .Where(q=>q!=null)
                    .Select(q=>new{name=q.name,position=q.transform.position.ToString(),
                        type=q.GetType().Name}).ToArray();
                var surrounding=all.Where(t=>
                    (t.name.IndexOf("goods",StringComparison.OrdinalIgnoreCase)>=0 ||
                     t.name.IndexOf("brew",StringComparison.OrdinalIgnoreCase)>=0 ||
                     t.name.IndexOf("merchant",StringComparison.OrdinalIgnoreCase)>=0 ||
                     t.name.IndexOf("NPC",StringComparison.OrdinalIgnoreCase)>=0 ||
                     t.name.IndexOf("alchemist",StringComparison.OrdinalIgnoreCase)>=0) &&
                    t.parent!=null).Take(45).Select(t=>new{
                        name=t.name,position=t.position.ToString(),
                        parent=t.parent.name}).ToArray();
                var dangerous=All<Collider>(scene)
                    .Where(c=>c.enabled && c.gameObject.activeInHierarchy &&
                        c.bounds.size.x>6f && c.bounds.size.z>6f &&
                        c.bounds.center.x>105f && c.bounds.center.x<240f &&
                        c.bounds.center.z>35f && c.bounds.center.z<170f)
                    .Take(90).Select(c=>new{
                        name=c.name,parent=c.transform.parent!=null?
                            c.transform.parent.name:"ROOT",
                        size=c.bounds.size.ToString(),pos=c.bounds.center.ToString(),
                        type=c.GetType().Name}).ToArray();
                return new{interactions,surrounding,
                    largeForestColliders=dangerous};
            }
            finally{if(backup.Length>0)EditorSceneManager.RestoreSceneManagerSetup(backup);}
        }
    }
}
