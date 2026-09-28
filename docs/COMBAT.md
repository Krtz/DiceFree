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

Detailed input/selection/camera rules are documented in `docs/INPUT_CONTROLS.md`.

### Classic / mouse movement
Warcraft III / League-style click-to-move is the primary balance baseline.

Contextual right-click handles movement/attack/interact.

Attack-Move, Stop and Hold Position are dedicated commands.

### Direct / WASD movement
WASD feeds the same movement/combat systems and must not become the hidden requirement for encounter execution.

Classic Mouse and Direct/WASD use **different default keybind profiles**, because Direct mode reserves WASD for movement. Everything remains fully rebindable.

### Selection safety

Unit-selection behavior must be configurable enough to sit between ARPG and Warcraft III RTS conventions.

Accidental selection of allies/summons/other units must never make emergency movement unresponsive during combat.

A simultaneous Hybrid mode remains possible but not committed.

### Unit collision and ghosting

Heroes, enemies and normal controllable units occupy physical space and participate in pathing/collision.

They are not default ghost units that freely pass through each other.

Ghosting/phasing is a rare explicit mechanic for specific classes, passives, enemies or effects.

## Combat actions and ability composition

Basic attacks and active abilities should resolve through the same core combat-action/effect pipeline where practical.

A combat action can compose reusable pieces such as:
- targeting rule/shape;
- range;
- cast/wind-up time;
- resource cost;
- cooldown/cooldown groups;
- one or more damage/healing packets;
- Physical/Magical channel;
- element;
- applied status/effects;
- movement/knockback;
- summon/spawn behavior;
- threat;
- resource generation/consumption;
- other hooks.

Content should be data/composition-first, with a clean custom-code escape hatch for genuinely unusual class mechanics.

Do not require a bespoke combat engine for each class.

## Basic attacks

Every normal class has a Warcraft III / League-style repeating basic attack:
- command an attack on a target;
- move into attack range if necessary;
- repeatedly attack while the target remains valid.

Basic attacks may be melee or ranged depending on class.

Because basic attacks use the common action/effect pipeline, classes and gear may hook them to:
- generate Mana/Rage/Energy/Combo Points or other resources;
- apply on-hit effects;
- consume charges/stacks;
- trigger class mechanics;
- interact with lifesteal, elemental imbuements, accuracy/blind, threat and proc systems.

A later class may, for example, build Combo Points from ordinary auto-attacks and spend them on active skills.

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

A class may have **multiple simultaneous resources**.

Examples:
- Mana only;
- Energy + Combo Points;
- Mana + Souls;
- Rage + Charges;
- several bespoke resources for exceptional classes.

Resources are independently defined components/data rather than one universal resource enum or mandatory Mana field.

### Resource lifecycle

Each resource defines its own lifecycle rules, including combinations of:
- persist normally;
- reset on combat end;
- reset on death;
- persist through death;
- decay over time;
- decay only out of combat;
- regenerate over time;
- generate only through actions;
- cap/floor behavior.

There is no universal "resources reset after combat" rule.

### Target-bound vs owner-bound resources

The framework supports both:
- **owner-bound** resources, such as generic Combo Points stored on the player;
- **target-bound** resources, such as Combo Points/stacks attached to a specific enemy relationship.

A class can choose either model.

### Multi-resource costs

One ability may require/consume several costs simultaneously.

Examples:
- 30 Mana + 3 Combo Points;
- 20% current HP + 50 Mana;
- one item/charge + Mana;
- all available stacks plus a flat resource cost.

Cost validation/payment should be composable rather than bespoke per class.

Intelligence should never be completely useless to a class just because that class does not use Mana, but Intelligence does **not** universally mean resource regeneration.

Class/resource interactions with Intelligence are defined by class/system design.

An advancement may keep, modify, add or replace resources entirely.

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

## Ability charges

Abilities may have multiple charges.

The framework supports different recharge policies, including:
- **independent recharge** — each spent charge tracks/recharges independently;
- **sequential recharge** — one charge recharges, then the next begins;
- **all-at-once recharge** — one cooldown restores all charges together.

Charges may interact with normal/shared cooldown groups according to the ability definition.

## Casting, channeling and interruption

Every ability defines its own cast/movement/interrupt behavior.

Supported dimensions include:
- instant vs cast-time;
- normal cast vs channel;
- can/cannot move while casting;
- movement interrupts or does not interrupt;
- taking damage interrupts or does not interrupt;
- explicit interrupt/silence behavior;
- channel tick cadence;
- cast/channel completion behavior;
- optional spell pushback/delay.

**Default direction:** no universal spell pushback. Damage does not automatically delay every cast.

Spell pushback is supported for abilities/classes that explicitly use it.

## Cooldowns and shared cooldown groups

DiceFree has **no universal global cooldown by default**.

