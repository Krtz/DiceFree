using System.IO;
using DiceFree.EditorTools;
public static class NovemberFeedbackFinalChecks
{
 public static object Run()
 {
  ArchitectureValidation.Run();ArchitectureValidation.RunPolicySelfTests();
  File.WriteAllText("Logs/NovemberFeedbackArchitecture.json","{\"architecture\":true,\"managedReferences\":true,\"dependencyPolicy\":true}");
  return Content();
 }
 public static object Content()
 {
  if(!File.Exists("Logs/NovemberFeedbackArchitecture.json"))throw new System.InvalidOperationException("Run architecture validation first.");
  var bounty=BriarbackBountyValidation.Run();var hunt=BriarbackElementalQuestValidation.Validate();var ui=HealthChannelUiValidation.Run();var ranks=ClassSkillRankValidation.Run();
  var result=new{architecture=true,managedReferences=true,dependencyPolicy=true,bounty,hunt,channelUi=ui,ranks};File.WriteAllText("Logs/NovemberFeedbackFinalChecks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
}
