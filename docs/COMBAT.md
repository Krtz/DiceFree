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

Core attributes:
- **Vitality**
- **Strength**
- **Agility**
- **Intelligence**
- **Spirit**

Every attribute should provide at least one useful secondary effect.

Every class has one or more **primary attributes** used for its base/basic-attack scaling.

Multi-primary scaling is defined **case by case per class**. There is no universal hybrid formula. A class may weight two stats differently, equally, or in some other deliberate way.

A future secret class using all five attributes as primary is compatible with the system.

Current secondary-effect ideas remain provisional:
- Strength -> physical durability and/or HP-related benefit;
- Agility -> attack speed, movement speed and/or evasion;
- Intelligence -> magical durability and/or resource-related benefit;
- Spirit -> healing-related benefit;
- Vitality -> maximum HP.

## Health

Vitality contributes to maximum HP.

Conceptually:

```
Max HP = class base HP + (Vitality × HP coefficient) + other modifiers
```

Open question: whether the Vitality coefficient is universal or class-specific.

## Resources

Different classes can use different resources.

Intelligence should never be completely useless to a class just because that class does not use Mana, but Intelligence does **not** universally mean resource regeneration.

Class/resource interactions with Intelligence are defined by class/system design.

An advancement may keep, modify or replace the previous resource entirely.

## Damage instances, channels and elements

Every damage event is represented as one or more **separate damage instances**.

Each damage instance has its own:
- damage amount;
- Physical or Magical channel;
- element(s), if any;
- mitigation/resistance calculation;
- on-hit/trigger context where relevant.

If an attack contains multiple packets, they are processed independently.

Example:

```
One sword swing:
- 80 Physical Fire
- 20 Magical Fire
```

Those are two separate damage instances, not one blended calculation.

If an attack has multiple elements, those should also be represented as separate damage instances rather than one multi-element packet.

Example:

```
One attack:
- 50 Physical Coffee
- 50 Physical Fire
```

This keeps resistance calculations, logs, triggers and debugging clear.

## Physical and Magical Defense

DiceFree has:
- **Physical Defense**
- **Magical Defense**

Both use diminishing returns rather than linear immunity scaling.

Most survivability comes from:
- class base stats;
- equipment;
- abilities/passives;
- temporary effects.

Attributes may contribute modestly, but should not replace those systems.

The exact diminishing-returns formula will be balanced later.

## Elemental resistance

Characters can gain resistance to specific elements.

Channel mitigation and elemental resistance multiply rather than add.

Example:

```
Incoming damage: 100 Physical Coffee
Physical mitigation: 50%
Coffee resistance: 50%

100 × 0.50 × 0.50 = 25 damage taken
```

The exact caps, negative resistance rules and penetration rules remain to be designed.

## Attack speed

Basic attacks have attack-speed values.

Classes can have very different:
- base attack speed;
- attack animation timing;
- attack range;
- scaling opportunities.

## Critical hits

Critical hits are **not assumed to be a universal baseline mechanic for every class**.

Crit can instead be introduced through:
- class passives;
- abilities;
- equipment;
- special systems;
- specific advancement identities.

Different systems may define whether attacks, spells, healing or damage-over-time effects are allowed to crit.

Exact crit rules remain to be designed.

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

Bosses and special enemies may deliberately override ordinary threat.

### Taunts and threat builders

Tank kits can include:
- temporary forced-target taunts;
- very high threat-generating abilities;
- abilities that both force target and establish/catch up threat.

Not every tank tool needs to work the same way.

## Healers

Healers are full combat classes, not passive health-bar babysitters.

Healers should:
- have real damage rotations;
- contribute meaningful DPS while keeping the party alive;
- be able to play solo;
- farm and grind effectively;
- use healing as part of their class loop rather than their only activity.

Different healer classes may connect damage and healing in very different ways.

## Healing/support targeting

Support abilities can target:
- characters in the 3D world;
- eligible allies through party frames/UI.

## Fixed kits

Classes use **fixed ability kits** rather than freely swapping from a large skill library.

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

Resurrection Sickness affects the **five primary attributes only**:
- Vitality
- Strength
- Agility
- Intelligence
- Spirit

Current working direction:
- each stack reduces those five attributes by roughly 10%.

Derived effects then naturally change because the underlying attributes changed.

Exact duration, stack cap and stacking math remain to be balanced.

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
- detailed crit rules;
- exact Resurrection Sickness math;
- exact ??? level threshold;
- in-combat gear swapping.
