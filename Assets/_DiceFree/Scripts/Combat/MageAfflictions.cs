using System;using System.Linq;using System.Collections.Generic;using UnityEngine;
namespace DiceFree.Combat
{
 [DisallowMultipleComponent] public sealed class MageAfflictions:MonoBehaviour
 {
  sealed class Entry {public string id;public float until;public bool defense;}
  readonly Dictionary<string,Entry> effects=new(StringComparer.Ordinal);
  ActorStats stats;void Awake()=>stats=GetComponent<ActorStats>();
  public void ApplyDefense(string id,float amount,float seconds){
   stats.SetDefenseModifier(id+":physical",new DefenseModifier{channel=DamageChannel.Physical,reductionFlat=amount});
   stats.SetDefenseModifier(id+":magical",new DefenseModifier{channel=DamageChannel.Magical,reductionFlat=amount});
   effects[id]=new Entry{id=id,until=Time.time+seconds,defense=true};
  }
  public void ApplyResistance(string id,ElementDefinition element,float fraction,float seconds){
   if(element==null)return;
   stats.SetResistanceModifier(id,new ElementalModifier(element.stableId,-fraction));
   effects[id]=new Entry{id=id,until=Time.time+seconds};
  }
  public void Remove(string id){
   if(!effects.Remove(id,out var e))return;
   if(e.defense){stats.RemoveDefenseModifier(id+":physical");stats.RemoveDefenseModifier(id+":magical");}
   else stats.RemoveResistanceModifier(id);
  }
  void Update(){foreach(var e in effects.Values.Where(e=>Time.time>=e.until).ToArray())Remove(e.id);}
  void OnDisable(){foreach(string id in effects.Keys.ToArray())Remove(id);}
 }
}
