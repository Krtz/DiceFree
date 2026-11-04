using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Original Humanoid muscle variants. The user's existing source clips/controllers are read only.
public static class PlayerMotionAuthoring
{
    const string Folder="Assets/_DiceFree/Resources/PlayerMotion";
    const string Source="Assets/_DiceFree/Art/Characters/Novice/Animations/";
    public static object Rebuild()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required.");
        foreach(var label in new[]{"Novice","Physical","Magical"})foreach(var state in new[]{"Idle","Locomotion","Attack","ArmedAttack","Cast","Heavy","Guard","Heal","Hit","Death"})
        {
            string source=state=="Locomotion"?"NoviceLocomotion":state=="Attack"||state=="ArmedAttack"||state=="Heavy"?"NoviceUnarmedAttack":"NoviceIdle";
            float duration=state switch{"Idle"=>2.4f,"Locomotion"=>.85f,"Attack"=>.85f,"ArmedAttack"=>.9f,"Cast"=>.85f,"Heavy"=>1.05f,"Guard"=>.8f,"Heal"=>.9f,"Hit"=>.3f,_=>1.1f};
            var clip=Make(AssetDatabase.LoadAssetAtPath<AnimationClip>(Source+source+".anim"),state,duration,Array.IndexOf(new[]{"Novice","Physical","Magical"},label));var destination=AssetDatabase.LoadAssetAtPath<AnimationClip>(Folder+"/"+label+"/"+state+".anim");EditorUtility.CopySerialized(clip,destination);UnityEngine.Object.DestroyImmediate(clip);EditorUtility.SetDirty(destination);AssetDatabase.SaveAssetIfDirty(destination);
        }
        return new{clips=30,guidsPreserved=true};
    }
    public static object Author()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||AssetDatabase.IsValidFolder(Folder))throw new InvalidOperationException("Fresh output folder and Edit Mode required.");
        AssetDatabase.CreateFolder("Assets/_DiceFree/Resources","PlayerMotion");
        var rows=new[]{("Idle","NoviceIdle",2.4f),("Locomotion","NoviceLocomotion",.85f),("Attack","NoviceUnarmedAttack",.85f),("ArmedAttack","NoviceUnarmedAttack",.9f),("Cast","NoviceIdle",.85f),("Heavy","NoviceUnarmedAttack",1.05f),("Guard","NoviceIdle",.8f),("Heal","NoviceIdle",.9f),("Hit","NoviceIdle",.3f),("Death","NoviceIdle",1.1f)};
        for(int kind=0;kind<3;kind++)
        {
            string name=new[]{"Novice","Physical","Magical"}[kind];AssetDatabase.CreateFolder(Folder,name);var controller=AnimatorController.CreateAnimatorControllerAtPath(Folder+"/"+name+".controller");controller.AddParameter("Speed",AnimatorControllerParameterType.Float);controller.AddParameter("Attack",AnimatorControllerParameterType.Trigger);
            foreach(var row in rows)
            {
                var source=AssetDatabase.LoadAssetAtPath<AnimationClip>(Source+row.Item2+".anim");if(source==null)throw new InvalidOperationException("Missing source clip.");
                var clip=Make(source,row.Item1,row.Item3,kind);AssetDatabase.CreateAsset(clip,Folder+"/"+name+"/"+row.Item1+".anim");var state=controller.layers[0].stateMachine.AddState(row.Item1);state.motion=clip;if(row.Item1=="Idle")controller.layers[0].stateMachine.defaultState=state;
            }
        }
        AssetDatabase.SaveAssets();return new{classes=3,clips=30,controllers=3,sourceClipsPreserved=true,output=Folder};
    }
    static AnimationClip Make(AnimationClip source,string state,float duration,int kind)
    {
        var clip=new AnimationClip{name=new[]{"Novice ","Physical ","Magical "}[kind]+state,frameRate=30};float scale=duration/source.length;
        foreach(var binding in AnimationUtility.GetCurveBindings(source))
        {
            var curve=AnimationUtility.GetEditorCurve(source,binding);var keys=curve.keys;for(int i=0;i<keys.Length;i++){keys[i].time*=scale;keys[i].inTangent/=scale;keys[i].outTangent/=scale;}curve.keys=keys;AnimationUtility.SetEditorCurve(clip,binding,curve);
        }
        float S(float a,float b,float t)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,t));
        void Curve(string property,Func<float,float> pose)
        {var binding=EditorCurveBinding.FloatCurve("",typeof(Animator),property);int frames=Mathf.CeilToInt(duration*30);var keys=new Keyframe[frames+1];for(int i=0;i<=frames;i++){float t=i/(float)frames;keys[i]=new Keyframe(t*duration,Mathf.Clamp(pose(t),-1,1));}var curve=new AnimationCurve(keys);for(int i=0;i<keys.Length;i++)AnimationUtility.SetKeyLeftTangentMode(curve,i,AnimationUtility.TangentMode.ClampedAuto);for(int i=0;i<keys.Length;i++)AnimationUtility.SetKeyRightTangentMode(curve,i,AnimationUtility.TangentMode.ClampedAuto);AnimationUtility.SetEditorCurve(clip,binding,curve);}
        float Pose(string property,float t)
        {
            float charge=S(0,.38f,t)*(1-S(.56f,.86f,t)),release=S(.38f,.60f,t)*(1-S(.65f,1,t));float breath=Mathf.Sin(t*Mathf.PI*2);
            bool left=property.StartsWith("Left ");float sign=left?-1:1;
            if(property.EndsWith("Arm Down-Up"))
            {
                float idle=kind==0?-.78f:kind==1?-.58f:-.67f;
                return state switch{"Locomotion"=>idle+Mathf.Sin(t*Mathf.PI*2+(left?Mathf.PI:0))*(kind==1? .15f:.11f),"Attack"=>left?-.55f:Mathf.Lerp(-.7f,-.18f,charge)+release*.1f,"ArmedAttack"=>left?-.6f:Mathf.Lerp(-.65f,.12f,charge),"Heavy"=>Mathf.Lerp(idle,.32f,charge),"Cast"=>Mathf.Lerp(idle,-.08f,charge),"Heal"=>Mathf.Lerp(idle,-.2f,charge),"Guard"=>Mathf.Lerp(idle,-.2f,charge),"Death"=>Mathf.Lerp(idle,-.95f,S(0,.7f,t)),_=>idle+breath*.012f};
            }
            if(property.EndsWith("Arm Front-Back"))return state switch{"Attack"=>left? .2f:.12f+charge*.35f-release*.6f,"ArmedAttack"=>left? .12f:charge*.2f-release*.55f,"Heavy"=>charge*.2f-release*.55f,"Cast"=>-.42f*charge,"Heal"=>-.18f*charge,"Guard"=>-.35f*charge,"Locomotion"=>Mathf.Sin(t*Mathf.PI*2+(left?0:Mathf.PI))*.16f,_=>kind==1?-.16f:kind==2?-.12f:0};
            if(property.EndsWith("Forearm Stretch"))return state switch{"Attack"=>left?.3f:.65f-.85f*charge+release*.2f,"ArmedAttack"=>left?.45f:.7f-.9f*charge+release*.15f,"Heavy"=>.65f-.85f*charge+release*.25f,"Cast"=>.7f-.5f*charge,"Heal"=>.6f-.6f*charge,"Guard"=>.5f-.6f*charge,_=>kind==1?.45f:kind==2?.5f:.75f};
            if(property=="Chest Twist Left-Right")return state=="Attack"||state=="ArmedAttack"?-charge*(kind==1? .22f:.15f)+release*.25f:state=="Locomotion"?breath*.035f:0;
            if(property=="Chest Front-Back")return state switch{"Heavy"=>charge*.20f-release*.25f,"Cast"=>charge*.12f,"Hit"=>-Mathf.Sin(t*Mathf.PI)*.2f,"Death"=>-.5f*S(0,.8f,t),_=>kind==1?-.045f:kind==2? .055f:breath*.018f};
            if(property.EndsWith("Upper Leg In-Out"))return state=="Death"?S(0,.7f,t)*.12f:kind==1? .09f:0;
            if(property.EndsWith("Lower Leg Stretch"))return state=="Death"?-.85f*S(0,.65f,t):-.06f;
            return 0;
        }
        foreach(var p in new[]{"Left Arm Down-Up","Right Arm Down-Up","Left Arm Front-Back","Right Arm Front-Back","Left Forearm Stretch","Right Forearm Stretch","Chest Twist Left-Right","Chest Front-Back"})Curve(p,t=>Pose(p,t));
        // Keep locomotion's original retargeted leg and foot curves.
        if(state=="Death")foreach(var p in new[]{"Left Upper Leg In-Out","Right Upper Leg In-Out","Left Lower Leg Stretch","Right Lower Leg Stretch"})Curve(p,t=>Pose(p,t));
        if(state=="Death")
        {
            var b=EditorCurveBinding.FloatCurve("",typeof(Animator),"RootT.y");var old=AnimationUtility.GetEditorCurve(clip,b);float height=old?.Evaluate(0)??1;AnimationUtility.SetEditorCurve(clip,b,AnimationCurve.EaseInOut(0,height,duration,height-.25f));
        }
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=state=="Idle"||state=="Locomotion";settings.loopBlendPositionY=true;AnimationUtility.SetAnimationClipSettings(clip,settings);return clip;
    }
}
