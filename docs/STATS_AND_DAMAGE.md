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

Default baseline:

```
Max HP = class base HP + (Vitality × 15 HP) + modifiers
Base HP regeneration = Vitality × 0.1 HP/sec
```

These are **default coefficients**, not immutable universal constants.

Classes may explicitly override their Vitality-to-HP and Vitality-to-regeneration coefficients as part of class identity.

Items, passives, buffs and other effects may also modify the effective coefficients or add independent flat/percentage HP and regeneration.

If no override is authored, use:
- **+15 maximum HP per Vitality**;
- **+0.1 HP/sec per Vitality**.

### Strength

Default secondary effect:
- **1 Strength = +0.2 Physical Defense**.

This is a default coefficient rather than an immutable universal constant.

Classes/items/passives/effects may explicitly modify the Strength-to-Physical-Defense relationship.

Strength should remain a secondary contributor and should not replace class base defense, equipment or defensive passives.

### Agility

Default secondary effects:
- **1 Agility = +0.025% Attack Speed**;
- **1 Agility = +0.01% Movement Speed**.

The Attack Speed coefficient is intentionally tiny so high-Agility classes do not automatically become absurd machineguns.

The coefficient is a default rather than an immutable universal constant. Classes/items/passives/effects may explicitly modify the Agility-to-Attack-Speed relationship.

The Movement Speed coefficient is intentionally tiny so Agility does not automatically let classes permanently outrun encounter design.

All numeric secondary-stat coefficients in this section are current balance defaults. They are expected to be tunable through playtesting and may change without altering the underlying stat architecture.

Meaningful Attack Speed/Movement Speed increases should primarily come from:
- gear;
- abilities;
- passives;
- temporary effects;
- class identity.

Generic Evasion is not currently committed.

### Intelligence

Default secondary effect:
- **1 Intelligence = +0.2 Magical Defense**.

This mirrors Strength -> Physical Defense.

The coefficient is a default rather than an immutable universal constant. Classes/items/passives/effects may explicitly modify the Intelligence-to-Magical-Defense relationship.

Intelligence can additionally interact with class resources where appropriate, but it does **not** universally mean Mana, resource regeneration or maximum resource.

It must still provide useful value to non-Mana classes.

### Spirit

Default secondary effects currently settled:
- **1 Spirit = +0.15% Healing Done**;
- **1 Spirit = +0.075% Healing Received**.

These intentionally use different coefficients.

Both coefficients are balance defaults, not immutable canon. Classes/items/passives/effects may explicitly modify or override them.

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

Each class defines its own starting Physical Defense and Magical Defense as part of its base stat package.

Physical and Magical Defense use the **same diminishing-return formula family**.

Current balance-default formula:

```
q = (abs(Defense) / 300)^0.7
```

For nonnegative Defense:

```
DamageTakenMultiplier = 1 / (1 + q)
Mitigation = q / (1 + q)
```

For negative Defense, use the same curve symmetrically as vulnerability:

```
DamageTakenMultiplier = 1 + q / (1 + q)
```

Examples:
- +10 Defense -> ~91.5% damage taken (~8.5% mitigation)
- +100 Defense -> ~68.3% damage taken (~31.7% mitigation)
- +300 Defense -> 50% damage taken
- +1000 Defense -> ~30.1% damage taken (~69.9% mitigation)
- -10 Defense -> ~108.5% damage taken
- -100 Defense -> ~131.7% damage taken
- -300 Defense -> 150% damage taken
- extremely negative Defense approaches, but does not exceed, 200% damage taken from Defense alone.

The constants `300` and exponent `0.7` are current balance defaults and may change through playtesting without changing the underlying Defense architecture.

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
- there is currently **no hard negative resistance floor**;
- exceptional classes/items/effects may raise the 75% cap;
- if uncapped negative resistance becomes a balance problem later, a floor can be introduced.

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
- -20% Fire resistance -> Fire healing × 0.80;
- -100% Fire resistance -> Fire healing × 0;
- below -100% Fire resistance -> matching Fire "healing" becomes damage instead.

This creates support interactions where resistance buffs can simultaneously protect and improve matching healing.

Whether a heal is elemental or non-elemental is defined case by case by that heal/ability.

Non-elemental healing ignores elemental resistance.

Elemental healing uses the full matching resistance value unless a specific ability explicitly says otherwise.

