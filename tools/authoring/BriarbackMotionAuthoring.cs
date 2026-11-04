using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Briarback;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Editable Generic-rig variants derived from the project's original creature clips.
public static class BriarbackMotionAuthoring
{
    const string Folder="Assets/_DiceFree/Art/Creatures/Briarback/RegionalMotion";
    const string Model="Assets/_DiceFree/Art/Creatures/Briarback/Models/Quilboar.fbx";
    const string Recovery="Assets/_DiceFree/Settings/Recovery/Cornberg-before-regional-motion-20261104.unity";
    static void Need(bool ok,string text){if(!ok)throw new InvalidOperationException(text);}
    static float Smooth(float a,float b,float t)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,t));
    static void Arm(Transform root,Transform[] bones,string side,Vector3 target,float weight)
    {
        if(weight<=0)return;var upper=bones.Single(b=>b.name==side+"Arm");var lower=bones.Single(b=>b.name==side+"ForeArm");var hand=root.GetComponentsInChildren<Transform>(true).Single(b=>b.name==side+"Hand");
        target=Vector3.Lerp(hand.position,target,weight);var start=upper.position;float a=Vector3.Distance(start,lower.position),b=Vector3.Distance(lower.position,hand.position);var line=target-start;float distance=Mathf.Clamp(line.magnitude,.03f,a+b-.001f);var direction=line.normalized;
        float cosine=Mathf.Clamp((distance*distance+a*a-b*b)/(2*distance*a),-1,1);var pole=root.right*(side=="Right"?1:-1)+root.forward*.35f;var bend=Vector3.ProjectOnPlane(pole,direction).normalized;
        var elbow=start+direction*(cosine*a)+bend*(Mathf.Sqrt(1-cosine*cosine)*a);
        upper.rotation=Quaternion.FromToRotation(lower.position-start,elbow-start)*upper.rotation;
        lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,target-lower.position)*lower.rotation;
    }
    public static object RebuildClips()
    {
        Need(!EditorApplication.isPlayingOrWillChangePlaymode,"Edit Mode required.");var sources=AssetDatabase.LoadAllAssetsAtPath(Model).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();AnimationClip Source(string n)=>sources.Single(c=>c.name.EndsWith("|"+n,StringComparison.Ordinal));
        foreach(var row in new[]{("Idle","Idle",3f),("Locomotion","Locomotion",1f),("Attack","Attack",1f),("Cast","Attack",1.2f),("Throw","Attack",1.1f),("Slam","Slam",1.8f),("Death","Death",Source("Death").length)})
        {var destination=AssetDatabase.LoadAssetAtPath<AnimationClip>(Folder+"/"+row.Item1+".anim");Need(destination!=null,"Missing owned clip.");var clip=Make(Source(row.Item2),row.Item1,row.Item3);EditorUtility.CopySerialized(clip,destination);Object.DestroyImmediate(clip);EditorUtility.SetDirty(destination);AssetDatabase.SaveAssetIfDirty(destination);}
        return new{clipsUpdated=7};
    }
    public static object Author()
    {
        var scene=SceneManager.GetActiveScene();Need(scene.name=="Cornberg"&&!scene.isDirty&&!EditorApplication.isPlayingOrWillChangePlaymode,"Clean Cornberg required.");
        Need(!AssetDatabase.IsValidFolder(Folder)&&!System.IO.File.Exists(Recovery),"Motion output or recovery already exists.");
        Need(EditorSceneManager.SaveScene(scene,Recovery,true),"Motion recovery failed.");AssetDatabase.CreateFolder("Assets/_DiceFree/Art/Creatures/Briarback","RegionalMotion");
        var sources=AssetDatabase.LoadAllAssetsAtPath(Model).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();
        AnimationClip Source(string name)=>sources.Single(c=>c.name.EndsWith("|"+name,StringComparison.Ordinal));
        var controller=AnimatorController.CreateAnimatorControllerAtPath(Folder+"/Quilboar regional.controller");
        foreach(var row in new[]{("Idle","Idle",3f),("Locomotion","Locomotion",1f),("Attack","Attack",1f),("Cast","Attack",1.2f),("Throw","Attack",1.1f),("Slam","Slam",1.8f),("Death","Death",Source("Death").length)})
        {
            var clip=Make(Source(row.Item2),row.Item1,row.Item3);AssetDatabase.CreateAsset(clip,Folder+"/"+row.Item1+".anim");
            var state=controller.layers[0].stateMachine.AddState(row.Item1);state.motion=clip;if(row.Item1=="Idle")controller.layers[0].stateMachine.defaultState=state;
        }
        int actors=0;foreach(var p in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<QuilboarPresentation>(true)))
        {Need(p.model!=null,"Creature animator missing.");p.model.runtimeAnimatorController=controller;EditorUtility.SetDirty(p.model);actors++;}
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);Need(EditorSceneManager.SaveScene(scene),"Motion scene save failed.");return new{actors,clips=7,controller=Folder+"/Quilboar regional.controller"};
    }
    static AnimationClip Make(AnimationClip source,string state,float duration)
    {
        var clip=new AnimationClip{name="Briarback "+state,frameRate=24};
        foreach(var binding in AnimationUtility.GetCurveBindings(source))
        {
            var curve=AnimationUtility.GetEditorCurve(source,binding);var keys=curve.keys;float scale=duration/source.length;
            for(int i=0;i<keys.Length;i++){keys[i].time*=scale;keys[i].inTangent/=scale;keys[i].outTangent/=scale;}
            curve.keys=keys;AnimationUtility.SetEditorCurve(clip,binding,curve);
        }
        if(state=="Idle"||state=="Death"){var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=state=="Idle";AnimationUtility.SetAnimationClipSettings(clip,settings);return clip;}
        var copy=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Model));copy.name="Temporary original rig sampling";copy.hideFlags=HideFlags.HideAndDontSave;copy.SetActive(false);
        try
        {
            var bones=copy.GetComponentsInChildren<Transform>(true).Where(t=>new[]{"Chest","Spine","Head","RightArm","LeftArm","RightForeArm","LeftForeArm"}.Contains(t.name)).ToArray();
            var curves=bones.ToDictionary(b=>b,b=>Enumerable.Range(0,4).Select(_=>new List<Keyframe>()).ToArray());int frames=Mathf.CeilToInt(duration*24);
            for(int i=0;i<=frames;i++)
            {
                float t=i/(float)frames;source.SampleAnimation(copy,t*source.length);float charge=Smooth(0,.48f,t)*(1-Smooth(.66f,.83f,t));float recoil=Smooth(.66f,.75f,t)*(1-Smooth(.75f,1,t));
                foreach(var bone in bones)
                {
                    Vector3 delta=Vector3.zero;
                    if(state=="Locomotion")delta=bone.name=="Chest"?new Vector3(Mathf.Sin(t*Mathf.PI*4)*2,Mathf.Sin(t*Mathf.PI*2)*5,0):Vector3.zero;
                    if(state=="Cast")delta=bone.name switch{"Chest"=>new(-7*charge,0,0),"RightArm"=>new(-38*charge,0,-24*charge),"LeftArm"=>new(-30*charge,0,24*charge),"RightForeArm"=>new(-18*charge,0,0),"Head"=>new(7*charge,0,0),_=>Vector3.zero};
                    if(state=="Throw")delta=bone.name switch{"Chest"=>new(-8*charge+12*recoil,-18*charge+12*recoil,0),"RightArm"=>new(-65*charge+22*recoil,15*charge,0),"LeftArm"=>new(0,0,15*charge),_=>Vector3.zero};
                    if(state=="Slam")delta=bone.name switch{"Chest"=>new(-16*charge+25*recoil,0,0),"RightArm"=>new(-80*charge+20*recoil,0,-12*charge),"LeftArm"=>new(-55*charge+15*recoil,0,12*charge),"Head"=>new(12*charge-10*recoil,0,0),_=>Vector3.zero};
                    if(state=="Attack")delta=bone.name switch{"Chest"=>new(-3*charge,-13*charge+17*recoil,0),"RightArm"=>new(-15*charge,0,-12*charge),_=>Vector3.zero};
                    bone.localRotation*=Quaternion.Euler(delta);
                }
                var head=bones.Single(b=>b.name=="Head").position;var chest=bones.Single(b=>b.name=="Chest").position;var r=copy.transform.right;var f=copy.transform.forward;
                if(state=="Slam"){Arm(copy.transform,bones,"Right",head+Vector3.up*.20f+r*.32f,charge);Arm(copy.transform,bones,"Left",head+Vector3.up*.16f-r*.30f,charge);}
                if(state=="Cast"){Arm(copy.transform,bones,"Right",chest+f*.60f+r*.30f+Vector3.up*.10f,charge);Arm(copy.transform,bones,"Left",chest+f*.48f-r*.30f+Vector3.up*.08f,charge);}
                if(state=="Throw")Arm(copy.transform,bones,"Right",head+Vector3.up*.12f-f*.25f+r*.42f,charge);
                foreach(var bone in bones){var q=bone.localRotation;float[] values={q.x,q.y,q.z,q.w};for(int axis=0;axis<4;axis++)curves[bone][axis].Add(new Keyframe(t*duration,values[axis]));}
            }
            foreach(var bone in bones)for(int axis=0;axis<4;axis++)
            {string path=AnimationUtility.CalculateTransformPath(bone,copy.transform);AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve(path,typeof(Transform),"m_LocalRotation."+"xyzw"[axis]),new AnimationCurve(curves[bone][axis].ToArray()));}
            clip.EnsureQuaternionContinuity();var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=state=="Locomotion";AnimationUtility.SetAnimationClipSettings(clip,settings);return clip;
        }
        finally{Object.DestroyImmediate(copy);}
    }
}
