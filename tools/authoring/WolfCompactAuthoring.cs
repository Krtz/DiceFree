using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Core;
using DiceFree.Dungeons;
using DiceFree.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
public static class WolfCompactAuthoring
{
 const string Folder="Assets/_DiceFree/Settings/WolfSurvival";
 const string ScenePath="Assets/_DiceFree/Scenes/WolfSurvival.unity";
 public static object Author()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Clean Edit Mode required");
  var previous=SceneManager.GetActiveScene();var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
  try
  {
   var root=scene.GetRootGameObjects().Single(g=>g.name=="Moonlit Clearing - Wolf Survival").transform;
   if(root.Find("Forest cover - pack routes")!=null)throw new InvalidOperationException("Cover already authored; preserve it");
   const string backup="Assets/_DiceFree/Settings/Recovery/WolfSurvival-before-cover-20261105.unity";
   if(File.Exists(backup)||!EditorSceneManager.SaveScene(scene,backup,true))throw new InvalidOperationException("Recovery exists or failed");
   if(!AssetDatabase.CopyAsset(Folder+"/Wolf Survival NavMesh.asset","Assets/_DiceFree/Settings/Recovery/WolfSurvivalNav-before-cover-20261105.asset"))throw new InvalidOperationException("Nav recovery failed");
   SceneManager.SetActiveScene(scene);var cover=new GameObject("Forest cover - pack routes").transform;cover.SetParent(root,false);
   var rock=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/Faceted boulder.asset");var slate=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Forest slate.mat");var wood=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Weathered timber.mat");
   if(rock==null||slate==null||wood==null)throw new InvalidOperationException("Existing original art missing");
   void Rock(string name,Vector3 position,Vector3 size,float yaw)
   {
    var go=new GameObject(name);go.layer=9;go.transform.SetParent(cover,false);go.transform.localPosition=position;go.transform.localScale=size;go.transform.localRotation=Quaternion.Euler(0,yaw,0);
    go.AddComponent<MeshFilter>().sharedMesh=rock;go.AddComponent<MeshRenderer>().sharedMaterial=slate;go.AddComponent<BoxCollider>();go.AddComponent<TreeCameraOccluder>().ConfigureFocusDistance(14);
   }
   Rock("Western granite cover",new Vector3(-8,1.5f,0),new Vector3(4,3,4.2f),-12);
   Rock("Western low shoulder",new Vector3(-6.3f,.8f,1.4f),new Vector3(2.8f,1.6f,2.5f),23);
   Rock("Eastern split boulder",new Vector3(8,1.7f,4),new Vector3(3.8f,3.4f,4.4f),28);
   Rock("Eastern low shoulder",new Vector3(10,.75f,2),new Vector3(2.4f,1.5f,2.8f),-18);
   var log=GameObject.CreatePrimitive(PrimitiveType.Cylinder);log.name="Fallen timber beside cover";log.transform.SetParent(cover,false);log.transform.localPosition=new Vector3(6,.3f,6.8f);log.transform.localScale=new Vector3(.55f,1.8f,.55f);log.transform.localRotation=Quaternion.Euler(90,25,0);log.layer=2;log.GetComponent<Renderer>().sharedMaterial=wood;Object.DestroyImmediate(log.GetComponent<Collider>());
   Physics.SyncTransforms();var bounds=new Bounds(WolfDungeonRun.Origin+Vector3.up*4,new Vector3(85,25,85));var sources=new List<NavMeshBuildSource>();
   var markups=root.GetComponentsInChildren<Collider>().Where(c=>c.enabled&&c.gameObject.layer==9).Select(c=>new NavMeshBuildMarkup{root=c.transform,overrideArea=true,area=1}).ToList();
   NavMeshBuilder.CollectSources(bounds,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,markups,sources);
   var settings=NavMesh.GetSettingsByID(0);settings.agentRadius=.75f;settings.agentHeight=2.2f;settings.agentClimb=.4f;settings.agentSlope=40;settings.overrideVoxelSize=true;settings.voxelSize=.16f;
   var data=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);if(data==null)throw new InvalidOperationException("Cover navigation failed");
   var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(Folder+"/Wolf Survival NavMesh.asset");EditorUtility.CopySerialized(data,saved);Object.DestroyImmediate(data);EditorUtility.SetDirty(saved);
   var tuning=AssetDatabase.LoadAssetAtPath<WolfDungeonTuning>(Folder+"/Wolf Survival.asset");tuning.definition.stagingSeconds=30;tuning.intermissionSeconds=4;tuning.victorySeconds=5;EditorUtility.SetDirty(tuning.definition);EditorUtility.SetDirty(tuning);
   AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Arena save failed");
   var result=new{coverClusters=2,solidRocks=4,staging=30,intermission=4,victory=5,warning=2,waveBudgetsPreserved=true};File.WriteAllText("Logs/WolfCompactAuthoring.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
  }
  finally{EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(previous);}
 }
}
