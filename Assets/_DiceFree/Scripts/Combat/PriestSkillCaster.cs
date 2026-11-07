using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Foundation;
using UnityEngine;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatActor), typeof(PriestSkillProgression), typeof(ActorResourceController))]
    public sealed class PriestSkillCaster : MonoBehaviour, IManifestationSessionState, IChannelingAbility
    {
        public const float Range = 18f;
        private const float BounceRange = 7f;
        private const float BounceMultiplier = .9f;
        private const float EffectTether = 80f;
        [SerializeField] private AttackDefinition holyLight, lightBurst, iceAura, wildGrowth;

        private sealed class TimedSpell
        {
            public CombatActor Target;
            public uint DeathSequence;
            public int Rank, PulsesLeft;
            public float NextPulse, NextVisual;
            public bool Empowered, Friendly;
        }

        private readonly List<TimedSpell> bursts = new(), shields = new(), growths = new();
        private readonly float[] readyAt = new float[PriestSkillProgression.SkillCount];
        private CombatActor actor;
        private ActorResourceController resources;
        private PriestSkillProgression skills;
        private BasicAttack basic;
        private string shieldId, growthId;
        private bool casting, graceArmed, reviveEmpowered;
        private CombatActor reviveTarget;
        private uint reviveDeath;
        private Vector3 channelOrigin;
        private float channelEnd;

        public bool GraceArmed => graceArmed && GraceFull;
        public float Grace => resources.Current(PriestSkillProgression.GraceResourceId);
        public float GraceMaximum => resources.Maximum(PriestSkillProgression.GraceResourceId);
        public bool GraceFull => GraceMaximum > 0 && Grace >= GraceMaximum;
        public float CooldownRemaining => Remaining(PriestSkill.HolyLight);
        public string Feedback { get; private set; }
        public event Action<PriestSkill> Casted;

        public bool IsChanneling => ChannelDuration > 0;
        public string ChannelLabel => "Revive";
        public float ChannelDuration { get; private set; }
        public float ChannelRemaining => IsChanneling ? Mathf.Max(0, channelEnd - Time.time) : 0;

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            resources = GetComponent<ActorResourceController>();
            skills = GetComponent<PriestSkillProgression>();
            basic = GetComponent<BasicAttack>();
            shieldId = "priest.ice-aegis:" + GetEntityId();
            growthId = "priest.wild-growth:" + GetEntityId();
        }

        private void OnEnable()
        {
            actor.Health.Damaged += OnDamaged;
            actor.Health.Died += OnDied;
        }
        private void OnDisable()
        {
            actor.Health.Damaged -= OnDamaged;
            actor.Health.Died -= OnDied;
            ResetForManifestationLoad();
        }

        public void Configure(AttackDefinition attack) => holyLight = attack;
        public void ConfigureComplete(AttackDefinition burst, AttackDefinition ice, AttackDefinition nature)
        {
            lightBurst = burst;
            iceAura = ice;
            wildGrowth = nature;
        }

        public static float ManaCost(int rank) => 18 + 2 * (rank - 1);
        public static float Cooldown(int rank) => 13 - 4f * (rank - 1) / 17;
        public static float Coefficient(int rank) => 1.8f + .16f * (rank - 1);
        public float BaseAmount(int rank) => BaseAmount(PriestSkill.HolyLight, rank);
        public float BaseAmount(PriestSkill skill, int rank) =>
            Attack(skill)?.RawDamage(actor.Stats.Attributes, PriestSpellRules.Coefficient(skill, rank)) ?? 0;
        public float ShieldAmount(int rank) => PriestSpellRules.Shield(actor.Stats.Attributes.spirit, rank);
        public float Remaining(PriestSkill skill) => Mathf.Max(0, readyAt[(int)skill] - Time.time);

        private AttackDefinition Attack(PriestSkill skill) => skill switch
        {
            PriestSkill.HolyLight => holyLight,
            PriestSkill.LightBurst => lightBurst,
            PriestSkill.IceAegis => iceAura,
            PriestSkill.WildGrowth => wildGrowth,
            _ => null
        };

        public bool ArmGrace()
        {
            if (casting || IsChanneling || !actor.CanAct || skills.Rank(PriestSkill.Grace) == 0 || !GraceFull)
                return Fail("Grace must be full and the Priest ready before it can be armed.");
            graceArmed = true;
            Feedback = "Grace armed for your next successful spell.";
            return true;
        }

        public bool CanPrepareHolyLight() => CanPrepare(PriestSkill.HolyLight);
        public bool CanPrepare(PriestSkill skill)
        {
            int rank = skills.Rank(skill);
            if (rank == 0 || !actor.CanAct || casting || IsChanneling || skill == PriestSkill.Grace)
                return Fail("This Priest spell is unavailable.");
            if (Remaining(skill) > 0) return Fail("This spell is on cooldown.");
            if (skill == PriestSkill.Revive)
                return resources.Has(ResourceIds.Mana) && resources.Current(ResourceIds.Mana) > .5f * resources.Maximum(ResourceIds.Mana) ||
                    Fail("Revive requires more than half Mana.");
            if (Attack(skill) == null) return Fail("This spell is missing its combat definition.");
            return resources.CanSpend(ResourceIds.Mana, PriestSpellRules.ManaCost(skill, rank)) || Fail("Not enough Mana.");
        }

        public bool CastHolyLight(CombatActor target) => Cast(PriestSkill.HolyLight, target);
        public bool Cast(PriestSkill skill, CombatActor target)
        {
            if (!CanPrepare(skill)) return false;
            if (skill == PriestSkill.Revive) return BeginRevive(target);
            if (!CanReach(actor, target, PriestSpellRules.Range(skill)) ||
                skill == PriestSkill.IceAegis && !actor.IsFriendlyTo(target))
                return Fail("Choose a living target of the correct allegiance in range and sight.");

            int rank = skills.Rank(skill);
            bool empowered = GraceArmed;
            casting = true;
            try
            {
                StopToCast();
                if (!resources.TrySpend(ResourceIds.Mana, PriestSpellRules.ManaCost(skill, rank))) return Fail("Not enough Mana.");
                readyAt[(int)skill] = Time.time + PriestSpellRules.Cooldown(skill, rank);
                if (empowered) ConsumeGrace();
                int earned = skill switch
                {
                    PriestSkill.HolyLight => HolyLight(target, rank, empowered),
                    PriestSkill.LightBurst => LightBurst(target, rank, empowered),
                    PriestSkill.IceAegis => IceAegis(target, rank, empowered),
                    PriestSkill.WildGrowth => WildGrowth(target, rank, empowered),
                    _ => 0
                };
                EarnGrace(earned);
                Feedback = "";
                Casted?.Invoke(skill);
                return true;
            }
            finally { casting = false; }
        }

        private int HolyLight(CombatActor target, int rank, bool empowered)
        {
            int earned = HolyLightImpact(actor, target, rank, 1);
            if (!empowered) return earned;
            var previous = target;
            float potency = 1;
            for (int hop = 0; hop < rank && previous != null && actor.Alive; hop++)
            {
                var eligible = CombatActor.All.Where(candidate => candidate != previous &&
                    candidate.Alive && Within(previous, candidate, BounceRange) && previous.HasSightOf(candidate));
                var next = eligible.Where(candidate => actor.IsFriendlyTo(candidate) && candidate.Health.Current < candidate.Health.Maximum)
                    .OrderBy(candidate => candidate.Health.Current / candidate.Health.Maximum)
                    .ThenBy(candidate => (candidate.transform.position - previous.transform.position).sqrMagnitude)
                    .FirstOrDefault();
                next ??= eligible.Where(actor.IsHostileTo)
                    .OrderBy(candidate => (candidate.transform.position - previous.transform.position).sqrMagnitude).FirstOrDefault();
                if (next == null) break;
                potency *= BounceMultiplier;
                HolyLightImpact(previous, next, rank, potency);
                previous = next;
            }
            return earned;
        }

        private int HolyLightImpact(CombatActor origin, CombatActor target, int rank, float potency)
        {
            EarlySkillVfx.PlayHolyLight(origin.transform.position, target.transform.position, actor.IsFriendlyTo(target));
            return ApplyMixed(target, PriestSkill.HolyLight, rank, potency);
        }

        private int LightBurst(CombatActor target, int rank, bool empowered)
        {
            var effect = Effect(target, rank, empowered, 2, 1);
            int earned = BurstPulse(target, rank);
            if (empowered) bursts.Add(effect);
            return earned;
        }

        private int BurstPulse(CombatActor center, int rank)
        {
            EarlySkillVfx.PlayPriestPulse("Light Burst", center.transform.position, PriestSpellRules.BurstRadius, new Color(1f, .75f, .32f));
            int earned = 1;
            foreach (var unit in Nearby(center, PriestSpellRules.BurstRadius))
                if (unit != null && unit.Alive) earned = Mathf.Max(earned, ApplyMixed(unit, PriestSkill.LightBurst, rank, 1));
            return earned;
        }

        private int IceAegis(CombatActor target, int rank, bool empowered)
        {
            shields.RemoveAll(effect => effect.Target == target);
            target.Health.AddTemporaryShield(shieldId, ShieldAmount(rank), PriestSpellRules.AegisDuration, iceAura.element.stableId);
            shields.Add(Effect(target, rank, empowered, 0, PriestSpellRules.AegisTick));
            EarlySkillVfx.PlayPriestPulse("Ice Aegis", target.transform.position,
                empowered ? PriestSpellRules.AegisRadius : 1.1f, new Color(.33f, .78f, 1f));
            return 1;
        }

        private int WildGrowth(CombatActor target, int rank, bool empowered)
        {
            target.Stats.RemoveMovementSpeedPercentModifier(growthId);
            growths.RemoveAll(effect => effect.Target == target);
            bool friendly = actor.IsFriendlyTo(target);
            int ticks = PriestSpellRules.GrowthTicks * (friendly && empowered ? 2 : 1);
            var effect = Effect(target, rank, empowered, ticks - 1, PriestSpellRules.GrowthTick);
            growths.Add(effect);
            if (empowered && !friendly) target.Stats.SetMovementSpeedPercentModifier(growthId, -PriestSpellRules.GrowthSlow(rank));
            EarlySkillVfx.PlayPriestPulse("Wild Growth", target.transform.position, 1.05f, new Color(.39f, .94f, .48f));
            return ApplyMixed(target, PriestSkill.WildGrowth, rank, 1);
        }

        private TimedSpell Effect(CombatActor target, int rank, bool empowered, int pulses, float delay) => new()
        {
            Target = target, DeathSequence = target.Health.DeathSequence, Rank = rank, Empowered = empowered,
            Friendly = actor.IsFriendlyTo(target), PulsesLeft = pulses, NextPulse = Time.time + delay, NextVisual = Time.time + .85f
        };

        private int ApplyMixed(CombatActor target, PriestSkill skill, int rank, float potency)
        {
            if (actor.IsHostileTo(target))
            {
                DamageResolver.Hit(actor, target, Attack(skill), coefficientOverride: PriestSpellRules.Coefficient(skill, rank), rawMultiplier: potency);
                return 1;
            }
            if (!actor.IsFriendlyTo(target)) return 1;
            float amount = BaseAmount(skill, rank) * potency;
            float potential = Mathf.Max(0, amount * actor.Stats.HealingDone) * target.Stats.HealingReceived;
            float missing = target.Health.Maximum - target.Health.Current;
            target.Health.HealFrom(actor.Stats, amount);
            return potential > 0 && potential <= missing ? 2 : 1;
        }

        private CombatActor[] Nearby(CombatActor center, float radius) => CombatActor.All
            .Where(unit => unit.Alive && Vector3.Distance(center.transform.position, unit.transform.position) <= radius + unit.Radius &&
                (unit == center || center.HasSightOf(unit))).ToArray();
        private static bool Within(CombatActor from, CombatActor target, float range) =>
            Vector3.Distance(from.transform.position, target.transform.position) <= from.Radius + target.Radius + range;
        private static bool CanReach(CombatActor from, CombatActor target, float range) =>
            target != null && target.Alive && Within(from, target, range) && (target == from || from.HasSightOf(target));
        private bool Valid(TimedSpell effect) => effect.Target != null && effect.Target.Alive &&
            effect.Target.Health.DeathSequence == effect.DeathSequence && Within(actor, effect.Target, EffectTether);

        private void Update()
        {
            if (reviveTarget != null || ChannelDuration > 0) UpdateRevive();
            if (!actor.Alive || !skills.Active) return;
            UpdateBursts();
            UpdateShields();
            UpdateGrowth();
        }

        private void UpdateBursts()
        {
            for (int i = bursts.Count - 1; i >= 0; i--)
            {
                var effect = bursts[i];
                while (effect.PulsesLeft > 0 && Valid(effect) && Time.time >= effect.NextPulse)
                {
                    effect.NextPulse += 1;
                    effect.PulsesLeft--;
                    BurstPulse(effect.Target, effect.Rank);
                }
                if (effect.PulsesLeft == 0 || !Valid(effect)) bursts.RemoveAt(i);
            }
        }

        private void UpdateShields()
        {
            for (int i = shields.Count - 1; i >= 0; i--)
            {
                var effect = shields[i];
                if (!Valid(effect) || effect.Target.Health.TemporaryShieldFrom(shieldId) <= 0)
                {
                    if (effect.Target != null) effect.Target.Health.RemoveTemporaryShield(shieldId);
                    shields.RemoveAt(i);
                    continue;
                }
                if (!effect.Empowered) continue;
                while (Time.time >= effect.NextPulse && Valid(effect) && effect.Target.Health.TemporaryShieldFrom(shieldId) > 0)
                {
                    effect.NextPulse += PriestSpellRules.AegisTick;
                    foreach (var enemy in Nearby(effect.Target, PriestSpellRules.AegisRadius).Where(actor.IsHostileTo))
                        if (enemy != null && enemy.Alive) DamageResolver.Hit(actor, enemy, iceAura,
                            coefficientOverride: PriestSpellRules.Coefficient(PriestSkill.IceAegis, effect.Rank));
                }
                if (Time.time >= effect.NextVisual && Valid(effect))
                {
                    effect.NextVisual = Time.time + .85f;
                    EarlySkillVfx.PlayPriestPulse("Aegis frost", effect.Target.transform.position,
                        PriestSpellRules.AegisRadius, new Color(.26f, .67f, 1f), false);
                }
            }
        }

        private void UpdateGrowth()
        {
            for (int i = growths.Count - 1; i >= 0; i--)
            {
                var effect = growths[i];
                while (effect.PulsesLeft > 0 && Valid(effect) && Time.time >= effect.NextPulse)
                {
                    effect.NextPulse += PriestSpellRules.GrowthTick;
                    effect.PulsesLeft--;
                    ApplyMixed(effect.Target, PriestSkill.WildGrowth, effect.Rank, 1);
                }
                if (!Valid(effect) || effect.PulsesLeft == 0 && Time.time >= effect.NextPulse)
                {
                    if (effect.Target != null) effect.Target.Stats.RemoveMovementSpeedPercentModifier(growthId);
                    growths.RemoveAt(i);
                }
                else if (Time.time >= effect.NextVisual)
                {
                    effect.NextVisual = Time.time + 1.15f;
                    EarlySkillVfx.PlayPriestPulse("Growth leaves", effect.Target.transform.position, .85f,
                        effect.Friendly ? new Color(.53f, 1f, .42f) : new Color(.35f, .75f, .28f), false);
                }
            }
        }

        public bool IsValidCorpse(CombatActor target) =>
            target != null && target != actor && target.isActiveAndEnabled && target.Faction == actor.Faction &&
            actor.GetComponent<ICombatPartyMembership>()?.SameParty(target) == true &&
            target.GetComponent<ICombatResurrectionTarget>()?.CanReviveInPlace == true;

        private bool CanReachCorpse(CombatActor target) => IsValidCorpse(target) &&
            Within(actor, target, PriestSpellRules.Range(PriestSkill.Revive)) && actor.HasSightOf(target);

        private bool BeginRevive(CombatActor target)
        {
            if (!CanReachCorpse(target)) return Fail("Choose a fallen party member in range and sight.");
            if (OtherChannelActive()) return Fail("Finish your current channel first.");
            StopToCast();
            reviveTarget = target;
            reviveDeath = target.Health.DeathSequence;
            reviveEmpowered = GraceArmed;
            ChannelDuration = PriestSpellRules.ReviveDuration * (reviveEmpowered ? .5f : 1f);
            channelEnd = Time.time + ChannelDuration;
            channelOrigin = transform.position;
            EarlySkillVfx.PlayHolyLight(transform.position, target.transform.position, true);
            Feedback = "Reviving " + target.DisplayName + "…";
            return true;
        }

        private void UpdateRevive()
        {
            if (!actor.CanAct || !skills.Active || !CanReachCorpse(reviveTarget) ||
                reviveTarget.Health.DeathSequence != reviveDeath || actor.Motor.Travelling ||
                (transform.position - channelOrigin).sqrMagnitude > .01f || basic?.Target != null || OtherChannelActive())
            {
                CancelRevive();
                return;
            }
            if (Time.time < channelEnd) return;
            var target = reviveTarget;
            var revive = target.GetComponent<ICombatResurrectionTarget>();
            bool empowered = reviveEmpowered;
            ClearChannel();
            casting = true;
            try
            {
                if (!revive.ReviveInPlace()) { Fail("The party member is no longer available to revive."); return; }
                resources.TrySpend(ResourceIds.Mana, resources.Current(ResourceIds.Mana));
                readyAt[(int)PriestSkill.Revive] = Time.time + PriestSpellRules.ReviveCooldown;
                if (empowered) ConsumeGrace();
                EarnGrace(1);
                EarlySkillVfx.PlayPriestPulse("Revive", target.transform.position, 1.65f, new Color(1f, .89f, .54f));
                Feedback = target.DisplayName + " revived.";
                Casted?.Invoke(PriestSkill.Revive);
            }
            finally { casting = false; }
        }

        private bool OtherChannelActive() => GetComponents<MonoBehaviour>()
            .Any(component => component != this && component is IChannelingAbility channel && channel.IsChanneling);
        private void StopToCast() { basic?.Cancel(); actor.Motor.Stop(); }
        public void CancelRevive()
        {
            if (ChannelDuration <= 0) return;
            ClearChannel();
            Feedback = "Revive interrupted.";
        }
        private void ClearChannel()
        {
            reviveTarget = null;
            reviveEmpowered = false;
            ChannelDuration = channelEnd = 0;
        }

        private void OnDamaged(CombatActor _, DamageResult damage) => CancelRevive();
        private void OnDied() { CancelRevive(); ClearEffects(); }
        private void ConsumeGrace()
        {
            graceArmed = false;
            resources.TrySpend(PriestSkillProgression.GraceResourceId, Grace);
        }
        private void EarnGrace(int amount) => resources.Restore(PriestSkillProgression.GraceResourceId, amount);
        private bool Fail(string reason) { Feedback = reason; return false; }

        private void ClearEffects()
        {
            foreach (var effect in shields)
                if (effect.Target != null) effect.Target.Health.RemoveTemporaryShield(shieldId);
            foreach (var effect in growths)
                if (effect.Target != null) effect.Target.Stats.RemoveMovementSpeedPercentModifier(growthId);
            shields.Clear();
            growths.Clear();
            bursts.Clear();
        }

        public void ResetForManifestationLoad()
        {
            ClearChannel();
            ClearEffects();
            Array.Clear(readyAt, 0, readyAt.Length);
            graceArmed = false;
            Feedback = "";
        }
    }
}
