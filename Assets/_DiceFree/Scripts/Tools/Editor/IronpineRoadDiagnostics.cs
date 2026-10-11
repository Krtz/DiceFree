using System;
using System.Linq;
using Unity.Pipeline.Commands;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiceFree.EditorTools
{
 internal static class IronpineRoadDiagnostics
 {
 [CliCommand("dicefree.ironpine.road-debug","Inspect road mesh vertex, triangle winding and world elevation.")]
 public static object DebugRoad()
 {
 var setup=EditorSceneManager.GetSceneManagerSetup();
 try{
 var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity",OpenSceneMode.Single);
 var a=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).ToArray();
 var road=a.First(t=>t.name=="Winding pine forest road");
 var mesh=road.GetComponent<MeshFilter>().sharedMesh;
 var renderer=road.GetComponent<MeshRenderer>();
 var vertices=mesh.vertices;
 var indices=mesh.triangles;
 Vector3 v0=road.TransformPoint(vertices[indices[0]]);
 Vector3 v1=road.TransformPoint(vertices[indices[1]]);
 Vector3 v2=road.TransformPoint(vertices[indices[2]]);
 var normal=Vector3.Cross(v1-v0,v2-v0).normalized;
 var old=a.First(t=>t.name=="Mountain to east road");
 var oldMat=old.GetComponent<MeshRenderer>().sharedMaterial;
 var groundCollider=a.First(t=>t.name=="Cornberg valley").GetComponent<MeshCollider>();
 bool floorHit=groundCollider.Raycast(new Ray(new Vector3(186f,80f,102f),Vector3.down),out var hit,160f);
 Vector3 roadAt=new Vector3(186f,0f,102f);
 return new {active=road.gameObject.activeInHierarchy,enabled=renderer.enabled,
  layer=road.gameObject.layer,position=road.position.ToString("F3"),
  scale=road.lossyScale.ToString("F2"),material=renderer.sharedMaterial?.name,
  shader=renderer.sharedMaterial?.shader?.name,oldMaterial=oldMat.name,
  isSameMaterial=renderer.sharedMaterial==oldMat,vertexCount=vertices.Length,
  normals=mesh.normals.Length,
  topY=normal.y,bounds=renderer.bounds.ToString("F1"),
  firstV0=v0.ToString("F3"),firstV1=v1.ToString("F3"),firstV2=v2.ToString("F3"),
  ground= CornbergLandscape.Ground(v0.x,v0.z).ToString("F3"),
  firstTriangles=string.Join(",",indices.Take(15)),
 forestMaterials=a.Where(t=>t.name.StartsWith("Ironpine conifer ")).Take(3)
 .SelectMany(t=>t.GetComponentsInChildren<MeshRenderer>(true)).Take(12)
 .Select(r=>new {name=r.name,mat=r.sharedMaterial?.name,shader=r.sharedMaterial?.shader?.name,supported=r.sharedMaterial?.shader?.isSupported}).ToArray(),
 floorRaycast=floorHit?hit.point.ToString("F3"):"no ground",
 meadow=groundCollider.bounds.ToString("F1"),
 roadExpectedY=CornbergLandscape.Ground(186f,102f).y+.145f};
 }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
 }
 }
}