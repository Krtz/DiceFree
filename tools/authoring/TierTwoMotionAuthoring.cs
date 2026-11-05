using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Native controller using already tracked original clips. No vendor animation is copied.
public static class TierTwoMotionAuthoring
{
    const string Folder="Assets/_DiceFree/Resources/PlayerMotion/TierTwo";
    public static string Author()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||AssetDatabase.IsValidFolder(Folder))throw new InvalidOperationException("Edit Mode and a fresh TierTwo folder required.");
        AssetDatabase.CreateFolder("Assets/_DiceFree/Resources/PlayerMotion","TierTwo");
        var controller=AnimatorController.CreateAnimatorControllerAtPath(Folder+"/Greatblade.controller");
        controller.AddParameter("Speed",AnimatorControllerParameterType.Float);controller.AddParameter("Attack",AnimatorControllerParameterType.Trigger);controller.AddParameter("SpinDash",AnimatorControllerParameterType.Trigger);
        foreach(string name in new[]{"Idle","Locomotion","Attack","ArmedAttack","Cast","Heavy","Guard","Heal","Hit","Death","Spin Dash"})
        {
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/_DiceFree/Resources/PlayerMotion/Physical/"+(name=="Spin Dash"?"ArmedAttack":name)+".anim");
            if(clip==null)throw new InvalidOperationException("Missing original motion clip "+name);
            if(name=="Spin Dash")
            {
                clip=UnityEngine.Object.Instantiate(clip);clip.name="Greatblade fallback Spin Dash";var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,settings);AssetDatabase.CreateAsset(clip,Folder+"/Spin Dash.anim");
            }
            var state=controller.layers[0].stateMachine.AddState(name);state.motion=clip;
            if(name=="Idle")controller.layers[0].stateMachine.defaultState=state;
        }
        AssetDatabase.SaveAssets();return "Created one Tier II controller and one original fallback clip; existing source assets untouched.";
    }
}
