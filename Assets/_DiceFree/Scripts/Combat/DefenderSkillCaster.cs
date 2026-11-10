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
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(DefenderSkillProgression), typeof(ActorResourceController))]
    [RequireComponent(typeof(BasicAttack))]
    public sealed class DefenderSkillCaster : MonoBehaviour, IManifestationSessionState
    {
        public const string ResolveId = "resource.resolve";
        public const float BashRange = 5f;
        public const float AllyRadius = 5f;
        private const string ChargeSpeed = "defender.shield-bash";
        private const string GuardLock = "defender.iron-guard";
        private const string Passive = "defender.hold-the-line";
        private const string Bulwark = "defender.bulwark";
        private static readonly Color Brass = new(.99f, .72f, .30f);
        private static readonly Color Blue = new(.33f, .72f, .92f);
        private static readonly int[] Cooldowns = { 8, 15, 22, 11, 0, 22 };
        private static readonly int[] Costs = { 0, 0, 25, 0, 0, 20 };
        private static readonly HashSet<DefenderSkillCaster> Defenders = new();

        [SerializeField] private AttackDefinition bash, sweep;
        private readonly float[] readyAt = new float[DefenderSkillProgression.SkillCount];
        private readonly List<CombatActor> targets = new();
        private readonly List<ActorStats> sweepRecipients = new();
        private CombatActor actor;
        private BasicAttack basic;
        private ActorResourceController resources;
        private DefenderSkillProgression skills;
        private IChannelingAbility[] channels;
        private Coroutine charge;
        private GameObject aura;
        private float guardUntil, bulwarkUntil, sweepUntil, nextDecay, combatUntil, gainBudget, gainUpdatedAt;
        private string sweepKey;

        public bool Active => isActiveAndEnabled && actor.Alive && skills.Active;
        public float Resolve => resources?.Current(ResolveId) ?? 0;
        public bool Charging { get; private set; }
        public bool Guarding => guardUntil > 0;
        public bool BulwarkActive => bulwarkUntil > 0;
        public bool Engaged => Active && (actor.InCombat || Time.time < combatUntil);
        public int AuraRank => Active ? skills.Rank(DefenderSkill.HoldTheLine) : 0;
        public event Action<DefenderSkill> Casted;

        public static int Cooldown(DefenderSkill skill) => Cooldowns[(int)skill];
        public static int Cost(DefenderSkill skill) => Costs[(int)skill];
        public static float BulwarkReduction(int rank) => .30f + .008f * (rank - 1);
        public static float ChallengeDuration(int rank) => 2f + .08f * (rank - 1);
        private const float BashDamagePerRank = 5f, SweepDamagePerRank = 4f;

        public float PreviewDamage(DefenderSkill skill, int rank)
        {
            var attack = skill == DefenderSkill.ShieldBash ? bash : sweep;
            float bonus = skill == DefenderSkill.ShieldBash ? BashDamagePerRank : SweepDamagePerRank;
            return attack.RawDamage(actor.Stats.Attributes) + bonus * (rank - 1);
        }

        public float Remaining(DefenderSkill skill) => Mathf.Max(0, readyAt[(int)skill] - Time.time);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => Defenders.Clear();

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            basic = GetComponent<BasicAttack>();
            resources = GetComponent<ActorResourceController>();
            skills = GetComponent<DefenderSkillProgression>();
            sweepKey = "defender.sweep." + gameObject.GetEntityId();
        }

        private void Start() => channels = GetComponents<MonoBehaviour>().OfType<IChannelingAbility>().ToArray();

        private void OnEnable()
        {
            Defenders.Add(this);
            actor.Health.PacketResolved += Incoming;
            actor.Health.Died += ResetForManifestationLoad;
            if (basic != null) basic.Connected += BasicConnected;
            skills.Changed += RefreshPassive;
            RefreshPassive();
        }

        private void OnDisable()
        {
            Defenders.Remove(this);
            actor.Health.PacketResolved -= Incoming;
            actor.Health.Died -= ResetForManifestationLoad;
            if (basic != null) basic.Connected -= BasicConnected;
            skills.Changed -= RefreshPassive;
            ResetForManifestationLoad();
            RemoveDefense(actor.Stats, Passive);
            if (aura != null) Destroy(aura);
            aura = null;
        }

        private void BasicConnected(CombatActor target, DamageResult packet)
        {
            if (Active && packet.raw > 0) TouchCombat();
        }

        private void TouchCombat() => combatUntil = Time.time + 3;

        private void Incoming(CombatActor source, DamageResult packet)
        {
            if (!Active || !actor.IsHostileTo(source) || packet.Missed || packet.raw <= 0 ||
                packet.Outcome == PacketOutcome.ResistanceRestoration) return;
            TouchCombat();
            // A twelve-point burst can be earned immediately. A rolling shared budget
            // limits sustained gain from split actions, very rapid DOTs and many attackers.
            float now = Time.time;
            gainBudget = Mathf.Min(12, gainBudget + Mathf.Max(0, now - gainUpdatedAt) * 12);
            gainUpdatedAt = now;
            float eligible = Mathf.Min(gainBudget, Mathf.Min(12, 2f + packet.raw * .03f) * ResolveGainFactor());
            gainBudget -= eligible;
            resources.Restore(ResolveId, eligible);
        }

        private float ResolveGainFactor() => 1 + .035f * AuraRank;

        private void Update()
        {
            float now = Time.time;
            if (guardUntil > 0 && now >= guardUntil) EndGuard();
            if (bulwarkUntil > 0 && now >= bulwarkUntil) EndBulwark();
            if (sweepUntil > 0 && now >= sweepUntil) ClearSweep();
            if (!Active) return;
            if (Engaged || Resolve <= 0)
            {
                nextDecay = 0;
                return;
            }
            if (nextDecay <= 0) nextDecay = now + 3;
            if (now < nextDecay) return;
            int ticks = 1 + Mathf.FloorToInt((now - nextDecay) / 3);
            resources.TrySpend(ResolveId, Mathf.Min(ticks, Resolve));
            nextDecay += ticks * 3;
        }

        private void RefreshPassive()
        {
            int rank = AuraRank;
            if (rank > 0)
            {
                SetDefense(actor.Stats, Passive, rank * 2f, rank);
                if (aura == null) aura = CreateAura();
            }
            else
            {
                RemoveDefense(actor.Stats, Passive);
                if (aura != null) Destroy(aura);
                aura = null;
            }
        }

        private GameObject CreateAura()
        {
            var ring = new GameObject("Hold the Line - allied radius");
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = Vector3.up * .09f;
            var line = ring.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.widthMultiplier = .027f;
            line.sharedMaterial = Resources.Load<Material>("EarlySkillVfx/SoftRing");
            if (line.sharedMaterial == null)
            {
                Destroy(ring);
                return null;
            }
            line.startColor = line.endColor = new Color(.46f, .73f, .90f, .32f);
            line.positionCount = 56;
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = Mathf.PI * 2 * i / line.positionCount;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * AllyRadius);
            }
            return ring;
        }

        private bool Protects(CombatActor ally) =>
            actor.IsFriendlyTo(ally) &&
            (actor == ally || actor.GetComponent<ICombatPartyMembership>()?.SameParty(ally) != false) &&
            Vector3.Distance(transform.position, ally.transform.position) <= AllyRadius;

        public static float StrongestResistance(ActorStats target)
        {
            if (Defenders.Count == 0 || !target.TryGetComponent<CombatActor>(out var ally)) return 0;
            int strongest = 0;
            foreach (var defender in Defenders)
                if (defender.AuraRank > strongest && defender.Protects(ally)) strongest = defender.AuraRank;
            return strongest * .01f;
        }

        public bool CanPrepare(DefenderSkill skill) => skill != DefenderSkill.HoldTheLine && Active &&
            actor.CanAct && !Charging && skills.Rank(skill) > 0 && Remaining(skill) == 0 &&
            resources.CanSpend(ResolveId, Cost(skill)) && channels?.Any(channel => channel.IsChanneling) != true;

        private bool Begin(DefenderSkill skill)
        {
            int cost = Cost(skill);
            if (cost > 0 && !resources.TrySpend(ResolveId, cost)) return false;
            readyAt[(int)skill] = Time.time + Cooldown(skill);
            basic?.Cancel();
            Casted?.Invoke(skill);
            return true;
        }

        public bool CastBash(CombatActor target)
        {
            const DefenderSkill skill = DefenderSkill.ShieldBash;
            if (!CanPrepare(skill) || !actor.CanTargetHostile(target) || !actor.HasSightOf(target) ||
                !actor.Motor.Ready || !actor.Motor.MotionAllowed) return false;
            var start = transform.position;
            var direction = Vector3.ProjectOnPlane(target.transform.position - start, Vector3.up);
            float touch = ContactRange(target);
            if (Vector3.Distance(start, target.transform.position) > BashRange + actor.Radius + target.Radius ||
                (direction.magnitude > touch && NavMesh.Raycast(start,
                    target.transform.position - direction.normalized * (touch - .17f), out _, NavMesh.AllAreas)))
                return false;
            Face(direction);
            if (!Begin(skill)) return false;
            if (Vector3.Distance(start, target.transform.position) <= touch) BashImpact(target);
            else charge = StartCoroutine(Charge(target));
            return true;
        }

        private float ContactRange(CombatActor target) => actor.Radius + target.Radius + .55f;

        private IEnumerator Charge(CombatActor target)
        {
            Charging = true;
            float travel = 0;
            float end = Time.time + .48f;
            actor.Motor.SetSpeedFactor(ChargeSpeed, 3.35f);
            // Leave the first motion step to the next frame so start/cancel own one coroutine handle.
            yield return null;
            while (actor.CanAct && actor.Motor.MotionAllowed && actor.CanTargetHostile(target) && actor.HasSightOf(target))
            {
                Vector3 start = transform.position;
                Vector3 delta = target.transform.position - start;
                if (delta.magnitude <= ContactRange(target))
                {
                    BashImpact(target);
                    break;
                }
                if (Time.time >= end || travel >= BashRange) break;
                float step = Mathf.Min(Time.deltaTime, (BashRange - travel) / actor.Motor.Speed);
                actor.Motor.MoveDirect(delta.normalized, step);
                travel += Mathf.Min(BashRange, actor.Motor.Speed * step);
                yield return null;
            }
            EndCharge();
        }

        public void CancelCharge()
        {
            if (charge == null) return;
            StopCoroutine(charge);
            EndCharge();
        }

        private void EndCharge()
        {
            charge = null;
            Charging = false;
            actor.Motor.RemoveSpeedFactor(ChargeSpeed);
        }

        private void BashImpact(CombatActor target)
        {
            Face(target.transform.position - transform.position);
            int rank = skills.Rank(DefenderSkill.ShieldBash);
            if (!Hit(target, bash, rank, BashDamagePerRank)) return;
            foreach (var component in target.GetComponents<MonoBehaviour>())
                if (component is IInterruptibleCombatCast cast) cast.TryInterruptCast();
            target.Effects?.TryApplyStun(.75f, false);
            Gain(8);
            EarlySkillVfx.PlayPriestPulse("Shield Bash", target.transform.position, .75f, Brass);
        }

        public bool CastChallenge()
        {
            const DefenderSkill skill = DefenderSkill.Challenge;
            if (!CanPrepare(skill)) return false;
            int rank = skills.Rank(skill);
            SelectHostiles(AllyRadius, false);
            int affected = 0;
            foreach (var target in targets)
                if (target.TryGetComponent<IChallengeTarget>(out var enemy) &&
                    enemy.TryChallenge(actor, ChallengeDuration(rank), 35 + 8 * rank)) affected++;
            if (affected == 0 || !Begin(skill)) return false;
            Gain(10);
            EarlySkillVfx.PlayPriestPulse("Challenge", transform.position, AllyRadius, Brass);
            return true;
        }

        public bool CastBulwark()
        {
            const DefenderSkill skill = DefenderSkill.Bulwark;
            if (!CanPrepare(skill) || !Begin(skill)) return false;
            float reduction = BulwarkReduction(skills.Rank(skill));
            actor.Stats.SetDamageTakenModifier(Bulwark + ".physical", new DamageTakenModifier(DamageChannel.Physical, reduction));
            actor.Stats.SetDamageTakenModifier(Bulwark + ".magical", new DamageTakenModifier(DamageChannel.Magical, reduction));
            bulwarkUntil = Time.time + 4;
            EarlySkillVfx.PlayPriestPulse("Bulwark", transform.position, 1.05f, Blue);
            return true;
        }

        public bool CastSweep()
        {
            const DefenderSkill skill = DefenderSkill.ShieldSweep;
            if (!CanPrepare(skill) || !Begin(skill)) return false;
            int rank = skills.Rank(skill);
            SelectHostiles(3.3f, true);
            int hits = 0;
            foreach (var target in targets)
                if (actor.IsHostileTo(target) && Hit(target, sweep, rank, SweepDamagePerRank)) hits++;
            EarlySkillVfx.Play(EarlySkillVfx.Cue.PhysicalHeavy, transform.position + transform.forward * 1.25f);
            ClearSweep();
            if (hits == 0) return true;
            Gain(Mathf.Min(15, 5 * hits));
            float bonus = rank * Mathf.Min(hits, 5);
            foreach (var ally in CombatActor.All)
            {
                if (!Protects(ally)) continue;
                SetDefense(ally.Stats, sweepKey, bonus, bonus);
                sweepRecipients.Add(ally.Stats);
            }
            sweepUntil = Time.time + 4;
            EarlySkillVfx.PlayPriestPulse("Shield Sweep protection", transform.position, AllyRadius, Blue);
            return true;
        }

        public bool CastIronGuard()
        {
            const DefenderSkill skill = DefenderSkill.IronGuard;
            if (!CanPrepare(skill) || !Begin(skill)) return false;
            float defense = 500 * skills.Rank(skill);
            guardUntil = Time.time + 2;
            SetDefense(actor.Stats, GuardLock, defense, defense);
            actor.SetActionBlocked(GuardLock, true);
            actor.Motor.SetMotionBlocked(GuardLock, true);
            EarlySkillVfx.PlayPriestPulse("Iron Guard", transform.position, 1.2f, Brass);
            return true;
        }

        private void SelectHostiles(float radius, bool frontal)
        {
            targets.Clear();
            foreach (var target in CombatActor.All)
            {
                if (!actor.IsHostileTo(target) || !actor.HasSightOf(target)) continue;
                Vector3 delta = target.transform.position - transform.position;
                if (delta.sqrMagnitude > radius * radius) continue;
                if (frontal && Vector3.Dot(transform.forward, Vector3.ProjectOnPlane(delta, Vector3.up).normalized) < .10f)
                    continue;
                targets.Add(target);
            }
        }

        private bool Hit(CombatActor target, AttackDefinition attack, int rank, float damagePerRank)
        {
            var action = AccuracyResolver.Resolve(actor, attack);
            var packet = DamageResolver.CalculatePacket(actor.Stats, target.Stats, attack, action,
                finalRawMultiplier: 1 + damagePerRank * (rank - 1) / attack.RawDamage(actor.Stats.Attributes));
            var result = target.Health.ApplyPacket(actor, packet);
            return !result.Missed && result.DamagePotential > 0 && (result.applied > 0 || result.absorbed > 0);
        }

        private void Gain(float amount)
        {
            TouchCombat();
            resources.Restore(ResolveId, amount * ResolveGainFactor());
        }

        private void Face(Vector3 direction)
        {
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            if (direction.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(direction);
        }

        private static void SetDefense(ActorStats stats, string id, float physical, float magical)
        {
            stats.SetDefenseModifier(id + ".physical", new DefenseModifier { channel = DamageChannel.Physical, buffFlat = physical });
            stats.SetDefenseModifier(id + ".magical", new DefenseModifier { channel = DamageChannel.Magical, buffFlat = magical });
        }

        private static void RemoveDefense(ActorStats stats, string id)
        {
            stats.RemoveDefenseModifier(id + ".physical");
            stats.RemoveDefenseModifier(id + ".magical");
        }

        private void ClearSweep()
        {
            foreach (var recipient in sweepRecipients)
                if (recipient != null) RemoveDefense(recipient, sweepKey);
            sweepRecipients.Clear();
            sweepUntil = 0;
        }

        private void EndGuard()
        {
            guardUntil = 0;
            RemoveDefense(actor.Stats, GuardLock);
            actor.SetActionBlocked(GuardLock, false);
            actor.Motor.SetMotionBlocked(GuardLock, false);
        }

        private void EndBulwark()
        {
            bulwarkUntil = 0;
            actor.Stats.RemoveDamageTakenModifier(Bulwark + ".physical");
            actor.Stats.RemoveDamageTakenModifier(Bulwark + ".magical");
        }

        public void ResetForManifestationLoad()
        {
            CancelCharge();
            EndGuard();
            EndBulwark();
            ClearSweep();
            combatUntil = nextDecay = 0;
            gainUpdatedAt = Time.time;
            gainBudget = 12;
            Array.Clear(readyAt, 0, readyAt.Length);
            if (Resolve > 0) resources.TrySpend(ResolveId, Resolve);
        }

        public void Configure(AttackDefinition bashAttack, AttackDefinition sweepAttack)
        {
            bash = bashAttack;
            sweep = sweepAttack;
        }
    }
}
