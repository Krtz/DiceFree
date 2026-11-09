using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Foundation;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(PsionSkillProgression), typeof(ActorResourceController))]
    [RequireComponent(typeof(BasicAttack))]
    public sealed class PsionSkillCaster : MonoBehaviour, IChannelingAbility, IInterruptibleCombatCast, IManifestationSessionState
    {
        public const float BoltRange = 7;
        public const float ImageRange = 6;
        public const float StormRange = 9;
        public const float StormRadius = 4;
        public const float ShieldRadius = 8;
        private static readonly Color Silver = new(.82f, .87f, 1f);
        private static readonly Color Violet = new(.69f, .48f, 1f);
        private static readonly int[] Cooldowns = { 8, 0, 12, 15, 30 };

        [SerializeField] private AttackDefinition bolt, storm, imageBlade, imageRupture;
        [SerializeField] private ElementDefinition metal;
        private CombatActor actor, selected;
        private BasicAttack basic;
        private ActorResourceController resources;
        private PsionSkillProgression progression;
        private IChannelingAbility[] otherChannels;
        private readonly float[] readyAt = new float[PsionSkillProgression.SkillCount];
        private readonly List<PsionMentalImage> images = new();
        private readonly List<CombatActor> victims = new();
        private SharedDamageShield shield;
        private Vector3 castOrigin, area;
        private float startedAt;
        private int castRank, wave, sessionVersion;
        private PsionSkill casting;

        public bool Active => isActiveAndEnabled && progression.Active && actor.Alive;
        public bool IsChanneling { get; private set; }
        public string ChannelLabel => PsionSkillProgression.Name(casting);
        public float ChannelDuration => casting == PsionSkill.LightningStorm ? .7f : CastTime(casting);
        public float ChannelRemaining => IsChanneling ? Mathf.Max(0, startedAt + ChannelDuration - Time.time) : 0;
        public int WavesReleased { get; private set; }
        public float ShieldRemaining => shield?.Remaining ?? 0;
        public float BasicPenetration => Active ? progression.Rank(PsionSkill.PsionicBladesMastery) : 0;
        public event Action<PsionSkill> Casted;

        public static int Cost(PsionSkill skill, int rank) => skill switch
        {
            PsionSkill.PsychicBolt => 8 + 3 * rank,
            PsionSkill.MentalImage => 14 + 5 * rank,
            PsionSkill.LightningStorm => 20 + 7 * rank,
            PsionSkill.MetalShield => 18 + 6 * rank,
            _ => 0
        };
        public static int Cooldown(PsionSkill skill) => Cooldowns[(int)skill];
        public static float CastTime(PsionSkill skill) => skill is PsionSkill.PsychicBolt or PsionSkill.LightningStorm ? .1f : .2f;
        public static float ImageDuration(int rank) => 1.6f + .2f * (rank - 1);
        public float Remaining(PsionSkill skill) => Mathf.Max(0, readyAt[(int)skill] - Time.time);
        public CriticalRule BasicCriticalRule()
        {
            float rank = BasicPenetration;
            return rank > 0 ? new CriticalRule(PsionSkillProgression.PsionClassId, "psion.basic-crit", true, rank * .025f, 1.5f) : null;
        }

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            basic = GetComponent<BasicAttack>();
            resources = GetComponent<ActorResourceController>();
            progression = GetComponent<PsionSkillProgression>();
        }

        private void Start() => otherChannels = GetComponents<MonoBehaviour>().OfType<IChannelingAbility>()
            .Where(channel => !ReferenceEquals(channel, this)).ToArray();
        private void OnEnable() => actor.Health.Died += CancelChannel;
        private void OnDisable()
        {
            actor.Health.Died -= CancelChannel;
            ResetForManifestationLoad();
        }

        public bool CanPrepare(PsionSkill skill)
        {
            int rank = progression.Rank(skill);
            return Active && actor.CanAct && !IsChanneling && rank > 0 && skill != PsionSkill.PsionicBladesMastery &&
                Remaining(skill) == 0 && resources.CanSpend(ResourceIds.Mana, Cost(skill, rank)) &&
                otherChannels?.Any(channel => channel.IsChanneling) != true;
        }

        public bool CastBolt(CombatActor target)
        {
            const PsionSkill skill = PsionSkill.PsychicBolt;
            if (!CanPrepare(skill) || !actor.CanTargetHostile(target) || !BoltInRange(target)) return false;
            selected = target;
            Face(target.transform.position);
            return Begin(skill);
        }

        public bool CastImage(Vector3 point) => CastGround(PsionSkill.MentalImage, point, ImageRange);
        public bool CastStorm(Vector3 point) => CastGround(PsionSkill.LightningStorm, point, StormRange);
        public bool CastShield() => CanPrepare(PsionSkill.MetalShield) && Begin(PsionSkill.MetalShield);

        private bool CastGround(PsionSkill skill, Vector3 point, float range)
        {
            if (!CanPrepare(skill) || !NavMesh.SamplePosition(point, out var ground, .65f, NavMesh.AllAreas) ||
                Vector3.Distance(transform.position, ground.position) > range ||
                Physics.Linecast(transform.position + Vector3.up, ground.position + Vector3.up * .30f,
                    1 << 9, QueryTriggerInteraction.Ignore) ||
                (skill == PsionSkill.MentalImage && !FreeImagePoint(ground.position))) return false;
            area = ground.position;
            Face(area);
            return Begin(skill);
        }

        private bool FreeImagePoint(Vector3 point)
        {
            foreach (var other in CombatActor.All)
            {
                if (!other.Alive) continue;
                float clearance = other.Radius + .46f;
                if ((other.transform.position - point).sqrMagnitude < clearance * clearance) return false;
            }
            return !Physics.CheckCapsule(point + Vector3.up*.36f, point + Vector3.up*1.20f, .32f, 1 << 9,
                QueryTriggerInteraction.Ignore);
        }

        private bool Begin(PsionSkill skill)
        {
            int rank = progression.Rank(skill);
            if (!resources.TrySpend(ResourceIds.Mana, Cost(skill, rank))) return false;
            readyAt[(int)skill] = Time.time + Cooldown(skill);
            basic.Cancel();
            casting = skill;
            castRank = rank;
            castOrigin = transform.position;
            startedAt = Time.time;
            IsChanneling = true;
            wave = 0;
            if (skill == PsionSkill.LightningStorm) WavesReleased = 0;
            Casted?.Invoke(skill);
            return true;
        }

        private void Update()
        {
            if (images.Count > 0) images.RemoveAll(image => image == null);
            if (!IsChanneling) return;
            if (!Active || !actor.CanAct || actor.Motor.Travelling || (transform.position - castOrigin).sqrMagnitude > .0225f)
            {
                CancelChannel();
                return;
            }

            float elapsed = Time.time - startedAt;
            if (casting == PsionSkill.LightningStorm)
            {
                // Stun or movement wins if it arrives before the final pulse is processed.
                while (IsChanneling && actor.CanAct && wave < 3 && elapsed >= .1f + .3f * wave)
                {
                    StormPulse();
                    wave++;
                    WavesReleased++;
                }
                if (wave == 3) CancelChannel();
                return;
            }

            if (elapsed < CastTime(casting)) return;
            switch (casting)
            {
                case PsionSkill.PsychicBolt:
                    if (actor.CanTargetHostile(selected) && BoltInRange(selected)) LaunchBolt(selected, castRank);
                    break;
                case PsionSkill.MentalImage:
                    if (FreeImagePoint(area))
                        images.Add(PsionMentalImage.Summon(actor, basic, area, castRank, imageBlade, imageRupture,
                            BasicCriticalRule(), BasicPenetration));
                    break;
                case PsionSkill.MetalShield:
                    ApplyShield(castRank);
                    break;
            }
            CancelChannel();
        }

        private bool BoltInRange(CombatActor target) =>
            Vector3.Distance(transform.position, target.transform.position) <= BoltRange + actor.Radius + target.Radius &&
            actor.HasSightOf(target);

        private void LaunchBolt(CombatActor target, int rank)
        {
            float raw = 30 + 8 * rank + 1.2f * actor.Stats.Attributes.intelligence;
            int version = sessionVersion;
            TargetedCombatProjectile.Launch(actor, target, "Psychic Bolt", Silver, .23f, 24, recipient =>
            {
                if (version != sessionVersion || !actor.IsHostileTo(recipient)) return;
                var result = recipient.Health.ApplyPacket(actor, DamageResolver.CalculateRaw(actor.Stats, recipient.Stats, bolt, raw));
                if (recipient.Alive && (result.applied > 0 || result.absorbed > 0))
                    recipient.Effects?.TryApplyStun(.15f * rank, false);
                EarlySkillVfx.PlayPriestPulse("Psychic Bolt impact", recipient.transform.position, .53f, Violet);
            });
        }

        public void LaunchBlade(CombatActor target, Action<CombatActor> impact)
        {
            int version = sessionVersion;
            TargetedCombatProjectile.Launch(actor, target, "Psionic blade", Violet, .15f, 26, recipient =>
            {
                if (version == sessionVersion && actor.IsHostileTo(recipient)) impact(recipient);
            });
        }

        private void StormPulse()
        {
            float raw = 15 + 5 * castRank + .6f * actor.Stats.Attributes.intelligence;
            victims.Clear();
            foreach (var other in CombatActor.All)
                if (actor.IsHostileTo(other) && (other.transform.position - area).sqrMagnitude <= StormRadius * StormRadius)
                    victims.Add(other);
            foreach (var other in victims)
                if (actor.IsHostileTo(other))
                    other.Health.ApplyPacket(actor, DamageResolver.CalculateRaw(actor.Stats, other.Stats, storm, raw));
            PsionStormVisual.Play(area, StormRadius, wave);
        }

        private void ApplyShield(int rank)
        {
            shield?.Expire();
            shield = new SharedDamageShield(100 + 30 * rank + .75f * actor.Stats.Attributes.intelligence, 8, metal);
            var party = actor.GetComponent<ICombatPartyMembership>();
            foreach (var ally in CombatActor.All)
            {
                if (!actor.IsFriendlyTo(ally) || (ally != actor && party?.SameParty(ally) == false) ||
                    (ally.transform.position - transform.position).sqrMagnitude > ShieldRadius * ShieldRadius) continue;
                ally.Health.AddSharedShield(shield);
                EarlySkillVfx.PlayPriestPulse("Metal Shield", ally.transform.position, .71f, Silver);
            }
        }

        private void Face(Vector3 point)
        {
            var direction = Vector3.ProjectOnPlane(point - transform.position, Vector3.up);
            if (direction.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(direction);
        }

        public bool TryInterruptCast()
        {
            if (!IsChanneling) return false;
            CancelChannel();
            return true;
        }

        public void CancelChannel()
        {
            IsChanneling = false;
            selected = null;
        }

        public void ResetForManifestationLoad()
        {
            CancelChannel();
            sessionVersion++;
            foreach (var image in images)
                if (image != null) image.DismissForSessionReset();
            images.Clear();
            shield?.Expire();
            shield = null;
            WavesReleased = 0;
            Array.Clear(readyAt, 0, readyAt.Length);
        }

        public void Configure(AttackDefinition psychicBolt, AttackDefinition lightningStorm, AttackDefinition cloneBlade,
            AttackDefinition cloneRupture, ElementDefinition metalElement)
        {
            bolt = psychicBolt;
            storm = lightningStorm;
            imageBlade = cloneBlade;
            imageRupture = cloneRupture;
            metal = metalElement;
        }
    }
}