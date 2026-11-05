using System;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.EditorTools;
using DiceFree.Items;
using DiceFree.Quests;
using UnityEditor;
public static class HandPoseWolfFinalChecks
{
 public static object Run()
 {
  ArchitectureValidation.Run();ArchitectureValidation.RunPolicySelfTests();
  var quest=AssetDatabase.LoadAssetAtPath<QuestDefinition>("Assets/_DiceFree/Settings/Enemies/Cornberg repeat wolf hunt.asset");
  var wolf=AssetDatabase.LoadAssetAtPath<ActorDefinition>("Assets/_DiceFree/Settings/Ironpine/GreyWolf-Lv15-stats.asset");
  if(!quest.repeatable||quest.stages[0].count!=15||quest.rewardXp!=20*wolf.experienceReward||quest.completedQuestIds.Length!=0||!quest.stages[0].familyIds.OrderBy(x=>x).SequenceEqual(new[]{"ironpine.alpha-wolf","ironpine.wolf"}))throw new InvalidOperationException("Wolf hunt data mismatch.");
  var sword=AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/_DiceFree/Settings/Items/Wolf-Fang Greatsword.asset");if(sword.handsRequired!=2)throw new InvalidOperationException("Sword hand-use metadata changed.");
  var result=new{architecture=true,dependencyPolicy=true,managedReferences=true,wolfHunt=new{kills=15,xp=quest.rewardXp,referenceLevel=15,referenceKillXp=wolf.experienceReward,mixedFamilies=true,repeatable=true,noSlimePrerequisite=true},twoHandMetadataUnchanged=true};File.WriteAllText("Logs/HandPoseWolfFinalChecks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
}
