# Combat

## Direction

DiceFree should use real-time 3D isometric action-RPG combat.

Current working assumptions:
- PC-first controls;
- **mouse-driven click-to-move is the primary/default control style and primary balance target**;
- WASD/direct movement is also supported as an alternative control style;
- mouse aiming/targeting/context interaction;
- a compact but class-dependent active toolkit;
- readable enemy attacks and boss mechanics;
- distinct class resources;
- strong hit feedback without unreadable visual noise.

## Control schemes

DiceFree should support both of these from the beginning:

### Classic / mouse movement

The primary control scheme, inspired by Warcraft III, Dota and League of Legends.

Typical behaviour:
- click the ground to move;
- movement uses pathfinding around valid obstacles;
- mouse input handles targeting and contextual actions;
- abilities receive targets/positions independently of the movement system.

This is the **design and balance baseline** for movement speed, encounter spacing, boss telegraphs, kiting, attack ranges, cast times and other movement-sensitive mechanics.

### Direct / WASD movement

Players may instead control movement directly with WASD.

WASD is a supported first-class option, not an accessibility afterthought, but it should not become the reference point that causes encounter design to assume action-game movement precision unavailable to click-to-move players.

### Shared movement architecture

The two input styles should feed the same underlying character movement/combat systems rather than become separate implementations.

```
Classic click-to-move ─┐
                       ├─> movement intent -> character motor -> gameplay
Direct WASD movement ──┘
```

Stuns, roots, slows, speed modifiers, combat restrictions, animation and networking should therefore operate consistently regardless of control style.

A future Hybrid option that accepts both click-to-move and WASD simultaneously may be useful, but it is **not yet a committed requirement**.

## Movement and mobility

There is **no universal dodge/roll**.

Mobility is part of class identity:
- some classes may have dashes, teleports, rolls or other evasive tools;
- some may have none;
- classes may have different base movement speeds;
- stronger mobility should come with appropriate tradeoffs elsewhere in the kit.

This preserves the Warcraft/ORPG-style importance of ordinary positioning instead of making every encounter assume universal invulnerability frames.

## Ability loadouts

DiceFree should keep the number of buttons relatively small, but there is **no requirement that every class has the same number of active abilities**.

A class may lean toward:
- more active abilities;
- more passive abilities;
- transformations;
- summons;
- stance/resource mechanics;
- a very compact core kit.

Advancement may add, evolve or replace abilities depending on that class.

The goal is strong class identity, not UI symmetry.

## Party design

The game should support solo play and eventual co-op.

Classes may naturally lean toward:
- durability/tanking;
- support/healing;
- ranged damage;
- melee burst;
- summoning;
- control;
- hybrid roles.

However, ordinary progression should not require a fixed MMO trinity composition.

## Abilities

Abilities should be modular enough to support:
- direct damage;
- area damage;
- projectiles;
- summons;
- damage over time;
- healing/shields;
- movement;
- crowd control;
- resource generation/spending;
- conditional effects;
- item-modified variants.

Ability behaviour should not depend on whether the player uses Classic or Direct movement. Targeted, ground-targeted, directional and self-cast abilities should resolve through shared targeting/ability systems.

## Bosses

Boss encounters should contain recognizable mechanics rather than only inflated health.

Harder dungeon/raid modes should be able to add:
- new attacks/mechanics;
- tighter timings;
- modified arena hazards;
- stronger enemy combinations;
- increased stats;
- altered/new loot tables.

Movement-sensitive mechanics must be tested primarily with Classic click-to-move so WASD does not quietly become required for reliable execution.

## Open questions

- Exact click/attack/context command conventions.
- Whether Hybrid mode should ship.
- Exact targeting behaviour.
- Potion/item hotkeys.
- Controller support expectations.
- Threat/taunt system depth.
- Whether basic attack is universal or class-specific.
