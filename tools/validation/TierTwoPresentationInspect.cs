using System.IO;
using System.Linq;
using DiceFree.Characters;
using DiceFree.Combat;
using DiceFree.UI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
public static class TierTwoPresentationInspect
{
    public static string State()
    {
        var p=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();var m=p.GetComponent<PlayerMotionPresentation>();var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        return Newtonsoft.Json.JsonConvert.SerializeObject(new{motion=m.State,root=p.GetComponent<NovicePresentationDriver>().VisualRoot.localEulerAngles.ToString(),actor=p.transform.eulerAngles.ToString(),spinning=typeof(PlayerMotionPresentation).GetField("spinning",flags).GetValue(m),spinRest=typeof(PlayerMotionPresentation).GetField("spinRest",flags).GetValue(m).ToString(),busy=p.GetComponent<GreatbladeSkillCaster>().Busy,stunned=p.Effects.Stunned});
    }
    public static string Run()
    {
        var p=Object.FindAnyObjectByType<TraversalInput>().GetComponent<CombatActor>();var driver=p.GetComponent<NovicePresentationDriver>();var animator=driver.VisualAnimator;
        var controller=animator.runtimeAnimatorController;string folder="VisualBaselines/TierTwo/Before";Directory.CreateDirectory(folder);ScreenCapture.CaptureScreenshot(folder+"/"+p.Stats.Definition.stableId+".png");
        return Newtonsoft.Json.JsonConvert.SerializeObject(new{classId=p.Stats.Definition.stableId,controller=controller.name,path=AssetDatabase.GetAssetPath(controller),clips=controller.animationClips.Select(c=>new{c.name,c.length,path=AssetDatabase.GetAssetPath(c)}).ToArray(),model=animator.name,isHuman=animator.isHuman});
    }
}
