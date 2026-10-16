using System.Collections;using UnityEngine;
namespace DiceFree.Combat
{
 public sealed class MageSpellVfx:MonoBehaviour
 {
  Material owned;CombatActor target;Vector3 from;float began,duration;
  void OnDestroy(){if(owned!=null)Destroy(owned);}
  static GameObject Shape(string name,PrimitiveType type,Vector3 point,Vector3 size,Color color,float seconds){
   var go=GameObject.CreatePrimitive(type);go.name=name;go.layer=2;go.transform.position=point;go.transform.localScale=size;
   var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
   var life=go.AddComponent<MageSpellVfx>();life.owned=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color};
   life.owned.EnableKeyword("_EMISSION");life.owned.SetColor("_EmissionColor",color*.4f);go.GetComponent<Renderer>().sharedMaterial=life.owned;
   Destroy(go,seconds);return go;
  }
  public static void Burst(Vector3 point,Color color,float scale=.6f)=>Shape("Mage elemental impact",PrimitiveType.Sphere,point+Vector3.up*.9f,Vector3.one*scale,color,.22f);
  public static void Bolt(Vector3 point,CombatActor target,float seconds,Color color){
   var go=Shape("Ember Bolt projectile",PrimitiveType.Sphere,point,Vector3.one*.22f,color,seconds+.05f);
   var life=go.GetComponent<MageSpellVfx>();life.target=target;life.from=point;life.began=Time.time;life.duration=seconds;
  }
  void Update(){if(target!=null)transform.position=Vector3.Lerp(from,target.transform.position+Vector3.up,Mathf.Clamp01((Time.time-began)/duration));}
  public static void Line(Vector3 a,Vector3 b,Color color){
   var go=Shape("Arc Lightning",PrimitiveType.Cube,(a+b)*.5f,new Vector3(.055f,.055f,Vector3.Distance(a,b)),color,.22f);
   go.transform.rotation=Quaternion.LookRotation(b-a);
  }
  public static GameObject Ground(Vector3 point,float radius,float seconds)=>Shape("Quicksand field",PrimitiveType.Cylinder,point+Vector3.up*.035f,new Vector3(radius*2,.025f,radius*2),new Color(.46f,.31f,.10f),seconds);
  public static GameObject Barrier(Transform owner,float seconds){
   var parent=new GameObject("Math Barrier geometry");parent.transform.SetParent(owner,false);parent.transform.localPosition=Vector3.up;
   for(int i=0;i<12;i++){
    float angle=i*Mathf.PI/6;var go=Shape("Math Barrier rune",PrimitiveType.Cube,Vector3.zero,new Vector3(.08f,.08f,.08f),new Color(.7f,.3f,1),seconds);
    go.transform.SetParent(parent.transform,false);go.transform.localPosition=new Vector3(Mathf.Cos(angle)*.7f,Mathf.Sin(angle)*.7f,0);
   }
   Destroy(parent,seconds);return parent;
  }
 }
}
