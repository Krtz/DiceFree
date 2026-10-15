using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Development;
using DiceFree.UI;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace DiceFree.EditorTools
{
 [InitializeOnLoad] public static class LocomotionDiagnosis
 {
  const string Key="DiceFree.LocomotionDiagnosis.";
  static readonly Stack<IEnumerator> steps=new();
  static readonly List<Row> rows=new();
  static double deadline;
  [Serializable] public sealed class Row {
   public string classId,controller,clip,state,avatar;
   public bool humanoid,loop;
   public float distance,maxSpeed,footTravel,legAngle,normalizedTime,idleSpeed;
   public string[] states;
  }
  static LocomotionDiagnosis()=>EditorApplication.playModeStateChanged+=Changed;
  [CliCommand("dicefree.locomotion.diagnose","Measure real walk pose, clip loop and movement for all four playable forms using an isolated Echo.")]
  public static object Start(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode.");
   SessionState.SetString(Key+"status","running");SessionState.SetBool(Key+"cleanup",true);
   SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);
   PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(Path.Combine(Path.GetTempPath(),"DiceFree-Locomotion-"+Guid.NewGuid().ToString("N")));
   EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/DevelopmentRoom.unity");EditorApplication.EnterPlaymode();return Status();
  }
  [CliCommand("dicefree.locomotion.diagnosis-status","Read locomotion pose diagnosis.")]
  public static object Status()=>new{status=SessionState.GetString(Key+"status","idle")};
  static void Changed(PlayModeStateChange state){
   if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running"){
    rows.Clear();steps.Clear();steps.Push(Flow());deadline=EditorApplication.timeSinceStartup+90;EditorApplication.update+=Tick;
   }
   if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false)){
    SessionState.SetBool(Key+"cleanup",false);
    EditorApplication.update-=Tick;PersistenceTestGuard.ClearValidationOverrides();
    var path=SessionState.GetString(Key+"scene","");if(!string.IsNullOrEmpty(path))EditorSceneManager.OpenScene(path);
   }
  }
  static void Tick(){if(!EditorApplication.isPlaying)return;try{
   if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Diagnosis timeout.");
   while(steps.Count>0){var s=steps.Peek();if(!s.MoveNext()){steps.Pop();continue;}if(s.Current is IEnumerator e){steps.Push(e);continue;}return;}
   Finish("completed");
  }catch(Exception e){Debug.LogException(e);Finish(e.ToString());}}
  [Serializable] sealed class Report {public string status;public Row[] rows;}
  static void Finish(string status){EditorApplication.update-=Tick;SessionState.SetString(Key+"status",status);
   File.WriteAllText("Library/LocomotionDiagnosis.json",JsonUtility.ToJson(new Report{status=status,rows=rows.ToArray()},true));EditorApplication.ExitPlaymode();}
  static IEnumerator Wait(float seconds){float until=Time.time+seconds;while(Time.time<until)yield return null;}
  static IEnumerator Flow(){
   while(DevelopmentRoomController.Current==null||DevelopmentRoomController.Current.Map==null)yield return null;
   var room=DevelopmentRoomController.Current;var p=room.player;
   foreach(var id in new[]{"class.novice",PhysicalBlessedAuthoring.ClassId,MagicalTouchedAuthoring.ClassId,DiceFree.Progression.GreatbladeProgressionRules.ClassId}){
    if(!room.SelectClass(id))throw new InvalidOperationException("Select "+id);
    yield return Wait(.3f);
    p.GetComponent<BasicAttack>().Cancel();p.Motor.Teleport(new Vector3(-10,0,-6));
    var driver=p.GetComponent<NovicePresentationDriver>();var animator=driver.VisualAnimator;
    if(animator==null)throw new InvalidOperationException("No runtime animator "+id);
    var controller=animator.runtimeAnimatorController as AnimatorController;
    var row=new Row{classId=id,controller=AssetDatabase.GetAssetPath(animator.runtimeAnimatorController),
     avatar=animator.avatar?.name,humanoid=animator.isHuman,
     states=controller?.layers[0].stateMachine.states.Select(s=>s.state.name).ToArray()};
    var left=animator.GetBoneTransform(HumanBodyBones.LeftFoot);
    var leg=animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
    var footStart=p.transform.InverseTransformPoint(left.position);var legStart=leg.localRotation;
    var start=p.transform.position;
    p.Motor.MoveTo(new Vector3(-10,0,10));
    float until=Time.time+2.3f;
    while(Time.time<until){
     row.maxSpeed=Mathf.Max(row.maxSpeed,driver.LastDrivenSpeed);
     row.footTravel=Mathf.Max(row.footTravel,Vector3.Distance(footStart,p.transform.InverseTransformPoint(left.position)));
     row.legAngle=Mathf.Max(row.legAngle,Quaternion.Angle(legStart,leg.localRotation));
     yield return null;
    }
    row.distance=Vector3.Distance(start,p.transform.position);
    var clip=animator.GetCurrentAnimatorClipInfo(0).FirstOrDefault().clip;
    row.clip=clip?.name;row.loop=clip!=null&&clip.isLooping;row.normalizedTime=animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    row.state=string.Join(",",row.states??Array.Empty<string>());
    p.Motor.Stop();yield return Wait(.7f);row.idleSpeed=driver.LastDrivenSpeed;rows.Add(row);
   }
  }
 }
}
