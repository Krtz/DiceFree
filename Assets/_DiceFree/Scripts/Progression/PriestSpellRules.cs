using UnityEngine;

namespace DiceFree.Skills
{
    public static class PriestSpellRules
    {
        public const float BurstRadius = 4.8f;
        public const float AegisRadius = 3.1f;
        public const float AegisDuration = 10f;
        public const float AegisTick = .1f;
        public const float GrowthTick = .25f;
        public const int GrowthTicks = 32;
        public const float ReviveDuration = 8f;
        public const float ReviveCooldown = 60f;

        public static string Name(PriestSkill skill) => skill switch
        {
            PriestSkill.HolyLight => "Holy Light",
            PriestSkill.LightBurst => "Light Burst",
            PriestSkill.IceAegis => "Ice Aegis",
            PriestSkill.WildGrowth => "Wild Growth",
            PriestSkill.Revive => "Revive",
            _ => "Grace"
        };
        public static float Range(PriestSkill skill) => skill switch
        {
            PriestSkill.HolyLight => PriestSkillCaster.Range,
            PriestSkill.Revive => 12f,
            PriestSkill.LightBurst => 16f,
            _ => 15f
        };
        public static float ManaCost(PriestSkill skill, int rank) => skill switch
        {
            PriestSkill.HolyLight => PriestSkillCaster.ManaCost(rank),
            PriestSkill.LightBurst => 25 + 2.5f * (rank - 1),
            PriestSkill.IceAegis => 22 + 2 * (rank - 1),
            PriestSkill.WildGrowth => 18 + 2 * (rank - 1),
            _ => 0
        };
        public static float Cooldown(PriestSkill skill, int rank) => skill switch
        {
            PriestSkill.HolyLight => PriestSkillCaster.Cooldown(rank),
            PriestSkill.LightBurst => RankCurve(15, 11, rank),
            PriestSkill.IceAegis => RankCurve(18, 14, rank),
            PriestSkill.WildGrowth => RankCurve(12, 8, rank),
            PriestSkill.Revive => ReviveCooldown,
            _ => 0
        };
        public static float Coefficient(PriestSkill skill, int rank) => skill switch
        {
            PriestSkill.HolyLight => PriestSkillCaster.Coefficient(rank),
            PriestSkill.LightBurst => 1.15f + .10f * (rank - 1),
            PriestSkill.WildGrowth => .075f + .006f * (rank - 1),
            PriestSkill.IceAegis => .036f + .003f * (rank - 1),
            _ => 0
        };
        public static float Shield(float spirit, int rank) => 38 + 5 * (rank - 1) + spirit * (2.6f + .20f * (rank - 1));
        public static float GrowthSlow(int rank) => RankCurve(10, 50, rank);
        private static float RankCurve(float first, float last, int rank) => Mathf.Lerp(first, last, (rank - 1) / 17f);
    }
}
