using System.Linq;
using DiceFree.Combat;
using DiceFree.Foundation;
using UnityEngine;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatActor), typeof(PriestSkillProgression))]
    public sealed class PriestSkillCaster : MonoBehaviour, IManifestationSessionState
    {
        public const float Range = 18f;
        private const float BounceRange = 7f;
        private const float BounceMultiplier = 0.9f;
        [SerializeField] private AttackDefinition holyLight;

        private CombatActor actor;
        private ActorResourceController resources;
        private PriestSkillProgression skills;
        private float readyAt;
        private bool casting;
        private bool graceArmed;

        public bool GraceArmed => graceArmed && GraceFull;
        public float Grace => resources.Current(PriestSkillProgression.GraceResourceId);
        public float GraceMaximum => resources.Maximum(PriestSkillProgression.GraceResourceId);
        public bool GraceFull => GraceMaximum > 0 && Grace >= GraceMaximum;
        public float CooldownRemaining => Mathf.Max(0, readyAt - Time.time);
        public string Feedback { get; private set; }

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            resources = GetComponent<ActorResourceController>();
            skills = GetComponent<PriestSkillProgression>();
        }

        public void Configure(AttackDefinition attack) => holyLight = attack;

        public static float ManaCost(int rank) => 18 + 2 * (rank - 1);
        public static float Cooldown(int rank) => 13 - 4f * (rank - 1) / 17;
        public static float Coefficient(int rank) => 1.8f + 0.16f * (rank - 1);
        public float BaseAmount(int rank) => holyLight.RawDamage(actor.Stats.Attributes, Coefficient(rank));

        public bool ArmGrace()
        {
            if (casting || !actor.CanAct || skills.Rank(PriestSkill.Grace) == 0 || !GraceFull)
                return Fail("Grace must be full before it can be armed.");
            graceArmed = true;
            Feedback = "Grace armed for your next successful spell.";
            return true;
        }

        public bool CanPrepareHolyLight()
        {
            int rank = skills.Rank(PriestSkill.HolyLight);
            if (casting || !actor.CanAct || rank == 0 || holyLight == null) return Fail("Holy Light is unavailable.");
            if (CooldownRemaining > 0) return Fail("Holy Light is on cooldown.");
            if (!resources.CanSpend(ResourceIds.Mana, ManaCost(rank))) return Fail("Not enough Mana.");
            return true;
        }

        public bool CastHolyLight(CombatActor target)
        {
            if (!CanPrepareHolyLight()) return false;
            if (!CanReach(actor, target, Range)) return Fail("Choose a living ally or enemy in range and sight.");

            int rank = skills.Rank(PriestSkill.HolyLight);
            bool empowered = GraceArmed;
            casting = true;
            try
            {
                actor.GetComponent<BasicAttack>()?.Cancel();
                actor.Motor.Stop();
                if (!resources.TrySpend(ResourceIds.Mana, ManaCost(rank))) return Fail("Not enough Mana.");

                readyAt = Time.time + Cooldown(rank);
                if (empowered)
                {
                    graceArmed = false;
                    resources.TrySpend(PriestSkillProgression.GraceResourceId, Grace);
                }

                int earned = ApplyHolyLight(actor, target, rank, 1);
                if (empowered) Bounce(target, rank);
                resources.Restore(PriestSkillProgression.GraceResourceId, earned);
                Feedback = "";
                return true;
            }
            finally
            {
                casting = false;
            }
        }

        private void Bounce(CombatActor previous, int rank)
        {
            float potency = 1;
            for (int hop = 0; hop < rank; hop++)
            {
                if (previous == null || !actor.Alive) return;
                var eligible = CombatActor.All.Where(candidate => candidate != previous && CanReach(previous, candidate, BounceRange));
                var next = eligible.Where(candidate => actor.IsFriendlyTo(candidate) && candidate.Health.Current < candidate.Health.Maximum)
                    .OrderBy(candidate => candidate.Health.Current / candidate.Health.Maximum)
                    .ThenBy(candidate => (candidate.transform.position - previous.transform.position).sqrMagnitude)
                    .FirstOrDefault();
                if (next == null)
                    next = eligible.Where(actor.IsHostileTo)
                        .OrderBy(candidate => (candidate.transform.position - previous.transform.position).sqrMagnitude)
                        .FirstOrDefault();
                if (next == null) return;
                potency *= BounceMultiplier;
                ApplyHolyLight(previous, next, rank, potency);
                previous = next;
            }
        }

        private int ApplyHolyLight(CombatActor origin, CombatActor target, int rank, float potency)
        {
            bool friendly = actor.IsFriendlyTo(target);
            EarlySkillVfx.PlayHolyLight(origin.transform.position, target.transform.position, friendly);
            if (!friendly)
            {
                DamageResolver.Hit(actor, target, holyLight, coefficientOverride: Coefficient(rank), rawMultiplier: potency);
                return 1;
            }

            float amount = BaseAmount(rank) * potency;
            float potential = Mathf.Max(0, amount * actor.Stats.HealingDone) * target.Stats.HealingReceived;
            float missing = target.Health.Maximum - target.Health.Current;
            target.Health.HealFrom(actor.Stats, amount);
            return potential > 0 && potential <= missing ? 2 : 1;
        }

        private static bool CanReach(CombatActor origin, CombatActor target, float range) =>
            target != null && target.Alive &&
            Vector3.Distance(origin.transform.position, target.transform.position) <= origin.Radius + target.Radius + range &&
            (target == origin || origin.HasSightOf(target));

        private bool Fail(string reason)
        {
            Feedback = reason;
            return false;
        }

        public void ResetForManifestationLoad()
        {
            readyAt = 0;
            graceArmed = false;
            Feedback = "";
        }
    }
}
