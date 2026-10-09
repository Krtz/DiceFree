using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
namespace DiceFree.EditorTools {
internal static class HealingWellVisualUpgrade {
const string ScenePath="Assets/_DiceFree/Scenes/Cornberg.unity";
const string ModelPath="Assets/_DiceFree/Art/World/CornbergHealingWell/Models/CornbergHealingWell.fbx";
const string Root="Assets/_DiceFree/Art/World/CornbergHealingWell";
const string GroupName="VisualOverhaul_HealingWell";
static GameObject Find(Scene s,string name) => s.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name==name)?.gameObject;
[CliCommand("dicefree.visual01.well.install","Add the Blender-authored well above the existing functional healing area, preserve gameplay.")]
public static object Install(){
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Requires Edit Mode");
foreach(var s in Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt))if(s.isDirty)throw new InvalidOperationException("Open scene has unsaved edits");
var setup=EditorSceneManager.GetSceneManagerSetup();
try {
var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
var anchor=Find(scene,"Cornberg magical well healing area");
if(anchor==null)throw new InvalidOperationException("Healing well functionality absent");
var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
if(prefab==null)throw new InvalidOperationException("Imported custom well model unavailable");
var existing=scene.GetRootGameObjects().FirstOrDefault(g=>g.name==GroupName);
if(existing!=null)return new{success=true,alreadyInstalled=true,position=existing.transform.position.ToString()};
var group=new GameObject(GroupName);SceneManager.MoveGameObjectToScene(group,scene);
group.transform.position=anchor.transform.position;
var visual=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
visual.name="Original_Blender_Healing_Well";
visual.transform.SetParent(group.transform,false);
visual.transform.localPosition=Vector3.zero;
visual.transform.localScale=Vector3.one;
foreach(var collider in visual.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(collider);
var urp=Shader.Find("Universal Render Pipeline/Lit");
if(urp!=null){
string materials=Root+"/Materials";
if(!AssetDatabase.IsValidFolder(materials))AssetDatabase.CreateFolder(Root,"Materials");
foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true)){
var originals=renderer.sharedMaterials;var replacements=new Material[originals.Length];
for(int i=0;i<originals.Length;i++){
string name=originals[i]!=null?originals[i].name:"Old limestone";
string path=materials+"/"+System.Text.RegularExpressions.Regex.Replace(name,@"[^a-zA-Z0-9_-]","_")+".mat";
var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
if(mat==null){mat=new Material(urp){name=name};
Color c=new Color(.52f,.50f,.42f);
if(name.Contains("oak")||name.Contains("Oak"))c=new Color(.31f,.19f,.10f);
if(name.Contains("iron")||name.Contains("Iron"))c=new Color(.19f,.19f,.20f);
if(name.Contains("moss")||name.Contains("Moss"))c=new Color(.23f,.40f,.18f);
if(name.Contains("water"))c=new Color(.17f,.60f,.68f);
if(name.Contains("trim")||name.Contains("enchanted"))c=new Color(.75f,.62f,.29f);
mat.SetColor("_BaseColor",c);AssetDatabase.CreateAsset(mat,path);}
replacements[i]=mat;}renderer.sharedMaterials=replacements;}
}
EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
return new{success=true,model=ModelPath,anchor=anchor.transform.position.ToString(),renderers=visual.GetComponentsInChildren<Renderer>(true).Length,healingPreserved=true};
}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
}
[CliCommand("dicefree.visual01.well.validate","Verify custom model exists without replacing Cornberg healing functionality.")]
public static object Validate(){
var setup=EditorSceneManager.GetSceneManagerSetup();
try {var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
var well=scene.GetRootGameObjects().FirstOrDefault(g=>g.name==GroupName);
var anchor=Find(scene,"Cornberg magical well healing area");
if(well==null||anchor==null)throw new InvalidOperationException("Visual well or healing anchor absent");
if((well.transform.position-anchor.transform.position).sqrMagnitude>.001f)throw new InvalidOperationException("Visual model not aligned with healing anchor");
int colliders=well.GetComponentsInChildren<Collider>(true).Length;int renderers=well.GetComponentsInChildren<Renderer>(true).Length;
if(colliders!=0||renderers<20)throw new InvalidOperationException("Model invalid: "+colliders+" colliders and "+renderers+" renderers");
return new{success=true,renderers,colliders,marker="VISUAL_WELL_PRESERVED"};
}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
}
}
}