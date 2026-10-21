using DiceFree.Combat;using DiceFree.Progression;using DiceFree.Skills;using UnityEngine;
using Kind=DiceFree.Progression.MageProgressionRules.Kind;
namespace DiceFree.UI
{
 public sealed class MageAbilityProvider:IClassAbilityProvider
 {
  readonly MageSkillProgression skills;readonly MageSkillCaster caster;
  public MageAbilityProvider(MageSkillProgression s,MageSkillCaster c){skills=s;caster=c;}
  public string ClassId=>MageProgressionRules.ClassId;
  public int Count=>MageProgressionRules.Skills.Length;public int UltimateIndex=>-1;
  public int UnspentPoints=>skills.UnspentPoints;
  public AbilityBarSource.SlotView View(int index,string key){
   if(index<0||index>=Count)return default;var kind=(Kind)index;var rule=MageProgressionRules.For(kind);int rank=skills.Rank(kind);
   string icon=kind switch{Kind.EmberBolt=>"fire-imbuement",Kind.FrostSnare=>"ice-burst",Kind.ArcLightning=>"magic-sand",Kind.MathBarrier=>"mend",_=>"mage-"+kind.ToString().ToLowerInvariant()};
   return new AbilityBarSource.SlotView(MageProgressionRules.Name(kind),rule.passive?"":key,rank,rule.rankLevels.Length,caster.CooldownRemaining(kind),rank>0,Describe(kind,rank),SkillIconLibrary.Get(icon),icon,rule.passive,skills.CanSpend(kind));
  }
  public bool Upgrade(int index)=>index>=0&&index<Count&&skills.Spend((Kind)index);
  public string UpgradeTooltip(int index)=>index<0||index>=Count?"":"Spend 1 skill point\n"+Describe((Kind)index,skills.Rank((Kind)index)+1);
  string Describe(Kind kind,int rank){
   var rule=MageProgressionRules.For(kind);int r=Mathf.Max(1,rank);
   string text=MageProgressionRules.Name(kind)+" — "+(rule.passive?"Passive":"Spell")+"\n"+(rank>0?"Rank "+rank:"Unlock level "+rule.rankLevels[0])+"\n";
   text+=kind switch{
    Kind.EmberBolt=>$"Fire projectile: 15 + {MageProgressionRules.Coefficient(kind,r):0.##} × INT magical damage.",
    Kind.FrostSnare=>$"Ice: 15 + {MageProgressionRules.Coefficient(kind,r):0.##} × INT damage. Freeze up to {MageProgressionRules.FrostFreezeDuration(r):0.##}s, using shared stun resistance and immunity.",
    Kind.ArcLightning=>$"Electric damage: 15 + {MageProgressionRules.Coefficient(kind,r):0.##} × INT. Hits up to {MageProgressionRules.ArcLightningTargetCount(r)} distinct enemies; 6m jumps; each jump deals 80% of the previous hit.",
    Kind.Quicksand=>$"Nature field, 3.5m radius for {4+r}s. Once per second: 15 + {MageProgressionRules.Coefficient(kind,r):0.##} × INT damage, {MageProgressionRules.QuicksandSlowPercent(r)}% slow.",
    Kind.HalfLife=>$"Radiation DoT: 15 + {MageProgressionRules.Coefficient(kind,r):0.##} × INT each second for {6+2*r}s. Reduces both defenses by {MageProgressionRules.HalfLifeDefenseReduction(r)}; same caster refreshes rather than stacking.",
    Kind.MathBarrier=>$"Self only. For {2*r}s redirects {25+5*r}% of post-shield damage to Mana, at {2-.1f*r:0.#} Mana per damage absorbed. Insufficient Mana absorbs only what is affordable; remaining damage reaches HP.",
    Kind.ElementalWeaving=>$"One stack per unique element cast; repeated elements refresh the shared 8s delay. +5% spell/basic damage per stack. After 8s without an elemental cast, lose one oldest stack per second. Each cast reduces OTHER Mage cooldowns by 0.2s. Nearby enemies within {caster.Tuning.auraRadius:0.#}m lose {MageProgressionRules.WeavingResistanceReduction(r)} percentage points of resistance to the six Mage schools; strongest overlapping aura wins. No hit is needed. Damage ticks, bounces, targets and basic attacks do not create stacks.\nCurrent stacks: {caster.WeavingStacks}",
    _=>$"+{30*r} maximum Mana and +{.5f*r:0.#} Mana regeneration per second."
   };
   if(!rule.passive)text+=$"\nMana {MageProgressionRules.Cost(kind,r):0}; cooldown {MageProgressionRules.Cooldown(kind):0}s; range {(kind==Kind.MathBarrier?"self":"18m")}.";
   int next=rule.NextRankLevel(rank);if(next<int.MaxValue)text+="\nNext rank: class level "+next+" and 1 skill point.";
   if(skills.UnspentPoints>0)text+="\nBanked class skill points: "+skills.UnspentPoints+".";
   return text;
  }
  public bool Activate(int index,SkillTargetingController targeting,IQueuedOrders orders,bool append){
   if(index<0||index>=Count||targeting==null)return false;var kind=(Kind)index;
   if(!caster.CanPrepare(kind))return false;
   if(kind==Kind.Quicksand){targeting.BeginGround(MageProgressionRules.Name(kind),caster.CastQuicksand,caster.Tuning.range);return true;}
   if(kind==Kind.MathBarrier){if(append&&orders!=null)return orders.SubmitAction(()=>caster.Cast(kind),true);orders?.ClearOrders();targeting.Cancel();return caster.Cast(kind);}
   targeting.BeginHostile(MageProgressionRules.Name(kind),target=>caster.Cast(kind,target),caster.Tuning.range);return true;
  }
 }
}
