using System;
using System.Linq;
using UnityEditor;
using UnityEngine.AI;
using DiceFree.Core;
using UnityEngine;
using Unity.Pipeline.Commands;
using DiceFree.World;
using DiceFree.Characters;
namespace DiceFree.EditorTools {
 internal static class FogObstructionRuntimeValidation {
 [CliCommand("dicefree.fog.forest-preview",
    "Teleport playtest player to an existing walkable woodland road for visual tree-fade review.")]
 public static object ForestPreview()
 {
   if(!EditorApplication.isPlaying)throw new InvalidOperationException("Play Mode only");
   var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
   if(player==null)throw new InvalidOperationException("No player");
   var motor=player.GetComponent<TraversalMotor>();
   var candidates=new[]{new Vector3(92f,0,34f),new Vector3(82f,0,24f),
     new Vector3(76f,0,19f),new Vector3(95f,0,40f)};
   foreach(var target in candidates)
   {
     if(NavMesh.SamplePosition(target,out var nav,8f,NavMesh.AllAreas) &&
         motor.Teleport(nav.position))
     {
       var camera=UnityEngine.Object.FindFirstObjectByType<ExplorationCamera>();
       if(camera!=null)camera.RecenterForTravel(player.transform);
       return new{success=true,position=player.transform.position.ToString("F1")};
     }
   }
   throw new InvalidOperationException("No safe playable forest position found");
 }

 [CliCommand("dicefree.fog.los.runtime-test",
  "In Play Mode verify house blocks both sight and exploration, plus physical tree collisions.")]
 public static object Test() {
  if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode");
  var fog=UnityEngine.Object.FindFirstObjectByType<WorldFogOfWar>();
  var player=UnityEngine.Object.FindFirstObjectByType<TraversalInput>();
  if(fog==null||player==null)throw new InvalidOperationException("Cornberg fog/player missing");
  var blockers=UnityEngine.Object.FindObjectsByType<CapsuleCollider>(FindObjectsSortMode.None)
      .Where(c=>c.enabled&&c.gameObject.activeInHierarchy&&
        c.name=="Playtest trunk collision").ToArray();
  if(blockers.Length<600)throw new InvalidOperationException(
    "Expected 600+ physical tree trunks but got "+blockers.Length);
  if(fog.BlockingObjectsCount<600)throw new InvalidOperationException(
    "Fog has not indexed solid scenery: "+fog.BlockingObjectsCount);
  var temp=new GameObject("Fog LOS validation test dummy â€” Play Mode only");
  try {
   temp.transform.position=new Vector3(17,0,-22);
   fog.Configure(temp.transform,Camera.main,new Vector2(-70,-60),new Vector2(275,190),24);
   fog.RefreshVisionNow();
   Vector3 near=new Vector3(17,0,-19);
   Vector3 behind=new Vector3(17,0,-4);
   if(!fog.IsVisible(near))throw new InvalidOperationException(
    "Before house should be revealed");
   if(fog.IsVisible(behind))throw new InvalidOperationException(
    "General goods house leaked visibility to opposite side");
   if(fog.IsExplored(behind))throw new InvalidOperationException(
    "Standing behind a house revealed unexplored opposite side");
   bool treeOcclusion=false; string testedTree="";
   foreach(var trunk in blockers.Take(120)) {
    Vector3 center=trunk.bounds.center;
    if(center.x<35||center.x>180||center.z<-35||center.z>135)continue;
    temp.transform.position=new Vector3(center.x-5,0,center.z);
    fog.RefreshVisionNow();
    var destination=new Vector3(center.x+5,0,center.z);
    if(!fog.IsVisible(destination)){
     treeOcclusion=true;testedTree=center.ToString("F1");break;
    }
   }
   if(!treeOcclusion)throw new InvalidOperationException(
    "No solid trunk was found to block the fog of war");
   var first=blockers[0];
   float radius=first.radius*Mathf.Max(Mathf.Abs(first.transform.lossyScale.x),
      Mathf.Abs(first.transform.lossyScale.z));
   if(radius<.77f)throw new InvalidOperationException("Tree trunk still tiny");
   return new{success=true,physicalTreeColliders=blockers.Length,
       indexedOccluders=fog.BlockingObjectsCount,nearHouseVisible=true,
       behindHouseHidden=true,behindHouseNotExplored=true,
       sampleTreeBlocksSight=treeOcclusion,sampleTree=testedTree,
       trunkRadius=radius};
  } finally {
   fog.Configure(player.transform,Camera.main,new Vector2(-70,-60),
      new Vector2(275,190),24);
   UnityEngine.Object.DestroyImmediate(temp);
  }
 }
 }
}