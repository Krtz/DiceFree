using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Foundation;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(RangerSkillProgression), typeof(ActorResourceController))]
    public sealed class RangerSkillCaster : MonoBehaviour, IChannelingAbility, IManifestationSessionState
    {
        private const string MasteryId = "passive.ranger.bow-mastery";
        private const string StepId = "buff.ranger.evasive-step";
        public const float RainRange = 12f;
        public const float RainRadius = 3f;
        public const float BlastRadius = 2.5f;
        private static readonly int[] Mana = { 10, 20, 30, 15, 12, 0 };
        private static readonly int[] Cooldowns = { 6, 12, 20, 15, 15, 0 };
        [SerializeField] private AttackDefinition aimed, explosive, rain, hawk;
        private readonly float[] readyAt = new float[RangerSkillProgression.SkillCount];
        private readonly List<CombatActor> areaTargets = new();
        private CombatActor actor, marked, aimedTarget;
        private BasicAttack basic;
        private ActorResourceController resources;
        private RangerSkillProgression progression;
        private IChannelingAbility[] otherChannels;
        private GameObject markVisual;
        private float markUntil, markBonus, stepUntil, stepEvasion, castAt;
        private int masteryRank, castRank, skyShots;
        private Vector3 castOrigin, rainPoint;
        private RangerSkill castSkill;

        public bool Active => isActiveAndEnabled && progression.Active && actor.Alive;
        public float RangeBonus => Active ? masteryRank * .1f : 0;
        public float BasicEvasion => Active && Time.time < stepUntil ? stepEvasion : 0;
        public CombatActor MarkedTarget => Active && Time.time < markUntil && marked != null && marked.Alive ? marked : null;
        public bool IsChanneling => castAt > 0;
        public string ChannelLabel => RangerSkillProgression.Name(castSkill);
        public float ChannelDuration => castSkill == RangerSkill.ArrowRain ? 1.5f : .45f;
        public float ChannelRemaining => IsChanneling ? Mathf.Max(0, castAt - Time.time) : 0;
        public event Action<RangerSkill> Casted;

        public static int Cost(RangerSkill skill) => Mana[(int)skill];
        public static int Cooldown(RangerSkill skill) => Cooldowns[(int)skill];
        public float Remaining(RangerSkill skill) => Mathf.Max(0, readyAt[(int)skill] - Time.time);
        public float Range(RangerSkill skill) => skill == RangerSkill.AimedShot ? 14f + RangeBonus : 12f;
        public float DamageMultiplier(CombatActor target) => target != null && target == MarkedTarget ? 1 + markBonus : 1;
        public CriticalRule CriticalRule() => Active && masteryRank > 0
            ? new CriticalRule(MasteryId, "ranger.bow-crit", true, masteryRank * .01f, 1.4f) : null;

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            basic = GetComponent<BasicAttack>();
            resources = GetComponent<ActorResourceController>();
            progression = GetComponent<RangerSkillProgression>();
        }

        private void Start() => otherChannels = GetComponents<MonoBehaviour>().OfType<IChannelingAbility>()
            .Where(channel => !ReferenceEquals(channel, this)).ToArray();

        private void OnEnable()
        {
            progression.Changed += RefreshPassive;
            actor.Health.Died += ClearEffects;
            RefreshPassive();
        }

        private void OnDisable()
        {
            progression.Changed -= RefreshPassive;
            actor.Health.Died -= ClearEffects;
            actor.Stats.RemoveAttributeContribution(MasteryId);
            masteryRank = 0;
            ResetForManifestationLoad();
        }

        private void RefreshPassive()
        {
            int rank = progression.Rank(RangerSkill.BowMastery);
            if (masteryRank == rank) return;
            masteryRank = rank;
            if (rank > 0) actor.Stats.SetAttributeContribution(MasteryId, new AttributeValues { agility = rank * 2 });
            else actor.Stats.RemoveAttributeContribution(MasteryId);
        }

        private void Update()
        {
            if (marked != null && MarkedTarget == null) ClearMark();
            if (stepUntil > 0 && Time.time >= stepUntil) ClearStep();
            if (!IsChanneling) return;
            if (!actor.CanAct || actor.Motor.Travelling || (transform.position - castOrigin).sqrMagnitude > .025f)
            {
                CancelChannel();
                return;
            }

            if (castSkill == RangerSkill.ArrowRain)
            {
                float elapsed = ChannelDuration - ChannelRemaining;
                if (skyShots == 0 && elapsed >= .40f) FireSkyArrow(true);
                if (skyShots == 1 && elapsed >= 1.08f) FireSkyArrow(false);
            }

            if (Time.time < castAt) return;
            castAt = 0;
            if (castSkill == RangerSkill.ArrowRain)
            {
                var damage = basic.DamageRange * (.4f + .035f * (castRank - 1));
                StartCoroutine(Rainfall(rainPoint, damage));
            }
            else if (actor.CanTargetHostile(aimedTarget) && InRange(RangerSkill.AimedShot, aimedTarget))
            {
                var target = aimedTarget;
                int rank = castRank;
                var action = AccuracyResolver.Resolve(actor, aimed, critical: CriticalRule());
                float raw = RollBow() * (1.8f + .12f * (rank - 1));
                if (target == MarkedTarget) raw += .5f * actor.Stats.Attributes.agility * (1 + .05f * (rank - 1));
                Launch(target, RangerVfx.ArrowTint, () => Hit(target, aimed, raw, action));
            }
            aimedTarget = null;
        }

        public bool CanPrepare(RangerSkill skill) =>
            skill != RangerSkill.BowMastery && Active && actor.CanAct && !IsChanneling &&
            (skill != RangerSkill.EvasiveStep || actor.Motor.MotionAllowed) && progression.Rank(skill) > 0 &&
            Remaining(skill) == 0 && resources.CanSpend(ResourceIds.Mana, Cost(skill)) &&
            otherChannels?.Any(channel => channel.IsChanneling) != true;

        private bool Spend(RangerSkill skill)
        {
            if (!resources.TrySpend(ResourceIds.Mana, Cost(skill))) return false;
            readyAt[(int)skill] = Time.time + Cooldown(skill);
            return true;
        }

        private bool InRange(RangerSkill skill, CombatActor target) =>
            Vector3.Distance(transform.position, target.transform.position) <= Range(skill) + actor.Radius + target.Radius &&
            actor.HasSightOf(target);

        public bool CastHostile(RangerSkill skill, CombatActor target)
        {
            if (skill != RangerSkill.AimedShot && skill != RangerSkill.ExplodingArrow && skill != RangerSkill.HawkDive)
                return false;
            if (!CanPrepare(skill) || !actor.CanTargetHostile(target) || !InRange(skill, target) || !Spend(skill))
                return false;
            basic.Cancel();
            Face(target.transform.position);
            int rank = progression.Rank(skill);
            if (skill == RangerSkill.AimedShot)
            {
                aimedTarget = target;
                BeginChannel(skill, rank);
            }
            else if (skill == RangerSkill.ExplodingArrow)
            {
                float bow = RollBow();
                var action = Guaranteed(explosive);
                Launch(target, RangerVfx.FireTint, () => Explode(target, bow, rank, action));
            }
            else
            {
                float raw = RollBow() * (.85f + .10f * (rank - 1));
                StartCoroutine(Dive(target, raw, rank));
            }
            Casted?.Invoke(skill);
            return true;
        }

        public bool CastRain(Vector3 point)
        {
            const RangerSkill skill = RangerSkill.ArrowRain;
            if (!CanPrepare(skill) || Vector3.Distance(transform.position, point) > RainRange + actor.Radius ||
                !NavMesh.SamplePosition(point, out var ground, .75f, NavMesh.AllAreas) ||
                Physics.Linecast(transform.position + Vector3.up, ground.position + Vector3.up * .3f, 1 << 9,
                    QueryTriggerInteraction.Ignore) || !Spend(skill)) return false;
            basic.Cancel();
            rainPoint = ground.position;
            Face(rainPoint);
            BeginChannel(skill, progression.Rank(skill));
            Casted?.Invoke(skill);
            return true;
        }

        private void BeginChannel(RangerSkill skill, int rank)
        {
            castSkill = skill;
            castRank = rank;
            castOrigin = transform.position;
            castAt = Time.time + ChannelDuration;
            skyShots = 0;
        }

        public void CancelChannel()
        {
            castAt = 0;
            aimedTarget = null;
        }

        private void FireSkyArrow(bool large)
        {
            skyShots++;
            var start = Muzzle;
            RangerVfx.Arrow(start, start + Vector3.up * (large ? 13 : 10) + transform.forward * 3,
                large ? .42f : .33f, large ? 1.4f : .82f, RangerVfx.ArrowTint);
        }

        private IEnumerator Rainfall(Vector3 point, Vector2 damage)
        {
            var action = Guaranteed(rain);
            for (int wave = 0; wave < 4; wave++)
            {
                RangerVfx.Rain(point, RainRadius, wave);
                yield return new WaitForSeconds(.5f);
                if (!Active) yield break;
                SelectArea(point, RainRadius);
                foreach (var target in areaTargets)
                    if (actor.IsHostileTo(target)) Hit(target, rain, UnityEngine.Random.Range(damage.x, damage.y), action);
            }
        }

        private IEnumerator Dive(CombatActor target, float raw, int rank)
        {
            RangerVfx.Hawk(transform.position, target.transform.position, .38f);
            yield return new WaitForSeconds(.38f);
            if (!Active || !actor.IsHostileTo(target)) yield break;
            Hit(target, hawk, raw, Guaranteed(hawk));
            if (!target.Alive) yield break;
            ClearMark();
            marked = target;
            markBonus = .10f + .015f * (rank - 1);
            markUntil = Time.time + 8f;
            markVisual = RangerVfx.Mark(target.transform, target.Radius);
        }

        public bool CastStep(Vector3 direction)
        {
            const RangerSkill skill = RangerSkill.EvasiveStep;
            if (!CanPrepare(skill) || !actor.Motor.Ready) return false;
            Vector3 start = transform.position;
            var delta = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (delta.sqrMagnitude < .01f) return false;
            Vector3 intended = start + delta.normalized * 3.6f;
            if (NavMesh.Raycast(start, intended, out var wall, NavMesh.AllAreas))
                intended = wall.position - delta.normalized * .4f;
            if (!NavMesh.SamplePosition(intended, out var destination, .45f, NavMesh.AllAreas) ||
                Vector3.Dot(destination.position - start, delta.normalized) < .5f ||
                NavMesh.Raycast(start, destination.position, out _, NavMesh.AllAreas)) return false;

            foreach (var other in CombatActor.All)
            {
                if (other == actor || !other.Alive) continue;
                float clearance = actor.Radius + other.Radius;
                if ((other.transform.position - destination.position).sqrMagnitude < clearance * clearance) return false;
            }

            basic.Cancel();
            if (!actor.Motor.Teleport(destination.position) || !Spend(skill)) return false;
            Face(destination.position + delta);
            stepUntil = Time.time + 3;
            stepEvasion = .01f * progression.Rank(skill);
            actor.Stats.SetMovementSpeedPercentModifier(StepId, 10);
            RangerVfx.Step(start, transform.position);
            Casted?.Invoke(skill);
            return true;
        }

        private void Explode(CombatActor primary, float bow, int rank, ActionResolution action)
        {
            Vector3 point = primary.transform.position;
            SelectArea(point, BlastRadius);
            Hit(primary, explosive, bow * (1.10f + .07f * (rank - 1)), action);
            float splash = bow * (.55f + .035f * (rank - 1));
            foreach (var target in areaTargets)
                if (actor.IsHostileTo(target)) Hit(target, explosive, splash, action);
            RangerVfx.Explosion(point);
        }

        private void SelectArea(Vector3 point, float radius)
        {
            areaTargets.Clear();
            foreach (var target in CombatActor.All)
                if (actor.IsHostileTo(target) && (target.transform.position - point).sqrMagnitude <= radius * radius)
                    areaTargets.Add(target);
        }

        private ActionResolution Guaranteed(AttackDefinition attack) =>
            ActionResolution.Resolve(new ActionProvenance(attack.stableId, actor.Stats.Definition.stableId));

        private void Hit(CombatActor target, AttackDefinition attack, float raw, ActionResolution action)
        {
            var packet = action.Hit.missed ? new DamageResult { channel = attack.channel, element = attack.element } :
                DamageResolver.Calculate(actor.Stats, target.Stats, attack, action.Critical, skillFlatDamage: raw,
                    finalRawMultiplier: DamageMultiplier(target));
            packet.action = action;
            target.Health.ApplyPacket(actor, packet);
        }

        private float RollBow()
        {
            var range = basic.DamageRange;
            return UnityEngine.Random.Range(range.x, range.y);
        }

        private Vector3 Muzzle => transform.position + Vector3.up * 1.22f + transform.forward * .4f;
        private void Face(Vector3 point)
        {
            var direction = Vector3.ProjectOnPlane(point - transform.position, Vector3.up);
            if (direction.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(direction);
        }

        public void Launch(CombatActor target, Color tint, Action impact)
        {
            var start = Muzzle;
            float flight = Mathf.Clamp(Vector3.Distance(start, target.transform.position) / 29f, .09f, .56f);
            StartCoroutine(Fly(start, target, tint, flight, impact));
        }

        private IEnumerator Fly(Vector3 start, CombatActor target, Color tint, float flight, Action impact)
        {
            var arrow = RangerVfx.TrackArrow(start, target.transform.position + Vector3.up, tint, flight);
            for (float elapsed = 0; elapsed < flight; elapsed += Time.deltaTime)
            {
                if (!Active || !actor.IsHostileTo(target)) yield break;
                RangerVfx.PlaceArrow(arrow, start, target.transform.position + Vector3.up, elapsed / flight);
                yield return null;
            }
            if (Active && actor.IsHostileTo(target)) impact();
        }

        private void ClearMark()
        {
            if (markVisual != null) Destroy(markVisual);
            markVisual = null;
            marked = null;
            markUntil = markBonus = 0;
        }

        private void ClearStep()
        {
            stepUntil = stepEvasion = 0;
            actor.Stats.RemoveMovementSpeedPercentModifier(StepId);
        }

        private void ClearEffects()
        {
            CancelChannel();
            StopAllCoroutines();
            ClearMark();
            ClearStep();
        }

        public void ResetForManifestationLoad()
        {
            ClearEffects();
            Array.Clear(readyAt, 0, readyAt.Length);
        }

        public void Configure(AttackDefinition aimedShot, AttackDefinition explodingArrow, AttackDefinition arrowRain,
            AttackDefinition hawkDive)
        {
            aimed = aimedShot;
            explosive = explodingArrow;
            rain = arrowRain;
            hawk = hawkDive;
        }
    }
}
