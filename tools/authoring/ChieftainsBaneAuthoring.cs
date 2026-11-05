using System;
using System.IO;
using System.Linq;
using DiceFree.Briarback;
using DiceFree.Combat;
using DiceFree.Items;
using DiceFree.Quests;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class ChieftainsBaneAuthoring
{
    public const string QuestId="quest.briarback.chieftains-bane";
    public const string QuestPath="Assets/_DiceFree/Resources/HonorificQuests/ChieftainsBane.asset";
    const string RewardPath="Assets/_DiceFree/Settings/Briarback/Chieftains Bane reward.asset";
    public static object Author()
    {
        var scene=SceneManager.GetActiveScene();Need(scene.name=="Cornberg"&&!scene.isDirty&&!EditorApplication.isPlayingOrWillChangePlaymode,"Clean Cornberg Edit Mode required.");
        const string recovery="Assets/_DiceFree/Settings/Recovery/Cornberg-before-elder-quest-20261104.unity";
        Need(!File.Exists(recovery)&&AssetDatabase.LoadAssetAtPath<QuestDefinition>(QuestPath)==null,"Elder quest/recovery already exists.");
        var navigation=Object.FindAnyObjectByType<WorldNavigation>();var nav=NavMesh.AddNavMeshData(navigation.Data);
        Vector3 point=default,approachPoint=default;bool found=false;
        try
        {
            var interactives=Object.FindObjectsByType<InteractionTarget>().Select(t=>t.transform.position).ToArray();
            Need(NavMesh.SamplePosition(new Vector3(248,0,180),out var origin,8,NavMesh.AllAreas),"Trimery origin missing.");
            var candidates=new[]{new Vector3(268,0,170),new Vector3(224,0,180),new Vector3(267,0,189),new Vector3(228,0,163),new Vector3(265,0,157),new Vector3(236,0,188)}.Concat(Enumerable.Range(0,11).SelectMany(x=>Enumerable.Range(0,11).Select(z=>new Vector3(222+x*5,0,155+z*5))));
            foreach(var candidate in candidates)
            {
                if(!NavMesh.SamplePosition(candidate,out var hit,3,NavMesh.AllAreas)||interactives.Any(p=>Vector3.Distance(p,hit.position)<5))continue;
                if(!NavMesh.SamplePosition(hit.position+Vector3.forward*1.7f,out var approach,1,NavMesh.AllAreas)||!Clear(hit.position)||!Clear(approach.position))continue;
                var path=new NavMeshPath();if(!NavMesh.CalculatePath(origin.position,approach.position,NavMesh.AllAreas,path)||path.status!=NavMeshPathStatus.PathComplete)continue;
                point=hit.position;approachPoint=approach.position;found=true;break;
            }
            Need(found,"No connected collision-clear elder location.");
            Need(EditorSceneManager.SaveScene(scene,recovery,true),"Recovery failed.");
            if(!AssetDatabase.IsValidFolder("Assets/_DiceFree/Resources/HonorificQuests"))AssetDatabase.CreateFolder("Assets/_DiceFree/Resources","HonorificQuests");
            var reward=ScriptableObject.CreateInstance<FixedRewardDefinition>();reward.name="Chieftains Bane reward";reward.gold=DiceFree.EditorTools.BriarbackBountyAuthoring.PlaceholderGold;AssetDatabase.CreateAsset(reward,RewardPath);
            var boss=Object.FindAnyObjectByType<BriarbackChieftainEncounter>();var quest=ScriptableObject.CreateInstance<QuestDefinition>();quest.name="Chieftain's Bane";quest.stableId=QuestId;quest.title="Chieftain's Bane";
            quest.offer="The tribes are gathering behind one chieftain. If he unites them, Trimery will face an invasion. Find the level-40 Chieftain in the northern stronghold and defeat him. Return to me here in Trimery for your reward and the title Chieftain's Bane. This hunt is optional; the city road remains yours to travel.";
            quest.activeDialogue="The Chieftain commands the upper court of the northern stronghold. Watch his overhead smash and the two guards at his side. Return to me in Trimery when he falls.";
            quest.locationHint="Northern Briarback stronghold, upper court. Return to Trimery's village elder.";quest.rewardXp=10000;quest.reward=reward;quest.rewardTitle="Chieftain's Bane";
            quest.stages=new[]{new QuestObjective{kind=ObjectiveKind.Kill,contentId=boss.GetComponent<ActorStats>().Definition.stableId,count=1,creditMode=QuestCreditMode.NearbyParty,creditRadius=50,instruction="Defeat the level-40 Chieftain in the northern stronghold, then return to Trimery's village elder."}};
            AssetDatabase.CreateAsset(quest,QuestPath);
            var template=Object.FindObjectsByType<QuestGiver>().Single(g=>g.name=="Trimery Elemental Scholar - Briarback");var giver=Object.Instantiate(template.gameObject,point,Quaternion.identity,GameObject.Find("13 - Trimery lumber village").transform);giver.name="Trimery Village Elder - Chieftain's Bane";
            if(PrefabUtility.IsPartOfPrefabInstance(giver))PrefabUtility.UnpackPrefabInstance(giver,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            foreach(var old in giver.GetComponents<QuestGiver>())Object.DestroyImmediate(old);
            var npc=giver.AddComponent<QuestGiver>();npc.Configure(quest);npc.ConfigureName("Trimery Village Elder");var approachGo=new GameObject("Elder conversation approach");approachGo.transform.SetParent(giver.transform);approachGo.transform.position=approachPoint;npc.ConfigureApproach(approachGo.transform);
            foreach(var journal in Object.FindObjectsByType<QuestJournal>(FindObjectsInactive.Include)){Need(!journal.Definitions.Any(q=>q.stableId==QuestId),"Quest already registered.");journal.Configure(journal.Definitions.Concat(new[]{quest}).ToArray());EditorUtility.SetDirty(journal);}
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Scene save failed.");
            return new{questId=quest.stableId,killContentId=quest.stages[0].contentId,elderPosition=point.ToString(),title=quest.rewardTitle,rewardXp=quest.rewardXp,gold=reward.gold,rewardsAreExistingTuningPlaceholders=true,optional=true};
        }
        finally{if(nav.valid)nav.Remove();}
    }
    public static object RepairElder()
    {
        var scene=SceneManager.GetActiveScene();Need(scene.name=="Cornberg"&&!scene.isDirty&&!EditorApplication.isPlayingOrWillChangePlaymode,"Clean Cornberg required.");
        var elder=GameObject.Find("Trimery Village Elder - Chieftain's Bane");Need(elder!=null,"Elder missing.");
        const string recovery="Assets/_DiceFree/Settings/Recovery/Cornberg-before-elder-prefab-repair-20261104.unity";Need(!File.Exists(recovery)&&EditorSceneManager.SaveScene(scene,recovery,true),"Repair recovery exists/failed.");
        if(PrefabUtility.IsPartOfPrefabInstance(elder))PrefabUtility.UnpackPrefabInstance(elder,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        var keep=elder.GetComponents<QuestGiver>().Single(g=>g.Quest!=null&&g.Quest.stableId==QuestId);foreach(var old in elder.GetComponents<QuestGiver>().Where(g=>g!=keep))Undo.DestroyObjectImmediate(old);
        Need(elder.GetComponentsInChildren<InteractionTarget>(true).Length==1,"Elder still has competing interactions.");EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Repair save failed.");
        EditorSceneManager.OpenScene(scene.path);elder=GameObject.Find("Trimery Village Elder - Chieftain's Bane");Need(elder.GetComponents<QuestGiver>().Length==1&&elder.GetComponent<QuestGiver>().Quest.stableId==QuestId,"Duplicate returned after scene reload.");return new{passed=true,elderInteractions=1,sceneReloadVerified=true};
    }
    static bool Clear(Vector3 p)=>!Physics.OverlapCapsule(p+Vector3.up*.5f,p+Vector3.up*1.5f,.42f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore).Any(c=>c.bounds.max.y>p.y+.35f);
    static void Need(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
}
