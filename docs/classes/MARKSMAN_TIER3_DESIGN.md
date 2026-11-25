# DiceFree — Marksman (Tier III Ranger evolution) — design interview

**2026-11-25 — ROUND 1 OPEN FOR USER ANSWERS.** User explicitly selected **Ranger → Marksman** as the FIRST Tier III evolution to design in the new **at least one Tier III per Tier II class** coverage pass. **Greatblade → Berserker follows next**, then choose first evolution for the five other uncovered Tier II parents. This is an AUTHORIZED DESIGN INTERVIEW, **NOT Unity code/asset/build implementation permission**.

**Status:** The **Marksman class name and the choice of Ranger as its parent in this rollout are USER CONFIRMED**. A precision-focused, longer-ranged longbow archetype was discussed in Ranger's earlier Tier II interviews as an **IDEA**; it is NOT YET an approved detailed Tier III skill set or a numerical range. Questions below are open, with suggested directions clearly marked as suggestions. Do not confuse this with an already implemented Tier III class.

**Source documents:** [Tier III coverage roadmap](../TIER3_ONE_PER_TIER2_ROADMAP.md), [Ranger Tier II design](RANGER_TIER2_DESIGN.md), [Class Progression](../CLASS_PROGRESSION.md), [Stats and Damage](../STATS_AND_DAMAGE.md).

## Parent Ranger baseline to preserve

On `feat/verdant-wilds` the actual actor YAML `Assets/_DiceFree/Settings/Ranger/Ranger class.asset` has **VIT13 / STR8 / AGI18 / INT7 / SPR9** at Level1, growth **VIT1.50 / STR0.70 / AGI3.10 / INT0.80 / SPR1.15** per level; total starting attributes55, growth7.25/level. Base move speed5, base HP10, +15 HP per VIT, base PDEF/MDEF0.

`Assets/_DiceFree/Settings/Ranger/Ranger bow basic.asset` gives:
- Ordinary Ranger bow attack **Physical** (channel0 in actor asset), **AGI primary** attack raw weights 1.05 AGI, **baseDamage5**, interval **1.42s**, windup **0.47s**, **base range11m**;
- `requiresAccuracy=1`; normal shots may miss under ordinary combat rules, whereas Ranger **Exploding Arrow is EXPLICITLY guaranteed to hit** under the Tier II user correction;
- Current both shortbows and longbows are **TWO-HANDED**, share class-authored base attack range, and have item-level attack-speed vs raw attack-power tradeoffs; no equipped quiver offhand.
- Normal attacks do **NOT** cost Mana. Ranger uses Mana only; `Assets/_DiceFree/Settings/Ranger/Ranger Mana.asset` current profile max = `60+1×INT` and regeneration/sec = `4+0.1×SPR`.
- Ranger's existing six skills and exact 80-class-rank gates are **approved for its Tier II manifestation**: **Aimed Shot20**, **Exploding Arrow18**, **Arrow Rain16**, **Hawk Dive10**, **Evasive Step8**, **Bow Mastery8**.
- **Aimed Shot** rewards attacking the target marked by the Ranger, with the user's confirmed +`0.5×AGI×(1+0.05×(r−1))` extra raw marked-target bonus; Bow Mastery's crit and +0.1m/rank bonus apply to Aimed Shot and basics.
- **Hawk Dive** marks the enemy for own additional damage, normal 8s first-rank Hunter's Mark, only one marked target per Ranger.
- **Exploding Arrow** deals direct PLUS Gun-element area damage around the successfully struck primary enemy, **cannot miss**; do not invent miss/detonation-on-ground behavior.
- **Arrow Rain** channels 1.5s firing large then small arrows skyward; **four area damage waves over two seconds** after successful completion, 3m circle, channel canceled by movement or stun but rain persists after channel completes.
- **Evasive Step** leaping reposition + temporary dodge and movespeed; cannot use when rooted/stunned. **Bow Mastery** grants AGI, short range extension, crit chance at 1.4x for BASIC+Aimed Shot ONLY.

