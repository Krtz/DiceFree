using System;using System.Collections;using System.Collections.Generic;using System.Linq;
using DiceFree.Foundation;using DiceFree.Progression;using DiceFree.Skills;using UnityEngine;
using Kind=DiceFree.Progression.MageProgressionRules.Kind;
namespace DiceFree.Combat
{
 [DisallowMultipleComponent,RequireComponent(typeof(CombatActor),typeof(MageSkillProgression),typeof(ActorResourceController))]
 public sealed class MageSkillCaster:MonoBehaviour,IManifestationSessionState,IIncomingDamageRedirector
 {
  [SerializeField] MageCombatTuning tuning;
  readonly Dictionary<Kind,float> readyAt=new();readonly List<string> elements=new();
  readonly Dictionary<string,int> dotVersions=new();
  readonly HashSet<(MageAfflictions target,string id)> ownedEffects=new();
  readonly List<GameObject> sessionVfx=new();
  CombatActor actor;MageSkillProgression skills;ActorResourceController resources;
  float nextDecay,barrierUntil;int barrierRank,serial;
  public MageCombatTuning Tuning=>tuning;
  public string Feedback{get;private set;}="";
  public int WeavingStacks=>elements.Count;
  public string[] WovenElements=>elements.ToArray();
  public float DecayStartsAt=>nextDecay;
  public float BarrierRemaining=>Mathf.Max(0,barrierUntil-Time.time);
  public event Action<Kind> Casted;
  public void Configure(MageCombatTuning value)=>tuning=value;
  void Awake(){actor=GetComponent<CombatActor>();skills=GetComponent<MageSkillProgression>();resources=GetComponent<ActorResourceController>();}
  void OnEnable(){actor??=GetComponent<CombatActor>();if(actor.Health!=null)actor.Health.Died+=ResetForManifestationLoad;}
  void OnDisable(){if(actor?.Health!=null)actor.Health.Died-=ResetForManifestationLoad;ResetForManifestationLoad();}
  void Update(){
   if(!skills.ActiveForCurrentClass){if(elements.Count>0||barrierUntil>0)ResetForManifestationLoad();return;}
   while(elements.Count>0&&Time.time>=nextDecay){elements.RemoveAt(0);nextDecay+=tuning.decayInterval;SyncWeaving();}
  }
  public float CooldownRemaining(Kind kind)=>readyAt.TryGetValue(kind,out float t)?Mathf.Max(0,t-Time.time):0;
  public bool CanPrepare(Kind kind){
   if(!skills.ActiveForCurrentClass||!actor.CanAct||tuning==null)return Fail("Mage skills are not available.");
   if(MageProgressionRules.For(kind).passive||skills.Rank(kind)==0)return Fail("Learn this spell first.");
   if(CooldownRemaining(kind)>0)return Fail("Spell is cooling down.");
   if(!resources.CanSpend(ResourceIds.Mana,MageProgressionRules.Cost(kind,skills.Rank(kind))))return Fail("Not enough Mana.");
   return true;
  }
  bool Fail(string message){Feedback=message;return false;}
  public bool Cast(Kind kind,CombatActor target=null){
   if(kind==Kind.Quicksand)return Fail("Choose a ground target.");
   if(!CanPrepare(kind))return false;
   if(kind!=Kind.MathBarrier&&(!actor.IsHostileTo(target)||Vector3.Distance(transform.position,target.transform.position)>tuning.range+actor.Radius+target.Radius||!actor.HasSightOf(target)))return Fail("Choose a visible hostile target within range.");
   int rank=skills.Rank(kind);
   Commit(kind,rank);
   switch(kind){
    case Kind.EmberBolt:StartCoroutine(Ember(target,rank));break;
    case Kind.FrostSnare:
     Hit(target,kind,rank);if(target.Alive)target.Effects?.ApplyMovementSpeedDebuff(EffectId("frost"),30+rank*4,3+rank*.4f);
     MageSpellVfx.Burst(target.transform.position,Color.cyan);break;
    case Kind.ArcLightning:
     var seen=new HashSet<CombatActor>();var current=target;Vector3 from=transform.position+Vector3.up;
     for(int i=0;i<3+(rank-1)/2&&current!=null;i++){
      seen.Add(current);MageSpellVfx.Line(from,current.transform.position+Vector3.up,new Color(.5f,.7f,1));
      Hit(current,kind,rank,Mathf.Pow(.8f,i));from=current.transform.position+Vector3.up;
      var previous=current;
      current=CombatActor.All.Where(t=>actor.IsHostileTo(t)&&!seen.Contains(t)&&Vector3.Distance(previous.transform.position,t.transform.position)<=tuning.chainRange&&previous.HasSightOf(t)).OrderBy(t=>Vector3.Distance(previous.transform.position,t.transform.position)).ThenBy(t=>t.GetEntityId().ToString()).FirstOrDefault();
     }break;
    case Kind.HalfLife:
     var aff=target.GetComponent<MageAfflictions>()??target.gameObject.AddComponent<MageAfflictions>();
     string id=EffectId("half-life");aff.ApplyDefense(id,rank*5,6+rank*2);ownedEffects.Add((aff,id));
     int version=++serial;dotVersions[target.GetEntityId().ToString()]=version;StartCoroutine(HalfLife(target,rank,version));break;
    case Kind.MathBarrier:
     barrierRank=rank;barrierUntil=Time.time+MageProgressionRules.BarrierDuration(rank);
     sessionVfx.Add(MageSpellVfx.Barrier(transform,MageProgressionRules.BarrierDuration(rank)));break;
   }
   return true;
  }
  public bool CastQuicksand(Vector3 point){
   if(!CanPrepare(Kind.Quicksand))return false;
   if(!float.IsFinite(point.x)||!float.IsFinite(point.y)||!float.IsFinite(point.z)||Vector3.Distance(transform.position,point)>tuning.range+actor.Radius)return Fail("Choose ground within range.");
   var ray=point+Vector3.up*.2f-(transform.position+Vector3.up);
   if(Physics.Raycast(transform.position+Vector3.up,ray.normalized,ray.magnitude,1<<9,QueryTriggerInteraction.Ignore))return Fail("The ground is obstructed.");
   int rank=skills.Rank(Kind.Quicksand);Commit(Kind.Quicksand,rank);StartCoroutine(Quicksand(point,rank));return true;
  }
  void Commit(Kind kind,int rank){
   
   GetComponent<BasicAttack>()?.Cancel();actor.Motor.Stop();
   if(!resources.TrySpend(ResourceIds.Mana,MageProgressionRules.Cost(kind,rank)))throw new InvalidOperationException("Mage cast resource validation changed.");
   readyAt[kind]=Time.time+MageProgressionRules.Cooldown(kind);
   if(skills.Rank(Kind.ElementalWeaving)>0){
    string element=tuning.attacks[(int)kind]?.element?.stableId;
    if(!string.IsNullOrWhiteSpace(element)&&!elements.Contains(element))elements.Add(element);
    nextDecay=Time.time+tuning.decayDelay;SyncWeaving();
    foreach(var other in readyAt.Keys.ToArray())if(other!=kind)readyAt[other]=Mathf.Max(Time.time,readyAt[other]-.2f);
   }
   Feedback=MageProgressionRules.Name(kind)+" cast.";Casted?.Invoke(kind);
  }
  void SyncWeaving()=>actor.Stats.SetBasicAttackDamagePercentContribution("mage.weaving",WeavingStacks*5);
  string EffectId(string name)=>"mage."+name+":"+actor.GetEntityId();
  void Hit(CombatActor target,Kind kind,int rank,float falloff=1){
   if(!actor.IsHostileTo(target))return;
   var attack=tuning.attacks[(int)kind];int weaving=skills.Rank(Kind.ElementalWeaving);
   if(weaving>0&&attack.element!=null){
    var aff=target.GetComponent<MageAfflictions>()??target.gameObject.AddComponent<MageAfflictions>();
    string id=EffectId("resistance."+attack.element.stableId);
    aff.ApplyResistance(id,attack.element,weaving*.01f,8);ownedEffects.Add((aff,id));
   }
   DamageResolver.Hit(actor,target,attack,coefficientOverride:MageProgressionRules.Coefficient(kind,rank),rawMultiplier:(1+WeavingStacks*.05f)*falloff);
  }
  IEnumerator Ember(CombatActor target,int rank){
   Vector3 from=transform.position+Vector3.up*1.2f;float delay=Mathf.Clamp(Vector3.Distance(transform.position,target.transform.position)/16,.1f,.7f);
   MageSpellVfx.Bolt(from,target,delay,new Color(1,.32f,.06f));yield return new WaitForSeconds(delay);
   if(actor.Alive&&skills.ActiveForCurrentClass&&target!=null)Hit(target,Kind.EmberBolt,rank);
  }
  IEnumerator HalfLife(CombatActor target,int rank,int version){
   for(int i=0;i<6+rank*2;i++){
    yield return new WaitForSeconds(1);
    if(!actor.Alive||!skills.ActiveForCurrentClass||target==null||!target.Alive||!dotVersions.TryGetValue(target.GetEntityId().ToString(),out int active)||active!=version)yield break;
    Hit(target,Kind.HalfLife,rank);MageSpellVfx.Burst(target.transform.position,new Color(.45f,1,.12f),.25f);
   }
  }
  IEnumerator Quicksand(Vector3 point,int rank){
   sessionVfx.Add(MageSpellVfx.Ground(point,tuning.quicksandRadius,4+rank));
   for(int i=0;i<4+rank;i++){
    if(!actor.Alive||!skills.ActiveForCurrentClass)yield break;
    foreach(var t in CombatActor.All.Where(t=>actor.IsHostileTo(t)&&Vector2.Distance(new Vector2(t.transform.position.x,t.transform.position.z),new Vector2(point.x,point.z))<=tuning.quicksandRadius+t.Radius).ToArray()){
     Hit(t,Kind.Quicksand,rank);if(t.Alive)t.Effects?.ApplyMovementSpeedDebuff(EffectId("quicksand"),40+rank*3,1.2f);
    }yield return new WaitForSeconds(1);
   }
  }
  public float RedirectDamage(float incoming){
   if(!isActiveAndEnabled||!skills.ActiveForCurrentClass||Time.time>=barrierUntil||incoming<=0)return incoming;
   float efficiency=MageProgressionRules.ManaPerDamage(barrierRank);
   float absorbed=Mathf.Min(incoming*MageProgressionRules.BarrierFraction(barrierRank),resources.Current(ResourceIds.Mana)/efficiency);
   return absorbed>0&&resources.TrySpend(ResourceIds.Mana,absorbed*efficiency)?incoming-absorbed:incoming;
  }
  public void ResetForManifestationLoad(){
   StopAllCoroutines();readyAt.Clear();elements.Clear();dotVersions.Clear();barrierUntil=0;barrierRank=0;
   if(actor!=null)actor.Stats.RemoveBasicAttackDamagePercentContribution("mage.weaving");
   foreach(var entry in ownedEffects)if(entry.target!=null)entry.target.Remove(entry.id);ownedEffects.Clear();foreach(var v in sessionVfx)if(v!=null)Destroy(v);sessionVfx.Clear();Feedback="";
  }
 }
}