There is intentionally **no clamp at zero**. Extremely negative matching resistance can invert elemental healing into damage.

This is considered valid/cursed design space rather than an error.

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

### Penetration and elemental healing

Exact penetration/healing interaction is intentionally open.

Current design intuition: if an attacker/healer has +10% Fire penetration, a Fire heal might treat the target as having **10 percentage points more Fire resistance** for that heal, effectively making penetration beneficial to matching elemental healing.

This is not yet locked and must be validated for consistency/exploit risk.

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


## Resolution buckets and ordering

Damage/healing calculations should use explicit resolution stages ("buckets") rather than ad-hoc modifier order.

The exact order is not fully locked yet, but the framework must distinguish at least:

1. **Source construction**
   - base value;
   - skill coefficients;
   - class/resource modifiers;
   - additive/multiplicative source bonuses.
2. **Instance definition**
   - Physical/Magical channel;
   - element;
   - crit eligibility;
   - penetration/reduction tags;
   - special flags.
3. **Target mitigation**
   - Physical/Magical Defense;
   - elemental resistance;
   - shields/absorbs;
   - immunity/invulnerability rules.
4. **Final modifiers**
   - encounter-specific reductions/amplifications;
   - vulnerability windows;
   - minimum/maximum rules if any.
5. **Post-resolution triggers**
   - lifesteal;
   - on-hit;
   - thorns/reflection;
   - proc generation;
   - combat telemetry.

The precise mathematical ordering inside these buckets will be finalized during combat prototyping and must be deterministic/documented.

## Regeneration

### HP regeneration

Vitality provides flat HP regeneration.

Default: **0.1 HP/sec per Vitality** unless class/content explicitly overrides the coefficient.

HP regeneration is **always active**, including in combat.

Classes/items/effects may add:
- increased out-of-combat regeneration;
- delayed regeneration bonuses;
- combat-only regeneration;
- conditional regeneration.

### Resource regeneration

Resource regeneration is entirely resource/class-specific.

A resource may:
- regenerate constantly;
- regenerate only out of combat;
- build through attacks/abilities;
- decay over time;
- refill through class mechanics;
- use no passive regeneration at all.

## Shields and absorbs

The combat framework must support generic and filtered shields.

A shield definition can specify:
- total absorb amount;
- eligible damage channels;
- eligible elements;
- per-channel/per-element efficiency;
- priority/order;
- expiration;
- whether overflow passes through.

Examples:
- 500 generic damage absorb;
- 500 Fire damage absorb;
- 250 absorb against non-Fire damage;
- absorb Magical damage only;
- Mana shield that spends resource instead of HP;
- shield that converts prevented damage into another effect.

Shield behaviour is designed case by case using one common framework.

## Overhealing

Default behaviour:
- healing above maximum HP is lost.

Framework hooks should allow classes/items/passives to convert overhealing into:
- shields;
- resources;
- buffs;
- damage;
- other effects.

## Lifesteal and damage-to-healing

Lifesteal / spell-vamp / damage-to-healing effects are framework-level mechanics.

They must support:
- percentage of damage dealt;
- eligible damage types/abilities;
- self-only or ally healing;
- caps;
- reduced coefficients for DoTs/AoE where needed;
- element tags if relevant.

Avoid implementing these as one-off class hacks.

## Reflection / thorns

Reflection/thorns is framework-level.

Reflected damage must carry a special tag/context so it does **not recursively trigger further reflection** unless a specific mechanic intentionally allows it.

Framework should support:
- flat reflected damage;
- percentage reflected damage;
- channel/element restrictions;
- melee-only/ranged-only conditions;
- attacker/defender scaling.

## Immunity and targeting flags

Combat entities need framework-level flags/status support for cases such as:
- Invulnerable;
- Untargetable;
- Physical Immune;
- Magical Immune;
- Element Immune;
- Damage Immune but targetable;
- CC Immune;
- Knockback Immune;
- Root Immune;
- Stun Immune;
- Slow Immune.

These should be data/status driven rather than hard-coded boss exceptions.

## Crowd-control resistance and diminishing returns

The framework must support:
- resistance to individual CC categories;
- class/passive/item bonuses to CC resistance;
- boss-specific CC resistance/immunity;
- diminishing returns for repeated CC chains.

Goal: four stun-heavy classes should not permanently lock a boss simply by alternating stuns.

