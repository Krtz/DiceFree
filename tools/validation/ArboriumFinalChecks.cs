using System;
using System.IO;
using System.Linq;
using DiceFree.EditorTools;
using DiceFree.Quests;
using DiceFree.Skills;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
public static class ArboriumFinalChecks
{
 public static object Run()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required.");
  ArchitectureValidation.Run();ArchitectureValidation.RunPolicySelfTests();PhysicalBlessedValidation.Validate();
  var heavy=AssetDatabase.LoadAssetAtPath<PhysicalSkillDefinition>("Assets/_DiceFree/Settings/Skills/Physical/Heavy Strike.asset");
  var bane=AssetDatabase.LoadAssetAtPath<QuestDefinition>("Assets/_DiceFree/Resources/HonorificQuests/ChieftainsBane.asset");
  var journey=AssetDatabase.LoadAssetAtPath<QuestDefinition>("Assets/_DiceFree/Settings/Arborium/Road to Arborium.asset");
  var wells=Object.FindObjectsByType<HealingArea>();
  if(wells.Length!=1||new SerializedObject(wells[0]).FindProperty("hpPerSecond").floatValue!=25||heavy.FlatDamage(1)!=10||heavy.FlatDamage(6)!=60||bane.rewardXp!=10000||journey.completedQuestIds.Length!=0||journey.stages.Last().kind!=ObjectiveKind.TalkTo)throw new InvalidOperationException("Balance/city data mismatch");
  var result=new{architecture=true,dependencyPolicy=true,managedReferences=true,physicalKit=true,wellHpPerSecond=25,heavyFlatPerRank=10,chieftainsBaneXp=10000,cityHasNoBossPrerequisite=true,cityFinishesByGuildConversation=true};File.WriteAllText("Logs/ArboriumFinalChecks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
}
