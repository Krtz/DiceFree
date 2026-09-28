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

### Quest/event architecture

Quests consume semantic gameplay events through reusable objective definitions rather than inspecting UI/display strings.

The framework should support:
- Kill / Interact / ReachArea / Collect / TalkTo / UseItem / CompleteDungeon objectives;
- nested AND/OR objective groups;
- Echo-once, session-instance-once, timeline-once and repeatable scopes;
- consumer-specific credit policies such as global, nearby, threat-participation and last-hit;
- reusable NPC/world interaction actions with optional default action and multi-action menus.

Combat/world event emitters provide facts; quest/XP/achievement consumers decide their own eligibility/credit policy.

### Ability/effect architecture

Abilities are composition-first:
- reusable targeting, cost, cooldown, damage/heal, element, status, movement, summon, threat and resource primitives;
- custom-code hooks remain available for genuinely unusual mechanics.

Effects own explicit stacking and dispel metadata.

Basic attacks use the same general action/effect pipeline so they can generate resources or class mechanics.

Resource architecture must support:
- multiple simultaneous resources;
- owner-bound and target-bound resources;
- independent persistence/reset/decay/regen policies;
- composite multi-resource/HP/item costs.

Ability runtime must support:
- multiple charge-recharge models;
- cast/channel behavior defined per ability;
- movement/damage interruption policies;
- optional pushback rather than a universal rule.

Cooldown architecture must support individual cooldowns, arbitrary shared groups, optional GCD-like groups and exceptional cross-player shared cooldowns.

Trigger/proc architecture must:
- expose extensible semantic hooks;
- retain action origin/context;
- prevent accidental recursion by default;
- allow explicitly bounded/controlled recursion for authored mechanics.

Periodic effects support snapshot and dynamic scaling.

Summon-origin events retain both summon source and owner attribution.

Auras should be reusable effect emitters rather than separate one-off aura code.

### Item/content authoring architecture

Equipment should use a shared structured item model that supports both:
- runtime randomized items;
- fully handcrafted authored items.

Random generation should expose the same constraints/budget logic to editor/design tooling.

Designer tooling should be able to:
- request candidates by level/rarity/slot/family/theme/drop-source;
- generate/reroll valid candidates;
- inspect budget allocation;
- freeze an accepted candidate into a stable authored item asset/definition;
- manually edit the frozen result afterward.

Item generation uses a gear-score/stat-budget model rather than arbitrary independent stat rolls.

Item level is source/content-defined rather than scaled to the current player by default.

Item budget/power diagnostics are development-only:
- expose expected vs actual budget;
- flag deliberate/accidental over-budget authored items;
- never require runtime/player UI to present budget as an item-quality verdict.

Runeword/socket architecture must allow ordered combinations on already-rare/magical items while retaining the base item's identity and existing affixes.

Affix validity, item eligibility, socket/runeword/set data and proc effects should use stable IDs/tags and generic systems.

Do not require handcrafted and randomized equipment to use separate combat/stat engines.

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

Dungeon lifecycle:
1. first player enters physical entrance;
2. shared staging room begins a 60-second countdown;
3. other eligible party members may enter during staging;
4. countdown expiry starts the active run;
5. roster and equipped gear are locked;
6. no mid-run joins;
7. full party wipe normally resets the entire run;
8. abandonment destroys/resets the active run;
9. successful completion moves each participant to a private loot room.

Dungeon data must be able to define:
- internal/self-respawn checkpoint policy;
- default whole-run reset plus explicit exceptional reset rules;
- encounter/trash reset behavior;
- loot-room reconnect window;
- authored completion conditions.

Dungeon trash normally does not use timed overworld respawns during an active attempt.

Boss encounter state must be cleanly resettable.

Consumables stay functional during active runs even though equipped gear/class switching are locked.

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
