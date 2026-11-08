using System;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Foundation;
using UnityEngine;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(AcolyteSkillProgression), typeof(ActorResourceController))]
    public sealed class AcolyteSkillCaster : MonoBehaviour, IManifestationSessionState, IChannelingAbility
    {
        public const float Range = 16f;
        [SerializeField] private AttackDefinition drainLife, mortalCoil;

        private readonly float[] readyAt = new float[AcolyteSkillProgression.SkillCount];
        private CombatActor actor, target;
        private BasicAttack basic;
        private ActorResourceController resources;
        private AcolyteSkillProgression progression;
        private DiceFree.Foundation.IChannelingAbility[] otherChannels;
        private AcolyteSkill channel;
        private int rank, ticksLeft;
        private uint targetLife;
        private float nextTick, end;
        private Vector3 origin;

        public bool IsChanneling => ticksLeft > 0;
        public string ChannelLabel => IsChanneling ? AcolyteSkillProgression.Name(channel) : "";
        public float ChannelDuration => IsChanneling && channel == AcolyteSkill.DrainLife ? 5 : IsChanneling ? 1 : 0;
        public float ChannelRemaining => IsChanneling ? Mathf.Max(0, end - Time.time) : 0;
        public float SoulCharges => resources.Current(AcolyteSkillProgression.SoulResourceId);
        public static int ManaCost(AcolyteSkill skill) => skill == AcolyteSkill.DrainLife ? 24 : 14;
        public static int Cooldown(AcolyteSkill skill) => skill == AcolyteSkill.DrainLife ? 14 : 7;
        public float Remaining(AcolyteSkill skill) => (uint)skill < readyAt.Length ? Mathf.Max(0, readyAt[(int)skill] - Time.time) : 0;

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            basic = GetComponent<BasicAttack>();
            resources = GetComponent<ActorResourceController>();
            progression = GetComponent<AcolyteSkillProgression>();
        }

        private void Start() => otherChannels = GetComponents<MonoBehaviour>()
            .OfType<DiceFree.Foundation.IChannelingAbility>().Where(ability => !ReferenceEquals(ability, this)).ToArray();
        private void OnEnable() => actor.Health.Died += CancelChannel;
        private void OnDisable() { actor.Health.Died -= CancelChannel; ResetForManifestationLoad(); }

        private bool Busy() => otherChannels?.Any(ability => ability.IsChanneling) == true;

        public bool CanPrepare(AcolyteSkill skill) => progression.Rank(skill) > 0 && actor.CanAct && !IsChanneling &&
            !Busy() && Remaining(skill) == 0 && resources.CanSpend(ResourceIds.Mana, ManaCost(skill));

        public bool Cast(AcolyteSkill skill, CombatActor victim)
        {
            if (!CanPrepare(skill) || !ValidTarget(skill, victim) ||
                !resources.TrySpend(ResourceIds.Mana, ManaCost(skill))) return false;
            basic?.Cancel();
            actor.Motor.Stop();
            channel = skill;
            rank = progression.Rank(skill);
            origin = transform.position;
            target = victim;
            targetLife = victim.Health.DeathSequence;
            ticksLeft = skill == AcolyteSkill.DrainLife ? 5 : 1;
            end = Time.time + ticksLeft;
            nextTick = Time.time + 1;
            readyAt[(int)skill] = Time.time + Cooldown(skill);
            EarlySkillVfx.PlayPriestPulse(ChannelLabel, actor.transform.position, .9f, new Color(.68f,.40f,.95f), false);
            return true;
        }

        private bool ValidTarget(AcolyteSkill skill, CombatActor candidate) =>
            candidate != null && candidate.Alive &&
            (actor.IsHostileTo(candidate) || (skill == AcolyteSkill.MortalCoil && actor.IsFriendlyTo(candidate))) &&
            (candidate == actor || (Vector3.Distance(transform.position, candidate.transform.position) <= Range + actor.Radius + candidate.Radius &&
                actor.HasSightOf(candidate)));

        private void Update()
        {
            if (!IsChanneling) return;
            if (!progression.Active || !actor.CanAct || !ValidTarget(channel, target) ||
                target.Health.DeathSequence != targetLife || actor.Motor.Travelling ||
                (transform.position - origin).sqrMagnitude > .01f || basic?.Target != null || Busy())
            {
                CancelChannel();
                return;
            }
            while (IsChanneling && Time.time >= nextTick)
            {
                Pulse();
                if (!IsChanneling) return; // A synchronous health callback can cancel the channel.
                ticksLeft--;
                nextTick += 1;
                if (ticksLeft == 0 || target == null || !target.Alive)
                {
                    CancelChannel();
                    return;
                }
            }
        }

        private void Pulse()
        {
            var recipient = target;
            bool draining = channel == AcolyteSkill.DrainLife;
            bool friendly = actor.IsFriendlyTo(recipient);
            EarlySkillVfx.PlayPriestPulse(AcolyteSkillProgression.Name(channel), recipient.transform.position, .82f,
                friendly ? new Color(.70f,.63f,1f) : new Color(.61f,.22f,.93f));
            if (friendly)
            {
                var attributes = actor.Stats.Attributes;
                float raw = (10 + .25f * attributes.intelligence + .70f * attributes.spirit) * (1 + .08f * (rank - 1));
                var resistance = recipient.Stats.ResolveResistance(mortalCoil.element, ElementalContext.Healing, actor.Stats);
                var heal = new ElementalHealingResult(raw, mortalCoil.element, resistance);
                if (heal.InversionDamage > 0)
                    recipient.Health.ApplyPacket(actor, new DamageResult { channel = DamageChannel.Magical,
                        element = mortalCoil.element, mitigated = heal.InversionDamage * actor.Stats.HealingDone * recipient.Stats.HealingReceived });
                else recipient.Health.HealFrom(actor.Stats, heal.Healing);
                return;
            }

            var attack = draining ? drainLife : mortalCoil;
            float scale = 1 + (draining ? .06f : .08f) * (rank - 1);
            var hit = recipient.Health.ApplyPacket(actor, DamageResolver.Calculate(actor.Stats, recipient.Stats, attack, rawMultiplier: scale));
            if (hit.applied > 0 || hit.absorbed > 0)
                resources.Restore(AcolyteSkillProgression.SoulResourceId, 1);
            if (draining && hit.applied > 0)
                actor.Health.Heal(hit.applied * (.25f + .02f * (rank - 1)));
        }

        public void CancelChannel()
        {
            ticksLeft = 0;
            target = null;
        }

        public void ResetForManifestationLoad()
        {
            CancelChannel();
            Array.Clear(readyAt, 0, readyAt.Length);
        }

        public void Configure(AttackDefinition drain, AttackDefinition coil) { drainLife = drain; mortalCoil = coil; }
    }
}
