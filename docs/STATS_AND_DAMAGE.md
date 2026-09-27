# Stats, Damage and Resistance

## Core attributes

DiceFree uses five primary attributes:

- **Vitality**
- **Strength**
- **Agility**
- **Intelligence**
- **Spirit**

Every class defines its own level-1 base values and per-level growth.

Players do not manually allocate attributes.

Classes may use one or more attributes as attack-scaling primary attributes. Multi-primary formulas are defined per class rather than by a global rule.

Examples can include:
- STR only;
- STR + INT with custom weights;
- highest of STR or AGI;
- highest single primary attribute;
- total of all five attributes for an exceptional class.

## Attribute secondary effects

### Vitality

Primary secondary effects:
- maximum HP;
- **flat HP regeneration**.

Conceptually:

```
Max HP = class base HP + (Vitality × class/universal coefficient) + modifiers
```

Exact coefficient model remains open.

### Strength

Current direction:
- contributes a **small/modest amount of Physical Defense**.

Strength should not replace class base defense, equipment or defensive passives.

### Agility

Current direction:
- contributes a **very small amount of Attack Speed**;
- contributes a **very small amount of Movement Speed**.

The coefficients should be intentionally small so high-Agility classes do not automatically become absurd machineguns or permanently outrun encounter design.

Meaningful Attack Speed/Movement Speed increases should primarily come from:
- gear;
- abilities;
- passives;
- temporary effects;
- class identity.

Generic Evasion is not currently committed.

### Intelligence

Current direction:
- contributes a **small/modest amount of Magical Defense**.

Intelligence can additionally interact with class resources where appropriate, but it does **not** universally mean Mana, resource regeneration or maximum resource.

It must still provide useful value to non-Mana classes.

### Spirit

Current direction:
- improves healing done;
- improves healing received.

These two effects can use different coefficients.

Spirit is the default/expected healer attribute, but not every healer must use Spirit as its primary attack attribute and not every tank/damage class must ignore Spirit.

## Class identity beats attribute stereotypes

Attributes provide a common language, not rigid class boxes.

Examples:
- a normal physical tank may favor STR;
- a mage tank may favor INT and use a Mana-shield mechanic;
- a healer may favor SPI;
- another healer may be STR/SPI or INT/SPI;
- secret/hybrid classes can use unusual combinations.

## Skill scaling

Every ability defines its own scaling.

Abilities may scale from:
- one attribute;
- multiple weighted attributes;
- the highest of a set of attributes;
- the highest attribute overall;
- total attributes;
- HP/resource values;
- other class-specific values.

The class's primary attribute is a default identity, not a rule that every skill must use exactly that stat.

## Tooltip presentation

Normal tooltips should prioritize readability, League-of-Legends style:

```
Deals 245 damage
Costs 40 Mana
Cooldown: 8s
```

Holding a modifier key such as **Shift** should expand the tooltip to show the underlying calculation, coefficients and relevant damage-instance breakdown.

Exact modifier/control remains to be finalized.

## Damage instances

Every damage packet is a separate damage instance.

Each instance independently carries:
- raw amount;
- Physical or Magical channel;
- one element or no element;
- crit eligibility/rules;
- penetration/resistance modifiers;
- trigger/on-hit context.

Mixed attacks therefore resolve as multiple instances.

Example:

```
Flaming sword hit:
- 80 Physical Fire
- 20 Magical Fire
```

A multi-element attack likewise splits into separate instances:

```
- 50 Physical Coffee
- 50 Physical Fire
```

This rule exists for predictable calculations, triggers, debugging and combat logging.

## Physical and Magical Defense

DiceFree has:
- **Physical Defense**
- **Magical Defense**

Both use diminishing-return formulas.

Primary sources:
- class base values;
- equipment;
- abilities/passives;
- temporary effects.

STR/INT can contribute modest amounts but are secondary sources.

Exact formulas remain a balance problem.

## Elemental resistances

DiceFree retains the element system from DiceBound.

An element is independent from Physical/Magical channel.

Examples:
- Physical Fire;
- Magical Fire;
- Physical Coffee;
- Magical Coffee.

Characters and enemies can have separate resistance values for each element.

### Negative baseline

The current direction is that characters and enemies begin with **negative elemental resistance by default**, rather than neutral 0%.

Default elemental resistance starts at **-10%** for players and enemies unless content/class design explicitly overrides it.

