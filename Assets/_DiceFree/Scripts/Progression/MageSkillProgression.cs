using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using UnityEngine;
using Kind=DiceFree.Progression.MageProgressionRules.Kind;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent,RequireComponent(typeof(ExperienceProgression),typeof(ActorStats))]
    public sealed class MageSkillProgression : MonoBehaviour,IClassSkillState
    {
        const string MasterySource="skill.mage.mastery";
        readonly Dictionary<Kind,int> ranks=new();
        ExperienceProgression xp;
        ActorStats stats;
        bool active;
        public string ClassId=>MageProgressionRules.ClassId;
        public bool ActiveForCurrentClass=>active;
        public int Level=>xp?.Level??1;
        public int SpentPoints=>MageProgressionRules.Skills.Sum(r=>Mathf.Max(0,Rank(r.kind)-StarterRank(r.kind)));
        public int UnspentPoints=>active?Mathf.Max(0,MageProgressionRules.AllocationBudget(Level)-SpentPoints):0;
        public event Action Changed;
        public static string Id(Kind kind)=>"skill.mage."+kind.ToString().ToLowerInvariant();
        public static int StarterRank(Kind kind)=>0;
        void Awake()
        {
            xp=GetComponent<ExperienceProgression>();stats=GetComponent<ActorStats>();
            active=stats.Definition?.stableId==ClassId;
            ApplyPassive();
        }
        void OnEnable(){xp??=GetComponent<ExperienceProgression>();xp.Changed+=OnLevel;}
        void OnDisable(){if(xp!=null)xp.Changed-=OnLevel;ClearPassive();}
        public int Rank(Kind kind)=>active?(ranks.TryGetValue(kind,out int n)?n:StarterRank(kind)):0;
        public bool CanSpend(Kind kind)=>active&&UnspentPoints>0&&MageProgressionRules.CanRankUp(kind,Level,Rank(kind));
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
                var matching=MageProgressionRules.Skills.Where(r=>Id(r.kind)==value.stableId).ToArray();
                if(matching.Length==0)continue;
                var rule=matching[0];
                if(value.rank>rule.AvailableRank(level))return false;
                spent+=Mathf.Max(0,value.rank-StarterRank(rule.kind));
            }
            return level>=1&&spent<=MageProgressionRules.AllocationBudget(level);
        }
        public void RestoreState(SkillRankState[] values)
        {
            if(!CanRestore(values,Level))throw new InvalidOperationException("Invalid Mage skill ranks/level gates.");
            ranks.Clear();
            foreach(var value in values??Array.Empty<SkillRankState>())
                foreach(var rule in MageProgressionRules.Skills)
                    if(Id(rule.kind)==value.stableId)ranks[rule.kind]=value.rank;
            ApplyPassive();Changed?.Invoke();
        }
        public SkillRankState[] CaptureState()=>!active?Array.Empty<SkillRankState>():
            MageProgressionRules.Skills.Select(r=>new SkillRankState(Id(r.kind),Rank(r.kind))).ToArray();
        public void SetClassActive(bool value)
        {
            if(active==value)return;
            active=value;ranks.Clear();ApplyPassive();Changed?.Invoke();
        }
        void OnLevel(){ApplyPassive();Changed?.Invoke();}
        void ApplyPassive()
        {
            var resources=GetComponent<ActorResourceController>();if(resources==null)return;
            int rank=Rank(Kind.ManaAttunement);
            resources.SetMaximumFlatModifier(ResourceIds.Mana,MasterySource,rank*30);
            resources.SetRegenerationFlatModifier(ResourceIds.Mana,MasterySource,rank*.5f);
        }
        void ClearPassive(){
            var resources=GetComponent<ActorResourceController>();if(resources==null)return;
            resources.SetMaximumFlatModifier(ResourceIds.Mana,MasterySource,0);
            resources.SetRegenerationFlatModifier(ResourceIds.Mana,MasterySource,0);
        }
    }
}
