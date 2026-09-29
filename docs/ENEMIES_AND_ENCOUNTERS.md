# Enemies, AI and Encounters

## Philosophy

Enemy and encounter authoring should be data-driven enough that simple enemies are trivial to build while unusually complex bosses can still express deep conditional behavior.

Do not force every enemy into one giant behavior tree, and do not require a bespoke MonoBehaviour/class for every named boss.

The framework should support:
- extremely simple authored behavior;
- reusable archetypes and modifiers;
- composable conditional logic;
- custom-code escape hatches for genuinely exceptional mechanics.

## Enemy definitions and archetypes

Enemy content can be built from a **base archetype** plus authored variant data.

Examples:
- Slime base archetype;
- Crop Slime;
- Road Slime;
- Fire Slime;
- named/elite Slime;
- Slime boss.

A base archetype may provide:
- locomotion/navigation assumptions;
- animation/presentation family;
- collision/body defaults;
- basic AI package;
- common tags;
- common combat actions;
- default leash/home rules.

Variants can override or extend:
- level;
- stats;
- abilities;
- elements/resistances;
- AI logic;
- threat/targeting behavior;
- loot/XP;
- presentation;
- encounter membership;
- special rules.

Inheritance/composition should remain shallow/readable. Do not build an opaque inheritance tower where understanding Road Slime requires opening nine parent assets.

## Fixed authored levels

Enemies have authored/fixed levels by default.

There is **no universal player-level scaling**.

Content identity remains fixed:
- a level-5 enemy remains level 5 when a level-100 character returns;
- old enemies can be overpowered later;
- individual content may explicitly opt into special scaling/sync mechanics.

## Enemy stats

Support both:

### Primary-attribute model
An enemy may use VIT/STR/AGI/INT/SPI and derived stat rules where that improves consistency or enables interactions.

### Direct monster-stat model
An enemy may directly author combat-relevant values such as:
- Max HP;
- Physical Defense;
- Magical Defense;
- movement speed;
- attack timings;
- damage;
- resistances;
- regeneration;
- other monster-specific values.

A Slime or environmental creature should not need fake Spirit/Intelligence values unless a mechanic actually needs them.

Both models should resolve into the same combat/stat pipeline.

## Semantic AI / role tags

Enemies can carry semantic tags such as:
- Melee;
- Ranged;
- Caster;
- Healer;
- Tank;
- Summoner;
- Ambusher;
- Coward;
- Patrol;
- Elite;
- Boss;
- other authored tags.

Tags are metadata for queries, abilities, loot, encounter composition and AI helpers.

They do not hard-code one mandatory behavior.

## AI decision model

Enemy decision logic should scale from trivial to complex.

### Simple behavior
Very simple enemies may use rules such as:
- attack nearest hostile;
- attack highest-threat target;
- approach and basic attack;
- flee below X% HP.

### Conditional / authored behavior
More complex enemies can choose actions using weighted and conditional rules.

Predicates may include:
- own HP/resource/effect state;
- target HP/resource/effect state;
- distance/range;
- number/type of nearby allies/enemies;
- threat ranking;
- ability cooldown/charges;
- phase;
- time in combat;
- summons/adds alive;
- world/arena state;
- previous action;
- difficulty mode;
- arbitrary AND / OR / NOT composition.

Examples:
- heal an ally below 40% HP;
- prefer Fireball if target is farther than 6m;
- do not summon if 3 adds are already alive;
- use escape ability if surrounded;
- attack closest enemy unless a healer has generated enough threat;
- execute a long authored priority/condition chain for a boss.

Support weighted choice among multiple currently-valid actions.

Custom code remains possible for mechanics that cannot sensibly fit the generic decision model.

## Threat and targeting

Normal threat-aware enemies may default to targeting highest threat.

Per-enemy targeting can override or modify that with rules such as:
- nearest hostile;
- farthest hostile;
- lowest HP;
- random eligible target;
- healer/support target;
- non-tank preference;
- fixation/forced target;
- target with specific effect/tag;
- scripted encounter target.

Targeting rules can use AND/OR conditions and may change by phase.

Threat remains tracked even when an enemy temporarily uses a non-threat target unless the mechanic explicitly says otherwise.

## Home, leash and reset behavior

