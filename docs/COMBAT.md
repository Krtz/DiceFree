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

Default baseline:

```
Max HP = class base HP + (Vitality × 15 HP) + other modifiers
HP regeneration = Vitality × 0.1 HP/sec + other modifiers
```

Classes can explicitly override these default Vitality coefficients. Items/effects can modify them further.

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

### Load/reset defaults

Normal save/load is not a combat-state resume:
- manifestation loads at full HP;
- ordinary combat/consumable cooldowns reset;
- temporary buffs/debuffs reset;
- threat/aggro/targets/casts/projectiles reset;
- resources reset by default unless that resource explicitly opts into load persistence.

The resource/effect frameworks must still allow explicit persistent exceptions for rare authored mechanics.

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

## Summons, pets and controllable units

Owned combat units use a shared summon/companion framework.

Important rules:
- owned units do not consume player party slots;
- there is no universal gameplay summon cap;
- each ability/class defines its own quantity/lifetime limits;
- autonomous, directly controllable and hybrid units are supported;
- ownership and control are separate concepts and control can be transferred;
- summons share the owner's **level** rather than leveling independently;
- stats can be authored, snapshot-inherited, dynamically inherited or mixed;
- owned units can have their own/shared/no resources;
- own threat by default, with authored threat-transfer mechanics;
- persistent companions can have stable IDs and equipment;
- ally targeting is tag-filterable;
- no special summon collision exception is assumed until playtesting demonstrates a need.

See `docs/SUMMONS_AND_COMPANIONS.md`.

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

## Enemy AI and encounter architecture

Enemies use authored/fixed levels by default and do not universally scale to the player.

Enemy definitions can compose:
- base archetype;
- variant overrides;
- direct monster stats and/or primary attributes;
- combat actions;
- threat/targeting policy;
- AI decision rules;
- leash/home behavior;
- semantic tags;
- reusable modifiers.

AI may be as simple as "attack closest enemy" or use complex AND/OR conditional priorities for advanced enemies/bosses.

Encounter-wide state such as phases, hazards, doors, adds and timers belongs to reusable encounter definitions rather than being forced into one boss actor.

See `docs/ENEMIES_AND_ENCOUNTERS.md`.

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


### Defense penetration basis and order

Percentage Defense penetration resolves before flat Defense penetration, but percentage penetration is calculated from the target's **original positive Defense**, not from the already-reduced remainder.

Flat penetration may push effective Defense below zero.

If original Defense is negative, percentage Defense reduction and percentage Defense penetration do not apply; negative base Defense is not an intended normal content state.


### Penetration below zero

Penetration is not clamped at zero Defense.

If penetration exceeds the target's current Defense, effective Defense becomes negative and the normal negative-Defense vulnerability curve applies.

Example:
```
5 Defense - 10 flat penetration = -5 effective Defense
```


### Defense reduction vs penetration

Defense reduction resolves before penetration.

Percentage reduction and percentage penetration both reference the target's original positive Defense value for the resolution.

Default conceptual order:
1. establish original Defense;
2. subtract percentage reduction(s), additive from original positive Defense;
3. subtract flat reduction;
4. subtract percentage penetration from original positive Defense;
5. subtract flat penetration;
6. apply mitigation/vulnerability from final effective Defense.


### Full Defense modifier ordering

Percentage Defense reduction resolves before flat Defense reduction.

Default Physical/Magical Defense pipeline:

1. establish original Defense;
2. percentage Defense reduction(s), additive from original positive Defense;
3. flat Defense reduction;
4. percentage Defense penetration, calculated from original positive Defense;
5. flat Defense penetration;
6. final mitigation/vulnerability curve.

Flat reduction and flat penetration can create negative effective Defense.

Reduction is target-state modification; penetration is attacker-specific.


### Underlying Defense for percentage shred/penetration

Percentage Defense reduction and percentage Defense penetration reference the target's underlying Defense before temporary positive Defense buffs.

Positive Defense buffs resolve percentage first, then flat.

