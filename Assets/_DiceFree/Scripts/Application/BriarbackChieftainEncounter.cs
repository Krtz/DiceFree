using System.Collections.Generic;
using System.Linq;
using DiceFree.AI;
using DiceFree.Combat;
using DiceFree.UI;
using UnityEngine;

namespace DiceFree.Briarback
{
    /// <summary>Single-phase outdoor boss behavior for the upper Briarback court.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(BasicAttack), typeof(AggroBehaviour))]
    public sealed class BriarbackChieftainEncounter : MonoBehaviour, IEncounterHudSource, IInterruptibleCombatCast, IChallengeThreatReceiver
    {
        public AttackDefinition overheadSmash;
        public AttackDefinition axeThrow;
        public CombatActor eliteWarrior;
        public CombatActor eliteBrute;
        [Min(0)] public float smashDamage = 520f;
        [Min(0)] public float axeDamage = 320f;
        [Min(0.1f)] public float axeSpeed = 16f;
        [Min(0.1f)] public float axeMaxRange = 24f;

        const string SmashBlock = "briarback.chieftain.smash";
        public const float SmashWindupSeconds = 1.5f;
        readonly Dictionary<CombatActor, float> threat = new();
        CombatActor actor;
        BasicAttack basic;
        AggroBehaviour aggro;
        float smashReadyAt, axeReadyAt, smashImpactAt;
        Vector3 smashForward;
        CombatActor smashTarget;
        GameObject telegraph;
        bool winding;

        public bool Winding => winding;
        public bool AllowsForcedChallenge => false;

        public void AddChallengeThreat(CombatActor source, float amount)
        {
            threat.TryGetValue(source, out float previous);
            threat[source] = previous + amount;
        }

        public bool TryInterruptCast()
        {
            if (!winding) return false;
            CancelSmash();
            return true;
        }
        public CombatActor EncounterActor => actor;
        public int LivingGuards => (eliteWarrior != null && eliteWarrior.Alive ? 1 : 0) + (eliteBrute != null && eliteBrute.Alive ? 1 : 0);
        public string CastLabel => "Overhead smash - leave the red strip";
        public float CastRemaining => winding ? Mathf.Clamp(smashImpactAt - Time.time, 0, SmashWindupSeconds) : 0;
        public float CastDuration => SmashWindupSeconds;
        public bool EngagedWith(CombatActor viewer) => isActiveAndEnabled && actor.Alive && !aggro.Returning && viewer != null &&
            (basic.Target == viewer || (winding && smashTarget == viewer) ||
             (eliteWarrior != null && eliteWarrior.Alive && eliteWarrior.GetComponent<BasicAttack>()?.Target == viewer) ||
             (eliteBrute != null && eliteBrute.Alive && eliteBrute.GetComponent<BasicAttack>()?.Target == viewer));
        public float SmashCooldownRemaining => Mathf.Max(0, smashReadyAt - Time.time);
        public float AxeCooldownRemaining => Mathf.Max(0, axeReadyAt - Time.time);
        public bool AxeThrowStarted { get; private set; }
        public int SmashImpacts { get; private set; }
        public int SmashAttempts { get; private set; }
        public int SmashHostileCandidates { get; private set; }
        public int SmashLineCandidates { get; private set; }
        public int AxeHits { get; private set; }

        void Awake()
        {
            actor = GetComponent<CombatActor>();
            basic = GetComponent<BasicAttack>();
            aggro = GetComponent<AggroBehaviour>();
        }

        void OnEnable()
        {
            EncounterHud.Register(this);
            actor.Health.Damaged += OnDamaged;
            actor.Health.Died += OnDied;
            actor.Health.Restored += OnRestored;
            CombatActor.ThreatReduced += ReduceThreat;
        }

        void OnDisable()
        {
            EncounterHud.Unregister(this);
            CombatActor.ThreatReduced -= ReduceThreat;
            if (actor?.Health != null)
            {
                actor.Health.Damaged -= OnDamaged;
                actor.Health.Died -= OnDied;
                actor.Health.Restored -= OnRestored;
            }
            CancelSmash();
        }

        void OnDamaged(CombatActor source, DamageResult result)
        {
            if (source == null || source.Faction != 0 || !source.Alive) return;
            threat[source] = threat.TryGetValue(source, out var total) ? total + Mathf.Max(1, result.applied) : Mathf.Max(1, result.applied);
            EngageGuards(source);
        }

