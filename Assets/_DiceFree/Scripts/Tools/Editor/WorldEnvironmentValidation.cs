using System;using System.IO;using System.Linq;using System.Collections;using System.Collections.Generic;
using DiceFree.Core;using DiceFree.Advancement;using DiceFree.Characters;using DiceFree.Combat;using DiceFree.Development;using DiceFree.Dungeons;using DiceFree.Items;using DiceFree.Persistence;using DiceFree.Progression;using DiceFree.Quests;using DiceFree.UI;using DiceFree.Visuals;using DiceFree.World;using DiceFree.AI;using DiceFree.Briarback;
using UnityEngine;using UnityEngine.AI;using UnityEditor;using UnityEditor.SceneManagement;using Unity.Pipeline.Commands;using Object=UnityEngine.Object;
namespace DiceFree.EditorTools
{
 [InitializeOnLoad] public static class WorldEnvironmentValidation
 {
  const string Key="DiceFree.Environment.";static readonly Stack<IEnumerator> steps=new();static double deadline;static int checks;
  [Serializable]sealed class Report{public string status,error;public int checks,walked,detailedTrees;public float sunShadowShift,moonShadowShift;public bool sunShadows,moonShadows;}
  static Report report;
  static WorldEnvironmentValidation()=>EditorApplication.playModeStateChanged+=Changed;
  static void Need(bool value,string text){if(!value)throw new InvalidOperationException(text);checks++;}
  static void Phase(string p){SessionState.SetString(Key+"phase",p);SessionState.SetInt(Key+"checks",checks);}
  [CliCommand("dicefree.environment.test","Verify textured tree integration, live clock, dungeon lighting isolation and GPU sun/moon shadow displacement.")]
  public static object Start(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode only");
   SessionState.SetString(Key+"scene",EditorSceneManager.GetActiveScene().path);SessionState.SetBool(Key+"options",EditorSettings.enterPlayModeOptionsEnabled);SessionState.SetInt(Key+"flags",(int)EditorSettings.enterPlayModeOptions);
   SessionState.SetBool(Key+"cleanup",true);SessionState.SetBool(Key+"resume",false);SessionState.SetString(Key+"status","running");SessionState.SetString(Key+"phase","initializing");SessionState.SetString(Key+"error","");SessionState.SetInt(Key+"checks",0);
   string root=Path.Combine(Path.GetTempPath(),"DiceFree-Environment-"+Guid.NewGuid().ToString("N"));SessionState.SetString(Key+"root",root);PersistenceTestGuard.UseIsolatedSaveRootForNextPlay(root);EditorSettings.enterPlayModeOptionsEnabled=false;
   EditorSceneManager.OpenScene(BriarbackAuthoring.Scene);EditorApplication.EnterPlaymode();return Status();
  }
  [CliCommand("dicefree.environment.status","Read the focused tree and lighting verification.")]
  public static object Status()=>new{status=SessionState.GetString(Key+"status","idle"),phase=SessionState.GetString(Key+"phase",""),checks=SessionState.GetInt(Key+"checks",0),error=SessionState.GetString(Key+"error","")};
  static void Changed(PlayModeStateChange s){
   if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetString(Key+"status","")=="running"){checks=0;report=new Report();steps.Clear();steps.Push(Flow());deadline=EditorApplication.timeSinceStartup+600;EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
   if(s==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+"cleanup",false)){
    SessionState.SetBool(Key+"cleanup",false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;PersistenceTestGuard.ClearValidationOverrides();
    EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt(Key+"flags",0);EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Key+"options",false);
    string p=SessionState.GetString(Key+"scene","");if(!string.IsNullOrWhiteSpace(p))EditorSceneManager.OpenScene(p);
   }
  }
  static void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception)Finish(false,text+"\n"+stack);}
  static void Tick(){if(!EditorApplication.isPlaying)return;try{
   if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Environment validation timed out");
   while(steps.Count>0){var step=steps.Peek();if(!step.MoveNext()){steps.Pop();continue;}if(step.Current is IEnumerator child){steps.Push(child);continue;}return;}Finish(true,null);
  }catch(Exception e){Finish(false,e.ToString());}}
  static void Finish(bool pass,string error){
   EditorApplication.update-=Tick;Application.logMessageReceived-=Log;report??=new Report();report.status=pass?"passed":"failed";report.error=error;report.checks=checks;
   SessionState.SetString(Key+"status",report.status);SessionState.SetString(Key+"error",error??"");SessionState.SetInt(Key+"checks",checks);File.WriteAllText("Library/EnvironmentPlayableResults.json",JsonUtility.ToJson(report,true));EditorApplication.ExitPlaymode();
  }
  static IEnumerator Wait(float t){float end=Time.time+t;while(Time.time<end)yield return null;}
  static IEnumerator Frames(int n){for(int i=0;i<n;i++)yield return null;}
  static Texture2D Render(Camera camera){
   var target=RenderTexture.GetTemporary(960,600,24,RenderTextureFormat.ARGB32);var old=camera.targetTexture;var active=RenderTexture.active;var image=new Texture2D(960,600,TextureFormat.RGB24,false);
   try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,960,600),0,0);image.Apply();return image;}
   finally{camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);}
  }
  static float Picture(Camera camera,string name){var image=Render(camera);float mean=image.GetPixels().Average(c=>c.grayscale);File.WriteAllBytes("Library/Environment-"+name+".png",image.EncodeToPNG());Object.Destroy(image);return mean;}
  static Vector2 Shadow(Camera camera,WorldDayNightCycle clock,float hour){
   clock.SetHour(hour,false);clock.CelestialLight.shadows=LightShadows.Soft;var shadow=Render(camera);
   clock.CelestialLight.shadows=LightShadows.None;var plain=Render(camera);clock.CelestialLight.shadows=LightShadows.Soft;
   var a=shadow.GetPixels();var b=plain.GetPixels();float weight=0;Vector2 center=Vector2.zero;int pixels=0;
   for(int i=0;i<a.Length;i++){float d=b[i].grayscale-a[i].grayscale;if(d<.008f)continue;weight+=d;center+=new Vector2(i%960,i/960)*d;pixels++;}
   File.WriteAllBytes("Library/Environment-Shadow-"+hour+".png",shadow.EncodeToPNG());Object.Destroy(shadow);Object.Destroy(plain);
   Need(pixels>100&&weight>.5f,"No GPU shadow pixels at hour "+hour+" pixels="+pixels+" weight="+weight);return center/weight;
  }
  static IEnumerator Flow(){
   CombatActor p=null;while(p==null){p=Object.FindAnyObjectByType<TraversalInput>()?.GetComponent<CombatActor>();yield return null;}
   var save=p.GetComponent<ManifestationPersistence>();while(!save.Ready||!p.Motor.Ready)yield return null;
   p.GetComponent<TraversalInput>().enabled=false;p.GetComponent<CombatInput>().enabled=false;p.GetComponent<BasicAttack>().enabled=false;p.Health.Invulnerable=true;
   Phase("tree integration and live clock");
   var trees=Object.FindObjectsByType<DetailedTreeVisual>();report.detailedTrees=trees.Length;Need(trees.Length>800,"Missing detailed trees");
   foreach(var tree in trees){
    Need(tree.DetailedRoot!=null,"Missing local tree resource");
    Need(tree.Originals.All(r=>!r.enabled),"Original overlaps replacement");
    Need(tree.DetailedRoot.GetComponentsInChildren<Collider>(true).Length==0,"Replacement changes collision");
    Need(tree.DetailedRoot.GetComponentInChildren<LODGroup>().GetLODs().Length==3,"Missing tree LODs");
    Need(tree.DetailedRoot.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial.shader.name=="Universal Render Pipeline/Lit"&&r.sharedMaterial.GetTexture("_BaseMap")!=null&&r.sharedMaterial.IsKeywordEnabled("_ALPHATEST_ON")),"Leaf material integration");
   }
   var landmark=Object.FindObjectsByType<Renderer>().Single(r=>r.name=="Ancient trunk");
   Need(landmark.bounds.min.z>BriarbackAuthoring.Bounds.yMax+80,"Distant city backdrop overlaps playable region");
   var clock=Object.FindAnyObjectByType<WorldDayNightCycle>();Need(clock!=null&&clock.CycleMinutes==48,"Outdoor clock missing");
   clock.SetHour(10,true);int frame=Time.frameCount;float hour=clock.Hour;yield return Wait(.5f);Need(Time.frameCount>frame+5&&clock.Hour>hour,"Clock or game frozen");
   clock.SetHour(12,false);hour=clock.Hour;yield return Wait(.25f);Need(Mathf.Approximately(hour,clock.Hour),"Paused clock advanced");
   clock.SetInterior(true);Need(clock.IsInterior&&Mathf.Approximately(clock.CelestialLight.intensity,1.8f),"Dungeon lighting not isolated");
   clock.SetHour(0,false);Need(Mathf.Approximately(clock.CelestialLight.intensity,1.8f),"Night darkens dungeon");clock.SetInterior(false);Need(clock.MoonActive&&clock.CelestialLight.intensity>.3f,"Moon handover");
   Need(NavMesh.SamplePosition(new Vector3(50,0,38),out var photoSpot,15,1),"Forest photo path");
   Need(p.Motor.Teleport(photoSpot.position),"Forest photo relocation");Object.FindAnyObjectByType<ExplorationCamera>().RecenterForTravel(p.transform);
   yield return Frames(10);
   foreach(var sample in new[]{(12f,"Noon"),(18.3f,"Dusk"),(0f,"Night")}){clock.SetHour(sample.Item1,false);yield return Frames(5);Picture(Camera.main,sample.Item2);}
   Need((Camera.main.cullingMask&(1<<31))==0&&(clock.CelestialLight.cullingMask&(1<<31))==0,"Portrait light isolation");
   Phase("GPU shadow movement");
   var fixture=new GameObject("Temporary shadow verification fixture");var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=new Color(.18f,.18f,.18f);
   var cameraGo=new GameObject("Shadow test camera");var camera=cameraGo.AddComponent<Camera>();camera.enabled=false;camera.cullingMask=~(1<<31);camera.orthographic=true;camera.orthographicSize=12;camera.nearClipPlane=.1f;camera.farClipPlane=80;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
   bool fog=RenderSettings.fog;
   try{
    var plane=GameObject.CreatePrimitive(PrimitiveType.Cube);plane.transform.SetParent(fixture.transform);plane.transform.position=new Vector3(2000,-.1f,2000);plane.transform.localScale=new Vector3(35,.2f,35);plane.GetComponent<Renderer>().sharedMaterial=material;
    var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);cube.transform.SetParent(fixture.transform);cube.transform.position=new Vector3(2000,2,2000);cube.transform.localScale=new Vector3(2,4,2);cube.GetComponent<Renderer>().sharedMaterial=material;
    camera.transform.position=new Vector3(2000,28,2000);camera.transform.rotation=Quaternion.Euler(90,0,0);RenderSettings.fog=false;
    yield return Frames(3);
    var morning=Shadow(camera,clock,9);var afternoon=Shadow(camera,clock,15);report.sunShadowShift=Vector2.Distance(morning,afternoon);Need(report.sunShadowShift>20,"Sun shadow did not move");report.sunShadows=true;
    var evening=Shadow(camera,clock,21);var late=Shadow(camera,clock,3);report.moonShadowShift=Vector2.Distance(evening,late);Need(report.moonShadowShift>20,"Moon shadow did not move");report.moonShadows=true;
   }finally{RenderSettings.fog=fog;Object.Destroy(fixture);Object.Destroy(cameraGo);Object.Destroy(material);}
   Phase("actual forest captures and navigation");
   Need(NavMesh.SamplePosition(new Vector3(50,0,38),out var spot,15,1),"Forest path sample");
   Need(p.Motor.Teleport(spot.position),"Forest relocation");var exploration=Object.FindAnyObjectByType<ExplorationCamera>();exploration.RecenterForTravel(p.transform);
   var main=Camera.main;yield return Frames(8);
   foreach(var sample in new[]{(12f,"Noon"),(18.3f,"Dusk"),(0f,"Night")}){clock.SetHour(sample.Item1,false);yield return Frames(5);Picture(main,sample.Item2);}
   Need(trees.All(t=>t.Originals.All(r=>!r.enabled)),"Original tree renderers reactivated after visibility/fade updates");
   var from=p.transform.position;Need(NavMesh.SamplePosition(from+Vector3.forward*8,out var next,5,1),"Nearby path");
   Need(p.Motor.MoveTo(next.position),"Walk order");float until=Time.time+8;
   while(Vector3.Distance(p.transform.position,next.position)>1&&Time.time<until)yield return null;
   Need(Vector3.Distance(p.transform.position,next.position)<=1,"Visual replacements blocked walking");report.walked=1;
   clock.SetHour(10,true);yield return Frames(3);
  }
 }
}
