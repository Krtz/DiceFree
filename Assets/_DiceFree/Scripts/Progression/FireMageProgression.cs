using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent, RequireComponent(typeof(ExperienceProgression))]
    public sealed class FireMageProgression : MonoBehaviour, IClassSkillState
    {
        public const string Id = "class.fire-mage";
        private static readonly string[] Keys = { "fireball", "flame-ward", "ignite", "living-bomb", "overheat-mastery", "infernal-presence" };
        private static readonly string[] Names = { "Fireball", "Flame Ward", "Ignite", "Living Bomb", "Overheat Mastery", "Infernal Presence" };
        private static readonly int[][] Gates = {
            new[]{1,2,11,15,20,24,30,31,37,41,45,50,54,60,61,67,71,75,80,84,90,91,97,101,105,110,114,120,121,127,131,135,140,144,150},
            new[]{5,7,16,21,27,34,40,46,51,57,64,70,76,81,87,94,100,106,111,117,124,130,136,141,147},
            new[]{3,6,12,18,22,28,32,38,42,48,52,58,62,68,72,78,82,88,92,98,102,108,112,118,122,128,132,138,142,148},
            new[]{8,9,13,19,23,29,33,39,43,49,53,59,63,69,73,79,83,89,93,99,103,109,113,119,123,129,133,139,143,149},
            new[]{1,17,26,36,47,56,66,77,86,96,107,116,126,137,146},
            new[]{10,14,25,35,44,55,65,74,85,95,104,115,125,134,145}
        };
        private readonly int[] ranks = new int[6];
        private ExperienceProgression experience;
        public string ClassId => Id;
        public bool Active { get; private set; }
        public int UnspentPoints => Active ? Mathf.Max(0, Mathf.Min(150,experience.Level)-ranks.Sum()) : 0;
        public event Action Changed;
        private void Awake() => experience=GetComponent<ExperienceProgression>();
        private void OnEnable() => experience.Changed+=Notify;
        private void OnDisable() => experience.Changed-=Notify;
        private void Notify() => Changed?.Invoke();
        public static string Name(int i) => (uint)i<6 ? Names[i] : "";
        public static string SkillId(int i) => "skill.fire-mage."+Keys[i];
        public static int MaxRank(int i) => (uint)i<6 ? Gates[i].Length : 0;
        public static int RequiredLevel(int i,int rank) => (uint)i<6 && rank>0 && rank<=MaxRank(i) ? Gates[i][rank-1] : int.MaxValue;
        public int Rank(int i) => Active && (uint)i<6 ? ranks[i] : 0;
        public bool CanSpend(int i) => (uint)i<6 && UnspentPoints>0 && Rank(i)<MaxRank(i) && experience.Level>=RequiredLevel(i,Rank(i)+1);
        public bool Spend(int i) { if(!CanSpend(i))return false; ranks[i]++; Notify(); return true; }
        public bool CanRestore(SkillRankState[] values,int level)
        {
            int sum=0; var seen=new HashSet<string>();
            foreach(var value in values??Array.Empty<SkillRankState>())
            {
                if(string.IsNullOrWhiteSpace(value.stableId)||value.rank<0||!seen.Add(value.stableId))return false;
                int i=Array.FindIndex(Keys,key=>"skill.fire-mage."+key==value.stableId);
                if(i<0)continue;
                if(value.rank>MaxRank(i)||value.rank>0&&level<RequiredLevel(i,value.rank))return false;
                sum+=value.rank;
            }
            return level>=1 && sum<=Mathf.Min(level,150);
        }
        public SkillRankState[] CaptureState() => Active ? Enumerable.Range(0,6).Select(i=>new SkillRankState(SkillId(i),ranks[i])).ToArray() : Array.Empty<SkillRankState>();
        public void RestoreState(SkillRankState[] values)
        {
            if(!CanRestore(values,experience.Level))throw new InvalidOperationException("Invalid FireMage ranks.");
            Array.Clear(ranks,0,6);
            foreach(var value in values??Array.Empty<SkillRankState>())for(int i=0;i<6;i++)if(value.stableId==SkillId(i))ranks[i]=value.rank;
            Notify();
        }
        public void SetClassActive(bool value) { if(Active==value)return; Active=value; Array.Clear(ranks,0,6); Notify(); }
    }
}
