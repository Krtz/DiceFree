using System;
using System.Collections.Generic;

namespace DiceFree.Dungeons
{
    public enum WolfRank { Weak=1, Strong=2, Alpha=3 }
    public readonly struct WolfWaveComposition
    {
        public readonly int Weak, Strong, Alpha;
        public int Points => Weak+2*Strong+3*Alpha;
        public int Count => Weak+Strong+Alpha;
        public WolfWaveComposition(int weak,int strong,int alpha){Weak=weak;Strong=strong;Alpha=alpha;}
        public IEnumerable<WolfRank> Wolves
        {
            get { for(int i=0;i<Weak;i++)yield return WolfRank.Weak;for(int i=0;i<Strong;i++)yield return WolfRank.Strong;for(int i=0;i<Alpha;i++)yield return WolfRank.Alpha; }
        }
    }
    public static class WolfWavePlan
    {
        public const int WaveCount=10;
        public static int Budget(int wave)
        { if(wave<1||wave>WaveCount)throw new ArgumentOutOfRangeException(nameof(wave));return wave+2; }
        public static WolfWaveComposition Create(int wave,int seed)
        {
            int budget=Budget(wave);
            if(wave==10)return new WolfWaveComposition(0,0,4);
            var candidates=new List<WolfWaveComposition>();
            int minAlpha=wave>=7?2:wave>=4?1:0,maxAlpha=wave>=4?budget/3:0;
            for(int alpha=minAlpha;alpha<=maxAlpha;alpha++)for(int strong=0;strong<=(budget-3*alpha)/2;strong++)
            {
                if(wave>=2&&wave<=3&&strong==0)continue;
                candidates.Add(new WolfWaveComposition(budget-3*alpha-2*strong,strong,alpha));
            }
            var random=new Random(unchecked(seed*397^wave*7919));
            return candidates[random.Next(candidates.Count)];
        }
    }
}
