using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PriestSkillCaster))]
    public sealed class PriestAbilityProvider : MonoBehaviour, IClassAbilityProvider
    {
        private PriestSkillProgression skills;
        private PriestSkillCaster caster;
        public string ClassId => PriestSkillProgression.PriestClassId;
        public int Count => 2;
        public int UltimateIndex => -1;
        public int UnspentPoints => skills.UnspentPoints;

        private void Awake()
        {
            skills = GetComponent<PriestSkillProgression>();
            caster = GetComponent<PriestSkillCaster>();
        }

        public AbilityBarSource.SlotView View(int index, string key)
        {
            if (index < 0 || index >= Count) return default;
            var skill = (PriestSkill)index;
            int rank = skills.Rank(skill);
            bool isGrace = skill == PriestSkill.Grace;
            string title = isGrace ? (caster.GraceArmed ? "Grace Armed" : "Grace") : "Holy Light";
            string icon = isGrace ? "guard" : "mend";
            return new AbilityBarSource.SlotView(title, key, rank, PriestSkillProgression.MaxRank(skill),
                isGrace ? 0 : caster.CooldownRemaining, rank > 0, Describe(skill, rank),
                SkillIconLibrary.Get(icon), icon, canUpgrade: skills.CanSpend(skill));
        }

        public bool Upgrade(int index) => index >= 0 && index < Count && skills.Spend((PriestSkill)index);

        public string UpgradeTooltip(int index) => index >= 0 && index < Count
            ? "Spend 1 skill point\n" + Describe((PriestSkill)index, skills.Rank((PriestSkill)index) + 1) : "";

        public bool Activate(int index, SkillTargetingController targeting, IQueuedOrders orders, bool append)
        {
            if (targeting == null || index < 0 || index >= Count) return false;
            if ((PriestSkill)index == PriestSkill.Grace)
            {
                if (!caster.GraceFull) return false;
                targeting.Cancel();
                if (!append) orders?.ClearOrders();
                return append && orders != null ? orders.SubmitAction(caster.ArmGrace, true) : caster.ArmGrace();
            }

            if (!caster.CanPrepareHolyLight()) return false;
            targeting.BeginAnyUnit("Holy Light", caster.CastHolyLight, PriestSkillCaster.Range);
            return true;
        }

        private string Describe(PriestSkill skill, int rank)
        {
            int previewRank = Mathf.Max(1, rank);
            string text = skill == PriestSkill.Grace
                ? $"Grace {caster.Grace:0}/{caster.GraceMaximum:0}\nArm at full to empower the next successful spell. A heal with zero overheal earns 2. Other successful spells earn 1.\nRank {previewRank} capacity: {11 - previewRank}."
                : $"Holy Light heals a living ally or deals Light damage to an enemy.\nBase effect {caster.BaseAmount(previewRank):0.#} before healing or damage modifiers.\nMana {PriestSkillCaster.ManaCost(previewRank):0}, cooldown {PriestSkillCaster.Cooldown(previewRank):0.#}s, range {PriestSkillCaster.Range:0}m.\nGrace adds up to {previewRank} bounces. Each hop retains 90% potency and prioritizes wounded allies.";
            if (rank < PriestSkillProgression.MaxRank(skill))
                text += $"\nNext rank requires level {PriestSkillProgression.RequiredLevel(skill, rank + 1)} and 1 skill point.";
            return text;
        }
    }
}
