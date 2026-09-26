# Class Progression

## Core concept

Every character begins as a **Novice**.

Class advancement is permanent for that character and is intended to create a lineage rather than a reversible loadout choice. Players will have multiple character slots, while some account systems such as the bank are shared.

## Advancement cadence

Current advancement targets:

```
Novice        -> reach level 10  -> advance -> level resets
Tier I        -> reach level 30  -> advance -> level resets
Tier II       -> reach level 60  -> advance -> level resets
Tier III      -> reach level 120 -> advance -> level resets
Tier IV       -> reach level 200 -> advance -> level resets
Tier V
```

The current-class level resets when an advancement is completed. The exact handling of persistent stats/experience totals around that reset still needs implementation design.

All normal and secret classes use the same advancement-level cadence.

## Tree shape

The earlier 4 -> 8 -> 16 -> 32 -> 64 model is no longer a strict target.

The current leading idea is a broader first distinction:

```
                 Novice
                /      \
          Physical      Magic
```

A possible next split is:

```
Physical -> melee / ranged
Magic    -> melee / ranged
```

This is a **working design direction**, not yet a finalized class list or naming scheme.

After that, branches can become increasingly specialized.

The tree should remain as balanced/even as practical, but it is not required to be perfectly binary. Some classes may:
- have more or fewer descendants;
- branch into secret paths;
- retain the same identity/name family across advancements;
- become an advanced version of themselves instead of receiving a completely new fantasy/name.

## Secret classes

Secret classes can branch from different points in the tree.

They may require hidden achievements, encounters, items, quests, account discoveries or other unusual conditions, but once entered they still follow the same advancement level thresholds as ordinary classes.

## Advancement philosophy

An advancement should feel like a transformation.

Depending on the class, an advancement may:
- add new active abilities;
- evolve existing abilities;
- replace old abilities;
- add or replace passives;
- change resource mechanics;
- alter stat growth;
- unlock/restrict equipment categories;
- change the character model, silhouette or VFX;
- preserve the class identity while advancing it.

There is deliberately **no global rule that every ability must survive or every class must gain exactly the same number of abilities**.

Advancement should usually involve gameplay: a quest, trial, boss, discovery or condition rather than simply clicking a menu button.

## Stats on level-up

Classes have different automatic stat gains when they level.

Players do **not** manually allocate stat points.

Equipment also contributes stats.

The detailed stat list and per-class growth model remain to be designed.

## Permanence and alts

A chosen advancement branch is permanent on that character.

The game should therefore support:
- multiple character slots;
- meaningful replay through alternate lineages;
- a shared bank to reduce pointless item shuffling between characters.

Exact character-slot count is not yet decided.

## Data model requirement

Class definitions should be data-driven and refer to:
- ID and display name;
- parent class/path;
- advancement requirements;
- automatic stat growth;
- resource type;
- passive(s);
- active abilities;
- ability evolution/replacement rules;
- equipment permissions/restrictions;
- visual package;
- next advancements;
- secret-unlock requirements where applicable.

Avoid scattering class-specific conditionals across unrelated systems.

## Unresolved

- Final names and identities for the first Physical/Magic split.
- Whether the second split is actually melee/ranged on both sides.
- Exact persistent-stat behaviour when class level resets.
- Number of character slots.
- Whether any extremely rare system can ever undo a class choice.
