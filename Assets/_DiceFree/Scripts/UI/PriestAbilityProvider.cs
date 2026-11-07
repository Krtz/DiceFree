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
        public int Count => PriestSkillProgression.SkillCount;
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
            string title = skill == PriestSkill.Grace && caster.GraceArmed ? "Grace Armed" : PriestSpellRules.Name(skill);
            string icon = skill switch
            {
                PriestSkill.Grace => "guard",
                PriestSkill.LightBurst => "priest-burst",
                PriestSkill.IceAegis => "priest-aegis",
                PriestSkill.WildGrowth => "priest-growth",
                PriestSkill.Revive => "priest-revive",
                _ => "mend"
            };
            return new AbilityBarSource.SlotView(title, key, rank, PriestSkillProgression.MaxRank(skill),
                caster.Remaining(skill), rank > 0, Describe(skill, rank),
                SkillIconLibrary.Get(icon), icon, canUpgrade: skills.CanSpend(skill));
        }

        public bool Upgrade(int index) => index >= 0 && index < Count && skills.Spend((PriestSkill)index);

        public string UpgradeTooltip(int index) => index >= 0 && index < Count
            ? "Spend 1 skill point\n" + Describe((PriestSkill)index, skills.Rank((PriestSkill)index) + 1) : "";

        public bool Activate(int index, SkillTargetingController targeting, IQueuedOrders orders, bool append)
        {
            if (targeting == null || index < 0 || index >= Count) return false;
            var skill = (PriestSkill)index;
            if (skill == PriestSkill.Grace)
            {
                if (!caster.GraceFull) return false;
                targeting.Cancel();
                if (!append) orders?.ClearOrders();
                return append && orders != null ? orders.SubmitAction(caster.ArmGrace, true) : caster.ArmGrace();
            }

            if (!caster.CanPrepare(skill)) return false;
            string title = PriestSpellRules.Name(skill);
            float range = PriestSpellRules.Range(skill);
            if (skill == PriestSkill.IceAegis)
                targeting.BeginFriendly(title, target => caster.Cast(skill, target), range);
            else if (skill == PriestSkill.Revive)
                targeting.BeginCorpse(title, target => caster.Cast(skill, target), caster.IsValidCorpse, range);
            else
                targeting.BeginAnyUnit(title, target => caster.Cast(skill, target), range);
            return true;
        }

        private string Describe(PriestSkill skill, int rank)
        {
            int preview = Mathf.Max(1, rank);
            string text = skill switch
            {
                PriestSkill.Grace =>
                    $"Grace {caster.Grace:0}/{caster.GraceMaximum:0}\nArm at full to empower your next successful spell. A heal with zero overheal earns 2 Grace. Other successful spells earn 1.\nRank {preview} capacity {11 - preview}.",
                PriestSkill.LightBurst =>
                    $"Radiate Light in a {PriestSpellRules.BurstRadius:0.#}m circle around the target. Heal allies and damage enemies.\nGrace repeats the burst twice as the target moves.",
                PriestSkill.IceAegis =>
                    $"Shield yourself or an ally for up to {PriestSpellRules.AegisDuration:0}s. The shield absorbs Ice damage more efficiently.\nGrace surrounds the shielded ally with a damaging Ice aura until the shield breaks or expires.",
                PriestSkill.WildGrowth =>
                    $"Heal an ally or deal Nature damage to an enemy over 8 seconds.\nGrace doubles the duration on allies or slows enemies by {PriestSpellRules.GrowthSlow(preview):0}%.",
                PriestSkill.Revive =>
                    "Channel for 8 seconds to raise a fallen party member with 15% health and mana. You need more than half mana to begin. Success spends all mana and starts a 60s cooldown. The ally gains a stacking 10% attribute penalty for 5 minutes.\nGrace halves the channel to 4 seconds.",
                _ =>
                    $"Heal a living ally or deal Light damage to an enemy.\nBase effect {caster.BaseAmount(preview):0.#} before modifiers.\nGrace adds up to {preview} bounces. Each hop retains 90% potency and prioritizes wounded allies."
            };
            if (skill is not (PriestSkill.Grace or PriestSkill.Revive))
                text += $"\nMana {PriestSpellRules.ManaCost(skill, preview):0.#}, cooldown {PriestSpellRules.Cooldown(skill, preview):0.#}s, range {PriestSpellRules.Range(skill):0}m.";
            if (rank < PriestSkillProgression.MaxRank(skill))
                text += $"\nNext rank requires level {PriestSkillProgression.RequiredLevel(skill, rank + 1)} and 1 skill point.";
            return text;
        }
    }
}
