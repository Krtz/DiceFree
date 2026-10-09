using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
namespace DiceFree.EditorTools {
internal static class VisualCleanup {
const string Path="Assets/_DiceFree/Scenes/Cornberg.unity";
static readonly string[] Groups={"VisualOverhaulBatch01_Decoration","VIS02_CornbergVillageLife","VisualOverhaul_HealingWell"};
[CliCommand("dicefree.visual.cleanup","Remove flawed free-placed decoration and upside-down well overlay; restore original scene presentation and healing functionality.")]
public static object Run(){
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit mode only");
for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved work; abort");
var setup=EditorSceneManager.GetSceneManagerSetup();
try {
var scene=EditorSceneManager.OpenScene(Path,OpenSceneMode.Single);
var roots=scene.GetRootGameObjects();
if(!roots.SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).Any(t=>t.name=="Cornberg magical well healing area"))throw new InvalidOperationException("Existing healing anchor missing");
int removed=0;
foreach(var name in Groups){var group=scene.GetRootGameObjects().FirstOrDefault(x=>x.name==name);if(group!=null){removed+=group.transform.childCount;UnityEngine.Object.DestroyImmediate(group);}}
if(removed>0){EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);}
return new{success=true,removed,healingAnchorPreserved=true,marker="VISUAL_CLEANUP_OK"};
}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
}
}}
