using System;
using System.IO;
using System.Linq;
using DiceFree.EditorTools;
using DiceFree.Visuals;
using DiceFree.World;
using UnityEditor;
using UnityEngine;
public static class ArboriumPolishFinalChecks
{
 public static object Run()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required.");
  ArchitectureValidation.Run();ArchitectureValidation.RunPolicySelfTests();WalkingVillagerValidation.Validate();
  var root=GameObject.Find("Arborium arrival district - polish");if(root==null||root.GetComponentsInChildren<ArboriumAmbientRoute>().Length!=1||root.GetComponentsInChildren<VillageHomeWander>().Length!=8)throw new InvalidOperationException("City-life composition mismatch.");
  if(root.GetComponentsInChildren<Renderer>().Any(r=>r.sharedMaterials.Any(m=>m==null))||root.GetComponentsInChildren<MeshFilter>().Any(f=>f.sharedMesh==null))throw new InvalidOperationException("Missing art reference.");
  foreach(var path in Directory.GetFiles("Assets/_DiceFree/Art/World/ArboriumPolish","*.wav")){var bytes=File.ReadAllBytes(path);if(bytes.Length<44||System.Text.Encoding.ASCII.GetString(bytes,0,4)!="RIFF"||BitConverter.ToInt32(bytes,40)!=bytes.Length-44)throw new InvalidOperationException("Invalid WAV: "+path);}
  var result=new{architecture=true,managedReferences=true,dependencyPolicy=true,originalFourVillagerRigs=true,newCityCitizens=8,convoys=1,meshMaterialReferences=true,wavHeaders=true};File.WriteAllText("Logs/ArboriumPolishFinalChecks.json",Newtonsoft.Json.JsonConvert.SerializeObject(result,Newtonsoft.Json.Formatting.Indented));return result;
 }
}
