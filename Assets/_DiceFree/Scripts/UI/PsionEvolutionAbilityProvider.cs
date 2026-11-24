using DiceFree.Skills;
using UnityEngine;
namespace DiceFree.UI
{
    public abstract class PsionEvolutionAbilityProvider : MonoBehaviour,IClassAbilityProvider
    {
        [SerializeField] private Texture2D[] icons;
        private PsionEvolutionCaster caster;
        private PsionEvolutionProgression progression;
        protected abstract bool Knight {get;}
        public string ClassId=>Knight?PsionEvolutionProgression.KnightId:PsionEvolutionProgression.MageId;
        public int Count=>5;
        public int UltimateIndex=>-1;
        public int UnspentPoints=>progression.UnspentPoints;
        protected virtual void Awake(){caster=GetComponent<PsionEvolutionCaster>();progression=Knight?(PsionEvolutionProgression)GetComponent<PsionicKnightProgression>():GetComponent<MirageMageProgression>();}
        public AbilityBarSource.SlotView View(int i,string key)
        {
            if((uint)i>=5)return default;int rank=progression.Rank(i);
            return new AbilityBarSource.SlotView(PsionEvolutionProgression.Name(Knight,i),key,rank,PsionEvolutionProgression.MaxRank(Knight,i),caster.Remaining(i),rank>0,Describe(i,rank),icons!=null&&i<icons.Length?icons[i]:null,i.ToString(),passive:PsionEvolutionCaster.Passive(Knight,i),canUpgrade:progression.CanSpend(i));
        }
        public bool Upgrade(int i)=>progression.Spend(i);
        public string UpgradeTooltip(int i)=>(uint)i<5?"Spend 1 skill point\n"+Describe(i,progression.Rank(i)):"";
        public bool Activate(int i,SkillTargetingController targeting,IQueuedOrders orders,bool append)
        {
            if((uint)i>=5||targeting==null||!caster.CanPrepare(i))return false;
            string title=PsionEvolutionProgression.Name(Knight,i);
            if(i==1)targeting.BeginGround(title,caster.CastImage,6);
            else if(!Knight&&i==0)targeting.BeginHostile(title,caster.CastBolt,7);
            else if(!Knight&&i==2)targeting.BeginGround(title,caster.CastStorm,9);
            else return targeting.RunSelfAction(()=>caster.CastSelf(i));
            return true;
        }
        public string Describe(int i,int actualRank)
        {
            int r=Mathf.Max(1,actualRank);float raw=caster.Preview(i,r);string name=PsionEvolutionProgression.Name(Knight,i);
            string text=name+"\n";
            if(i==4)text+=$"Personal attack/movement speed +{r}%. Allies within 12m gain +{r*.1f:0.#}% each, including you. Strongest Speed Presence aura only.\nScaling: +1% personal and +0.1% allied speed per rank. Illusions receive but do not emit the aura.";
            else if(Knight)text+=i switch
            {
                0=>$"Basic attacks gain {r}% critical chance, {(1.6f+.01f*r)*100:0}% critical damage and ignore {r} flat Physical Defense.\nScaling: +1 percentage point crit chance/damage and +1 PDEF bypass per rank. Magical Void bonus cannot crit.",
                1=>$"Summon a mobile partner for {PsionEvolutionCaster.ImageDuration(r):0.##}s. Follows you and attacks your target or nearby enemies.\nBasic hit: {SkillTooltips.Range(GetComponent<DiceFree.Combat.BasicAttack>().DamageRange*PsionEvolutionCaster.ImageFraction(r))} raw Physical ({100*PsionEvolutionCaster.ImageFraction(r):0.##}% of your basic attack).\nFull compatible passives; temporary buff potency copied once at the same percentage. On expiry/destruction: +{Mathf.Lerp(1,50,(r-1)/24f):0.##}% attack speed for {Mathf.Lerp(1,5,(r-1)/24f):0.##}s; strongest only. No explosion. Buff lost if you are dead.",
                2=>$"Protect yourself with {raw:0.##} Void shield HP for 6s.\nScaling: 180 + 48×rank + (0.50 + 0.03×rank)×AGI + (0.85 + 0.055×rank)×INT. Matching Void damage consumes 0.8 shield HP; other damage 1.2 per damage blocked.",
                _=>$"Empower the next {1+r/3} landed basic attacks for 12s. Each adds {raw:0.##} raw Magical Void damage and costs {4+2*r} Mana.\nScaling: 30 + 17×rank + [0.4+0.016×(rank−1)]×AGI + [0.1+0.004×(rank−1)]×INT.\nMisses cost nothing. Shielded hits consume a charge and Mana. Insufficient Mana: normal Physical hit only, preserving charges. Bonus cannot crit. Recast replaces charges. Remaining: {caster.Charges}."
            };
            else text+=i switch
            {
                0=>$"Deal {raw:0.##} raw Magical Metal damage; stun {Mathf.Min(3,.15f*r):0.##}s before Tenacity.\nScaling: 45 + 11×rank + (1.30 + 0.10×rank)×INT. Mirrored bolts have no stun.",
                1=>$"Place a stationary image for {PsionEvolutionCaster.ImageDuration(r):0.##}s. Basic hits deal 10% of yours. Death/expiry explodes for {raw:0.##} raw Magical Void damage within 4m.\nExplosion: 55 + 15×rank + (0.65 + 0.04×rank)×AGI + (0.80 + 0.07×rank)×INT.\nMirrors spells after 0.4s at {PsionEvolutionCaster.MirrorFraction(r)*100:0.##}% damage/shield capacity; no extra Mana or copied control effects. Scaling: 5% + 20%×(rank−1)/24. Mirrored image has half HP/basic/lifetime, full explosion and cannot mirror again.",
                2=>$"Channel three 4m Lightning waves. Each deals {raw:0.##} raw Magical damage; full storm {3*raw:0.##} per target. Moving or stun interrupts remaining waves.\nPer wave: 25 + 7×rank + (0.70 + 0.07×rank)×INT.",
                _=>$"Allies within 10m share {raw:0.##} Metal shield HP for 8s. Protection remains after leaving.\nScaling: 160 + 42×rank + (0.85 + 0.055×rank)×INT. Illusion casts an independent weaker shared shield within 8m. Oldest eligible pool absorbs first."
            };
            if(!PsionEvolutionCaster.Passive(Knight,i))text+=$"\nMana {PsionEvolutionCaster.Cost(Knight,i,r)} · Cooldown {PsionEvolutionCaster.Cooldown(Knight,i):0.#}s";
            if(actualRank<PsionEvolutionProgression.MaxRank(Knight,i))text+=$"\nNext rank requires class level {PsionEvolutionProgression.RequiredLevel(Knight,i,actualRank+1)}.";
            if(!string.IsNullOrEmpty(caster.Feedback))text+="\n"+caster.Feedback;
            return text;
        }
        public void Configure(Texture2D[] values)=>icons=values;
    }
}