Exact DR system is not yet decided, but should be able to track categories such as:
- stun;
- root;
- silence;
- slow;
- knockback/knockup;
- fear/charm/confuse;
- other future hard/soft CC types.

Possible future models include:
- duration reduction after repeated applications;
- escalating temporary immunity;
- per-category DR;
- boss break-bars/stagger systems.

These values do not necessarily need to be prominent on the default player stat sheet, but they must exist in the underlying stat/effect framework and be inspectable where relevant.


## Pure damage

The combat framework supports a rare **Pure Damage** channel for exceptional mechanics.

Pure Damage:
- ignores Physical Defense;
- ignores Magical Defense;
- ignores elemental resistance;
- ignores ordinary penetration calculations because there is nothing to penetrate;
- deals its stated amount unless an effect explicitly says it can block/modify Pure Damage.

Example:

```
500 Pure Damage = 500 damage
```

Pure Damage is reserved for special cases and should not become a normal class damage channel.

Whether shields/immunities can absorb Pure Damage is defined explicitly by those shields/statuses.

## Shield resolution order

Shield consumption follows these defaults:

1. explicit shield priority overrides;
2. matching/specific shields before generic shields;
3. among equally eligible generic shields, **oldest applied is consumed first**.

Example:
- a Fire-only shield is consumed before a generic shield against Fire damage;
- a generic shield with an explicit high-priority "explode when broken" rule can override normal ordering;
- two ordinary generic shields resolve first-applied, first-consumed.

Incoming damage is normally mitigated by Defense/resistance **before** shield absorption.

Individual shield mechanics may explicitly override normal behavior.

## Mana/resource shields

Default resource-shield behavior:
- resolve normal damage mitigation first;
- then convert/absorb the remaining eligible damage through the shield's resource rule.

Classes may define exceptions case by case.

## Healing inversion damage

When elemental healing becomes negative because matching resistance is below -100%, the negative result becomes **damage with no Physical/Magical channel**.

It is not automatically Magical or Physical.

This inverted-healing damage uses the original healing element/context and is intended to integrate with the special/pure-like damage framework without pretending it was an ordinary attack.

Exact interaction with shields, immunity flags and Pure Damage rules should be explicit per effect.

## Healing crits

Healing does not crit by default.

If a class, passive, item or effect enables healing crits, eligible elemental and non-elemental heals may crit under that effect's rules.

## Lifesteal resolution

Default lifesteal/damage-to-healing uses:
- **final damage actually dealt after mitigation**;
- capped by **actual HP damage inflicted**, not theoretical overkill.

Example:
- target has 10 HP;
- attack resolves for 10,000 final damage;
- default lifesteal calculation sees 10 actual HP damage.

Effects may explicitly override these defaults.

## Reflection trigger defaults

Reflected/thorns damage does not trigger ordinary lifesteal, on-hit, reflection or attack-proc chains unless an effect explicitly opts in.

This prevents recursive/proc-loop nonsense while preserving room for intentionally stupid builds later.

## Aura stacking

Auras/effects with the same stacking identity do **not** normally stack by multiplying copies from several players.

Default:
- strongest eligible aura of the same identity applies.

Individual effects can define different rules if desired.

## Dispel strength

Every dispellable effect defines its own dispel interaction.

Framework levels:
- **Weak Dispel**
- **Strong Dispel**
- **Undispellable**

An effect can specify which dispel strength can remove it.

Do not hard-code broad "all poison/all magic" removal categories unless a particular skill deliberately references such a tag.

## Death prevention

Framework must support case-by-case death-prevention effects such as:
- cannot fall below 1 HP for X seconds;
- consume a buff/shield/resource instead of dying;
- self-revive;
- delayed death;
- revive with defined HP/resource;
- conditional death immunity.

These are reusable effect definitions rather than bespoke class code.

## Summon stat behavior

Summon framework must support both:
- **snapshot stats** when summoned;
- **dynamic inheritance** that updates from owner stats while active.

Each summon defines its own model.

Summons have independent threat entries unless explicitly designed otherwise.

## Combat-state membership

Primary combat-state rule:
- an entity is considered **in combat while it has active threat-list membership / is actively engaged or aggroed**.

Combat can propagate through support actions when those actions add threat or link the supporter to an active encounter.

Exact encounter cleanup/grace timing remains an implementation detail.

