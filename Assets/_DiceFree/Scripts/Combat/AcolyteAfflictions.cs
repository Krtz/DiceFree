using System;
using System.Collections.Generic;
using DiceFree.Combat;
using UnityEngine;

namespace DiceFree.Skills
{
    internal sealed class AcolyteAfflictions
    {
        private const double PlagueInterval = .1;
        private const float SpreadRadius = 3;
        private const float SpreadContact = 2;
        private readonly CombatActor owner;
        private readonly Action earnSoul;
        private readonly string curseId;
        private readonly Dictionary<CombatActor, Curse> curses = new();
        private readonly Dictionary<CombatActor, Infection> infections = new();
        private readonly Dictionary<CombatActor, Poison> poisons = new();
        private readonly List<CombatActor> expired = new();
        private readonly List<(CombatActor target, Infection source)> spread = new();
        private double nextSpread;

        private readonly struct Curse
        {
            public readonly double expires;
            public readonly uint life;
            public Curse(CombatActor victim, double expires) { this.expires = expires; life = victim.Health.DeathSequence; }
        }

        private sealed class Infection
        {
            public double expires, nextTick;
            public int rank;
            public uint life;
            public Action onDeath;
            public bool resolvingTick;
            public HashSet<CombatActor> visited;
            public readonly Dictionary<CombatActor, double> contacts = new();
        }

        private sealed class Poison
        {
            public double expires, nextTick;
            public uint life;
        }

        public AcolyteAfflictions(CombatActor owner, Action earnSoul)
        {
            this.owner = owner;
            this.earnSoul = earnSoul;
            curseId = "status.withering-curse:" + owner.GetEntityId();
        }

        public void ApplyCurse(CombatActor victim, int rank)
        {
            // Use the shared control-resistance rating. Stun immunity itself does not erase a curse.
            float strength = 1 - (victim.Effects?.StunResistance ?? 0);
            victim.Stats.SetDefenseModifier(curseId, new DefenseModifier {
                channel = DamageChannel.Magical, reductionFlat = (4 + rank) * strength
            });
            victim.Stats.SetAttackRateReduction(curseId, (.10f + .005f * (rank - 1)) * strength);
            curses[victim] = new Curse(victim, Time.timeAsDouble + 10);
        }

        public void ApplyPlague(CombatActor victim, int rank)
        {
            double now = Time.timeAsDouble;
            double expiresAt = now + 3 + (rank - 1) / 3;
            if (infections.TryGetValue(victim, out var current))
            {
                if (current.life == victim.Health.DeathSequence && current.expires >= now)
                {
                    current.expires = expiresAt;
                    current.rank = rank;
                    return;
                }
                RemoveInfection(victim);
            }
            AddInfection(victim, expiresAt, rank, new HashSet<CombatActor>(), now);
        }

        private void AddInfection(CombatActor victim, double expiresAt, int rank, HashSet<CombatActor> visited, double now)
        {
            var infection = new Infection {
                expires = expiresAt, nextTick = now + PlagueInterval, rank = rank,
                life = victim.Health.DeathSequence, visited = visited
            };
            infection.onDeath = () =>
            {
                if (owner.Alive && (infection.resolvingTick || Time.timeAsDouble <= infection.expires))
                    earnSoul();
            };
            visited.Add(victim);
            infections.Add(victim, infection);
            victim.Health.Died += infection.onDeath;
        }

        public void ApplyPoison(CombatActor victim)
        {
            double now = Time.timeAsDouble;
            if (!poisons.TryGetValue(victim, out var current) || current.life != victim.Health.DeathSequence || current.expires < now)
            {
                current = new Poison { nextTick = now + 1, life = victim.Health.DeathSequence };
                poisons[victim] = current;
            }
            current.expires = now + 3;
        }

        public void Tick(AttackDefinition radiation, AttackDefinition coffee, int passiveRank)
        {
            double now = Time.timeAsDouble;
            foreach (var entry in curses)
                if (!Living(entry.Key, entry.Value.life) || now >= entry.Value.expires) expired.Add(entry.Key);
            foreach (var victim in expired) RemoveCurse(victim);
            expired.Clear();

            foreach (var entry in infections)
            {
                var victim = entry.Key;
                var infection = entry.Value;
                while (Living(victim, infection.life) && Due(infection.nextTick, infection.expires, now))
                {
                    infection.resolvingTick = true;
                    try { Deal(victim, radiation, 1 + .05f * (infection.rank - 1)); }
                    finally { infection.resolvingTick = false; }
                    infection.nextTick += PlagueInterval;
                }
                if (!Living(victim, infection.life) || now > infection.expires) expired.Add(victim);
            }
            foreach (var victim in expired) RemoveInfection(victim);
            expired.Clear();

            if (infections.Count > 0 && now >= nextSpread)
            {
                nextSpread = now + PlagueInterval;
                Spread(now);
            }

            foreach (var entry in poisons)
            {
                var victim = entry.Key;
                var poison = entry.Value;
                while (passiveRank > 0 && Living(victim, poison.life) && Due(poison.nextTick, poison.expires, now))
                {
                    Deal(victim, coffee, 1, .35f * (passiveRank - 1));
                    poison.nextTick += 1;
                }
                if (passiveRank == 0 || !Living(victim, poison.life) || now > poison.expires) expired.Add(victim);
            }
            foreach (var victim in expired) poisons.Remove(victim);
            expired.Clear();
        }

        private void Spread(double now)
        {
            foreach (var entry in infections)
            {
                var source = entry.Key;
                var infection = entry.Value;
                if (now >= infection.expires) continue;
                foreach (var candidate in CombatActor.All)
                {
                    bool near = owner.IsHostileTo(candidate) && !infections.ContainsKey(candidate) &&
                        !infection.visited.Contains(candidate) &&
                        (candidate.transform.position - source.transform.position).sqrMagnitude <= SpreadRadius * SpreadRadius;
                    if (!near) infection.contacts.Remove(candidate);
                    else if (!infection.contacts.TryGetValue(candidate, out var since))
                        infection.contacts.Add(candidate, now);
                    else if (now - since >= SpreadContact)
                        spread.Add((candidate, infection));
                }
            }
            foreach (var (target, source) in spread)
                if (!infections.ContainsKey(target) && !source.visited.Contains(target))
                    AddInfection(target, source.expires, source.rank, source.visited, now);
            spread.Clear();
        }

        private static bool Living(CombatActor target, uint life) =>
            target != null && target.Alive && target.Health.DeathSequence == life;
        private static bool Due(double next, double expires, double now) => next <= now && next <= expires + .00001;
        private void Deal(CombatActor victim, AttackDefinition attack, float scale, float bonus = 0) =>
            victim.Health.ApplyPacket(owner, DamageResolver.Calculate(owner.Stats, victim.Stats, attack,
                rawMultiplier: scale, skillFlatDamage: bonus));

        private void RemoveCurse(CombatActor victim)
        {
            if (victim != null)
            {
                victim.Stats.RemoveDefenseModifier(curseId);
                victim.Stats.RemoveAttackRateReduction(curseId);
            }
            curses.Remove(victim);
        }

        private void RemoveInfection(CombatActor victim)
        {
            if (victim != null) victim.Health.Died -= infections[victim].onDeath;
            infections.Remove(victim);
        }

        public void Clear()
        {
            expired.AddRange(curses.Keys);
            foreach (var victim in expired) RemoveCurse(victim);
            expired.Clear();
            expired.AddRange(infections.Keys);
            foreach (var victim in expired) RemoveInfection(victim);
            expired.Clear();
            poisons.Clear();
            spread.Clear();
        }
    }
}
