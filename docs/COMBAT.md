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

The detailed stat model is documented in `docs/STATS_AND_DAMAGE.md`.

Core attributes:
- Vitality
- Strength
- Agility
- Intelligence
- Spirit

Current secondary direction:
- VIT -> HP and HP regeneration;
- STR -> modest Physical Defense;
- AGI -> very small Attack Speed and Movement Speed gains;
- INT -> modest Magical Defense plus class-specific resource interactions where appropriate;
- SPI -> healing done and healing received, with separate scaling.

Class base stats, gear, abilities and passives remain more important than these secondary attribute bonuses.

Classes can use one or more primary attributes, and individual skills can use bespoke/adaptive scaling.

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

## Damage, defense, resistances and crit

Detailed damage-instance, Physical/Magical Defense, elemental resistance, elemental healing, penetration and crit rules are maintained in `docs/STATS_AND_DAMAGE.md`.

Important combat rules:
- mixed damage uses separate damage instances;
- every instance has one Physical/Magical channel and one element/no element;
- Physical/Magical Defense and elemental resistance are separate mitigation layers;
- elemental resistance may be negative;
- elemental healing can use matching resistance as a positive healing modifier;
- crit is opt-in rather than universally assumed.

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

### Threat UI

Default threat presentation should be simple and readable, using color/state indicators rather than a permanent wall of numbers.

Players may enable more detailed threat information as an option.

The underlying system should always track exact values for debugging and encounter logic.

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

Classes use fixed ability kits rather than freely swapping from a large skill library.

Advancement may add, evolve or replace parts of that fixed kit.

Gear, Intrinsics and special effects can modify how the fixed kit behaves.

Normal tooltips should show readable final values. Holding a modifier key such as Shift should expose detailed formulas/scaling where useful, including the currently-selected attribute for adaptive-scaling abilities.

## Shields, sustain and combat-effect framework

The detailed generic framework for shields, lifesteal, reflection, immunities and CC resistance is documented in `docs/STATS_AND_DAMAGE.md`.

Key rules:
- shields can filter/scale differently by damage channel or element;
- default overhealing disappears, but classes/items may convert it;
- lifesteal/damage-to-healing is framework-level;
- reflection/thorns cannot accidentally recurse forever;
- immunity/untargetable flags are first-class statuses;
- CC resistance and repeated-CC diminishing returns are supported from the framework level.

HP regeneration remains active in combat. Resource regeneration is class/resource-specific.

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
