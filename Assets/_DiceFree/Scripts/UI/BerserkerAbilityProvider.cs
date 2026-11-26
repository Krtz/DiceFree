using DiceFree.Skills;
using UnityEngine;
namespace DiceFree.UI
{
    [RequireComponent(typeof(BerserkerCaster))]
    public sealed class BerserkerAbilityProvider : MonoBehaviour,IClassAbilityProvider
    {
        [SerializeField] private Texture2D[] icons;
        private BerserkerCaster caster;
        private BerserkerProgression ranks;
        public string ClassId=>BerserkerProgression.Id;
        public int Count=>5;
        public int UltimateIndex=>-1;
        public int UnspentPoints=>ranks.UnspentPoints;
        private void Awake(){caster=GetComponent<BerserkerCaster>();ranks=GetComponent<BerserkerProgression>();}
        public AbilityBarSource.SlotView View(int i,string key)
        {
            if((uint)i>=5)return default;
            int r=ranks.Rank(i);
            return new(BerserkerProgression.Name(i),key,r,BerserkerProgression.MaxRank(i),caster.Remaining(i),r>0,
                Describe(i,r),icons!=null&&i<icons.Length?icons[i]:null,"berserker-"+i,passive:i==3,canUpgrade:ranks.CanSpend(i));
        }
        public bool Upgrade(int i)=>(uint)i<5&&ranks.Spend(i);
        public string UpgradeTooltip(int i)=>(uint)i<5?"Spend 1 skill point\n"+Describe(i,ranks.Rank(i)):"";
        public bool Activate(int i,SkillTargetingController targeting,IQueuedOrders orders,bool append)
        {
            if((uint)i>=5||targeting==null||!caster.CanPrepare(i))return false;
            if(i==4)return targeting.RunSelfAction(caster.CastCoffeeShield);
            if(i==2)targeting.BeginHostile(BerserkerProgression.Name(i),caster.CastRageBreaker,caster.Range(i));
            else targeting.BeginGround(BerserkerProgression.Name(i),i==0?caster.CastWideArc:caster.CastSpinDash,caster.Range(i));
            return true;
        }
        public string Describe(int i,int rank)
        {
            if((uint)i>=5)return "";
            int r=Mathf.Max(1,rank);
            string text=BerserkerProgression.Name(i)+"\n";
            text+=i switch {
                0=>"A 110° physical cleave within 3m after a 0.35s windup. Ordinary accuracy; never crits.",
                1=>"Spin up to 12m along a reachable route, striking each enemy once within 1.8m. Never crits. Gain 2 Rage per unique enemy hit, maximum 20 Rage per activation.",
                2=>$"A guaranteed physical hit within 1.2m after a 0.55s windup. Can crit only from external sources. A successful hit, including shield absorption, grants +{2*r}% attack speed for 4s; refreshes, never stacks.",
                3=>$"Successful basic hits grant {BerserkerCaster.RagePerBasic(r):0} Rage (1 + ceil(0.1 × rank)), including enemy shields. +{.5f*r:0.#}% physical basic/skill damage with eligible two-handed axes or swords. Missing-HP bonus rises smoothly from zero at full HP to +{40f*r/25:0.#}% at 1% HP; multiply both bonuses once. Out-of-combat Rage decay: {BerserkerCaster.DecayPerSecond(r):0.###}/s. No innate crit chance.",
                _=>$"Self-only Coffee elemental shield for 5s. Scaling: {BerserkerCaster.ShieldFraction(r)*100:0.##}% × maximum HP. Current total: {caster.PreviewShield(r):0.##} shield HP. Uses ordinary elemental shield matching rules. Expiry does not heal."
            };
            if(i<3)
            {
                var f=BerserkerCaster.Formula(i,r);
                text+=$"\nScaling: {f.x:0.###} + {f.y:0.###} × current STR.\nCurrent total: {caster.PreviewDamage(i,r):0.##} raw physical damage per target, including current Anger Management bonuses.";
            }
            if(i!=3)text+=$"\nRage {BerserkerCaster.Cost(i)} · Cooldown {BerserkerCaster.Cooldown(i):0}s after the cast/attack finishes.";
            if(rank<BerserkerProgression.MaxRank(i))text+=$"\nNext rank requires Berserker level {BerserkerProgression.RequiredLevel(i,rank+1)}.";
            return text;
        }
        public void Configure(Texture2D[] values)=>icons=values;
    }
}
