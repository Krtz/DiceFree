using System;
using System.Linq;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace DiceFree.EditorTools{
internal static class RemoveGiantGreenHill{
const string Path="Assets/_DiceFree/Scenes/Cornberg.unity";
[CliCommand("dicefree.visual.remove-green-hill","Remove the huge green distant hill sphere overlapping eastern playable forest.")]
public static object Remove(){
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Exit Play Mode");
for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)
  throw new InvalidOperationException("Unsaved scene");
var setup=EditorSceneManager.GetSceneManagerSetup();
try{
var s=EditorSceneManager.OpenScene(Path,OpenSceneMode.Single);
var hill=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true))
 .FirstOrDefault(t=>t.name=="Distant wooded hill");
if(hill==null)return new{success=true,alreadyRemoved=true};
var renderer=hill.GetComponent<Renderer>();
if(renderer==null||renderer.bounds.size.x<100f||renderer.bounds.size.z<90f)
 throw new InvalidOperationException("Wrong scenery selected");
Vector3 center=renderer.bounds.center, size=renderer.bounds.size;
UnityEngine.Object.DestroyImmediate(hill.gameObject);
EditorSceneManager.MarkSceneDirty(s);
if(!EditorSceneManager.SaveScene(s))throw new InvalidOperationException("Save failed");
return new{success=true,removed="Distant wooded hill",center=center.ToString("F1"),size=size.ToString("F1")};
}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
}
}
}