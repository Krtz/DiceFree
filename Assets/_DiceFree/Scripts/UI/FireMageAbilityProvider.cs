using DiceFree.Combat;
using DiceFree.Skills;
using UnityEngine;
namespace DiceFree.UI
{
    [RequireComponent(typeof(FireMageCaster))]
    public sealed class FireMageAbilityProvider:MonoBehaviour,IClassAbilityProvider
    {
        [SerializeField]private Texture2D[] icons;
        private FireMageCaster caster;private FireMageProgression ranks;
        public string ClassId=>FireMageProgression.Id;
        public int Count=>6;
        public int UltimateIndex=>-1;
        public int UnspentPoints=>ranks.UnspentPoints;
        private void Awake(){caster=GetComponent<FireMageCaster>();ranks=GetComponent<FireMageProgression>();}
        public AbilityBarSource.SlotView View(int i,string key)=>(uint)i<6?new(FireMageProgression.Name(i),key,ranks.Rank(i),FireMageProgression.MaxRank(i),caster.Remaining(i),ranks.Rank(i)>0,Describe(i,ranks.Rank(i)),icons!=null&&i<icons.Length?icons[i]:null,"fire-mage-"+i,passive:i>=4,canUpgrade:ranks.CanSpend(i)):default;
        public bool Upgrade(int i)=>ranks.Spend(i);
        public string UpgradeTooltip(int i)=>(uint)i<6?"Spend 1 skill point\n"+Describe(i,ranks.Rank(i)):"";
        public bool Activate(int i,SkillTargetingController targeting,IQueuedOrders orders,bool append)
        {
            if(targeting==null||!caster.CanPrepare(i))return false;
            if(i==1)return targeting.RunSelfAction(()=>caster.Cast(i));
            targeting.BeginHostile(FireMageProgression.Name(i),target=>caster.Cast(i,target),caster.Range);return true;
        }
        public string Describe(int i,int rank)
        {
            if((uint)i>=6)return "";int r=Mathf.Max(1,rank);string text=FireMageProgression.Name(i)+"\n";
            text+=i switch{
                0=>"Single-target Fire projectile, 0.2s cast. Empowered: leaves a 4m burning field for six seconds, six ticks at 25% of this hit each. Fields overlap independently.",
                1=>$"Instant self ward for {FireMageCaster.WardDuration(r):0.##}s. Redirects {FireMageCaster.WardFraction(r)*100:0.##}% post-shield damage to Mana at {FireMageCaster.WardEfficiency(r):0.##} Mana/HP prevented. Empowered: adds a 10s Fire shield. Shield scaling: 50 + {(.5f+.1f*r):0.##} × INT; current total {FireMageCaster.ShieldAmount(r,GetComponent<ActorStats>().Attributes.intelligence):0.##} shield HP. Shield absorbs before Mana redirection; no expiry healing.",
                2=>"Instant Ignite: eight Fire ticks over eight seconds. Empowered: 3× tick damage. Recast refreshes duration and keeps the stronger snapshotted tick; distinct casters coexist.",
                3=>"0.2s cast. Living Bomb explodes after 3s in a 3m radius. Empowered: 2× initial explosion and attaches one generation of normal bombs to nearby enemies. Independent bombs stack; carrier death detonates every attached bomb immediately.",
                4=>$"Successful ordinary Fire spells generate 10 + rank Heat ({10+r}). At 100 Heat the next spell consumes 100 and is empowered, with no same-cast refill. Heat decays 10/s after 10s without a successful Fire cast.",
                _=>$"Always-on 15m aura, including you: allied Fire resistance +0.5 percentage point × rank (+{.5f*r:0.#}pp), enemy Fire resistance −{.5f*r:0.#}pp. Strongest identical aura only."
            };
            if(i==0||i==2||i==3){var f=FireMageCaster.Formula(i,r);float raw=caster.PreviewDamage(i,r);text+=$"\nScaling: {f.x:0.###} + {f.y:0.###} × INT. Current total: {raw:0.##} raw Fire damage"+(i==2?$" per tick; {raw*8:0.##} over 8 ticks.":" per target.")+" No innate critical hits. Offense snapshots; target mitigation applies at impact.";}
            if(i<4)text+=$"\nMana: {FireMageCaster.ManaCost(i,r)}. Cooldown: {FireMageCaster.Cooldown(i,r):0.##}s. At full Heat: same Mana + 100 Heat.";
            if(rank<FireMageProgression.MaxRank(i))text+=$"\nNext rank requires Fire Mage level {FireMageProgression.RequiredLevel(i,rank+1)}.";
            return text;
        }
        public void Configure(Texture2D[] values)=>icons=values;
    }
}
