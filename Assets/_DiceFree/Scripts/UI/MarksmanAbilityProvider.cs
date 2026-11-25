using DiceFree.Skills;
using UnityEngine;
namespace DiceFree.UI
{
    [RequireComponent(typeof(MarksmanCaster))]
    public sealed class MarksmanAbilityProvider : MonoBehaviour,IClassAbilityProvider
    {
        [SerializeField]Texture2D[] icons;MarksmanCaster caster;MarksmanProgression ranks;
        public string ClassId=>MarksmanProgression.Id;
        public int Count=>6;
        public int UltimateIndex=>-1;
        public int UnspentPoints=>ranks.UnspentPoints;
        void Awake(){caster=GetComponent<MarksmanCaster>();ranks=GetComponent<MarksmanProgression>();}
        public AbilityBarSource.SlotView View(int i,string key)
        {if((uint)i>=6)return default;int r=ranks.Rank(i);return new AbilityBarSource.SlotView(MarksmanProgression.Name(i),key,r,MarksmanProgression.MaxRank(i),caster.Remaining(i),r>0,Describe(i,r),icons!=null&&i<icons.Length?icons[i]:null,"marksman-"+i,passive:i==2||i==4,canUpgrade:ranks.CanSpend(i));}
        public bool Upgrade(int i)=>(uint)i<6&&ranks.Spend(i);
        public string UpgradeTooltip(int i)=>(uint)i<6?"Spend 1 skill point\n"+Describe(i,ranks.Rank(i)):"";
        public bool Activate(int i,SkillTargetingController targeting,IQueuedOrders orders,bool append)
        {
            if((uint)i>=6||targeting==null||!caster.CanPrepare(i))return false;
            if(i==3)return targeting.RunSelfAction(caster.CastStrafe);
            if(i==0)targeting.BeginHostile(MarksmanProgression.Name(i),caster.CastAimed,caster.Range(i));
            else targeting.BeginGround(MarksmanProgression.Name(i),i==1?caster.CastRain:caster.CastPower,caster.Range(i));return true;
        }
        public string Describe(int i,int rank)
        {
            int r=Mathf.Max(1,rank);string detail=i switch{
                0=>$"A guaranteed physical shot after a 1.5s stationary windup. Ignores {10f*r/30:0.##}% PDEF for this shot. Bow Mastery crit applies at 1.6×; no mark required.",
                1=>"Channel for 1.5s. Six noncritical physical waves then fall in a 3m circle over 2s, persisting while you move or are stunned. Moving/stun before release cancels the channel.",
                2=>$"+{r*3} AGI, +{r*.2f:0.0}m basic/Aimed range, +{r*1.5f:0.#} percentage points of basic/Aimed crit chance. Critical damage 1.6×. Reach caps at 15m.",
                3=>$"+{r*2}% attack speed and −{r*2}% movement speed for {Mathf.Ceil(1+r/5f):0}s. Activation cost only. Arrows still require stationary windups.",
                4=>$"Permanent +{r}% allied ranged BASIC raw damage, including yourself. Radius {caster.Range(0):0.0}m follows Bow Mastery. Strongest same-name aura only; no skill damage.",
                _=>$"Fire a noncritical physical arrow through enemies in a 1.5m-wide line, up to {caster.Range(5):0.0}m. Stops at solid walls. Each successive distinct enemy takes 80% of the previous raw damage, before its own defense."
            };
            if(i==0||i==1||i==5){var f=MarksmanCaster.Formula(i,r);float raw=caster.PreviewDamage(i,r);detail+=$"\nScaling: {f.x:0.###} + {f.y:0.###} × current AGI.\nCurrent total: {raw:0.##} raw"+(i==1?$" per wave; {raw*6:0.##} across six waves.":" on the first target.");}
            if(i!=2&&i!=4)detail+=$"\nMana {MarksmanCaster.Cost(i,r)} · Cooldown {MarksmanCaster.Cooldown(i)}s";
            if(rank<MarksmanProgression.MaxRank(i))detail+=$"\nNext rank requires Marksman level {MarksmanProgression.RequiredLevel(i,rank+1)}.";return detail;
        }
        public void Configure(Texture2D[] values)=>icons=values;
    }
}
