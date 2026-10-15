using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Unity.Pipeline.Commands;
namespace DiceFree.EditorTools
{
 public static class LocomotionRepair
 {
  internal static void ConfigureImportedLoops(ModelImporter importer){
   var clips=importer.clipAnimations;
   if(clips.Length==0)clips=importer.defaultClipAnimations;
   foreach(var clip in clips)
    if(clip.name.Contains("Locomotion")||clip.name.Contains("Idle")){
     clip.loopTime=true;clip.loopPose=true;
    }
   importer.clipAnimations=clips;
  }
  [CliCommand("dicefree.locomotion.repair","Repair looping on the shared original Novice FBX and portable clips used by Novice and both first-tier forms.")]
  public static object Run(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode.");
   const string model="Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx";
   var importer=AssetImporter.GetAtPath(model) as ModelImporter;
   if(importer==null)throw new InvalidOperationException("Original Novice importer missing.");
   ConfigureImportedLoops(importer);importer.SaveAndReimport();
   int count=0;
   foreach(string name in new[]{"NoviceIdle","NoviceLocomotion"}){
    var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/_DiceFree/Art/Characters/Novice/Animations/"+name+".anim");
    if(clip==null)continue;
    var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;settings.loopBlend=true;
    AnimationUtility.SetAnimationClipSettings(clip,settings);
    EditorUtility.SetDirty(clip);AssetDatabase.SaveAssetIfDirty(clip);count++;
   }
   return new{marker="DICEFREE_SHARED_LOCOMOTION_LOOP_REPAIRED",portableClips=count};
  }
 }
}
