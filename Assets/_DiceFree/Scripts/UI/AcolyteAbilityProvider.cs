using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(AcolyteSkillCaster))]
    public sealed class AcolyteAbilityProvider : MonoBehaviour, IClassAbilityProvider
    {
        [SerializeField] private Texture2D drainIcon, coilIcon;
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
            return new AbilityBarSource.SlotView(AcolyteSkillProgression.Name(skill), key, rank,
                AcolyteSkillProgression.MaxRank(skill), caster.Remaining(skill), rank > 0, Describe(skill, rank),
                skill == AcolyteSkill.DrainLife ? drainIcon : coilIcon, skill.ToString(), canUpgrade: progression.CanSpend(skill));
        }

        public bool Upgrade(int index) => index >= 0 && index < Count && progression.Spend((AcolyteSkill)index);
        public string UpgradeTooltip(int index) => index >= 0 && index < Count
            ? "Spend 1 skill point\n" + Describe((AcolyteSkill)index, progression.Rank((AcolyteSkill)index)) : "";

        public bool Activate(int index, SkillTargetingController targeting, IQueuedOrders orders, bool append)
        {
            if (index < 0 || index >= Count || targeting == null) return false;
            var skill = (AcolyteSkill)index;
            if (!caster.CanPrepare(skill)) return false;
            string title = AcolyteSkillProgression.Name(skill);
            if (skill == AcolyteSkill.DrainLife)
                targeting.BeginHostile(title, target => caster.Cast(skill, target), AcolyteSkillCaster.Range);
            else
                targeting.BeginAnyUnit(title, target => caster.Cast(skill, target), AcolyteSkillCaster.Range);
            return true;
        }

        private static string Describe(AcolyteSkill skill, int rank)
        {
            int preview = Mathf.Max(1, rank);
            string detail = skill == AcolyteSkill.DrainLife
                ? $"Channel for 5s, dealing Void damage each second. Each damaging or shielded tick grants 1 Soul Charge. Heal for {25 + 2 * (preview - 1)}% of HP damage dealt."
                : "Channel for 1s. Deal Void damage to an enemy or heal yourself or an ally with Void energy. Enemy damage earns 1 Soul Charge.";
            detail += $"\nMana {AcolyteSkillCaster.ManaCost(skill)} · Cooldown {AcolyteSkillCaster.Cooldown(skill)}s";
            if (rank < AcolyteSkillProgression.MaxRank(skill))
                detail += $"\nNext rank requires level {AcolyteSkillProgression.RequiredLevel(skill, rank + 1)}.";
            return detail;
        }

        public void Configure(Texture2D drain, Texture2D coil) { drainIcon = drain; coilIcon = coil; }
    }
}
