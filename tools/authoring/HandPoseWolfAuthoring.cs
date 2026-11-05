using System;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Quests;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class HandPoseWolfAuthoring
{
 public static object Apply()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean Edit Mode required.");
  var wolves=AssetDatabase.LoadAssetAtPath<QuestDefinition>("Assets/_DiceFree/Settings/Enemies/Cornberg repeat wolf hunt.asset");
  if(wolves==null||!wolves.repeatable||wolves.stages.Length!=1||wolves.stages[0].count!=15)throw new InvalidOperationException("Existing repeatable wolf hunt required.");
  var grey=AssetDatabase.LoadAssetAtPath<ActorDefinition>("Assets/_DiceFree/Settings/Ironpine/GreyWolf-Lv15-stats.asset");
  wolves.rewardXp=20*grey.experienceReward;wolves.completedQuestIds=Array.Empty<string>();
  wolves.offer="Wolves on the old road keep threatening travelers. Defeat fifteen Ironpine wolves, then return to Mira. The bounty pays the experience of twenty regular Grey Wolves.";
  EditorUtility.SetDirty(wolves);AssetDatabase.SaveAssetIfDirty(wolves);
  return new{quest=wolves.stableId,kills=15,rewardXp=wolves.rewardXp,referenceWolfLevel=15,referenceKillXp=grey.experienceReward,availableWithoutSlimeQuest=true};
 }
}
