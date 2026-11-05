using System.Linq;
using DiceFree.World;
using UnityEngine;
using UnityEngine.AI;
public static class NovemberFeedbackInspect
{
 public static string Tree()
 {
  var t=Object.FindObjectsByType<DiceFree.Core.TreeCameraOccluder>().First(x=>x.GetComponentsInChildren<Renderer>().Any(r=>r.sharedMaterial!=null&&r.sharedMaterial.HasProperty("_BaseColor")));var r=t.GetComponentsInChildren<Renderer>().First(x=>x.sharedMaterial!=null&&x.sharedMaterial.HasProperty("_BaseColor"));var focus=t.transform.position+Vector3.up;var center=r.bounds.center;
  return Newtonsoft.Json.JsonConvert.SerializeObject(new{tree=t.name,root=t.transform.position.ToString(),mesh=r.name,bounds=r.bounds.ToString(),focus=focus.ToString(),center=center.ToString(),r.enabled,material=r.sharedMaterial.name,alpha=r.sharedMaterial.color.a,cached=typeof(DiceFree.Core.TreeCameraOccluder).GetProperty("Renderers",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(t) is Renderer[] rr?rr.Length:-1});
 }
 public static string Run()=>Newtonsoft.Json.JsonConvert.SerializeObject(new{
  anchors=Object.FindObjectsByType<ResurrectionAnchor>(FindObjectsInactive.Include).Select(a=>new{a.StableId,root=a.transform.position.ToString(),stone=a.ActivationPosition.ToString(),a.ActivationRadius,rootNavigable=NavMesh.SamplePosition(a.transform.position,out var hit,2,NavMesh.AllAreas)}),
  surfaces=Object.FindObjectsByType<TravelSurface>(FindObjectsInactive.Include).Select(s=>new{s.name,s.StableId}),
  dice=Object.FindObjectsByType<ResurrectionDiceVisual>(FindObjectsInactive.Include).Select(d=>new{anchor=d.Anchor?.StableId,d.Glowing,visual=d.transform.position.ToString(),pip=d.WorldPip?.bounds.center.ToString()}),
  roads=Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include).Where(m=>m.name.ToLowerInvariant().Contains("road")||m.name.ToLowerInvariant().Contains("trail")).Select(m=>new{m.name,path=Path(m.transform),mesh=m.sharedMesh?.name})
 });
 static string Path(Transform t)=>t.parent!=null?Path(t.parent)+"/"+t.name:t.name;
}
