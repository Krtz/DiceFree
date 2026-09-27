# Combat

## Direction

DiceFree is a real-time 3D isometric action RPG.

Primary design assumptions:
- mouse-driven click-to-move is the default/reference control style and primary balance target;
- WASD/direct movement is a fully supported alternative;
- compact, class-dependent kits;
- readable mechanics;
- class-specific movement and resources;
- strong class identity over universal action-game mechanics.

## Control schemes

### Classic / mouse movement
Warcraft III / League-style click-to-move is the primary balance baseline.

### Direct / WASD movement
WASD feeds the same movement/combat systems and must not become the hidden requirement for encounter execution.

A simultaneous Hybrid mode remains possible but not committed.

## Basic attacks

Every normal class has a Warcraft III / League-style repeating basic attack:
- command an attack on a target;
- move into attack range if necessary;
- repeatedly attack while the target remains valid.

Basic attacks may be melee or ranged depending on class.

Most caster classes will be ranged, but magical melee classes are fully valid.

A future class with no conventional basic attack is possible only as an intentional special-case design.

## Primary attributes

Current core attributes:

- **Vitality**
- **Strength**
- **Agility**
- **Intelligence**
- **Spirit**

Every attribute should have at least one useful secondary effect in addition to any role it plays as a class's primary attack attribute.

Every class has one or more **primary attributes** used for its base/basic-attack scaling.

Most classes will likely use one primary attribute, but hybrid classes may legitimately use multiple. A future secret class using all attributes as primary is explicitly compatible with the system.

Exact secondary effects are still being designed.

Current candidates include:
- Strength -> physical durability and/or HP-related benefit;
- Agility -> evasion, movement speed and/or attack speed;
- Intelligence -> resource regeneration, maximum resource and/or magical durability;
- Spirit -> healing given and healing received;
- Vitality -> maximum HP.

Do not lock these candidate secondary effects until the complete stat model is designed.

## Health

Vitality contributes to maximum HP.

Conceptually:

```
Max HP = class base HP + (Vitality × HP coefficient) + other modifiers
```

Open question: whether the Vitality coefficient is universal or class-specific.

## Damage channels and elements

Every damaging hit can carry at least two independent classifications:

1. **Damage channel:** Physical or Magical.
2. **Element:** one of DiceFree's elements inherited/expanded from DiceBound, or no special element where appropriate.

An element is not inherently physical or magical. The same element can appear on either channel depending on the attack.

Example:
- Physical Coffee damage
- Magical Coffee damage

This lets classes, monsters and gear interact separately with broad damage channels and specific elements.

## Physical and Magical Defense

DiceFree has:
- **Physical Defense**
- **Magical Defense**

Both use diminishing returns rather than linear immunity scaling.

Defense is converted into mitigation against its corresponding damage channel.

The exact diminishing-returns formula will be balanced later.

## Elemental resistance

Characters can also gain resistance to specific elements.

Channel mitigation and elemental resistance multiply rather than add.

Example:

```
Incoming damage: 100 Physical Coffee
Physical mitigation: 50%
Coffee resistance: 50%

100 × 0.50 × 0.50 = 25 damage taken
```

This means defenses stack strongly but do not simply add to 100% immunity.

The exact caps, negative resistance rules, penetration and diminishing-return behaviour remain to be designed.

## Equipment families and Intrinsics

There is no armor-weight system such as Cloth/Leather/Mail/Plate.

Classes instead differ through which equipment families they can equip.

Equipment families can have significantly different fixed stat identities.

**Intrinsics** remain a core gear concept and can provide much larger fixed stat bonuses than their DiceBound equivalents.

## Attack speed

Basic attacks have attack-speed values.

Classes can have very different:
- base attack speed;
- attack animation timing;
- attack range;
- scaling opportunities.

Attack speed can later be modified by attributes, equipment, effects or class mechanics as appropriate.

## Critical hits

Critical-hit design is intentionally unresolved.

Possible models include:
- crit as a broadly available derived stat;
- crit tied partly to attributes;
- crit granted only by certain classes/passives/items;
- different crit rules for attacks, spells, healing and damage-over-time effects.

Do not assume every class automatically has the same crit system.

## Movement and mobility

There is no universal dodge/roll.

Mobility is class identity:
- some classes get dashes/teleports/rolls;
- others get none;
- base move speed can differ by class;
- strong mobility should have tradeoffs.

## Threat

DiceFree has a proper threat system.

Every relevant combat action can define its own threat value/coefficient, including:
- basic attacks;
- damaging skills;
- healing;
- buffs/support;
- special tank abilities;
- crowd control where appropriate.

Healing generates threat.

Healers are also expected to contribute damage, closer to Final Fantasy XIV's healer philosophy than a pure "stand still and only heal" model.

### Taunts

Tank kits can use multiple styles of threat control.

Examples:
- forced target/taunt for X seconds;
- large threat-generation abilities;
- abilities that both force target temporarily and establish/catch up threat.

Boss mechanics may deliberately ignore or override normal threat.

## Healing/support targeting

Support abilities can target:
- characters in the 3D world;
- eligible allies through party frames/UI.

## Fixed kits

Classes use **fixed ability kits**, not a large library of skills that players swap in and out freely.

Advancement may add, evolve or replace parts of that fixed kit.

Gear, Intrinsics and special effects can modify how the fixed kit behaves.

## Consumables

Consumables have cooldowns.

Different consumables can use different cooldown durations/groups.

Classes, passives, gear and profession effects may modify consumable behaviour.

## Resurrection

Repeatable healer resurrection is allowed but costly:
- long cast time;
- high resource cost;
- stacking Resurrection Sickness.

Current working sickness direction:
- each stack reduces **all character stats by 10%**.

Exact duration, maximum stacks and whether the reduction uses additive or multiplicative stacking still need balancing.

## Enemy level display

Enemy levels are normally visible.

Current working idea:
- enemies far enough above the character may display **???** instead of an exact level.

A threshold around 20+ levels above the character is a candidate, not yet final.

## Open questions

- exact mouse-button/context conventions;
- offensive targeting details;
- Hybrid mode;
- controller support;
- final secondary effect of each primary attribute;
- universal vs class-specific Vitality-to-HP coefficient;
- exact defense diminishing-returns formula;
- resistance caps/penetration;
- crit system;
- exact Resurrection Sickness math;
- exact ??? level threshold.
