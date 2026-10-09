using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
namespace DiceFree.EditorTools {
internal static class CornbergHousePass {
const string Scene="Assets/_DiceFree/Scenes/Cornberg.unity";
const string Group="CuratedCornbergHouseDetails";
const string Folder="Assets/_DiceFree/Art/World/VisualBatch01/Models/";
[CliCommand("dicefree.visual.houses.install","Add small intentional house-front clusters visible from Cornberg well camera.")]
public static object Install(){
if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Requires Edit Mode");
for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved scene edits");
var setup=EditorSceneManager.GetSceneManagerSetup();
try{
var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
var prior=scene.GetRootGameObjects().FirstOrDefault(x=>x.name==Group);
if(prior!=null)return new{success=true,alreadyInstalled=true,count=prior.transform.childCount};
var root=new GameObject(Group);SceneManager.MoveGameObjectToScene(root,scene);
var specs=new (string id,float x,float z,float scale,float yaw)[]{
("barrel_03",10.3f,8.3f,.67f,5),("crate_04",11.2f,8.1f,.69f,22),
("lantern_02",10.7f,9.5f,.65f,0),


};
foreach(var v in specs){
var model=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+v.id+".fbx");
if(model==null)throw new InvalidOperationException("Missing "+v.id);
var go=(GameObject)PrefabUtility.InstantiatePrefab(model,scene);
go.transform.SetParent(root.transform,false);go.transform.position=new Vector3(v.x,0,v.z);
go.transform.rotation=Quaternion.Euler(0,v.yaw,0);go.transform.localScale=Vector3.one*v.scale;
go.name="HouseDetail_"+v.id;
foreach(var c in go.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(c);
}
EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
return new{success=true,count=root.transform.childCount,colliders=0};
}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
}
[CliCommand("dicefree.visual.houses.trim","Remove three river-side trial props after visual inspection.")] public static object Trim(){var old=EditorSceneManager.GetSceneManagerSetup();try{var s=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var g=s.GetRootGameObjects().FirstOrDefault(x=>x.name==Group);if(g==null)throw new InvalidOperationException("House group missing");int removed=0;foreach(var n in new[]{"HouseDetail_log_02","HouseDetail_stump_02","HouseDetail_crate_02"}){var t=g.transform.Cast<Transform>().FirstOrDefault(x=>x.name==n);if(t!=null){UnityEngine.Object.DestroyImmediate(t.gameObject);removed++;}}EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);return new{removed,remaining=g.transform.childCount};}finally{EditorSceneManager.RestoreSceneManagerSetup(old);}}
[CliCommand("dicefree.visual.houses.validate","Check curated house props and no new colliders.")]
public static object Validate(){
var setup=EditorSceneManager.GetSceneManagerSetup();try{
var s=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var g=s.GetRootGameObjects().FirstOrDefault(x=>x.name==Group);
if(g==null||g.transform.childCount!=3||g.GetComponentsInChildren<Collider>(true).Length!=0)throw new InvalidOperationException("House detail validation failed");
return new{success=true,count=3,colliders=0};
}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
}
}}

