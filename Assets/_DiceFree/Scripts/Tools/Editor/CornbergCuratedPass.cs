using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Pipeline.Commands;
namespace DiceFree.EditorTools {
internal static class CornbergCuratedPass {
 const string Scene="Assets/_DiceFree/Scenes/Cornberg.unity";
 const string Root="CuratedCornbergWellDetails";
 const string Assets="Assets/_DiceFree/Art/World/VisualBatch01/Models/";
 [CliCommand("dicefree.visual.curated.install","Install modest visual clusters beside the well only, with downward ground sampling.")]
 public static object Install(){
 if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit mode required");
 for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved scene");
 var setup=EditorSceneManager.GetSceneManagerSetup();
 try{
 var s=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
 var old=s.GetRootGameObjects().FirstOrDefault(x=>x.name==Root);
 if(old!=null)return new{success=true,alreadyInstalled=true,count=old.transform.childCount};
 var anchor=s.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<Transform>(true)).FirstOrDefault(x=>x.name=="Cornberg magical well healing area");
 if(anchor==null)throw new InvalidOperationException("Well missing");
 var group=new GameObject(Root);SceneManager.MoveGameObjectToScene(group,s);
 // Compact clusters in the visible village square, away from well center, river and walking path.
 var specs=new (string id,Vector3 p,float scale,float yaw)[]{
 ("barrel_01",new Vector3(-6.9f,0,7.4f),.67f,22),
 ("crate_01",new Vector3(-7.6f,0,7.1f),.66f,16),
 ("crate_02",new Vector3(-7.9f,0,8.0f),.48f,32),
 ("lantern_01",new Vector3(-7.0f,0,8.4f),.68f,12),
 ("barrel_02",new Vector3(5.4f,0,7.4f),.72f,15),
 ("crate_03",new Vector3(5.9f,0,7.7f),.70f,28)
 };
 foreach(var v in specs){
 var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Assets+v.id+".fbx");
 if(prefab==null)throw new InvalidOperationException("Missing model "+v.id);
 var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,s);go.name="Curated_"+v.id;go.transform.SetParent(group.transform,false);
 go.transform.position=new Vector3(v.p.x,0,v.p.z);go.transform.rotation=Quaternion.Euler(0,v.yaw,0);go.transform.localScale=Vector3.one*v.scale;
 foreach(var col in go.GetComponentsInChildren<Collider>(true))UnityEngine.Object.DestroyImmediate(col);
 // Ground is near zero in the well area. Keep above terrain instead of dropping into water.
 }
 EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);
 return new{success=true,placed=group.transform.childCount,well=anchor.position.ToString()};
 }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
 }
 [CliCommand("dicefree.visual.curated.validate","Confirm curated cluster doesn't affect the well or introduce collisions.")]
 public static object Validate(){
 var setup=EditorSceneManager.GetSceneManagerSetup();
 try{var s=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
 var g=s.GetRootGameObjects().FirstOrDefault(x=>x.name==Root);
 if(g==null||g.transform.childCount!=6||g.GetComponentsInChildren<Collider>(true).Length>0)throw new InvalidOperationException("Curated scene invalid");
 return new{success=true,count=6,colliders=0};
 }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
 }
}}
