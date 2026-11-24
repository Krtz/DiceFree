using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;

namespace DiceFree.Skills
{
    public abstract class PsionEvolutionProgression : MonoBehaviour, IClassSkillState
    {
        public const string KnightId = "class.psionic-knight", MageId = "class.mirage-mage";
        private static readonly int[][] KnightGates = { new[]{1, 3, 8, 16, 21, 26, 30, 35, 41, 46, 51, 56, 60, 65, 70, 77, 82, 87, 91, 96, 101, 106, 112, 117, 121, 126, 131, 136, 141, 146},
new[]{1, 4, 13, 19, 25, 31, 38, 44, 50, 55, 61, 68, 74, 79, 86, 92, 97, 103, 109, 116, 122, 128, 134, 139, 144},
new[]{6, 7, 9, 17, 22, 27, 33, 37, 42, 47, 53, 59, 64, 69, 73, 78, 83, 89, 94, 100, 105, 110, 114, 120, 125, 130, 135, 140, 145},
new[]{1, 2, 5, 14, 18, 23, 28, 32, 36, 40, 45, 49, 54, 58, 63, 67, 72, 75, 80, 84, 88, 93, 98, 102, 107, 111, 115, 119, 124, 129, 133, 138, 143, 148},
new[]{10, 11, 12, 15, 20, 24, 29, 34, 39, 43, 48, 52, 57, 62, 66, 71, 76, 81, 85, 90, 95, 99, 104, 108, 113, 118, 123, 127, 132, 137, 142, 147} };
        private static readonly int[][] MageGates = { new[]{1, 2, 4, 15, 19, 24, 28, 32, 36, 41, 46, 50, 55, 59, 64, 68, 73, 76, 81, 85, 89, 94, 98, 103, 108, 112, 116, 120, 125, 130, 134, 139, 144, 148},
new[]{1, 3, 14, 21, 26, 33, 39, 45, 51, 56, 62, 69, 75, 80, 87, 93, 99, 104, 110, 117, 123, 129, 135, 140, 145},
new[]{5, 6, 7, 17, 22, 27, 31, 37, 42, 47, 52, 57, 61, 66, 71, 78, 83, 88, 92, 97, 102, 107, 113, 118, 122, 127, 132, 137, 142, 147},
new[]{8, 9, 13, 18, 23, 29, 34, 38, 43, 49, 54, 60, 65, 70, 74, 79, 84, 90, 95, 101, 106, 111, 115, 121, 126, 131, 136, 141, 146},
new[]{10, 11, 12, 16, 20, 25, 30, 35, 40, 44, 48, 53, 58, 63, 67, 72, 77, 82, 86, 91, 96, 100, 105, 109, 114, 119, 124, 128, 133, 138, 143, 149} };
        private static readonly string[] KnightKeys={"blades-mastery","mental-image","void-shield","empowered-attacks","speed-presence"};
        private static readonly string[] MageKeys={"psychic-bolt","mental-image","lightning-storm","metal-shield","speed-presence"};
        private static readonly string[] KnightNames={"Psionic Blades Mastery","Mobile Mental Image","Void Aegis","Empowered Attacks","Speed Presence"};
        private static readonly string[] MageNames={"Psychic Bolt","Mirroring Mental Image","Lightning Storm","Metal Shield","Speed Presence"};
        private readonly int[] ranks=new int[5];
        private ExperienceProgression experience;
        public abstract string ClassId {get;}
        public bool Knight => ClassId==KnightId;
        public bool Active {get;private set;}
        public int UnspentPoints => Active?Mathf.Max(0,Mathf.Min(experience.Level,150)-ranks.Sum()):0;
        public event Action Changed;
        protected virtual void Awake()=>experience=GetComponent<ExperienceProgression>();
        protected virtual void OnEnable()=>experience.Changed+=Notify;
        protected virtual void OnDisable()=>experience.Changed-=Notify;
        private void Notify()=>Changed?.Invoke();
        public static string Name(bool knight,int i)=>(uint)i<5?(knight?KnightNames:MageNames)[i]:"";
        public static string SkillId(bool knight,int i)=>"skill."+(knight?"psionic-knight.":"mirage-mage.")+(knight?KnightKeys:MageKeys)[i];
        public static int MaxRank(bool knight,int i)=>(uint)i<5?(knight?KnightGates:MageGates)[i].Length:0;
        public static int RequiredLevel(bool knight,int i,int rank)=>(uint)i<5&&rank>=1&&rank<=MaxRank(knight,i)?(knight?KnightGates:MageGates)[i][rank-1]:int.MaxValue;
        public int Rank(int i)=>Active&&(uint)i<5?ranks[i]:0;
        public bool CanSpend(int i)=>(uint)i<5&&UnspentPoints>0&&Rank(i)<MaxRank(Knight,i)&&experience.Level>=RequiredLevel(Knight,i,Rank(i)+1);
        public bool Spend(int i){if(!CanSpend(i))return false;ranks[i]++;Notify();return true;}
        public bool CanRestore(SkillRankState[] values,int level)
        {
            var seen=new HashSet<string>();int sum=0;
            foreach(var v in values??Array.Empty<SkillRankState>())
            {
                if(string.IsNullOrWhiteSpace(v.stableId)||v.rank<0||!seen.Add(v.stableId))return false;
                int i=Enumerable.Range(0,5).Where(n=>SkillId(Knight,n)==v.stableId).DefaultIfEmpty(-1).First();
                if(i<0)continue;
                if(v.rank>MaxRank(Knight,i)||v.rank>0&&level<RequiredLevel(Knight,i,v.rank))return false;sum+=v.rank;
            }
            return level>=1&&sum<=Mathf.Min(level,150);
        }
        public SkillRankState[] CaptureState()=>Active?Enumerable.Range(0,5).Select(i=>new SkillRankState(SkillId(Knight,i),ranks[i])).ToArray():Array.Empty<SkillRankState>();
        public void RestoreState(SkillRankState[] values)
        {
            if(!CanRestore(values,experience.Level))throw new InvalidOperationException("Invalid Tier III Psion ranks.");
            Array.Clear(ranks,0,5);foreach(var v in values??Array.Empty<SkillRankState>())for(int i=0;i<5;i++)if(v.stableId==SkillId(Knight,i))ranks[i]=v.rank;Notify();
        }
        public void SetClassActive(bool active){if(Active==active)return;Active=active;Array.Clear(ranks,0,5);Notify();}
    }
}

