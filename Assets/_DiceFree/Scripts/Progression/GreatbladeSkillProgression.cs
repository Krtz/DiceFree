using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;
using Kind=DiceFree.Progression.GreatbladeProgressionRules.Kind;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent,RequireComponent(typeof(ExperienceProgression),typeof(ActorStats))]
    public sealed class GreatbladeSkillProgression : MonoBehaviour,IClassSkillState
    {
        const string MasterySource="skill.greatblade.mastery";
        readonly Dictionary<Kind,int> ranks=new();
        ExperienceProgression xp;
        ActorStats stats;
        bool active;
        public string ClassId=>GreatbladeProgressionRules.ClassId;
        public bool ActiveForCurrentClass=>active;
        public int Level=>xp?.Level??1;
        public int SpentPoints=>GreatbladeProgressionRules.Skills.Sum(r=>Mathf.Max(0,Rank(r.kind)-StarterRank(r.kind)));
        public int UnspentPoints=>active?Mathf.Max(0,Level-1-SpentPoints):0;
        public event Action Changed;
        public static string Id(Kind kind)=>"skill.greatblade."+kind.ToString().ToLowerInvariant();
        public static int StarterRank(Kind kind)=>GreatbladeProgressionRules.For(kind).rankLevels[0]==1?1:0;
        void Awake()
        {
            xp=GetComponent<ExperienceProgression>();stats=GetComponent<ActorStats>();
            active=stats.Definition?.stableId==ClassId;
            ApplyPassive();
        }
        void OnEnable(){xp??=GetComponent<ExperienceProgression>();xp.Changed+=OnLevel;}
        void OnDisable(){if(xp!=null)xp.Changed-=OnLevel;ClearPassive();}
        public int Rank(Kind kind)=>active?(ranks.TryGetValue(kind,out int n)?n:StarterRank(kind)):0;
        public bool CanSpend(Kind kind)=>active&&UnspentPoints>0&&GreatbladeProgressionRules.CanRankUp(kind,Level,Rank(kind));
        public bool Spend(Kind kind)
        {
            if(!CanSpend(kind))return false;
            ranks[kind]=Rank(kind)+1;ApplyPassive();Changed?.Invoke();return true;
        }
        public bool CanRestore(SkillRankState[] values,int level)
        {
            int spent=0;var seen=new HashSet<string>(StringComparer.Ordinal);
            foreach(var value in values??Array.Empty<SkillRankState>())
            {
                if(string.IsNullOrWhiteSpace(value.stableId)||value.rank<0||!seen.Add(value.stableId))return false;
                var matching=GreatbladeProgressionRules.Skills.Where(r=>Id(r.kind)==value.stableId).ToArray();
                if(matching.Length==0)continue;
                var rule=matching[0];
                if(value.rank>rule.AvailableRank(level))return false;
                spent+=Mathf.Max(0,value.rank-StarterRank(rule.kind));
            }
            return level>=1&&spent<=level-1;
        }
        public void RestoreState(SkillRankState[] values)
        {
            if(!CanRestore(values,Level))throw new InvalidOperationException("Invalid Greatblade skill ranks/level gates.");
            ranks.Clear();
            foreach(var value in values??Array.Empty<SkillRankState>())
                foreach(var rule in GreatbladeProgressionRules.Skills)
                    if(Id(rule.kind)==value.stableId)ranks[rule.kind]=Mathf.Max(StarterRank(rule.kind),value.rank);
            ApplyPassive();Changed?.Invoke();
        }
        public SkillRankState[] CaptureState()=>!active?Array.Empty<SkillRankState>():
            GreatbladeProgressionRules.Skills.Select(r=>new SkillRankState(Id(r.kind),Rank(r.kind))).ToArray();
        public void SetClassActive(bool value)
        {
            if(active==value)return;
            active=value;ranks.Clear();ApplyPassive();Changed?.Invoke();
        }
        void OnLevel(){ApplyPassive();Changed?.Invoke();}
        void ApplyPassive()
        {
            if(stats==null)return;
            int rank=Rank(Kind.GreatswordMastery);
            if(rank==0){ClearPassive();return;}
            stats.SetAttributeContribution(MasterySource,new AttributeValues{strength=GreatbladeProgressionRules.StrengthBonus(rank)});
            stats.SetBasicAttackDamagePercentContribution(MasterySource,GreatbladeProgressionRules.DamageBonusPct(rank));
        }
        void ClearPassive(){if(stats==null)return;stats.RemoveAttributeContribution(MasterySource);stats.RemoveBasicAttackDamagePercentContribution(MasterySource);}
    }
}
