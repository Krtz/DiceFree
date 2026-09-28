# Physically Blessed Novice

## Status

**Working Tier 1 direction.** Exact skills, coefficients, mana costs and equipment lists are not yet final.

## Identity

Physically Blessed Novice is still a broad physical blank slate rather than a fully specialized Fighter/Rogue/Ranger/Tank.

It is:
- primarily melee;
- primarily Strength/Agility driven;
- sturdier and more physically defined than Novice;
- broad enough to foreshadow several later physical archetypes.

Current intended Tier-1 kit shape is **5 skills, 6 ranks each**:
1. single-target melee damage + stun;
2. defensive active;
3. attack-speed/physical-tempo buff;
4. ground-targeted Arrow Rain AoE;
5. broad physical passive.

A good continuity rule is to evolve some Novice ideas rather than erase the first ten levels completely.

### Skill-point cadence

Tier 1 restarts its own skill progression:
- level 1 begins with 1 Tier-1 skill point;
- gain 1 skill point per level;
- each of the 5 skills has 6 ranks;
- reaching level 30 provides exactly 30 Tier-1 skill points, enough to max all five skills.

Novice ranks do not numerically carry into Tier 1; inherited/evolved skills are conceptual descendants with their own ranks.

## Resource direction

Strong current direction: **Mana** at Tier 1.

Reason:
- introduces class resources in a simple shared way;
- fits the Warcraft-inspired foundation;
- avoids inventing bespoke mechanics before specialization;
- later physical classes can replace Mana with Rage, Combo Points/Energy, Focus, etc.

Mana should not imply "spellcaster only." In DiceFree it can represent generic ability power/effort where a class uses it.

Physical Mana philosophy:
- lower ability costs than Magically Touched;
- lower dependence on Mana regeneration;
- low INT must not make the class dysfunctional;
- base Mana/base regen and costs are class-defined.

Exact base Mana, regeneration and INT interaction remain balance work.

## Basic attack and equipment

Basic attacks scale from **whichever is higher of Strength or Agility**.

Physically Blessed is still fundamentally melee at Tier 1.

Equipment direction:
- can equip essentially **all ordinary low-level melee weapons** that are not reserved for a more specialized class;
- examples can include one-handed and two-handed swords, axes, maces, daggers, spears/polearms and similar conventional melee families as they are introduced;
- shields can be supported as ordinary offhand equipment where appropriate;
- no true ranged basic-attack weapon identity yet.

The exact attack coefficient/speed remains balance work.

## Working skill identities

### 1. Heavy Strike
- melee single-target physical skill;
- evolves the Novice stun concept;
- damage uses the **higher of STR or AGI**;
- keeps a stun, but the stun obeys ordinary CC resistance/DR and true immunity.

Current stun-duration balance target across ranks 1–6:
- rank 1: **0.4 s**;
- +0.2 s per additional rank;
- rank 6: **1.4 s**.

Damage coefficient, Mana cost and cooldown remain open.

Do not split "AGI controls damage / STR controls stun" unless later playtesting proves it adds meaningful build choice; the simpler adaptive formula is preferred.

### 2. Guard / Brace
- short-duration defensive active;
- useful without requiring a shield;
- reduces both Physical and Magical incoming damage;
- current duration target: **~3 seconds**;
- current mitigation target: **15% at rank 1**, +5 percentage points per additional rank, reaching **40% at rank 6**;
- previews the future sword-and-board/tank lineage.

Cooldown and Mana cost remain open.

### 3. Quickening
- evolves the Novice attack-speed buff idea;
- **self-only**;
- increases Attack Speed;
- also has a small Movement Speed bonus;
- current duration target: **~5 seconds**;
- previews agile/Rogue-style descendants.

Exact Attack Speed/Movement Speed values, cooldown and Mana cost remain open.

### 4. Arrow Rain
- ground-targeted physical AoE;
- calls down a small manifested/spectral rain of arrows without requiring the class to equip a bow;
- uses **higher of STR or AGI** scaling;
- resolves as a **short burst**, not a long persistent damage zone;
- deliberately previews the future Ranger branch while Physically Blessed itself remains melee.

Exact burst duration/hit count, damage, radius, cooldown and Mana cost remain open.

### 5. Martial Aptitude
- broad, deliberately straightforward physical passive;
- increases **basic-attack damage**;
- increases **Physical Defense**;
- benefits both STR-leaning and AGI-leaning builds without choosing a specialization.

Exact per-rank values remain open. More exotic passive mechanics belong to later specialized classes.

## Starting stats — current balance target

Level 1:
- Vitality: **12**
- Strength: **12**
- Agility: **12**
- Intelligence: **5**
- Spirit: **5**

Total starting primary stats: 46.

## Level growth — current balance target

Average per level:
- +1 VIT
- +2 STR
- +2 AGI
- +0.5 INT
- +0.5 SPI

Implementation/UI preference:
- avoid displaying half-stat gains;
- instead award +1 INT and +1 SPI every second level.

Total average growth: 6 primary-stat points per level.

## Possible level-30 branches

Current candidate four-way split:
1. **Sword-and-board / tank archetype**
2. **Rogue / agile melee archetype**
3. **Ranger / physical ranged archetype**
4. **Two-handed melee DPS archetype**

This is not yet final, but keeping Rogue and 2H melee separate at this tier has advantages:
- AGI vs STR identity can diverge immediately;
- weapon/armor permissions differ naturally;
- movement/positioning fantasy differs;
- later specialization has a cleaner foundation.

Large class count is an intentional long-term goal rather than a reason to collapse branches.

Development does **not** require symmetric completion. One Tier-2 class may initially have one implemented successor while another has several. The tree can grow unevenly over time as long as implemented paths are coherent and the architecture does not assume equal branch counts.

## Open questions

- exact damage coefficients;
- exact Quickening values;
- exact Martial Aptitude values;
- base HP/defenses/movement;
- exact Mana pool/regeneration/cost model;
- exact cooldowns;
- final level-30 branch count and names.
