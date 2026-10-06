using System;
using System.Collections;
using System.Collections.Generic;
using DiceFree.Foundation;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(PhysicalSkillProgression), typeof(ActorResourceController))]
    public sealed class PhysicalSkillCaster : MonoBehaviour, IManifestationSessionState
    {
        public const string ManaResourceId = "resource.mana";

        private readonly Dictionary<string, float> readyAt = new(StringComparer.Ordinal);
        private CombatActor actor;
        private PhysicalSkillProgression progression;
        private ActorResourceController resources;
        private TargetSelection selection;
        private CombatStatusController effects;
        private BasicAttack basicAttack;

        public string Feedback { get; private set; } = "";

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            progression = GetComponent<PhysicalSkillProgression>();
            resources = GetComponent<ActorResourceController>();
            selection = GetComponent<TargetSelection>();
            effects = GetComponent<CombatStatusController>();
            basicAttack = GetComponent<BasicAttack>();
        }

        public float CooldownRemaining(PhysicalSkillDefinition definition) =>
            definition == null || !readyAt.TryGetValue(definition.stableId, out float at)
                ? 0
                : Mathf.Max(0, at - Time.time);

        public bool CastSlot(int slot)
        {
            var definition = progression.ActiveAtSlot(slot);
            if (definition == null) return Fail("No Physical skill in that slot.");
            if (definition.kind == PhysicalSkillKind.ArrowRain)
                return Fail("Arrow Rain needs a ground point.");
            return Cast(definition, selection == null ? null : selection.Selected);
        }

        public bool Cast(PhysicalSkillDefinition definition, CombatActor requestedTarget = null)
        {
            if (!CanBegin(definition, out int rank)) return false;

            switch (definition.kind)
            {
                case PhysicalSkillKind.HeavyStrike:
                    return HeavyStrike(definition, rank, requestedTarget ?? selection?.Selected);
                case PhysicalSkillKind.Guard:
                    return Guard(definition, rank);
                case PhysicalSkillKind.Quickening:
                    return Quickening(definition, rank);
                case PhysicalSkillKind.ArrowRain:
                    return Fail("Arrow Rain needs a ground point.");
                default:
                    return Fail("Passive skills cannot be cast.");
            }
        }

        public bool CastArrowRain(PhysicalSkillDefinition definition, Vector3 groundPoint)
        {
            if (!CanBegin(definition, out int rank)) return false;
            if (definition.kind != PhysicalSkillKind.ArrowRain)
                return Fail("That skill is not ground targeted.");
            if (Vector3.Distance(transform.position, groundPoint) > definition.range)
                return Fail("Ground target is out of range.");
            if (!CommitCostAndCooldown(definition, rank)) return false;

            basicAttack?.Cancel();
            actor.Motor.Stop();
            StartCoroutine(ArrowRainRoutine(definition, rank, groundPoint));
            Feedback = definition.displayName;
            return true;
        }

        private bool HeavyStrike(PhysicalSkillDefinition definition, int rank, CombatActor target)
        {
            if (target == null || !actor.IsHostileTo(target)) return Fail("Heavy Strike needs a hostile target.");
            if (!actor.HasSightOf(target)) return Fail("Target is not visible.");
            float reach = actor.Radius + target.Radius + definition.range;
            if (Vector3.Distance(transform.position, target.transform.position) > reach)
                return Fail("Target is out of melee range.");
            if (!CommitCostAndCooldown(definition, rank)) return false;

            basicAttack?.Cancel();
            actor.Motor.Stop();
            Face(target.transform.position);
            float applied = DamageResolver.Hit(actor, target, definition.attack, null, definition.DamageCoefficient(rank));
            if (applied > 0)
                target.Effects?.TryApplyStun(definition.HeavyStunSeconds(rank), false);
            Feedback = definition.displayName;
            return true;
        }

        private bool Guard(PhysicalSkillDefinition definition, int rank)
        {
            if (effects == null) return Fail("Status controller is unavailable.");
            if (!CommitCostAndCooldown(definition, rank)) return false;
            effects.ApplyDamageReduction(definition.stableId, definition.GuardReduction(rank), definition.durationSeconds);
            Feedback = definition.displayName;
            return true;
        }

        private bool Quickening(PhysicalSkillDefinition definition, int rank)
        {
            if (effects == null) return Fail("Status controller is unavailable.");
            if (!CommitCostAndCooldown(definition, rank)) return false;
            effects.ApplyAttackSpeedBuff(definition.stableId, definition.QuickeningAttackSpeedPercent(rank), definition.durationSeconds);
            effects.ApplyMovementSpeedBuff(definition.stableId, definition.QuickeningMoveSpeedPercent(rank), definition.durationSeconds);
            Feedback = definition.displayName;
            return true;
        }

        private IEnumerator ArrowRainRoutine(PhysicalSkillDefinition definition, int rank, Vector3 point)
        {
            int hits = Mathf.Max(1, definition.arrowHitCount);
            for (int wave = 0; wave < hits; wave++)
            {
                ResolveArrowWave(definition, rank, point);
                SpawnArrowRainVisual(point, definition.arrowRadius, wave);
                if (wave + 1 < hits && definition.arrowHitInterval > 0)
                    yield return new WaitForSeconds(definition.arrowHitInterval);
            }
        }

        private void ResolveArrowWave(PhysicalSkillDefinition definition, int rank, Vector3 point)
        {
            foreach (var candidate in FindObjectsByType<CombatActor>())
            {
                if (candidate == null || !actor.IsHostileTo(candidate)) continue;
                var flatA = new Vector2(candidate.transform.position.x, candidate.transform.position.z);
                var flatB = new Vector2(point.x, point.z);
                if (Vector2.Distance(flatA, flatB) > definition.arrowRadius + candidate.Radius) continue;
                DamageResolver.Hit(actor, candidate, definition.attack, null, definition.DamageCoefficient(rank));
            }
        }

        private static void SpawnArrowRainVisual(Vector3 point, float radius, int wave)
        {
            const int arrows = 7;
            for (int i = 0; i < arrows; i++)
            {
                float angle = (i * 137.5f + wave * 31f) * Mathf.Deg2Rad;
                float distance = radius * Mathf.Sqrt((i + 0.5f) / arrows);
                var position = point + new Vector3(Mathf.Cos(angle) * distance, 2.8f + 0.18f * i, Mathf.Sin(angle) * distance);
                var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                visual.name = "SpectralArrow";
                var collider = visual.GetComponent<Collider>();
                if (collider != null) Destroy(collider);
                visual.transform.position = position;
                visual.transform.localScale = new Vector3(0.018f, 0.32f, 0.018f);
                visual.transform.rotation = Quaternion.Euler(0, 0, 180);
                var renderer = visual.GetComponent<Renderer>();
                if (renderer != null) renderer.material.color = new Color(0.72f, 0.82f, 0.95f, 0.8f);
                visual.AddComponent<ArrowRainVisual>();
            }
        }

        private bool CanBegin(PhysicalSkillDefinition definition, out int rank)
        {
            rank = 0;
            if (!progression.ActiveForCurrentClass) return Fail("Physically Blessed skills are inactive.");
            if (definition == null || !definition.Active) return Fail("That is not an active Physical skill.");
            if (!actor.CanAct) return Fail("Cannot act right now.");
            rank = progression.Rank(definition.stableId);
            if (rank <= 0) return Fail("Skill is not learned.");
            if (CooldownRemaining(definition) > 0) return Fail("Skill is on cooldown.");
            if (!resources.Has(ManaResourceId)) return Fail("Mana is unavailable.");
            if (!resources.CanSpend(ManaResourceId, definition.ManaCost(rank))) return Fail("Not enough Mana.");
            return true;
        }

        private bool CommitCostAndCooldown(PhysicalSkillDefinition definition, int rank)
        {
            if (!resources.TrySpend(ManaResourceId, definition.ManaCost(rank)))
                return Fail("Not enough Mana.");
            readyAt[definition.stableId] = Time.time + definition.cooldownSeconds;
            return true;
        }

        private void Face(Vector3 point)
        {
            var facing = Vector3.ProjectOnPlane(point - transform.position, Vector3.up);
            if (facing.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(facing);
        }

        private bool Fail(string message)
        {
            Feedback = message;
            return false;
        }

        public void ResetForManifestationLoad()
        {
            StopAllCoroutines();
            readyAt.Clear();
            Feedback = "";
        }
    }

    public sealed class ArrowRainVisual : MonoBehaviour
    {
        private Vector3 velocity = Vector3.down * 8f;
        private float expires;

        private void Awake() => expires = Time.time + 0.5f;

        private void Update()
        {
            transform.position += velocity * Time.deltaTime;
            if (Time.time >= expires) Destroy(gameObject);
        }
    }
}
