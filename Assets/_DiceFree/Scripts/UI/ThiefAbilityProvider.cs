using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(ThiefSkillCaster))]
    public sealed class ThiefAbilityProvider : MonoBehaviour, IClassAbilityProvider
    {
        [SerializeField] private Texture2D[] icons;
        private ThiefSkillProgression progression;
        private ThiefSkillCaster caster;

        public string ClassId => ThiefSkillProgression.ThiefClassId;
        public int Count => ThiefSkillProgression.SkillCount;
        public int UltimateIndex => -1;
        public int UnspentPoints => progression.UnspentPoints;

        private void Awake()
        {
            progression = GetComponent<ThiefSkillProgression>();
            caster = GetComponent<ThiefSkillCaster>();
        }

        public AbilityBarSource.SlotView View(int index,string key)
        {
            if ((uint)index>=Count) return default;
            var skill = (ThiefSkill)index;
            int rank = progression.Rank(skill);
            string tooltip = Describe(skill,rank);
            if (skill==ThiefSkill.Stealth && caster.Hidden) tooltip += $"\nHidden for {caster.StealthRemaining:0.0}s";
            if (skill==ThiefSkill.KeenEdge && caster.KeenEdgeRemaining>0) tooltip += $"\nActive for {caster.KeenEdgeRemaining:0.0}s";
            return new AbilityBarSource.SlotView(ThiefSkillProgression.Name(skill),key,rank,
                ThiefSkillProgression.MaxRank(skill),caster.Remaining(skill),rank>0,tooltip,
                icons!=null&&index<icons.Length?icons[index]:null,skill.ToString(),
                passive:skill==ThiefSkill.Cutpurse,canUpgrade:progression.CanSpend(skill));
        }

        public bool Upgrade(int index) => (uint)index<Count && progression.Spend((ThiefSkill)index);
        public string UpgradeTooltip(int index) => (uint)index<Count ?
            "Spend 1 skill point\n"+Describe((ThiefSkill)index,progression.Rank((ThiefSkill)index)) : "";

        public bool Activate(int index,SkillTargetingController targeting,IQueuedOrders orders,bool append)
        {
            if ((uint)index>=Count || targeting==null) return false;
            var skill=(ThiefSkill)index;
            if (!caster.CanPrepare(skill)) return false;
            string name=ThiefSkillProgression.Name(skill);
            if (skill==ThiefSkill.Envenom)
                targeting.BeginHostile(name,target=>caster.Cast(skill,target),ThiefSkillCaster.EnvenomRange);
            else if (skill==ThiefSkill.SmokeBomb)
                targeting.BeginGround(name,caster.CastSmoke,ThiefSkillCaster.SmokeRange);
            else return targeting.RunSelfAction(()=>caster.Cast(skill));
            return true;
        }

        private string Describe(ThiefSkill skill,int rank)
        {
            int preview=Mathf.Max(1,rank);
            var stab = skill == ThiefSkill.Envenom ? caster.PreviewStab() : Vector2.zero;
            string detail=skill switch
            {
                ThiefSkill.Stealth => $"Hide for {ThiefSkillCaster.StealthDuration(preview):0.0}s and drop 95% of enemy threat. The first basic attack or Envenom stab reveals you and deals {ThiefSkillCaster.StealthOpener(preview):0.00}× raw damage.",
                ThiefSkill.Envenom => $"Stab for {stab.x:0.#}–{stab.y:0.#} raw Physical damage and apply a three-second poison for {caster.PreviewDamage(skill, preview):0.#} raw Nature damage per tick. Only the stab gets the Stealth bonus.",
                ThiefSkill.KeenEdge => $"Guarantee 1.5× critical basic attacks for {ThiefSkillCaster.KeenDuration(preview):0.0}s. Can be used while hidden.",
                ThiefSkill.FanOfKnives => $"Deal {caster.PreviewDamage(skill, preview):0.#} raw Physical damage to each foe within 4m. Can be cast while moving. Reveals you without an opener bonus.",
                ThiefSkill.SmokeBomb => $"Throw a 4m cloud for 5s. Enemies inside take {caster.PreviewDamage(skill, preview):0.#} raw Tech damage per tick and have a 20% basic-attack miss chance. Overlapping clouds do not stack.",
                _ => $"+{preview} AGI and +{preview}% basic critical chance. Critical basic attacks deal 1.5× raw damage."
            };
            if (skill!=ThiefSkill.Cutpurse) detail += $"\nMana {ThiefSkillCaster.Cost(skill)} · Cooldown {ThiefSkillCaster.Cooldown(skill)}s";
            if (rank<ThiefSkillProgression.MaxRank(skill)) detail += $"\nNext rank requires level {ThiefSkillProgression.RequiredLevel(skill,rank+1)}.";
            return detail;
        }

        public void Configure(Texture2D[] values) => icons=values;
    }
}
