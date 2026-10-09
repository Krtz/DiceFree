using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.Pipeline.Commands;
namespace DiceFree.EditorTools {
internal static class NoviceEpicMotionValidation {
[CliCommand("dicefree.art.novice.motion-validate","Check Novice idle, walk, attack actually move its Humanoid bones.")]
public static object Validate(){
const string path="Assets/_DiceFree/Art/Characters/Novice/Models/Novice.fbx";
var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
if(prefab==null)throw new InvalidOperationException("Novice FBX missing");
var clips=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();
var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
try{
 var bones=go.GetComponentsInChildren<Transform>(true);
 float Delta(string label,string bone,float a,float b){
  var clip=clips.FirstOrDefault(x=>x.name.Contains(label));
  if(clip==null)throw new InvalidOperationException("Clip missing: "+label);
  var transform=bones.FirstOrDefault(x=>x.name==bone);
  if(transform==null)throw new InvalidOperationException("Bone missing: "+bone);
  clip.SampleAnimation(go,Mathf.Min(a,clip.length));
  var qa=transform.localRotation;
  clip.SampleAnimation(go,Mathf.Min(b,clip.length));
  return Quaternion.Angle(qa,transform.localRotation);
 }
 var idle=Delta("Idle","Head",0.02f,0.61f);
 var gait=Delta("Locomotion","LeftUpLeg",0.03f,0.48f);
 var swing=Delta("Locomotion","RightArm",0.04f,0.50f);
 var attack=Delta("UnarmedAttack","RightForeArm",0.03f,0.33f);
 if(idle<.2f||gait<2f||swing<2f||attack<2f)
   throw new InvalidOperationException("Animation too static: "+idle+" / "+gait+" / "+swing+" / "+attack);
 return new {success=true,idleHeadDegrees=idle,walkLegDegrees=gait,walkArmDegrees=swing,attackForearmDegrees=attack};
}finally{UnityEngine.Object.DestroyImmediate(go);}
}
}}

