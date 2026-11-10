using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(PsionSkillCaster))]
    public sealed class PsionAbilityProvider : MonoBehaviour, IClassAbilityProvider
    {
        [SerializeField] private Texture2D[] icons;
        private PsionSkillProgression progression;
        private PsionSkillCaster caster;

        public string ClassId => PsionSkillProgression.PsionClassId;
        public int Count => PsionSkillProgression.SkillCount;
        public int UltimateIndex => -1;
        public int UnspentPoints => progression.UnspentPoints;

        private void Awake()
        {
            progression = GetComponent<PsionSkillProgression>();
            caster = GetComponent<PsionSkillCaster>();
        }

        public AbilityBarSource.SlotView View(int index, string key)
        {
            if ((uint)index >= Count) return default;
            var skill = (PsionSkill)index;
            int rank = progression.Rank(skill);
            return new AbilityBarSource.SlotView(PsionSkillProgression.Name(skill), key, rank,
                PsionSkillProgression.MaxRank(skill), caster.Remaining(skill), rank > 0, Describe(skill, rank),
                icons != null && index < icons.Length ? icons[index] : null, skill.ToString(),
                passive: skill == PsionSkill.PsionicBladesMastery, canUpgrade: progression.CanSpend(skill));
        }

        public bool Upgrade(int index) => (uint)index < Count && progression.Spend((PsionSkill)index);
        public string UpgradeTooltip(int index) => (uint)index < Count
            ? "Spend 1 skill point\n" + Describe((PsionSkill)index, progression.Rank((PsionSkill)index)) : "";

        public bool Activate(int index, SkillTargetingController targeting, IQueuedOrders orders, bool append)
        {
            if ((uint)index >= Count || targeting == null) return false;
            var skill = (PsionSkill)index;
            if (!caster.CanPrepare(skill)) return false;
            string title = PsionSkillProgression.Name(skill);
            switch (skill)
            {
                case PsionSkill.PsychicBolt:
                    targeting.BeginHostile(title, caster.CastBolt, PsionSkillCaster.BoltRange);
                    break;
                case PsionSkill.MentalImage:
                    targeting.BeginGround(title, caster.CastImage, PsionSkillCaster.ImageRange);
                    break;
                case PsionSkill.LightningStorm:
                    targeting.BeginGround(title, caster.CastStorm, PsionSkillCaster.StormRange);
                    break;
                case PsionSkill.MetalShield:
                    return targeting.RunSelfAction(caster.CastShield);
                default:
                    return false;
            }
            return true;
        }

        private string Describe(PsionSkill skill, int rank)
        {
            int preview = Mathf.Max(1, rank);
            string detail = skill switch
            {
                PsionSkill.PsychicBolt => $"Fire a Metal bolt for {caster.PreviewAmount(skill, preview):0.#} raw Magical damage. Stuns for {.15f*preview:0.00}s before resistance.",
                PsionSkill.PsionicBladesMastery => $"Basic attacks ignore {preview} Physical Defense and gain {2.5f*preview:0.#}% chance to crit for 1.5× damage.",
                PsionSkill.MentalImage => $"Place an image for {PsionSkillCaster.ImageDuration(preview):0.0}s. It strikes at 10% of your basic damage and bursts for {caster.PreviewAmount(skill, preview):0.#} raw Void damage on death or expiry.",
                PsionSkill.LightningStorm => $"Channel three waves. Each deals {caster.PreviewAmount(skill, preview):0.#} raw Lightning damage to enemies currently within 4m. Moving interrupts remaining waves.",
                _ => $"You and nearby allies share {caster.PreviewAmount(skill, preview):0.#} shield capacity for 8s. Applies to allies within 8m at cast."
            };
            if (skill != PsionSkill.PsionicBladesMastery)
                detail += $"\nMana {PsionSkillCaster.Cost(skill, preview)} · Cooldown {PsionSkillCaster.Cooldown(skill)}s";
            if (rank < PsionSkillProgression.MaxRank(skill))
                detail += $"\nNext rank requires level {PsionSkillProgression.RequiredLevel(skill, rank+1)}.";
            return detail;
        }

        public void Configure(Texture2D[] values) => icons = values;
    }
}