using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent,RequireComponent(typeof(ExperienceProgression))]
    public sealed class MarksmanProgression : MonoBehaviour,IClassSkillState
    {
        public const string Id="class.marksman";
        static readonly string[] Keys={"aimed-shot","arrow-rain","bow-mastery","strafe","ranged-aura","power-shot"};
        static readonly string[] Names={"Aimed Shot","Arrow Rain","Bow Mastery","Strafe","Ranged Damage Aura","Power Shot"};
        static readonly int[][] Gates={
            new[]{1,2,9,15,21,27,29,35,41,45,51,57,59,65,71,75,81,87,89,95,101,105,111,117,119,125,131,135,141,147},
            new[]{5,6,13,19,25,31,37,43,49,55,61,67,73,79,85,91,97,103,109,115,121,127,133,139,145},
            new[]{1,4,17,23,33,39,47,53,63,69,77,83,93,99,107,113,123,129,137,143},
            new[]{1,3,12,16,22,28,30,36,42,46,52,58,60,66,72,76,82,88,90,96,102,106,112,118,120,126,132,136,142,148},
            new[]{10,11,18,24,34,40,48,54,64,70,78,84,94,100,108,114,124,130,138,144},
            new[]{7,8,14,20,26,32,38,44,50,56,62,68,74,80,86,92,98,104,110,116,122,128,134,140,146}
        };
        readonly int[] ranks=new int[6];ExperienceProgression experience;
        public string ClassId=>Id;
        public bool Active{get;private set;}
        public int UnspentPoints=>Active?Mathf.Max(0,Mathf.Min(150,experience.Level)-ranks.Sum()):0;
        public event Action Changed;
        void Awake()=>experience=GetComponent<ExperienceProgression>();
        void OnEnable()=>experience.Changed+=Notify;
        void OnDisable()=>experience.Changed-=Notify;
        void Notify()=>Changed?.Invoke();
        public static string Name(int i)=>(uint)i<6?Names[i]:"";
        public static string SkillId(int i)=>"skill.marksman."+Keys[i];
        public static int MaxRank(int i)=>(uint)i<6?Gates[i].Length:0;
        public static int RequiredLevel(int i,int rank)=>(uint)i<6&&rank>0&&rank<=MaxRank(i)?Gates[i][rank-1]:int.MaxValue;
        public int Rank(int i)=>Active&&(uint)i<6?ranks[i]:0;
        public bool CanSpend(int i)=>(uint)i<6&&UnspentPoints>0&&Rank(i)<MaxRank(i)&&experience.Level>=RequiredLevel(i,Rank(i)+1);
        public bool Spend(int i){if(!CanSpend(i))return false;ranks[i]++;Notify();return true;}
        public bool CanRestore(SkillRankState[] values,int level)
        {
            int sum=0;var seen=new HashSet<string>();
            foreach(var v in values??Array.Empty<SkillRankState>()){
                if(string.IsNullOrWhiteSpace(v.stableId)||v.rank<0||!seen.Add(v.stableId))return false;
                int i=Array.FindIndex(Keys,k=>"skill.marksman."+k==v.stableId);if(i<0)continue;
                if(v.rank>MaxRank(i)||v.rank>0&&level<RequiredLevel(i,v.rank))return false;sum+=v.rank;
            }
            return level>=1&&sum<=Mathf.Min(level,150);
        }
        public SkillRankState[] CaptureState()=>Active?Enumerable.Range(0,6).Select(i=>new SkillRankState(SkillId(i),ranks[i])).ToArray():Array.Empty<SkillRankState>();
        public void RestoreState(SkillRankState[] values){if(!CanRestore(values,experience.Level))throw new InvalidOperationException("Invalid Marksman ranks.");Array.Clear(ranks,0,6);foreach(var v in values??Array.Empty<SkillRankState>())for(int i=0;i<6;i++)if(v.stableId==SkillId(i))ranks[i]=v.rank;Notify();}
        public void SetClassActive(bool value){if(Active==value)return;Active=value;Array.Clear(ranks,0,6);Notify();}
    }
}