        void ReduceThreat(CombatActor source, float retained)
        {
            if (threat.TryGetValue(source,out float amount)) threat[source] = amount * retained;
        }

        void OnDied()
        {
            CancelSmash();
            ResetLivingGuards();
        }

        void OnRestored()
        {
            CancelSmash();
            threat.Clear();
            smashReadyAt = axeReadyAt = 0;
            ResetLivingGuards(); // Dead guards are intentionally left to their own respawn timers.
        }

        void Update()
        {
            if (!actor.Alive) return;
            if (aggro.Returning)
            {
                CancelSmash();
                return;
            }

            PruneThreat();
            if (winding)
            {
                if (!actor.Alive || (actor.Effects != null && actor.Effects.Stunned) || smashTarget == null || !smashTarget.Alive || aggro.Returning) { CancelSmash(); return; }
                if (Time.time < smashImpactAt) return;
                ResolveSmash();
                CancelSmash();
                return;
            }

            var target = basic.Target;
            if (target == null) return;
            EngageGuards(target);

            float distance = Vector3.Distance(actor.transform.position, target.transform.position);
            if (distance <= 8f && Time.time >= smashReadyAt && overheadSmash != null)
            {
                BeginSmash(target);
                return;
            }

            if (distance > (basic.Definition?.reach ?? 1.1f) + actor.Radius + target.Radius &&
                Time.time >= axeReadyAt && axeThrow != null)
            {
                var ranged = HighestThreatOutsideMelee();
                if (ranged != null && Vector3.Distance(transform.position, ranged.transform.position) <= axeMaxRange &&
                    actor.HasSightOf(ranged))
                {
                    axeReadyAt = Time.time + 15f;
                    AxeThrowStarted = true;
                    StartCoroutine(ThrowAxe(ranged));
                }
            }
        }

        void BeginSmash(CombatActor target)
        {
            Vector3 toward = Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up);
            if (toward.sqrMagnitude < .001f) return;
            smashForward = toward.normalized;
            smashTarget = target;
            transform.rotation = Quaternion.LookRotation(smashForward);
            winding = true;
            smashReadyAt = Time.time + 12f;
            smashImpactAt = Time.time + SmashWindupSeconds;
            basic.Cancel();
            actor.Motor.Stop();
            actor.SetActionBlocked(SmashBlock, true);
            actor.Motor.SetMotionBlocked(SmashBlock, true);
            ShowSmashTelegraph();
        }

        void ResolveSmash()
        {
            BriarbackChieftainEffects.SmashImpact(transform.position + Vector3.up * .075f, smashForward);
            SmashAttempts++;
            SmashHostileCandidates = SmashLineCandidates = 0;
            foreach (var victim in CombatActor.All.ToArray())
            {
                if (!actor.IsHostileTo(victim)) continue;
                SmashHostileCandidates++;
                Vector3 delta = Vector3.ProjectOnPlane(victim.transform.position - transform.position, Vector3.up);
                float along = Vector3.Dot(delta, smashForward);
                float across = Mathf.Abs(Vector3.Dot(delta, Vector3.Cross(Vector3.up, smashForward)));
                if (along + victim.Radius < 0f || along - victim.Radius > 6f || across > .5f + victim.Radius) continue;
                SmashLineCandidates++;

                var packet = DamageResolver.Calculate(actor.Stats, victim.Stats, overheadSmash,
                    rawMultiplier: overheadSmash.RollDamageMultiplier(actor.Stats.Attributes), skillFlatDamage: smashDamage);
                packet = victim.Health.ApplyPacket(actor, packet);
                if (packet.Missed) continue;
                SmashImpacts++;
                // Shared status rules resolve shield absorption, resistance and immunity.
                victim.Effects?.TryApplyStun(1f, false);
            }
        }

