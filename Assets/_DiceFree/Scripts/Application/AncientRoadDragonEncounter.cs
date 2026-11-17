using System;
using System.Collections.Generic;
using DiceFree.Combat;
using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Dungeons
{
    [RequireComponent(typeof(CombatActor))]
    public sealed class AncientRoadDragonEncounter : MonoBehaviour, IEncounterHudSource, IInterruptibleCombatCast
    {
        private enum Attack { Bite, Breath, Tail, Stonefall }
        private static readonly Attack[][] Patterns =
        {
            new[] { Attack.Breath, Attack.Bite, Attack.Stonefall },
            new[] { Attack.Tail, Attack.Breath, Attack.Stonefall, Attack.Bite },
            new[] { Attack.Stonefall, Attack.Breath, Attack.Tail }
        };

        [SerializeField] private AttackDefinition bite, breath, tail, stonefall;
        [SerializeField] private Material telegraphMaterial;
        [SerializeField] private Transform model;
        [SerializeField] private Transform neck;
        [SerializeField] private Transform jaw;
        [SerializeField] private Transform tailRoot;
        [SerializeField] private DungeonRewardPool rewards;

        private readonly DragonDangerZone[] zones = new DragonDangerZone[3];
        private readonly DragonAttackTelegraph[] warnings = new DragonAttackTelegraph[3];
        private Action onDefeated;
        private Action<string> witness;
        private IReadOnlyList<CombatActor> participants = Array.Empty<CombatActor>();
        private CombatActor actor;
        private Animator animator;
        private Material impactMaterial;
        private Vector3 facing, originalScale;
        private string caption;
        private float captionUntil;
        private int attackIndex, attackPhase, zoneCount;
        private float readyAt, impactAt, duration, fallAt = -1;
        private bool fighting, winding;
        private Attack casting;
        private Vector3 modelPosition;
        private Quaternion modelRotation;

        public CombatActor EncounterActor => actor;
        public int LivingGuards => 0;
        public int Phase { get; private set; } = 1;
        public bool Fighting => fighting;
        public string Caption => Time.time < captionUntil ? caption : null;
        public DungeonRewardPool Rewards => rewards;
        public string EncounterStatus => Phase switch { 1 => "The Warden of the Road", 2 => "The ridge trembles", _ => "Last stand" };
        public string CastLabel => casting switch
        {
            Attack.Bite => "Rending bite · Step away from the jaws",
            Attack.Breath => "Verdant breath · Move out of the cone",
            Attack.Tail => "Tail sweep · Move in front of the dragon",
            _ => "Stonefall · Leave the marked circles"
        };
        public float CastDuration => duration;
        public float CastRemaining => winding ? Mathf.Max(0, impactAt - Time.time) : 0;
        public bool EngagedWith(CombatActor viewer) => fighting && Includes(viewer);

        public void Configure(Transform visual, Transform neckJoint, Transform jawJoint, Transform tailJoint,
            AttackDefinition biteAttack, AttackDefinition breathAttack, AttackDefinition tailAttack,
            AttackDefinition stoneAttack, Material warningsMaterial, DungeonRewardPool loot)
        {
            model = visual;
            neck = neckJoint;
            jaw = jawJoint;
            tailRoot = tailJoint;
            bite = biteAttack;
            breath = breathAttack;
            tail = tailAttack;
            stonefall = stoneAttack;
            telegraphMaterial = warningsMaterial;
            rewards = loot;
        }

        public void Arm(IReadOnlyList<CombatActor> roster, Action defeated, Action<string> witness)
        {
            onDefeated = defeated;
            participants = roster;
            this.witness = witness;
        }

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            animator = model.GetComponent<Animator>();
            impactMaterial = DragonAttackTelegraph.CreateImpactMaterial();
            originalScale = model.localScale;
            modelPosition = model.localPosition;
            modelRotation = model.localRotation;
        }

        private void OnEnable()
        {
            EncounterHud.Register(this);
            actor.Health.Damaged += OnDamaged;
            actor.Health.Defeated += OnDefeated;
        }

        private void OnDisable()
        {
            EncounterHud.Unregister(this);
            actor.Health.Damaged -= OnDamaged;
            actor.Health.Defeated -= OnDefeated;
            CancelCast();
        }

        private void OnDamaged(CombatActor source, DamageResult result)
        {
            if (!fighting && OwnsKill(source)) Engage();
        }

        private bool OwnsKill(CombatActor source) => source != null &&
            (Includes(source) || Includes(source.GetComponent<DefeatReporter>()?.CreditOwner));

        private bool Includes(CombatActor candidate)
        {
            if (candidate == null) return false;
            foreach (var participant in participants)
                if (candidate == participant) return true;
            return false;
        }

        private CombatActor FindTarget(float minimumOffset)
        {
            CombatActor target = null;
            float distance = float.PositiveInfinity;
            foreach (var participant in participants)
            {
                if (participant == null || !participant.Alive ||
                    participant.transform.position.z < AncientRoadDragonLairPortal.Origin.z + minimumOffset)
                    continue;
                float candidate = (participant.transform.position - transform.position).sqrMagnitude;
                if (candidate >= distance || !actor.HasSightOf(participant)) continue;
                target = participant;
                distance = candidate;
            }
            return target;
        }

        private void OnDefeated(CombatActor source)
        {
            bool credited = OwnsKill(source);
            Cease();
            if (animator != null) animator.speed = 0;
            fallAt = Time.time;
            if (credited) onDefeated?.Invoke();
        }

        public void Cease()
        {
            fighting = false;
            CancelCast();
            if (actor.Alive) actor.Health.Invulnerable = true;
            participants = Array.Empty<CombatActor>();
        }

        private void Engage()
        {
            if (onDefeated == null || !actor.Alive) return;
            fighting = true;
            caption = "I know you, Echo. You will get no answers from me.";
            captionUntil = Time.time + 4.3f;
            readyAt = Time.time + 2.85f;
        }

        private void Update()
        {
            if (onDefeated == null || !actor.Alive) return;
            if (!fighting)
            {
                if (FindTarget(-7) != null) Engage();
                if (!fighting) return;
            }

            float health = actor.Health.Current / actor.Health.Maximum;
            Phase = health <= .32f ? 3 : health <= .68f ? 2 : 1;
            if (winding)
            {
                if (!actor.CanAct) { CancelCast(); return; }
                float progress = 1 - CastRemaining / duration;
                for (int i = 0; i < zoneCount; i++) warnings[i].Windup(progress);
                if (Time.time >= impactAt) Impact();
                return;
            }
            if (Time.time < readyAt || !actor.CanAct) return;
            var target = FindTarget(-26);
            if (target != null) CastNext(target);
        }

        private void CastNext(CombatActor target)
        {
            if (attackPhase != Phase) { attackPhase = Phase; attackIndex = 0; }
            var pattern = Patterns[Phase - 1];
            casting = pattern[attackIndex++ % pattern.Length];
            Vector3 at = transform.position;
            Vector3 direction = Vector3.ProjectOnPlane(target.transform.position - at, Vector3.up);
            if (direction.sqrMagnitude < .01f) direction = transform.forward;
            float distance = direction.magnitude;
            facing = direction.normalized;
            if (casting == Attack.Bite && distance > 7.6f) casting = Attack.Stonefall;
            if (casting == Attack.Breath && distance > 21) casting = Attack.Stonefall;

            zoneCount = 1;
            Color color;
            switch (casting)
            {
                case Attack.Bite:
                    color = new Color(1f,.56f,.18f);
                    duration = .86f;
                    zones[0] = new DragonDangerZone(at, facing, 7.8f, 51);
                    break;
                case Attack.Breath:
                    color = new Color(.56f,1f,.19f);
                    duration = Phase == 3 ? 1.45f : 1.85f;
                    zones[0] = new DragonDangerZone(at, facing, 21, Phase == 3 ? 42 : 33);
                    break;
                case Attack.Tail:
                    color = new Color(1f,.42f,.12f);
                    duration = Phase == 3 ? 1.35f : 1.65f;
                    facing = transform.forward;
                    zones[0] = new DragonDangerZone(at, -facing, 12.0f, 103);
                    break;
                default:
                    color = new Color(.82f,.31f,.95f);
                    duration = Phase == 3 ? 1.45f : 1.75f;
                    Vector3 mark = target.transform.position;
                    zones[0] = new DragonDangerZone(mark, facing, Phase == 3 ? 4.5f : 3.7f);
                    if (Phase > 1)
                    {
                        zoneCount = 3;
                        var sideways = Vector3.Cross(Vector3.up, facing);
                        zones[1] = new DragonDangerZone(mark + sideways * 6.3f + facing * 1.7f, facing, 3.1f);
                        zones[2] = new DragonDangerZone(mark - sideways * 6.3f + facing * 1.7f, facing, 3.1f);
                    }
                    break;
            }
            for (int i = 0; i < zoneCount; i++)
                warnings[i] = DragonAttackTelegraph.Create(transform.parent, zones[i], color, telegraphMaterial);
            impactAt = Time.time + duration;
            winding = true;
            witness?.Invoke("mechanic.dragon-" + casting.ToString().ToLowerInvariant());
        }

        private void Impact()
        {
            var attack = casting switch
            {
                Attack.Bite => bite,
                Attack.Breath => breath,
                Attack.Tail => tail,
                _ => stonefall
            };
            int count = zoneCount;
            winding = false;
            readyAt = Time.time + (Phase == 3 ? 1.65f : Phase == 2 ? 2.35f : 2.7f);
            for (int i = 0; i < count; i++)
            {
                warnings[i].Impact();
                warnings[i] = null;
            }
            if (casting == Attack.Breath)
            {
                var mouth = jaw != null ? jaw.position : transform.position + Vector3.up * 2.5f;
                DragonAttackTelegraph.EmitImpact(transform.parent, mouth, facing, impactMaterial, true);
            }
            else if (casting == Attack.Stonefall)
            {
                for (int i = 0; i < count; i++)
                    DragonAttackTelegraph.EmitImpact(transform.parent, zones[i].Center + Vector3.up * .25f,
                        Vector3.up, impactMaterial, false);
            }
            zoneCount = 0;
            foreach (var participant in participants)
            {
                if (participant == null || !participant.Alive || !actor.HasSightOf(participant)) continue;
                for (int i = 0; i < count; i++)
                {
                    if (!zones[i].Contains(participant.transform.position)) continue;
                    DamageResolver.Hit(actor, participant, attack);
                    break;
                }
            }
        }

        public bool TryInterruptCast()
        {
            if (!winding) return false;
            CancelCast();
            return true;
        }

        private void CancelCast()
        {
            winding = false;
            readyAt = Time.time + 1.6f;
            for (int i = 0; i < zoneCount; i++)
            {
                if (warnings[i] != null) Destroy(warnings[i].gameObject);
                warnings[i] = null;
            }
            zoneCount = 0;
        }

        private void LateUpdate()
        {
            float charge = winding ? 1 - CastRemaining / duration : 0;
            if (winding && casting != Attack.Stonefall)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(facing), Time.deltaTime * 400);
            if (fallAt >= 0)
            {
                float down = Mathf.SmoothStep(0, 1, (Time.time - fallAt) / .86f);
                model.localRotation = modelRotation * Quaternion.Euler(0, 0, -18 * down);
                model.localPosition = modelPosition - Vector3.up * (1.08f * down);
                model.localScale = Vector3.Scale(originalScale, new Vector3(1 + .08f * down, 1 - .35f * down, 1));
                return;
            }
            // Imported idle animation supplies each frame's pose; attack motion adds to it.
            float rise = Mathf.Sin(charge * Mathf.PI * .75f);
            if (neck != null) neck.localRotation *= Quaternion.Euler((casting == Attack.Breath ? -23 : -12) * rise, 0, 0);
            if (jaw != null) jaw.localRotation *= Quaternion.Euler(27 * rise, 0, 0);
            if (tailRoot != null && casting == Attack.Tail)
                tailRoot.localRotation *= Quaternion.Euler(0, 52 * rise, 0);
            model.localScale = Vector3.Scale(originalScale,
                new Vector3(1 + rise * .035f, 1 + rise * .14f, 1 - rise * .028f));
        }

        private void OnDestroy() => Destroy(impactMaterial);
    }
}

