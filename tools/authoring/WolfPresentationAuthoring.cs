using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiceFree.Dungeons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
public static class WolfPresentationAuthoring
{
 const string Folder="Assets/_DiceFree/Settings/WolfSurvival";
 const string Art="Assets/_DiceFree/Art/World/WolfPresentation";
 public static object Author()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().isDirty||AssetDatabase.IsValidFolder(Art))throw new InvalidOperationException("Clean Edit Mode and unauthored presentation required");
  var previous=SceneManager.GetActiveScene();var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/WolfSurvival.unity",OpenSceneMode.Additive);
  try
  {
   if(!EditorSceneManager.SaveScene(scene,"Assets/_DiceFree/Settings/Recovery/WolfSurvival-before-presentation-20261105.unity",true)||!AssetDatabase.CopyAsset(Folder+"/Wolf Survival NavMesh.asset","Assets/_DiceFree/Settings/Recovery/WolfSurvivalNav-before-presentation-20261105.asset"))throw new InvalidOperationException("Recovery failed");
   SceneManager.SetActiveScene(scene);AssetDatabase.CreateFolder("Assets/_DiceFree/Art/World","WolfPresentation");
   var root=scene.GetRootGameObjects().Single(g=>g.name=="Moonlit Clearing - Wolf Survival").transform;
   const int size=2048;var texture=new Texture2D(size,size,TextureFormat.RGB24,true);var pixels=new Color[size*size];
   var trails=new[]{new Vector2(-23,9),new Vector2(23,9),new Vector2(0,25)};
   float Segment(Vector2 p,Vector2 a,Vector2 b){var d=b-a;return Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude));}
   for(int y=0;y<size;y++)for(int x=0;x<size;x++)
   {
    var p=new Vector2(x/(size-1f)*144-72,y/(size-1f)*144-72);float broad=Mathf.PerlinNoise(p.x*.075f+37,p.y*.075f+29),grain=Mathf.PerlinNoise(p.x*3.4f+61,p.y*3.4f+12);
    float edge=20+broad*5+Mathf.Sin(Mathf.Atan2(p.y,p.x)*7)*1.2f;float clearing=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(edge-4,edge+3,p.magnitude));float trailDistance=100;
    foreach(var pad in trails){var outer=pad.normalized*39;var bend=pad+new Vector2(-pad.y,pad.x).normalized*1.8f;trailDistance=Mathf.Min(trailDistance,Mathf.Min(Segment(p,outer,bend),Segment(p,bend,pad.normalized*10)));}
    float worn=Mathf.Max(clearing,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.7f+broad,3.4f+broad,trailDistance)));
    var grass=Color.Lerp(new Color(.16f,.23f,.115f),new Color(.34f,.38f,.21f),broad*.65f+grain*.35f);var earth=Color.Lerp(new Color(.24f,.185f,.125f),new Color(.45f,.36f,.24f),grain*.5f+broad*.5f);
    var color=Color.Lerp(grass,earth,worn);float leaf=Mathf.PerlinNoise(p.x*11+90,p.y*11+6);if(leaf>.76f)color=Color.Lerp(color,new Color(.39f,.31f,.145f),.45f);pixels[y*size+x]=color;
   }
   texture.SetPixels(pixels);texture.Apply();string png=Art+"/Moonlit clearing ground.png";File.WriteAllBytes(png,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(png,ImportAssetOptions.ForceSynchronousImport);
   var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Forest floor - soft earth and moss",color=Color.white,mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(png)};mat.SetFloat("_Smoothness",.05f);AssetDatabase.CreateAsset(mat,Art+"/Forest floor.mat");
   int grounds=0,hidden=0;
   foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
   {
    string asset=AssetDatabase.GetAssetPath(filter.sharedMesh);
    if(asset==Folder+"/Continuous clearing floor.asset"||asset==Folder+"/Forest beyond the clearing.asset")
    {var mesh=filter.sharedMesh;mesh.uv=mesh.vertices.Select(v=>new Vector2((v.x+72)/144,(v.z+72)/144)).ToArray();mesh.SetPreBakeCollisionMesh(false,true);EditorUtility.SetDirty(mesh);filter.GetComponent<Renderer>().sharedMaterial=mat;grounds++;}
    if(filter.name=="Central trampled clearing"||filter.name.StartsWith("Pack entry trail")){filter.GetComponent<Renderer>().enabled=false;hidden++;}
   }
   if(grounds!=2)throw new InvalidOperationException("Expected two owned ground meshes");
   int moved=0;var trunks=root.GetComponentsInChildren<Collider>().Where(c=>c.name=="Boundary tree trunk").ToArray();
   foreach(Transform tree in root)
   {
    if(!tree.name.StartsWith("Tree_Pine")&&!tree.name.StartsWith("Tree_Spruce"))continue;
    var p=tree.localPosition;var xz=new Vector2(p.x,p.z);if(xz.magnitude>39)continue;
    foreach(var pad in trails)
    {
     var direction=pad.normalized;var side=new Vector2(-direction.y,direction.x);float along=Vector2.Dot(xz,direction),across=Vector2.Dot(xz,side);if(along<26||Mathf.Abs(across)>3.5f)continue;
     var delta=side*((across<0?-1:1)*5-across);var shift=new Vector3(delta.x,0,delta.y);
     var collider=trunks.FirstOrDefault(c=>Vector3.Distance(c.transform.localPosition,p)<.1f);tree.localPosition+=shift;if(collider!=null)collider.transform.localPosition+=shift;moved++;break;
    }
   }
   Physics.SyncTransforms();var bounds=new Bounds(WolfDungeonRun.Origin+Vector3.up*4,new Vector3(85,25,85));var sources=new List<NavMeshBuildSource>();var markups=root.GetComponentsInChildren<Collider>().Where(c=>c.enabled&&c.gameObject.layer==9).Select(c=>new NavMeshBuildMarkup{root=c.transform,overrideArea=true,area=1}).ToList();
   NavMeshBuilder.CollectSources(bounds,(1<<8)|(1<<9),NavMeshCollectGeometry.PhysicsColliders,0,markups,sources);var settings=NavMesh.GetSettingsByID(0);settings.agentRadius=.75f;settings.agentHeight=2.2f;settings.agentClimb=.4f;settings.agentSlope=40;settings.overrideVoxelSize=true;settings.voxelSize=.16f;
   var data=NavMeshBuilder.BuildNavMeshData(settings,sources,bounds,Vector3.zero,Quaternion.identity);if(data==null)throw new InvalidOperationException("Trail navigation bake failed");var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(Folder+"/Wolf Survival NavMesh.asset");EditorUtility.CopySerialized(data,saved);Object.DestroyImmediate(data);EditorUtility.SetDirty(saved);
   FinishBackdrop();AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Scene save failed");
   var result=new{groundTexture=size,grounds,hidden,movedTrees=moved};File.WriteAllText("Logs/WolfPresentationAuthoring.json",Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }
  finally{EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(previous);}
 }
 public static object FinishBackdrop()
 {
  if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode required");
  var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/Forest beyond the clearing.asset");if(mesh.bounds.extents.x<70){mesh.vertices=mesh.vertices.Select(v=>v*1.6f).ToArray();mesh.uv=mesh.vertices.Select(v=>new Vector2((v.x+72)/144,(v.z+72)/144)).ToArray();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);}
  var importer=(TextureImporter)AssetImporter.GetAtPath(Art+"/Moonlit clearing ground.png");if(importer.wrapMode!=TextureWrapMode.Clamp){importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();}AssetDatabase.SaveAssets();return new{backdropRadius=mesh.bounds.extents.x,wrap="Clamp"};
 }
}
