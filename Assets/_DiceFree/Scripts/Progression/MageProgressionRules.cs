using System;
using UnityEngine;
namespace DiceFree.Progression
{
 public static class MageProgressionRules
 {
  public const string ClassId="class.mage";
  public const int TotalRanks=80;
  public enum Kind {EmberBolt,FrostSnare,ArcLightning,Quicksand,HalfLife,MathBarrier,ElementalWeaving,ManaAttunement}
  public readonly struct Rule {
   public readonly Kind kind; public readonly bool passive;public readonly int[] rankLevels;
   public Rule(Kind k,bool p,params int[] levels){kind=k;passive=p;rankLevels=levels;}
   public int AvailableRank(int level){int n=0;foreach(int gate in rankLevels)if(level>=gate)n++;return n;}
   public int NextRankLevel(int rank)=>rank>=0&&rank<rankLevels.Length?rankLevels[rank]:int.MaxValue;
  }
  public static readonly Rule[] Skills={
   new(Kind.EmberBolt,false,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18),
   new(Kind.FrostSnare,false,1,8,15,22,29,36,43,50,57,64,71,78),
   new(Kind.ArcLightning,false,6,12,18,24,30,36,42,48,54,60,66,72),
   new(Kind.Quicksand,false,12,19,26,33,40,47,54,61,68,75),
   new(Kind.HalfLife,false,18,22,26,30,34,38,42,46),new(Kind.MathBarrier,false,6,12,18,24,30),
   new(Kind.ElementalWeaving,true,1,12,23,34,45,56,67,78),new(Kind.ManaAttunement,true,1,14,27,40,53,66,79)};
  public static int AllocationBudget(int classLevel)=>Mathf.Min(Mathf.Max(0,classLevel),TotalRanks);
  public static Rule For(Kind k){foreach(var r in Skills)if(r.kind==k)return r;throw new ArgumentOutOfRangeException(nameof(k));}
  public static bool CanRankUp(Kind kind,int level,int rank)=>rank>=0&&rank<For(kind).rankLevels.Length&&level>=For(kind).rankLevels[rank];
  public static string Name(Kind k)=>k switch {Kind.EmberBolt=>"Ember Bolt",Kind.FrostSnare=>"Frost Snare",Kind.ArcLightning=>"Arc Lightning",Kind.Quicksand=>"Quicksand",Kind.HalfLife=>"Half-Life",Kind.MathBarrier=>"Math Barrier",Kind.ElementalWeaving=>"Elemental Weaving",_=>"Mana Attunement"};
  public static float Cooldown(Kind k)=>k switch {Kind.EmberBolt=>5,Kind.FrostSnare=>8,Kind.ArcLightning=>12,Kind.Quicksand=>14,Kind.HalfLife=>10,Kind.MathBarrier=>18,_=>0};
  public static float Cost(Kind k,int r)=>k switch {Kind.EmberBolt=>10+2*r,Kind.FrostSnare=>12+2*r,Kind.ArcLightning=>20+3*r,Kind.Quicksand=>18+3*r,Kind.HalfLife=>16+2*r,Kind.MathBarrier=>12+2*r,_=>0};
  public static float Coefficient(Kind k,int r)=>k switch {Kind.EmberBolt=>2+.4f*(r-1),Kind.FrostSnare=>1.2f+.25f*(r-1),Kind.ArcLightning=>1.8f+.35f*(r-1),Kind.Quicksand=>.35f+.1f*(r-1),Kind.HalfLife=>.45f+.12f*(r-1),_=>0};
  public static float BarrierDuration(int rank)=>rank*2;
  public static float BarrierFraction(int rank)=>(25+rank*5)/100f;
  public static float ManaPerDamage(int rank)=>2-rank*.1f;
  public static float FrostFreezeDuration(int rank)=>Mathf.Min(2.5f,.75f+.25f*Mathf.Max(0,rank));
  public static int ArcLightningTargetCount(int rank)=>Mathf.Min(8,3+Mathf.Max(0,rank-1)/2);
  public static float HalfLifeDefenseReduction(int rank)=>Mathf.Min(40,5*Mathf.Max(0,rank));
  public static float QuicksandSlowPercent(int rank)=>Mathf.Min(40,4*Mathf.Max(0,rank));
  public static float WeavingResistanceReduction(int rank)=>Mathf.Min(8,Mathf.Max(0,rank));
 }
}