Percentage reduction/penetration may exceed 100% and can push final effective Defense below zero.


### Defense percentage stacking

Positive percentage Defense buffs stack additively.

Percentage Defense reductions stack additively.

Percentage Defense penetrations stack additively.

All three use the relevant underlying Defense reference already defined by the Defense pipeline; reduction and penetration may exceed 100%.


### Defense reference stability and precision

Temporary negative Defense effects do not alter the underlying Defense reference; they affect only current/effective Defense.

Defense math keeps fractional precision through intermediate calculations. Rounding is presentation-only unless a specific mechanic explicitly requires discrete values.

The exact components that make up underlying Defense remain an open design question.


### Explicit critical-hit rules

DiceFree has no universal baseline crit chance or crit multiplier.

An attack, heal, DoT tick or other resolved effect can crit only when an explicit gameplay source grants permission. The granting source defines that crit rule's chance and multiplier/behavior.

Therefore:
- baseline crit chance is 0%;
- there is no default 150%, 200% or other global crit multiplier;
- direct damage does not automatically crit;
- DoTs do not automatically crit;
- healing does not automatically crit;
- any of those may crit if a skill, passive, item or other effect explicitly says they can.

Multiple simultaneous crit-rule interaction remains open; do not collapse authored crit rules into a global chance/multiplier without a later decision.


### Multiple explicit crit rules

When several explicit crit rules apply to the same resolved action, ordinary crit resolution uses priority by multiplier:

1. sort applicable crit rules from highest multiplier to lowest;
2. roll the highest-multiplier rule first;
3. on success, apply that rule and stop;
4. on failure, continue to the next rule;
5. if all fail, the action is non-critical.

This is intentionally not a merged global crit chance.

Rare authored rules may explicitly stack with or multiply another crit result, but such behavior must be stated by the source and is not part of default crit resolution.

### Explicit crit-rule modification

A source may explicitly alter another crit rule instead of adding its own independent roll.

Examples include modifying chance, modifying multiplier, or granting crit permission to a normally ineligible effect.

Such modifiers must declare their intended target/eligibility. There is no automatic global Crit Chance or Crit Damage stat.


### Crit modifier resolution order

Explicit crit-rule modifiers apply before crit-rule priority is determined.

After modifiers, ordinary crit rules are sorted by their final multiplier and rolled highest-first.

Ordinary crit chance is clamped to 0-100%. Values above 100% have no special default behavior; any overflow mechanic must be explicitly authored.

### Crit position in the damage pipeline

Default crit resolution modifies the raw damage packet before mitigation layers.

Default order:
1. construct raw damage packet;
2. apply successful crit multiplier/behavior;
3. apply Physical/Magical Defense;
4. apply elemental resistance;
5. apply resulting HP damage.

Rare explicit mechanics may override where their crit behavior is applied.


### Equal-multiplier crit tie-break

Ordinary crit rules sort by final modified multiplier descending.

For equal final multipliers:
1. explicit authored crit priority, if present;
2. otherwise stable deterministic source-ID order.

This keeps provenance deterministic without inventing a gameplay advantage from equal multipliers.

### Action-wide crit resolution

Default crit resolution happens once per action.

A multi-packet action shares one crit result across all eligible raw packets, which are then mitigated independently by their own Physical/Magical Defense and elemental resistance layers.

Per-packet crit rolls require an explicit mechanic.

Default critical-hit signaling is one critical-hit event per action. Explicit per-packet crit mechanics may opt into per-packet critical events.


### Crit modifier clarity and provenance

Crit modifiers are an available but intentionally uncommon tool.

Any modifier to crit chance or multiplier must explicitly define its operation, such as:
- additive percentage points;
- multiplicative change to existing chance;
- additive multiplier amount;
- multiplicative change to existing multiplier.

Do not infer one meaning from ambiguous "+X% crit" wording.

Critical resolution preserves both the originating action identity and the winning crit-rule/source identity. These are separate provenance fields for future action-trigger and source-trigger logic.