This creates room for:
- resistance-building as meaningful defense;
- resistance auras;
- temporary resistance buffs;
- resistance debuffs;
- monster-specific weaknesses;
- monster-specific strengths;
- elemental party synergies.

### Resistance buffs/debuffs

Examples the system should support:
- +5% all elemental resistance aura to allies;
- +15% Fire resistance to allies;
- -10% Fire resistance to enemies;
- +35% all resistance for 10 seconds;
- boss has high Tech resistance but Coffee weakness.

### Damage calculation

Physical/Magical mitigation and elemental resistance multiply.

Example:

```
100 Physical Coffee damage
50% Physical mitigation
50% Coffee resistance

100 × 0.50 × 0.50 = 25 final damage
```

Negative resistance increases elemental damage.

### Resistance representation

Working preference: elemental resistance is expressed to players as an actual **percentage**, with:
- a normal maximum resistance cap;
- negative values allowed;
- exceptional classes/items/effects capable of raising the normal cap.

Default normal maximum elemental resistance is **75%**.

Elemental resistance is represented directly as a percentage rather than as a hidden rating conversion.

Rules:
- baseline: -10%;
- normal cap: 75%;
- negative resistance is allowed;
- exceptional classes/items/effects may raise the 75% cap;
- exact minimum floor and cap-raising limits remain open.

## Elemental healing

Healing is simply **Healing**; it does not use Physical/Magical channels.

Healing may optionally have an element.

Elemental healing uses the target's matching elemental resistance in the **opposite gameplay direction from elemental damage**:

- positive resistance reduces matching elemental damage;
- positive resistance increases matching elemental healing;
- negative resistance increases matching elemental damage;
- negative resistance reduces matching elemental healing.

Conceptually:

```
Final elemental heal = Base heal × (1 + target elemental resistance)
```

Examples:
- +50% Fire resistance -> Fire healing × 1.50;
- -20% Fire resistance -> Fire healing × 0.80.

This creates support interactions where resistance buffs can simultaneously protect and improve matching healing.

Whether a heal is elemental or non-elemental is defined case by case by that heal/ability.

Non-elemental healing ignores elemental resistance.

Elemental healing uses the full matching resistance value unless a specific ability explicitly says otherwise.

## Penetration and resistance modification

The framework must support, case by case:
- **flat Physical Defense penetration**;
- **percentage Physical Defense penetration**;
- **flat Magical Defense penetration**;
- **percentage Magical Defense penetration**;
- flat/percentage elemental resistance penetration where appropriate;
- resistance reduction;
- temporary vulnerability;
- resistance-cap modification.

Different classes/advancements may use different penetration models. An early class might use flat penetration while a later advancement upgrades into percentage penetration.

Do not assume every class has access to these.

## Critical hits

Crit is not universal by default.

Classes/items/abilities can opt into crit mechanics.

Different effect types can define separate crit eligibility:
- basic attacks;
- abilities;
- DoTs;
- healing;
- summons.

Exact crit formulas remain open.

## Damage-over-time effects

Each DoT tick is its own damage instance.

A tick uses the target's **current** defenses, elemental resistance, penetration/debuff state and other relevant mitigation at the moment that tick resolves.

This means defensive buffs applied after a DoT is already active can reduce later ticks.

## Adaptive-scaling UI

If an ability uses adaptive scaling such as "highest of STR/AGI" or "highest attribute", the expanded tooltip must show which attribute is currently being used and the resulting coefficient/calculation.

## Stat transparency

Character-sheet/stat tooltips should expose what each attribute currently contributes.

Examples:
- current HP gained from Vitality;
- current Physical Defense contribution from Strength;
- current Attack/Move Speed contribution from Agility;
- current Magical Defense contribution from Intelligence;
- current healing bonuses from Spirit.

The normal UI can remain clean, but the underlying numbers should be inspectable.

## Combat telemetry

Build combat resolution so every damage/healing instance can expose debug information such as:
- source;
- target;
- raw value;
- attribute scaling;
- channel;
- element;
- defense mitigation;
- elemental resistance;
- penetration;
- crit/modifiers;
- final value.

A developer combat log/damage-meter framework should exist even if detailed player-facing meters/logs are never shipped.

## Floating combat text

Damage and healing numbers should be visible in combat, with user options to reduce or disable them.

The framework should support aggregation/grouping later if rapid multi-instance attacks become visually noisy, but aggregation behaviour is not yet defined.
