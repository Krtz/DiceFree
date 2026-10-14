using DiceFree.Combat;
using DiceFree.Progression;
using DiceFree.Skills;
using UnityEngine;
using Kind=DiceFree.Progression.GreatbladeProgressionRules.Kind;
namespace DiceFree.UI
{
    public static class GreatbladeAbilityViews
    {
        public static string Name(Kind kind)=>kind switch{
            Kind.WideArc=>"Wide Arc",Kind.SpinDash=>"Spin Dash",Kind.OverheadBreaker=>"Overhead Breaker",
            Kind.BattleCry=>"Battle Cry",Kind.GreatswordMastery=>"Greatsword Mastery",_=>"Heavy Rhythm"};
        public static AbilityBarSource.SlotView View(GreatbladeSkillProgression skills,GreatbladeSkillCaster caster,int slot,string key)
        {
            if(skills==null||caster==null||slot<0||slot>=GreatbladeProgressionRules.Skills.Length)return default;
            var kind=(Kind)slot;var rule=GreatbladeProgressionRules.For(kind);int rank=skills.Rank(kind);
            string icon="greatblade-"+kind.ToString().ToLowerInvariant();
            return new AbilityBarSource.SlotView(Name(kind),key,rank,rule.rankLevels.Length,caster.CooldownRemaining(kind),
                rank>0,Describe(skills,caster,kind,rank),SkillIconLibrary.Get(icon),icon,rule.passive,skills.CanSpend(kind));
        }
        public static string Upgrade(GreatbladeSkillProgression skills,GreatbladeSkillCaster caster,int slot)=>
            "Spend 1 skill point\n"+Describe(skills,caster,(Kind)slot,skills.Rank((Kind)slot)+1);
        public static string Describe(GreatbladeSkillProgression skills,GreatbladeSkillCaster caster,Kind kind,int rank)
        {
            var t=caster.Tuning;var rule=GreatbladeProgressionRules.For(kind);int r=Mathf.Max(1,rank);
            string text=Name(kind)+" — "+(rule.passive?"Passive":"Active")+"\n"+(rank>0?"Rank "+rank:"Unlock level "+rule.rankLevels[0])+"\n";
            if(t==null)return text;
            int mastery=skills.Rank(Kind.GreatswordMastery);
            text+=kind switch{
                Kind.WideArc=>$"Frontal cleave: {t.ArcCoefficient(r):0.##} × STR physical damage (+ mastery/buffs).\nReach {t.arcReach:0.#}m beyond unit radii; angle {t.arcAngle+(mastery>=3?15+3*(mastery-3):0):0}°.\nWind-up {t.arcWindup:0.##}s. Connecting hits generate Momentum; capped per cast.",
                Kind.SpinDash=>$"Ground-target spinning dash, up to {t.spinRange:0.#}m.\n{t.SpinCoefficient(r):0.##} × STR physical damage (+ mastery/buffs), once per enemy along the actual route.\nCollision-safe: walls and living units block movement. Generates Momentum.",
                Kind.OverheadBreaker=>$"Single-target finisher: {t.BreakerCoefficient(r):0.##} × STR physical damage (+ mastery/buffs).\nConsumes ALL Momentum, requires {t.breakerMinimumMomentum:0}.\nEach Momentum adds 1.5% damage (100 = +150%).\nWind-up {t.breakerWindup:0.##}s; interrupted or escaped strikes do not refund.",
                Kind.BattleCry=>$"Self offensive buff: +{t.cryDamagePercent+3*(r-1):0}% sword/skill damage for {t.cryDuration:0.#}s. No Momentum cost.",
                Kind.GreatswordMastery=>$"+{GreatbladeProgressionRules.DamageBonusPct(r):0}% greatsword damage; +{GreatbladeProgressionRules.StrengthBonus(r):0} STR."+
                    (r>=2?$"\nWide Arc grants +{8+2*(r-2):0}% attack speed for {3+.25f*(r-2):0.##}s.":"\nRank 2 adds brief attack speed after Wide Arc.")+
                    (r>=3?$"\nWide Arc widens by {15+3*(r-3):0}°.":"\nRank 3 widens Wide Arc.")+
                    (r>=4?$"\nConnecting Breaker grants {5+.5f*(r-4):0.#}% max-HP shield for 5s (provisional).":"\nRank 4 adds a small finisher shield (provisional)."),
                _=>$"Momentum gained on hits: ×{GreatbladeProgressionRules.MomentumGainFactor(r):0.##}.\nOut-of-combat decay: {t.decayPerSecond*GreatbladeProgressionRules.MomentumDecayFactor(r):0.##}/s after {t.decayDelay:0.#}s without a hit.\n100 cap; no Momentum threshold buffs."
            };
            if(!rule.passive)text+="\nCooldown "+caster.Cooldown(kind).ToString("0.#")+"s.";
            int next=rule.NextRankLevel(rank);
            if(next<int.MaxValue)text+="\nNext rank requires class level "+next+" and 1 skill point.";
            return text;
        }
        public static bool Activate(GreatbladeSkillProgression skills,GreatbladeSkillCaster caster,SkillTargetingController targeting,IQueuedOrders orders,int slot,bool append)
        {
            if(slot<0||slot>=GreatbladeProgressionRules.Skills.Length||skills==null||caster==null||targeting==null||GreatbladeProgressionRules.For((Kind)slot).passive)return false;
            var kind=(Kind)slot;if(!caster.CanPrepare(kind))return false;
            switch(kind)
            {
                case Kind.WideArc:targeting.BeginGround(Name(kind),caster.CastWideArc,caster.Tuning.arcReach);return true;
                case Kind.SpinDash:targeting.BeginGround(Name(kind),caster.CastSpinDash,caster.Tuning.spinRange);return true;
                case Kind.OverheadBreaker:targeting.BeginHostile(Name(kind),caster.CastOverheadBreaker,caster.Tuning.breakerReach);return true;
                case Kind.BattleCry:
                    if(append&&orders!=null)return orders.SubmitAction(caster.CastBattleCry,true);
                    orders?.ClearOrders();targeting.Cancel();return caster.CastBattleCry();
                default:return false;
            }
        }
    }
}
