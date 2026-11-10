using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(AcolyteSkillCaster))]
    public sealed class AcolyteAbilityProvider : MonoBehaviour, IClassAbilityProvider
    {
        [SerializeField] private Texture2D drainIcon, coilIcon, curseIcon, plagueIcon, skeletonIcon, adeptIcon;
        private AcolyteSkillProgression progression;
        private AcolyteSkillCaster caster;
        public string ClassId => AcolyteSkillProgression.AcolyteClassId;
        public int Count => AcolyteSkillProgression.SkillCount;
        public int UltimateIndex => -1;
        public int UnspentPoints => progression.UnspentPoints;

        private void Awake()
        {
            progression = GetComponent<AcolyteSkillProgression>();
            caster = GetComponent<AcolyteSkillCaster>();
        }

        public AbilityBarSource.SlotView View(int index, string key)
        {
            if (index < 0 || index >= Count) return default;
            var skill = (AcolyteSkill)index;
            int rank = progression.Rank(skill);
            var icon = skill switch
            {
                AcolyteSkill.DrainLife => drainIcon, AcolyteSkill.MortalCoil => coilIcon,
                AcolyteSkill.WitheringCurse => curseIcon, AcolyteSkill.RadiationPlague => plagueIcon,
                AcolyteSkill.SummonSkeleton => skeletonIcon, _ => adeptIcon
            };
            return new AbilityBarSource.SlotView(AcolyteSkillProgression.Name(skill), key, rank,
                AcolyteSkillProgression.MaxRank(skill), caster.Remaining(skill), rank > 0, Describe(skill, rank),
                icon, skill.ToString(), passive: skill == AcolyteSkill.OccultAdept, canUpgrade: progression.CanSpend(skill));
        }

        public bool Upgrade(int index) => index >= 0 && index < Count && progression.Spend((AcolyteSkill)index);
        public string UpgradeTooltip(int index) => index >= 0 && index < Count
            ? "Spend 1 skill point\n" + Describe((AcolyteSkill)index, progression.Rank((AcolyteSkill)index)) : "";

        public bool Activate(int index, SkillTargetingController targeting, IQueuedOrders orders, bool append)
        {
            if (index < 0 || index >= Count || targeting == null) return false;
            var skill = (AcolyteSkill)index;
            if (!caster.CanPrepare(skill)) return false;
            if (skill == AcolyteSkill.SummonSkeleton)
                return targeting.RunSelfAction(() => caster.Cast(skill, null));
            string title = AcolyteSkillProgression.Name(skill);
            if (skill == AcolyteSkill.MortalCoil)
                targeting.BeginAnyUnit(title, target => caster.Cast(skill, target), AcolyteSkillCaster.Range);
            else
                targeting.BeginHostile(title, target => caster.Cast(skill, target), AcolyteSkillCaster.Range);
            return true;
        }

        private string Describe(AcolyteSkill skill, int rank)
        {
            int preview = Mathf.Max(1, rank);
            string detail = skill switch
            {
                AcolyteSkill.DrainLife => $"Channel for 5s, dealing {caster.PreviewDamage(skill, preview):0.#} raw Void damage each second. Each damaging or shielded tick grants 1 Soul Charge. Heal for {25 + 2 * (preview - 1)}% of HP damage dealt.",
                AcolyteSkill.MortalCoil => $"Channel for 1s. Deal {caster.PreviewDamage(skill, preview):0.#} raw Void damage to an enemy or heal yourself, an ally or a skeleton for {caster.PreviewCoilHealing(preview):0.#} before healing modifiers. Enemy HP damage earns 1 Soul Charge.",
                AcolyteSkill.WitheringCurse => $"Lower Magical Defense by {4 + preview} and attack rate by {.10f + .005f * (preview - 1):0.###} attacks/s for 10s. Tenacity reduces the effect.",
                AcolyteSkill.RadiationPlague => $"Deal {caster.PreviewDamage(skill, preview):0.#} raw Radiation damage every 0.1s for {3 + (preview - 1) / 3}s. Spreads after 2s within 3m, carrying the remaining duration. A marked enemy's death earns 1 Soul Charge.",
                AcolyteSkill.SummonSkeleton => $"Raise one skeleton for 15s. Its attacks deal {caster.PreviewDamage(skill, preview):0.#} raw Physical damage. Up to four can fight at once. Left-click a skeleton to select it, then right-click a destination or enemy.",
                _ => $"+{2 * preview} INT and +{preview} SPR. Landed wand attacks apply or refresh a 3s Magical Coffee poison for {caster.PreviewDamage(skill, preview):0.#} raw damage per second, without restarting its tick."
            };
            if (skill == AcolyteSkill.SummonSkeleton) detail += "\n4 Soul Charges · Cooldown 3s";
            else if (skill != AcolyteSkill.OccultAdept)
                detail += $"\nMana {AcolyteSkillCaster.ManaCost(skill)} · Cooldown {AcolyteSkillCaster.Cooldown(skill)}s";
            if (rank < AcolyteSkillProgression.MaxRank(skill))
                detail += $"\nNext rank requires level {AcolyteSkillProgression.RequiredLevel(skill, rank + 1)}.";
            return AcolyteSkillProgression.Name(skill) + "\n" + detail;
        }

        public void Configure(Texture2D drain, Texture2D coil) { drainIcon = drain; coilIcon = coil; }
        public void ConfigureComplete(Texture2D curse, Texture2D plague, Texture2D skeleton, Texture2D adept)
        { curseIcon = curse; plagueIcon = plague; skeletonIcon = skeleton; adeptIcon = adept; }
    }
}
