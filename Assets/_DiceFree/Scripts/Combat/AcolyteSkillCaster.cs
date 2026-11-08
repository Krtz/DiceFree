using System;
using System.Collections.Generic;
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
        [SerializeField] private AttackDefinition drainLife, mortalCoil, radiation, coffeePoison;

        private readonly float[] readyAt = new float[AcolyteSkillProgression.SkillCount];
        private CombatActor actor, target;
        private BasicAttack basic;
        private ActorResourceController resources;
        private AcolyteSkillProgression progression;
        private AcolyteAfflictions afflictions;
        private TargetSelection selection;
        private readonly List<AcolyteSkeleton> skeletons = new();
        private Func<bool> autoRetaliate;
        private const string PassiveSource = "passive.acolyte.occult-adept";
        private int passiveRank;
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
        public event Action<AcolyteSkill> Casted;
        public IReadOnlyList<AcolyteSkeleton> Skeletons => skeletons;
        public static int ManaCost(AcolyteSkill skill) => skill switch
        {
            AcolyteSkill.DrainLife => 24, AcolyteSkill.MortalCoil => 14,
            AcolyteSkill.WitheringCurse => 12, AcolyteSkill.RadiationPlague => 15, _ => 0
        };
        public static int Cooldown(AcolyteSkill skill) => skill switch
        {
            AcolyteSkill.DrainLife => 14, AcolyteSkill.MortalCoil => 7, AcolyteSkill.WitheringCurse => 12,
            AcolyteSkill.RadiationPlague => 10, AcolyteSkill.SummonSkeleton => 3, _ => 0
        };
        public float Remaining(AcolyteSkill skill) => (uint)skill < readyAt.Length ? Mathf.Max(0, readyAt[(int)skill] - Time.time) : 0;

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            basic = GetComponent<BasicAttack>();
            resources = GetComponent<ActorResourceController>();
            progression = GetComponent<AcolyteSkillProgression>();
            selection = GetComponent<TargetSelection>();
            afflictions = new AcolyteAfflictions(actor, () => resources.Restore(AcolyteSkillProgression.SoulResourceId, 1));
        }

        private void Start() => otherChannels = GetComponents<MonoBehaviour>()
            .OfType<DiceFree.Foundation.IChannelingAbility>().Where(ability => !ReferenceEquals(ability, this)).ToArray();
        private void OnEnable()
        {
            actor.Health.Died += ClearCombat;
            progression.Changed += RefreshPassive;
            if (basic != null) basic.Connected += ApplyPassivePoison;
            RefreshPassive();
        }

        private void OnDisable()
        {
            actor.Health.Died -= ClearCombat;
            progression.Changed -= RefreshPassive;
            if (basic != null) basic.Connected -= ApplyPassivePoison;
            actor.Stats.RemoveAttributeContribution(PassiveSource);
            passiveRank = 0;
            ResetForManifestationLoad();
        }

        private void RefreshPassive()
        {
            int current = progression.Rank(AcolyteSkill.OccultAdept);
            if (current == passiveRank) return;
            passiveRank = current;
            if (current == 0) actor.Stats.RemoveAttributeContribution(PassiveSource);
            else actor.Stats.SetAttributeContribution(PassiveSource, new AttributeValues { intelligence = current * 2, spirit = current });
        }

        private void ApplyPassivePoison(CombatActor victim, DamageResult hit)
        {
            if (passiveRank > 0 && coffeePoison != null && actor.IsHostileTo(victim) && (hit.applied > 0 || hit.absorbed > 0))
                afflictions.ApplyPoison(victim);
        }

        public void SetAutoRetaliateSource(Func<bool> source) => autoRetaliate = source;

        private bool Busy() => otherChannels?.Any(ability => ability.IsChanneling) == true;

        public bool CanPrepare(AcolyteSkill skill) => skill != AcolyteSkill.OccultAdept && progression.Rank(skill) > 0 &&
            actor.CanAct && !IsChanneling && !Busy() && Remaining(skill) == 0 &&
            (skill == AcolyteSkill.SummonSkeleton ? SoulCharges >= 4 : resources.CanSpend(ResourceIds.Mana, ManaCost(skill))) &&
            (skill != AcolyteSkill.RadiationPlague || radiation != null);

        public bool Cast(AcolyteSkill skill, CombatActor victim)
        {
            if (!CanPrepare(skill)) return false;
            if (skill == AcolyteSkill.SummonSkeleton) return Summon();
            if (!ValidTarget(skill, victim) || !resources.TrySpend(ResourceIds.Mana, ManaCost(skill))) return false;
            basic?.Cancel();
            actor.Motor.Stop();
            readyAt[(int)skill] = Time.time + Cooldown(skill);
            int skillRank = progression.Rank(skill);
            if (skill == AcolyteSkill.WitheringCurse || skill == AcolyteSkill.RadiationPlague)
            {
                if (skill == AcolyteSkill.WitheringCurse) afflictions.ApplyCurse(victim, skillRank);
                else afflictions.ApplyPlague(victim, skillRank);
                EarlySkillVfx.PlayPriestPulse(AcolyteSkillProgression.Name(skill), victim.transform.position, .95f,
                    skill == AcolyteSkill.WitheringCurse ? new Color(.65f,.30f,.95f) : new Color(.58f,.91f,.32f));
                Casted?.Invoke(skill);
                return true;
            }
            channel = skill;
            rank = skillRank;
            origin = transform.position;
            target = victim;
            targetLife = victim.Health.DeathSequence;
            ticksLeft = skill == AcolyteSkill.DrainLife ? 5 : 1;
            end = Time.time + ticksLeft;
            nextTick = Time.time + 1;
            EarlySkillVfx.PlayPriestPulse(ChannelLabel, actor.transform.position, .9f, new Color(.68f,.40f,.95f), false);
            Casted?.Invoke(skill);
            return true;
        }

        private bool Summon()
        {
            var skeleton = AcolyteSkeleton.Raise(actor, progression.Rank(AcolyteSkill.SummonSkeleton), autoRetaliate);
            if (skeleton == null) return false;
            if (!resources.TrySpend(AcolyteSkillProgression.SoulResourceId, 4))
            {
                skeleton.Dismiss();
                return false;
            }
            skeletons.RemoveAll(unit => unit == null || !unit.isActiveAndEnabled);
            if (skeletons.Count >= 4)
            {
                skeletons[0].Dismiss();
                skeletons.RemoveAt(0);
            }
            skeletons.Add(skeleton);
            readyAt[(int)AcolyteSkill.SummonSkeleton] = Time.time + Cooldown(AcolyteSkill.SummonSkeleton);
            EarlySkillVfx.PlayPriestPulse("Summon Skeleton", skeleton.transform.position, 1.1f, new Color(.65f,.42f,1));
            Casted?.Invoke(AcolyteSkill.SummonSkeleton);
            return true;
        }

        private bool ValidTarget(AcolyteSkill skill, CombatActor candidate) =>
            candidate != null && candidate.Alive &&
            (actor.IsHostileTo(candidate) || (skill == AcolyteSkill.MortalCoil && actor.IsFriendlyTo(candidate))) &&
            (candidate == actor || (Vector3.Distance(transform.position, candidate.transform.position) <= Range + actor.Radius + candidate.Radius &&
                actor.HasSightOf(candidate)));

        private void Update()
        {
            if (progression.Active && actor.Alive)
            {
                afflictions.Tick(radiation, coffeePoison, passiveRank);
                skeletons.RemoveAll(unit => unit == null || !unit.isActiveAndEnabled);
                foreach (var skeleton in skeletons) skeleton.ShowSelected(selection?.Selected == skeleton.Actor);
            }
            else return;
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
            if (hit.applied > 0 || (draining && hit.absorbed > 0))
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
            ClearCombat();
            Array.Clear(readyAt, 0, readyAt.Length);
        }

        private void ClearCombat()
        {
            CancelChannel();
            afflictions.Clear();
            foreach (var skeleton in skeletons)
                if (skeleton != null) skeleton.Dismiss();
            skeletons.Clear();
        }

        public void Configure(AttackDefinition drain, AttackDefinition coil) { drainLife = drain; mortalCoil = coil; }
        public void ConfigureComplete(AttackDefinition plague, AttackDefinition poison)
        {
            radiation = plague;
            coffeePoison = poison;
        }
    }
}
