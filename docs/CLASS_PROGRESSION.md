# Class Progression

## Core concept

Every new character begins as a **Novice**.

Around level 10, the Novice makes a first major advancement into a small set of base classes. The current working model is four.

Each later tier branches again.

```
Novice
  ↓
4 classes
  ↓
8 classes
  ↓
16 classes
  ↓
32 classes
  ↓
64 classes
  ↓
future tiers if the game ever deserves them
```

This is a long-term architecture target, not a promise to implement 124+ advanced classes before release.

## Advancement philosophy

An advancement should feel like becoming a new class.

It may change:
- class name;
- visual identity;
- stat growth;
- resource system;
- passive;
- active abilities;
- ability upgrades/replacements;
- equipment preferences;
- animation/VFX package;
- available future branches.

Advancement should usually involve gameplay: a quest, trial, boss, discovery or condition rather than simply clicking a button in a menu.

## Visibility

Normal advancement paths may be visible.

Secret classes should often be hidden until discovered or hinted at through achievements, NPCs, items, world events or unusual requirements.

## First implementation target

A sensible first playable class set is:

- Novice
- 4 Tier-I classes
- 8 Tier-II classes

= 13 total class definitions.

Later tiers expand without changing the underlying class architecture.

## Data model requirement

Class definitions should be data-driven and refer to:
- ID and display name;
- parent class;
- advancement requirements;
- stat growth;
- resource type;
- passive(s);
- abilities;
- allowed or preferred equipment;
- visual package;
- next advancements.

Avoid scattering class-specific conditionals across unrelated systems.

## Unresolved

- Exact four first classes.
- Advancement level cadence after level 10.
- Whether old skills evolve, remain, or are selectively replaced.
- Whether respec/class reversal exists.
- Character level cap versus class tier cap.
- How secret classes interact with the normal binary tree.