Abilities normally use their own:
- cooldown;
- resource cost;
- cast/wind-up/recovery rules.

The framework must still support:
- per-class or per-ability shared cooldown groups;
- explicit GCD-like groups for classes that need them;
- item/consumable cooldown groups;
- cooldown modifiers;
- **shared cooldown state across different players** when an encounter/class mechanic explicitly requires it.

Cross-player shared cooldowns are exceptional authored mechanics, not the default for ordinary abilities.

## Fixed kits

Classes use fixed ability kits rather than freely swapping from a large skill library.

Advancement may add, evolve or replace parts of that fixed kit.

Gear, Intrinsics and special effects can modify how the fixed kit behaves.

Normal tooltips should show readable final values. Holding a modifier key such as Shift should expose detailed formulas/scaling where useful, including the currently-selected attribute for adaptive-scaling abilities.

## Trigger / proc framework

Classes, gear, effects, encounters and world systems may react to semantic combat/gameplay triggers.

The framework should support a broad trigger vocabulary, for example:
- OnBasicAttack;
- OnAttackStarted;
- OnHit;
- OnCrit;
- OnDamageDealt;
- OnDamageTaken;
- OnHealGiven;
- OnHealReceived;
- OnKill;
- OnDeath;
- OnResourceGenerated;
- OnResourceSpent;
- OnEffectApplied;
- OnEffectRemoved;
- OnCastStarted;
- OnCastCompleted;
- OnInterrupt;
- OnBlock/Absorb;
- OnThornsDamageGiven;
- OnThornsDamageReceived;
- authored/custom trigger conditions.

The trigger system should be extensible enough for intentionally strange future mechanics rather than limited to a frozen small enum.

### Proc origin and recursion safety

Generated actions/events carry semantic origin/context such as:
- basic attack;
- active ability;
- DoT/HoT tick;
- proc;
- reflection/thorns;
- aura;
- summon;
- environmental/world effect;
- other authored origins.

Default proc policies prevent accidental infinite recursion.

Examples:
- reflection should not recursively reflect itself forever;
- a proc-generated hit should not automatically retrigger the same proc chain unless allowed.

**Important:** recursion prevention is a default safety policy, not a hard ban.

A future class/item may deliberately allow **controlled recursion**. Such mechanics must opt in explicitly and define limits/conditions such as depth, count, cooldown, diminishing value or eligible trigger origins.

## Periodic effects: DoTs / HoTs

Periodic effects can choose their scaling model:
- **snapshot** — relevant source stats/modifiers are captured when applied;
- **dynamic** — values are recalculated from live state each tick.

Finite-duration DoTs/HoTs commonly default to snapshot unless content specifies otherwise, but both models are first-class.

## Summon source attribution

Summon actions retain both:
- the **immediate source** (the summon);
- the **owner/controller** (the player/Echo manifestation or other owning actor).

Different systems can choose the attribution they need.

Examples:
- summon owns its own threat entry;
- quest/XP credit may resolve to the owning player;
- a proc may explicitly care about summon-origin damage;
- combat logs can show both source and owner.

## Auras

Auras are reusable **effect emitters**.

An aura defines:
- emitter/source;
- radius/shape;
- valid target relationship/filter;
- emitted effect identity;
- update/application/removal behavior.

Leaving the aura's valid area, emitter death/despawn, or aura removal stops that emitter's contribution.

Normal effect stacking policy then resolves overlap:
- strongest wins;
- unique per source;
- capped stacks;
- etc.

The same framework supports beneficial player auras, enemy debuff auras, item auras and future world effects.

## Effect stacking and dispels

Effects define their own stacking/refresh policy in data.

Supported policies include:
- unique per source;
- refresh duration;
- replace with stronger;
- add stacks up to a cap;
- independent instances;
- strongest copy wins;
- other explicit authored policies.

**Default direction:** many ordinary effects are **unique per source**, allowing different actors to maintain their own copy without one source creating uncontrolled duplicates.

### Dispel strength

The primary gameplay-facing dispel axis is:
- **Weak Dispel**;
- **Strong Dispel**;
- **Undispellable**.

A Strong Dispel can remove effects that a Weak Dispel can remove plus effects explicitly requiring Strong Dispel.

Undispellable effects cannot be removed by ordinary dispels.

Effects may additionally carry semantic tags such as:
- Poison;
- Curse;
- Disease;
- Bleed;
- Magic;
- CC;
- elemental/status families.

These tags support situational tools such as antidotes or specialized class abilities, but normal healer kits should not be forced into a large collection of narrow one-tag-only cleanse buttons unless that restriction is part of the ability's identity.

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

## Combat state

Threat-list membership / active aggro is the primary definition of being "in combat".

See `docs/STATS_AND_DAMAGE.md` for shield ordering, Pure Damage, lifesteal, reflection, dispels, summons, death prevention and CC framework rules.

## Open questions

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
