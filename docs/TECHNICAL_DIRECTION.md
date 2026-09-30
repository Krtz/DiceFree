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

### Persistence / save architecture

Persistence has explicit ownership boundaries:
- Echo/account-wide state;
- per-class manifestation state;
- transient session-instance state.

Core requirements:
- aggressive autosave of durable progression;
- loading a class spawns at its registered resurrection point rather than exact quit coordinates;
- active dungeon runs are transient and not persisted across quitting;
- already-earned persistent XP/gold survive dungeon abandonment/quit;
- advancement and bank/inventory transfers are atomic transactions;
- stable IDs and versioned save schemas;
- explicit save migrations;
- unknown/missing content records preserved inertly rather than silently deleted;
- rotating local backups;
- cloud-save provider abstraction;
- stable internal user/Echo UUIDs separate from platform IDs;
- multiple Echo profiles per user, with unobtrusive profile-management UX;
- Echo-level logical cloud revisions with internally chunked save data;
- explicit divergent-cloud conflict selection rather than unsafe field merging;
- offline and LAN-capable operation without mandatory cloud/backend connectivity;
- developer Profile Inspector tooling.

Initial PC direction supports Steam Cloud, but gameplay code must not depend directly on Steam-specific persistence APIs.

Prepare interfaces for future user/account/database services without requiring an MMO-style always-online backend.

Multiplayer persistence separates:
- host-authoritative live world presentation;
- per-player durable credit for events/quests/world outcomes actually earned.

See `docs/PERSISTENCE_AND_SAVES.md`.

### No future monolith
Do not repeat the early DiceBound pattern of allowing one giant file to become the game.

### Summon / controllable-unit architecture

Use one ownership/control framework for summons, pets, companions, turrets, clones, charmed units and related actors.

Requirements:
- owner and controller are separate references;
- no universal gameplay summon cap;
- class/ability data defines active-count/lifetime/upkeep/replacement rules;
- autonomous / directly-controlled / hybrid control models;
- generic commands and stance layer;
- selected unit can drive the shared command-grid UI;
- authored/snapshot/dynamic/mixed stat inheritance;
- owner-level-based level scaling; no independent summon XP/level track;
- independent/shared/no resource models;
- own threat plus configurable threat transfer/redirect;
- temporary/combat/session/manifestation persistence scopes;
- stable IDs for persistent companions;
- optional companion equipment/inventory capability;
- source + owner event attribution;
- host-authoritative AI/state with client-issued control commands;
- host migration/reconnect compatibility.

Do not implement a separate pet combat engine.

See `docs/SUMMONS_AND_COMPANIONS.md`.

### Enemy / encounter architecture

Enemy content should be composition-first:
- reusable base archetypes;
- authored variants/overrides;
- fixed authored levels;
- direct monster stats and/or five-attribute participation;
- semantic tags;
- reusable AI decision rules;
- threat/target selectors;
- home/leash/reset policies;
- authored pack/patrol definitions;
- reusable enemy modifiers.

AI logic must support both trivial policies and complex nested conditional/weighted rules.

Encounter definitions own fight-wide orchestration:
- phases;
- arena state;
- hazards;
- add waves;
- doors/objects;
- timers/enrage;
- difficulty-mode overrides;
- reset/completion state.

Encounter reset must be deterministic/inspectable for multiplayer host migration and validation.

Build developer encounter-test tooling early enough that bosses can be spawned, phase-forced and inspected without replaying full dungeons.

See `docs/ENEMIES_AND_ENCOUNTERS.md`.

### Network authority / host migration

Runtime networking should use a host-authoritative simulation:
- clients submit intentions/commands;
- host resolves authoritative combat/world/session results;
- guest persistent saves consume validated authoritative outcomes.

Solo/LAN/online should reuse the same core command/event/simulation paths where practical.

**Host migration is required.**

Do not architect session state so the original host is the only recoverable copy.

Plan for:
- replicated session snapshots/checkpoints;
- host election/rebinding;
- reconnect grace;
- participant slot reservation;
- live overworld state transfer;
- active dungeon/run transfer;
- safe fallback when exact live recovery is impossible;
- preservation of already-committed durable player progression under failure.

The implementation may choose snapshot/event-journal/replication details later.

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


## Development sequence: Cornberg then cube-world proof

Current prototype sequence:

1. complete the Cornberg / World 1 starter vertical-slice PoC far enough to prove the core RPG loop;
2. then create a **separate focused cube-world traversal PoC** before committing the full world architecture.

The cube-world PoC should test the hard technical fantasy directly:
- traversing/crossing a 90-degree cube edge;
- local gravity/orientation transition;
- Classic click-to-move/pathfinding;
- Direct/WASD;
- isometric camera behavior;
- enemies/navigation/leashes;
- projectiles;
- ground-targeted AoEs;
- multiplayer/network-state compatibility when that layer is ready.

Do not expand Faces 2–6 into full content just to test cube traversal.

The point of this PoC is to decide whether a genuinely seamless cube surface is practical and fun, or whether DiceFree should preserve the cube-world fantasy with authored/streamed transitions.

## World scale and distance units

DiceFree uses **meters** for authored/player-facing world distances.

Engine convention:
- **1 Unity world unit = 1 meter**.

Therefore authored values such as:
- interaction range;
- aggro/awareness radius;
- leash radius;
- quest-credit radius;
- ability range;
- AoE radius;
- travel distance

should be interpreted and documented in meters unless a system explicitly says otherwise.

Movement motors resolve physical speed in meters/second. Player-facing Move Speed uses the separate 10:1 presentation convention documented in `STATS_AND_DAMAGE.md` (50 Move Speed = 5 m/s).

Do not introduce a second fantasy-distance conversion layer.

