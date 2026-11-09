using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(DefenderSkillCaster))]
    public sealed class DefenderAbilityProvider : MonoBehaviour, IClassAbilityProvider
    {
        [SerializeField] private Texture2D[] icons;
        private DefenderSkillProgression progression;
        private DefenderSkillCaster caster;

        public string ClassId => DefenderSkillProgression.DefenderClassId;
        public int Count => DefenderSkillProgression.SkillCount;
        public int UltimateIndex => -1;
        public int UnspentPoints => progression.UnspentPoints;

        private void Awake()
        {
            progression = GetComponent<DefenderSkillProgression>();
            caster = GetComponent<DefenderSkillCaster>();
        }

        public AbilityBarSource.SlotView View(int index, string key)
        {
            if ((uint)index >= Count) return default;
            var skill = (DefenderSkill)index;
            int rank = progression.Rank(skill);
            return new AbilityBarSource.SlotView(DefenderSkillProgression.Name(skill), key, rank,
                DefenderSkillProgression.MaxRank(skill), caster.Remaining(skill), rank > 0, Describe(skill, rank),
                icons != null && index < icons.Length ? icons[index] : null, skill.ToString(),
                passive: skill == DefenderSkill.HoldTheLine, canUpgrade: progression.CanSpend(skill));
        }

        public bool Upgrade(int index) => (uint)index < Count && progression.Spend((DefenderSkill)index);
        public string UpgradeTooltip(int index) => (uint)index < Count
            ? "Spend 1 skill point\n" + Describe((DefenderSkill)index, progression.Rank((DefenderSkill)index)) : "";

        public bool Activate(int index, SkillTargetingController targeting, IQueuedOrders orders, bool append)
        {
            if ((uint)index >= Count || targeting == null) return false;
            var skill = (DefenderSkill)index;
            if (!caster.CanPrepare(skill)) return false;
            if (skill == DefenderSkill.ShieldBash)
            {
                targeting.BeginHostile(DefenderSkillProgression.Name(skill), caster.CastBash, DefenderSkillCaster.BashRange);
                return true;
            }

            return skill switch
            {
                DefenderSkill.Challenge => targeting.RunSelfAction(caster.CastChallenge),
                DefenderSkill.Bulwark => targeting.RunSelfAction(caster.CastBulwark),
                DefenderSkill.ShieldSweep => targeting.RunSelfAction(caster.CastSweep),
                DefenderSkill.IronGuard => targeting.RunSelfAction(caster.CastIronGuard),
                _ => false
            };
        }

        private static string Describe(DefenderSkill skill, int rank)
        {
            int preview = Mathf.Max(1, rank);
            string detail = skill switch
            {
                DefenderSkill.ShieldBash => "Charge a hostile within 5m and strike with your shield. The impact interrupts vulnerable casts, briefly stuns and grants Resolve.",
                DefenderSkill.Challenge => $"Raise your threat against nearby foes. Susceptible enemies focus you for {DefenderSkillCaster.ChallengeDuration(preview):0.0}s. Gain Resolve if enemies respond.",
                DefenderSkill.Bulwark => $"Reduce Physical and Magical damage by {100 * DefenderSkillCaster.BulwarkReduction(preview):0.#}% for 4s. Full incoming hits still build Resolve.",
                DefenderSkill.ShieldSweep => $"Strike foes in front of you. You and allies within 5m gain +{preview} Physical and Magical Defense per enemy hit for 4s, counting up to five.",
                DefenderSkill.HoldTheLine => $"You gain +{preview * 2} Physical Defense, +{preview} Magical Defense and build Resolve faster. You and allies within 5m gain {preview}% elemental resistance.",
                _ => $"Brace for 2s with +{500 * preview} Physical and Magical Defense. You cannot move or act while braced."
            };
            if (skill != DefenderSkill.HoldTheLine)
                detail += $"\nResolve {DefenderSkillCaster.Cost(skill)} · Cooldown {DefenderSkillCaster.Cooldown(skill)}s";
            if (rank < DefenderSkillProgression.MaxRank(skill))
                detail += $"\nNext rank requires level {DefenderSkillProgression.RequiredLevel(skill, rank + 1)}.";
            return detail;
        }

        public void Configure(Texture2D[] values) => icons = values;
    }
}