Overworld enemies can define:
- home position/area;
- awareness/aggression range;
- chase rules;
- leash distance/shape;
- return-home behavior;
- HP/state restoration;
- respawn policy.

These are authored per enemy/archetype.

Some overworld enemies may deliberately violate ordinary leash rules:
- never leash;
- leash only after crossing a region;
- patrol-specific return;
- chase until an event ends;
- retreat rather than reset;
- other authored behavior.

Dungeon and boss encounters can use encounter-defined reset zones/rules instead of ordinary overworld leash behavior.

## Enemy packs and patrol groups

Enemy groups can be authored as reusable **pack definitions**.

A pack can define:
- member composition;
- spawn positions/formation;
- patrol route;
- shared/linked aggro;
- pull relationships;
- reset-together behavior;
- leader/follower roles;
- replacement/respawn behavior;
- encounter tags.

Example:
- 2 melee enemies + 1 healer;
- healer stays behind while melee engage;
- pulling one alerts the whole pack;
- pack resets together.

Pack logic should not require duplicating the same setup across every placement.

## Reusable enemy modifiers

Support reusable authored modifiers/affixes for enemies.

Examples:
- Elite;
- Enraged;
- Regenerating;
- Volatile;
- Armored;
- Hasted;
- Element-infused;
- other authored modifiers.

A modifier can add or alter:
- stats;
- abilities;
- effects;
- tags;
- AI rules;
- presentation;
- rewards.

This is an authoring framework, not a commitment to random Diablo-style elite affixes everywhere.

Content decides where modifiers are used and whether they are fixed or randomized.

## Encounter definitions

Complex fights should use an **Encounter definition** separate from individual enemy actors.

An encounter can own:
- participating enemies/bosses;
- arena boundaries;
- doors/barriers;
- hazards;
- interactable objects;
- add waves;
- timers;
- enrage state;
- dialogue/cues;
- phase state;
- encounter-wide variables;
- completion/failure predicates;
- reset behavior;
- difficulty overrides;
- semantic events.

Do not jam encounter-wide orchestration into the boss actor if it belongs to the arena/fight as a whole.

## Boss phases / encounter state machine

Bosses and encounters support authored phases/states.

Transitions can be driven by predicates such as:
- boss HP threshold;
- elapsed time;
- add deaths;
- object interaction;
- ability count;
- player state;
- arena state;
- another boss's state;
- difficulty mode;
- prior phase outcomes;
- arbitrary nested AND / OR / NOT conditions.

Simple boss:
- phase 1 -> below 50% HP -> phase 2.

Complex boss:
- may have a large authored condition graph/priority set.

The framework must support both without forcing simple encounters to use unnecessary complexity.

## Difficulty modes

Difficulty modes can patch/override encounter definitions.

Harder modes may:
- add mechanics;
- alter phase transitions;
- add enemies/add waves;
- modify AI priorities;
- modify stats;
- modify timers/enrage;
- alter hazards;
- change loot/rewards;
- change completion conditions.

Difficulty must not be limited to an HP/damage multiplier.

Overrides should remain inspectable so designers can understand what a mode changes.

## Encounter reset

Default boss/encounter reset restores authored initial encounter state, including:
- boss/enemy HP and state;
- phases;
- adds/summons owned by the encounter;
- hazards;
- arena objects/doors;
- timers/enrage;
- encounter-local variables;
- temporary encounter effects/debuffs where appropriate.

Persistent player-owned state that should survive attempts remains.

Examples:
- Resurrection Sickness remains unless its own rules remove it;
- permanent quest/progression state does not roll back;
- consumables already spent are not automatically restored unless explicitly designed.

Encounters may define explicit exceptions where some arena/boss state intentionally persists across attempts.

## Debug / encounter test harness

Every substantial enemy/encounter should be testable outside the full progression path.

Provide developer tooling to:
- spawn enemy/pack/encounter by stable ID;
- choose difficulty;
- set player count 1-4 or simulated participant parameters;
- set boss HP/resource;
- jump/force phase;
- force an enemy ability;
- inspect AI decision scores/valid predicates;
- inspect threat tables/target choice;
- inspect encounter variables;
- reset encounter;
- toggle invulnerability/time scale where useful;
- capture combat logs/events.

The harness is development-only.

Goal: iterating on boss #40 should not require walking through a 50-minute dungeon for every test.
