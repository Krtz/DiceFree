using System;
using DiceFree.Combat;
using UnityEngine;
using static DiceFree.EditorTools.CombatMathValidation;

namespace DiceFree.EditorTools
{
    public static class ActionHitValidation
    {
        private sealed class Rolls : IMissRollSource, ICriticalRollSource
        {
            public float value; public int calls;
            public float NextUnit() { calls++; return value; }
        }
        private static void Reject(Action action)
        {
            try { action(); } catch (ArgumentException) { return; }
            throw new InvalidOperationException("Invalid miss input accepted.");
        }
        public static void Check(CombatActor player, CombatActor enemy)
        {
            var origin = new ActionProvenance("fixture.action", "fixture.actor");
            var missRolls = new Rolls(); var critRolls = new Rolls();
            var crit = new[] { new CriticalRule("fixture.hat", "eight", true, 1, 8) };
            var ordinary = ActionResolution.Resolve(origin, missRolls: missRolls, critRolls: critRolls);
            Require(!ordinary.Hit.missed && missRolls.calls == 0 && critRolls.calls == 0, "Default-hit consumed RNG.");
            var low = new MissChance(.25f, -.5f); var high = new MissChance(.8f, .4f);
            Require(low.Effective == 0 && low.Unclamped == -.25f && high.Effective == 1 && high.Unclamped > 1, "Miss clamp failed.");
            var combined = new MissChance(.25f, .15f);
            Require(Mathf.Approximately(combined.Effective, .4f), "Miss deltas must be additive percentage points.");
            foreach (var roll in new[] { .2499f, .25f, .2501f })
            {
                missRolls.value = roll;
                var result = ActionResolution.Resolve(origin, new MissChance(.25f), missRolls);
                Require(result.Hit.missed == (roll < .25f) && result.Hit.roll == roll && result.Origin == origin, "Miss threshold/provenance failed.");
            }
            int prior = missRolls.calls;
            Require(!ActionResolution.Resolve(origin, low, missRolls).Hit.missed && missRolls.calls == prior, "Explicit 0% consumed RNG.");
            var missed = ActionResolution.Resolve(origin, high, missRolls, crit, critRolls);
            Require(missed.Hit.missed && missed.Critical == null && missed.Origin == origin &&
                missRolls.calls == prior && critRolls.calls == 0, "Guaranteed miss must skip all RNG and crit.");
            var attack = player.Stats.Definition.basicAttack;
            for (int i = 0; i < 2; i++)
            {
                // Null stats prove the miss branch cannot accidentally construct/mitigate packets.
                var packet = DamageResolver.CalculatePacket(null, null, attack, missed);
                float hp = enemy.Health.Current;
                var applied = enemy.Health.ApplyPacket(player, packet);
                Require(applied.Missed && applied.action == missed && applied.raw == 0 && applied.applied == 0 && applied.restored == 0 &&
                    enemy.Health.Current == hp && critRolls.calls == 0 && missRolls.calls == prior, "Missed action resolved a packet or rerolled.");
            }
            missRolls.value = .5f;
            var hit = ActionResolution.Resolve(origin, new MissChance(.25f), missRolls, crit, critRolls);
            Require(!hit.Hit.missed && hit.Critical.Triggered && hit.Critical.Winner.requestedMultiplier == 8 &&
                hit.Origin == origin && hit.Critical.Origin == origin && critRolls.calls == 1, "Hit must allow normal crit after miss check.");
            var defaultCrit = ActionResolution.Resolve(origin, critRules: crit, critRolls: critRolls);
            Require(defaultCrit.Critical.Triggered && critRolls.calls == 2, "No miss rule must still allow explicit crit.");
            Reject(() => new MissChance(float.NaN)); Reject(() => new MissChance(0, float.PositiveInfinity));
            foreach (float invalid in new[] { -1f, 1f, float.NaN, float.PositiveInfinity })
            { missRolls.value = invalid; Reject(() => ActionResolution.Resolve(origin, new MissChance(.25f), missRolls)); }
            foreach (var actor in UnityEngine.Object.FindObjectsByType<CombatActor>(FindObjectsInactive.Include))
            {
                var stats = actor.GetComponent<ActorStats>(); var target = actor == player ? enemy.Stats : player.Stats;
                var baseline = DamageResolver.Calculate(stats, target, stats.Definition.basicAttack);
                var packet = DamageResolver.CalculatePacket(stats, target, stats.Definition.basicAttack, ordinary);
                Require(!packet.Missed && packet.raw == baseline.raw && packet.mitigated == baseline.mitigated,
                    "Default-hit Cornberg damage changed.");
            }
            Debug.Log("ACTION_HIT_OK: default hit, boundaries, additive clamp, deterministic extremes, miss-before-crit, shared packets and provenance.");
        }
    }
}
