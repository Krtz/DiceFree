# Technical Direction

This document records direction, not a frozen implementation contract.

## Engine

The initialized project uses **Unity 6000.6.3f1**, 3D URP, PC-first.

## Architecture principles

### Data-driven content
Classes, items, enemies, loot tables, regions, advancement requirements, dungeon rewards and crafting recipes should primarily be data definitions rather than hard-coded chains.

### Modular systems
Keep combat, stats, classes, items, world, quests, dungeons, UI, persistence and networking separated enough to evolve independently.

### Large class-tree architecture

DiceFree is expected to grow into a **very large, unevenly branching class tree**.

Architecture requirements:
- no assumption that every class has the same number of children;
- no assumption that sibling branches are implemented at the same time;
- class definitions should be data-driven and independently registerable;
- advancement edges/requirements should be data rather than switch/case ladders;
- requirements must support composable AND/OR expressions and multiple predicate types;
- class milestones/discovered Ways need durable Echo-wide stable IDs;
- the graph must support convergence: multiple parents/evidence sets can lead to one class;
- one prerequisite set may unlock multiple independent classes;
- advancement route/presentation data must be separable from resulting class identity, so different NPCs/quests/locations can lead to the same class;
- abilities should be reusable/composable building blocks rather than copied class-specific implementations;
- resources (Mana, Rage, Combo Points, etc.) should use generic resource interfaces/components where practical;
- effects, targeting, scaling and elements should be generic systems consumed by class data;
- UI must render whatever implemented branches actually exist;
- missing/unimplemented descendants must not crash or corrupt existing manifestations;
- class rigs/animation sets should reuse shared foundations where possible without forcing all classes into one silhouette.

The goal is to make adding the 50th class mostly content/design work, not an architecture rewrite.

### No future monolith
Do not repeat the early DiceBound pattern of allowing one giant file to become the game.

### Multiplayer-aware
DiceFree's intended session model is lobby/session co-op rather than MMO/open-world servers.

Initial target:
- up to 4 players;
- create/join lobby;
- Steam invite support;
- players can join an already-started overworld session;
- joiners spawn at the town/resurrection point their manifestation last selected;
- a player cannot join a dungeon/raid instance that has already started.

The networking architecture should support these rules without requiring a permanent dedicated-server backend.

## World/session structure

The host/session contains the current overworld game state for the party.

Quest credit should be granted only to eligible manifestations.

The exact authority/network transport solution is not locked yet.

## Dungeon instance rule

Starting a dungeon creates a closed run for its current participants.

No mid-run joins.

Full party wipe normally resets the entire run.

## Party scaling

Systems should expose party-size scaling hooks, but actual tuning/formulas are a later balance problem.

## Cube-world prototype requirement

Before committing to a seamless physical cube implementation, prototype:
- local gravity changes;
- click-to-move/pathfinding across a 90-degree edge;
- WASD across the same edge;
- isometric camera transition;
- enemy navigation/leashes;
- projectile behaviour;
- ground AoEs;
- multiplayer synchronization across the transition.

Compare this against a segmented/streamed face implementation with authored crossing routes.

The design goal is the **illusion/fantasy of inhabiting a six-faced die**, not technical purity for its own sake.

## Likely runtime areas

```
Core/
Characters/
Combat/
Classes/
Abilities/
Items/
World/
Quests/
Dungeons/
AI/
Multiplayer/
UI/
Persistence/
Tools/
```

## Character art pipeline

Before producing large quantities of wearable gear:
1. define scale;
2. define base rig/forms;
3. define bone/socket names;
4. define armor slots;
5. define skinned-mesh rules;
6. define export/import conventions;
7. test several radically different class forms/armor sets.

## Source control

GitHub is the central project home. Git LFS is used for binary art/media.
