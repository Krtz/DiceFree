using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Progression;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    /// <summary>Class-owned presentation and activation. The HUD does not prescribe a kit size or rank economy.</summary>
    public interface IClassAbilityProvider
    {
        string ClassId { get; }
        int Count { get; }
        int UltimateIndex { get; }
        int UnspentPoints { get; }
        AbilityBarSource.SlotView View(int index, string key);
        bool Upgrade(int index);
        string UpgradeTooltip(int index);
        bool Activate(int index, SkillTargetingController targeting, IQueuedOrders orders, bool append);
    }

    public sealed class DefinitionAbilityProvider<T> : IClassAbilityProvider where T : UnityEngine.Object
    {
        public string ClassId { get; }
        public int UltimateIndex => -1;
        public int Count => Definitions.Count;
        public int UnspentPoints => points();
        private readonly Func<IReadOnlyList<T>> definitions;
        private readonly Func<T, bool> active;
        private readonly Func<int> points;
        private readonly Func<T, string, AbilityBarSource.SlotView> view;
        private readonly Func<T, bool> upgrade;
        private readonly Func<T, string> upgradeTooltip;
        private readonly Func<T, SkillTargetingController, IQueuedOrders, bool, bool> activate;
        private IReadOnlyList<T> Definitions => definitions();
        public DefinitionAbilityProvider(string classId, Func<IReadOnlyList<T>> definitions, Func<T,bool> active,
            Func<int> points, Func<T,string,AbilityBarSource.SlotView> view, Func<T,bool> upgrade,
            Func<T,string> upgradeTooltip, Func<T,SkillTargetingController,IQueuedOrders,bool,bool> activate)
        {
            ClassId=classId;this.definitions=definitions;this.active=active;this.points=points;
            this.view=view;this.upgrade=upgrade;this.upgradeTooltip=upgradeTooltip;this.activate=activate;
        }
        // Authored order is authoritative; actives and passives may be interleaved.
        private T At(int index) => index < 0 ? null : Definitions.ElementAtOrDefault(index);
        public AbilityBarSource.SlotView View(int index,string key) { var d=At(index);return d==null?default:view(d,active(d)?key:""); }
        public bool Upgrade(int index) { var d=At(index);return d!=null&&upgrade(d); }
        public string UpgradeTooltip(int index) { var d=At(index);return d==null?"":upgradeTooltip(d); }
        public bool Activate(int index,SkillTargetingController targeting,IQueuedOrders orders,bool append)
        { var d=At(index);return d!=null&&active(d)&&activate(d,targeting,orders,append); }
    }

    public sealed class GreatbladeAbilityProvider : IClassAbilityProvider
    {
        private readonly GreatbladeSkillProgression skills;
        private readonly GreatbladeSkillCaster caster;
        public GreatbladeAbilityProvider(GreatbladeSkillProgression skills,GreatbladeSkillCaster caster) {this.skills=skills;this.caster=caster;}
        public string ClassId => GreatbladeProgressionRules.ClassId;
        public int Count => GreatbladeProgressionRules.Skills.Length;
        public int UltimateIndex => -1;
        public int UnspentPoints => skills.UnspentPoints;
        private GreatbladeProgressionRules.Kind Kind(int index) => GreatbladeProgressionRules.Skills[index].kind;
        public AbilityBarSource.SlotView View(int index,string key) => index<0||index>=Count?default:GreatbladeAbilityViews.View(skills,caster,(int)Kind(index),key);
        public bool Upgrade(int index) => index>=0&&index<Count&&skills.Spend(Kind(index));
        public string UpgradeTooltip(int index) => index<0||index>=Count?"":GreatbladeAbilityViews.Upgrade(skills,caster,(int)Kind(index));
        public bool Activate(int index,SkillTargetingController targeting,IQueuedOrders orders,bool append) =>
            index>=0&&index<Count&&GreatbladeAbilityViews.Activate(skills,caster,targeting,orders,(int)Kind(index),append);
    }

    public static class ClassAbilityProviders
    {
        public static IEnumerable<IClassAbilityProvider> ExistingKits(GameObject owner)
        {
            var novice=owner.GetComponent<NoviceSkillProgression>();var nc=owner.GetComponent<NoviceSkillCaster>();
            if(novice!=null&&nc!=null)yield return new DefinitionAbilityProvider<NoviceSkillDefinition>(
                novice.ClassId,()=>novice.Definitions,d=>d.Active,()=>novice.UnspentPoints,
                (d,key)=>new AbilityBarSource.SlotView(d.displayName,key,novice.Rank(d.stableId),d.maxRank,d.Active?nc.CooldownRemaining(d):0,
                    novice.Rank(d.stableId)>0,SkillTooltips.Describe(d,novice.Rank(d.stableId),key),SkillIconLibrary.Get(d.kind),SkillIconLibrary.Key(d.kind),!d.Active,
                    novice.Rank(d.stableId)<d.maxRank&&novice.UnspentPoints>0),
                d=>novice.Spend(d.stableId),d=>"Spend 1 skill point\n"+SkillTooltips.Describe(d,novice.Rank(d.stableId)+1),
                (d,targeting,orders,append)=>{
                    if(!nc.CanPrepare(d))return false;
                    if(d.kind==NoviceSkillKind.StrengthMeleeStun||d.kind==NoviceSkillKind.MagicSand)
                        targeting.BeginHostile(d.displayName,target=>nc.Cast(d,target),d.range);
                    else targeting.BeginFriendly(d.displayName,target=>nc.Cast(d,target),d.range);
                    return true;
                });
            var physical=owner.GetComponent<PhysicalSkillProgression>();var pc=owner.GetComponent<PhysicalSkillCaster>();
            if(physical!=null&&pc!=null)yield return new DefinitionAbilityProvider<PhysicalSkillDefinition>(
                physical.ClassId,()=>physical.Definitions,d=>d.Active,()=>physical.UnspentPoints,
                (d,key)=>new AbilityBarSource.SlotView(d.displayName,key,physical.Rank(d.stableId),d.maxRank,d.Active?pc.CooldownRemaining(d):0,
                    physical.Rank(d.stableId)>0,SkillTooltips.Describe(d,physical.Rank(d.stableId),key),SkillIconLibrary.Get(d.kind),SkillIconLibrary.Key(d.kind),!d.Active,
                    physical.Rank(d.stableId)<d.maxRank&&physical.UnspentPoints>0),
                d=>physical.Spend(d.stableId),d=>"Spend 1 skill point\n"+SkillTooltips.Describe(d,physical.Rank(d.stableId)+1),
                (d,targeting,orders,append)=>{
                    if(!pc.CanPrepare(d))return false;
                    switch(d.kind){
                        case PhysicalSkillKind.HeavyStrike:targeting.BeginHostile(d.displayName,target=>pc.Cast(d,target),d.range);return true;
                        case PhysicalSkillKind.ArrowRain:targeting.BeginGround(d.displayName,point=>pc.CastArrowRain(d,point),d.range);return true;
                        case PhysicalSkillKind.Guard:case PhysicalSkillKind.Quickening:
                            if(append&&orders!=null)return orders.SubmitAction(()=>pc.Cast(d),true);
                            orders?.ClearOrders();targeting.Cancel();return pc.Cast(d);
                        default:return false;}
                });
            var magical=owner.GetComponent<MagicalSkillProgression>();var mc=owner.GetComponent<MagicalSkillCaster>();
            if(magical!=null&&mc!=null)yield return new DefinitionAbilityProvider<MagicalSkillDefinition>(
                magical.ClassId,()=>magical.Definitions,d=>d.Active,()=>magical.UnspentPoints,
                (d,key)=>new AbilityBarSource.SlotView(d.displayName,key,magical.Rank(d.stableId),d.maxRank,d.Active?mc.CooldownRemaining(d):0,
                    magical.Rank(d.stableId)>0,SkillTooltips.Describe(d,magical.Rank(d.stableId),key),SkillIconLibrary.Get(d.kind),SkillIconLibrary.Key(d.kind),!d.Active,
                    magical.Rank(d.stableId)<d.maxRank&&magical.UnspentPoints>0),
                d=>magical.Spend(d.stableId),d=>"Spend 1 skill point\n"+SkillTooltips.Describe(d,magical.Rank(d.stableId)+1),
                (d,targeting,orders,append)=>{
                    if(!mc.CanPrepare(d))return false;
                    switch(d.kind){
                        case MagicalSkillKind.MagicSand:targeting.BeginHostile(d.displayName,target=>mc.Cast(d,target),d.range);return true;
                        case MagicalSkillKind.Mend:case MagicalSkillKind.FireImbuement:targeting.BeginFriendly(d.displayName,target=>mc.Cast(d,target),d.range);return true;
                        case MagicalSkillKind.IceBurst:targeting.BeginGround(d.displayName,point=>mc.CastIceBurst(d,point),d.range);return true;
                        default:return false;}
                });
            var mage=owner.GetComponent<MageSkillProgression>();var mageCaster=owner.GetComponent<MageSkillCaster>();
            if(mage!=null&&mageCaster!=null)yield return new MageAbilityProvider(mage,mageCaster);
            var greatblade=owner.GetComponent<GreatbladeSkillProgression>();var gc=owner.GetComponent<GreatbladeSkillCaster>();
            if(greatblade!=null&&gc!=null)yield return new GreatbladeAbilityProvider(greatblade,gc);
        }
    }
}
