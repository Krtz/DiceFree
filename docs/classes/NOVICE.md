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

## Intended early-game pacing

The normal first-playthrough Novice phase is deliberately short.

Current target:
- a new player should normally reach **level 10 in about 30 minutes or less**;
- the ordinary Novice introduction should **not approach an hour** before the first blessing/advancement opportunity;
- the exact quest/XP tuning may move during playtesting, but the intended experience is a brisk tutorial/identity-sampling phase rather than a long Tier-0 campaign.

Players may still deliberately refuse advancement and continue leveling Novice toward the level-200 secret path. The short pacing target applies to the normal route to the first advancement, not to the optional long-term Novice challenge.

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

On every Novice level-up:
- +1 Vitality;
- +1 Strength;
- +1 Agility;
- +1 Intelligence;
- +1 Spirit.

Starting HP direction:
- **10 base HP + Vitality-derived HP bonus**

Exact Vitality coefficient remains part of global stat balancing.

## Basic attack

Novice's baseline basic attack is intentionally simple: **fists/unarmed attacks**.

This avoids spending disproportionate animation/model effort on a class most players will only use briefly.

Novice basic attacks scale from the **highest of all five primary attributes**.

The attack remains a simple physical/unarmed hit even when INT/SPI/etc. happens to be the highest stat; the adaptive stat decides scaling, not the attack's visual identity.

Exact coefficient/basic-attack formula is still balance data.

## Resource

Novice has **no class resource**.

Its abilities are governed by cooldowns.

Cooldowns should be intentionally somewhat long so the class does not feel like a fully-developed rotation.

## Skill system

Novice has exactly **four normal active skills** plus a simple stat passive that can absorb additional skill points.

Each normal active skill can be raised to **skill level 10**.

Skill-point rules:
- a fresh level-1 Novice spawns with **1 unspent skill point**;
- all four active skills begin **unlearned / rank 0**;
- rank 1 must be purchased with a skill point;
- every character level grants **1 additional skill point**;
- unspent points can also be invested into the Novice stat passive.

The normal skill menu is therefore an explicit allocation system rather than automatic skill unlocks.

All numerical formulas below are **current balance targets**, not immutable final numbers.

### Strength skill — melee stun

Identity:
- melee;
- Strength sample;
- direct damage + hard control.

Current formula:
- Damage = **Strength × 2**
- Stun duration = **Skill Level × 0.2 seconds**

So:
- rank 1 = 0.2 s;
- rank 10 = 2.0 s.

The skill is melee-range regardless of equipped Novice weapon.

**Novice exception:** this stun ignores normal **CC resistance**. A valid target receives the listed stun duration rather than having it reduced by ordinary CC-resistance rules.

Explicit hard **Stun Immune** effects/flags still block it. The Novice exception bypasses ordinary CC resistance/DR, not true immunity.

Exact cooldown/damage tuning remains open.

### Intelligence skill — ranged magic sand

Identity:
- ranged magical attack;
- Intelligence sample;
- accuracy/attack debuff.

Current formula:
- Damage = **Intelligence × 2**
- Target attack miss chance increase = **Skill Level × 7.5%**
- Duration: **5 seconds**

The miss effect applies to attacks/skills tagged as **requiring accuracy**, which will primarily mean basic attacks in early Cornberg combat.

At rank 10 this reaches 75% miss chance, intentionally leaving the ability potentially useful even in much later content if a player somehow brings a high-level Novice there.

Stacking/refresh behavior remains balance data.

### Agility skill — attack-speed buff

Identity:
- self/support buff;
- Agility sample.

Current direction:
- Attack Speed bonus = **10 + (Skill Level × 0.2 × Agility)%**
- Current duration target: **~5 seconds**

The buff can target:
- self;
- allied units/players.

The exact coefficient/duration remain balance targets to verify in playtesting.

### Spirit skill — heal

Identity:
- healing sample;
- Spirit sample.

Current formula:
- Heal = **Spirit × 10**

Targeting:
- self;
- allies.

Current behavior:
- **instant cast**;
- relatively long cooldown, consistent with the deliberately clunky Novice kit.

Exact cooldown remains to be balanced.

## Novice stat passive

Novice has a deliberately simple skill-point sink/passive:

- each point invested grants **+1 Vitality, +1 Strength, +1 Agility, +1 Intelligence and +1 Spirit**.

This lets skill points remain useful after the four active skills reach rank 10 and reinforces the Novice's all-rounder/blank-slate identity.

The passive has a maximum of **160 ranks**.

An additional Echo-wide bonus may exist later, but that is not currently defined as part of the Novice kit.

## Novice-only respec

Cornberg's retired adventurer couple can help a Novice respec/reallocate Novice skill points.

This service is **Novice-only**.

The respec is **free**. Exact dialogue/presentation remains open.

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

### Branch creation behavior

Advancement happens **in the current game/session**.

When the player chooses a blessing:
1. a new Tier-1 manifestation is created;
2. the game immediately continues as that new manifestation at level 1;
3. the parent Novice remains preserved as a playable slot at the branch point;
4. the child inherits a snapshot/copy of the parent's current manifestation-specific progression state at that moment;
5. after the fork, parent and child can diverge independently.

### Full manifestation snapshot on branch

Branch creation snapshots **all manifestation-specific state** into the child.

This includes, at minimum:
- equipped gear;
- carried inventory;
- carried gold/currency;
- quest state;
- world/progression state;
- learned skills/ranks and other manifestation-specific progression needed to continue coherently.

The parent keeps its original state and the child receives a duplicate snapshot.

All duplicated items remain account-bound under DiceFree's normal item-binding rules, and equipment requirements naturally limit usefulness across classes.

After the fork, parent and child diverge independently.

Echo-wide state remains shared as usual.

### Parent resume point

When the player later returns to the preserved parent Novice, that manifestation resumes **immediately before the blessing choice that created the child**.

The blessing is still available to that Novice, so the player can choose another branch and create another manifestation.

Open:
- exact total slot limits;
- exact UI for visualizing parent/child timeline relationships.

## Design goal

Novice should feel like:
- simple;
- weak;
- flexible;
- readable;
- slightly unfinished;
- surprisingly deep only if someone refuses to advance and pushes it absurdly far.

It is not intended to compete normally with specialized Tier 1+ classes at equivalent progression.