The new Tier III must have its OWN starting stats, growth, resources, abilities, gear permissions and advancement path. It begins as a new class Level1 manifestation after eligible Tier II Ranger Level60 advancement, preserving the Ranger parent. Do NOT copy an existing Tier III's skill caps, exact gates, Mana or quest without a new design round.

## Current identity direction (suggestion, NOT yet a locked detailed kit)

**Marksman** would evolve Ranger's bow expertise into a class specializing in **precise ranged hits, long sightlines, higher single-target damage and exploiting marked targets**. Suggested tradeoff: less instant burst movement/area disruption than Ranger or future Beastmaster, in exchange for reach and stronger punish windows. **A longbow-oriented visual identity is a proposal; whether shortbows and other compatible bows are allowed is a user choice.**

Suggested initial metrics only, **not authorized values**:
- Range beyond Ranger's actual current **11m**, e.g., 14m / 16m / 18m, from Marksman's CLASS attack definition; preserve the normal rule that compatible different bows at the same Marksman rank have the same base reach unless user explicitly changes it.
- Strength focused on AGI, with meaningful VIT for survivability. No stat numbers approved yet.
- Retain bow-shot readability in Warcraft III/LoL-style click movement and cast controls. Whether attacks require standing still, an upgrade to Hunter's Mark, or partial movement while drawing are design choices.

## Round 1: FIVE QUESTIONS TO ASK USER (OPEN)

1. **Combat rhythm:** Should Marksman primarily be an **immobile long-range sniper** with heavy windups and massive payoff, a **mobile accurate archer** with useful repositioning, or **balanced** (powerful stationary shots plus modest mobility)? User may propose other combat style.
2. **Base range and weapon identity:** Current Ranger bow reach is **11m**. Should Marksman start at **14m**, **16m**, **18m**, or another value? Should Marksman use **both longbows and shortbows**, with longbows *favored*, or **longbows exclusively**? Future crossbow/guns should NOT be silently added.
3. **Hunter's Mark evolution:** Should the Marksman keep the Ranger-style **Hawk Dive to apply a single target mark**, place it with a new aimed attack, or get a more elaborate **target tracking/weak-point stacking** system? Mark's damage bonus and eventual multi-target limits are undecided.
4. **Inherited skills:** From **Aimed Shot, Exploding Arrow, Arrow Rain, Hawk Dive, Evasive Step and Bow Mastery**, which feel essential to **retain and improve** for Marksman and which should be **replaced by new advanced skills**? There is no mandatory five-/six-ability cap until the user decides.
5. **Signature new Marksman mechanic:** Which deserves a signature skill/passive: **charged armor-piercing shot**, **critical weak-point exploitation**, **long-range piercing/multi-target line shot**, **focus/steady-aim stance**, or a different idea? Do not add a new spendable Focus resource without user's choice.

## Next design stages after questions answered

- Confirm inherited vs new active and passive roster, exact rank caps/rank gates (Tier III per-class level1–150 pattern from the established Psion precedent is a useful candidate, but MUST be explicitly designed/audited for Marksman), elements, Physical/Magical channels and crit permission.
- Define rank-growing AGI damage formulas, cooldowns/Mana scaling and a separate class Mana/HP profile appropriate to user-approved higher-tier growth direction, using actual Ranger baseline and Psion Tier III for comparison, not blindly copying Psion's huge Mana numbers.
- Choose AGI stat budget and level growth stronger than Ranger, final bow equipment compatibility, visuals, quest teacher/location, a suitable solo trial only if user wants one, and extra shot/mark/projectile/LoS edge cases.
- Present decisions in five-question rounds and **update this GitHub design doc after each completed round**, following user's standing workflow.
- After Marksman: [Tier III coverage roadmap](../TIER3_ONE_PER_TIER2_ROADMAP.md) schedules **Greatblade → Berserker next**.
