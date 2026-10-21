using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Progression
{
    /// <summary>
    /// Tier II Greatblade rank gates and bounded allocation rules.
    /// </summary>
    public static class GreatbladeProgressionRules
    {
        public const int Tier=2;
        public const string ClassId="class.greatblade";
        public const int TotalRanks=78;
        public enum Kind
        {
            WideArc,SpinDash,OverheadBreaker,BattleCry,
            GreatswordMastery,HeavyRhythm
        }
        public readonly struct Rule
        {
            public readonly Kind kind;
            public readonly int slot;
            public readonly bool passive;
            public readonly int[] rankLevels;
            public Rule(Kind kind,int slot,bool passive,params int[] ranks)
            {
                this.kind=kind;
                this.slot=slot;
                this.passive=passive;
                rankLevels=ranks;
            }
            public int AvailableRank(int level)
            {
                int result=0;
                foreach(int required in rankLevels)
                    if(level>=required)result++;
                return result;
            }
            public int NextRankLevel(int currentRank) =>
                currentRank>=0 && currentRank<rankLevels.Length?
                rankLevels[currentRank]:int.MaxValue;
        }

        public static readonly Rule[] Skills={
            new(Kind.WideArc,0,false,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20),
            new(Kind.SpinDash,1,false,1,6,11,16,21,26,31,36,41,46,51,56,61,66,71,76),
            new(Kind.OverheadBreaker,2,false,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23),
            new(Kind.BattleCry,3,false,18,26,34,42,50,58,66,74),
            new(Kind.GreatswordMastery,4,true,1,11,21,31,41,51,61,71),
            new(Kind.HeavyRhythm,5,true,1,12,23,34,45,56,67,78)
        };
        public static int AllocationBudget(int classLevel)=>Mathf.Min(Mathf.Max(0,classLevel),TotalRanks);
        public static Rule For(Kind kind)
        {
            foreach(var rule in Skills)if(rule.kind==kind)return rule;
            throw new ArgumentOutOfRangeException(nameof(kind));
        }
        public static bool CanRankUp(Kind kind,int classLevel,int currentRank)
        {
            var rule=For(kind);
            return currentRank>=0 &&
                currentRank<rule.rankLevels.Length &&
                classLevel>=rule.rankLevels[currentRank];
        }
        public static bool IsUltimateSlotAllowed(int classTier)=>classTier>=4;
        public static bool GetsMasteryCleaveHaste(int rank)=>rank>=3;
        public static bool GetsMasteryWideArcWidth(int rank)=>rank>=5;
        public static bool GetsMasteryFinisherProtection(int rank)=>rank>=7;
        public static float MasteryHastePercent(int rank)=>rank>=3?8+2*(rank-3):0;
        public static float MasteryHasteDuration(int rank)=>rank>=3?3+.25f*(rank-3):0;
        public static float MasteryArcWidth(int rank)=>rank>=5?15+3*(rank-5):0;
        public static float MasteryShieldCapacity(float maximumHp)=>100+Mathf.Max(0,maximumHp)*.05f;
        public static float DamageBonusPct(int rank)=>
            Mathf.Max(0,rank)*2f;
        public static float StrengthBonus(int rank)=>
            Mathf.Max(0,rank)*1f;
        public static float MomentumGainFactor(int rhythmRank)=>
            1f+Mathf.Max(0,rhythmRank)*.08f;
        public static float MomentumDecayFactor(int rhythmRank)=>
            Mathf.Max(.3f,1f-Mathf.Max(0,rhythmRank)*.07f);
        // Deliberately no Momentum-threshold bonuses.
    }
}