## Target dummies

Training/test dummies should exist as both dev tools and world objects.

Direction:
- early/start-town dummies have low/simple defenses;
- later towns/areas can offer tougher dummies with higher HP/Defense/resistances or specialized configurations.

Exact player-facing configuration UI is deferred.

## Combat-number presentation

Floating-number color indicates broad result/channel:
- Physical
- Magical
- Pure
- Healing

Elements are communicated primarily with **element icons**, not by assigning a unique floating-text color to every element.

This keeps Coffee/Donut/Math/etc. readable without creating an unusable rainbow.


### Percentage reduction and penetration basis

Percentage Defense reduction and percentage Defense penetration are both calculated from the target's **original positive Defense value for that resolution**, not from the already-reduced remainder.

Multiple percentage reductions are therefore additive against that original value.

Example:

```
Original Defense: 100
50% Defense reduction -> subtract 50
60% Defense reduction -> subtract another 60
Result after percentage reduction: -10 Defense
```

If the target's original Defense is already negative, percentage Defense reduction and percentage Defense penetration do not apply. Negative base Defense is considered an unusual/content-authoring edge case rather than an intended baseline state.

Percentage penetration uses the same original positive Defense basis.

Example:

```
Original Defense: 100
50% reduction -> -50
20% penetration -> -20
10 flat penetration -> -10
Final effective Defense = 20
```

Flat reduction and flat penetration may both push effective Defense below zero.

Within each layer, percentage effects resolve before flat effects, but percentage values always reference the original positive Defense rather than the intermediate Defense remainder.


### Penetration below zero

Defense penetration may push effective Defense below zero.

Example:

```
Target Defense: 5
Flat penetration: 10
Effective Defense: -5
```

Negative effective Defense then uses the normal negative-Defense vulnerability curve.

Penetration is therefore not clamped at zero.


### Defense reduction vs penetration order

Defense reduction resolves before penetration.

Default conceptual order:
1. establish the target's original Defense for this resolution;
2. subtract all percentage Defense reduction, each calculated from original positive Defense;
3. subtract flat Defense reduction;
4. subtract percentage Defense penetration, calculated from original positive Defense;
5. subtract flat Defense penetration;
6. evaluate the resulting positive/negative Defense through the normal mitigation/vulnerability curve.

Flat reduction and penetration may push Defense below zero.

Reduction changes the target state for relevant attackers; penetration remains attacker-specific.


### Internal Defense-reduction order

When an effect has both percentage and flat Physical/Magical Defense reduction, **percentage Defense reduction is applied before flat Defense reduction**.

Percentage reductions do not compound on the intermediate remainder; each references original positive Defense.

The complete default Defense ordering is therefore:

1. establish original Defense;
2. percentage Defense reduction(s), additive from original positive Defense;
3. flat Defense reduction;
4. percentage Defense penetration, from original positive Defense;
5. flat Defense penetration;
6. evaluate final positive/negative Defense through the mitigation/vulnerability curve.


### Underlying Defense reference for percentage reduction/penetration

The percentage-reference Defense used by percentage Defense reduction and percentage Defense penetration is the target's **underlying Defense before temporary positive Defense buffs**.

Example:

```
Underlying Defense: 100
Temporary +50 Defense buff -> current Defense 150
Enemy 20% Defense reduction -> subtract 20, not 30
```

Thus positive Defense buffs improve actual Defense without increasing the amount removed by enemy percentage reduction/penetration.

### Positive Defense buff ordering

When both percentage and flat positive Defense buffs apply, resolve:
1. percentage positive Defense buffs;
2. flat positive Defense buffs.

Example:

```
Underlying Defense: 100
+20% Defense -> 120
+50 flat Defense -> 170 current Defense
```

### Percentage values above 100%

Percentage Defense reduction and percentage Defense penetration are not capped at 100%.

They may exceed 100% and push effective Defense below zero.

Example:

```
Underlying Defense: 100
120% penetration -> subtract 120
Effective Defense before flat modifiers: -20
```


### Percentage Defense buff stacking

Multiple positive percentage Defense buffs stack **additively** against the underlying Defense value.

Example:

```
Underlying Defense: 100
+20% Defense
+30% Defense
= +50% total
= 150 Defense before flat positive Defense buffs
```

### Percentage Defense reduction stacking

