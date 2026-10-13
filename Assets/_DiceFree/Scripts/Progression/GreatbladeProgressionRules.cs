using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiceFree.Progression
{
    /// <summary>
    /// Tier II Greatblade prototype rank rules. Data-only; does not mutate
    /// existing three starter class save structures or action bar.
    /// </summary>
    public static class GreatbladeProgressionRules
    {
        public const int Tier=2;
        public const string ClassId="class.greatblade";
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
            new(Kind.WideArc,0,false,1,6,12,18,24,30),
            new(Kind.SpinDash,1,false,1,9,18,27),
            new(Kind.OverheadBreaker,2,false,6,12,18,24,30),
            new(Kind.BattleCry,3,false,18,24,30),
            new(Kind.GreatswordMastery,4,true,1,6,12,18,24,30),
            new(Kind.HeavyRhythm,5,true,1,5,10,15,20,25,30)
        };
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
        public static bool GetsMasteryCleaveHaste(int rank)=>rank>=2;
        public static bool GetsMasteryWideArcWidth(int rank)=>rank>=3;
        public static bool GetsMasteryFinisherProtection(int rank)=>rank>=4;
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
