using System;
using System.Collections.Generic;
using System.Linq;
using DiceFree.Combat;
using DiceFree.Foundation;
using UnityEngine;
using UnityEngine.AI;

namespace DiceFree.Skills
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(ThiefSkillProgression), typeof(ActorResourceController))]
    public sealed class ThiefSkillCaster : MonoBehaviour, IManifestationSessionState
    {
        public const float EnvenomRange = 1.05f;
        public const float SmokeRange = 15;
        public const float FanRadius = 4;
        private const string PassiveId = "passive.thief.cutpurse";
        private static readonly int[] Mana = { 20, 12, 16, 18, 20, 0 };
        private static readonly int[] Cooldowns = { 24, 8, 18, 9, 18, 0 };

        [SerializeField] private AttackDefinition stab, naturePoison, fan, techSmoke;
        private readonly float[] readyAt = new float[ThiefSkillProgression.SkillCount];
        private readonly List<ThiefSmokeCloud> clouds = new();
        private readonly List<CombatActor> fanTargets = new();
        private CombatActor actor;
        private BasicAttack basic;
        private ActorResourceController resources;
        private ThiefSkillProgression progression;
        private IChannelingAbility[] channels;
        private float concealUntil, keenUntil;
        private int passive;
        public event Action<ThiefSkill> Casted;
        public bool Hidden => actor.Concealed && Time.time < concealUntil;
        public float StealthRemaining => Hidden ? concealUntil - Time.time : 0;
        public float KeenEdgeRemaining => Mathf.Max(0, keenUntil - Time.time);
        internal int Session { get; private set; }
        internal bool Active => isActiveAndEnabled && progression.Active && actor.Alive;
        public static int Cost(ThiefSkill skill) => (uint)skill < Mana.Length ? Mana[(int)skill] : 0;
        public static int Cooldown(ThiefSkill skill) => (uint)skill < Cooldowns.Length ? Cooldowns[(int)skill] : 0;
        public static float StealthDuration(int rank) => 4 + 4 * (rank - 1) / 15f;
        public static float StealthOpener(int rank) => 1.1f + .9f * (rank - 1) / 15f;
        public static float KeenDuration(int rank) => 1 + (rank - 1) / 3f;
        public float Remaining(ThiefSkill skill) => (uint)skill < readyAt.Length ? Mathf.Max(0, readyAt[(int)skill] - Time.time) : 0;

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            basic = GetComponent<BasicAttack>();
            resources = GetComponent<ActorResourceController>();
            progression = GetComponent<ThiefSkillProgression>();
        }

        private void Start() => channels = GetComponents<MonoBehaviour>().OfType<IChannelingAbility>().ToArray();

        private void OnEnable()
        {
            actor.Health.Died += ClearEffects;
            progression.Changed += RefreshPassive;
            RefreshPassive();
        }

        private void OnDisable()
        {
            actor.Health.Died -= ClearEffects;
            progression.Changed -= RefreshPassive;
            actor.Stats.RemoveAttributeContribution(PassiveId);
            passive = 0;
            ResetForManifestationLoad();
        }

        private void Update()
        {
            if (concealUntil > 0 && (Time.time >= concealUntil || !actor.Concealed || !progression.Active))
            {
                concealUntil = 0;
                actor.Reveal();
            }
            if (clouds.Count != 0) clouds.RemoveAll(cloud => cloud == null);
        }

        private void RefreshPassive()
        {
            int rank = progression.Rank(ThiefSkill.Cutpurse);
            if (passive == rank) return;
            passive = rank;
            if (rank > 0) actor.Stats.SetAttributeContribution(PassiveId, new AttributeValues { agility = rank });
            else actor.Stats.RemoveAttributeContribution(PassiveId);
        }

        public bool CanPrepare(ThiefSkill skill) =>
            skill != ThiefSkill.Cutpurse && progression.Rank(skill) > 0 && actor.CanAct &&
            Remaining(skill) == 0 && resources.CanSpend(ResourceIds.Mana, Cost(skill)) &&
            channels?.Any(channel => channel.IsChanneling) != true;

        public bool Cast(ThiefSkill skill, CombatActor target = null)
        {
            if (skill == ThiefSkill.SmokeBomb || !CanPrepare(skill)) return false;
            if (skill == ThiefSkill.Envenom && (!actor.CanTargetHostile(target) ||
                Vector3.Distance(transform.position,target.transform.position) > EnvenomRange + actor.Radius + target.Radius ||
                !actor.HasSightOf(target))) return false;
            if (!Spend(skill)) return false;
            int rank = progression.Rank(skill);
            switch (skill)
            {
                case ThiefSkill.Stealth:
                    if (basic?.Target != null) basic.Cancel();
                    concealUntil = Time.time + StealthDuration(rank);
                    actor.Conceal(.05f);
                    EarlySkillVfx.PlayPriestPulse("Stealth",transform.position,.72f,new Color(.40f,.45f,.61f));
                    break;
                case ThiefSkill.Envenom:
                    ResolveEnvenom(target,rank);
                    break;
                case ThiefSkill.KeenEdge:
                    keenUntil = Time.time + KeenDuration(rank);
                    break;
                case ThiefSkill.FanOfKnives:
                    ResolveFan(rank);
                    break;
            }
            Casted?.Invoke(skill);
            return true;
        }

        public bool CastSmoke(Vector3 point)
        {
            const ThiefSkill skill = ThiefSkill.SmokeBomb;
            if (!CanPrepare(skill) || Vector3.Distance(transform.position,point) > SmokeRange + actor.Radius ||
                !NavMesh.SamplePosition(point,out var ground,.75f,NavMesh.AllAreas) ||
                Physics.Linecast(transform.position+Vector3.up*.8f,ground.position+Vector3.up*.2f,1<<9,QueryTriggerInteraction.Ignore) ||
                !Spend(skill)) return false;
            actor.Reveal();
            clouds.Add(ThiefSmokeCloud.Place(actor,ground.position,techSmoke,progression.Rank(skill)));
            Casted?.Invoke(skill);
            return true;
        }

        private bool Spend(ThiefSkill skill)
        {
            if (!resources.TrySpend(ResourceIds.Mana, Cost(skill))) return false;
            readyAt[(int)skill] = Time.time + Cooldown(skill);
            return true;
        }

        public float ConsumeOpener()
        {
            if (!Hidden) return 1;
            float multiplier = StealthOpener(progression.Rank(ThiefSkill.Stealth));
            actor.Reveal();
            concealUntil = 0;
            return multiplier;
        }

        public CriticalRule BasicCriticalRule()
        {
            if (!Active) return null;
            float chance = KeenEdgeRemaining > 0 ? 1 : .01f * passive;
            return chance > 0 ? new CriticalRule(ThiefSkillProgression.ThiefClassId,"thief.basic-crit",true,chance,1.5f) : null;
        }

        internal static float PeriodicScale(int rank) => 1 + .05f * (rank - 1);
        private static float FanScale(int rank) => 1 + .06f * (rank - 1);

        public Vector2 PreviewStab()
        {
            float opener = Hidden ? StealthOpener(progression.Rank(ThiefSkill.Stealth)) : 1;
            return basic.DamageRange * (2 * opener) + Vector2.one * stab.RawDamage(actor.Stats.Attributes);
        }

        public float PreviewDamage(ThiefSkill skill, int rank)
        {
            var attributes = actor.Stats.Attributes;
            return skill switch
            {
                ThiefSkill.Envenom => naturePoison.RawDamage(attributes) * PeriodicScale(rank),
                ThiefSkill.FanOfKnives => fan.RawDamage(attributes) * FanScale(rank),
                ThiefSkill.SmokeBomb => techSmoke.RawDamage(attributes) * PeriodicScale(rank),
                _ => 0
            };
        }

        private void ResolveEnvenom(CombatActor target, int rank)
        {
            float opener = ConsumeOpener();
            var range = basic.DamageRange;
            var action = AccuracyResolver.Resolve(actor,stab);
            var hit = action.Hit.missed ? new DamageResult { action = action, channel = stab.channel } :
                DamageResolver.Calculate(actor.Stats,target.Stats,stab,skillFlatDamage:2 * UnityEngine.Random.Range(range.x,range.y) * opener);
            hit.action = action;
            target.Health.ApplyPacket(actor,hit);
            if (!hit.Missed && target.Alive)
                ThiefPoison.Apply(target,this,actor,naturePoison,rank);
            EarlySkillVfx.PlayPriestPulse("Envenom",target.transform.position,.45f,new Color(.61f,.92f,.28f));
        }

        private void ResolveFan(int rank)
        {
            actor.Reveal();
            fanTargets.Clear();
            foreach (var target in CombatActor.All)
                if (actor.IsHostileTo(target) && Within(transform.position,target.transform.position,FanRadius) && actor.HasSightOf(target))
                    fanTargets.Add(target);
            foreach (var target in fanTargets)
                if (actor.IsHostileTo(target))
                    target.Health.ApplyPacket(actor,DamageResolver.Calculate(actor.Stats,target.Stats,fan,
                        rawMultiplier:FanScale(rank)));
            fanTargets.Clear();
            ThiefSmokeCloud.KnifeFan(transform.position);
        }

        internal static bool Within(Vector3 center,Vector3 point,float radius)
        {
            var difference = point-center;
            return Mathf.Abs(difference.y)<1.7f && difference.x*difference.x+difference.z*difference.z<=radius*radius;
        }

        private void ClearEffects()
        {
            actor.Reveal();
            concealUntil = keenUntil = 0;
            foreach (var cloud in clouds) if (cloud != null) cloud.Dismiss();
            clouds.Clear();
            Session++;
        }

        public void ResetForManifestationLoad()
        {
            ClearEffects();
            Array.Clear(readyAt,0,readyAt.Length);
        }

        public void Configure(AttackDefinition direct,AttackDefinition poison,AttackDefinition knives,AttackDefinition smoke)
        {
            stab = direct;
            naturePoison = poison;
            fan = knives;
            techSmoke = smoke;
        }
    }

    [DisallowMultipleComponent]
    internal sealed class ThiefPoison : MonoBehaviour
    {
        private CombatActor target, source;
        private ThiefSkillCaster caster;
        private AttackDefinition attack;
        private double nextTick, expires;
        private uint life;
        private int rank, session;

        public static void Apply(CombatActor victim, ThiefSkillCaster owner, CombatActor attacker, AttackDefinition attack, int rank)
        {
            var poison = victim.GetComponent<ThiefPoison>() ?? victim.gameObject.AddComponent<ThiefPoison>();
            double now = Time.timeAsDouble;
            if (!poison.Valid() || now > poison.expires) poison.nextTick = now + 1;
            poison.target = victim;
            poison.source = attacker;
            poison.caster = owner;
            poison.attack = attack;
            poison.rank = rank;
            poison.session = owner.Session;
            poison.life = victim.Health.DeathSequence;
            poison.expires = now + 3;
            poison.enabled = true;
        }

        private bool Valid() => caster != null && caster.Active && caster.Session == session &&
            target != null && target.Alive && target.Health.DeathSequence == life;

        private void Update()
        {
            double now = Time.timeAsDouble;
            while (Valid() && nextTick <= now && nextTick <= expires)
            {
                nextTick++;
                target.Health.ApplyPacket(source,DamageResolver.Calculate(source.Stats,target.Stats,attack,
                    rawMultiplier:ThiefSkillCaster.PeriodicScale(rank)));
            }
            if (!Valid() || now > expires)
            {
                caster = null;
                enabled = false;
            }
        }
    }
}