Multiple percentage Defense reductions stack **additively** against the underlying positive Defense reference.

Example:

```
Underlying Defense: 100
50% reduction + 60% reduction = 110% total reduction
Subtract 110 Defense
```

Percentage Defense reduction is not capped at 100%.

### Percentage Defense penetration stacking

Multiple percentage Defense penetration sources on the attacker stack **additively** against the same underlying positive Defense reference.

Example:

```
20% penetration + 15% penetration = 35% total penetration
```

Percentage Defense penetration is not capped at 100%.


### Defense reference stability

Temporary negative Defense effects do not rewrite the underlying Defense reference used for percentage calculations.

They modify current/effective Defense only.

Example: if the current underlying reference is 100 Defense, applying a temporary reduction does not cause later percentage effects to calculate from the reduced remainder.

The exact composition of the underlying Defense reference remains intentionally open for later design review.

### Defense precision

Defense calculations retain fractional values internally.

Example:

```
137 underlying Defense
17% reduction = 23.29 Defense removed
```

Do not round intermediate Defense math for gameplay resolution.

UI may round or format Defense values for readability.


## Critical-hit authorization

Critical hits are **opt-in mechanics**, not a universal baseline stat.

Default rules:
- baseline crit chance is **0%**;
- there is **no universal default crit-damage multiplier**;
- an action/effect can crit only when an explicit source grants crit permission;
- the source granting crit permission also defines the relevant crit chance and crit multiplier/behavior;
- direct damage, periodic damage, healing and other effect categories do **not** gain crit permission merely because of their category;
- DoTs, healing or any other normally non-critting effect may crit when an explicit skill/passive/item/effect grants permission.

Examples of valid authored crit rules include:
- 50% chance to deal 2x damage;
- 12.5% chance to deal 8x damage.

There is currently no assumption that distinct crit-granting sources combine into one global Crit Chance/Crit Damage pair. Exact interaction between multiple simultaneous crit rules remains a separate design question.


### Multiple crit rules

An action may have multiple applicable explicit crit rules.

Default resolution:
1. collect all applicable crit rules;
2. sort them by **crit multiplier from highest to lowest**;
3. roll the highest-multiplier rule first;
4. if it succeeds, use that crit result and stop;
5. if it fails, roll the next rule;
6. continue until one succeeds or all fail.

Example:
- 12.5% chance for 8x;
- 50% chance for 2x.

The 8x rule rolls first. If it succeeds, the 2x rule is not rolled. If it fails, the 2x rule may then roll.

Crit rules are therefore not merged into one combined global crit chance.

Rare authored exceptions may explicitly state that they stack with or multiply another crit result. Such behavior is not default and must be stated by the source.

### Crit-rule modifiers

A gameplay source may explicitly modify another eligible crit rule instead of creating a separate roll.

Examples:
- add percentage points to an existing crit rule's chance;
- modify that rule's multiplier;
- permit a normally ineligible effect category to use a specific crit rule.

These interactions are opt-in and source-specific. No generic crit modifier is assumed unless the source explicitly defines what it modifies.


### Crit modifier ordering and chance cap

Explicit crit-rule modifiers resolve **before** crit rules are ordered for rolling.

After all applicable explicit modifiers are applied:
1. determine each rule's final crit chance;
2. determine each rule's final crit multiplier/behavior;
3. sort ordinary crit rules by final multiplier, highest first;
4. roll in that order until the first success.

Example:
- Rule A: 12.5% at 8x;
- Rule B: 50% at 2x;
- explicit modifier changes Rule B to 10x;
- Rule B now rolls before Rule A.

Ordinary crit chance is clamped to **0-100%** after modifiers.

Chance above 100% has no implicit extra meaning. Any overflow conversion, bonus roll, super-crit behavior or other >100% mechanic must be explicitly authored.

### Crit placement in damage resolution

By default, a successful crit modifies the **raw damage packet before Defense and elemental resistance**.

Example:

```
100 raw damage
2x crit
= 200 raw critical damage
then Defense mitigation
then elemental resistance
```

Rare mechanics may explicitly override this placement, but such behavior must be stated by the source.


### Equal-multiplier crit ordering

Crit rules are primarily ordered by final modified multiplier, highest first.

If multiple ordinary crit rules have the same final multiplier:
1. an explicitly authored crit-rule priority may decide the order;
2. otherwise use a **stable deterministic source-ID order**.

