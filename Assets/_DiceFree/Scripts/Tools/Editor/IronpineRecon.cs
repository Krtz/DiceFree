using System;
using System.Linq;
using Unity.Pipeline.Commands;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using DiceFree.World;
namespace DiceFree.EditorTools
{
 internal static class IronpineRecon
 {
  [CliCommand("dicefree.ironpine.recon","Survey forest assets, road mesh, terrain and north-east walkable route.")]
  public static object Recon()
  {
   if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit mode");
   var setup=EditorSceneManager.GetSceneManagerSetup();
   try {
    var scene=EditorSceneManager.OpenScene("Assets/_DiceFree/Scenes/Cornberg.unity",OpenSceneMode.Single);
    var all=scene.GetRootGameObjects().SelectMany(o=>o.GetComponentsInChildren<Transform>(true)).ToArray();
    var nav=all.Select(t=>t.GetComponent<WorldNavigation>()).FirstOrDefault(x=>x!=null);
    var navInstance=NavMesh.AddNavMeshData(nav.Data);
    try{
     Vector3[] coords={new(112,0,43),new(125,0,43),new(145,0,44),new(175,0,45),new(175,0,68),new(183,0,100),new(199,0,119),new(217,0,142),new(227,0,154)};
     var tests=coords.Select(p=>{
      var sample=NavMesh.SamplePosition(p,out var hit,8,NavMesh.AllAreas);
      var path=new NavMeshPath();
      bool complete=sample&&NavMesh.SamplePosition(coords[0],out var origin,8,NavMesh.AllAreas)&&
       NavMesh.CalculatePath(origin.position,hit.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;
      return new{p=p.ToString("F1"),nav=sample,hit=sample?hit.position.ToString("F1"):"-",route=complete};
     }).ToArray();
     var treeSamples=all.Where(t=>t.gameObject.activeInHierarchy &&
       (t.name.IndexOf("Pine",StringComparison.OrdinalIgnoreCase)>=0 ||
       t.name.Contains("Trees/Tree_")))
      .Where(t=>t.GetComponentsInChildren<MeshRenderer>().Length>=2)
      .GroupBy(t=>t.name).Select(g=>g.First()).Take(26)
      .Select(t=>new{name=t.name,parent=t.parent?.name,position=t.position.ToString("F1"),
       collisions=t.GetComponentsInChildren<Collider>().Length,
       obstacles=t.GetComponentsInChildren<NavMeshObstacle>().Length,
       scale=t.lossyScale.ToString("F1")}).ToArray();
     var landmarks=all.Where(t=>t.name.IndexOf("road",StringComparison.OrdinalIgnoreCase)>=0||
       t.name.IndexOf("trail",StringComparison.OrdinalIgnoreCase)>=0||
       t.name.IndexOf("pine",StringComparison.OrdinalIgnoreCase)>=0)
      .Where(t=>t.gameObject.activeInHierarchy&&t.GetComponent<MeshRenderer>()!=null&&t.position.x>110)
      .Take(35).Select(t=>new{name=t.name,pos=t.position.ToString("F1"),
       material=t.GetComponent<MeshRenderer>().sharedMaterial?.name}).ToArray();
     var oldroad=all.First(t=>t.name=="Mountain to east road");
     var mesh=oldroad.GetComponent<MeshFilter>().sharedMesh;
     var models=AssetDatabase.FindAssets("t:Model",new[]{"Assets/_DiceFree/Art/World/VisualBatch01/Models"})
       .Select(AssetDatabase.GUIDToAssetPath).Where(s=>s.Contains("pine_"))
       .Select(s=>new{file=s,model=AssetDatabase.LoadAssetAtPath<GameObject>(s)?.name}).ToArray();
     return new {tests,treeSamples,landmarks,roadMaterial=oldroad.GetComponent<Renderer>().sharedMaterial.name,
        roadVertices=mesh.vertexCount,roadUV=mesh.uv.Length,models};
    }finally{navInstance.Remove();}
   }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
  }
 }
}