using System;
using System.Collections;
using System.Collections.Generic;
using DiceFree.Foundation;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CombatActor), typeof(MagicalSkillProgression))]
    [RequireComponent(typeof(ActorResourceController))]
    public sealed class MagicalSkillCaster : MonoBehaviour, IManifestationSessionState
    {
        private readonly Dictionary<string, float> readyAt = new(StringComparer.Ordinal);

        private CombatActor actor;
        private MagicalSkillProgression progression;
        private ActorResourceController resources;
        private BasicAttack basicAttack;

        public string Feedback { get; private set; } = "";
        public event Action<MagicalSkillDefinition> Casted;

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            progression = GetComponent<MagicalSkillProgression>();
            resources = GetComponent<ActorResourceController>();
            basicAttack = GetComponent<BasicAttack>();
        }

        public float CooldownRemaining(MagicalSkillDefinition definition)
        {
            if (definition == null || !readyAt.TryGetValue(definition.stableId, out float time)) return 0;
            return Mathf.Max(0, time - Time.time);
        }

        public bool CastSlot(int slot) => Fail("Choose a target for the skill first.");

        public bool CanPrepare(MagicalSkillDefinition definition)
        {
            if (!ValidateActive(definition, out int rank)) return false;
            if (!resources.CanSpend(ResourceIds.Mana, definition.ManaCost(rank))) return Fail("Not enough Mana.");
            return true;
        }

        public bool Cast(MagicalSkillDefinition definition, CombatActor requestedTarget = null)
        {
            if (!ValidateActive(definition, out int rank)) return false;
            if (definition.kind == MagicalSkillKind.IceBurst)
                return Fail("Ice Burst requires a ground target.");

            bool hostile = definition.kind == MagicalSkillKind.MagicSand;
            CombatActor target = hostile
                ? requestedTarget
                : ResolveSupportTarget(requestedTarget);

            if (hostile && !actor.IsHostileTo(target)) return Fail("Select a hostile target.");
            if (!hostile && !actor.IsFriendlyTo(target)) return Fail("No valid friendly target.");
            if (!InRange(target, definition.range)) return Fail("Target is out of range.");
            if (target != actor && !actor.HasSightOf(target)) return Fail("Target is not in sight.");
            if ((definition.kind == MagicalSkillKind.MagicSand ||
                 definition.kind == MagicalSkillKind.FireImbuement) &&
                target.Effects == null)
                return Fail("Target cannot receive this magical effect.");

            float cost = definition.ManaCost(rank);
            if (!resources.CanSpend(ResourceIds.Mana, cost)) return Fail("Not enough Mana.");

            StopForCast();
            if (!resources.TrySpend(ResourceIds.Mana, cost)) return Fail("Not enough Mana.");

            switch (definition.kind)
            {
                case MagicalSkillKind.MagicSand:
                    EarlySkillVfx.Play(EarlySkillVfx.Cue.MagicalSand, target.transform.position);
                    if (definition.attack == null) return FailAfterSpend("Magic Sand attack data is missing.", cost);
                    DamageResolver.Hit(actor, target, definition.attack, null, definition.DamageCoefficient(rank));
                    if (target.Alive)
                        target.Effects.ApplyAccuracyPenalty(
                            EffectId(definition),
                            definition.MagicSandMissChance(rank),
                            definition.durationSeconds);
                    break;

                case MagicalSkillKind.Mend:
                    EarlySkillVfx.Play(EarlySkillVfx.Cue.MagicalMend, target.transform.position);
                    target.Health.HealFrom(actor.Stats, definition.HealAmount(actor.Stats.Attributes, rank));
                    break;

                case MagicalSkillKind.FireImbuement:
                    EarlySkillVfx.Play(EarlySkillVfx.Cue.MagicalFire, target.transform.position);
                    if (definition.secondaryAttack == null)
                        return FailAfterSpend("Fire Imbuement attack data is missing.", cost);
                    target.Effects.ApplyBasicAttackAugment(
                        EffectId(definition),
                        definition.secondaryAttack,
                        definition.ImbuementCoefficient(rank),
                        definition.durationSeconds);
                    break;

                default:
                    return FailAfterSpend("Passive/ground skills cannot use a unit-target cast.", cost);
            }

            CompleteCast(definition);
            return true;
        }

        public bool CastIceBurst(MagicalSkillDefinition definition, Vector3 point)
        {
            if (!ValidateActive(definition, out int rank)) return false;
            if (definition.kind != MagicalSkillKind.IceBurst) return Fail("That skill is not ground targeted.");

            var flatA = new Vector2(actor.transform.position.x, actor.transform.position.z);
            var flatB = new Vector2(point.x, point.z);
            if (Vector2.Distance(flatA, flatB) > definition.range + actor.Radius)
                return Fail("Ground target is out of range.");

            float cost = definition.ManaCost(rank);
            if (!resources.CanSpend(ResourceIds.Mana, cost)) return Fail("Not enough Mana.");

            StopForCast();
            if (!resources.TrySpend(ResourceIds.Mana, cost)) return Fail("Not enough Mana.");
            readyAt[definition.stableId] = Time.time + Mathf.Max(0, definition.cooldownSeconds);
            Feedback = definition.displayName + " forming...";
            Casted?.Invoke(definition);
            StartCoroutine(IceBurst(definition, rank, point));
            return true;
        }

        public void ResetForManifestationLoad()
        {
            StopAllCoroutines();
            readyAt.Clear();
            Feedback = "";
        }

        private IEnumerator IceBurst(MagicalSkillDefinition definition, int rank, Vector3 point)
        {
            EarlySkillVfx.Play(EarlySkillVfx.Cue.MagicalIceWarning, point, definition.iceRadius);
            if (definition.iceDelaySeconds > 0)
                yield return new WaitForSeconds(definition.iceDelaySeconds);
            ResolveIceBurst(definition, rank, point);
        }

        private void ResolveIceBurst(MagicalSkillDefinition definition, int rank, Vector3 point)
        {
            foreach (var target in FindObjectsByType<CombatActor>())
            {
                if (target == null || !actor.IsHostileTo(target)) continue;
                var flatA = new Vector2(target.transform.position.x, target.transform.position.z);
                var flatB = new Vector2(point.x, point.z);
                if (Vector2.Distance(flatA, flatB) > definition.iceRadius + target.Radius) continue;

                DamageResolver.Hit(actor, target, definition.attack, null, definition.DamageCoefficient(rank));
                if (target.Alive && target.Effects != null)
                    target.Effects.ApplyMovementSpeedDebuff(
                        EffectId(definition),
                        definition.IceSlowPercent(rank),
                        definition.slowDurationSeconds);
            }

            EarlySkillVfx.Play(EarlySkillVfx.Cue.MagicalIceImpact, point, definition.iceRadius);
            Feedback = definition.displayName + " exploded.";
        }

        private bool ValidateActive(MagicalSkillDefinition definition, out int rank)
        {
            rank = 0;
            if (!progression.ActiveForCurrentClass) return Fail("Magically Touched skills are inactive for this manifestation.");
            if (definition == null || !definition.Active) return Fail("No active magical skill in that slot.");
            rank = progression.Rank(definition.stableId);
            if (rank <= 0) return Fail(definition.displayName + " is unlearned.");
            if (!actor.CanAct) return Fail("Cannot use skills right now.");
            if (CooldownRemaining(definition) > 0) return Fail(definition.displayName + " is cooling down.");
            if (!resources.Has(ResourceIds.Mana)) return Fail("Mana is not active.");
            return true;
        }

        private void CompleteCast(MagicalSkillDefinition definition)
        {
            readyAt[definition.stableId] = Time.time + Mathf.Max(0, definition.cooldownSeconds);
            Feedback = definition.displayName + " used.";
            Casted?.Invoke(definition);
        }

        private void StopForCast()
        {
            basicAttack?.Cancel();
            actor.Motor.Stop();
        }

        private CombatActor ResolveSupportTarget(CombatActor requested)
        {
            return requested != null && actor.IsFriendlyTo(requested) ? requested : null;
        }

        private bool InRange(CombatActor target, float authoredRange)
        {
            if (target == null) return false;
            if (target == actor) return true;
            return Vector3.Distance(transform.position, target.transform.position)
                   <= actor.Radius + target.Radius + Mathf.Max(0, authoredRange);
        }

        private string EffectId(MagicalSkillDefinition definition) =>
            definition.stableId + ":" + actor.GetEntityId();

        private bool FailAfterSpend(string message, float amount)
        {
            resources.Restore(ResourceIds.Mana, amount);
            return Fail(message);
        }

        private bool Fail(string message)
        {
            Feedback = message;
            return false;
        }

        private static void SpawnIceVisual(Vector3 point, float radius, bool burst)
        {
            var primitive = GameObject.CreatePrimitive(burst ? PrimitiveType.Sphere : PrimitiveType.Cylinder);
            primitive.name = burst ? "Ice Burst impact" : "Ice Burst warning";
            primitive.transform.position = point + Vector3.up * (burst ? 0.45f : 0.025f);
            primitive.transform.localScale = burst
                ? new Vector3(radius * 0.7f, radius * 0.7f, radius * 0.7f)
                : new Vector3(radius * 2, 0.04f, radius * 2);
            UnityEngine.Object.Destroy(primitive.GetComponent<Collider>());
            UnityEngine.Object.Destroy(primitive, burst ? 0.18f : 0.8f);
        }
    }
}