        System.Collections.IEnumerator ThrowAxe(CombatActor target)
        {
            basic.Cancel();
            actor.Motor.Stop();
            Vector3 facing = Vector3.ProjectOnPlane(target.transform.position - transform.position, Vector3.up);
            if (facing.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(facing);
            yield return new WaitForSeconds(.65f); // Provisional wind-up; the design leaves Axe Throw wind-up open.
            AxeThrowStarted = false;
            if (!actor.CanTargetHostile(target) || aggro.Returning) yield break;
            var go = new GameObject();
            go.name = "Briarback Chieftain homing axe";
            go.layer = 2;
            go.transform.position = transform.position + transform.forward * .9f + Vector3.up * 1.3f;
            BriarbackChieftainEffects.DressAxe(go);
            var projectile = go.AddComponent<BriarbackHomingAxe>();
            projectile.Launch(actor, target, axeThrow, axeDamage, axeSpeed);
        }

        CombatActor HighestThreatOutsideMelee()
        {
            float melee = (basic.Definition?.reach ?? 1.1f) + actor.Radius;
            return threat.Where(pair => actor.CanTargetHostile(pair.Key) && pair.Key.Faction == 0 &&
                    Vector3.Distance(transform.position, pair.Key.transform.position) > melee)
                .OrderByDescending(pair => pair.Value).ThenBy(pair => (pair.Key.transform.position - transform.position).sqrMagnitude)
                .Select(pair => pair.Key).FirstOrDefault();
        }

        void PruneThreat()
        {
            foreach (var key in threat.Keys.Where(key => key == null || !key.Alive || key.Faction != 0).ToArray()) threat.Remove(key);
        }

        void EngageGuards(CombatActor target)
        {
            if (target == null || !target.Alive) return;
            foreach (var guard in new[] { eliteWarrior, eliteBrute })
                if (guard != null && guard.Alive) guard.GetComponent<BasicAttack>()?.Order(target);
        }

        void ResetLivingGuards()
        {
            foreach (var guard in new[] { eliteWarrior, eliteBrute })
            {
                if (guard == null || !guard.Alive) continue;
                guard.GetComponent<AggroBehaviour>()?.ResetEncounter();
            }
        }

        void ShowSmashTelegraph()
        {
            telegraph = BriarbackChieftainEffects.Warning(transform.position + Vector3.up * .07f, smashForward);
        }

        void CancelSmash()
        {
            winding = false;
            smashTarget = null;
            AxeThrowStarted = false;
            if (actor != null) { actor.SetActionBlocked(SmashBlock, false); actor.Motor.SetMotionBlocked(SmashBlock, false); }
            if (telegraph != null) Destroy(telegraph);
            telegraph = null;
        }

    }

    /// <summary>Homing axe using the same swept collision policy as the existing Ironpine homing quill.</summary>
    public sealed class BriarbackHomingAxe : MonoBehaviour
    {
        CombatActor owner, target;
        AttackDefinition definition;
        float damage, speed, expires;
        Vector3 velocity;

        public void Launch(CombatActor source, CombatActor recipient, AttackDefinition attack, float flatDamage, float projectileSpeed)
        {
            owner = source; target = recipient; definition = attack; damage = flatDamage;
            speed = projectileSpeed; expires = Time.time + 8f;
            velocity = (recipient.transform.position + Vector3.up * .85f - transform.position).normalized * speed;
        }

        void Update()
        {
            if (target == null || !target.Alive || owner == null || !owner.Alive || Time.time >= expires) { Destroy(gameObject); return; }
            Vector3 desired = (target.transform.position + Vector3.up * .85f - transform.position).normalized;
            velocity = Vector3.RotateTowards(velocity.normalized, desired, 240f * Mathf.Deg2Rad * Time.deltaTime, 0) * speed;
            float length = velocity.magnitude * Time.deltaTime;
            Vector3 direction = velocity.normalized;
            var hits = Physics.SphereCastAll(transform.position, .22f, direction, length, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(owner.transform)) continue;
                var hitActor = hit.collider.GetComponentInParent<CombatActor>();
                if (hitActor == target)
                {
                    BriarbackChieftainEffects.AxeImpact(hit.point);
                    if (owner.IsHostileTo(target))
                    {
                        var packet = DamageResolver.Calculate(owner.Stats, target.Stats, definition,
                            rawMultiplier: definition.RollDamageMultiplier(owner.Stats.Attributes), skillFlatDamage: damage);
                        target.Health.ApplyPacket(owner, packet);
                    }
                    Destroy(gameObject); return;
                }
                if (hitActor != null || hit.collider.gameObject.layer == 9) { BriarbackChieftainEffects.AxeImpact(hit.point); Destroy(gameObject); return; }
            }
            transform.position += direction * length;
            if (direction.sqrMagnitude > .001f) transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
