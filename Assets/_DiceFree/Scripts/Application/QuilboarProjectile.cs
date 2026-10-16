using DiceFree.Combat;using UnityEngine;
namespace DiceFree.Briarback
{
 public sealed class QuilboarProjectile:MonoBehaviour
 {
  CombatActor target;Vector3 from;float began,duration;Material material;
  public static void Launch(CombatActor source,CombatActor victim,float seconds,bool nature){
   if(victim==null)return;var go=GameObject.CreatePrimitive(nature?PrimitiveType.Sphere:PrimitiveType.Capsule);go.name=nature?"Briar bolt visual":"Bone javelin visual";go.layer=2;
   var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
   go.transform.localScale=nature?Vector3.one*.19f:new Vector3(.06f,.38f,.06f);
   var p=go.AddComponent<QuilboarProjectile>();p.from=source.transform.position+Vector3.up*1.35f;p.target=victim;p.began=Time.time;p.duration=Mathf.Max(.1f,seconds);
   p.material=new Material(Shader.Find("Universal Render Pipeline/Unlit")){color=nature?new Color(.4f,.85f,.16f):new Color(.85f,.73f,.43f)};go.GetComponent<Renderer>().sharedMaterial=p.material;go.transform.position=p.from;
  }
  void Update(){
   if(target==null){Destroy(gameObject);return;}
   float t=(Time.time-began)/duration;var to=target.transform.position+Vector3.up;
   transform.position=Vector3.Lerp(from,to,Mathf.Clamp01(t));var d=to-from;if(d.sqrMagnitude>.001f)transform.rotation=Quaternion.FromToRotation(Vector3.up,d.normalized);
   if(t>=1)Destroy(gameObject);
  }
  void OnDestroy(){if(material!=null)Destroy(material);}
 }
}