This tie-break does not change the damage multiplier. It exists so provenance/on-crit ownership is deterministic when equal-multiplier rules compete for the first-success result.

### Multi-packet actions

By default, crit resolution is **action-wide**, not packet-by-packet.

For an action containing multiple raw damage packets, resolve the action's applicable crit rule once. If it crits, apply that crit behavior to all eligible raw packets before their separate Defense/resistance resolution.

A mechanic may explicitly opt into per-packet crit resolution instead.

### Critical-hit event scope

By default, a critical action emits **one critical-hit event per action**, not one event per damage packet.

Explicit per-packet crit mechanics may define per-packet critical events when authored to do so.


### Crit modifier restraint and explicit math

Crit-rule modifiers are supported by the design, but they should be used sparingly.

When a source modifies crit chance or crit multiplier, the operation must be explicit rather than relying on ambiguous wording.

Examples of distinct chance operations:
- **+10 percentage points**: 25% -> 35%;
- **+20% of existing chance**: 25% -> 30%.

Examples of distinct multiplier operations:
- **+0.5x multiplier**: 2x -> 2.5x;
- **+50% of existing multiplier**: 2x -> 3x.

There is no assumption that DiceFree needs to use all of these modifier forms in ordinary content. Prefer simple authored crit rules unless a modifier adds meaningful design value.

### Crit provenance

A critical action preserves both:
- the **action/source context** that produced the resolved effect;
- the **winning crit rule/source** that caused the critical result.

Example:
- action: Fireball;
- winning crit source: Ridiculous Hat of Explosions.

These identities remain separate so future systems can distinguish "when this action crits" from "when this item/passive causes a crit".


### Hit resolution before crit

If an action can miss, hit/miss resolution occurs **before** crit resolution.

A missed action:
- does not roll crit rules;
- does not consume crit RNG;
- cannot produce a critical-hit result unless an explicit mechanic says otherwise.

### Critical action vs critical damage dealt

A successful crit roll and actual HP damage are separate facts.

An action may count as having critically hit even if the resulting damage is fully prevented by:
- immunity;
- shields/absorbs;
- another prevention layer.

For trigger purposes, distinguish at least:
- **critical action / critical hit occurred**;
- **critical HP damage dealt**.

Mechanics must state which fact they care about.

### Crit inheritance

Secondary effects/procs do **not** inherit the parent action's critical result by default.

A proc, explosion, DoT application, secondary strike or other child effect resolves its own crit eligibility/rules.

A source may explicitly state that a child effect inherits or copies the parent's crit result; this is an exception, not the default.


### Elemental penetration and healing

Elemental penetration may apply to elemental healing when the authored effect says it does.

For elemental healing, matching elemental penetration acts in the **opposite direction** from damage penetration: it increases the effective matching resistance used by the heal.

Example:

```
Base Fire heal: 100
Target Fire resistance: +50%
Healer Fire penetration affecting healing: +10 percentage points
Effective healing resistance: +60%
Final heal: 160
```

This is contextual behavior, not an excuse for ambiguous wording. An authored effect must make clear whether its penetration affects:
- damage;
- healing;
- both;
- or another explicitly defined context.

A real resistance debuff changes the target's resistance itself and therefore affects both matching elemental damage and matching elemental healing according to their normal formulas.

Negative elemental resistance remains uncapped. Matching elemental healing uses:

```
Healing = Base Healing × (1 + effective matching resistance)
```

Therefore:
- -50% resistance => 50% of base healing;
- -100% resistance => 0 healing;
- below -100% resistance => the result becomes damage.

Example:

```
100 Fire heal against -150% effective Fire resistance
= -50
= 50 Fire-context healing-inversion damage
```


### Resistance overcap and modifier ordering

Elemental resistance keeps its **raw uncapped value** internally.

The normal resistance cap limits the **effective value used for resolution**, not the stored/raw resistance itself.

Example:

```
Raw Fire resistance: 110%
Normal cap: 75%
Effective Fire resistance: 75%
```

If a -20 percentage-point Fire resistance debuff is applied:

```
110% raw - 20% = 90% raw
Effective after 75% cap = 75%
```

Therefore overcapping resistance is meaningful: debuffs and penetration must first remove the excess before effective resistance drops below the cap.

