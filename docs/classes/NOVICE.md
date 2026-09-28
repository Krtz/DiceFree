# Novice

## Role

**Novice** is DiceFree's Tier 0 starting class.

It is intentionally a **blank slate**.

The purpose of the level 1-10 Novice kit is to let the player sample several broad combat identities before choosing a first blessing:
- Strength / melee control;
- Intelligence / ranged magic and debuffing;
- Agility / attack-speed support;
- Spirit / healing.

Novice should be functional but deliberately weaker and less specialized than Tier 1+ classes.

## Starting presentation

The Echo begins with:
- no equipped gear;
- simple non-equipment baseline clothing: underwear + T-shirt;
- androgynous body/presentation;
- no strong class silhouette.

The starting appearance should read as an unshaped Echo rather than a warrior, mage, rogue or healer.

## Equipment permissions

Novice can equip **only gear explicitly permitted for Novice**.

Examples can include:
- basic sword;
- other intentionally simple starter weapons/gear.

Do not assume Novice can equip every generic item simply because it is the starter class.

## Starting stats

At level 1:
- Vitality: **1**
- Strength: **1**
- Agility: **1**
- Intelligence: **1**
- Spirit: **1**

Starting HP direction:
- **10 base HP + Vitality-derived HP bonus**

Exact Vitality coefficient remains part of global stat balancing.

## Basic attack scaling

Novice basic attacks scale from the **highest of all five primary attributes**.

This allows early gear/stat changes to shape the Novice without forcing an early commitment to one future branch.

Exact coefficient/basic-attack formula is still balance data.

## Resource

Novice has **no class resource**.

Its abilities are governed by cooldowns.

Cooldowns should be intentionally somewhat long so the class does not feel like a fully-developed rotation.

## Skill system

Novice has exactly **four normal active skills**.

Each normal skill can be raised to **skill level 10**.

The exact skill-point acquisition cadence and UI are still to be defined.

All numerical formulas below are **current balance targets**, not immutable final numbers.

### Strength skill — melee stun

Identity:
- melee;
- Strength sample;
- direct damage + hard control.

Current formula:
- Damage = **Strength × 2**
- Stun duration = **Skill Level × 0.2**

The duration unit is intended to be finalized during implementation/balance.

### Intelligence skill — ranged magic sand

Identity:
- ranged magical attack;
- Intelligence sample;
- accuracy/attack debuff.

Current formula:
- Damage = **Intelligence × 2**
- Target attack miss chance increase = **Skill Level × 7.5%**

Debuff duration and exact stacking/refresh behavior remain open.

### Agility skill — attack-speed buff

Identity:
- self/support buff;
- Agility sample.

Current direction:
- Attack Speed bonus = **10 + (Skill Level × 0.2 × Agility)%**

The exact interpretation/coefficient and duration are balance targets to verify in playtesting.

### Spirit skill — heal

Identity:
- healing sample;
- Spirit sample.

Current formula:
- Heal = **Spirit × 10**

Cooldown, cast time and targeting details remain to be balanced.

## No normal passive

Novice has **no ordinary passive** during normal early progression.

An Echo-wide bonus may exist later, but that is not currently defined as part of the Novice kit.

## Level-200 Novice secret

A player is allowed to ignore the level-10 advancement and continue leveling Novice all the way to level 200.

At level 200, Novice should gain access to a **hidden stronger payoff**.

Current direction:
- a stronger secret active skill;
- potentially Novice's first passive/secret passive;
- this secret active skill does **not** appear in the normal skill-level menu.

Exact secret ability/passive and unlock presentation remain open.

The intent is that a level-200 Novice is no longer merely weak/comedic: extreme commitment should make the class genuinely capable in its own strange way.

## Advancement branching does not delete Novice

Reaching level 10 and choosing a blessing does **not** overwrite or transform the existing Novice manifestation.

Instead:
1. the level-10 Novice remains as its own playable manifestation/save slot;
2. choosing a branch creates a **new manifestation/character slot** for the advanced class at level 1;
3. the player can return to the original Novice later;
4. the same Novice can also take the other first branch, creating another new manifestation.

Example:
- Slot 1: Level 10 Novice
- Slot 2: Level 1 Physically Blessed Novice
- Slot 3: Level 1 Magically Touched Novice

This is a direct gameplay expression of the Echo/timeline concept.

Open implementation questions:
- what inventory/equipped state is copied into a newly-created branch manifestation;
- how quest state is inherited vs manifestation-specific;
- whether branch creation has any slot-management limits/costs later.

## Design goal

Novice should feel like:
- simple;
- weak;
- flexible;
- readable;
- slightly unfinished;
- surprisingly deep only if someone refuses to advance and pushes it absurdly far.

It is not intended to compete normally with specialized Tier 1+ classes at equivalent progression.
