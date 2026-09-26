# Class Progression

## Core concept

Every character begins as a **Novice**.

Class advancement is permanent for that character. Multiple character slots represent alternate manifestations/timelines of the same Echo.

## Advancement cadence

```
Novice        -> reach level 10  -> advance -> new class starts at level 1
Tier I        -> reach level 30  -> advance -> new class starts at level 1
Tier II       -> reach level 60  -> advance -> new class starts at level 1
Tier III      -> reach level 120 -> advance -> new class starts at level 1
Tier IV       -> reach level 200 -> advance -> new class starts at level 1
Tier V
```

A character may continue gaining levels after reaching the required advancement level.

When they advance:
- the new class starts at level 1;
- excess XP does **not** carry over;
- the new class uses its own base stats/growth.

Overleveling provides no inherent permanent reward. Its main practical benefit is being stronger while attempting that class's advancement quest.

Extreme overleveling remains possible and can itself become secret progression. A **level-200 Novice** is an explicit example of a future secret-class unlock condition.

## Class stats

Each class/advancement defines its own:
- level-1 base stats;
- per-level stat growth;
- base HP;
- movement speed where relevant;
- primary stat for basic-attack scaling;
- combat resources/mechanics.

Later classes generally begin with higher level-1 bases.

Players do not manually allocate stat points.

### Vitality and HP

Vitality contributes to maximum HP:

```
Max HP = class base HP + (Vitality × coefficient) + modifiers
```

Whether that coefficient is universal or defined per class remains open.

## Opening branches

First advancement:

- **Physically Blessed Novice**
- **Magically Touched Novice**

These are evolutions of the Novice rather than fully specialized fantasy jobs.

Possible next split remains physical melee/ranged and magical melee/ranged, but names/fantasies are not finalized.

## Equipment and attack identity

Each class defines:
- allowed equipment families;
- usable weapon categories;
- melee/ranged basic-attack behaviour;
- primary stat;
- attack speed/range where applicable.

Weapon categories themselves will be designed around actual class needs rather than locking a giant universal list up front.

## Tree shape

Keep the tree reasonably even where practical, but do not force strict binary symmetry.

Some classes can:
- have unusual branch counts;
- branch into secret paths;
- retain the same identity across tiers;
- become an advanced version of themselves.

## Secret classes

Secret classes can branch from many points and may use unusual requirements such as achievements, items, encounters, account discoveries or extreme overleveling.

Once entered, secret classes still use the shared advancement threshold structure.

## Advancement philosophy

Advancement may:
- add abilities;
- evolve abilities;
- replace abilities;
- change passives;
- replace resources;
- change stats/growth;
- alter equipment permissions;
- change visuals/model/VFX;
- preserve or rename identity.

Advancement quests are designed individually by class and may deliberately send characters into much higher-level regions.

## Resources

Different classes can use different combat resources. Advancement may keep, modify or replace them.

## Data model requirement

Class definitions should be data-driven and include:
- ID/display name;
- parent path;
- requirements;
- base stats;
- growth;
- base HP;
- primary stat;
- movement speed;
- basic-attack definition;
- resource definition;
- passives;
- active abilities;
- ability evolution/replacement;
- equipment permissions;
- visual package;
- next advancements;
- secret unlock requirements.

## Unresolved

- Final primary-stat list.
- Universal vs class-specific Vitality coefficient.
- Exact second-tier branches.
- Weapon families as concrete classes require them.
- Number of character slots.
