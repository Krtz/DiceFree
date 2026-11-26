using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent, RequireComponent(typeof(ExperienceProgression))]
    public sealed class BerserkerProgression : MonoBehaviour, IClassSkillState
    {
        public const string Id = "class.berserker";
        private static readonly string[] Keys = { "wide-arc", "spin-dash", "rage-breaker", "anger-management", "coffee-shield" };
        private static readonly string[] Names = { "Wide Arc", "Spin Dash", "Rage Breaker", "Anger Management", "Coffee Shield" };
        private static readonly int[][] Gates = {
            new[]{1,2,3,4,8,12,16,20,24,28,32,36,40,44,48,52,56,60,64,68,72,76,80,84,88,92,96,100,104,108,112,116,120,124,128},
            Enumerable.Range(0,30).Select(i=>3+5*i).ToArray(),
            Enumerable.Range(0,30).Select(i=>6+4*i).ToArray(),
            Enumerable.Range(0,25).Select(i=>1+6*i).ToArray(),
            Enumerable.Range(0,30).Select(i=>8+4*i).ToArray()
        };
        private readonly int[] ranks = new int[5];
        private ExperienceProgression experience;
        public string ClassId => Id;
        public bool Active { get; private set; }
        public int UnspentPoints => Active ? Mathf.Max(0, Mathf.Min(150,experience.Level)-ranks.Sum()) : 0;
        public event Action Changed;
        private void Awake() => experience=GetComponent<ExperienceProgression>();
        private void OnEnable() => experience.Changed+=Notify;
        private void OnDisable() => experience.Changed-=Notify;
        private void Notify() => Changed?.Invoke();
        public static string Name(int i) => (uint)i<5 ? Names[i] : "";
        public static string SkillId(int i) => "skill.berserker."+Keys[i];
        public static int MaxRank(int i) => (uint)i<5 ? Gates[i].Length : 0;
        public static int RequiredLevel(int i,int rank) => (uint)i<5 && rank>0 && rank<=MaxRank(i) ? Gates[i][rank-1] : int.MaxValue;
        public int Rank(int i) => Active && (uint)i<5 ? ranks[i] : 0;
        public bool CanSpend(int i) => (uint)i<5 && UnspentPoints>0 && Rank(i)<MaxRank(i) && experience.Level>=RequiredLevel(i,Rank(i)+1);
        public bool Spend(int i) { if(!CanSpend(i))return false; ranks[i]++; Notify(); return true; }
        public bool CanRestore(SkillRankState[] values,int level)
        {
            int sum=0; var seen=new HashSet<string>();
            foreach(var value in values??Array.Empty<SkillRankState>())
            {
                if(string.IsNullOrWhiteSpace(value.stableId)||value.rank<0||!seen.Add(value.stableId))return false;
                int i=Array.FindIndex(Keys,key=>"skill.berserker."+key==value.stableId);
                if(i<0)continue;
                if(value.rank>MaxRank(i)||value.rank>0&&level<RequiredLevel(i,value.rank))return false;
                sum+=value.rank;
            }
            return level>=1 && sum<=Mathf.Min(level,150);
        }
        public SkillRankState[] CaptureState() => Active ? Enumerable.Range(0,5).Select(i=>new SkillRankState(SkillId(i),ranks[i])).ToArray() : Array.Empty<SkillRankState>();
        public void RestoreState(SkillRankState[] values)
        {
            if(!CanRestore(values,experience.Level))throw new InvalidOperationException("Invalid Berserker ranks.");
            Array.Clear(ranks,0,5);
            foreach(var value in values??Array.Empty<SkillRankState>())for(int i=0;i<5;i++)if(value.stableId==SkillId(i))ranks[i]=value.rank;
            Notify();
        }
        public void SetClassActive(bool value) { if(Active==value)return; Active=value; Array.Clear(ranks,0,5); Notify(); }
    }
}
