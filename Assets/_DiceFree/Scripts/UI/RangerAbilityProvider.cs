using DiceFree.Skills;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiceFree.UI
{
    [DisallowMultipleComponent, RequireComponent(typeof(RangerSkillCaster))]
    public sealed class RangerAbilityProvider : MonoBehaviour, IClassAbilityProvider
    {
        [SerializeField] private Texture2D[] icons;
        private RangerSkillProgression progression;
        private RangerSkillCaster caster;

        public string ClassId => RangerSkillProgression.RangerClassId;
        public int Count => RangerSkillProgression.SkillCount;
        public int UltimateIndex => -1;
        public int UnspentPoints => progression.UnspentPoints;

        private void Awake()
        {
            progression = GetComponent<RangerSkillProgression>();
            caster = GetComponent<RangerSkillCaster>();
        }

        public AbilityBarSource.SlotView View(int index, string key)
        {
            if ((uint)index >= Count) return default;
            var skill = (RangerSkill)index;
            int rank = progression.Rank(skill);
            return new AbilityBarSource.SlotView(RangerSkillProgression.Name(skill), key, rank,
                RangerSkillProgression.MaxRank(skill), caster.Remaining(skill), rank > 0, Describe(skill, rank),
                icons != null && index < icons.Length ? icons[index] : null, skill.ToString(),
                passive: skill == RangerSkill.BowMastery, canUpgrade: progression.CanSpend(skill));
        }

        public bool Upgrade(int index) => (uint)index < Count && progression.Spend((RangerSkill)index);
        public string UpgradeTooltip(int index) => (uint)index < Count
            ? "Spend 1 skill point\n" + Describe((RangerSkill)index, progression.Rank((RangerSkill)index)) : "";

        public bool Activate(int index, SkillTargetingController targeting, IQueuedOrders orders, bool append)
        {
            if ((uint)index >= Count || targeting == null) return false;
            var skill = (RangerSkill)index;
            if (!caster.CanPrepare(skill)) return false;
            string name = RangerSkillProgression.Name(skill);
            if (skill == RangerSkill.ArrowRain)
                targeting.BeginGround(name, caster.CastRain, RangerSkillCaster.RainRange);
            else if (skill == RangerSkill.EvasiveStep)
            {
                if (Mouse.current == null || Camera.main == null) return false;
                var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                var plane = new Plane(Vector3.up, transform.position);
                if (!plane.Raycast(ray, out float distance)) return false;
                Vector3 direction = ray.GetPoint(distance) - transform.position;
                return targeting.RunSelfAction(() => caster.CastStep(direction));
            }
            else targeting.BeginHostile(name, target => caster.CastHostile(skill, target), caster.Range(skill));
            return true;
        }

        private static string Describe(RangerSkill skill, int rank)
        {
            int preview = Mathf.Max(1, rank);
            string detail = skill switch
            {
                RangerSkill.AimedShot => $"Plant your feet and fire for {1.8f + .12f * (preview - 1):0.00}× bow damage. Your mark adds {.5f * (1 + .05f * (preview - 1)):0.000}× AGI before the mark bonus.",
                RangerSkill.ExplodingArrow => "Fire a guaranteed hit. The primary target takes a direct Gun hit and a blast. Enemies within 2.5m take blast damage.",
                RangerSkill.ArrowRain => "Spend 1.5s firing two arrows skyward. Four waves then strike a 3m area over 2s, even while you move.",
                RangerSkill.HawkDive => $"Call a hawk for one strike. Mark its target for 8s and deal {10 + 1.5f * (preview - 1):0.#}% more damage to it.",
                RangerSkill.EvasiveStep => $"Leap toward the cursor. Gain 10% move speed and {preview}% basic-attack evasion for 3s. Requires freedom to move.",
                _ => $"+{2 * preview} AGI. Basic attacks and Aimed Shot gain {preview * .1f:0.0}m range and {preview}% chance to crit for 1.4× raw damage."
            };
            if (skill != RangerSkill.BowMastery)
                detail += $"\nMana {RangerSkillCaster.Cost(skill)} · Cooldown {RangerSkillCaster.Cooldown(skill)}s";
            if (rank < RangerSkillProgression.MaxRank(skill))
                detail += $"\nNext rank requires level {RangerSkillProgression.RequiredLevel(skill, rank + 1)}.";
            return detail;
        }

        public void Configure(Texture2D[] values) => icons = values;
    }
}
