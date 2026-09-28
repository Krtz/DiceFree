# Class Progression

## Core concept

Every new Echo lineage begins as a **Novice**.

See `docs/classes/NOVICE.md` for the detailed starting class.

Advancement branches create alternate manifestations rather than deleting the parent manifestation.

The original manifestation remains playable, while the chosen advancement creates a new character slot/timeline branch at level 1.

## Advancement cadence

```
Novice        -> reach level 10  -> branch -> new Tier I manifestation starts at level 1
Tier I        -> reach level 30  -> advance -> new class starts at level 1
Tier II       -> reach level 60  -> advance -> new class starts at level 1
Tier III      -> reach level 120 -> advance -> new class starts at level 1
Tier IV       -> reach level 200 -> advance -> new class starts at level 1
Tier V
```

A character may continue leveling beyond an advancement threshold.

When they eventually advance:
- the new class begins at level 1;
- excess XP does not carry over;
- the new class uses its own base stats and growth.

Overleveling gives no automatic permanent bonus, but can make advancement quests easier and may satisfy hidden conditions.

A level-200 Novice is intentionally possible.

It is also intended to receive a hidden stronger Novice payoff (secret skill and potentially first passive) rather than remaining permanently identical to the level-10 kit.

## Branching manifestation model

Advancement is a **branch**, not an overwrite.

When a manifestation reaches an advancement threshold and chooses a class:
- the current manifestation is preserved as a playable parent slot at the fork point;
- a new manifestation/character slot is created for the chosen advancement;
- the game immediately continues as that new advanced manifestation in the same session/world state;
- the new advanced manifestation begins at level 1 with its own class base stats/growth;
- **all manifestation-specific state** is snapshotted into the child at the fork, including gear, carried inventory/currency, quest/world progression and relevant class/skill state;
- the parent keeps its original state and the child receives a duplicate snapshot;
- the preserved parent resumes immediately before the blessing choice if loaded later;
- Echo-wide state remains shared;
- the parent manifestation can later choose another available branch, creating another child manifestation.

Example after the first split:
- Level 10 Novice remains playable;
- Level 1 Physically Blessed Novice exists as another slot;
- Level 1 Magically Touched Novice can later be created from the same Novice.

This branching-slot model is part of the Echo/timeline fantasy.

Open:
- exact slot limits;
- UI presentation of parent/child lineage.

## Core attributes

Current attribute set:
- Vitality
- Strength
- Agility
- Intelligence
- Spirit

Every class defines:
- level-1 base attributes;
- per-level attribute growth;
- starting Physical Defense;
- starting Magical Defense;
- one or more primary attack attributes;
- movement speed;
- base HP;
- resource(s);
- other class-specific base values.

Later advancements generally start with higher base stats.

Players do not manually allocate stat points.

### Primary attributes

A class may use one or multiple primary attributes.

There is **no universal multi-primary formula**.

Each class defines its own attack scaling according to identity and balance.

Examples could include:
- equal STR + INT weighting;
- mostly STR with some INT;
- AGI + SPI;
- all five attributes for an exceptional secret class.

The exact coefficients are class data.

### Vitality

Vitality contributes to HP:

```
Max HP = class base HP + (Vitality × coefficient) + modifiers
```

The coefficient may be universal or class-dependent; unresolved.

### Secondary effects

Current direction:
- Vitality -> HP and HP regeneration;
- Strength -> modest Physical Defense;
- Agility -> very small Attack Speed and Movement Speed;
- Intelligence -> modest Magical Defense plus class-specific resource value;
- Spirit -> healing done and healing received with separate coefficients.

Class base stats, equipment, abilities and passives remain the main source of large combat differences.

See `docs/STATS_AND_DAMAGE.md` for the detailed model.

### Skill scaling

Skill scaling is ability-specific.

Abilities can use:
- weighted combinations of stats;
- highest of selected stats;
- highest stat overall;
- total stats;
- class-specific values.

The class's primary attribute(s) do not force every ability to use the same formula.

## Opening branches

First advancement:
- **Physically Blessed Novice**
- **Magically Touched Novice**

Working Tier-1 class sheets:
- `docs/classes/PHYSICALLY_BLESSED_NOVICE.md`
- `docs/classes/MAGICALLY_TOUCHED_NOVICE.md`

Current strong direction:
- both Tier-1 branches introduce **Mana** as a simple shared resource;
- specialized resources arrive on later classes where they reinforce identity;
- Physically Blessed remains a broad STR/AGI melee generalist;
- Magically Touched remains a broad INT/SPI magical generalist.

Current candidate level-30 branching:
- Physical: sword-and-board, rogue, ranger, two-handed melee DPS;
- Magical: shield/buff healer-support, healing-focused healer, arcane wizard, occult caster.

This four-plus-four split is still a working direction rather than locked final structure.

## Class identity

Each class defines:
- allowed equipment families;
- weapon categories;
- melee/ranged basic attack;
- attack range/speed;
- primary attribute(s);
- primary-stat coefficients;
- resource;
- fixed ability kit;
- passive mechanics;
- advancement quest;
- visual identity.

Weapon categories are created according to actual class needs rather than predefining every possible weapon.

## Fixed kits and advancement

Classes use fixed kits.

Advancement may:
- add abilities;
- evolve abilities;
- replace abilities;
- change passives;
- replace resources;
- change stats;
- alter equipment permissions;
- change visual form.

## Secret classes

Secret classes may branch from many points and use unusual conditions:
- achievements;
- items;
- exploration;
- encounters;
- account discoveries;
- extreme overleveling;
- strange class-specific feats.

Secret classes use the same advancement thresholds after entering their lineage.

## Healer viability

Healer/support lineages must be fully viable for solo progression.

They should have:
- a real damage rotation;
- sufficient solo damage to quest/farm/grind;
- healing/support tools that become especially valuable in groups;
- class-specific ways to connect offense and sustain where appropriate.

## Open questions

- exact slot limits;
- exact secondary effect of each attribute;
- universal vs class-specific Vitality coefficient;
- exact second-tier classes;
- weapon categories as classes require them;
- number of character slots.
