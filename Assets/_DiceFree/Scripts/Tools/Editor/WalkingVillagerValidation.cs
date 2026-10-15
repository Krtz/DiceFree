using System;
using System.IO;
using System.Linq;
using DiceFree.World;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
using Object=UnityEngine.Object;
namespace DiceFree.EditorTools
{
 public static class WalkingVillagerValidation
 {
  static void Need(bool value,string m){if(!value)throw new InvalidOperationException(m);}
  [CliCommand("dicefree.villager-walk.validate","Prove all four wandering NPCs have looping clips, bound weights, moving leg bones and deforming visible meshes.")]
  public static object Validate(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode.");
   var setup=EditorSceneManager.GetSceneManagerSetup();int changed=0;
   try{
    var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
    var walkers=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<VillageHomeWander>(true)).ToArray();
    Need(walkers.Length==4,"Four village walkers required");
    foreach(var npc in walkers){
     var driver=npc.GetComponent<VillageWalkingAnimator>();Need(driver!=null&&driver.Model!=null,"Missing rig "+npc.name);
     Need(!npc.GetComponent<VillageVisualIdle>().enabled&&!npc.GetComponent<VillageCharacterGestures>().enabled,"Old rigid-body gait still enabled");
     Need(npc.GetComponent<Collider>()!=null&&npc.enabled,"Gameplay collision/wandering lost");
     var model=Object.Instantiate(npc.GetComponent<VillageVisualIdle>().Visual.gameObject);
     try{
      var animator=model.GetComponentInChildren<Animator>(true);
      var controller=animator.runtimeAnimatorController as AnimatorController;
      Need(controller!=null,"Missing controller");
      var walk=controller.layers[0].stateMachine.states.First(s=>s.state.name=="Walk").state.motion as AnimationClip;
      var idle=controller.layers[0].stateMachine.states.First(s=>s.state.name=="Idle").state.motion as AnimationClip;
      Need(walk!=null&&idle!=null&&walk.isLooping&&idle.isLooping&&!animator.applyRootMotion,"Walk/idle loop or root motion");
      var leg=animator.GetComponentsInChildren<Transform>(true).First(t=>t.name=="LeftUpperLeg");
      var skin=animator.GetComponentsInChildren<SkinnedMeshRenderer>(true).First(s=>s.name=="ClothPrimary_mesh");
      Need(skin.sharedMesh.boneWeights.Length==skin.sharedMesh.vertexCount,"Unbound clothing vertices");
      Need(animator.GetComponentsInChildren<SkinnedMeshRenderer>(true).All(s=>s.sharedMaterials.All(m=>m!=null&&m.shader.name.StartsWith("Universal Render Pipeline/"))),"Unsupported or missing village materials");
      var a=new Mesh();var b=new Mesh();
      try{
       walk.SampleAnimation(animator.gameObject,.08f);var q=leg.localRotation;skin.BakeMesh(a);
       walk.SampleAnimation(animator.gameObject,.35f);skin.BakeMesh(b);
       Need(Quaternion.Angle(q,leg.localRotation)>5,"Walking leg does not articulate");
       Need(a.vertexCount==b.vertexCount,"Pose changed geometry count");
       int moved=a.vertices.Zip(b.vertices,(x,y)=>(x-y).sqrMagnitude>.000001f?1:0).Sum();
       Need(moved>20,"Rig does not deform its trousers");changed+=moved;
      }finally{Object.DestroyImmediate(a);Object.DestroyImmediate(b);}
     }finally{Object.DestroyImmediate(model);}
    }
    return new{marker="DICEFREE_WALKING_VILLAGERS_POSE_OK",walkers=walkers.Length,changedClothingVertices=changed};
   }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
  }
  [CliCommand("dicefree.villager-walk.capture","Render the existing walker appearances at idle and two opposite walk phases.")]
  public static object Capture(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode.");
   var setup=EditorSceneManager.GetSceneManagerSetup();Scene stage=default;RenderTexture rt=null;Camera camera=null;
   try{
    var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity");
    var walkers=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<VillageWalkingAnimator>(true))
     .GroupBy(w=>w.GetComponent<VillageCharacterGestures>().Role).Select(g=>g.First()).ToArray();
    stage=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
    for(int col=0;col<walkers.Length;col++)for(int row=0;row<3;row++){
     var model=Object.Instantiate(walkers[col].GetComponent<VillageVisualIdle>().Visual.gameObject);
     SceneManager.MoveGameObjectToScene(model,stage);model.transform.SetPositionAndRotation(new Vector3((col-1)*3,0,(1-row)*3.2f),Quaternion.Euler(0,180,0));
     foreach(var t in model.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
     var animator=model.GetComponentInChildren<Animator>(true);var controller=(AnimatorController)animator.runtimeAnimatorController;
     var clip=(AnimationClip)controller.layers[0].stateMachine.states.First(s=>s.state.name==(row==0?"Idle":"Walk")).state.motion;
     clip.SampleAnimation(animator.gameObject,row==0?.05f:row==1?.20f:.70f);
    }
    var cam=new GameObject("Walker pose camera");SceneManager.MoveGameObjectToScene(cam,stage);camera=cam.AddComponent<Camera>();
    camera.orthographic=true;camera.orthographicSize=5.7f;camera.cullingMask=1<<31;
    camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.15f,.19f);
    camera.transform.position=new Vector3(4,10,-17);camera.transform.LookAt(new Vector3(0,1,0));
    var light=new GameObject("Walker pose light").AddComponent<Light>();SceneManager.MoveGameObjectToScene(light.gameObject,stage);
    light.type=LightType.Directional;light.intensity=1.5f;light.transform.rotation=Quaternion.Euler(45,-30,0);
    rt=new RenderTexture(1280,1000,24);camera.targetTexture=rt;camera.Render();
    var prior=RenderTexture.active;RenderTexture.active=rt;
    var image=new Texture2D(1280,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,1000),0,0);image.Apply();
    RenderTexture.active=prior;string output=Path.GetFullPath("docs/screenshots/WalkingVillagers-Poses.png");
    File.WriteAllBytes(output,image.EncodeToPNG());Object.DestroyImmediate(image);
    return new{file=output,rows="idle / walk phase .20 / walk phase .70",roles=walkers.Select(w=>w.GetComponent<VillageCharacterGestures>().Role.ToString()).ToArray()};
   }finally{if(camera!=null)camera.targetTexture=null;if(rt!=null){rt.Release();Object.DestroyImmediate(rt);}
    if(stage.IsValid())EditorSceneManager.CloseScene(stage,true);EditorSceneManager.RestoreSceneManagerSetup(setup);}
  }
 }
}
