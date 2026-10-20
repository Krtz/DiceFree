using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace DiceFree.Combat
{
 /// <summary>Decorative Mage effects. Gameplay timing/targets remain owned by MageSkillCaster.</summary>
 public sealed class MageSpellVfx:MonoBehaviour
 {
  static readonly Dictionary<string,GameObject> prefabs=new();
  static Material lines;
  public static readonly string[] ResourceKeys={"EmberProjectile","EmberImpact","FrostImpact","FrostCircle","LightningImpact","SandBurst","SandLoop","MathShield","RadiationAura","RadiationImpact"};
  Transform follow;CombatActor watched;Vector3 offset,from,last;float began,duration,spin;bool projectile,tracked;
  LineRenderer[] flicker;Vector3[] lightningPoints;Color lightningColor;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void ResetCache(){prefabs.Clear();if(lines!=null)Destroy(lines);lines=null;}
  static Material LineMaterial(){
   if(lines!=null)return lines;
   lines=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")){name="DiceFree Mage luminous lines"};
   lines.SetColor("_BaseColor",Color.white);lines.SetTexture("_BaseMap",Texture2D.whiteTexture);
   lines.SetFloat("_Surface",1);lines.SetFloat("_Blend",2);lines.SetFloat("_ZWrite",0);lines.SetFloat("_Cull",0);
   lines.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);lines.SetFloat("_DstBlend",(float)BlendMode.One);
   lines.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");lines.SetOverrideTag("RenderType","Transparent");lines.renderQueue=3000;return lines;
  }
  static GameObject Root(string name,Vector3 point,float seconds){
   var root=new GameObject("Mage VFX - "+name);root.layer=2;root.transform.position=point;
   var life=root.AddComponent<MageSpellVfx>();life.began=Time.time;life.duration=Mathf.Max(.03f,seconds);
   Destroy(root,life.duration);return root;
  }
  static GameObject Spawn(string key,Vector3 point,float seconds,float scale,Color tint,bool loop=false){
   var root=Root(key,point,seconds);
   if(!prefabs.TryGetValue(key,out var prefab)){prefab=Resources.Load<GameObject>("MageVfxLicensed/"+key);prefabs[key]=prefab;}
   if(prefab!=null){
    var visual=Instantiate(prefab,root.transform,false);visual.name=key+" licensed particles";visual.transform.localScale*=scale;
    foreach(var t in visual.GetComponentsInChildren<Transform>(true))t.gameObject.layer=2;
    foreach(var ps in visual.GetComponentsInChildren<ParticleSystem>(true)){
     var main=ps.main;main.loop=loop;main.stopAction=ParticleSystemStopAction.None;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
     var color=main.startColor;
     if(color.mode==ParticleSystemGradientMode.Color)main.startColor=color.color*tint;
     ps.Play(false);
    }
   }else Fallback(root.transform,tint,Mathf.Max(.25f,scale),loop);
   return root;
  }
  static void Fallback(Transform parent,Color color,float scale,bool loop){
   var go=new GameObject("Portable elemental sparks");go.layer=2;go.transform.SetParent(parent,false);
   var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var main=ps.main;main.duration=1;main.loop=loop;main.startLifetime=.6f;main.startSpeed=.8f*scale;main.startSize=.13f;main.startColor=color;main.maxParticles=80;main.scalingMode=ParticleSystemScalingMode.Hierarchy;
   var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=scale*.3f;
   var emission=ps.emission;emission.rateOverTime=loop?22:0;if(!loop)emission.SetBursts(new[]{new ParticleSystem.Burst(0,32)});
   var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Resources.Load<Material>("EarlySkillVfx/SoftSparks")??LineMaterial();renderer.shadowCastingMode=ShadowCastingMode.Off;ps.Play();
  }
  static LineRenderer Stroke(Transform parent,string name,Vector3[] points,Color color,float width,bool closed=false,bool world=false){
   var go=new GameObject(name);go.layer=2;go.transform.SetParent(parent,false);var line=go.AddComponent<LineRenderer>();
   line.sharedMaterial=LineMaterial();line.useWorldSpace=world;line.loop=closed;line.positionCount=points.Length;line.SetPositions(points);
   line.startWidth=line.endWidth=width;line.startColor=line.endColor=color;line.numCapVertices=3;line.numCornerVertices=2;
   line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;return line;
  }
  static LineRenderer Ring(Transform parent,string name,float radius,Color color,float width,Quaternion rotation){
   var points=new Vector3[64];
   for(int i=0;i<points.Length;i++){float angle=i*Mathf.PI*2/points.Length;points[i]=rotation*new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius);}
   return Stroke(parent,name,points,color,width,true);
  }
  public static GameObject Burst(Vector3 point,Color color,float scale=.6f)=>Spawn("RadiationImpact",point+Vector3.up*.8f,.65f,scale,color);
  public static GameObject Bolt(Vector3 point,CombatActor target,float seconds,Color color){
   var root=Spawn("EmberProjectile",point,seconds,.38f,Color.white);
   var life=root.GetComponent<MageSpellVfx>();life.projectile=true;life.watched=target;life.from=point;life.last=target!=null?target.transform.position+Vector3.up:point;
   root.transform.rotation=Quaternion.LookRotation((life.last-point).sqrMagnitude>.001f?life.last-point:Vector3.forward);
   foreach(var ps in root.GetComponentsInChildren<ParticleSystem>()){var main=ps.main;main.loop=true;main.simulationSpace=ParticleSystemSimulationSpace.World;}
   return root;
  }
  public static GameObject EmberImpact(Vector3 point)=>Spawn("EmberImpact",point+Vector3.up*.8f,.85f,.42f,Color.white);
  public static GameObject Frost(Vector3 point){
   var root=Spawn("FrostImpact",point+Vector3.up*.4f,1.1f,.65f,Color.white);
   var circle=Spawn("FrostCircle",point+Vector3.up*.06f,.9f,.48f,Color.white);circle.transform.SetParent(root.transform,true);
   Ring(root.transform,"Ice shock ring",.8f,new Color(.3f,.85f,1,.8f),.045f,Quaternion.identity).transform.localPosition=Vector3.down*.33f;
   return root;
  }
  public static GameObject Line(Vector3 a,Vector3 b,Color color){
   var root=Root("Arc Lightning",Vector3.zero,.32f);var life=root.GetComponent<MageSpellVfx>();
   int count=Mathf.Clamp(Mathf.CeilToInt(Vector3.Distance(a,b)/.4f),5,48);
   var points=new Vector3[count];var direction=(b-a).normalized;var side=Vector3.Cross(direction,Vector3.up).normalized;if(side.sqrMagnitude<.01f)side=Vector3.right;
   for(int i=0;i<count;i++){float t=i/(float)(count-1);float envelope=Mathf.Sin(t*Mathf.PI);points[i]=Vector3.Lerp(a,b,t)+side*Mathf.Sin(i*5.3f)*.22f*envelope+Vector3.up*Mathf.Cos(i*3.7f)*.14f*envelope;}
   life.lightningPoints=points;life.lightningColor=color;
   life.flicker=new[]{Stroke(root.transform,"Electric blue glow",points,new Color(color.r,color.g,color.b,.38f),.20f,false,true),
    Stroke(root.transform,"White-hot lightning core",points,new Color(.85f,.94f,1,1),.045f,false,true)};
   for(int n=0;n<2;n++){
    int start=count/3+n*count/4;var branch=new[]{points[start],points[start]+side*(n==0?-.28f:.28f)+Vector3.up*.18f,points[Mathf.Min(start+2,count-1)]};
    Stroke(root.transform,"Forked arc",branch,new Color(.55f,.8f,1,.75f),.025f,false,true);
   }
   var impact=Spawn("LightningImpact",b,.7f,.5f,Color.white);impact.transform.SetParent(root.transform,true);
   return root;
  }
  public static GameObject Ground(Vector3 point,float radius,float seconds){
   var root=Root("Quicksand field",point+Vector3.up*.055f,seconds);
   var burst=Spawn("SandBurst",root.transform.position,.8f,radius*.30f,Color.white);burst.transform.SetParent(root.transform,true);
   var dust=Spawn("SandLoop",root.transform.position,seconds,radius*.30f,new Color(1,.85f,.62f,.55f),true);dust.transform.SetParent(root.transform,true);
   Ring(root.transform,"True Quicksand radius",radius,new Color(.8f,.56f,.21f,.65f),.045f,Quaternion.identity);
   Ring(root.transform,"Swirling inner sand",radius*.68f,new Color(.95f,.72f,.30f,.45f),.04f,Quaternion.Euler(0,12,0));
   root.GetComponent<MageSpellVfx>().spin=12;return root;
  }
  public static GameObject Radiation(CombatActor target,float seconds){
   var root=Spawn("RadiationAura",target.transform.position+Vector3.up*.5f,seconds,.6f,new Color(.65f,1,.35f,.8f),true);
   root.transform.SetParent(target.transform,true);
   var life=root.GetComponent<MageSpellVfx>();life.follow=target.transform;life.watched=target;life.tracked=true;life.offset=Vector3.up*.5f;
   Ring(root.transform,"Radiation ground halo",.62f,new Color(.34f,1f,.12f,.95f),.055f,Quaternion.identity).transform.localPosition=Vector3.down*.42f;
   Ring(root.transform,"Radiation vertical orbit",.62f,new Color(.55f,1f,.18f,.9f),.045f,Quaternion.Euler(90,0,0));
   Ring(root.transform,"Radiation cross orbit",.62f,new Color(.26f,.9f,.1f,.78f),.035f,Quaternion.Euler(0,0,90));
   life.spin=25;return root;
  }
  public static GameObject RadiationTick(Vector3 point)=>Spawn("RadiationImpact",point+Vector3.up*.9f,.55f,.32f,new Color(.7f,1,.3f,.7f));
  public static GameObject Barrier(Transform owner,float seconds){
   var root=Spawn("MathShield",owner.position+Vector3.up,seconds,.72f,new Color(.8f,.65f,1,.65f),true);
   root.transform.SetParent(owner,true);
   var life=root.GetComponent<MageSpellVfx>();life.follow=owner;life.watched=owner.GetComponent<CombatActor>();life.tracked=true;life.offset=Vector3.up;life.spin=20;
   foreach(var rotation in new[]{Quaternion.identity,Quaternion.Euler(90,0,0),Quaternion.Euler(0,0,90)})
    Ring(root.transform,"Math Barrier orbit",.95f,new Color(.68f,.42f,1,.6f),.025f,rotation);
   for(int i=0;i<8;i++){
    float a=i*Mathf.PI/4;var center=new Vector3(Mathf.Cos(a),.05f,Mathf.Sin(a))*.95f;
    Stroke(root.transform,"Geometric rune",new[]{center+Vector3.up*.12f,center+Vector3.right*.09f,center-Vector3.up*.12f,center-Vector3.right*.09f},new Color(.85f,.7f,1,.8f),.025f,true);
   }
   return root;
  }
  void Update(){
   if(projectile){if(watched!=null)last=watched.transform.position+Vector3.up;transform.position=Vector3.Lerp(from,last,Mathf.Clamp01((Time.time-began)/duration));}
   else if(tracked){if(follow==null||watched==null||!watched.Alive){Destroy(gameObject);return;}transform.position=follow.position+offset;}
   if(spin!=0)transform.Rotate(Vector3.up,spin*Time.deltaTime,Space.World);
   if(flicker!=null){
    float fade=Mathf.Clamp01(1-(Time.time-began)/duration);float pulse=.65f+.35f*Mathf.Sin((Time.time-began)*120);
    for(int i=0;i<flicker.Length;i++){
     var line=flicker[i];if(line==null)continue;
     line.startColor=line.endColor=i==0?new Color(lightningColor.r,lightningColor.g,lightningColor.b,fade*.38f):new Color(.85f,.94f,1,fade*pulse);
    }
   }
  }
 }
}
