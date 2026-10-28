using System.Linq;
using UnityEngine;
using DiceFree.World;
public static class DiceAnchorInspect {
 public static object Run() { return Object.FindObjectsByType<ResurrectionAnchor>().Select(a=>new {id=a.StableId,path=Path(a.transform),pos=a.transform.position.ToString(),activation=a.ActivationPosition.ToString(),siblings=Object.FindObjectsByType<Transform>().Where(t=>Vector3.Distance(t.position,a.ActivationPosition)<7).ToArray().Select(t=>new{name=t.name,pos=t.position.ToString(),scale=t.lossyScale.ToString(),renderer=t.GetComponent<Renderer>()!=null}).ToArray()}).ToArray(); }
 static string Path(Transform t)=>t.parent==null?t.name:Path(t.parent)+"/"+t.name;
}
