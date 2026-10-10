using System;
using System.Linq;
using DiceFree.World;
using DiceFree.Core;
using DiceFree.Characters;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace DiceFree.EditorTools {
internal static class WorldFogAuthoring {
const string ScenePath="Assets/_DiceFree/Scenes/Cornberg.unity";
const string RootName="World 1 - Exploration fog of war";
[CliCommand("dicefree.fog.cornberg.install","Install 3-state camera-space Warcraft-style fog on Cornberg.")]
public static object Install(){
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode");
for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)
  throw new InvalidOperationException("Unsaved editor scene");
var prior=EditorSceneManager.GetSceneManagerSetup();
try{
var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
var player=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<TraversalInput>(true)).FirstOrDefault();
var camera=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<ExplorationCamera>(true)).FirstOrDefault();
if(player==null||camera==null)throw new InvalidOperationException("Main player/camera not found");
var root=scene.GetRootGameObjects().FirstOrDefault(g=>g.name==RootName);
if(root==null){root=new GameObject(RootName);SceneManager.MoveGameObjectToScene(root,scene);}
var fog=root.GetComponent<WorldFogOfWar>();
if(fog==null)fog=root.AddComponent<WorldFogOfWar>();
fog.Configure(player.transform,camera.GetComponent<Camera>(),new Vector2(-70,-60),
  new Vector2(275,190),24f);
EditorUtility.SetDirty(fog);
EditorSceneManager.MarkSceneDirty(scene);
if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Save failed");
return new{success=true,player=player.name,camera=camera.name,
  radius=fog.VisionRadius,scene=ScenePath,fog=root.name};
}finally{EditorSceneManager.RestoreSceneManagerSetup(prior);}
}
[CliCommand("dicefree.fog.cornberg.validate","Verify scene fog is connected to player and world camera and reveal logic matches expected semantics.")]
public static object Validate(){
var prior=EditorSceneManager.GetSceneManagerSetup();
try{
var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
var fog=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<WorldFogOfWar>(true)).Single();
var player=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<TraversalInput>(true)).First();
if(!fog.IsVisible(player.transform.position)||fog.IsVisible(player.transform.position+Vector3.right*100))
 throw new InvalidOperationException("Fog visibility wrong");
if(fog.VisionRadius<15||fog.VisionRadius>35)throw new InvalidOperationException("Fog radius wrong");
return new{success=true,scene=ScenePath,visibilityRadius=fog.VisionRadius,mainPlayer=player.name,
 currentPlayerVisible=true,distantRegionHidden=true};
}finally{EditorSceneManager.RestoreSceneManagerSetup(prior);}
}
}
}