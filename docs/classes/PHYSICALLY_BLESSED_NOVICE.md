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

## Working skill identities

### 1. Heavy Strike
- melee single-target physical skill;
- evolves the Novice stun concept;
- damage should use the **higher of STR or AGI** as the simple Tier-1 blank-slate rule;
- keeps a stun, but the stun obeys ordinary CC resistance/DR and true immunity.

Avoid splitting "AGI controls damage / STR controls stun" unless later playtesting proves it adds meaningful build choice; the simpler adaptive formula is preferred for now.

### 2. Guard / Brace
- short-duration defensive active;
- useful without requiring a shield;
- broad Physical + Magical mitigation direction;
- previews the future sword-and-board/tank lineage.

### 3. Quickening
- evolves the Novice attack-speed buff idea;
- physical tempo/attack-speed identity;
- previews agile/Rogue-style descendants.

Exact self/ally targeting remains open.

### 4. Arrow Rain
- ground-targeted physical AoE;
- calls down a small rain of arrows without requiring the class to equip a bow;
- deliberately previews the future Ranger branch while Physically Blessed itself remains melee.

### 5. Martial Aptitude
- broad physical passive;
- should benefit both STR-leaning and AGI-leaning builds;
- exact bonus remains open.

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

- exact passive;
- exact three active abilities;
- which Novice abilities evolve into this kit;
- basic-attack coefficient;
- base HP/defenses/movement;
- equipment permissions;
- Mana pool/regeneration/cost model;
- final level-30 branch count and names.
