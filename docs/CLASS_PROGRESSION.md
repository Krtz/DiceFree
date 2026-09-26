# Class Progression

## Core concept

Every character begins as a **Novice**.

Class advancement is permanent for that character and is intended to create a lineage rather than a reversible loadout choice. Players have multiple character slots representing alternate manifestations/timelines of the same Echo, while some account systems such as the bank are shared.

## Advancement cadence

Current advancement targets:

```
Novice        -> reach level 10  -> advance -> new class starts at level 1
Tier I        -> reach level 30  -> advance -> new class starts at level 1
Tier II       -> reach level 60  -> advance -> new class starts at level 1
Tier III      -> reach level 120 -> advance -> new class starts at level 1
Tier IV       -> reach level 200 -> advance -> new class starts at level 1
Tier V
```

A character may continue gaining levels after reaching the level required for their next advancement if they delay the advancement quest.

When they finally advance:
- the new class starts at level 1;
- excess experience does **not** carry over;
- the new class uses its own base stats and level-growth rules.

Overleveling is not intended to produce special long-term power by itself. Its practical value is mostly making the current class stronger before tackling its advancement quest.

There is no normal hard cap at the advancement threshold. Extreme overleveling is allowed. For example, a **level 200 Novice** should be possible and is explicitly a good kind of condition for a future secret-class unlock.

All normal and secret classes use the same advancement-level cadence.

## Class stats

Each class/advancement has its own:
- level-1 base stats;
- per-level stat growth;
- movement speed where relevant;
- resources/mechanics.

Later classes generally begin with a higher level-1 stat baseline appropriate to their tier.

Stats are therefore not simply accumulated forever from every earlier class. The new class defines the character's new base package, while equipment adds on top of it.

Players do **not** manually allocate stat points.

## Opening branches

The current first advancement identities are:

- **Physically Blessed Novice**
- **Magically Touched Novice**

These are both evolutions of the Novice rather than fully specialized fantasy jobs.

They create the broad Physical/Magic split while preserving the idea that the Echo is still only beginning to acquire a true shape.

A possible next split remains:

```
Physically Blessed Novice -> physical melee / physical ranged
Magically Touched Novice  -> magical melee / magical ranged
```

The actual second-tier class names and identities are not finalized.

## Tree shape

The class tree should remain as balanced/even as practical, but it is not required to be perfectly binary.

Some classes may:
- have more or fewer descendants;
- branch into secret paths;
- retain the same identity/name family across advancements;
- become an advanced version of themselves instead of receiving a completely new fantasy/name.

## Secret classes

Secret classes can branch from different points in the tree.

They may require hidden achievements, encounters, items, quests, account discoveries or unusual progression conditions such as intentionally refusing advancement and reaching an absurd level.

Once entered, secret classes still follow the same advancement-level thresholds as ordinary classes.

## Advancement philosophy

An advancement should feel like a transformation.

Depending on the class, an advancement may:
- add new active abilities;
- evolve existing abilities;
- replace old abilities;
- add or replace passives;
- replace the class resource entirely;
- change stat baselines and growth;
- unlock/restrict equipment categories;
- change the character model, silhouette or VFX;
- preserve the class identity while advancing it.

There is deliberately **no global rule that every ability must survive or every class must gain exactly the same number of abilities**.

Advancement quests are designed on a class-by-class basis. They do not need to occur in the character's current intended-level region. A stealth class could, for example, receive a level-30 advancement trial that requires sneaking into the sixth face's capital while being far too weak to fight the enemies surrounding it.

## Resources

Different classes can use different combat resources.

An advancement may:
- keep the previous resource;
- modify it;
- replace it entirely;
- introduce a new secondary resource.

There is no universal mana requirement across the class tree.

## Permanence and alternate manifestations

A chosen advancement branch is permanent on that character slot.

The game supports multiple character slots representing alternate manifestations/timelines of the same underlying Echo.

Exact character-slot count is not yet decided.

## Data model requirement

Class definitions should be data-driven and refer to:
- ID and display name;
- parent class/path;
- advancement requirements;
- level-1 base stats;
- automatic stat growth;
- movement speed;
- resource type/mechanics;
- passive(s);
- active abilities;
- ability evolution/replacement rules;
- equipment permissions/restrictions;
- visual package;
- next advancements;
- secret-unlock requirements where applicable.

Avoid scattering class-specific conditionals across unrelated systems.

## Unresolved

- Exact second-tier melee/ranged branch names and fantasies.
- Exact stat list and formulas.
- Number of character slots.
- Whether any extremely rare system can ever undo a class choice.
