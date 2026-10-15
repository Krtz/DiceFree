using System;
using System.Linq;
using System.Collections.Generic;
using DiceFree.World;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Pipeline.Commands;
using Object=UnityEngine.Object;
namespace DiceFree.EditorTools
{
 public static class WalkingVillagerAuthoring
 {
  const string Root="Assets/_DiceFree/Art/World/WalkingVillagers";
  static void Ensure(string path){if(AssetDatabase.IsValidFolder(path))return;var parent=System.IO.Path.GetDirectoryName(path).Replace('\\','/');Ensure(parent);AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(path));}
  static Bounds Bounds(GameObject go){var renderers=go.GetComponentsInChildren<Renderer>(true);if(renderers.Length==0)throw new InvalidOperationException("No NPC renderers");var b=renderers[0].bounds;foreach(var r in renderers.Skip(1))b.Encapsulate(r.bounds);return b;}
  [CliCommand("dicefree.villager-walk.install","Install first-party rigged walk/idle variants on only the four existing village wanderers; preserve NPC gameplay and original sources.")]
  public static object Install(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode.");
   var setup=EditorSceneManager.GetSceneManagerSetup();int count=0;
   try{
    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);Ensure(Root+"/Controllers");Ensure(Root+"/Prefabs");
    var models=new Dictionary<string,GameObject>();
    foreach(var role in new[]{"Farmer","Villager","Herbalist"}){
     string path=Root+"/Models/NPC_"+role+".fbx";
     var importer=AssetImporter.GetAtPath(path) as ModelImporter;
     if(importer==null)throw new InvalidOperationException("Missing derived walker "+path);
     importer.animationType=ModelImporterAnimationType.Generic;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.importAnimation=true;importer.importCameras=false;importer.importLights=false;importer.addCollider=false;
     importer.materialImportMode=ModelImporterMaterialImportMode.None;
     var clips=importer.clipAnimations.Length>0?importer.clipAnimations:importer.defaultClipAnimations;
     foreach(var clip in clips){clip.loopTime=true;clip.loopPose=true;}
     importer.clipAnimations=clips;importer.SaveAndReimport();
     var imported=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();
     var idle=imported.First(c=>c.name.EndsWith("Idle"));var walk=imported.First(c=>c.name.EndsWith("Walk"));
     string controllerPath=Root+"/Controllers/"+role+".controller";
     var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
     if(controller==null){
      controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
      var machine=controller.layers[0].stateMachine;var a=machine.AddState("Idle");a.motion=idle;machine.defaultState=a;
      var b=machine.AddState("Walk");b.motion=walk;
      var move=a.AddTransition(b);move.hasExitTime=false;move.duration=.12f;move.AddCondition(AnimatorConditionMode.Greater,.08f,"Speed");
      var stop=b.AddTransition(a);stop.hasExitTime=false;stop.duration=.12f;stop.AddCondition(AnimatorConditionMode.Less,.05f,"Speed");
     }
     models[role]=AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
    var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
    var walkers=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<VillageHomeWander>(true)).ToArray();
    if(walkers.Length!=4)throw new InvalidOperationException("Expected the four authored wanderers, found "+walkers.Length);
    foreach(var wander in walkers){
     var idle=wander.GetComponent<VillageVisualIdle>();if(idle==null||idle.Visual==null)throw new InvalidOperationException("Walker lacks its original visual");
     string role=wander.GetComponent<VillageCharacterGestures>().Role.ToString();
     if(!models.ContainsKey(role))throw new InvalidOperationException("Unexpected walker role "+role);
     if(wander.GetComponent<VillageWalkingAnimator>()!=null){count++;continue;}
     var container=idle.Visual.gameObject;var before=Bounds(container);
     var materials=container.GetComponentsInChildren<Renderer>(true).GroupBy(r=>r.name).ToDictionary(g=>g.Key,g=>g.First().sharedMaterials);
     var template=Object.Instantiate(models[role]);template.name="Rigged "+role;
     var animator=template.GetComponentInChildren<Animator>(true);
     if(animator==null)throw new InvalidOperationException("Derived walker has no Animator");
     animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<AnimatorController>(Root+"/Controllers/"+role+".controller");
     animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
     foreach(var renderer in template.GetComponentsInChildren<SkinnedMeshRenderer>(true)){
      if(!materials.TryGetValue(renderer.name,out var slots))throw new InvalidOperationException("Original material mapping missing: "+renderer.name);
      renderer.sharedMaterials=slots;renderer.updateWhenOffscreen=true;
     }
     string prefab=Root+"/Prefabs/"+role+".prefab";PrefabUtility.SaveAsPrefabAsset(template,prefab);Object.DestroyImmediate(template);
     idle.enabled=false;wander.GetComponent<VillageCharacterGestures>().enabled=false;
     foreach(Transform child in idle.Visual.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
     var copy=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefab),idle.Visual);
     copy.transform.localPosition=Vector3.zero;copy.transform.localRotation=Quaternion.identity;copy.transform.localScale=Vector3.one;
     var after=Bounds(copy);copy.transform.localScale*=before.size.y/Mathf.Max(.001f,after.size.y);
     after=Bounds(copy);copy.transform.position+=new Vector3(before.center.x-after.center.x,before.min.y-after.min.y,before.center.z-after.center.z);
     var driver=wander.gameObject.AddComponent<VillageWalkingAnimator>();driver.Configure(copy.GetComponentInChildren<Animator>(true));
     EditorUtility.SetDirty(driver);EditorUtility.SetDirty(idle);count++;
    }
    EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Could not save rigged NPC composition");
    AssetDatabase.SaveAssets();return new{marker="DICEFREE_WALKING_VILLAGERS_INSTALLED",walkers=count};
   }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
  }
 }
}