Default resistance resolution:
1. establish raw resistance;
2. apply ordinary resistance bonuses/debuffs additively;
3. apply applicable resistance penetration/reduction to the uncapped raw value;
4. apply the current resistance cap to obtain effective positive resistance;
5. use that effective resistance in damage/healing resolution.

Negative elemental resistance remains uncapped.

### Resistance modifier stacking

Ordinary elemental resistance bonuses and debuffs stack **additively** by default.

Example:

```
+30% Fire resistance
+20% Fire resistance
-15% Fire resistance
= +35% raw Fire resistance
```

Special multiplicative or otherwise unusual stacking requires an explicitly authored mechanic.


### Elemental resistance percentage semantics

Elemental resistance and elemental resistance penetration are percentage-based mechanics.

All authored/player-facing values must therefore be written with an explicit `%` sign.

Examples:
- `+30% Fire Resistance`;
- `-20% Fire Resistance`;
- `15% Fire Resistance Penetration`.

For elemental resistance penetration, the numeric value represents **percentage points removed from matching resistance**, not a multiplicative percentage of the current resistance.

Example:

```
Target Fire Resistance: 50%
Attacker Fire Resistance Penetration: 20%

50% - 20 percentage points = 30%
```

Do not author ambiguous text such as bare "20 Fire penetration" when the mechanic means 20%.

Multiple matching penetration sources stack additively by default.

Example:

```
15% + 10% + 5% Fire Resistance Penetration
= 30% total Fire Resistance Penetration
```

### Resistance-cap increases and resistance above 100%

The normal positive resistance cap is 75%, but explicit mechanics may raise the cap.

Cap increases are percentage-point increases.

Example:

```
Normal Fire Resistance cap: 75%
+10% Fire Resistance Cap
= 85% Fire Resistance cap
```

The framework must permit an explicitly raised elemental resistance cap to exceed 100%, even though normal content should not generally be designed around reaching that state.

When effective matching elemental resistance exceeds 100%, matching elemental damage changes sign and becomes healing.

Default elemental damage resistance factor:

```
ResultMultiplier = 1 - EffectiveResistance
```

Examples:

```
75% resistance  -> 25% of otherwise-resolved matching damage
100% resistance -> 0
105% resistance -> -5%, therefore healing equal to 5% of the matching damage that would have resolved at 0% resistance
```

This is an emergent/highly unusual interaction, not a baseline progression target.

Negative resistance remains uncapped in the opposite direction.


### Damage-inversion healing semantics

When effective elemental resistance above 100% inverts matching elemental damage into HP restoration, that restoration is a distinct **resistance-inversion** result.

Default rules:
- it is **not** multiplied by Healing Received;
- it still counts as the entity being hit by the originating elemental action;
- it does **not** count as taking damage;
- it restores HP;
- it is not automatically treated as an ordinary heal for generic "received a heal" triggers unless an effect explicitly includes resistance-inversion healing;
- ordinary damage shields do not consume capacity because the post-resistance result is not positive damage.

Example:

```
Damage entering resistance step: 100 Fire
Effective Fire resistance: 105%
Result: -5 damage
=> restore 5 HP
```

The 5 HP restoration is not modified by Healing Received.

This preserves the originating action/element context while keeping ordinary damage, healing and resistance-inversion restoration distinct for future trigger logic.


### Mixed elemental packet outcomes

Damage packets resolve independently.

A single action may therefore produce both HP damage and resistance-inversion HP restoration when different packets interact differently with the target's defenses/resistances.

Example:

```
Packet A: Fire, effective Fire resistance 105% -> HP restoration
Packet B: Ice, effective Ice resistance 20% -> HP damage
```

Both outcomes remain part of the same action, but they are recorded separately.

Do not collapse mixed packet outcomes into one net "damage" value for trigger/telemetry purposes.

Track at least:
- actual HP damage dealt;
- actual HP restored by resistance inversion;
- net HP delta may be derived separately for presentation if useful.

If any packet deals actual HP damage, the target counts as having taken damage from the action even if another packet restores HP.

### Resistance-inversion overheal

Resistance-inversion HP restoration is capped by missing HP.

Example:

```
Missing HP: 3
Resistance-inversion restoration: 20
Actual HP restored: 3
Excess: 17 lost
```

Excess resistance-inversion restoration is discarded by default unless an explicit future mechanic converts or uses it.
