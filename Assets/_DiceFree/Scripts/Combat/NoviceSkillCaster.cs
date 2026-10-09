using System;
using System.Collections.Generic;
using DiceFree.Foundation;
using DiceFree.Skills;
using UnityEngine;

namespace DiceFree.Combat
{
    [DisallowMultipleComponent, RequireComponent(typeof(CombatActor), typeof(NoviceSkillProgression))]
    public sealed class NoviceSkillCaster : MonoBehaviour, IManifestationSessionState
    {
        private readonly Dictionary<string, float> readyAt = new(StringComparer.Ordinal);
        private CombatActor actor;
        private NoviceSkillProgression progression;
        private NoviceCombatVfx vfx;

        public string Feedback { get; private set; } = "";
        public event Action<NoviceSkillDefinition> Casted;

        private void Awake()
        {
            actor = GetComponent<CombatActor>();
            progression = GetComponent<NoviceSkillProgression>();
            vfx = GetComponent<NoviceCombatVfx>() ?? gameObject.AddComponent<NoviceCombatVfx>();
        }

        public float CooldownRemaining(NoviceSkillDefinition definition)
        {
            if (definition == null || !readyAt.TryGetValue(definition.stableId, out float time)) return 0;
            return Mathf.Max(0, time - Time.time);
        }

        public bool CastSlot(int slot) => Fail("Choose a target for the skill first.");

        public bool CanPrepare(NoviceSkillDefinition definition)
        {
            if (!progression.ActiveForCurrentClass) return Fail("Novice skills are inactive for this manifestation.");
            if (definition == null || !definition.Active) return Fail("No active skill in that slot.");
            int rank = progression.Rank(definition.stableId);
            if (rank <= 0) return Fail(definition.displayName + " is unlearned.");
            if (!actor.CanAct) return Fail("Cannot use skills right now.");
            if (CooldownRemaining(definition) > 0) return Fail(definition.displayName + " is cooling down.");
            return true;
        }

        public bool Cast(NoviceSkillDefinition definition, CombatActor requestedTarget = null)
        {
            if (!CanPrepare(definition)) return false;
            int rank = progression.Rank(definition.stableId);

            bool hostile = definition.kind == NoviceSkillKind.StrengthMeleeStun || definition.kind == NoviceSkillKind.MagicSand;
            CombatActor target = hostile
                ? requestedTarget
                : ResolveSupportTarget(requestedTarget);
            if (hostile && !actor.IsHostileTo(target)) return Fail("Select a hostile target.");
            if (!hostile && !actor.IsFriendlyTo(target)) return Fail("No valid friendly target.");
            if (!InRange(target, definition.range)) return Fail("Target is out of range.");
            if (target != actor && !actor.HasSightOf(target)) return Fail("Target is not in sight.");
            if (definition.kind != NoviceSkillKind.SpiritHeal && target.Effects == null)
                return Fail("Target cannot receive this skill effect.");

            actor.GetComponent<BasicAttack>()?.Cancel();
            actor.Motor.Stop();

            switch (definition.kind)
            {
                case NoviceSkillKind.StrengthMeleeStun:
                    if (definition.attack == null) return Fail("Strength skill attack data is missing.");
                    vfx?.PlayMeleeStun(actor, target);
                    EarlySkillVfx.Play(EarlySkillVfx.Cue.NoviceStrike, target.transform.position);
                    DamageResolver.Hit(actor, target, definition.attack);
                    if (target.Alive)
                        target.Effects.TryApplyStun(definition.StunDuration(rank), true);
                    break;

                case NoviceSkillKind.MagicSand:
                    if (definition.attack == null) return Fail("Magic Sand attack data is missing.");
                    vfx?.PlayMagicSand(actor, target);
                    EarlySkillVfx.Play(EarlySkillVfx.Cue.NoviceSand, target.transform.position);
                    DamageResolver.Hit(actor, target, definition.attack);
                    if (target.Alive)
                        target.Effects.ApplyAccuracyPenalty(definition.stableId, definition.MagicSandMissChance(rank), definition.durationSeconds);
                    break;

                case NoviceSkillKind.AgilityAttackSpeedBuff:
                    target.Effects.ApplyAttackSpeedBuff(definition.stableId,
                        definition.AttackSpeedBonusPercent(actor.Stats.Attributes, rank), definition.durationSeconds);
                    vfx?.PlayAttackSpeedGlow(target, definition.durationSeconds);
                    EarlySkillVfx.Play(EarlySkillVfx.Cue.NoviceHaste, target.transform.position);
                    break;

                case NoviceSkillKind.SpiritHeal:
                    if (target.Health.HealFrom(actor.Stats, definition.HealAmount(actor.Stats.Attributes, rank)) > 0)
                        { vfx?.PlayHealLight(target); EarlySkillVfx.Play(EarlySkillVfx.Cue.NoviceHeal, target.transform.position); }
                    break;

                default:
                    return Fail("Passive skills cannot be cast.");
            }

            readyAt[definition.stableId] = Time.time + Mathf.Max(0, definition.cooldownSeconds);
            Feedback = definition.displayName + " used.";
            Casted?.Invoke(definition);
            return true;
        }

        public void ResetCooldowns()
        {
            readyAt.Clear();
            Feedback = "";
        }

        public void ResetForManifestationLoad() => ResetCooldowns();

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

        private bool Fail(string message)
        {
            Feedback = message;
            return false;
        }
    }
}
